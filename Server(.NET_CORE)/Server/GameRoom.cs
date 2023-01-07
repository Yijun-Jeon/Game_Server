using System;
using System.Collections.Generic;

namespace Server
{
	class GameRoom
	{
        // 방에 있는 Client들 리스트 
        List<ClientSession> _sessions = new List<ClientSession>();
		object _lock = new object();

        // 입장
        public void Enter(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Add(session);
				session.Room = this;
			}
		}

		// 퇴장 
		public void Leave(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session);
			}
		}

        // 채팅메시지 전달
        public void BroadCast(ClientSession session, string chat)
		{
			S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
			ArraySegment<byte> segment = packet.Write();

            // 다른 Thread와 공유하고 있는 변수를 다루면 반드시 lock
            // 멀티쓰레드에서 대부분의 Thread가 여기서 대기
            lock (_lock)
			{
				foreach (ClientSession s in _sessions)
                    s.Send(segment);
			}
		}
    }
}

