using PetFinderAPI.App_Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;

namespace PetFinderAPI.Models
{
    public class Pet
    {
        public enum GenderNames
        {
            Male,
            Female,
            Other
        }
        public int Id { get; set; }
        [Required(ErrorMessage = "UserId")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required(ErrorMessage = "UserId")]
        public int PetCategoryId { get; set; }
        [ForeignKey("PetCategoryId")]
        public PetCategory PetCategory { get; set; }

        [Required(ErrorMessage = "Name")]

        public string Name { get; set; }
        public string Breed { get; set; } // chung loai
        public string Bio { get; set; }
        public DateTime? Birthday { get; set; }
        // tinh ra age
        public GenderNames Gender { get; set; }
        public string Avatar { get; set; }
        public string Color { get; set; }
        public string Weight { get; set; }
        public string Address { get; set; }
        public Boolean IsActive { get; set; }
        // 1:can cho
        // 2: mating: giao phoois
        // 3: bao mat 

        public Pet(PetDto petDto)
        {
            this.Id = petDto.Id;
            this.PetCategoryId = petDto.Category.Id;
            this.Name = petDto.Name;
            this.Breed = petDto.Breed;
            this.Bio = petDto.Bio;
            this.Birthday = petDto.Birthday;
            this.Gender = petDto.Gender;
            this.Avatar = petDto.Avatar;
            this.Color = petDto.Color;
            this.Weight = petDto.Weight;
            this.Address = petDto.Address.Address;
            this.IsActive = petDto.IsActive;

        }
        public Pet()
        {
        }
        public static List<Pet> GetPets()
        {
            using (var ctx = new PetContext())
            {
                return ctx.Pets.Include("User").ToList();
            }
        }
        public static List<Pet> GetCategory()
        {
            using (var ctx = new PetContext())
            {
                return ctx.Pets.Include("PetCategory").ToList();
            }
        }
    }
}

