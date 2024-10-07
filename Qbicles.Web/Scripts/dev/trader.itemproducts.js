var tabItemproduct = "";
var apiDoc = $('#api-uri').val();
function initProductTab() {
    tabItemproduct = getTabTrader().TraderTab;
    activeTab = getTabTrader().SubTraderTab;

    if (activeTab === "pricebook-tab") {
        SelectPriceBookTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "items-import") {
        SelectItemsImportTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "adjuststock-tab") {
        SelectAdjustStockTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "inventory-tab") {
        selectInventoryTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "resource-tab") {
        selectResourceTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "waste-report-tab") {
        $('a[href="#adjuststock-tab"]').tab('show');
        SelectAdjustStockWasteTab();
    } else if (activeTab === "inventoryaudit-tab") {
        SelectInventoryAuditTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "movement-tab") {
        SelectMovementTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    }
    else {
        isChangeItem = true;
        selectTraderItemsTab();
    }
}
var curentTab = 'item-tab';
var isChangeItem = false;
// Filter
var filter = {
    Item: {
        Key: ""
    },
    PriceBook: {
        Product: ' ',
        Key: ''
    }
}
// filter Item
function setDefauleInputSearchItem() {
    if (filter.Item.Key)
        $('#search_dt').val(filter.Item.Key);
}
function onItemKeySearchChanged(ev) {
    filter.Item.Key = $(ev).val();
    setTimeout(function () { searchOnTableItems(); }, 200);
}
function searchOnTableItems() {
    var listKey = [];

    var keys = $('#search_dt').val().split(' ');
    if ($('#search_dt').val() !== "" && $('#search_dt').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#tb_trader_items").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#tb_trader_items").val("");
}
// End filter item

// filter price book
function setDefauleInputSearchPriceB() {
    $('#subfilter-group-price').val(filter.PriceBook.Product);
    $('#search_priceb').val(filter.PriceBook.Key);
}
function onPriceBKeySearchChanged(ev) {
    filter.PriceBook.Key = $(ev).val();
    setTimeout(function () { searchOnTablePriceBs(); }, 200);
}
function onSelectWorkgroupPriceB(ev) {
    filter.PriceBook.Product = $(ev).val();
    setTimeout(function () { searchOnTablePriceBs(); }, 200);
}
function searchOnTablePriceBs() {
    var listKey = [];
    if ($('#subfilter-group-price').val() !== " " && $('#subfilter-group-price').val() !== "" && $('#subfilter-group-price').val() !== null) {
        listKey.push($('#ssubfilter-group-price').val());
    }

    var keys = $('#search_priceb').val().split(' ');
    if ($('#search_priceb').val() !== "" && $('#search_priceb').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#tb_pricebooks").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#tb_pricebooks").val("");
}

//End filter price book

function setType(t, tab) {
    LoadingOverlay();
    curentTab = tab;
    if ($('#local-manage-select').val() === null || $('#local-manage-select').val() === "" || $('#local-manage-select').val() === "0") {
        cleanBookNotification.error(_L("ERROR_MSG_622"), "Qbicles");
        return false;
    }
    var location = $('#local-manage-select').val();
    $('#app-trader-inventory-item-add').empty();
    $('#app-trader-inventory-item-add').load('/Trader/AddTraderItem?locationId=' + (location ? location : 0) + '&type=' + t + '&callback=true');
    $('#app-trader-inventory-item-add').modal('toggle');
    LoadingOverlayEnd();
};
function editTraderItem(type, traderId, tab) {
    LoadingOverlay();
    curentTab = tab;
    $('#wizard-add-item #theform').empty();//Fix conflict js with business profile
    var location = $('#local-manage-select').val();
    if ($('#form_inventory #local-manage-select').length > 0)
        location = 0;
    $('#app-trader-inventory-item-add').empty();
    $('#app-trader-inventory-item-add').load('/Trader/AddTraderItem?locationId=' + (location ? location : 0) + '&traderItemId=' + traderId + '&type=' + type + '&callback=true');
    $('#app-trader-inventory-item-add').modal('toggle');
    LoadingOverlayEnd();
}
var selectedNode = null;
var lstParent = [];
var lstUnits = [];
var idAccount = "";
var recipes = [];
var int = 99999999;

function LoadAccountsTree(linkedId) {
    LoadingOverlay();
    if (linkedId == null)
        linkedId = 0;
    $("#app-bookkeeping-treeview").html("");
    $("#app-bookkeeping-treeview").load("/Bookkeeping/TreeViewAccountByNodeIdPartial?id=0&number=0&linkedId=" + linkedId, function () {
        initJsTree();
        $('.jstree').on("select_node.jstree", function (evt, data) {
            $('#content').html("<div class='text-center' style='margin-top: 50px;'><img src='/Content/DesignStyle/img/loading.gif' class='loader'></div>");
            var type = data.node.data.node;
            var value = data.node.data.value;
            selectedNode = data.node.data;
            if (typeof (selectedNode.parent) === "number") {
                lstParent.push(selectedNode.parent);
            } else if (typeof (selectedNode.parent) === "string") {
                lstParent = selectedNode.parent.split(',');
            }

            if ($(document).width() < 978) {
                $('html, body').animate({
                    scrollTop: $('#content').offset().top - 120
                }, 'slow');
            }
        });
        LoadingOverlayEnd();
    });
}
function activeLocation() {
    if ($('#form_specifics_active_in_all').html() === 'Applied') {
        changelocationAll();
    }
    $('.bookkeeping-connected').toggle(); $('.no-bookkeeping').toggle();
}
function getUnitsOnServer(id) {
    var dfd = new $.Deferred();
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/TraderItem/GetUnitByTraderItem?itemId=' + id,
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            dfd.resolve(response);
        },
        error: function (er) {
            $.LoadingOverlay("hide");
        }
    });
    return dfd.promise();
}
function getIngredientOnServer(lstIng) {
    var dfd = new $.Deferred();
    LoadingOverlay();
    $.ajax({
        type: 'post',
        url: '/TraderItem/GetListIngredient',
        data: { lstIng: lstIng },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            dfd.resolve(response);
        },
        error: function (er) {
            $.LoadingOverlay("hide");
        }
    });
    return dfd.promise();
}

function getIngredientOnServer3(obj) {
    var res;
    $.ajax({
        type: 'post',
        url: '/TraderItem/GetListIngredient',
        data: { lstIng: obj.Ingredients, currentLocationId: $("#slLocation_items").val() },
        dataType: 'json',
        success: function (response) {
            test3(obj, response);
        },
        error: function (er) {
            $.LoadingOverlay("hide");
        }
    });
}

function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $('#' + idAccount).val(id);
    var nodeSelected = selectedNode.name;
    if (name !== null && name !== "")
        nodeSelected = name;
    if (idAccount === 'form_purchase') {
        $('#btn_addpurchase_account').addClass('hidden');
        $('#btn_editpurchase_account').removeClass('hidden');
        $('#form_purchase_account p').text(nodeSelected);
    } else if (idAccount === 'form_salesaccount') {
        $('#btn_addsales_account').addClass('hidden');
        $('#btn_editsales_account').removeClass('hidden');
        $('#form_sales_account p').text(nodeSelected);
    } else if (idAccount === 'form_inventoryaccount') {
        $('#btn_addinventory_account').addClass('hidden');
        $('#btn_editinventory_account').removeClass('hidden');
        $('#form_inventory_account p').text(nodeSelected);
    } else if (idAccount === 'form_traderitem_contact_account') {
        $('#btn_addcontact_account').addClass('hidden');
        $('#btn_editcontact_account').removeClass('hidden');
        $('#form_trader_account p').text(nodeSelected);
    }

    $('#app-bookkeeping-treeview').modal('toggle');
}
function checkBookkeepingConnected() {
    var def = new $.Deferred();
    $.ajax({
        type: 'get',
        url: '/TraderItem/CheckBookkeepingConnected',
        dataType: 'json',
        success: function (response) {
            def.resolve(response.result);
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            def.resolve(false);
        }
    });
    return def.promise();
}

var classActive = 'li_specifics';
function validFormAdd(liClass, a) {
    //if (!$(a.parentElement).hasClass('active')) {
    //    if (classActive === 'li_specifics' && liClass !== classActive) {
    //    } else if (classActive === 'li_specifics' && liClass !== classActive) {
    //    } else if (classActive === 'li_specifics' && liClass !== classActive) {
    //    } else if (classActive === 'li_specifics' && liClass !== classActive) {
    //    } else {
    //    }
    //}
}

function checknumber(e, evt) {
    var keyCode = evt.which;
    /*
      8 - (backspace)
      32 - (space)
      48-57 - (0-9)Numbers
    */
    if ((keyCode !== 8 || keyCode === 32) && (keyCode < 48 || keyCode > 57)) {
        evt.preventDefault();
    }
}

