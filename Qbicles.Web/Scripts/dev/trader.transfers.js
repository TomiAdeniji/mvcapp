var $locationManage = $("#local-manage-select").val();

var $transferRequirement = "";
var $workgroupId = 0;
var $editMode = "";
//------------id selected ---------
var $pointToPointId = 0,
    $saleKey = "",
    $purchaseId = 0;
var filter = {
    Workgroup: "",
    Key: ""
}
function onSelectWorkgroup(ev) {
    filter.Workgroup = $(ev).val();
    //setTimeout(function () { searchOnTableTransfer(); }, 200);
    CallBackFilterDataTransferServeSide();
}
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    //setTimeout(function () { searchOnTableTransfer(); }, 200);
    CallBackFilterDataTransferServeSide();
}
function searchOnTableTransfer() {
    var listKey = [];
    if ($('#filter-group').val() !== "" && $('#filter-group').val() !== null) {
        listKey.push($('#filter-group').val());
    }
    var keys = $('#search_transfer').val().split(' ');
    if ($('#search_transfer').val() !== "" && $('#search_transfer').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#trader-transfer-list").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#trader-transfer-list").val("");
}

EnableNextButton = function () {
    if ($workgroupId !== "")
        if ($saleKey != "" || $purchaseId > 0 || $pointToPointId > 0)
            $(".btnNext").removeAttr("Disabled");
};
DisableNextButton = function () {
    $(".btnNext").attr("Disabled", "Disabled");
};

//------------ filter -----------------------
//



//-----------End filter -------------

// ----------- workgroup ---------
function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    $workgroupId = $("#transfer-workgroup-select").val();
    if ($workgroupId !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
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
                EnableNextButton();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        })
    } else {
        DisableNextButton();
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('show');
}
// ------- end workgroup -------------

