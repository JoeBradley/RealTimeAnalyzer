namespace cc.RealTimeAnalyzer.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using StackExchange.Redis;

    /// <summary>
    /// AWS DynamoDB Repository
    /// 
    /// Runs locally using DynamoDbLocal.jar
    /// 
    /// see: http://docs.aws.amazon.com/amazondynamodb/latest/developerguide/LowLevelDotNetItemCRUD.html
    /// see: https://github.com/ServiceStack/PocoDynamo
    /// </summary>
    public class RedisRepository 
    {
        private RedisDataContext _context;

        RedisChannel _timespanChannel;
        // Gte a name for the datatype
        
        public RedisRepository(RedisDataContext context)
        {
            _context = context;
            _timespanChannel = new RedisChannel($"channel:timespan", RedisChannel.PatternMode.Auto);
        }

        public async Task<TItem> GetItemAsync<TItem>(string id)
        {
            var redisKey = $"{KeyName<TItem>()}:{id}";
            var value = await _context.Db.StringGetAsync(redisKey);
            return JsonConvert.DeserializeObject<TItem>(value);
        }

        public void Insert<TItem>(TItem item) where TItem : Models.DataPoint
        {
            try
            {
                var json = JsonConvert.SerializeObject(item);

                var id = _context.Db.StringIncrement($"{KeyName<TItem>()}:id",1);

                var timespan = item.Timestamp.ToString("yyyyMMddHHmmss");

                //_context.Db.StringSet($"{KeyName<TItem>()}:{id}", json);

                //_context.Db.GeoAdd($"{KeyName<TItem>()}:geo:{id}", new GeoEntry(item.Longitude, item.Latitude, json));

                _context.Db.SetAdd($"{KeyName<TItem>()}:timespan:{timespan}", json);

                // Publish to channel when new timespan finished
                var oldTimespan = _context.Db.StringGetSet($"{KeyName<TItem>()}:timespan", timespan);
                if (oldTimespan != timespan)
                {
                    // Notifiy that we have finished adding to the old timespan window, so analysis can be done on it: aggregation, etc.
                    if (!string.IsNullOrEmpty(oldTimespan))
                        _context.Db.Publish(_timespanChannel, $"{KeyName<TItem>()}:timespan:{oldTimespan}");
                }

            }
            catch (Exception)
            {
                // do something
            }
        }

        private string KeyName<TItem>() => typeof(TItem).Name.ToLower();
    }
}