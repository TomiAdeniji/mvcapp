//var isBusyAddmediaForm = false;
//var $form_media_addedit = $("#form_media_addedit");
//function ClearMediaInputModalAddEdit() {
//    $('#mediaTitle').val("");
//    $('#mediaAttachments').val("");
//    $('#mediaDescription').val("");
//    $('#media-preview').hide();
//    ClearError();
//}
//function AddNewFileClick() {
//    ClearMediaInputModalAddEdit();
//}
//function SaveMedia() {
//    if (isBusyAddmediaForm) {
//        return;
//    }
   
//    if ($form_media_addedit.valid()) {
//        $.LoadingOverlay("show");
//        $.ajax({
//            url: "/Medias/DuplicateMediaNameCheck",
//            data: { cubeId: $('#mediaQbicleId').val(), mediaId: $mediaId, MediaName: $('#mediaTitle').val() },
//            type: "GET",
//            dataType: "json"
//        }).done(function (refModel) {
//            if (refModel.result)
//                $form_media_addedit.validate()
//                    .showErrors({ Name: "Name of Media already exists in the current Qbicle." });
//            else {
//                    if ($('#mediaAttachments').val()) {
//                        var typeIsvalid = checkfile($('#mediaAttachments').val());
//                        if (typeIsvalid.stt) {
//                            $form_media_addedit.trigger("submit");
//                        } else {
//                            $form_media_addedit.validate().showErrors({ mediaAttachments: typeIsvalid.err });
//                        }
//                    } else {
//                        $form_media_addedit.trigger("submit");
//                    }
//            }
//            LoadingOverlayEnd();
//        }).fail(function (err) {
//            console.log(err);
//            $form_media_addedit.validate()
//                .showErrors({ Name: "Error checking existing name of Media in the current Qbicle" });
//            LoadingOverlayEnd();
//        });
//    }
//}
//$(document).ready(function () {
//    $form_media_addedit.validate({
//        rules: {
//            Name: {
//                required: true,
//                maxlength: 250
//            },
//            mediaAttachments: {
//                filesize: true
//            },
//            Description: {
//                maxlength: 500
//            }
//        }
//    });
//    $form_media_addedit.submit(function (e) {
//        $.LoadingOverlay("show");
//        e.preventDefault();
//        if ($("#dashboard-page-display").length > 0)
//            isDisplayFlicker(true);
//        else
//            isPlaceholder(true, '#list-medias');
//        $.ajax({
//            type: this.method,
//            cache: false,
//            url: this.action,
//            enctype: 'multipart/form-data',
//            data: new FormData(this),
//            processData: false,
//            contentType: false,
//            beforeSend: function (xhr) {
//                isBusyAddmediaForm = true;
//            },
//            success: function (data) {
//                if (data.result) {
//                    $('#create-media').modal('toggle');
//                    isBusyAddmediaForm = false;
//                    ClearMediaInputModalAddEdit();
//                    if (data.Object.topic)
//                        addTopicToFilter(data.Object.topic.Id, data.Object.topic.Name);
//                    if (data.Object.ImgPath)
//                        generateMediaForActivityPage(data.Object, false);
//                    if ($('#block-media-list').length) {
//                        $('#select-list-mediaFolder').val($('#add-media-select-media-folder').val()).change();
//                    }
//                    if ($("ul.subapps-nav li.active a.link-media").length > 0) {
//                        $('#media-folder-qbicle').val($('#add-media-select-media-folder').val()).trigger('change');
//                    }
//                } else if (!data.result && data.msg) {
//                    cleanBookNotification.error(data.msg, "Qbicles");
//                }
//                LoadingOverlayEnd();
//            },
//            error: function (data) {
//                isBusyAddmediaForm = false;
//                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
//                isDisplayFlicker(false);
//                LoadingOverlayEnd();
//                
//            }

//        });
//    });
//});
