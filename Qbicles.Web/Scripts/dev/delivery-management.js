function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}


function uncheck() {
    $('input[name="row-check"]').each(function () {
        $(this).prop("checked", false);
    });
    $('input[name="bulk-check"]').each(function () {
        $(this).prop("checked", false);
    });
    $('.alert_matches.projects').removeClass('active');
    $('#checked').html('0');
}

function multiselected() {
    var checkedcount = $('input[name="row-check"]:checked').length;
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
        $('input[name="row-check"]').each(function () {
            $(this).prop("checked", true);
        });
        multiselected();
    } else {
        uncheck();
    }
}

var isDataUpdating = false;
var refreshIntervalId = 0;



function rememberedFilter(reload = true) {
    $.ajax({
        type: "post",
        dataType: "json",
        url: "/Delivery/rememberedFilter",
        data: { parameter: $deliveryFilter },
        success: function () {
            if (reload)
                $("#delivery-mngt-table").DataTable().ajax.reload();
        }
    })
}



function CreateDelivery() {
    $.ajax({
        type: 'post',
        url: '/Delivery/CreateDelivery?locationId=' + $("#add-new-location").val(),
        //data: { location: markupDiscount },
        dataType: 'json',
        success: function (response) {
            isDataUpdating = true;
            $("#delivery-mngt-table").DataTable().ajax.reload();
        },
        error: function (er) {

        }
    }).always(function () {
        //$('#tb_form_item').LoadingOverlay("hide", true);
    });
}













function loadDeliveryManagement() {
    //if (isBusy)
    //    return;

    var dataTable = $("#delivery-mngt-table")
        .on('processing.dt', function (e, settings, processing) {
            //$('#processingIndicator').css('display', 'none');
            if (processing) {
                if (isDataUpdating) {
                    $("#refreshing").show();
                } else {
                    $("#delivery-mngt-table").LoadingOverlay("show");
                }
            } else {
                if (isDataUpdating) {
                    $("#refreshing").hide();
                    $("#refreshed").show();
                    setTimeout(function () {
                        $('#refreshed').fadeOut();
                    }, 2000);
                }
                //initTimer();
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
            "order": [[2, "desc"]],
            'beforeSend': function (xhr) {
                isBusy = true;
            },
            "drawCallback": function () {
                ShowHideColumn($deliveryFilter.Columns);
                $("input[name='bulk-check']").prop("checked", false);
                uncheck();
                $('input[name="row-check"]').bind('click', function () {
                    multiselected();
                });

                $("#delivery-mngt-table").LoadingOverlay("hide", true);
            },
            "ajax": {
                "url": '/Delivery/GetDeliveries',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        //"locationId": $("#location-selector").val(),
                        //"saleChannelStr": JSON.stringify($("#channel-selector").val()),
                        //"daterange": $("#daterange-input").val(),
                        //"keyword": $("#orderref-input").val(),
                        //"isCompletedShownOnly": $("#completed-shown-input").is(":checked")
                    });
                }
            },
            "columns": [
                {
                    data: "Id",
                    orderable: false,
                    render: function (value, type, row) {
                        var htmlString = "";
                        htmlString += "<input type='checkbox' id='" + row.Id + "' onclick='$(\"#bulk\").removeAttr(\"disabled\");' name='row-check'>";
                        return htmlString;
                    }
                },
                {
                    data: "Reference",
                    orderable: true,
                },
                {
                    data: "Date",
                    orderable: true
                },
                {
                    data: "Location",
                    orderable: true
                },
                {
                    data: "Driver",
                    orderable: true
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (value, type, row) {
                        var style = "success";
                        if (row.Status == 1)
                            style = 'primary'
                        else if (row.Status == 2)
                            style = 'info'
                        else if (row.Status == 3)
                            style = 'success'
                        else if (row.Status == 4)
                            style = 'success'
                        else if (row.Status == 5)
                            style = 'warning'
                        else if (row.Status == 6)
                            style = 'danger'

                        return '<span class="label label-lg label-' + style + '">' + row.StatusText + '</span>'
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
                        
                        if (row.Status < 4)
                            htmlString += '<li><a target="_blank" href="/Delivery/Management?deliveryKey=' + row.Key + '">Manage</a></li>';
                        else
                            htmlString += '<li><a target="_blank" href="/Delivery/Management?deliveryKey=' + row.Key + '">View delivery</a></li>';
                            //htmlString += '<a href="/Delivery/Management?deliveryKey=' + row.Key + '">Manage</a>';

                        htmlString += '<li><a href="javascript:void(0)" data-toggle="modal" data-target="#dds-orders-view" onclick="viewDeliveryOrder(\'' + row.Id + '\')">View orders</a></li>';
                        //htmlString += '<li><a href="javascript:void(0)" onclick="openUdpateStatusModal(\'' + row.Id + '\')">Change status</a></li>';

                        if (row.DiscussionKey == "")
                            htmlString += '<li><a href="javascript:void(0)" data-toggle="modal" data-target="#create-discussion-delivery" onclick="AddEditDeliveryDiscussion(' + row.Id + ')">Add a discussion</a></li>';
                        else
                            htmlString += '<li><a target="_blank" href="/Qbicles/DiscussionQbicle?disKey=' + row.DiscussionKey + '">Discuss</a></li>';


                        htmlString += '    </ul>';
                        htmlString += '</div>';
                        return htmlString;
                    }
                },
            ]
        });
}

