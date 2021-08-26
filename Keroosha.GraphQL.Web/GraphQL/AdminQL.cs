using System.Collections.Generic;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Types;
using Keroosha.GraphQL.Web.Auth;

namespace Keroosha.GraphQL.Web.GraphQL
{
    
    [ExtendObjectType(typeof(AdminQL))]
    public class AdminQLExtensions
    {
    }
    
    public class AdminQL
    {
        public long Id { get; set; }
        public string Email { get; set; }
    }

    public class AdminRolesQL
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}