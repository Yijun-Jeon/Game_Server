using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using ServerCore;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS(Domain Name System)
            string host = Dns.GetHostName();
            // 로컬 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 경우에 따라 주소 여러개의 배열을 반환함
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소 - IP : 식당 주소  Port : 식당 문 번호
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 7000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });

            while (true)
            {
                try
                {
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                // 1초마다 반복  
                Thread.Sleep(1000);
            }
        }
    }
}

