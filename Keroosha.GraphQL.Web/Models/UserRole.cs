using LinqToDB.Mapping;

namespace Keroosha.GraphQL.Web.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";

        public static readonly string[] ValidRolesList =
        {
            Admin
        };
    }

    public class UserRole
    {
        [PrimaryKey] [Identity] public int Id { get; set; }

        [Column] public int UserId { get; set; }

        [Column] public string Role { get; set; }
    }
}