// add transsfer -------------
function AddTransferAll() {
    $("#app-trader-purchase-transfer").empty();
    $("#app-trader-sale-transfer").empty();
    var ajaxUri = "/TraderTransfers/TraderTransferAddEditPartial?id=0";
    $.LoadingOverlay("show");

    $('#app-trader-transfer-modal').empty();
    $('#app-trader-transfer-modal').load(ajaxUri, function () {
        $("#app-trader-transfer-modal").modal("show");
        initSaleTransfer();
        initPurchaseTransfer();
        LoadingOverlayEnd();
    });
};
// ----------------------------------------------------- Specifics Tab ------------------------------------
//--- Transfer requirement selected ---------------
ChangeTransferRequest = function () {
    var method = $("#transfer_request").val();
    $transferRequirement = method;

    $saleKey = "";
    $purchaseId = 0;
    $pointToPointId = 0;

    $('#point-to-point').empty();
    //$('#purchase-transfer').empty();
    //$('#sale-transfer').empty();
    $("#point-to-point-type").hide();
    DisableNextButton();

    if (method === "p2p") {
        $('#sale-transfer').hide();
        $('#purchase-transfer').hide();
        $saleKey = "";
        $purchaseId = 0;
        $pointToPointId = 1;
        $("#point-to-point-type").show();
        TransferAddEditPointToPointSpecificsTab();
        EnableNextButton();
    }

    if (method === "goods_in") {
        $('#sale-transfer').hide();
        $('#purchase-transfer').show();
        $('.search_transfer-purchase').val('');
        tablePurchaseLoad();
        //TransferAddEditPurchaseSpecificsTab();
    }

    if (method === "goods_out") {
        //TransferAddEditSaleSpecificsTab();
        $('#sale-transfer').show();
        $('#purchase-transfer').hide();
        $(".search_transfer-sale").val('');
        tableSaleLoad();
    }
};
function tableSaleLoad() {
    var $salesTable = $('#sale-list-table');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#sale-list-table')) {
        $salesTable.DataTable().destroy();
    }
    $salesTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $.LoadingOverlay("show");
        } else {
            $.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[2, "desc"]],
        ajax: {
            "url": "/TraderTransfers/GetByLocationPagination",
            "data": function (d) {
                return $.extend({}, d, {
                    "daterange": $("#search_date_range").val()
                });
            }
        },
        columns: [
            { "title": "#", "data": "FullRef", "searchable": true, "orderable": true },
            { "title": "Workgroup", "data": "WorkgroupName", "searchable": true, "orderable": true, "className": "workgroup" },
            { "title": "Created date", "data": "CreatedDate", "searchable": true, "orderable": true, "className": "createdDate" },
            { "title": "Contact", "data": "Contact", "searchable": true, "orderable": true, "className": "contact" },
            { "title": "Dimensions", "data": "Dimensions", "searchable": false, "orderable": false, "className": "dimension" },
            { "data": "SaleTotal", "searchable": true, "orderable": false, "className": "total" },
            { "title": "Active transfers", "data": "TransferCount", "searchable": true, "orderable": false, "className": "count" },
            { "title": "Use", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 0,
            "data": "FullRef",
            "render": function (data, type, row, meta) {
                return '#<span class="saleid hidden">' + row.Id + '</span><span class="salekey hidden">' + row.Key + '</span><span>' + data + '</span>';
            }
        },
        {
            "targets": 3,
            "data": "Contact",
            "render": function (data, type, row, meta) {
                return '<a href="/Trader/AppTrader" onclick="setTabTrader("contacts")">' + data + '</a>';
            }
        },
        {
            "targets": 7,
            "data": "Key",
            "render": function (data, type, row, meta) {
                return '<button class="btn btn-success" onclick="SelectSale(\'' + row.Key + '\');"><i class="fa fa-check"></i></button>';
            }
        }],
        rowId: function (a) {
            return 'tr_sale_' + a.Key;
        },
    });
    $('#sale-table_filter').hide();
}
function tablePurchaseLoad() {
    var $purchasesTable = $('#purchase-list-table');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#purchase-list-table')) {
        $purchasesTable.DataTable().destroy();
    }
    $purchasesTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $.LoadingOverlay("show");
        } else {
            $.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[2, "desc"]],
        ajax: {
            "url": "/TraderTransfers/GetByLocationPurchasePagination",
            "data": function (d) {
                return $.extend({}, d, {
                    "daterange": $("#search_purchase_date_range").val()
                });
            }
        },
        columns: [
            { "title": "#", "data": "FullRef", "searchable": true, "orderable": true },
            { "title": "Workgroup", "data": "WorkgroupName", "searchable": true, "orderable": true, "className": "workgroup" },
            { "title": "Created date", "data": "CreatedDate", "searchable": true, "orderable": true, "className": "createdDate" },
            { "title": "Contact", "data": "Contact", "searchable": true, "orderable": true, "className": "contact" },
            { "title": "Dimensions", "data": "Dimensions", "searchable": false, "orderable": false, "className": "dimension" },
            { "data": "SaleTotal", "searchable": true, "orderable": false, "className": "total" },
            { "title": "Active transfers", "data": "TransferCount", "searchable": true, "orderable": false, "className": "count" },
            { "title": "Use", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 0,
            "data": "FullRef",
            "render": function (data, type, row, meta) {
                return '#<span class="purchaseid hidden">' + row.Id + '</span><span>' + data + '</span>';
            }
        },
        {
            "targets": 3,
            "data": "Contact",
            "render": function (data, type, row, meta) {
                return '<a href="/Trader/AppTrader" onclick="setTabTrader("contacts")">' + data + '</a>';
            }
        },
        {
            "targets": 7,
            "data": "Id",
            "render": function (data, type, row, meta) {
                return '<button class="btn btn-success" onclick="SelectPurchase(' + data + ');"><i class="fa fa-check"></i></button>';
            }
        }],
        rowId: function (a) {
            return 'tr_purchase_' + a.Id;
        },
    });
    $('#purchase-table_filter').hide();
}
// ----------- point to point change type In-Out
TransferTypeChange = function () {
    var method = $("#transfer_type_add").val();
    $("#inbound").hide();
    $("#outbound").hide();
    $(method).toggle();

    InitMessageLocation();
};

