
$(function () {


    $("#table-return-items").DataTable();
    $("#table-sale-items").DataTable();

    $('.sale-item-row-select').bootstrapToggle();
    $('.sale-item-return-row-select').bootstrapToggle();


    $("#search_add_edit_return").keyup(delay(function () {
        CallBackDataTableTraderSaleSelect();
    }, 1000));
    $('.datetimerangesale').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
    $('.datetimerangesale').val('');
    $('.datetimerangesale').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('.datetimerangesale').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        filter.DateRange = $("#sale-select-date-range").val();
        CallBackDataTableTraderSaleSelect();
    });
    $('.datetimerangesale').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        filter.DateRange = $("#sale-select-date-range").val();
        $('.datetimerangesale').html('full history');
        CallBackDataTableTraderSaleSelect();
    });

    $("#trader_sale_return_add_workgroup").select2();
    LoadDataSalesSelect();
});


function LoadDataSalesSelect() {


    $("#tb_trader_sales_select").on('processing.dt', function (e, settings, processing) {
        
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
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
            "url": '/TraderSales/GetDataTableSales',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search_add_edit_return").val(),
                    "workGroupId": 0,
                    "channel": "",
                    "datetime": $("#sale-select-date-range").val(),
                    "isApproved": true
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
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = "<a saleId = '" + row.Id + "'  saleRef = '" + row.FullRef + "' workgroup = '" + row.WorkgroupName + "' saleTotal = '" + row.SaleTotal + "' " +
                        " contact = '" + row.Contact + "' approvedOn = '" + row.ApprovedOn + "'saleOrderId = '" + row.SaleOrderId + "' saleOderRef = '" + row.SaleOderRef + "'                             " +
                        "id='sale-select-id-" + row.Id + "' onclick='SelectSaleNextToItemsTab(" + row.Id + ", \"" + row.Key + "\")' class='btn btn-success'><i class='fa fa-check'></i></a>";

                    return str;
                }
            }
        ],
        "order": [[2, "desc"]]
    });
    CallBackDataTableTraderSaleSelect();
}

function CallBackDataTableTraderSaleSelect() {
    $("#tb_trader_sales_select").DataTable().ajax.reload();
}

var $saleIdSelected = 0;

function SelectSaleNextToItemsTab(saleId, saleKey) {
    $saleIdSelected = saleId;

    var saleSelected = $('#sale-select-id-' + saleId);


    $(".sale-reference-href").attr("href", "/TraderSales/SaleMaster?key=" + saleKey);

    $(".sale-reference-fullref").text(saleSelected.attr("saleRef"));
    $(".sale-reference-workgroup-name").text(saleSelected.attr("workgroup"));
    $(".sale-reference-total").text(saleSelected.attr("saleTotal"));
    $(".sale-reference-customer-name").text(saleSelected.attr("contact"));
    $(".sale-reference-approved-on").text(saleSelected.attr("approvedOn"));

    var sOrderId = saleSelected.attr("saleOrderId");
    $(".sale-reference-order-href").attr("href", "/TraderSales/SaleOrder?id=" + sOrderId);
    $(".sale-reference-order-reference-fullref").text(saleSelected.attr("saleOderRef"));

    //Choose items included in this Return
    $.LoadingOverlay("show");
    var elementId = "div-sale-items";
    $('#' + elementId).empty();
    $('#' + elementId).load("/TraderSalesReturn/TraderSaleSelected2Return?saleKey=" + saleKey, function () {
        var table = $('#table-return-items').DataTable();
        table.clear().draw(false);
        ValidationSaveAction();
        //validation case Continue edit, but selecting a Sale
        var saleReturnId = parseInt($("#sale-return-id").val());
        if (saleReturnId > 0) {
            $("#sale-return-id").val(0);
            $("#add-edit-return-title").text("Add a Sale Return");
            var refedit = parseInt($("#refedit").text()) + 1;
            $("#refedit").text(refedit);
            $("#refedit-input").val(refedit);
        }
        

        LoadingOverlayEnd();
        $("#return-item-tab").trigger('click');
    });


}


