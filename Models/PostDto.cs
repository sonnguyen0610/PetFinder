using System;
using System.Collections.Generic;

namespace PetFinderAPI.Models
{
    public class PostDto
    {

        public int Id { get; set; }
        public PetDto Pet { get; set; }
        public String Content { get; set; }
        public List<ImageDto> Images { get; set; } // list image 


        public PostCategoryDto Category { get; set; } // 1 category -> getName

        public Boolean IsLiked;
        public double DistanceFromUser; // khoang cach (gps of user -> address of pet)
        public DateTime CreatedAt;
        public Boolean IsActive;

        public PostDto(Post post)
        {
            Id = post.Id;
            Pet = new PetDto(post.Pet);
            Content = post.Content;
            Images = post.Images != null && post.Images.Count != 0 ? post.Images.ConvertAll(i => new ImageDto(i)
            ) : new List<ImageDto> { };
            Category = new PostCategoryDto(post.PostCategory);
            CreatedAt = post.CreatedAt;
            IsActive = post.IsActive;
            IsLiked = false;
            DistanceFromUser = 0;
        }

        public PostDto()
        {

        }

    }
}
