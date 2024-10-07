$(function () {


    $("#search_waste").keyup(searchThrottle(function () {
        $("#waste-list").DataTable().search($(this).val()).draw();
    }));

    $("#waste-workgroup-filter,#waste-status-filter").on("change",
        function () { $("#waste-list").DataTable().ajax.reload(); });

    $("#waste-workgroup-filter,#waste-status-filter").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true,
    });

    $('#waste-datetimerange').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
    $('#waste-datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('#waste-datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));

        $("#waste-list").DataTable().ajax.reload();
    });
    $('#waste-datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#waste-datetimerange').html('full history');
        $("#waste-list").DataTable().ajax.reload();
    });

    tableWasteReportLoad();
});


var filter = {
    Workgroup: " ",
    Key: ""
}

function searchOnTableWaste() {
    var listKey = [];
    if ($('#waste-workgroup-filter').val() !== " " && $('#waste-workgroup-filter').val() !== "" && $('#waste-workgroup-filter').val() !== null) {
        listKey.push($('#waste-workgroup-filter').val());
    }

    var keys = $('#search_waste').val().split(' ');
    if ($('#search_waste').val() !== "" && $('#search_waste').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#waste-list").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#waste-list").val("");
}



$('.manage-columns input[type="checkbox"]').on('change', function () {
    var table = $('#waste-list').DataTable();
    var column = table.column($(this).attr('data-column'));
    column.visible(!column.visible());
});

function addWasteReport(id) {
    if (!id) id = 0;
    var ajaxUri = '/TraderWasteReport/AddEditWasteReport?id=' + id;
    $.LoadingOverlay("show");
    $('#app-trader-waste-report').empty();
    $('#app-trader-waste-report').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        LoadingOverlayEnd();
    });
};

function editWasteReport(id) {
    if (!id) id = 0;
    var ajaxUri = '/TraderWasteReport/AddEditWasteReport?id=' + id;
    $.LoadingOverlay("show");
    $('#app-trader-waste-report').empty();
    $('#app-trader-waste-report').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        LoadingOverlayEnd();
    });
};
// ----------- workgroup ---------
function WorkGroupSelectedChange() {

    $workgroupId = $("#transfer-workgroup-select").val();
    if ($workgroupId !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                $('.preview-workgroup').show();
                changeItemByWorkGroup();
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
                $('#transfer-workgroup-select-error').remove();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        LoadingOverlayEnd();
        $('.preview-workgroup').attr('style', 'display: none;');
    }
};

function ShowGroupMember() {

    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
};

function changeItemByWorkGroup() {
    var workgroupId = $("#transfer-workgroup-select").val();
    var locationId = $("#local-manage-select").val();
    ResetWasteItemSelected('tb_form_item', 'item-select', workgroupId, locationId);
};

// ------------------ items -----------------

function ValidWasteReport() {
    var valid = true;

    if ($("#waste_name").val() === "") {
        valid = false;
        $("#form_tabspec").validate().showErrors({ waste_name: "Name is required." });
    }
    if ($("#waste_Description").val() === "") {
        valid = false;
        $("#form_tabspec").validate().showErrors({ waste_description: "Description is required." });
    }
    if ($("#transfer-workgroup-select").val() === "") {
        valid = false;

        $("#form_tabspec").validate().showErrors({ workgroup: _L("ERROR_MSG_244") });
    } else {
        $('#transfer-workgroup-select-error').remove();
    }
    return valid;
};

function btnWasteReportNext() {
    if (ValidWasteReport()) {
        $('a[href="#waste-tab2"]').tab('show');
    } else {
        setTimeout(function () {
            $('a[href="#waste-tab1"]').tab('show');
        }, 500);

    }
};

