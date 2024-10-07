$(function () {
    $("#subfilter-group").select2();
    $("#subfilter-channel").select2();
    $("#subfilter-group").on("change", delay(function () {
        callBackDataTableTraderSale();
    }, 0));
    $("#search_dt").keyup(delay(function () {
        callBackDataTableTraderSale();
    },
        2000));
    $('.manage-columns input[type="checkbox"]').on('change',
        function () {
            var table = $('#tb_trader_sales').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
    $("#subfilter-channel").on("change", delay(function () {
        callBackDataTableTraderSale();
    },
        0));
    $('.datetimerange').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        startDate: new Date($("#fromDateTime").val()),
        endDate: new Date($("#toDateTime").val()),
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
    $('.datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('.datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        filter.DateRange = $("#sale-input-datetimerange").val();
        callBackDataTableTraderSale();
    });
    $('.datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        filter.DateRange = $("#sale-input-datetimerange").val();
        $('.datetimerange').html('full history');
        callBackDataTableTraderSale();
    });
    LoadDataSales();
    //ShowTableSaleValue();
});
function onSelectWorkgroup(ev) {
    filter.Workgroup = $(ev).val();
    setTimeout(function () { searchOnTable(); }, 200);
}

function OnSaleChanelChange(ev) {
    filter.SaleChanel = $(ev).val();
    setTimeout(function () { searchOnTable(); }, 200);
}
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();;
    setTimeout(function () { searchOnTable(); }, 200);
}
function searchOnTable() {
    var listKey = [];
    if ($('#subfilter-group').val() !== "" && $('#subfilter-group').val() !== null) {
        listKey.push($('#subfilter-group').val());
    }
    var keys = $('#search_dt').val();
    if ($('#search_dt').val() !== "" && $('#search_dt').val() !== null && keys && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#tb_trader_sales").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#tb_trader_sales_filter input").val("");
}

function addSale() {
    $.LoadingOverlay("show");
    $('#app-trader-sale-add').empty();
    $('#app-trader-sale-add').load('/TraderSales/TraderSaleAdd?locationId=' + $('#local-manage-select').val(), function () {
        $('#tb_form_item').on("draw.dt", function () {
            $(this).find(".dataTables_empty").parents('tbody').empty();
        }).DataTable(
            {
                "destroy": true,
                "paging": false,
                "searching": false,
                "responsive": true,
                "ordering": false,
                "info": false
            });
        LoadingOverlayEnd();
    });
}

function editSale(tradersaleKey) {
    $.LoadingOverlay("show");
    $('#app-trader-sale-add').empty();
    $('#app-trader-sale-add').load('/TraderSales/TraderSaleAdd?locationId=' + $('#local-manage-select').val() + '&tradersaleKey=' + tradersaleKey, function () {
        $('#app-trader-sale-add').modal('toggle');
        $('#tb_form_item').on("draw.dt", function () {
            $(this).find(".dataTables_empty").parents('tbody').empty();
        }).DataTable(
            {
                "destroy": true,
                "paging": false,
                "searching": false,
                "responsive": true,
                "ordering": false,
                "info": false
            });
        ResetItemSelected('tb_form_item', 'item-select-manage', undefined, undefined, $('#local-manage-select').val(), false, true, false);
        LoadingOverlayEnd();
    });
}


ChangeSaleWorkgroup = function (ev) {
    WorkGroupSelectedChange();
    $('#item-select-manage').empty().trigger("change");
    var wgId = $(ev).val();
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, wgId,
        $('#local-manage-select').val(), false, true, false);
};

