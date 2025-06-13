using BookAppApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookAppApi.Services
{
    public class BookListService
    {
        private readonly IMongoCollection<Book> _books;

        public BookListService(IOptions<MongoDbSettings> settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _books = database.GetCollection<Book>("Books");
        }

        public async Task<List<Book>> GetAsync() =>
            await _books.Find(book => true).ToListAsync();

        public async Task<Book> CreateAsync(Book book)
        {
            await _books.InsertOneAsync(book);
            return book;
        }

        public async Task<List<Book>> SearchAsync(string term)
        {
            var filter = Builders<Book>.Filter.Or(
                Builders<Book>.Filter.Regex(b => b.Title, new BsonRegularExpression(term, "i")),
                Builders<Book>.Filter.Regex(b => b.Author, new BsonRegularExpression(term, "i"))
            );
            return await _books.Find(filter).ToListAsync();
        }

        public async Task UpdateAsync(string id, Book bookIn) =>
            await _books.ReplaceOneAsync(book => book.Id == id, bookIn);

        public async Task DeleteAsync(string id) =>
            await _books.DeleteOneAsync(book => book.Id == id);
    }
}
