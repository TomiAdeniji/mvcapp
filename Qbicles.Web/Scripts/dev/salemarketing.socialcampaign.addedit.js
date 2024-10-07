$(document).ready(function () {
    $("#frm_marketing-email-post").validate({
        ignore: "",
        rules: {
            Headline: {
                maxlength: 500
            },
            ButtonText: {
                maxlength: 100
            }
        }
    });
    $("#sm-social-campaign-upload-media").change(function () {
        
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });
});

ProcessSocialCampaign = function (modalId) {
    $.LoadingOverlay("show");
    if (!$('#frm_marketing-social-campaign_addedit').valid()) {
        $('.admintabs a[href=#social-overview]').tab('show');
        return;
    }
    //
    var workgroupid = $('#social-campaign-workgroup').val();
    if (!workgroupid) {
        cleanBookNotification.error("Please select a Workgroup!", "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        return;
    }
    var folder = $('#social-campaign-folders').val();
    if (!folder || (folder == "0" && !$('#social-newfolder-name').val())) {
        cleanBookNotification.error("Please select a campaign folder!", "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        return;
    }
    var cpbrand = $('#slBrandCampaign').val();
    if (!cpbrand) {
        cleanBookNotification.error("Please select a Brand!", "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        return;
    }

    var files = document.getElementById("sm-social-campaign-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-social-campaign-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#sm-social-campaign-object-key").val(mediaS3Object.objectKey);
                $("#sm-social-campaign-object-name").val(mediaS3Object.fileName);
                $("#sm-social-campaign-object-size").val(mediaS3Object.fileSize);

                SubmitSocialCampaign(modalId);
            }
        });

    }
    else {
        $("#sm-social-campaign-object-key").val("");
        $("#sm-social-campaign-object-name").val("");
        $("#sm-social-campaign-object-size").val("");
        SubmitSocialCampaign(modalId);
    }
};

function SubmitSocialCampaign(modalId) {
    var frmData = new FormData($('#frm_marketing-social-campaign_addedit')[0]);
    var opwgSelect = $('#social-campaign-workgroup option:selected');

    frmData.append("qbicleFolderId", opwgSelect.attr("qbicleid"));
    frmData.append("topicid", opwgSelect.attr("topicid"));
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveSocialCampaign",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#' + modalId + '').modal('hide');
                //$('#app-marketing-social-campaign-add').modal('hide');
                LoadSocicalCampains(true);
                cleanBookNotification.success(_L("ERROR_MSG_178"), "Sales Marketing");
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");

            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusyAddTaskForm = false;
    });
};



function ProcessEmailCampaign() {
    LoadingOverlay();
    if (!$('#frm_marketing-email-campaign_addedit').valid()) {
        $('.admintabs a[href=#social-overview]').tab('show');
        LoadingOverlayEnd();
        return;
    }

    var workgroupid = $('#social-campaign-workgroup').val();
    if (!workgroupid) {
        cleanBookNotification.error(_L("ERROR_MSG_168"), "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        LoadingOverlayEnd();
        return;
    }
    var folder = $('#social-campaign-folders').val();
    if (!folder || (folder == "0" && !$('#social-newfolder-name').val())) {
        cleanBookNotification.error(_L("ERROR_MSG_169"), "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        LoadingOverlayEnd();
        return;
    }
    var cpbrand = $('#slBrandCampaign').val();
    if (!cpbrand) {
        cleanBookNotification.error(_L("ERROR_MSG_170"), "Sales Marketing");
        $('.admintabs a[href=#social-overview]').tab('show');
        LoadingOverlayEnd();
        return;
    }

    var files = document.getElementById("sm-email-campaign-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-email-campaign-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-email-campaign-object-key").val(mediaS3Object.objectKey);
                $("#sm-email-campaign-object-name").val(mediaS3Object.fileName);
                $("#sm-email-campaign-object-size").val(mediaS3Object.fileSize);

                SubmitEmailCampaign();
            }
        });

    }
    else {
        $("#sm-email-campaign-object-key").val("");
        $("#sm-email-campaign-object-name").val("");
        $("#sm-email-campaign-object-size").val("");
        SubmitEmailCampaign();
    }
};

function SubmitEmailCampaign() {
    {
        var frmData = new FormData($('#frm_marketing-email-campaign_addedit')[0]);
        var opwgSelect = $('#social-campaign-workgroup option:selected');

        frmData.append("qbicleFolderId", opwgSelect.attr("qbicleid"));
        frmData.append("topicid", opwgSelect.attr("topicid"));
        $.ajax({
            type: "post",
            cache: false,
            url: "/SalesMarketing/SaveEmailCampaign",
            enctype: 'multipart/form-data',
            data: frmData,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                isBusyAddTaskForm = true;
            },
            success: function (data) {
                if (data.result) {
                    $('#app-marketing-email-campaign-add').modal('hide');
                    $('#app-marketing-email-campaign-edit').modal('hide');

                    LoadEmailCampaigns(true);
                    cleanBookNotification.success(_L("ERROR_MSG_371"), "Sales Marketing");
                } else {
                    cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                }
            },
            error: function (data) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        }).always(function () {
            LoadingOverlayEnd();
            isBusyAddTaskForm = false;
        });
    }
};



ProcessCampaignResource = function () {
    if (!$("#form_media_smresource").valid()) {
        return;
    }

    $.ajax({
        url: "/Medias/DuplicateMediaNameCheck",
        data: { qbicleId: $('#media-qbicleId').val(), mediaId: 0, MediaName: $('#create-resource input[name=name]').val() },
        type: "GET",
        dataType: "json",
    }).done(function (refModel) {
        if (refModel.result) {
            $("#form_media_smresource").validate().showErrors({ name: "Name of Media already exists." });
            return;

        }
    }).fail(function () {
        $("#form_media_smresource").validate().showErrors({ name: _L("ERROR_MSG_173") });
        return;
    })
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-campaign-resource-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-campaign-resource-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#sm-campaign-resource-object-key").val(mediaS3Object.objectKey);
                $("#sm-campaign-resource-object-name").val(mediaS3Object.fileName);
                $("#sm-campaign-resource-object-size").val(mediaS3Object.fileSize);

                SubmitCampaignResource();
            }
        });

    }
    else {
        $("#sm-campaign-resource-object-key").val("");
        $("#sm-campaign-resource-object-name").val("");
        $("#sm-campaign-resource-object-size").val("");
        SubmitCampaignResource();
    }
};

function SubmitCampaignResource() {
    var frmData = new FormData($("#form_media_smresource")[0]);
    frmData.append("qbicleId", $('#media-qbicleId').val());
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveCampaignResource",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#create-resource').modal('hide');
                
                cleanBookNotification.success(_L("ERROR_MSG_197"), "Sales Marketing");
                MedLoadMediasByFolder();
                $form_media_addedit.trigger("reset");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
               
            }

        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            LoadingOverlayEnd();        }

    }).always(function () {
        LoadingOverlayEnd();
        isBusyAddTaskForm = false;
    });
};