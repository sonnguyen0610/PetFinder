using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PetFinderAPI.App_Data;
using PetFinderAPI.Models;
using PagedList;

namespace PetFinderAPI.Controllers.Admin

{

    public class UsersController : Controller
    {
        private PetContext _db = new PetContext();
        public ActionResult Index(int? page)
        {
            List<Role> role = Models.Role.GetRoles();
            ViewBag.Role = role;
            List<User> list = Models.User.GetUsers();
            if (page == null) page = 1;

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Store()
        {
            ViewBag.Roles = new SelectList(_db.Roles, "Name", "Name");
            List<Role> roles = Models.Role.GetRoles();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public ActionResult Store(User _user)
        {
            Debug.WriteLine("USER: " + ModelState.Values.ToString());
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

                    var email = _user.Email;
                    var count = _db.Users.Where(s => s.Email.Equals(email)).Count();

                    if (count > 0)
                    {
                        ViewBag.message = "Email already exists";
                        ViewBag.Roles = new SelectList(_db.Roles, "Name", "Name");
                        List<Role> roles = Models.Role.GetRoles();
                        ViewBag.Roles = roles;
                        return View();
                    }
                    if (_user.RoleId == 0)
                    {
                        ViewBag.message = "Role is required!";
                        ViewBag.Roles = new SelectList(_db.Roles, "Name", "Name");
                        List<Role> roles = Models.Role.GetRoles();
                        ViewBag.Roles = roles;
                        return View();
                    }
                    if (String.IsNullOrEmpty(_user.Password))
                    {
                        ViewBag.message = "Password is required!";
                        ViewBag.Roles = new SelectList(_db.Roles, "Name", "Name");
                        List<Role> roles = Models.Role.GetRoles();
                        ViewBag.Roles = roles;
                        return View();
                    }

                    HttpPostedFileBase ImageUrl = Request.Files["Avatar"];
                    if (ImageUrl != null && ImageUrl.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(ImageUrl.FileName);
                        Console.WriteLine("name" + fileName);
                        var path = Path.Combine(Server.MapPath("~/Content/img/uploads/"), fileName);
                        ImageUrl.SaveAs(path);
                        _user.Avatar = fileName;
                    }
                    ViewBag.user = _user;

                    Debug.WriteLine("USER: " + _user.ToString());
                    String hashPass = Crypto.HashPassword(_user.Password);
                    _user.Password = hashPass;
                    _db.Users.Add(_user);
                    _db.SaveChanges();
                    return RedirectToAction("Index");

                }
                else
                {
                    ViewBag.message = "Insert failed!";
                    ViewBag.Roles = new SelectList(_db.Roles, "Name", "Name");
                    List<Role> roles = Models.Role.GetRoles();
                    ViewBag.Roles = roles;
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
        public ActionResult Edit(int id)
        {
            var data = _db.Users.Where(s => s.Id == id).FirstOrDefault();

            List<Role> roles = Models.Role.GetRoles();
            ViewBag.Roles = roles;
            data.Password = "";
            ViewBag.user = data;

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,DateOfBirth,Phone,Gender,Email,Password,New_Password,Avatar,Address,RoleId,IsActive")] User user)
        {
            //Debug.WriteLine("Path: " + file.ContentLength);
            HttpPostedFileBase ImageUrl = Request.Files["Avatar"];
            if (ImageUrl != null && ImageUrl.ContentLength > 0)
            {
                var fileName = Path.GetFileName(ImageUrl.FileName);
                Console.WriteLine("name" + fileName);
                var path = Path.Combine(Server.MapPath("~/Content/img/uploads/"), fileName);
                ImageUrl.SaveAs(path);
                user.Avatar = fileName;
            }


            if (ModelState.IsValid)
            {
                var data = _db.Users.Where(s => s.Id == user.Id).FirstOrDefault();

                if (!(String.IsNullOrEmpty(user.Password)))
                {

                    if (Crypto.VerifyHashedPassword(data.Password, user.Password))
                    {
                        user.Password = Crypto.HashPassword(user.New_Password);
                        _db.Entry(data).State = EntityState.Detached;
                        _db.Entry(user).State = EntityState.Modified;
                        _db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        List<Role> roles2 = Models.Role.GetRoles();
                        ViewBag.Roles = roles2;
                        ViewBag.message = "Current password is fail!";
                        ViewBag.user = user;
                        return View(user);
                    }
                }

                //var data = _db.Users.Where(s => s.Id == user.Id).FirstOrDefault();
                try
                {
                    if (data != null)
                    {
                        _db.Entry(data).State = EntityState.Detached;
                    }
                    user.Password = data.Password;
                    //user.New_Password = data.Password;
                    _db.Entry(user).State = EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index");

                }
                catch (Exception e)
                {

                    Debug.WriteLine(" mess: " + e.Message);
                    return RedirectToAction("Index");

                }

            }
            List<Role> roles = Models.Role.GetRoles();
            ViewBag.Roles = roles;
            ViewBag.user = user;

            return View(user);
        }
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            var user = _db.Users.Where(u => u.Id == id).First();
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public JsonResult GetUserID(int userID)
        {
            var user = _db.Users.Where(u => u.Id == userID).Include("Role").ToList();

            return Json(user, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUserName(string name)
        {
            var user = _db.Users.Where(u => u.UserName.Contains(name)).Include("Role").ToList();
            return Json(user, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUserRole(int idRole)
        {
            var role = _db.Users.Where(u => u.RoleId == idRole).Include("Role").ToList();
            return Json(role, JsonRequestBehavior.AllowGet);
        }
        public async Task<string> ResetPass(String id)
        {
            using (var ctx = new PetContext())
            {
                int Iduser = int.Parse(id);
                var user = ctx.Users.Where(u => (u.Id == Iduser)).FirstOrDefault();
                if (user != null)
                {
                    user.Password = Crypto.HashPassword("0000");
                    ctx.SaveChanges();
                    return "Reset password success";
                }
                else return "Reset password not success";
            }
        }
        public async Task<string> DeletePassword(String id)
        {
            int Iduser = int.Parse(id);
            using (var ctx = new PetContext())
            {
                var user = ctx.Users.Where(u => (u.Id == Iduser)).FirstOrDefault();
                if (user != null)
                {
                    user.Password = Crypto.HashPassword("");
                    ctx.SaveChanges();
                    return "Delete password success";
                }
                else return "Delete password not success";
            }
        }
    }


}