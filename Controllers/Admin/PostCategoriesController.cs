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
    public class PostCategoriesController : Controller
    {
        // GET: PostCategories
        private PetContext _db = new PetContext();
        public ActionResult Index()
        {
            List<PostCategory> postcategories = Models.PostCategory.getPostCategory();
            ViewBag.PostCategories = postcategories;
            return View();
        }
        public ActionResult Store()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Store(PostCategory _postcategory)
        {
            if (ModelState.IsValid)
            {
                _postcategory.Id = 1;
                _db.PostCategories.Add(_postcategory);
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
            var data = _db.PostCategories.Where(s => s.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Name,Description,IsActive")] PostCategory postcategory)
        {
            Debug.WriteLine("PostCategory: " + ModelState.Values.ToString());

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
                var _postcategory = _db.PostCategories.Find(id);
                _postcategory.Name = postcategory.Name;
                _postcategory.Description = postcategory.Description;
                _postcategory.IsActive = postcategory.IsActive;

                _db.Entry(_postcategory).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(postcategory);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            var postcategories = _db.PostCategories.Where(u => u.Id == id).First();
            _db.PostCategories.Remove(postcategories);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}