var $editMode = "Sale";
var $transferRequirement = "";
var $traderSaleId = $("#TraderSaleId").val();

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
                $.LoadingOverlay("hide");
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
                $('#sale-review-contact-preview').empty();
                $('#sale-review-contact-preview').load('/TraderSales/TraderSaleReviewContactPreview?id=' + $traderSaleId);
                setTimeout(function () {
                    $("#app-trader-sale-contact").modal('hide');
                    $.LoadingOverlay("hide");
                }, 500);
            }
            LoadingOverlayEnd();
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
                var workgroupItemId = $("#sale-item-workgroup-id").val();
                var locationItemId = $("#sale-item-location-id").val();
                ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, true, false);
                $("#sale-review-items-modal").modal('toggle');
                LoadingOverlayEnd();
            });
        } else if (res.result && res.Object !== "PendingReview") {
            cleanBookNotification.error(_L("ERROR_MSG_647"), "Qbicles");
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
            window.location.reload();
        }
    });
};




function ChangeSelectedUnit() {
    var currentLocationId = $("#location-id").val();
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
        error: function () {
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

};
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
        CostPerUnit: parseFloat($('#cpu').val().replace(/\,/g, "")),
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
    $($(clone).find('td.row_image div')).attr('style', "background-image: url('" + $('#api_url').val() + item.TraderItem.ImageUri + "&size=T');");
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
        cleanBookNotification.error(_L("ERROR_MSG_373"), "Qbicles");
    }
    setTimeout(function () {
        $('#form_add_transaction')[0].reset();
        $('#form_add_transaction select').not('.multi-select').select2();
        $('#conversionsunitid').val("");
        $('#conversionsunitid').select2({
            placeholder: "Please select"
        });
    }, 100);
    $("#btnAddRowItem").prop('disabled', true);
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
    //var priceExcludeTax = priceIncludeTax / (1 + taxRate);
    //var taxValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);
    $('#tb_form_item tbody tr' + id + ' td.row_cost').text(toCurrencyDecimalPlace(priceIncludeTax))
    $('#tb_form_item tbody tr' + id + ' td.row_price input').val(priceIncludeTax.toFixed($decimalPlace));
    var staxtnameinit = $('#tb_form_item tbody tr' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr' + id + ' td.row_taxname').html(showTaxRatesDetail(salePricePerUnit, quantity, discount, staxtnameinit));
};

function updatePriceCost(id) {
    var salePricePerUnit = $('#tb_form_item tbody tr' + id + ' td.row_costprice input').val();
    var quantity = $('#tb_form_item tbody tr' + id + ' td.row_quantity input').val();
    var discount = $('#tb_form_item tbody tr' + id + ' td.row_discount input').val();
    var taxRate = $('#tb_form_item tbody tr' + id + ' td.row_taxrate').text();
    if (isNaN(parseFloat(salePricePerUnit)) || isNaN(parseFloat(quantity)) || isNaN(parseFloat(discount)) || isNaN(parseFloat(taxRate))) {
        return;
    }

    var priceIncludeTax = parseFloat(salePricePerUnit) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + parseFloat(taxRate));
    //var priceExcludeTax = priceIncludeTax / (1 + taxRate);
    //var taxValue = parseFloat(priceIncludeTax - priceExcludeTax).toFixed($decimalPlace);

    $('#tb_form_item tbody tr' + id + ' td.row_cost').text(toCurrencySymbol(priceIncludeTax, false))
    $('#tb_form_item tbody tr' + id + ' td.row_price input').text(priceIncludeTax.toFixed($decimalPlace));
    var staxtnameinit = $('#tb_form_item tbody tr' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr' + id + ' td.row_taxname').html(showTaxRatesDetail(salePricePerUnit, quantity, discount, staxtnameinit));

}
function removeRowItem(id) {
    $('#tb_form_item tbody tr' + id).remove();
    var workgroupItemId = $("#sale-item-workgroup-id").val();
    var locationItemId = $("#sale-item-location-id").val();
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, workgroupItemId, locationItemId, false, true, false);
};

