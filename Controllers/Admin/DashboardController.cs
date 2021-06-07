using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PetFinderAPI.Controllers.Admin
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {

            using (var ctx = new App_Data.PetContext())
            {
                ViewBag.TotalUsers = ctx.Users.Count();
                ViewBag.TotalPets = ctx.Pets.Count();
                ViewBag.TotalPosts = ctx.Posts.Count();

                ViewBag.Users = ctx.Users.Take(6).ToList();
                ViewBag.Posts = ctx.Posts.Include("Pet.PetCategory").Include("PostCategory").Take(5).ToList();

                ViewBag.Pets = ctx.Pets.Include("PetCategory").Take(4).ToList();


            }

            return View();
        }
    }
}
