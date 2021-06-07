﻿using System;
using System.Collections.Generic;
using System.Linq;
using PetFinderAPI.App_Data;

namespace PetFinderAPI.Models
{
    public class PostCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public PostCategory()
        {
        }
        public static List<PostCategory> getPostCategory()
        {
            using (var ctx = new PetContext())
            {
                return ctx.PostCategories.ToList();
            }
        }
    }
}
