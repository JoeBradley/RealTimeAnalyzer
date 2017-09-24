using cc.RealTimeAnalyzer.Data;
using cc.RealTimeAnalyzer.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cc.RealTimeAnalyzer.DataAnalyzer
{
    public class DataAggregator
    {
        RedisDataContext _redisContext;
        RedisChannel _timespanChannel;
        RedisChannel _aggregateChannel;

        ISubscriber _subscriber;

        public DataAggregator(RedisDataContext redisContext)
        {
            _redisContext = redisContext;
            _timespanChannel = new RedisChannel($"channel:timespan", RedisChannel.PatternMode.Auto);
            _aggregateChannel = new RedisChannel($"channel:timespan:sum", RedisChannel.PatternMode.Auto);

            _subscriber = _redisContext.Db.Multiplexer.GetSubscriber();
            BindSubscriptions();
        }

        private void BindSubscriptions()
        {
            _subscriber.Subscribe(_timespanChannel, (channel, value) => {
                AggregateTimespan(value);
            });
        }

        private void AggregateTimespan(string timespanKey)
        {
            var members = (_redisContext.Db.SetMembers(timespanKey));
            var dataPoints = members.Select(x => JsonConvert.DeserializeObject<DataPoint>(x.ToString())).ToList();

            var aggregateDataPoint = JsonConvert.SerializeObject(new AggregateData(dataPoints));

            _redisContext.Db.StringSet($"{timespanKey}:sum", aggregateDataPoint);
            _redisContext.Db.Publish(_aggregateChannel, aggregateDataPoint);
        }
    }
}
