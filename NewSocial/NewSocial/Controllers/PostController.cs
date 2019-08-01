﻿using NewSocial.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NewSocial.Controllers
{
    public class PostController : Controller
    {
        DbCalls db = new DbCalls();
        public JsonResult SaveProfile(string image)
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                var User = (Models.User)Session["ApplicationUser"];
                code = 200;
                Message = "Saved";
                UserMedia currentUerMedia = new UserMedia();
                currentUerMedia.UserID = User.UserID;
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
        public byte[] convertIntoByte(string byte_array)
        {
            byte[] bytes;
            if (byte_array != "")
            {

                bytes = System.Convert.FromBase64String((byte_array.Split(',') as string[])[1]);
                return bytes;
            }
            else
            {
                bytes = null;
            }
            return bytes;
        }
        public string convertByteIntostring(byte[] currentImage)
        {
            string image;
            if (currentImage==null)
            {
                image = "Not Available";
            }
            else
            {
                image = "data:image/jpeg;base64," + Convert.ToBase64String(currentImage);
            }
          
            return image;
        }
        public JsonResult addPost(string text, string image, string video)
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                if ((text != "" || image != "" || video != "") && text.Length <= 500)
                {
                    var User = (Models.User)Session["ApplicationUser"];

                    Post currentPost = new Post();
                    if (image != "" && image.Length>=50)
                    {
                        byte[] contents = convertIntoByte(image);
                        string subpath = "~/images/userPosts/";
                        string fileName = User.UserID + "_Post_" + Guid.NewGuid() + ".jpg";
                        var uploadPath = HttpContext.Server.MapPath(subpath);
                        var path = Path.Combine(uploadPath, Path.GetFileName(fileName));
                        System.IO.File.WriteAllBytes(path, contents);

                        currentPost.imageURL = "/images/userPosts/" + fileName;
                    }
                    currentPost.UserID = User.UserID;
                    currentPost.text = text;
                    currentPost.postTime = DateTime.UtcNow;
                    currentPost.updateTime = DateTime.UtcNow;
                    db.Post.Add(currentPost);
                    db.SaveChanges();
                    code = 200;
                    Message = "Post Successfully added";
                    return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    code = 400;
                    Message = "UnAuthorized Changing";
                    return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult postList()
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                var User = (Models.User)Session["ApplicationUser"];


                                

                var friendlist1 = (from f in db.Friend.Where(u=>u.UserID1==User.UserID).ToList()
                                  select new
                                  {
                                      f.UserID2,
                                  }).ToList();
                var friendlist2 = (from f in db.Friend.Where(u =>u.UserID2 == User.UserID).ToList()
                                   select new
                                   {
                                       f.UserID1,
                                   }).ToList();
                List<long> friendIds=new List<long>();
                for (int i = 0; i < friendlist1.Count; i++)
                {
                    friendIds.Add(friendlist1[i].UserID2);
                }
                for (int i = 0; i < friendlist2.Count; i++)
                {
                    friendIds.Add(friendlist2[i].UserID1);
                }
                friendIds.Add(User.UserID);
                var postList = (from ep in friendIds
                              join i in db.Post.Include("Comments").Include("Reactions").ToList()
                               on ep equals i.UserID
                                select new
                                {
                                    i.PostID,
                                    i.UserID,

                                   userProfile = (from k in db.UserMedia.Where(u => u.UserID == i.UserID && u.type == 1).ToList()
                                                              select new
                                                              {
                                                                  k.ImageUrl
                                                              }).LastOrDefault(),
                                    userName = (from j in db.User.Where(u => u.UserID == i.UserID).ToList()
                                                                    select new
                                                                    {
                                                                        j.name
                                                                    }).FirstOrDefault(),
                                    i.text,
                                    imageURl= "../../" + i.imageURL,
                                    //i.video,
                                    postTime=TimeZone.CurrentTimeZone.ToLocalTime(i.postTime),
                                    agreeCount = i.Reactions.Where(u => u.reactionType == 1).ToList().Count(),
                                    agree = (from j in i.Reactions.Where(u => u.reactionType == 1).ToList()
                                             select new
                                             {
                                                 userName = (from x in db.User.Where(x => x.UserID == j.UserID)
                                                             select new
                                                             {
                                                                 x.name,
                                                                 x.UserID
                                                             }
                                                             ).FirstOrDefault(),
                                             }).ToList(),
                                    disagreeCount = i.Reactions.Where(u => u.reactionType == 0).ToList().Count(),
                                    disAgree = (from j in i.Reactions.Where(u => u.reactionType == 0).ToList()
                                                select new
                                                {
                                                    userName = (from x in db.User.Where(x => x.UserID == j.UserID)
                                                                select new
                                                                {
                                                                    x.name,
                                                                    x.UserID
                                                                }
                                                                ).FirstOrDefault(),
                                                }).ToList(),
                                    Comments = (from j in i.Comments
                                                select new
                                                {
                                                    userProfile = (from k in db.UserMedia.Where(u => u.UserID == j.UserID && u.type == 1).ToList()
                                                                   select new
                                                                   {
                                                                       k.ImageUrl
                                                                   }).LastOrDefault(),

                                                    userName = (from x in db.User.Where(x => x.UserID == j.UserID)
                                                                select new {
                                                                    x.name,
                                                                    x.UserID}
                                                                ).FirstOrDefault(),
                                                    j.CommentID,
                                                    j.commentText,
                                                    commentTime = j.commentTime.ToString("dd-MMM-yy hh:mm tt"),
                                                    subAgreeCount = db.SubReaction.Where(u => u.reactionType == 1 && u.CommentID==j.CommentID).ToList().Count(),
                                                    subDisAgreeCount = db.SubReaction.Where(u => u.reactionType == 0 && u.CommentID == j.CommentID).ToList().Count(),
                                                }).ToList(),
                                }).ToList().OrderByDescending(u => u.postTime).Take(30);
                code = 200;
                Message = "Post List Available";
                return Json(new { code, Message, postList }, JsonRequestBehavior.AllowGet);
                
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult postListofCurrentUser()
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {


                var User = (Models.User)Session["ApplicationUser"];
                var postList = (from i in db.Post.Include("Comments").Include("Reactions").Where(u=>u.UserID==User.UserID).ToList()
                                select new
                                {
                                    i.PostID,
                                    i.UserID,

                                    userProfile = (from k in db.UserMedia.Where(u => u.UserID == i.UserID && u.type == 1).ToList()
                                                   select new
                                                   {
                                                       k.ImageUrl
                                                   }).LastOrDefault(),
                                    userName = (from j in db.User.Where(u => u.UserID == i.UserID).ToList()
                                                select new
                                                {
                                                    j.name
                                                }).FirstOrDefault(),
                                    i.text,
                                    imageURl = "../../" + i.imageURL,
                                    //i.video,
                                    postTime = TimeZone.CurrentTimeZone.ToLocalTime(i.postTime),
                                    agreeCount = i.Reactions.Where(u => u.reactionType == 1).ToList().Count(),
                                    //agree = i.Reactions.Where(u => u.reactionType == 1).ToList(),
                                    disagreeCount = i.Reactions.Where(u => u.reactionType == 0).ToList().Count(),
                                    //disAgree = i.Reactions.Where(u => u.reactionType == 0).ToList(),
                                    Comments = (from j in i.Comments
                                                select new
                                                {
                                                    userProfile = (from k in db.UserMedia.Where(u => u.UserID == j.UserID && u.type == 1).ToList()
                                                                   select new
                                                                   {
                                                                       k.ImageUrl
                                                                   }).LastOrDefault(),

                                                    userName = (from x in db.User.Where(x => x.UserID == j.UserID)
                                                                select new
                                                                {
                                                                    x.name,
                                                                    x.UserID
                                                                }
                                                                ).FirstOrDefault(),
                                                    j.CommentID,
                                                    j.commentText,
                                                    commentTime = j.commentTime.ToString("dd-MMM-yy hh:mm tt")
                                                }).ToList(),
                                }).ToList().OrderByDescending(u => u.PostID).Take(30);
                code = 200;
                Message = "Post List Available";
                return Json(new { code, Message, postList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult postListofSelectedUser(Int64 FriendID)
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                if (Session["ViewdUser"] != null)
                {
                    var postList = (from i in db.Post.Include("Comments").Include("Reactions").Where(u=>u.UserID==FriendID).ToList()
                                    select new
                                    {
                                        i.PostID,
                                        i.UserID,

                                        userProfile = (from k in db.UserMedia.Where(u => u.UserID == i.UserID && u.type == 1).ToList()
                                                       select new
                                                       {
                                                           k.ImageUrl
                                                       }).LastOrDefault(),
                                        userName = (from j in db.User.Where(u => u.UserID == i.UserID).ToList()
                                                    select new
                                                    {
                                                        j.name
                                                    }).FirstOrDefault(),
                                        i.text,
                                        imageURl = "../../" + i.imageURL,
                                        //i.video,
                                        postTime = TimeZone.CurrentTimeZone.ToLocalTime(i.postTime),
                                        agreeCount = i.Reactions.Where(u => u.reactionType == 1).ToList().Count(),
                                        //agree = i.Reactions.Where(u => u.reactionType == 1).ToList(),
                                        disagreeCount = i.Reactions.Where(u => u.reactionType == 0).ToList().Count(),
                                        //disAgree = i.Reactions.Where(u => u.reactionType == 0).ToList(),
                                        Comments = (from j in i.Comments
                                                    select new
                                                    {
                                                        userProfile = (from k in db.UserMedia.Where(u => u.UserID == j.UserID && u.type == 1).ToList()
                                                                       select new
                                                                       {
                                                                           k.ImageUrl
                                                                       }).LastOrDefault(),

                                                        userName = (from x in db.User.Where(x => x.UserID == j.UserID)
                                                                    select new
                                                                    {
                                                                        x.name,
                                                                        x.UserID
                                                                    }
                                                                    ).FirstOrDefault(),
                                                        j.CommentID,
                                                        j.commentText,
                                                        commentTime = j.commentTime.ToString("dd-MMM-yy hh:mm tt")
                                                    }).ToList(),
                                    }).ToList().OrderByDescending(u => u.PostID).Take(30);
                    code = 200;
                    Message = "Post List Available";
                    return Json(new { code, Message, postList }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    code = 400;
                    Message = "Try Again";
                    return Json(new { code, Message}, JsonRequestBehavior.AllowGet);

                }

              
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult deletePost(int currentPostID)
        {
            string Message;
            int code;
            if (Session["ApplicationUser"] != null)
            {
                var User = (Models.User)Session["ApplicationUser"];
                var currentPost = db.Post.Where(u => u.PostID == currentPostID && (u.UserID == User.UserID || User.UserID==1)).FirstOrDefault();
                if (currentPost != null)
                {
                    var comments = db.Comment.Where(u => u.PostID == currentPost.PostID).ToList();
                    var reactions = db.Reaction.Where(u => u.PostID == currentPost.PostID).ToList();

                    foreach (var comment in comments)
                    {
                        db.Comment.Remove(comment);
                    }
                    foreach (var reaction in reactions)
                    {
                        db.Reaction.Remove(reaction);
                    }
                    db.Post.Remove(currentPost);
                    db.SaveChanges();
                    code = 200;
                    Message = "Post Deleted";
                    return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    code = 400;
                    Message = "UnAuthorized Changing";
                    return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                code = 401;
                Message = "Login First";
                return Json(new { code, Message }, JsonRequestBehavior.AllowGet);
            }


        }


    }
}