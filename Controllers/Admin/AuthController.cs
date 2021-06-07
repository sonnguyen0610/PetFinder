using Newtonsoft.Json;
using PetFinderAPI.App_Data;
using PetFinderAPI.Controllers.Api;
using PetFinderAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PetFinderAPI.Controllers.Admin
{
    public class AuthController : Controller
    {
        // GET: Authentication
        public ActionResult Index()
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

        public ActionResult Login()
        {

            return View();
        }
        public ActionResult Logout()
        {
            Session.Remove("user");
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("/Login");
        }
        [HttpPost]
        public ActionResult Login(string email, string password)
        {

            using (var ctx = new PetContext())
            {
                var login = ctx.Users.Where(u => u.Email.Equals(email) && u.RoleId == 2).FirstOrDefault();
                if (login == null)
                {
                    ViewBag.ErrorMessage = "Username is not exits";
                }
                else
                {
                    bool verified = Crypto.VerifyHashedPassword(login.Password, password);
                    if (!verified)
                    {
                        ViewBag.ErrorMessage = "Password is wrong";
                    }
                    else
                    {
                        //var token =JwtManager.GenerateToken(login.UserName,login.Email,1);
                        //Session.Add("token", token);
                        Session.Add("user", login);
                        return Redirect("~/admin");
                    }
                }
            }

            return View();
        }

        public JsonResult CheckUsername(string email)
        {
            string retval = "";

            using (var ctx = new PetContext())
            {

                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);

                if (!match.Success)
                {
                    retval = "email";
                }
                else if (ctx.Users.Any(r => r.Email == email))
                {
                    retval = "true";
                }
                else
                {
                    retval = "false";
                }
            }
            return Json(retval, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Forgot_password()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Forgot_password(string email)
        {
            using (var ctx = new PetContext())
            {
                var u = ctx.Users.Where(user => user.Email.Equals(email)).FirstOrDefault();

                if (u == null)
                {
                    @ViewBag.ErrorMessage = "Email is not exits";
                }

                var tokens = JwtManager.GenerateToken(u.Id, 10080);
                var message = new MailMessage();
                message.To.Add(new MailAddress(email));
                message.From = new MailAddress("badao231199@gmail.com");
                message.Subject = "Forgot Password";
                message.Body = string.Format("Hi !\r\nFollow this link to change password : \r\nhttps://localhost:44318/admin/Auth/ConfirmEmailForgot?token={0}", tokens);

                message.IsBodyHtml = true;
                using (var smtp = new SmtpClient())
                {
                    await smtp.SendMailAsync(message);
                    await Task.FromResult(0);
                    return Redirect("https://mail.google.com/");
                }
                return View();
            }
            return View();
        }
        public ActionResult Error()
        {

            return View();
        }
        public static string CreatePassword(int length = 12)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string number = "1234567890";

            var middle = length / 2;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                if (middle == length)
                {
                    res.Append(number[rnd.Next(number.Length)]);
                }
                else
                {
                    if (length % 2 == 0)
                    {
                        res.Append(lower[rnd.Next(lower.Length)]);
                    }
                    else
                    {
                        res.Append(upper[rnd.Next(upper.Length)]);
                    }
                }
            }
            return res.ToString();
        }
        [HttpGet]
        public async Task<ActionResult> ConfirmEmailForgot( string token)
        {
            var simplePrinciple = JwtManager.GetPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            var EmailClaim = identity.FindFirst(ClaimTypes.Sid);
            var id = EmailClaim?.Value;
            int iduser = Int32.Parse(id);
            //bool validatedToken = JwtManager.ValidateToken(token);
            //if (validatedToken)
            //{
            using (var ctx = new PetContext())
                {
                    var user = ctx.Users.Where(u => (u.Id== iduser)).FirstOrDefault();
                    if (user != null)
                    {
                        string newPassword = CreatePassword();
                        user.Password = Crypto.HashPassword(newPassword);
                        ctx.SaveChanges();
                        var message = new MailMessage();
                        message.To.Add(new MailAddress(user.Email));
                        message.From = new MailAddress("badao231199@gmail.com");
                        message.Subject = "New Password";
                        message.Body = "The new password is " + newPassword;

                        message.IsBodyHtml = true;
                        using (var smtp = new SmtpClient())
                        {
                            await smtp.SendMailAsync(message);
                            await Task.FromResult(0);
                            return Redirect("~/admin/Auth/Login");
                        }

                    }
                    else return Redirect("~/admin/Auth/Error");

                //}
            }
            return Redirect("~/admin/Auth/Error");
        }
        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(String oldPassword, String newPassword)
        {

            using (var ctx = new PetContext())
            {
                var u = Session["user"] as PetFinderAPI.Models.User;
                if (u != null)
                {

                    int idUser = u.Id;
                    User user = ctx.Users.Find(idUser);

                    if (user != null)
                    {
                        bool verified = Crypto.VerifyHashedPassword(user.Password, oldPassword);
                        if (verified)
                        {
                            user.Password = Crypto.HashPassword(newPassword);
                            ctx.SaveChanges();

                            return Redirect("~/admin");
                        }
                        else return View();
                        //return Ok("Password is wrong");
                    }
                }

            }
            return View();
        }
    }
}