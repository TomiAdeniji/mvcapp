
var $traderSpotCountId = $("#trader-spot-count-id").val();
var disable = "";
function UpdateItems(notify) {
    $.LoadingOverlay("show");
    CheckStatus($traderSpotCountId, 'SportCount').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object != "StockAdjusted") {
            // load form update
            $.LoadingOverlay("show");

            var productList = [];
            var items = $('#tb_form_item tbody tr');            
            if (items.length > 0) {
                for (var i = 0; i < items.length; i++) {
                    var id = parseInt($($(items[i]).find('td.row_id input')).val());
                    var unit = $($(items[i]).find('td.row_unit select')).val().split('|');
                    var spotCountItem = {
                        Id: isNaN(id) ? 0 : id,
                        CountUnit: { Id: unit[1] },
                        Adjustment: parseFloat($($(items[i]).find('td.row_adjustment input')).val()),
                        SpotCountValue: parseFloat($($(items[i]).find('td.row_spotcountvalue input')).val()),
                        SavedInventoryCount: parseFloat($($(items[i]).find('td.row_savedinventorycount input')).val()),
                        Notes: $($(items[i]).find('td.row_spotcountnote input')).val()
                    }
                    productList.push(spotCountItem);
                }
            }
            var spotCount = {
                Id: $traderSpotCountId,
                ProductList: productList
            }

            $.ajax({
                type: 'post',
                url: '/TraderSpotCount/UpdateSportCountItems',
                data: { spotCount: spotCount },
                dataType: 'json',
                success: function (response) {
                    LoadingOverlayEnd();
                    if (response.actionVal === 2) {
                        if(notify)
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

        } else if (res.result && res.Object == "StockAdjusted") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");

        }
        LoadingOverlayEnd();
    });


};

function SelectedChangeBu(id) {
    if (disable === "")
        UpdateItems(true);  
};  