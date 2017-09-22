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

        public async Task<bool> InsertAsync<TItem>(TItem item) where TItem : Models.GeoPoint
        {
            try
            {
                var json = JsonConvert.SerializeObject(item);

                var id = _context.Db.StringIncrement($"{KeyName<TItem>()}:id",1);

                var timespan = item.Timestamp.ToString("yyyyMMddHHmmss");
                
                await _context.Db.GeoAddAsync($"{KeyName<TItem>()}:geo:{id}", new GeoEntry(item.Longitude, item.Latitude, json));

                await _context.Db.SetAddAsync($"{KeyName<TItem>()}:timespan:{timespan}", json);

                // Publish when new timespan finished
                var oldTimespan = _context.Db.StringGet($"{KeyName<TItem>()}:timespan)");
                if (oldTimespan != timespan)
                {
                    _context.Db.StringSet($"{KeyName<TItem>()}:timespan)", timespan);

                    // Notifiy that we have finished adding to the old timespan window, so analysis can be done on it: aggregation, etc.
                    if (!string.IsNullOrEmpty(oldTimespan))
                        await _context.Db.PublishAsync(_timespanChannel, $"{KeyName<TItem>()}:timespan:{oldTimespan}");
                }

                return await _context.Db.StringSetAsync($"{KeyName<TItem>()}:{id}", json);
            }
            catch (Exception)
            {

            }
            return false;
        }

        private string KeyName<TItem>() => typeof(TItem).Name.ToLower();
    }
}