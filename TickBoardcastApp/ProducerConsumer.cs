using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TickBoardcastApp
{
    public class ProducerConsumer
    {
        ConcurrentQueue<int> counter = new ConcurrentQueue<int>();
        public void Producer()
        {
            Thread tProducer = new Thread(() =>
            {
                int n = 0;
                while (1 > 0)
                {
                    n++;
                    counter.Enqueue(n);
                    Console.WriteLine($"Enqueue:{n}");
                }
            })
            {
                Name = "Producer"
            };
            tProducer.Start();
            tProducer.Join();
        }

        public void Consumer()
        {
            Thread tConsumer = new Thread(() =>
            {
                while (counter.TryDequeue(out int n))
                {
                    Console.WriteLine($"Dequeue:{n}");
                }
            })
            {
                Name = "Consumer"
            };
            tConsumer.Start();
            tConsumer.Join();
        }
        public void Count()                                                     
        {
            Thread tCount = new Thread(() =>
            {
                while (!counter.IsEmpty)
                {
                    Console.WriteLine($"Count:{counter.Count}");
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
