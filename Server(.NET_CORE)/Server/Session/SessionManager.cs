﻿using System;
using System.Collections.Generic;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; }}

		int _sessionId = 0;
		Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
		object _lock = new object();

		public ClientSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
				_sessions.Add(sessionId,session);

				Console.WriteLine($"Connected : {sessionId}");

				return session;
            }
		}

		public ClientSession Find(int sessionId)
		{
			lock (_lock)
			{
                ClientSession session = null;
                if (_sessions.TryGetValue(sessionId, out session))
                    return session;
                return null;
            }
		}

		public void Remove(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session.SessionId);
			}
		}
	}
}