LocationInOuChange = function () {
    $("#location-in-out-selected").empty();
    var locationId = $("#in-out-location").val();
    if (locationId && locationId > 0)
        $.ajax({
            type: 'post',
            url: '/TraderTransfers/LocationSelectToHtml',
            data: { id: locationId },
            dataType: 'json',
            success: function (response) {
                $("#location-in-out-selected").append(response.msg);
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {

        });
    //location-in-out-selected
};

InitMessageLocation = function () {

    if ($transferRequirement === "p2p" || $editMode !== "") {
        $("#confirm-tranfer-type").text($("#transfer_type_add :selected").text());
        $("#confirm-for").text("Point to Point");
    }
    else if ($transferRequirement === "goods_in") {
        $("#confirm-tranfer-type").text("Purchase");
        $("#confirm-for").text("Purchase #" + $purchaseId);
    }
    else if ($transferRequirement === "goods_out") {
        $("#confirm-tranfer-type").text("Sale");
        $("#confirm-for").text("Sale #" + $saleKey);
    }

    $("#confirm-tranfer-requirement").text($transferRequirement.replace("p2p", "Transfer Point to point").replace("goods_in", "Purchase transfer").replace("goods_out", "Sale transfer"));

    $("#contact-source").empty();
    $("#contact-destination").empty();
    if ($transferRequirement === "p2p" || $editMode !== "") {
        if ($("#transfer_type_add").val() === "#inbound") {
            $("#source-destination-manage").text("Destination");
            $("#source-destination-select").text("Source");
            $("#location-in-out-selected").clone().removeAttr("id").appendTo("#contact-source");
            $("#location-manage-confirm").clone().removeAttr("id").appendTo("#contact-destination");

        } else if ($("#transfer_type_add").val() === "#outbound") {
            $("#source-destination-manage").text("Source");
            $("#source-destination-select").text("Destination");

            $("#location-in-out-selected").clone().removeAttr("id").appendTo("#contact-destination");
            $("#location-manage-confirm").clone().removeAttr("id").appendTo("#contact-source");
        }
    }
    else if ($transferRequirement === "goods_in") {
        //purchase
        $("#source-destination-manage").text("Destination");
        $("#location-manage-confirm").clone().appendTo("#contact-destination");
        $("#contact-confirm").clone().appendTo("#contact-source");
    }
    else if ($transferRequirement === "goods_out") {
        //sale
        $("#source-destination-manage").text("Source");
        $("#location-manage-confirm").clone().appendTo("#contact-source");
        $("#contact-confirm").clone().appendTo("#contact-destination");
    }

};

function TransferAddEditPointToPointSpecificsTab() {
    var ajaxUri = "/TraderTransfers/TransferAddEditPointToPointSpecificsTab?locationId=" + $("#local-manage-select").val();
    $.LoadingOverlay("show");

    $('#point-to-point').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};

SelectPurchase = function (id) {
    $saleKey = ""; $purchaseId = id; $pointToPointId = 0;
    $("#purchase_model_id").val(id);
    $("#p_transfer_id").text($("#tr_purchase_" + id + " td span.purchaseid").text());
    $(".p_workgroup").text($("#tr_purchase_" + id + " td.workgroup").text());
    $(".p_createdDate").text($("#tr_purchase_" + id + " td.createdDate").text());
    $(".p_contact").empty();
    $(".p_contact").append($("#tr_purchase_" + id + " td.contact a").clone());
    $(".p_dimension").text($(".tr_purchase_" + id + " td.dimension").text());
    $(".p_total").text($("#tr_purchase_" + id + " td.total").text());
    $(".p_tranfer").text($("#tr_purchase_" + id + " td.count").text());
    $('.purchase-select').hide(); $('.selected-purchase').show();
    EnableNextButton();
};

UnSelectPurchase = function () {
    $("#purchase_model_id").val(0);
    $saleKey = ""; $purchaseId = 0; $pointToPointId = 0;
    $('.selected-purchase').hide();
    $('.purchase-select').show();
    DisableNextButton();
};

SelectSale = function (key) {
    $saleKey = key; $purchaseId = 0; $pointToPointId = 0;
    $("#sale_model_key").val(key);
    $("#s_sale_id").text($("#tr_sale_" + key + " td span.saleid").text());
    $(".s_workgroup").text($("#tr_sale_" + key + " td.workgroup").text());
    $(".s_createdDate").text($("#tr_purchase_" + key + " td.createdDate").text());
    $(".s_contact").empty();
    $(".s_contact").append($("#tr_sale_" + key + " td.contact a").clone());

    $(".s_dimension").text($("#tr_sale_" + key + " td.dimension").text());
    $(".s_total").text($("#tr_sale_" + key + " td.total").text());
    $(".s_tranfer").text($("#tr_sale_" + key + " td.count").text());
    $('.sale-select').hide(); $('.selected-sale').show();
    EnableNextButton();
};

UnSelectSale = function () {
    $("#sale_model_key").val("");
    $saleKey = ""; $purchaseId = 0; $pointToPointId = 0;
    $('.selected-sale').hide();
    $('.sale-select').show();
    DisableNextButton();
};


// ---------------------------------------------------------------------- Items Tab -------------------------------------------

function NextToItemsTab() {
    $.LoadingOverlay("show");
    $('#route_p2p').empty();
    $('#route_goods').empty();
    if (!$('#transfer-workgroup-select').val()) {
        setTimeout(function () {
            $("#form_transferadd").validate().showErrors({ workgourptransfer: "Workgroup is required" });
            $('.admintabs a[href="#transfer-specifics"]').tab('show');
            LoadingOverlayEnd();
        }, 500);
        return false;
    }
    var ajaxUri = "";
    if ($pointToPointId > 0) {
        var locationId = 0;
        var tranferType = $("#transfer_type_add").val();
        if (tranferType === "#inbound") {
            locationId = $("#in-out-location").val();
        } else {
            locationId = $("#local-manage-select").val();
        }
        ajaxUri = "/TraderTransfers/TransferAddEditPointToPointItemsTab?id=" + locationId;

    } else if ($purchaseId > 0) {
        ajaxUri = "/TraderTransfers/TransferAddEditPurchaseItemsTab?purchaseId=" + $purchaseId;

    } else if ($saleKey != "") {
        ajaxUri = "/TraderTransfers/TransferAddEditSaleItemsTab?saleKey=" + $saleKey;
    }
    $('#route_items').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};

function nextToItemsTransfer() {
    setTimeout(function () {
        $("#form_add_transaction")[0].reset();
        $("#form_add_transaction select").not(".multi-select").select2({ placeholder: "Please select" });
        InitTransferItemSelect2Ajax('item-select', $('#trader-group-select').val(), $('#originating-location-select').val());
    }, 100);
};


// ------ item tab P2p ------------

validateItemsInTable = function () {
    var trans = $("#tb_form_item tbody tr");

    if (trans === null || trans.length === 0) {
        $(".btnNextConfirm").attr("Disabled", "Disabled");
    } else {
        $(".btnNextConfirm").removeAttr("Disabled");
    }
};

function ChangeSelectedUnit() {
    if ($("#item-select").val() === null || $("#item-select").val() === "") {
        resetFormTransfer();
        return;
    }
    $("#item_selected").empty();
    $("#item_selected").load("/TraderTransfers/UnitsSelectByItem?idLocation=" + $("#local-manage-select").val() + "&idTraderItem=" + $("#item-select").val().split(":")[0]);

};

function removeRowItem(id) {
    $("#tb_form_item tbody tr.tr_id_" + id).remove();
    validateItemsInTable();
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
        Quantity: parseFloat($("#form_item_quantity").val()),
        Fee: parseFloat($("#item_fee").val())
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
    //$($(clone).find("td.row_unit select")).attr("onchange", "rowUnitChange('" + item.Id + "')");
    var elQuantity = $($(clone).find("td.row_quantity input"));
    elQuantity.val(item.Quantity);
    elQuantity.attr("onchange", "checkQuantityTransItem(this," + item.TraderItem.Id + ")");

    var elFee = $($(clone).find("td.row_fee input"));
    elFee.val(item.Fee);

    $($(clone).find("td.row_button button")).attr("onclick", "removeRowItem('" + item.Id + "')");
    $($(clone).find("td.row_button input.traderItem")).val($("#item-select").val());
    $($(clone).find("td.row_button input.row_id")).val(item.Id);
    $($(clone).find("td select")).not(".multi-select").select2();
    $("#tb_form_item tbody").append(clone);
    validateItemsInTable();
    resetFormTransfer();
    $("#item-select").val('').trigger('change');
};

function checkQuantityTransItem(el, item) {
    var transferType = $("#transfer_type_add").val();

    if (transferType === "#outbound") {
        var locationId = $("#local-manage-select").val();
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
                    LoadingOverlayEnd();
                    return false;
                }
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }
};

// ---------- end items tab P2P -------

// ------------- items tab Sale ------------------


// -------------end items tab Sale ------------------


// ------------- items tab Purchase ------------------

// -------------end items tab Purchase ------------------




// ----------------------------------------------- confirm tab -----------------------------------------------------------------------

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
    ConfirmPointToPointTab();
    InitTableJs();
    return;
};

