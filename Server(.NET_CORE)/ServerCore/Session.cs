﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public abstract class PacketSession : Session
	{
		// 헤더(패킷 크기) 사이
		public static readonly int HeaderSize = 2;

        // [size(2)][pacektId(2)][...][size(2)][pacektId(2)][...]...
        public sealed override int OnRecv(ArraySegment<byte> buffer)
		{
			// 몇 바이트 처리했는지 기록 
			int processLen = 0;

			while (true)
			{
				// 최소한 헤더는 파싱할 수 있는지 확인
				if (buffer.Count < HeaderSize)
					break;

				// 패킷이 완전체로 도착했는지 확인
				ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
				if (buffer.Count < dataSize)
					break;

				// 패킷 조립 가능
				OnRecvPacket(new ArraySegment<byte>(buffer.Array,buffer.Offset,dataSize));

				processLen += dataSize;
				// 다음 패킷 부분으로 버퍼 변경
				buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
			}

			return processLen;
		}

		// 완전체 패킷이 있는 배열을 받아서 원하는 작업 처리 
		public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

	public abstract class Session
	{
		Socket _socket;
		int _disconnected = 0;

		RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
		Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

		public abstract void OnConnected(EndPoint endPoint);
		public abstract int OnRecv(ArraySegment<byte> buffer);
		public abstract void OnSend(int numOfBytes);
		public abstract void OnDisconnected(EndPoint endPoint);

		void Clear()
		{
			lock (_lock)
			{
				_sendQueue.Clear();
				_pendingList.Clear();
			}
		}


        public void Start(Socket socket)
		{
			_socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

			RegisterRecv();
		}

        public void Send(ArraySegment<byte> sendBuff)
        {
			lock (_lock)
			{
                _sendQueue.Enqueue(sendBuff);
                // 전송 가능
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

		public void Disconnect()
		{
			// 중복 처리 방지
			if (Interlocked.Exchange(ref _disconnected, 1) == 1)
				return;

			OnDisconnected(_socket.RemoteEndPoint); 
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();

			Clear(); 
		}

        #region 네트워크 통신 
        void RegisterRecv()
		{
			if (_disconnected == 1)
				return;

			_recvBuffer.Clean();
			ArraySegment<byte> segment = _recvBuffer.WriteSegment;
			_recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

			try
			{
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch(Exception e)
			{
				Console.WriteLine($"RegisterRecv Failed {e}");
			}	
		}

		void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
		{
			// 전달받은 바이트 검사
			if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
				try
				{
					// Write 커서 이동
					if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
					{
						Disconnect();
						return;
					}

					// 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다 
					int processLen = OnRecv(_recvBuffer.ReadSegment);
					if(processLen < 0 || processLen > _recvBuffer.DataSize )
					{
						Disconnect();
						return;
					}

					// Read 커서 이동
					if(_recvBuffer.OnRead(processLen) == false)
					{
						Disconnect();
						return;
					}

                    RegisterRecv();
                }
				catch(Exception e)
				{
					Console.WriteLine($"OnRecvCompleted Failed {e}");
				}
            }
            else
			{
				Disconnect();
			}
		}

		void RegisterSend()
		{
			if (_disconnected == 1)
				return;

			while(_sendQueue.Count > 0)
			{
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
			// 하나씩 Add는 불가능 
			_sendArgs.BufferList = _pendingList;
			try
			{
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch(Exception e)
			{
				Console.WriteLine($"RegisterSend Failed {e}");
			}
			
		}

		void OnSendCompleted(object sender, SocketAsyncEventArgs args)
		{
			// 콜백 방식도 있기 때문에 lock 필요  
			lock(_lock)
			{
                // 보낼 바이트 검사
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
						_sendArgs.BufferList = null;
						_pendingList.Clear();

						OnSend(_sendArgs.BytesTransferred);
						
						if (_sendQueue.Count > 0)
							RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnRecvCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        #endregion
    }
}

