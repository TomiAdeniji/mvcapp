$(document).ready(function () {
    $('.icdaterange').daterangepicker({
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'cancel',
            format: $dateFormatByUser.toUpperCase()
        }
    });
    $("#frm-filter input[name=date]").change(function () {
        var drp = $('#frm-filter input[name=date]').data('daterangepicker');
        $('.reporting_period').text(drp.startDate.format("DD MMMM YYYY") + " - " + drp.endDate.format("DD MMMM YYYY"));
        reportLoad();
    });
    $("#frm-filter select[name=view]").change(function () {
        reportLoad();
    });
    $("#frm-filter select[name=dimensions]").change(function () {
        reportLoad();
    });

    $('.icdaterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $('.reporting_period').text(picker.startDate.format("DD MMMM YYYY") + " - " + picker.endDate.format("DD MMMM YYYY"));
        //reportLoad();
    });

    $('.icdaterange').on('cancel.daterangepicker', function (ev, picker) {
        //$(this).val(null);
    });
});
function reportLoad() {
    var date = $('#frm-filter input[name=date]').val();
    if (date) {
        var _dimensions=$('#frm-filter select[name=dimensions]').val();
        var $tabprint = $('#tab-print');
        $tabprint.LoadingOverlay("show");
        var data = {
            date: date,
            view: $('#frm-filter select[name=view]').val(),
            dimensions: _dimensions?_dimensions:[],
            company_name: $('#frm-filter input[name=company_name]').val(),
            report_title: $('#frm-filter input[name=report_title]').val(),
            showlogo: $('#frm-filter input[name=isShowLogo]').prop("checked"),
            treeConfig: $('#treevalue').val()
        };
        $.ajax({
            type: "POST",
            url: "/Bookkeeping/FilterReport",
            dataType: "html",
            data: data,
            success: function (html) {
                if (html) {
                    $tabprint.html(html);
                }
                $tabprint.LoadingOverlay("hide", true);
            }
        });
    }
}
function exportPDF() {
    $.LoadingOverlay("show");
    var byteImgCapture = "";
    window.html2canvas(document.getElementById('income-report-capture'), {
        allowTaint: true,
        svgRendering: true,
        onrendered: function (canvas) {
            byteImgCapture = canvas.toDataURL("image/png").replace("image/png", "image/octet-stream");
            var fileName = 'income-report.pdf';
            $.ajax({
                type: 'post',
                url: '/Bookkeeping/DownloadReport',
                datatype: 'json',
                data: { data: byteImgCapture },
                success: function (data) {
                    var link = document.createElement("a");
                    link.download = fileName;
                    link.href = 'data:application/pdf;base64,' + data;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    delete link;
                }, error: function (err) {
                    cleanBookNotification.error(err.responseText, "Qbicles");
                }
            });
        }
    });
    LoadingOverlayEnd();
}
function printPDF() {
    $.LoadingOverlay("show");
    var elm = document.getElementById('income-report-capture');
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