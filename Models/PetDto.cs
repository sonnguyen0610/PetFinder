using System;
namespace PetFinderAPI.Models
{
    public class PetDto
    {

        public int Id { get; set; } // IdPet
        public PetOwerDto Owner { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; } // chung loai
        public string Bio { get; set; }
        public PetCategoryDto Category { get; set; }
        public int Age { get; set; }
        public DateTime? Birthday { get; set; }

        public Pet.GenderNames Gender { get; set; }
        public string Color { get; set; }
        public string Weight { get; set; }
        public AddressDto Address { get; set; }
        public bool IsFollowed { get; set; }
        public Boolean IsActive { get; set; }

        private int CalculateAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null)
            {
                return 0;
            }

            DateTime now = DateTime.Now;
            return (now.Month - dateOfBirth?.Month) + 12 * (now.Year - dateOfBirth?.Year) ?? 0;
        }


        public PetDto(Pet pet)
        {
            Id = pet.Id;
            Address = new AddressDto(pet.Address);
            Age = CalculateAge(pet.Birthday);
            Avatar = pet.Avatar;
            Bio = pet.Bio;
            Birthday = pet.Birthday;
            Breed = pet.Breed;
            Color = pet.Color;
            Gender = pet.Gender;
            Name = pet.Name;
            Category = new PetCategoryDto(pet.PetCategory);
            Owner = new PetOwerDto(pet.User);
            Weight = pet.Weight;
            IsFollowed = false;
        }

        public PetDto()
        {
        }
    }
}
