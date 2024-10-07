var isServerBusy = false;
function DiscussionQBInit() {

    $('#frm-discussion-qb').validate({
        rules: {
            Title: {
                required: true,
                maxlength: 150
            },
            FeaturedImage: {
                filesize: true
            },
            Summary: {
                maxlength: 500
            }
        }
    });

    $('.preview-discussion-img').change(function () {
        var target = $(this).data('target');
        readURLImage(this, target);
        $(target).fadeIn();
    });
    function S_Assigee_Options(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    function S_Assigee_Selected(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    $('.select2avatar-discussion').select2({
        placeholder: 'Please select',
        templateResult: S_Assigee_Options,
        templateSelection: S_Assigee_Selected
    });
    $('.singledateandtime-dis').daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        drops: "up",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });

}
function CreateQbiclesDiscussion() {

    if ($('#frm-discussion-qb').valid()) {
        $('#add-discussion-body').LoadingOverlay("show");
        //$.LoadingOverlay("show");
        var files = document.getElementById("discussion-image-upload").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("discussion-image-upload").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    //LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    //LoadingOverlayEnd();
                    SaveDiscussionForQbicle(mediaS3Object.objectKey);
                }
            });
        }
        else {
            //LoadingOverlayEnd();
            SaveDiscussionForQbicle("");
        }
    }
}
function SaveDiscussionForQbicle(objectKey) {
    //isDisplayFlicker(true);
    /*$('#LoadingOverlay').LoadingOverlay("show");*/
    var discussionModel = {
        Key: $("#discussion-key").val(),
        Title: $("#discussion-title").val(),
        Summary: $("#discussion-summary").val(),
        ExpiryDate: $("#discussion-expiryDate").val(),
        IsExpiry: $("#discussion-is-expiry").prop('checked'),
        Topic: $("#discussion-topic").val(),
        Assignee: $("#discussion-assignee").val(),
        FeaturedOption: $("#discussion-featured-option").val(),
        MediaDiscussionUse: $("#mediaDiscussionUse").val(),
        UploadKey: objectKey
    };
    $.ajax({
        type: "post",
        cache: false,
        url: "/Discussions/SaveDiscussionForQbicle",
        data: {
            model: discussionModel
        },
        beforeSend: function (xhr) {            
            isServerBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#create-discussion-qb').modal('hide');                
                cleanBookNotification.success(_L("ERROR_MSG_398"), "Qbicles");
                
                if (data.msg != '') {
                    htmlActivityRender(data.msg, 0);
                    //isDisplayFlicker(false);
                }

                $('#frm-discussion-qb').trigger("reset");
                $('#frm-discussion-qb select[name=Topic]').val('').trigger('change');
                $('#frm-discussion-qb select[name=Assignee]').val('').trigger('change');
                $('#frm-discussion-qb input[name=IsExpiry]').bootstrapToggle('off');
                $('.setexpiry').hide();
                $('#frm-discussion-qb').validate().resetForm();
            } else if (!data.result && data.Object) {
                $('#frm-discussion-qb').validate().showErrors(data.Object);
            } else {
                cleanBookNotification.error(_L(data.msg), "Qbicles");
            }
            isServerBusy = false;
        },
        error: function (data) {
            isServerBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isDisplayFlicker(false);
            
        }

    }).always(function () {
        $('#add-discussion-body').LoadingOverlay("hide", true);
    });
}
$(document).ready(function () {
    DiscussionQBInit();
});