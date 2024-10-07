function RenderContent(update) {
    $.LoadingOverlay("show");
    var $tillPaymentId = $("#tillpayment-id").val();
    $('#tillpayment-review-content').load('/CashManagement/TillPaymentReviewContent?tillPaymentId=' + $tillPaymentId, function () {
        $.LoadingOverlay("hide");
        $('.loadingoverlay').hide();
        if (update)
            cleanBookNotification.updateSuccess();
    });
}

function UpdateStatusApproval(apprKey, tillPaymentId) {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: $("#action_approval").val() },
        success: function (response) {
            if (response.actionVal > 0)
            {
                $.ajax({
                    url: "/CashManagement/UpdateTillPayment",
                    type: "POST",
                    dataType: "json",
                    data: { tillPaymentId: tillPaymentId }
                });
                RenderContent(true);
            }
            else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}