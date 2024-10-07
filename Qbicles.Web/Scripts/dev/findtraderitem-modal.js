$(document).ready(function () {
    $("#traderitem_filter_list_filter").hide();
    $("#invenory-item .select2, #non-invenory-item .select2").select2(); 
    getInventory();
    getNoneInventory();
    //Event for Inventory
    $('#invenory-item input[name=search]').keyup(delay(function () {
        $('#tblInventory').DataTable().ajax.reload();
    }, 1000));
    $('#invenory-item select[name=location]').change(function () {
        $('#tblInventory').DataTable().ajax.reload();
    });
    $('#invenory-item select[name=group]').change(function () {
        $('#tblInventory').DataTable().ajax.reload();
    });
    //Event for None Inventory
    $('#non-invenory-item input[name=search]').keyup(delay(function () {
        $('#tblNoneInventory').DataTable().ajax.reload();
    }, 1000));
    $('#non-invenory-item select[name=location]').change(function () {
        $('#tblNoneInventory').DataTable().ajax.reload();
    });
    $('#non-invenory-item select[name=group]').change(function () {
        $('#tblNoneInventory').DataTable().ajax.reload();
    });
})
function getInventory() {
    var $tblInventory = $('#tblInventory');
    $tblInventory.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblInventory.LoadingOverlay("show");
        } else {
            $tblInventory.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        autoWidth: true,
        searchDelay: 800,
        pageLength: 10,
        //deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/TraderItem/GetTradeItemInventory",
            "data": function (d) {
                var groupId = $('#invenory-item select[name=group]').val();
                return $.extend({}, d, {
                    "sku": $('#invenory-item input[name=search]').val(),
                    "locationId": $('#invenory-item select[name=location]').val(),
                    "groupId": (groupId ? groupId:0),
                    "isNoneInventory": false,
                });
            }
        },
        columns: [
            {
                "data": "ImageUri",
                "orderable": false,
                "render": function (value, type, row) {
                    return '<div class="table-avatar mini" style="background-image: url(\'' + value +'\');"></div>';
                }
            },
            { "data": "Name", "orderable": true },
            { "data": "SKU", "orderable": true },
            { "data": "Group", "orderable": true },
            {
                "data": "Id",
                "orderable": false,
                "render": function (value, type, row) {
                    var _html = '<button type="button" class="btn btn-success" onclick="selectTraderItem(' + value + ',\'' + row.SKU + '\',\'' + row.ImageUri + '\')"><i class="fa fa-check"></i></button>';
                    return _html;
                }
            }
        ]
    });
}
function getNoneInventory() {
    var $tblNoneInventory = $('#tblNoneInventory');
    $tblNoneInventory.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblNoneInventory.LoadingOverlay("show");
        } else {
            $tblNoneInventory.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        autoWidth: true,
        searchDelay: 800,
        pageLength: 10,
        //deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/TraderItem/GetTradeItemInventory",
            "data": function (d) {
                var groupId = $('#non-invenory-item select[name=group]').val();
                return $.extend({}, d, {
                    "sku": $('#non-invenory-item input[name=search]').val(),
                    "locationId": $('#non-invenory-item select[name=location]').val(),
                    "groupId": (groupId ? groupId:0),
                    "isNoneInventory": true,
                });
            }
        },
        columns: [
            {
                "data": "ImageUri",
                "orderable": false,
                "render": function (value, type, row) {
                    return '<div class="table-avatar mini" style="background-image: url(\'' + value + '\');"></div>';
                }
            },
            { "data": "Name", "orderable": true },
            { "data": "SKU", "orderable": true },
            { "data": "Group", "orderable": true },
            {
                "data": "Id",
                "orderable": false,
                "render": function (value, type, row) {
                    var _html = '<button type="button" class="btn btn-success" onclick="selectTraderItem(' + value + ',\'' + row.SKU + '\',\'' + row.ImageUri + '\')"><i class="fa fa-check"></i></button>';
                    return _html;
                }
            }
        ]
    });
}