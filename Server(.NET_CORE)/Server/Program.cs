using System;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        // 예약 작업 - Room이외에도 개별적으로 적용
        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());

            // 0.25초 후에 다시 실행해달라고 계속 예약
            JobTimer.Instance.Push(FlushRoom, 250);
        }

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

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            FlushRoom();
            while (true)
            {
                // 실행할 일감이 있는지만 계속 검사 
                JobTimer.Instance.Flush();    
            }
        }
    }
}

