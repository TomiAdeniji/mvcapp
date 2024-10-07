//----------------Show child tabs, Modals Content----------------
function ShowTabShiftManagement (tabName) {
    var SMTab = getTabTrader().TraderTab;
    $('a[href="#' + tabName + '"]').tab('show');
    setTabTrader(SMTab, tabName);

    var ajaxUri = '/PointOfSale/PointOfSaleContent?value=' + tabName;
    if (tabName === "subCM") {
        ajaxUri = '/ShiftManagement/SMCashManagementTabContent?isSafe=true&&isTill=true';
    } else if (tabName === "subVoidReturn") {
        ajaxUri = '/CashManagement/SMVoidReturnTabContent';
    }
    $('#shift-cash').LoadingOverlay('show');
    $('#shift-cash').empty();
    $('#shift-cash').load(ajaxUri, function () {
        $('#shift-cash').LoadingOverlay('hide');
    });
    $('#shift-cash').LoadingOverlay('hide');
}

function FilterSMCashManagementTab() {
    $('#shift-cash').LoadingOverlay('show');
    var showType = $("#sm-cash-management_show-type").val();
    var isSafe = true;
    var isTill = true;
    if (showType == 1) {
        isTill = false;
    } else if (showType == 2) {
        isSafe = false;
    }
    var key = $("#sm-cash-management_key-search").val();

    ajaxUri = '/ShiftManagement/SMCashManagementTabContent?isSafe=' + isSafe + '&&isTill=' + isTill + '&&key=' + key;
    $('#shift-cash').empty();
    $('#shift-cash').load(ajaxUri, function () {
        $('#shift-cash').LoadingOverlay('hide');
    });
    $("#sm-cash-management_show-type").focus();
    $('#shift-cash').LoadingOverlay('hide');
}

function AddTillPayIn(tillId) {
    var _url = "/ShiftManagement/SMTillPaymentTabContent?tillId=" + tillId + "&&type=payin";
    var modalElementId = "app-trader-sm-till-pay-in-out";
    AjaxElementShowModal(_url, modalElementId);
}

function AddTillPayOut(tillId) {
    var _url = "/ShiftManagement/SMTillPaymentTabContent?tillId=" + tillId + "&&type=payout";
    var modalElementId = "app-trader-sm-till-pay-in-out";
    AjaxElementShowModal(_url, modalElementId);
}

function AddTillCheckpointTabContent(tillId) {
    var _url = "/ShiftManagement/TillCheckpointAddTabContent?tillId=" + tillId;
    var modalElementId = "app-trader-pos-cash-checkpoint-add";
    AjaxElementShowModal(_url, modalElementId);
}

function AddSafeCheckpointTabContent(safeId) {
    var _url = "/ShiftManagement/SafeCheckpointAddTabContent?safeId=" + safeId;
    var modalElementId = "app-trader-pos-safe-checkpoint-add";
    AjaxElementShowModal(_url, modalElementId);
}

