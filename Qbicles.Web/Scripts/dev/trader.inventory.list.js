$(function () {

    var _dateFormat = $dateFormatByUser.toUpperCase();
    $(".inventory-select2").select2();
    $('#filter_daterange').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('#filter_daterange').val("");
    $('#filter_daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#filter_daterange').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        CallBackFilterDataInventoryServeSide();
    });
    $('#filter_daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#filter_daterange').html('full history');
        //CallBackFilterDataInventoryServeSide();
    });

    $("#inventory-basis-select,#max-day-to-last").on("change",
        function () {
            CallBackFilterDataInventoryServeSide();
        });

    $("#day-to-last-basis-select").on("change",
        function () {
            if ($("#day-to-last-basis-select").val() === '3') {
                $("#div-filter-by-date-range").show();
            } else {
                $("#div-filter-by-date-range").hide();
                $('.datetimerange').val("");
                CallBackFilterDataInventoryServeSide();
            }
        });

    $("#inventory_search_text").keyup(delay(function () {
        if ($("#inventory_search_text").val().length === 0 || $("#inventory_search_text").val().length >= 3)
            CallBackFilterDataInventoryServeSide();
    }, 1000));

    $('.manage-inventory-columns input[type="checkbox"]').on('change', function () {
        var table = $('#tb_inventories').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    GetDefaultInventoryTab();
    //$.LoadingOverlay("show");
    GetDataInventories();
    //Reorders
    InitReorderSearch();
    InitTableReorder();
    
});
function CallBackFilterDataInventoryServeSide() {
    $("#tb_inventories").DataTable().ajax.reload();
};


function GetDataInventories() {
    var url = '/TraderInventory/GetInventoryServerSide';

    var columns = [
        {
            name: "Icon",
            data: "Icon",
            orderable: false,
            render: function (value, type, row) {
                return "<div class='table-avatar' style='background-image: url(\"" + row.Icon + "\");'></div>";
            }
        },
        {
            name: "Item",
            data: "Item",
            orderable: true
        },
        {
            name: "Description",
            data: "Description",
            orderable: true
        },
        {
            id: "",
            name: "Unit",
            data: "Unit",
            orderable: true,
            render: function (value, type, row) {
                return "<span id='row-unit-item-" + row.Id + "'>" + row.Unit + "</span>";
            }
        },
        {
            name: "AverageCost",
            data: "AverageCost",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-average-cost-" + row.Id + "'>" + row.AverageCost + "</span>";
            }
        },
        {
            name: "LatestCost",
            data: "LatestCost",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-latest-cost-" + row.Id + "'>" + row.LatestCost + "</span>";
            }
        },
        {
            name: "CurrentInventory",
            data: "CurrentInventory",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-current-inventory-" + row.Id + "'>" + row.CurrentInventory + "</span>";
            }
        },
        {
            name: "Can reorder",
            data: "isBought",
            orderable: false,
            render: function (value, type, row) {
                return value?'Yes':'No';
            }
        },
        {
            name: "DaysToLast",
            data: "DaysToLast",
            orderable: false,
            render: function (value, type, row) {
                if (row.DaysToLastHighlighted)
                    return "<span id='row-day-to-last-" + row.Id + "' class='label label-danger' data-tooltip='Reorder required' data-tooltip-color='#dd4b39'>" + row.DaysToLast + "</span>";
                return "<span id='row-day-to-last-" + row.Id + "'>" + row.DaysToLast + "</span>";
            }
        },
        {
            name: "MinInventory",
            data: "MinInventory",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-min-inventory-" + row.Id + "'>" + row.MinInventory + "</span>";
            }
        },
        {
            name: "MaxInventory",
            data: "MaxInventory",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-max-inventory-" + row.Id + "'>" + row.MaxInventory + "</span>";
            }
        },
        {
            name: "InventoryTotal",
            data: "InventoryTotal",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-inventory-total-" + row.Id + "'>" +row.InventoryTotal + "</span>";
            }
        },
        {
            name: "Associated",
            data: "Associated",
            orderable: false,
            render: function (value, type, row) {
                if (row.Associated === "0 item(s)") {
                    return "";
                } else {
                    return "<button onclick='ShowIngredientsItemAssociated(" + row.Id + ")' class='btn btn-info'><i class='fa fa-cube'></i> &nbsp; " + row.Associated + "</button>";
                }
            }
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = "<div class='btn-group options'><button type='button' class='btn btn-success dropdown-toggle' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>";
                str += "<i class='fa fa-cog'></i> &nbsp; Options</button><ul class='dropdown-menu dropdown-menu-right' style='right: 0;'>";
                str += "<li><a href='javascript:' onclick=ShowChangeItemUnits(" + row.Id + ")>Change display unit</a></li>";
                str += "<li><a href='javascript:' onclick='editTraderItem(" + row.EditType + ", " + row.Id + ",\"inventory-tab\")'>Edit</a></li>";
                str += "</ul></div>";
                return str;
            }
        }
    ];

    $("#tb_inventories").on('processing.dt', function (e, settings, processing) {
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
            "infoFiltered": "",
            "decimal": '.',
            "thousands": ',',
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
                    keySearch: $('#inventory_search_text').val(),
                    inventoryBasis: $('#inventory-basis-select').val(),
                    maxDayToLast: $('#max-day-to-last').val(),
                    days2Last: $('#filter_daterange').val(),
                    dayToLastOperator: $("#day-to-last-basis-select").val(),
                    hasSymbol : false
                });
            }
        },
        "columns": columns,
        "columnDefs": [
            {
                "targets": [0, 2],
                "visible": false
                //"searchable": false
            }],
        "order": [[1, "asc"]]
    });
    CallBackFilterDataInventoryServeSide();
};



