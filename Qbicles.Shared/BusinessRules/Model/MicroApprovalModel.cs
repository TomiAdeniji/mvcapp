using Newtonsoft.Json;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System.Collections.Generic;

namespace Qbicles.BusinessRules
{
    public class ApprovalOverviewModel
    {
        public int ActivityId { get; set; }
        public string ApprovalKey { get; set; }
        public string Reference { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string Total { get; set; }
        public ApprovalContactInfo ContactInfo { get; set; }        
        public string DeliveryAddress { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public string InQbicle { get; set; }
        public string VoucherType { get; set; }
        public string VoucherName { get; set; }
        public string VoucherDescription { get; set; }
    }


    public class ApprovalTeamModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string Roles { get; set; }
        public MicroCommunity DetailInfo { get; set; }
    }

    public class SalesContact
    {
        public string CreatedBy { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string Name { get; set; } = "";
        public string Img { get; set; } = "";
    }

    public class ApprovalTimelineModel
    {
        public string Date { get; set; }
        public List<ApprovalTimelineDetailModel> Timelines { get; set; } = new List<ApprovalTimelineDetailModel>();
    }

    public class ApprovalTimelineDetailModel
    {
        public string Time { get; set; }
        public string Img { get; set; }
        public string Description { get; set; }
    }

    public class ApprovalItemModel
    {
        public string Img { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Quantity { get; set; }
        public string UnitPrice { get; set; }
        public string Discount { get; set; }
        public string Taxes { get; set; }
        public string Total { get; set; }
    }

    public class ApprovalCommentModel: MicroActivityComment
    {
    }

    public class ApprovalFileModel: MicroActivityMedia
    {
    }
    
    public class MicroCommentApprovalModel
    {
        public string ApprovalKey { get; set; }
        public string Message { get; set; }
    }
    public class MicroApprovalModel
    {
        public string ApprovalKey { get; set; }
        public ApprovalReq.RequestStatusEnum Status { get; set; }
        public string Name { get; set; }
    }

    public class ApprovalContactInfo
    {
        public string ContactId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ContactReference { get; set; }
        public string Address { get; set; }
    }
}