function test3(obj, res) {
    debugger;
    obj.Ingredients = res;
    var lastItemInLoop = obj.Ingredients.length - 1;
    for (var i = 0; i < obj.Ingredients.length; i++) {
        $('#template-options-select tr.new_row_recipe2 td.item_selected').empty();
        $('#template-options-select tr.new_row_recipe2 td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\"  data-placeholder=\"Choose an unit\"></select>");
        var selected = $('#template-options-select tr.new_row_recipe2 td.item_selected select');
        if (obj.Ingredients[i].Units.length > 0) {
            var multipleBaseUnit = [];
            var htmlStr = "<option value=\"\"></option> ";
            for (var k = 0; k < obj.Ingredients[i].Units.length; k++) {
                multipleBaseUnit.push(parseFloat(obj.Ingredients[i].Units.find(e => e.Id == obj.Ingredients[i].Units[k].Id).QuantityOfBaseunit));
                htmlStr += "<option value=\"" + obj.Ingredients[i].Units[k].Id + "\" quantityOfBaseunit = \"" + multipleBaseUnit[k] + "\">" + obj.Ingredients[i].Units[k].Name + "</option> ";
            }
            selected.append(htmlStr);
        } else {
            var htmlStr2 = "<option value=\"\"></option> ";
            selected.append(htmlStr2);
        }
        if (!obj.Ingredients[i].SubItem) obj.Ingredients[i].SubItem = {};
        if (obj.Ingredients[i].SubItem.InventoryDetails.length > 0) {
            if (obj.Ingredients[i].SubItem.InventoryDetails[0].AverageCost === 0) {
                obj.Ingredients[i].AverageCost = "0";
            } else obj.Ingredients[i].AverageCost = (obj.Ingredients[i].SubItem.InventoryDetails[0].AverageCost * multipleBaseUnit[0]).toString();
            if (obj.Ingredients[i].SubItem.InventoryDetails[0].LatestCost === 0) {
                obj.Ingredients[i].LatestCost = "0";
            } else obj.Ingredients[i].LatestCost = (obj.Ingredients[i].SubItem.InventoryDetails[0].LatestCost * multipleBaseUnit[0]).toString()
        }
        var clone = $('#template-options-select tbody tr.new_row_recipe2').clone();
        $(clone).addClass("gen" + i).attr("thPosition", i);
        $(clone).find(".recipe-ingredient-add-mew").addClass("yeu-em-never-sai");
        $("#tb_edit_recipe tbody").append(clone);
        var selectOption = $("#tb_edit_recipe tbody .gen" + i + "")
        if (lastItemInLoop == i) {
            initSelectOption2(obj.Ingredients[i].SubItem.Id, selectOption, true);
        } else {
            initSelectOption2(obj.Ingredients[i].SubItem.Id, selectOption, false);
        }
        var target = $('.new_row_recipe2.gen' + i + '');
        // $($(target).find('td.item_name select')).val(obj.Ingredients[i].SubItem.Id);
        $($(target).find('td.item_name input')).val(obj.Ingredients[i].Id);
        $($(target).find('td.item_quantity input')).val(obj.Ingredients[i].Quantity);
        $($(target).find(".item_averagecost")).val(obj.Ingredients[i].SubItem.InventoryDetails[0].AverageCost);
        $($(target).find(".item_latestcost")).val(obj.Ingredients[i].SubItem.InventoryDetails[0].LatestCost)
        $($(target).find('td.item_selected select')).val(obj.Ingredients[i].Unit.Id);

        var baseUnit = obj.Ingredients[i].Units.find(e => e.Id == obj.Ingredients[i].Unit.Id).QuantityOfBaseunit;
        $($(target).find(".item_averagecost span")).html(obj.Ingredients[i].AverageCost * baseUnit);
        $($(target).find(".item_latestcost span")).html(obj.Ingredients[i].LatestCost * baseUnit);
    }
    //triggle on select option
    $('#tb_edit_recipe tr.new_row_recipe2 td.item_selected select').select2().on("select2:select", function (param) {
        var step = 1;
        $.each(param.params.data.element.attributes, function (indexInArray, valueOfElement) {
            if (valueOfElement.name == "quantityofbaseunit")
                step = parseFloat(valueOfElement.value);
        });
        var averageBase = parseFloat($(param.currentTarget).parent().parent().children(".item_averagecost").val());
        var latestBase = parseFloat($(param.currentTarget).parent().parent().children(".item_latestcost").val());
        $(param.currentTarget).parent().parent().children(".item_averagecost").find("span").val(averageBase * step).html(averageBase * step);
        $(param.currentTarget).parent().parent().children(".item_latestcost").find("span").val(latestBase * step).html(latestBase * step);
    });
}

function validateSpecificsForm() {
    var valid = true;
    if ($("#form_specifics_category").val() === "") {
        valid = false;
        $("#tab_form_specifics").validate().showErrors({ specifics_category: "Category is required." });
    }
    if ($("#form_specifics_name").val() === "") {
        valid = false;
        $("#tab_form_specifics").validate().showErrors({ specifics_name: "Item name is required." });
    } else if ($("#form_specifics_name").val().length < 4 || $("#form_specifics_name").val().length > 50) {
        valid = false;
        $("#tab_form_specifics").validate().showErrors({ specifics_name: _L("ERROR_MSG_261") });
    }
    if ($("#form_specifics_description").val() === "") {
        valid = false;
        $("#tab_form_specifics").validate().showErrors({ form_specifics_description: "Item description is required." });
    }

    if ($("#form_specifics_icon").val() === "" && $("#form_specifics_icon_text").val() === "" && $('#traderItem_id').val().toString() === "0") {
        valid = false;
        if ($('#product_image').val() === "1")
            $("#tab_form_specifics").validate().showErrors({ specifics_icon: "Item image is required." });
        else
            cleanBookNotification.error(_L("ERROR_MSG_378"), "Qbicles");
    }

    return valid;
}
function validateInventoryForm(check) {
    if (!check) return true;
    var valid = true;
    if ($("#form_inventory_unitcost").val() === "" || $("#form_inventory_unitcost").val() === "0") {
        valid = false;
        $("#form_inventory").validate().showErrors({ unitcost: "Unit cost is required." });
    }
    return valid;
}
function nextStepSpecifics() {
    if (validateSpecificsForm()) {
        //$('li.li_inventory a').attr('data-toggle', 'tab');
        $('#a_next_item1').click();
        inventoryTabSelected();
    }
}
function inventoryTabSelected() {
    $('#item-2 table').removeClass('datatable');
    $('#item-2 table').addClass('datatable');
}
function validateUniqueSKUandBarcode() {
    var SKU = $('#form_inventory_SKU').val();
    var Barcode = $('#form_inventory_barcode').val();
    var validateObj = $("#frmvalidateSkuAndBarcode").validate();
    var valid = true;
    if (!SKU) {
        valid = false;
        validateObj.showErrors({ sku: "The Product SKU is required!" });
    }
    if (SKU) {
        $.ajax({
            method: "POST",
            data: { traderId: $('#traderItem_id').val(), SKU: SKU, Barcode: Barcode },
            url: "/TraderItem/validateUniqueSKUandBarcode",
            dataType: "json",
            async: false
        }).done(function (data) {
            if (data.sku) {
                valid = false;
                validateObj.showErrors({ sku: "The Product SKU already exists!" });
            }
            if (data.barcode) {
                valid = false;
                validateObj.showErrors({ barcode: "The Product Barcode already exists!" });
            }
        });
    }
    return valid;
}
function nextStepInventory() {
    if (validateUniqueSKUandBarcode()) {
        if ($('#form_inventory_checkbox')[0].checked === true) {
            if (validateInventoryForm($('#form_inventory_checkbox')[0].checked)) {
                $('#a_next_item2').click();
            }
        } else {
            $('#a_next_item2').click();
        }
    }
}

function changeName() {
    $('#form_inventory_name').text('"' + $("#form_specifics_name").val() + '"' + ' inventory specifics');
}
function getSelectedUnits() {
    var names = $('#unit_conversions td.row_name .unitName');
    var quantityOfBase = $('#unit_conversions td.row_name span');
    var ids = $('#unit_conversions td.row_name .unitIds');
    var selected = $('#td_add_units select');
    if (ids.length > 0) {
        selected.empty();
        var htmlStr = "<option value=\"\"></option> ";
        for (var i = 0; i < ids.length; i++) {
            htmlStr += "<option value=\"" + $(ids[i]).val() + "|" + $(quantityOfBase[i]).text() + "\">" + $(names[i]).val() + "</option> ";
        }
        selected.append(htmlStr);
        $('#td_add_units select').not('.multi-select').select2();
    }
}
function addNewUnitConversion() {
    var isEmptyRow = false;
    if ($('#unit_conversions tbody tr').length === 0 || ($('#unit_conversions tbody tr').length === 1 && $('#unit_conversions tbody tr td').length === 1)) {
        $('#unit_conversions tbody').empty();
        isEmptyRow = true;
    }
    var htmlAddNew = "<tr id=\"add-unit-row\" style=\"display: none;\">";
    htmlAddNew += "<td id=\"td_add_name\"><input type=\"text\" class=\"form-control\" placeholder=\"e.g. \'Palette\'\" style=\"width:100%\"></td>";
    if (isEmptyRow) {
        htmlAddNew += "<td id=\"td_add_quantity\"><input type=\"number\" min=\"0\" step=\".001\" value=\"1\" style=\"display: none;\" class=\"form-control\" placeholder=\"Quantity\" value=\"\" style=\"width: 85px;\">";
        htmlAddNew += "</td> <td id=\"td_add_units\" style=\"max-width: 300px\"> <select onchange=\"onSelectUnitChange(this)\" data-placeholder=\"Please select\" name=\"units\" class=\"form-control select2\" style=\"width: 100%;display: none;\">";
        htmlAddNew += "<option value=\"\"></option>  </select> </td>";
        htmlAddNew += "<td id=\"td_add_primary\" style=\"display: none\"><label> <input type=\"radio\" checked class=\"form-control\"></label></td>";
    } else {
        htmlAddNew += "<td id=\"td_add_quantity\"> <input type=\"number\" min=\"0\" step=\".001\" class=\"form-control\" placeholder=\"Quantity\" value=\"\" style=\"width: 85px;\">";
        htmlAddNew += "</td> <td id=\"td_add_units\" style=\"max-width: 300px\"> <select onchange=\"onSelectUnitChange(this)\" data-placeholder=\"Please select\" name=\"units\" class=\"form-control select2\" style=\"width: 100%;\">";
        htmlAddNew += "<option value=\"\"></option>  </select></td>";
        htmlAddNew += "<td id=\"td_add_primary\" style=\"display: none\"><label> <input type=\"radio\" class=\"form-control\"></label></td>";
    }

    htmlAddNew += "<td id=\"td_add_active\"><label><input type=\"checkbox\" class=\"form-control\" checked></label></td>";
    htmlAddNew += "<td style=\"width: 80px;\"> <div class=\"pull-right\">";
    if (isEmptyRow) {
        htmlAddNew += "<a href=\"#\" class=\"btn btn-success\" onclick=\"confirmNewRowUnits(this)\"><i class=\"fa fa-check\" style=\"color: #fff;\"></i></a>";
    } else {
        htmlAddNew += "<a href=\"#\" class=\"btn btn-success \" disabled onclick=\"confirmNewRowUnits(this)\"><i class=\"fa fa-check\" style=\"color: #fff;\"></i></a>";
    }

    htmlAddNew += "&nbsp;<a href=\"#\" class=\"btn btn-danger\" onclick=\"removeAddUnits()\"><i class=\"fa fa-remove\" style=\"color: #fff;\"></i></a>";
    htmlAddNew += "</div> <div class=\"clearfix\"></div> </td> </tr>";
    $('#unit_conversions tbody').append(htmlAddNew);

    getSelectedUnits();
    $('#add-unit-row').fadeIn();
    $("#btn_addNewUnitConversion").attr("disabled", "");
}
function onSelectUnitChange(ev) {
    if (!$(ev).val() || $(ev).val().split("|")[0] === "") {
        $($(ev.parentElement.parentElement).find('td div.pull-right a.btn-success')[0]).attr('disabled');
        return false;
    }
    $($(ev.parentElement.parentElement).find('td div.pull-right a.btn-success')[0]).removeAttr('disabled');
    var quantity = $('#td_add_quantity input').val();
    if (quantity === 0 || quantity === "") {
        var quantityRef = $('tr.tr_units_' + $(ev).val().split("|")[0] + ' td.row_quantity').text();
        quantityRef = parseFloat(quantityRef);
        $('#td_add_quantity input').val(quantityRef);
    }
}
function validateUnitConversion() {
    var valid = true;
    if ($('#td_add_name input').val().trim() === "") {
        valid = false;
        cleanBookNotification.error(_L("ERROR_MSG_379"), "Qbicles");
    }
    return valid;
}

var isAddNewConversions = false;
function confirmNewRowUnits(ev) {
    if ($(ev).attr("disabled")) return false;
    //validate Name of unit
    var newUnitName = $('#td_add_name input').val();
    var listComponentUnit = $('#td_add_units select option');
    for (var i = 0; i < listComponentUnit.length; i++) {
        if (newUnitName == '') {
            $("#td_add_name input").attr("Name", "InvalidUnitName");
            validateObj = $("#form-unit-table").validate();
            validateObj.showErrors({ InvalidUnitName: "This field is required." });
            $("#td_add_name input").removeAttr("Name", "InvalidUnitName");
            $("#td_add_name input").val("");
            return;
        } else if (newUnitName == $(listComponentUnit[i]).text()) {
            $("#td_add_name input").attr("Name", "InvalidUnitName");
            validateObj = $("#form-unit-table").validate();
            validateObj.showErrors({ InvalidUnitName: "Unit name exsisted in the item." });
            $("#td_add_name input").removeAttr("Name", "InvalidUnitName");
            $("#td_add_name input").val("");
            return;
        }
    }

    if (!validateUnitConversion()) return;
    var idBuild = UniqueId();
    // var idUnit = $('#td_add_units select').val().split("|")[0];
    //var idRef = $('#td_add_units select').val().split("|")[1];
    var productUnit = {
        Id: idBuild,
        IsBase: false,
        Name: $('#td_add_name input').val(),
        Quantity: parseFloat($('#td_add_quantity input').val()),
        QuantityOfBaseunit: $('#td_add_quantity input').val(),
        ParentUnit: {
            Id: $('#td_add_units select').val().split("|")[0],
            Name: $('#td_add_units select option:selected').text(),
            QuantityOfBaseunit: parseFloat($('#td_add_units select').val().split("|")[1])
        },
        //IsPrimary: $('#td_add_primary input')[0].checked,
        IsActive: $('#td_add_active input')[0].checked
    }
    if (productUnit.ParentUnit === null || productUnit.ParentUnit.Id === 0 || productUnit.ParentUnit.Id === "") {
        productUnit.IsBase = true;
        productUnit.QuantityOfBaseunit = 1; // base
    } else {
        productUnit.IsBase = false;
        productUnit.QuantityOfBaseunit = productUnit.Quantity * productUnit.ParentUnit.QuantityOfBaseunit; // base
    }
    if (productUnit.IsBase) {
        if ($('#form_inventory_name .unit-base-iv').length > 0)
            $('#form_inventory_name .unit-base-iv').text(" (Inventory units: " + productUnit.Name + ")");
        else
            $("#form_inventory_name").append(" <span class=\"unit-base-iv\">(Inventory units: " + productUnit.Name + ")</span>");
    }

    //if (idRef && idRef.length > 0 && idUnit !== 'BaseUnit') {
    //    var quaUnitRef = parseFloat($('tr.tr_units_' + idRef + ' td.row_name span').text());
    //    productUnit.QuantityOfBaseunit = parseFloat(productUnit.Quantity) * quaUnitRef;
    //}
    // remove row add
    $('#add-unit-row').remove();

    var html = "<tr class=\"tr_units_" + idBuild + "\"><td class=\"row_name\"> <input class='unitIds' value=\"" + idBuild + "\" type=\"hidden\" />";
    html += "<input required type='text' onchange=\"changeUnitName('" + idBuild + "')\" id=\"unit_" + idBuild + "\" value='" + productUnit.Name + "' class='form-control unitName' style=\"width: 100%\"/> <span class=\"hidden\">" + productUnit.QuantityOfBaseunit + "</span><p class=\"hidden\">" + productUnit.IsBase + "</p> </td>";
    html += "<td class=\"row_quantity\">" + productUnit.Quantity.toFixed(3) + "</td> <td class=\"row_componentUnit\">";
    html += "<p class='parent_" + productUnit.ParentUnit.Id + " mod-css'>" + productUnit.ParentUnit.Name + "</p> <input value=\"" + productUnit.ParentUnit.Id + "\" type=\"hidden\" />";
    html += "</td> <td class=\"row_primary\" style=\"display: none\"> <label> ";
    if (productUnit.IsPrimary) {
        html += "<input type=\"radio\" name=\"primary[]\" checked class=\"form-control\">";
    } else {
        html += "<input type=\"radio\" name=\"primary[]\" class=\"form-control\">";
    }

    html += "</label> </td> <td class=\"row_active\"> <label> ";
    if (productUnit.IsActive)
        html += "<input type=\"checkbox\" value=\"" + productUnit.IsActive + "\" checked class=\"form-control\">";
    else
        html += "<input type=\"checkbox\" value=\"" + productUnit.IsActive + "\" class=\"form-control\">";
    if (!productUnit.IsBase) {
        html += "</label> </td> <td class=\"row_delete\"> <a href=\"#\" onclick=\"deleteUnit(0, this)\" class=\"btn btn-danger pull-right\">";
        html += "<i class=\"fa fa-trash\" style=\"color: #fff;\"></i> </a>  </td> </tr>";
    } else {
        html += "</label> </td> <td> </td>";
    }
    if (productUnit.IsPrimary) {
        var tr_uncheckPris = $('#unit_conversions td.row_primary input');
        if (tr_uncheckPris.length > 0) {
            for (var j = 0; j < tr_uncheckPris.length; j++) {
                $(tr_uncheckPris[j])[0].checked = false;
            }
        }
    }
    if (productUnit.ParentUnit.Id === 0 || productUnit.ParentUnit.Id === "") {
        productUnit.ParentUnit = null;
    }
    //add new row
    $('#unit_conversions tbody').append(html);
    $("#btn_addNewUnitConversion").removeAttr("disabled");
}
function deleteUnit(unitId, ev) {
    if (isUnitABaseOfAnotherUnit(ev)) {
        cleanBookNotification.error("Delete failed. The unit is in use elsewhere.", "Qbicles");
    } else if (unitId > 0) {
        $.ajax({
            type: 'get',
            url: '/TraderItem/DeleteUnitConversion?unitId=' + unitId,
            dataType: 'json',
            success: function (response) {
                if (response.actionVal === 1) {
                    cleanBookNotification.removeSuccess();
                    $(ev.parentElement.parentElement).remove();
                    getSelectedUnits();
                }
                else if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $(ev.parentElement.parentElement).remove();
        getSelectedUnits();
    }
}

function isUnitABaseOfAnotherUnit(ev) {
    var unitDelete = $(ev).parent().parent().find(".unitName").val();
    var list = $(".row_componentUnit > p");
    for (let index = 0; index < list.length; index++) {
        var element = list[index].outerText;
        if (unitDelete.localeCompare(element) == 0) return true;
    }
    return false;
}

function removeAddUnits() {
    $('#add-unit-row').remove();
    $("#btn_addNewUnitConversion").removeAttr("disabled");
    if ($('#unit_conversions tbody tr').length === 0) {
        var htmlEmpty = "<tr class=\"odd\"><td valign=\"top\" colspan=\"5\" class=\"dataTables_empty\">No data available in table</td></tr>";
        $('#unit_conversions tbody').append(htmlEmpty);
    }
}

function changelocationAll() {
    //$(ev).attr('disabled', 'disabled');
    if ($('#form_specifics_active_in_all').html() === 'Apply') {
        $('#form_specifics_active_in_all').html('Applied');
        if (!$('#form_specifics_active_in_current')[0].checked) {
            $('#form_specifics_active_in_current')[0].checked = true;
            $($('#form_specifics_active_in_current')[0].parentElement).removeClass('btn-default off');
            $($('#form_specifics_active_in_current')[0].parentElement).addClass('btn-success');
        }
    } else
        $('#form_specifics_active_in_all').html('Apply');
}
function changeCurrentLocation(id) {
    $.ajax({
        type: 'post',
        url: '/TraderItem/AddRemoveLocation',
        data: { locationId: $('#local-manage-select').val(), traderItemId: id },
        dataType: 'json',
        success: function (response) {
            if (response.Object) {
                if (response.Object.IsActiveInAllLocations) {
                    $('#form_specifics_active_in_all').html('Applied');
                } else {
                    $('#form_specifics_active_in_all').html('Apply');
                }
            }
            isChangeItem = true;
            if (curentTab === 'item-tab') {
                CallBackFilterDataItemOverViewServeSide(true);
            } else if (curentTab === 'inventory-tab') {
                selectInventoryTab();
            } else if (curentTab === 'resource-tab') {
                selectResourceTab();
            } else if (curentTab === 'pricebook-tab') {
                SelectPriceBookTab();
            }
            if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
// recipe
function addRecipes() {
    var trs_recipe = $('#tb_main_recipe tbody tr');
    if (recipes.length === 0 && !(trs_recipe.length === 1 && $('#tb_main_recipe tbody tr td').length === 1)) {
        for (var i = 0; i < trs_recipe.length; i++) {
            recipes.push({
                Id: $($(trs_recipe[i]).find('td.row_recipes_name input')).val(),
                Name: $($(trs_recipe[i]).find('td.row_recipes_name p')).text(),
                Ingredients: [],
                IsCurrent: $(trs_recipe[i]).find('td.row_recipes_iscurrent input')[0].checked,
                IsActive: $(trs_recipe[i]).find('td.row_recipes_active input')[0].checked
            });
        }
        setDefaultIsCurrentRecipe();
    }
    $("#tb_add_recipe tbody").empty();
    $("#form_add_recipe_name").val("");
    $('#form_add_recipe_iscurrent')[0].checked = false;
    addNewRowRecipe2('tb_add_recipe');
    $('#editrecipe').hide();
    $('#newrecipe').show();
}
function editRecipes(id) {
    var recipeItem = null;
    if (recipes.length === 0) {
        var trs_recipe = $('#tb_main_recipe tbody tr');
        for (var i = 0; i < trs_recipe.length; i++) {
            recipes.push({
                Id: $($(trs_recipe[i]).find('td.row_recipes_name input')).val(),
                Name: $($(trs_recipe[i]).find('td.row_recipes_name p')).text(),
                Ingredients: [],
                IsCurrent: $(trs_recipe[i]).find('td.row_recipes_iscurrent input')[0].checked,
                IsActive: $(trs_recipe[i]).find('td.row_recipes_active input')[0].checked
            });
        }
        setDefaultIsCurrentRecipe();
    }

    for (var j = 0; j < recipes.length; j++) {
        if (recipes[j].Id.toString() === id.toString())
            recipeItem = recipes[j];
    }
    if (recipeItem) {
        loadRecipeEdit6(recipeItem);
    } else if (!recipeItem && (id !== '0' || id !== '')) {
        getRecipeById(id).then(function (res) {
            recipeItem = res;
            loadRecipeEdit6(recipeItem);
        });
    }

    //if (id.toString().startsWith("99999")) {
    //    //get recipe in script
    //    for (var i = 0; i < recipes.length; i++) {
    //        if (recipes[i].Id.toString() === id.toString())
    //            recipeItem = recipes[i];
    //        loadRecipeEdit(recipeItem);
    //    }
    //} else if(){
    //    // get recipe in db
    //    getRecipeById(id).then(function (res) {
    //        recipeItem = res;
    //        loadRecipeEdit(recipeItem);
    //    });
    //}
    $('#newrecipe').hide();
    $('#editrecipe').show();
}
function loadRecipeEdit(obj) {
    $('#edit_recipe_name').val(obj.Name);
    $('#edit_recipe_form_id').val(obj.Id);
    $('#edit_recipe_iscurrent')[0].checked = obj.IsCurrent;
    $("#tb_edit_recipe tbody").empty();
    if (obj.Ingredients && obj.Ingredients.length > 0) {
        getIngredientOnServer(obj.Ingredients).then(function (res) {
            obj.Ingredients = res;
            for (var i = 0; i < obj.Ingredients.length; i++) {
                $('#table_template tr.new_row_recipe td.item_selected').empty();
                $('#table_template tr.new_row_recipe td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\"  data-placeholder=\"Choose an unit\"></select>");
                var selected = $('#table_template tr.new_row_recipe td.item_selected select');
                selected.empty();
                if (obj.Ingredients[i].Units.length > 0) {
                    var htmlStr = "<option value=\"\"></option> ";
                    for (var k = 0; k < obj.Ingredients[i].Units.length; k++) {
                        htmlStr += "<option value=\"" + obj.Ingredients[i].Units[k].Id + "\">" + obj.Ingredients[i].Units[k].Name + "</option> ";
                    }
                    selected.append(htmlStr);
                } else {
                    var htmlStr2 = "<option value=\"\"></option> ";
                    selected.append(htmlStr2);
                }
                if (!obj.Ingredients[i].SubItem) obj.Ingredients[i].SubItem = {};
                if (obj.Ingredients[i].SubItem.InventoryDetails.length > 0) {
                    if (obj.Ingredients[i].SubItem.InventoryDetails[0].AverageCost === 0) {
                        obj.Ingredients[i].AverageCost = "0";
                    } else obj.Ingredients[i].AverageCost = obj.Ingredients[i].SubItem.InventoryDetails[0].AverageCost.toString();

                    if (obj.Ingredients[i].SubItem.InventoryDetails[0].LatestCost === 0) {
                        obj.Ingredients[i].LatestCost = "0";
                    } else obj.Ingredients[i].LatestCost = obj.Ingredients[i].SubItem.InventoryDetails[0].LatestCost.toString();
                }

                var clone = $('#table_template tbody tr.new_row_recipe').clone();
                $($(clone).find('td.item_name select')).addClass('form-control select2 ');
                $($(clone).find('td.item_name select')).val(obj.Ingredients[i].SubItem.Id);
                $($(clone).find('td.item_name input')).val(obj.Ingredients[i].Id);
                $($(clone).find('td.item_quantity input')).val(obj.Ingredients[i].Quantity);
                $($(clone).find('td.item_selected select')).val(obj.Ingredients[i].Unit.Id);
                $($(clone).find('td.item_averagecost span')).text(obj.Ingredients[i].AverageCost);
                $($(clone).find('td.item_latestcost span')).text(obj.Ingredients[i].LatestCost);
                $("#tb_edit_recipe tbody").append(clone);
            }
            $('#tb_edit_recipe tbody tr.new_row_recipe td select').not('.multi-select').select2();
        });
    }
}

function loadRecipeEdit6(obj) {
    $('#edit_recipe_name').val(obj.Name);
    $('#edit_recipe_form_id').val(obj.Id);
    $('#edit_recipe_iscurrent')[0].checked = obj.IsCurrent;
    $("#tb_edit_recipe tbody").empty();
    $.LoadingOverlay("show");
    if (obj.Ingredients && obj.Ingredients.length > 0) {
        getIngredientOnServer3(obj)
    }
    // $.LoadingOverlay("hide");
}

function validateAddRecipeForm(tbid) {
    var valid = true;
    if (tbid === 'tb_add_recipe') {
        if ($("#form_add_recipe_name").val().trim() === "") {
            valid = false;
            $("#form_new_recipe").validate().showErrors({ recipe_name: "Recipe name is required." });
        }
    } else {
        if ($("#edit_recipe_name").val().trim() === "") {
            valid = false;
            $("#form_edit_recipe").validate().showErrors({ recipe_name_edit: "Recipe name is required." });
        }
    }
    //Validate Recipe Items
    var rows = $('#' + tbid + " tr");
    for (var i = 0; i < rows.length; i++) {
        var FieldName = $(rows[i]).find("td.item_name select");
        if (FieldName.val() === "") {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_623"), "Recipe items");
            break;
        }
        var FieldQuantity = $(rows[i]).find("td.item_quantity input");
        if (FieldQuantity.val() <= 0) {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_624"), "Recipe items");
            break;
        }
        var FieldUnit = $(rows[i]).find("td.item_selected select");
        if (FieldUnit.val() === "") {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_625"), "Recipe items");
            break;
        }
    }
    //End Validate Recipe
    return valid;
}
function saveRecipe(item, traderItemId) {
    var dfd = new $.Deferred();
    if (traderItemId > 0) {
        $.ajax({
            type: 'post',
            url: '/TraderItem/saverecipe',
            dataType: 'json',
            data: { recipe: item, traderItemId: traderItemId },
            success: function (response) {
                dfd.resolve(response.Object);
                if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                    dfd.resolve(null);
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                dfd.resolve(null);
            }
        });
    }
    return dfd.promise();
}
function getRecipeById(id) {
    var dfd = new $.Deferred();
    if (id > 0 && !id.toString().startsWith("99999")) {
        $.ajax({
            type: 'get',
            url: '/TraderItem/getrecipebyid?id=' + id,
            dataType: 'json',
            success: function (response) {
                dfd.resolve(response.Object);
                if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                    dfd.resolve(null);
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                dfd.resolve(null);
            }
        });
    }
    return dfd.promise();
}

function activeRecipe(id, ev) {
    var value = ev.checked;
    if (id > 0 && !id.toString().startsWith("99999")) {
        $.ajax({
            type: 'post',
            url: '/TraderItem/ChangeActiveRecipe',
            data: { recipeId: id, value: value },
            dataType: 'json',
            success: function (response) {
                for (var i = 0; i < recipes.length; i++) {
                    if (recipes[i].Id === id) recipes[i].IsActive = value;
                }
                if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else if (id.toString().startsWith("99999")) {
        for (var i = 0; i < recipes.length; i++) {
            if (recipes[i].Id === id) recipes[i].IsActive = value;
        }
    }
}

function addNewRowRecipe(tbid) {
    $('#table_template tr.new_row_recipe td.item_selected').empty();
    $('#table_template tr.new_row_recipe td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\"  data-placeholder=\"Choose an unit\"></select>");
    var selected = $('#table_template tr.new_row_recipe td.item_selected select');
    selected.empty();
    var htmlStr = "<option value=\"\"></option> ";
    var clone = $('#table_template tbody tr.new_row_recipe').clone();
    $($(clone).find('td.item_name select')).addClass('form-control select2 ');
    $("#" + tbid + " tbody").append(clone);
    selected.append(htmlStr);
    $('#' + tbid + ' select').not('.multi-select').select2();
}

function addNewRowRecipe2(tbid) {
    //find existed thPosition
    var listExistedId = [];
    $.each($(".new_row_recipe2"), function (indexInArray, valueOfElement) {
        if ($(valueOfElement).attr("thPosition") != null) listExistedId.push(parseInt($(valueOfElement).attr("thPosition")));
    });
    var nextId = Math.max.apply(null, listExistedId) + 1;

    //create new options
    $('#template-options-select tr.new_row_recipe2 td.item_selected').empty();
    $('#template-options-select tr.new_row_recipe2 td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\"  data-placeholder=\"Choose an unit\"></select>");
    var clone = $('#template-options-select tbody tr.new_row_recipe2').clone();
    $(clone).addClass("gen" + nextId).attr("thPosition", nextId);
    $(clone).find(".recipe-ingredient-add-mew").addClass('yeu-em-never-sai');
    $("#" + tbid + " tbody").append(clone);
    var selectOption = $("#" + tbid + " tbody" + " .gen" + nextId);
    //apply select2
    $("#" + tbid + " tbody" + " .gen" + nextId + " td.item_selected select").select2();
    initSelectOption2(null, selectOption, true);
}

function removeRowRecipe(ev) {
    $(ev).remove();
}

//onSelectItem2() below
function onSelectItem(ev) {
    if ($(ev).val() !== "") {
        $($(ev.parentElement.parentElement).find('td.item_averagecost span')).text($(ev).find(":selected").attr("average-cost"));
        $($(ev.parentElement.parentElement).find('td.item_latestcost span')).text($(ev).find(":selected").attr("latest-cost"));
    }
    $(ev.parentElement.parentElement).find('td.item_selected').empty();
    $(ev.parentElement.parentElement).find('td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\" data-placeholder=\"Choose an unit\"></select>");
    var selected = $(ev.parentElement.parentElement).find('td.item_selected select');
    getUnitsOnServer($(ev).val()).then(function (res) {
        lstUnits = res;
        if (lstUnits.length > 0) {
            var htmlStr = "<option value=\"\"></option> ";
            for (var i = 0; i < lstUnits.length; i++) {
                htmlStr += "<option value=\"" + lstUnits[i].Id + "\">" + lstUnits[i].Name + "</option> ";
            }
            selected.append(htmlStr);
        } else {
            var htmlStr2 = "<option value=\"\"></option> ";
            selected.append(htmlStr2);
        }

        selected.not('.multi-select').select2();
    });
}
function changeQuantity(ev) {
    var unitcost = 0;
    var quantity = 0;
    if (!isNaN(parseFloat($('#form_inventory_unitcost').val().toString()))) {
        unitcost = parseFloat($('#form_inventory_unitcost').val().toString());
    }
    if (!isNaN(parseFloat($($(ev).find('td.item_quantity input')).val().toString()))) {
        quantity = parseFloat($($(ev).find('td.item_quantity input')).val().toString());
    }
    $($(ev).find('td.item_cost span')).text(unitcost * quantity);
}

function saveNewRecipe(traderItemId, tbid) {
    if (validateAddRecipeForm(tbid)) {
        // get item recipe
        var recipeName = "";
        var recipeId = 0;
        var isCurrent = false;
        if (tbid === 'tb_add_recipe') {
            recipeName = $('#form_add_recipe_name').val();
            recipeId = int;
            isCurrent = $('#form_add_recipe_iscurrent')[0].checked;
            int--;
        }
        else if (tbid === 'tb_edit_recipe') {
            recipeName = $('#edit_recipe_name').val();
            recipeId = $('#edit_recipe_form_id').val();
            isCurrent = $('#edit_recipe_iscurrent')[0].checked;
        }
        var recipeItem = {
            Id: recipeId,
            Name: recipeName,
            Ingredients: [],
            IsActive: true,
            IsCurrent: isCurrent
        }

        var trnews = $('#' + tbid + ' tbody tr');
        if (trnews.length > 0) {
            for (var i = 0; i < trnews.length; i++) {
                if (!$($(trnews[i]).find('td.item_name select')).val()) {
                    $($(trnews[i]).find('td.item_name select')).val('');
                }
                recipeItem.Ingredients.push({
                    Id: $($(trnews[i]).find('td.item_name input')).val(),
                    SubItem: { Id: $($(trnews[i]).find('td.item_name select')).val().split('|')[0] },
                    Quantity: $($(trnews[i]).find('td.item_quantity input')).val(),
                    Unit: { Id: $($(trnews[i]).find('td.item_selected select')).val() },
                    AverageCost: $($(trnews[i]).find('td.item_averagecost span')).text(),
                    LatestCost: $($(trnews[i]).find('td.item_latestcost span')).text()
                });
            }
        }
        addObjectToRecipeTable(recipeItem);
        $('#newrecipe').hide();
        $('#editrecipe').hide();
    }
}
function addObjectToRecipeTable(obj) {
    if ($('#tb_main_recipe tbody tr').length === 1 && $('#tb_main_recipe tbody tr td').length === 1) {
        $('#tb_main_recipe tbody').empty();
    }
    var isExists = false;
    var isCurrent = false;
    // check exists currrentrecipe
    for (var i = 0; i < recipes.length; i++) {
        if (recipes[i].IsCurrent === true) isCurrent = true;
    }
    if (!isCurrent) obj.IsCurrent = true;

    for (var j = 0; j < recipes.length; j++) {
        if (recipes[j].Id.toString() === obj.Id.toString()) {
            isExists = true;
            recipes[j] = obj;
            if (obj.IsCurrent === true) {
                changeIsCurrentRecipe(obj.Id);
            }
        }
    }

    if (obj.Id > 0 && !obj.Id.toString().startsWith("99999") && $('#tb_main_recipe tbody tr').length >= 1 && $('#tb_main_recipe tbody tr td').length > 1) {
        $('#tb_main_id_' + obj.Id).val(obj.Id);
        $($($('#tb_main_id_' + obj.Id)[0].parentElement).find('p')).text(obj.Name);
        for (var k = 0; k < recipes.length; k++) {
            if (obj.Id === recipes[k].Id) recipes[k] = obj;
        }
    } else if (isExists) {
        var tr = $('#tb_main_id_' + obj.Id)[0].parentElement.parentElement;
        $($(tr).find('td.row_recipes_name p')).text(obj.Name);
        $($(tr).find('td.row_recipes_name input')).val(obj.Id);

        $($(tr).find('td.row_recipes_name input')).attr('id', 'tb_main_id_' + obj.Id);
        $($(tr).find('td.row_recipes_iscurrent input'))[0].checked = obj.IsCurrent;
        $($(tr).find('td.row_recipes_iscurrent input')).attr('onchange', 'changeIsCurrentRecipe(' + obj.Id + ')');
        $($(tr).find('td.row_recipes_active input')).attr('onchange', 'activeRecipe(' + obj.Id + ', this)');
        $($(tr).find('td button.btn-warning')).attr('onclick', 'editRecipes(' + obj.Id + ')');
    } else {
        var htmlStr = "<tr> <td class=\"row_recipes_name\" colspan=\"1\"> <p>" + obj.Name + "</p>";
        htmlStr += "<input type=\"hidden\"  id=\"tb_main_id_" + obj.Id + "\" value=\"" + obj.Id + "\"/> </td>";
        htmlStr += "<td class=\"row_recipes_active\"> <label>";
        if (obj.IsActive)
            htmlStr += "<input type=\"checkbox\" onchange=\"activeRecipe(" + obj.Id + ", this)\" value=\"" + obj.IsActive + "\" checked class=\"form-control\"> ";
        else
            htmlStr += "<input type=\"checkbox\" onchange=\"activeRecipe(" + obj.Id + ", this)\" value=\"" + obj.IsActive + "\" class=\"form-control\"> ";
        htmlStr += "</label> </td>";
        htmlStr += "<td class=\"row_recipes_iscurrent\"> <label>";
        if (obj.IsCurrent) {
            htmlStr += "<input type=\"radio\" name=\"primaryRecipe[]\" checked onchange=\"changeIsCurrentRecipe(" + obj.Id + ")\">";
        } else {
            htmlStr += "<input type=\"radio\" name=\"primaryRecipe[]\" onchange=\"changeIsCurrentRecipe(" + obj.Id + ")\">";
        }
        htmlStr += "</label> </td>";
        htmlStr += "<td><button type=\"button\" class=\"btn btn-warning\" onclick=\"editRecipes(" + obj.Id + ")\"><i class=\"fa fa-pencil\"></i> &nbsp; Edit</button></td> </tr>";

        $('#tb_main_recipe tbody').append(htmlStr);

        recipes.push(obj);
        if (obj.IsCurrent === true) {
            changeIsCurrentRecipe(obj.Id);
        }
    }

    var rows = $('td.row_recipes_iscurrent input');
    for (var l = 0; l < rows.count; l++) {
        rows[l][0].checked = false;
    }
    var trCurrent = $('#tb_main_id_' + obj.Id)[0].parentElement.parentElement;
    $($(trCurrent).find('td.row_recipes_iscurrent input'))[0].checked = obj.IsCurrent;
}
function changeIsCurrentRecipe(id) {
    setDefaultIsCurrentRecipe();
    for (var i = 0; i < recipes.length; i++) {
        recipes[i].IsCurrent = false;
        if (recipes[i].Id.toString() === id.toString()) {
            recipes[i].IsCurrent = true;
        }
    }
}
function setDefaultIsCurrentRecipe() {
    if (recipes && recipes.length > 0) {
        var isCurrent = false;
        for (var i = 0; i < recipes.length; i++) {
            if (recipes[i].IsCurrent === true) {
                isCurrent = true;
            }
        }
        if (!isCurrent) {
            recipes[0].IsCurrent = true;
        }
    }
};

var BKAccount = {};

function closeSelected() {
};
function initSelectedAccount() {
}
function initInventoryCreateModal(text) {
    $('.iv-item-name').text($('#form_specifics_name').val());
    var trbaseunit = $("#unit_conversions tr td.row_name p:contains('true')");
    if (trbaseunit.text().startsWith('true')) {
        var unitname = trbaseunit.closest('tr').find('td.row_name .unitName').val();
        if (unitname) {
            $('.iv-unit-base-name').text(unitname);
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_700"), "Qbicles");
            return;
        }
    } else {
        cleanBookNotification.error(_L("ERROR_MSG_700"), "Qbicles");
        return;
    }
    var _txtLocation = $('#local-manage-select option:selected').text();
    $('.iv-locations').text(_txtLocation);
    $('#frmCreateInventory').validate(
        {
            rules: {
                ivquantity: {
                    required: true,
                    min: 0
                },
                ivunitcost: {
                    required: true,
                    min: 0
                }
            }
        });
    $("#frmCreateInventory").submit(function (event) {
        event.preventDefault(); //prevent default action
        if ($('#frmCreateInventory').valid()) {
            var val = parseFloat($('#ivquantity').val()) + parseFloat($('#hdfCurrentInventory').val());
            $('#form_inventory_instock').val(val);
            $("#app-trader-inventory-create").modal("hide");
        } else {
            $('#ivquantity').val(0);
            $('#ivunitcost').val(0);
        }
    });
    $("#app-trader-inventory-create").modal("show");
}
function save() {
    if (!validateUniqueSKUandBarcode()) {
        $('#theform .admintabs a[href=#item-2]').click();
        return;
    }

    if (!validateInventoryForm($('#form_inventory_checkbox')[0].checked) || !validateSpecificsForm()) {
        cleanBookNotification.error(_L("ERROR_MSG_626"), "Qbicles");
        return;
    }

    if (!$('#item_protags').val()) {
        valid = false;
        $("#additional-form").validate().showErrors({ extratags: "This field is required." });
        $('#additional-tab a').trigger('click');
        return;
    }

    //if (!isNaN(parseInt($('#traderItem_id').val())) && parseInt($('#traderItem_id').val()) > 0)
    //    changeCurrentLocation(parseInt($('#traderItem_id').val()));

    $.LoadingOverlay("show");

    var trader_item = {
        Id: $('#traderItem_id').val(),
        Group: { Id: $("#form_specifics_category").val() },
        Name: $("#form_specifics_name").val(),
        Description: $("#form_specifics_description").text(),
        DescriptionText: $("#form_specifics_description_text").val(),
        ImageUri: "no-image",
        Locations: [],
        Units: [],
        Barcode: $("#form_inventory_barcode").val(),
        SKU: $("#form_inventory_SKU").val(),
        IsCompoundProduct: $('#form_showrecipes')[0].checked,
        //update new model
        PrimaryVendors: [],
        InventoryDetails: [],
        ResourceDocuments: [],
        AdditionalInfos: [],
        AssociatedRecipes: [],
        VendorsPerLocation: getVenDorLocationObj(),
        PurchaseAccount: { Id: $('#form_purchase').val() },
        SalesAccount: { Id: $('#form_salesaccount').val() },
        TaxRates: [],
        InventoryAccount: { Id: $('#form_inventoryaccount').val() },
        IsCommunityProduct: false,
        IsBought: $('#form_specifics_isSold')[0].checked,
        IsSold: !$('#form_specifics_isSold')[0].checked,
        IsActiveInAllLocations: $('#form_specifics_active_in_all').text().toLocaleLowerCase() === "applied" ? true : false,
        GalleryItems: GetGalleryItems()
    };

    if ($('#form_specifics_isell')[0])
        trader_item.IsSold = $('#form_specifics_isell')[0].checked;
    if (recipes.length > 0) {
        for (var i = 0; i < recipes.length; i++) {
            trader_item.AssociatedRecipes.push(recipes[i]);
        }
    }
    if (trader_item.IsCompoundProduct && trader_item.AssociatedRecipes.length === 0) {
        cleanBookNotification.error(_L("ERROR_MSG_628"), "Qbicles");
        $.LoadingOverlay("hide");
        return false;
    }

    if ($('#item_brand').val() !== "" && $('#item_brand').val() !== null) {
        trader_item.AdditionalInfos.push({ Id: $('#item_brand').val(), Type: 1 });
    }
    if ($('#item_quality').val() !== "" && $('#item_quality').val() !== null) {
        trader_item.AdditionalInfos.push({ Id: $('#item_quality').val(), Type: 3 });
    }
    if ($('#item_need').val()) {
        for (var o = 0; o < $('#item_need').val().length; o++) {
            trader_item.AdditionalInfos.push({ Id: $('#item_need').val()[o], Type: 2 });
        }
    }

    if ($('#item_protags').val()) {
        var $tagNames = $("#item_protags").val();
        var $name = JSON.parse($tagNames);
        for (var i = 0; i < $name.length; i++) {
            trader_item.AdditionalInfos.push({ Id: 0, Name: $name[i].value, Type: 4 });
        }
    }

    var purchase_taxrates = $('#form_taxrate_select').val();
    if (purchase_taxrates) {
        for (var i = 0; i < purchase_taxrates.length; i++) {
            trader_item.TaxRates.push({ Id: purchase_taxrates[i] });
        }
    }
    var sale_taxrates = $('#form_taxrate_select_sale').val();
    if (sale_taxrates) {
        for (var i = 0; i < sale_taxrates.length; i++) {
            trader_item.TaxRates.push({ Id: sale_taxrates[i] });
        }
    }
    // doc resource
    if ($('#value_resource_document').val() && $('#value_resource_document').val().length > 0) {
        var docResource = $('#value_resource_document').val().split(',');
        for (var d = 0; d < docResource.length; d++) {
            if (docResource[d].trim().length > 0)
                trader_item.ResourceDocuments.push({ Id: docResource[d].trim() });
        }
    }

    var contacts = $('#form_specifics_contacts').val();
    if (contacts && contacts.length > 0) {
        //update new model
        for (var x = 0; x < contacts.length; x++) {
            trader_item.PrimaryVendors.push({ Id: contacts[x] });
        }
    }

    if ($('#unit_conversions tbody tr#add-unit-row').length > 0) {
        $('#unit_conversions tbody tr#add-unit-row').remove();
    }
    if ($('#unit_conversions tbody tr').length > 0 && $('#unit_conversions tbody tr td').length > 1) {
        var units = $('#unit_conversions tbody tr');
        if (units.length > 0) {
            var unitsConversions = [];
            for (var j = 0; j < units.length; j++) {
                var unitTemp = $($(units[j]).find('td.row_componentUnit input')).val();
                var uC = {
                    Id: $($(units[j]).find('td.row_name .unitIds')).val(),
                    Name: $($(units[j]).find('td.row_name .unitName')).val(),
                    IsBase: $($(units[j]).find('td.row_name p')).text() === 'true' ? true : false,
                    MeasurementType: $('#item_measure_type').val(),
                    Quantity: $($(units[j]).find('td.row_quantity')).text(),
                    QuantityOfBaseunit: $($(units[j]).find('td.row_name span')).text(),
                    IsActive: $(units[j]).find('td.row_active input')[0].checked,
                    //IsPrimary: $(units[j]).find('td.row_primary input')[0].checked,
                    ParentUnit: (unitTemp && unitTemp !== "") ? { Id: unitTemp } : null
                };
                unitsConversions.push(uC);
            }
            if (unitsConversions.length > 0) {
                for (var m = 0; m < unitsConversions.length; m++) {
                    unitsConversions[m].ParentUnit = selectedChild(unitsConversions[m], unitsConversions)
                }

                for (var k = 0; k < unitsConversions.length; k++) {
                    var strId = unitsConversions[k].Id.toString();
                    if (isNaN(parseInt(unitsConversions[k].Id.toString()))) {
                        if (trader_item.AssociatedRecipes && trader_item.AssociatedRecipes.length > 0) {
                            for (var z = 0; z < trader_item.AssociatedRecipes.length; z++) {
                                if (trader_item.AssociatedRecipes[z].Ingredients && trader_item.AssociatedRecipes[z].Ingredients.length > 0)
                                    for (var n = 0; n < trader_item.AssociatedRecipes[z].Ingredients.length; n++) {
                                        if (trader_item.AssociatedRecipes[z].Ingredients[n].Unit.Id === unitsConversions[k].Id) {
                                            trader_item.AssociatedRecipes[z].Ingredients[n].Unit.Id = int;
                                        }
                                    }
                            }
                        }
                        unitsConversions[k].Id = int;
                        for (var l = 0; l < unitsConversions.length; l++) {
                            if (k !== l && unitsConversions[l].Id === strId) {
                                unitsConversions[l].Id = int;
                            }
                        }
                        int--;
                    }
                }
                trader_item.Units = unitsConversions;
            }
        }
    }
    // inventory
    var currentLocationId = $('#local-manage-select').val();
    if ($('#form_inventory_checkbox')[0].checked === true) {
        if (currentLocationId) {
            trader_item.InventoryDetails.push({
                Location: { Id: currentLocationId },
                MinInventorylLevel: $('#form_inventory_mininv').val().replace(/\,/g, ""),
                MaxInventoryLevel: $('#form_inventory_maxinv').val().replace(/\,/g, ""),
                CurrentInventoryLevel: $('#hdfCurrentInventory').val().replace(/\,/g, "")
            });
        }
    }
    //Fix currentLocationId in page Business Profile
    if ($('#form_inventory #local-manage-select').length > 0)
        currentLocationId = 0;
    //End
    var isCurrentLocation = $('#form_specifics_active_in_current')[0].checked;

    var _ivquantity = $('#ivquantity').val();
    var _ivunitcost = $('#ivunitcost').val();
    var createInventory = {
        LocationId: $('#local-manage-select').val(),
        Quantity: _ivquantity ? _ivquantity : 0,
        Unitcost: _ivunitcost ? _ivunitcost : 0
    };

    if ($('#form_specifics_icon').val() !== '') {
        UploadMediaS3ClientSide("form_specifics_icon").then(function (mediaS3Object) {
            trader_item.ImageUri = mediaS3Object.objectKey;
            $('#form_specifics_icon_text').val(mediaS3Object.objectKey);
            ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);
        });
    } else {
        trader_item.ImageUri = $('#form_specifics_icon_text').val();
        ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);
    }
}

ItemProductSubmit = function (traderItem, createInventory, currentLocationId, isCurrentLocation) {
    $.ajax({
        type: 'post',
        url: '/TraderItem/SaveItemProduct?currentLocationId=' + currentLocationId + '&isCurrentLocation=' + isCurrentLocation,
        data: { item: traderItem, createInventory: createInventory },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-inventory-item-add').modal('toggle');
                isChangeItem = true;
                if (curentTab === 'item-tab') {
                    CallBackFilterDataItemOverViewServeSide(true);
                } else if (curentTab === 'inventory-tab') {
                    selectInventoryTab();
                } else if (curentTab === 'pricebook-tab') {
                    SelectPriceBookTab();
                }
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-inventory-item-add').modal('toggle');
                isChangeItem = true;
                if (curentTab === 'item-tab') {
                    CallBackFilterDataItemOverViewServeSide(true);
                } else if (curentTab === 'inventory-tab') {
                    selectInventoryTab();
                } else if (curentTab === 'pricebook-tab') {
                    SelectPriceBookTab();
                }
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            isChangeItem = true;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function selectedChild(item, lstItem) {
    for (var i = 0; i < lstItem.length; i++) {
        if (item && item.ParentUnit && item.ParentUnit.Id === lstItem[i].Id) {
            return lstItem[i];
        }
    }
    return null;
}

function selectTraderItemsTab() {
    var url = $('#item_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/Trader/ListTraderItem?locationId=' + $('#local-manage-select').val() + '&callback=true';
    $('#item-tab').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#item-tab').load(ajaxUri, function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#tb_trader_items').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#item-tab').LoadingOverlay("hide");
        setDefauleInputSearchItem();
        searchOnTableItems();
    });
};

function selectResourceTab() {
    var url = $('#resource_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/TraderResource/ListResource';
    $('#resource-tab').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#resource-tab').load(ajaxUri, function () {
        $('#resource-tab').LoadingOverlay("hide");
    });
};

function selectInventoryTab() {
    var url = $('#inventory_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/Trader/ListInventory?locationId=' + $('#local-manage-select').val() + '&callback=true';
    $('#inventory-tab').LoadingOverlay("show");
    setTimeout(function () {
        cleanItemAndProductSubTabs();
        $('#inventory-tab').load(ajaxUri, function () {
            $('#inventory-tab').LoadingOverlay("hide");
        });
    }, 1000);
};

function SelectPriceBookTab() {
    var ajaxUri = '/TraderPriceBooks/ListPriceBook?locationId=' + $('#local-manage-select').val() + '&callback=true';
    var url = $('#pricebook_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    $('#pricebook-tab').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#pricebook-tab').load(ajaxUri, function () {
        $('.manage-columns.pricebook input[type="checkbox"]').on('change', function () {
            var table = $('#tb_pricebooks').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#pricebook-tab').LoadingOverlay("hide");
        setDefauleInputSearchPriceB();
        searchOnTablePriceBs();
    });
};

function SelectItemsImportTab() {
    var ajaxUri = '/Commerce/LoadBusinessProfileTab?tab=items-import&reload=false';
    var url = $('#items_import').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    $('#items-import').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#items-import').load(ajaxUri, function () {
        $('#items-import').LoadingOverlay("hide");
    });
}

// Item VenDor
function selectLocationVendorContact(idModel, selectId) {
    var id = $('#' + selectId).val();
    if (validateContact(id)) return;
    var idBuild = UniqueId();
    var locationId = $("#local-manage-select").val();
    var name = $('#' + selectId + ' option:selected').text();
    if (id !== "") {
        var strHtmlVendor = "<tr class=\"vendor_location_" + idBuild + "\"><td class=\"location_name\" >";
        strHtmlVendor += "<p>" + name + "</p> <input type=\"hidden\" value=\"0\" />";
        strHtmlVendor += "</td ><td class=\"vendor_location_primary\"><label>";
        if (!validateItemVendor()) {
            strHtmlVendor += "<input type=\"radio\" name=\"itemvendor[]\" checked>";
        } else {
            strHtmlVendor += "<input type=\"radio\" name=\"itemvendor[]\">";
        }
        strHtmlVendor += "</label></td> <td class=\"vendor_button\"><input class=\"location_id\" type=\"hidden\" value=\"" + locationId + "\"/>";
        strHtmlVendor += "<input class=\"vendor_id\" type=\"hidden\" value=\"" + id + "\" /><button class=\"btn btn-danger\" onclick=\"deleteItemVenDor('" + idBuild + "'," + idModel + ")\"><i class=\"fa fa-trash\"></i></button></td></tr>";
        $('#tb_location_contact tbody').append(strHtmlVendor);
        // set default PrimaryVendor
        setDefaultPrimaryVendor();
    }
}
function getVenDorLocationObj() {
    var locationVendor = [];
    var trs = $('#tb_location_contact tbody tr');
    if (trs.length > 0) {
        for (var i = 0; i < trs.length; i++) {
            var idItem = $($(trs[i]).find('td.location_name input')).val();
            var location = $($(trs[i]).find('td.vendor_button input.location_id')).val();
            var vendor = $($(trs[i]).find('td.vendor_button input.vendor_id')).val();
            if (parseInt(idItem) === 0) {
                locationVendor.push({
                    Id: 0,
                    Vendor: { Id: vendor },
                    Location: { Id: location },
                    IsPrimaryVendor: $(trs[i]).find('td.vendor_location_primary input')[0].checked
                });
            }
        }
    }
    return locationVendor;
}
function validateContact(vendorid) {
    if (isNaN(vendorid)) vendorid = "0";
    var location = $('#local-manage-select').val();
    var trs = $('#tb_location_contact tbody tr');
    if (trs.length > 0) {
        for (var i = 0; i < trs.length; i++) {
            var trVenId = $($(trs[i]).find('td.vendor_button input.vendor_id')).val();
            var trLoId = $($(trs[i]).find('td.vendor_button input.location_id')).val();
            if (parseInt(vendorid) === parseInt(trVenId) && parseInt(location) === parseInt(trLoId)) {
                return true;
            }
        }
    }
    return false
}
function changeLocationVendorPrimary(traderItemId, locationvendorId) {
    $.ajax({
        type: 'get',
        url: '/TraderItem/LocationVendorPrimaryChange',
        data: { traderItemId: traderItemId, locationvendorId: locationvendorId },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
}
function deleteItemVenDor(id, idModel) {
    if (isNaN(parseInt(id))) {
        $('#tb_location_contact tbody tr.vendor_location_' + id).remove();
        setDefaultPrimaryVendor();
    } else {
        $.ajax({
            type: 'post',
            url: '/TraderItem/RemoveLocationVendor',
            data: { vendorId: $('#tb_location_contact tbody tr.vendor_location_' + id + ' td.location_name input').val(), traderItemId: idModel },
            dataType: 'json',
            success: function (response) {
                if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                } else {
                    $('#tb_location_contact tbody tr.vendor_location_' + id).remove();
                    setDefaultPrimaryVendor();
                }
            },
            error: function (er) {
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    }
}
function validateItemVendor() {
    var valid = true;
    var isPrimaryVendors = $('#tb_location_contact tbody tr td.vendor_location_primary input');
    if (isPrimaryVendors.length > 0) {
        for (var i = 0; i < isPrimaryVendors.length; i++) {
            if (isPrimaryVendors[i].checked === true)
                return true;
        }
        valid = false;
    } else valid = true;
    return valid;
}
function setDefaultPrimaryVendor() {
    var trs = $('#tb_location_contact tbody tr');
    var tds = $('#tb_location_contact tbody tr td');
    if (trs.length > 0 && tds.length > 1) {
        var valid = false;
        for (var i = 0; i < trs.length; i++) {
            if ($(trs[i]).find('td.vendor_location_primary label input')[0].checked) {
                valid = true;
            }
        }
        if (!valid) {
            $(trs[0]).find('td.vendor_location_primary label input')[0].checked = true;
        }
    }
}
function nextLocationVendor() {
    if (validateItemVendor()) {
        $('#a_next_itemvendor').click();
    }
    else {
        cleanBookNotification.warning("A primary vendor is required.", "Qbicles");
    }
}
// update
function editContact(id) {
    var locationId = $('#local-manage-select').val();
    $('#app-trader-add-contact').empty();
    $('#app-trader-add-contact').load('/Trader/AddContactInTraderItem?locationId=' + (locationId ? locationId : 0) + '&traderItemId=' + $('#traderItem_id').val() + '&callback=true' + '&contactid=' + id);
    $('#app-trader-add-contact').modal('toggle');
}

function jumtoconfig(modalId) {
    $(modalId).modal("hide");
    $('#menu_traderapp a[href=#Config]').click();
}
function validAddContact() {
    var valid = true;
    if ($('#form_traderitem_contact_name').val() === '') {
        valid = false;
        $("#form_add_contact_traderItem").validate().showErrors({ name: "Name is required." });
    }
    if (isNaN(parseInt($('#form_traderitem_contact_phone').val().replace(/ /g, "").replace(/\+/g, "")))) {
        valid = false;
        $("#form_add_contact_traderItem").validate().showErrors({ phone: "Phone is required number." });
    }
    if ($('#form_traderitem_contact_email').val() === '') {
        valid = false;
        $("#form_add_contact_traderItem").validate().showErrors({ email: "Email is required." });
    }
    return valid;
}
function addContactbutton() {
    editContact(0);
}
function loadContact() {
    $('#table_contact').removeClass("hidden");
    $('#table_contact').empty();
    $('#table_contact').load('/Trader/ListContactTraderItem');
}
function removeContact(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'delete',
        url: '/TraderItem/DeleteVendorContact?id=' + id,
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1) {
                cleanBookNotification.removeSuccess();
                $('#a_process').attr('disabled', false);
                loadContact();
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            if (response.result === false) {
                $('#a_process').attr('disabled', true);
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            $.LoadingOverlay("hide");
        }
    });
}
function AddContact() {
    if (!validAddContact()) return;
    var contact = {
        Id: $('#form_traderitem_contact_id').val(),
        Name: $('#form_traderitem_contact_name').val(),
        AvatarUri: $('#form_traderitem_contact_avataruri').val(),
        CompanyName: $('#form_traderitem_contact_company').val(),
        JobTitle: $('#form_traderitem_contact_jpbtitle').val(),
        Address: {
            Id: $('#form_traderitem_contact_address_id').val(),
            AddressLine1: $('#form_traderitem_contact_address1').val(),
            AddressLine2: $('#form_traderitem_contact_address2').val(),
            City: $('#form_traderitem_contact_city').val(),
            State: $('#form_traderitem_contact_state').val(),
            PostCode: $('#form_traderitem_contact_postcode').val(),
            Country: {
                CommonName: $('#form_traderitem_contact_country').val()
            }
        },
        PhoneNumber: $('#form_traderitem_contact_phone').val(),
        CustomerAccount: { Id: $('#form_traderitem_contact_account').val() },
        Email: $('#form_traderitem_contact_email').val(),
        ContactGroup: { Id: $('#form_traderitem_contact_group').val() }
    }
    if ($('#form_traderitem_contact_avataruri_file').val() !== '') {
        UploadMediaS3ClientSide("form_traderitem_contact_avataruri_file").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === null || mediaS3Object.objectKey === 'no-image') {
                cleanBookNotification.error(_L("ERROR_MSG_629"), "Qbicles");
            } else {
                contact.AvatarUri = response;
                $('#form_traderitem_contact_avataruri').val(mediaS3Object.objectKey);

                SubmitItemProductContact(contact);
            }
        });
    } else {
        contact.AvatarUri = $('#form_traderitem_contact_avataruri').val();
        SubmitItemProductContact(contact);
    }
}

SubmitItemProductContact = function (contact) {
    $.ajax({
        type: 'post',
        url: '/TraderItem/SaveContact?country=' + $('#form_traderitem_contact_country').val(),
        data: { contact: contact },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#a_process').attr('disabled', false);
                loadContact();
                cancelContact();
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#a_process').attr('disabled', false);
                loadContact();
                cancelContact();
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function cancelContact() {
    $('#app-trader-add-contact').modal('toggle');
    $('#form_add_contact_traderItem')[0].reset();
}
function showBookkeepingTreeView() {
    $('#app-bookkeeping-treeview').modal('toggle');
}

function cleanItemAndProductSubTabs() {
    //Item Overview Tab
    $('#item-tab').empty();
    //Import Tab
    $('#items-import').empty();
    //Resources Tab
    $('#resource-tab').empty();
    //Inventory Tab
    $('#inventory-tab').empty();
    //Price Books Tab
    $('#pricebook-tab').empty();
    //Movement Tab
    $('#movement-tab').empty();
    //Ajust Stock Tab
    $('#adjuststock-tab').empty();
    //Shift Audit Tab
    $('#inventoryaudit-tab').empty();
}
// Adjust stock tab
function SelectAdjustStockTab() {
    var url = $('#adjuststock_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/TraderItem/AdjustStockTab';
    $('#adjuststock-tab').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#adjuststock-tab').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        $('#adjuststock-tab').LoadingOverlay("hide");
        SelectSpotCountTab();
    });
};

//Spot count tab
function SelectSpotCountTab() {
    var ajaxUri = '/TraderSpotCount/TraderItemSpotCountContent';
    $('#spot-count-tab').LoadingOverlay("show");
    $('#spot-count-tab').empty();
    $('#waste-report-tab').empty();
    $('#spot-count-tab').load(ajaxUri, function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#spotList').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#spot-count-tab').LoadingOverlay("hide");
    });
};
//Waste report tab
function SelectWasteReportTab(reload) {
    var ajaxUri = '/TraderWasteReport/TraderItemWasteReportContent';
    if (!reload)
        $('#waste-report-tab').LoadingOverlay("show");
    $('#spot-count-tab').empty();
    $('#waste-report-tab').empty();
    $('#waste-report-tab').load(ajaxUri, function () {
        if (!reload)
            $('#waste-report-tab').LoadingOverlay("hide");
    });
};

function SelectAdjustStockWasteTab() {
    var ajaxUri = '/TraderItem/AdjustStockTab';
    $('#adjuststock-tab').LoadingOverlay("show");
    $('#adjuststock-tab').empty();
    $('#adjuststock-tab').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        $('a[href="#waste-report-tab"]').tab('show');
        SelectWasteReportTab();
        $('#adjuststock-tab').LoadingOverlay("hide");
    });
};

// Inventory audit tab
function SelectInventoryAuditTab() {
    var url = $('#inventoryaudit_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/TraderStockAudits/InventoryAuditTab?locationId=' + $('#local-manage-select').val();
    $('#inventoryaudit-tab').LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $('#inventoryaudit-tab').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        $('#inventoryaudit-tab').LoadingOverlay("hide");
    });
};

function showStockAuditTable() {
    $("#stockaudit-table").on('processing.dt', function (e, settings, processing) {
        if (processing) {
            $('#stockaudit-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#stockaudit-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "serverSide": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderStockAudits/ListStockAudit',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search_stockaudit").val(),
                    "workgroupId": $("#stockaudit-workgroup-filter").val(),
                });
            }
        },
        "columns": [
            {
                name: "Name",
                data: "Name",
                orderable: true
            },
            {
                name: "Workgroup",
                data: "Workgroup",
                orderable: true
            },
            {
                name: "StartedDate",
                data: "StartedDate",
                orderable: true
            },
            {
                name: "FinishedDate",
                data: "FinishedDate",
                orderable: true
            },
            {
                name: "Items",
                data: "Items",
                orderable: true
            },
            {
                name: null,//"Status",
                data: null,//"Status",
                orderable: true,
                render: function (value, type, row) {
                    switch (row.Status) {
                        case 0:
                            return "<span class='label label-lg label-primary'>" + row.StatusName + "</span>";
                        case 1:
                            return "<span class='label label-lg label-info'>" + row.StatusName + "</span>";
                        case 2:
                            return "<span class='label label-lg label-primary'>" + row.StatusName + "</span>";
                        case 3:
                            return "<span class='label label-lg label-success'>" + row.StatusName + "</span>";
                        case 4:
                            return "<span class='label label-lg label-danger'>" + row.StatusName + "</span>";
                        case 5:
                            return "<span class='label label-lg label-warning'>" + row.StatusName + "</span>";
                    }
                }
            },
            //{
            //    name: null,//"Processed",
            //    data: null,//"Processed",
            //    orderable: true,
            //    render: function (value, type, row) {
            //        switch (row.Status) {
            //            case ShiftAuditStatus.Draft:
            //                return "<span class='label label-lg label-primary'>Draft</span>";

            //            case ShiftAuditStatus.Approved:
            //                return "<span class='label label-lg label-success'>Approved</span>";

            //            case ShiftAuditStatus.Denied:
            //                return "<span class='label label-lg label-warning'>Denied</span>";

            //            case ShiftAuditStatus.Discarded:
            //                return "<span class='label label-lg label-info'>Discarded</span>";

            //            case ShiftAuditStatus.Reviewed:
            //                return "<span class='label label-lg label-warning'>Reviewed</span>";

            //            case ShiftAuditStatus.Pending:
            //                return "<span class='label label-lg label-warning'>Pending</span>";

            //        }
            //        return "";
            //    }
            //},
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    if (row.Status == 0)
                        return "<button class='btn btn-info' data-toggle='modal' onclick='addStockAudit(" + row.Id + ")' data-target='#app-trader-inventory-stock-audit'><i class='fa fa-pencil'></i> &nbsp; Continue</button>";
                    else
                        return "<button class='btn btn-primary' onclick='location = \"/TraderStockAudits/ShiftAuditMaster?id=" + row.Id + "\"'><i class='fa fa-eye'></i> &nbsp; Manage</button>";
                }
            }
        ],
        "drawCallback": function (settings) {
            $.getScript("/Content/DesignStyle/js/html5tooltips.js");
        },
        "order": [[2, "desc"]]
    });

    //var ajaxUri = '/TraderStockAudits/ListStockAudit?locationId=' + $('#local-manage-select').val() + '&callback=true';
    //$('#stockaudit_table').LoadingOverlay("show", { minSize: "70x60px" });
    //$('#stockaudit_table').empty();
    //$('#stockaudit_table').load(ajaxUri.trim().replace(/\s/g, ""), function () {
    //    $('.manage-columns input[type="checkbox"]').on('change', function () {
    //        var table = $('#stockaudit-list').DataTable();
    //        var column = table.column($(this).attr('data-column'));
    //        column.visible(!column.visible());
    //    });
    //    $('#stockaudit_table').LoadingOverlay("hide");
    //    searchOnTableStockAudit();
    //});
}

// Movement tab
function SelectMovementTab() {
    var url = $('#movement_tab').attr('href');
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = '/Trader/TraderMovementTabShow?locationId=' + $('#local-manage-select').val() + '&callback=true';
    $('#movement-tab').LoadingOverlay("show");
    setTimeout(function () {
        cleanItemAndProductSubTabs();
        $('#movement-tab').load(ajaxUri, function () {
            $('#movement-tab').LoadingOverlay("hide");
        });
    }, 1000);
};
function changedisplayunit(idItem, idUnit) {
    $('#app-trader-change-unit').LoadingOverlay("show");
    $('#app-trader-change-unit').empty();
    $('#app-trader-change-unit').load("/Trader/ShowProductUnitByItem?itemId=" + idItem + "&unitId=" + idUnit, function () {
        $('#app-trader-change-unit').LoadingOverlay("hide");
    });
}
function inventoryBatchConfirmChangeUnit(itemId) {
    var unitId = $('#batchchangeunit').val().split('|')[0];
    $.getJSON('/Trader/ChangeUnitItemInMovement', { traderItemId: itemId, unitId: unitId, stringdate: $('#movement_daterange').val() }, function (response) {
        //alert(JSON.stringify(response));
        var table = $('#movement_table').DataTable();
        var data = table.row('#tr_movementitem_' + itemId).data();
        if (response) {
            data.BaseUnitId = response.BaseUnitId;
            data.In = response.In;
            data.Out = response.Out;
            data.Difference = response.Difference;
            table.row('#tr_movementitem_' + itemId).data(data);
        }
    });
}
$('#movement_daterange').on('apply.daterangepicker', function (ev, picker) {
    //var _dateFormat = $dateFormatByUser.toUpperCase();
    $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
    //$('#movement_daterange').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
    SelectMovementTab();
});

//#region chart data
var viewTrendData = {
    Chart: {
        Label: [],
        QuantityIn: [],
        QuantityOut: [],
        QuantityDeff: []
    },
    ChartValue: {
        LabelValue: [],
        QuantityInValue: [],
        QuantityOutValue: [],
        QuantityDeffValue: []
    }
};

function clearTrendDate() {
    viewTrendData = {
        Chart: {
            Label: [],
            QuantityIn: [],
            QuantityOut: [],
            QuantityDeff: []
        },
        ChartValue: {
            LabelValue: [],
            QuantityInValue: [],
            QuantityOutValue: [],
            QuantityDeffValue: []
        }
    }
}
var mychart = '';
var mychart2 = '';
var doesLoadingChartViewTrendFinish = 1;
// 0 running, 2 waiting, 1 not running
function loadingChartViewTrend(itemId) {
    clearTrendDate();
    $.ajax({
        type: "POST",
        data: {
            itemId: itemId,
            datestring: $('#movement_daterange').val(),
            isGenSystem: ($('#systemgen').val() && $('#systemgen').val() != '') ? true : false,
            locationId: $('#local-manage-select').val(),
            unitId: $('#viewtrend_product').val()
        },
        url: "/Trader/ShowChartViewTrendServer",
        beforeSend: function (xhr, opts) {
            clearTrendDate();
            if (doesLoadingChartViewTrendFinish == 0 || doesLoadingChartViewTrendFinish == 2) {
                doesLoadingChartViewTrendFinish = 2;
                xhr.abort();
            } else {
                doesLoadingChartViewTrendFinish = 0;
            }
        },
        success: function (response) {
            if (Object.keys(response).length == 1) {
                viewTrendData.Chart.QuantityIn.push(0);
                viewTrendData.ChartValue.QuantityInValue.push(0);
                viewTrendData.Chart.QuantityOut.push(0);
                viewTrendData.ChartValue.QuantityOutValue.push(0);
                viewTrendData.Chart.QuantityDeff.push(0);
                viewTrendData.ChartValue.QuantityDeffValue.push(0);
                viewTrendData.Chart.Label.push(0);
                viewTrendData.ChartValue.LabelValue.push(0);
            };
            response.forEach(element => {
                viewTrendData.Chart.QuantityIn.push(element.AvgInQuantity);
                viewTrendData.ChartValue.QuantityInValue.push(element.AvgIn);
                viewTrendData.Chart.QuantityOut.push(element.AvgOutQuantity);
                viewTrendData.ChartValue.QuantityOutValue.push(element.AvgOut);
                viewTrendData.Chart.QuantityDeff.push(element.DeffQuantity);
                viewTrendData.ChartValue.QuantityDeffValue.push(element.Deff);
                label = element.DateByMonth + "." + element.DateByYear;
                if (element.DateByWeek) {
                    label = element.DateByMonth + "." + element.DateByYear + ", W" + element.DateByWeek;
                } else if (element.DateByDay) {
                    label = element.DateByDay + "." + element.DateByMonth + "." + element.DateByYear;
                }
                viewTrendData.Chart.Label.push(label)
                viewTrendData.ChartValue.LabelValue.push(label)
            })
            if (mychart != '') {
                updateChartData(mychart, mychart2);
            }
        },
        complete: function () {
            if (doesLoadingChartViewTrendFinish == 2) {
                doesLoadingChartViewTrendFinish = 1;
                loadingChartViewTrend(itemId);
            }
            else {
                doesLoadingChartViewTrendFinish = 1;
            }
        }
    })
}

function updateChartData(chart, chart2) {
    chart.data.labels = viewTrendData.ChartValue.LabelValue;
    chart2.data.labels = viewTrendData.ChartValue.LabelValue;
    chart.data.datasets.forEach((dataset) => {
        switch (dataset.label) {
            case "Quantity in":
                dataset.data = viewTrendData.Chart.QuantityIn;
                break;
            case "Quantity out":
                dataset.data = viewTrendData.Chart.QuantityOut;
                break;
            case "Absolute quantity difference":
                dataset.data = viewTrendData.Chart.QuantityDeff;
                break;
            default:
                break;
        }
    })
    chart2.data.datasets.forEach((dataset) => {
        switch (dataset.label) {
            case "Value in":
                dataset.data = viewTrendData.ChartValue.QuantityInValue;
                break;
            case "Value out":
                dataset.data = viewTrendData.ChartValue.QuantityOutValue;
                break;
            case "Absolute value difference":
                dataset.data = viewTrendData.ChartValue.QuantityDeffValue;
                break;
            default:
                break;
        }
    })
    chart.update();
    chart2.update();
}

//#endregion

// view trend
function viewtrend(itemId, unitId) {
    $.LoadingOverlay("show");
    $('#app-trader-item-movement').load('/Trader/ShowTraderItemTrend', { itemId: itemId, unitId: unitId, locationId: $('#local-manage-select').val(), datestring: $('#movement_daterange').val() }, function () {
    });

    loadingChartViewTrend(itemId);
}

function templateViewTrend() {
    var htmlcontent = '';
    htmlcontent += `<div class="modal-xl modal-dialog" role="document">
    <div class="modal-content">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">×</span></button>
            <h5 class="modal-title"></h5>
        </div>
        <div class="modal-body">
            <dl class="breakitdown inamodal cx-25">
                <dt>Date time range</dt>
                <dd></dd>
                <dt>Item name</dt>
                <dd></dd>
                <dt>Product group</dt>
                <dd></dd>
                <dt>SKU</dt>
                <dd></dd>
                <dt>Latest cost (£)</dt>
                <dd></dd>
                <dt>On-hand inventory</dt>
                <dd id="onHandInventory"></dd>
            </dl>

            <div class="well custom rounded" style="min-width: 300px; padding: 20px 20px 5px 20px;">
                <div class="row">
                    <div class="col-xs-12 col-sm-4 col-lg-3">
                        <div class="form-group">
                            <label for="unit">Change display unit</label>
                            <select name="unit" id="viewtrend_product" class="form-control select2 select2-hidden-accessible" style="width: 100%;" tabindex="-1" aria-hidden="true">
                            </select><span class="select2 select2-container select2-container--default" dir="ltr" style="width: 100%;"><span class="selection"><span class="select2-selection select2-selection--single" role="combobox" aria-haspopup="true" aria-expanded="false" tabindex="0" aria-labelledby="select2-viewtrend_product-container"><span class="select2-selection__rendered" id="select2-viewtrend_product-container" title="The whole thing"></span><span class="select2-selection__arrow" role="presentation"><b role="presentation"></b></span></span></span><span class="dropdown-wrapper" aria-hidden="true"></span></span>
                        </div>
                    </div>
                    <div class="col-xs-12 col-sm-4 col-lg-3">
                        <div class="form-group">
                            <label>Display format</label>
                            <select name="format" class="form-control select2 select2-hidden-accessible" style="width: 100%;" id="format" tabindex="-1" aria-hidden="true">
                                <option value="0" selected="">Quantity</option>
                                <option value="1">Value</option>
                            </select><span class="select2 select2-container select2-container--default" dir="ltr" style="width: 100%;"><span class="selection"><span class="select2-selection select2-selection--single" role="combobox" aria-haspopup="true" aria-expanded="false" tabindex="0" aria-labelledby="select2-format-container"><span class="select2-selection__rendered" id="select2-format-container" title="Quantity"></span><span class="select2-selection__arrow" role="presentation"><b role="presentation"></b></span></span></span><span class="dropdown-wrapper" aria-hidden="true"></span></span>
                        </div>
                    </div>
                    <div class="col-xs-12 col-sm-4 col-lg-3">
                        <label>Display system generated movements</label>
                        <div class="checkbox toggle">
                            <label>
                                <div id="systemgen" class="toggle btn off" data-toggle="toggle" style="width: 0px; height: 0px;">
                                    <div class="toggle-group">
                                        <label class="btn btn-success toggle-on" onclick="toggleDisplaySystemGen()">On</label>
                                        <label class="btn btn-default active toggle-off" onclick="toggleDisplaySystemGen()">Off</label>
                                        <span class="toggle-handle btn btn-default"></span>
                                    </div>
                                </div>
                            </label>
                        </div>
                    </div>
                </div>
            </div>

            <br><br>
            <div class="tab-content">
                <div class="tab-pane fade in active" id="movement-viewtrend-table">
                <div id="viewtrend-table_wrapper" class="dataTables_wrapper form-inline dt-bootstrap"><div class="row"><div class="col-sm-6"><div class="dataTables_length" id="viewtrend-table_length"><label>Show <select name="viewtrend-table_length" aria-controls="viewtrend-table" class="form-control input-sm"><option value="10">10</option><option value="20">20</option><option value="50">50</option><option value="100">100</option></select> entries</label></div></div><div class="col-sm-6"></div></div><div class="row"><div class="col-sm-12"><table id="viewtrend-table" class="datatable table-striped table-hover community-list dataTable dtr-inline" style="width: 100%; background: rgb(255, 255, 255);" role="grid" aria-describedby="viewtrend-table_info">
                    <thead>
                        <tr role="row"><th class="sorting_asc" tabindex="0" aria-controls="viewtrend-table" rowspan="1" colspan="1" aria-label="Date &amp;amp; time: activate to sort column descending" style="width: 255px;" aria-sort="ascending">Date &amp; time</th><th class="sorting" tabindex="0" aria-controls="viewtrend-table" rowspan="1" colspan="1" aria-label="Trigger: activate to sort column ascending" style="width: 184px;">Trigger</th><th class="qty sorting" tabindex="0" aria-controls="viewtrend-table" rowspan="1" colspan="1" aria-label="Qty In: activate to sort column ascending" style="width: 162px;">Qty In</th><th class="qty sorting" tabindex="0" aria-controls="viewtrend-table" rowspan="1" colspan="1" aria-label="Qty Out: activate to sort column ascending" style="width: 190px;">Qty Out</th><th class="qty sorting" tabindex="0" aria-controls="viewtrend-table" rowspan="1" colspan="1" aria-label="Absolute Qty Difference: activate to sort column ascending" style="width: 448px;">Absolute Qty Difference</th></tr>
                    </thead>
                    <tbody>

                    <tr role="row" class="odd"><td class="sorting_1" tabindex="0"></td><td></td><td></td><td></td><td></td></tr><tr role="row" class="even"><td class="sorting_1" tabindex="0"></td><td></td><td></td><td></td><td></td></tr></tbody>
                    <tfoot style="background: #fff;">
                        <tr><td style="border-top: 1px solid rgba(0, 0, 0, 0.3);" rowspan="1" colspan="1"><strong>Totals</strong></td><td style="border-top: 1px solid rgba(0, 0, 0, 0.3);" rowspan="1" colspan="1"></td><td style="border-top: 1px solid rgba(0, 0, 0, 0.3);" class="qty" rowspan="1" colspan="1"></td><td style="border-top: 1px solid rgba(0, 0, 0, 0.3);" class="qty" rowspan="1" colspan="1"></td><td style="border-top: 1px solid rgba(0, 0, 0, 0.3);" class="qty" rowspan="1" colspan="1"></td></tr>
                    </tfoot>
                </table></div></div><div class="row"><div class="col-sm-5"><div class="dataTables_info" id="viewtrend-table_info" role="status" aria-live="polite">Showing 1 to 1 of 1 entries</div></div><div class="col-sm-7"><div class="dataTables_paginate paging_simple_numbers" id="viewtrend-table_paginate"><ul class="pagination"><li class="paginate_button previous disabled" id="viewtrend-table_previous"><a href="#" aria-controls="viewtrend-table" data-dt-idx="0" tabindex="0">Previous</a></li><li class="paginate_button active"><a href="#" aria-controls="viewtrend-table" data-dt-idx="1" tabindex="0">1</a></li><li class="paginate_button next disabled" id="viewtrend-table_next"><a href="#" aria-controls="viewtrend-table" data-dt-idx="2" tabindex="0">Next</a></li></ul></div></div></div></div>
                </div>
            </div>
        </div>
        </div>
    </div>`
    return htmlcontent;
}

function viewtrendTest(itemId, unitId) {
    var currentTable = $('#app-trader-item-movement #movement-viewtrend-table #viewtrend-table');
    // $('#app-trader-item-movement #movement-viewtrend-table #viewtrend-table').DataTable().destroy();
    currentTable.DataTable().clear().draw();
    currentTable.DataTable({
        "destroy": true,
        "serverSide": true,
        "paging": true,
        "pagingTag": 'button',
        "searching": false,
        "responsive": {
            details: {
                type: 'none'
            }
        },
        columnDefs: [
            { responsivePriority: -1, targets: 0 },
            { responsivePriority: -1, targets: 2 },
            { responsivePriority: -1, targets: 3 },
            { responsivePriority: -1, targets: 4 },
            { responsivePriority: -1, targets: 5 },
            { responsivePriority: -1, targets: 6 },
            { responsivePriority: -1, targets: 7 },
        ],
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/Trader/ShowTableViewTrendServer',
            "type": 'POST',
            "data": function (d) {
                $.LoadingOverlay("show");
                return $.extend({}, d, {
                    "itemId": itemId,
                    "unitId": $('#viewtrend_product').val(),
                    "locationId": $('#local-manage-select').val(),
                    "isGenSystem": ($('#systemgen').val() && $('#systemgen').val() != '') ? true : false,
                    "datestring": $('#movement_daterange').val()
                })
            },
            "complete": function () {
                LoadingOverlayEnd();
                LoadingOverlayEnd();
            }
        },
        "columns": [
            { data: 'date' },
            {
                data: null,
                render: function (data, type, row) {
                    var urlLink = '/';
                    switch (row.trigger) {
                        case "Sale":
                            if (row.saleKey != '#') {
                                urlLink = '/TraderSales/SaleMaster?key=' + row.saleKey;
                            } else {
                                urlLink = '/TraderTransfers/TransferMaster?key=' + row.transferKey;
                            }
                            break;
                        case "Purchase":
                            if (row.purchaseKey != '#') {
                                urlLink = '/TraderPurchases/PurchaseReviewForMovenmentTrend?key=' + row.purchaseKey;
                            } else {
                                urlLink = '/TraderTransfers/TransferMaster?key=' + row.transferKey;
                            }
                            break;
                        case "Waste Report Adjustment":
                            if (row.wasteKey != '#') {
                                urlLink = '/TraderWasteReport/WasteReportMasterForMovenmentTrend?key=' + row.wasteKey;
                            } else {
                                urlLink = '/TraderTransfers/TransferMaster?key=' + row.transferKey;
                            }
                            break;
                        case "Spot Count Adjustment":
                            if (row.spotKey != '#') {
                                urlLink = '/TraderSpotCount/SpotCountMasterForMovenmentTrend?key=' + row.spotKey;
                            } else {
                                urlLink = '/TraderTransfers/TransferMaster?key=' + row.transferKey;
                            }
                            break;
                        case "Manufacturing Job Adjustment":
                            if (row.manufacturingKey != '#') {
                                urlLink = '/Manufacturing/ManufacturingMasterForMovenmentTrend?key=' + row.manufacturingKey;
                                break;
                            }
                        case "Point To Point":
                        case "Inventory Creation":
                            urlLink = '/TraderTransfers/TransferMaster?key=' + row.transferKey;
                            break;
                        case "System Generated":
                        default: return row.trigger;
                    }
                    if (row.transferKey != "#") {
                        return '<a href="' + urlLink + '" target="_blank">' + row.trigger + '</a>'
                    }
                    return row.trigger;
                }
            },
            { data: 'quantityIn' },
            { data: 'quantityOut' },
            { data: 'absoluteQuantity' },
            { data: 'valueIn' },
            { data: 'valueOut' },
            { data: 'absoluteValue' }
        ],
        "footerCallback": function (row, data, start, end, display) {
            var api = this.api();
            var intVal = function (i) {
                return typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 : typeof i === 'number' ? i : 0;
            }
            if (data[0] === undefined) {
                var decimalCurrency = 1
            } else {
                var decimalCurrency = data[0].decimalPlace;
            }

            quantityIn = api
                .column(2, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0).toFixed(decimalCurrency);
            $(api.column(2).footer()).html(quantityIn);

            quantityOut = api
                .column(3, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0).toFixed(decimalCurrency);
            $(api.column(3).footer()).html(quantityOut);
            absoluteQuantity = api
                .column(4, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return Math.abs(quantityIn - quantityOut);
                }, 0);
            $(api.column(4).footer()).html(absoluteQuantity);
            valueIn = api
                .column(5, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0).toFixed(decimalCurrency);
            $(api.column(5).footer()).html(valueIn);
            valueOut = api
                .column(6, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0).toFixed(decimalCurrency);
            $(api.column(6).footer()).html(valueOut);
            absoluteValue = api
                .column(7, { page: 'current' })
                .data()
                .reduce(function (a, b) {
                    return Math.abs(valueIn - valueOut);
                }, 0).toFixed(decimalCurrency);
            $(api.column(7).footer()).html(absoluteValue);
        },
        "initComplete": function (settings, json) {
            changeValueOnHandInventoryDetails(itemId, $('#local-manage-select').val(), $('#viewtrend_product').val());
            LoadingOverlayEnd();

            isHideValue($('#format').val());
        }
    })
}

// change value on-hand inventory
function changeValueOnHandInventoryDetails(itemId, locationId, unitId) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/Trader/GetCurrentOnHandInventoryDetails',
        data: { traderItem: itemId, locationId: locationId, unitId: unitId },
        dataType: 'json',
        success: function (response) {
            $("#onHandInventory").text(response);
            LoadingOverlayEnd();
        },
        error: function (er) {
            console.log("OnHandInventory " + er);
            LoadingOverlayEnd();
        }
    });
}

// optional hide columns in DataTable
function isHideValue(isHideValue) {
    var table = $('#app-trader-item-movement #movement-viewtrend-table #viewtrend-table').DataTable();
    if (isHideValue == 0) {
        table.columns([2, 3, 4]).visible(true);
        table.columns([5, 6, 7]).visible(false);
    } else {
        table.columns([2, 3, 4]).visible(false);
        table.columns([5, 6, 7]).visible(true);
    }
}

// End movement tab

function getResourceImage() {
    var ajaxUri = "/TraderItem/GetListRreourceImage";
    $('#app-trader-resources-image-select').empty();
    $('#app-trader-resources-image-select').load(ajaxUri, function () {
    });
}

var dataDocument = [];
function getResourceDocument() {
    var ajaxUri = "/TraderItem/GetListRreourceDocument";
    $('#app-trader-resources-docs-select').empty();
    $('#app-trader-resources-docs-select').load(ajaxUri, function () {
    });
}
function selectResourceImage(image, name, type) {
    $('#image_resource_select').text(name + '.' + type);
    $('#form_specifics_icon_text').val(image);
    $('#app-trader-resources-image-select').modal("toggle");
}
function selectedChangeDocument(id) {
    for (var i = 0; i < dataDocument.length; i++) {
        if (dataDocument[i].Id === id) {
            dataDocument[i].selected = !dataDocument[i].selected;
        }
    }
}
function selectedDocumentResource() {
    $('#app-trader-resources-docs-select').modal("toggle");
    var valueResourceDoc = '';
    var htmlDoc = '';
    for (var i = 0; i < dataDocument.length; i++) {
        var docRe = dataDocument[i];
        if (docRe.selected) {
            valueResourceDoc += valueResourceDoc === '' ? docRe.Id : ',' + docRe.Id;
            htmlDoc += "<li> <span class=\"btn btn-danger delete_item\" onclick=\"deleteDocument(this," + docRe.Id + ")\"><i class=\"fa fa-trash\"></i></span>";
            htmlDoc += "<a href=\"" + apiDoc + docRe.ImageUri + "\"><img src=\"" + docRe.Type.IconPath + "\" style=\"max-width: 60px; height: auto; padding-right: 10px;\">" + docRe.Name + "</a> </li>";
        }
    }
    $('.buy-doc-attachments ul').empty();
    $('.buy-doc-attachments ul').append(htmlDoc);
    $('#value_resource_document').val(valueResourceDoc);
    $('.buy-doc-attachments').show();
}
function deleteDocument(ev, id) {
    $(ev).parent().remove();
    var docRes = $('#value_resource_document').val();
    docRes = docRes.replace(id, "").replace(/\,\,/g, ",");
    $('#value_resource_document').val(docRes);
    if ($('.buy-doc-attachments ul li').length === 0) {
        $('.buy-doc-attachments').hide();
    }
}

function SearchInventoryBatch(stringdate, key) {
    var ajaxUri = '/Trader/ListItemInventoryBatch?locationId=' + $('#local-manage-select').val() + '&callback=true' + '&datestring=' + (stringdate + "").trim().replace(/\s/g, "");
    $('#movement-table').LoadingOverlay('show');
    $('#movement-table').empty();
    $('#movement-table').load(ajaxUri, function () {
        $("#movement_table").DataTable().draw();
        if (key) {
            $("#movement_table").DataTable().search(key).draw();
        }
        $('#movement-table').LoadingOverlay('hide');
    });
}

function LoadTableDataMovement(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#" + tableid).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $("#" + tableid).LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": ""
        },
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "deferLoading": 30,
        "pageLength": 12,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keysearch: $('#movement_search_dt').val(),
                    stringdate: $('#movement_daterange').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]],
        "rowId": function (a) {
            return 'tr_movementitem_' + a.Id;
        },
    });
}
function FilterDataByServerSide() {
    var url = '/Trader/GetDataItemInventoryBatch';

    var columns = [
        {
            data: "ImageUri",
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                str += '<div class="table-avatar" style="background-image: url(' + apiDoc + row.ImageUri + ');"></div>';
                return str;
            }
        },
        {
            name: "ItemName",
            data: "ItemName",
            orderable: true
        },
        {
            name: "In",
            data: "In",
            orderable: false
        },
        {
            name: "Out",
            data: "Out",
            orderable: false
        },
        {
            name: "Difference",
            data: "Difference",
            orderable: false
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                str += '<div class="btn-group options"> <button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                str += '<i class="fa fa-cog"></i> &nbsp; Options</button> <ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                str += '<li><a href="#" data-toggle="modal" onclick="changedisplayunit(' + row.Id + ', ' + row.BaseUnitId + ')" data-target="#app-trader-change-unit">Change display unit</a></li>';
                str += '<li><a href="#" data-toggle="modal" onclick="viewtrend(' + row.Id + ', ' + row.BaseUnitId + ')" data-target="#app-trader-item-movement">View trend</a></li></ul></div>';

                return str;
            }
        }
    ];
    LoadTableDataMovement('movement_table', url, columns);
    CallBackFilterDataMoventoryServeSide();
}