function nextToConfirm() {
    $("#div-confirm").empty();
    InitMessageLocation();
    loadDataTableConfirm();
};

function loadDataTableConfirm() {

    if ($transferRequirement === "p2p" || $editMode !== "")
        ConfirmPointToPointTab();
    else if ($transferRequirement === "goods_in")
        ConfirmPurchaseTab();
    else if ($transferRequirement === "goods_out")
        ConfirmSaleTab();

    InitTableJs();

};

ConfirmPointToPointTab = function () {

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
};

ConfirmSaleTab = function () {

    var strTable = "<table id='tb_confirm' class='datatable table-hover' style='width: 100%; background: #fff;' data-order='[[1, \"asc\"]]'>";
    strTable += "<thead><tr><th data-orderable='false'></th><th>Name</th><th>Unit</th><th>Qty</th><th>Transfer unit</th><th>Transfer qty</th><th>Remaining</th></tr></thead><tbody>";
    var trs = $("#transfer_sale_table tbody tr");
    var trd = $("#transfer_sale_table tbody tr td");

    if (trs.length === 1 && trd.length === 1)
        return;

    for (var i = 0; i < trs.length; i++) {

        strTable += "<tr> <td>" + $($(trs[i]).find("td.row_image")).html() + "</td>";
        strTable += "<td>" + $($(trs[i]).find("td.row_name")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.transfer_td_sale_unit_name")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.transfer_td_sale_quan")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.row_unit select option:selected")).text() + "</td>";
        strTable += " <td>" + $($(trs[i]).find("td.transfer_td_tran_quan input")).val() + "</td> ";
        strTable += " <td>" + $($(trs[i]).find("td.remainder span")).text() + "</td> ";
        strTable += "</tr>";
    }
    strTable += "</tbody></table>";

    $("#div-confirm").append(strTable);
};

