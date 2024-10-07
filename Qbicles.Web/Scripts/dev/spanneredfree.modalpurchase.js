var isChecking = false;
var isConfirm = false;
var arrayUpdate = [];
function addPurchase() {
    $('#app-trader-purchase-add').modal("show");
    $('#app-trader-purchase-add').empty();
    $('#app-trader-purchase-add').load('/TraderPurchases/TraderPurchaseAdd?locationId=' + $slLocation.val());
}
function editPurchase(purchaseId) {
    $('#app-trader-purchase-add').empty();
    $('#app-trader-purchase-add').load('/TraderPurchases/TraderPurchaseAdd?locationId=' + $slLocation.val() + '&purchaseId=' + purchaseId);
    $('#app-trader-purchase-add').modal('toggle');
}
function resetPurchaseForm() {
    $('#cpu').attr('disabled', true);
    $('#addNowForm').attr('disabled', true);
    $('#label_cost').text('Cost per <selected unit>');
}
function ChangeSelectedUnit() {
    $('#item_selected').empty();
    //$.LoadingOverlay("show");

    var itemId = $('#item-select-manage').find(':selected').attr('itemId');


    $('#item_selected').load('/Trader/TraderSaleSelectUnit?idLocation=' + $slLocation.val() + '&idTraderItem=' + (itemId?itemId:0), function () {
        //LoadingOverlayEnd();
        if ($('#item-select-manage').val() !== "" && $('#item-select-manage').val() !== "0") {
            $('#label_cost').text('Cost per <selected unit>');
            $('#cpu').removeAttr('disabled');
        }
    });

}
function quantityChange() {
    var quantityElement = $('#form_item_quantity').val();
    var costElement = $('#cpu').val();
    if ((parseFloat(quantityElement).toString() !== "NaN" && parseFloat(quantityElement) > 0)
        && (parseFloat(costElement).toString() !== "NaN" && parseFloat(costElement) > 0)) {
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
function removeRowItem(id) {
    $('#tb_form_item tbody tr.tr_id_' + id).remove();
    ResetSpanneredItemSelected('tb_form_item', 'item-select-manage');
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
        Discount: isNaN(parseInt($('#form_item_discount').val())) ? 0 : parseInt($('#form_item_discount').val()),
        Dimensions: [],
        Quantity: parseInt($('#form_item_quantity').val()),
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
    $($(clone).find('td.row_cost')).text(toCurrencySymbol(item.Cost, false));
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
    ResetSpanneredItemSelected('tb_form_item', 'item-select-manage');
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

    $('#tb_form_item tbody tr.tr_id_' + id + ' td.row_cost').text(toCurrencySymbol(priceIncludeTax, false));
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
        var cost = stringToNumber($($(trs[i]).find('td.row_cost')).text());
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
        strTrs += "<td>" + $($(trs[i]).find('td.row_cost')).text() + "</td> </tr>";
        $('#tb_confirm tbody').append(strTrs);
    }
    $('#total_id').text(toCurrencySymbol(total));
    validateSendToReview();
}
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
                Cost: stringToNumber($($(items[i]).find('td.row_cost')).text())
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
        Id: $('#tradersale_form_id').val(),
        Location: { Id: $slLocation.val() },
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
        url: '/Spanneredfree/SaveTraderPurchaseAsset',
        data: { traderPurchase: traderPurchase, assetId: $('#assetId').val() },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-purchase-add').modal('toggle');
                cleanBookNotification.createSuccess();
                ReloadRelatedPurchases();
            } else if (response.actionVal === 2) {
                $('#app-trader-purchase-add').modal('toggle');
                cleanBookNotification.updateSuccess();
                ReloadRelatedPurchases();
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
        }
    });
};
function ReloadRelatedPurchases() {
    $('#asset-relatedpurchases div.spacing').load("/Spanneredfree/ReloadRelatedPurchases", { assetId: $('#assetId').val() }, function () {
        $('#tblRelatedPurchases').DataTable();
    });
}
function ChangeWorkgroup(ev) {
    WorkGroupSelectedChange();
    $('#item-select-manage').empty().trigger("change");
    var wgId = $(ev).val();
    $.ajax({
        type: "get",
        url: '/Spanneredfree/GetItemProductByWorkgroupIsBought?wgid=' + wgId + "&locationId=" + $slLocation.val() + "&assetId=" + ($('#assetId').val()?$('#assetId').val():0),
        dataType: "json",
        success: function (response) {
            if (response.result) {
                lstTraderItems = response.Object;
                ResetSpanneredItemSelected('tb_form_item', 'item-select-manage', $('#trader_purchase_add_workgroup').val());
            } else {
            }
        },
        error: function (er) {
        }
    });
};
function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    $workgroupId = $("#trader_purchase_add_workgroup").val();
    if ($workgroupId !== "") {
        //$.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                //LoadingOverlayEnd();
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
                //LoadingOverlayEnd();
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
function ResetSpanneredItemSelected(tableId, selectId, workgroupId) {
    if (typeof workgroupId !== "undefined")
        if (workgroupId === null) return;
    var isWg = !isNaN(workgroupId);
    if ($('#' + tableId) && $('#' + selectId)) {
        var trs = $('#' + tableId + ' tbody tr');
        var tds = $('#' + tableId + ' tbody tr td');
        var lstId = [];
        if (trs.length > 0 && tds.length > 1) {
            for (var i = 0; i < trs.length; i++) {
                lstId.push($($(trs[i]).find(' td input.traderItem')).val().split(':')[0]);
            }
        }
        var traderItemsSelected = jQuery.map(lstTraderItems, function (item) {
            if (lstId.indexOf(item.Id.toString()) === -1) {
                return item;
            }
        });
        //if (isWg) {
        //    traderItemsSelected = jQuery.map(traderItemsSelected,
        //        function (item) {
        //            if (item.WgIds && (item.WgIds.join(',') + ',').indexOf(workgroupId + ',') > -1) {
        //                return item;
        //            }
        //        });
        //}
        var html = "<option value=''></option>";
        for (var j = 0; j < traderItemsSelected.length; j++) {
            html +=
                "<option itemId='" + traderItemsSelected[j].Id + "'" +
                " itemName='" + traderItemsSelected[j].Name + "'" +
                " itemImage='" + traderItemsSelected[j].ImageUri + "'" +
                " taxName='" + traderItemsSelected[j].TaxRateName + "'" +
                " taxRate='" + traderItemsSelected[j].TaxRateValue + "'" +
                " costUnit ='" + traderItemsSelected[j].CostUnit + "'" +
                "value=\""
                + traderItemsSelected[j].Id
                + "\">" + traderItemsSelected[j].Name + "</option>";
        }
        $("#" + selectId).empty();
        $("#" + selectId).append(html);
        $("#" + selectId).not('.multi-select').select2({ placeholder: "Please select" });
        qbicleLog('ResetItemSelected');
    }
};
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
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#trader_purchase_add_workgroup").val() + "&title=Purchase team members");
    $('#app-trader-workgroup-preview').modal('toggle');
}
function EnableNextButton() {
    if ($workgroupId !== "")
        $("#app-trader-purchase-add .btnNext").removeAttr("Disabled");
};
function DisableNextButton() {
    $("#app-trader-purchase-add .btnNext").attr("Disabled", "Disabled");
};

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
        ResetSpanneredItemSelected('tb_form_item', 'item-select-manage', $('#trader_purchase_add_workgroup').val());
    }
}
