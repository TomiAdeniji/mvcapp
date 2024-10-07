
var example = null;
var array = [];
var isDataUpdating = false;
var isBusy = false;

function InitChangeEvent() {
    $("#location-selector, #channel-selector, #completed-shown-input").on('change', function () {
        $("#order-mngt-table").DataTable().ajax.reload();
    });
    $('#daterange-input').daterangepicker({
        timePicker: true,
        //autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        startDate: moment().startOf('day'),
        endDate: moment().endOf('day'),
        opens: "right",
        locale: {
            cancelLabel: 'cancel',
            format: $dateTimeFormatByUser
        }
    });
    $("#daterange-input").on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $("#order-mngt-table").DataTable().ajax.reload();
    });
    $("#daterange-input").on('apply.daterangepicker', function (e, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('.reporting_period').text(picker.startDate.format($dateTimeFormatByUser) + " - " + picker.endDate.format($dateTimeFormatByUser));
        $("#order-mngt-table").DataTable().ajax.reload();
    })

    $('#orderref-input').keyup(delay(function () {
        $("#order-mngt-table").DataTable().ajax.reload();
    }, 500));

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

function uncheck() {
    $('input[name="check"]').each(function () {
        $(this).prop("checked", false);
    });
    $('.alert_matches.projects').removeClass('active');
    $('#checked').html('0');
}

function multiselected() {
    var checkedcount = $('input[name="check"]:checked').length;
    $('#checked').html(checkedcount);
    if (checkedcount >= 1) {
        $('.alert_matches.projects').addClass('active');
    } else {
        $('.alert_matches.projects').removeClass('active');
    }
}

function bulkChecking() {
    var ischecked = $("input[name='bulk-check']").is(":checked");
    if (ischecked) {
        $('input[name="check"]').each(function () {
            $(this).prop("checked", true);
        });
        multiselected();
    } else {
        uncheck();
    }
}

$(function () {
    initTimer();
    // Init onchange event for ordermanagent datatable condition filters
    InitChangeEvent();

    loadOrderManagementDT();
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        selectAllJustVisible: true,
        includeResetOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true,
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true
    });
})

function showPosOrder(id) {
    LoadingOverlay();
    $('#pos-order-summary').empty();
    $('#pos-order-summary').load('/TraderReports/ShowQueueOrderSummary?queueOrderId=' + id, function () {
        LoadingOverlayEnd();
    });
}

