using System;
using System.Collections.Generic;
using ServerCore;

namespace Server
{
	class GameRoom : IJobQueue
	{
        // 방에 있는 Client들 리스트 
        List<ClientSession> _sessions = new List<ClientSession>();
		object _lock = new object();
		JobQueue _jobQueue = new JobQueue();

		public void Push(Action job)
		{
			_jobQueue.Push(job);
		}

        // 입장
        public void Enter(ClientSession session)
		{
			
			_sessions.Add(session);
			session.Room = this;
	
		}

		// 퇴장 
		public void Leave(ClientSession session)
		{
			_sessions.Remove(session);
		}

        // 채팅메시지 전달
        public void BroadCast(ClientSession session, string chat)
		{
			S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
			ArraySegment<byte> segment = packet.Write();

			foreach (ClientSession s in _sessions)
				s.Send(segment);
		}
    }
}

