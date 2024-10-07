namespace Qbicles.BusinessRules
{
	using System.ComponentModel;
	public static class Enums
	{

		public enum EndpointApi
		{
			DocApi = 1,
			TraderApi = 2,
			QbiclesJobApi = 3,
			SignalRApi = 4
		}

		public enum PromotionType
		{
			Premium = 1,
			Pinned = 2
		}

		public enum RepositoryItemType
		{
			Qbicle = 1,
			Task = 2,
			Discussion = 3,
			Alert = 4,
			Media = 5,
			Event = 6,
			Approval = 7,
			Process = 8,
			UploadTransaction = 9,
			UserAvata = 10,
			Bookkepping = 11,
			Community = 12,
			Domain = 13,
			Trader = 14,
			Default = 15
		}

		public enum AmountType
		{
			Credit = 1,
			Debit = 2,
			Balance = 3
		}
		public enum QbicleDisplayOrderEnum
		{
			LatestActivity = 1,
			OpenQbicles = 2,
			ClosedQbicles = 3
		}

		public enum EmailSendResult
		{
			CompleteSend = 1,
			FalseSend = 2,
			FalseSaveSendLog = 3,
			EmailConfigNotExist = 4,
		}

		public enum QbicleModule
		{
			[Description("Dashboard")]
			Dashboard = 1,
			[Description("Discussions")]
			Discussions = 2,
			[Description("Tasks")]
			Tasks = 3,
			[Description("Alerts")]
			Alerts = 4,
			[Description("Media")]
			Media = 5,
			[Description("Events")]
			Events = 6,
			[Description("Settings")]
			Settings = 7,
			[Description("SubActivities")]
			SubActivities = 8,
			[Description("Approvals")]
			Approvals = 9,
			[Description("Topic")]
			Topic = 10,
			[Description("Post")]
			Post = 11,
			[Description("Links")]
			Links = 12,
			[Description("Orders")]
			DiscussionsOrders = 13

		}

		public enum OrderByDate
		{
			[Description("Newest First")]
			NewestFirst = 1,
			[Description("Oldest First")]
			OldestFirst = 2
		}

		public enum TaskStatus
		{
			[Description("All Tasks")]
			AllTasks = 1,
			[Description("My Tasks")]
			MyTasks = 2,
			[Description("Only Regular Tasks")]
			OnlyRegularTasks = 3,
			[Description("Only Form Tasks")]
			OnlyFormTasks = 4,
			[Description("Completed Tasks")]
			CompletedTasks = 5
		}

		public enum DiscussionStatus
		{
			[Description("All discussions")]
			AllDiscussions = 1,
			[Description("My discussions")]
			MyDiscussions = 2
		}

		public enum AlertStatus
		{
			[Description("All alerts")]
			AllAlerts = 1,
			[Description("General alerts")]
			GeneralAlerts = 2,
			[Description("Logistics alerts")]
			LogisticsAlerts = 3,
			[Description("Staffing alerts")]
			StaffingAlerts = 4,
			[Description("Have attachments")]
			HaveAttachments = 5,
			[Description("My alerts")]
			MyAlerts = 6

		}

		public enum ActivityStatus
		{
			[Description("Newest First")]
			NewestFirst = 1,
			[Description("Oldest First")]
			OldestFirst = 2,
		}

		public enum TypeGetData
		{
			Filter = 1,
			LoadNext = 2
		}

		public enum NotificationEvent
		{
			[Description("created")]
			Created = 1,
			[Description("updated")]
			Updated = 2
		}

		/// <summary>
		/// file type id, id file type in filetypes table
		/// </summary>
		public enum FileTypeId : long
		{
			excelId = 4,
			csvId = 3
		}

		public enum FileTypeEnum
		{
			Image = 1,
			Video = 2,
			Document = 3
		}


		public enum TaskExecutionInterval : int
		{
			[Description("Daily")]
			Daily = 1,
			[Description("Weekly")]
			Weekly = 2,
			[Description("Monthly")]
			Monthly = 3,
			[Description("Annual")]
			Yearly = 4
		}
		public enum UpdateFrequency : int
		{
			[Description("Daily")]
			Daily = 1,
			[Description("Weekly")]
			Weekly = 2,
			[Description("Monthly")]
			Monthly = 3,
			[Description("Annual")]
			Yearly = 4
		}
		public enum tranMatchingType : int
		{
			[Description("Not Applicable")]
			NotApplicable = 1,
			[Description("Debit to Debit")]
			DebitToDebit = 2,
			[Description("Debit to Credit")]
			DebitToCredit = 3
		}


		public static class TypeOfTask
		{
			[Description("Transaction Matching")]
			public const int TransactionMatching = 1;

			[Description("Transaction Analysis")]
			public const int TransactionAnlysis = 2;

			[Description("Control Report")]
			public const int ControlReport = 3;
			[Description("Upload Data")]
			public const int UploadData = 4;
			[Description("Balance Analysis")]
			public const int BalanceAnalysis = 4;

		}

		/// <summary>
		/// get reason sent email
		/// </summary>
		public enum ReasonSentEmail
		{
			[Description("Confirm your account")]
			UserCreation = 1,
			[Description("Reset your password")]
			ForgotPassword
		}
		/// <summary>
		/// transaction matching by field setup
		/// </summary>
		public static class TransactionMatchingBy
		{
			public const string ManyToMany = "ManyToMany";
			public const string ReferenceAndDate = "ReferenceAndDate";
			public const string ReferenceToReference1AndDate = "ReferenceToReference1AndDate";
			public const string ReferenceToDescriptionAndDate = "ReferenceToDescriptionAndDate";
			public const string Reference = "Reference";
			public const string ReferenceToReference1 = "ReferenceToReference1";
			public const string ReferenceToDescription = "ReferenceToDescription";
			public const string DescriptionAndDate = "DescriptionAndDate";
			public const string Description = "Description";
			public const string AmountAndDate = "AmountAndDate";
			public const string Reversals = "Reversals";
			public const string Manual = "Manual";
		}

		public static class transactionmatchingrelationshipId
		{
			public const int NotSet = 1;
			public const int OneToOne = 2;
			public const int OneToMany = 3;
			public const int ManyToOne = 4;
			public const int ManyToMany = 5;
		}

		public static class transactionmatchingmethodId
		{
			public const int NotSet = 1;
			public const int ManyToMany = 2;
			public const int ReferenceAndDate = 3;
			public const int ReferenceToReference1AndDate = 4;
			public const int ReferenceToDescriptionAndDate = 5;
			public const int Reference = 6;
			public const int ReferenceToReference1 = 7;
			public const int ReferenceToDescription = 8;
			public const int DescriptionAndDate = 9;
			public const int Description = 10;
			public const int AmountAndDate = 11;
			public const int Reversals = 12;
			public const int Manual = 13;
		}
		public static class changeType
		{
			public const string NEW = "NEW";
			public const string EDIT = "EDIT";
			public const string DELETE = "DELETE";
			public const string LOGIN = "LOGIN";
			public const string LOGOUT = "LOGOUT";
			public const string TaskRun = "TaskRun";
			public const string ReportGenerated = "ReportGenerated";
			public const string UnMatch = "UnMatch";
			public const string ManualMatch = "Manual Match";
		}
		public enum SortOrderBy
		{
			[Description("Order alphabetically A-Z")]
			NameAZ = 1,
			[Description("Order alphabetically Z-A")]
			NameZA = 2,
			[Description("Order by balance low to high")]
			BalanceHigh = 3,
			[Description("Order by balance high to low")]
			BalanceLow = 4,
			[Description("Last Update Date (Newest-Oldest)")]
			LastUpdateNewest = 5,
			[Description("Last Update Date (Oldest-Newest)")]
			LastUpdateOldest = 6,
			[Description("Order by Data Manager (A-Z)")]
			DataManagerAZ = 7,
			[Description("Order by Data Manager (Z-A)")]
			DataManagerZA = 8,
			[Description("Order by Linked Tasks (Most-Least)")]
			LinkedTasksMost = 9,
			[Description("Order by Linked Tasks (Least-Most)")]
			LinkedTasksLeast = 10,
			[Description("Order by Account 1 balance low to high")]
			Account1BalanceHigh = 11,
			[Description("Order by Account 1 balance high to low")]
			Account1BalanceLow = 12,
			[Description("Order by Account 2 balance low to high")]
			Account2BalanceHigh = 13,
			[Description("Order by Account 2 balance high to low")]
			Account2BalanceLow = 14,
			[Description("Order by Unmatched (Most-Least)")]
			UnmatchedMost = 15,
			[Description("Order by Unmatched (Least-Most)")]
			UnmatchedLeast = 16,
			[Description("Order by Instances (Most-Least)")]
			InstancesMost = 17,
			[Description("Order by Instances (Least-Most)")]
			InstancesLeast = 18
		}
	}
	public static class TransactionMatchingBy
	{
		public const string ManyToMany = "ManyToMany";
		public const string ReferenceAndDate = "ReferenceAndDate";
		public const string ReferenceToReference1AndDate = "ReferenceToReference1AndDate";
		public const string ReferenceToDescriptionAndDate = "ReferenceToDescriptionAndDate";
		public const string Reference = "Reference";
		public const string ReferenceToReference1 = "ReferenceToReference1";
		public const string ReferenceToDescription = "ReferenceToDescription";
		public const string DescriptionAndDate = "DescriptionAndDate";
		public const string Description = "Description";
		public const string AmountAndDate = "AmountAndDate";
		public const string Reversals = "Reversals";
		public const string Manual = "Manual";
	}

	public static class transactionmatchingrelationshipId
	{
		public const int NotSet = 1;
		public const int OneToOne = 2;
		public const int OneToMany = 3;
		public const int ManyToOne = 4;
		public const int ManyToMany = 5;
	}

	public static class transactionmatchingmethodId
	{
		public const int NotSet = 1;
		public const int ManyToMany = 2;
		public const int ReferenceAndDate = 3;
		public const int ReferenceToReference1AndDate = 4;
		public const int ReferenceToDescriptionAndDate = 5;
		public const int Reference = 6;
		public const int ReferenceToReference1 = 7;
		public const int ReferenceToDescription = 8;
		public const int DescriptionAndDate = 9;
		public const int Description = 10;
		public const int AmountAndDate = 11;
		public const int Reversals = 12;
		public const int Manual = 13;
	}
	public static class changeType
	{
		public const string NEW = "NEW";
		public const string EDIT = "EDIT";
		public const string DELETE = "DELETE";
		public const string LOGIN = "LOGIN";
		public const string LOGOUT = "LOGOUT";
		public const string TaskRun = "TaskRun";
		public const string ReportGenerated = "ReportGenerated";
		public const string UnMatch = "UnMatch";
		public const string ManualMatch = "Manual Match";
	}

	public enum MediaPreviewType
	{
		Qbicle = 6,
		UserImage = 1,
		FileImage = 2,
		FileDocs = 3,
		FileCompressed = 4,
		PostAvatar = 5
	}

	public enum MoneyCurrency
	{
		[Description("£")]
		GBP = 1,
		[Description("₦")]
		NGN = 2,
		[Description("$")]
		USD = 3
	}

	public enum DecimalPlace
	{
		Zero = 0,
		One = 1,
		Two = 2
	}

	public enum PosUserType
	{
		[Description("Don't include")]
		None = 1,
		[Description("Include as Till User")]
		User = 2,
		[Description("Include as Till Cashier")]
		Cashier = 3,
		[Description("Include as Till Supervisor")]
		Supervisor = 4,
		[Description("Include as Till Manager")]
		Manager = 5,
		[Description("Include as PoS Administrator")]
		Admin = 6,
	}
	public static class MyDeskKeyStoredUiSettings
	{
		public const string ddlOrderbyPinned = "ddlOrderbyPinned";
		public const string ddlOrderbyTask = "ddlOrderbyTask";
		public const string ddlOrderbyEvent = "ddlOrderbyEvent";
		public const string ddlOrderbyMedia = "ddlOrderbyMedia";
		public const string ddlOrderbyLink = "ddlOrderbyLink";
		public const string ddlOrderbyDiscussion = "ddlOrderbyDiscussion";
		public const string ddlOrderbyProcess = "ddlOrderbyProcess";
		public const string txtDaterangeTask = "txtDaterangeTask";
		public const string txtDaterangeEvent = "txtDaterangeEvent";
		public const string txtDaterangePined = "txtDaterangePined";
		public const string txtDaterangeMedia = "txtDaterangeMedia";
		public const string txtDaterangeLink = "txtDaterangeLink";
		public const string txtDaterangeDiscussion = "txtDaterangeDiscussion";
		public const string txtDaterangeProcess = "txtDaterangeProcess";
		public const string dllDomainIdPined = "dllDomainIdPined";
		public const string dllDomainIdTask = "dllDomainIdTask";
		public const string dllDomainIdEvent = "dllDomainIdEvent";
		public const string dllDomainIdMedia = "dllDomainIdMedia";
		public const string dllDomainIdLink = "dllDomainIdLink";
		public const string dllDomainIdDiscussion = "dllDomainIdDiscussion";
		public const string dllDomainIdProcess = "dllDomainIdProcess";
		public const string ddlQbicleIdPined = "ddlQbicleIdPined";
		public const string ddlQbicleIdTask = "ddlQbicleIdTask";
		public const string ddlQbicleIdEvent = "ddlQbicleIdEvent";
		public const string ddlQbicleIdMedia = "ddlQbicleIdMedia";
		public const string ddlQbicleIdLink = "ddlQbicleIdLink";
		public const string ddlQbicleIdDiscussion = "ddlQbicleIdDiscussion";
		public const string ddlQbicleIdProcess = "ddlQbicleIdProcess";
		public const string ddlTagsLink = "ddlTagsLink";
		public const string ddlTagsTask = "ddlTagsTask";
		public const string ddlTagsEvent = "ddlTagsEvent";
		public const string ddlTagsMedia = "ddlTagsMedia";
		public const string ddlTagsPined = "ddlTagsPined";
		public const string ddlTagsDiscussion = "ddlTagsDiscussion";
		public const string ddlTagsProcess = "ddlTagsProcess";
		public const string isHideInPined = "isHideInPined";
		public const string isHideInTask = "isHideInTask";
		public const string isHideInEvent = "isHideInEvent";
		public const string isHideInDiscussion = "isHideInDiscussion";
		public const string ddlActType = "ddlActType";

		public const string ddlStatusTask = "ddlStatusTask";
		public const string ddlStatusProcess = "ddlStatusProcess";
		public const string ddlType = "ddlType";
		public const string ddlUserTask = "ddlUserTask";
		public const string ddlUserEvent = "ddlUserEvent";
		public const string tabIndex = "tabIndex";
		public const string calendarDate = "calendarDate";

	}
	public static class QbiclePages
	{
		public const string pageSalesMarketing = "SalesMarketing";
		public const string pageSpannered = "Spannered";
	}
	public static class SaleMarketingStoreUiSettings
	{
		public const string tabActive = "tabActive";
	}

	public enum TransferType
	{
		P2P = 1,
		Sale = 2,
		Purchase = 3
	}
	public static class SpanneredKeyStoredUiSettings
	{
		public const string ddlLocationId = "ddlLocationId";
	}
	public enum AdminLevel
	{
		[Description("Domain User")]
		Users = 1,
		[Description("Qbicle Creator")]
		QbicleManagers = 2,
		[Description("Domain administrator")]
		Administrators = 3
	}

	public enum StreamType
	{
		[Description("Discussion")]
		Discussion = 1,
		[Description("Alert")]
		Alert = 2,
		[Description("Event")]
		Event = 3,
		[Description("Task")]
		Task = 4,
		[Description("Approval")]
		Approval = 5,
		[Description("Media")]
		Medias = 6,
		[Description("bookkeeping")]
		Bookkeeping = 7,
		[Description("Link")]
		Link = 8,
		[Description("Chat")]
		Post = 9,
		[Description("Trader Transfer")]
		Transfer = 10,
		[Description("Shift Audit")]
		StockAudits = 11,
		[Description("Sale")]
		Sale = 12,
		[Description("Purchase")]
		Purchase = 13,
		[Description("Trader Contact")]
		TraderContact = 14,
		[Description("Invoice")]
		Invoice = 15,
		[Description("Payments")]
		Payments = 16,
		[Description("Spot Counts")]
		SpotCounts = 17,
		[Description("Waste Reports")]
		WasteReports = 18,
		[Description("Manufacturing jobs")]
		Manufacturingjobs = 19,
		[Description("CreditNotes")]
		CreditNotes = 20,
		[Description("Budget Scenario Item Groups")]
		BudgetScenarioItemGroups = 21,
		[Description("Trader Returns")]
		TraderReturns = 22,
		[Description("Consumption Reports")]
		ConsumptionReports = 23,
		[Description("Till Payment")]
		TillPayment = 24,
		[Description("Approval Request")]
		ApprovalRequestApp = 25,
		jounralEntries = 26,
		[Description("Campaig Post")]
		CampaigPost = 27,
		[Description("Campaig Email Post")]
		EmailPost = 28,
		[Description("Operator Clock In")]
		OperatorClockIn = 29,
		[Description("Operator Clock Out")]
		OperatorClockOut = 30,
		[Description("Purchase Invoice")]
		InvoicePurchase = 31,
		[Description("Sale Invoice")]
		InvoiceSale = 32,
		[Description("Cleanbook Task")]
		Cleanbook = 33,
		[Description("ComplianceTask Task")]
		ComplianceTask = 34,
		[Description("Qbicles")]
		Qbicles = 35,
		[Description("Discussion Order")]
		DiscussionOrder = 36,
		[Description("Share Highlight")]
		HLSharedPost = 37,
		[Description("Shared Promotion")]
		LoyaltySharedPromotion = 38,
	}

	public enum B2CFilterInvoiceType
	{
		[Description("Show all")]
		ShowAll = 0,
		[Description("Not provided")]
		NotProvided = 1,
		[Description("Provided")]
		Provided = 2
	}
	public enum B2CFilterPaymentType
	{
		[Description("Show all")]
		ShowAll = 0,
		[Description("Not paid")]
		NotPaid = 1,
		[Description("Paid in part")]
		PaidInPart = 2,
		[Description("Paid in full")]
		PaidInFull = 3
	}
	public enum B2CFilterDeliveryType
	{
		[Description("Show all")]
		ShowAll = 0,
		[Description("Not assigned")]
		NotAssigned = 1,
		[Description("In progress")]
		InProgress = 2,
		[Description("Delivered")]
		Delivered = 3
	}

	public enum ChatType
	{
		Qbicle = 1,
		B2C = 2,
		C2C = 3,
		/// <summary>
		/// B2B
		/// </summary>
		Commerce = 4,
		/// <summary>
		/// Chat on B2C order
		/// </summary>
		Order = 5
	}
	public enum OrderChatTo
	{
		None = 0,
		Business = 1,
		Customer = 2
	}
	public enum VoucherOfUserType
	{
		[Description("Valid")]
		IsValid = 0,
		[Description("Redeemed")]
		IsRedeemed = 1,
		[Description("Expired")]
		IsExpired = 2
	}
}