function changeContact(ev) {

    if ($(ev).val() === "") return;
    if ($("#sales_delivery").val() === "Delivery") {
        $('.delivery-stored').fadeIn();
    } else {
        $('.delivery-stored').hide();
    }
    $('.delivery-details').hide();
    $.ajax({
        type: 'get',
        url: '/TraderSales/GetContactById?id=' + $(ev).val(),
        dataType: 'json',
        success: function (response) {
            var strContact = response.Name;
            if (response.Address) {
                $('#contact_address_id').val(response.Address.Id);
                if (response.Address.AddressLine1) {
                    $('#form_contact_address_1').val(response.Address.AddressLine1);
                    strContact += ",</br>" + response.Address.AddressLine1;

                }
                if (response.Address.City) {
                    $('#form_contact_address_2').val(response.Address.City);
                    strContact += ",</br>" + response.Address.City;
                }

                if (response.Address.AddressLine2) {
                    $('#form_contact_address_city').val(response.Address.AddressLine2);
                    strContact += ",</br>" + response.Address.AddressLine2;
                }
                if (response.Address.State) {
                    $('#form_contact_address_state').val(response.Address.State);
                    strContact += ",</br>" + response.Address.State;
                }
                if (response.Address.Country) {
                    $('#form_contact_address_country').val(response.Address.Country.CommonName).trigger("change");
                    strContact += ",</br>" + response.Address.Country.CommonName;
                }
                if (response.Address.PostCode) {
                    $('#form_contact_address_postcode').val(response.Address.PostCode);
                    strContact += ",</br>" + response.Address.PostCode;
                }

                $('#address_info').empty();
                $('#address_info').append(strContact);
            }

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
var contact_address_id = $('#contact_address_id').val();
function changeDelivery() {
    contact_address_id = $('#contact_address_id').val();
    $('#contact_address_id').val(0);
}
function usedDeliveryExists() {
    $('.delivery-details').hide(); $('.delivery-stored').show();
    $('#contact_address_id').val(contact_address_id);
}
function nextToItems() {
    $('#form_add_transaction')[0].reset();
}
function ChangeSelectedUnit() {
    if ($('#item-select-manage').val() === null)
        return;
    $('#item_selected').empty();
    var itemId = $('#item-select-manage').find(':selected').attr('itemId');

    var ajaxUri = '/Trader/TraderSaleSelectUnit?idLocation=' + $('#local-manage-select').val() + '&idTraderItem=' + itemId + '&issale=true';
    $('#item_selected').load(ajaxUri, function () {
        GetPriceSale($('#local-manage-select').val(), itemId);
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
            cpu = parseFloat(val[3]);
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
function enableBtnAddRowTraderItem(ev) {

    var unitVal = $('#conversionsunitid').val();
    var quantity = parseFloat($('#form_item_quantity').val());
    if (unitVal && quantity > 0)
        $('#btnAddRowItem').prop('disabled', false);
    else
        $('#btnAddRowItem').prop('disabled', true);

}
function removeRowItem(id) {
    $('#tb_form_item tbody tr' + id).remove();
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, undefined, $('#local-manage-select').val(), false, true, false);
}
function addRowItem() {
    var idBuild = UniqueId();
    var dimensionArrayId = $('#form_item_dimensions').val();
    var units = $('#item_selected select').val();
    var cpu = $('#cpu').val();

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

    var costUnitBase = parseFloat($('#item_selected input.costunitbase').val());

    item.Cost = ((item.CostPerUnit * item.Quantity) * (1 - (item.Discount / 100))).toFixed($decimalPlace);

    //var priceCost = item.SalePricePerUnit * item.Quantity * (1 - (item.Discount / 100)) * (1 + (tax / 100));



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
    var htmlUnit = "<input type=\"hidden\" class=\"costunitbase\" value=\"" + costUnitBase + "\" />";
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

    $($(clone).find('td.row_price input')).val(item.Price.toFixed($decimalPlace)).attr('onchange', 'UpdateSaleItemPrice(' + itemId + ')');

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
        cleanBookNotification.error(_L("ERROR_MSG_646"), "Qbicles");
    }
    setTimeout(function () {
        $('#form_add_transaction')[0].reset();
        $('#form_add_transaction select').not('.multi-select').not('[name="traderitem"]').select2("val", "");
        $('#conversionsunitid').val("");
        $('#conversionsunitid').select2({
            placeholder: "Please select"
        });
    }, 100);
    $("#btnAddRowItem").prop('disabled', true);
    ResetItemSelected('tb_form_item', 'item-select-manage', undefined, undefined, $('#local-manage-select').val(), false, true, false);
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

        cpu = parseFloat(val[3]);
        priceSale = $('#tb_form_item tbody tr' + id + ' td.row_button input.price_Value').val();
        $('#tb_form_item tbody tr' + id + ' td.row_unit input.costunitbase').val(cpu);
    } else {
        cpu = parseFloat(val[3]) * parseFloat($('#tb_form_item tbody tr' + id + ' td.row_unit input.costunitbase').val());
        priceSale = parseFloat(val[3]) * parseFloat($('#tb_form_item tbody tr' + id + ' td.row_button input.price_Value').val());
    }
    $('#tb_form_item tbody tr' + id + ' td.row_costperunit input').val(cpu);
    $('#tb_form_item tbody tr' + id + ' td.row_costprice input').val(priceSale);
    UpdateCost(id);

}

//Only discount + total value can be changed
function UpdateSaleItemPrice(itemId, elementChanged) {
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
            var newDiscount = 100;
            $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val(newDiscount);
        } else {
            // Re-Calculate discount
            var actualTotalCost = Number(quantity) * Number(costPerUnit) * (1 + Number(taxRate) / 100);
            var newDiscount = ((1 - Number(totalCost) / actualTotalCost) * 100).toFixed($decimalPlace);
            $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_discount input').val(newDiscount);
        }

        // Re-Calculate Tax name
        $('#tb_form_item tbody tr#tr_id_' + itemId + ' td.row_taxname').html(showTaxRatesDetail(costPerUnit, quantity, newDiscount, taxName));
    }
}

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
    $('#tb_form_item tbody tr' + id + ' td.row_cost').text(toCurrencySymbol(priceIncludeTax, false));
    $('#tb_form_item tbody tr' + id + ' td.row_price input').val(priceIncludeTax.toFixed($decimalPlace));
    var staxtnameinit = $('#tb_form_item tbody tr' + id + ' td.row_taxname input.txt-taxname').val();
    $('#tb_form_item tbody tr' + id + ' td.row_taxname').html(showTaxRatesDetail(salePricePerUnit, quantity, discount, staxtnameinit));
}
function nextToConfirm() {
    $('#tb_confirm tbody').empty();
    var trs = $('#tb_form_item tbody tr');
    var trd = $('#tb_form_item tbody tr td');
    var total = 0;
    if (trs.length === 1 && trd.length === 1) return;
    for (var i = 0; i < trs.length; i++) {
        var cost = stringToNumber($($(trs[i]).find('td.row_price input')).val());
        total += cost;
        var strTrs = "<tr> <td>" + $($(trs[i]).find('td.row_image')).html() + "</td>";
        strTrs += "<td>" + $($(trs[i]).find('td.row_name')).text() + "</td> <td>" + $($(trs[i]).find('td.row_unit select option:selected')).text() + "</td>";
        strTrs += "<td>" + $($(trs[i]).find('td.row_costprice input')).val() + "</td>";
        strTrs += " <td>" + $($(trs[i]).find('td.row_quantity input')).val() + "</td> " +
            "<td>" + $($(trs[i]).find('td.row_discount input')).val() + "%</td> " +
            "<td>" + $($(trs[i]).find('td.row_taxname')).html() + "</td>";
        strTrs += "<td>";
        var dimensions = $($(trs[i]).find('td.row_dimensions select option:selected'));
        if (dimensions.length > 0) {
            for (var j = 0; j < dimensions.length; j++) {
                strTrs += "<span class=\"label label-info label-lg\">" + $(dimensions[j]).text() + "</span> ";
            }
        }
        strTrs += "</td>";
        strTrs += "<td>" + $($(trs[i]).find('td.row_price input')).val() + "</td> </tr>";
        $('#tb_confirm tbody').append(strTrs);
    }
    $('#total_id').text(toCurrencySymbol(total));
    validateSendToReview();
}
function validateSendToReview() {

    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length > 0 && tds.length > 1) {
        $('#a_send_toreview_sale').removeAttr('disabled');
        $('#a_send_toreview_sale').removeClass('disabled');
    } else {
        $('#a_send_toreview_sale').attr('disabled');
        $('#a_send_toreview_sale').addClass('disabled');
    }
}
//// Save Trader Sale  /////
function SaveSaleDraft() {
    SaveSale("Draft");
}
function SaveSaleReview() {
    SaveSale("PendingReview");
}

