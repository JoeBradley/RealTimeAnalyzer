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
    public class QueuePublisher : IDisposable
    {
        private const string ExchangeName = "logs";
        private const string ExchangeType = "fanout";

        private static IConnection _connection;
        private static IModel _taskChannel;
        private static IBasicProperties _taskProperties;

        public QueuePublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQHost"].ToString(),
                Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQPort"].ToString())
            };

            _connection = factory.CreateConnection();

            _taskChannel = _connection.CreateModel();
            _taskChannel.QueueDeclare(queue: ExchangeName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

            _taskProperties = _taskChannel.CreateBasicProperties();
            _taskProperties.Persistent = true;
        }

        public void Send(string message)
        {
            //Console.WriteLine($"Publishing to RabbitMQ on Host: {factory.HostName}:{factory.Port}");

            using (var channel = _connection.CreateModel())
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

                // Console.WriteLine("Message: {0}", message);
            }
        }

        public void SendTask(string message)
        {
            //Console.WriteLine($"Publishing to RabbitMQ on Host: {factory.HostName}:{factory.Port}");

            var body = Encoding.UTF8.GetBytes(message);

            _taskChannel.BasicPublish(exchange: "",
                routingKey: ExchangeName,
                basicProperties: _taskProperties,
                body: body);
        }

        public void Dispose()
        {
            _taskChannel?.Dispose();
            _connection?.Dispose();
        }
    }
}