function openUdpateStatusModal(deliveryId) {
    if (deliveryId == null) {
        var deliverySelected = $('input[name="row-check"]:checked').length;
        if (deliverySelected == 0) {
            return;
        } else {
            $("#check-count").text(deliverySelected);
            $("#delivery-status-change-batch .help-text").show();
        }
        $("#change-delivery-status-confirm").attr("onclick", "updatDeliveryStatus(null)");
    } else if (deliveryId != null) {
        $("#delivery-status-change-batch .help-text").hide();
        $("#change-delivery-status-confirm").attr("onclick", "updatDeliveryStatus(" + deliveryId + ")");
    }

    $("#delivery-status-change").val(null).trigger('change');
    $("#problem-description").val("");

    $("#delivery-status-change-batch").modal("show");
}

function updatDeliveryStatus(deliveryId) {
    alert('This has not yet been implemented')
    cleanBookNotification.updateSuccess();
    $("#delivery-status-change-batch").modal("hide");
    return;



    var deliveryIds = [];
    if (deliveryId == null) {
        $('input[name="row-check"]:checked').each(function () {
            deliveryIds.push($(this).attr('id'));
        });
    } else {
        deliveryIds.push(deliveryId);
    }

    var _url = "/Delivery/UpdateDeliveryStatus";
    var $status = $('#delivery-status-change').val();
    debugger;
    $.ajax({
        type: 'POST',
        datatype: 'JSON',
        url: _url,
        data: {
            "deliveryIds": JSON.stringify(deliveryIds),
            "upcomingStatus": Number($status),
            "problemDescription": $("#problem-description").val()
        },
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#delivery-status-change-batch").modal("hide");
                $("#delivery-mngt-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
}

function toggleShowingProblemDescription() {
    if ($('#delivery-status-change').val() == 5) {
        //Completed with Problems
        $("#problem-description-container").show();
    } else {
        $("#problem-description-container").hide();
    }
}

function AddEditDeliveryDiscussion(deliveryId) {
    LoadingOverlay();
    $('#create-discussion-delivery').empty();
    $('#create-discussion-delivery').load('/Delivery/AddDeliveryDiscussion?deliveryId=' + deliveryId, function () {
        LoadingOverlayEnd();
    });
}

function viewDeliveryOrder(deliveryId) {
    LoadingOverlay();
    $('#dds-orders-view').empty();
    $('#dds-orders-view').load('/Delivery/ShowDeliveryOrder?deliveryId=' + deliveryId, function () {
        LoadingOverlayEnd();
        //$("#dds-orders-view").show();
    });
}

function SaveDeliveryDiscussion() {
    var discussion = {
        Id: $('#delivery-discussionId').val(),
        Topic: { Id: $('#delivery-discussion-topic').val() },
        Summary: $('#delivery-discussion-summary').val(),
        Name: $('#delivery-discussion-name').val()
    }
    // contact
    var lstUserIds = $('#delivery-discussion-contact').val();
    var lstUsers = [];
    for (var i = 0; i < lstUserIds.length; i++) {
        lstUsers.push({ Id: lstUserIds[i] });
    }
    discussion.ActivityMembers = lstUsers;
    // expiryDate
    if ($('#expiryDate')[0].checked) {
        discussion.ExpiryDate = $('#delivery-discussion-expirydate').val();
    }
    var delivery = {
        Id: $('#delivery-id').val(),
        Discussion: discussion
    }
    LoadingOverlay();
    $.ajax({
        type: 'post',
        url: '/Delivery/SaveDeliveryDiscussion',
        datatype: 'json',
        data: {
            delivery: delivery
        },
        success: function (refModel) {
            if (refModel.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#create-discussion-delivery').modal('toggle');
            } else if (refModel.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#create-discussion-delivery').modal('toggle');
            } else {
                cleanBookNotification.error(refModel.msg, "Qbicles");
            }
            $("#delivery-mngt-table").DataTable().ajax.reload();
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}