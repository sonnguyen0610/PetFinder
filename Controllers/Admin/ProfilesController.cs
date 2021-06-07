using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PetFinderAPI.App_Data;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Admin
{
    public class ProfilesController : Controller
    {
        // GET: Profiles
        private PetContext _db = new PetContext();
        public ActionResult index()
        {
            if (Session["user"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("admin/Auth/Login");
            }
        }

        public ActionResult MyProfile(int id)
        {
            if (Session["user"] != null)
            {
                var data = _db.Users.Where(s => s.Id == id).FirstOrDefault();

                ViewBag.user = data;

                return View(data);
            }
            else
            {
                return RedirectToAction("admin/Auth/Login");
            }
        }

        public ActionResult EditProfile(int id)
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
            var user = _db.Users.Where(u => u.Id == userID).ToList();

            return Json(user, JsonRequestBehavior.AllowGet);
        }
    }


}