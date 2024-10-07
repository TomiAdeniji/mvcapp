
var $button_add_group = $("#button_add_group"),
    $approvalAppsGroupId = $("#approval_group"),
    $approvalAppId = 0,
    $appIdDelete = 0,
    $liDelete = null;

var $modal_group = $("#modal_group"),
    $modal_group_title = $("#modal_group [class='modal-title']"),
    $input_group_id = $("#input_group_id"),
    $input_group_name = $("#approval-group-name-input");
var $modal_app = $("#create-approval-type"), $modal_app_remove = $("#modal-delete-app");
$(document).ready(function () {

    LoadApps();

    $button_add_group.bind('click',
        function (event) {
            if (this.classList.contains('isDisabled')) {
                event.preventDefault();
                return;
            }
            ClearError();
            $input_group_id.val(0);
            $input_group_name.val('');
            $modal_group_title.text("Create a group");
            $modal_group.modal('toggle');
            $("#save-group").text("Add now");
        });
});

function LoadApps() {
    cleanBookNotification.clearmessage();
    $.ajax({
        url: "/Approvals/LoadApproval/",
        cache: false,
        type: "POST",
        success: function (data) {
            if (data.length !== 0) {
                $(data.ModelString).insertAfter("#app-content").hide().fadeIn(1000);
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};

function SaveApprovalAppGroup() {
    if ($('#form-approval-group').valid()) {
        $.ajax({
            url: "/ApprovalApps/DuplicateApprovalGroupNameCheck",
            data: { groupId: $input_group_id.val(), groupName: $("#approval-group-name-input").val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                $('#form-approval-group').validate().showErrors({ name: "Approval Group name already in use." });
            else {
                $('#form-approval-group').trigger("submit");
            }
        }).fail(function () {
            $("form-approval-group").validate().showErrors({ name: _L("ERROR_MSG_84") });
        })
    }
}
$("#form-approval-group").submit(function (e) {

    e.preventDefault();
    $.ajax({
        type: this.method,
        url: this.action,
        data: {
            groupId: $input_group_id.val(),
            groupName: $("#approval-group-name-input").val()
        },
        dataType: "json",
        success: function (refModel) {
            if (refModel.result) {
                $("#app-page-display").empty();
                $("#app-page-display").html("<div id='app-content'></div>");
                LoadApps();
                $modal_group.modal('toggle');
                if ($input_group_id.val() === "0") {
                    cleanBookNotification.createSuccess();
                    $("#approval_group").prepend("<option value='" + refModel.msgId + "'>" + refModel.msgName + "</option>");
                }
                else {
                    cleanBookNotification.updateSuccess();
                    $('#approval_group [value="' + refModel.msgId + '"]').text(refModel.msgName);
                }
            }
            else
                cleanBookNotification.error(refModel.msg, "Qbicles");
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });
});

function EditGroup(groupId) {

    if ($("#button_edit_group_grid-" + groupId).hasClass('isDisabled')) {
        return;
    }
    ClearError();
    if (groupId && groupId > 0) {
        $.ajax({
            type: 'GET',
            url: "/ApprovalApps/GetApprovalGroup",
            datatype: 'json',
            data: { groupId: groupId },
            success: function (refModel) {
                if (refModel.result) {

                    $("#form-approval-group").validate().resetForm();
                    $input_group_id.val(refModel.msgId);
                    $input_group_name.val(refModel.msg);

                    $modal_group_title.text("Edit Group");
                    $("#save-group").text("Confirm");

                    $modal_group.modal('toggle');
                }
                else
                    cleanBookNotification.error(_L("ERROR_MSG_85"), "Qbicles");
            },
            error: function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });

    }
}

function EditProcessApp(id, groupId, ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }
    ClearError();
    ResetApprovalAppForm();
    $("#approval-title").text("Edit Approval Request Process");
    $approvalAppId = id;
    $approvalAppsGroupId.val(groupId);
    //get data to modal edit

    GetApprovalApps($approvalAppId).done(function (approvalApp) {

        if (approvalApp.Id) {
            //detail tab
            $approvalAppId = approvalApp.Id;
            $approvalAppsGroupId.val(approvalApp.GroupId);
            $('#processImageUrl').val(approvalApp.ProcessImage);
            $('#approvalPicture').attr('src', approvalApp.ProcessImage);
            $('#approval_type').val(approvalApp.Type);
            if (approvalApp.Type === 4) {
                $('#initiates').hide();
            } else {
                $('#initiates').show();
            }

            $('#approval_group').val(approvalApp.GroupId).trigger("change");;
            $("#approval_title").val(approvalApp.Name);
            $("#approval_description").val(approvalApp.Description);
            //get roles tab
            var Initiators = approvalApp.Initiate.split(',');
            $("#approval_Initiate").select2().val(Initiators).change();
            var Reviewers = approvalApp.Reviewer.split(',');
            $("#approval_Reviewer").select2().val(Reviewers).change();
            var Approvers = approvalApp.Approval.split(',');
            $("#approval_Approver").select2().val(Approvers).change();
            //document tab
            $("#pro-document-tr").append(approvalApp.DocumentHtml).hide().fadeIn(1000);
            $modal_app.modal('toggle');
        }
    })
};

function GetApprovalApps(id) {
    return $.ajax({
        url: "/ApprovalApps/GetApprovalApp",
        type: "GET",
        dataType: "json",
        data: { approvalAppId: id }
    });
}

function approvalPictureChange(sender) {
    readURL(sender, "approvalPicture");
}


function ValidApprovalTab() {
    if ($("#form_approval_app_addedit").valid()) {
        NextTab(2);
    }
    else {
        NextTab(-1);
    }
};

function AddNewApprovalApp(groupId) {

    ResetApprovalAppForm();
    //$approvalAppsGroupId = groupId;
    if (groupId === 0)
        $approvalAppsGroupId.val($("#approval_group option:first").val());
    else
        $approvalAppsGroupId.val(groupId);
    $("#approval-title").text("Create a Request Process");
    $modal_app.modal('toggle');
}


function ResetApprovalAppForm() {
    ClearApprovalAppForm();
    ClearDocumentForm();
    ClearRolesForm();
    ClearError();
    $(document).find('.active').removeClass('active');
    $('.toggleable').hide();
    $("#approval_related option:first").attr('selected', 'selected');
    $("#pro-document-tr").empty();
    $("#tab1").addClass('in active');
    $("#tab1-li").addClass('in active');
    $('#related_items').empty();
    $('#initiates').show();
}

function ClearApprovalAppForm() {
    document.getElementById("form_approval_app_addedit").reset();
    ClearApprovalAppPicture();
    $approvalAppId = 0;
}
function ClearApprovalAppPicture() {
    $("#approvalPictureId").val("");
    $('#approvalPicture').attr('src', "https://www.placehold.it/300x250/EFEFEF/AAAAAA&text=no+image+selected");
}


function ClearRolesForm() {
    document.getElementById("form_approval_role_addedit").reset();
    $("#approval_Initiate").select2("val", "");
    $("#approval_Reviewer").select2("val", "");
    $("#approval_Approver").select2("val", "");
}

$("#form_approval_app_addedit").submit(function (e) {
    e.preventDefault();
});

function ApprovalFinishAndSave() {

    $(".approval-app-form input").each(function () {
        if ($.trim($(this).val()).length === 0) {
            $(document).find('.active').removeClass('active');
            $("#tab1").addClass('in active');
            $("#tab1-li").addClass('in active');
            $("#form_process_addedit").trigger("submit");
            return;
        }
    });

    var tableDocumentOfObj = $('.pro-document-tr').map(function () {

        var tdId = $(this).attr('id');
        return {
            Document: $(this).find("#document-" + tdId).html(),
            DocumentImage: $(this).find("#documentImage-" + tdId).attr('data-url'),
            FileTypeId: $(this).find("#filetypeId-" + tdId).html()
        }

    }).get();

    var documents = JSON.stringify(tableDocumentOfObj)
    //get process
    var $approval_Initiate = JSON.stringify($("#approval_Initiate").val());
    var $approval_Reviewer = JSON.stringify($("#approval_Reviewer").val());
    var $approval_Approver = JSON.stringify($("#approval_Approver").val());

    var approvalApp = {
        Id: $approvalAppId,
        Name: $("#approval_title").val(),
        Description: $("#approval_description").val(),
        GroupId: $approvalAppsGroupId.val(),
        Initiate: $approval_Initiate,
        Reviewer: $approval_Reviewer,
        Approval: $approval_Approver,
        ProcessImage: $('#processImageUrl').val(),
        Type: $('#approval_type').val()
    };
    var listItems = $('#related_items .item');
    var formRelatedIds = [];

    listItems.each(function (index, item) {
        formRelatedIds.push(item.id.split('-')[1]);
    });

    $.ajax({
        type: 'post',
        url: '/ApprovalApps/FinishAndSaveApproval',
        dataType: 'json',
        data: { approvalApp: approvalApp, documents: documents, formRelatedIds: formRelatedIds },
        success: function (refModel) {
            if (refModel.result) {
                $("#app-page-display").empty();
                $("#app-page-display").html("<div id='app-content'></div>");
                LoadApps();
                $modal_app.modal('toggle');
                if ($approvalAppId === 0)
                    cleanBookNotification.createSuccess();
                else
                    cleanBookNotification.updateSuccess();
            }
            else {
                $(document).find('.active').removeClass('active');
                $("#tab1").addClass('in active');
                $("#tab1-li").addClass('in active');
                $('#form_approval_app_addedit').validate().showErrors({ Title: refModel.msg });
            }
        }
    });
}

function ClearError() {
    $("label.error").hide();
    $(".error").removeClass("error");
    $("label.valid").hide();
    $(".valid").removeClass("valid");
};

function DeleteApproval(id, name, ev) {
    if (ev.className.indexOf('isDisabled') >= 0 || id <= 0) {
        return;
    }
    $liDelete = $("#approval-" + id);
    $appIdDelete = id;
    $("#confirm-del-app").show();
    $('#app-name-confirm').text(name);

    $modal_app_remove.modal('toggle');

};
function ApprovalDelete() {
    if ($appIdDelete <= 0)
        return;

    $.ajax({
        type: 'post',
        url: '/ApprovalApps/DeleteApprovalRequest',
        dataType: 'json',
        data: { id: $appIdDelete },
        success: function (res) {
            if (res.status) {
                $modal_app_remove.modal('toggle');

                $($liDelete).css("background-color", "#FF3700");
                $($liDelete).fadeOut(1500,
                    function () {
                        $($liDelete).remove();
                    });
                $appIdDelete = 0;
                cleanBookNotification.removeSuccess();
            } else
                cleanBookNotification.error(_L("ERROR_MSG_86"), "Qbicles");
        }
    });
};

function ViewProcessApp(id, groupId, ev) {

    ClearError();
    GetApprovalApps(id).done(function (approvalApp) {

        if (approvalApp.Id) {
            $('#approval_group_view').val(approvalApp.GroupName);
            $("#approval_title_view").val(approvalApp.Name);
            $("#approval_description_view").val(approvalApp.Description);
            $('#approvalPictureView').attr('src', approvalApp.ProcessImage);
            $('#approval_type_view').val(approvalApp.TypeName);


            //get roles tab
            $("#approval_initiate_view").val(approvalApp.InitiateName);
            $("#approval_review_view").val(approvalApp.ReviewerName);
            $("#approval_approve_view").val(approvalApp.ApprovalName);
            $(document).find('.active').removeClass('active');
            $("#tab1_view").addClass('in active');
            $("#tab1-li_view").addClass('in active');

            $("#view-trader-approval").modal('toggle');
        }
    });
};

NextView = function () {
    $(document).find('.active').removeClass('active');
    $("#tab2_view").addClass('in active');
    $("#tab2-li_view").addClass('in active');
};

PreviousView = function () {
    $(document).find('.active').removeClass('active');
    $("#tab1_view").addClass('in active');
    $("#tab1-li_view").addClass('in active');
}