using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class Session
	{
		Socket _socket;
		int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();


        public void Start(Socket socket)
		{
			_socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            // 버퍼 설정
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
			RegisterRecv();
		}

        public void Send(byte[] sendBuff)
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

			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
		}

        #region 네트워크 통신 
        void RegisterRecv()
		{
			bool pending = _socket.ReceiveAsync(_recvArgs);
			if (pending == false)
				OnRecvCompleted(null, _recvArgs);
		}

		void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
		{
			// 전달받은 바이트 검사
			if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
				try
				{
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

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
			while(_sendQueue.Count > 0)
			{
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
			// 하나씩 Add는 불가능 
			_sendArgs.BufferList = _pendingList;

			bool pending = _socket.SendAsync(_sendArgs);
			if (pending == false)
				OnSendCompleted(null, _sendArgs);
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

						Console.WriteLine($"Transferred bytes: {_sendArgs.BytesTransferred}");

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

