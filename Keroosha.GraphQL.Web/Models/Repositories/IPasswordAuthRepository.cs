namespace Keroosha.GraphQL.Web.Models.Repositories
{
    public interface IPasswordAuthRepository<TUser> where TUser : IHaveId, IHavePasswordAuth
    {
        public TUser GetById(int id);
        public TUser FindByLogin(string login);
    }
}