ConfirmPurchaseTab = function () {

    var strTable =
        "<table id='tb_confirm' class='datatable table-hover tb_confirm' style='width: 100%; background: #fff;' data-order='[[1, \"asc\"]]'>";
    strTable +=
        "<thead><tr><th data-orderable='false'></th><th>Name</th><th>Unit</th><th>Qty</th><th>Transfer unit</th><th>Transfer qty</th><th>Remaining</th></tr></thead><tbody>";
    var trs = $("#transfer_purchase_table tbody tr");
    var trd = $("#transfer_purchase_table tbody tr td");

    if (trs.length === 1 && trd.length === 1)
        return;

    for (var i = 0; i < trs.length; i++) {

        strTable += "<tr> <td>" + $($(trs[i]).find("td.row_image")).html() + "</td>";
        strTable += "<td>" + $($(trs[i]).find("td.row_name")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.transfer_td_sale_unit_name")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.transfer_td_sale_quan")).text() + "</td> ";
        strTable += "<td>" + $($(trs[i]).find("td.row_unit select option:selected")).text() + "</td>";
        strTable += " <td>" + $($(trs[i]).find("td.transfer_td_tran_quan input")).val() + "</td> ";
        strTable += " <td>" + $($(trs[i]).find("td.remainder span")).text() + "</td> ";
        strTable += "</tr>";
    }
    strTable += "</tbody></table>";

    $("#div-confirm").append(strTable);
};

