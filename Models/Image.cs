using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetFinderAPI.Models
{
    public class Image
    {
        public int Id { get; set; }
        public String Url { get; set; }
        [Required(ErrorMessage = "Post is requied!")]
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public Image()
        {
        }
    }
}