function SaveSale(status) {
    var $workgroup = {
        Id: $("#trader_sale_add_workgroup").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null) {
        $('.admintabs a[href="#sale-1"]').tab('show');
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        return;
    }
    var $contact = $("#form_sale_contact").val();
    if ($contact === "" || $contact === null) {
        $('.admintabs a[href="#sale-1"]').tab('show');
        cleanBookNotification.error(_L("ERROR_MSG_644"), "Qbicles");
        return;
    }
    var address = { Id: $('#contact_address_id').val() };
    var Reference = {
        Id: $('#reference_id').val(),
        NumericPart: parseFloat($('#refedit').text()),
        Type: $('#reference_type').val(),
        Prefix: $('#reference_prefix').val(),
        Suffix: $('#reference_suffix').val(),
        Delimeter: $('#reference_delimeter').val(),
        FullRef: $('#reference_fullref').val()
    }
    if (address.Id === '0') {
        address.AddressLine1 = $('#form_contact_address_1').val();
        address.AddressLine2 = $('#form_contact_address_2').val();
        address.City = $('#form_contact_address_city').val();
        address.State = $('#form_contact_address_state').val();
        address.Country = { CommonName: $('#form_contact_address_country').val() }
        address.PostCode = $('#form_contact_address_postcode').val();
    }
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
            var quantity = $($(items[i]).find('td.row_quantity input'));
            if (parseFloat(quantity.val()) <= 0) {
                cleanBookNotification.error(_L("ERROR_MSG_637"), "Qbicles");
                $('.admintabs a[href="#sale-2"]').tab('show');
                return;
            }
            var dimensions = $($(items[i]).find('td.row_dimensions select')).val();
            var dimens = [];
            if (dimensions && dimensions.length > 0) {
                for (var j = 0; j < dimensions.length; j++) {
                    dimens.push({ Id: dimensions[j] });
                }
            }
            if (dimens.length === 0 && status === "PendingReview") {
                cleanBookNotification.error(_L("ERROR_MSG_635"), "Qbicles");
                $('.admintabs a[href="#sale-2"]').tab('show');
                return;
            }

            var unit = $($(items[i]).find('td.row_unit select')).val().split('|');

            var saleItem = {
                Id: isNaN(id) ? 0 : id,
                TraderItem: item,
                Discount: $($(items[i]).find('td.row_discount input')).val(),
                Dimensions: dimens,
                Quantity: $($(items[i]).find('td.row_quantity input')).val(),
                Unit: { Id: unit[1] },
                CostPerUnit: stringToNumber($($(items[i]).find('td.row_costperunit input')).val()),
                SalePricePerUnit: stringToNumber($($(items[i]).find('td.row_costprice input')).val()),
                Cost: stringToNumber($($(items[i]).find('td.row_cost')).text()),
                Price: stringToNumber($($(items[i]).find('td.row_price input')).val()),
                PriceBookPrice: {
                    Id: $($(items[i]).find('td.row_button input.price_id')).val()
                },
                PriceBookPriceValue: $($(items[i]).find('td.row_button input.price_Value')).val()

            };
            saleItems.push(saleItem);
        }
    }
    var sale = {
        Key: $('#tradersale_form_key').val(),
        Location: { Id: $('#local-manage-select').val() },
        SaleItems: saleItems,
        SaleTotal: stringToNumber($('#total_id').text()),
        Purchaser: { Id: $('#form_sale_contact').val() },
        DeliveryAddress: address,
        Status: status,
        Reference: Reference,
        SalesChannel: "Trader",
        DeliveryMethod: $('#sales_delivery').val(),
        Workgroup: $workgroup
    };
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderSales/SaveTraderSale?countryName=' + $('#form_contact_address_country').val(),
        data: { traderSale: sale },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $('#app-trader-sale-add').modal('toggle');
                cleanBookNotification.createSuccess();
                setTimeout(function () {
                    ShowTableSaleValue(true);
                }, 500);
            } else if (response.actionVal === 2) {
                $('#app-trader-sale-add').modal('toggle');
                cleanBookNotification.updateSuccess();
                setTimeout(function () {
                    ShowTableSaleValue(true);
                }, 500);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
};

