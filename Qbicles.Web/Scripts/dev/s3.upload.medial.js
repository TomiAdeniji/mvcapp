

function UploadQbiclesMedia(uploadFromId) {
    $('#form_media_addedit').LoadingOverlay("show");

    var files = document.getElementById(uploadFromId).files;
    if (files) {
        UploadMediaS3ClientSide(uploadFromId).then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                $('#propertyType-table').LoadingOverlay("hide", true);
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                $('#form_media_addedit').LoadingOverlay("hide", true);
                return;
            }
            else {
                //LoadingOverlayEnd();
                SubmitQbicleMedia(mediaS3Object);
            }
        });
    }
}

function SubmitQbicleMedia(mediaS3Object) {

    var media = {
        Name: $("#mediaTitle").val(),
        Description: $("#mediaDescription").val(),
        Topic: { Id: $("#media-topic-selected").val() }
    };

    var s3ObjectUpload = {
        FileKey: mediaS3Object.objectKey,
        FileName: mediaS3Object.fileName,
        FileType: mediaS3Object.fileType,
        FileSize: mediaS3Object.fileSize
    };
    //Save media activity and process file
    $.ajax({
        type: "post",
        cache: false,
        url: "/QbicleFiles/SaveMedia",
        //enctype: 'multipart/form-data',
        data: {
            media: media,
            s3ObjectUpload: s3ObjectUpload,
            mediaFolderId: parseInt($("#add-media-select-media-folder").val()),
            qbicleId: $('#mediaQbicleId').val(),
            isMediaOntab: $("#add-media-on-tab").val()
        },
        //processData: false,
        //contentType: false,
        beforeSend: function (xhr) { },
        success: function (data) {
            if (data.result) {
               
                if ($("#dashboard-page-display").length > 0)
                    isDisplayFlicker(true);
                else
                    isPlaceholder(true, '#list-medias');

                if (data.msg != '') {
                    htmlActivityRender(data.msg, 0);
                    $('#list-medias').prepend(data.msg);
                    isDisplayFlicker(false);
                }
                $('#create-media').modal('hide');
                ClearMediaInputModalAddEdit();

                if ($('#block-media-list').length) {
                    $('#select-list-mediaFolder').val($('#add-media-select-media-folder').val()).change();
                }
                if ($("ul.subapps-nav li.active a.link-media").length > 0) {
                    $('#media-folder-qbicle').val($('#add-media-select-media-folder').val()).trigger('change');
                }
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {

            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isDisplayFlicker(false);
        }

    }).always(function () {
        $('#propertyType-table').LoadingOverlay("hide", true);
        $('#form_media_addedit').LoadingOverlay("hide", true);
    });
}

function SaveMedia() {

    var $formMediaAddEdit = $("#form_media_addedit");

    if ($formMediaAddEdit.valid()) {
        $('#form_media_addedit').LoadingOverlay("show");
        $.ajax({
            url: "/Medias/DuplicateMediaNameCheck",
            data: {
                qbicleId: $('#mediaQbicleId').val(),
                mediaId: $mediaId,
                MediaName: $('#mediaTitle').val()
            },
            type: "GET",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result) {
                $formMediaAddEdit.validate()
                    .showErrors({ Name: "Name of Media already exists in the current Qbicle." });
                $('#form_media_addedit').LoadingOverlay("hide", true);
            }
            else {
                if ($('#mediaAttachments').val()) {
                    var typeIsvalid = checkfile($('#mediaAttachments').val());
                    if (typeIsvalid.stt) {

                        UploadQbiclesMedia("mediaAttachments");

                    } else {
                        $('#form_media_addedit').LoadingOverlay("hide", true);
                        $formMediaAddEdit.validate().showErrors({ mediaAttachments: typeIsvalid.err });
                    }
                }
            }
        }).fail(function (err) {
            $formMediaAddEdit.validate()
                .showErrors({ Name: "Error checking existing name of Media in the current Qbicle" });
        });
    }
}
function AddNewFileClick() {
    ClearMediaInputModalAddEdit();
}
function ClearMediaInputModalAddEdit() {
    $('#mediaTitle').val("");
    $('#mediaAttachments').val("");
    $('#mediaDescription').val("");
    ClearError();
}
