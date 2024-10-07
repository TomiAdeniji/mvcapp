var $qbiclesFileTypes = [];
var $traderBillAttachments = [];
var $traderBillAttachmentExisted = {
    AssociatedFiles: []
};

$(document).ready(function () {
    $qbiclesFileTypes = [];
    $.ajax({
        type: 'post',
        url: '/FileTypes/GetFileTypes',
        dataType: 'json',
        success: function (response) {
            $qbiclesFileTypes = response;
        },
        error: function (er) {
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
});


var $traderPurchaseId = $("#TraderPurchaseId").val();
//approval update
function UpdateStatusApproval(apprKey) {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: $("#action_approval").val() },
        success: function (rs) {
            if (rs.actionVal > 0) {
                LoadingOverlayEnd();
                cleanBookNotification.updateSuccess();
                setTimeout(function () { window.location.reload(true); }, 500);
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};


//contact
function ChangeContact(id) {
    $.LoadingOverlay("show");
    $('#purchase-review-contact').empty();
    $('#purchase-review-contact').load('/TraderPurchases/TraderPurchaseReviewContact?id=' + id);
    setTimeout(function () {
        LoadingOverlayEnd();
        $("#app-trader-purchase-contact").modal('toggle');
    }, 500);
};

function changeDelivery() {
    $('#contact_address_id').val(0);
    $('.delivery-stored').hide();
    $('.delivery-details').fadeIn();
};

function OnChangeDelivery() {
    if ($('#purchases_delivery').val() === "Delivery") {
        $('.delivery-stored').fadeIn();
    } else {
        $('.delivery-stored').hide();
    }
};

function SelectContactChange(ev) {
    if ($(ev).val() === "") return;
    $.ajax({
        type: 'get',
        url: '/TraderPurchases/GetContactById?id=' + $(ev).val(),
        dataType: 'json',
        success: function (response) {
            var strContact = response.Name + ",</br>";
            if (response.Address) {
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
};
function UpdateContact() {
    $.LoadingOverlay("show");

    var purchase = {
        Id: $traderPurchaseId,
        DeliveryMethod: $('#purchases_delivery').val(),
        Vendor: { Id: $('#form_purchase_contact').val() }
    }

    $.ajax({
        type: 'post',
        url: '/TraderPurchases/UpdateTraderPurchaseContact?countryName=' + $('#form_contact_address_country').val(),
        data: { traderPurchase: purchase },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $('#purchase-review-contact-preview').empty();
                $('#purchase-review-contact-preview').load('/TraderPurchases/TraderPurchaseReviewContactPreview?id=' + $traderPurchaseId);
            }
            setTimeout(function () {
                $("#app-trader-purchase-contact").modal('hide');
                LoadingOverlayEnd();
            }, 500);
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
//items tab   
function ChangeItems(id) {
    $.LoadingOverlay("show");
    CheckStatus(id, 'Purchase').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object !== "PurchaseApproved") {
            // load form update
            $.LoadingOverlay("show");
            $('#purchase-review-items').empty();
            $('#purchase-review-items').load('/TraderPurchases/TraderPurchaseReviewItems?id=' + id, function () {
                LoadingOverlayEnd();
                $("#purchase-review-items-modal").modal('toggle');
                // reset item selected
                var workgroupItemId = $("#purchase-item-workgroup-id").val();
                var locationItemId = $("#purchase-item-location-id").val();
                ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, false, true);
            });

        } else if (res.result && res.Object === "PurchaseApproved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");

        }
    });
};



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
    $($(clone).find('td.row_cost input')).val(item.Cost).attr('onchange', 'updateTotalReviewItemCost(' + itemId + ')');
    $($(clone).find('td.row_button button')).attr('onclick', "removeRowItem('" + itemId + "')");
    $($(clone).find('td.row_button input.traderItem')).val(itemId);
    $($(clone).find('td.row_button input.row_id')).val(itemId);
    $($(clone).find('td select')).not('.multi-select').select2();
    $('#tb_form_item tbody').append(clone);
    setTimeout(function () {
        $('#form_add_transaction')[0].reset();
        $('#form_add_transaction select').not('.multi-select').select2();
        resetPurchaseForm();
    }, 100);
    setTimeout(function () {
        var workgroupItemId = $("#purchase-item-workgroup-id").val();
        var locationItemId = $("#purchase-item-location-id").val();
        ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, false, true);
    }, 200);
};

function updateTotalReviewItemCost(itemId) {
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

    $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_cost input').val(priceIncludeTax.toFixed($decimalPlace));
    var staxtnameinit = $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_taxname').html(showTaxRatesDetail(costofper, quantity, discount, staxtnameinit));
};

function removeRowItem(id) {
    $('#tb_form_item tbody tr.tr_id_' + id).remove();
    var workgroupItemId = $("#purchase-item-workgroup-id").val();
    var locationItemId = $("#purchase-item-location-id").val();
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, false, true);
};

function UpdateItems() {
    $.LoadingOverlay("show");
    CheckStatus($traderPurchaseId, 'Purchase').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object !== "PurchaseApproved") {
            // update item
            $.LoadingOverlay("show");
            var purchaseItems = [];
            
            var items = $('#tb_form_item tbody tr');
            if (items.length > 0) {
                for (var i = 0; i < items.length; i++) {

                    var pQuantity = $($(items[i]).find('td.row_quantity input')).val();

                    if (parseFloat(pQuantity).toString() === "NaN") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_636"), "Qbicles");
                        return false;
                    } else if (parseFloat(pQuantity) <= 0) {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_637"), "Qbicles");
                        return false;
                    }

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
                    if (dimens.length === 0) {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_635"), "Qbicles");
                        return false;
                    }
                    var unit = $($(items[i]).find('td.row_unit select')).val().split('|');                    

                    var purchaseItem = {
                        Id: isNaN(id) ? 0 : id,
                        TraderItem: item,
                        Discount: stringToNumber($($(items[i]).find('td.row_discount input')).val()),
                        Dimensions: dimens,
                        Quantity: stringToNumber(pQuantity),
                        Unit: { Id: unit[1] },
                        CostPerUnit: stringToNumber($($(items[i]).find('td.row_costperunit input')).val()),
                        Cost: stringToNumber($($(items[i]).find('td.row_cost input')).val())
                    }
                    
                    purchaseItems.push(purchaseItem);
                }
            }
            var purchase = {
                Id: $traderPurchaseId,
                PurchaseItems: purchaseItems,
                PurchaseTotal: PurchaseTotal()
            }
            $.ajax({
                type: 'post',
                url: '/TraderPurchases/UpdateTraderPurchaseItems',
                data: { traderPurchase: purchase },
                dataType: 'json',
                success: function (response) {

                    if (response.actionVal === 1) {
                        $('#table-purchase-review-items-preview').empty();
                        $('#table-purchase-review-items-preview').load(
                            '/TraderPurchases/TraderPurchaseReviewItemsPreview?id=' + $traderPurchaseId,
                            function () {
                                LoadingOverlayEnd();
                            });
                        $("#purchase-total").text(toCurrencySymbol(purchase.PurchaseTotal,false));

                        $("#purchase-review-items-modal").modal("hide");
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
            // end update item
        } else if (res.result && res.Object === "PurchaseApproved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
        }
        LoadingOverlayEnd();
    });
};


PurchaseTotal = function () {
    var trs = $('#tb_form_item tbody tr');
    var trd = $('#tb_form_item tbody tr td');
    var total = 0;
    if (trs.length === 1 && trd.length === 1) return 0;
    for (var i = 0; i < trs.length; i++) {
        total += stringToNumber($($(trs[i]).find('td.row_cost input')).val());
    }
    return total;
};

function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#transfer-workgroup-select").val());
}
function InitTransfer(key) {
    $('#app-trader-purchase-transfer').empty();
    $('#app-trader-purchase-transfer').load("/TraderTransfers/InitPurchaseTransfer?key=" + key);
}
function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    var id = $("#transfer-workgroup-select").val();
    if (id && id !== "") {
        $.LoadingOverlay("show");
        $('#transfer-workgroup-select + label.error').remove();
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
        LoadingOverlayEnd();
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
}


var $editMode = "Purchase";
var $transferRequirement = "";



function SendToPickup(status) {
    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
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
    }
    var transfer = {
        Id: 0,
        Status: status,
        Purchase: { Id: $('#TraderPurchaseId').val() },
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
    $.LoadingOverlay("show");
    $.ajax({
        type: "post",
        url: "/TraderTransfers/SaveTransfer",
        data: { transfer: transfer },
        dataType: "json",
        success: function (response) {
            $('#app-trader-purchase-transfer').modal('toggle');
            LoadingOverlayEnd();

            $("#app-trader-purchase-transfer").modal("hide");
            cleanBookNotification.createSuccess();
            reloadTableTransfer();
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}
// validate Add new  
function resetPurchaseForm() {
    $('#cpu').attr('disabled', 'true');
    $('#addNowForm').attr('disabled', true);
}
function ChangeSelectedUnit() {

    $.LoadingOverlay("show");
    $('#item_selected').empty();
    var currentLocationId = $("#location-id").val();
    var itemId = $('#item-select-manage').find(':selected').attr("itemId");

    $('#item_selected').load('/Trader/TraderSaleSelectUnit?idLocation=' + currentLocationId + '&idTraderItem=' + itemId, function () {
        LoadingOverlayEnd();
        if ($('#item-select-manage').val() !== "" && $('#item-select-manage').val() !== "0") {
            $('#label_cost').text('Cost per <selected unit>');
            $('#cpu').removeAttr('disabled');
        }
    });

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
// End validate Add new
function SavePurchaseTransfer(status) {

    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
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
    $.LoadingOverlay("show");
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
                reloadTableTransfer();
            }
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

};

function reloadTableTransfer() {
    $('#table_transfer').empty();
    $('#table_transfer').load("/TraderTransfers/GetTablePurchaseTransfer?id=" + $("#TraderPurchaseId").val());
};

function EditTransfer(id, purchaseId) {
    $('#app-trader-purchase-transfer').empty();
    $('#app-trader-purchase-transfer').load("/TraderTransfers/EditPurchaseTransfer?id=" + id + '&idPurchase=' + purchaseId);
};



// purchase order
function ClearText() {
    $("#purchase-order-additions").val("");
};
function SavePurchaseOrder(purchaseId) {

    $.LoadingOverlay("show");
    var Reference = {
        Id: $('#reference_id').val(),
        NumericPart: parseFloat($('#refedit').text()),
        Type: $('#reference_type').val(),
        Prefix: $('#reference_prefix').val(),
        Suffix: $('#reference_suffix').val(),
        Delimeter: $('#reference_delimeter').val(),
        FullRef: $('#reference_fullref').val()
    }
    var purchaseOrder = {
        Purchase: {
            Id: purchaseId
        },
        AdditionalInformation: $("#purchase-order-additions").val(),
        Reference: Reference
    };


    $.ajax({
        type: 'post',
        url: "/TraderPurchases/SavePurchaseOrder",
        datatype: 'json',
        data: purchaseOrder,
        success: function (refModel) {
            LoadingOverlayEnd();
            if (refModel.result) {
                cleanBookNotification.createSuccess();
                setTimeout(function () {
                    window.location.href = "/TraderPurchases/PurchaseOrder?id=" + refModel.msgId;
                }, 1000);

            }
        }, error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};

// Trader purchase bill
function loadBills(id) {
    $('#table_bills').load('/TraderPurchases/ShowTableBillByPurchase?id=' + id);
}
function AddEditTraderPurchaseBill(id, purchaseId) {
    $.LoadingOverlay("show");
    $traderBillAttachmentExisted = {
        AssociatedFiles: []
    };
    $traderBillAttachments = [];

    $('#app-trader-bill-add').load('/TraderPurchases/AddEditTraderPurchaseBill?id=' + id + '&purchaseId=' + purchaseId, function () {
        initTableBill(purchaseId);
        LoadingOverlayEnd();
    });
}
//Thomas refactor code Table bill
function initTableBill(purchaseId) {
    var $tablebill = $('table.purchase_item_table').DataTable({
        "ajax": "/TraderPurchases/GetInvoiceItemsFromPurchase?purchaseId=" + purchaseId,
        "columns": [
            {
                "data": "InvoiceChecked",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    let _htmlCheck = '<div class="checkbox toggle"><label><input class="rowmask invoice_checked trackChecked" data-toggle="toggle" data-onstyle="success" data-on="True" data-off="False" type="checkbox" ' + (data ? 'checked' : '') + '></label></div>';
                    return _htmlCheck;
                }
            },
            {
                "data": "TransItemUri",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    let _htmlImage = '<div class="table-avatar" style="background-image: url(\'' + data + '\');"></div>';
                    return _htmlImage;
                }
            },
            { "data": "TransItemName" },
            { "data": "TransItemUnitName" },
            { "data": "TransItemQuantity" },
            { "data": "TransItemDiscount" },
            { "data": "TransItemHtmlTaxRates" },
            { "data": "TransItemCost" },
            {
                "data": "InvoiceQuantity",
                "render": function (data, type, row, meta) {
                    let _htmlInput = '<input id="ivQuantityVal-' + row.TransItemId + '" type="text" onkeypress="numberKeyPress(event)" value="' + toCurrencyDecimalPlace(data) + '" name="invqty" min="0" maxlength="15" class="form-control invoice_quantity trackQuantity isnumber" style="width: 110px;">';
                    return _htmlInput;
                }
            },
            {
                "data": "InvoiceTaxes",
                "render": function (data, type, row, meta) {
                    return '<span id="ivTaxesVal-' + row.TransItemId + '">' + toCurrencyDecimalPlace(data)+'</span>';
                }
            },
            {
                "data": "InvoiceDiscount",
                "render": function (data, type, row, meta) {
                    return '<span id="ivDiscountVal-' + row.TransItemId + '">' + toCurrencyDecimalPlace(data) + '</span>';
                }
            },
            {
                "data": "InvoiceCost",
                "render": function (data, type, row, meta) {
                    return '<span id="ivCostVal-' + row.TransItemId + '">' + toCurrencyDecimalPlace(data) + '</span>';
                }
            }
        ],
        "drawCallback": function (settings) {
            $(".trackChecked").on("change", function () {
                var $row = $(this).parents("tr");
                var rowData = $('table.purchase_item_table').DataTable().row($row).data();
                if ($(this).prop('checked'))
                    rowData.InvoiceChecked = true;
                else
                    rowData.InvoiceChecked = false;
            })
            $(".trackQuantity").on("change", function () {
                var $row = $(this).parents("tr");
                var $objectRow=$('table.purchase_item_table').DataTable().row($row);
                var rowData = $objectRow.data();
                let quantityInput=$(this).val().replace(/\,/g, "");
                rowData.InvoiceQuantity = parseFloat(quantityInput ? quantityInput : 0.00);
                if (rowData.InvoiceQuantity > rowData.TransItemQuantity)
                {
                    rowData.InvoiceQuantity = rowData.TransItemQuantity;
                    $('#ivQuantityVal-' + rowData.TransItemId).val(toCurrencyDecimalPlace(rowData.InvoiceQuantity));
                }
                let billPrice = (rowData.PricePerUnit * rowData.InvoiceQuantity);
                rowData.InvoiceDiscount = billPrice * (rowData.TransItemDiscount / 100);
                rowData.InvoiceTaxes = (billPrice - rowData.InvoiceDiscount) * rowData.TransItemSumValTaxRates;
                rowData.InvoiceCost = billPrice * (1 - (rowData.TransItemDiscount / 100)) * (1 + rowData.TransItemSumValTaxRates);
                //Binding value InvoiceTaxes,InvoiceDiscount,InvoiceCost
                $($row).LoadingOverlay('show');
                $('#ivTaxesVal-' + rowData.TransItemId).text(toCurrencyDecimalPlace(rowData.InvoiceTaxes));
                $('#ivDiscountVal-' + rowData.TransItemId).text(toCurrencyDecimalPlace(rowData.InvoiceDiscount));
                $('#ivCostVal-' + rowData.TransItemId).text(toCurrencyDecimalPlace(rowData.InvoiceCost));
                $($row).LoadingOverlay('hide');

            })
        }
    });
    $tablebill.on('draw.dt', function () {
        $('.invoice_checked').bootstrapToggle();
        $(".trackQuantity ").change();
    });
}

function SaveBill(status) {

    $.LoadingOverlay("show");

    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        $("#form_bill_addedit").validate().showErrors({ workgroup: "Workgroup is required." });
        $('a[href="#setup"]').tab("show");
        LoadingOverlayEnd();
        return false;
    }
    var Reference = {};

    Reference = {
        Id: $('#app-trader-bill-add  #reference_id').val(),
        NumericPart: parseFloat($('#app-trader-bill-add  #refedit').text()),
        Type: $('#app-trader-bill-add  #reference_type').val(),
        Prefix: $('#app-trader-bill-add  #reference_prefix').val(),
        Suffix: $('#app-trader-bill-add  #reference_suffix').val(),
        Delimeter: $('#app-trader-bill-add  #reference_delimeter').val(),
        FullRef: $('#app-trader-bill-add  #reference_fullref').val()
    }
    var mydate = moment($('#bill_date').val(), $dateFormatByUser.toUpperCase());
    var dueDateChanged = moment(mydate).format("YYYY/MM/DD");
    var billInvoice = {
        Id: $('#bill_id').val(),
        DueDate: dueDateChanged,
        InvoicePDF: "",
        Reference: Reference,
        PaymentDetails: $('#bill_notes').val(),
        Workgroup: $workgroup,
        Purchase: { Id: $('#bill_purchase').val() },
        Status: status,
        InvoiceItems: []
    }

    //Thomas:fix bug https://atomsinteractive.atlassian.net/browse/QBIC-3182
    var $tablebill = $('table.purchase_item_table').DataTable();
    $tablebill.rows().every(function () {
        const _row = this.data();
        if (_row.InvoiceChecked) {
            var invoiceTransaction = {
                Id: 0,
                TransactionItem: { Id: _row.TransItemId },
                InvoiceValue: _row.InvoiceCost,
                InvoiceDiscountValue: _row.InvoiceDiscount,
                InvoiceItemQuantity: _row.InvoiceQuantity,
                InvoiceTaxValue: _row.InvoiceTaxes
            }
            if (!invoiceTransaction.InvoiceValue || invoiceTransaction.InvoiceValue === "") {
                invoiceTransaction.InvoiceValue = 0;
            }
            billInvoice.InvoiceItems.push(invoiceTransaction);
        }
    });
    //var tr = $('.purchase_item_table tbody tr');
    //var td = $('.purchase_item_table tbody tr td');
    //if (tr.length > 0 && td.length > 1) {
    //    for (var i = 0; i < tr.length; i++) {
    //        if ($($(tr[i]).find('input.invoice_checked')).length > 0 && $($(tr[i]).find('input.invoice_checked'))[0].checked) {
    //            var invoiceTransaction = {
    //                Id: $($(tr[i]).find('input.transaction_id')).val(),
    //                TransactionItem: { Id: $($(tr[i]).find('input.transaction_id')).val() },
    //                InvoiceValue: $($(tr[i]).find('span.invoice_value')).text().replace(/\,/g, ""),
    //                InvoiceDiscountValue: $($(tr[i]).find('span.invoice_discountValue')).text().replace(/\,/g, ""),
    //                InvoiceItemQuantity: $($(tr[i]).find('input.invoice_quantity')).val().replace(/\,/g, ""),
    //                InvoiceTaxValue: $($(tr[i]).find('span.invoice_taxvalue')).text().replace(/\,/g, "")
    //            }
    //            if (!invoiceTransaction.InvoiceValue || invoiceTransaction.InvoiceValue === "") {
    //                invoiceTransaction.InvoiceValue = "0";
    //            }
    //            invoiceTransaction.InvoiceValue = parseFloat(invoiceTransaction.InvoiceValue).toFixed($decimalPlace);
    //            billInvoice.InvoiceItems.push(invoiceTransaction);
    //        }
    //    }
    //}

    if ($('#bill_upload_file').val() !== '') {
        UploadMediaS3ClientSide("bill_upload_file").then(function (mediaS3Object) {
            billInvoice.InvoicePDF = mediaS3Object.objectKey;
            $('#bill_invoicePDF').val(mediaS3Object.objectKey);
            BillProcessMedia(billInvoice);
        });
    } else {
        billInvoice.InvoicePDF = $('#bill_invoicePDF').val();
        BillProcessMedia(billInvoice);
    }

};


BillProcessMedia = function (billInvoice) {

    //upload media

    if ($traderBillAttachments.length > 0) {

        UploadBatchMediasS3ClientSide($traderBillAttachments).then(function () {
            SubmitPurchaseBill(billInvoice);
        });
    }
    else {
        SubmitPurchaseBill(billInvoice);
    }
}

SubmitPurchaseBill = function (billInvoice) {


    var files = [];
    _.forEach($traderBillAttachments, function (file) {
        file.File = {};
        files.push(file);
    });

    $.ajax({
        type: "post",
        url: "/TraderPurchases/SaveBillInvoice",
        data: {
            invoice: billInvoice,
            traderBillAssociatedFiles: $traderBillAttachmentExisted,
            traderBillAttachments: files
        },
        dataType: "json",
        success: function (response) {
            if (response.actionVal === 1) {
                $("#app-trader-bill-add").modal("hide");
                cleanBookNotification.createSuccess();
                loadBills($('#bill_purchase').val());
            } else if (response.actionVal === 2) {
                $("#app-trader-bill-add").modal("hide");
                cleanBookNotification.updateSuccess();
                loadBills($('#bill_purchase').val());
            } else {
                cleanBookNotification.error(response.msg);
            }
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

};

//function ChangeBillValue(ev, id, price) {
//    var billQuantity = parseFloat($(ev).val().replace(/\,/g, ""));
//    var quantity = parseFloat($('span.quantity_value_' + id).text().replace(/\,/g, ""));
//    billQuantity = billQuantity ? billQuantity : 0;
//    if (billQuantity > quantity) {
//        billQuantity = quantity;
//        $(ev).val(billQuantity);
//    }
//    price = price ? price : 0;
//    var discount = stringToNumber($('span.discount_value_' + id).text());
//    var purchaseValue = stringToNumber($('span.sale_value_' + id).text());
//    var taxRate = stringToNumber($('span.taxrate_value_' + id).text());
//    var billPrice = price * billQuantity;
//    var billDisCount = billPrice * (discount / 100);
//    var billTaxValue = (billPrice - billDisCount) * taxRate;
//    var billValue = billPrice * (1 - (discount / 100)) * (1 + taxRate);
//    $('td.invoice_td_value_' + id + ' span.invoice_value').text(parseFloat(billValue).toFixed($decimalPlace));
//    $('span.invoice_discountValue_' + id).text(parseFloat(billDisCount).toFixed($decimalPlace));
//    $('span.invoice_taxvalue_' + id).text(parseFloat(billTaxValue).toFixed($decimalPlace));
//    $('span.invoice_taxvalue_' + id).hide();
//    $('#taxBill_' + id).show();
//    $('#taxBill_' + id).html(showTaxRatesDetail(price, billQuantity, discount, $('#taxBill_' + id + ' input.txt-taxname').val()));
//    return true;
//};




function PurchaseBillAttachmentConfirmAddViewer(index) {
    $traderBillAttachmentExisted = {
        AssociatedFiles: []
    };
    $traderBillAttachments = [];
    var fileExtension = "";

    var inputFiles = $("#attachments-view-bill .attachments_" + index + " input.inputfile");
    for (var i = 0; i < inputFiles.length; i++) {
        // check input file name and set file name
        if ($(".attachments_" + index + " .inputfilename" + (i + 1)).val() === "" && inputFiles[i].files.length > 0)
            $(".attachments_" + index + " .inputfilename" + (i + 1)).val(inputFiles[i].files[0].name.substr(0, inputFiles[i].files[0].name.lastIndexOf('.')));

        var fielId = $(".file-id-input" + (i + 1)).val();

        if (inputFiles[i].files.length > 0) {
            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: fielId,//GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if ($("div.attachments_" + index + " .inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("div.attachments_" + index + " .inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $traderBillAttachments.push(attachmentAdd);
        }
        else {
            //edit bull attachment
            if ($("#file_id_" + (i + 1)).length > 0) {
                $traderBillAttachmentExisted.AssociatedFiles.push(
                    {
                        Id: parseInt($("#file_id_" + (i + 1)).val()),
                        Name: $(".inputfilename" + (i + 1)).val(),
                        IconPath: $("#inputiconpath_edit" + (i + 1)).val()
                    });
            }
        }
    }
    PurchaseBillLoadAttachmentListViewer();

};

function PurchaseBillLoadAttachmentListViewer() {

    $("ul.domain-change-list").empty();
    var attachmentCount = $traderBillAttachmentExisted.AssociatedFiles.length + $traderBillAttachments.length;
    $("#trader-bill-attachment-manage").text(" Attachments (" + attachmentCount + ")");

    if (attachmentCount > 0)
        $("#trader-bill-attachment-icon").removeClass("fa fa-plus").addClass("fa fa-paperclip");


    _.forEach($traderBillAttachmentExisted.AssociatedFiles, function (file) {
        var li = " <li id='att-" + file.Id + "'>" +
            "<input class=\"file-id\" type=\"hidden\" value=\"" + file.Id + "\" />" +
            "<button class='btn btn-danger' onclick=\"PurchaseBillAttachmentRemove('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });

    _.forEach($traderBillAttachments, function (file) {
        var li = " <li id='att-" + file.Id + "'>" +
            "<input class=\"file-id\" type=\"hidden\" value=\"" + file.Id + "\" />" +
            "<button class='btn btn-danger' onclick=\"PurchaseBillAttachmentRemove('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });

};

function PurchaseBillAttachmentChangeFile(evt, index, indexform) {
    var fileName = $(".attachments_" + indexform + " input.inputfilename" + index).val();
    if ($(evt).length > 0 && $(evt)[0].files.length > 0 && fileName.length === 0)
        fileName = $(evt)[0].files[0].name;
    var files = $(".attachments_" + indexform + " input.inputfile");
    for (var i = 0; i < files.length; i++) {
        if (files[0] !== evt && $(files[0]).val() === $(evt).val()) {
            $(evt).val('');
            cleanBookNotification.error(_L("ERROR_MSG_616", [fileName]), "Qbicles");
            fileName = '';
        }
    }
    $('.attachments_' + indexform + ' .inputfilename' + index).val(fileName.split('.').slice(0, -1).join('.'));
}

function PurchaseBillAttachmentAddAnother(index) {
    var idFile = GenerateUUID();
    var inputFiles = $(".attachments_" + index + " input.inputfile");
    var attInput = "<div class=\"row attachment_row att-id-" + idFile + "\"> <div class=\"col-xs-12\">";
    attInput += "<input type=\"hidden\" class=\"file-id-input" + (inputFiles.length + 1) + "\" value=\"" + idFile + "\"/>";
    attInput += "<div class=\"form-group\"> <label for=\"name\">Name</label> <input type=\"text\" name=\"name\" class=\"form-control inputfilename" + (inputFiles.length + 1) + "\">";
    attInput += "</div> </div> <div class=\"col-xs-12\"> <div class=\"form-group\"> <label for=\"file\">File</label>";
    attInput += "<input type=\"file\" name=\"file\" onchange=\"PurchaseBillAttachmentChangeFile(this," + (inputFiles.length + 1) + "," + index + ")\" class=\"form-control inputfile\">  </div>  </div> </div>";
    $(".attachments_" + index + " div.repeater_wrap").append(attInput);
};

function PurchaseBillAttachmentRemove(id) {
    $("#att-" + id).remove();
    $(".att-id-" + id).remove();
    _.remove($traderBillAttachments, function (file) {
        return file.Id == id;
    });
    _.remove($traderBillAttachmentExisted.AssociatedFiles, function (file) {
        return file.Id == id;
    });

    var attachmentCount = $traderBillAttachmentExisted.AssociatedFiles.length + $traderBillAttachments.length;
    $("#trader-bill-attachment-manage").text(" Attachments (" + attachmentCount + ")");
};


// End purchase bill
//function IssuePurchaseOrder(purchaseOrderId) {
//    $.LoadingOverlay("show");
//    setTimeout(function () {
//        //$("#purchase-order-template").removeClass("hidden");
//        //$("#purchase-order-template").addClass("hidden");
//        $.ajax({
//            url: "/TraderPurchases/IssuePurchaseOrder",
//            type: "POST",
//            dataType: "json",
//            data: { id: purchaseOrderId },
//            success: function (rs) {
//                if (rs.result) {
//                    LoadingOverlayEnd();
//                    cleanBookNotification.updateSuccess();
//                }
//            },
//            error: function (err) {
//                $("#purchase-order-template").addClass("hidden");
//                LoadingOverlayEnd(); cleanBookNotification.error(err.responseText, "Qbicles");
//                cleanBookNotification.error("Issue error: " + err, "Qbicles");
//            }
//        });
//    }, 500);

//};


function IssuePurchaseOrder(purchaseOrderId) {
    var emails = $("#mail-addresses").val();
    if (emails.length > 0) {
        var invalid = validateEmails(emails);
        if (invalid.length > 0) {
            cleanBookNotification.error("Email format is not correct:'\n'" + invalid, "Qbicles");

            return;
        }
    }
    $('#adding-other-mail-addresses').LoadingOverlay("show");
    $("#sale-order-template").removeClass("hidden");
    $.ajax({
        url: "/TraderPurchases/IssuePurchaseOrder",
        type: "POST",
        dataType: "json",
        data: {
            id: purchaseOrderId,
            emails: emails
        },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                $("#adding-other-mail-addresses").modal('hide');
            }
        },
        error: function (err) {
            $("#sale-order-template").addClass("hidden");
            cleanBookNotification.error(err.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        $('#adding-other-mail-addresses').LoadingOverlay("hide");
    });

};
function OpenIssuePurchaseOrderModal() {
    $("#mail-addresses").text('');
    $("#mail-addresses").val('');
    $("#adding-other-mail-addresses").modal('show');
};
