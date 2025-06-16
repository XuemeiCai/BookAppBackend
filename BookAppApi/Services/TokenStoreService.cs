using BookAppApi.Models;

namespace BookAppApi.Services
{
    public class TokenStoreService
    {
        private readonly Dictionary<string, StoredRefreshToken> _refreshTokens = new();

        public void Save(string token, string username, DateTime expiry)
        {
            _refreshTokens[token] = new StoredRefreshToken
            {
                Username = username,
                Expiry = expiry
            };
        }

        public bool TryGetUser(string token, out string? username)
        {
            if (_refreshTokens.TryGetValue(token, out var storedToken))
            {
                Console.WriteLine("current " + DateTime.UtcNow);
                Console.WriteLine("expiry " + storedToken.Expiry);
                if (storedToken.Expiry >= DateTime.UtcNow)
                {
                    username = storedToken.Username;
                    Console.WriteLine("find user success ");
                    return true;
                }
                Console.WriteLine("find user time issue");
                _refreshTokens.Remove(token);
                username = null;
                return false;
            }
            Console.WriteLine("not found user");
            username = null;
            return false;
        }

        public void Remove(string token)
        {
            _refreshTokens.Remove(token);
        }
    }
}