InitTableJs = function () {

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
    $('#cpu').attr('disabled', 'true');
    $('#addNowForm').attr('disabled', true);
}
function SendToPickup(status) {

    var $workgroup = {};
    $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return false;
    };
    var Reference = {
        Id: $('#sale-reference_id').val(),
        NumericPart: parseFloat($('#sale-refedit').text()),
        Type: $('#sale-reference_type').val(),
        Prefix: $('#sale-reference_prefix').val(),
        Suffix: $('#sale-reference_suffix').val(),
        Delimeter: $('#sale-reference_delimeter').val(),
        FullRef: $('#sale-reference_fullref').val()
    };

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
    $.LoadingOverlay("show");
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
                ShowTableTransferValue(true);
            } else if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                ShowTableTransferValue(true);
            }

        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

};
function SaveTransfer(status) {

    if ($purchaseId > 0) {
        SavePurchaseTransfer(status);
        return;
    }
    else if ($saleKey != "") {
        SaveSaleTransfer(status);
        return;
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
    $.LoadingOverlay("show");
    var transferItems = [];
    var trans = $("#tb_form_item tbody tr");

    if (trans === null || trans.length === 0) {
        LoadingOverlayEnd();
        cleanBookNotification.error(_L("ERROR_MSG_654"), "Qbicles");
        $('.admintabs a[href="#transfer-item-tab"]').tab('show');
        return;
    }

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
            if (tempChk !== $(elQuantity).val()) {
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

    var $workgroup = {};
    var $originatingLocation = {};
    var $destinationLocation = {};
    $workgroup = {
        Id: $workgroupId
    };
    var transferId = $("#tranfer_form_id").val();
    if (transferId === "0") {
        if ($("#transfer_type_add").val() === "#outbound") {
            $originatingLocation = {
                Id: $("#local-manage-select").val()
            };
            $destinationLocation = {
                Id: $("#in-out-location").val()
            };
        } else if ($("#transfer_type_add").val() === "#inbound") {
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
        Workgroup: $workgroup,
        Reference: Reference
    };

    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $("#app-trader-transfer-modal").modal("hide");
                cleanBookNotification.createSuccess();
                $("#div-confirm").empty();
                ShowTableTransferValue(true);
            } else if (response.actionVal === 2) {
                $("#app-trader-transfer-modal").modal("hide");
                cleanBookNotification.updateSuccess();
                $("#div-confirm").empty();
                ShowTableTransferValue(true);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
};

// ------------ End confirm tab -----------




//--------------- start edit transfer Point to point/ sale transfer/ purchase transfer --------------------
function EditTransfer(transferId) {
    $("#app-trader-purchase-transfer").empty();
    $("#app-trader-sale-transfer").empty();
    $('#app-trader-transfer-modal').empty();

    $("#app-trader-transfer-modal").load("/TraderTransfers/TraderTransferAddEdit?locationId=" + $("#local-manage-select").val() + "&id=" + transferId);
    $("#app-trader-transfer-modal").modal("show");

};

function ShowEditSaleTransfer(id, saleKey) {
    $.LoadingOverlay("show");
    $("#app-trader-purchase-transfer").empty();
    $("#app-trader-sale-transfer").empty();
    $('#app-trader-transfer-modal').empty();
    $("#div-confirm").empty();
    $editMode = "Sale";

    $('#app-trader-sale-transfer').load("/TraderTransfers/EditSaleTransfer?id=" + id + '&keySale=' + saleKey, function () {
        LoadingOverlayEnd();
        $('#app-trader-sale-transfer').modal('show');
    });

};

function ShowEditPurchaseTransfer(id, purchaseId) {
    $.LoadingOverlay("show");
    $("#app-trader-purchase-transfer").empty();
    $("#app-trader-sale-transfer").empty();
    $('#app-trader-transfer-modal').empty();
    $editMode = "Purchase";
    $('#app-trader-purchase-transfer').load("/TraderTransfers/EditPurchaseTransfer?id=" + id + '&idPurchase=' + purchaseId, function () {
        LoadingOverlayEnd();
        $('#app-trader-purchase-transfer').modal('show');
    });

};






// ------------------- end edit transfer ----------------------



// ------------ save to db --------------------------

function SaveSaleTransfer(status) {


    $.LoadingOverlay("show");
    var $workgroup = {
        Id: $workgroupId
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error("Work group is required", "Qbicles");
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
    };

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
            $("#app-trader-transfer-modal").modal("hide");
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
            } if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            }
            ShowTableTransferValue(true);
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

    return false;
};

function SavePurchaseTransfer(status) {

    $.LoadingOverlay("show");
    var $workgroup = {
        Id: $workgroupId
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error("Work group is required", "Qbicles");
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
                TraderItem: { Id: $($(tr[i]).find('input.purchaseitem_td_traderitem_id')).val() },
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

            $('#app-trader-transfer-modal').modal('hide');
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
            } if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            }
            ShowTableTransferValue(true);
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

    return false;
};

