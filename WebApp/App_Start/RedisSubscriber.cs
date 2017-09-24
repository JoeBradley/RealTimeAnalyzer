using cc.RealTimeAnalyzer.Data;
using cc.RealTimeAnalyzer.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Controllers;

namespace WebApp.App_Start
{
    public static class RedisSubscriber
    {
        static RedisDataContext _redisContext;
        static RedisChannel _channel;
        static ISubscriber _subscriber;

        public static void Startup() {
            try {
                _redisContext = new RedisDataContext();
                _channel = new RedisChannel($"channel:timespan:sum", RedisChannel.PatternMode.Auto);

                _subscriber = _redisContext.Db.Multiplexer.GetSubscriber();
                _subscriber.Subscribe(_channel, (channel, value) => {
                    DataHubManager.Instance.SetData(JsonConvert.DeserializeObject<AggregateData>(value));
                });
            }
            catch (Exception) {
                // TODO: handle
            }
        }
    }
}