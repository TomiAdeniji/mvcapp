var quillEditor, isBusy = false;
$(document).ready(function () {

    $('#headlinefont').change(function () {
        $('#headline').css('font-family', $(this).val());
    });

    $('#bodyfont').change(function () {
        $('#bodypreview p').css('font-family', $(this).val());
    });

    $('#buttonfont').change(function () {
        $('#button').css('font-family', $(this).val());
    });
    var Block = Quill.import('blots/block');
    Block.tagName = 'div';
    Quill.register(Block);
    quillEditor = new Quill('#editor', {
        modules: {
            toolbar: '#toolbar-container'
        },
        placeholder: 'Enter your email\'s main content here...',
        theme: 'snow'
    });
    quillEditor.on('text-change', function (delta, oldDelta, source) {
        var html = quillEditor.root.innerHTML;
        $('#bodypreview p').html(html);
    })
    initFormBuilder();
    $(".previewimg").change(function () {
        var target = $(this).data('target');
        readURL(this, target);
        $(target).fadeIn();
    });
});
function initFormBuilder() {
    var $frmFormBuilder = $('#frmFormBuilder');
    $frmFormBuilder.validate({
        ignore: ".ql-container *",
        rules: {
            TemplateName: {
                required: true,
                maxlength: 150
            },
            TemplateDescription: {
                required: true,
                maxlength: 350
            }
        },
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $frmFormBuilder.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if (!$(this).valid()) {
            return;
        } else {
            var _bodycontent = quillEditor.root.innerHTML;
            if (_bodycontent == "<p><br></p>") {
                cleanBookNotification.error(_L("ERROR_MSG_176"), "Sales Marketing");
                return;
            }
            //Validate Featured Image
            if (!$('#FeaturedImage').val() && !$('#FileFeaturedImg').val()) {
                cleanBookNotification.error(_L("ERROR_MSG_817"), "Sales Marketing");
                return;
            }
            //Validate Advertiserment
            if ($('input[name=AdvertImgiIsHidden]').prop('checked') && !$('#AdvertImage').val() && !$('#FileAdvImg').val()) {
                cleanBookNotification.error(_L("ERROR_MSG_816"), "Sales Marketing");
                return;
            }
            $.LoadingOverlay("show");
            var fileFeaturedImg = document.getElementById("FileFeaturedImg").files;
            var fileAdvImg = document.getElementById("FileAdvImg").files;

            if (fileFeaturedImg && fileFeaturedImg.length > 0 && fileAdvImg && fileAdvImg.length > 0) {
                Promise.all([UploadMediaS3ClientSide("FileFeaturedImg"), UploadMediaS3ClientSide("FileAdvImg")]).then(mediaS3Object => {
                    $("#sm-email-builder-feature-object-key").val(mediaS3Object[0].objectKey);
                    $("#sm-email-builder-feature-object-name").val(mediaS3Object[0].fileName);
                    $("#sm-email-builder-feature-object-size").val(mediaS3Object[0].fileSize);

                    $("#sm-email-builder-adv-object-key").val(mediaS3Object[1].objectKey);
                    $("#sm-email-builder-adv-object-name").val(mediaS3Object[1].fileName);
                    $("#sm-email-builder-adv-object-size").val(mediaS3Object[1].fileSize);
                    SaveEmailBuilder();
                }).catch(reason => {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                });
            }
            else if (fileFeaturedImg && fileFeaturedImg.length > 0 && !fileAdvImg && fileAdvImg.length >= 0) {
                UploadMediaS3ClientSide("FileFeaturedImg").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#sm-email-builder-feature-object-key").val(mediaS3Object.objectKey);
                        $("#sm-email-builder-feature-object-name").val(mediaS3Object.fileName);
                        $("#sm-email-builder-feature-object-size").val(mediaS3Object.fileSize);
                        SaveEmailBuilder();
                    }
                });
            }
            else if (!fileFeaturedImg && fileFeaturedImg.length <= 0 && fileAdvImg && fileAdvImg.length > 0) {
                UploadMediaS3ClientSide("FileAdvImg").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {

                        $("#sm-email-builder-adv-object-key").val(mediaS3Object.objectKey);
                        $("#sm-email-builder-adv-object-name").val(mediaS3Object.fileName);
                        $("#sm-email-builder-adv-object-size").val(mediaS3Object.fileSize);
                        SaveEmailBuilder();
                    }
                });
            }
            else
                SaveEmailBuilder();
        }
    });
}



