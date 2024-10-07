//----------------Show child tabs Content----------------
ShowTabCashManagement = function (tabName) {
    var tabCM = getTabTrader().TraderTab;

    $('a[href="#' + tabName + '"]').tab('show');
    setTabLevel(3, tabName);

    var ajaxUri = '/PointOfSale/PointOfSaleContent?value=' + tabName;
    if (tabName === "subGeneral") {
        ajaxUri = '/CashManagement/CMGeneralContent';
    } else if (tabName === "subDevices") {
        ajaxUri = '/CashManagement/CMDevicesContent?isSafe=true&&isTill=true';
    }
    $('#cm-content').LoadingOverlay('show');
    $('#cm-content').empty();
    $('#cm-content').load(ajaxUri, function () {
        $('#cm-content').LoadingOverlay('hide');
    });
    $('#cm-content').LoadingOverlay('hide');
}

FilterCMDeviceTab = function () {
    LoadingOverlay();
    var showType = $("#show-type").val();
    var isSafe = true;
    var isTill = true;
    if (showType == 1) {
        isTill = false;
    } else if (showType == 2) {
        isSafe = false;
    }
    var key = $("#key-search").val();

    ajaxUri = '/CashManagement/CMDevicesContent?isSafe=' + isSafe + '&&isTill=' + isTill + '&&key=' + key;
    $('#cm-content').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
    LoadingOverlayEnd();
}

//----------------Add Edit Safe----------------
SafeAddEdit = function (id) {
    var ajaxUri = "/CashManagement/CashMangementSafeAddEdit";
    AjaxElementShowModal(ajaxUri, 'app-trader-pos-cash-safe-edit');
}

function SaveSafe() {
    //Validate form 
    if (!$("#form_safe_add").valid())
        return;

    var safe = {
        Id: $("#cashmanagement_safe-id").val(),
        Name: $("#safe-name").val()
    };
    var bankAccountId = $("#safe-bank-account").val();
    if (bankAccountId.toLowerCase() === "please select") {
        $("#select2-safe-bank-account-container").parent()
            .append("<label id='PosDevices-error' class='error' for='PosDevices'>No Cash and Bank Account selected.</label>")
            .attr("onclick", "(function(){$('#PosDevices-error').remove()})()");
        //cleanBookNotification.error("No Cash and Bank Account selected.", "Qicles");
        return;
    }

    $.LoadingOverlay("show");
    _url = '/CashManagement/SaveSafe?traderCashAccountId=' + bankAccountId;

    var _isReloadPageNeeded = $("#isReloadPageNeeded").val();

    $.ajax({
        type: 'post',
        url: _url,
        data: { safe: safe },
        dataType: 'json',
        success: function (response) {
            if (response.result == true) {
                cleanBookNotification.success("Save Safe successfully.", "Qbicles");
                if (_isReloadPageNeeded === "false") {
                    FilterCMDeviceTab();
                } else {
                    location.reload();
                };
                $('#app-trader-pos-cash-safe-edit').modal('toggle');
            } else {
                $("#form_safe_add").validate().showErrors({ Name: response.msg });
                if (_isReloadPageNeeded === "false") {
                    FilterCMDeviceTab();
                } else {
                    location.reload();
                };
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qicles");
            if (_isReloadPageNeeded === "false") {
                FilterCMDeviceTab();
            } else {
                location.reload();
            }
        }
    })
}

//----------------Add Edit Till----------------
TillAddEdit = function (id) {
    var ajaxUri = "/CashManagement/CashManagementTillAddEdit?tillId=" + id;
    var elementId = 'app-trader-pos-cash-device-add';

    $.LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).load(ajaxUri, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: true,
            selectAllJustVisible: true,
            includeResetOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true,
            enableFiltering: true,
            enableCaseInsensitiveFiltering: true
        })

        LoadingOverlayEnd();
        $("#" + elementId).modal('show');
    });
}

TillDelete = function (id) {
    var _url = "/CashManagement/DeleteTill?tillId=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: _url,
        dataType: 'json',
        success: function (response) {
            if (response.result == true) {
                cleanBookNotification.removeSuccess();
                FilterCMDeviceTab()
            } else {
                cleanBookNotification.error("Delete Till failed.", "Qbicles");
                FilterCMDeviceTab()
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            FilterCMDeviceTab();
        }
    })
}

