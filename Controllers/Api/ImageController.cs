using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;
using PetFinderAPI.App_Data;
using PetFinderAPI.Filter;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Api
{
    public class ImageController : ApiController
    {
        public ImageController()
        {
        }
        //POST: api/posts/foruser cua user
        [Route("api/images/post")]
        [JwtAuthentication]
        public IHttpActionResult PostImageForUser()
        {
            if (HttpContext.Current.Request.Files.Count == 0)
            {
                return BadRequest();
            }
            var files = HttpContext.Current.Request.Files;
            List<ImageDto> listImages = new List<ImageDto>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];


                if (file != null && file.ContentLength > 0)
                {
                    var exention = file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    var fileName = System.Guid.NewGuid().ToString() + exention;

                    var path = Path.Combine(
                        HttpContext.Current.Server.MapPath("/Images/files/"),
                        fileName
                    );

                    file.SaveAs(path);

                    listImages.Add(new ImageDto()
                    {
                        Url = fileName,
                    });

                }
            }

            return Ok(listImages);
        }

        [Route("api/images/avatar")]
        [JwtAuthentication]
        public IHttpActionResult PostAvatarForUser()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                    HttpContext.Current.Request.Files[0] : null;

            if (file != null && file.ContentLength > 0)
            {
                var exention = file.FileName.Substring(file.FileName.LastIndexOf('.'));
                var filename = System.Guid.NewGuid().ToString() + exention;
                var path = Path.Combine(
                    HttpContext.Current.Server.MapPath("/Content/img/uploads"),
                    filename
                );

                file.SaveAs(path);
                return Ok(filename);
            }

            return BadRequest();
        }
    }
}