//------------------- edn save --------------
function clickAddNew(form) {
    ResetFormControl(form);
}

function ConfirmDeleteTransfer(id) {
    $("#id-transfer-delete").val(id);
}

DeleteTransfer = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/TraderTransfers/DeleteTransfer",
        data: { id: $("#id-transfer-delete").val() },
        dataType: "json",
        success: function (response) {
            if (response === "OK") {
                setTimeout(function () {
                    $("#transfers-content").load("/TraderTransfers/LoadTransfersLocation?locationId=" +
                        $("#local-manage-select").val());
                    LoadingOverlayEnd();
                    cleanBookNotification.removeSuccess();
                },
                    2000);

            } else if (response === "Fail") {
                LoadingOverlayEnd();
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

// show table
$(function () {
    ShowTableTransferValue();
});
function ShowTableTransferValue(isLoading) {
    if (!isLoading)
        $('#transfers-content').LoadingOverlay("show");
    var ajaxUri = "/TraderTransfers/ShowTransfersContent";
    $('#transfers-content').load(ajaxUri, function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#transfers-content table').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#transfers-content').LoadingOverlay("hide");
    });
}

function LoadTableDataTransfer(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
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
            "infoFiltered": "",
            "processing": loadingoverlay_value
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keysearch: $('#search_transfer').val(),
                    route: $('#subfilter_route').val(),
                    groupid: $('#filter-group').val(),
                    date: $('#filter_daterange').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "desc"]]
    });
}
function FilterDataTransferByServerSide() {
    var url = '/TraderTransfers/GetTransfersContent';
    var columns = [
        {
            name: "FullRef",
            data: "FullRef",
            orderable: false
        },
        {
            name: "WorkGroupName",
            data: "WorkGroupName",
            width: "150px",
            orderable: true
        },
        {
            name: "Route",
            data: "Route",
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                switch (row.Route) {
                    case "InBound":
                        str += "<span class=\"label label-lg label-primary\">Inbound</span>";
                        break;
                    case "OutBound":
                        str += "<span class=\"label label-lg label-info\">Outbound</span>";
                        break;
                    default:
                        str += "<span class=\"label label-lg label-info\">" + row.Route + "</span>";
                        break;
                }
                return str;
            }
        },
        {
            name: "From",
            data: "From",
            width: "200px",
            orderable: false
        },
        {
            name: "To",
            data: "To",
            width: "200px",
            orderable: false
        },
        {
            name: "Date",
            data: "Date",
            width: "150px",
            orderable: true
        },
        {
            name: "Reason",
            data: "Reason",
            width: "150px",
            orderable: true
        },
        {
            name: "Status",
            data: null,
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                switch (row.Status) {
                    case "Initiated":
                        str += "<span class=\"label label-lg label-info\">Initiated</span>";
                        break;
                    case "PendingPickup":
                        str += "<span class=\"label label-lg label-primary\">Pending Pickup</span>";
                        break;
                    case "PickedUp":
                        str += "<span class=\"label label-lg label-success\">Picked up</span>";
                        break;
                    case "Delivered":
                        str += "<span class=\"label label-lg label-success\">Delivered</span>";
                        break;
                    case "Draft":
                        str += "<span class=\"label label-lg label-primary\">Draft</span>";
                        break;
                    case "Denied":
                        str += "<span class=\"label label-lg label-danger\">Denied</span>";
                        break;
                    case "Discarded":
                        str += "<span class=\"label label-lg label-danger\">Discarded</span>";
                        break;
                }
                return str;
            }
        },
        {
            data: null,
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                if (row.Status === 'Draft') {
                    var hidden = " hidden";
                    if (row.AllowEdit) {
                        hidden = "";
                    }
                    if (!row.Sale && !row.Purchase) {
                        str += '<button class="btn btn-info' + hidden + '" onclick="EditTransfer(' + row.Id + ')"><i class="fa fa-truck"></i> &nbsp; Continue</button>';
                    } else if (row.Sale && !row.Purchase) {
                        str += '<button class="btn btn-info' + hidden + '" onclick="ShowEditSaleTransfer(' + row.Id + ', \'' + row.Sale.Key + '\')"><i class="fa fa-calculator"></i> &nbsp; Continue</button>';
                    } else if (!row.Sale && row.Purchase) {
                        str += '<button class="btn btn-info' + hidden + '" onclick="ShowEditPurchaseTransfer(' + row.Id + ', ' + row.Purchase.Id + ')"><i class="fa fa-calculator"></i> &nbsp; Continue</button>';
                    }
                } else {
                    str += '<button class="btn btn-primary" onclick="window.location.href=\'/TraderTransfers/TransferMaster?key=' + row.Key + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                }
                return str;
            }
        }
    ];
    LoadTableDataTransfer('trader-transfer-list', url, columns, 5);
    CallBackFilterDataTransferServeSide();
}

