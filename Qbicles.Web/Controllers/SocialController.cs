using System.Collections.Generic;
using Qbicles.BusinessRules.BusinessRules.Social;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using System.Linq;

namespace Qbicles.Web.Controllers
{
    public class SocialController : BaseController
    {
        [HttpGet]
        public ActionResult HighlightPost()
        {
            var userSetting = CurrentUser();
            var rules = new HighlightPostRules(dbContext);
            var HLStreamModel = rules.GetListHlPostCustomPagination(userSetting, 0, "", 0, 0, new List<string>(), true, true, 0);
            var lstHighlightPostCustom = HLStreamModel.HighlightPosts;
            var lstNonFollowedDomain = rules.GetListRandomDomainToFollow(userSetting.Id);
            var lstTags = rules.GetHighlightPostTags();
            var lstDomainFollowed = rules.GetListDomainFollowed(userSetting.Id);
            var lstCountries = new CountriesRules().GetAllCountries();

            ViewBag.ListCountries = lstCountries;
            ViewBag.ListPostTags = lstTags.Take(10).ToList();
            ViewBag.BookmarkedNum = HLStreamModel.BookmarkedNumber;
            ViewBag.FlaggedNum = HLStreamModel.FlaggedNumber;
            ViewBag.NonFollowedDomain = lstNonFollowedDomain;
            ViewBag.FollowedDomain = lstDomainFollowed;
            ViewBag.PropertyTypeList = rules.GetAllPropertyType();
            ViewBag.PropertyList = rules.GetAllProperties();

            if (lstHighlightPostCustom == null)
                return View("Error");

            ViewBag.CurrentPage = "highlight";
            SetCurrentPage("highlight");
            return View("~/Views/HighlightPost/HighlightPost.cshtml", lstHighlightPostCustom);
        }

    }
}