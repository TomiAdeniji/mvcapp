
var $alertId = 0;
var $eventId = 0;
var $eventKey = "";
var $mediaId = 0;
var $taskName = $('#taskName'),
    $taskId = 0;


var $approvalModal = $('#create-approval'),
    $approvalQbicleId = $("#approvalQbicleId"),
    $form_approval_addedit = $('#form_approval_addedit'),
    $approvalName = $('#approvalName'),
    $TypeOfRequest = $('#TypeOfRequest'),
    $approvalAttachments = $('#approvalAttachments'),
    $txtNotes = $('#txtNotes'),
    $approvalId = 0;


var $userModal = $("#create-user"),
    $username = $('#username'),
    $email = $("#email"),
    $forename = $("#forename"),
    $surname = $("#surname"),
    $password = $('#password'),
    $password_repeat = $('#password_repeat');

var $selectUsersDomains = $("#selectUsersDomains");
var $validTopic = -1;//exists- already exits in qbicle-will update; addnew-no exists, will add new;duplicate-duplicate-throw error
var strNotificationSelected = "";

jQuery(function ($) {

    $('.reply_options a').bind('click', function (e) {
        e.preventDefault();
        var target = "." + $(this).data('target');
        $(target).toggle();
        $inputfileDiscussion.val('');
    });

    $('#taskDocumentInviteGuests').on('beforeItemAdd', function (event) {
        var tag = event.item;
        // Do some processing here
        if (!event.options || !event.options.preventPost) {
            if (validateEmail(tag)) {
                $.ajax({
                    type: 'post',
                    url: '/Account/CheckUserEmailInSystem',
                    datatype: 'json',
                    data: {
                        userEmail: event.item
                    },
                    success: function (refModel) {
                        if (refModel.result) {
                            $('#taskDocumentInviteGuests').tagsinput('remove', tag, { preventPost: true });
                            event.item = refModel.msg;//exist user in the system, set by user's name
                            $('#taskDocumentInviteGuests').tagsinput('add', event.item, { preventPost: true });
                        }
                    }
                });
                return event;
            }
            else {
                event.cancel = true;
            }
        }
    });
    $('#discussionGuests').on('beforeItemAdd', function (event) {
        var tag = event.item;
        // Do some processing here
        if (!event.options || !event.options.preventPost) {
            if (validateEmail(tag)) {
                $.ajax({
                    type: 'post',
                    url: '/Account/CheckUserEmailInSystem',
                    datatype: 'json',
                    data: {
                        userEmail: event.item
                    },
                    success: function (refModel) {
                        if (refModel.result) {
                            $('#discussionGuests').tagsinput('remove', tag, { preventPost: true });
                            event.item = refModel.msg;//exist user in the system, set by user's name
                            $('#discussionGuests').tagsinput('add', event.item, { preventPost: true });
                        }
                    }
                });
                return event;
            }
            else {
                event.cancel = true;
            }
        }
    });

    $.fn.modal.Constructor.prototype.enforceFocus = function () { };

});


function generateMediaForActivityPage(media, isAppend) {
    //console.log('NOTE: All pages needing append must set parent id as list-medias');
    isPlaceholder(false, '');
    var html = "";
    html += "<article class=\"activity media\">";
    html += "<div class='activity-avatar' style=\"background-image: url('" + media.CreatedAvatar + "&size=T');\"></div>";
    html += "<div class=\"activity-detail\" style=\"width: 100%; max-width: 100%;\">";
    html += "<div class=\"activity-meta\">";
    html += "<h4>" + media.CreatedByName + "</h4>";
    html += "<small class=\"db-date\">" + media.CreatedDate + "</small>";
    html += "<br class=\"visible-xs\">";
    html += "</div>";
    html += "<div class=\"activity-overview media\">";
    html += "<div class=\"row\">";
    html += "<div class=\"col-xs-12 col-sm-5 col-lg-4\">";
    html += "<a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('" + media.Key + "')\">";
    html += " <img src=\"" + media.ImgPath + "\" class=\"img-responsive\">";
    html += "</a>";
    html += "</div>";
    html += "<div class=\"col-xs-12 col-sm-7 col-lg-8 description\">";
    html += "<h5>" + media.Name + "</h5>";
    html += "<p>";
    html += " " + media.Description + " ";
    html += "</p>";
    html += "<small>" + media.Type + " | Update <span class=\"db-start-date\">" + media.LastUpdate + "</span></small>";
    html += "</div>";
    html += "</div>";
    html += "</div>";
    html += "</div>";
    html += "<div class=\"clearfix\"></div>";
    html += "</article>";
    isDisplayFlicker(false);
    if (isAppend) {
        $('#list-medias').append(html);
    } else {
        $('#list-medias').prepend(html);
    }
}

