
var $saleReturnId = $("#sale-return-id").val();
//approval update
function UpdateStatusApproval(apprKey) {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: $("#action_approval").val() },
        success: function (response) {
            if (response.actionVal > 0)
                RenderContentUpdated(true);
            else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function RenderContentUpdated(showMess) {

    $('#sale-return-review-content').empty();
    $('#sale-return-review-content').load('/TraderSalesReturn/SaleReturnReviewContent?id=' + $saleReturnId, function () {

        LoadingOverlayEnd(); cleanBookNotification.updateSuccess();
    });
}