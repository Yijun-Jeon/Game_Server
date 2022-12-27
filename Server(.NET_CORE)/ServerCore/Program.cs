﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            // 로컬 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 경우에 따라 주소 여러개의 배열을 반환함
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소 - IP : 식당 주소  Port : 식당 문 번호
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 7000);

            // www.yijun.com -> 123.123.123.12
            // 문지기
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // TCP 사용

            try
            {
                // 문지기 교육
                listenSocket.Bind(endPoint);

                // 영업 시작
                // backlog : 최대 대기 수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 손님을 입장시킴 - 대리인 생성
                    // 입장을 하지 않으면 아래 단계는 진행이 되지 않음 
                    Socket clientSocket = listenSocket.Accept();

                    // 받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    // 버퍼, 시작 인덱스, 받은 바이트
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                    clientSocket.Send(sendBuff);

                    // 쫒아낸다
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

