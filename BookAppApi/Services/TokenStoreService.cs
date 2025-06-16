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
                if (storedToken.Expiry >= DateTime.UtcNow)
                {
                    username = storedToken.Username;
                    return true;
                }
                _refreshTokens.Remove(token);
                username = null;
                return false;
            }
            username = null;
            return false;
        }

        public void Remove(string token)
        {
            _refreshTokens.Remove(token);
        }
    }
}
