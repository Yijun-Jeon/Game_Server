﻿using System;
using ServerCore;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //Program.Room.Enter(this);
            // jobQueue 사용
            Program.Room.Push(() =>
            {
                Program.Room.Enter(this);
            });
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room != null)
            {
                //Room.Leave(this);
                // jobQueue 사용
                GameRoom room = Room;

                room.Push(() =>
                {
                    room.Leave(this);
                });
                
                Room = null;
            }

            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
