var $invoiceKey = $('#invoice-key').val();
//approval update
function UpdateStatusApproval(apprKey) {

    $.LoadingOverlay("show");
    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: $("#action_approval").val() },
        success: function (rs) {
            if (rs.actionVal > 0) {
                RenderContentUpdated();
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

};

function RenderContentUpdated() {
    LoadingOverlay();
    $('#invoice_content').empty();
    $('#invoice_content').load('/TraderInvoices/InvoiceReviewContent?key=' + $invoiceKey, function () {
        LoadingOverlayEnd();
        cleanBookNotification.updateSuccess();
    });
}


var dueDateOld = "";
var dueDateChanged = "";

$('#invoice-duedate').on('show.daterangepicker', function (ev, picker) {
    dueDateOld = picker.startDate.format('YYYY-MM-DD');
});

$('#invoice-duedate').on('apply.daterangepicker', function (ev, picker) {
    dueDateChanged = picker.startDate.format('YYYY-MM-DD');
    if (dueDateOld !== dueDateChanged)
        UpdateInvoiceReview();
});

var strValueOld = "";
var strValueChanged = "";
function OnFocusOutControl(value) {
    strValueChanged = value;
    if (strValueChanged !== strValueOld)
        UpdateInvoiceReview();
};

function OnFocusControl(value) {
    strValueOld = value;
};

function UpdateInvoiceReview() {
    $.LoadingOverlay("show");
    CheckStatus($("#invoice-id").val(), 'Invoice').then(function (res) {
        
        LoadingOverlayEnd();
        if (res.result && res.Object == "PendingReview") {
            $.LoadingOverlay("show");
            if (dueDateChanged === "") {
                //specify the date string and the format it's initially in
                var mydate = moment($('#invoice-duedate').val(), 'DD/MM/YYYY');
                //format that date into a different format
                dueDateChanged = moment(mydate).format("MM/DD/YYYY");
            }


            var invoice = {
                Key: $("#invoice-key").val(),
                DueDate: dueDateChanged,
                InvoiceAddress: $("#invoice-invoiceaddress").val(),
                PaymentDetails: $("#invoice-paymentdetails").val()
            };

            $.ajax({
                url: "/TraderInvoices/UpdateInvoiceReview",
                type: "POST",
                dataType: "json",
                data: { invoice: invoice },
                success: function (rs) {
                    if (rs.result) {
                        cleanBookNotification.updateSuccess();
                    } else {
                        cleanBookNotification.UpdateFail();
                    }
                    setTimeout(function () {
                        $.LoadingOverlay("hide");
                    }, 500);
                },
                error: function (err) {
                    $.LoadingOverlay("hide");
                    Console.log(err);
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else if (res.result && res.Object != "PendingReview") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });


};