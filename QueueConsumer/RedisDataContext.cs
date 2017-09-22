using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using StackExchange.Redis;

namespace cc.RealTimeAnalyzer.QueueConsumer
{
    /// <summary>
    ///     Redis
    ///     
    ///     Redis Download (Windows MSI): https://github.com/MSOpenTech/redis
    ///     
    ///     Uses StackExchange .Net client: https://github.com/StackExchange/StackExchange.Redis
    ///         Documentation: https://stackexchange.github.io/StackExchange.Redis/    
    /// 
    ///     
    ///     Start Docker from CLI: 
    ///         $ docker run --name myRedisDb -d redis redis-server --appendonly yes --requirepass Pword123!
    ///     Docker notes: Host is machine name: "Groot", port is default redis port: "6379", need to change docker container settings once started to use port "6379".
    ///     
    ///     Using GUI: http://www.fastonosql.com/
    /// </summary>
    public class RedisDataContext
    {
        // Docker configuration: var config = new ConfigurationOptions() { Password = "Pword123!", EndPoints = { "Groot:6379" }, AbortOnConnectFail = false };
        // Local Service configuration: var config = new ConfigurationOptions() { EndPoints = { "localhost" } };
        public static Lazy<ConnectionMultiplexer> Redis = new Lazy<ConnectionMultiplexer>(() =>
        {
            var endpoints = ConfigurationManager.AppSettings["RedisEndpoints"].ToString();
            var password = ConfigurationManager.AppSettings["RedisPassword"].ToString();

            // local runing redis service config
            var config = new ConfigurationOptions() { EndPoints = { endpoints }, AbortOnConnectFail = false };
            if (!string.IsNullOrEmpty(password)) config.Password = password;

            var cm = ConnectionMultiplexer.Connect(config);
            return cm;
        });

        public readonly IDatabase Db; 

        public RedisDataContext()
        {
            Db = Redis.Value.GetDatabase();
        }        
    }
}