using BookAppApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookAppApi.Services
{
    public class QuoteService
    {
        private readonly IMongoCollection<Quote> _quotes;

        public QuoteService(IOptions<MongoDbSettings> settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _quotes = database.GetCollection<Quote>("Quotes");
        }

        public async Task<List<Quote>> GetAsync() =>
            await _quotes.Find(_ => true).ToListAsync();

        public async Task<Quote> CreateAsync(Quote quote)
        {
            await _quotes.InsertOneAsync(quote);
            return quote;
        }

    }
}
