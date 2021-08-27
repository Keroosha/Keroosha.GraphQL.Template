using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Keroosha.GraphQL.Web.Auth;

namespace Keroosha.GraphQL.Web.GraphQL
{
    public class AdminQL
    {
        public long Id { get; set; }
        public string Email { get; set; }
        [Authorize(Roles = new[] {"Admin"})]
        public IEnumerable<string> Roles(ClaimsPrincipal claims, [Service] UserAuthManager authManager) =>
            authManager.Roles(claims.Claims.First(x => x.Type == "Token").Value);
    }
}