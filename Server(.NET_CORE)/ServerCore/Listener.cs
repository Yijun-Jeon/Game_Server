﻿using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
	public class Listener
	{
		// 문지기  
		Socket _listenSocket;
		Action<Socket> _onAcceptHandler;

		public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
		{
			_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기 수
            _listenSocket.Listen(10);

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			// 콜백으로 전달해주는 방식
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			RegisterAccept(args);
        }

		// 비동기 방식  
		void RegisterAccept(SocketAsyncEventArgs args)
		{
			// 기존에 있던 socket 초기화 
			args.AcceptSocket = null;

			// pending 이라는 추후 처리 여부를 반환해줌  
			bool pending = _listenSocket.AcceptAsync(args);
			if (pending == false)
				OnAcceptCompleted(null, args);
        }

		// EventHandler의 양식   
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			if(args.SocketError == SocketError.Success)
			{
				_onAcceptHandler.Invoke(args.AcceptSocket);
			}
			else
			{
				Console.WriteLine(args.SocketError.ToString());
			}

			// 다음 작업을 위해 등록
			RegisterAccept(args);
		}
	}
}
