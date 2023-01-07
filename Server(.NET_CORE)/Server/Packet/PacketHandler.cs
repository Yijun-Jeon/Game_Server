using System;
using ServerCore;

namespace Server
{
    // 해당 packet이 조립이 다 된 경우 무엇을 호출할 지 - 전부 수동으로 만드는 부분
    // ClientSession의 OnRecvPacket() switch 내 역할
    internal class PacketHandler
    {         
        // 사용자 지정 함수 - 원하는 액션
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
		{
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq: playerId({p.playerId}) name({p.name})");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"Skill({skill.id}) ({skill.level}) ({skill.duration})");
                foreach (PlayerInfoReq.Skill.Attribute att in skill.attributes)
                    Console.WriteLine($"Attribute({att.att})");
            }
        }
	}
}

