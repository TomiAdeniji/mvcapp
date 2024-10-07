
$(function () {
    var type = $("#group-type").val();

    if (type === "ItemsIBuy") {
        $('.sale-col-qty').hide();
        $('.sale-col-price').hide();
        $('.purchase-col-qty').show();
        $('.purchase-col-cost').show();
    }
    else if (type === "ItemsISell") {
        $('.purchase-col-qty').hide();
        $('.purchase-col-cost').hide();
        $('.sale-col-qty').show();
        $('.sale-col-price').show();
    }
    else {
        $('.sale-col-qty').show();
        $('.sale-col-price').show();
        $('.purchase-col-qty').show();
        $('.purchase-col-cost').show();
    }
});


//Add edit budget
var $workgroupId = 0;
var $budgetScenarioItemGroupId = 0;
var $budgetScenarioId = 0;
BudgetAddEditItem = function (id, budgetScenarioId, oView) {
    //$.LoadingOverlay("show");

    $budgetScenarioItemGroupId = id;
    $budgetScenarioId = budgetScenarioId;
    var ajaxUri = '/TraderBudget/BudgetAddEditItem?budgetScenarioItemGroupId=' + id + '&budgetScenarioId=' + $budgetScenarioId + '&oView=' + oView;
    AjaxElementShowModal(ajaxUri, "app-trader-budget-item-add-edit");

};


//Budget scenario items
function BudgetGroupTypeSelected(type, budgetScenarioId) {

    $budgetScenarioItemGroupId = 0;
    $budgetScenarioId = budgetScenarioId;

    DisplayBudgetItemInput(type);
    $('.unitextra').hide();

    $("#budget-item-table > tbody").html("");

    $("#dynamic-budget-add-item").show();
    $('.dynamic-budget-item-table').hide();
    $('#budget-ready').hide();
    $("#budget-draft").hide();
};

function DisplayBudgetItemInput(type) {
    var ajaxUri = '/TraderBudget/BudgetAddEditChoseItem?budgetScenarioItemGroupId=' + $budgetScenarioItemGroupId + '&itemGroupType=' + type + '&budgetScenarioId=' + $budgetScenarioId;
    AjaxElementLoad(ajaxUri, "dynamic-budget-add-item");
}

var itemSelect = {
    Id: "",
    Name: "",
    Image: "",
    Unit: ""
};
function ItemSelected() {
    var selected = $('#item-select').find(':selected');
    if (selected.val() === "") {
        $('.unitextra').hide();
        itemSelect.Id = "";
        itemSelect.Name = "";
        itemSelect.Image = "";
        itemSelect.Unit = "";
        return;
    }
    itemSelect.Id = selected.attr("itemId");
    itemSelect.Name = selected.attr("itemName");
    itemSelect.Image = selected.attr("itemImage");
    itemSelect.Unit = selected.attr("itemUnit");

    $("#item-unit-name").text(itemSelect.Unit);
    $('.unitextra').show();//$('.unitextra').prop('disabled', false);
};

ValuePurchaseOnChange = function () {
    var qty = $("#item-purchase-quantity").val();
    var cost = $("#item-purchase-average-cost").val();
    $("#item-purchase-amount").val(qty * cost);
};


ValueSaleOnChange = function () {
    var qty = $("#item-sale-quantity").val();
    var price = $("#item-sale-average-price").val();
    $("#item-sale-amount").val(qty * price);
};


var $rowRemoveId = 0;
function RemoveRowItem() {

    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/TraderBudget/RemoveBudgetScenarioItem?budgetScenarioGroupId=' + $budgetScenarioItemGroupId + '&itemId=' + $rowRemoveId,
        //data: { budgetScenarioGroup: budgetScenarioGroup },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $('#confirm-delete').modal('hide');
                $('#budget-item-table #tr_id_' + $rowRemoveId).remove();
                $('#budget-item-table-approval #tr_id_' + $rowRemoveId).remove();
                ResetBudgetItemSelected('budget-item-table', 'item-select');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });


};

