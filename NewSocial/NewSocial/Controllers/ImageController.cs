using NewSocial.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace NewSocial.Controllers
{
    public class ImageController : Controller
    {
        DbCalls db = new DbCalls();
        // GET: Iamge
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult SaveProfile(string image)
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                var User = (Models.User)Session["ApplicationUser"];
                code = 200;
                Message = "Saved";


                byte[] contents = convertIntoByte(image);
                string subpath = "~/images/userProfiles/";
                string fileName = User.UserID+"_Profile_" + Guid.NewGuid() + ".jpg";
                var uploadPath = HttpContext.Server.MapPath(subpath);
                var path = Path.Combine(uploadPath, Path.GetFileName(fileName));

                System.IO.File.WriteAllBytes(path, contents);



                UserMedia currentUerMedia = new UserMedia();
                currentUerMedia.UserID = User.UserID;
                currentUerMedia.type = 1;
                currentUerMedia.ImageUrl = "/images/userProfiles/" + fileName;
                db.UserMedia.Add(currentUerMedia);
                db.SaveChanges();
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
            }

        }
        
        //public void UploadIamge(string img)
        //{
        //    byte[] contents = convertIntoByte(img);
        //    string subpath = "~/images/userProfiles";
        //    string fileName = "user" + Guid.NewGuid()+".jpg";
        //    var uploadPath = HttpContext.Server.MapPath(subpath);
        //    var path = Path.Combine(uploadPath, Path.GetFileName(fileName));
        //    System.IO.File.WriteAllBytes(path, contents);

        //}


        public byte[] convertIntoByte(string byte_array)
        {
            byte[] bytes = System.Convert.FromBase64String((byte_array.Split(',') as string[])[1]);
            return bytes;
        }

    }
}