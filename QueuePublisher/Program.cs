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
        private static Timer timer;
        private static QueuePublisher _publisher;
        
        static void Main(string[] args)
        {
            try
            {
                var _publisher = new QueuePublisher();

                timer = new Timer(state => {
                    _publisher.SendTask(new DataPoint().ToString());
                }, new object(), new TimeSpan(0), new TimeSpan(0, 0, 0, 0, 10));

                Console.WriteLine("Press any key to exit");
                Console.ReadKey(false);
            }
            catch { }
            finally
            {
                _publisher?.Dispose();
                timer?.Dispose();
            }
        }
        
    }
}
