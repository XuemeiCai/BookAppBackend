using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookAppApi.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ISBN { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
