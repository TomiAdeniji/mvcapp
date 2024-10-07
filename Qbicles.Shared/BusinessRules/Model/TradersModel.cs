using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.Trader.Settings;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class TraderContactModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Workgroup { get; set; }
        public string ContactGroup { get; set; }
        public string AccountBalance { get; set; }
        public string InvoiceBalance { get; set; }
        public string BillBalance { get; set; }
        public string Status { get; set; }
        public bool IsDisabled { get; set; }
        public string Action { get; set; }
        public string Key { get; set; }
        public string PIN { get; set; }
    }

    public class CashAccountTransactionModel
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Reference { get; set; }
        public string Source { get; set; }
        public string PaymentMethod { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
        public string InOut { get; set; }
        public string For { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }

    public class CashAccountTransactionReport
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Reference { get; set; }
        public string Source { get; set; }
        public string PaymentMethod { get; set; }
        public string Destination { get; set; }
        public string Type { get; set; }
        public string InOut { get; set; }
        public string For { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
    }

    public class MasterSetupModel
    {
        public List<MasterSetup> MasterSetups { get; set; }
        public List<TraderGroup> TraderGroups { get; set; }
        public List<TaxRate> PurchaseTaxRates { get; set; }
        public List<TaxRate> SaleTaxRates { get; set; }
    }

    public class TabGroupConfigModel
    {
        public MasterSetup MasterSetup { get; set; }
        public TraderGroup TraderGroup { get; set; }
        public List<TaxRate> PurchaseTaxRates { get; set; }
        public List<TaxRate> SaleTaxRates { get; set; }
        public List<TraderLocation> Locations { get; set; }
        public List<SalesChannelEnum> SalesChannels { get; set; }
        public IBuySettingsModel IBuySettings { get; set; }
        public IBuySellSettingsModel IBuySellSettings { get; set; }
        public ISellSCompoundSettingsModel ISellSCompoundSettings { get; set; }
        public ISellServiceSettingsModel ISellServiceSettings { get; set; }
    }

    public class IBuySettingsModel
    {
        public List<int> ibuy_purchaseTaxRate { get; set; }
        public int ibuy_purchaseAccount { get; set; }
        public int ibuy_pnventoryAccount { get; set; }
        public string ibuy_purchaseAccountText { get; set; }
        public string ibuy_pnventoryAccountText { get; set; }
    }

    public class IBuySellSettingsModel
    {
        public List<int> ibuysell_purchaseTaxRate { get; set; }
        public int ibuysell_purchaseAccount { get; set; }
        public List<int> ibuysell_salesTaxRate { get; set; }
        public int ibuysell_salesAccount { get; set; }
        public int ibuysell_inventoryAccount { get; set; }
        public string ibuysell_purchaseAccountText { get; set; }
        public string ibuysell_salesAccountText { get; set; }
        public string ibuysell_inventoryAccountText { get; set; }
    }

    public class ISellSCompoundSettingsModel
    {
        public List<int> isellcompound_salesTaxRate { get; set; }
        public int isellcompound_salesAccount { get; set; }
        public int isellcompound_inventoryAccount { get; set; }
        public string isellcompound_salesAccountText { get; set; }
        public string isellcompound_inventoryAccountText { get; set; }
    }

    public class ISellServiceSettingsModel
    {
        public List<int> isellservices_salesTaxRate { get; set; }
        public int isellservices_salesAccount { get; set; }
        public int isellservices_inventoryAccount { get; set; }
        public string isellservices_salesAccountText { get; set; }
        public string isellservices_inventoryAccountText { get; set; }
    }

    public class ApplyTypeSettingsModel
    {
        public int GroupId { get; set; }
        public IBuySettingsModel IBuy { get; set; }
        public IBuySellSettingsModel IBuySell { get; set; }
        public ISellSCompoundSettingsModel ISellSCompound { get; set; }
        public ISellServiceSettingsModel ISellService { get; set; }
        public ApplyTypeEnum ApplyType { get; set; }

        public enum ApplyTypeEnum
        {
            All = 0,
            IBuy = 1,
            IBuySell = 2,
            ISellSCompound = 3,
            ISellService = 4
        }
    }

    public class AccountingItemSettingsModel
    {
        public int TraderItemId { get; set; }
        public IBuySettingsModel IBuy { get; set; }
        public IBuySellSettingsModel IBuySell { get; set; }
        public ISellSCompoundSettingsModel ISellSCompound { get; set; }
        public ISellServiceSettingsModel ISellService { get; set; }
        public ApplyTypeEnum ApplyType { get; set; }

        public enum ApplyTypeEnum
        {
            All = 0,
            IBuy = 1,
            IBuySell = 2,
            ISellSCompound = 3,
            ISellService = 4
        }
    }

    public class ConfigsPriceModel
    {
        public int GroupId { get; set; }
        public List<int> Locations { get; set; }
        public List<int> Salechannels { get; set; }
        public decimal MarkupValue { get; set; }
        public int MarkupMethod { get; set; }
        public decimal DiscountValue { get; set; }
        public int DiscountMethod { get; set; }
        public bool IsExistingOverwritten { get; set; }
    }

    public class ReorderItemGroupCustomModel
    {
        public int Id { get; set; }
        public int ReorderId { get; set; }
        public int DomainId { get; set; }
        public int PrimaryContactId { get; set; }
        public Models.Trader.DeliveryMethodEnum? DeliveryMethod { get; set; }
        public int DaysToLastBasis { get; set; }
        public int DaysToLast { get; set; }
        public string Days2Last { get; set; }
        public List<ReorderItemCustomModel> Items { get; set; }
    }

    public class ReorderItemCustomModel
    {
        public int Id { get; set; }
        public List<int> Dimensions { get; set; }
        public int PrimaryContactId { get; set; }
        public int UnitId { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal Discount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsForReorder { get; set; }
    }

    public class ReorderCustomModel
    {
        public int Id { get; set; }
        public int WorkgroupId { get; set; }
        public int? ExcludeGroupId { get; set; }
        public Models.Trader.DeliveryMethodEnum Delivery { get; set; }
        public string TypeSubmit { get; set; }
    }

    public class TraderProcessesConst
    {
        public const string Purchase = "Purchase";
        public const string Sale = "Sale";
        public const string Transfer = "Transfer";
        public const string Payment = "Payment";
        public const string Contact = "Contact";
        public const string Invoice = "Invoice";
        public const string SpotCount = "Spot Count";
        public const string WasteReport = "Waste Report";
        public const string Manufacturing = "Manufacturing";
        public const string StockAudits = "Stock Audits";
        public const string PointOfSale = "Point of Sale";
        public const string CreditNotes = "Credit Notes";
        public const string Budget = "Budget";
        public const string ShiftAudit = "Shift Audit";
        public const string SaleReturn = "Sale Return";
        public const string CashManagement = "Cash Management";
        public const string Reorder = "Reorder";
    }

    public class VariantOptionModel
    {
        public List<Models.Catalogs.VariantOption> Options { get; set; }
        public string Name { get; set; }
    }

    public class TraderConst
    {
        public const string DefaultTraderLocationName = "Main";
        public const string DefaultTraderWorkGroupName = "Main";
        public const string DefaultTraderCashAccountName = "Main";
        public const string DefaultTraderGroupName = "Unassigned";
        public const string DefaultTransactionDimensionName = "Unassigned";
    }

    public class PaymentMethodNameConst
    {
        public const string Cash = "Cash";
        public const string Card = "Card";
        public const string ElectronicTransfer = "Electronic Transfer";
        public const string CashOnDelivery = "Cash on Delivery";
        public const string StoreCredit = "Store Credit";
    }

    public class CompressTraderTransferItem
    {
        public virtual TraderItem TraderItem { get; set; }
        public virtual TraderTransactionItem TransactionItem { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class ManufactureProductModel
    {
        public TraderTransfer tradTransferOut { get; set; }
        public TraderTransfer tradTransferIn { get; set; }
        public decimal compoundItemUnitCost { get; set; }
    }

    public class InvoiceItemModel
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public bool InvoiceChecked { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public decimal InvoiceTaxes { get; set; }
        public decimal InvoiceDiscount { get; set; }
        public decimal InvoiceCost { get; set; }
        public string TransItemUri { get; set; }
        public int TransItemId { get; set; }
        public string TransItemName { get; set; }
        public string TransItemUnitName { get; set; }
        public decimal TransItemQuantity { get; set; }
        public decimal TransItemDiscount { get; set; }
        public decimal TransItemSumValTaxRates { get; set; }
        public string TransItemHtmlTaxRates { get; set; }
        public string InvoiceTaxesInfo { get; set; }
        public decimal TransItemCost { get; set; }
        public decimal PricePerUnit { get; set; }
    }

    public class ItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public string SKU { get; set; }
        public List<ItemUnitModel> Units { get; set; } = new List<ItemUnitModel>();
    }

    public class ItemUnitModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
    }
}