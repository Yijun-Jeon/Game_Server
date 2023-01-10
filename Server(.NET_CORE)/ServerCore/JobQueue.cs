using System;
using System.Collections.Generic;

namespace ServerCore
{
    // C#에서 처리해야할 행위 자체를 Action으로 처리
    public interface IJobQueue
	{
		void Push(Action iob);
	}

	public class JobQueue : IJobQueue
	{
		Queue<Action> _jobQueue = new Queue<Action>();
		object _lock = new object();
		// lock을 도와줄 condition 변수 
		bool _flush = false;

		public void Push(Action job)
		{
			lock (_lock)
			{
				bool flush = false;

                _jobQueue.Enqueue(job);
				if (_flush == false)
					flush = _flush = true;

				if (flush)
					Flush();
            }
		}

		// 일감을 하나씩 뽑으면서 실행시킴
		// 실제 Flush를 실행하는 Thread는 하나만 있게 됨 
		void Flush()
		{
			while (true)
			{
				Action action = Pop();
				if (action == null)
					return;

				action.Invoke();
			}
		}


        Action Pop()
		{
			lock (_lock)
			{
				if (_jobQueue.Count == 0)
				{
					_flush = false;
                    return null;
                }
					
				return _jobQueue.Dequeue();
			}
		}
    }
}

