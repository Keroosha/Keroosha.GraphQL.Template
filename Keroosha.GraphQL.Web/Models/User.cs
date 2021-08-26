using LinqToDB.Mapping;

namespace Keroosha.GraphQL.Web.Models
{
    public interface IHaveId
    {
        public int Id { get; set; }
    }

    public interface IHavePasswordAuth
    {
        public string Login { get; }
        public string PasswordHash { get; }
    }

    [Table("Users")]
    public class User : IHaveId, IHavePasswordAuth
    {
        [PrimaryKey, Identity] public int Id { get; set; }
        [Column] public string Name { get; set; }

        [Column, Nullable] public string Email { get; set; }
        string IHavePasswordAuth.Login => Email;

        [Column, Nullable] public bool Confirmed { get; set; }
        [Column, Nullable] public string ConfirmationCode { get; set; }

        [Column, Nullable] public string PasswordHash { get; set; }
        [Column, Nullable] public long AvatarImage { get; set; }
    }
}