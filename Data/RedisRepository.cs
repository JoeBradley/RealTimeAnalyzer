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

        public void Insert<TItem>(TItem item, string json) where TItem : Models.DataPoint
        {
            try
            {
                //var json = JsonConvert.SerializeObject(item);

                //var id = _context.Db.StringIncrement($"{KeyName<TItem>()}:id",1);
                //_context.Db.StringSet($"{KeyName<TItem>()}:{id}", json);
                //_context.Db.GeoAdd($"{KeyName<TItem>()}:geo:{id}", new GeoEntry(item.Longitude, item.Latitude, json));


                var timespan = item.Timestamp.ToString("yyyyMMddHHmmss");
                var msTimespan = item.Timestamp.ToString("yyyyMMddHHmmssFFF");

                // Get/Set ensures that only 1 entry for each ms is stored in the second timespan bucket.  So if 2 or more entries for the same ms are created, only the latest is retained.
                var oldJson = _context.Db.StringGetSet($"{KeyName<TItem>()}:ms:{msTimespan}", json);
                // Set expiry of ms key
                _context.Db.KeyExpire($"{KeyName<TItem>()}:ms:{msTimespan}", new TimeSpan(0, 0, 3));

                if (string.IsNullOrEmpty(oldJson))
                {
                    // Add ms data point to sec bucket.  Accumulation/Analysis will be done on the bucket when full (at start of next second)
                    _context.Db.SetAdd($"{KeyName<TItem>()}:timespan:{timespan}", json);
                    _context.Db.KeyExpire($"{KeyName<TItem>()}:timespan:{timespan}", new TimeSpan(0, 0, 20));
                    // keep track of keys generated for ensuring distinct ms entries.  This will be deleted at the start of the next second.
                    //_context.Db.SetAdd($"{KeyName<TItem>()}:timespan:{timespan}:keyset", $"{KeyName<TItem>()}:ms:{msTimespan}");
                }

                // Publish to channel when new timespan finished
                var oldTimespan = _context.Db.StringGetSet($"{KeyName<TItem>()}:timespan", timespan);
                if (oldTimespan != timespan && !string.IsNullOrEmpty(oldTimespan))
                {
                    // Notifiy that we have finished adding to the old timespan window, so analysis can be done on it: accumulation/aggregation/analysis, etc.
                    _context.Db.Publish(_timespanChannel, $"{KeyName<TItem>()}:timespan:{oldTimespan}");

                    // Delete all ms keys used for distinct ms entries that were created in the previous second
                    //_context.Db.KeyDelete($"{KeyName<TItem>()}:timespan:{oldTimespan}:keyset");
                }

            }
            catch (Exception)
            {
                // do something
            }
        }

        private string KeyName<TItem>() => "data";  // typeof(TItem).Name.ToLower();
    }
}