// ----------- workgroup ---------
function WorkGroupSelectedChange() {

    $workgroupId = $("#trader_sale_add_workgroup").val();
    if ($workgroupId !== "") {

        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
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

            },
            error: function (er) {
                $('.preview-workgroup').hide();
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    } else {

        $('.preview-workgroup').hide();
    }
};
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
}
// ------- end workgroup -------------

// show table
function ShowTableSaleValue(isReload) {
    if (!isReload)
        $('#trader-sale-content').LoadingOverlay("show");
    $('#trader-sale-content').empty();
    $('#trader-sale-content').load('/TraderSales/TraderSaleRecordsTable', filter, function (reFilter) {

        $('#trader-sale-content').LoadingOverlay("hide");
    });
}

function callBackDataTableTraderSale() {
    $("#tb_trader_sales").DataTable().ajax.reload();
    getDashBoardTotalValue();
}

function LoadDataSales() {
    $("#tb_trader_sales").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#table_show').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#table_show').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderSales/GetDataTableSales',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search_dt").val(),
                    "workGroupId": $("#subfilter-group").val() ? $("#subfilter-group").val() : 0,
                    "channel": $("#subfilter-channel").val(),
                    "datetime": $("#sale-input-datetimerange").val(),
                    "isApproved": false
                });
            }
        },
        "columns": [
            {
                name: "FullRef",
                data: "FullRef",
                orderable: true
            },
            {
                name: "WorkgroupName",
                data: "WorkgroupName",
                orderable: true
            },
            {
                name: "CreatedDate",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "SalesChannel",
                data: "SalesChannel",
                orderable: true
            },
            {
                name: "Contact",
                data: "Contact",
                orderable: true
            },
            {
                name: "Dimensions",
                data: "Dimensions",
                orderable: false
            },
            {
                name: "SaleTotal",
                data: "SaleTotal",
                orderable: true
            },
            {
                name: "Status",
                data: "Status",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = '<span class="label label-lg ' + row.LabelStatus + '">' + row.Status + '</span>';
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = '';
                    if (row.Status === 'Draft') {
                        if (row.AllowEdit) {
                            str += '<button class="btn btn-info" onclick="editSale(\'' + row.Key + '\')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                        } else {
                            str += '<button class="btn btn-info hidden" onclick="editSale(\'' + row.Key + '\')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                        }

                        } else {
                            str += '<button class="btn btn-primary" onclick="window.location.href = \'/TraderSales/SaleMaster?key=' + row.Key + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                        }
                        return str;
                    }
                }
            ],
            "order": [[2, "desc"]]
        });
}


