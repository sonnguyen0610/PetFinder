using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFinderAPI.Models
{
    public class PetLikes
    {
        public int Id { get; set; }


        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public Pet Pet { get; set; }

        public PetLikes()
        {
        }
    }
}
