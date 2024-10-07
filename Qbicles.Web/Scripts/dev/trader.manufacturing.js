var unitsSlected = [];
var itemIdSelectedAddEdit = 0;
var filter = {
    Ass: {
        Group: " ",
        Key: ""
    },
    Avai: {
        Workgroup: " ",
        Key: ""
    }
}
// Assembled
function setDefaulFilterAss() {
    //$('#subfilter-group-assembledlist').val(filter.Ass.Group);
    //$('#subfilter-group-assembledlist').select2();
    //$('#search_assembledlist').val(filter.Ass.Key);
}
//function onSelectWorkgroupManuAss(ev) {
//    filter.Ass.Group = $(ev).val();
//    setTimeout(function () { searchOnTableAss(); }, 200);
//}
//function onKeySearchChangedAss(ev) {
//    filter.Ass.Key = $(ev).val();
//    setTimeout(function () { searchOnTableAss(); }, 200);
//}
//function searchOnTableAss() {
//    var listKey = [];
//    if ($('#subfilter-group-assembledlist').val() !== " " && $('#subfilter-group-assembledlist').val() !== "" && $('#subfilter-group-assembledlist').val() !== null) {
//        listKey.push($('#subfilter-group-assembledlist').val());
//    }
//    var keys = $('#search_assembledlist').val().split(' ');
//    if ($('#search_assembledlist').val() !== "" && $('#search_assembledlist').val() !== null && keys.length > 0) {
//        for (var i = 0; i < keys.length; i++) {
//            if (keys[i] !== "") listKey.push(keys[i]);
//        }
//    }
//    $("#community-assembledlist").DataTable().search(listKey.join("|"), true, false, true).draw();
//    $("#community-assembledlist_filter input").val("");
//}


function addEditManufacturing(id, itemid) {
    if (!id) id = 0;
    if (!itemid) itemid = 0;
    $('#app-trader-inventory-manufacturing').LoadingOverlay("show");
    $('#app-trader-inventory-manufacturing').empty();
    $('#app-trader-inventory-manufacturing').load("/Manufacturing/AddEdit?id=" + id + "&locationId=" + $('#local-manage-select').val() + "&itemId=" + itemid, function () {
        $('#app-trader-inventory-manufacturing').LoadingOverlay("hide");
    });
}
function addEditManufacturingSpecific(id) {
    if (!id) id = 0;
    $('#app-trader-inventory-manufacturing-specific').LoadingOverlay("show");
    $('#app-trader-inventory-manufacturing-specific').empty();
    $('#app-trader-inventory-manufacturing-specific').load("/Manufacturing/AddEditSpecific?id=" + id, function () {
        $('#app-trader-inventory-manufacturing-specific').LoadingOverlay("hide");
    });
}
function manuJobViewer(id) {
    if (!id) id = 0;
    $('#app-trader-inventory-recipe-view').LoadingOverlay("show");
    $('#app-trader-inventory-recipe-view').empty();
    $('#app-trader-inventory-recipe-view').load("/Manufacturing/ManujobViewer?id=" + id, function () {
        $('#app-trader-inventory-recipe-view').LoadingOverlay("hide");
    });
}
function showTableManujob() {
    $('#assembledlist').LoadingOverlay("show");
    $('#assembledlist').empty();
    $('#assembledlist').load("/Manufacturing/ShowTableManuJob?locationId=" + $('#local-manage-select').val(), function () {
        $('#assembledlist').LoadingOverlay("hide");
        //setDefaulFilterAss();
        assembledlistSearch();
	});
}
function showTableManujobAvailable() {
    $('#itemlist').LoadingOverlay("show");
    $('.manage-columns-available').removeClass('manage-columns');
    $('#itemlist').empty();
    $('#itemlist').load("/Manufacturing/ShowTableManuJoAvailable?locationId=" + $('#local-manage-select').val(), function () {
        $('#itemlist').LoadingOverlay("hide");
    });
}
function showTraderGroupItems() {
    //Re-init select2 with ajax
    var workGroupId = $("select[name=workgroup]").val();
    var locationId = $("#local-manage-select").val();
    var _url = "/Select2Data/GetGroupedTraderItemsByWG";
    var param = {
        'workGroupId': workGroupId,
        'locationId': locationId
    }

    initSelect2MethodAJAX("tradergroupitem", _url, param, false);
}
function unitOnchange() {
    $('#manu-2').empty();
    $('#manu-2').load("/Manufacturing/ShowManujobByUnit?unitId=" + $('#unit_selected').val() + "&locationId=" + $('#local-manage-select').val(), function () {
        $('#manu-2').LoadingOverlay("hide");
    });
    $('#unit_selected-error').remove();
}
function loadUnit() {
    $("#unit_selected").empty();
    $("#unit_selected").val("");
    if (unitsSlected && unitsSlected.length > 0) {
        var html = "";
        for (var i = 0; i < unitsSlected.length; i++) {
            if ($("#manu_unit").val() == unitsSlected[i].Id) {
                html += "<option selected value=\"" + unitsSlected[i].Id + "\">" + unitsSlected[i].Name + "</option>"
            } else {
                html += "<option value=\"" + unitsSlected[i].Id + "\">" + unitsSlected[i].Name + "</option>"
            }
        }
        $("#unit_selected").append(html);
        $("#unit_selected").not('.multi-select').select2();

    } else {
        $("#unit_selected").not('.multi-select').select2();
    }
    unitOnchange();
}
function traderGroupOnChange() {
    var traderItemId = $('#tradergroupitem').val();

    if (traderItemId != "") {
        $("#manu_item_product").empty();
        $("#manu_item_product").load("/Manufacturing/GetTraderItem?itemId=" + traderItemId + "&locationid=" + $('#local-manage-select').val(), function () {
            if ($('#tradergroupitem').val() === null || $('#tradergroupitem').val() === "") {
                $('.item-info').hide();
            } else $('.item-info').show();

            loadUnit();
        });
    } else {
        $('.item-info').hide();
    }
    $('#tradergroupitem-error').remove();
}
function WorkGroupSelectedChange() {

    $workgroupId = $('#trader_workgroup_select').val();

    showTraderGroupItems();
    if ($workgroupId !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                LoadingOverlayEnd();
                $('.preview-workgroup').show();
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
                $('#trader_workgroup_select-error').remove();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $('.preview-workgroup').hide();
    }
};
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $('#trader_workgroup_select').val());
    $('#app-trader-workgroup-preview').modal('toggle');
}
function validateForm() {
    debugger;
    var valid = true;
    $('#manu_quantity-error').remove();
    $('#unit_selected-error').remove();
    $('#tradergroupitem-error').remove();
    $('#trader_workgroup_select-error').remove();
    $('#manu_quantity-error').remove();

    if ($("#unit_selected").val() == null || $("#unit_selected").val() === "") {
        valid = false;
        $("#form_item").validate().showErrors({ unit_select: "Unit is required." });
    }

    if ($("#tradergroupitem").val() == null || $("#tradergroupitem").val() === "") {
        valid = false;
        $("#form_item").validate().showErrors({ item_select: "Item is required." });
    }

    if ($("#trader_workgroup_select").val() === null || $("#trader_workgroup_select").val() === "") {
        valid = false;
        $("#form_workgroup").validate().showErrors({ workgroup: "Work group is required." });
    }

    if ($('#manu_quantity').val() == null || $('#manu_quantity').val() == '' || $('#manu_quantity').val().length > 15) {
        valid = false;
        $("#form_item").validate().showErrors({ manu: "Please enter a quantity to assemble." });
    }
    
    return valid;
}


