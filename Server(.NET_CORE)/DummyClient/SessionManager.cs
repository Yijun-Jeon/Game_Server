using System;
using System.Collections.Generic;

namespace DummyClient
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		List<ServerSession> _sessions = new List<ServerSession>();
		object _lock = new object();

        // Session 생성
        public ServerSession Generate()
		{
			lock (_lock)
			{
				ServerSession session = new ServerSession();
				_sessions.Add(session);
				return session;
			}
		}

        // 모든 Session이 Server로 메시지 보냄
        public void SendForEach()
		{
			lock (_lock)
			{
				foreach(ServerSession session in _sessions)
				{
					C_Chat chatPacket = new C_Chat();
					chatPacket.chat = $"Hello Server!";
					ArraySegment<byte> segment = chatPacket.Write();

					session.Send(segment);
				}
			}
		}
    }
}

