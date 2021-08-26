using System.Collections.Generic;

namespace Keroosha.GraphQL.Web.Models.Repositories
{
    public interface IRoleRepository
    {
        void AttachRole(int userId, string role);
        void AttachRoles(int userId, List<UserRole> roles);
        void RemoveRole(int id);
        List<UserRole> UserRolesByIds(params int[] userId);
        bool UserHasRole(int userId, string role);
    }
}