function ShowIngredientsItemAssociated(id) {
    var ajaxUri = '/TraderInventory/ShowIngredientsItemAssociated?itemId=' + id;
    AjaxElementShowModal(ajaxUri, 'associated-items-view');
};

function ShowChangeItemUnits(id) {
    var ajaxUri = '/TraderInventory/ShowChangeItemUnits?itemId=' + id;
    AjaxElementShowModal(ajaxUri, 'app-trader-change-unit');
};

function UpdateItemUnit(itemId) {

    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/TraderInventory/UpdateChangeItemUnit',
        data: {
            unitId: $("#display-unit-change").val(),
            itemId: itemId,
            inventoryBasis: $('#inventory-basis-select').val(),
            maxDayToLast: $('#max-day-to-last').val(),
            days2Last: $('#filter_daterange').val(),
            dayToLastOperator: $("#day-to-last-basis-select").val(),
            hasSymbol: false

        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {

                var $id = response.Object.Id;
                $("#row-unit-item-" + $id).text(response.Object.Unit);
                $("#row-average-cost-" + $id).text(response.Object.AverageCost);
                $("#row-latest-cost-" + $id).text(response.Object.LatestCost);
                $("#row-current-inventory-" + $id).text(response.Object.CurrentInventory);
                $("#row-day-to-last-" + $id).text(response.Object.DaysToLast);
                $("#row-min-inventory-" + $id).text(response.Object.MinInventory);
                $("#row-max-inventory-" + $id).text(response.Object.MaxInventory);
                $("#row-inventory-total-" + $id).text(response.Object.InventoryTotal);

                $("#app-trader-change-unit").modal('hide');
                cleanBookNotification.updateSuccess();

            } else {
                cleanBookNotification.error("Have an error, detail: " + response.msg, "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};
function InitTableReorder() {
    var $tblReorders = $('#tblReorders');
    $tblReorders.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderInventory/SearchReorders",
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $('#txtSearchReorder').val(),
                    "status": $('#slReorderStatus').val(),
                    "daterange": $('#txtReorderDateRanger').val()
                });
            }
        },
        columns: [
            { "title": "Reference", "data": "Reference", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Items", "data": "Items", "searchable": true, "orderable": false },
            { "data": "Total", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": true, "orderable": true },
            { "title": "", "data": "Id", "searchable": false, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 2,
                "data": "Items",
                "render": function (data, type, row, meta) {
                    var _htmlItems = '<button class="btn btn-info" onclick="ShowReorderItems(' + row.Id + ',\'' + row.Reference+'\')"><i class="fa fa-cube"></i> &nbsp; ' + data + '</button>';
                    return _htmlItems;
                }
            },
            {
                "targets": 3,
                "data": "Total",
                "render": function (data, type, row, meta) {
                    return toCurrencySymbol(data,false);
                }
            },
            {
                "targets": 4,
                "data": "Status",
                "render": function (data, type, row, meta) {
                    var _htmlItems = '<span class="label label-lg label-primary">Incomplete</span>';
                    if (data == 1)
                        _htmlItems = '<span class="label label-lg label-success">Complete</span>';
                    return _htmlItems;
                }
            },
            {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '';
                    if (row.Status == 0)
                        _htmlOptions = '<button class="btn btn-info" onclick="window.location.href=\'/Trader/TraderReorder?id=' + data + '\';"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function InitReorderSearch() {
    $('#txtSearchReorder').keyup(delay(function () {
        $('#tblReorders').DataTable().ajax.reload();
    }, 700));
    $('#slReorderStatus').change(delay(function () {
        $('#tblReorders').DataTable().ajax.reload();
    }, 700));
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('#txtReorderDateRanger').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('#txtReorderDateRanger').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#tblReorders').DataTable().ajax.reload();
    });
    $('#txtReorderDateRanger').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
    });
}
function ShowReorderItems(id) {
    if (id > 0) {
        $('#reorder-breakdown').load("/TraderInventory/ReorderBreakdown", { reorderid: id }, function () {
            $("#reorder-breakdown .select2").select2({
                placeholder: "Please select"
            });
            $('#txtsearchbreakdown').keyup(delay(function () {
                $('#tblReorderitems').DataTable().search(this.value).draw();
            }, 200));
            $('#slprimarycontactbreakdown').change(delay(function () {
                var keyword = $('#slprimarycontactbreakdown option:selected').text();
                $('#tblReorderitems').DataTable().search(keyword == "Show all" ? "" : keyword).draw();
            }, 200));
        });
        $('#reorder-breakdown').modal("show");
    }
}
function ConfirmReorderProcess() {
    
    var params = {
        keySearch: $('#inventory_search_text').val(),
        inventoryBasis: $('#inventory-basis-select').val(),
        maxDayToLast: $('#max-day-to-last').val(),
        days2Last: $('#filter_daterange').val(),
        dayToLastOperator: $("#day-to-last-basis-select").val()
    };
    if (parseInt(params.maxDayToLast) == 1)
        $('#maxdayshow').text("1 day")
    else
        $('#maxdayshow').text(params.maxDayToLast + " days")
    $.get("/TraderInventory/CountReorderItems", params)
        .done(function (response) {
            if (response) {
                $('#countReorderItems').text(response);
            }
        });
    $('#app-trader-inventory-reorder').modal("show");
}
function AgreeReorderProcess() {
    var params = {
        keySearch: $('#inventory_search_text').val(),
        inventoryBasis: $('#inventory-basis-select').val(),
        maxDayToLast: $('#max-day-to-last').val(),
        days2Last: $('#filter_daterange').val(),
        dayToLastOperator: $("#day-to-last-basis-select").val()
    };
    $.LoadingOverlay('show');
    $.post("/TraderInventory/TriggeringReorderProcess", params, function (response) {
        if (response.result) {
            window.location.href = "/Trader/TraderReorder?id="+response.Object.id;
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
        LoadingOverlayEnd();
    });
}
function GetDefaultInventoryTab() {
    var inventorykey = "sub-inventory-tab";
    if (getLocalStorage(inventorykey))
        activeSubTab = getLocalStorage(inventorykey);
    $('a[href="#' + activeSubTab + '"]').tab('show');
}
function SaveInventoryTab(tab) {
    var inventorykey = "sub-inventory-tab";
    setLocalStorage(inventorykey, tab);
}