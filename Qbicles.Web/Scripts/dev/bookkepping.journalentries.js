
var startDate = "", endDate = "";

$(document).ready(function () {
    if (jQuery().daterangepicker) {
        $('.daterange-journal').daterangepicker({
            autoUpdateInput: false,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase()
            }
        });

        $('.daterange-journal').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
            $('#filter-date').html(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));

            startDate = picker.startDate.format($dateFormatByUser.toUpperCase());
            endDate = picker.endDate.format($dateFormatByUser.toUpperCase());

            ReloadJournalEntries();
        });

        $('.daterange-journal').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('Limit to a date range');
            $('#filter-date').html('full history');
            startDate = ""; endDate = "";
            ReloadJournalEntries();
        });

    }

    ClearFilterJournalEntries();

    $('#filter-account').change(function () {
        ReloadJournalEntries();
    });
    $('#filter-status').change(function () {
        ReloadJournalEntries();
    });
    $('#filter-group').change(function () {
        ReloadJournalEntries();
    });

});

filterJournalEntries = function () {

    $("#tb_journal_entries")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing && $('.loadingoverlay').length === 0) {
                $("#tb_journal_entries").LoadingOverlay("show");
            } else {
                $("#tb_journal_entries").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
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
            "pageLength": 10,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "ajax": {
                "url": '/Bookkeeping/BKJournalEntriesContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "accounts": $("#filter-account").val(),
                        "status": $("#filter-status").val(),
                        "groups": $("#filter-group").val(),
                        "dates": startDate + ";" + endDate
                    });
                },
                "dataSrc": function (data) {
                    $("#total-journals").text(data.recordsTotal);
                    return data.data;
                }
            },
            "columns": [
                {
                    data: "Number",
                    orderable: true
                },
                {
                    data: "Group",
                    orderable: true
                },
                {
                    data: "PostedDate",
                    orderable: true
                },
                {
                    data: "Description",
                    orderable: true
                },
                {
                    name: "Status",
                    data: "Status",
                    orderable: true,
                    render: function (value, type, row) {
                        var strStatus = '<span class="label label-lg ' + row.StatusCss + '">' + row.Status + '</span>';
                        return strStatus;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    width: "150px",
                    render: function (value, type, row) {
                        var str = "<a href='/Bookkeeping/JournalEntry?id=" + row.Id + "' class='btn btn-primary'><i class='fa fa-eye' style='color: #fff;'></i></a>";

                        return str;
                    }
                },
            ],
            "drawCallback": function (settings) {
                
            },
            "initComplete": function (settings, json) {
                $('#tb_journal_entries').DataTable().ajax.reload();
            },
            "order": [[0, "asc"]]
        });
};

ClearFilterJournalEntries = function () {
    $('.daterange-journal').val('Limit to a date range');
    $('#filter-date').html('full history');
    startDate = ""; endDate = "";

    $("#filter-account").val("").trigger("change");
    $("#filter-status").val("").trigger("change");
    $("#filter-group").val("").trigger("change");

    filterJournalEntries();
}

function ReloadJournalEntries() {
    $("#tb_journal_entries").DataTable().ajax.reload(); //Reload data
}