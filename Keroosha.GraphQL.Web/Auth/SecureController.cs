using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Keroosha.GraphQL.Web.Auth
{
    public abstract class SecureController : Controller
    {
        private readonly UserAuthManager _authManager;

        protected SecureController(UserAuthManager authManager)
        {
            _authManager = authManager;
        }

        protected User ApiUser { get; private set; }
        protected List<string> UserRoles { get; private set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (Token() is null)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var info = _authManager.Auth(Token());
            if (info is null)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            ApiUser = info.Value.user;
            UserRoles = _authManager.Roles(Token());

            await base.OnActionExecutionAsync(context, next);
        }

        protected bool CheckRoles(params string[] requiredRoles)
        {
            var roles = _authManager.Roles(Token());
            var valid = roles.Any(requiredRoles.Contains);
            return valid;
        }

        private string Token()
        {
            return HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split("Bearer ").Last();
        }
    }
}