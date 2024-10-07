var $manuJobId = $("#manu-job-id").val();


$(function () {
    $('textarea').each(function () {
        $(this).val($(this).val().trim());
    }
    );
});

//approval update
function UpdateStatusApproval(apprKey) {   
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    CheckStatusApproval(apprKey).then(function (res) {
        LoadingOverlayEnd();
       
        if (res.result && res.Object.toLocaleLowerCase() == statusOld.toLocaleLowerCase()) {
            // apply  

            $('#approval-button div.loading-approval').removeClass('hidden');
            $('#approval-button div.general-approval').addClass('hidden');
            $('#approval-button').addClass('animated shake disabled');
            $('#spot-count-item-tbody').addClass('disabled');
            var appStatus = $("#action_approval").val();
            LoadingOverlay();
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: appStatus },
                success: function (rs) {
                   
                    LoadingOverlayEnd();
                    if (rs.actionVal > 0) {
                        RenderContentUpdated(true);
                    }
                    $('#approval-button div.loading-approval').addClass('hidden');
                    $('#approval-button div.general-approval').removeClass('hidden');
                },
                error: function (err) {
                    LoadingOverlayEnd();
                    $('#spot-count-item-tbody').removeClass('disabled');
                    $('#approval-button div.loading-approval').addClass('hidden');
                    $('#approval-button div.general-approval').removeClass('hidden');
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
            // and apply
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
            setTimeout(function () {
                RenderContentUpdated();
            }, 1500);
        }
    });

};

//Update ManuJob
UpdateManuJobReview = function () {
    $.LoadingOverlay("show");
    CheckStatus($manuJobId, 'ManuJob').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object != "Approved") {
            // load form update
            $.LoadingOverlay("show");

            var unit = {
                Id: $("#unit-select").val()
            }

            var manuJob = {
                Id: $manuJobId,
                AssemblyUnit: unit,
                Quantity: $("#item-quantity").val()
            }

            $.ajax({
                type: 'post',
                url: '/Manufacturing/UpdateManuJobReview',
                data: { manuJob: manuJob },
                dataType: 'json',
                success: function (response) {
                    LoadingOverlayEnd();
                    cleanBookNotification.updateSuccess();
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });

        } else if (res.result && res.Object == "Approved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
        LoadingOverlayEnd();
    });




};

function RenderContentUpdated(showMess) {
    LoadingOverlay();
    $('#manujob_view_content').empty();
    $('#manujob_view_content').load('/Manufacturing/ManuJobReviewContent?id=' + $manuJobId, function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);
    });
}