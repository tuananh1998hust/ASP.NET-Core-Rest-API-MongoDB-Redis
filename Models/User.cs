using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mvc.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string phone { get; set; }
    }
}