function saveManuJob() {

    if (validateForm()) {
        $.LoadingOverlay("show");
        var recipe_id = $('#manujob_recipe_id').val();
        var trs = $('#table_recipe_ingredients tbody tr');
        var tds = $('#table_recipe_ingredients tbody tr td');
        if (recipe_id == "0" || recipe_id == "") {
            cleanBookNotification.error(_L("ERROR_MSG_633"), "Qbicles");
            return false;
        }
        var Reference = {
            Id: $('#reference_id').val(),
            NumericPart: parseFloat($('#refedit').text()),
            Type: $('#reference_type').val(),
            Prefix: $('#reference_prefix').val(),
            Suffix: $('#reference_suffix').val(),
            Delimeter: $('#reference_delimeter').val(),
            FullRef: $('#reference_fullref').val()
        }
        var manu = {
            Id: $('#tradersale_form_id').val(),
            AssemblyUnit: { Id: $('#unit_selected').val() },
            WorkGroup: { Id: $('#trader_workgroup_select').val() },
            Product: { Id: $('#tradergroupitem').val() },
            Quantity: $('#manu_quantity').val(),
            Location: { Id: $('#local-manage-select').val() },
            Status: "Pending",
            SelectedRecipe: { Id: $('#manu_recipe').val() },
            Reference: Reference
        }
        $.ajax({
            type: 'post',
            url: '/Manufacturing/SaveManujob',
            data: { manujob: manu },
            dataType: 'json',
            success: function (response) {
                LoadingOverlayEnd();
                if (response.actionVal === 1) {
                    $('#app-trader-inventory-manufacturing').modal('toggle');
                    cleanBookNotification.createSuccess();
                    showTableManujob();
                } else if (response.actionVal === 2) {
                    $('#app-trader-inventory-manufacturing').modal('toggle');
                    cleanBookNotification.updateSuccess();
                    showTableManujob();
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

}

// show table
$(function () {

    ShowTableManufacturingValue();
});
function ShowTableManufacturingValue() {
    $('#manufacring_content').LoadingOverlay("show");
    $('#manufacring_content').load("/Manufacturing/GetManufacturingTab", function () {
        $('#manufacring_content').LoadingOverlay("hide");
    });
}

function ManufacturingHistoryViewer(id) {
    var ajaxUri = '/Manufacturing/ManufacturingHistoryViewer?id=' + id;
    AjaxElementShowModal(ajaxUri, 'compound-item-history');
    return;
    ////compound-item-history
    //if (!id) id = 0;
    //$('#app-trader-inventory-recipe-view').LoadingOverlay("show");
    //$('#app-trader-inventory-recipe-view').empty();
    //$('#app-trader-inventory-recipe-view').load("/Manufacturing/ManujobViewer?id=" + id, function () {
    //    $('#app-trader-inventory-recipe-view').LoadingOverlay("hide");
    //});
}