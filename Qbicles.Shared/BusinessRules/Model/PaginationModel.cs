using System.Collections;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class PaginationResponseAlert: PaginationResponse
    {
        public bool IsShowAlertBusiness { get; set; }
        public bool IsShowAlertCustomer { get; set; }
    }
    public class PaginationResponse
    {
        public IEnumerable items { get; set; }
        public int totalNumber { get; set; } = 0;
        public int totalPage { get; set; } = 0;
        public IEnumerable listBrands { get; set; }
    }
    public class PaginationRequest
    {
        public string keyword { get; set; }
        public string orderBy { get; set; }
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
    }

    public class AlertNotificationParameter
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public List<int> Ids { get; set; } = new List<int>();
        public bool IsShowAlertCustomer { get; set; } = true;
        public bool IsShowAlertBusiness { get; set; } = true;
    }
}
