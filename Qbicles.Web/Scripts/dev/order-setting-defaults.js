
$(document).ready(function () {
    $('#b2b-location-setting,#b2c-location-setting').select2({ placeholder: "Choice an location" });
    //$('#default-location-setting').select2({ placeholder:"Choice an location"});
    B2BLocationChange();
    B2CLocationChange();
});

function ChangeB2bSetting() {
    var paramaters = {
        LocationId: $('#b2b-location-setting').val(),
        B2bOrder: $('#b2b_order').val(),
        //Sale defaults
        DefaultSaleWorkGroupId: $('#b2bSaleWg').val(),
        DefaultInvoiceWorkGroupId: $('#b2bInvoiceWg').val(),
        DefaultTransferWorkGroupId: $('#b2bSaleTransferWg').val(),
        DefaultPaymentWorkGroupId: $('#b2bSalePaymentWg').val(),
        DefaultPaymentAccountId: $('#b2bSaleCashBankAccount').val(),
        //Purchase defaults
        DefaultPurchaseWorkGroupId: $('#b2bPurchaseWg').val(),
        DefaultBillWorkGroupId: $('#b2bBillwg').val(),
        DefaultPurchasePaymentWorkGroupId: $('#b2bPurchasePaymentWg').val(),
        DefaultPurchaseTransferWorkGroupId: $('#b2bPurchaseTransferWg').val(),
        DefaultPurchasePaymentAccountId: $('#b2bPurchasePaymentAccount').val()
    };
    $.post("/TraderChannels/SaveB2BOrderConfigDefault", paramaters, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else {
            cleanBookNotification.error(response.msg, "Trader");
        }
    });
};

function ChangeB2cSetting() {

    if (!$update) return;

    var paramaters = {
        LocationId: $('#b2c-location-setting').val(),
        B2cOrder: $('#b2c_order').val(),
        DefaultSaleWorkGroupId: $('#b2cSaleWg').val(),
        DefaultInvoiceWorkGroupId: $('#b2cInvoiceWg').val(),
        DefaultTransferWorkGroupId: $('#b2cTransferWg').val(),
        DefaultPaymentWorkGroupId: $('#b2cPaymentWg').val(),
        DefaultPaymentAccountId: $('#b2cCashBankAccount').val(),
        SaveSettings: $('#b2cSaveSettings')[0].checked
    };

    if (paramaters.SaveSettings &&
        (paramaters.DefaultSaleWorkGroupId == '' || paramaters.DefaultInvoiceWorkGroupId == ''
            || paramaters.DefaultTransferWorkGroupId == '' || paramaters.DefaultPaymentWorkGroupId == ''
            || paramaters.DefaultPaymentAccountId == '')) {

        $update = false;
        $('#b2cSaveSettings').bootstrapToggle('off');
        $update = true;
        cleanBookNotification.error("All Workgroups processing are required!", "Trader");
        return;
    }


    $.post("/TraderChannels/SaveB2COrderConfigDefault", paramaters, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else {
            cleanBookNotification.error(response.msg, "Trader");
        }
    });
};

var $update = false;

