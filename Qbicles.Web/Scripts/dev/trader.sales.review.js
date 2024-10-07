
var $traderSaleId = $("#TraderSaleId").val();
var $traderSaleKey = $("#TraderSaleKey").val();
//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    CheckStatusApproval(apprKey).then(function (res) {
        if (res.result && res.Object.toLocaleLowerCase() === statusOld.toLocaleLowerCase()) {
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: $("#action_approval").val() },
                success: function (rs) {
                    $.LoadingOverlay("hide");
                    setTimeout(function () {
                        RenderContentUpdated(true, true);
                    }, 100);
                    
                },
                error: function (err) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
            setTimeout(function () {
                window.location.reload();
            }, 1000);
        }
    });
};
function RenderContentUpdated(reload, update) {
    $.LoadingOverlay("show");
    $('#salereview_content').load('/TraderSales/SaleReview?key=' + $traderSaleKey + '&content=' + reload, function () {
        $.LoadingOverlay("hide");
        $('.loadingoverlay').hide();
        if (update)
            cleanBookNotification.updateSuccess();
    });
}

//contact
function ChangeContact(id) {
    $.LoadingOverlay("show");
    $('#sale-review-contact').empty();
    $('#sale-review-contact').load('/TraderSales/TraderSaleReviewContact?id=' + id);
    setTimeout(function () {
        LoadingOverlayEnd();
        $("#app-trader-sale-contact").modal('toggle');
    }, 500);
};

function changeDelivery() {
    $('#contact_address_id').val(0);
    $('.delivery-stored').hide();
    $('.delivery-details').fadeIn();
};

function OnChangeDelivery() {
    if ($('#sales_delivery').val() === "Delivery") {
        $('.delivery-stored').fadeIn();
    } else {
        $('.delivery-stored').hide();
    }
};

function SelectContactChange(ev) {
    if ($(ev).val() === "") return;
    $.ajax({
        type: 'get',
        url: '/TraderSales/GetContactById?id=' + $(ev).val(),
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
    var address = { Id: $('#contact_address_id').val() };

    if (address.Id === '0') {
        address.AddressLine1 = $('#form_contact_address_1').val();
        address.AddressLine2 = $('#form_contact_address_2').val();
        address.City = $('#form_contact_address_city').val();
        address.State = $('#form_contact_address_state').val();
        address.Country = { CommonName: $('#form_contact_address_country').val() }
        address.PostCode = $('#form_contact_address_postcode').val();
    }

    var sale = {
        Id: $traderSaleId,
        DeliveryAddress: address,
        DeliveryMethod: $('#sales_delivery').val(),
        Purchaser: { Id: $('#form_sale_contact').val() }
    }

    $.ajax({
        type: 'post',
        url: '/TraderSales/UpdateTraderSaleContact?countryName=' + $('#form_contact_address_country').val(),
        data: { traderSale: sale },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                var addrInfo = address.AddressLine1 ? address.AddressLine1 + "," : "";
                addrInfo += address.AddressLine2 ? address.AddressLine2 + "," : "";
                addrInfo += address.City ? address.City + "," : "";
                addrInfo += address.State ? address.State + "," : "";
                addrInfo += address.Country ? address.Country.CommonName : "";
                $('#deliveryAddressInfo').text(addrInfo);
                setTimeout(function () {
                    $("#app-trader-sale-contact").modal('hide');
                    $.LoadingOverlay("hide");
                }, 500);
            }
            location.reload();
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
    CheckStatus(id, 'Sale').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object === "PendingReview") {
            $.LoadingOverlay("show");

            $('#sale-review-items').empty();
            $('#sale-review-items').load('/TraderSales/TraderSaleReviewItems?id=' + id, function () {
                LoadingOverlayEnd();
                $("#sale-review-items-modal").modal('toggle');
                // reset item selected
                var workgroupItemId = $("#sale-item-workgroup-id").val();
                var locationItemId = $("#sale-item-location-id").val();
                ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, true, false);
            });
        } else if (res.result && res.Object !== "PendingReview") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");
            window.location.reload();
        }
    });




};