function loadOrderManagementDT() {
    if (isBusy)
        return;

    var dataTable = $("#order-mngt-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                if (isDataUpdating) {
                    $("#refreshing").show();
                } else {
                    $("#order-mngt-table").LoadingOverlay("show");
                }
            } else {
                if (isDataUpdating) {
                    $("#refreshing").hide();
                    $("#refreshed").show();
                    setTimeout(function () {
                        $('#refreshed').fadeOut();
                    }, 2000);
                }
                initTimer();
                isDataUpdating = false;
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "order": [[0, "desc"]],
            'beforeSend': function (xhr) {
                isBusy = true;
            },
            "drawCallback": function () {
                $("input[name='bulk-check']").prop("checked", false);
                uncheck();
                $('input[name="check"]').bind('click', function () {
                    multiselected();
                });

                // Show/Unshow dataTable columns
                $("#filterColumn ul li").on('change', function (e) {
                    var dataTable = $("#order-mngt-table");
                    var colId = $(this).find('input[type=checkbox]').val();
                    var col = dataTable.DataTable().column(colId);
                    col.visible(!col.visible());
                })

                isBusy = false;
                $("#order-mngt-table").LoadingOverlay("hide", true);
            },
            "ajax": {
                "url": '/OrderManagement/GetOrderTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationId": $("#location-selector").val(),
                        "saleChannelStr": JSON.stringify($("#channel-selector").val()),
                        "daterange": $("#daterange-input").val(),
                        "keyword": $("#orderref-input").val(),
                        "isCompletedShownOnly": $("#completed-shown-input").is(":checked")
                    });
                }
            },
            "columns": [
                {
                    data: "Id",
                    orderable: false,
                    render: function (value, type, row) {
                        var htmlString = "";
                        htmlString += "<input type='checkbox' id='" + row.Id + "' onclick='$(\"#bulk\").removeAttr(\"disabled\");' name='check'>";
                        return htmlString;
                    }
                },
                {
                    data: "OrderRef",
                    orderable: true,
                },
                {
                    data: "LocationName",
                    orderable: true
                },
                {
                    data: "SaleChannel",
                    orderable: true
                },
                {
                    data: "ItemCount",
                    orderable: false,
                    render: function (value, type, row) {
                        var htmlString = "";
                        htmlString += '<button class="btn btn-info" onclick="showPosOrder(\'' + row.Id + '\')" data-toggle="modal" data-target="#pos-order-summary"><i class="fa fa-list"></i> &nbsp; ' + row.ItemCount + '</button>';
                        return htmlString
                    }
                },
                {
                    data: "TotalStr",
                    orderable: false
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (value, type, row) {
                        var htmlString = "";
                        if (row.Status != "") {
                            htmlString += '<span class="label label-lg label-primary">' + row.Status + '</span>'
                        }
                        return htmlString;
                    }
                },
                {
                    data: "QueuedInfo",
                    orderable: false,
                },
                {
                    data: "Pending",
                    orderable: false,
                },
                {
                    data: "Preparing",
                    orderable: false,
                },
                {
                    data: "Completion",
                    orderable: false,
                },
                {
                    data: "DeliveryStatus",
                    orderable: false,
                },
                {
                    data: "PaidStatus",
                    orderable: false,
                    render: function (value, type, row) {
                        var htmlString = "";
                        htmlString += row.PaidStatusHtml;
                        return htmlString;
                    }
                },
                {
                    orderable: false,
                    render: function (value, type, row) {
                        var htmlString = "";
                        htmlString += '<div class="btn-group dropdown">';
                        htmlString += '    <button class="btn btn-primary dropdown-toggle" data-toggle="dropdown">';
                        htmlString += '        Options &nbsp; <i class="fa fa-angle-down"></i>';
                        htmlString += '    </button>';
                        htmlString += '    <ul class="dropdown-menu primary dropdown-menu-right">';
                        // htmlString += '        <li><a href="#" data-toggle="modal" data-target="#order-status-info">Order info</a></li>';
                        htmlString += '        <li><a href="#" onclick="openUdpateStatusModal(\'' + row.Id + '\')">Change status</a></li>';
                        htmlString += '        <li><a href="javascript:void(0)" data-toggle="modal" data-target="#create-discussion-pos" onclick="AddEditDiscussion(\'' + row.Id + '\')">Discuss</a></li>';
                        // htmlString += '        <li><a href="#" data-toggle="modal" data-target="#order-send-to-delivery">Send to delivery</a></li>';
                        // htmlString += '        <li><a href="#" data-toggle="modal" data-target="#">Send to prep</a></li>';
                        // htmlString += '        <li><a href="#" data-toggle="modal" data-target="#">Return/cancel</a></li>';
                        htmlString += '    </ul>';
                        htmlString += '</div>';
                        return htmlString;
                    }
                },
            ]
        });
}

function updatPrepQueueOrderStatus(orderId) {
    var orderIds = [];
    if (orderId == null) {
        $('input[name="check"]:checked').each(function () {
            orderIds.push($(this).attr('id'));
        });
    } else {
        orderIds.push(orderId);
    }

    var _url = "/OrderManagement/UpdatePrepQueueOrderStatus";
    var $status = $('select[name="order-status"]').val();
    $.ajax({
        type: 'POST',
        datatype: 'JSON',
        url: _url,
        data: {
            "lstOrderIdsStr": JSON.stringify(orderIds),
            "upcomingStatus": Number($status),
            "problemDescription": $("#problem-description").val()
        },
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#order-status-change-batch").modal("hide");
                $("#order-mngt-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
}

function openUdpateStatusModal(orderId) {
    if (orderId == null) {
        var orderSelectedNumber = $('input[name="check"]:checked').length;
        if (orderSelectedNumber == 0) {
            return;
        } else {
            $("#check-count").text(orderSelectedNumber);
            $("#order-status-change-batch .help-text").show();
        }
        $("#order-status-change-batch button[name='change-status-confirm-btn']").attr("onclick", "updatPrepQueueOrderStatus(null)");
    } else if (orderId != null) {
        $("#order-status-change-batch .help-text").hide();
        $("#order-status-change-batch button[name='change-status-confirm-btn']").attr("onclick", "updatPrepQueueOrderStatus(" + orderId + ")");
    }

    $("#problem-description").val("");

    $("#order-status-change-batch").modal("show");
}

function toggleShowingProblemDescription() {
    var chosenStatus = $('select[name="order-status"]').val();
    if (chosenStatus == 5) {
        //Completed with Problems
        $("#problem-description-container").show();
    } else {
        $("#problem-description-container").hide();
    }
}