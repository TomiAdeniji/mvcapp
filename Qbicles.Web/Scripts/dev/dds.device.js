
function SearchDevice() {
    if ($('#device-search').val().length > 3 || $('#device-search').val().length === 0) {

        var ajaxUri = '/DDS/DdsDevicesSearch?name=' + $('#device-search').val() + "&ddsQueueId=" + $("#device-queue-search").val();
        AjaxElementLoad(ajaxUri, "device-list");
    }
};

DdsDeviceAddEdit = function (id) {
    var ajaxUri = '/DDS/DdsDeviceAddEdit?id=' + id;
    AjaxElementShowModal(ajaxUri, "app-trader-dds-device-add-edit");
};


function SaveDdsDevice() {
    if (!$("#device-form").valid())
        return;

    $.LoadingOverlay("show");
    var $id = $("#device-id").val();
    var $deviceAdmins = $("#device-admin").val();




    if ($deviceAdmins === null || $deviceAdmins === 0) {
        $("#device-form").validate().showErrors({ deviceadmin: "There must be at least one Administrator selected." });
        LoadingOverlayEnd();
        return;
    }

    var url = "/DDS/CreateDdsDevice?adminIds=" + $deviceAdmins + "&userIds=" + $("#device-user").val();
    if ($id > 0)
        url = "/DDS/UpdateDdsDevice?adminIds=" + $deviceAdmins + "&userIds=" + $("#device-user").val();

    var device = {
        Id: $id,
        Name: $("#device-name").val(),
        Type: {
            Id: $("#ods-device-type").val()
        },
        Queue: { Id: $("#device-queue").val() }
    };

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (!rs.result) {
                if (rs.actionVal === 9)
                    $("#device-form").validate().showErrors({ devicename: rs.msg });
                else
                    cleanBookNotification.error(rs.msg, "Qbicles");
                return;
            }

            if ($id > 0) {
                $("#device-name-main-" + $id).text(device.Name);
                $("#device-type-main-" + $id).text($("#ods-device-type option:selected").text());
                $("#device-queue-main-" + $id).text($("#device-queue option:selected").text());
                $("#device-admin-name-" + $id).text($('#device-admin option:selected').toArray().map(item => item.text).join());
                $("#device-user-name-" + $id).text($('#device-admin option:selected').toArray().map(item => item.text).join());
                cleanBookNotification.updateSuccess();
            }
            else {
                $('#device-list').append(rs.msg);
                cleanBookNotification.createSuccess();
            }

            $('#app-trader-dds-device-add-edit').modal('hide');
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


var $deviceIdDelete = 0;
function ConfirmDeleteDevice(id) {
    $deviceIdDelete = id;

    $("#name-delete").text($("#device-name-main-" + $deviceIdDelete).text());
    $("#confirm-delete").modal('show');
};

function CancelDelete() {
    $('#confirm-delete').modal('hide');
};

function DeleteDds() {
    $.LoadingOverlay("show");
    var url = "/DDS/DeleteDdsDevice";

    $.ajax({
        type: "delete",
        url: url,
        data: { id: $deviceIdDelete },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                $("#dds-device-item-" + $deviceIdDelete).remove();
                cleanBookNotification.removeSuccess();
                $('#confirm-delete').modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error delete device, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};