// media display on dashboard
function generateMediaForDashboardPage(media, isAppend) {
    isPlaceholder(false, '');
    var html = "<div class='col'>";
    html += "<div class='media-folder-item activity-overview task'>";
    html += "<a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('" + media.Key + "',false)\">";
    html += "<div class='preview' style=\"background-image: url('" + media.ImgPath + "');\"></div>";
    html += "</a>";
    html += "<div class='meta_desc'>";
    html += "<h5>" + media.Name + "</h5>";;
    html += "<small>" + media.Type + " &nbsp; | &nbsp; Updated " + media.LastUpdate + "</small>";
    html += "</div>";
    html += "<a href='#' data-toggle='modal' data-target='#move-media' onclick=\"QbicleLoadMoveMediaFolders(" + media.FolderId + ",'" + media.Key + "')\" class='btn btn-primary move'><i class='fa fa-exchange'></i> &nbsp; Move</a>";
    html += "</div>";
    html += "</div>";
    isDisplayFlicker(false);
    if (isAppend) {
        $('#list-medias').append(html);
    } else {
        $('#list-medias').prepend(html);
    }
}

$(document).ready(function () {

    $('#form_alert_addedit').validate({
        rules: {
            Name: {
                maxlength: 250
            },
            Description: {
                maxlength: 500
            }
        }
    });

    $("#form_alert_addedit").submit(function (e) {
        $.LoadingOverlay("show");
        e.preventDefault();
        $.ajax({
            type: this.method,
            cache: false,
            url: this.action,
            enctype: 'multipart/form-data',
            data: new FormData(this),
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                isBusyAddAlertForm = true;
            },
            success: function (data) {
                if (data.result) {
                    isBusyAddAlertForm = false;

                    //addTopicToFilter(data.Object.topic.Id, data.Object.topic.Name)

                    $('#create-alert').modal('toggle');
                }
                LoadingOverlayEnd();
            },
            error: function (data) {
                isBusyAddAlertForm = false;
                LoadingOverlayEnd();
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }

        });
    });

    $("#form_approval_addedit").submit(function (e) {
        $.LoadingOverlay("show");
        e.preventDefault();
        $.ajax({
            type: this.method,
            cache: false,
            url: this.action,
            enctype: 'multipart/form-data',
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (data) {
                if (data.result) {
                    $('#create-approval').modal('toggle');
                }
                LoadingOverlayEnd();
            },
            error: function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
                
            }
        });
    });
    initInviteModal();

});



//Create alert
function AddNewAlertClick(currentUserId) {
    ClearAlertInputModalAddEdit();
    $('#selectLinkAlertTo').select2().val([currentUserId]).change();
}
function ClearAlertInputModalAddEdit() {
    $('#alertName').val("");
    $('#selectAlertType').val($('#selectAlertType option:first').val());
    $('#selectAlertPriority').val($('#selectAlertPriority option:first').val());
    $('#txtcontent').val("");
    $('#falertAttachments').val("");
    $("#alert-topic-selected").val("");
    ClearError();
}

var isBusyAddAlertForm = false;
function SaveAlert() {

    if (isBusyAddAlertForm) {
        return;
    }

    var $form_alert_addedit = $('#form_alert_addedit');
    if ($form_alert_addedit.valid()) {
        $.ajax({
            url: "/Alerts/DuplicateAlertNameCheck",
            data: { cubeId: $('#alertQbicleId').val(), alertId: $alertId, AlertName: $('#alertName').val() },
            type: "GET",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result)
                $form_alert_addedit.validate().showErrors({ Name: "Name of Alert already exists in the current Qbicle." });
            else {
                var $falertAttachments = $('#falertAttachments');
                if ($falertAttachments.val()) {
                    var typeIsvalid = checkfile($falertAttachments.val());
                    if (typeIsvalid.stt) {
                        $form_alert_addedit.trigger("submit");
                    } else {
                        $form_alert_addedit.validate().showErrors({ alertAttachments: typeIsvalid.err });
                    }
                } else {
                    $form_alert_addedit.trigger("submit");
                }
            }
        }).fail(function (er) {
            $form_alert_addedit.validate().showErrors({ Name: "Error checking existing name of Alert in the current Qbicle" });
        });
    }
}
//End create alert

