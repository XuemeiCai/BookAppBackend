using BookAppApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace BookAppApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDbSettings> dbSettings, IMongoClient client)
        {
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetAllAsync() => await _users.Find(_ => true).ToListAsync();

        public async Task<User?> GetByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateAsync(string id, User user) =>
            await _users.ReplaceOneAsync(u => u.Id == id, user);

        public async Task DeleteAsync(string id) =>
            await _users.DeleteOneAsync(u => u.Id == id);
    }
}
