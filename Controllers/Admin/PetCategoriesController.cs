using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PetFinderAPI.App_Data;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Admin
{
    public class PetCategoriesController : Controller
    {
        // GET: PetCategories
        private PetContext _db = new PetContext();
        public ActionResult Index()
        {
            List<PetCategory> petcategories = Models.PetCategory.GetPetCategories();
            ViewBag.PetCategories = petcategories;
            return View();
        }
        public ActionResult Store()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Store(PetCategory _petcategory)
        {
            if (ModelState.IsValid)
            {
                _petcategory.Id = 1;
                _db.PetCategories.Add(_petcategory);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Insert failed!";
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            var data = _db.PetCategories.Where(s => s.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Name,Description,IsActive")] PetCategory petcategory)
        {
            Debug.WriteLine("PetCategory: " + ModelState.Values.ToString());

            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValidField("Name") && ModelState.IsValidField("Description") && ModelState.IsValidField("IsActive"))
            {
                int id = int.Parse(this.RouteData.Values["id"].ToString());
                var _petcategory = _db.PetCategories.Find(id);
                _petcategory.Name = petcategory.Name;
                _petcategory.Description = petcategory.Description;
                _petcategory.IsActive = petcategory.IsActive;

                _db.Entry(_petcategory).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(petcategory);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            var petcategories = _db.PetCategories.Where(u => u.Id == id).First();
            _db.PetCategories.Remove(petcategories);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}