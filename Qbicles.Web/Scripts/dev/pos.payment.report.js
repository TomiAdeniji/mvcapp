// show table
$(function () {
    //ShowTableContactValue();
    ShowTablePosPaymnetValue();
});
function LoadTableDataPosPaymnet(tableid, url, columns, orderIndex) {
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
                    keyword: $('#pp_keyword').val(),
                    datelimit: $('#limit_date_range').val(),
                    locations: $('#pp_locations').val(),
                    methods: $('#pp_methods').val(),
                    accounts: $('#pp_accounts').val(),
                    cashiers: $('#pp_cashiers').val(),
                    devices: $('#pp_devices').val(),
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function ShowTablePosPaymnetValue() {
    var url = '/TraderReports/TraderPosPaymentDataTable';

    var columns = [
        {
            name: "CreatedDate",
            data: "CreatedDate",
            orderable: true,
        },
        {
            name: "LocationName",
            data: "LocationName",
            orderable: true
        },
        {
            name: "RefFull",
            data: "RefFull",
            orderable: true
        },
        {
            name: "Method",
            data: "Method",
            orderable: true
        },
        {
            name: "AccountName",
            data: "AccountName",
            orderable: true
        },
        {
            name: "Cashier",
            data: "Cashier",
            orderable: true
        },
        {
            name: "PosDevice",
            data: "PosDevice",
            orderable: true,
        },
        {
            name: "Amount",
            data: "Amount",
            orderable: true,
        }
    ];
    LoadTableDataPosPaymnet('community-list', url, columns, 1);
    CallBackFilterDataPosPaymnetServeSide();
}

function CallBackFilterDataPosPaymnetServeSide() {
    $("#community-list").DataTable().ajax.reload();
}


var delayTimer;
function doSearch() {
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        CallBackFilterDataPosPaymnetServeSide();
    }, 2000); // Will do the ajax stuff after 1000 ms, or 1 s
}
function applyChange() {
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        CallBackFilterDataPosPaymnetServeSide();
    }, 1000); // Will do the ajax stuff after 1000 ms, or 1 s
}