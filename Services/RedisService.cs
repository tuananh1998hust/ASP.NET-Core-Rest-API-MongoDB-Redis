using StackExchange.Redis;
using System;
using System.Linq;
using MongoDB.Driver;

namespace mvc.Services
{
    public class RedisService
    {
        public static string RedisHost { get; set; }
        public static string RedisPort { get; set; }
        private readonly ConnectionMultiplexer _connection;

        public RedisService()
        {
            try
            {
                string redisURL = RedisHost + ":" + RedisPort;
                _connection = ConnectionMultiplexer.Connect(redisURL);
            }
            catch (Exception error)
            {
                throw new Exception("Redis Connect Error", error);
            }
        }

        public IDatabase RedisClient
        {
            get
            {
                return _connection.GetDatabase();
            }
        }
    }
}