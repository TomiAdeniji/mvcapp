var disable = ""; 
function UpdateItems(itemId) {
    var wasteId = $('#trader-waste-id').val();
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
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } 
    });
     
};

function UpdateWasteItems(id) { 
    UpdateItems(id);
};  