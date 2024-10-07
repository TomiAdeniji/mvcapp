var filter = {
    BudgetGroupId: $("#budget-group-Id").val(),
    BudgetScenarioId: $("#budget-scenario-Id").val(),
    WorkgroupId: 0,
    Dimensions: "",
    DateRange: ""
};

WorkGroupFilter = function () {
    CallBackDataTableBudgetGroupReport();
};

DimensionFilter = function () {
    CallBackDataTableBudgetGroupReport();
};

$(function () {


    var dateTimeFormat = getDateTimeFormat();

    $('#budget-group-report-date-range').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: dateTimeFormat
        }
    });
    $('#budget-group-report-date-range').val('');
    $('#budget-group-report-date-range').on('apply.daterangepicker', function (ev, picker) {
        var startDate = picker.startDate.format(dateTimeFormat);
        var endDate = picker.startDate.format(dateTimeFormat);

        $('#budget-group-report-date-range').html(startDate + ' - ' + endDate);
        CallBackDataTableBudgetGroupReport();

    });
    $('#budget-group-report-date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#budget-group-report-date-range').html('full history');
        CallBackDataTableBudgetGroupReport();
    });


    LoadDataBudgetGroupReport();
});



function CallBackDataTableBudgetGroupReport() {

    var dimensions = $("#dimensions-filter").val();
    if (dimensions === null || typeof dimensions === "undefined")
        dimensions = "";
    filter.WorkgroupId = $("#work-group-filter").val();
    filter.Dimensions = dimensions.toString();
    filter.DateRange = $("#budget-group-report-date-range").val();
    
    $("#table-budget-group-report").DataTable().ajax.reload();
    $("#total-amount-group-report").text("0");
};

function LoadDataBudgetGroupReport() {
   

    $("#table-budget-group-report").on('processing.dt', function (e, settings, processing) {
        
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
            "url": '/TraderBudget/GetDataTableBudgetGroupReport',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "budgetScenarioId": filter.BudgetScenarioId,
                    "budgetGroupId": filter.BudgetGroupId,
                    "dimensions": filter.Dimensions,
                    "workGroupId": filter.WorkgroupId,
                    "dateRange": filter.DateRange
                });
            }
        },
        "columns": [
            {
                name: "FullRef",
                data: "FullRef",
                orderable: true,
                render: function (value, type, row) {
                    
                    $("#total-amount-group-report").text(row.TotalAmount);
                    return row.FullRef;
                }
            },
            {
                name: "Item",
                data: "Item",
                orderable: true
            },
            {
                name: "Unit",
                data: "Unit",
                orderable: true
            },
            {
                name: "Dimension",
                data: "Dimension",
                orderable: true
            },
            {
                name: "Quantity",
                data: "Quantity",
                orderable: true
            },
            {
                name: "Date",
                data: "Date",
                orderable: true
            },
            {
                name: "Total",
                data: "Total",
                orderable: true
            }
        ],
        "order": [[1, "desc"]]
    });
    CallBackDataTableBudgetGroupReport();

}