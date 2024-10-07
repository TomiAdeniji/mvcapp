
// begin Items & Product funsions
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

$(document).ready(function () {    
    var $quickaddfrm = $("#form_group_add");
    $quickaddfrm.submit(function (e) {
        e.preventDefault();
        addgroup();
    });
});

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
        $('#comfig-content').load('/TraderConfiguration/TraderConfigurationContent?value=group');
    }, 500);
}

// end location functions

// begin Contact group
function addContactGroup() {
    $('#title-contact-group').text('Add Contact Group');
    $('#contact-group-id').val(0);
    $('#contact-group-edit-name').val("");
}
function editContactGroup(id, name) {
    $('#title-contact-group').text('Edit Contact Group');
    $('#contact-group-id').val(id);
    $('#contact-group-edit-name').val(name);
}  

function confirmDeleteContactGroup(id, name) {
    $('#label-confirm-contact-group').text("Do you want delete group: " + name);
    $('#id-contact-group-delete').val(id);
}
    
function deletecontactgroup() {
    $.ajax({
        type: 'delete',
        url: '/TraderConfiguration/DeleteContactGroup',
        data: { id: $('#id-contact-group-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                reloadTableContactGroup();
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}


function savecontactgroup(type) {  
    var id = $('#contact-group-id').val();
    var name = $('#contact-group-edit-name').val() + "";
    if (name.trim() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_374"), "Qbicles");
        return;
    } else {
        $.ajax({
            type: 'post',
            url: '/TraderConfiguration/SaveContactGroup',
            data: {
                group: { Id: id, Name: name, saleChannelGroup: type }
            },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        reloadTableContactGroup();
                        $('#app-contact-group-edit').modal('hide');
                    }
                    else if (response.actionVal === 2) {  

                        cleanBookNotification.updateSuccess();
                        reloadTableContactGroup();
                        $('#app-contact-group-edit').modal('hide');
                    }
                    else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");

                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_375"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            //ResetFormControl('form_group_add');
        });
    }
}

function reloadTableContactGroup() {
    $.LoadingOverlay("show");
    $('#table_ContactGroup').load("/TraderConfiguration/ShowTableContactGroup", function () {
        $.LoadingOverlay("hide");
    });
}

function deleteContactGroup(id) {
    $.ajax({
        type: 'delete',
        url: '/TraderConfiguration/DelteContactGroup?id=' + id,
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                cleanBookNotification.removeSuccess();
                $('#tr_contact_group_' + id).remove();
                $('#tab-groups-contacts .datatable').DataTable().draw();

            } else
                cleanBookNotification.error(_L("ERROR_MSG_376"), "Qbicles");
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_group_add');
    });
}
function ShowGroupMemberContact(id) {
    $('#group-items-view').load("/TraderConfiguration/ShowListMemberForContactGroup?contactGroupId=" + id);
}
// end Contact group