function UpdateItems() {

    $.LoadingOverlay("show");
    var id = $('#TraderSaleId').val();
    CheckStatus(id, 'Sale').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object === "PendingReview") {
            $.LoadingOverlay("show");
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
                    var quantity = $($(items[i]).find('td.row_quantity input')).val();
                    if (parseFloat(quantity) <= 0) {
                        $.LoadingOverlay("hide");
                        cleanBookNotification.error(_L("ERROR_MSG_637"), "Qbicles");
                        return;
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
                        return;
                    }
                    var unit = $($(items[i]).find('td.row_unit select')).val().split('|');

                    var saleItem = {
                        Id: isNaN(id) ? 0 : id,
                        TraderItem: item,
                        Discount: stringToNumber($($(items[i]).find('td.row_discount input')).val()),
                        Dimensions: dimens,
                        Quantity: stringToNumber(quantity),
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
                        LoadingOverlayEnd();
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                    else {
                        LoadingOverlayEnd();
                    }
                },
                error: function (er) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else if (res.result && res.Object !== "PendingReview") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
        } else if (res.result === false) {
            cleanBookNotification.error(_L("ERROR_MSG_380", res.msg), "Qbicles");

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

function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#transfer-workgroup-select").val());
};

function InitTransfer(key) {
    $('#app-trader-sale-transfer').empty();
    $('#app-trader-sale-transfer').load("/TraderTransfers/InitSaleTransfer?key=" + key);
};

function EditTransfer(id, saleKey) {
    $('#app-trader-sale-transfer').empty();
    $('#app-trader-sale-transfer').load("/TraderTransfers/EditSaleTransfer?id=" + id + '&keySale=' + saleKey);
};

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
                LoadingOverlayEnd();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};


function SendToPickup(status) {
    var $workgroup = {};
    $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
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
        Reference: Reference,
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
            LoadingOverlayEnd();
            reloadTableTransfer();
            $("#app-trader-sale-transfer").modal("hide");
            if (response.actionVal === 2) {
                $("#app-trader-purchase-transfer").modal("hide");
                cleanBookNotification.updateSuccess();
            } else if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
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
    $('#table_transfer').load("/TraderTransfers/GetTableTransfer?id=" + $("#TraderSaleId").val() + '&callBack=true');
};

// Invoice 

function checknumber(e, evt, value) {
    var keyCode = evt.which;
    var text = $(e).val().trim();
    var zeronumber = false;
    if (keyCode === 48) zeronumber = true;
    if ((keyCode === 46 && text.indexOf('.') > 0)) {
        evt.preventDefault();
    } else {
        if (!((keyCode >= 48 && keyCode <= 57) || (keyCode === 8 || keyCode === 32 || keyCode === 44 || keyCode === 46))) {
            evt.preventDefault();
        } else if (keyCode === 44 && text.indexOf('.') > 0) {
            evt.preventDefault();
        } else {
            if (!ChangeInvoiceValue(e, value, 0)) {
                evt.preventDefault();
            }
        }
    }
    /*
      8 - (backspace)
      32 - (space)
      48-57 - (0-9)Numbers
    */
    //if ((keyCode != 8 || keyCode == 32) && (keyCode < 48 || keyCode > 57)) {
    //    evt.preventDefault();
    //}

};

