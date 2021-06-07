using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PetFinderAPI.App_Data;
using PetFinderAPI.Models;

namespace PetFinderAPI.Controllers.Admin
{
    public class PostsController : Controller
    {
        // GET: Blog
        private PetContext _db = new PetContext();
        public ActionResult Index()
        {
            List<Pet> pets = Models.Pet.GetPets();
            ViewBag.Pets = pets;

            List<PostCategory> postcategories = Models.PostCategory.getPostCategory();
            ViewBag.PostCategories = postcategories;

            List<Post> posts = Models.Post.GetPosts();
            List<Image> images = new List<Image>();
            for (int i = 0; i < posts.Count; i++)
            {
                images.Add(Post.GetImages(posts[i].Id)[0]);
            }
            ViewBag.Posts = posts;
            ViewBag.Images = images;


            return View();
        }
        public ActionResult Store()
        {
            List<PostCategory> postCategories = new List<PostCategory>();
            postCategories = _db.PostCategories.ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (PostCategory item_Currency in postCategories)
            {
                listItems.Add(new SelectListItem()
                {
                    Value = item_Currency.Id.ToString(),
                    Text = item_Currency.Name
                });
            }
            ViewBag.hhh = new SelectList(listItems, "Value", "Text");

            List<Pet> pet = new List<Pet>();
            pet = _db.Pets.ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (Pet i in pet)
            {
                list.Add(new SelectListItem()
                {
                    Value = i.Id.ToString(),
                    Text = i.Name
                });
            }
            ViewBag.ABC = new SelectList(list, "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Store(Post post, string[] images)
        {
            Debug.WriteLine("IMG: " + images.ToString());
            _db.Posts.Add(post);
            _db.SaveChanges();

            if (images.Length > 1)
            {
                for (int i = 1; i < images.Length; i++)
                {
                    Image image = new Image();
                    image.PostId = post.Id;
                    image.Url = images[i];

                    _db.Images.Add(image);
                    _db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            List<Post> posts = Models.Post.GetPosts();
            List<Image> images = Post.GetImages(id);

            ViewBag.Images = images;


            var data = _db.Posts.Where(s => s.Id == id).FirstOrDefault();
            List<Pet> u = new List<Pet>();
            u = _db.Pets.ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (Pet item_Currency in u)
            {
                listItems.Add(new SelectListItem()
                {
                    Value = item_Currency.Id.ToString(),
                    Text = item_Currency.Name
                });
            }
            ViewBag.Pets = new SelectList(listItems, "Value", "Text");

            List<PostCategory> p = new List<PostCategory>();
            p = _db.PostCategories.ToList();
            List<SelectListItem> l = new List<SelectListItem>();
            foreach (PostCategory i in p)
            {
                l.Add(new SelectListItem()
                {
                    Value = i.Id.ToString(),
                    Text = i.Name
                });
            }
            ViewBag.PostCategory = new SelectList(l, "Value", "Text");
            ViewBag.post = data;
            return View(data);
        }

        public bool checkImg(string[] img, Post post)
        {
            var image = _db.Images.Where(b => b.PostId == post.Id).ToList();
            foreach (var i in image)
            {
                foreach (string j in img)
                {
                    if (!i.Url.Equals(j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id, PetId,Content,PostCategoryId,CreatedAt,IsActive")] Post post, string[] images)
        {
            Debug.WriteLine("Posts: " + ModelState.Values.ToString());

            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
            }

            List<Image> image = _db.Images.Where(b => b.PostId == post.Id).ToList();
           
            if (checkImg(images, post))
            {
                foreach (Image i in image)
                {
                    _db.Images.Remove(i);
                }
                for (int j = 1; j < images.Length; j++)
                {
                    Image ima = new Image();
                    ima.PostId = post.Id;
                    ima.Url = images[j];

                    _db.Images.Add(ima);
                }
            }

            var data = _db.Posts.Where(s => s.Id == post.Id).FirstOrDefault();
            if (data != null)
            {
                _db.Entry(data).State = EntityState.Detached;
                _db.Entry(post).State = EntityState.Modified;
                _db.SaveChanges();
                ViewBag.post = post;
                return RedirectToAction("Index");
            }
            ViewBag.post = post;
            return View(post);

        }

            [HttpGet]
        public ActionResult Delete(int? id)
        {
            var post = _db.Posts.Where(u => u.Id == id).First();
            _db.Posts.Remove(post);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}