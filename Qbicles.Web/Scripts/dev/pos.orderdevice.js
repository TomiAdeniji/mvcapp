// load table
function LoadTableDataPosDeviceType(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 0;
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
                    keyword: $('#device_search').val(),
                    orderTypeId: $('#device_filter').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function LoadDataByServerSide() {
    var url = '/PointOfSale/TraderPosDeviceTypetDataTable';

    var columns = [
        {
            name: "Name",
            data: "Name",
            orderable: true,
        },
        {
            name: "OrderTypes",
            data: "OrderTypes",
            orderable: false,
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                str += '<button class="btn btn-warning" style="margin-right: 10px;" data-toggle="modal" data-target="#app-trader-pos-device-type-addedit" onclick="addEditPosDeviceType(' + row.Id + ')"><i class="fa fa-pencil"></i></button>';
                if (row.IsUse) {
                    str += '<button class="btn btn-danger" disabled><i class="fa fa-trash"></i></button>';
                } else {
                    str += '<button class="btn btn-danger" onclick="deleteDeviceType(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                }
                return str;
            }
        }
    ];
    LoadTableDataPosDeviceType('community-list', url, columns, 0);
    CallBackFilterPosDeviceTypeServeSide();
}

function CallBackFilterPosDeviceTypeServeSide() {
    $("#community-list").DataTable().ajax.reload();
}

var delayTimer;
function searchKeyWork(noTimeer) {
    if (noTimeer) {
        clearTimeout(delayTimer);
        delayTimer = setTimeout(function () {
            CallBackFilterPosDeviceTypeServeSide();
        }, 1500); // Will do the ajax stuff after 1500 ms, or 1 s
    } else {
        CallBackFilterPosDeviceTypeServeSide();
    }
}

function addEditPosDeviceType(id) {
    LoadingOverlay();
    $('#app-trader-pos-device-type-addedit').load('/PointOfSale/AddEditDeviceType?id=' + id, function () {
        LoadingOverlayEnd();
    });
}

function saveDeviceType() {
    if ($("#devicetype_form").valid()) {
        var orderTypes = [];
        var orderTypeValue = $("#devicetype_types").val();
        if (orderTypeValue && orderTypeValue.length > 0) {
            for (var i = 0; i < orderTypeValue.length; i++) {
                orderTypes.push({
                    Id: orderTypeValue[i]
                });
            }
        }
        var deviceType = {
            Id: $("#devicetype_id").val(),
            Name: $("#devicetype_name").val(),
            PosOrderTypes: orderTypes
        }
        LoadingOverlay();
        $.ajax({
            type: "post",
            url: "/PointOfSale/SaveDeviceType",
            datatype: "json",
            data: {
                deviceType: deviceType
            },
            success: function (refModel) {
                CallBackFilterPosDeviceTypeServeSide();
                if (refModel.actionVal == 1) {
                    cleanBookNotification.createSuccess();
                    $('#app-trader-pos-device-type-addedit').modal("toggle");
                }
                else if (refModel.actionVal == 2) {
                    cleanBookNotification.updateSuccess();
                    $('#app-trader-pos-device-type-addedit').modal("toggle");
                } else if (refModel.actionVal == 4) {
                    $("#devicetype_form").validate().showErrors({ devicename: refModel.msg });
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
function deleteDeviceType(id) {
    LoadingOverlay();
    $.ajax({
        type: 'delete',
        url: '/PointOfSale/DeletePosDeviceType?id=' + id,
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                cleanBookNotification.removeSuccess();
                CallBackFilterPosDeviceTypeServeSide();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};