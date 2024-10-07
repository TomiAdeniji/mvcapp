var busycomment = false;

function ChangeImageItem(index, type) {

    UploadMediaS3ClientSide(type + "-" + index).then(function (mediaS3Object) {

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
        }
        else {
            $("#" + type + "-key-" + index).val(mediaS3Object.objectKey);
            $("#" + type + "-name-" + index).val(mediaS3Object.fileName);
            $("#" + type + "-size-" + index).val(mediaS3Object.fileSize);
        }
    });
}

$(function () {
    initAuditForms();
});
function initAuditForms() {
    var $frmAuditForms = $('form[name=audit-form]');
    $.each($frmAuditForms, function (index, value) {
        $(value).validate({
            ignore: "",
        });
    });
    $frmAuditForms.submit(function (e) {
        e.preventDefault();
        if ($(this).valid()) {
            
            $(this).find('.elscore').parent().css('color', '');
            $.LoadingOverlay("show");
            var frmData = new FormData($(this)[0]);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: frmData,
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
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        location.reload();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            $(this).find('.elscore').parent().css('color', '#b84c4c');
        }
    });
    $('.audit-choices li a').bind('click', function (e) {
        e.preventDefault();
        var parent = $(this).closest('nav-pills');
        var field = $(this).parent().parent().parent().find('input.data-val');
        var selection = $(this).data('value');
        var steptitle = $(this).parent().parent().parent().find('p.title');

        if ($('i', steptitle).hasClass('fa-check')) {
            // Do nothing
        } else {
            $(steptitle).append('&nbsp; <i class="fa fa-check green"></i>');
        }

        $(this).parent().siblings().removeClass('active');
        $(field).val(selection);
        $(this).parent().addClass('active');
    });
}
function validateForm(element, type, elmId) {
    if (type == "fa-comment") {
        var _note = $(elmId + ' textarea.ip-note');
        if (_note.valid()) {
            $(elmId).find('.elnote').addClass('fa-check green').removeClass('fa-comment');
            $(element).parent().parent().hide();
            $(elmId).find('.elnote').parent().css('color', '');
        } else {
            $(elmId).find('.elnote').addClass('fa-comment').removeClass('fa-check green');
            $(elmId).find('.elnote').parent().css('color', '#b84c4c');
        }
    } else if (type == "fa-camera") {
        var _imageName = $(elmId + ' input.ip-imagename');
        var _imageFile = $(elmId + ' input.ip-imagefile');
        if (_imageName.valid() && _imageFile.valid()) {
            $(elmId).find('.elphoto').removeClass('fa-camera').addClass('fa-check green');
            $(element).parent().parent().hide();
            $(elmId).find('.elphoto').parent().css('color', '');
        } else {
            $(elmId).find('.elphoto').removeClass('fa-check green').addClass('fa-camera');
            $(elmId).find('.elphoto').parent().css('color', '#b84c4c');
        }
    } else if (type == "fa-file-text") {
        var _docName = $(elmId + ' input.ip-docname');
        var _docFile = $(elmId + ' input.ip-docfile');
        if (_docName.valid() && _docFile.valid()) {
            $(elmId).find('.eldocument').removeClass('fa-file-text').addClass('fa-check green');
            $(element).parent().parent().hide();
            $(elmId).find('.eldocument').parent().css('color', '');
        } else {
            $(elmId).find('.eldocument').removeClass('fa-check green').addClass('fa-file-text');
            $(elmId).find('.eldocument').parent().css('color', '#b84c4c');
        }
    } else if (type == "fa-trophy") {
        var _score = $(elmId + ' select.ip-score');
        if (_score.valid()) {
            $(elmId).find('.elscore').parent().css('color', '');
            $(elmId).find('.elscore').removeClass('fa-trophy').addClass('fa-check green');
            $(element).parent().parent().hide();

        } else {
            $(elmId).find('.elscore').removeClass('fa-check green').addClass('fa-trophy');
            $(elmId).find('.elscore').parent().css('color', '#b84c4c');
        }
    } else {
        if ($(element).val())
            $(elmId).addClass('fa fa-check green');
        else
            $(elmId).removeClass('fa fa-check green');
    }
}

function restartTask(complianceTaskId, taskId) {
    $.LoadingOverlay("show");
    $.post("/Operator/RestartTask", { complianceTaskId: complianceTaskId, taskId: taskId }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
            location.reload();
        } else if (!response.result && data.msg) {
            cleanBookNotification.error(_L(data.msg), "Operator");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
        LoadingOverlayEnd();
    });
}