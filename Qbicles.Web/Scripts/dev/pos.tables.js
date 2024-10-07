
function initSaveTableFrm() {
    $addedittablefrm = $("#addedittableform");
    $addedittablefrm.validate({
        rules: {
            name: {
                required: true,
            }
        }
    });

    $addedittablefrm.submit(function (e) {
        if ($addedittablefrm.valid()) {
            e.preventDefault();
            var tableName = $("#table-name").val();
            var tableSummary = $("#table-summary").val();
            var _url = "/PosTable/AddEditPosTable";
            var posTable = {
                'Id': $("#postableid").val(),
                'Name': tableName,
                'Summary': tableSummary
            };
            LoadingOverlay();
            $.ajax({
                type: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    'posTable': posTable
                },
                success: function (response) {
                    if (response.result) {
                        $("#app-trader-pos-table-add").modal("hide");
                        $(".modal-backdrop")[0].remove();
                        if (posTable.Id > 0) {
                            cleanBookNotification.success("Edited PosTable successfully!", "Qbicles");
                        } else {
                            cleanBookNotification.success("Added PosTable successfully!", "Qbicles");
                        }
                        ShowSubPOSSetting('Tables');
                        
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                        
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                    
                }

            }).always(function () {
                LoadingOverlayEnd();
            });
        }
    })
}

function showAddEditTable(postableid) {
    var _url = "/PosTable/ShowAddEditTableModal?posTableId=" + postableid;
    $("#app-trader-pos-table-add").empty();
    $("#app-trader-pos-table-add").load(_url);
    $("#app-trader-pos-table-add").modal("show");
}

function deletePosTable(tableId) {
    var _url = "/PosTable/DeletePosTable?posTableId=" + tableId;
    LoadingOverlay();
    $.ajax({
        type: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Delete table successfully!", "Qbicles");
                ShowSubPOSSetting('Tables');
            
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, 'Qbicles');
            
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

// Table Layout functions
function initSaveTableLayoutFrm() {
    var $tablelayoutfrm = $("#table-layout-form");
    $tablelayoutfrm.validate({
        rules: {
            file: {
                required: true
            }
        }
    });

    $tablelayoutfrm.submit(function (e) {
        e.preventDefault();
        if ($tablelayoutfrm.valid()) {
            //Process with uploading image
            var files = document.getElementById("layoutImageUri").files;

            if (files && files.length > 0) {
                UploadMediaS3ClientSide("layoutImageUri").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize,
                        };
                        SavePosTableLayout(s3Object);
                    }
                });
            } else {
                return;
            }

            //End
        }
    });
}

function SavePosTableLayout(uploadModel) {
    //Get data
    var currentLocationId = $("#pos-location-id").val();

    var _url = "/PosTable/SavePosTableLayout";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            uploadModel: uploadModel,
            locationId: currentLocationId
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $(".modal-backdrop")[0].remove();
                ShowSubPOSSetting('Tables');
                $("#app-trader-pos-diagram-edit").modal("hide");
                
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            $("#app-trader-pos-diagram-edit").modal("hide");
            
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function DeleteTableLayout() {
    var currentLocationId = $("#pos-location-id").val();
    var _url = "/PosTable/DeletePosTableLayout?locationId=" + currentLocationId;
    LoadingOverlay();
    $.ajax({
        type: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Remove Tables Layout successfully!", "Qbicles");
                ShowSubPOSSetting('Tables');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}