//approvals
function SaveQbicleApprovals() {
    if ($("#form_approval_addedit").valid()) {
        $.ajax({
            url: "/Approvals/DuplicateApprovalNameCheck",
            data: { cubeId: $approvalQbicleId.val(), approvalId: $approvalId, approvalName: $approvalName.val() },
            type: "GET",
            dataType: "json",
            async: false,
        }).done(function (refModel) {
            if (refModel.result)
                $("#form_approval_addedit").validate().showErrors({ Name: "Name of Approval already exists in the current Qbicle." });
            else {
                if ($('#taskAttachments').val()) {
                    var typeIsvalid = checkfile($('#taskAttachments').val());
                    if (typeIsvalid.stt) {
                        $("#form_approval_addedit").trigger("submit");
                    } else {
                        $('#form_approval_addedit').validate().showErrors({ taskAttachments: typeIsvalid.err });
                    }
                } else {
                    $("#form_approval_addedit").trigger("submit");
                }
            }

        }).fail(function () {
            $("#form_approval_addedit").validate().showErrors({ approvalName: "Error checking existing name of approval in the current Qbicle" });
        })
    }

};
function AddnewApprovalClick(currentUserId) {
    ClearApprovalModal();

}
function ClearApprovalModal() {
    $approvalName.val("");
    $approvalAttachments.val("");
    $txtNotes.val("");
    $approvalId = 0;
    $TypeOfRequest.val(0);
}

//Create User
function AddnewUserDomain(currentUserId) {
    ClearUserInputModalAddEdit();
    $("#create-user").modal('show');
}
function ClearUserInputModalAddEdit() {
    $username.val(''),
        $email.val(''),
        $password.val(''),
        $password_repeat.val('');
    $forename.val('');
    $surname.val('');
    $("#emailGuest").val('');
    ClearError();
}
function SaveNewUser(packgeType) {
    var domainId = null;
    Save_User();
}
function Save_User() {

    if ($("#create-user-main").valid()) {
        $.ajax({
            url: "/Domain/CreateUser",
            data: { email: $email.val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (!refModel.result) {
                $("#create-user-main").validate().showErrors({ email: refModel.msg });
            }

            else {
                window.location.reload(true);
            }

        }).fail(function () {
            $("#create-user-main").validate().showErrors({ email: "Error invite user into current Domain." });
        })
    }

};

//discussion add new user/guest
function SaveNewGuestDiscussion(packgeType) {
    var domainId = null;
    SaveGuestDiscussion();
}
function SaveGuestDiscussion() {

    if ($("#create-guest-discussion-main").valid()) {
        $.ajax({
            url: "/Discussions/AddNewGuestToDiscussion",
            data: {
                email: $("#emailGuest").val()
            },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (!refModel.result) {
                $("#create-guest-discussion-main").validate().showErrors({ emailGuest: refModel.msg });
            }
            else {
                $("#create-guest").modal('hide');
                $("#emailGuest").val('');
            }

        }).fail(function (xhr, err) {

            $("#create-guest-discussion-main").validate().showErrors({ emailGuest: "Error invite guest into current Discussion." });
        })
    }
};

function AddNewParticipantUsersToDiscussion() {
    $selectUsersDomains.select2().val(null).change();

}
function SaveParticipantUsersToDiscussion() {

    if (!$("#create-participant-domain").valid())
        return;
    $.ajax({
        url: "/Discussions/AddNewParticipantToDiscussion",
        type: "POST",
        dataType: "json",
        data: {
            usersDomainAssign: $selectUsersDomains.val()
        }
    }).done(function (refModel) {
        if (!refModel.result) {
            $("#create-participant-domain").validate().showErrors({ 'usersDomainAssign[]': refModel.msg });
        }
        else {
            var userArray = $selectUsersDomains.val();
            $("#create-participant").modal('hide');
            $.each(userArray, function (key, value) {
                $("#selectUsersDomains option[value='" + value + "']").remove().trigger("change");
            });

        }

    }).fail(function (xhr, err) {

        $("#create-participant-domain").validate().showErrors({ 'usersDomainAssign[]': "Error invite user into current Discussion." });
    });
}
//manage user from our people
function showImageFromInputFile(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {

            $('#domain-logo').attr('src', e.target.result);
            $('#domain-logo').css({ "display": "block" });
        };
        reader.readAsDataURL(input.files[0]);

    }
}

//End create domain

