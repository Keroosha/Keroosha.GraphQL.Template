using System.Collections.Generic;

namespace Keroosha.GraphQL.Web.Models.Repositories
{
    public interface IUserRepository : IPasswordAuthRepository<User>
    {
        int Create(string email, string name, string passwordHash);
        void Update(User user);
        User FindByConfirmCode(string code);
        List<User> GetByIds(IEnumerable<int> ids);
    }
}