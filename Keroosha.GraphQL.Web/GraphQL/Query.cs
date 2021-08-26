using System.Security.Claims;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Keroosha.GraphQL.Web.Auth;
using Keroosha.GraphQL.Web.Dto;
using Mapster;

namespace Keroosha.GraphQL.Web.GraphQL
{
    public class Query
    {
        public UserProfileDto Me([Service] UserAuthManager authManager, string login, string password)
        {
            var res = authManager.Login(login, password);
            return res.Success ? res.Value.user.Adapt<UserProfileDto>() : new UserProfileDto();
        }
        
        public string Token([Service] UserAuthManager authManager, string login, string password)
        {
            var res = authManager.Login(login, password);
            return res.Success ? res.Value.token : "hui";
        }

        [Authorize]
        public string Test(ClaimsPrincipal claims)
        {
            return "ok";
        }
    }
}