function AddInvoice(id, saleKey) {
    $.LoadingOverlay("show");
    $('#app-trader-invoice-add').empty();
    $('#app-trader-invoice-add').load("/TraderSales/InitSaleInvoice?id=" + id + '&saleKey=' + saleKey, function () {
        initTableInvoice(saleKey, id);
        LoadingOverlayEnd();
    });
}
//Thomas refactor code Table bill
function initTableInvoice(saleKey, invoiceKey) {
    var $tablebill = $('#table_invoiceitems').DataTable({
        "ajax": "/TraderSales/GetInvoiceItemsFromSale?saleKey=" + saleKey + "&invoiceId=" + invoiceKey,
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
            {
                "data": "PricePerUnit",
                "render": function (data, type, row, meta) {
                    return toCurrencyDecimalPlace(data);
                }
            },
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
                    return '<span id="ivTaxesVal-' + row.TransItemId + '">' + toCurrencyDecimalPlace(data) + '</span>';
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
                var rowData = $('#table_invoiceitems').DataTable().row($row).data();
                if ($(this).prop('checked'))
                    rowData.InvoiceChecked = true;
                else
                    rowData.InvoiceChecked = false;
            })
            $(".trackQuantity").on("change", function () {
                var $row = $(this).parents("tr");
                var $objectRow = $('#table_invoiceitems').DataTable().row($row);
                var rowData = $objectRow.data();
                let quantityInput = $(this).val().replace(/\,/g, "");
                rowData.InvoiceQuantity = parseFloat(quantityInput ? quantityInput : 0.00);
                if (rowData.InvoiceQuantity > rowData.TransItemQuantity) {
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
    });
}

function EditInvoice(id, saleKey) {
    $.LoadingOverlay("show");
    $('#app-trader-invoice-add').empty();
    $('#app-trader-invoice-add').load("/TraderSales/InitSaleInvoice?id=" + id + '&saleKey=' + saleKey, function () {
        initTableInvoice(saleKey, id);
        LoadingOverlayEnd();
    });
}
function SaveInvoice(status) {

    var $workgroup = {};
    $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return false;
    }
    $.LoadingOverlay("show");
    var mydate = moment($('#invoice_dueDate').val(), $dateFormatByUser.toUpperCase());
    var dueDateChanged = moment(mydate).format("YYYY/MM/DD");
    var reference = {
        Id: $('#referenceInvoice_id').val(),
        NumericPart: parseFloat($('#refeditInvoice').text()),
        Type: $('#referenceInvoice_type').val(),
        Prefix: $('#referenceInvoice_prefix').val(),
        Suffix: $('#referenceInvoice_suffix').val(),
        Delimeter: $('#referenceInvoice_delimeter').val(),
        FullRef: $('#referenceInvoice_fullref').val()
    };
    var invoice = {
        Key: $('#invoice_key').val(),
        Status: status,
        Sale: { Key: $('#sale_key').val() },
        InvoiceItems: [],
        DueDate: dueDateChanged,
        Reference: reference,
        InvoiceAddress: $('#invoice_address').val(),
        PaymentDetails: $('#invoice_paymentdetails').val(),
        Workgroup: $workgroup
    };
    //Thomas:fix bug https://atomsinteractive.atlassian.net/browse/QBIC-3182
    var $tablebill = $('#table_invoiceitems').DataTable();
    $tablebill.rows().every(function () {
        const _row = this.data();
        if (_row.InvoiceChecked) {
            var invoiceTransaction = {
                Id: _row.Id,
                TransactionItem: { Id: _row.TransItemId },
                InvoiceValue: _row.InvoiceCost,
                InvoiceDiscountValue: _row.InvoiceDiscount,
                InvoiceItemQuantity: _row.InvoiceQuantity,
                InvoiceTaxValue: _row.InvoiceTaxes
            }
            if (!invoiceTransaction.InvoiceValue || invoiceTransaction.InvoiceValue === "") {
                invoiceTransaction.InvoiceValue = 0;
            }
            invoice.InvoiceItems.push(invoiceTransaction);
        }
    });
    //var tr = $('#table_invoiceitems tbody tr');
    //var td = $('#table_invoiceitems tbody tr td');
    //if (tr.length > 0 && td.length > 1) {
    //    for (var i = 0; i < tr.length; i++) {
    //        var ischeck = $($(tr[i]).find('input.invoice_checked')).checked;
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
    //            invoice.InvoiceItems.push(invoiceTransaction);
    //        }
    //    }
    //}

    $.ajax({
        type: "post",
        url: "/TraderSales/SaveSaleInvoice?saleKey=" + $('#sale_key').val(),
        data: { invoice: invoice },
        dataType: "json",
        success: function (response) {
            LoadingOverlayEnd();
            $('#app-trader-invoice-add').modal('toggle');
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            }
            reloadTableInVoice();
        },
        error: function (er) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

//function ChangeInvoiceValue(ev, id, value) {
//    var invoiceQuantity = stringToNumber($(ev).val());
//    var quantity = stringToNumber($('span.quantity_value_' + id).text());
//    invoiceQuantity = invoiceQuantity ? invoiceQuantity : 0;
//    if (invoiceQuantity > quantity) {
//        invoiceQuantity = quantity;
//        $(ev).val(invoiceQuantity);
//    }
//    var discount = stringToNumber($('span.discount_value_' + id).text());
//    var taxRate = stringToNumber($('span.taxrate_value_' + id).text());
//    var invoicePrice = value * invoiceQuantity;
//    var invoiceDisCount = invoicePrice * (discount / 100);
//    var invoiceTaxValue = (invoicePrice - invoiceDisCount) * taxRate;
//    var invoiceValue = invoicePrice * (1 - (discount / 100)) * (1 + taxRate);
//    $('td.invoice_td_value_' + id + ' span.invoice_value').text(toCurrencySymbol(invoiceValue, false));
//    $('span.invoice_discountValue_' + id).text(toCurrencySymbol(invoiceDisCount, false));
//    $('span.invoice_taxvalue_' + id).text(toCurrencySymbol(invoiceTaxValue, false));
//    $('span.invoice_taxvalue_' + id).hide();
//    value = value ? value : 0;
//    $('#taxInvoive_' + id).show();
//    $('#taxInvoive_' + id).html(showTaxRatesDetail(value, invoiceQuantity, discount, $('#taxInvoive_' + id + ' input.txt-taxname').val()));
//    return true;
//};

function reloadTableInVoice() {
    $('#table_invoice').empty();
    $('#table_invoice').load("/TraderSales/GetTableInvoice?key=" + $("#TraderSaleKey").val() + '&callBack=true');
};


// Sale order
function ClearText() {
    $("#sale-order-additions").val("");
};

function SaveSaleOrder(saleId) {

    $.LoadingOverlay("show");    
    var Reference = {
        Id: $('#referenceSale_id').val(),
        NumericPart: parseFloat($('#refeditSale').text()),
        Type: $('#referenceSale_type').val(),
        Prefix: $('#referenceSale_prefix').val(),
        Suffix: $('#referenceSale_suffix').val(),
        Delimeter: $('#referenceSale_delimeter').val(),
        FullRef: $('#referenceSale_fullref').val()
    }
    var saleOrder = {
        Sale: {
            Id: saleId
        },
        AdditionalInformation: $("#sale-order-additions").val(),
        Reference: Reference
    };


    $.ajax({
        type: 'post',
        url: "/TraderSales/SaveSaleOrder",
        datatype: 'json',
        data: saleOrder,
        success: function (refModel) {
            LoadingOverlayEnd();
            if (refModel.result) {
                cleanBookNotification.createSuccess();
                setTimeout(function () {
                    window.location.href = "/TraderSales/SaleOrder?id=" + refModel.msgId;
                }, 1000);

            }
        }, error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};

function IssueSaleOrder(saleOrderId) {
    var emails = $("#mail-addresses").val();
    if (emails.length > 0) {
        var invalid = validateEmails(emails);
        if (invalid.length > 0) {
            cleanBookNotification.error("Email format is not correct:'\n'" + invalid, "Qbicles");

            return;
        }
    }
    $('#adding-other-mail-addresses').LoadingOverlay("show");
    setTimeout(function () {
        $("#sale-order-template").removeClass("hidden");
        $.ajax({
            url: "/TraderSales/IssueSaleOrder",
            type: "POST",
            dataType: "json",
            data: {
                id: saleOrderId,
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
    }, 500);

};
function OpenIssueSaleOrderModal() {
    $("#mail-addresses").text('');
    $("#mail-addresses").val('');
    $("#adding-other-mail-addresses").modal('show');
};
