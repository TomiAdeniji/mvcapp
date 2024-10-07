
$(function () {
    loadDefaultPage();
});

function loadDefaultPage() {
    $('#report-tab-content').LoadingOverlay('show');
    $('#report-tab-content').empty();
    $('#report-tab-content').load('/TraderReports/LoadDefaultReportTab', function () {
        $('#report-tab-content').LoadingOverlay('hide');
    });
}
function initDateRange() {
    $('.datetimerange').daterangepicker({
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: $dateFormatByUser.toUpperCase()
        },
        timePicker: false,
        maxDate: moment()
    });
    $('.datetimerange').val(getDateToDate());
    $('.datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $('.datetimerange').html(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        // action here
    });
    $('.datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('.datetimerange').html('full history');
        // action here
    });
}
function getDateToDate(nDay) {
    if (!nDay) {
        nDay = -7;
    }
    var date = new Date();
    var dateStr = "";
    var datePart = date.getDate();
    var monthPart = date.getMonth() + 1;
    if (Number(datePart) < 10) {
        datePart = "0" + datePart;
    }
    if (Number(monthPart) < 10) {
        monthPart = "0" + monthPart;
    }

    dateStr += datePart + "/" + monthPart + "/" + date.getFullYear();

    date.setUTCDate(date.getDate() + nDay);

    datePart = date.getDate();
    monthPart = date.getMonth() + 1;
    if (Number(datePart) < 10) {
        datePart = "0" + datePart;
    }
    if (Number(monthPart) < 10) {
        monthPart = "0" + monthPart;
    }
    var dateFrom = datePart + "/" + monthPart + "/" + date.getFullYear();
    dateStr = dateFrom + ' - ' + dateStr;
    return dateStr;
}

function genarateReportTimeFrame() {
    var dateRange = $('#reportTime').val();
    $.ajax({
        type: "POST",
        url: "/TraderReports/GenarateReportTimeFrame",
        data: { daterange: dateRange },
        success: function (res) {

            if (res) {
                location.href = "/TraderReports/ReportPosOrders";
            }

        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });
}