var isShowDashboard = false;
function showDashboard() {
    isShowDashboard = !isShowDashboard;
    if (isShowDashboard) {
        $('.dash-concise').fadeIn();
    } else {
        $('.dash-concise').fadeOut();
    }
}

function getDashBoardTotalValue() {
    if ($('.loadingoverlay').length === 0)
        $.LoadingOverlay("show");
    $('#section_dashboard').load("/TraderSales/TraderSaleGetDataDashBoard?keyword=" + $("#search_dt").val() + "&workGroupId=" + $("#subfilter-group").val() + "&datetime=" + $("#sale-input-datetimerange").val().replace(/\s/g, '%20') + "&channel=" + $("#subfilter-channel").val(), function () {
        LoadingOverlayEnd();
    });
}
function showDetailTraderItemByGroup(ids) {
    $.LoadingOverlay("show");
    var datetime = $("#sale-input-datetimerange").val();
    $('#app-trader-product-group').load("/TraderSales/TraderSaleGetDetailTraderItems?ids=" + ids, { datetime: datetime }, function () {
        LoadingOverlayEnd();
    });
}

// ----------------------------------------
//         EXPORT FUNCTIONALITY
// ----------------------------------------
function GetDataTableContentExport(exportType = 1) {
    $.LoadingOverlay("show");

    var params = $('#tb_trader_sales').DataTable().ajax.params();
    console.log(params, "params");

    //init params
    var _parameter = {
        ...params,
        "export": exportType
    };

    $.ajax({
        type: 'POST',
        url: '/Report/GetSalesOrderTableContentExport',
        dataType: 'json',
        data: _parameter,
        success: function (response) {
            if (response.result == true) {
                console.log(response, 'response');
                window.location.href = response.Object;
                cleanBookNotification.success("Export downloaded successfully!", "Qbicles");
            }
            $.LoadingOverlay("hide");
        },
        error: function (err) {
            console.log(err, 'error');
            $.LoadingOverlay("hide");
        }
    });
}


// ----------- export ---------
