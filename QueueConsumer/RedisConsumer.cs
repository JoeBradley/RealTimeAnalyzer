using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using Newtonsoft.Json;
using cc.RealTimeAnalyzer.Models;
using System.Threading;
using cc.RealTimeAnalyzer.Data;
using cc.RealTimeAnalyzer.DataAnalyzer;

namespace cc.RealTimeAnalyzer.QueueConsumer
{
    public class RedisConsumer
    {
        private const string ExchangeName = "logs";
        private const string ExchangeType = "fanout";

        private readonly RedisRepository _repository;
        private readonly DataAggregator _aggregator;
        
        public RedisConsumer(RedisRepository repository) {
            _repository = repository;
            _aggregator = new DataAggregator(new RedisDataContext());
        }

        public void Receive(CancellationToken ct)
        {            
            var factory = new ConnectionFactory()
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQHost"].ToString(),
                Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQPort"].ToString())
            };

            //Console.WriteLine($"Listening to RabbitMQ on Host: {factory.HostName}:{factory.Port}");

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: ExchangeName,
                                  routingKey: "");

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Messaged received from RabbitMQ: " + message);
                    var dataPoint = JsonConvert.DeserializeObject<DataPoint>(message);
                    
                    _repository.Insert(dataPoint);
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                while (!ct.IsCancellationRequested) { Thread.Sleep(500); }
             }
        }
    }
}
