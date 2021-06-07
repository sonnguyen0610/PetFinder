using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFinderAPI.Models
{
    public class PostLikes
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }


        public PostLikes()
        {
        }
    }
}
