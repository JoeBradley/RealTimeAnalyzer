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
    public class RedisConsumer : IDisposable
    {
        private const string ExchangeName = "logs";
        private const string ExchangeType = "fanout";

        private readonly RedisRepository _repository;
        private readonly DataAggregator _aggregator;
        private readonly ConnectionFactory _factory;

        public RedisConsumer(RedisRepository repository)
        {
            _repository = repository;
            _aggregator = new DataAggregator(new RedisDataContext());

            _factory = new ConnectionFactory()
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQHost"].ToString(),
                Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQPort"].ToString())
            };
        }

        public void Dispose()
        {
            // _channel?.Dispose();
            //_connection?.Dispose();
        }

        public void Receive(CancellationToken ct)
        {
            //Console.WriteLine($"Listening to RabbitMQ on Host: {factory.HostName}:{factory.Port}");

            using (var _connection = _factory.CreateConnection())
            using (var _channel = _connection.CreateModel())
            {
                _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType);

                var queueName = _channel.QueueDeclare().QueueName;
                _channel.QueueBind(queue: queueName,
                                  exchange: ExchangeName,
                                  routingKey: "");

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    //Console.WriteLine("Messaged received from RabbitMQ: " + message);

                    var dataPoint = JsonConvert.DeserializeObject<DataPoint>(message);

                    _repository.Insert(dataPoint, message);
                };

                _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                while (!ct.IsCancellationRequested) { Thread.Sleep(500); }
            }
        }

        public void ReceiveTasks(CancellationToken ct)
        {
            //Console.WriteLine($"Listening to RabbitMQ on Host: {factory.HostName}:{factory.Port}");
            Parallel.For(0, 5, (i) =>
            {
                using (var _connection = _factory.CreateConnection())
                using (var _channel = _connection.CreateModel())
                {
                    _channel.QueueDeclare(queue: ExchangeName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    
                    var consumer = new EventingBasicConsumer(_channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        //Console.WriteLine("Messaged received from RabbitMQ: " + message);

                        var dataPoint = JsonConvert.DeserializeObject<DataPoint>(message);

                        _repository.Insert(dataPoint, message);
                    };

                    _channel.BasicConsume(queue: ExchangeName, autoAck: true, consumer: consumer);

                    while (!ct.IsCancellationRequested) { Thread.Sleep(500); }
                }
            });
        }
    }
}
