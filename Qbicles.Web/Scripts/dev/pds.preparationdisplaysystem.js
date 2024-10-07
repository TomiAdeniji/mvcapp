
var $locationManager = $('#location-manager').val();

$("#pds-search").on("keyup", function () {
    if ($('#pds-search').val().length > 3 || $('#pds-search').val().length === 0) {
        var ajaxUri = '/PDS/SearchPrepDisplayDevice?name=' + $('#pds-search').val();
        AjaxElementLoad(ajaxUri, "pds-list");
    }
});


PrepDisplayDeviceAddEdit = function (id) {
    var ajaxUri = '/PDS/AddEditPrepDisplayDevice?id=' + id;
    AjaxElementShowModal(ajaxUri, "app-trader-pds-add-edit");
};

function SavePrepDisplayDevice() {
    var pdsAdmins = $('#pds-admin').val();
    if (pdsAdmins === null || typeof pdsAdmins === 'undefined') {
        cleanBookNotification.error("PDS required least one Administrator!", "Qbicles");
        return;
    }
    var pdsUsers = $('#device-user').val();
    if (pdsUsers === null || typeof pdsUsers === 'undefined') {
        cleanBookNotification.error("PDS required least one User!", "Qbicles");
        return;
    }

    if ($("#pds-name").val() === "") {
        $("#pds-form").validate().showErrors({ pdsname: "This field is required." });
        return;
    }
    //if (!$("#pds-form").valid()) return;
    $.LoadingOverlay("show");
    var $id = $("#pds-id").val();
    var listCategoryExclution = $("#pds-assocsets").val();
    var prepDisplayDevice = {
        Id: $id,
        Name: $("#pds-name").val(),
        Queue: {
            Id: $("#pds-queue").val()
        },
        Type: {
            Id: $("#ods-device-type").val()
        },
        Administrators: [],
        Users: [],
        CategoryExclusionSets: []
    };
    for (var i = 0; i < pdsAdmins.length; i++) {
        prepDisplayDevice.Administrators.push({
            Id: pdsAdmins[i]
        });
    }
    for (var j = 0; j < pdsUsers.length; j++) {
        prepDisplayDevice.Users.push({
            Id: pdsUsers[j]
        });
    }
    if(listCategoryExclution != null){
        for (var z = 0; z< listCategoryExclution.length; z++){
            prepDisplayDevice.CategoryExclusionSets.push({
                Id: listCategoryExclution[z]
            })
        }
    }

    var url = "/PDS/CreatePrepDisplayDevice";
    if ($id > 0)
        url = "/PDS/UpdatePrepDisplayDevice";

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { prepDisplayDevice: prepDisplayDevice },
        success: function (rs) {
            if (rs.actionVal === 1) {
                if ($id > 0) {
                    $("#pds-type-main-" + $id).text($("#ods-device-type option:selected").text());
                    $("#pds-queue-name-main-" + $id).text(rs.Object.Name);
                    $("#pds-queue-name-main-" + $id).text(rs.Object.Queue);
                    $("#pds-admins-name-main-" + $id).text(rs.Object.Admins);
                }
                else {
                    $('#pds-list').append(rs.msg);
                }

                $('#app-trader-pds-add-edit').modal('hide');

                cleanBookNotification.createSuccess();
            } else {
                $("#pds-form").validate().showErrors({ pdsname: rs.msg });
            }
            showSubPOSDevice('pds-device', 'tab-pds-device');
            //reload 
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        $('.modal-backdrop.fade.in').remove();
    });
};

var $prepDisplayDeviceIdDelete = 0;
function ConfirmDeletePrepDisplayDevice(id) {
    $prepDisplayDeviceIdDelete = id;
    $("#name-delete").text($("#pds-name-main-" + $prepDisplayDeviceIdDelete).text());
    $("#confirm-delete").modal('show');
}

function updateOrderDisplayRefresh(ev) {
    var orderDisplayRefreshInterval = $(ev).val();
    if (!isNaN(parseFloat(orderDisplayRefreshInterval)) && parseFloat(orderDisplayRefreshInterval) < 0) {
        orderDisplayRefreshInterval = 0;
        $(ev).val(0);
    }
    var setting = {
        Id: $('#setting_id').val(),
        OrderDisplayRefreshInterval: orderDisplayRefreshInterval,
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveOrderDisplayRefreshSetting";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { setting: setting },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }

        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function CancelDelete() {
    $('#confirm-delete').modal('hide');
};
function DeletePrepQueue() {
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/PDS/DeletePrepDisplayDevice",
        data: { id: $prepDisplayDeviceIdDelete },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                $("#pds-item-" + $prepDisplayDeviceIdDelete).remove();
                $('#confirm-delete').modal('hide');
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error delete menu, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};
