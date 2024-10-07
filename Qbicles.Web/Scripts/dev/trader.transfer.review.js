var $traderSaleId = $("#TraderSaleId").val();
var $transferId = $('#transfer_id').val();
var pageType = "purchase";

//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    //CheckStatusApproval(apprKey).then(function (res) {

    $('#btn-transfer-approval').prop('disabled', true);
    $("#confirm-button-approval").hide();

    //if (res.result && res.Object.toLocaleLowerCase() == statusOld.toLocaleLowerCase()) {

    // apply 
    //$.LoadingOverlay("show");
    $.ajax({
        //url: "/Qbicles/SetRequestStatusForApprovalRequest",
        url: "/Qbicles/SetPurchaseTransferApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: $("#action_approval").val() },
        success: function (rs) {
            if (rs.actionVal > 0) {
                $("#confirm-button-approval").hide();
                location.reload();
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
    // and apply
    //} else {
    //    $("#confirm-button-approval").hide();
    //    cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
    //    location.reload();
    //}
    //});
};

// transfer manage
var $editMode = "Sale";
var $transferRequirement = "";


// trader transfer
function ShowEditTransfer(locationId, id) {
    $.LoadingOverlay("show");
    CheckStatus(id, 'Transfer').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && (res.Object == "Pending" || res.Object == "PendingPickup")) {

            var ajaxUri = "/TraderTransfers/TraderTransferAddEdit?locationId=" + locationId + "&id=" + id + '&onPage=manage';
            $.LoadingOverlay("show");
            $('#app-trader-edit-items').empty();
            $('#app-trader-edit-items').load(ajaxUri, function () {
                LoadingOverlayEnd();
            });
        } else if (res.result && res.Object != "Pending" && res.Object != "PendingPickup") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
            $('#app-trader-edit-items').modal('hide');
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");

        }
    });
};
function ChangeSelectedUnit() {
    if ($("#item-select").val() === null || $("#item-select").val() === "") {
        resetFormTransfer();
        return;
    }
    $("#item_selected").empty();
    $("#item_selected").load("/TraderTransfers/UnitsSelectByItem?idTraderItem=" +
        $("#item-select").val().split(":")[0]);
};
function InitMessageLocation() {
    if ($("#tranfer_form_id").val() === "0") {
        if ($("#transfer_type_add").val() === "#outbound") {
            $("#span-from").text($("#local-manage-select option:selected").text());
            $("#span-to").text($("#in-out-location option:selected").text());
        } else if ($("#transfer_type_add").val() === "#inbound") {
            $("#span-to").text($("#local-manage-select option:selected").text());
            $("#span-from").text($("#in-out-location option:selected").text());
        }
    } else {
        $("#span-from").text($("#originating-location-select option:selected").text());
        $("#span-to").text($("#destination-location-select option:selected").text());
    }
    $("#p-confirm-message").removeClass("hidden");
};
function addRowTransferItem() {
    if (!$("#form_add_transaction").valid())
        return;
    var idBuild = UniqueId();
    var units = $("#item_selected select").val();
    checkQuantityTransItem($("#form_item_quantity"));
    var item = {
        Id: idBuild,
        TraderItem: {
            Id: $("#item-select").val().split(":")[0],
            ImageUri: $("#item-select option:selected").attr("itemimage"),
            Name: $("#item-select option:selected").text()
        },
        Quantity: parseFloat($("#form_item_quantity").val())
    };

    var clone = $("#tb_form_template tbody tr").clone();
    $(clone).addClass("tr_id_" + item.Id);
    // filter to table 
    $($(clone).find("td.row_image div")).attr("style",
        "background-image: url('" + $("#api_url").val() + item.TraderItem.ImageUri + "&size=T');");

    $($(clone).find("td.row_name")).text(item.TraderItem.Name);

    $($(clone).find("td.row_unit")).empty();
    var unitClone = $("#item_selected select").clone();
    $($(clone).find("td.row_unit")).append(unitClone);
    $($(clone).find("td.row_unit select")).val(units);
    $($(clone).find("td.row_unit select")).attr("onchange", "rowUnitChange('" + item.Id + "')");
    var elQuantity = $($(clone).find("td.row_quantity input"));
    elQuantity.val(item.Quantity);
    elQuantity.attr("onchange", "checkQuantityTransItem(this," + item.TraderItem.Id + ")");
    $($(clone).find("td.row_button button")).attr("onclick", "removeRowItem('" + item.Id + "')");
    $($(clone).find("td.row_button input.traderItem")).val($("#item-select").val());
    $($(clone).find("td.row_button input.row_id")).val(item.Id);
    $($(clone).find("td select")).not(".multi-select").select2();
    $("#tb_form_item tbody").append(clone);
    setTimeout(function () {
        $("#form_add_transaction")[0].reset();
        ($("#form_add_transaction").validate()).resetForm();
    },
        100);
    $("#item-select").val('').trigger('change');
    resetFormTransfer();
};
function checkQuantityTransItem(el, item) {
    var transfer_type = $("#transfer_type_add").val();
    if (transfer_type === "#outbound") {
        var locationId = $("#destination-location-select").val();
        var quantity = $(el).val();
        var valItem = $('#item-select').val();
        if (valItem && !item) {
            valItem = valItem.split(':')[0];
        } else {
            valItem = item;
        }
        $.ajax({
            type: "post",
            url: "/TraderTransfers/GetCurrentInventory",
            data: { locationId: locationId, itemId: valItem },
            dataType: "json",
            async: false,
            success: function (response) {
                if (response && response.currentInventory < quantity) {
                    cleanBookNotification.error(_L("ERROR_MSG_382", [response.currentInventory]), "Qbicles");
                    $(el).val(response.currentInventory);
                    $(el).focus();
                    return false;
                }
            }
        });
    }
}
function nextToItemsTransfer() {
    setTimeout(function () {
        $("#form_add_transaction")[0].reset();
        $("#form_add_transaction select").not(".multi-select").select2();
        InitTransferItemSelect2Ajax('item-select', $('#trader-group-select').val(), $('#originating-location-select').val());
    },
        100);
};
function nextToConfirm() {
    loadDataTableConfirm();
};
function loadDataTableConfirm() {
    $("#tb_confirm tbody").empty();
    var trs = $("#tb_form_item tbody tr");
    var trd = $("#tb_form_item tbody tr td");

    if (trs.length === 1 && trd.length === 1)
        return;

    for (var i = 0; i < trs.length; i++) {

        var strTrs = "<tr> <td>" + $($(trs[i]).find("td.row_image")).html() + "</td>";
        strTrs += "<td>" +
            $($(trs[i]).find("td.row_name")).text() +
            "</td> " +
            "<td>" +
            $($(trs[i]).find("td.row_unit select option:selected")).text() +
            "</td>";
        strTrs += " <td>" + $($(trs[i]).find("td.row_quantity input")).val() + "</td> ";
        strTrs += "</tr>";
        $("#tb_confirm tbody").append(strTrs);

    }
}
function removeRowItem(id) {
    $('#tb_form_item tbody tr.tr_id_' + id).remove();
};
function rowUnitChange(id) {
    var item = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_button input.traderItem').val().split(':');
    var val = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_unit select').val().split(':');
    if (val[1] === "BaseUnit") {
        $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_costperunit input').val(item[3]);
    } else if (val[1] === "Conversion Unit") {
        $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_costperunit input').val(parseFloat(item[3]) * parseFloat(val[3]));
    }
    updateCost(id);
};
function SaveTransfer(status) {

    var id = $("#tranfer_form_id").val();
    var valid = true;
    $.LoadingOverlay("show");
    CheckStatus(id, 'Transfer').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object != "PendingPickup") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            $('#btn_change_' + id).addClass('hidden');
            $('#app-trader-edit-items').modal('hide');

            valid = false;
            return;
        }
    });
    if (!valid) return;

    var transferItems = [];
    var trans = $("#tb_form_item tbody tr");
    if (trans.length > 0) {
        for (var i = 0; i < trans.length; i++) {
            var id = parseInt($($(trans[i]).find("td.row_button input.row_id")).val());
            var item =
            {
                Id: 0
            };
            if ($($(trans[i]).find("td.row_button input.traderItem")).val().split(":").length >= 1) {
                item.Id = $($(trans[i]).find("td.row_button input.traderItem")).val().split(":")[0];
            } else {
                item = null;
            }
            var elQuantity = $(trans[i]).find("td.row_quantity input");
            var tempChk = $(elQuantity).val();
            checkQuantityTransItem(elQuantity, item.Id);
            if (tempChk != $(elQuantity).val()) {
                $(elQuantity).focus();
                $('.admintabs a[href="#transfer-2"]').tab('show');
                return;
            }
            var unit = $($(trans[i]).find("td.row_unit select")).val().split("|");

            var tran = {
                Id: isNaN(id) ? 0 : id,
                TraderItem: item,
                QuantityAtPickup: $(elQuantity).val(),
                Unit: { Id: unit[1] }
            };
            transferItems.push(tran);
        }
    }
    if (transferItems.length == 0 && status == 'PendingPickup') {
        cleanBookNotification.error("Please add product item.", "Qbicles");
        $('.admintabs a[href="#transfer-2"]').tab('show');
        return;
    }
    $.LoadingOverlay("show");
    var $workgroup = {};
    var $originatingLocation = {};
    var $destinationLocation = {};
    $workgroup = {
        Id: $("#trader-group-select").val()
    };
    var transferId = $("#tranfer_form_id").val();
    if (transferId === "0") {
        if ($("#transfer_type_add").val() === "#inbound") {
            $originatingLocation = {
                Id: $("#local-manage-select").val()
            };
            $destinationLocation = {
                Id: $("#in-out-location").val()
            };
        } else if ($("#transfer_type_add").val() === "#outbound") {
            $destinationLocation = {
                Id: $("#local-manage-select").val()
            };
            $originatingLocation = {
                Id: $("#in-out-location").val()
            };
        }
    } else {
        $destinationLocation = {
            Id: $("#destination-location-select").val()
        };
        $originatingLocation = {
            Id: $("#originating-location-select").val()
        };
    }

    var transfer = {
        Id: transferId,
        TransferItems: transferItems,
        Status: status,
        OriginatingLocation: $originatingLocation,
        DestinationLocation: $destinationLocation,
        Workgroup: $workgroup
    };

    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $("#app-trader-purchase-transfer").modal("hide");
                cleanBookNotification.createSuccess();
                setTimeout(function () { window.location.reload(true); }, 500);

            } else if (response.actionVal === 2) {
                $("#app-trader-purchase-transfer").modal("hide");
                cleanBookNotification.updateSuccess();
                setTimeout(function () { window.location.reload(true); }, 500);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

// edit purchase
function SavePurchaseTransfer(status) {

    $.LoadingOverlay("show");
    var $workgroup = {};
    $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id == "" || $workgroup.Id == null) {
        cleanBookNotification.error(_L("ERROR_MSG_244"), "Qbicles");
        return false;
    }

    var Reference = {
        Id: $('#purchase-reference_id').val(),
        NumericPart: parseFloat($('#purchase-refedit').text()),
        Type: $('#purchase-reference_type').val(),
        Prefix: $('#purchase-reference_prefix').val(),
        Suffix: $('#purchase-reference_suffix').val(),
        Delimeter: $('#purchase-reference_delimeter').val(),
        FullRef: $('#purchase-reference_fullref').val()
    };
    var transfer = {
        Id: $('#transfer_model_id').val(),
        Status: status,
        Purchase: { Id: $('#purchase_model_id').val() },
        TransferItems: [],
        Workgroup: $workgroup,
        Reference: Reference
    };

    var tr = $('#transfer_purchase_table tbody tr');
    var td = $('#transfer_purchase_table tbody tr td');
    if (tr.length > 0 && td.length > 1) {
        for (var i = 0; i < tr.length; i++) {
            var unit = $($(tr[i]).find('select.transfer_td_tran_unit')).val().split("|");

            var transferItem = {
                Id: $($(tr[i]).find('input.transfer_td_id')).val(),
                TraderItem: { Id: $($(tr[i]).find('input.transfer_td_traderitem_id')).val() },
                Unit: { Id: unit[1] },
                QuantityAtPickup: $($(tr[i]).find('td.transfer_td_tran_quan input')).val(),
                TransactionItem: { Id: $($(tr[i]).find('input.purchaseitem_td_id')).val() }
            }
            transfer.TransferItems.push(transferItem);
        }
    }

    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            $('#app-trader-purchase-transfer').modal('toggle');
            LoadingOverlayEnd();
            if (response.actionVal === 2) {
                $("#app-trader-purchase-transfer").modal("hide");
                cleanBookNotification.updateSuccess();
                setTimeout(function () { window.location.reload(true); }, 500);
            }
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}
function ShowEditPurchaseTransfer(id, purchaseId) {

    var transferId = $('#transfer_id').val();
    $.LoadingOverlay("show");
    CheckStatus(transferId, 'Transfer').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && (res.Object == "Pending" || res.Object == "PendingPickup")) {
            // load form update
            $editMode = "Purchase";
            var ajaxUri = "/TraderTransfers/EditPurchaseTransfer?id=" + id + '&idPurchase=' + purchaseId + '&onPage=manage';
            $.LoadingOverlay("show");
            $('#app-trader-purchase-transfer').empty();
            $('#app-trader-purchase-transfer').load(ajaxUri, function () {
                LoadingOverlayEnd();
            });
            // load  form transfer

        } else if (res.result && res.Object != "Pending" && res.Object != "PendingPickup") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
            $('#app-trader-purchase-transfer').modal('hide');
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
        }
    });


};