function ConfirmRemoveRowItem(id) {
    $rowRemoveId = id;
};

function CancelDelete() {
    $('#confirm-delete').modal('hide');
};

function BudgetItemAddNow(type) {
    if ($workgroupId === 0 || $workgroupId === "") {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return;
    }

    if (itemSelect.Id === 0 || itemSelect.Id === "") {
        cleanBookNotification.error(_L("ERROR_MSG_617"), "Qbicles");
        return;
    }
    SaveItem(type).then(function (response) {
        AddItemToTableList(response.actionVal, type);
    });
};

function PeriodBreakdownItem(scenarioItemId, type) {
    if ($workgroupId === 0 || $workgroupId === "") {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return;
    }

    if (itemSelect.Id === 0 || itemSelect.Id === "") {
        cleanBookNotification.error(_L("ERROR_MSG_617"), "Qbicles");
        return;
    }
    SaveItem(type).then(function (response) {
        //actionVal is Sneario item
        var sItemId = response.actionVal;
        if (scenarioItemId > 0)
            sItemId = scenarioItemId;
        PeriodBreakdownItemShow(type, sItemId);
    });
};

function PeriodBreakdownItemOnTable(scenarioItemId, type) {
    PeriodBreakdownItemShow(type, scenarioItemId);
};

function SaveItem(type) {
    $.LoadingOverlay("show");
    var dfd = new $.Deferred();
    var budgetScenarioItems = [
        {
            //Id: itemSelect.Id,
            Item: { Id: itemSelect.Id },
            SaleQuantity: $("#item-sale-quantity").val(),
            AverageSalePrice: $("#item-sale-average-price").val(),

            PurchaseQuantity: $("#item-purchase-quantity").val(),
            AveragePurchaseCost: $("#item-purchase-average-cost").val()
        }
    ];


    var budgetScenarioGroup = {
        Id: $budgetScenarioItemGroupId,
        BudgetScenario: {
            Id: $budgetScenarioId
        },
        WorkGroup: {
            Id: $("#budget-workgroup-select").val()
        },
        Status: 1,
        Type: type,
        BudgetScenarioItems: budgetScenarioItems
    };



    $.ajax({
        async: false,
        type: 'post',
        url: '/TraderBudget/AddBudgetScenarioItem',
        data: { budgetScenarioGroup: budgetScenarioGroup },
        dataType: 'json',
        success: function (response) {
            if (response.msgId > 0) {
                $budgetScenarioItemGroupId = response.msgId;
                dfd.resolve(response);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                dfd.resolve(response);
                return;
            }
        },
        error: function (er) {
            dfd.resolve("");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
    return dfd.promise();
};

AddItemToTableList = function (scenarioItemId, type) {

    if (type === 1) {
        $('.sale-col-qty').hide();
        $('.sale-col-price').hide();
        $('.purchase-col-qty').show();
        $('.purchase-col-cost').show();
    }
    else if (type === 2) {
        $('.purchase-col-qty').hide();
        $('.purchase-col-cost').hide();
        $('.sale-col-qty').show();
        $('.sale-col-price').show();
    }
    else {
        $('.sale-col-qty').show();
        $('.sale-col-price').show();
        $('.purchase-col-qty').show();
        $('.purchase-col-cost').show();
    }

    $('.dynamic-budget-item-table').show();
    $('#budget-ready').show();
    $("#budget-draft").show();

    var clone = $('#tb_form_template tbody tr').clone();
    $(clone).attr('id', 'tr_id_' + itemSelect.Id);
    //// filter to table 
    $($(clone).find('td.row_image div')).attr('style', "background-image: url('" + itemSelect.Image + "');");
    $($(clone).find('td.row_name')).text(itemSelect.Name);
    $($(clone).find('td.row_unit')).text(itemSelect.Unit);
    $($(clone).find('td.row_button input.traderItem')).val(itemSelect.Id);

    $($(clone).find('td.purchase-col-qty')).text(numberString($("#item-purchase-quantity").val(), ''));
    $($(clone).find('td.purchase-col-cost')).text(toCurrencySymbol($("#item-purchase-average-cost").val()));
    $($(clone).find('td.sale-col-qty')).text(numberString($("#item-sale-quantity").val(), ''));
    $($(clone).find('td.sale-col-price')).text(toCurrencySymbol($("#item-sale-average-price").val()));

    var total = 0;
    if (type === 1)
        total = $("#item-purchase-amount").val();
    else if (type === 2)
        total = $("#item-sale-amount").val();
    else {

        var pQty = $("#item-purchase-quantity").val();
        var pCost = $("#item-purchase-average-cost").val();
        var sQty = $("#item-sale-quantity").val();
        var sPrice = $("#item-sale-average-price").val();
        total = (pQty * pCost) + (sQty * sPrice);
    }
    $($(clone).find('td.row_total')).text(toCurrencySymbol(total));


    //$($(clone).find('td.row_button')).attr("disabled", true);
    $($(clone).find('td.row_button button.remove')).attr('onclick', "ConfirmRemoveRowItem('" + itemSelect.Id + "')");

    $($(clone).find('td.row_button button.period')).attr('onclick', "PeriodBreakdownItemOnTable(" + scenarioItemId + "," + type + ")");

    var productItem = $('#budget-item-table tbody tr#' + 'tr_id_' + itemSelect.Id);

    if (productItem.length === 0) {
        $('#budget-item-table tbody').append(clone);
        var rowApproval = $('#budget-item-table #tr_id_' + itemSelect.Id).clone();
        $('#budget-item-table-approval tbody').append(rowApproval);
        $('#budget-item-table-approval td.row_button').attr("hidden", true);
    }
    else {
        cleanBookNotification.error(_L("ERROR_MSG_373"), "Qbicles");
    }

    ResetBudgetItemSelected('budget-item-table', 'item-select');
    itemSelect.Id = "";
    $('#dynamic-budget-add-item input').val('0');
};

PeriodBreakdownItemShow = function (type, id) {
    var ajaxUri = '/TraderBudget/PeriodBreakdownItemManagement?budgetScenarioItemId=' + id + '&type=' + type;
    AjaxElementShowModal(ajaxUri, "app-trader-budget-item-breakdown-buysell");
};
var $quantityTotal = $("#qty-total").val();


SaveDraft = function () {
    $.LoadingOverlay("show");
    $('#app-trader-budget-item-add-edit').modal('hide');
    LoadingOverlayEnd();
};

SendToReview = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderBudget/SendToReviewBudgetScenario',
        data: { budgetScenarioGroupId: $budgetScenarioItemGroupId },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-budget-item-add-edit').modal('hide');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};




//approval update
function UpdateStatusApproval(apprKey) {

    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    CheckStatusApproval(apprKey).then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object.toLocaleLowerCase() === statusOld.toLocaleLowerCase()) {
            // apply  
            var appStatus = $("#action_approval").val();
            $.LoadingOverlay("show");
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: appStatus },
                success: function (rs) {
                    var ajaxUri = '/TraderBudget/ReloadBudgetStatusApproval?id=' + $("#budget-group-item-id").val();
                    $('#manager-approval').load(ajaxUri, function () {
                        if (appStatus !== "Pending") {
                            $("#edit-budget").hide();
                        } else {
                            $("#edit-budget").show();
                        }
                       
                        $('#status-css').removeClass().addClass('label').addClass(rs.msgName);
                        $('#status-name').text(rs.msg);
                        LoadingOverlayEnd();
                        cleanBookNotification.updateSuccess();
                    });
                },
                error: function (err) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            }).always(function () {
            });
            // and apply
        } else {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");

        }
    });
};

function HideApprovalAndMediaCommen() {
    $(".approval-action").addClass('hidden');
    $(".manager-action").removeClass('hidden');
};
function ShowApprovalAndMediaCommen() {
    $(".manager-action").addClass('hidden');
    $(".approval-action").removeClass('hidden');
};