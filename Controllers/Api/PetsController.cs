using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using PetFinderAPI.App_Data;
using PetFinderAPI.Filter;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Api
{
    public class PetsController : ApiController
    {


        //GET: api/users/pets/{id}
        [JwtAuthentication]
        [Route("api/pets/{id}")]
        public IHttpActionResult GetPet(int id)
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
                var loggedUserIdInt = int.Parse(loggedUserId);
                List<PetLikes> petLikes = ctx.PetLikes.Where(p => p.UserId == loggedUserIdInt).ToList();


                Pet pet = ctx.Pets.Where(p => p.Id == id).Include(p => p.PetCategory).Include(p => p.User).FirstOrDefault();

                if (pet != null)
                {
                    var IsFollowed = petLikes.Where(pl => pl.PetId == pet.Id).Count() > 0;
                    PetDto petDto = new PetDto(pet)
                    {
                        IsFollowed = IsFollowed,
                    };

                    return Ok(petDto);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        //POST: api/users/pets
        [JwtAuthentication]
        [Route("api/pets")]

        public IHttpActionResult PostPet(PetDto petDto)
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
                Pet newPet = new Pet(petDto);
                newPet.UserId = int.Parse(loggedUserId);
                ctx.Pets.Add(newPet);
                ctx.SaveChanges();
                petDto.Id = newPet.Id;

                if (newPet != null)
                {
                    return Ok(petDto);
                }
                else
                {
                    return InternalServerError();
                }
            }

        }
        // PUT: api/pets/5
        [JwtAuthentication]
        [Route("api/pets")]
        public IHttpActionResult PutPet(PetDto petDto)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
            if (int.Parse(loggedUserId) != petDto.Owner.Id)
            {
                return BadRequest();
            }
            using (var _context = new PetContext())
            {

                Pet newPet = new Pet(petDto);
                newPet.UserId = int.Parse(loggedUserId);

                _context.Entry(newPet).State = EntityState.Modified;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // inspect pet exits
                    if (!(_context.Pets.Any(e => e.Id == newPet.Id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Pet returnPet = _context.Pets.Where(p => p.Id == newPet.Id).Include(p => p.PetCategory)
                                      .Include(p => p.User).FirstOrDefault();


                return Ok(new PetDto(returnPet));

            }
        }


        [JwtAuthentication]
        [Route("api/pets/{id}")]
        public IHttpActionResult DeletePet(int id)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }

            using (var _context = new PetContext())
            {
                Pet pet = _context.Pets.Find(id);

                if (int.Parse(loggedUserId) != pet.UserId)
                {
                    return BadRequest();
                }
                _context.Entry(pet).State = EntityState.Deleted;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // inspect pet exits
                    if (!(_context.Pets.Any(e => e.Id == id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(pet);

            }
        }


        [JwtAuthentication]
        [Route("api/pets/{id}/like")]
        public IHttpActionResult LikePet(int id)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }

            using (var _context = new PetContext())
            {
                int userId = int.Parse(loggedUserId);
                var loggedUser = _context.Users.Find(userId);

                var pet = _context.Pets.Find(id);
                if (pet == null)
                {
                    return NotFound();
                }

                PetLikes petLike = _context.PetLikes.Where(p => p.UserId == userId && p.PetId == id).FirstOrDefault();

                if (petLike == null)
                {
                    _context.PetLikes.Add(new PetLikes()
                    {
                        PetId = id,
                        UserId = userId,
                    });
                    _context.SaveChanges();

                    FcmToken.SendNotificationToUser(loggedUser.UserName + " follows your pet", pet.UserId);
                }
                else
                {
                    _context.Entry(petLike).State = EntityState.Deleted;
                    _context.SaveChanges();
                }

                return Ok();
            }
        }


    }
}