using System;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PostsController : BaseController
    {
     
 
        public ActionResult DeletePost(string key)
        {
            var postId = 0;
            if (!string.IsNullOrEmpty(key?.Trim()))
            {
                postId = Int32.Parse(EncryptionService.Decrypt(key));
            }
            var refModel = new PostsRules(dbContext).DeletePost(postId, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ForwardPost(string key, string qbicleKey)
        {
            var postId = 0;
            var qbicleId = 0;
            if (!string.IsNullOrEmpty(key?.Trim()))
            {
                postId = Int32.Parse(EncryptionService.Decrypt(key));
            }
            if (!string.IsNullOrEmpty(qbicleKey?.Trim()))
            {
                qbicleId = Int32.Parse(EncryptionService.Decrypt(qbicleKey));
            }
            var refModel = new PostsRules(dbContext).ForwardPost(postId, qbicleId, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetMessageOfPost(string key)
        {
            var postId = 0;
            if (!string.IsNullOrEmpty(key?.Trim()))
            {
                postId = Int32.Parse(EncryptionService.Decrypt(key));
            }
            var post = new PostsRules(dbContext).GetPostById(postId);
            return Json(new { message= (post?.Message??"")}, JsonRequestBehavior.AllowGet);
        }
    }
}