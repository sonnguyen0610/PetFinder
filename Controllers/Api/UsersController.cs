using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using MySql.Data.MySqlClient;
using PetFinderAPI.App_Data;
using PetFinderAPI.Filter;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Api
{
    public class UsersController : ApiController
    {

        public UsersController()
        {
        }



        [JwtAuthentication]
        [Route("api/users/count")]

        public IHttpActionResult GetCountAll()
        {
            using (var ctx = new PetContext())
            {

                List<PetCategory> petCategories = ctx.PetCategories.ToList();
                List<int> counts = new List<int>();
                foreach (var petCat in petCategories)
                {
                    int posts = ctx.Posts.Where(p => p.Pet.PetCategoryId == petCat.Id).ToList().Count;
                    counts.Add(posts);
                }
                return Ok(counts);
            }
        }

        // laays all pet user
        //GET: api/users/pets

        [JwtAuthentication]
        [Route("api/users/pets")]

        public IHttpActionResult GetAllPets()
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
                int id = int.Parse(loggedUserId);

                List<Pet> pets = ctx.Pets.Where(p => p.UserId == id).Include(p => p.PetCategory).Include(p => p.User).ToList();
                List<PetDto> petDtos = pets.ConvertAll<PetDto>(p => new PetDto(p));
                return Ok(petDtos);
            }
        }


        [JwtAuthentication]
        [Route("api/users/{id}/pets")]

        public IHttpActionResult GetAllPets(int id)
        {
            using (var ctx = new PetContext())
            {

                List<Pet> pets = ctx.Pets.Where(p => p.UserId == id).Include(p => p.PetCategory).Include(p => p.User).ToList();
                List<PetDto> petDtos = pets.ConvertAll<PetDto>(p => new PetDto(p));
                return Ok(petDtos);
            }
        }
        [JwtAuthentication]
        [Route("api/users/{id}/posts")]

        public IHttpActionResult GetAllPosts(int id)
        {
            using (var ctx = new PetContext())
            {
                List<Post> posts = ctx.Posts.Where(p => p.Pet.UserId == id).Include(p => p.PostCategory).Include(p => p.Pet.User).Include(p => p.Pet.PetCategory).Include(p => p.Images).OrderByDescending(x => x.CreatedAt).ToList();
                List<PostDto> postDtos = posts.ConvertAll<PostDto>(p => new PostDto(p));

                return Ok(postDtos);
            }
        }
        //GET: api/users
        [JwtAuthentication]
        public IEnumerable<User> GetAllUsers()
        {
            List<User> users = Models.User.GetUsers();
            return users;
        }
        //GET: api/profile
        [JwtAuthentication]
        public IHttpActionResult GetProfile()
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
                int id = int.Parse(loggedUserId);

                User user = ctx.Users.Find(id);

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

        [JwtAuthentication]
        public IHttpActionResult PutProfile(User user)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;


            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
            int id = int.Parse(loggedUserId);
            if (id != user.Id)
            {
                return BadRequest();
            }
            using (var _context = new PetContext())
            {
                User userDB = _context.Users.Find(id);
                userDB.UserName = user.UserName;
                userDB.Phone = user.Phone;
                userDB.Avatar = user.Avatar;
                userDB.Address = user.Address;

                try
                {
                    _context.Entry(userDB).State = EntityState.Detached;
                    _context.Entry(userDB).State = EntityState.Modified;

                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // inspect user exits
                    if (!(_context.Users.Any(e => e.Id == id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(userDB);

            }
        }
        //POST: api/users
        [JwtAuthentication]

        public IHttpActionResult PostTodoItem(User user)
        {
            using (var ctx = new PetContext())
            {

                User newUser = ctx.Users.Add(user);
                ctx.SaveChanges();

                if (newUser != null)
                {
                    return Ok(newUser);
                }
                else
                {
                    return InternalServerError();
                }
            }

        }

        public class TokenDto
        {
            public string serial { get; set; }
            public string newToken { get; set; }
        }

        [JwtAuthentication]
        [Route("api/users/token")]
        public IHttpActionResult PostToken([FromBody] TokenDto tokenDto)
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
                int id = int.Parse(loggedUserId);

                var tokens = FcmToken.GetTokenForSerial(tokenDto.serial);
                if (tokens.Count >= 0)
                {
                    foreach (var t in tokens)
                    {
                        ctx.Entry(t).State = EntityState.Deleted;
                        ctx.SaveChanges();


                    }
                }

                ctx.Tokens.Add(new FcmToken()
                {
                    Serial = tokenDto.serial,
                    Token = tokenDto.newToken,
                    UserId = id,
                });

                ctx.SaveChanges();

            }

            return Ok();

        }
    }

}