using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TickBoardcastApp
{
    public class ProducerConsumer
    {
        public ConcurrentQueue<int> Counter = new ConcurrentQueue<int>();
        public void Producer()
        {
            int n = 0;
            while (1 > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                n++;
                Counter.Enqueue(n);
            }
        }

        public void Consumer(Action<int> action)
        {
            while (1 > 0)
            {
                if (Counter.TryDequeue(out int n))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    action.Invoke(n);
                }
            }
        }
        public void Count()
        {
            Thread tCount = new Thread(() =>
            {
                while (!Counter.IsEmpty)
                {
                    Console.WriteLine($"Count:{Counter.Count}");
                }
            })
            {
                Name = "Count"
            };
            tCount.Start();
            tCount.Join();
            Console.ReadLine();
        }
    }
}
