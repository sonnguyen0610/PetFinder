using System;
namespace PetFinderAPI.Models
{
    public class PetCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PetCategoryDto(PetCategory category)
        {
            Id = category.Id;
            Name = category.Name;
            Description = category.Description;
        }

        public PetCategoryDto()
        {
        }
    }
}