function CallBackFilterDataMoventoryServeSide() {
    $("#movement_table").DataTable().ajax.reload();
}

// Overview
function showTraderItemAdditional(traderItemId) {
    var ajaxUri = '/TraderItem/GetTraderItem?id=' + traderItemId;
    $('#app-trader-item-additional').LoadingOverlay("show");
    $('#app-trader-item-additional').empty();
    $('#app-trader-item-additional').load(ajaxUri, function () {
        $('#app-trader-item-additional').LoadingOverlay("hide");
    });
}

function showTraderItemDescription(traderItemId) {
    $('#app-trader-item-description').empty();
    $('#app-trader-item-description').LoadingOverlay("show");
    var ajaxUri = '/TraderItem/GetTraderItemDescription?id=' + traderItemId;
    $('#app-trader-item-description').load(ajaxUri, function () {
        $('#app-trader-item-description').LoadingOverlay("hide");
    });
}

var applyFilter = false;
function applyItemOverviewFilter() {
    applyFilter = true;
    itemOverViewFilter.GroupIds = $('#itemoverview-filter-group').val();
    itemOverViewFilter.Types = $('#itemoverview-filter-type').val();
    itemOverViewFilter.Brands = $('#itemoverview-filter-brand').val();
    itemOverViewFilter.Needs = $('#itemoverview-filter-need').val();
    itemOverViewFilter.Rating = $('#itemoverview-filter-rating').val();
    itemOverViewFilter.Tags = $('#itemoverview-filter-tag').val();
    itemOverViewFilter.ActiveInLocation = $('#itemoverview-filter-activeinlocation').val();
    $(".itemOverviewFilterRemove").show();
    CallBackFilterDataItemOverViewServeSide(false);
}
function setDefaultFilter(reset) {
    if (!reset)
        itemOverViewFilter = {
            GroupIds: null,
            Types: null,
            Brands: null,
            Needs: null,
            Rating: null,
            Tags: null,
            ActiveInLocation: 1
        };
    $('#itemoverview-filter-group').val(itemOverViewFilter.GroupIds);
    $('#itemoverview-filter-type').val(itemOverViewFilter.Types);
    $('#itemoverview-filter-brand').val(itemOverViewFilter.Brands);
    $('#itemoverview-filter-need').val(itemOverViewFilter.Needs);
    $('#itemoverview-filter-rating').val(itemOverViewFilter.Rating);
    $('#itemoverview-filter-tag').val(itemOverViewFilter.Tags);
    $('#itemoverview-filter-activeinlocation').val(itemOverViewFilter.ActiveInLocation);
    //$('.multiselect-selected-text').text('None selected');
    if (!reset)
        applyItemOverviewFilter();
    $(".itemOverviewFilterRemove").hide();
}
function removeFilter() {
    applyFilter = false;
    setDefaultFilter(applyFilter);
}
// Filter OverView
function LoadTableDataItemOverView(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    if ($.fn.DataTable.isDataTable("#" + tableid)) {
        $("#" + tableid).DataTable().destroy();
    }

    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "deferLoading": 30,
        "pageLength": 10,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "ajax": {
            "url": url,
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var _currentLocationId = $('#slLocation_items').val();
                return $.extend({}, d, {
                    keysearch: $('#search_dt').val(),
                    groups: itemOverViewFilter.GroupIds === null ? [] : itemOverViewFilter.GroupIds,
                    types: itemOverViewFilter.Types === null ? [] : itemOverViewFilter.Types,
                    bands: itemOverViewFilter.Brands === null ? [] : itemOverViewFilter.Brands,
                    needs: itemOverViewFilter.Needs === null ? [] : itemOverViewFilter.Needs,
                    rating: itemOverViewFilter.Rating === null ? [] : itemOverViewFilter.Rating,
                    tags: itemOverViewFilter.Tags === null ? [] : itemOverViewFilter.Tags,
                    activeInLocation: itemOverViewFilter.ActiveInLocation,
                    clid: (_currentLocationId ? _currentLocationId : null)//If post current LocationId from client it will ignore CurrentLocationManage
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function FilterDataItemOverViewByServerSide() {
    var url = '/Trader/GetDataItemOverView';

    var columns = [
        {
            data: "ImageUri",
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                str += '<a href="' + apiDoc + row.ImageUri + '" class="table-avatar image-pop" rel="resources" style="display: block; background-image: url(\'' + apiDoc + row.ImageUri + '&size=T\');">&nbsp;</a>';
                return str;
            }
        },
        {
            name: "ItemName",
            data: "ItemName",
            orderable: true,
            render: function (value, type, row) {
                var str = row.ItemName;
                return str;
            }
        },
        {
            name: "SKU",
            data: "SKU",
            orderable: true
        },
        {
            name: "Barcode",
            data: "Barcode",
            orderable: true
        },
        {
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                if (row.IsBought) {
                    str += '<span class="label label-lg label-warning">Item I buy</span> ';
                }
                if (row.IsSold) {
                    str += '<span class="label label-lg label-success">Item I sell</span> ';
                }
                if (row.IsCompoundProduct) {
                    str += '<span class="label label-lg label-primary">Compound</span> ';
                }
                return str;
            }
        },
        {
            name: "GroupName",
            data: "GroupName",
            orderable: true
        },
        {
            name: "Description",
            //data: "Description",
            render: function (value, type, row) {
                return '<a data-toggle="modal" href="#app-trader-item-description" onclick="showTraderItemDescription(\'' + row.Id + '\')">' + row.Description + '</a>';
            },
            orderable: true
        },
        {
            name: "Vendor",
            data: "Vendor",
            orderable: false
        },
        {
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                str += '<button class="btn btn-info" data-toggle="modal" onclick="showTraderItemAdditional(\'' + row.Id + '\')" data-target="#app-trader-item-additional"><i class="fa fa-list"></i> &nbsp; View</button>';
                return str;
            }
        },
        {
            name: "IsActive",
            data: "IsActive",
            orderable: true,
            render: function (value, type, row) {
                var str = '';
                if (row.IsActive) {
                    str += '<label><i class="fa fa-check green" style="width: 15px;"></i> &nbsp; Active</label>';
                } else {
                    str += '<label><i class="fa fa-remove red" style="width: 15px;"></i> &nbsp; Inactive</label>';
                }
                return str;
            }
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                str += '<div class="btn-group options">';
                str += '<button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                str += '<i class="fa fa-cog"></i> &nbsp; Options </button>';
                str += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                if (row.IsActive) {
                    str += '<li><a href="javascript:void(0)" onclick="showMenusInLocations(' + row.Id + ',' + !row.IsActive + ', true)">Deactivate in this location</a></li>';
                } else {
                    str += '<li><a href="javascript:void(0)" onclick="setIsActiveByLocations(' + row.Id + ', true, true)">Active in this location</a></li>';
                }

                if (row.IsBought) {
                    str += '<li><a href="javascript:void(0)" onclick="editTraderItem(1, ' + row.Id + ', \'item-tab\')">Edit</a></li>';
                } else {
                    str += '<li><a href="javascript:void(0)" onclick="editTraderItem(2, ' + row.Id + ', \'item-tab\')">Edit</a></li>';
                }
                // str += '<li><a href="javascript:void(0)">Delete</a></li></ul></div>';
                return str;
            }
        }
    ];
    LoadTableDataItemOverView('tb_trader_items', url, columns);
    CallBackFilterDataItemOverViewServeSide(false);
}

