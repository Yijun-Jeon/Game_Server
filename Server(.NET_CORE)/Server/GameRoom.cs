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
			lock (_lock)
			{
				S_Chat packet = new S_Chat();
                packet.playerId = session.SessionId;
                packet.chat = chat;
				ArraySegment<byte> segment = packet.Write();

				lock (_lock)
				{
					foreach (ClientSession s in _sessions)
                        s.Send(segment);
				}
			}
		}
    }
}