// edit sale
function ShowEditSaleTransfer(key) {
    $editMode = "Sale";
    var ajaxUri = "/TraderTransfers/InitSaleTransfer?key=" + key;
    $.LoadingOverlay("show");
    $('#app-trader-sale-transfer').empty();
    $('#app-trader-sale-transfer').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};
function ShowEditSaleTransfer(id, saleKey) {
    $.LoadingOverlay("show");
    CheckStatus(id, 'Transfer').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && (res.Object === "Pending" || res.Object === "PendingPickup")) {

            $editMode = "Sale";
            var ajaxUri = "/TraderTransfers/EditSaleTransfer?id=" + id + '&keySale=' + saleKey + '&onPage=manage';
            $.LoadingOverlay("show");
            $('#app-trader-sale-transfer').empty();
            $('#app-trader-sale-transfer').load(ajaxUri, function () {
                LoadingOverlayEnd();
            });

        } else if (res.result && res.Object != "PendingPickup") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
            $('#app-trader-sale-transfer').modal('hide');
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM", [res.msg]), "Qbicles");

        }
    });

};
function EditSaleSendToPickup(status) {

    var $workgroup = {};
    $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id == "" || $workgroup.Id == null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return false;
    }
    var transfer = {
        Id: $('#transfer_model_id').val(),
        Status: status,
        Sale: { Key: $('#sale_model_key').val() },
        TransferItems: [],
        Workgroup: $workgroup
    };

    var tr = $('#transfer_sale_table tbody tr');
    var td = $('#transfer_sale_table tbody tr td');
    if (tr.length > 0 && td.length > 1) {
        for (var i = 0; i < tr.length; i++) {
            var unit = $($(tr[i]).find('select.transfer_td_tran_unit')).val().split("|");

            var transferItem = {
                Id: $($(tr[i]).find('input.transfer_td_id')).val(),
                TraderItem: { Id: $($(tr[i]).find('input.saleitem_td_traderitem_id')).val() },
                Unit: { Id: unit[1] },
                QuantityAtPickup: $($(tr[i]).find('td.transfer_td_tran_quan input')).val(),
                TransactionItem: { Id: $($(tr[i]).find('input.saleitem_td_id')).val() }
            }
            transfer.TransferItems.push(transferItem);
        }
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            $('#app-trader-sale-transfer').modal('toggle');
            LoadingOverlayEnd();
            if (response.actionVal === 2) {
                $("#app-trader-sale-transfer").modal("hide");
                cleanBookNotification.updateSuccess();
                setTimeout(function () { window.location.reload(true); }, 500);
            }
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });

}
// save transfer sale
function SendToPickup(status) {
    $.LoadingOverlay("show");
    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id == "" || $workgroup.Id == null) {
        cleanBookNotification.error(_L("ERROR_MSG_244"), "Qbicles");
        return false;
    }
    var Reference = {
        Id: $('#sale-reference_id').val(),
        NumericPart: parseFloat($('#sale-refedit').text()),
        Type: $('#sale-reference_type').val(),
        Prefix: $('#sale-reference_prefix').val(),
        Suffix: $('#sale-reference_suffix').val(),
        Delimeter: $('#sale-reference_delimeter').val(),
        FullRef: $('#sale-reference_fullref').val()
    }
    var transfer = {
        Id: $('#transfer_model_id').val(),
        Status: status,
        Sale: { Key: $('#sale_model_key').val() },
        TransferItems: [],
        Workgroup: $workgroup,
        Reference: Reference
    };

    var tr = $('#transfer_sale_table tbody tr');
    var td = $('#transfer_sale_table tbody tr td');
    if (tr.length > 0 && td.length > 1) {
        for (var i = 0; i < tr.length; i++) {
            var unit = $($(tr[i]).find('select.transfer_td_tran_unit')).val().split("|");

            var transferItem = {
                Id: $($(tr[i]).find('input.transfer_td_id')).val(),
                TraderItem: { Id: $($(tr[i]).find('input.saleitem_td_traderitem_id')).val() },
                Unit: { Id: unit[1] },
                QuantityAtPickup: $($(tr[i]).find('td.transfer_td_tran_quan input')).val(),
                TransactionItem: { Id: $($(tr[i]).find('input.saleitem_td_id')).val() }
            }
            transfer.TransferItems.push(transferItem);
        }
    }
    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            LoadingOverlayEnd();
            $("#app-trader-sale-transfer").modal("hide");
            if (response.actionVal === 2) {
                $("#app-trader-purchase-transfer").modal("hide");
                cleanBookNotification.updateSuccess();
                window.location.reload(true);
            } else if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                window.location.reload(true);
            }

        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}
