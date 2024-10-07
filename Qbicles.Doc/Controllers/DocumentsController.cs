//using System.Web.Http;
//using Qbicles.BusinessRules;
//using Qbicles.Doc.Helper;

//namespace Qbicles.Doc.Controllers
//{
//    public class DocumentsController : ApiController
//    {
//        [System.Web.Mvc.Authorize]
//        [System.Web.Mvc.Route("api/documents/storefile")]
//        [System.Web.Mvc.AcceptVerbs("POST")]
//        public MediaModel StoreFile(RepositoryItem repositoryItem)
//        {
//            return QbiclesDocsRepository.Instance.StoreFile(repositoryItem);
//        }
//    }
//}