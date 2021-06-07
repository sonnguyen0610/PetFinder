using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PetFinderAPI.App_Data;
using System.Linq;
using System.ComponentModel;

namespace PetFinderAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Email required")]
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public List<FcmToken> Tokens { get; set; }

        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Role is requied!")]
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        [DefaultValue(1)]
        public Role Role { get; set; }
        //người dùng phải nhập một chữ thường, một chữ hoa, một chữ số, với một ký tự đặt biệt
        //[Required]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$")]
        public string Password { get; set; }
        [NotMapped]
        //[Required]
        public string New_Password { get; set; }
        [DefaultValue(false)]
        public Boolean IsActive { get; set; }
        public List<PetLikes> PetLikes { get; set; }
        public List<Pet> Pets { get; set; }
        public User()
        {
        }

        public static List<User> GetUsers()
        {
            using (var ctx = new PetContext())
            {
                return ctx.Users.Include("Role").ToList();
            }
        }

        public override string ToString()
        {
            return "username: " + UserName + "email: " + Email;
        }
    }
}