function SaveEmailBuilder() {

    $('#BodyContent').val($(quillEditor.root.innerHTML).html());
    var frmData = new FormData($('#frmFormBuilder')[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveEmailBuilder",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                isBusy = false;
                location.href = data.Object;
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
            LoadingOverlayEnd();
        }
    });
};









function changeResourseType(elm, type) {
    if ($(elm).val() == '1') {
        if (type == 'FeaturedImage') {
            $('.campaign-resource-picker').show();
            $('.uploadnew').hide();
            loadResources('Choose-campaign-resources');

        } else {
            $('.campaign-resource-picker-ad').show();
            $('.uploadnew-ad').hide();
            loadResources('Choose-campaign-resources-ad');
        }
    } else {
        if (type == "FeaturedImage") {
            $('.campaign-resource-picker').hide();
            $('.uploadnew').show();
        } else {
            $('.campaign-resource-picker-ad').hide();
            $('.uploadnew-ad').show();
        }

    }
    $(elm).valid();
}
function loadResources(elmId) {
    $.LoadingOverlay("show");
    $('#' + elmId).load("/SalesMarketing/LoadImageResources?refdivid=" + elmId, function () {
        LoadingOverlayEnd();
    });
}
function chooseMediaAdd(el, id, link, isUse, refdivid) {
    $('#' + refdivid + ' .change' + id).hide();
    $('#' + refdivid + '.usetheme' + id).hide();
    if (isUse) {
        $(el).hide();
        $('#' + refdivid + ' .change' + id).show();
        $('#' + refdivid + ' .other').hide();
        $('#' + refdivid + ' .rs-' + id).show();
        if (refdivid == 'Choose-campaign-resources') {
            $('#FeaturedImage').val(id);
            $('#fimg').attr('src', link);
        }
        else {
            $('#AdvertImage').val(id);
            $('#adimg').attr('src', link);
        }

    } else {
        $(el).hide();
        $('#' + refdivid + ' .usetheme' + id).show();
        $('#' + refdivid + ' .other').show();
        if (refdivid == 'Choose-campaign-resources') {
            $('#FeaturedImage').val('');
            $('#fimg').attr('src', 'https://via.placeholder.com/600x200?text=Featured+image (600x200px)');
        }
        else {
            $('#AdvertImage').val('');
            $('#adimg').attr('src', 'https://via.placeholder.com/600x200?text=Optional+advert (600x200px)');
        }
        $('html, body').animate({
            scrollTop: $('#' + refdivid).offset().top - 200
        }, 200);
    }
}
function validateAdve(elm) {
    if ($(elm).prop('checked')) {
        $('input[name=AdvertLink]').prop('required', true);
        $('select[name=AdvertImageType]').prop('required', true);
    } else {
        $('input[name=AdvertLink]').prop('required', false);
        $('select[name=AdvertImageType]').prop('required', false);
    }
}
function validateBodyButton(elm) {
    if ($(elm).prop('checked')) {
        $('input[name=ButtonText]').prop('required', true);
        $('input[name=ButtonLink]').prop('required', true);
        $('input[name=ButtonFontSize]').prop('required', true);
        $('input[name=ButtonText]').valid();
        $('input[name=ButtonLink]').valid();
        $('input[name=ButtonFontSize]').valid();
        $('#button').css("display", "inline-block");
    } else {
        $('input[name=ButtonText]').prop('required', false);
        $('input[name=ButtonLink]').prop('required', false);
        $('input[name=ButtonFontSize]').prop('required', false);
        $('input[name=ButtonText]').valid();
        $('input[name=ButtonLink]').valid();
        $('input[name=ButtonFontSize]').valid();
        $('#button').css("display", "none");
    }
}
function validateSocial(elm, refInputName) {
    if ($(elm).prop('checked')) {
        $('input[name=' + refInputName + ']').prop('required', true);
        $('input[name=' + refInputName + ']').valid();
    } else {
        $('input[name=' + refInputName + ']').prop('required', false);
        $('input[name=' + refInputName + ']').valid();
    }
}
function validateAdvImg(elm) {
    if ($(elm).prop('checked')) {
        $('input[name=AdvertLink]').prop('required', true);
        $('input[name=AdvertLink]').valid();
        $('select[name=AdvertImageType]').prop('required', true);
        $('select[name=AdvertImageType]').valid();
    } else {
        $('input[name=AdvertLink]').prop('required', false);
        $('input[name=AdvertLink]').valid();
        $('select[name=AdvertImageType]').prop('required', false);
        $('select[name=AdvertImageType]').valid();
    }
}
function readURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }

}