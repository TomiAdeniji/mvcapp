
// begin dimension funsions
function clickAddDimension() {
    ResetFormControl('form_dimension_add');
}
function EditDimension(dimensionId) {
    ResetFormControl('form_dimension_edit');
    $.ajax({
        type: "get",
        url: "/Bookkeeping/GetEditDimensionById",
        data: { id: dimensionId },
        dataType: "json",
        success: function (dimension) {
            $("#dimension-edit-id").val(dimension.Id);
            $("#dimension-edit-name").val(dimension.Name);
            $("#dimension-edit-createby").val(dimension.CreatedBy);
            $("#dimension-edit-createdate").val(dimension.CreatedDate);
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function addDimension() {
    if ($("#form_dimension_add").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveDimension',
            data: { Name: $('#addnew-dimension-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result == true) {
                    if (response.actionVal == 1) {
                        cleanBookNotification.createSuccess();
                        reloadDimemssion();
                        $('#app-dimension-add').modal('toggle');
                    }
                    else if (response.actionVal == 2) {

                        cleanBookNotification.updateSuccess();
                        reloadDimemssion();
                        $('#app-dimension-edit').modal('toggle');
                    }
                    else if (response.actionVal == 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            ResetFormControl('form_dimension_add');
        });
    }
}
function updateDimension() {
    if ($("#form_dimension_edit").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveDimension',
            data: { Id: $('#dimension-edit-id').val(), Name: $('#dimension-edit-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result == true) {
                    if (response.actionVal == 1) {
                        $('#app-dimension-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        reloadDimemssion();
                    } else if (response.actionVal == 2) {
                        $('#app-dimension-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        reloadDimemssion();
                    } else if (response.actionVal == 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            ResetFormControl('form_dimension_add');
        });
    }
}
function ConfirmDeleteDimension(id, name) {
    $('#label-confirm-dimension').text("Do you want delete dimension: " + name);
    $('#id-itemdimension-delete').val(id);
}
function deleteDimension() {
    $.ajax({
        type: 'delete',
        url: '/Bookkeeping/DeleteDimension',
        data: { id: $('#id-itemdimension-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response == "OK") {
                cleanBookNotification.removeSuccess();
                reloadDimemssion();
            } else if (response == "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
}

reloadDimemssion = function(){
    setTimeout(function () {
        $('#comfig-content').load('/TraderConfiguration/TraderConfigurationContent?value=dimension');
    }, 500);
}
// end dimension functions