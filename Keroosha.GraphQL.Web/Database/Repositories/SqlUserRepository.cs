using System.Collections.Generic;
using System.Linq;
using Keroosha.GraphQL.Web.Models;
using Keroosha.GraphQL.Web.Models.Repositories;
using LinqToDB;

namespace Keroosha.GraphQL.Web.Database.Repositories
{
    internal class SqlUserRepository : IUserRepository
    {
        private readonly AppDbContextManager _db;

        public SqlUserRepository(AppDbContextManager db) => _db = db;

        public User GetById(int id) => _db.Exec(db => db.Users.First(x => x.Id == id));

        public User FindByLogin(string login) => _db.Exec(db => db.Users.FirstOrDefault(x => x.Email == login));

        public int Create(string email, string name, string passwordHash) =>
            _db.Exec(d => d.InsertWithInt32Identity(new User
            {
                Email = email,
                Name = name,
                PasswordHash = passwordHash
            }));

        public User FindByConfirmCode(string code) =>
            _db.Exec(db => db.Users.FirstOrDefault(x => x.ConfirmationCode == code));

        public void Update(User user) => _db.Exec(d => d.Update(user));

        public List<User> GetByIds(IEnumerable<int> ids) => _db.Exec(d =>
            d.Users.Where(u => ids.Contains(u.Id))
                .ToList());
    }
}