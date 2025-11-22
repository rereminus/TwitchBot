namespace TwitchBot
{
    public class AuthSettings
    {
        public string? ClientId {  get; set; }
        public string? ClientSecret { get; set; }
        public string? Username { get; set; }

        public AccessToken? AccessToken { get; set; }

        public AuthSettings() { }
    }

    public class AccessToken
    {
        public string? Token { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}