//

//function SelectedChangeBu(id) {
//    SetTransferCost(id);
//};
//function SetTransferCost(id) {
//    if ($editMode == "Sale") {
//        SetTransferCostSale(id);
//    } else if ($editMode == "Purchase") {
//        SetTransferCostPurchase(id);
//    }
//}

function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    var id = $("#transfer-workgroup-select").val();
    if (id !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + id,
            dataType: "json",
            success: function (response) {
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
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    } else {
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};

function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#transfer-workgroup-select").val());
};


validateItemsInTable = function () {
    var trans = $("#tb_form_item tbody tr");

    if (trans === null || trans.length === 0) {
        $(".btnNextConfirm").attr("Disabled", "Disabled");
    } else {
        $(".btnNextConfirm").removeAttr("Disabled");
    }
};


function nextToConfirmP2PEdit() {
    //InitMessageLocation
    if ($("#tranfer_form_id").val() === "0") {
        if ($("#transfer_type_add").val() === "#outbound") {
            $("#span-from").text($("#local-manage-select option:selected").text());
            $("#span-to").text($("#in-out-location option:selected").text());
        } else if ($("#transfer_type_add").val() === "#inbound") {
            $("#span-to").text($("#local-manage-select option:selected").text());
            $("#span-from").text($("#in-out-location option:selected").text());
        }
    } else {
        $("#span-from").text($("#originating-location-select option:selected").text());
        $("#span-to").text($("#destination-location-select option:selected").text());
    }
    $("#p-confirm-message").removeClass("hidden");

    //loadDataTableConfirm
    $("#div-confirm").empty();
    var strTable =
        "<table id='tb_confirm' class='datatable table-hover' style='width: 100%; background: #fff;' data-order='[[1, \"asc\"]]'>";
    strTable += "<thead><tr><th data-orderable='false'></th><th>Name</th><th>Unit</th><th>Quantity</th></tr></thead><tbody>";
    var trs = $("#tb_form_item tbody tr");
    var trd = $("#tb_form_item tbody tr td");

    if (trs.length === 1 && trd.length === 1)
        return;

    for (var i = 0; i < trs.length; i++) {

        strTable += "<tr> <td>" + $($(trs[i]).find("td.row_image")).html() + "</td>";
        strTable += "<td>" + $($(trs[i]).find("td.row_name")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.row_unit select option:selected")).text() + "</td>";
        strTable += " <td>" + $($(trs[i]).find("td.row_quantity input")).val() + "</td> ";
        strTable += "</tr>";
    };
    strTable += "</tbody></table>";

    $("#div-confirm").append(strTable);
    $("#tb_confirm").DataTable({
        responsive: true,
        "lengthChange": true,
        "pageLength": 10,
        "columnDefs": [
            {
                "targets": 3,
                "orderable": false
            }
        ],
        "order": []
    });
};

function resetPurchaseForm() {
    $('#cpu').attr('disabled', true);
    $('#addNowForm').attr('disabled', true);
    $('#label_cost').text('Cost per <selected unit>');
}

function InitTransferItemSelect2Ajax(selectId, select2WorkGroupId, select2LocationId = 0) {
    // Init Select 2
    if (select2WorkGroupId == null) {
        select2WorkGroupId = $("select[name=workgroup]").val();
    }
    if (select2WorkGroupId == null) {
        select2WorkGroupId = 0;
    }
    var select2AjaxUrl = '/Select2Data/GetPoint2PointTransferItemsByWorkgroup?workGroupId=' + select2WorkGroupId + "&locationId=" + select2LocationId;
    //Get default set of options for select2
    var $defaultResults = $("#" + selectId + " option:not([selected])");
    var defaultResults = [];
    $defaultResults.each(function () {
        var $option = $(this);
        defaultResults.push({
            id: $option.attr('value'),
            text: $option.text()
        });
    });
    //Initialize select2 object
    var parameters = {};
    $("#" + selectId).not('.multi-select').select2({
        ajax: {
            url: select2AjaxUrl,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                parameters['keySearch'] = params.term;
                return parameters;
            },
            cache: true,
            processResults: function (data) {
                var lstData = [];
                lstData = lstData.concat(data.Object);
                data.Object.forEach(function (item) {
                    lstData.push({
                        "text": item.Name,
                        "itemId": item.Id,
                        "itemName": item.Name,
                        "itemImage": item.ImageUri,
                        "taxName": item.TaxRateName,
                        "taxRate": item.TaxRateValue,
                        "costUnit": item.CostUnit,
                        "value": item.Id,
                        "id": item.Id,
                        "newTag": true
                    });
                })
                return {
                    results: lstData
                };
            }
        },
        //minimumInputLength: 1,
        defaultResults: defaultResults,
    }).on('select2:selecting', function (e) {
        var data = e.params.args.data;
        var htmlStr = "<option itemId='" + data["itemId"] + "' selected value='" + data["itemId"] + "' itemName='"
            + data["itemName"] + "' itemImage='" + data["itemImage"] + "' itemUnit=''>" + data["text"] + "</option>";
        $("#" + selectId).html(htmlStr);
        e.preventDefault();
        $("#" + selectId).select2('close').trigger('change');
    }).val(0).trigger('change');
}

function EditTransferChangeGroup() {
    InitTransferItemSelect2Ajax('item-select', $('#trader-group-select').val(), $('#originating-location-select').val());
}
