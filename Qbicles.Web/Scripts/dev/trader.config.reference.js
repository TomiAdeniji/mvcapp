function confirm() {
    if (validateSetting()) {
        return;
    }
    var setting = {
        Id: $('#setting_id').val(),

        SalePrefix: $('#sale_prefix').val(),
        SaleSuffix: $('#sale_suffix').val(),
        SalesOrderPrefix: $('#sale_orderprefix').val(),
        SalesOrderSuffix: $('#sale_ordersuffix').val(),

        SaleReturnPrefix: $('#sale_return_prefix').val(),
        SaleReturnSuffix: $('#sale_return_suffix').val(),
        SalesReturnOrderPrefix: $('#sale_return_orderprefix').val(),
        SalesReturnOrderSuffix: $('#sale_return_ordersuffix').val(),

        PurchaseOrderPrefix: $('#purchase_orderprefix').val(),
        PurchaseOrderSuffix: $('#purchase_ordersuffix').val(),
        PurchasePrefix: $('#purchase_prefix').val(),
        PurchaseSuffix: $('#pruchase_suffix').val(),

        TransferPrefix: $('#transfer_prefix').val(),
        TransferSuffix: $('#transfer_suffix').val(),

        ManuJobPrefix: $('#manu_prefix').val(),
        ManuJobSuffix: $('#manu_suffix').val(),

        InvoicePrefix: $('#invoice_prefix').val(),
        InvoiceSuffix: $('#invoice_suffix').val(),
        Delimeter: $('#config_delimeter').val(),

        BillPrefix: $('#bill_prefix').val(),
        BillSuffix: $('#bill_suffix').val(),
        AllocationPrefix: $('#alllocation_prefix').val(),
        AllocationSuffix: $('#alllocation_suffix').val(),
        CreditNotePrefix: $('#creditnote_prefix').val(),
        CreditNoteSuffix: $('#creditnote_suffix').val(),
        DebitNotePrefix: $('#debitnote_prefix').val(),
        DebitNoteSuffix: $('#debitnote_suffix').val(),

        ReorderPrefix: $('#reorder_prefix').val(),
        ReorderSuffix: $('#reorder_suffix').val(),

        OrderPrefix: $('#order_prefix').val(),
        OrderSuffix: $('#order_suffix').val(),

        PaymentPrefix: $('#payment_prefix').val(),
        PaymentSuffix: $('#payment_suffix').val(),

        DeliveryPrefix: $('#delivery_prefix').val(),
        DeliverySuffix: $('#delivery_suffix').val(),

        AlertGroupPrefix: $('#alertgroup_prefix').val(),
        AlertGroupSuffix: $('#alertgroup_suffix').val(),

        AlertReportPrefix: $('#alertreport_prefix').val(),
        AlertReportSuffix: $('#alertreport_suffix').val(),
    }
    $.ajax({
        type: 'post',
        url: '/TraderConfiguration/UpdateSetting',
        data: { setting: setting },
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                if (response.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                    ShowSetting('references', setTabReference);
                }
                else if (response.actionVal === 2) {
                    cleanBookNotification.updateSuccess();
                    ShowSetting('references', setTabReference);
                }
                else if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_group_add');
    });
}
function selectTabMenu(tab) {
    setLocalStorage('reference-children-tab', tab);
}
function setTabReference() {
    var tab = getLocalStorage('reference-children-tab');
    if (!tab) {
        tab = 'refs-0';
        selectTabMenu(tab);
    }
    $('a[href="#' + tab + '"]').tab('show');

}
function resetsettingform(value) {

    $('#config_delimeter').val(value);
    $('#config_delimeter').select2({
        placeholder: "Please select"
    });
}
function validateSetting() {
    var valid = false;
    var delimeter = $('#config_delimeter').val();
    if (!delimeter || delimeter === "") {
        valid = true;
        cleanBookNotification.error(_L("ERROR_MSG_621"), "Qbicles");
    }
    return valid;
}
