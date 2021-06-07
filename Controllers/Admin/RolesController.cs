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
    public class RolesController : Controller
    {
        // GET: Roles
        private PetContext _db = new PetContext();
        public ActionResult Index()
        {
            List<Role> roles = Models.Role.GetRoles();
            ViewBag.Roles = roles;
            return View();
        }

        public ActionResult Store()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Store(Role _role)
        {
            if (ModelState.IsValid)
            {
                _role.Id = 1;
                _db.Roles.Add(_role);
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
            var data = _db.Roles.Where(s => s.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Name,Description,IsActive")] Role role)
        {
            Debug.WriteLine("Role: " + ModelState.Values.ToString());

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
                var _role = _db.Roles.Find(id);
                _role.Name = role.Name;
                _role.Description = role.Description;
                _role.IsActive = role.IsActive;

                _db.Entry(_role).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(role);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            var role = _db.Roles.Where(u => u.Id == id).First();
            _db.Roles.Remove(role);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}