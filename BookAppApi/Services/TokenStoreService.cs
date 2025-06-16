using BookAppApi.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace BookAppApi.Services
{
    public class TokenStoreService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public TokenStoreService(IOptions<MongoDbSettings> settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _usersCollection = database.GetCollection<User>("Users");
        }

        public async Task SaveAsync(string token, string username, DateTime expiry)
        {
            var update = Builders<User>.Update
                .Set(u => u.RefreshToken, token)
                .Set(u => u.Expiry, expiry);

            var result = await _usersCollection.UpdateOneAsync(
                u => u.Username == username,
                update
            );

            if (result.MatchedCount == 0)
            {
                Console.WriteLine($"[SaveAsync] No user found with username: {username}");
            }
        }

        public async Task<(bool success, string? username)> TryGetUserAsync(string token)
        {
            var user = await _usersCollection
                .Find(u => u.RefreshToken == token)
                .FirstOrDefaultAsync();

            if (user == null)
                return (false, null);

            if (user.Expiry >= DateTime.UtcNow)
            {
                Console.WriteLine($"[TryGetUserAsync] Token valid for user: {user.Username}");
                return (true, user.Username);
            }

            // Token expired — remove it
            var update = Builders<User>.Update
                .Set(u => u.RefreshToken, string.Empty)
                .Set(u => u.Expiry, DateTime.MinValue);

            await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);

            Console.WriteLine($"[TryGetUserAsync] Token expired for user: {user.Username}");
            return (false, null);
        }

        public async Task RemoveAsync(string token)
        {
            var update = Builders<User>.Update
                .Set(u => u.RefreshToken, string.Empty)
                .Set(u => u.Expiry, DateTime.MinValue);

            var result = await _usersCollection.UpdateOneAsync(
                u => u.RefreshToken == token,
                update
            );

            if (result.ModifiedCount > 0)
            {
                Console.WriteLine($"[RemoveAsync] Token removed.");
            }
            else
            {
                Console.WriteLine($"[RemoveAsync] No matching token found to remove.");
            }
        }

        public async Task UpdateTokenAsync(string oldToken, string newToken, DateTime newExpiry)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.RefreshToken, oldToken);
                var update = Builders<User>.Update
                    .Set(u => u.RefreshToken, newToken)
                    .Set(u => u.Expiry, newExpiry);

                var result = await _usersCollection.FindOneAndUpdateAsync(filter, update);

                if (result == null)
                {
                    Console.WriteLine($"[UpdateTokenAsync] No user found with old token: {oldToken}");
                }
                else
                {
                    Console.WriteLine($"[UpdateTokenAsync] Token updated for user: {result.Username}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateTokenAsync] Error: {ex.Message}");
            }
        }
    }
}
