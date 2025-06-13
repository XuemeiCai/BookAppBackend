namespace BookAppApi.Services
{
    public class TokenStoreService
    {
        private readonly Dictionary<string, string> _refreshTokens = new();

        public void Save(string token, string username)
        {
            _refreshTokens[token] = username;
        }

        public bool TryGetUser(string token, out string username)
        {
            return _refreshTokens.TryGetValue(token, out username!);
        }

        public void Remove(string token)
        {
            _refreshTokens.Remove(token);
        }
    }
}
