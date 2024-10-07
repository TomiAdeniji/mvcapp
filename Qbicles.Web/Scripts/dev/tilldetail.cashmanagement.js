// Add apply event for datarangepicker and render the dataTable
$(function () {

    $("#till-filter-option,#till-filter-status").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });

    $('#till-transaction-input-datetimerange').daterangepicker({
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

    $('#till-transaction-input-datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('#till-transaction-input-datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        filter.DateRange = $("#till-transaction-input-datetimerange").val();
        callBackDataTableReload("till-transaction-list-table");
    });
    $('#till-transaction-input-datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        filter.DateRange = $("#till-transaction-input-datetimerange").val();
        $('#till-transaction-input-datetimerange').html('full history');
        callBackDataTableReload("till-transaction-list-table");
    });

    renderDataTable();
    $("#till-transaction-list-table").addClass("datatable");

});

function renderDataTable() {
    $("#till-transaction-list-table").on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $("#till-transaction-list-table").LoadingOverlay("show");
        } else {
            $("#till-transaction-list-table").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
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
            }
        },
        "ajax": {
            "url": '/CashManagement/GetDataTableTillPayment',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#search_dt").val(),
                    tillId: $("#till-id").val() ? $("#till-id").val() : 0,
                    isApproved: false,
                    datetime: $("#till-transaction-input-datetimerange").val(),
                    status: $("#till-filter-status").val()
                });
            },
            "dataSrc": function (data) {               
                $("#till-balance-title").text(data.customResponse);
                return data.data;
            }
        },
        "columns": [
            {
                name: "TransactionDateString",
                data: "TransactionDateString",
                orderable: false
            },
            {
                name: "DeviceName",
                data: "DeviceName",
                orderable: true
            },
            {
                name: "TillName",
                data: "TillName",
                orderable: true
            },
            {
                name: "SafeName",
                data: "SafeName",
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
                            str += '<td><button onclick="window.location.href=\'/CashManagement/DiscussionCashManagement?tillPaymentId=0&&checkpointId=' + row.Id + '\'" class="btn btn-primary"><i class="fa fa-comments"></i> &nbsp; Discuss</button></td>'
                        } else {
                            str += '<td><button onclick="window.location.href=\'/CashManagement/DiscussionCashManagement?checkpointId=0&&tillPaymentId=' + row.Id + '\'" class="btn btn-primary"><i class="fa fa-comments"></i> &nbsp; Discuss</button></td>'
                        }
                    } else {
                        str += '';
                    }
                    return str;
                }
            }
        ],
        "order": [[2, "desc"]],
        "drawCallback": function (settings) {            
            console.log(settings);
        }
    });
}