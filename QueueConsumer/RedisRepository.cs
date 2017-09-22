namespace cc.RealTimeAnalyzer.QueueConsumer
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

        // Gte a name for the datatype
        
        public RedisRepository(RedisDataContext context)
        {
            _context = context;
        }

        public async Task<TItem> GetItemAsync<TItem>(string id)
        {
            var redisKey = $"{KeyName<TItem>()}:{id}";
            var value = await _context.Db.StringGetAsync(redisKey);
            return JsonConvert.DeserializeObject<TItem>(value);
        }

        public async Task<bool> InsertAsync<TItem>(TItem item)
        {
            try
            {
                var id = _context.Db.StringIncrement($"{KeyName<TItem>()}:id",1);
                return await _context.Db.StringSetAsync($"{KeyName<TItem>()}:{id}", JsonConvert.SerializeObject(item));
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        private string KeyName<TItem>() => typeof(TItem).Name.ToLower();
    }
}