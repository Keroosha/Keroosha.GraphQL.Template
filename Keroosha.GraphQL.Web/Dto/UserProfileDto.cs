using Keroosha.GraphQL.Web.Models;

namespace Keroosha.GraphQL.Web.Dto
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public long AvatarImage { get; set; }

        public static UserProfileDto FromUser(User user)
        {
            return new()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                AvatarImage = user.AvatarImage
            };
        }
    }
}