
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
                        RenderContentUpdated(true);
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
function RenderContentUpdated(showMess) {
    LoadingOverlay();
    $('#waste_content').empty();
    $('#waste_content').load('/TraderWasteReport/UpdateWasteReportContent?id=' + $('#wasteReport_id').val(), function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);
    });
}


function UpdateItems(itemId) {
    var wasteId = $('#wasteReport_id').val();
    var statusold = $('#wasteReport_status').val();
    $.LoadingOverlay("show");
    CheckStatus(wasteId, 'WasteItem').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object.toLocaleLowerCase() == statusold.toLocaleLowerCase()) {
            // load form update
            $.LoadingOverlay("show");

            var waste = {
                ProductList: []
            }
            var wasteItem = {
                Id: itemId,
                CountUnit: { Id: $("#select-" + itemId).val() },
                WasteCountValue: $("#waste-" + itemId).val(),
                Notes: $("#note-" + itemId).val()
            };
            waste.ProductList.push(wasteItem);


            $.ajax({
                type: 'post',
                url: '/TraderWasteReport/UpdateWasteReportItems',
                data: { wasteReport: waste },
                dataType: 'json',
                success: function (response) {
                    LoadingOverlayEnd();
                    if (response.actionVal === 2) {
                        cleanBookNotification.updateSuccess();
                    }
                    else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });

        } else if (res.result && res.Object.toLocaleLowerCase() != statusold.toLocaleLowerCase()) {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");

        }
    });




};

function UpdateWasteItems(id) {
    UpdateItems(id);
};  