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

        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);
        
        public async Task<User?> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();



        
    }
}