function CallBackFilterDataItemOverViewServeSide(isKeepCurrentPage) {
    if (!applyFilter) {
        setDefaultFilter(false);
    }
    setTimeout(function () {
        if (!isKeepCurrentPage) {
            $("#tb_trader_items").DataTable().ajax.reload();
        } else {
            $("#tb_trader_items").DataTable().ajax.reload(null, false);
        }
    }, 500);
}
// End overview
function ShowSaleTaxRates(elm) {
    if ($(elm).prop("checked")) {
        $('#TaxRates-Sell').show();
    } else {
        $('#TaxRates-Sell').hide();
    }
}

var isCancel = false;
function showMenusInLocations(itemId, isActive, isReloadData) {
    var locationIds = [];
    locationIds.push($("#local-manage-select").val());
    $.ajax({
        type: 'post',
        url: '/Trader/GetPosMenusByLocationIds',
        data: { locationIds: locationIds, itemId: itemId },
        dataType: 'json',
        success: function (response) {
            $("#confirm-menus ul").html("");
            if (response.result) {
                if (response.Object.length > 0) {
                    for (var i = 0; i < response.Object.length; i++) {
                        $("#confirm-menus ul").append("<li>" + response.Object[i] + "</li>")
                    }
                    $('#confirm-menus').modal('show');
                    $("#btn-confirm-deactive").data("itemid", itemId);
                    $("#btn-confirm-deactive").data("isactive", isActive);
                    $("#btn-confirm-deactive").data("isreload", isReloadData);
                } else {
                    setIsActiveByLocations(itemId, isActive, isReloadData);
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
}

function setIsActiveByLocations(itemId, isActive, isReloadData) {
    if (isReloadData) {
        var locationIds = [];
        locationIds.push($("#local-manage-select").val());
        $.ajax({
            type: 'post',
            url: '/Trader/setIsActiveByLocations',
            data: { itemId: itemId, isActive: isActive, locationIds: locationIds },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    if (isReloadData != undefined && isReloadData != null) {
                        CallBackFilterDataItemOverViewServeSide(true);
                    }
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(er.error, "Qbicles");
            }
        });
    }
}

function changeUnitName(unitId) {
    var unitNewName = $("tr.tr_units_" + unitId + " td.row_name .unitName").val();

    if (unitNewName == '') {
        $("#unit_" + unitId).attr("Name", "InvalidUnitName");
        var validateObj = $("#form-unit-table").validate();
        validateObj.showErrors({ InvalidUnitName: "The Unit Name is required!" });
        $("#unit_" + unitId).removeAttr("Name", "InvalidUnitName");
        return;
    }

    LoadingOverlay()
    if (typeof unitId == 'number') {
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: '/TraderItem/ChangeUnitName',
            data: { unitId: unitId, unitNewName: unitNewName },
            success: function (response) {
                if (!response.result) {
                    $("tr.tr_units_" + unitId + " td.row_name .unitName").attr("Name", "InvalidUnitName");
                    var validateObj = $("#form-unit-table").validate();
                    validateObj.showErrors({ InvalidUnitName: response.msg });
                    $("tr.tr_units_" + unitId + " td.row_name .unitName").removeAttr("Name", "InvalidUnitName");
                    $("#unit_" + unitId).val($("#unit_" + unitId).attr("value"));
                } else {
                    $("td.row_componentUnit .parent_" + unitId).text(unitNewName);
                    $("#unit_" + unitId).attr("value", unitNewName);
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        })
    } else {
        //validate name to be unique for current Item
        var listUnitName = $(".row_name .unitName");
        for (var i = 0; i < listUnitName.length; i++) {
            if ($(listUnitName[i]).attr("id") != ("unit_" + unitId)) {
                if ($(listUnitName[i]).val() == unitNewName) {
                    $("#unit_" + unitId).attr("Name", "InvalidUnitName");
                    var validateObj = $("#form-unit-table").validate();
                    $("#form-unit-table").validate().showErrors({ InvalidUnitName: "Unit Name exsisted in the item." });
                    $("#unit_" + unitId).removeAttr("Name", "InvalidUnitName");
                    $("#unit_" + unitId).val("");
                    LoadingOverlayEnd();
                    return;
                }
            }
        }

        $("td.row_componentUnit .parent_" + unitId).text(unitNewName);
    }
    LoadingOverlayEnd();
}

function checkInventoryChoose(el) {
    var value = $(el).val();
    if (value) {
        $('.iv-locations').text($(el).find(":selected").text());
        $('.btnaddinventory').prop('disabled', false);
        getInventoryByLocationId();
    }
    else
        $('.btnaddinventory').prop('disabled', true);
}
function getInventoryByLocationId() {
    $.get("/TraderItem/GetCurrentInventory", { locationId: $('#local-manage-select').val(), itemId: $('#traderItem_id').val() })
        .done(function (data) {
            if (data) {
                $('#hdfCurrentInventory').val(data.currentInventory);
                $('#form_inventory_instock').val(data.currentInventory);
                $('#form_inventory_mininv').val(data.minInventorylLevel);
                $('#form_inventory_maxinv').val(data.maxInventoryLevel);
                $('.lblavgcost').text(data.averageCost);
                $('.lbllatestcost').text(data.latestCost);
            }
        });
}

function GetGalleryItems() {
    var items = $('#itemgalerylist figure');
    var galleryItems = []

    if (items.length > 0) {
        for (var i = 0; i < items.length; i++) {
            var uri = $($(items[i]).find('span.image_row')).text();
            var galleryItem = {
                FileUri: uri,
                Order: i
            };
            galleryItems.push(galleryItem)
        }
    }
    return galleryItems;
}

function ItemGaleryDelete(id) {
    var check = confirm('Are you sure you want to delete this image?');
    if (check) {
        $("#figure-" + id).remove();

        var items = $('#itemgalerylist figure');
        if (items.length <= 0) {
            $(".gallerynone").show();
            $(".gallerystart").hide();
        }
    }
}

function changeOrderGalery(oldIndex, newIndex) {
    // get order = newIndex -> set order = oldIndex
    // get order = oldIndex -> set order = newIndex
    console.log(oldIndex);
}

function AddImageGalery() {
    $('#theform').LoadingOverlay("show");

    UploadMediaS3ClientSide("image-galery-input").then(function (mediaS3Object) {
        //trader_item.ImageUri = mediaS3Object.objectKey;
        //$('#form_specifics_icon_text').val(mediaS3Object.objectKey);
        //ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            $('#theform').LoadingOverlay("hide", true);
            return;
        }
        var newImage = "<figure id='figure-" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='flex-contain' id='" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='col img'>";
        newImage += "<div style=\"background-image: url('" + apiDoc + mediaS3Object.objectKey + "')\"></div>";
        newImage += "</div>";
        newImage += "<div class='col options'>";
        newImage += '<span class="image_row" style="display:none">' + mediaS3Object.objectKey + '</span>'
        newImage += "<div class='dropdown contactoptside'>";
        newImage += "<button class='btn btn-primary dropdown-toggle' type='button' data-toggle='dropdown'>";
        newImage += "<i class='fa fa-ellipsis-h'></i>";
        newImage += "</button>";
        newImage += "<ul class='dropdown-menu dropdown-menu-right'>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryEdit('" + mediaS3Object.objectKey + "')\">Edit</a></li>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryDelete('" + mediaS3Object.objectKey + "')\">Delete</a></li>";
        newImage += "</ul></div></div></div>";
        //<li><a onclick="ItemGaleryEdit('@image.FileUri')">Edit</a></li>
        //<li><a onclick="ItemGaleryDelete('@image.FileUri')">Delete</a></li>
        $("#itemgalerylist").append(newImage);
        $(".gallerynone").hide();
        $(".gallerystart").show();
        $('#theform').LoadingOverlay("hide", true);
    });
}

var $imageChanged = "";

function ItemGaleryEdit(id) {
    $imageChanged = id;
    $('#image-galery-edit').trigger('click');
}

function AddImage() {
    $('#image-galery-input').trigger('click');
}

function ChangeImageGalery() {
    $('#theform').LoadingOverlay("show");

    UploadMediaS3ClientSide("image-galery-edit").then(function (mediaS3Object) {
        //trader_item.ImageUri = mediaS3Object.objectKey;
        //$('#form_specifics_icon_text').val(mediaS3Object.objectKey);
        //ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            $('#theform').LoadingOverlay("hide", true);
            return;
        }

        $("#" + $imageChanged).remove();

        var newImage = "<div class='flex-contain' id='" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='col img'>";
        newImage += "<div style=\"background-image: url('" + apiDoc + mediaS3Object.objectKey + "')\"></div>";
        newImage += "</div>";
        newImage += "<div class='col options'>";
        newImage += '<span class="image_row" style="display:none">' + mediaS3Object.objectKey + '</span>'
        newImage += "<div class='dropdown contactoptside'>";
        newImage += "<button class='btn btn-primary dropdown-toggle' type='button' data-toggle='dropdown'>";
        newImage += "<i class='fa fa-ellipsis-h'></i>";
        newImage += "</button>";
        newImage += "<ul class='dropdown-menu dropdown-menu-right'>";

        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryEdit('" + mediaS3Object.objectKey + "')\">Edit</a></li>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryDelete('" + mediaS3Object.objectKey + "')\">Delete</a></li>";
        newImage += "</ul></div></div>";

        $("#figure-" + $imageChanged).append(newImage);

        var figure = document.getElementById("figure-" + $imageChanged);
        figure.setAttribute("id", "figure-" + mediaS3Object.objectKey);

        $('#theform').LoadingOverlay("hide", true);
    });
}

function initSelectOption(defautValue, selectOption) {
    //some variables
    //numberResultPerPage : Get number item per request page (I choose to requset from FE)
    //totalPage = Math.Ceiling(totalItem/numberResultPerPage) : Total page to request (BE only)
    //page : current page. ( request from FE)
    var numberResultPerPage = 15;
    var traderItemId = $("#traderItem_id").attr("value");
    //speacial variables:
    //traderItemId
    //locationId
    // var locationId = $('#local-manage-select').val();
    if (defautValue == null) defautValue = '';
    var lovationId = $("#slLocation_items").val();
    var urk_link = "/trader/GetTraderItemsCustomModel"
    var itemSelected;
    $(".yeu-em-never-sai").select2({
        placeholder: "Test",
        ajax: {
            url: urk_link,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    search: params.term,
                    page: params.page || 0,
                    numberResult: numberResultPerPage,
                    traderItemId: traderItemId,
                    locationId: (lovationId ? lovationId : 0),
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
                const listItems = data.items.map(item => {
                    return {
                        id: item.Id,
                        text: item.Name
                    }
                })
                return {
                    results: listItems,
                    pagination: {
                        more: (params.page * 10) < data.totalItem
                    }
                };
            },
            cache: true,
        }
    })
}
function initSelectOption2(defautValue, selectOption, isLastStep) {
    /*
    ajax only work when user is in a domain. If not, throw ex

    defautValue : value is selected by default;
    selectOption : parent class that have option-select class
    isLastStep : the last ajax will call LoadingOverlay("hide"). It sometime isn't working properly
    some variables
    numberResultPerPage : Get number item per request page (I choose to requset from FE)
    totalPage = Math.Ceiling(totalItem/numberResultPerPage) : Total page to request (BE only)
    page : current page. ( request from FE)
    speacial variables:
    traderItemId
    locationId
    */
    var numberResultPerPage = 30;
    var traderItemId = $("#traderItem_id").attr("value");
    var optionInit;

    var className = $(selectOption).find(".recipe-ingredient-add-mew");
    if (defautValue == null) defautValue = 0;
    //var locationId = $("#slLocation_items").val();
    var locationId = $("#local-manage-select").val();
    var urk_link = "/trader/GetTraderItemsCustomModel"
    $(className).select2({
        ajax: {
            url: urk_link,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    search: params.term,
                    page: params.page || 0,
                    numberResult: numberResultPerPage,
                    traderItemId: traderItemId,
                    locationId: (locationId ? locationId : 0),
                };
            },
            // process data before sent to select2
            processResults: function (data, params) {
                params.page = params.page || 0;
                const listItems = data.items.map(item => {
                    return {
                        id: item.Id,
                        text: item.Name,
                        averageCost: item.AverageCost,
                        latestCost: item.LatestCost
                    }
                })
                // sent to select2 listItems and pagination
                return {
                    results: listItems,
                    pagination: {
                        more: (params.page * 10) < data.totalItem
                    }
                };
            },
            cache: true
        },
        // prepare default selected
        initSelection: function (element, callback) {
            var nameItem = "none";
            $.ajax({
                url: urk_link,
                data: {
                    search: null,
                    selectedItems: defautValue,
                    locationId: (locationId ? locationId : 0),
                },
                dataType: "json",
                success: function (response) {
                    if (response.items.length > 0) {
                        nameItem = response.items[0].Name;
                        optionInit = new Option(nameItem, defautValue, true, true);
                    }
                    // $($(selectOption).find('.item_averagecost span')).text(response.items[0].AverageCost);
                    // $($(selectOption).find('.item_latestcost span')).text(response.items[0].LatestCost);
                },
                error: function () {
                    $.LoadingOverlay("hide");
                },
                complete: function () {
                    callback({ "id": defautValue, "text": nameItem });
                    $(className).append(optionInit);
                    if (isLastStep) $.LoadingOverlay("hide");
                }
            });
        }
    }).on("select2:select", function (e) {
        //I do not use onSelectItem(), because it was built based on client-side
        onSelectItem2(e);
    });
}

