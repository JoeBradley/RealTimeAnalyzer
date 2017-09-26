using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using cc.RealTimeAnalyzer.Models;

namespace RealTimeAnalyzer
{
    class Program
    {
        private static List<Timer> timers = new List<Timer>();
        private static QueuePublisher _publisher;
        
        static void Main(string[] args)
        {
            try
            {
                _publisher = new QueuePublisher();

                Parallel.For(1, 3, (i) => {
                    Thread.Sleep(1);
                    timers.Add( new Timer(state => {
                        Parallel.For(1, 10, (x) =>
                        {
                            Thread.Sleep(1);
                            _publisher.SendTask(new DataPoint().ToString());
                        });
                    }, new object(), new TimeSpan(0), new TimeSpan(0, 0, 0, 0, 10)));

                });

                Console.WriteLine("Press any key to exit");
                Console.ReadKey(false);
            }
            catch { }
            finally
            {
                _publisher?.Dispose();
                timers.ForEach(timer => timer?.Dispose());
            }
        }
        
    }
}
