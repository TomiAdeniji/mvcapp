var filter = {
    Workgroup: "",
    Key: ""
};
function onSelectWorkgroup(ev) {
    CallBackFilterDataPurchaseServeSide();
}
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTable(); }, 200);
}

function addPurchase() {
    LoadingOverlay();
    $('#app-trader-purchase-add').empty();
    $('#app-trader-purchase-add').load('/TraderPurchases/TraderPurchaseAdd?locationId=' + $('#local-manage-select').val());
    LoadingOverlayEnd();
}
function editPurchase(purchaseId) {
    LoadingOverlay();
    $('#app-trader-purchase-add').empty();
    $('#app-trader-purchase-add').load('/TraderPurchases/TraderPurchaseAdd?locationId=' + $('#local-manage-select').val() + '&purchaseId=' + purchaseId);
    $('#app-trader-purchase-add').modal('toggle');
    LoadingOverlayEnd();
}

function changeContact(ev) {
    if ($(ev).val() === "") return;
    $.ajax({
        type: 'get',
        url: '/TraderSales/GetContactById?id=' + $(ev).val(),
        dataType: 'json',
        success: function (response) {
            var strContact = response.Name + ",</br>";
            if (response.Address) {
                $('#contact_address_id').val(response.Address.Id);
                if (response.Address.AddressLine1)
                    strContact += ",</br>" + response.Address.AddressLine1;
                if (response.Address.City)
                    strContact += ",</br>" + response.Address.City;
                if (response.Address.AddressLine2)
                    strContact += ",</br>" + response.Address.AddressLine2;
                if (response.Address.State)
                    strContact += ",</br>" + response.Address.State;
                if (response.Address.Country)
                    strContact += ",</br>" + response.Address.Country.CommonName;
                if (response.Address.PostCode)
                    strContact += ",</br>" + response.Address.PostCode;
            }
            $('#address_info').empty();
            $('#address_info').append(strContact);
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
// ----------- workgroup ---------
EnableNextButton = function () {
    if ($workgroupId !== "")
        $(".btnNext").removeAttr("Disabled");
};
DisableNextButton = function () {
    $(".btnNext").attr("Disabled", "Disabled");
};

ChangeWorkgroup = function (ev) {
    WorkGroupSelectedChange();
    $('#item-select-manage').empty().trigger("change");
    var wgId = $(ev).val();
    ResetItemSelected('tb_form_item', 'item-select-manage', wgId, wgId, $('#local-manage-select').val(), false, false, true);
};

function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    $workgroupId = $("#trader_purchase_add_workgroup").val();
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
        });
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
    $('#app-trader-workgroup-preview').modal('toggle');
}
// ------- end workgroup -------------
function changeDelivery() {
    $('#contact_address_id').val(0);
}

var arrayUpdate = [];
function nextToItems() {
    if (!$('#trader_purchase_add_workgroup').val() || !$("#form_sale_contact").val()) {
        arrayUpdate.push(1);
        setTimeout(function () {
            if (arrayUpdate.length > 1) {
                $('.admintabs a[href="#sale-1"]').tab('show');
                if (!$("#form_sale_contact").val()) {
                    $("#form_specifics").validate().showErrors({ contact: "Contact is required" });
                }
                if (!$('#trader_purchase_add_workgroup').val())
                    $("#form_specifics").validate().showErrors({ workgroup: "Workgroup is required" });
                arrayUpdate = [];
            }
        },
            500);
    } else {
        $('#form_add_transaction')[0].reset();
        // reset item selected
        ResetItemSelected('tb_form_item', 'item-select-manage', $('#trader_purchase_add_workgroup').val(), undefined, $('#local-manage-select').val(), false, false, true);
    }
}
// validate Add new
function resetPurchaseForm() {
    $('#cpu').attr('disabled', true);
    $('#addNowForm').attr('disabled', true);
    $('#label_cost').text('Cost per <selected unit>');
}
function ChangeSelectedUnit() {
    $('#item_selected').empty();
    $.LoadingOverlay("show");

    var itemId = $('#item-select-manage').find(':selected').attr('itemId');

    $('#item_selected').load('/Trader/TraderSaleSelectUnit?idLocation=' + $('#local-manage-select').val() + '&idTraderItem=' + itemId, function () {
        LoadingOverlayEnd();
        if ($('#item-select-manage').val() !== "" && $('#item-select-manage').val() !== "0") {
            $('#label_cost').text('Cost per <selected unit>');
            $('#cpu').removeAttr('disabled');
        }
    });
}
function quantityChange() {
    var quantityElement = $('#form_item_quantity').val();
    var costElement = $('#cpu').val();
    var itemUnit = $("#conversionsunitid").val();
    var itemdiscount = $("#form_item_discount").val();
    if (parseFloat(quantityElement).toString() !== "NaN" && parseFloat(itemdiscount) > 100) {
        $("#form_item_discount").val(100);
    }
    if ((parseFloat(quantityElement).toString() !== "NaN" && parseFloat(quantityElement) > 0)
        && (parseFloat(costElement).toString() !== "NaN" && parseFloat(costElement) > 0) && itemUnit != "") {
        $('#addNowForm').removeAttr('disabled');
    } else {
        $('#addNowForm').attr('disabled', 'true');
    }
}
function selectedUnit(ev) {
    var val = $(ev);
    if (val) {
        $('#label_cost').text('Cost per ' + $(val.find('option:selected')).text());
    }
    else {
        $('#label_cost').text('Cost per <selected unit>');
    }
}
// End validate Add new
function removeRowItem(id) {
    $('#tb_form_item tbody tr.tr_id_' + id).remove();
    ResetItemSelected('tb_form_item', 'item-select-manage', $('#trader_purchase_add_workgroup').val(), undefined, $('#local-manage-select').val(), false, false, true);
}
function addRowItem() {
    var idBuild = UniqueId();
    var dimensionArrayId = $('#form_item_dimensions').val();
    if (!dimensionArrayId) {
        dimensionArrayId = [];
    }
    var units = $('#item_selected select').val();
    var selected = $('#item-select-manage').find(':selected');
    var itemId = selected.attr("itemId");
    var itemName = selected.attr("itemName");
    var itemImage = selected.attr("itemImage");
    var taxName = selected.attr("taxName");
    var taxRate = selected.attr("taxRate");
    var costUnit = selected.attr("costUnit");

    var item = {
        Id: idBuild,
        TraderItem: {
            Id: itemId,
            TaxName: taxName,
            ImageUri: itemImage,
            Name: itemName,
            TaxRate: taxRate,
            CostUnit: costUnit
        },
        Discount: isNaN(parseFloat($('#form_item_discount').val())) ? 0 : parseFloat($('#form_item_discount').val()),
        Dimensions: [],
        Quantity: parseFloat($('#form_item_quantity').val()),
        Unit: {},
        CostPerUnit: parseFloat($('#cpu').val()),
        Cost: 0
    };

    var priceIncludeTax = parseFloat(item.CostPerUnit) * parseFloat(item.Quantity) * (1 - (parseFloat(item.Discount) / 100)) * (1 + (parseFloat(item.TraderItem.TaxRate) / 100));
    var priceExcludeTax = priceIncludeTax / (1 + (item.TraderItem.TaxRate / 100));
    var priceValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);

    item.Cost = priceIncludeTax.toFixed($decimalPlace);

    var clone = $('#tb_form_template tbody tr').clone();
    $(clone).addClass('tr_id_' + itemId);
    // filter to table
    $($(clone).find('td.row_image div')).attr('style', "background-image: url('" + $('#api_url').val() + item.TraderItem.ImageUri + "&size=T');");
    $($(clone).find('td.row_name')).text(item.TraderItem.Name);
    $($(clone).find('td.row_unit')).empty();
    var unitClone = $('#item_selected select').clone();
    $($(clone).find('td.row_unit')).append(unitClone);
    $($(clone).find('td.row_unit select')).val(units);
    $($(clone).find('td.row_unit select')).attr('onchange', "rowUnitChange('" + itemId + "')");
    $($(clone).find('td.row_costperunit input')).val(item.CostPerUnit);
    $($(clone).find('td.row_costperunit input')).attr('onchange', "updateCost('" + itemId + "')");
    $($(clone).find('td.row_quantity input')).val(item.Quantity);
    $($(clone).find('td.row_quantity input')).attr('onchange', "updateCost('" + itemId + "')");
    $($(clone).find('td.row_discount input')).val(item.Discount);
    $($(clone).find('td.row_discount input')).attr('onchange', "updateCost('" + itemId + "')");
    $($(clone).find('td.row_taxrate')).text(item.TraderItem.TaxRate);

    $($(clone).find('td.row_taxname')).html(showTaxRatesDetail(item.CostPerUnit, item.Quantity, item.Discount, item.TraderItem.TaxName));

    $($(clone).find('td.row_dimensions select')).val(dimensionArrayId);
    $($(clone).find('td.row_cost input')).val(item.Cost).attr('onchange', 'updateTotalCost(' + itemId + ')');;
    $($(clone).find('td.row_button button')).attr('onclick', "removeRowItem('" + itemId + "')");
    $($(clone).find('td.row_button input.traderItem')).val(itemId);
    $($(clone).find('td.row_button input.row_id')).val(itemId);
    $($(clone).find('td select')).not('.multi-select').select2();

    var productItem = $('#tb_form_item tbody tr.' + 'tr_id_' + itemId);
    if (productItem.length === 0)
        $('#tb_form_item tbody').append(clone);
    else {
        cleanBookNotification.error(_L("ERROR_MSG_646"), "Qbicles");
    }

    setTimeout(function () {
        $('#form_add_transaction')[0].reset();
        $('#form_add_transaction select').not('.multi-select').not('[name="traderitem"]').select2();
        resetPurchaseForm();
    }, 100);
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, undefined, $('#local-manage-select').val(), false, false, true);
}
function rowUnitChange(id) {
    var item = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_button input.traderItem').val().split(':');
    var val = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_unit select').val().split(':');
    if (val[1] === "BaseUnit") {
        $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_costperunit input').val(item[3]);
    } else if (val[1] === "Conversion Unit") {
        $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_costperunit input').val(parseFloat(item[3]) * parseFloat(val[3]));
    }
    updateCost(id);
}
function updateCost(id) {
    var costofper = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_costperunit input').val();
    var quantity = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_quantity input').val();
    var discount = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_discount input').val();

    var taxRate = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_taxrate').text();
    if (isNaN(parseFloat(costofper)) || isNaN(parseFloat(quantity)) || isNaN(parseFloat(discount)) || isNaN(parseFloat(taxRate))) {
        return;
    }

    var priceIncludeTax = parseFloat(costofper) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + parseFloat(taxRate) / 100);
    //var priceExcludeTax = priceIncludeTax / (1 + (taxRate / 100));
    //var taxValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);

    $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_cost input').val(priceIncludeTax.toFixed($decimalPlace)).attr('onchange', 'updateTotalCost(' + id + ')');
    var staxtnameinit = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_taxname').html(showTaxRatesDetail(costofper, quantity, discount, staxtnameinit));
}
function validateSendToReview() {
    var contact = $('#form_sale_contact').val();
    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length > 0 && tds.length > 1) {
        $('#a_send_toreview_purchase').removeAttr('disabled');
        $('#a_send_toreview_purchase').removeClass('disabled');
    } else {
        $('#a_send_toreview_purchase').attr('disabled');
        $('#a_send_toreview_purchase').addClass('disabled');
    }
}

