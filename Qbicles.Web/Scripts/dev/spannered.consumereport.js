
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
            $.LoadingOverlay("show");
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: appStatus },
                success: function (rs) {
                    LoadingOverlayEnd();
                    if (rs.actionVal > 0) {
                        location.reload();
                    }
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
function updateUsedConsumeItems(ciId,value) {
    $.post("/Spanneredfree/UpdateUsedOfConsumeItems", { ciId: ciId, value:value }, function (response) {
        if(response.result)
        {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
        }else if(!response.result&&response.msg)
        {
            cleanBookNotification.error(_L(response.msg), "Spannered");
        }
        else
        {
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
        }
    });
}
$(document).ready(function () { 
    $('#txtfilterSearch').keyup(delay(function () {
        $('#tblConsumeItems').DataTable().search($(this).val()).draw();
    },500));
    $('#slfiltergroup').change(function () {
        var value = $(this).val();
        $('#tblConsumeItems').DataTable().columns(4).search(value=='0'?'':value).draw();
    });
    $('#tblConsumeItems_filter').hide();
});