function ChangeSelectedUnit() {
    var currentLocationId = $("#location-id").val();;
    $('#item_selected').empty();

    var itemId = $('#item-select-manage').find(':selected').attr("itemId");

    $('#item_selected').load('/Trader/TraderSaleSelectUnit?idLocation=' + currentLocationId + '&idTraderItem=' + itemId + '&issale=true', function () {
        GetPriceSale(currentLocationId, itemId);
        $('#form_item_quantity').val("0");
        $('#form_item_discount').val("0");
        $('#form_item_dimensions').val([]);
        $('#form_item_dimensions').not('.multi-select').select2({
            placeholder: "Please select."
        });
    });
};
function GetPriceSale(locationId, itemId) {
    if (isNaN(itemId))
        return;
    $.LoadingOverlay("show");
    $.ajax({
        type: "get",
        url: "/TraderSales/GetPriceByLocationIdItemId?locationId=" + locationId + '&itemId=' + itemId,
        dataType: "json",
        success: function (response) {
            LoadingOverlayEnd();
            $("#price_id").val(response.Id);
            $("#price_value").val(response.NetPrice);
            $("#priceSale").val(response.NetPrice);
            selectedUnit($("#conversionsunitid option:first").val());
        },
        error: function (er) {
            LoadingOverlayEnd();
            selectedUnit($("#conversionsunitid option:first").val());
        }
    });
}
function selectedUnit(ev) {
    var val = $('#conversionsunitid').val();
    if (val === null || val === "") {
        $("#cpu").val("");
        //$("#cpu").prop('disabled', true);
        $('#label_cost').text('Cost per <selected unit>');
        $('#label_price').text('Price per <selected unit>');
    }
    else {
        val = $('#conversionsunitid').val().split('|');
        //$("#cpu").prop('disabled', false);
        $('#label_cost').text('Cost per ' + val[2]);
        $('#label_price').text('Price per ' + val[2]);
        var cpu = 0;
        var priceSale = 0;
        if (val[5] === "true") {
            cpu = parseFloat(val[4]);
            priceSale = $("#price_value").val();
            $("#item_selected input.costunitbase").val(cpu);
        } else {
            cpu = parseFloat(val[3]) * parseFloat($('#item_selected input.costunitbase').val());
            priceSale = parseFloat(val[3]) * parseFloat($("#price_value").val());
        }
        if (isNaN(cpu)) {
            cpu = 0;
        }
        if (isNaN(priceSale)) {
            priceSale = 0;
        }
        $("#priceSale").val(priceSale);
        $('#cpu').val(cpu);
    }

    enableBtnAddRowTraderItem(true);
}
function enableBtnAddRowTraderItem() {
    var unitVal = $('#conversionsunitid').val();
    var quantity = parseFloat($('#form_item_quantity').val());
    if (unitVal && quantity > 0)
        $('#btnAddRowItem').prop('disabled', false);
    else
        $('#btnAddRowItem').prop('disabled', true);

}
function addRowItem() {
    var idBuild = UniqueId();
    var dimensionArrayId = $('#form_item_dimensions').val();
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
        SalePricePerUnit: parseFloat($('#priceSale').val()),
        Unit: {},
        PriceBookPrice: {
            Id: $('#price_id').val(),
            Value: $('#price_value').val()
        },
        CostPerUnit: parseFloat($('#cpu').val()),
        Cost: 0,
        Price: 0
    };
    var costunitBase = parseFloat($('#item_selected input.costunitbase').val());

    item.Cost = ((item.CostPerUnit * item.Quantity) * (1 - (item.Discount / 100))).toFixed($decimalPlace);

    var priceIncludeTax = parseFloat(item.SalePricePerUnit) * parseFloat(item.Quantity) * (1 - (parseFloat(item.Discount) / 100)) * (1 + (parseFloat(item.TraderItem.TaxRate) / 100));
    var priceExcludeTax = priceIncludeTax / (1 + (item.TraderItem.TaxRate / 100));
    var priceValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);
    item.Price = priceIncludeTax;


    var clone = $('#tb_form_template tbody tr').clone();
    $(clone).attr('id', 'tr_id_' + itemId);
    // filter to table 
    $($(clone).find('td.row_image div')).attr('style', "background-image: url('" + $('#api_url').val() + item.TraderItem.ImageUri + "');");
    $($(clone).find('td.row_name')).text(item.TraderItem.Name);
    $($(clone).find('td.row_unit')).empty();
    var unitClone = $('#item_selected select').clone();
    $($(clone).find('td.row_unit')).append(unitClone);
    var htmlUnit = "<input type=\"hidden\" class=\"costunitbase\" value=\"" + costunitBase + "\" />";
    $($(clone).find('td.row_unit')).append(htmlUnit);
    $($(clone).find('td.row_unit select')).val(units);
    $($(clone).find('td.row_unit select')).attr('onchange', "rowUnitChange('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_costperunit input')).val(item.CostPerUnit);
    $($(clone).find('td.row_costperunit input')).attr('onchange', "UpdateCost('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_costprice input')).val(item.SalePricePerUnit);
    //$($(clone).find('td.row_costprice input')).attr('onchange', "UpdateCost('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_quantity input')).val(item.Quantity);
    $($(clone).find('td.row_quantity input')).attr('onchange', "UpdateCost('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_discount input')).val(item.Discount);
    $($(clone).find('td.row_discount input')).attr('onchange', "UpdateCost('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_taxrate')).text(item.TraderItem.TaxRate);

    $($(clone).find('td.row_taxname')).html(showTaxRatesDetail(item.SalePricePerUnit, item.Quantity, item.Discount, item.TraderItem.TaxName));

    $($(clone).find('td.row_dimensions select')).val(dimensionArrayId);
    $($(clone).find('td.row_cost')).text(toCurrencySymbol(item.Cost, false));
    $($(clone).find('td.row_price input')).val(item.Price.toFixed($decimalPlace)).attr('onchange', 'UpdateSaleItemReviewedPrice(' + itemId + ')');
    $($(clone).find('td.row_button button')).attr('onclick', "removeRowItem('" + '#tr_id_' + itemId + "')");
    $($(clone).find('td.row_button input.traderItem')).val(itemId);
    $($(clone).find('td.row_button input.row_id')).val(item.Id);
    $($(clone).find('td.row_button input.price_id')).val(item.PriceBookPrice.Id);
    $($(clone).find('td.row_button input.price_Value')).val(item.PriceBookPrice.Value);
    $($(clone).find('td select')).not('.multi-select').select2();
    var productItem = $('#tb_form_item tbody tr#' + 'tr_id_' + itemId);
    if (productItem.length === 0)
        $('#tb_form_item tbody').append(clone);
    else {
        cleanBookNotification.error("This item already exists.", "Qbicles");
    }
    setTimeout(function () {
        $('#form_add_transaction')[0].reset();
        $('#form_add_transaction select').not('.multi-select').select2();
        $('#conversionsunitid').val("");
        $('#conversionsunitid').select2({
            placeholder: "Please select"
        });
    }, 100);

    setTimeout(function () {
        var workgroupItemId = $("#sale-item-workgroup-id").val();
        var locationItemId = $("#sale-item-location-id").val();
        ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, true, false);
    }, 200);
};

//Only discount + total value can be changed
function UpdateSaleItemReviewedPrice(itemId) {
    // Values
    var quantity = $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_quantity input').val();
    var discount = isNaN($('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val())
        ? 0 : $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val();
    var taxRate = $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_taxrate').text();
    var taxName = $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_taxname .txt-taxname').val();
    var totalCost = $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_price input').val();
    var costPerUnit = $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_costprice input').val();

    if (!isNaN(quantity) && !isNaN(discount) && !isNaN(totalCost) && quantity != 0 && discount != 100 && taxRate != -100) {
        var newDiscount = discount;
        if (totalCost == 0) {
            newDiscount = 100;
            $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val(newDiscount);
        } else {
            var actualTotalCost = Number(quantity) * Number(costPerUnit) * (1 + taxRate / 100);
            newDiscount = ((1 - Number(totalCost) / actualTotalCost) * 100).toFixed($decimalPlace);
            $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val(newDiscount);
        }
        // Re-Calculate Tax name
        $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_taxname').html(showTaxRatesDetail(costPerUnit, quantity, newDiscount, taxName));
    }
}


function rowUnitChange(id) {
    if (!isNaN(parseFloat(id.toString()))) {
        id = '#tr_id_' + id;
    }
    var item = $('#tb_form_item tbody tr' + id + ' td.row_button input.traderItem').val().split('|');
    var val = $('#tb_form_item tbody tr' + id + ' td.row_unit select').val().split('|');
    if (val[1] === "BaseUnit") {
        $('#tb_form_item tbody tr' + id + ' td.row_costperunit input').val(item[3]);
    } else if (val[1] === "Conversion Unit") {
        $('#tb_form_item tbody tr' + id + ' td.row_costperunit input').val(parseFloat(item[3]) * parseFloat(val[3]));
    }
    var cpu = 0;
    var priceSale = 0;
    if (val[5] === "true") {

        cpu = parseFloat(val[4]);
        priceSale = $('#tb_form_item tbody tr' + id + ' td.row_button input.price_Value').val();
        $('#tb_form_item tbody tr' + id + ' td.row_unit input.costunitbase').val(cpu);
    } else {
        cpu = parseFloat(val[3]) * parseFloat($('#tb_form_item tbody tr' + id + ' td.row_unit input.costunitbase').val());
        priceSale = parseFloat(val[3]) * parseFloat($('#tb_form_item tbody tr' + id + ' td.row_button input.price_Value').val());
    }
    $('#tb_form_item tbody tr' + id + ' td.row_costperunit input').val(cpu);
    $('#tb_form_item tbody tr' + id + ' td.row_costprice input').val(priceSale);
    UpdateCost(id);
};
function UpdateCost(id) {
    var salePricePerUnit = $('#tb_form_item tbody tr' + id + ' td.row_costprice input').val();
    var quantity = $('#tb_form_item tbody tr' + id + ' td.row_quantity input').val();
    var discount = $('#tb_form_item tbody tr' + id + ' td.row_discount input').val();
    var taxRate = $('#tb_form_item tbody tr' + id + ' td.row_taxrate').text();
    if (isNaN(parseFloat(salePricePerUnit)) || isNaN(parseFloat(quantity)) || isNaN(parseFloat(discount)) || isNaN(parseFloat(taxRate))) {
        return;
    }

    var priceIncludeTax = parseFloat(salePricePerUnit) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + parseFloat(taxRate) / 100);
    //var priceExcludeTax = priceIncludeTax / (1 + (taxRate / 100));
    //var taxValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);
    $('#tb_form_item tbody tr' + id + ' td.row_cost').text(toCurrencySymbol(priceIncludeTax, false));
    $('#tb_form_item tbody tr' + id + ' td.row_price input').val(priceIncludeTax.toFixed($decimalPlace));
    var staxtnameinit = $('#tb_form_item tbody tr' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr' + id + ' td.row_taxname').html(showTaxRatesDetail(salePricePerUnit, quantity, discount, staxtnameinit));
};

function removeRowItem(id) {
    $('#tb_form_item tbody tr' + id).remove();
    var workgroupItemId = $("#sale-item-workgroup-id").val();
    var locationItemId = $("#sale-item-location-id").val();
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, true, false);
};

function UpdateItems() {
    $.LoadingOverlay("show");
    CheckStatus($traderSaleId, 'Sale').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object === "PendingReview") {

            var saleItems = [];
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
                    if (dimens.length === 0) {
                        cleanBookNotification.error(_L("ERROR_MSG_635"), "Qbicles");
                        return false;
                    }
                    var unit = $($(items[i]).find('td.row_unit select')).val().split('|');
                    var saleItem = {
                        Id: isNaN(id) ? 0 : id,
                        TraderItem: item,
                        Discount: stringToNumber($($(items[i]).find('td.row_discount input')).val()),
                        Dimensions: dimens,
                        Quantity: stringToNumber($($(items[i]).find('td.row_quantity input')).val()),
                        Unit: { Id: unit[1] },
                        CostPerUnit: stringToNumber($(items[i]).find('td.row_costperunit input').val()),
                        SalePricePerUnit: stringToNumber($($(items[i]).find('td.row_costprice input')).val()),
                        Cost: stringToNumber($($(items[i]).find('td.row_cost')).text()),
                        Price: stringToNumber($($(items[i]).find('td.row_price input')).val()),
                        PriceBookPrice: {
                            Id: $($(items[i]).find('td.row_button input.price_id')).val()
                        },
                        PriceBookPriceValue: $($(items[i]).find('td.row_button input.price_Value')).val()
                    }
                    saleItems.push(saleItem);
                }
            }
            $.LoadingOverlay("show");
            var sale = {
                Id: $traderSaleId,
                SaleItems: saleItems,
                SaleTotal: SaleTotal()
            }
            $.ajax({
                type: 'post',
                url: '/TraderSales/UpdateTraderSaleItems',
                data: { traderSale: sale },
                dataType: 'json',
                success: function (response) {

                    if (response.actionVal === 1) {
                        $('#table-sale-review-items-preview').empty();
                        $('#table-sale-review-items-preview').load('/TraderSales/TraderSaleReviewItemsPreview?id=' + $traderSaleId);
                        setTimeout(function () {
                            LoadingOverlayEnd();
                            $("#sale-review-items-modal").modal("hide");
                        }, 500);
                        $('#sale-total h3').text(toCurrencySymbol(SaleTotal()));
                    }
                    else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });


        } else if (res.result && res.Object !== "PendingReview") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");
            window.location.reload();
        }
    });

};


SaleTotal = function () {
    var trs = $('#tb_form_item tbody tr');
    var trd = $('#tb_form_item tbody tr td');
    var total = 0;
    if (trs.length === 1 && trd.length === 1) return 0;
    for (var i = 0; i < trs.length; i++) {
        total += stringToNumber($($(trs[i]).find('td.row_price input')).val());
    }
    return total;
};