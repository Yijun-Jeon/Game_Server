using System;
using System.Collections.Generic;
using ServerCore;

namespace Server
{
	class GameRoom : IJobQueue
	{
        // 방에 있는 Client들 리스트 
        List<ClientSession> _sessions = new List<ClientSession>();
		JobQueue _jobQueue = new JobQueue();

		// 패킷 모아보내기를 위해 임시로 패킷들을 저장해 둘 리스트
		List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

		public void Push(Action job)
		{
			_jobQueue.Push(job);
		}

		// 리스트 단위로 Flush
		public void Flush()
		{
            foreach (ClientSession s in _sessions)
            	s.Send(_pendingList);

			Console.WriteLine($"Flushed {_pendingList.Count} items");
			_pendingList.Clear();
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

			// 패킷 모아보내기 
			_pendingList.Add(segment);
			//foreach (ClientSession s in _sessions)
			//	s.Send(segment);
		}
    }
}