function BackToSelectItemsTab() {
    $("#return-item-tab").trigger('click');
};

function BackToSelectSalesTab() {
    $("#return-sale-tab").trigger('click');
};

function NextToConfigureReturnTab() {
    $("#return-configuration-tab").trigger('click');
};


SaleItemRowChose = function (isCheck, traderTransactionItemId) {
    //check if isCheck then add to table, else remove to table
    if (!isCheck) {
        RemoveRow(traderTransactionItemId);
        return;
    }
    //add traderTransactionItemId into table table-return-items




    var itemSelected = $('#table-sale-items tbody tr#tr-sale-item-' + traderTransactionItemId);

    var image = $($(itemSelected).find('td.item-row-image input')).val();

    var name = $($(itemSelected).find('td.item-row-name')).text();
    var unit = $($(itemSelected).find('td.item-row-unit')).text();
    var quantity = $($(itemSelected).find('td.item-row-quantity')).text();
    var price = $($(itemSelected).find('td.item-row-price')).text();


    //$('#table-return-items').DataTable().destroy();

    var tbl = $("#table-return-items").DataTable();
    //tbl.draw();

    var rowNode = tbl.row.add(
        [
            "<div class='table-avatar' style=\"background-image: url('" + image + "\");'>&nbsp;</div>",
            "<input class='return-item-id' hidden='' value='" + 0 + "' />" +//returnId= 0???
            "<input class='transaction-item-id' hidden='' value='" + traderTransactionItemId + "' />" +
            "<span>" + name + "</span>",
            unit,
            "<input type='hidden' value='" + quantity + "'>" +
            quantity,
            price,
            "<input maxlength='15' onkeypress='decimalKeyPress(this, event)' onpaste='decimalOnPaste(this, event)' type='text' name='qty1' min='1' max='" + quantity + "' class='form-control' style='width: 80px;' value='" + quantity + "'>",//return quantity
            "<div class='checkbox toggle'><label>" +
            "<input checked class='sale-item-return-row-select-" + traderTransactionItemId + "' data-toggle='toggle' data-size='small' data-onstyle='success' type='checkbox'>"//return to inventory
            + "</label></div>",
            "<div class='input-group' style='width: 100%;'><span class='input-group-addon'>" + currencySetting.CurrencySymbol + "</span>" +
            "<input value='0' min='0' maxlength='15' onkeypress='decimalKeyPress(this, event)' onpaste='decimalOnPaste(this, event)' type='text' class='form-control'>" +//Credit
            "</div>",
            "<button onclick='RemoveReturnRow(" +
            traderTransactionItemId +
            ")' class='btn btn-danger'><i class='fa fa-trash'></i></button>"
        ]).draw().node();// //.draw(false);

    $(rowNode).find('td').eq(1).addClass('return-row-item');
    $(rowNode).find('td').eq(3).addClass('return-row-sale-quantity');
    $(rowNode).find('td').eq(5).addClass('return-row-quantity');
    $(rowNode).find('td').eq(6).addClass('return-row-to-inventory');
    $(rowNode).find('td').eq(7).addClass('return-row-credit');
    $(rowNode).attr('id', "table-row-return-item-" + traderTransactionItemId);
    $(rowNode).attr('class', "table-row-return-item-list");
    $(".sale-item-return-row-select-" + traderTransactionItemId).bootstrapToggle();

    ValidationSaveAction();
};

RemoveReturnRow = function (traderTransactionItemId) {
    RemoveRow(traderTransactionItemId);
    $("#sale-item-transaction-selected-" + traderTransactionItemId).bootstrapToggle('off');
};

RemoveRow = function (traderTransactionItemId) {
    var $trDelete = $("#table-row-return-item-" + traderTransactionItemId);
    $($trDelete).css("background-color", "#FF3700");
    $($trDelete).fadeOut(500,
        function () {
            var table = $('#table-return-items').DataTable();
            table.row("#table-row-return-item-" + traderTransactionItemId).remove().draw(false);
            ValidationSaveAction();
        });

};

