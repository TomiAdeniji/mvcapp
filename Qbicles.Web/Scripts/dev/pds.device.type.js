
var $locationManager = $('#location-manager').val();


SearchOdsDeviceType = function () { 
      var ajaxUri = '/PDS/SearchOdsDeviceTypeName?name=' + $('#ods-device-type-search').val();
    if ($("#ods-order-type-search").val() !== '-1')
        ajaxUri = '/PDS/SearchOdsDeviceTypeAndName?name=' + $('#ods-device-type-search').val() + "&orderTypeId=" + $("#ods-order-type-search").val();
    AjaxElementLoad(ajaxUri, "ods-device-types-content");
};


OdsDeviceTypeAddEdit = function (id) {
    var ajaxUri = '/PDS/AddEditOdsDeviceType?id=' + id;
    AjaxElementShowModal(ajaxUri, "app-trader-pos-device-type-add-edit");
};





























function SaveOdsDeviceType() {

    //ods-devie-type-form
    if (!$('#ods-devie-type-form').valid())
        return;

    var types = $('#ods-devie-type-type').val();
    if (types === null || typeof types === 'undefined') {
        cleanBookNotification.error(_L("ERROR_VALUE_REQUIRED", ["Type(s)"]), "Qbicles");

        return;
    }
    var status = $('#ods-devie-type-status').val();
    if (status === null || typeof status === 'undefined') {
        cleanBookNotification.error(_L("ERROR_VALUE_REQUIRED", ["Status(es)"]), "Qbicles");
        return;
    }

    if ($("#ods-devie-type-name").val() === "") {
        cleanBookNotification.error(_L("ERROR_VALUE_REQUIRED", ["Name"]), "Qbicles");
        return;
    }

    $.LoadingOverlay("show");
    var $id = $("#ods-device-type-id").val();
    var odsDeviceType = {
        Id: $id,
        Name: $("#ods-devie-type-name").val(),
        Queue: {
            Id: $("#pds-queue").val()
        },
        AssociatedOrderTypes: [],
        OrderStatus: []
    };
    for (var i = 0; i < types.length; i++) {
        odsDeviceType.AssociatedOrderTypes.push({
           Id: types[i]
        }            
        );
    }
    for (var j = 0; j < status.length; j++) {
        odsDeviceType.OrderStatus.push(
            { Status: status[j]}
        );
    }

    var url = "/PDS/CreateOdsDeviceType";
    if ($id > 0)
        url = "/PDS/UpdateOdsDeviceType";

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { odsDeviceType: odsDeviceType },
        success: function (rs) {
            
            if (rs.result) {
                
                var ajaxUri = "/PDS/SearchOdsDeviceTypeName?name=";
                
                AjaxElementLoad(ajaxUri, "ods-device-types-content");

                $('#app-trader-pos-device-type-add-edit').modal('hide');

                cleanBookNotification.createSuccess();
            } else {
                $("#ods-devie-type-form").validate().showErrors({ namedevicetype: rs.msg });
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

var $OdsDeviceTypeIdDelete = 0;
function ConfirmOdsDeviceTypeDelete(id) {
    
    $OdsDeviceTypeIdDelete = id;
    $("#name-pds-device-type-delete").text($("#ods-device-type-name-" + $OdsDeviceTypeIdDelete).val());
    $("#confirm-pds-device-type-delete").modal('toggle');
}


function CancelDelete() {
    $('#confirm-pds-device-type-delete').modal('hide');
};
function OdsDeviceTypeDelete() {
   
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/PDS/DeleteOdsDeviceType",
        data: { id: $OdsDeviceTypeIdDelete },
        dataType: "json",
        success: function (response) {            
            if (response.result) {
                $("#ods-device-type-row-id-" + $OdsDeviceTypeIdDelete).remove();
                $('#confirm-pds-device-type-delete').modal('hide');
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
