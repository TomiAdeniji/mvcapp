var idAccount = "";
var idGroup = 0;
var typeGroup = "";
$(document).ready(function () {
    $("#tabs-pgroup a:first").click();
});
initmultiselectPG();
function initmultiselectPG() {
    $("#slProductGroups").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true,
        onChange: function (option, checked) {
            checkshowtabconfig();
            if (checked)
                addGroupTab($(option).val(), fixQuoteCode($(option).text()));
            else
                removeGroupTab($(option).val());
        }
    });
    $("#version-1 .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
}
function checkshowtabconfig() {
    var groups = $('#slProductGroups').val();
    if (groups && groups.length > 0) {
        $('#version-1').show();
    } else {
        $('#version-1').hide();
    }
}
function addGroupTab(groupid,groupname) {
    $("#tabs-pgroup").append(
        "<li><a data-toggle=\"tab\" onclick=\"$('#tblGroup" + groupid+"').DataTable().ajax.reload();\" href='#group" + groupid + "'>" + groupname + "</a></li>"
    );
    $("#tabs-content").append(
        "<div class=\"tab-pane fade\" id='group" + groupid + "'></div>"
    );
    $('#group' + groupid).load("/TraderConfiguration/LoadGroupConfigTab?groupid=" + groupid, function () {
        $('select.select2').select2({ placeholder: 'Please select' });
        $("#version-1 .checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: true,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $("#tabs-pgroup a[href=#group" + groupid + "]").click();
    });
    $("#tabs-pgroup a[href=#group" + groupid + "]").tab("show");
}
function removeGroupTab(groupid) {
    var tabIdStr = "#group" + groupid;
    // Remove the panel
    $(tabIdStr).remove();

    // Remove the tab
    var hrefStr = "a[href='" + tabIdStr + "']";
    $(hrefStr).closest("li").remove();
    $("#tabs-pgroup a:first").tab('show');
    $.post('/TraderConfiguration/RemoveMasterSetup', { groupid: groupid });
}
function initDatatable(groupId) {
    var $tblItems = $('#tblGroup' + groupId);
    $tblItems.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblItems.LoadingOverlay("show");
        } else {
            $tblItems.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[0, "asc"]],
        deferLoading: 10,
        ajax: {
            "url": "/TraderConfiguration/ItemsTraderGroupMaster",
            "data": function (d) {
                return $.extend({}, d, {
                    "groupid": groupId,
                    "keyword": $('#group' + groupId + ' .txtsearch').val(),
                    "type": $('#group' + groupId + ' .sltype').val()
                });
            }
        },
        columns: [
            { "title": "Item", "data": "Item", "searchable": true, "orderable": true },
            { "title": "Type", "data": "Type", "searchable": true, "orderable": true },
            { "title": "Taxes", "data": "Taxes", "searchable": true, "orderable": true },
            { "title": "Accounts", "data": "Accounts", "searchable": true, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": true, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 4,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button type="button" class="btn btn-warning" onclick="loadAccountingItemConfig(' + data + ',' + groupId+')"><i class="fa fa-pencil"></i></button>';
                    return _htmlOptions;
                }
            }]
    });
    $('#group' + groupId + ' .txtsearch').keyup(delay(function () {
        $tblItems.DataTable().ajax.reload();
    },500));
    $('#group' + groupId + ' .sltype').change(function () {
        $tblItems.DataTable().ajax.reload();
    });
}
function LoadAccountsTree() {
    LoadingOverlay();
    $("#app-bookkeeping-treeview").html("");
    $("#app-bookkeeping-treeview").load("/Bookkeeping/TreeViewAccountByNodeIdPartial?id=0&number=0", function () {
        initJsTree();
        $('.jstree').on("select_node.jstree", function (evt, data) {
            $('#content').html("<div class='text-center' style='margin-top: 50px;'><img src='/Content/DesignStyle/img/loading.gif' class='loader'></div>");
            var type = data.node.data.node;
            var value = data.node.data.value;
            selectedNode = data.node.data;
            if (typeof (selectedNode.parent) === "number") {
                lstParent.push(selectedNode.parent);
            } else if (typeof (selectedNode.parent) === "string") {
                lstParent = selectedNode.parent.split(',');
            }

            if ($(document).width() < 978) {
                $('html, body').animate({
                    scrollTop: $('#content').offset().top - 120
                }, 'slow');
            }
        });
        LoadingOverlayEnd();
    });
}
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    if (idGroup > 0) {
        $('#btn_' + typeGroup + '_add' + idAccount + '_account' + idGroup).addClass('hidden');
        $('#btn_' + typeGroup + '_edit' + idAccount + '_account' + idGroup).removeClass('hidden');
        $('#' + typeGroup + '_' + idAccount + '_account' + idGroup).val(id);
        $('#' + typeGroup + '_' + idAccount + '_accounttext' + idGroup).text(name);
    } else {
        $('#btn_' + typeGroup + '_add' + idAccount + '_account').addClass('hidden');
        $('#btn_' + typeGroup + '_edit' + idAccount + '_account').removeClass('hidden');
        $('#' + typeGroup + '_' + idAccount + '_account').val(id);
        $('#' + typeGroup + '_' + idAccount + '_accounttext').text(name);
    }
    $('#app-bookkeeping-treeview').modal('hide');
}
function ApplyAll() {
    var groupid = $('#cfgroupid').val();
    var paramaters = {
        GroupId: groupid,
        IBuy: {
            //Items I buy
            ibuy_purchaseTaxRate: $('#ibuy_purchaseTaxRate' + groupid).val() ? $('#ibuy_purchaseTaxRate' + groupid).val():[],
            ibuy_purchaseAccount: $('#ibuy_purchase_account' + groupid).val(),
            ibuy_pnventoryAccount: $('#ibuy_inventory_account' + groupid).val()
            //End I buy
        },
        IBuySell: {
            //Items I buy & sell
            ibuysell_purchaseTaxRate: $('#ibuysell_purchaseTaxRate' + groupid).val() ? $('#ibuysell_purchaseTaxRate' + groupid).val() : [],
            ibuysell_purchaseAccount: $('#ibuysell_purchase_account' + groupid).val(),
            ibuysell_salesTaxRate: $('#ibuysell_salesTaxRate' + groupid).val() ? $('#ibuysell_salesTaxRate' + groupid).val() : [],
            ibuysell_salesAccount: $('#ibuysell_sale_account' + groupid).val(),
            ibuysell_inventoryAccount: $('#ibuysell_inventory_account' + groupid).val(),
            //End I buy & sell
        },
        ISellSCompound: {
            //Items I sell (compound)
            isellcompound_salesTaxRate: $('#isellcompound_salesTaxRate' + groupid).val() ? $('#isellcompound_salesTaxRate' + groupid).val() : [],
            isellcompound_salesAccount: $('#isellcompound_sale_account' + groupid).val(),
            isellcompound_inventoryAccount: $('#isellcompound_inventory_account' + groupid).val(),
            //End I sell (compound)
        },
        ISellService: {
            //Items I sell (services)
            isellservices_salesTaxRate: $('#isellservices_salesTaxRate' + groupid).val() ? $('#isellservices_salesTaxRate' + groupid).val() : [],
            isellservices_salesAccount: $('#isellservices_sale_account' + groupid).val(),
            isellservices_inventoryAccount: $('#isellservices_inventory_account' + groupid).val()
            //End I sell
        },
        ApplyType: $('#cftype').val()
    };
    $.LoadingOverlay("show");
    $.post("/TraderConfiguration/SaveApplyMasterSetup", paramaters, function (response) {
        if (response.result) {
            $('#confirm-apply').modal('hide');
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            $('#tblGroup' + groupid).DataTable().ajax.reload();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
        LoadingOverlayEnd();
    });
}
function bindTabApply(groupid, type) {
    $('#cfgroupid').val(groupid);
    $('#cftype').val(type);
}
function loadAccountingItemConfig(id,groupid) {
    $("#app-trader-master-accounting-item-edit").html("");
    $("#app-trader-master-accounting-item-edit").modal("show");
    $("#app-trader-master-accounting-item-edit").load("/TraderConfiguration/LoadAccountingConfigItem?traderitemid=" + id, function () {
        $("#app-trader-master-accounting-item-edit .checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: true,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#hdfGroupId').val(groupid);
    });
}
function UpdateAccountingItemSetting(itemid,groupid) {
    var paramaters = {
        TraderItemId: itemid,
        IBuy: {
            //Items I buy
            ibuy_purchaseTaxRate: $('#ibuy_purchaseTaxRate').val() ? $('#ibuy_purchaseTaxRate').val() : [],
            ibuy_purchaseAccount: $('#ibuy_purchase_account').val(),
            ibuy_pnventoryAccount: $('#ibuy_inventory_account').val()
            //End I buy
        },
        IBuySell: {
            //Items I buy & sell
            ibuysell_purchaseTaxRate: $('#ibuysell_purchaseTaxRate').val() ? $('#ibuysell_purchaseTaxRate').val() : [],
            ibuysell_purchaseAccount: $('#ibuysell_purchase_account').val(),
            ibuysell_salesTaxRate: $('#ibuysell_salesTaxRate').val() ? $('#ibuysell_salesTaxRate').val() : [],
            ibuysell_salesAccount: $('#ibuysell_sale_account').val(),
            ibuysell_inventoryAccount: $('#ibuysell_inventory_account').val(),
            //End I buy & sell
        },
        ISellSCompound: {
            //Items I sell (compound)
            isellcompound_salesTaxRate: $('#isellcompound_salesTaxRate').val() ? $('#isellcompound_salesTaxRate').val() : [],
            isellcompound_salesAccount: $('#isellcompound_sale_account').val(),
            isellcompound_inventoryAccount: $('#isellcompound_inventory_account').val(),
            //End I sell (compound)
        },
        ISellService: {
            //Items I sell (services)
            isellservices_salesTaxRate: $('#isellservices_salesTaxRate').val() ? $('#isellservices_salesTaxRate').val() : [],
            isellservices_salesAccount: $('#isellservices_sale_account').val(),
            isellservices_inventoryAccount: $('#isellservices_inventory_account').val()
            //End I sell
        }
    };
    $.LoadingOverlay("show");
    $.post("/TraderConfiguration/UpdateAccountingItemSettings", paramaters, function (response) {
        if (response.result) {
            $('#app-trader-master-accounting-item-edit').modal('hide');
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            var groupid = $('#hdfGroupId').val();
            $('#tblGroup' + groupid).DataTable().ajax.reload();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
        LoadingOverlayEnd();
    });
}
function closeSelected() {
//init empty for AccountsTree
};
function initSelectedAccount() {
//init empty for AccountsTree
}
function initFormAppconfigPrice(groupid) {
    var $frmConfigsPrice = $('#frmConfigsPrice' + groupid);
    $frmConfigsPrice.validate({
        ignore: "",
        rules: {
            Locations: {
                required: true
            },
            Salechannels: {
                required: true
            },
            MarkupValue: {
                required: true,
            },
            DiscountValue: {
                required: true,
            }
        }
    });
    $frmConfigsPrice.submit(function (e) {
        e.preventDefault();
        if ($frmConfigsPrice.valid()) {
            var groupid = $('#cfpricegroupid').val();
            $.LoadingOverlay("show");
            var _data = $(this).serializeArray();
            _data.push({ name: "IsExistingOverwritten", value: $("#isOverwrite").is(":checked") });

            $.post("/TraderConfiguration/ApplyConfigPriceByGroup", $.param(_data), function (response) {
                if (response.result) {
                    $('#confirm-price-apply').modal('hide');
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
                    $('#tblPrice' + groupid).DataTable().ajax.reload();
                } else if (!response.result&&response.msg)
                {
                    cleanBookNotification.warning(response.msg, "Qbicles");
                }
                else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
                LoadingOverlayEnd();
            });
        } else {
            return;
        }
        LoadingOverlayEnd();
    });
}
function initDatatablePrices(groupId) {
    var $tblPrice = $('#tblPrice' + groupId);
    $tblPrice.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblPrice.LoadingOverlay("show");
        } else {
            $tblPrice.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[0, "asc"]],
        deferLoading: 10,
        ajax: {
            "url": "/TraderConfiguration/PricesTraderGroupMaster",
            "data": function (d) {
                return $.extend({}, d, {
                    "groupid": groupId,
                    "keyword": $('#txtKeyword' + groupId).val(),
                    "locationid": $('#slFilterLocation' + groupId).val(),
                    "saleschannel": $('#slFilterSalesChannel' + groupId).val()
                });
            }
        },
        columns: [
            { "title": "Item", "data": "Item", "searchable": true, "orderable": true },
            { "title": "Location", "data": "Location", "searchable": true, "orderable": true },
            { "title": "Sales Channel", "data": "SalesChannel", "searchable": true, "orderable": true },
            { "data": "AverageCost", "searchable": true, "orderable": false },
            { "data": "PriceExcTax", "searchable": true, "orderable": true },
            { "data": "Tax", "searchable": true, "orderable": true },
            { "data": "GrossPrice", "searchable": true, "orderable": true }
        ],
        columnDefs: [
            {
                "targets": 4,
                "data": "PriceExcTax",
                "render": function (data, type, row, meta) {
                    var _htmlPriceExcTax = '<input id="numPriceExcTax' + row.Id + '" onchange="updateValuePrice(' + row.Id+',false,$(this).val())" type="number" class="form-control" value="' + data+'">';
                    return _htmlPriceExcTax;
                }
            },
            {
                "targets": 6,
                "data": "GrossPrice",
                "render": function (data, type, row, meta) {
                    var _htmlPriceIncTax = '<input id="numPriceIncTax' + row.Id + '" onchange="updateValuePrice(' + row.Id + ',true,$(this).val())" type="number" class="form-control" value="' + toCurrencyDecimalPlace(row.GrossPrice) + '">';
                    return _htmlPriceIncTax;
                }
            }
        ]
    });
    $('#txtKeyword' + groupId).keyup(delay(function () {
        $tblPrice.DataTable().ajax.reload();
    }, 500));
    $('#slFilterLocation' + groupId).change(function () {
        $tblPrice.DataTable().ajax.reload();
    });
    $('#slFilterSalesChannel' + groupId).change(function () {
        $tblPrice.DataTable().ajax.reload();
    });
}
function loadReloadPrices(groupId) {
    var $tblprice= $('#tblPrice' + groupId);
    if (!$.fn.DataTable.isDataTable('#tblPrice' + groupId))
        setTimeout(function () {
            $tblprice.DataTable().ajax.reload();
        }, 1000);
    else
        $tblprice.DataTable().ajax.reload();
        
}
function loadReloadAccounting(groupId) {
    var $tblGroup = $('#tblGroup' + groupId);
    if (!$.fn.DataTable.isDataTable('#groupId' + groupId))
        setTimeout(function () {
            $tblGroup.DataTable().ajax.reload();
        }, 1500);
    else
        $tblGroup.DataTable().ajax.reload();

}
function updateValuePrice(id, isInclusiveTax, price) {
    price = price ? price : 0;
    $.post("/TraderConfiguration/UpdateValuePrice", { id: id, isInclusiveTax: isInclusiveTax, value: price }, function (response) {
        if (response.result) {
            var data = response.Object;
            if (isInclusiveTax) {
                $('#numPriceExcTax' + id).val(data.PriceExcTax);
                $('#numPriceExcTax' + id).closest("tr").find("td:eq(5)").text(data.Tax);
            } else {
                $('#numPriceIncTax' + id).val(parseFloat(data.PriceExcTax) + parseFloat(data.Tax));
                $('#numPriceExcTax' + id).closest("tr").find("td:eq(5)").text(data.Tax);
            }
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else {
            if (response.Object2 < 0) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
            }
        }
    });
}
function applyPriceConfig(groupid) {
    if (!$("#frmConfigsPrice" + groupid).valid())
        return;
    else {
        $('#cfpricegroupid').val(groupid);
        $('#confirm-price-apply').modal("show");
    }
        
}