function SaveTill() {
    //Validate form 
    if (!$("#form_till_add").valid())
        return;

    var till = {
        Id: $("#till-id").val(),
        Name: $("#till-name").val()
    };
    if ($("#PosDevices").val() == null) {
        $(".multiselect-native-select .btn-group")
            .append("<label id='PosDevices-error' class='error' for='PosDevices'>No POS Devies selected.</label>")
            .attr("onclick", "(function(){$('#PosDevices-error').remove()})()");
        //cleanBookNotification.error("No POS Devies selected.", "Qbicles");
        return;
    }

    var posDevicesString = $("#PosDevices").val().join();

    $.LoadingOverlay("show");
    _url = '/CashManagement/SaveTill';

    var _isReloadPageNeeded = $("#isReloadPageNeeded").val();

    $.ajax({
        type: 'post',
        url: _url,
        data: { till: till, listPosDeviceString: posDevicesString },
        dataType: 'json',
        success: function (response) {
            if (response.result == true) {
                cleanBookNotification.success("Save Till Successfully.", "Qbicles");
                if (_isReloadPageNeeded === "false") {
                    FilterCMDeviceTab();
                } else {
                    location.reload();
                };
                $('#app-trader-pos-cash-device-add').modal('toggle');
            } else {
                //cleanBookNotification.error(response.msg, "Qbicles");
                $("#form_till_add").validate().showErrors({ Name: response.msg });
                if (_isReloadPageNeeded === "false") {
                    FilterCMDeviceTab();
                } else {
                    location.reload();
                };
            };
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            if (_isReloadPageNeeded === "false") {
                FilterCMDeviceTab();
            } else {
                location.reload();
            };
        }
    })
}

//----------------Add Edit Till Payment----------------
function SaveTillPayInReview() {
    SaveTillPayment("PendingReview", "PayIn");
}

function SaveTillPayOutReview() {
    SaveTillPayment("PendingReview", "PayOut");
}

