using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PetFinderAPI.App_Data;
using PetFinderAPI.Models;
using System.Data.Entity;
using System.Security.Claims;
using PetFinderAPI.Filter;
using System;
using System.Data.Entity.Infrastructure;

namespace PetFinderAPI.Controllers
{
    public class PostsController : ApiController
    {
        //GET: api/posts/foruser cua user
        [JwtAuthentication]
        [Route("api/posts/foruser")]
        public IHttpActionResult GetAllPostsUser()
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


                List<Post> posts = ctx.Posts.Where(p => p.Pet.UserId == id).Include(p => p.PostCategory).Include(p => p.Pet.User).Include(p => p.Pet.PetCategory).Include(p => p.Images).OrderByDescending(x => x.CreatedAt).ToList();
                List<PostDto> postDtos = posts.ConvertAll<PostDto>(p => new PostDto(p));

                return Ok(postDtos);
            }
        }


        //POST: api/posts/foruser cua user
        [Route("api/posts/foruser")]
        [JwtAuthentication]
        public IHttpActionResult PostPostForUser([FromBody] PostDto postDto)
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

                if (postDto.Pet.Owner.Id != id)
                {
                    return StatusCode(System.Net.HttpStatusCode.Forbidden);

                }
                Post newPost = new Post(postDto);
                ctx.Posts.Add(newPost);
                ctx.SaveChanges();
                List<ImageDto> imgs = postDto.Images;
                foreach (ImageDto img in imgs)

                {
                    Image i = new Image
                    {
                        Url = img.Url,
                        PostId = newPost.Id,
                    };

                    ctx.Images.Add(i);
                    ctx.SaveChanges();
                }


                return Ok(postDto);
            }
        }

        [JwtAuthentication]
        [Route("api/posts/{id}")]
        public IHttpActionResult DeletePost(int id)
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
                Post post = _context.Posts.Where(p => p.Id == id).Include(p => p.Pet).FirstOrDefault();

                if (post == null) return NotFound();

                if (int.Parse(loggedUserId) != post.Pet.UserId)
                {
                    return BadRequest();
                }
                _context.Entry(post).State = EntityState.Deleted;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // inspect post exits
                    if (!(_context.Posts.Any(e => e.Id == id)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(post);

            }
        }

        [JwtAuthentication]
        [Route("api/posts/{id}/like")]
        public IHttpActionResult LikePost(int id)
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

                var post = _context.Posts.Include(p => p.Pet).Where(p => p.Id == id).FirstOrDefault();
                if (post == null)
                {
                    return NotFound();
                }

                PostLikes postLike = _context.PostLikes.Where(p => p.UserId == userId && p.PostId == id).FirstOrDefault();

                if (postLike == null)
                {
                    _context.PostLikes.Add(new PostLikes()
                    {
                        PostId = id,
                        UserId = userId,
                    });
                    _context.SaveChanges();

                    FcmToken.SendNotificationToUser(loggedUser.UserName + " likes your post", post.Pet.UserId);
                }
                else
                {
                    _context.Entry(postLike).State = EntityState.Deleted;
                    _context.SaveChanges();
                }

                return Ok();
            }
        }

        public void sendNotification(String mess, List<String> tokens)
        {
            //todo
        }


        [JwtAuthentication]
        [Route("api/posts/foruser")]
        public IHttpActionResult PutPost(PostDto postDto)
        {
            string loggedUserId = null;

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity?.Claims;
            loggedUserId = claims?.Where(p => p.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;

            if (loggedUserId == null)
            {
                return StatusCode(System.Net.HttpStatusCode.Forbidden);
            }
            if (int.Parse(loggedUserId) != postDto.Pet.Owner.Id)
            {
                return BadRequest();
            }
            using (var _context = new PetContext())
            {
                Post newPost = new Post(postDto);
                _context.Entry(newPost).State = EntityState.Detached;
                _context.Entry(newPost).State = EntityState.Modified;
                _context.SaveChanges();

                List<Image> imageDB = _context.Images.Where(b => b.PostId == postDto.Id).ToList();
                foreach (Image img in imageDB)
                {
                    _context.Entry(img).State = EntityState.Deleted;
                    _context.SaveChanges();
                }
                List<ImageDto> imgs = postDto.Images;
                foreach (ImageDto img in imgs)
                {
                    Image i = new Image
                    {
                        Url = img.Url,
                        PostId = newPost.Id,
                    };

                    _context.Images.Add(i);
                    _context.SaveChanges();

                }

                Post returnPost = _context.Posts.Where(p => p.Id == newPost.Id).Include(p => p.PostCategory)
                                         .Include(p => p.Pet.User)
                                         .Include(p => p.Pet.PetCategory)
                                         .Include(p => p.Images).FirstOrDefault();


                return Ok(new PostDto(returnPost));
            }
        }

        public class GetLastestPostFilterQuery
        {
            public string SearchQuery { get; set; }
            public int? PetCategoryId { get; set; }
            public List<int> PostCategoryIds { get; set; }
        }


        public class GetLastestPostDistanceQuery
        {
            public double? Lat { get; set; }
            public double? Lon { get; set; }
            public double? Radius { get; set; }

            public bool ShouldComputeDistance
            {
                get { return Lat != null && Lon != null; }
            }

            public bool ShouldFilterByRadius
            {
                get { return Lat != null && Lon != null && Radius != null; }
            }
        }

        private double calculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var baseRad = Math.PI * lat1 / 180;
            var targetRad = Math.PI * lat2 / 180;
            var theta = lon1 - lon2;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist * 1.609344;
        }

        private Tuple<double, double> extractLatLonFromAddress(String address)
        {
            var list = address.Split(';');
            return new Tuple<double, double>(double.Parse(list[0]), double.Parse(list[1]));
        }

        [JwtAuthentication]
        public IHttpActionResult GetLastestPost([FromUri] GetLastestPostFilterQuery filterQuery, [FromUri] GetLastestPostDistanceQuery distanceQuery)
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
                var baseQuery = ctx.Posts.Include(p => p.PostCategory)
                                         .Include(p => p.Pet.User)
                                         .Include(p => p.Pet.PetCategory)
                                         .Include(p => p.Images);

                if (!String.IsNullOrEmpty(filterQuery.SearchQuery))
                {
                    baseQuery = baseQuery.Where(p => p.Content.Contains(filterQuery.SearchQuery) || p.Pet.Name.Contains(filterQuery.SearchQuery) || p.Pet.Bio.Contains(filterQuery.SearchQuery));
                }

                if (filterQuery.PetCategoryId != null)
                {
                    baseQuery = baseQuery.Where(p => p.Pet.PetCategoryId == filterQuery.PetCategoryId);
                }

                if (filterQuery.PostCategoryIds != null && filterQuery.PostCategoryIds.Count != 0)
                {
                    baseQuery = baseQuery.Where(p => filterQuery.PostCategoryIds.Contains(p.PostCategoryId));
                }

                List<Post> posts = baseQuery.OrderByDescending(x => x.CreatedAt).ToList();

                var loggedUserIdInt = int.Parse(loggedUserId);
                List<PostLikes> postLikes = ctx.PostLikes.Where(p => p.UserId == loggedUserIdInt).ToList();

                List<PostDto> postDtos = posts.ConvertAll<PostDto>(p =>
                {
                    var latLonTuple = extractLatLonFromAddress(p.Pet.Address);
                    double petLat = latLonTuple.Item1;
                    double petLon = latLonTuple.Item2;

                    var isLiked = postLikes.Where(pl => pl.PostId == p.Id).Count() > 0;

                    return new PostDto(p)
                    {
                        Pet = new PetDto(p.Pet)
                        {
                            Address = new AddressDto(p.Pet.Address)
                            {
                                Lat = petLat,
                                Long = petLon,
                            },
                        },
                        IsLiked = isLiked,
                        DistanceFromUser = distanceQuery.ShouldComputeDistance ? calculateDistance(petLat, petLon, distanceQuery.Lat ?? 0, distanceQuery.Lon ?? 0) : 0,
                    };
                });

                if (distanceQuery.ShouldFilterByRadius)
                {
                    postDtos = postDtos.Where(p => p.DistanceFromUser <= distanceQuery.Radius).ToList();
                }

                return Ok(postDtos);
            }
        }
    }

}