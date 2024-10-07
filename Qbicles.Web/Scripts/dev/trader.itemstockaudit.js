var $workgroupId = "";

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
$("#search_stockaudit").keyup(searchThrottle(function () {
    reloadDataStockAudit()
}));

function onSelectWorkgroupStockAudit(ev) {
    reloadDataStockAudit();
}
function reloadDataStockAudit() {
    $("#stockaudit-table").DataTable().ajax.reload(null, false);
}


function addStockAudit(id, view) {
    if (!id) id = 0;
    var ajaxUri = '/TraderStockAudits/AddStockAudit?id=' + id + '&view=' + view;
    $.LoadingOverlay("show");
    $('#app-trader-inventory-stock-audit').empty();
    $('#app-trader-inventory-stock-audit').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        LoadingOverlayEnd();
    });
}
function GetProductItems(wgId, locationId) {
    if (!wgId) wgId = 0;
    var locationId = $("#local-manage-select").val();

    var elementId = "item";
    var url = '/Select2Data/GetStockAuditItemsByWorkgroup?workGroupId=' + wgId + '&locationId=' + locationId;

    //Get default set of options for select2
    var $defaultResults = $("#" + elementId + " option:not([selected])");
    var defaultResults = [];
    $defaultResults.each(function () {
        var $option = $(this);
        defaultResults.push({
            id: $option.attr('value'),
            text: $option.text()
        });
    });

    var parameters = {};
    //Initialize select2 object
    $("#" + elementId).select2({
        ajax: {
            url: url,
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
                return {
                    results: lstData
                };
            }
        },
        //minimumInputLength: 1,
        defaultResults: defaultResults
    })
}

