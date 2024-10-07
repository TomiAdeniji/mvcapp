initAdminPendingExtensionRequestTable = function () {
    var dataTable = $("#pending-extensionrequest-list")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#pending-extensionrequest-list').LoadingOverlay("show");
            } else {
                $('#pending-extensionrequest-list').LoadingOverlay("hide", true);
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
            "drawCallback": function (settings) {
                $('.withselected').fadeOut();
            },
            "ajax": {
                "url": '/DomainExtension/LoadExtensionRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#pending-key").val(),
                        "createdUserIdSearch": $("#pending-creator").val(),
                        "dateRange": $("#pending-daterange").val(),
                        "extensionTypeSearch": $("#pending-type").val(),
                        "lstRequestStatusSearch": [2],
                        "domainKey": ""
                    });
                }
            },
            "columns": [
                {
                    data: "requestId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var requestId = row.RequestId;
                        var htmlStr = "";
                        htmlStr += '<input type="checkbox" requestId="' + requestId + '" style="position: relative; top: 0;" onclick="if($(\'#pending-extensionrequest-list input:checked\').length > 0){$(\'.withselected\').fadeIn();}else{$(\'.withselected\').fadeOut();};">';
                        return htmlStr;
                    }
                },
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "RequestedByName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var creatorName = row.RequestedByName;
                        var creatorLogoUri = row.RequestedByLogoUri;
                        var creatorId = row.RequestById;
                        var htmlStr = "";
                        htmlStr += '<a href="/Community/UserProfilePage?uId=' + creatorId + '">';
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + creatorLogoUri + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + creatorName + '</div>'
                        htmlStr += '<div class="clearfix"></div></a>';
                        return htmlStr;
                    }
                },
                {
                    data: "DomainName",
                    orderable: true
                },
                {
                    data: "TypeName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<label class="label label-lg label-info">Highlights</label>';
                        htmlStr += '&nbsp; ' + row.TypeName + '</td>';
                        return htmlStr;
                    }
                },
                {
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        var rqId = row.RequestId;
                        var dmKey = row.DomainKey;
                        var rqType = row.Type;
                        var rqTypeName = row.TypeName;
                        var dmName = row.DomainName;
                        var approveStt = 3;
                        var rejectStt = 4;
                        htmlStr += '<div class="btn-group">';
                        htmlStr += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                        htmlStr += 'Actions &nbsp; <i class="fa fa-angle-down"></i>';
                        htmlStr += '</button>';
                        htmlStr += '<ul class="dropdown-menu dropdown-menu-right primary">';
                        htmlStr += '<li><a href="#app-mbm-admin-transfers" onclick="ProcessExtensionRequest(' + rqId + ', \'' + dmKey + '\',' + rqType + ', ' + approveStt + ')" data-toggle="modal">Approve</a></li>';
                        htmlStr += '<li><a href="#" onclick="rejectSingleRequest(' + rqId + ', \'' + dmKey + '\',' + rqType + ', ' + rejectStt + ')">Reject</a></li>';
                        htmlStr += '</ul>';
                        htmlStr += '</div>';
                        return htmlStr;
                    }
                }
            ]
        });
}

initAdminHistoryExtensionRequestTable = function () {
    var dataTable = $("#history-extensionrequest-list")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#history-extensionrequest-list').LoadingOverlay("show");
            } else {
                $('#history-extensionrequest-list').LoadingOverlay("hide", true);
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
            "ajax": {
                "url": '/DomainExtension/LoadExtensionRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#history-key").val(),
                        "createdUserIdSearch": $("#history-creator").val(),
                        "dateRange": $("#history-daterange").val(),
                        "extensionTypeSearch": $("#history-type").val(),
                        "lstRequestStatusSearch": [3, 4],
                        "domainKey": ""
                    });
                }
            },
            "columns": [
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "RequestedByName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var creatorName = row.RequestedByName;
                        var creatorLogoUri = row.RequestedByLogoUri;
                        var creatorId = row.RequestById;
                        var htmlStr = "";
                        htmlStr += '<a href="/Community/UserProfilePage?uId=' + creatorId + '">';
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + creatorLogoUri + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + creatorName + '</div>'
                        htmlStr += '<div class="clearfix"></div></a>';
                        return htmlStr;
                    }
                },
                {
                    data: "DomainName",
                    orderable: true
                },
                {
                    data: "TypeName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<label class="label label-lg label-info">Highlights</label>';
                        htmlStr += '&nbsp; ' + row.TypeName + '</td>';
                        return htmlStr;
                    }
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return row.StatusLabel;
                    }
                },
                {
                    data: "Note",
                    orderable: true
                }
            ]
        });
}

function rejectSingleRequest(requestId, domainKey, type, status) {
    var _url = "/DomainExtension/GetExtensionRequestById?requestId=" + requestId;
    $.ajax({
        method: 'GET',
        dataType: 'JSON',
        url: _url
    }).done(function (data) {
        if (data.result == false) {
            cleanBookNotification.error(data.msg, "Qbicles");
            return;
        } else {
            var request = data.Object;
            var domainName = request.DomainName;
            var typeName = request.TypeName;

            var $reasonModal = $("#reject-reason-modal");
            $("#reject-reason").val("");
            $reasonModal.find('.modal-body p').html('You\'re about to reject <strong>' + domainName + '\'s</strong> request to post <strong>' + typeName + ' Highlights</strong>. You can optionally provide a covering note to explain why.');
            $reasonModal.find('button[type="submit"]').attr("onClick", 'ProcessExtensionRequest(' + requestId + ', \'' + domainKey + '\', ' + type + ', ' + status + ')');
            $reasonModal.modal("show");
        }
    })
}

function rejectMultipleRequest() {
    var checkedNumber = $('#pending-extensionrequest-list input:checked').length;
    var $reasonModal = $("#reject-reason-modal");
    $("#reject-reason").val("");
    $reasonModal.find('.modal-body p').html('You\'re about to reject <strong> all ' + checkedNumber + ' selected</strong> requests.You can optionally provide a covering note to explain why.');
    $reasonModal.find('button[type="submit"]').attr("onClick", 'ProcessMultipleExtensionRequest(4)');
    $reasonModal.modal("show");
}

function ProcessExtensionRequest(requestId, domainKey, type, status) {
    var _url = "/DomainExtension/ChangeExtensionRequestStatus";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            requestId: requestId,
            domainKey: domainKey,
            type: type,
            status: status,
            note: $("#reject-reason").val()
        },
        success: function (response) {
            if (response.result) {
                $("#pending-extensionrequest-list").DataTable().ajax.reload();
                $("#history-extensionrequest-list").DataTable().ajax.reload();
                $("#reject-reason").val("");
                cleanBookNotification.updateSuccess();
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
            $("#reject-reason").val("");
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
}

function ProcessMultipleExtensionRequest(status) {
    LoadingOverlay();

    var selectedRequest = [];
    $('#pending-extensionrequest-list input:checked').each(function () {
        selectedRequest.push($(this).attr('requestid'));
    });

    var _url = "/DomainExtension/UpdateMultipleExtensionRequestStatus";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            lstRequestIds: selectedRequest,
            status: status,
            note: $("#reject-reason").val()
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#pending-extensionrequest-list").DataTable().ajax.reload();
                $("#history-extensionrequest-list").DataTable().ajax.reload();
                $('.withselected').fadeOut();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
    LoadingOverlayEnd();
}

