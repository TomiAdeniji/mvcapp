function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        }, delay || 500);
    };
}

$('.manage-columns input[type="checkbox"]').on('change', function () {
    var table = $('#spot-list-table').DataTable();
    var column = table.column($(this).attr('data-column'));
    column.visible(!column.visible());
});

$(function () {


    $("#search_spot_count").keyup(searchThrottle(function () {
        $("#spot-list-table").DataTable().search($(this).val()).draw();
    }));
    $("#spotcount-workgroup-filter,#spotcount-status-filter").on("change", function () {
        $("#spot-list-table").DataTable().ajax.reload();
    });

    $("#spotcount-workgroup-filter,#spotcount-status-filter").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true,
    });

    $('#spotcount-datetimerange').daterangepicker({
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
    $('#spotcount-datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('#spotcount-datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));

        $("#spot-list-table").DataTable().ajax.reload();
    });
    $('#spotcount-datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#spotcount-datetimerange').html('full history');
        $("#spot-list-table").DataTable().ajax.reload();
    });

    tableSpotCountLoad();
});


function addSpotCount(id) {
    if (!id) id = 0;
    var ajaxUri = '/TraderSpotCount/AddEditSpotCount?id=' + id;
    $.LoadingOverlay("show");
    $('#app-trader-inventory-spot-count').empty();
    $('#app-trader-inventory-spot-count').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        var workgroupId = $("#transfer-workgroup-select").val();
        var locationId = $("#local-manage-select").val();
        ResetSpotCountItemSelected('tb_form_item', 'item', workgroupId, locationId, false);
        LoadingOverlayEnd();
    });
}
function editSpotCount(id) {
    if (!id) id = 0;
    var ajaxUri = '/TraderSpotCount/AddEditSpotCount?id=' + id;
    $.LoadingOverlay("show");
    $('#app-trader-inventory-spot-count').empty();
    $('#app-trader-inventory-spot-count').load(ajaxUri.trim().replace(/\s/g, ""), function () {
        var workgroupId = $("#transfer-workgroup-select").val();
        var locationId = $("#local-manage-select").val();
        ResetSpotCountItemSelected('tb_form_item', 'item', workgroupId, locationId, false);
        LoadingOverlayEnd();
    });
}

function validateSpecificsFormAdjust() {
    var valid = true;

    if ($("#spotCount_name").val() === "") {
        valid = false;
        $("#form_tabspec").validate().showErrors({ spot_name: "Name is required." });
    }
    if ($("#spotCount_Description").val() === "") {
        valid = false;
        $("#form_tabspec").validate().showErrors({ spot_description: "Description is required." });
    }
    if ($("#transfer-workgroup-select").val() === "") {
        valid = false;

        $("#form_tabspec").validate().showErrors({ workgroup: "Work group is required." });
    } else {
        $('#transfer-workgroup-select-error').remove();
    }
    return valid;
}
function btnSpotCountNext() {
    if (validateSpecificsFormAdjust()) {
        $('a[href="#spot-tab2"]').tab('show');
    } else {
        setTimeout(function () {
            $('a[href="#spot-tab1"]').tab('show');
        }, 500);

    }
}
function saveSpot(status) {
    var spot = {
        Id: $('#spotcount_id').val(),
        Name: $('#spotCount_name').val(),
        Description: $('#spotCount_Description').val(),
        Location: { Id: $('#local-manage-select').val() },
        ProductList: [],
        Workgroup: { Id: $('#transfer-workgroup-select').val() },
        Status: status
    }
    var trs = $('#tb_form_item tbody tr');
    var tds = $('#tb_form_item tbody tr td');
    if (trs.length > 0 && tds.length > 1) {
        for (var j = 0; j < trs.length; j++) {
            var id = $($(trs[j]).find('td.td_spot_name input.spot_id')).val();
            var spotItem = {
                Id: id,
                Product: { Id: $($(trs[j]).find('td.td_spot_sku input.spot_item_id')).val() },
                CountUnit: { Id: $($(trs[j]).find('td.td_spot_unit select')).val() },
                SpotCountValue: $($(trs[j]).find('td.td_spot_invoice span.demo')).text(),
                SavedInventoryCount: $($(trs[j]).find('td.td_spot_invoice span.demo')).text(),
                Notes: $($(trs[j]).find('td.td_spot_note input')).val()
            }
            var countUnit = $($(trs[j]).find('td.td_spot_unit select')).val();
            if (countUnit == null) {
                cleanBookNotification.error("The units of all items are required!", "Qbicles");
                LoadingOverlayEnd();
                return;
            }
            spot.ProductList.push(spotItem);
        }
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderSpotCount/SaveSpotCount',
        data: { spotCount: spot },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1 || response.actionVal === 2) {
                cleanBookNotification.createSuccess();
                $('#app-trader-inventory-spot-count').modal('toggle');
                //ShowSpotCountTable();
                $("#search_spot_count").val('');
                $("#spot-list-table").DataTable().ajax.reload();
            }
            else if (response.actionVal == 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

        }
    });
}
// Items
function ChangeSelectedUnit() {
    $('#tb_form_template tbody tr td.td_spot_unit').empty();
    var ajaxUri = '/Trader/TraderSaleSelectUnit?idLocation=' + $('#local-manage-select').val() + '&idTraderItem=' + $('#item').val().split(':')[0] + '&spotcount=true';
    $('#tb_form_template tbody tr td.td_spot_unit').load(ajaxUri, function () {
        $('#addrowitem').removeClass('disabledTab');
        $("#addrowitem").prop('disabled', false);
    });
};
function removeItem(id) {
    if ($('#spotcount_id').val() == 0) {
        $('#tb_form_item tbody tr.tr_spot_item_' + id).remove();
        var workgroupId = $("#transfer-workgroup-select").val();
        var locationId = $("#local-manage-select").val();

        ResetSpotCountItemSelected('tb_form_item', 'item', workgroupId, locationId, false);
    } else
        UpdateSpotCountProduct(id, true);

}

