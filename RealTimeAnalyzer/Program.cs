﻿using System;
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

        static void Main(string[] args)
        {
            try
            {
                // Console spinner for long running task
                timer = new Timer(state => {
                    QueuePublisher.Send(new DataPoint().ToString());
                }, new object(), new TimeSpan(0), new TimeSpan(0, 0, 0, 0, 100));

                Console.WriteLine("Press any key to exit");
                Console.ReadKey(false);
            }
            catch { }
            finally
            {
                timer?.Dispose();
            }
        }
        
    }
}