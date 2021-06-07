using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PetFinderAPI.App_Data;

namespace PetFinderAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public Pet Pet { get; set; }
        public String Content { get; set; }

        public int PostCategoryId { get; set; }
        [ForeignKey("PostCategoryId")]
        public PostCategory PostCategory { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }
        public Boolean IsActive { get; set; }



        public List<Image> Images { get; set; }

        // tao bang nhiu nhiu. de kiem tra user co like bai post nay hay k
        public Post()
        {
            this.CreatedAt = DateTime.Now;
        }
        public Post(PostDto postDto)
        {
            this.Id = postDto.Id;
            this.PetId = postDto.Pet.Id;
            this.Content = postDto.Content;
            this.PostCategoryId = postDto.Category.Id;
            this.IsActive = postDto.IsActive;

            this.CreatedAt = postDto.CreatedAt;

        }
        public static List<Post> GetPosts()
        {
            using (var ctx = new PetContext())
            {
                return ctx.Posts.Include("Pet").Include("PostCategory").ToList();
            }
        }

        public static List<Image> GetImages(int Id)
        {
            using (var ctx = new PetContext())
            {
                return ctx.Images.Where(i => i.PostId == Id).ToList();
            }

        }
    }
}