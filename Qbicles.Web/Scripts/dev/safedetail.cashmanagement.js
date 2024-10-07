$(function () {
    $("#search_dt").keyup(delay(function () {
        callBackDataTableReload("safe-transaction-list-table");
    },
        2000));

    $('#safe-transaction-input-datetimerange').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        startDate: new Date($("#fromDateTime").val()),
        endDate: new Date($("#toDateTime").val()),
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });

    $('#safe-transaction-input-datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('#safe-transaction-input-datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        filter.DateRange = $("#safe-transaction-input-datetimerange").val();
        callBackDataTableReload("safe-transaction-list-table");
    });
    $('#safe-transaction-input-datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        filter.DateRange = $("#safe-transaction-input-datetimerange").val();
        $('#safe-transaction-input-datetimerange').html('full history');
        callBackDataTableReload("safe-transaction-list-table");
    });
    renderSafeDetailDataTable();
    $("#safe-transaction-list-table").addClass("datatable");
});

function renderSafeDetailDataTable() {
    $("#safe-transaction-list-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#table_show').LoadingOverlay("show");
        } else {
            $('#table_show').LoadingOverlay("hide", true);
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            } else if (data.isTransfer) {
                $(row).removeClass().addClass("safeaccount even");
            }
        },
        "ajax": {
            "url": '/CashManagement/GetDataTableSafePayment',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search_dt").val(),
                    "datetime": $("#safe-transaction-input-datetimerange").val(),
                    "safeId": $("#safe-id").val() ? $("#safe-id").val() : 0,
                    "isApproved": false
                });
            }
        },
        "columns": [
            {
                name: "TransactionDateString",
                data: "TransactionDateString",
                orderable: false
            },
            {
                name: "TillName",
                data: "TillName",
                orderable: true
            },
            {
                name: "DirectionName",
                data: "DirectionName",
                orderable: true
            },
            {
                name: "Amount",
                data: "Amount",
                orderable: true
            },
            {
                name: "Balance",
                data: "Balance",
                orderable: true
            },
            {
                name: "Difference",
                data: "Difference",
                orderable: true
            },
            {
                name: "Status",
                data: "Status",
                orderable: false,
                render: function (value, type, row) {
                    var strStatus = '<span class="label label-lg ' + row.LabelStatus + '">' + row.Status + '</span>';
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = '';
                    if (!row.isPosPayment) {
                        if (row.isCheckpoint) {
                            str += '<td><button onclick="window.location.href=\'/CashManagement/DiscussionForSafeTransactions?cashAccountTransactionId=0&&tillPaymentId=0&&checkpointId=' + row.Id + '\'" class="btn btn-primary"><i class="fa fa-comments"></i> &nbsp; Discuss</button></td>'
                        } else if (row.isTillPayment) {
                            str += '<td><button onclick="window.location.href=\'/CashManagement/DiscussionForSafeTransactions?cashAccountTransactionId=0&&checkpointId=0&&tillPaymentId=' + row.Id + '\'" class="btn btn-primary"><i class="fa fa-comments"></i> &nbsp; Discuss</button></td>'
                        }
                    } else {
                        str += '';
                    }
                    return str;
                }
            }
        ],
        "order": [[2, "desc"]]
    });
}