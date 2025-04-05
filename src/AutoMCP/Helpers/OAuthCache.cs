namespace AutoMCP.Helpers
{
    public class OAuthCache
    {
        private string? _token;
        private DateTimeOffset _expiry = DateTimeOffset.MinValue;

        public string? GetToken()
        {
            if (_token != null && DateTimeOffset.UtcNow < _expiry)
            {
                return _token;
            }
            return null;
        }

        public void SetToken(string token, int expiresIn = 3600)
        {
            _token = token;
            _expiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        }
    }
}
