using System;
namespace PetFinderAPI.Models
{
    public class PetOwerDto
    {


        public int Id { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }

        public PetOwerDto(User user)
        {
            Id = user.Id;
            Avatar = user.Avatar;
            UserName = user.UserName;
        }

        public PetOwerDto()
        {
        }
    }
}
