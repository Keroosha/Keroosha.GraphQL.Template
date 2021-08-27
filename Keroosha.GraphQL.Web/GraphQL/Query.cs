using System.Linq;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Keroosha.GraphQL.Web.Auth;
using Mapster;

namespace Keroosha.GraphQL.Web.GraphQL
{
    public class Query
    {
        public string Token([Service] UserAuthManager authManager, string login, string password)
        {
            var res = authManager.Login(login, password);
            return res.Success ? res.Value.token : "hui";
        }

        [Authorize(Roles = new []{ "Admin" })]
        public AdminQL Admin(ClaimsPrincipal claims, [Service] UserAuthManager authManager)
        {
            var (user, _) = authManager.Auth(claims.Claims.First(x => x.Type == "Token").Value).Value;
            return user.Adapt<AdminQL>();
        }
    }
}