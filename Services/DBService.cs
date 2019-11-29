using System;
using System.Linq;
using MongoDB.Driver;
using mvc.Models;

namespace mvc.Services
{
    public class DBService
    {
        public static string ConnectionString { get; set; }
        public static string DatabaseName { get; set; }
        private readonly IMongoDatabase _db;

        public DBService()
        {
            try
            {
                _db = new MongoClient(ConnectionString).GetDatabase(DatabaseName);
            }
            catch (Exception error)
            {
                throw new Exception("MongoDB Connect Error", error);
            }
        }

        // Users
        public IMongoCollection<User> Users
        {
            get
            {
                return _db.GetCollection<User>("User");
            }
        }
    }
}