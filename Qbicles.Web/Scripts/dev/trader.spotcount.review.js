
var $traderSpotCountId = $("#trader-spot-count-id").val();
var disable = "";

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

                },
                error: function (err) {
                    $('#spot-count-item-tbody').removeClass('disabled');
                    $('#approval-button div.loading-approval').addClass('hidden');
                    $('#approval-button div.general-approval').removeClass('hidden');
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
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
    $.LoadingOverlay("show");
    $('#sport_content').empty();
    $('#sport_content').load('/TraderSpotCount/UpdateSpotCountContent?id=' + $traderSpotCountId, function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);
    });
};

// update spot count items
var $traderSpotCountId = $("#trader-spot-count-id").val();

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
                        if (notify)
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
    });


};

function SelectedChangeBu(id) {
    if (disable === "")
        UpdateItems(true);
};


//Update description
var strValueOld = "";
var strValueChanged = "";
function OnFocusOutControl(value) {
    strValueChanged = value;
    if (strValueChanged !== strValueOld)
        UpdateDescription();
};

function OnFocusControl(value) {
    strValueOld = value;
};

function UpdateDescription() {
    $.LoadingOverlay("show");

    var spotCount = {
        Id: $traderSpotCountId,
        Description: strValueChanged
    }
    $.ajax({
        type: 'post',
        url: '/TraderSpotCount/UpdateDescription',
        data: { spotCount: spotCount },
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
};