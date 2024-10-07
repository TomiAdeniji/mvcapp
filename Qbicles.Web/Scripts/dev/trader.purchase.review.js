
var $traderPurchaseId = $("#TraderPurchaseId").val();
//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    //CheckStatusApproval(apprKey).then(function (res) {
    //LoadingOverlayEnd();
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
            LoadingOverlayEnd();
            if (rs.actionVal > 0) {
                RenderContentUpdated(true);
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
    // and apply
    //    } else {
    //        cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
    //        setTimeout(function () {
    //            RenderContentUpdated();
    //        }, 1500);
    //    }
    //});
};

function RenderContentUpdated(showMess) {
    LoadingOverlay();
    $('#purchasereview_content').empty();
    $('#purchasereview_content').load('/TraderPurchases/PurchaseReviewContent?id=' + $traderPurchaseId, function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);
    });
}




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
                //$('#purchase-review-contact-preview').empty();
                //$('#purchase-review-contact-preview').load('/TraderPurchases/TraderPurchaseReviewContactPreview?id=' + $traderPurchaseId + '&display=' + $('#display_value').val());
                $('#purchase-contact-preview').empty();
                $('#purchase-contact-preview').append(response.msg);
            }
            setTimeout(function () {
                $("#app-trader-purchase-contact").modal('toggle');
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
        if (res.result && res.Object != "PurchaseApproved") {
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

        } else if (res.result && res.Object == "PurchaseApproved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result == false) {
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
    $($(clone).find('td.row_image div')).attr('style', "background-image: url('" + $('#api_url').val() + item.TraderItem.ImageUri + "');");
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

    var priceIncludeTax = parseFloat(costofper) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + (parseFloat(taxRate) / 100));

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

};

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
function UpdateItems() {
    $.LoadingOverlay("show");
    CheckStatus($traderPurchaseId, 'Purchase').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object != "PurchaseApproved") {
            // load form update
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
                        $.LoadingOverlay("hide");
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
                        $('#table-purchase-review-items-preview').load('/TraderPurchases/TraderPurchaseReviewItemsPreview?id=' + $traderPurchaseId, function () {
                            LoadingOverlayEnd();
                        });
                        $("#purchase-total").text(purchase.PurchaseTotal.toLocaleString());

                        $("#purchase-review-items-modal").modal("hide");
                        $('#purchase-items-total-price h3').text(toCurrencySymbol(PurchaseTotal()));
                    }
                    else if (response.actionVal === 3) {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
                }
            });

        } else if (res.result && res.Object == "PurchaseApproved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
        }
    });



};


PurchaseTotal = function () {
    var trs = $('#tb_form_item tbody tr');
    var trd = $('#tb_form_item tbody tr td');
    var total = 0;
    if (trs.length === 1 && trd.length === 1) return total;
    for (var i = 0; i < trs.length; i++) {
        total += stringToNumber($($(trs[i]).find('td.row_cost input')).val());
    }
    return total;
};