function SaveTillPayment(status, type) {


    $.LoadingOverlay("show");
    var $workgroup = $("#payment-workgroup-select").val();

    var $associatedTill = $("#associated-till-id").val();

    var $associatedSafe = $("#associated-safe-id").val();

    var workGroup = {
        Id: $workgroup
    }

    var associatedTill = {
        Id: $associatedTill
    }

    var associatedSafe = {
        Id: $associatedSafe
    }

    var tillPayment = {
        AssociatedTill: associatedTill,
        AssociatedSafe: associatedSafe,
        WorkGroup: workGroup,
        Amount: $("#amount-payment").val()
    }

    var _url = "/CashManagement/SaveTillPayment?type=payin";
    if (type === "PayOut") {
        _url = "/CashManagement/SaveTillPayment?type=payout";
    }

    $.ajax({
        type: "post",
        url: _url,
        dataType: "json",
        data: { tillPayment: tillPayment },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Till Payment Successfully.", "Qbicles");
                callBackDataTableReload('till-transaction-list-table');
                //ShowTillDetailTable($associatedTill);
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

}

//----------------Add Till/Safe Checkpoint----------------
SaveTillCheckpoint = function (tillId) {
    $.LoadingOverlay("show");
    var associatedTill = {
        Id: tillId
    };

    var associatedWorkgroup = {
        Id: $("#workgroup-select").val()
    };

    var tillCheckpoint = {
        WorkGroup: associatedWorkgroup,
        VirtualTill: associatedTill,
        Amount: $("#till-checkpoint-amount").val(),
        CheckpointDate: $("#checkpoint-date").val()
    }

    var _url = "/CashManagement/SaveCheckpoint";

    $.ajax({
        type: "post",
        url: _url,
        dataType: "json",
        data: { Checkpoint: tillCheckpoint },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Till Checkpoint Successfully.", "Qbicles");
                //ShowTillDetailTable(tillId);
                callBackDataTableReload('till-transaction-list-table');
            } else {
                cleanBookNotification.error(response.msg, "Create Till Checkpoint failed.");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

SaveSafeCheckpoint = function (safeId) {
    $.LoadingOverlay("show");
    var associatedSafe = {
        Id: safeId
    };

    var associatedWorkgroup = {
        Id: $("#workgroup-select").val()
    };

    var safeCheckpoint = {
        WorkGroup: associatedWorkgroup,
        VirtualSafe: associatedSafe,
        Amount: $("#safe-checkpoint-amount").val(),
        CheckpointDate: $("#checkpoint-date").val()
    };

    var _url = "/CashManagement/SaveCheckpoint";

    $.ajax({
        type: "post",
        url: _url,
        dataType: "json",
        data: { Checkpoint: safeCheckpoint },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Safe Checkpoint Successfully.", "Qbicles");
                ShowSafeDetailTable(safeId);
            } else {
                cleanBookNotification.error(response.msg, "Create Safe Checkpoint failed.");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Create Safe Checkpoint failed.");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


//----------------TillDetail Page's Tabs----------------
function ShowTillDetailTable(tillId) {
    $.LoadingOverlay("show");
    var keySearch = $("#search_dt").val();
    var dateRange = "";
    if ($("#till-transaction-input-datetimerange") != null) {
        dateRange = $("#till-transaction-input-datetimerange").val();
    }
    var filter = {
        Key: keySearch,
        DateRange: dateRange
    };
    //
    //$('#till-transaction-list-table').LoadingOverlay("show");
    //$('#till-left-panel').empty();
    //$("#till-detail-table-content").empty();
    $("#till-detail-table-content").load('/CashManagement/TillDetailTable?tillId=' + tillId, filter, function () {
        //till-left-panel
        var urlTill = "/CashManagement/TillLeftPanelPartial?tillId=" + tillId + "&unapproved=''";

        $('#till-left-panel').load(urlTill, function () {
            $.LoadingOverlay("hide");
            //$('#' + elementId).LoadingOverlay("hide");
        });
        //AjaxElementLoad(urlTill, 'till-left-panel');
        
    });
}

function ShowCashManagementDiscussion() {

}

//----------------SafeDetail Page's Tabs----------------
function ShowSafeDetailTable(safeId) {
    $.LoadingOverlay("show");
    var keySearch = $("#search_dt").val();
    var dateRange = "";
    if ($("#safe-transaction-input-datetimerange") != null) {
        dateRange = $("#safe-transaction-input-datetimerange").val();
    }

    var filter = {
        Key: keySearch,
        DateRange: dateRange
    };

    $("#safe-detail-table-content").empty();
    $("#safe-detail-table-content").load('/CashManagement/SafeDetailTable?safeId=' + safeId, filter, function () {

    });
    $.LoadingOverlay("hide");
}

function callBackDataTableReload(elementId) {
    $("#" + elementId).DataTable().ajax.reload();
}

//----------------Common functions----------------
$(document).on("blur", "#amount-payment", function () {
    SetDisabledSubmit();
});

function ChangeWorkGroup() {
    SetDisabledSubmit();
    $workgroupId = $("#payment-workgroup-select").val();
    if ($workgroupId !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                $('.preview-workgroup').show();
                LoadingOverlayEnd();
                if (response.result) {
                    $(".preview-workgroup table tr td.location_name").text(response.Object.Location);
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member span").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.location_name").text('');
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member span").text('');
                }
                $('#transfer-workgroup-select-error').remove();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        LoadingOverlayEnd();
        $('.preview-workgroup').attr('style', 'display: none;');
    }
}

function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
}

function SetDisabledSubmit() {
    var $workGroup = $("#payment-workgroup-select").val();

    var $paymentAmount = _.toNumber($("#amount-payment").val());
    var $lastBalance = _.toNumber($("#till-last-balance").val());

    var isWorkGroupChosen = false;
    var isAmountPaymentFilled = false;

    if ($workGroup !== "") {
        isWorkGroupChosen = true;
    }
    $("#till-last-balance-error").hide();
    if ($paymentAmount > 0) {
        isAmountPaymentFilled = true;
    }

    if ($("#pay-type").val() === 'payout') {
        if ($paymentAmount > $lastBalance) {
            $("#till-last-balance-error").show();
            isAmountPaymentFilled = false;
        }
    }


    if (isWorkGroupChosen && isAmountPaymentFilled) {
        $("#tillpayment-submit-btn").removeAttr("disabled");
    } else {
        $("#tillpayment-submit-btn").attr("disabled", "true");
    }
}


// Search on Table Till Detail
function searchOnTableTillDetail() {
    callBackDataTableReload('till-transaction-list-table');
    //var listKey = [];

    //var keys = $('#search_dt').val();
    //if ($('#search_dt').val() !== "" && $('#search_dt').val() !== null && keys && keys.length > 0) {
    //    keys = keys.split(' ');
    //    for (var i = 0; i < keys.length; i++) {
    //        if (keys[i] !== "") listKey.push(keys[i]);
    //    }
    //}
    //$("#till-transaction-list-table").DataTable().search(listKey.join("|"), true, false, true).draw();
}

function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTableTillDetail(); }, 200);
}

// Search on Table Safe Detail
function searchOnTableSafeDetail() {
    var listKey = [];

    var keys = $('#search_dt').val();
    if ($('#search_dt').val() !== "" && $('#search_dt').val() !== null && keys && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#safe-transaction-list-table").DataTable().search(listKey.join("|"), true, false, true).draw();
}

function onKeySearchChangedSafeDetail(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTableSafeDetail(); }, 200);
}