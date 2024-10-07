using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.IO;
using System.Linq;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroLinksRules : MicroRulesBase
    {
        public MicroLinksRules(MicroContext microContext) : base(microContext)
        {
        }


        public ReturnJsonModel CreateQbicleLinkActivity(MicroLinkQbicleModel linkQbicle)
        {
            if (linkQbicle.FeaturedImage.FileKey == "" && linkQbicle.Id == 0)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_403") };

            if (!Utility.ValidateUrl(linkQbicle.Url))
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_101") };

            var rules = new FileTypeRules(dbContext);

            var media = new MediaModel { };
            if (linkQbicle.FeaturedImage.FileKey != "")
                media = new MediaModel
                {
                    UrlGuid = linkQbicle.FeaturedImage.FileKey,
                    Name = linkQbicle.FeaturedImage.FileName,
                    Size = linkQbicle.FeaturedImage.FileSize,
                    Type = rules.GetFileTypeByExtension(linkQbicle.FeaturedImage.FileType) ?? rules.GetFileTypeByExtension(Path.GetExtension(linkQbicle.FeaturedImage.FileName))
                };


            if (!string.IsNullOrEmpty(media.UrlGuid))
            {
                if (media.Type == null)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_FILETYPE_406") };
            }

            var link = new QbicleLink
            {
                Id = linkQbicle.Id,
                Name = linkQbicle.Name,
                Description = linkQbicle.Description,
                URL = linkQbicle.Url,
                StartedDate = DateTime.UtcNow,
                State = QbicleActivity.ActivityStateEnum.Open,
                StartedBy = CurrentUser,
                ActivityType = QbicleActivity.ActivityTypeEnum.Link,
                App = QbicleActivity.ActivityApp.Qbicles,
                TimeLineDate = DateTime.UtcNow
            };

            return new LinksRules(dbContext).SaveLink(link, linkQbicle.QbicleId, linkQbicle.TopicId, media, CurrentUser.Id, linkQbicle.OriginatingConnectionId);

        }


        public MicroLinkActivity GetQbicleLink(int id)
        {
            var link = new LinksRules(dbContext).GetLinkById(id);
            return link.ToMicro(CurrentUser);
        }
    }
}
