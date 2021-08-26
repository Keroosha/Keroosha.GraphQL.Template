using Keroosha.GraphQL.Web.Models;

namespace Keroosha.GraphQL.Web.Dto
{
    public class UserRegisterFormDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public User ToUser(int id, string passwordHash)
        {
            return new()
            {
                Id = id,
                Email = Email,
                PasswordHash = passwordHash
            };
        }

        public void Deconstruct(out string email, out string password)
        {
            email = Email;
            password = Password;
        }
    }
}