function nextToConfirm() {
    $('#tb_confirm tbody').empty();
    var trs = $('#tb_form_item tbody tr');
    var trd = $('#tb_form_item tbody tr td');
    var total = 0;
    if (trs.length === 1 && trd.length === 1) return;
    for (var i = 0; i < trs.length; i++) {
        var cost = stringToNumber($($(trs[i]).find('td.row_cost input')).val());
        total += cost;
        var strTrs = "<tr> <td>" + $($(trs[i]).find('td.row_image')).html() + "</td>";
        strTrs += "<td>" + $($(trs[i]).find('td.row_name')).text() + "</td> <td>" + $($(trs[i]).find('td.row_unit select option:selected')).text() + "</td>";
        strTrs += " <td>" + $($(trs[i]).find('td.row_quantity input')).val() + "</td>" +
            " <td>" + $($(trs[i]).find('td.row_discount input')).val() + "%</td>" +
            " <td>" + $($(trs[i]).find('td.row_taxname')).html() + "</td>";
        strTrs += "<td>";
        var dimensions = $($(trs[i]).find('td.row_dimensions select option:selected'));
        if (dimensions.length > 0) {
            for (var j = 0; j < dimensions.length; j++) {
                strTrs += "<span class=\"label label-info label-lg\">" + $(dimensions[j]).text() + "</span> ";
            }
        }
        strTrs += "</td>";
        strTrs += "<td>" + $($(trs[i]).find('td.row_cost input')).val() + "</td> </tr>";
        $('#tb_confirm tbody').append(strTrs);
    }
    $('#total_id').text(toCurrencySymbol(total));
    validateSendToReview();
}
// Save Trader Sale  /////
function SavePurchaseDraft() {
    SavePurchase("Draft");
}
function SavePurchaseReview() {
    SavePurchase("PendingReview");
}

