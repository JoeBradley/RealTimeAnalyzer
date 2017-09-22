using cc.RealTimeAnalyzer.Data;
using cc.RealTimeAnalyzer.Models;
using cc.RealTimeAnalyzer.QueueConsumer;
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
        ISubscriber _subscriber;

        public DataAggregator(RedisDataContext redisContext)
        {
            _redisContext = redisContext;
            _timespanChannel = new RedisChannel($"channel:timespan", RedisChannel.PatternMode.Auto);

            _subscriber = _redisContext.Db.Multiplexer.GetSubscriber();            
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
            var dataPoints = members.Select(x => JsonConvert.DeserializeObject<DataPoint>(x.ToString()));

            _redisContext.Db.StringSet($"timespankey:count", dataPoints.Count());

            _redisContext.Db.StringSet($"timespankey:high:avg", dataPoints.Average(x => x.High));
            _redisContext.Db.StringSet($"timespankey:high:max", dataPoints.Max(x => x.High));
            _redisContext.Db.StringSet($"timespankey:high:min", dataPoints.Min(x => x.High));            
        }
    }
}
