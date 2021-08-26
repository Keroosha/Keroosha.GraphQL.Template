using System.Collections.Generic;
using System.Linq;
using Keroosha.GraphQL.Web.Models;
using Keroosha.GraphQL.Web.Models.Repositories;
using LinqToDB;
using LinqToDB.Data;

namespace Keroosha.GraphQL.Web.Database.Repositories
{
    public class SqlRoleRepository : IRoleRepository
    {
        private readonly AppDbContextManager _db;

        public SqlRoleRepository(AppDbContextManager db) => _db = db;

        public List<UserRole> UserRolesByIds(params int[] userIds) => _db.Exec(db =>
            db.UserRoles.Where(x => userIds.Contains(x.UserId))
                .ToList());

        public bool UserHasRole(int userId, string role) => _db.Exec(db =>
            db.UserRoles.Where(x => x.UserId == userId)
                .Select(x => x.Role)
                .Contains(role));

        public void AttachRoles(int userId, List<UserRole> roles) =>
            _db.Exec(db =>
            {
                using var transaction = db.Connection.BeginTransaction();
                db.UserRoles.Delete(x => x.UserId == userId);
                db.BulkCopy(roles);
                transaction.Commit();
            });

        public void RemoveRole(int id) => _db.Exec(db => db.UserRoles.Delete(x => x.Id == id));

        public void AttachRole(int userId, string role) =>
            _db.Exec(db =>
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null) return;
                db.Insert(new UserRole
                {
                    Role = role,
                    UserId = userId
                });
            });
    }
}