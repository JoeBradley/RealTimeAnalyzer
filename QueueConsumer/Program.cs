using cc.RealTimeAnalyzer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cc.RealTimeAnalyzer.QueueConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var consumer = new RedisConsumer(new RedisRepository(new RedisDataContext()));

            // Create the token source.
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
           
            var tsk = Task.Run(() =>
            {
                consumer.Receive(ct);
                Console.WriteLine("Finished listening");
            }, ct);

            Console.WriteLine("Receiving messages from RabbitMQ and inserting into Redis.");
            Console.WriteLine("Press [enter] to exit.");

            Console.ReadLine();

            cts.Cancel();

            Thread.Sleep(1000);
        }
    }
}