function addRowItemSpot() {

    var items = $('#item').val().split(':');
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
            $(clone).attr('class', 'tr_spot_item_' + item.Id);
            // filter to table 
            $($(clone).find('td.td_spot_name span')).text(item.Name);
            $($(clone).find('td.td_spot_name input')).val(0);
            $($(clone).find('td.td_spot_sku span')).text(item.SKU);
            $($(clone).find('td.td_spot_sku input')).val(item.Id);
            $($(clone).find('td.td_spot_invoice span.demo')).text(item.Level);
            $($(clone).find('td.td_spot_note')).val("");
            $($(clone).find('td.td_spot_unit select')).not('.multi-select').select2();
            $($(clone).find('td.td_spot_button button')).attr('onclick', "removeItem('" + item.Id + "')");

            var productItem = $('#tb_form_item tbody tr' + 'tr_spot_item_' + item.Id);
            if (productItem.length == 0)
                $('#tb_form_item tbody').append(clone);
            else {
                cleanBookNotification.error(_L("ERROR_MSG_373"), "Qbicles");
            }
            $('#item').val("");
            $('#item').select2({
                placeholder: 'Please select'
            });
            $("#addrowitem").prop('disabled', true);
            var workgroupId = $("#transfer-workgroup-select").val();
            var locationId = $("#local-manage-select").val();
            ResetSpotCountItemSelected('tb_form_item', 'item', workgroupId, locationId, false);

            if ($('#spotcount_id').val() > 0) {
                UpdateSpotCountProduct(item.Id, false);
            }
        },
        error: function (er) {

        }
    }).always(function () {
        $('#tb_form_item').LoadingOverlay("hide", true);
    });

}
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
                $('#transfer-workgroup-select-error').remove()
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $('.preview-workgroup').attr('style', 'display: none;');
    }
};
function changeItemByWorkGroup() {
    var workgroupId = $("#transfer-workgroup-select").val();
    var locationId = $("#local-manage-select").val();
    ResetSpotCountItemSelected('tb_form_item', 'item', workgroupId, locationId, false);
}
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
}
// ------- end workgroup -------------


