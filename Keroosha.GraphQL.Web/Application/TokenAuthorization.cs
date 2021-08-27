using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Keroosha.GraphQL.Web.Application
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly UserAuthManager _auth;

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, UserAuthManager auth) : base(options, logger, encoder, clock)
        {
            _auth = auth;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // validation comes in here
            if (!Request.Headers.ContainsKey("X-Token")) return AuthenticateResult.Fail("Header Not Found.");
            var token = Request.Headers["X-Token"].ToString();
            var res = _auth.Auth(token);
            if (res is null) return AuthenticateResult.Fail("Not authorized");
            var (user, _) = res.Value;

            var roles = _auth.Roles(token);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Id", user.Id.ToString()),
                new Claim("Token", token)
            }.Concat(roles.Select(x => new Claim(ClaimTypes.Role, x)));
            
            var claimsIdentity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
            
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(claimsIdentity), Scheme.Name);
            
            return AuthenticateResult.Success(ticket);
        }
    }
}