// ----------- workgroup ---------
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
}
RenderAuditWorkgroup = function () {
    var $workgroupId = $("#workgroup-select").val();
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
function WorkGroupSelectedChange() {
    $workgroupId = $("#workgroup-select").val();
    if ($workgroupId !== "") {
        $('#workgroup-select-error').remove();
        RenderAuditWorkgroup();
        GetProductItems($workgroupId, $("#local-manage-select").val());
    }
};
// ------- end workgroup -------------
// Item Add
function addRowItem() {

    $('#tb_form_item').LoadingOverlay("show");

    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length === 1 && tds.length === 1) {
        $('#tb_form_item tbody').empty();
    }
    var isStart = $('#stockaudit_isstart').val();
    var item = $('#item').val().split('|');
    var stockAuditItem = {
        Id: 0,
        Product: {
            Id: item[0],
            Name: item[1],
            SKU: item[2]
        },
        OpeningCount: 0
    }

    var guidId = UniqueId();

    $.ajax({
        type: "get",
        url: "/TraderItem/GetUnitsByItemId?itemId=" + stockAuditItem.Product.Id,
        dataType: "json",
        success: function (response) {
            var htmlStr = "";
            htmlStr += "<tr class=\"tr_" + guidId + "\">";
            htmlStr += "<td class=\"td_name\">";
            htmlStr += "<input type=\"hidden\" value=\"" + stockAuditItem.Product.Id + "\" />";
            htmlStr += "<input type='hidden' id='item-row-id-" + guidId + "' value='" + stockAuditItem.Product.Id + "|" + stockAuditItem.Product.Name + "|" + stockAuditItem.Product.SKU+"'/>";
            htmlStr += "<span>" + stockAuditItem.Product.Name + "</span>";
            htmlStr += "</td>";
            htmlStr += "<td class=\"td_sku\"><input type=\"hidden\" value=\"" + stockAuditItem.Id + "\"/><span>" + stockAuditItem.Product.SKU + "</span></td>";
            htmlStr += "<td class=\"td_unit\">";
            if (isStart == "false") {
                htmlStr += "<select class=\"form-control select-modal\">";
                htmlStr += response;
                //var units = lstTraderItems.filter(function (q) { if (q.Id == stockAuditItem.Product.Id) return q })[0].Units;
                //if (units && units.length > 0) {
                //    for (var i = 0; i < units.length; i++) {
                //        htmlStr += "<option value=\"" + units[i].Id + "\">" + units[i].Name + "</option>";
                //    }
                //}
                htmlStr += "</select>";
            } else {
                htmlStr += "<select disabled class=\"form-control select-modal\">";
                htmlStr += "</select>";
            }
            htmlStr += "</td>";

            htmlStr += "<td class=\"td_open_count\">";
            if (isStart == "true") {
                htmlStr += "<input type=\"number\" name=\"opening-1\" class=\"form-control\" disabled value=\"" + stockAuditItem.OpeningCount + "\">";
            } else {
                htmlStr += "<input type=\"number\" name=\"opening-1\" class=\"form-control\" value=\"" + stockAuditItem.OpeningCount + "\">";
            }
            htmlStr += "</td>";
            htmlStr += "<td class=\"td_button\">";
            if (isStart == "true") {
                htmlStr += "<button style=\"display: none\" class=\"btn btn-danger\"> <i class=\"fa fa-trash\"></i></button> ";
            } else {
                htmlStr += "<button style=\"display: block\" onclick=\"removeRowItem('" + guidId + "','" + stockAuditItem.Product.Name + "')\" class=\"btn btn-danger\"> <i class=\"fa fa-trash\"></i></button> ";
            }
            htmlStr += "</td>";
            htmlStr += "</tr>";
            $('#tb_form_item tbody').append(htmlStr);
            $('#tb_form_item tr select').select2();
            //ResetSelectedItems();

            RemoveOptionSelect2("item");

            document.getElementById("start-audit-button").disabled = false;
        },
        error: function (er) {
            $('.preview-workgroup').hide();
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        $('#tb_form_item').LoadingOverlay("hide", true);
        var $workgroupId = $("#workgroup-select").val();
        GetProductItems($workgroupId, $("#local-manage-select").val());
    });
    //TraderItem / GetUnitsByItemId

    //tb_form_item






}
function removeRowItem(id, name) {
    $('#tb_form_item tbody tr.tr_' + id).remove();
    //ResetSelectedItems();
    document.getElementById("start-audit-button").disabled = true;
    var $workgroupId = $("#workgroup-select").val();
    GetProductItems($workgroupId, $("#local-manage-select").val());
}
function ResetSelectedItems() {

    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length >= 1 && tds.length > 1) {
        var idItems = $('#tb_form_item tbody tr td.td_name input');
        var lstId = [];
        for (var i = 0; i < idItems.length; i++) {
            lstId.push($(idItems[i]).val());
        }
        if (idItems && idItems.length > 0) {
            var traderItemsSelected = jQuery.map(lstTraderItems, function (item) { if (lstId.indexOf(item.Id.toString()) === -1) return item });
            var html = "";
            for (var i = 0; i < traderItemsSelected.length; i++) {
                html += "<option value=\"" + traderItemsSelected[i].Id + "|" + traderItemsSelected[i].Name + "|" + traderItemsSelected[i].SKU + "\">" + traderItemsSelected[i].Name + "</option>";
            }
            $("#item").empty();
            $("#item").append(html);
        }
        document.getElementById("start-audit-button").disabled = false;
    }
    else {
        document.getElementById("start-audit-button").disabled = true;
    }
    $("#item").val("");
    $("#item").not('.multi-select').select2({
        placeholder: 'Please select'
    });
}
function validateForm() {
    var valid = true;
    if ($("#workgroup-select").val() === "") {
        valid = false;
        $("#form_audit").validate().showErrors({ workgroup: "Workgroup is required." });
    } else {
        $('#workgroup-select-error').remove();
    }
    if ($("#auditName").val() === "") {
        valid = false;
        $("#form_audit").validate().showErrors({ auditname: "Name is required." });
    }
    return valid;
}
function getStockAudit() {
    var stockAudit = {
        Id: $('#stockaudit_id').val(),
        Name: $('#auditName').val(),
        Notes: $('#stockaudit_notes').val(),
        Location: { Id: $('#local-manage-select').val() },
        WorkGroup: { Id: $("#workgroup-select").val() },
        ProductList: [],
        IsStarted: $('#stockaudit_isstart').val() == 'true' ? true : false,
        IsFinished: $('#stockaudit_isfinish').val() == 'true' ? true : false
    }
    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length > 0 && tds && tds.length > 1) {
        for (var i = 0; i < trs.length; i++) {
            var productItem = {
                Id: $($(trs[i]).find(' td.td_sku input')).val(),
                Product: {
                    Id: $($(trs[i]).find(' td.td_name input')).val()
                },
                AuditUnit: {
                    Id: $($(trs[i]).find(' td.td_unit select')).val()
                },
                OpeningCount: $($(trs[i]).find(' td.td_open_count input')).val()
            }
            stockAudit.ProductList.push(productItem);
        }
    }
    return stockAudit;
}
function updateVariance(id) {
    var openCountMoven = parseFloat($('#tb_closing tr.tr_' + id + ' td.td_expected').text().replace(/\s/g, ""));
    var closingCount = parseFloat($('#tb_closing tr.tr_' + id + ' td.td_closing_count input').val());
    $('#tb_closing tr.tr_' + id + ' td.td_variance span').text((openCountMoven - closingCount).toFixed(2))
}
function save() {
    if (!validateForm()) return;
    var stockAudit = getStockAudit();
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderStockAudits/SaveStockAudit',
        data: { stockAudit: stockAudit },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.createSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 2) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.updateSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function startAudit() {
    if (!validateForm()) return;
    var stockAudit = getStockAudit();
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderStockAudits/StartStockAudit',
        data: { stockAudit: stockAudit },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.createSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 2) {
                $('#app-trader-inventory-stock-audit').empty();
                addStockAudit(response.msgId);
                cleanBookNotification.updateSuccess();
                $('#app-trader-inventory-stock-audit').modal('toggle');
                showStockAuditTable();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function finishAudit() {
    if (!validateForm()) return;
    var stockAudit = {
        Id: $('#stockaudit_id').val(),
        Name: $('#auditName').val(),
        Notes: $('#stockaudit_notes').val(),
        Location: { Id: $('#local-manage-select').val() },
        WorkGroup: { Id: $("#workgroup-select").val() },
        ProductList: [],
        IsStarted: $('#stockaudit_isstart').val() == 'true' ? true : false,
        IsFinished: $('#stockaudit_isfinish').val() == 'true' ? true : false
    }
    var trs = $('#tb_closing tbody tr');
    var tds = $('#tb_closing tbody tr td');
    if (trs.length > 0 && tds && tds.length > 1) {
        for (var i = 0; i < trs.length; i++) {
            var productItem = {
                Id: $($(trs[i]).find(' td.td_name input')).val(),
                ClosingCount: $($(trs[i]).find(' td.td_closing_count input')).val(),
                Variance: parseFloat($($(trs[i]).find(' td.td_variance span')).text().replace(/\s/g, ""))
            }
            stockAudit.ProductList.push(productItem);
        }
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderStockAudits/FinishStockAudit',
        data: { stockAudit: stockAudit },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.createSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 2) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.updateSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}