function onSelectItem2(param) {
    var itsAverageCost = param.params.data.averageCost;
    var itsLatestCost = param.params.data.latestCost;
    var itsId = param.params.data.id;
    var itsText = param.params.data.text;
    var currentSelectOption = param.currentTarget;
    var classNewRowParent = $(currentSelectOption).parent().parent();
    var itsAverageCostValue = $(classNewRowParent[0]).find(".item_averagecost");
    var itsLatestCostValue = $(classNewRowParent[0]).find(".item_latestcost");

    //prepare unit
    $(classNewRowParent[0]).find('td.item_selected').empty();
    $(classNewRowParent[0]).find('td.item_selected').append("<select class=\"form-control select2\" style=\"width: 100%;\" data-placeholder=\"Choose an unit\"></select>");

    var selected = $(classNewRowParent[0]).find('td.item_selected select');

    getUnitsOnServer(itsId).then(function (res) {
        lstUnits = res;
        if (lstUnits.length > 0) {
            var htmlStr = "<option value=\"\"></option> ";
            for (var i = 0; i < lstUnits.length; i++) {
                htmlStr += "<option value=\"" + lstUnits[i].Id + "\" quantityOfBaseunit = \"" + lstUnits[i].QuantityOfBaseunit + "\" >" + lstUnits[i].Name + "</option> ";
            }
            selected.append(htmlStr);
        } else {
            var htmlStr2 = "<option value=\"\"></option> ";
            selected.append(htmlStr2);
        }

        //event on select unit
        selected.not('.multi-select').select2().on("select2:select", function (param) {
            var exchangeLastestCostValue = itsLatestCost;
            var exchangeAverageCostValue = itsAverageCost;
            var multipleBaseUnit = param.params.data.element.attributes.quantityofbaseunit.value;
            if (multipleBaseUnit == 0)
                multipleBaseUnit = 1;
            exchangeAverageCostValue = exchangeAverageCostValue * multipleBaseUnit;
            exchangeLastestCostValue = exchangeLastestCostValue * multipleBaseUnit;
            $(itsLatestCostValue).val(itsLatestCost).find("span").html(exchangeLastestCostValue);
            $(itsAverageCostValue).val(itsAverageCost).find("span").html(exchangeLastestCostValue);
        });
    });

    //select first or default unit

    //injector value to averageCost and latestCost;
    $(itsLatestCostValue).val(itsLatestCost).find("span").html(0);
    $(itsAverageCostValue).val(itsAverageCost).find("span").html(0);
}