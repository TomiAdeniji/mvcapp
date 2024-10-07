
//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
      
    $.LoadingOverlay("show");
    CheckStatusApproval(apprKey).then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object.toLocaleLowerCase() == statusOld.toLocaleLowerCase()) {
            $.LoadingOverlay("show");
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: $("#action_approval").val() },
                success: function (rs) {
                    $.LoadingOverlay("hide");
                    RenderContentUpdated(true); 
                },
                error: function (err) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
            setTimeout(function () {
                RenderContentUpdated(); 
            }, 1500);
        } 
    });


    
};

function RenderContentUpdated(showMess) {
    LoadingOverlay();
    $('#bill-content').empty();
    $('#bill-content').load('/TraderBill/BillReview?id=' + $("#bill-id").val()+'&isReload=true', function () {
        LoadingOverlayEnd();
        if (showMess)
            cleanBookNotification.updateSuccess();
    });
}