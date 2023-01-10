using System;
using System.Diagnostics.CodeAnalysis;
using ServerCore;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        // 실행 시간 
        public int execTick;
        // 행위
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            // 작은 것이 우선 순위
            return other.execTick - execTick;
        }
    }

    // 작업의 중앙관리 시스템
    public class JobTimer
	{
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        // 실행 행위와 몇 초 후에 실행할 지 받음 
        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter; // 현재시간 + 해당 작업의 Tick
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        // 실제 실행 
        public void Flush()
        {
            while (true)
            {
                // 현재 시간 
                int now = System.Environment.TickCount;

                JobTimerElem job;
                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    // 가장 이른 Tick이 아직 실행 타이밍이 아님 
                    if (job.execTick > now)
                        break;

                    // 실행 타이밍이 된 job
                    _pq.Pop();
                }
                job.action.Invoke();
            }
        }
	}
}

