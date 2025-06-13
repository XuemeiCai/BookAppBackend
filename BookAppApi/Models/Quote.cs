using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookAppApi.Models
{
    public class Quote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;
    }
}

