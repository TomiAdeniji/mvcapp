using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroMediaFolderRules : MicroRulesBase
    {
        public MicroMediaFolderRules(MicroContext microContext) : base(microContext)
        {
        }

        public List<string> FilesAccept()
        {
            return new FileTypeRules(dbContext).GetExtension();
        }



        public List<MicroMediaFolder> GetMediaFolders(int qbicleId)
        {
            var folders = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(qbicleId, "");
            return folders.ToMicro(CurrentUser.Timezone);
        }
        public MicroMediaFolder GetMediasInFolder(int qbicleId, int folderId)
        {
            var medias = new MediaFolderRules(dbContext).GetMediaItemByFolderId(folderId, qbicleId, CurrentUser.Timezone);
            return medias.ToMicro(CurrentUser.Timezone);
        }

        public ReturnJsonModel MediaMoveFolderById(int folderId, int mediaId, bool isCreatorTheCustomer)
        {
            return new MediasRules(dbContext).MediaMoveFolderById(mediaId, folderId, CurrentUser.Id, isCreatorTheCustomer);
        }

        public ReturnJsonModel CreateUpdateMediaFolder(int id, string name, int qbicleId, string userId)
        {
            if (name == HelperClass.GeneralName)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_322")
                };
            var rules = new MediaFolderRules(dbContext);
            if (rules.IsDuplicateFolderName(0, name, qbicleId))
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MEDIAFOLDERNAME_EXISTS")
                };

            MediaFolder mediaFolder;
            if (id == 0)
                mediaFolder = rules.InsertMediaFolder(name, userId, qbicleId);
            else
                mediaFolder = rules.UpdateMediaFolder(id, name, qbicleId);

            if (mediaFolder == null)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_224") };
            return new ReturnJsonModel { result = true, actionVal = mediaFolder.Id };
        }

    }
}
