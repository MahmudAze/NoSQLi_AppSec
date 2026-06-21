using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLinjection.Models
{
    public class User
    {
        // MongoDB-nin avtomatik yaratdığı "_id" sahəsini C#-a belə tanıdırıq
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
