$(function () {
    LoadDataByServerSide();
});
var example = null;
var array = [];
function initTimer() {
    array.push('runTimer');
    if (array.length > 1) return;
    example = new (function () {

        // Stopwatch element on the page
        var $stopwatch;

        // Timer speed in milliseconds
        var incrementTime = 70;
        // Current timer position in milliseconds
        var currentTime = [];

        // Start the timer
        $(function () {
            $stopwatch = $('.timer-order');
            example.Timer = $.timer(updateTimer, incrementTime, true);
            array = [];
        });

        // Output time and increment
        function updateTimer() {
            for (var i = 0; i < $stopwatch.length; i++) {
                if (!currentTime[i]) {
                    currentTime[i] = getMilliseconds($($stopwatch[i]).text());
                }
                var timeString = formatTime(currentTime[i]);
                $($stopwatch[i]).html(timeString);
                currentTime[i] += incrementTime + incrementTime / 20;
                }
            }

        // Reset timer
        this.resetStopwatch = function () {
            if ($stopwatch) {
                for (var i = 0; i < $stopwatch.length; i++) {
                    currentTime[i] = 0;
                }
            }
            example.Timer.stop().once();
        };
    });
}
function AddEditDiscussion(id) {
    LoadingOverlay();
    $('#create-discussion-pos').empty();
    $('#create-discussion-pos').load('/TraderReports/AddDiscussion?id=' + id, function () {
        LoadingOverlayEnd();
    });
}
function saveDiscussion() {
    var discussion = {
        Id: $('#queue-discussionId').val(),
        Topic: { Id: $('#discussion-topic').val() },
        Summary: $('#discussion-summary').val(),
        Name: $('#discussion-name').val()
    }
    // contact
    var lstUserIds = $('#discussion-contact').val();
    var lstUsers = [];
    for (var i = 0; i < lstUserIds.length; i++) {
        lstUsers.push({ Id: lstUserIds[i] });
    }
    discussion.ActivityMembers = lstUsers;
    // expiryDate
    if ($('#expiryDate')[0].checked) {
        discussion.ExpiryDate = $('#discussion-expirydate').val();
    }
    var queueOrder = {
        Id: $('#queueId').val(),
        Discussion: discussion
    }
    LoadingOverlay();
    $.ajax({
        type: 'post',
        url: '/TraderReports/SaveDiscussion',
        datatype: 'json',
        data: {
            queueOrder: queueOrder
        },
        success: function (refModel) {
            if (refModel.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#create-discussion-pos').modal('toggle');
            } else if (refModel.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#create-discussion-pos').modal('toggle');
            } else {
                cleanBookNotification.error(refModel.msg, "Qbicles");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function printPDF() {
    $.LoadingOverlay("show");
    var elm = document.getElementById('order-table');
    html2canvas(elm, {
        allowTaint: true,
        background: 'transparent',
        onrendered: function (canvas) {
            var myImage = canvas.toDataURL("image/jpeg");
            var nWindow = window.open();
            $(nWindow.document.body)
                .html("<style type=\"text/css\" media=\"print\">@page{size: auto;margin: 0mm;}</style ><img src=" + myImage + " style='width:100%;'></img>")
                .ready(function () {
                    nWindow.focus();
                    setTimeout(function () {
                        nWindow.print();
                    },
                        500);
                });
        }
    });

    LoadingOverlayEnd();
}
function exportPDF() {
    $.LoadingOverlay("show");
    var byteImgCapture = "";
    window.html2canvas(document.getElementById('order-table'), {
        allowTaint: true,
        svgRendering: true,
        onrendered: function (canvas) {
            byteImgCapture = canvas.toDataURL("image/png").replace("image/png", "image/octet-stream");
            var fileName = 'order-table.pdf';
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
                    cleanBookNotification.error(err.responseText, "Qbicles");
                }
            });
        }
    });
    LoadingOverlayEnd();
}
function showPosOrder(id) {
    LoadingOverlay();
    $('#pos-order-summary').empty();
    $('#pos-order-summary').load('/TraderReports/ShowOrderSummary?queueOrderId=' + id, function () {
        LoadingOverlayEnd();
    });
}

function changePageRefreshTime() {
    var pageRefreshTime = Number($("#pageRefreshTime").val());

    if (pageRefreshTime < 5) {
        pageRefreshTime = 5;
        $("#pageRefreshTime").val('5')
    }

    LoadingOverlay();
    var _url = "/TraderReports/UpdatePageRefreshTime";
    $.ajax({
        type: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            refreshTime: pageRefreshTime
        },
        success: function (data) {
            location.reload();
        }
    });
    LoadingOverlayEnd();
}

function pad(number, length) {
    var str = '' + number;
    while (str.length < length) { str = '0' + str; }
    return str;
}

 /// Get Elapsed Time with Format: Month -> Week -> Day -> Hour -> Min -> Second
 /// In average: 1 month have 30.4368 days
var monthToMilisecond = 1000 * 60 * 60 * 24 * 30.4368;
var weekToMilisecond = 1000 * 60 * 60 * 24 * 7;
var dayToMilisecond = 1000 * 60 * 60 * 24;
var hourToMilisecond = 1000 * 60 * 60;
var minuteToMilisecond = 1000 * 60;
var secondToMilisecond = 1000;

function formatTime(time) {
    var month = Math.floor(time / monthToMilisecond);
    var week = Math.floor(time / weekToMilisecond - month * 30.4368);
    var day = Math.floor(time / dayToMilisecond - month * 30.4368 - week * 7);
    var hour = Math.floor((time / hourToMilisecond) - (month * 30.4368 + week * 7 + day) * 24);
    var min = Math.floor((time / minuteToMilisecond) - ((month * 30.4368 + week * 7 + day) * 24 * 60 + hour * 60));
    var sec = Math.floor((time / secondToMilisecond) - ((month * 30.4368 + week * 7 + day) * 24 * 60 * 60 + hour * 60 * 60 + min * 60));

    var monthStr = "", weekStr = "", dayStr = "", hourStr = "", minStr = "", secStr = "";
    if (month > 0)
        monthStr = month + "m ";
    if (week > 0)
        weekStr = week + "w ";
    if (day > 0)
        dayStr = day + "d ";
    if (hour > 0)
        hourStr = pad(hour, 2) + "h ";

    return monthStr + weekStr + dayStr + hourStr + (min > 0 ? pad(min, 2) : "00") + "m " + pad(sec, 2) + "s";
}
function getMilliseconds(time) {
    if (!time || time.split(':').length < 6) {
        return 0;
    }
    var timelst = time.split(':');

    ////min, second
    //if (timelst.length == 2) {
    //    return (parseInt(timelst[0]) * minuteToMilisecond) + (parseInt(timelst[1]) * secondToMilisecond);
    //}
    ////hour, min, second
    //if (timelst.length == 3) {
    //    return (parseInt(timelst[0]) * hourToMilisecond) + (parseInt(timelst[1]) * minuteToMilisecond) + (parseInt(timelst[2]) * secondToMilisecond);
    //}
    ////day, hour, min, second
    //if (timelst.length == 4) {
    //    return (parseInt(timelst[0]) * dayToMilisecond) + (parseInt(timelst[1]) * hourToMilisecond) + (parseInt(timelst[2]) * minuteToMilisecond) + (parseInt(timelst[3]) * secondToMilisecond);
    //}
    ////week, day, hour, min, second
    //if (timelst.length == 5) {
    //    return (parseInt(timelst[0]) * weekToMilisecond) + (parseInt(timelst[1]) * dayToMilisecond) + (parseInt(timelst[2]) * hourToMilisecond) + (parseInt(timelst[3]) * minuteToMilisecond)
    //        + (parseInt(timelst[4]) * secondToMilisecond);
    //}
    //month, week, day, hour, min, second
    return (parseInt(timelst[0]) * monthToMilisecond) + (parseInt(timelst[1]) * weekToMilisecond) + (parseInt(timelst[2]) * dayToMilisecond) + (parseInt(timelst[3]) * hourToMilisecond)
        + (parseInt(timelst[4]) * minuteToMilisecond) + (parseInt(timelst[5]) * secondToMilisecond);
}

// load table
function LoadTableDataOrderHistory(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
            initTimer();
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
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "isCompleteShown": $("#isCompletedShown").is(":checked")
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function LoadDataByServerSide() {
    var url = '/TraderReports/TraderOrderHistoryDataTable';

    var columns = [
        {
            name: "OrderRef",
            data: "OrderRef",
            orderable: true,
        },
        {
            name: "Status",
            orderable: false,
            render: function (value, type, row) {
                if (row.Status != "") {
                    var str = '<span class="label label-lg label-primary">' + row.Status + '</span>';
                }
                return str;
            }
        },
        {
            name: "OrderItems",
            data: "OrderItems",
            orderable: true,
            width: "50px",
            render: function (value, type, row) {
                var str = '<button class="btn btn-info" onclick="showPosOrder(\'' + row.Id + '\')" data-toggle="modal" data-target="#pos-order-summary"><i class="fa fa-list"></i> &nbsp; ' + row.OrderItems + '</button>';
                return str;
            }
        },
        {
            name: "OrderTotal",
            data: "OrderTotal",
            width: "50px",
            orderable: true
        },
        {
            name: "Queued",
            data: "Queued",
            orderable: false
        },
        {
            name: "Pending",
            data: "Pending",
            orderable: false,
            render: function (value, type, row) {
                var str = row.Pending;
                return str;
            }
        },
        {
            name: "Preparing",
            data: "Preparing",
            orderable: false,
            render: function (value, type, row) {
                var str = row.Preparing;
                return str;
            }
        },
        {
            name: "Completion",
            data: "Completion",
            orderable: false,
            width: "50px",
            render: function (value, type, row) {
                var str = row.Completion;
                return str;
            }
        },
        {
            name: "DeliveryStatus",
            data: "DeliveryStatus",
            orderable: false,
            width: "50px",
            render: function (value, type, row) {
                var str = row.DeliveryStatus;
                return str;
            }
        },
        {
            name: "Payment",
            data: "Payment",
            orderable: false,
            width: "50px",
            render: function (value, type, row) {
                var str = row.Payment;
                return str;
            }
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                if (!row.Discussion) {
                    str += '<button class="btn btn-primary" onclick="AddEditDiscussion(\'' + row.Id + '\')" data-toggle="modal" data-target="#create-discussion-pos"><i class="fa fa-comments"></i> &nbsp; Discuss</button>';
                } else {
                    str += '<button onclick="AddEditDiscussion(\'' + row.Id + '\')" class="btn btn-primary" data-toggle="modal" data-target="#create-discussion-pos"><i class="fa fa-comments"></i> &nbsp; Discuss</button>';
                }
                return str;
            }
        }
    ];
    LoadTableDataOrderHistory('pos-order-table', url, columns, 4);
    CallBackFilterDataOrderHistoryServeSide();
}

function CallBackFilterDataOrderHistoryServeSide() {
    $("#pos-order-table").DataTable().ajax.reload();
}