ValidationSaveAction = function () {
    var items = $('#table-return-items tbody tr.table-row-return-item-list');
    if (items.length === 0)
        DisableSaveAction();
    else
        EnableSaveAction();
};

DisableSaveAction = function () {
    $("#button-save-draft").hide();
    $("#button-save-review").hide();
};
EnableSaveAction = function () {
    $("#button-save-draft").show();
    $("#button-save-review").show();
};

SaveDraftSaleReturn = function () {
    SaveSaleReturn("Draft");
};

SaveToReviewSaleReturn = function () {
    SaveSaleReturn("PendingReview");
};


var $saleItemsSelected = [];

SaveSaleReturn = function (status) {


    $.LoadingOverlay("show");

    var $workgroup = {
        Id: $("#trader_sale_return_add_workgroup").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null || $workgroup.Id === "0") {
        $('.admintabs a[href="#return-1"]').tab('show');
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
        LoadingOverlayEnd();
        return;
    }

    var reference = {
        Id: $('#reference_id').val(),
        NumericPart: parseFloat($('#refedit').text()),
        Type: $('#reference_type').val(),
        Prefix: $('#reference_prefix').val(),
        Suffix: $('#reference_suffix').val(),
        Delimeter: $('#reference_delimeter').val(),
        FullRef: $('#reference_fullref').val()
    };
    if ($saleIdSelected === 0)
        $saleIdSelected = $("#sale-reference-id").val();// in the case update

    var sale = {
        Id: $saleIdSelected
    };

    var returnItems = [];
    var items = $('#table-return-items tbody tr.table-row-return-item-list');
    if (items.length > 0) {
        for (var i = 0; i < items.length; i++) {
            var itemName = $($(items[i]).find('td.return-row-item span')).text();

            var returnItemId = parseInt($($(items[i]).find('td.return-row-item input.return-item-id')).val());
            var traderTransactionItemId = parseInt($($(items[i]).find('td.return-row-item input.transaction-item-id')).val());
            var returnQuantity = parseFloat($($(items[i]).find('td.return-row-quantity input')).val());
            var returnCredit = parseFloat($($(items[i]).find('td.return-row-credit input')).val());
            var returnToInventory = $($(items[i]).find('td.return-row-to-inventory input')).is(":checked");
            
            var saleQuantity = parseFloat($($(items[i]).find('td.return-row-sale-quantity input')).val());
            if (returnQuantity > saleQuantity) {
                
                cleanBookNotification.error(_L("ERROR_MSG_648", [itemName]), "Qbicles");
                LoadingOverlayEnd();
                return;
            }

            if (returnCredit < 0) {
                cleanBookNotification.error(_L("ERROR_MSG_649", [itemName]), "Qbicles");
                LoadingOverlayEnd();
                return;
            }


            var returnItem = {
                Id: returnItemId,
                SaleItem:
                {
                    Id: traderTransactionItemId
                }, //This is the Sale TraderTransactionItem with which this return item is associated
                ReturnQuantity: returnQuantity,
                Credit: returnCredit,
                IsReturnedToInventory: returnToInventory
            };

            returnItems.push(returnItem);


        }
    }

    var $saleReturn = {
        Id: $("#sale-return-id").val(),
        Location: { Id: $('#local-manage-select').val() },
        Reference: reference,
        Workgroup: $workgroup,
        Status: status,
        Sale: sale,
        ReturnItems: returnItems
    };

    $.ajax({
        type: 'post',
        url: '/TraderSalesReturn/SaveTraderSaleReturn',
        data: { traderSaleReturn: $saleReturn },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-sale-return-add').modal('hide');
                cleanBookNotification.createSuccess();
                setTimeout(function () {
                    ShowTableSaleReturnValue(true);//cal method from trader.sales.return.js
                }, 500);
            } else if (response.actionVal === 2) {
                $('#app-trader-sale-return-add').modal('hide');
                cleanBookNotification.updateSuccess();
                setTimeout(function () {
                    ShowTableSaleReturnValue(true);//cal method from trader.sales.return.js
                }, 500);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
};