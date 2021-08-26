using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Dto;
using Keroosha.GraphQL.Web.Models.Repositories;

namespace Keroosha.GraphQL.Web.Application
{
    public interface IRolesAppService
    {
        public Task<List<UserRoleDto>> GetUserRolesByIds(List<int> ids);
    }

    public class RolesAppService : IRolesAppService
    {
        private readonly IRoleRepository _roleRepository;

        public RolesAppService(IRoleRepository roleRepository) => _roleRepository = roleRepository;

        public async Task<List<UserRoleDto>> GetUserRolesByIds(List<int> ids)
        {
            await Task.Yield();
            return _roleRepository.UserRolesByIds(ids.ToArray())
                .Select(UserRoleDto.FromModel)
                .ToList();
        }
    }
}