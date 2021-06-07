using PetFinderAPI.App_Data;
using PetFinderAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace PetFinderAPI.Controllers.Admin
{
    public class PetsController : Controller
    {
        private PetContext _db = new PetContext();
        // GET: Pets
        public ActionResult Index(int? page)
        {
            var petsType = _db.PetCategories.ToList();
            ViewBag.PetType = petsType;
            List<Pet> petsList = Models.Pet.GetPets();
            ViewBag.Pet = petsList;
            if (page == null) page = 1;

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(petsList.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Store()
        {
            //List<User> users = Models.User.GetUsers();
            //ViewBag.Users = users;
            List<User> u = new List<User>();
            u = _db.Users.ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (User item_Currency in u)
            {
                listItems.Add(new SelectListItem()
                {
                    Value = item_Currency.Id.ToString(),
                    Text = item_Currency.UserName
                });
            }
            ViewBag.Users = new SelectList(listItems, "Value", "Text");

            //List<PetCategory> petCategories = Models.PetCategory.GetPetCategories();
            //ViewBag.Categories = petCategories;
            List<PetCategory> p = new List<PetCategory>();
            p = _db.PetCategories.ToList();
            List<SelectListItem> l = new List<SelectListItem>();
            foreach (PetCategory i in p)
            {
                l.Add(new SelectListItem()
                {
                    Value = i.Id.ToString(),
                    Text = i.Name
                });
            }
            ViewBag.PetCategory = new SelectList(l, "Value", "Text");


            return View();
        }

        [HttpPost]

        public ActionResult Store(Pet _pet)
        {
            HttpPostedFileBase ImageUrl = Request.Files["Avatar"];
            if (ImageUrl != null && ImageUrl.ContentLength > 0)
            {
                var fileName = Path.GetFileName(ImageUrl.FileName);
                Console.WriteLine("name" + fileName);
                var path = Path.Combine(Server.MapPath("~/Content/img/uploads/"), fileName);
                ImageUrl.SaveAs(path);
                _pet.Avatar = fileName;
            }
            Debug.WriteLine("Pet " + ModelState.Values.ToString());
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
            }
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Pets.Add(_pet);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.message = "Insert failed!";
                    return View();
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                ViewBag.message = "Insert failed vo!";
                return View();
            }

        }

        public ActionResult PetProfile(int id)
        {
            var data = _db.Pets.Where(s => s.Id == id).FirstOrDefault();
            return View(data);
        }
        public ActionResult EditPet(int id)
        {
            var data = _db.Pets.Where(s => s.Id == id).FirstOrDefault();
            List<User> u = new List<User>();
            u = _db.Users.ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (User item_Currency in u)
            {
                listItems.Add(new SelectListItem()
                {
                    Value = item_Currency.Id.ToString(),
                    Text = item_Currency.UserName
                });
            }
            ViewBag.Users = new SelectList(listItems, "Value", "Text");

            List<PetCategory> p = new List<PetCategory>();
            p = _db.PetCategories.ToList();
            List<SelectListItem> l = new List<SelectListItem>();
            foreach (PetCategory i in p)
            {
                l.Add(new SelectListItem()
                {
                    Value = i.Id.ToString(),
                    Text = i.Name
                });
            }
            ViewBag.PetCategory = new SelectList(l, "Value", "Text");
            ViewBag.pet = data;
            return View(data);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditPet([Bind(Include = "Id,Name,UserId,PetCategoryId,Breed,Bio,Birthday,Gender,Avatar,Color,Weight,Address,IsActive")] Pet pet)
        {
            Console.WriteLine("jhashfa" + Request.Files["Avatar"]);

            HttpPostedFileBase ImageUrl = Request.Files["Avatar"];
            if (ImageUrl != null && ImageUrl.ContentLength > 0)
            {
                var fileName = Path.GetFileName(ImageUrl.FileName);
                Console.WriteLine("name" + fileName);
                var path = Path.Combine(Server.MapPath("~/Content/img/uploads/"), fileName);
                ImageUrl.SaveAs(path);
                pet.Avatar = fileName;
            }
            var data = _db.Pets.Where(s => s.Id == pet.Id).FirstOrDefault();
            if (data != null)
            {
                _db.Entry(data).State = EntityState.Detached;
                _db.Entry(pet).State = EntityState.Modified;
                _db.SaveChanges();
                ViewBag.pet = pet;
                return RedirectToAction("Index");
            }
            ViewBag.pet = pet;
            return View(pet);
        }
        public ActionResult Delete(int? id)
        {
            var pet = _db.Pets.Where(p => p.Id == id).First();
            _db.Pets.Remove(pet);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetPetID(int petID)
        {
            //var pet = _db.Pets.Where(u => u.Id == petID).Include("U).ToList();
             var pet= _db.Pets.Where(p=> p.Id==petID).ToList();

            return Json(pet, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPetName(string name)
        {
            var pet = _db.Pets.Where(u => u.Name.Contains(name)).ToList();
            return Json(pet, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPetType(int idType)
        {
            var type = _db.Pets.Where(u => u.PetCategoryId == idType).ToList();
            return Json(type, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditProfile()
        {
            return View();
        }

    }
}