function ResetSpotCountItemSelected(tableId, selectId, select2WorkGroupId, select2LocationId = 0, hasShowAllItems = false) {
    //if ($('#' + tableId) && $('#' + selectId)) {
    if ($('#spotcount_id').val() > 0)
        initSelect2MethodAJAX(selectId, '/Select2Data/GetSpotCountItemsById?spotCountId=' + $("#spotcount_id").val() + "&workGroupId=" + select2WorkGroupId + "&locationId=" + select2LocationId);
    //} 
    else {
        var trs = $('#' + tableId + ' tbody tr');
        var tds = $('#' + tableId + ' tbody tr td');
        var lstId = [];
        if (trs.length > 0 && tds.length > 1) {
            for (var i = 0; i < trs.length; i++) {
                lstId.push($($(trs[i]).find(' td.td_spot_sku input.spot_item_id')).val());
            }
        }
        var itemIds = lstId.join(',');
        //Init select2 with AJAX
        initSelect2MethodAJAX(selectId, '/Select2Data/GetSpotCountWasteItemsByWorkgroup?workGroupId=' + select2WorkGroupId + "&locationId=" + select2LocationId + "&itemIds=" + itemIds, {}, false);
    }
}

function tableSpotCountLoad() {
    var $spotsTable = $('#spot-list-table');

    if ($.fn.DataTable.isDataTable('#spot-list-table')) {
        $spotsTable.DataTable().destroy();
    }
    $spotsTable.on('processing.dt', function (e, settings, processing) {
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
            "url": '/TraderSpotCount/GetByLocationPagination',
            "type": 'POST',
            "data": function (d) {
                var workgroups = $("#spotcount-workgroup-filter").val();
                var status = $("#spotcount-status-filter").val();
                return $.extend({}, d, {
                    keyword: $('#search_spot_count').val(),
                    datetime: $("#spotcount-datetimerange").val(),
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
                    case 0:
                        _statusHTML = '<span class="label-lg">Initiated</span>';
                        break;
                    case 1:
                        _statusHTML = '<span class="label label-lg label-primary">Draft</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-warning">Count Started</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-info">Count Completed</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-success">Stock Adjusted</span>';
                        break;
                    case 5:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
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
                    return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderSpotCount/SpotCountMaster?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                else
                    return '<button class="btn btn-info" data-toggle="modal" data-target="#app-trader-inventory-spot-count" onclick="editSpotCount(' + data + ')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
            }
        }],
        rowId: function (a) {
            return 'spotcount_' + a.Id;
        },
        "initComplete": function (settings, json) {
            $('#spot-list-table').DataTable().ajax.reload();
        }
    });
    $('#spotlist-table_filter').hide();
}


function UpdateSpotCountProduct(tradeItemId, isDelete) {
    if ($('#spotcount_id').val() == 0)
        return;

    $('#tb_form_item').LoadingOverlay("show");
    var tr = $('#tb_form_item tbody tr.tr_spot_item_' + tradeItemId);

    var spotCountItem = {
        SpotCount: { Id: $('#spotcount_id').val() },
        Id: $($(tr).find('td.td_spot_name input.spot_id')).val(),
        Product: { Id: $($(tr).find('td.td_spot_sku input.spot_item_id')).val() },
        CountUnit: { Id: $($(tr).find('td.td_spot_unit select')).val() },
        SpotCountValue: 0,
        SavedInventoryCount: $($(tr).find('td.td_spot_invoice span.demo')).text(),
        Notes: $($(tr).find('td.td_spot_note input')).val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderSpotCount/UpdateSpotCountProduct',
        data: { spotCountItem: spotCountItem, isDelete: isDelete },
        dataType: 'json',
        success: function (status) {
            if (status.result == false) {
                cleanBookNotification.error(status.msg, "Qbicles");
                return;
            }
            //if (isDelete) {
            //    $('#tb_form_item tbody tr.tr_spot_item_' + tradeItemId).remove();
            //}
            $("#tb_form_item").DataTable().ajax.reload(null, false);
            var workgroupId = $("#transfer-workgroup-select").val();
            var locationId = $("#local-manage-select").val();
            ResetSpotCountItemSelected('tb_form_item', 'item-select', workgroupId, locationId);
            $("#spot-list").DataTable().ajax.reload(null, false);
        },
        error: function (er) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $('#tb_form_item').LoadingOverlay("hide", true);
    });
}