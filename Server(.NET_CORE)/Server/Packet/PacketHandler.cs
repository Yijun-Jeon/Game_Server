using System;
using Server;
using ServerCore;


// 해당 packet이 조립이 다 된 경우 무엇을 호출할 지 - 전부 수동으로 만드는 부분
// ClientSession의 OnRecvPacket() switch 내 역할
class PacketHandler
{         
    // 사용자 지정 함수 - 원하는 액션
    public static void C_ChatHandler(PacketSession session, IPacket packet)
	{
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        // session의 메시지를 모두에게 뿌려주는 역할
        clientSession.Room.BroadCast(clientSession, chatPacket.chat);
    }
}