function laodWorkGroup(id) {
    var value = $('#' + id).val();
    if (!value) value = "0";
    $('#' + id).select2('destroy');
    var ajaxUri = '/TraderWasteReport/LoadWorkGroup';
    $('#' + id).empty();
    $('#' + id).load(ajaxUri.trim().replace(/\s/g, ""), function () {
        $('#' + id).val(value);
        $('#' + id).select2();
    });
};
function saveWaste(status) {
    if (status == 'Draft' && $('#waste_id').val() > 0) {
        cleanBookNotification.createSuccess();
        return;
    }

    $.LoadingOverlay("show");

    var waste = {
        Id: $('#waste_id').val(),
        Name: $('#waste_name').val(),
        Description: $('#waste_Description').val(),
        Location: { Id: $('#local-manage-select').val() },
        ProductList: [],
        Workgroup: { Id: $('#transfer-workgroup-select').val() },
        Status: status
    }
    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length > 0 && tds.length > 1) {
        for (var j = 0; j < trs.length; j++) {
            var id = $($(trs[j]).find('td.td_waste_name input.waste_id')).val();
            var wasteItem = {
                Id: id,
                Product: { Id: $($(trs[j]).find('td.td_waste_sku input.waste_item_id')).val() },
                CountUnit: { Id: $($(trs[j]).find('td.td_waste_unit select')).val() },
                WasteCountValue: 0,
                SavedInventoryCount: $($(trs[j]).find('td.td_waste_invoice span.demo')).text(),
                Notes: $($(trs[j]).find('td.td_waste_note input')).val()
            }
            var itemUnit = $($(trs[j]).find('td.td_waste_unit select')).val();
            if (itemUnit == null) {
                cleanBookNotification.error("The units of all items are required!", "Qbicles");
                LoadingOverlayEnd();
                return;
            }
            waste.ProductList.push(wasteItem);
        }
    }
    $.ajax({
        type: 'post',
        url: '/TraderWasteReport/SaveWasteReport',
        data: { wasteReport: waste },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1 || response.actionVal === 2) {
                cleanBookNotification.createSuccess();
                $('#app-trader-waste-report').modal('hide');
                $("#search_waste").val('');
                $("#waste-list").DataTable().ajax.reload();
                laodWorkGroup('waste-workgroup-filter');
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

};
// Items
function ChangeSelectedUnit() {
    $('#tb_form_template tbody tr td.td_waste_unit').empty();
    var ajaxUri = '/Trader/TraderSaleSelectUnit?idLocation=' + $('#local-manage-select').val() + '&idTraderItem=' + $('#item-select').val().split(':')[0] + '&spotcount=true';
    //var ajaxUri = '/TraderWasteReport/WasteUnitSelect?unitId=0&itemId=' + $('#item').val().split(':')[0];
    $('#tb_form_template tbody tr td.td_waste_unit').load(ajaxUri, function () {
        $('#addrowitem').removeClass('disabledTab');
        $("#addrowitem").prop('disabled', false);
    });
};


function removeWasteItem(id) {
    if ($('#waste_id').val() == 0) {
        $('#tb_form_item tbody tr.tr_waste_item_' + id).remove();
        var workgroupId = $("#transfer-workgroup-select").val();
        var locationId = $("#local-manage-select").val();

        ResetWasteItemSelected('tb_form_item', 'item', workgroupId, locationId, false);
    } else
        UpdateWasteReportProduct(id, true);
}
function addRowItemWaste() {
    var items = $('#item-select').val().split(':');
    $('#tb_form_item').LoadingOverlay("show");
    var ivId = parseInt(items[4]);
    $.ajax({
        type: 'post',
        url: '/Select2Data/GetUnusedInventoryQuantity',
        data: { inventoryId: ivId },
        dataType: 'json',
        success: function (currentInventory) {
            var item = {
                Id: items[0],
                ImageUri: items[1],
                Name: items[2],
                SKU: items[3],
                Level: currentInventory
            };

            var clone = $('#tb_form_template tbody tr').clone();
            $(clone).attr('class', 'tr_waste_item_' + item.Id);
            // filter to table 
            $($(clone).find('td.td_waste_name span')).text(item.Name);
            $($(clone).find('td.td_waste_name input')).val(0);
            $($(clone).find('td.td_waste_sku span')).text(item.SKU);
            $($(clone).find('td.td_waste_sku input')).val(item.Id);
            $($(clone).find('td.td_waste_invoice span.demo')).text(item.Level);

            $($(clone).find('td.td_waste_note')).val("");
            $($(clone).find('td.td_waste_note')).attr('onchange', "UpdateWasteReportProduct('" + item.Id + "',false)");

            $($(clone).find('td.td_waste_unit select')).not('.multi-select').select2();
            $($(clone).find('td.td_waste_unit select')).attr('onchange', "UpdateWasteReportProduct('" + item.Id + "',false)");

            $($(clone).find('td.td_waste_button button')).attr('onclick', "removeWasteItem('" + item.Id + "')");

            var productItem = $('#tb_form_item tbody tr' + 'tr_waste_item_' + item.Id);
            if (productItem.length === 0) {
                $('#tb_form_item tbody').prepend(clone);
                //$("#tb_form_item").DataTable().draw();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_373"), "Qbicles");
            }
            $('#item-select').val("");
            $('#item-select').select2({
                placeholder: 'Please select'
            });

            $("#addrowitem").prop('disabled', true);
            var workgroupId = $("#transfer-workgroup-select").val();
            var locationId = $("#local-manage-select").val();
            ResetWasteItemSelected('tb_form_item', 'item-select', workgroupId, locationId);


            if ($('#waste_id').val() > 0) {
                UpdateWasteReportProduct(item.Id, false);
            }
        },
        error: function (er) {

        }
    }).always(function () {
        $('#tb_form_item').LoadingOverlay("hide", true);
    });

};


function UpdateWasteReportProduct(tradeItemId, isDelete) {
    if ($('#waste_id').val() == 0)
        return;

    $('#tb_form_item').LoadingOverlay("show");
    var tr = $('#tb_form_item tbody tr.tr_waste_item_' + tradeItemId);
    var wasteItem = {
        WasteReport: { Id: $('#waste_id').val() },
        Id: $($(tr).find('td.td_waste_name input.waste_id')).val(),
        Product: { Id: $($(tr).find('td.td_waste_sku input.waste_item_id')).val() },
        CountUnit: { Id: $($(tr).find('td.td_waste_unit select')).val() },
        WasteCountValue: 0,
        SavedInventoryCount: $($(tr).find('td.td_waste_invoice span.demo')).text(),
        Notes: $($(tr).find('td.td_waste_note input')).val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderWasteReport/UpdateWasteReportProduct',
        data: { wasteItem: wasteItem, isDelete: isDelete },
        dataType: 'json',
        success: function (status) {
            if (status.result == false) {
                cleanBookNotification.error(status.msg, "Qbicles");
                return;
            }
            //if (isDelete) {
            //    $('#tb_form_item tbody tr.tr_waste_item_' + tradeItemId).remove();
            //}
            $("#tb_form_item").DataTable().ajax.reload(null, false);
            var workgroupId = $("#transfer-workgroup-select").val();
            var locationId = $("#local-manage-select").val();
            ResetWasteItemSelected('tb_form_item', 'item-select', workgroupId, locationId);
            $("#waste-list").DataTable().ajax.reload(null, false);
        },
        error: function (er) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $('#tb_form_item').LoadingOverlay("hide", true);
    });
}








