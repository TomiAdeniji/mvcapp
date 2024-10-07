var traderGroups = [];




function loadBudgetGroupContents() {
    $('#budget-groups').LoadingOverlay("show");
    $('#budget-groups').empty();
    $('#budget-groups').load('/TraderBudget/BudgetGroupContent', function () {
        $('#budget-groups').LoadingOverlay("hide");
    });
}

function searchBudgetGroup() {
    var listKey = [];
    var keys = $('#budget-group-search').val();
    if ($('#budget-group-search').val() !== "" && $('#budget-group-search').val() !== null && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    var orderValue = $('#bgroup_order_filter').val();
    var dataTableBudgetGroup = $("#budget-table").DataTable();
    dataTableBudgetGroup.search(listKey.join("|"), true, false, true);
    if (orderValue === "0") {
        //Name A-Z
        dataTableBudgetGroup.order([1, 'asc']);
    } else if (orderValue === "1") {
        //Name Z-A
        dataTableBudgetGroup.order([1, 'desc']);
    } if (orderValue === "2") {
        //CreateDate A-Z
        dataTableBudgetGroup.order([0, 'asc']);
    } if (orderValue === "3") {
        //CreateDate Z-A
        dataTableBudgetGroup.order([0, 'desc']);
    }
    dataTableBudgetGroup.draw();
}
function searchTableTraderItem() {
    var listKey = [];
    if ($('#search_traderItem_byGroup').val() !== "" && $('#search_traderItem_byGroup').val() !== null) {
        listKey.push($('#search_traderItem_byGroup').val());
    }
    var keys = $('#search_traderitem').val();
    if ($('#search_traderitem').val() !== "" && $('#search_traderitem').val() !== null && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#list-traderitem").DataTable().search(listKey.join("|"), true, false, true).draw();
}

function loadDataTableBudgetGroup() {
    $('#showdata-budgetgroup').LoadingOverlay("show");
    $('#showdata-budgetgroup').empty();
    $('#showdata-budgetgroup').load('/TraderBudget/BudgetGroupItems', function () {
        $('#showdata-budgetgroup').LoadingOverlay("hide");
    });
}
function loadDialogItemProductBudgetGroup(id) {
    $('#app-trader-budget-group-items').LoadingOverlay("show");
    $('#app-trader-budget-group-items').empty();
    $('#app-trader-budget-group-items').load('/TraderBudget/BudgetGroupItemProductsDialog?idBudget=' + id, function () {
        $('#app-trader-budget-group-items').LoadingOverlay("hide");
    });
}

function addEditBudgetGroup(id) {
    traderGroups = [];
    $('#app-trader-budget-group-add').LoadingOverlay("show");
    $('#app-trader-budget-group-add').empty();
    $('#app-trader-budget-group-add').load('/TraderBudget/BudgetGroupAddEditModal?id=' + id, function () {
        $('#app-trader-budget-group-add').LoadingOverlay("hide");
    });
};
function onBudgetTypeChange(ev) {
    if ($(ev).val() === 'Expenditure') {
        $('#budget_productgroup_Ex').removeClass('hidden');
        $('#budget_productgroup_Re').addClass('hidden');
    } else {
        $('#budget_productgroup_Re').removeClass('hidden');
        $('#budget_productgroup_Ex').addClass('hidden');
    }
}
function addEditNextTab() {
    if (!validateFormAddBudget()) {
        return false;
    } else {
        $('.btnNext').click()
    }

    destroyTableById('list-traderitem');
    var lstTraderGroupsId = [];
    var type = $('#budget_type').val();
    if (type === 'Expenditure') {
        lstTraderGroupsId = $('#budget_productgroup_Ex select').val();
    } else {
        lstTraderGroupsId = $('#budget_productgroup_Re select').val();
    }
    var tbody = $('#list-traderitem tbody');
    var lstGroups = [];
    tbody.empty();
    var html = '';
    if (lstTraderGroupsId && lstTraderGroupsId.length > 0) {
        for (var i = 0; i < traderGroups.length; i++) {
            var group = traderGroups[i];
            if (lstTraderGroupsId.indexOf(group.Id.toString()) != -1 && group.Items.length > 0) {
                for (var j = 0; j < group.Items.length; j++) {
                    var itemTrader = group.Items[j];
                    if (lstGroups.indexOf(itemTrader.GroupName) === -1) {
                        lstGroups.push(itemTrader.GroupName);
                    }
                    if (type === 'Expenditure' && itemTrader.IsBought) {
                        html += '<tr><td>';
                        html += '<div class="table-avatar min" style="background-image: url(\'' + itemTrader.ImageUri + '\');"></div></td>';
                        html += '<td>' + itemTrader.Name + '</td>';
                        html += '<td>' + itemTrader.SKU + '</td>';
                        html += '<td>' + itemTrader.GroupName + '</td></tr >';
                    } else if (type !== 'Expenditure' && itemTrader.IsSold) {
                        html += '<tr><td>';
                        html += '<div class="table-avatar min" style="background-image: url(\'' + itemTrader.ImageUri + '\');"></div></td>';
                        html += '<td>' + itemTrader.Name + '</td>';
                        html += '<td>' + itemTrader.SKU + '</td>';
                        html += '<td>' + itemTrader.GroupName + '</td></tr >';
                    }
                }
            }
        }
    }
    
    tbody.append(html);
    searchTableTraderItem();

    // set filter group
    if (lstGroups.length > 0) {
        var searchBygroup = $("#search_traderItem_byGroup");
        searchBygroup.select2('destroy');
        searchBygroup.empty();
        var htmlGroup = "<option value=\"\">Show all</option>";
        for (var i = 0; i < lstGroups.length; i++) {
            htmlGroup += "<option value=\"" + lstGroups[i] + "\">" + lstGroups[i] + "</option>";
        }
        searchBygroup.append(htmlGroup);
        searchBygroup.select2();
    }
}

function validateFormAddBudget() {
    var valid = true;
    // check title
    if ($('#budget_title').val() === '') {
        $("#form_addedit_budget").validate().showErrors({ budget_title: "Title is required." });
        valid = false;
    }
    // check payments
    if (!$('#budget_payment').val() || ($('#budget_payment').val() && $('#budget_payment').val() === '')) {
        $("#form_addedit_budget").validate().showErrors({ terms: "Payment is required." });
        valid = false;
    }
    // check Type
    if (!$('#budget_type').val() || ($('#budget_type').val() && $('#budget_type').val() === '')) {
        $("#form_addedit_budget").validate().showErrors({ budget_type: "Type is required." });
        valid = false;
    }
    return valid;
}


function saveBudget() {
    var budgetGroup = {
        Id: $('#budget_id').val(),
        Title: $('#budget_title').val(),
        Description: $('#budget_description').val(),
        ExpenditureGroups: [],
        RevenueGroups: [],
        Type: $('#budget_type').val(),
        PaymentTerms: { Id: $('#budget_payment').val() },

    }
    var lstTraderGroupsId = [];
    var type = $('#budget_type').val();
    if (type === 'Expenditure') {
        lstTraderGroupsId = $('#budget_productgroup_Ex select').val();
        if (lstTraderGroupsId && lstTraderGroupsId.length > 0) {
            for (var i = 0; i < lstTraderGroupsId.length; i++) {
                budgetGroup.ExpenditureGroups.push({ Id: lstTraderGroupsId[i] });
            }
        }
    } else {
        lstTraderGroupsId = $('#budget_productgroup_Re select').val();
        if (lstTraderGroupsId && lstTraderGroupsId.length > 0) {
            for (var i = 0; i < lstTraderGroupsId.length; i++) {
                budgetGroup.RevenueGroups.push({ Id: lstTraderGroupsId[i] });
            }
        }
    }
    
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderBudget/SaveBudgetGroup',
        data: { budgetGroup: budgetGroup },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            var idmaster = $('#budget_master_id').val();
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-budget-group-add').modal('toggle');
                setTimeout(function () {
                    if (idmaster)
                        loadMasterPageContent();
                    else
                        loadDataTableBudgetGroup();
                }, 500);
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-budget-group-add').modal('toggle');
                setTimeout(function () {
                    if (idmaster)
                        loadMasterPageContent();
                    else
                        loadDataTableBudgetGroup();
                }, 500);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function deleteBudgetGroup(id) {
    var result = confirm('Do you want to delete this budget group?');
    if (result == false) return false;
    $.ajax({
        type: 'delete',
        url: '/TraderBudget/DeleteBudgetGroup?id=' + id,
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                setTimeout(function () {
                    loadDataTableBudgetGroup();
                }, 500);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

// for master page

function loadMasterPageContent() {
    var id = $('#budget_master_id').val();
    var url = "/TraderBudget/BudgetGroupMasterContent?id=" + id;
    $('#content_master').LoadingOverlay("show");
    $('#content_master').empty();
    $('#content_master').load(url, function () {
        $('#content_master').LoadingOverlay("hide");
    });
}
function masterSearchTableTraderItem() {
    var listKey = [];
    if ($('#search_traderItem_byGroup_master').val() !== "" && $('#search_traderItem_byGroup_master').val() !== null) {
        listKey.push($('#search_traderItem_byGroup_master').val());
    }
    var keys = $('#search_traderitem_master').val();
    if ($('#search_traderitem_master').val() !== "" && $('#search_traderitem_master').val() !== null && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#master-list-traderitem").DataTable().search(listKey.join("|"), true, false, true).draw();
}

