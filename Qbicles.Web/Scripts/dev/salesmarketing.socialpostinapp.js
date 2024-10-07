var isBusy = false, $frm_social_post = $("#frm_marketing-social-post-edit"), CountPost = 1, CountMedia = 1,busycomment=false;
function PostApproval(id, campaignId)
{
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DeniedPostApproval',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                window.location.href = "/SalesMarketing/SMSocial?id=" + campaignId;
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_160"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function DenyEmailPost(id, campaignId) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DenyEmailPost',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                window.location.href = "/SalesMarketing/SMEmail?id=" + campaignId;
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_160"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function PostSetApproved(id) {
    $.LoadingOverlay("show");
    $("#button-approval-post").addClass('disabled');
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/PostSetApproved',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_185"), "Sales Marketing");
                location.reload();
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_186"), "Sales Marketing");
            }
        },
        error: function (err) {
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function ApproveEmailPost(id) {
    $.LoadingOverlay("show");
    $("#button-approval-post").addClass('disabled');
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/ApproveEmailPost',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_185"), "Sales Marketing");
                location.reload();
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_186"), "Sales Marketing");
            }
        },
        error: function (err) {
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function isVideo(filename) {
    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'mp4':
        case 'webm':
        case 'ogv':
            return true;
    }
    return false;
}
function getExtension(filename) {
    var parts = filename.split('.');
    return parts[parts.length - 1];
}
function AttachChange(el, isload) {
    $('#video-preview-edit').attr('src', '');
    if ($(el).val() === '1') {
        if (isload) {
            LoadMediaByCampaign();
            $('.newpreview').hide();
        }
        $('.campaign-resource-picker').show();
        $('.uploadnew').hide();
        $('.previewimgpost').val('').change();
    } else {
        $('.campaign-resource-picker').hide();
        $('.uploadnew').show();
        $('#ImageOrVideo').val(0);
        $('#ImageOrVideoEdit').val(0);
    }
}
function isFileImage(file) {
    file = file.files[0];
    const acceptedImageTypes = ['image/gif', 'image/jpeg', 'image/png', 'image/jpg', ];
    return file && $.inArray(file['type'], acceptedImageTypes);
}
function chooseMediaEdit(el, id, link, isUse, isvideo) {
    if (isUse) {
        $(el).hide();
        $('.changee' + id).show();
        $('.othere').hide();
        $('#rse-' + id).show();
        $('#ImageOrVideoEdit').val(id);
        if (isvideo) {
            $('.preview-post-edit').attr('src', '');
            $('#video-preview-edit').attr('src', link);
        } else {
            $('.preview-post-edit').attr('src', link);
            $('#video-preview-edit').attr('src', '');
        }

    } else {
        $(el).hide();
        $('.usethemee' + id).show();
        $('.othere').show();
        $('#ImageOrVideoEdit').val(0);
        if (isvideo) {
            $('.preview-post-edit').attr('src', '');
            $('#video-preview').attr('src', link);
        } else {
            $('.preview-post-edit').attr('src', link);
            $('#video-preview-edit').attr('src', '');
        }
    }
}
function validateAddComment() {
    var message = $('#txt-comment-link').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function AddCommentToSocialPostApproval(socialPostKey) {
    if (busycomment)
        return;
    var message = $('#txt-comment-link');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-approval');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentToSocialPost",
            data: { message: message.val(), socialPostKey: socialPostKey },
            type: "POST",
            success: function (result) {
                if (result) {
                    message.val("");
                }
                busycomment = false;
            },
            error: function (error) {
                isPlaceholder(false, '');
                busycomment = false;
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        });
    }
}
function LoadMorePostsApproval(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: CountPost * pageSize
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
            $('#' + divId).append(response).fadeIn(250);
            CountPost = CountPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function LoadMoreMediasApproval(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: CountMedia * pageSize
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
            $('#' + divId).append(response).fadeIn(250);
            CountMedia = CountMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });
}
$(document).ready(function () {
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $frm_social_post.validate({
        rules: {
            Title: {
                required: true,
                minlength: 5
            },
            Content: {
                required: true
            }
        }
    });
   
});


function ProcessSocialPost(modalId, formId) {
    var $frmSocialPost = $("#" + formId);
    if ($frmSocialPost.valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("sm-social-post-feature-upload-media").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("sm-social-post-feature-upload-media").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd('hide');
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }



                $("#sm-social-post-feature-image-object-key").val(mediaS3Object.objectKey);
                $("#sm-social-post-feature-image-object-name").val(mediaS3Object.fileName);
                $("#sm-social-post-feature-image-object-size").val(mediaS3Object.fileSize);

                SubmitSocialPost(modalId, formId);

            });

        }
        else {
            $("#sm-social-post-feature-image-object-key").val("");
            $("#sm-social-post-feature-image-object-name").val("");
            $("#sm-social-post-feature-image-object-size").val("");
            SubmitSocialPost(modalId, formId);
        }


    }
};
SubmitSocialPost = function (modalId, formId) {
    var $frmSocialPost = $("#" + formId);
    var frmData = new FormData($frmSocialPost[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveSocialPost",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#' + modalId).modal('hide');
                isBusy = false;
                cleanBookNotification.success(_L("ERROR_MSG_174"), "Sales Marketing");
                $frmSocialPost.trigger("reset");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");

            }

        },
        error: function (data) {

            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }

    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });


};


function ProcessMaualSocialPost(modalId, formId) {
    var $frmSocialPost = $("#" + formId);
    if ($frmSocialPost.valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("sm-social-post-feature-upload-media").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("sm-social-post-feature-upload-media").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd('hide');
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }


                $("#sm-social-post-feature-image-object-key").val(mediaS3Object.objectKey);
                $("#sm-social-post-feature-image-object-name").val(mediaS3Object.fileName);
                $("#sm-social-post-feature-image-object-size").val(mediaS3Object.fileSize);

                SubmitManualSocialPost(modalId, formId);

            });


        }
        else {
            $("#sm-social-post-feature-image-object-key").val("");
            $("#sm-social-post-feature-image-object-name").val("");
            $("#sm-social-post-feature-image-object-size").val("");
            SubmitManualSocialPost(modalId, formId);
        }


    }
};
SubmitManualSocialPost = function (modalId, formId) {
    var $frmSocialPost = $("#" + formId);
    var frmData = new FormData($frmSocialPost[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveSocialPost",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#' + modalId).modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_174"), "Sales Marketing");
                $frmSocialPost.trigger("reset");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }

        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            
        }

    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
};