function createRowForDomainTable(obj) {

    var activeOrClose = obj.Status === 2 ? "Closed" : "Active"
    var openOrCloseDomain = obj.Status === 1 ? "Close Domain" : "Open Domain";

    var tr = '';
    tr += '    <tr>';
    tr += "<td class='table_avatar'><div id ='avatar-" + obj.Id + "' class='table-avatar' style='background-image: url(\"" + obj.LogoUri + "&size=T\");'>&nbsp;</div></td> ";
    tr += '                   <td id="name-' + obj.Id + '">' + obj.Name + '</td>                                                                                                         ';
    tr += '                   <td>' + obj.CreatedDate + '</td>                                                                                              ';
    tr += '                   <td id="status-' + obj.Id + '">' + activeOrClose + '</td>                                    ';
    tr += '                   <td>                                                                                                                                           ';
    tr += '                       <div class="btn-group options">                                                                                                            ';
    tr += '                           <button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">       ';
    tr += '                               Options &nbsp; <i class="fa fa-angle-down"></i>                                                                                    ';
    tr += '                           </button>                                                                                                                              ';
    tr += '                           <ul class="dropdown-menu">                                                                                                             ';
    tr += '                                   <li><a href="javascript:void(0)" id="edit-anchor-' + obj.Id + '" onclick="editDomain(\'' + obj.Id + '\',\'' + obj.Name + '\', \'' + obj.LogoPath + '\')">Edit Domain</a></li>           ';
    tr += '                                   <li>                                                                                                                           ';
    tr += '                                       <a href="javascript:void(0)" id="openOrClose-' + obj.Id + '" onclick="openOrCloseDomainById(\'' + obj.Id + '\')"> ' + openOrCloseDomain + '  </a>     ';
    tr += '                                   </li>                                                                                                                          ';
    tr += '                           </ul>                                                                                                                                  ';
    tr += '                       </div>                                                                                                                                     ';
    tr += '                   </td>                                                                                                                                          ';
    tr += '               </tr>           ';
    return tr;
}

function OurPeopleManageUser(userId) {
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Tasks/SetTaskSelected',
        datatype: 'json',
        data: {
            id: 0, goBack: goBack
        },
        success: function (refModel) {

        }
    });

}

// Process Group

function AddNewProcessGroup() {
    $("#group-name-input").val('');
}

