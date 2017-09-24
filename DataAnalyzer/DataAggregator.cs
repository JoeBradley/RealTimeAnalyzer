using cc.RealTimeAnalyzer.Data;
using cc.RealTimeAnalyzer.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cc.RealTimeAnalyzer.DataAnalyzer
{
    public class DataAggregator
    {
        RedisDataContext _redisContext;
        // Subscribe to this, then do analysis, and then publish
        RedisChannel _subscribeChannel;
        // Publish to this, which is then read by SignalR HUb, and pushed to the clients
        RedisChannel _publishChannel;

        ISubscriber _puSUb;

        public DataAggregator(RedisDataContext redisContext)
        {
            _redisContext = redisContext;
            BindSubscriptions();
        }

        private void BindSubscriptions()
        {
            _subscribeChannel = new RedisChannel($"channel:timespan", RedisChannel.PatternMode.Auto);
            _publishChannel = new RedisChannel($"channel:timespan:sum", RedisChannel.PatternMode.Auto);

            _puSUb = _redisContext.Db.Multiplexer.GetSubscriber();

            _puSUb.Subscribe(_subscribeChannel, (channel, value) => {
                AggregateTimespan(value);
            });            
        }

        private void AggregateTimespan(string timespanKey)
        {
            var members = (_redisContext.Db.SetMembers(timespanKey));
            var dataPoints = members.Select(x => JsonConvert.DeserializeObject<DataPoint>(x.ToString())).ToList();

            // Do Analysis
            var ts = DateTime.ParseExact(timespanKey.Split(new char[] { ':' })[2], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var aggregateDataPoint = JsonConvert.SerializeObject(new AggregateData(ts,dataPoints));
            
            // Save back to Redis
            _redisContext.Db.StringSet($"{timespanKey}:sum", aggregateDataPoint);

            // Publish change
            _redisContext.Db.Publish(_publishChannel, aggregateDataPoint);
        }
    }
}
