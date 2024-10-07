function showListShortlistGroups() {
    var _url = "/Network/showListSlGroups";
    
    var searchKey = $("#slgroup-search").val();
    $("#slgroup-list").empty();
    $("#slgroup-list").load(_url, { keySearch: searchKey });
}

function showAddEditSlGroupModal(groupId) {
    LoadingOverlay();
    var _url = "/Network/showAddEditSlGroupView";
    $("#networklistings-group-add").empty();
    $("#networklistings-group-add").load(_url, { groupId: groupId });
    $("#networklistings-group-add").modal('show');
    LoadingOverlayEnd();
}

function initSaveSlGroupForm() {
    var $savegroupform = $("#add-slgroup-form");
    var groupid = $("#group-id").val();
    $savegroupform.validate({
        rules: {
            title: {
                required: true
            },
            summary: {
                required: true
            }
        }
    });

    $savegroupform.submit(function (e) {
        e.preventDefault();
        var files = document.getElementById("group-icon").files;
        if ($savegroupform.valid()) {
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("group-icon").then(function (mediaS3Object) {
                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveShortlistGroup(s3Object);
                    }
                });
            } else {
                if (groupid <= 0) {
                    $savegroupform.validate().showErrors({ icon: "This field is required." });
                    LoadingOverlayEnd('hide');
                } else {
                    saveShortlistGroup(null);
                }
            }
        }
    });
}

function saveShortlistGroup(uploadModel) {
    var _url = "/Network/saveSlGroup";
    var group = {
        Id: $("#group-id").val(),
        Title: $("#group-title").val(),
        Summary: $("#group-summary").val()
    };
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            slGroup: group,
            uploadModel: uploadModel
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Shortlist Group successfully.", "Qbicles");
                showListShortlistGroups();
                $("#networklistings-group-add").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    });
    
}

function deleteSlGroup(groupId) {
    if (confirm("Are you sure you want to delete this group? Any shortlisted candidates may be lost.")) {
        LoadingOverlay();
        var _url = "/Network/deleteSlGroup?slGroupId=" + groupId;
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success("Delete Shortlist Group successfully.", "Qbicles");
                    showListShortlistGroups();
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
}

function removeSlGroupCandidate(userid, groupId) {
    LoadingOverlay();
    var _url = "/Network/RemoveSLGroupCandidate?userId=" + userid + "&slGroupId=" + groupId;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Remove candidate successfully!", "Qbicles");
                $("#candidate-list").DataTable().ajax.reload();
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

function initSlGroupCandidateTable() {
    LoadingOverlay();
    $("#candidate-list").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#candidate-list').LoadingOverlay("show");
        } else {
            $('#candidate-list').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/Network/LoadShortlistGroupCandidates',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "slGroupId": $("#active-group-id").val(),
                    "searchKey": $("#candidate-search-key").val(),
                });
            }
        },
        "columns": [
            {
                name: "userFullName",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<a href="/Community/UserProfilePage?uId=' + row.userId + '" target="_blank">';
                    htmlString += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + row.LogoUri + '\');"></div>';
                    htmlString += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + row.userFullName + '</div>';
                    htmlString += '<div class="clearfix"></div>';
                    htmlString += '</a>';
                    return htmlString;
                }
            },
            {
                name: "Email",
                data: "Email",
                orderable: true
            },
            {
                name: "Tel",
                data: "Tel",
                orderable: true
            },
            {
                name: "Job",
                data: "Job",
                orderable: true
            },
            {
                name: "isConnectedC2C",
                data: "isConnectedC2C",
                render: function (value, type, row) {
                    return row.isConnectLabel;
                }
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    if (!row.isConnectedC2C) {
                        htmlString += '<li><a href="javascript:void(0)" onclick="connectC2CPublicProfilePage(\'' + row.userId + '\', \'' + row.userFullName + '\', false)">Connect</a></li>';
                    } else {
                        htmlString += '<li><a href="javascript:void(0)" onclick="connectC2CPublicProfilePage(\'' + row.userId + '\', \'' + row.userFullName + '\', true)">Talk in Community hub</a></li>';
                    }
                    htmlString += '<li><a href="javascript:void(0)" onclick="$(\'#candidate-id\').val(\'' + row.userId + '\');showChangeSlGroupView(\'' + row.userId + '\')">Change group</a></li>';
                    htmlString += '<li><a href="javascript:void(0)" onclick="removeSlGroupCandidate(\'' + row.userId + '\', ' + $("#active-group-id").val() + ')">Remove from list</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
    LoadingOverlayEnd();
}
