using System;
using DummyClient;
using ServerCore;

class PacketHandler
{
    // 사용자 지정 함수 - 원하는 액션
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if(chatPacket.playerId == 1)
            //Console.WriteLine(chatPacket.chat);
    }
}