function CallBackFilterDataTransferServeSide() {
    $("#trader-transfer-list").DataTable().ajax.reload();
}
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 500);
    };
}
function initSaleTransfer() {
    /*sale-transfer*/
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('#search_date_range').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('#search_date_range').change(function () {
        $('#sale-list-table').DataTable().ajax.reload();
    });
    $('#search_date_range').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#search_date_range').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#sale-list-table').DataTable().ajax.reload();
    });
    $('#search_date_range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#search_date_range').html('full history');
        $('#sale-list-table').DataTable().ajax.reload();
    });
    $(".search_transfer-sale").keyup(searchThrottle(function () {
        $('#sale-list-table').DataTable().search($(this).val()).draw();
    }));
    /*END sale-transfer*/
}
function initPurchaseTransfer() {
    /*purchase-transfer*/
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('#search_purchase_date_range').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });

    $('#search_purchase_date_range').change(function () {
        $("#purchase-list-table").DataTable().draw();
    });
    $('#search_purchase_date_range').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#search_purchase_date_range').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $("#purchase-list-table").DataTable().draw();
    });
    $('#search_purchase_date_range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#search_purchase_date_range').html('full history');
        $("#purchase-list-table").DataTable().draw();
    });

    $('.search_transfer-purchase').keyup(searchThrottle(function () {
        $('#purchase-list-table').DataTable().search($(this).val()).draw();
    }));
    /*END sale-transfer*/
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
