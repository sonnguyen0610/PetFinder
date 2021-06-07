using PetFinderAPI.App_Data;
using PetFinderAPI.Controllers.Api;
using PetFinderAPI.Filter;
using PetFinderAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;

namespace PetFinderAPI.Controllers
{
    public class AuthController : ApiController
    {
        [AllowAnonymous]
        [Route("api/account/register")]
        [HttpPost]
        public async Task<IHttpActionResult> Registers(User users)
        {
            using (var ctx = new PetContext())
            {
                var isRegister = ctx.Users.Where(u => u.Email.Equals(users.Email)).FirstOrDefault();
                if (isRegister == null)
                {
                    users.RoleId = 1;
                    users.Password = Crypto.HashPassword(users.Password);
                    ctx.Users.Add(users);
                    ctx.SaveChanges();
                    var tokens = JwtManager.GenerateToken(users.Id, 10080);
                        var message = new MailMessage();
                        message.To.Add(new MailAddress(users.Email));
                        message.From = new MailAddress("badao231199@gmail.com");
                        message.Subject = "Account Activation";
                        message.Body = string.Format("Hi !\r\nFollow this link to  : \r\nhttps://localhost:44318/api/account/confirmEmailToActive?token={0}", tokens);

                        message.IsBodyHtml = true;
                        using (var smtp = new SmtpClient())
                        {
                            await Task.FromResult(0);
                            await smtp.SendMailAsync(message);
                           
                            return Ok(users);
                        } 
                }
                else
                {
                    return Conflict();
                }
            }

        }

        [AllowAnonymous]
        [Route("api/account/confirmEmailToActive")]
        [HttpGet]
        public async Task<IHttpActionResult> ConfirmEmail(string token)
        {
            var simplePrinciple = JwtManager.GetPrincipal(token);
            try
            {
                var identity = simplePrinciple.Identity as ClaimsIdentity;

                var EmailClaim = identity.FindFirst(ClaimTypes.Sid);
                var id = EmailClaim?.Value;
                int iduser = Int32.Parse(id);

                using (var ctx = new PetContext())
                {
                    var user = ctx.Users.Where(u => (u.Id == iduser)).FirstOrDefault();
                    if (user != null)
                    {
                        user.IsActive = true;
                        ctx.SaveChanges();
                        return StatusCode(HttpStatusCode.OK);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch
            {
                return InternalServerError();
                //return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
        public class LoginDto
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        [AllowAnonymous]
        [Route("api/account/login")]
        [HttpPost]
        public IHttpActionResult login([FromBody] LoginDto login)
        {
            using (var ctx = new PetContext())
            {
                var user = ctx.Users.Where(u => (u.Email.Equals(login.email))).FirstOrDefault();

                if (user != null)
                {
                    bool verified = Crypto.VerifyHashedPassword(user.Password, login.password);
                    if (verified)
                    {
                        var tokens = JwtManager.GenerateToken(user.Id, 172800);

                        return Ok(new Token()
                        {
                            token = tokens
                        });
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }


        public class ForgotDto
        {
            public string email { get; set; }
        }
        [AllowAnonymous]
        [Route("api/account/forgotPassword")]
        [HttpPost]
        public async Task<IHttpActionResult> forgotPassword([FromBody] ForgotDto forgotDto)
        {
            using (var ctx = new PetContext())
            {
                var u = ctx.Users.Where(user => user.Email.Equals(forgotDto.email)).FirstOrDefault();
                if (u == null)
                {
                    return NotFound();
                }

                string newPassword = CreatePassword();
                u.Password = Crypto.HashPassword(newPassword);
                ctx.SaveChanges();
                //var tokens = JwtManager.GenerateToken(u.Id, 1440);
                var message = new MailMessage();
                message.To.Add(new MailAddress(u.Email));
                message.From = new MailAddress("badao231199@gmail.com");
                message.Subject = "Forgot Password";
                message.Body = string.Format("Password mới đã được cấp lại. vui lòng sử dụng mật khẩu mới để đăng nhập :" + newPassword);

                message.IsBodyHtml = true;
                using (var smtp = new SmtpClient())
                {
                    await smtp.SendMailAsync(message);
                    await Task.FromResult(0);
                    return StatusCode(HttpStatusCode.OK);
                }
            }
        }
        [AllowAnonymous]
        public static string CreatePassword(int length = 6)
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
        [AllowAnonymous]
        [Route("api/account/confirmEmailToForgot")]
        [HttpGet]
        public async Task<IHttpActionResult> ConfirmEmailForget(string token)
        {

            using (var ctx = new PetContext())
            {
                var simplePrinciple = JwtManager.GetPrincipal(token);
                var identity = simplePrinciple.Identity as ClaimsIdentity;

                var EmailClaim = identity.FindFirst(ClaimTypes.Sid);
                var id = EmailClaim?.Value;
                int iduser = Int32.Parse(id);
                var user = ctx.Users.Where(u => (u.Id == iduser)).FirstOrDefault();
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
                        return StatusCode(HttpStatusCode.OK);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        public class ChangePassDto
        {
            public string oldPassword { get; set; }
            public string newPasswod { get; set; }
        }

        [JwtAuthentication]
        [Route("api/account/changePassword")]
        [HttpPost]
        public async Task<IHttpActionResult> changePassword([FromBody] ChangePassDto changePassDto)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
            using (var ctx = new PetContext())
            {

                User user = ctx.Users.Find(int.Parse(loggedUserId));

                if (user != null)
                {
                    bool verified = Crypto.VerifyHashedPassword(user.Password, changePassDto.oldPassword);
                    if (verified)
                    {
                        user.Password = Crypto.HashPassword(changePassDto.newPasswod);
                        ctx.SaveChanges();

                        return StatusCode(HttpStatusCode.OK);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        [JwtAuthentication]
        [Route("api/account/getuser")]  
        public IHttpActionResult getuser()
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
            using (var ctx = new PetContext())
            {
                User user = ctx.Users.Find(int.Parse(loggedUserId));
                if (user != null)
                {
                    return Ok(user);

                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}
