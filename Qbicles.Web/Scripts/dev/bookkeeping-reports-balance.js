$(document).ready(function () {
    var $start_date = $("#report-start_date");
    $start_date.change(function () {
        var drp = $start_date.data('daterangepicker');
        $('.reporting_period').text('As on '+drp.startDate.format("MMMM Do, YYYY"));
        reportLoad();
    });
    $("#report-dimensions").change(function () {
        reportLoad();
    });
    var f_date = $start_date.attr("fm");
    $('.reporting_period').text('As on '+moment($start_date.val(), f_date.toUpperCase()).format("MMMM Do, YYYY"))
});
function reportLoad() {
    var date = $('#report-start_date').val();
    if (date) {
        var $reportcontent = $('#report-content');
        $reportcontent.LoadingOverlay("show");
        var dimensions = $('#report-dimensions').val();
        var data = {
            start_date: date,
            incomeReportEntry: JSON.parse($('#report-treevalue').val()),
            allNodeIds: JSON.parse($('#report-lstANodes').val())
        };
        $.ajax({
            type: "POST",
            url: "/Bookkeeping/FilterReportBalance",
            dataType: "html",
            data: data,
            success: function (html) {
                if (html) {
                    $reportcontent.html(html);
                }
                $reportcontent.LoadingOverlay("hide", true);
            }
        });
    }
}
function exportPDF() {
    $.LoadingOverlay("show");
    var byteImgCapture = "";
    window.html2canvas(document.getElementById('balance-report-capture'), {
        allowTaint: true,
        svgRendering: true,
        onrendered: function (canvas) {
            byteImgCapture = canvas.toDataURL("image/png").replace("image/png", "image/octet-stream");
            var fileName = 'balance-report.pdf';
            $.ajax({
                type: 'post',
                url: '/Bookkeeping/DownloadReport',
                datatype: 'json',
                data: { data: byteImgCapture },
                success: function (data) {
                    LoadingOverlayEnd();
                    var link = document.createElement("a");
                    link.download = fileName;
                    link.href = 'data:application/pdf;base64,' + data;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    delete link;
                }, error: function (err) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(err.responseText, "Qbicles");
                }
            });
        }
    });
    LoadingOverlayEnd();
}
function printPDF() {
    $.LoadingOverlay("show");
    var elm = document.getElementById('balance-report-capture');
    html2canvas(elm, {
        allowTaint: true,
        background: 'transparent',
        onrendered: function (canvas) {
            var myImage = canvas.toDataURL("image/jpeg");
            var nWindow = window.open('');
            $(nWindow.document.body)
                .html("<style type=\"text/css\" media=\"print\">@page{size: auto;margin: 0mm;}</style ><img src=" + myImage + " style='width:100%;'></img>")
                .ready(function () {
                    nWindow.focus();
                    setTimeout(function () {
                        nWindow.print();
                    }, 500)
                    
                });
        }
    });
    
    LoadingOverlayEnd();
}