using System;
namespace PetFinderAPI.Models
{
    public class PostCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PostCategoryDto(PostCategory category)
        {
            Id = category.Id;
            Name = category.Name;
            Description = category.Description;
        }

        public PostCategoryDto()
        {
        }
    }
}
