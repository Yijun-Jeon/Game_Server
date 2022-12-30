using System;
using System.Threading;

namespace ServerCore
{
    // 실제 외부에서 사용하는 클래스 
    public class SendBufferHelper
	{
        // SendBuffer의 인스턴스가 저장되는 전역변수
		// 나의 Thread에서만 고유하게 사용할 수 있는 전역 변수
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
		// 만들어지는 buffer의 크기 
		public static int ChunkSize { get; set; } = 4096 * 100;

		public static ArraySegment<byte> Open(int reserveSize)
		{
			// 현재 TLS에 버퍼가 만들어져 있지 않으면 만듦  
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			// 현재 버퍼에 빈 공간이 부족할 경우 버퍼 새로 만듦 
			if (CurrentBuffer.Value.FreeSize < reserveSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			// 빈 공간 양도 
			return CurrentBuffer.Value.Open(reserveSize);
		}

        public static ArraySegment<byte> Close(int usedSize)
        {
			return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
	{
		byte[] _buffer;
		// FreeSize의 시작 index && 사용중인 공간 크기 
		int _usedSize = 0;

		public int FreeSize { get { return _buffer.Length - _usedSize; } }

		public SendBuffer(int chunkSize)
		{
			_buffer = new byte[chunkSize];
		}

        // 사용 예약 - 예약된 segment 반환 
        public ArraySegment<byte> Open(int reserveSize)
		{
			if (reserveSize > FreeSize)
				return null;

			return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
		}

        // 실제 사용 완료 - 사용한 segment 반환 
        public ArraySegment<byte> Close(int usedSize)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
			_usedSize += usedSize;
			return segment;
		}
	}
}