// ------- end workgroup -------------
function ResetWasteItemSelected(tableId, selectId, select2WorkGroupId, select2LocationId = 0) {

    if ($('#' + tableId) && $('#' + selectId)) {
        if ($('#waste_id').val() > 0)
            initSelect2MethodAJAX(selectId, '/Select2Data/GetWasteReportItemsById?wasteReportId=' + $("#waste_id").val() + "&workGroupId=" + select2WorkGroupId + "&locationId=" + select2LocationId);
        else {
            var trs = $('#' + tableId + ' tbody tr');
            var tds = $('#' + tableId + ' tbody tr td');
            var lstId = [];
            if (trs.length > 0 && tds.length > 1) {
                for (var i = 0; i < trs.length; i++) {
                    lstId.push($($(trs[i]).find(' td.td_waste_sku input.waste_item_id')).val());
                }
            }
            var itemIds = lstId.join(',');
            //Init select2 with AJAX
            initSelect2MethodAJAX(selectId, '/Select2Data/GetSpotCountWasteItemsByWorkgroup?workGroupId=' + select2WorkGroupId + "&locationId=" + select2LocationId + "&itemIds=" + itemIds, {}, false);
        }
    }
}

function tableWasteReportLoad() {
    var $wasteTable = $('#waste-list');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#waste-list')) {
        $wasteTable.DataTable().destroy();
    }
    $wasteTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).dataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
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
        "order": [[1, "desc"]],
        "ajax": {
            "url": '/TraderWasteReport/GetByLocationPagination',
            "type": 'POST',
            "data": function (d) {
                var workgroups = $("#waste-workgroup-filter").val();
                var status = $("#waste-status-filter").val();
                return $.extend({}, d, {
                    keyword: $('#search_waste').val(),
                    datetime: $("#waste-datetimerange").val(),
                    workgroups: _.isNull(workgroups) ? [-1] : workgroups,
                    status: _.isNull(status) ? [-1] : status
                });
            }
        },
        columns: [
            { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Created", "data": "CreatedBy", "searchable": true, "orderable": true },
            { "title": "Workgroup", "data": "WorkgroupName", "searchable": true, "orderable": true },
            { "title": "Items", "data": "ItemsCount", "searchable": true, "orderable": false },
            { "title": "Description", "data": "Description", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Options", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 6,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 1:
                        _statusHTML = '<span class="label label-lg label-primary">Draft</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-warning">Started</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-info">Completed</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-success">Stock Adjusted</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                }
                return _statusHTML;
            }
        },
        {
            "targets": 7,
            "data": "Id",
            "render": function (data, type, row, meta) {
                if (row.Status != 1)
                    return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderWasteReport/WasteReportMaster?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                else
                    return '<button class="btn btn-info" data-toggle="modal" data-target="#app-trader-waste-report" onclick="editWasteReport(' + data + ')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
            }
        }],
        "initComplete": function (settings, json) {
            $('#waste-list').DataTable().ajax.reload();
        }
    });
    $('#waste-table_filter').hide();
}
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