namespace Keroosha.GraphQL.Web.Config
{
    public enum TokenStorageType
    {
        JWT,
        InMemory
    }

    public class TokenConfiguration
    {
        public TokenStorageType Type { get; set; } = TokenStorageType.InMemory;
        public JWTConfiguration JWT { get; set; } = new();
    }

    public class JWTConfiguration
    {
        public string Key { get; set; }
    }
}