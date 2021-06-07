using PetFinderAPI.App_Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PetFinderAPI.Models
{
    public class PetCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public PetCategory()
        {
        }
        public static List<PetCategory> GetPetCategories()
        {
            using (var ctx = new PetContext())
            {
                return ctx.PetCategories.ToList();
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