function SaveProcessGroup(groupId) {
    if ($('#form-process-group').valid()) {
        $.ajax({
            url: "/ProcessDocumentation/DuplicateProcessGroupNameCheck",
            data: { GroupId: groupId, GroupName: $("#group-name-input").val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                $('#form-process-group').validate().showErrors({ name: "Process Group name already in use." });
            else {
                $('#form-process-group').trigger("submit");
            }
        }).fail(function () {
            $("form-process-group").validate().showErrors({ name: _L("ERROR_MSG_11") });
        })
    }
}

$("#form-process-group").submit(function (e) {
    $.LoadingOverlay("show");
    e.preventDefault();
    $.ajax({
        type: this.method,
        url: this.action,
        data: { groupName: $("#group-name-input").val() },
        dataType: "json",
        success: function (data) {
            if (data.result) {
                $('#app-group-generic-add').modal('toggle');
                $("#process-group-div").prepend(data.msg);
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
            
        }

    });
});
//End of Process group

//Process add

function AddNewProcess(groupId, currentUserId) {
    ResetForm();
    $processGroupId = groupId;
    $("#selectProcessOwner").select2(
        {
            maximumSelectionLength: 1
        }
    ).val([currentUserId]).change();
}

//End of Process add

function AddStepToList() {
    if ($("#form_process_step_addedit").valid())
        $("#form_process_step_addedit").trigger("submit");
}


$("#form_process_step_addedit").submit(function (e) {
    $.LoadingOverlay("show");
    e.preventDefault();
    var SuffixOrder = $("#ul-pro-step li").length + 1;

    $("#StepOrder").val(SuffixOrder);
    $("#Suffix").val(ordinalSuffix(SuffixOrder));
    $.ajax({
        type: this.method,
        cache: false,
        url: this.action,
        data: new FormData(this),
        processData: false,
        contentType: false,
        success: function (refModel) {

            if (refModel.result) {

                $("#ul-pro-step").append(refModel.msg).hide().fadeIn(1000);
                ClearStepForm();
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
            
        }

    });

});

function AddDocumentTolist() {
    if ($("#form_process_related_addedit").valid()) {
        $("#form_process_related_addedit").trigger("submit");
    }
}


$("#form_process_related_addedit").submit(function (e) {
    $.LoadingOverlay("show");

    e.preventDefault();

    $.ajax({
        type: this.method,
        cache: false,
        url: this.action,
        data: new FormData(this),
        processData: false,
        contentType: false,
        success: function (refModel) {
            if (refModel.result) {
                $("#pro-document-tr").append(refModel.msg).hide().fadeIn(1000);
                ClearDocumentForm();
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            LoadingOverlayEnd();
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });

});


function ValidProcessTab() {
    if ($("#form_process_addedit").valid()) {
        NextTab(2);
    }
    else {
        NextTab(-1);
    }
};

function NextTab(tabId) {
    $(document).find('.active').removeClass('active');
    switch (tabId) {
        case -1:
            $("#tab1").addClass('in active');
            $("#tab1-li").addClass('in active');
            break;
        case -2:
            $("#tab2").addClass('in active');
            $("#tab2-li").addClass('in active');
            break;
        case 2:
            $("#tab2").addClass('in active');
            $("#tab2-li").addClass('in active');

            break;
        case 3:
            $("#tab3").addClass('in active');
            $("#tab3-li").addClass('in active');
            break;

    }

}

//process image remove

function ClearProcessForm() {
    document.getElementById("form_process_addedit").reset();
    ClearProcessPicture();
}

function ClearProcessPicture() {
    $("#processPictureId").val("");
    $('#processPictureId').attr('src', "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected");
}

function ResetForm() {
    ClearProcessForm();
    ClearDocumentForm();
    ClearStepForm();
    ClearError();
    $(document).find('.active').removeClass('active');
    $('.toggleable').hide();
    $("#approval_related option:first").attr('selected', 'selected');
    $("#pro-document-tr").empty();
    $("#ul-pro-step").empty();
    $("#tab1").addClass('in active');
    $("#tab1-li").addClass('in active');

}

function ClearStepPicture() {
    $("#stepPictureId").val("");
    $('#stepPicture').attr('src', "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected");
}
function ClearStepForm() {
    document.getElementById("form_process_step_addedit").reset();
    ClearStepPicture();
    $stepEdit = null;
    $stepEditId = null;

}
function ClearDocumentForm() {
    document.getElementById("form_process_related_addedit").reset();
    ClearDocumentPicture();
    ClearError();
}
function ClearDocumentPicture() {
    $("#documentPictureId").val("");
    $('#documentPicture').attr('src', "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected");
}
$("#form_process_addedit").submit(function (e) {
    $.LoadingOverlay("show");
    e.preventDefault();
    LoadingOverlayEnd();
});

function FinishAndSave() {
    $(".process-form input").each(function () {
        if ($.trim($(this).val()).length === 0) {
            $(document).find('.active').removeClass('active');
            $("#tab1").addClass('in active');
            $("#tab1-li").addClass('in active');
            $("#form_process_addedit").trigger("submit");
        }
    });
    var arrayOfObj = $('.pro-step-list').map(function () {
        var liId = $(this).attr('id');
        return {
            Title: $(this).find("#TitlePreview-" + liId).text(),
            Description: $(this).find("#DescriptionPreview-" + liId).text(),
            StepImage: $(this).find("#StepImagePreview-" + liId).text(),
            StepOrder: $(this).find("#StepOrderPreview-" + liId).text(),
        }

    }).get();

    var tableArrayOfObj = $('.pro-document-tr').map(function () {

        var tdId = $(this).attr('id');
        return {
            Document: $(this).find("#document-" + tdId).html(),
            DocumentImage: $(this).find("#documentImage-" + tdId).attr('data-url'),
            FileTypeId: $(this).find("#filetypeId-" + tdId).html()
        }

    }).get();
    var steps = JSON.stringify(arrayOfObj);
    var documents = JSON.stringify(tableArrayOfObj)
    //get process
    var owners = JSON.stringify($("#selectProcessOwner").val());

    var process = {
        Id: $("#process-Id").val(),
        Name: $("#pro-Name").val(),
        Description: $("#pro-Description").val(),
        StartState: $("#pro-StartState").val(),
        EndState: $("#pro-EndState").val(),
        ProcessOwner: owners,
        GroupId: $processGroupId,
        ProcessImage: $('#processImageUrl').val()
    };
    $.ajax({
        type: 'post',
        url: '/Apps/FinishAndSaveProcess',
        dataType: 'json',
        data: { process: process, documents: documents, steps: steps },
        success: function (refModel) {
            if (refModel.result) {

                if ($("#process-Id").val() > 0) {
                    window.location.reload(true);
                    return;
                }
                var insertTo = $("#group-top-" + $processGroupId);
                $(refModel.msg).insertAfter(insertTo).hide().fadeIn(1500);
                $('#app-process-add').modal('hide');
            }
            else {
                $(document).find('.active').removeClass('active');
                $("#tab1").addClass('in active');
                $("#tab1-li").addClass('in active');
                $('#form_process_addedit').validate().showErrors({ Name: refModel.msg });
            }
        }
    });
}

function OpenProcessItem(id) {
    window.location.href = '/ProcessDocumentation/AppProcessDocumentationItem/' + id;

}

function SearchProcess() {

    $("#process-group-div").empty();
    $("#textSearch").val();
    $.ajax({
        type: "POST",
        url: "/Apps/SearchProcess",
        data: { textSearch: $("#textSearch").val() },
        success: function (refModel) {

            if (refModel.result) {
                $("#process-group-div").append(refModel.msg).hide().fadeIn(1000);
            }

        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });

}



function updateAppsForDomain() {
    var appsIdAdded = [];
    $('.apps-account:checkbox:checked').each(function () {
        appsIdAdded.push($(this).val());
    });

    var appsIdRemoved = [];
    $('.apps-account:checkbox:not(:checked)').each(function () {
        appsIdRemoved.push($(this).val());
    });

    $.ajax({
        type: 'POST',
        url: '/Apps/UpdateAppsForDomain',
        dataType: 'JSON',
        data: JSON.stringify({ appsIdAdded: appsIdAdded, appsIdRemoved: appsIdRemoved, roleId: $('#selectRolesDomain').val() }),
        contentType: "application/json",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                var listAdd = refModel.Object;
                $.each(appsIdRemoved, function (index, item) {
                    $('#row-' + item).remove();
                });

                GenerateRows2TableAppByArr(listAdd);

                cleanBookNotification.updateSuccess();
                $('#add-app').modal('hide');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });

}

//step event
function RemoveStep(event) {

    var $stepRemove = $(event).closest('li');
    var $stepRemoveId = $stepRemove[0].id;

    $stepRemove.css("background-color", "#FF3700");
    $stepRemove.fadeOut(1000, function () {
        $stepRemove.remove();
    });
    setTimeout(function () {
        updatePosition();
    }, 1550);
    ClearStepForm();
}
var $stepEdit, $stepEditId;
function EditStep(event) {
    $stepEdit = $(event).closest('li');
    $stepEditId = $stepEdit[0].id;
    $("#pro-step-title").val($stepEdit.find($("#TitlePreview-" + $stepEditId)).text());
    $("#pro-step-description").val($stepEdit.find($("#DescriptionPreview-" + $stepEditId)).text());
    $('#stepPicture').attr('src', $stepEdit.find($("#StepImagePreview-" + $stepEditId)).text());
    $('.step_add').hide();
    $('.step_edit').show();
}
function ConfirmChangeStep() {
    $stepEdit.find($("#TitlePreview-" + $stepEditId)).text($("#pro-step-title").val());
    $stepEdit.find($("#DescriptionPreview-" + $stepEditId)).text($("#pro-step-description").val());
    $stepEdit.find($("#StepImagePreview-" + $stepEditId)).text($('#StepImage').val());
    ClearStepForm();

    $('.step_add').show();
    $('.step_edit').hide();

}
function CancelChangeStep() {
    ClearStepForm();
    $('.step_add').show();
    $('.step_edit').hide();
}
//document event

function RemoveDocument(id) {
    var $trDelete = $("#" + id);
    $trDelete.css("background-color", "#FF3700");
    $trDelete.fadeOut(1500, function () {
        $trDelete.remove();
    });
}

//check upload file here
function processPictureChange(sender) {
    readURL(sender, "processPicture");
}

function stepPictureChange(sender) {
    readURL(sender, "stepPicture");
}

function documentPictureChange(sender) {
    readURL(sender, "documentPicture");
}

function readURL(input, pictureboxId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var $inputFile = $('#' + pictureboxId);
            $inputFile.attr('src', e.target.result);
            //valid file type
            var file = input.files[0]
            if (!file)
                return;
            var formData = new FormData();
            formData.append("file", file);

            $.ajax({
                url: "/Apps/AddPictureProcessToGalerry",
                dataType: 'json',
                type: "POST",
                data: formData,
                processData: false,
                contentType: false
            }).done(function (refModel) {
                if (refModel.result) {
                    if (pictureboxId === "stepPicture")
                        $('#StepImage').val(refModel.msg);
                    else if (pictureboxId === "documentPicture")
                        $('#DocumentImage').val(refModel.msg);
                    else if (pictureboxId === "processPicture")
                        $('#processImageUrl').val(refModel.msg);
                    else if (pictureboxId === "approvalPicture")
                        $('#processImageUrl').val(refModel.msg);
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }).error(function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            });

        }
        reader.readAsDataURL(input.files[0]);
    }
}

function updatePosition() {
    $('.sortable li').each(function (i) {
        var $index = i + 1;
        //Update position after remove step
        $(this).find('.ordinal-position').text($index + ordinalSuffix($index));
        $(this).find('.ordinal-position-order').text($index);
    });
}



function SearchApprovalAppGroup() {

    $("#approval-group-div").empty();
    $("#textSearch").val();
    $.ajax({
        type: "POST",
        url: "/ApprovalApps/SearchProcess",
        data: { textSearch: $("#textSearch").val() },
        success: function (refModel) {

            if (refModel.result) {
                $("#approval-group-div").append(refModel.msg).hide().fadeIn(1000);
            }

        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });

}
//



function bindTopicFromCookie() {
    var topicId = getCookie("topic_stream");
    if (topicId) {
        $('#toppic-value').val(topicId);
    }
}

$('#toppic-value').ready(function () {
    bindTopicFromCookie();
});

function genarateRowsForAtivityDetail(listPost) {

    var html = "";
    listPost.forEach(function (post) {
        var picPath = "'" + post.ProfilePic + "&size=T'";
        html += "<article id=\"post-" + post.Id + "\" class=\"activity post\">";
        html += "<div class='activity-avatar' style=\"background-image: url(" + picPath + ");\"></div>";
        html += "        <div class=\"activity-detail\">";
        html += "            <div class=\"activity-meta\">";
        html += "                <h4>" + post.FullName + "</h4>";
        html += "                <small class=\"db-date\">" + post.CreatedDate + "</small>";
        html += "            </div>";
        html += "            <div class=\"activity-overview media-comment\">";
        html += "                <p>" + post.Message + "</p>";
        html += "        </div>";
        html += "    </div>";
        html += "    <div class=\"clearfix\"></div>";
        html += "</article>";
    });
    console.log(html)
    return html;
}



function ShowNotifications() {
    $('#notifications').modal('show');
    $('#notification-modal-show').LoadingOverlay("show");
    var ulHeight = $("#notification-modal-show").height();
    if (ulHeight === 0)
        ulHeight = '100%';
    else
        ulHeight = ulHeight + "px";
    $("#notification-modal-show").css({ "height": ulHeight });
    $("#notification-modal-show li").remove();
    $("#btnRemoveallNotification").attr("disabled", "disabled");
    $.ajax({
        type: 'post',
        url: '/Notifications/ShowNotificationsModal',
        dataType: 'json',
        data: { order: $("#orderNotification").val() },
        success: function (data) {
            if (data.length <= 0) {
                return;
            }
            $("#notification-modal-show").html(data.ModelString);
            if ($("#notification-modal-show input").length == 0) {
                $('.checkall').prop("disabled", true);
            }
            $('#notification-modal-show').LoadingOverlay("hide", true);

            if (strNotificationSelected !== "") {
                var isCheck = false;
                var arrId = strNotificationSelected.split(',');
                for (var i = 0; i < arrId.length; ++i) {
                    if ($('#post-notifi-' + arrId[i]).find('.cb-element').length > 0) {
                        $('#post-notifi-' + arrId[i]).find('.cb-element').prop("checked", true);
                        isCheck = true;
                    }
                    if ($('#activity-notifi-' + arrId[i]).find('.cb-element').length > 0) {
                        $('#activity-notifi-' + arrId[i]).find('.cb-element').prop("checked", true);
                        isCheck = true;
                    }
                    if ($('#qbicle-notifi-' + arrId[i]).find('.cb-element').length > 0) {
                        $('#qbicle-notifi-' + arrId[i]).find('.cb-element').prop("checked", true);
                        isCheck = true;
                    }
                }
                if ($(".cb-element").length === arrId.length && isCheck) {
                    $(".checkall").data('checked', true);
                    $(".checkall").html('Uncheck all');
                    $('.removeall').prop("disabled", false);
                }
                else {
                    if (isCheck) {
                        $(".checkall").html('Check all');
                        $(".checkall").data('checked', false);
                        $('.removeall').prop("disabled", false);
                    }
                }

            }
            else {
                $(".checkall").data('checked', false);
                $(".checkall").html('Check all');
                $('.removeall').prop("disabled", true);
            }
        }, error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Show Notifications");
            $('#notification-modal-show').LoadingOverlay("hide", true);
        }
    });
}

function InitNotificationsPagination() {
    $('#notifications').modal('show');
    var $data_container_notification = $('#data-container-notifications');
    var $pagination_container = $('#pagiation-notifications');
    $pagination_container.pagination({
        dataSource: '/Notifications/ShowNotificationsModal',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_notification.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 5,
        autoHidePrevious: true,
        autoHideNext: true,
        prevText: "Previous",
        nextText: "Next",
        ajax: {
            data: { orderBy: $("#orderNotification").val() },
            beforeSend: function () {
                $data_container_notification.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            $data_container_notification.empty();
            var tbl = '<table class="datatable table-hover" data-orderable="false" style="width: 100%;"><tbody></tbody></table>';
            $data_container_notification.html(tbl);
            if (data.length > 0)
                data.forEach(function (item) {
                    $('#data-container-notifications tbody').append(item);
                });

            $(".checkall").data('checked', false);
            $(".checkall").html('Check visible');
            $('.removeall').prop("disabled", true);
            strNotificationSelected = "";
        }
    });
}

function CheckNotifications(obj) {
    var value = ($(obj).val() + '').trim().split('_');
    if ($(obj).is(":checked")) {
        $("#btnRemoveallNotification").removeAttr("disabled");

        if (strNotificationSelected === "")
            strNotificationSelected = value[0];
        else
            strNotificationSelected += ',' + value[0];
    }
    else {
        //strNotificationSelected = strNotificationSelected.replace(value[0], "");
        if (strNotificationSelected.indexOf(value[0]) === 0)
            strNotificationSelected = strNotificationSelected.replace(value[0] + ',', "");
        else if (strNotificationSelected.lastIndexOf(value[0]) === strNotificationSelected.length)
            strNotificationSelected = strNotificationSelected.replace(',' + value[0], "");
        else
            strNotificationSelected = strNotificationSelected.replace(',' + value[0], "");

        if ($("input[name='notifications[]']:checked").length === 0) {
            $("#btnRemoveallNotification").attr("disabled", "disabled");
        }
        else {
            $("#btnRemoveallNotification").removeAttr("disabled");
        }
    }
    if ($(".cb-element").length === strNotificationSelected.split(',').length) {
        $(".checkall").data('checked', true);
        $(".checkall").html('Uncheck all');
        $('.removeall').prop("disabled", false);
    }
    else {
        $(".checkall").html('Check all');
        $(".checkall").data('checked', false);
        $('.removeall').prop("disabled", false);
    }
}

function DeleteAllNotification() {

    var strNotiId = "";
    if ($("input[name='notifications[]']:checked").length > 0) {
        $("input[name='notifications[]']:checked").each(function () {
            if (strNotiId === "")
                strNotiId = $(this).val();
            else
                strNotiId += "," + $(this).val();
        });
    }
    console.log(strNotiId);
    MarkAsMultipleReadNotification(strNotiId);
}
function QbicleSelected(key, module, paramaters) {
    $.ajax({
        type: 'post',
        url: '/Commons/BindingQbicleParameter',
        dataType: 'json',
        data: {
            key: key,
            ModuleSelected: module
        },
        success: function (refModel) {
            if (refModel.result === true) {
                if (typeof paramaters === "undefined")
                    window.location.href = "/Qbicles/Dashboard";
                else
                    window.location.href = "/Qbicles/Dashboard?" + paramaters;
            }
        }
    });
}
function postSelelected(qbicleKey, postKey, module) {
    $.ajax({
        type: 'post',
        url: '/Commons/BindingQbicleParameter',
        dataType: 'json',
        data: {
            key: qbicleKey,
            ModuleSelected: module
        },
        success: function (refModel) {
            if (refModel.result === true) {
                MarkAsReadActivity(postKey);
                window.location.href = "/Qbicles/Dashboard";
            }
        }
    });
}
function initInviteModal() {
    var $frmJoinQbicles = $('#frmJoinQbicles');
    $frmJoinQbicles.validate({
        ignore: "",
        rules: {
            email: {
                required: true,
                email: true
            },
            message: {
                required: true
            }
        }
    });
    $frmJoinQbicles.submit(function (e) {
        e.preventDefault();
        if ($frmJoinQbicles.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                datatype: "json",
                data: $(this).serialize(),
                success: function (data) {
                    if (data.result) {
                        $('.email-invite').text($('#frmJoinQbicles input[name=email]').val());
                        $('#invitesent').fadeIn();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Qbicles");
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
    $('#send-invite').on('shown.bs.modal', function (e) {
        $('#invitesent').hide();
        $frmJoinQbicles.trigger("reset");
        $frmJoinQbicles.data('validator').resetForm();
    })
}

$(function () {
    $("#orderNotification").on("change", function () {
        InitNotificationsPagination();
    });
    $(".checkall").bind('click', function () {
        var checked = $(this).data('checked');

        if (checked === false) {
            $(this).data('checked', true);
            $(this).html('Uncheck visible');
            $('.removeall').prop("disabled", false);
            $(".cb-element").each(function () {
                $(this).prop("checked", true);
            });
            var value = "";
            strNotificationSelected = "";
            if ($("input[name='notifications[]']:checked").length > 0) {
                $("input[name='notifications[]']:checked").each(function () {
                    value = ($(this).val() + '').trim().split('_');
                    if (strNotificationSelected === "")
                        strNotificationSelected = value[0];
                    else {
                        //if (strNotificationSelected.indexOf("," + value[0])<=0)
                        strNotificationSelected += "," + value[0];
                    }
                });
            }
        }

        if (checked === true) {
            $(this).data('checked', false);
            $(this).html('Check visible');
            $('.removeall').prop("disabled", true);
            $(".cb-element").each(function () {
                $(this).prop("checked", false);
            });
            strNotificationSelected = "";
        }
    });

});

function DeleteAllNotifications() {
    var confirmation = confirm("Are you sure you want to delete all notifications?");
    if (confirmation == true) {
        $("#notifications").LoadingOverlay("show");
        $.ajax({
            method: 'POST',
            url: '/Notifications/DeleteAllNotifications',
            dataType: 'JSON',
            success: function (response) {
                cleanBookNotification.success("Deleted notifications successfully!", "Qbicles");
                InitNotificationsPagination();
                $("#notifications").LoadingOverlay("hide");
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
                $("#notifications").LoadingOverlay("hide");
            }
        });
    }
}