function SavePurchase(status) {
    var purchaseItems = [];
    var items = $('#tb_form_item tbody tr');
    if (items.length > 0) {
        for (var i = 0; i < items.length; i++) {
            var id = parseInt($($(items[i]).find('td.row_button input.row_id')).val());
            var item =
            {
                Id: 0
            };

            if ($($(items[i]).find('td.row_button input.traderItem')).val() !== null) {
                item.Id = $($(items[i]).find('td.row_button input.traderItem')).val();
            } else {
                item = null;
            }
            var dimensions = $($(items[i]).find('td.row_dimensions select')).val();
            var dimens = [];
            if (dimensions && dimensions.length > 0) {
                for (var j = 0; j < dimensions.length; j++) {
                    dimens.push({ Id: dimensions[j] });
                }
            }
            if (status === 'PendingReview') {
                if (dimens.length === 0) {
                    cleanBookNotification.error(_L("ERROR_MSG_635"), "Qbicles");
                    $('.admintabs a[href="#sale-2"]').tab('show');
                    return false;
                }
            }
            var unit = $($(items[i]).find('td.row_unit select')).val().split('|');

            var purchaseItem = {
                Id: isNaN(id) ? 0 : id,
                TraderItem: item,
                Discount: $($(items[i]).find('td.row_discount input')).val(),
                Dimensions: dimens,
                Quantity: $($(items[i]).find('td.row_quantity input')).val(),
                Unit: { Id: unit[1] },
                CostPerUnit: stringToNumber($($(items[i]).find('td.row_costperunit input')).val()),
                Cost: stringToNumber($($(items[i]).find('td.row_cost input')).val())
            }
            if (parseFloat(purchaseItem.Quantity).toString() === "NaN") {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MSG_636"), "Qbicles");
                return false;
            }
            else if (parseFloat(purchaseItem.Quantity) <= 0) {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MSG_637"), "Qbicles");
                return false;
            }
            purchaseItems.push(purchaseItem);
        }
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
    var traderPurchase = {
        Key: $('#tradersale_form_key').val(),
        Location: { Id: $('#local-manage-select').val() },
        PurchaseItems: purchaseItems,
        Status: status,
        Reference: Reference,
        Workgroup: { Id: $('#trader_purchase_add_workgroup').val() },
        PurchaseTotal: stringToNumber($('#total_id').text()),
        Vendor: { Id: $('#form_sale_contact').val() },
        DeliveryMethod: $('#sales_delivery').val()
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderPurchases/SaveTraderPurchase',
        data: { traderPurchase: traderPurchase },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $('#app-trader-purchase-add').modal('toggle');
                cleanBookNotification.createSuccess();
                setTimeout(function () { ShowTablePurchaseValue(true); }, 500);
            } else if (response.actionVal === 2) {
                $('#app-trader-purchase-add').modal('toggle');
                cleanBookNotification.updateSuccess();
                setTimeout(function () { ShowTablePurchaseValue(true); }, 500);
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
        }
    });
};
// show table
$(function () {
    ShowTablePurchaseValue();
});
function ShowTablePurchaseValue(isloading) {
    if (!isloading)
        $('#trader-purchase-content').LoadingOverlay("show");
    $('#trader-purchase-content').load('/TraderPurchases/TraderPurchaseTable', function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#community-list').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#trader-purchase-content').LoadingOverlay("hide");
    });
}
function LoadTableDataPurchase(tableid, url, columns, orderIndex) {
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keysearch: $('#search_dt').val(),
                    groupid: $('#filterworkgroup').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function FilterDataByServerSide() {
    var url = '/TraderPurchases/TraderPurchaseDataTable';

    var columns = [
        {
            name: "FullRef",
            data: "FullRef",
            orderable: false,
        },
        {
            name: "WorkGroupName",
            data: "WorkGroupName",
            orderable: true
        },
        {
            name: "CreatedDate",
            data: "CreatedDate",
            orderable: true
        },
        {
            name: "Contact",
            data: "Contact",
            orderable: false
        },
        {
            name: "ReportingFilter",
            data: "ReportingFilter",
            orderable: false
        },
        {
            name: "Total",
            data: "Total",
            orderable: false
        },
        {
            name: "Status",
            data: "Status",
            orderable: false,
            width: "50px",
            render: function (value, type, row) {
                var str = '';
                switch (row.Status) {
                    case "Draft":
                        str += '<span class="label label-lg label-primary">Draft</span>';
                        break;
                    case "PendingReview":
                        str += '<span class="label label-lg label-warning">Awaiting Review</span>';
                        break;
                    case "PendingApproval":
                        str += '<span class="label label-lg label-info">Awaiting Approval</span>';
                        break;
                    case "PurchaseDenied":
                        str += '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    case "PurchaseApproved":
                        str += '<span class="label label-lg label-success">Approved</span>';
                        break;
                    case "PurchaseDiscarded":
                        str += '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                    case "PurchaseOrderIssued":
                        str += '<span class="label label-lg label-success">Order Issued</span>';
                        break;
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
                if (row.Status === 'Draft') {
                    if (row.AllowEdit) {
                        str += '<button class="btn btn-info" onclick="editPurchase(\'' + row.Id + '\')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                    } else {
                        str += '<button class="btn btn-info hidden" onclick="editPurchase(\'' + row.Id + '\')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                    }
                } else {
                    str += '<button class="btn btn-primary" onclick="window.location.href=\'/TraderPurchases/PurchaseMaster?id=' + row.Id + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                }
                return str;
            }
        }
    ];

    LoadTableDataPurchase('community-list', url, columns, 2);
    CallBackFilterDataPurchaseServeSide();
}

function CallBackFilterDataPurchaseServeSide() {
    $("#community-list").DataTable().ajax.reload();
}

function updateTotalCost(itemId) {
    var quantity = parseFloat($('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_quantity input').val());
    var discount = isNaN(parseFloat($('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_discount input').val()))
        ? 0 : parseFloat($('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_discount input').val());
    var taxRate = parseFloat($('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_taxrate').text());
    var taxName = $('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_taxname .txt-taxname').val();
    var totalCost = parseFloat($('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_cost input').val());
    if (!isNaN(quantity) && !isNaN(discount) && !isNaN(totalCost) && quantity != 0 && discount != 100 && taxRate != -100) {
        // Re-Calculate Unit per item
        var new_cost_per_unit = parseFloat(totalCost / (quantity * (1 - discount / 100) * (1 + taxRate / 100))).toFixed(5);
        $('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_costperunit input').val(new_cost_per_unit);
        // Re-Calculate tax price
        $('#tb_form_item tbody tr.tr_id_' + itemId + ' .row_taxname').html(showTaxRatesDetail(new_cost_per_unit, quantity, discount, taxName));
    }
}