function B2CLocationChange() {
    var locationId = $('#b2c-location-setting').val();
    $.getJSON('/SalesChannels/LoadB2C_WG_AC_ST_Default', { locationId: locationId },
        function (result) {
            //$('#b2b_order').empty();
            $('#b2c_order').empty();
            $('#b2cSaleWg').empty();
            $('#b2cInvoiceWg').empty();
            $('#b2cPaymentWg').empty();
            $('#b2cTransferWg').empty();
            $('#b2cCashBankAccount').empty();
            result.B2cOrderStatus.unshift({ id: "", text: "" });
            $('#b2c_order').select2({
                placeholder: "Select a state",
                data: result.B2cOrderStatus
            });
            result.SaleWorkgroups.unshift({ id: "", text: "" });
            $('#b2cSaleWg').select2({
                placeholder: "Choice a workgroup",
                data: result.SaleWorkgroups
            });
            result.InvoiceWorkgroups.unshift({ id: "", text: "" });
            $('#b2cInvoiceWg').select2({
                placeholder: "Choice a workgroup",
                data: result.InvoiceWorkgroups
            });
            result.PaymentWorkgroups.unshift({ id: "", text: "" });
            $('#b2cPaymentWg').select2({
                placeholder: "Choice a workgroup",
                data: result.PaymentWorkgroups
            });
            result.TransferWorkgroups.unshift({ id: "", text: "" });
            $('#b2cTransferWg').select2({
                placeholder: "Choice a workgroup",
                data: result.TransferWorkgroups
            });
            result.PaymentAccounts.unshift({ id: "", text: "" });
            $('#b2cCashBankAccount').select2({
                placeholder: "Choice an account",
                data: result.PaymentAccounts
            });

            $update = false;

            if (result.SaveSettings) {
                $('#b2cSaveSettings').bootstrapToggle('on');
            }
            else {
                $('#b2cSaveSettings').bootstrapToggle('off');
            }

            $('.defaults2').fadeIn();

            $update = true;
        });
};
function B2BLocationChange() {
    var locationId = $('#b2b-location-setting').val();
    $.getJSON('/SalesChannels/LoadB2B_WG_AC_ST_Default', { locationId: locationId },
        function (result) {
            $('#b2b_order').empty();
            //purchase
            $('#b2bPurchaseWg').empty();
            $('#b2bBillwg').empty();
            $('#b2bPurchasePaymentWg').empty();
            $('#b2bPurchaseTransferWg').empty();
            $('#b2bPurchasePaymentAccount').empty();
            //Sale
            $('#b2bSaleWg').empty();
            $('#b2bInvoiceWg').empty();
            $('#b2bSalePaymentWg').empty();
            $('#b2bSaleTransferWg').empty();
            $('#b2bSaleCashBankAccount').empty();
            //end
            result.B2bOrderStatus.unshift({ id: "", text: "" });
            $('#b2b_order').select2({
                placeholder: "Please select",
                data: result.B2bOrderStatus
            });
            //Purchase
            result.PurchaseWorkgroups.unshift({ id: "", text: "" });
            $('#b2bPurchaseWg').select2({
                placeholder: "Choice a workgroup",
                data: result.PurchaseWorkgroups
            });
            result.BillWorkgroups.unshift({ id: "", text: "" });
            $('#b2bBillwg').select2({
                placeholder: "Choice a workgroup",
                data: result.BillWorkgroups
            });
            result.PurchasePaymentWorkgroups.unshift({ id: "", text: "" });
            $('#b2bPurchasePaymentWg').select2({
                placeholder: "Choice a workgroup",
                data: result.PurchasePaymentWorkgroups
            });
            result.PurchaseTransfertWorkgroups.unshift({ id: "", text: "" });
            $('#b2bPurchaseTransferWg').select2({
                placeholder: "Choice a workgroup",
                data: result.PurchaseTransfertWorkgroups
            });
            result.PurchasePaymentAccounts.unshift({ id: "", text: "" });
            $('#b2bPurchasePaymentAccount').select2({
                placeholder: "Choice an account",
                data: result.PurchasePaymentAccounts
            });
            //Sale
            result.SaleWorkgroups.unshift({ id: "", text: "" });
            $('#b2bSaleWg').select2({
                placeholder: "Choice a workgroup",
                data: result.SaleWorkgroups
            });
            result.InvoiceWorkgroups.unshift({ id: "", text: "" });
            $('#b2bInvoiceWg').select2({
                placeholder: "Choice a workgroup",
                data: result.InvoiceWorkgroups
            });
            result.PaymentWorkgroups.unshift({ id: "", text: "" });
            $('#b2bSalePaymentWg').select2({
                placeholder: "Choice a workgroup",
                data: result.PaymentWorkgroups
            });
            result.TransferWorkgroups.unshift({ id: "", text: "" });
            $('#b2bSaleTransferWg').select2({
                placeholder: "Choice a workgroup",
                data: result.TransferWorkgroups
            });
            result.SalePaymentAccounts.unshift({ id: "", text: "" });
            $('#b2bSaleCashBankAccount').select2({
                placeholder: "Choice an account",
                data: result.SalePaymentAccounts
            });
            $('.defaults1').fadeIn();
        });
};
