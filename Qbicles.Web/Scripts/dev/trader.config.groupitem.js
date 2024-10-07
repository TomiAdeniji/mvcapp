
// begin group funsions
function clickAddgroup() {
    ResetFormControl('form_group_add');
}
function Editgroup(groupId) {
    ResetFormControl('form_group_edit');
    $.ajax({
        type: 'get',
        url: '/TraderConfiguration/GetEditGroupById',
        data: { id: groupId },
        dataType: 'json',
        success: function (group) {
            $('#group-edit-id').val(group.Id);
            $('#group-edit-name').val(group.Name);
            $('#group-edit-createby').val(group.CreatedBy);
            $('#group-edit-createdate').val(group.CreatedDate);
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function addgroup() {
    if ($("#form_group_add").valid()) {
        $.ajax({
            type: 'post',
            url: '/TraderConfiguration/SaveGroup',
            data: { Name: $('#addnew-group-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        reloadgroup();
                        $('#app-group-add').modal('toggle');
                    }
                    else if (response.actionVal === 2) {

                        cleanBookNotification.updateSuccess();
                        reloadgroup();
                        $('#app-group-edit').modal('toggle');
                    }
                    else if (response.actionVal === 3) {
                        //cleanBookNotification.error(response.msg, "Qbicles");
                        $("#form_group_add").validate().showErrors({ Name: response.msg });
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            //ResetFormControl('form_group_add');
        });
    }
}
function updategroup() {
    if ($("#form_group_edit").valid()) {
        $.ajax({
            type: 'post',
            url: '/TraderConfiguration/SaveGroup',
            data: { Id: $('#group-edit-id').val(), Name: $('#group-edit-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        $('#app-group-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        reloadgroup();
                    } else if (response.actionVal === 2) {
                        $('#app-group-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        reloadgroup();
                    } else if (response.actionVal === 3) {
                        //cleanBookNotification.error(response.msg, "Qbicles");
                        $("#form_group_edit").validate().showErrors({ Name: response.msg });
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            //ResetFormControl('form_group_add');
        });
    }
}
function ConfirmDeletegroup(id, name) {
    $('#label-confirm-group').text("Do you want delete group: " + name);
    $('#id-itemgroup-delete').val(id);
}
function deletegroup() {
    $.ajax({
        type: 'delete',
        url: '/TraderConfiguration/Deletegroup',
        data: { id: $('#id-itemgroup-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                reloadgroup();
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            $('.modal-cancel').click();
        }
    });
}

reloadgroup = function () {
    setTimeout(function () {
        $('#comfig-content').load('/TraderConfiguration/TraderConfigurationContent?value=groupItem');
    }, 500);
}
// end location functions