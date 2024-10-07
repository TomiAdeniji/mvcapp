var LinkCountPost = 1, LinkCountMedia = 1, busycomment = false;
var $downloadUri = "";
var $fileVersionId = 0;
var $select = document.querySelector('#version-file');
function updateFolderForMedia(mediaKey) {
    var folderId = $('#in-folder').val();
    $.ajax({
        url: "/Medias/UpdateFolderForMedia",
        data: { mediaKey: mediaKey, folderId: folderId },
        type: "POST",
        success: function (result) {
            if (!result) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_90"), "Qbicles");
            }
        },
        error: function (error) {
            cleanBookNotification.error(error.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function validateAddComment() {
    var message = $('#comment-media').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function addCommentToMedia(mediaKey) {
    if (busycomment)
        return;

    var message = $('#comment-media');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
    isPlaceholder(true, '#list-comments-media');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentToMedia",
            data: { message: message.val(), mediaKey: mediaKey },
            type: "POST",
            success: function (response) {
                if (response.result) {
                    message.val("");

                    if (response.msg != '') {
                        $('#list-comments-media').prepend(response.msg);
                        isDisplayFlicker(false);
                    }
                }
                busycomment = false;
            },
            error: function (error) {
                isPlaceholder(false, '');
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                busycomment = false;
            }
        });
    }
}
function LoadMoreMedias(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: LinkCountMedia * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            LinkCountMedia = LinkCountMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function LoadMorePosts(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: LinkCountPost * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadPosts').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            LinkCountPost = LinkCountPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
$(document).ready(function () {
    // set name,logo for Qbicle selected  

    setTimeout(function () {
        $(window).scrollTop(0);
    }, 1000);

    if ($('#version-file option').size() >= 0) {
        SelectVersion();
    };

    removeVersion = function () {
        var versionfilesCount = document.getElementById("version-file").length;
        if (versionfilesCount === 1) {
            cleanBookNotification.error(_L("ERROR_MSG_320"), "Qbicles");
            return;
        }
        $.ajax({
            url: "/Medias/DeleteVersionFile",
            data: { versionFileId: $fileVersionId },
            type: "GET",
            dataType: "json",
            async: false,
        }).done(function (refModel) {
            if (refModel.result) {
                cleanBookNotification.removeSuccess();
                $("#version-file option[value='" + $fileVersionId + "']").remove();
                $select.value = $("#version-file").find("option:first-child").val();
                $select.dispatchEvent(new Event('change'));
            } else {
                cleanBookNotification.error("Error remove: " + refModel.msg, "Qbicles");
            }
        }).fail(function (error) {
            cleanBookNotification.error(error, "Qbicles");
            cleanBookNotification.error(error.responseText, "Qbicles");
        });

    };

});

function SelectVersion() {
    var ajaxUri = '/Qbicles/MediaReview?id=' + $("#version-file").val();
    $('#preview .spacing-new').LoadingOverlay("show");
    $('#preview .spacing-new').load(ajaxUri, function () {
        window.scrollTo(0, 0);
        $fileVersionId = $("#version-file").val();
        $('#preview .spacing-new').LoadingOverlay("hide", true);
    });
}

uploadVersion = function () {
    ClearFileVersion();
    $('#create-file-version').modal('toggle');
};
function ClearFileVersion() {
    $("#inputFileVersion").val("");
    $("#processPicture").val("");
    $('#processPicture').attr('src', "https://via.placeholder.com/300x250?text=no+image+selected (300x250)");
    ClearError();
};
function pictureChange(sender) {
    readURLVersionFile(sender, "processPicture");
};
function readURLVersionFile(input, pictureboxId) {
    if (input.files && input.files[0]) {
        var typeIsvalid = checkfile($("#inputFileVersion").val());
        if (typeIsvalid.stt === false) {
            cleanBookNotification.error("Error detail: " + typeIsvalid.err, "Qbicles");
            ClearFileVersion();
            return;
        }
        var fileExtenssion = input.files[0].name.replace(/^.*\./, '');
        var $inputFile = $('#' + pictureboxId);
        var img = "";
        switch (fileExtenssion) {
            case "doc":
                img = "/Content/DesignStyle/img/media-item-doc.jpg";
                break;
            case "docx":
                img = "/Content/DesignStyle/img/media-item-docx.jpg";
                break;
            case "gif":
                img = "/Content/DesignStyle/img/media-item-gif.jpg";
                break;
            case "pdf":
                img = "/Content/DesignStyle/img/media-item-pdf.jpg";
                break;
            case "ppt":
                img = "/Content/DesignStyle/img/media-item-ppt.jpg";
                break;
            case "pptx":
                img = "/Content/DesignStyle/img/media-item-pptx.jpg";
                break;
            case "xls":
                img = "/Content/DesignStyle/img/media-item-xls.jpg";
                break;
            case "xlsx":
                img = "/Content/DesignStyle/img/media-item-xlsx.jpg";
                break;
            case "zip":
                img = "/Content/DesignStyle/img/media-item-zip.jpg";
                break;
        }
        if (img !== "") {
            $inputFile.attr('src', img);
            return;
        }
        var reader = new FileReader();
        reader.onload = function (e) {

            $inputFile.attr('src', e.target.result);
            //valid file type
            var file = input.files[0];

            if (!file)
                return;

            var formData = new FormData();
            formData.append("file", file);
        }
        reader.readAsDataURL(input.files[0]);
    }
};
function ValidateMedia() {
    var valid = true;
    if ($("#media_edit_name").val() === "") {
        valid = false;
        $("#form_media_edit").validate().showErrors({ title: "This field is required." });
    }
    if ($("#media_edit_description").val() === "") {
        valid = false;
        $("#form_media_edit").validate().showErrors({ Description: "This field is required." });
    }
    return valid;
}
function updateMedia() {
    if (ValidateMedia()) {

        var media =
        {
            Key: $('#media_edit_key').val(),
            Description: $('#media_edit_description').val(),
            Name: $('#media_edit_name').val(),
            TopicId: $('#media-edit-topic').val()
        }

        $.ajax({
            type: 'post',
            url: '/Medias/UpdateMedia',
            datatype: 'json',
            data: media,
            success: function (refModel) {
                if (refModel.result) {
                    $('#media-edit').modal('hide');
                    location.reload();
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }
        });
    }
};


function SaveFileVersion() {
    if ($('#form_file_version').valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("inputFileVersion").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("inputFileVersion").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    $("#media-version-object-key").val(mediaS3Object.objectKey);
                    $("#media-version-object-name").val(mediaS3Object.fileName);
                    $("#media-version-object-size").val(mediaS3Object.fileSize);

                    SaveVersion();
                }
            });


        }
    }
};
function SaveVersion() {
    $.ajax({
        type: "post",
        cache: false,
        url: "/Medias/SaveVersionFile",
        enctype: 'multipart/form-data',
        data: new FormData($("#form_file_version")[0]),
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.result) {
                $('#create-file-version').modal('hide');
                cleanBookNotification.updateSuccess();
                $('#version-file').prepend($("<option id='version-" + data.Object.Id + "' value='" + data.Object.Id + "'>" + data.Object.UploadedDate + "</option>"));
                $select.value = data.Object.Id;
                $select.dispatchEvent(new Event('change'));
                $('#media-vs-count').text($('#version-file option').length);
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            cleanBookNotification.error(error.responseText, "Qbicles");
        }

    }).always(function () {
        LoadingOverlayEnd();
    });
};