using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RealTimeAnalyzer
{
    /// <summary>
    /// RabbitMQ test
    /// Source: https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html
    /// </summary>
    public class QueuePublisher
    {
        private const string ExchangeName = "logs";
        private const string ExchangeType = "fanout";

        public static void WaitAndSendAndWait()
        {
            Console.WriteLine(" Press [enter] to start");
            Console.ReadLine();

            for (int i = 0; i < 10; i++)
            {
                var msg = $"Task {i}{(new string('.', i))}";
                Send(msg);
                System.Threading.Thread.Sleep(300 * i);
            }

            Console.WriteLine(" Press [enter] to exit");
            Console.ReadLine();
        }

        public static void SendAndWait(string message)
        {
            Send(message);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        public static void Send(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQHost"].ToString(),
                Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQPort"].ToString())
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                //Console.WriteLine(" [x] Sent {0}", message);
            }
        }


    }
}
