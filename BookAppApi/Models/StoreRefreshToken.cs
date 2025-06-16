namespace BookAppApi.Models
{
    public class StoredRefreshToken
    {
        public string Username { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }

        
    }
}