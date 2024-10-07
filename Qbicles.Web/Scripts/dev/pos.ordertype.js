// load table
function LoadTableDataPosOrderType(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
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
        "info": true,
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
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $('#order_type_search').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function LoadDataByServerSide() {
    var url = '/PointOfSale/TraderPosOrderTypetDataTable';

    var columns = [
        {
            name: "Name",
            data: "Name",
            orderable: true,
        },
        {
            name: "Classification",
            data: "Classification",
            orderable: true,
        },
        {
            name: "Summary",
            data: "Summary",
            orderable: true
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                str += '<button class="btn btn-warning" style="margin-right: 10px;" data-toggle="modal" data-target="#app-trader-pos-order-type-addedit" onclick="addEditPosOrderType(' + row.Id + ')"><i class="fa fa-pencil"></i></button>';
                if (row.IsUse) {
                    str += '<button class="btn btn-danger" disabled onclick="deleteOrderType(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                } else {
                    str += '<button class="btn btn-danger" onclick="deleteOrderType(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                }
                return str;
            }
        }
    ];
    LoadTableDataPosOrderType('community-list', url, columns, 0);
    CallBackFilterPosOrderTypeServeSide();
}

function CallBackFilterPosOrderTypeServeSide() {
    $("#community-list").DataTable().ajax.reload();
}

var delayTimer;
function searchKeyWork() {
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        CallBackFilterPosOrderTypeServeSide();
    }, 1500); // Will do the ajax stuff after 1500 ms, or 1 s
}

function addEditPosOrderType(id) {
    LoadingOverlay();
    $('#app-trader-pos-order-type-addedit').load('/PointOfSale/AddEditOrderType?id=' + id, function () {
        LoadingOverlayEnd();
    });
}

function saveOrderType() {
    if ($("#ordertype_form").valid()) {
        var orderType = {
            Id: $("#ordertype_id").val(),
            Name: $("#ordertype_name").val(),
            Summary: $("#ordertype_summary").val(),
            Classification: $("#ordertype_class").val()
        }
        LoadingOverlay();
        $.ajax({
            type: "post",
            url: "/PointOfSale/SaveOrderType",
            datatype: "json",
            data: {
                orderType: orderType
            },
            success: function (refModel) {
                CallBackFilterPosOrderTypeServeSide();
                if (refModel.actionVal == 1) {
                    cleanBookNotification.createSuccess();
                    $('#app-trader-pos-order-type-addedit').modal("toggle");
                }
                else if (refModel.actionVal == 2) {
                    cleanBookNotification.updateSuccess();
                    $('#app-trader-pos-order-type-addedit').modal("toggle");
                } else if (refModel.actionVal == 4) {
                    $("#ordertype_form").validate().showErrors({ name: refModel.msg });
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_380", [refModel.msg]), "Qbicles");
                    console.log(refModel.msg);
                }
            },
            error: function (xhr) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });;
    }
}
function deleteOrderType(id) {
    LoadingOverlay();
    $.ajax({
        type: 'delete',
        url: '/PointOfSale/DeletePosOrderType?id=' + id,
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                cleanBookNotification.removeSuccess();
                CallBackFilterPosOrderTypeServeSide();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_290"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};