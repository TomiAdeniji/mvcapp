$(function () {
    var isLinkBusy = false;
    var LinkCountPost = 1, LinkCountMedia = 1, busycomment = false;
    var ElmLink = {
        $frmlink: $('#frm-link'),
        $modallink: $('#create-link'),
        $previewlinkimg: $('.preview-link-img')
    };
    lkFunc = {
        validateAddComment: function () {
            var message = $('#txt-comment-link').val();
            if (message.length > 1500)
                $('#addcomment-error').show();
            else
                $('#addcomment-error').hide();
        },
        AddCommentToLink: function (linkKey) {
            if (busycomment)
                return;
            var message = $('#txt-comment-link');
            if (message.val() && !$('#addcomment-error').is(':visible')) {
                isPlaceholder(true, '#list-comments-link');
                busycomment = true;
                $.ajax({
                    url: "/QbicleComments/AddCommentToLink",
                    data: { message: message.val(), linkKey: linkKey },
                    type: "POST",
                    success: function (response) {
                        if (response.result) {
                            message.val("");

                            if (response.msg != '') {
                                $('#list-comments-link').prepend(response.msg);
                                isDisplayFlicker(false);
                            }
                        }
                        busycomment = false;
                    },
                    error: function (error) {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
                        isPlaceholder(false, '');
                        busycomment = false;
                    }
                });
            }

        },
        LoadMorePosts: function (activityKey, pageSize, divId) {

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

        },
        LoadMoreMedias: function (activityId, pageSize, divId) {
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
    };
    $(document).ready(function () {
        ElmLink.$frmlink.validate(
            {
                rules: {
                    Name: {
                        required: true,
                        minlength: 5
                    },
                    fileFeaturedImage: {
                        filesize: true
                    },
                    URL: {
                        required: true,
                        url: true
                    }
                }
            });
        function readURL(input, target) {

            if (input.files && input.files[0]) {
                var reader = new FileReader();

                reader.onload = function (e) {
                    $(target).attr('src', e.target.result);
                }

                reader.readAsDataURL(input.files[0]);
            }

        }
        ElmLink.$previewlinkimg.change(function () {
            var target = $(this).data('target');
            readURL(this, target);
            $(target).fadeIn();
        });
        ElmLink.$frmlink.submit(function (e) {
            e.preventDefault();
            if (isLinkBusy) {
                return;
            }
            if (ElmLink.$frmlink.valid()) {
                $.LoadingOverlay("show");
                var files = document.getElementById("link-upload-file").files;

                if (files && files.length > 0) {
                    UploadMediaS3ClientSide("link-upload-file").then(function (mediaS3Object) {

                        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                            LoadingOverlayEnd('hide');
                            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                            return;
                        }
                        else {
                            $("#media-object-key").val(mediaS3Object.objectKey);
                            $("#media-object-name").val(mediaS3Object.fileName);
                            $("#media-object-size").val(mediaS3Object.fileSize);
                            SubmitQbicleLink();
                        }
                    });
                } else {
                    SubmitQbicleLink();
                }


            }
        });
    });

    function SubmitQbicleLink() {
        isDisplayFlicker(true);
        var formData = new FormData(document.getElementById("frm-link"));
        $.ajax({
            type: "post",
            url: "/Links/SaveLink",
            data: formData,
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            beforeSend: function (xhr) { isLinkBusy = true; },
            success: function (data) {
                isLinkBusy = false;
                if (data.result) {
                    ElmLink.$modallink.modal('hide');
                    
                    if (data.msg != '') {
                        htmlActivityRender(data.msg, 0);
                        isDisplayFlicker(false);
                    }
                    if ($('#hdlinkKey').val() != "") {
                        cleanBookNotification.success(_L("ERROR_MSG_99"), "Qbicles");
                        location.reload();
                    } else {
                        ElmLink.$frmlink.trigger("reset");
                        ElmLink.$frmlink.validate().resetForm();
                        $('.preview-link').hide();
                        cleanBookNotification.success(_L("ERROR_MSG_100"), "Qbicles");
                    }
                } else if (data.result === false && data.Object)
                    uiEls.$form_topic_addedit.validate().showErrors(data.Object);
                else if (data.result === false && data.msg)
                    cleanBookNotification.error(data.msg, "Qbicles");
                else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

            },
            error: function (data) {
                isLinkBusy = false;
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                isDisplayFlicker(false);
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }
})