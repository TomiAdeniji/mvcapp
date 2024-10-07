var lstTransferTraderItems = [];
var statusTranferItems = '';
function loadModalTranferItems(itemId) {
    $.LoadingOverlay("show");
    $('#app-spannered-asset-transfer').empty();
    $("#app-spannered-asset-transfer").modal("show");
    $('#app-spannered-asset-transfer').load("/Spanneredfree/LoadModalTransferItems?locationId=" + $slLocation.val() + "&aiId=" + (itemId ? itemId : 0), function () {
        initFormTransfer();
        LoadingOverlayEnd();
    });
}
function workGroupTransferChange(wgel) {
    $('.preview-workgroup').show();
    $workgroupId = $(wgel).val();
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
                //EnableNextButton();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
        $.ajax({
            type: "get",
            url: '/Spanneredfree/GetItemProductByWorkgroupIsBought?wgid=' + $workgroupId + "&locationId=" + $slLocation.val() + "&assetId=" + ($('#assetId').val() ? $('#assetId').val() : 0),
            dataType: "json",
            success: function (response) {
                if (response.result) {
                    lstTransferTraderItems = response.Object;
                    ResetTransferItems('tbl_transfer_items', 'transfer-items', $('#transfer-workgroup-select').val());
                } else {
                }
            },
            error: function (er) {
                
            }
        });
    } else {
        //DisableNextButton();
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId + "&title=Transfers team members");
    $('#app-trader-workgroup-preview').modal('show');
}
function validateItemsInTable() {
    var trans = $("#tbl_transfer_items tbody tr");

    if (trans === null || trans.length === 0) {
        $("#app-spannered-asset-transfer .btnNextConfirm").attr("Disabled", "Disabled");
        $("#app-spannered-asset-transfer .btnNextConfirm").off();
    } else {
        $("#app-spannered-asset-transfer .btnNextConfirm").removeAttr("Disabled");
        $("#app-spannered-asset-transfer .btnNextConfirm").on('click', function () { 
            var parent = $(this).closest('.modal');
            if ($('#frmTransferItems').valid()) {
                $(parent).find('#tabTransferItemsNav .active').next('li').find('a').trigger('click');
            }
        });
    }
};
function changeTransferItemUnit() {
    var itemId=$("#transfer-items").val();
    if (!itemId) {
        resetFrmTransfer();
        ResetTransferItems('tbl_transfer_items', 'transfer-items', $('#transfer-workgroup-select').val());
        return;
    }
    $("#transfer-item-units").empty();
    $.get("/Spanneredfree/GetUnitByItemId?itemId=" + itemId, function (data) {
        $('#transfer-item-units').select2({
            data: (data ? data: [])
        });
    });
};
function removeRowTransferItem(id) {
    $("#tbl_transfer_items tbody tr.tr_id_" + id).remove();
    validateItemsInTable();
    ResetTransferItems('tbl_transfer_items', 'transfer-items', $('#transfer-workgroup-select').val());
};
function addRowTransferItem() {
    setRequiredFrmTransferItem(true);
    if ($('#transfer-items').valid() && $('#transfer-item-units').valid() && $('#transfer-item-quantity').valid()) {
        var idBuild = UniqueId();
        var unit = $("#transfer-item-units").val();
        checkQuantityTransItem($("#transfer-item-quantity"));
        var item = {
            Id: idBuild,
            TraderItem: {
                Id: $("#transfer-items").val().split(":")[0],
                ImageUri: $("#transfer-items option:selected").attr("itemimage"),
                Name: $("#transfer-items option:selected").text()
            },
            Quantity: parseFloat($("#transfer-item-quantity").val()),
            Fee: parseFloat($("#transfer-item-fee").val())
        };

        var clone = $("#tb_form_template_tranferitems tbody tr").clone();
        $(clone).addClass("tr_id_" + item.Id);
        // filter to table 
        $($(clone).find("td.row_image div")).attr("style",
            "background-image: url('" + $("#api-uri").val() + item.TraderItem.ImageUri + "&size=T');");

        $($(clone).find("td.row_name")).text(item.TraderItem.Name);

        $($(clone).find("td.row_unit")).empty();
        var unitClone = $("#transfer-item-units").clone();
        unitClone = $(unitClone).removeAttr("id");
        $($(clone).find("td.row_unit")).append(unitClone);
        $($(clone).find("td.row_unit select")).val(unit);
        //$($(clone).find("td.row_unit select")).attr("onchange", "rowUnitChange('" + item.Id + "')");
        var elQuantity = $($(clone).find("td.row_quantity input"));
        elQuantity.val(item.Quantity);
        elQuantity.attr("onchange", "checkQuantityTransItem(this," + item.TraderItem.Id + ")");

        var elFee = $($(clone).find("td.row_fee input"));
        elFee.val(item.Fee);

        $($(clone).find("td.row_button button")).attr("onclick", "removeRowTransferItem('" + item.Id + "')");
        $($(clone).find("td.row_button input.traderItem")).val($("#transfer-items").val());
        $($(clone).find("td.row_button input.row_id")).val(item.Id);
        $($(clone).find("td select")).not(".multi-select").select2();
        $("#tbl_transfer_items tbody").append(clone);
        validateItemsInTable();
        ResetTransferItems('tbl_transfer_items', 'transfer-items', $('#transfer-workgroup-select').val());
        resetFrmTransfer();
    }
    setRequiredFrmTransferItem(false);
};
function checkQuantityTransItem(el, item) {
    var transferType = $("#transfer_type").val();

    if (transferType === "#outbound") {
        var locationId = $slLocation.val();
        var quantity = $(el).val();
        var valItem = $('#transfer-items').val();
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
        });
    }
};
function initFormTransfer() {
    var $frmTransferItems = $('#frmTransferItems');
    $frmTransferItems.validate({
        ignore: "",
        rules: {
            workgourptransfer: {
                required: true
            }
        },
        invalidHandler: function () {
            if ($('##transfer-1 label.error:not([style*="display: none"])').length != 0) {
                $('#tabTransferItemsNav a[href="#transfer-1"]').tab('show');
            } else if ($('#transfer-2 label.error:not([style*="display: none"])').length != 0) {
                $('#tabTransferItemsNav a[href="#transfer-2"]').tab('show');
            }
        }
    });
    $frmTransferItems.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmTransferItems.valid()) {
            $.LoadingOverlay("show");
            var $originatingLocation = {};
            var $destinationLocation = {};
            if ($("#transfer_type").val() === "#outbound") {
                $originatingLocation = {
                    Id: $slLocation.val()
                };
                $destinationLocation = {
                    Id: $("#in-out-location").val()
                };
            }
            else if ($("#transfer_type").val() === "#inbound") {
                $destinationLocation = {
                    Id: $slLocation.val()
                };
                $originatingLocation = {
                    Id: $("#in-out-location").val()
                };
            }
            var transferItems = [];
            var trans = $("#tbl_transfer_items tbody tr");

            if (trans === null || trans.length === 0) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_654"), "Qbicles");
                $('a[href="#transfer-2"]').trigger('click');
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
                        $('a[href="#transfer-2"]').trigger('click');
                        return;
                    }
                    var unit = $($(trans[i]).find("td.row_unit select")).val();

                    var tran = {
                        Id: isNaN(id) ? 0 : id,
                        TraderItem: item,
                        QuantityAtPickup: $(elQuantity).val(),
                        Unit: { Id: unit }
                    };
                    transferItems.push(tran);
                }
            }
            var transfer = {
                Status: 'PendingPickup',
                OriginatingLocation: $originatingLocation,
                DestinationLocation: $destinationLocation,
                Workgroup: { Id: $('#transfer-workgroup-select').val() },
                TransferItems: transferItems
            };

            $.ajax({
                type: "post",
                url: "/Spanneredfree/SaveTransferAsset",
                data: {transfer: transfer },
                dataType: "json",
                success: function (response) {
                    $.LoadingOverlay("hide");
                    $('#app-spannered-asset-transfer').modal("hide");
                    if (response.result) {
                        cleanBookNotification.createSuccess();
                    } else {
                        cleanBookNotification.error(response.msg, "Spannered");
                    }
                },
                error: function (er) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            });
        } else {
            $('#frmTransferItemsAsset ul.app_subnav a[href=#transfer-1]').tab('show');
            return;
        }
    });
    $('#frmTransferItems .select2').not('.multi-select').select2();
    $('#transfer_type').on('change', function () {
        $(this).valid();
        var method = $(this).val();
        //alert(method);
        $('#inbound-lbl').hide();
        $('#outbound-lbl').hide();
        $('#block-in-out').show();
        $(method + '-lbl').toggle();
        $('#in-out-location').trigger('change');
    });
    $('#frmTransferItems .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        if ($frmTransferItems.valid()) {
            $(parent).find('#tabTransferItemsNav .active').next('li').find('a').trigger('click');
        }
    });

    $('#frmTransferItems .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('#tabTransferItemsNav .active').prev('li').find('a').trigger('click');
    });
}
function ResetTransferItems(tableId, selectId, workgroupId) {
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
        var traderItemsSelected = jQuery.map(lstTransferTraderItems, function (item) {
            if (lstId.indexOf(item.Id.toString()) === -1) {
                return item;
            }
        });
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
function resetFrmTransfer() {
    $('#transfer-item-units').empty();
    $('#transfer-item-units').append("<option></option>");
    $('#transfer-item-units').not('.multi-select').select2({ placeholder: "Please select" });

    $('#transfer-item-quantity').val("");
    $('#transfer-item-fee').val("");
};
function setRequiredFrmTransferItem(isRequired) {
    if (isRequired) {
        $('#transfer-items').attr("required",true);
        $('#transfer-item-units').attr("required", true);
        $('#transfer-item-quantity').attr("required", true);
    }else
    {
        $('#transfer-items').removeAttr("required");
        $('#transfer-item-units').removeAttr("required");
        $('#transfer-item-quantity').removeAttr("required");
    }
    
}
function nextToTransferConfirm() {
    $("#confirm-tranfer-type").text($("#transfer_type :selected").text());
    $("#div-confirm").empty();
    loadDataTableConfirm();
    $("#contact-source").empty();
    $("#contact-destination").empty();
    if ($("#transfer_type").val() === "#inbound") {

        $("#source-destination-manage").text("Destination");
        $("#source-destination-select").text("Source");
        $("#location-in-out-selected").clone().removeAttr("id").appendTo("#contact-source");
        $("#location-manage-confirm").clone().removeAttr("id").appendTo("#contact-destination");

    } else if ($("#transfer_type").val() === "#outbound") {

        $("#source-destination-manage").text("Source");
        $("#source-destination-select").text("Destination");

        $("#location-in-out-selected").clone().removeAttr("id").appendTo("#contact-destination");
        $("#location-manage-confirm").clone().removeAttr("id").appendTo("#contact-source");
    }
};
function loadDataTableConfirm() {
    var strTable =
            "<table id='tb_confirm' class='datatable table-hover' style='width: 100%; background: #fff;' data-order='[[1, \"asc\"]]'>";
    strTable += "<thead><tr><th data-orderable='false'></th><th>Name</th><th>Unit</th><th>Quantity</th></tr></thead><tbody>";
    var trs = $("#tbl_transfer_items tbody tr");
    var trd = $("#tbl_transfer_items tbody tr td");

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
function locationInOuChange() {
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
function saveTransferItems(status) {
    statusTranferItems = status;
    if (statusTranferItems)
        $('#frmTransferItems').trigger('submit');
}