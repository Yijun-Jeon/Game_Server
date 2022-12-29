using System;
namespace ServerCore
{
	public class RecvBuffer
	{
		ArraySegment<byte> _buffer;
		int _readPos;
		int _writePos;

		public RecvBuffer(int bufferSize)
		{
			_buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
		}

		// 버퍼에 아직 처리(read)가 안된 데이터의 크기
		public int DataSize { get { return _writePos - _readPos; } }
        // 빈 공간의 크기
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 버퍼의 아직 처리가 안된 데이터 공간을 참조하는 segment
        public ArraySegment<byte> ReadSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
		}

        // 빈 공간을 참조하는 segment
        public ArraySegment<byte> WriteSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
		}

        // read,write 커서를 맨 앞으로 끌어 옴으로써 버퍼에 있는 처리가 완료된 데이터를 치워버리는 메서드
        public void Clean()
		{
			int dataSize = DataSize;
			if(dataSize == 0)
			{
				// 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋 
				_readPos = _writePos = 0;
			}
			else
			{
				// 남은 데이터가 있으면 시작 위치로 복사
				Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, DataSize);
				_readPos = 0;
				_writePos = DataSize;
			}
		}

		// Read에 성공하였을 때 호출 
		public bool OnRead(int numOfBytes)
		{
			if (numOfBytes > DataSize)
				return false;
			_readPos += numOfBytes;
			return true;
		}
		// Write에 성공하였을 때 호출
		public bool OnWrite(int numOfBytes)
		{
			if (numOfBytes > FreeSize)
				return false;
			_writePos += numOfBytes;
			return true;
		}
    }
}

