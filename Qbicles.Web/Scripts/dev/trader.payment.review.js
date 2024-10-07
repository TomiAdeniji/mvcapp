
//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    CheckStatusApproval(apprKey).then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object.toLocaleLowerCase() == statusOld.toLocaleLowerCase()) {
            // apply   
            $.LoadingOverlay("show");
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: $("#action_approval").val() },
                success: function (rs) {
                    RenderContentUpdated(true);
                },
                error: function (err) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            }).always(function () {

            });
            // and apply
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
            setTimeout(function () {
                RenderContentUpdated();
            }, 500);
        }
    });
};


function RenderContentUpdated(showMess) {
    $('#payment_content').empty();
    $('#payment_content').load('/TraderPayments/PaymentReview?id=' + $('#payment_id').val() + '&reload=true', function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);
    });
}

