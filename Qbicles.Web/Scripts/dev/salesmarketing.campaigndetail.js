var $frm_campaign_edit = $('#frm_marketing-social-campaign_addedit'), isBusy = false,
    $form_media_addedit = $("#form_media_smresource"),
    $frm_social_post = $("#frm_marketing-social-post"),
    $frm_email_post = $("#frm_marketing-email-post"),
    $frm_email_campaign_edit = $('#frm_marketing-email-campaign_edit'),
    $quill;
function readImgURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}
function ReloadFolders() {
    var opwgSelect = $('#social-campaign-workgroup option:selected');
    var qbicleId = opwgSelect.attr("qbicleid");
    if (qbicleId) {
        $.getJSON("/SalesMarketing/LoadFoldersByQbicle", { qbicleId: qbicleId }, function (data) {
            if (data && data.length > 0) {
                data.push({ id: 0, text: "Create a new folder" });
                $('#social-campaign-folders').empty();
                $('#social-campaign-folders').append('<option value=""></option><option value="0">Create a new folder</option>');
                $('#social-campaign-folders').select2({
                    data: data,
                    placeholder: "Please select"
                });
                $('#social-campaign-folders').val($('#CurrentFolderId').val()).trigger("change");
            }
        });
    }

}
function isValidWorkgroup() {
    var workgroupid = $('#social-campaign-workgroup').val();
    var _opselect = $('#social-campaign-workgroup option:selected');
    $('td.info-process').text(_opselect.attr('process'));
    $('td.info-members').text(_opselect.attr('members'));
    disableCampaignForm(workgroupid ? false : true);
}
function disableCampaignForm(isDisable) {
    var frm = "#frm_marketing-social-campaign_addedit";
    if (isDisable) {
        $(frm + " input[name=Name]").prop("disabled", true);
        $(frm + " input[name=foldername]").prop("disabled", true);
        $(frm + " input[name=featuredimg]").prop("disabled", true);
        $(frm + " textarea[name=Details]").prop("disabled", true);
        $('#social-campaign-folders').prop("disabled", true);
        $(frm + " .checkmulti").multiselect('disable');
        $('.preview-workgroup').hide();
    } else {
        $(frm + " input[name=Name]").prop("disabled", false);
        $(frm + " input[name=foldername]").prop("disabled", false);
        $(frm + " input[name=featuredimg]").prop("disabled", false);
        $(frm + " textarea[name=Details]").prop("disabled", false);
        $('#social-campaign-folders').prop("disabled", false);
        $(frm + " .checkmulti").multiselect('enable');
        var wg = $('#social-campaign-workgroup');
        $('.preview-workgroup .info-process').text(wg.attr("process"));
        $('.preview-workgroup .info-members').text(wg.attr("members"));
        $('.preview-workgroup').show();
        ReloadFolders();
    }
}
function AutoGenerateFolderName() {
    var opwgSelect = $('#social-campaign-workgroup option:selected');
    var qbicleId = opwgSelect.attr("qbicleid");
    if (qbicleId) {
        $.getJSON("/SalesMarketing/AutoGenerateFolderName", { qbicleId: qbicleId }, function (data) {
            if (data) {
                $('#social-newfolder-name').val(data);
            }
        });
    }
}
function MedLoadMediasByFolder() {

    $('#network1-resources').LoadingOverlay("show");
    var fid = $('#media-folder-qbicle').val();
    var fbrandid = $("#media-brand-folder").val();
    var fideaid = $("#media-idea-folder").val();
    var qid = $('#media-qbicleId').val();
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/LoadMediasByFolderId',
        datatype: 'json',
        data: { fid: fid, fbrandid: fbrandid, qid: qid, fideaid: fideaid },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#network1-resources .flex-grid-thirds-lg');
                $divcontain.empty();
                $divcontain.html(listMedia);
                totop();

                $('#network1-resources').LoadingOverlay("hide");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            $('#network1-resources').LoadingOverlay("hide");
        }
    });
}
function LoadPostContent(type) {
    $.LoadingOverlay("show");
    var campaignId = $('#hdfcampaignId').val();
    if (type === "queue") {
        $('#network1-content-queue').load("/SalesMarketing/LoadPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-queue .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approved") {
        $('#network1-content-approved').load("/SalesMarketing/LoadPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approved .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approvals") {
        $('#network1-content-approvals').load("/SalesMarketing/LoadPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approvals .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else {
        $('#network1-content-sent').load("/SalesMarketing/LoadPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-sent .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    }
}
function LoadEmailPostContent(type) {
    $.LoadingOverlay("show");
    var campaignId = $('#hdfcampaignId').val();
    if (type === "queue") {
        $('#network1-content-queue').load("/SalesMarketing/LoadEmailPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-queue .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountEmailQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approved") {
        $('#network1-content-approved').load("/SalesMarketing/LoadEmailPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approved .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountEmailQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approvals") {
        $('#network1-content-approvals').load("/SalesMarketing/LoadEmailPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approvals .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountEmailQueue();
            LoadingOverlayEnd();
        });
    } else {
        $('#network1-content-sent').load("/SalesMarketing/LoadEmailPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-sent .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountEmailQueue();
            LoadingOverlayEnd();
        });
    }
}
function LoadManualPostContent(type) {
    $.LoadingOverlay("show");
    var campaignId = $('#hdfcampaignId').val();
    if (type === "queue") {
        $('#network1-content-queue').load("/SalesMarketing/LoadManualPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-queue .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approved") {
        $('#network1-content-approved').load("/SalesMarketing/LoadManualPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approved .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else if (type === "approvals") {
        $('#network1-content-approvals').load("/SalesMarketing/LoadManualPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-approvals .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    } else {
        $('#network1-content-sent').load("/SalesMarketing/LoadManualPostContent", { type: type, campaignId: campaignId }, function () {
            var tbl = $('#network1-content-sent .datatable').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'desc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            LoadCountQueue();
            LoadingOverlayEnd();
        });
    }
}
function LoadDownloadModal(id, type) {
    $.LoadingOverlay("show");
    $('#download-post').load("/SalesMarketing/LoadDownloadModal", { id: id, type: type }, function () {
        $('#download-post').modal("show");
        LoadingOverlayEnd();
    });
}


function DownloadMedia(uri, extension) {
    var fileName = uri + '.' + extension;
    var fileModel = {
        Uri: uri,
        Name: fileName
    };

    $.ajax({
        type: 'post',
        url: '/Medias/DownloadFile',
        datatype: 'json',
        data: fileModel,
        success: function (refModel) {
            var link = document.createElement("a");
            link.download = fileName;
            link.target = '_blank';
            link.href = refModel;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        }, error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}

function LoadCountQueue() {
    var campaignId = $('#hdfcampaignId').val();
    $.getJSON("/SalesMarketing/LoadCountQueue", { CampaignById: campaignId }, function (data) {
        if (data) {
            $(".count-queue").text(data.CountQueue);
            $(".count-approved").text(data.CountApproved);
            $(".count-approvals").text(data.CountApproval);
            $(".count-sent").text(data.CountSent);
        }
    });
}

function LoadCountEmailQueue() {
    var campaignId = $('#hdfcampaignId').val();
    $.getJSON("/SalesMarketing/LoadCountEmailQueue", { CampaignById: campaignId }, function (data) {
        if (data) {
            $(".count-queue").text(data.CountQueue);
            $(".count-approved").text(data.CountApproved);
            $(".count-approvals").text(data.CountApproval);
            $(".count-sent").text(data.CountSent);
        }
    });
}
function LoadMediaByCampaign() {
    var fid = $('#media-folder-qbicle').val();
    var fbrandid = $("#media-brand-folder").val();
    var fideaid = $("#media-idea-folder").val();
    var qid = $('#media-qbicleId').val();
    $.LoadingOverlay("show");
    $('#Choose-campaign-resource').load("/SalesMarketing/LoadCampaignResourcesContent", { fid: fid, qid: qid, fbrandid: fbrandid, fideaid: fideaid }, function () {
        LoadingOverlayEnd();
    });
}

function LoadPostEdit(pid) {
    $('#app-marketing-social-post-edit').load("/SalesMarketing/LoadPostEdit", { campaignPId: pid }, function () {
        $('#app-marketing-social-post-edit').modal("show");

        $("#frm_marketing-social-post-edit").validate({
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

        $("#app-marketing-social-post-edit .checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#app-marketing-social-post-edit select.select2').select2();
    });
}

function LoadSocialPostAdd() {
    $("#app-marketing-social-post-add").html("");
    $("#app-marketing-social-post-add").load("/SalesMarketing/LoadSocialPostAdd", { campId: $("#hdfcampaignId").val() }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $(".select2").select2({
            placeholder: "Please select"
        });
        $("input[data-toggle='toggle']").bootstrapToggle();
        $('.singledateandtime').daterangepicker({
            singleDatePicker: true,
            timePicker: true,
            autoApply: true,
            showDropdowns: true,
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateTimeFormatByUser
            }
        });
        $(".previewimgpost").change(function () {

            var vpc = $('#video-preview-container');
            var imgpr = $('#social-2 .preview-post-add');

            var target = $(this).data('target');
            if (isFileImage(this) > 0) {
                readImgURL(this, target);
                $(target).fadeIn();

                setTimeout(function () {
                    $("#social-2 .preview-post-add").attr("src", $("#social-1 .newpreview").attr("src"));
                }, 500);

                imgpr.show();
                vpc.hide();
            } else {
                $(target).attr('src', '');
                $(target).hide();
                if (isVideo($(this).val())) {
                    $('.preview-post-add').attr('src', '');
                    $('#video-preview').attr('src', URL.createObjectURL(this.files[0]));
                    imgpr.hide();
                    vpc.show();
                }
                else {
                    //$("#social-2 .preview-post-add").attr("src", $("#social-1 .newpreview").attr("src"));
                    $('#video-preview').attr('src', '');
                    //$('.preview-post-add').attr('src', URL.createObjectURL(this.files[0]));
                    imgpr.show();
                    vpc.hide();
                }
            }
        });

        $("#frm_marketing-social-post").validate({
            ignore: "",
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

    })
}


function ProcessSocialPost(modalId, formId) {
    var $frmSocialPost = $("#" + formId);
    if ($frmSocialPost.valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("sm-social-post-feature-upload-media").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("sm-social-post-feature-upload-media").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    $("#sm-social-post-feature-image-object-key").val(mediaS3Object.objectKey);
                    $("#sm-social-post-feature-image-object-name").val(mediaS3Object.fileName);
                    $("#sm-social-post-feature-image-object-size").val(mediaS3Object.fileSize);

                    SubmitSocialPost(modalId, formId);
                }
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
                LoadCountQueue();
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
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {

                    $("#sm-social-post-feature-image-object-key").val(mediaS3Object.objectKey);
                    $("#sm-social-post-feature-image-object-name").val(mediaS3Object.fileName);
                    $("#sm-social-post-feature-image-object-size").val(mediaS3Object.fileSize);

                    SubmitManualSocialPost(modalId, formId);
                }
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
                LoadCountQueue();
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


function LoadManualPostAdd() {
    $("#app-marketing-social-post-add").html("");
    $("#app-marketing-social-post-add").load("/SalesMarketing/LoadManualPostAdd", { campId: $("#hdfcampaignId").val() }, function () {

        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $(".select2").select2({
            placeholder: "Please select"
        });
        $("input[data-toggle='toggle']").bootstrapToggle();
        $('.singledateandtime').daterangepicker({
            singleDatePicker: true,
            timePicker: true,
            autoApply: true,
            showDropdowns: true,
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateTimeFormatByUser
            }
        });
        $(".previewimgpost").change(function () {

            var vpc = $('#video-preview-container');
            var imgpr = $('#social-2 .preview-post-add');

            var target = $(this).data('target');
            if (isFileImage(this) > 0) {
                readImgURL(this, target);
                $(target).fadeIn();

                setTimeout(function () {
                    $("#social-2 .preview-post-add").attr("src", $("#social-1 .newpreview").attr("src"));
                }, 500);

                imgpr.show();
                vpc.hide();
            } else {
                $(target).attr('src', '');
                $(target).hide();
                if (isVideo($(this).val())) {
                    $('.preview-post-add').attr('src', '');
                    $('#video-preview').attr('src', URL.createObjectURL(this.files[0]));
                    imgpr.hide();
                    vpc.show();
                }
                else {
                    //$("#social-2 .preview-post-add").attr("src", $("#social-1 .newpreview").attr("src"));
                    $('#video-preview').attr('src', '');
                    //$('.preview-post-add').attr('src', URL.createObjectURL(this.files[0]));
                    imgpr.show();
                    vpc.hide();
                }
            }
        });


        $("#frm_marketing-social-post").validate({
            ignore: "",
            rules: {
                Title: {
                    required: true,
                    minlength: 5
                },
                Content: {
                    required: true
                }
                //Reminder.ReminderDate: {
                //required: $('$isReminder').val() === 'true' ? true : false
                //},
                //Reminder.Content: {
                //    required: $('$isReminder').val() === 'true' ? true : false
                //}

            }
        });

    })
}
function isFileImage(file) {
    file = file.files[0];
    const acceptedImageTypes = ['image/gif', 'image/jpeg', 'image/png', 'image/jpg'];
    return file && $.inArray(file['type'], acceptedImageTypes);
}
function AddQueueSchedule() {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddQueueSchedule',
        datatype: 'json',
        data: { aid: $('#aId').val(), isNotifyWhenSent: $('#isNotifyWhenSent').prop('checked'), sPostingDate: $('#sPostingDate').val() },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data.result) {
                $('#app-marketing-schedule-queue').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_157"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-queue"]').trigger('click');
                LoadCountQueue();
            } else if (!data.result && data.msg)
                cleanBookNotification.error(data.msg, "Sales Marketing");
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function AddEmailQueueSchedule() {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddEmailQueueSchedule',
        datatype: 'json',
        data: { aid: $('#aId').val(), isNotifyWhenSent: $('#isNotifyWhenSent').prop('checked'), sPostingDate: $('#sPostingDate').val() },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data.result) {
                $('#app-marketing-schedule-queue').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_157"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-queue"]').trigger('click');
                LoadCountEmailQueue();
            } else if (!data.result && data.msg)
                cleanBookNotification.error(data.msg, "Sales Marketing");
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function DiscardPostApproval(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DeletePostApproval',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-discard-post').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                $(".nav-marketing li.active a").trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function DiscardEmailPostApproval(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DeleteEmailPostApproval',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-discard-post').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                $(".nav-marketing li.active a").trigger('click');
                LoadCountEmailQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function DiscardPostQueue(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DeletePostQueue',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-discard-post-queue').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-queue"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function DiscardEmailPostQueue(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/DeleteEmailPostQueue',
        datatype: 'json',
        data: { id: id },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-discard-post-queue').modal("hide");
                cleanBookNotification.success(_L("ERROR_MSG_159"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-queue"]').trigger('click');
                LoadCountEmailQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function SentBackToReview(aid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/SentBackToReview',
        datatype: 'json',
        data: { aid: aid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_161"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function SentBackEmailPostToReview(aid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/SentBackEmailPostToReview',
        datatype: 'json',
        data: { aid: aid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_161"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                LoadCountEmailQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function SentBackFromQueueToReview(queueid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/SentBackFromQueueToReview',
        datatype: 'json',
        data: { queueId: queueid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_161"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                LoadCountQueue();
            } else if (!data.result && data.msg)
                cleanBookNotification.error(data.msg, "Sales Marketing");
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function SentBackFromEmailQueueToReview(queueid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/SentBackFromEmailQueueToReview',
        datatype: 'json',
        data: { queueId: queueid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_161"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                LoadCountEmailQueue();
            } else if (!data.result && data.msg)
                cleanBookNotification.error(data.msg, "Sales Marketing");
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function AddPostImmediately(aid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddPostImmediately',
        datatype: 'json',
        data: { aid: aid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_367"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function AddEmailPostImmediately(aid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddEmailPostImmediately',
        datatype: 'json',
        data: { aid: aid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_367"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountEmailQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function ChangePostInApprovedToSent(aid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/ChangePostInApprovedToSent',
        datatype: 'json',
        data: { aid: aid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                cleanBookNotification.success(_L("ERROR_MSG_368"), "Sales Marketing");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function AddPostQueueImmediately(queueid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddPostQueueImmediately',
        datatype: 'json',
        data: { queueid: queueid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-post-success').modal("show");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function AddEmailPostQueueImmediately(queueid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/AddEmailPostQueueImmediately',
        datatype: 'json',
        data: { queueid: queueid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-post-success').modal("show");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountEmailQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function ChangePostInQueueToSent(queueid) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/ChangePostInQueueToSent',
        datatype: 'json',
        data: { queueid: queueid },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            if (data) {
                $('#app-marketing-post-success').modal("show");
                $('.nav-marketing a[data-target="#network1-content-sent"]').trigger('click');
                LoadCountQueue();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function loadPreview() {
    var select = $('#app-marketing-social-post-add select[name=SharingAccount] option:selected')[0];
    $('#social-2 .social-avatar').css('background-image', 'url(' + $(select).attr("avatarUrl") + ')');
    $('#social-2 .preview-disname').text($(select).attr("displayName"));
    $('#social-2 .preview-username').text($(select).text());
    $('#social-2 .preview-content').text($('#textfield').val());
};

function loadManualPreview() {
    $('#social-2 .preview-content').text($('#textfield').val());
};

function isVideo(filename) {
    var ext = getExtension(filename);
    switch (ext.toLowerCase()) {
        case 'mp4':
        case 'webm':
        case 'ogv':
            return true;
    }
    return false;
};

function getExtension(filename) {
    var parts = filename.split('.');
    return parts[parts.length - 1];
};

function AttachChange(el, isload) {

    $('#the-post').empty();
    $('#video-preview-edit').attr('src', '');

    if ($(el).val() === '1') {//Choose an existing Campaign Resource
        if (isload) {
            LoadMediaByCampaign();
            $('.newpreview').hide();
        }
        $('.campaign-resource-picker').show();
        $('.uploadnew').hide();
        $('.previewimgpost').val('').change();
    } else {//Upload a new image or video
        var ajaxUri = '/SalesMarketing/CampaignPostPreview?isVideo=false&link=empty&thumb=empty';
        $('#the-post').load(ajaxUri, function () {

        });
        $('.campaign-resource-picker').hide();
        $('.uploadnew').show();
        $('#ImageOrVideo').val(0);
        $('#ImageOrVideoEdit').val(0);
    }
}

function AttachPromotionResource(el) {
    if ($(el).val() === '1') {//Choose an existing Campaign Resource
        var fid = $('#media-folder-qbicle').val();
        var qid = $('#media-qbicleId').val();
        var fbrandid = $("#media-brand-folder").val();
        var fideaid = $("#media-idea-folder").val();
        var featuredimage = $('#initFeaturedImage').val();

        $.LoadingOverlay("show");
        $('#Choose-campaign-resource-promotion').load("/SalesMarketing/LoadPromotionResourcesContent", { fid: fid, qid: qid, fbrandid: fbrandid, fideaid: fideaid, featuredimage: featuredimage }, function () {
            LoadingOverlayEnd();
        });
        $('.campaign-resource-picker').show();
        $('.uploadnew').hide();
    } else {//Upload a new image or video
        $('#promotionalImg').val(0);
        $('.campaign-resource-picker').hide();
        $('.uploadnew').show();
    }
}

function AttachAdvertismentResouce(el) {
    if ($(el).val() === '1') {//Choose an existing Campaign Resource
        var fid = $('#media-folder-qbicle').val();
        var fbrandid = $("#media-brand-folder").val();
        var fideaid = $("#media-idea-folder").val();
        var qid = $('#media-qbicleId').val();
        var advimage = $('#initAdvImg').val();
        $.LoadingOverlay("show");
        $('#Choose-campaign-resource-ad').load("/SalesMarketing/LoadAdResourcesContent", { fid: fid, qid: qid, fbrandid: fbrandid, fideaid: fideaid, advimage: advimage }, function () {
            LoadingOverlayEnd();
        });
        $('.campaign-resource-picker-ad').show();
        $('.uploadnew-ad').hide();
    } else {//Upload a new image or video
        $('#adImg').val(0);
        $('.campaign-resource-picker-ad').hide();
        $('.uploadnew-ad').show();
    }
}

function chooseMediaAdd(el, id, link, isUse, isVideo, thumb) {
    $('.change' + id).hide();
    $('.usetheme' + id).hide();

    $('#the-post').empty();

    if (isUse) {
        $(el).hide();
        $('.change' + id).show();
        $('.other').hide();
        $('#rs-' + id).show();
        $('#ImageOrVideo').val(id);

        var ajaxUri = '/SalesMarketing/CampaignPostPreview?isVideo=' + isVideo + '&link=' + link + '&thumb=' + thumb;
        $('#the-post').load(ajaxUri, function () {

        });

    } else {
        $(el).hide();
        $('.usetheme' + id).show();
        $('.other').show();
        $('#ImageOrVideo').val(0);
    }
}

function choosePromotionMediaAdd(el, id, link, isUse, isVideo, thumb) {
    $('.change-promotion' + id).hide();
    $('.usetheme-promotion' + id).hide();

    if (isUse) {
        $(el).hide();
        $('.change-promotion' + id).show();
        $('.other-promotion').hide();
        $('#rs-promotion-' + id).show();
        $('#promotionalImg').val(id);

    } else {
        $(el).hide();
        $('.usetheme-promotion' + id).show();
        $('.other-promotion').show();
        $('#promotionalImg').val(0);
    }
}

function chooseAdMediaAdd(el, id, link, isUse, isVideo, thumb) {
    $('.change-ad' + id).hide();
    $('.usetheme-ad' + id).hide();

    if (isUse) {
        $(el).hide();
        $('.change-ad' + id).show();
        $('.other-ad').hide();
        $('#rs-ad-' + id).show();
        $('#adImg').val(id);

    } else {
        $(el).hide();
        $('.usetheme-ad' + id).show();
        $('.other-ad').show();
        $('#adImg').val(0);
    }
}

function chooseMediaEdit(el, id, link, isUse, isVideo) {
    $('.change' + id).hide();
    $('.usetheme' + id).hide();

    if (isUse) {
        $(el).hide();
        $('.changee' + id).show();
        $('.othere').hide();
        $('#rse-' + id).show();
        $('#ImageOrVideoEdit').val(id);
        if (isVideo) {
            $('.preview-post-edit').attr('src', '');

            $('#mp4-preview').attr('src', link.replace("mediaVideo", "mp4"));
            $('#webm-preview').attr('src', link.replace("mediaVideo", "webm"));
            $('#ogv-preview').attr('src', link.replace("mediaVideo", "ogv"));
        } else {
            $('.preview-post-edit').attr('src', link);
            $('#video-preview-edit').attr('src', '');
        }

    } else {
        $(el).hide();
        $('.usethemee' + id).show();
        $('.othere').show();
        $('#ImageOrVideoEdit').val(0);
        if (isVideo) {
            $('.preview-post-edit').attr('src', '');

            $('#mp4-preview').attr('src', link.replace("mediaVideo", "mp4"));
            $('#webm-preview').attr('src', link.replace("mediaVideo", "webm"));
            $('#ogv-preview').attr('src', link.replace("mediaVideo", "ogv"));
        } else {
            $('.preview-post-edit').attr('src', link);
            $('#video-preview-edit').attr('src', '');
        }
    }
}
function SocialBrandOptionsLoadForEdit(el) {
    var brandId = $(el).val();
    var cbrandId = $('#hdfBrandId').val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/SocialBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                if (cbrandId === brandId) {
                    var brandProducts = $('#hdfBrandProducts').val().split(",");
                    var valuePropositons = $('#hdfValuePropositons').val().split(",");
                    var brandAttributes = $('#hdfAttributes').val().split(",");
                    $('#brandoptions select[name=brandproducts]').val(brandProducts);
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val(brandAttributes);
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val(valuePropositons);
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                } else {
                    $('#brandoptions select[name=brandproducts]').val('');
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val('');
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val('');
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                }
            });
        }
    } else {
        $('#brandoptions').hide();
    }
}
function EmailBrandOptionsLoadForEdit(el) {
    var brandId = $(el).val();
    var cbrandId = $('#hdfBrandId').val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/EmailBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                if (cbrandId === brandId) {
                    var brandProducts = $('#hdfBrandProducts').val().split(",");
                    var valuePropositons = $('#hdfValuePropositons').val().split(",");
                    var brandAttributes = $('#hdfAttributes').val().split(",");
                    $('#brandoptions select[name=brandproducts]').val(brandProducts);
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val(brandAttributes);
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val(valuePropositons);
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                } else {
                    $('#brandoptions select[name=brandproducts]').val('');
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val('');
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val('');
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                }
            });
        }
    } else {
        $('#brandoptions').hide();
    }
}
function socialIdeaUseInCampaign(id, isuse) {
    if (isuse) {
        $('.other').hide();
        $('#rs-' + id).show();
        $('.usetheme' + id).hide();
        $('.change' + id).show();
        $('#ideaId').val(id);
    } else {
        $('.other').show();
        $('.change' + id).hide();
        $('.usetheme' + id).show();
    }
}
$(document).ready(function () {
    if (window.location.href.includes("SMEmail")) {
        LoadCountEmailQueue();
    } else {
        LoadCountQueue();
    }

    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    isValidWorkgroup();
    //load script init
    $frm_campaign_edit.validate({
        ignore: "#social-newfolder-name",
        rules: {
            Name: {
                required: true,
                minlength: 5
            }
        }
    });
    $frm_email_campaign_edit.validate({
        ignore: "#social-newfolder-name",
        rules: {
            Name: {
                required: true,
                minlength: 5
            }
        }
    });
    $form_media_addedit.validate({
        rules: {
            name: {
                required: true,
                minlength: 5
            },
            description: {
                required: true
            }
        }
    });
    $frm_social_post.validate({
        ignore: "",
        rules: {
            Title: {
                required: true,
                minlength: 5
            },
            Content: {
                required: true
            }
            //Reminder.ReminderDate: {
            //required: $('$isReminder').val() === 'true' ? true : false
            //},
            //Reminder.Content: {
            //    required: $('$isReminder').val() === 'true' ? true : false
            //}

        }
    });
    $frm_email_post.validate({
        ignore: ".ql-container *",
        rules: {
            Headline: {
                maxlength: 500
            },
            ButtonText: {
                maxlength: 100
            }
        }
    });
    var $frm_email_campaign_add = $('#frm_marketing-email-campaign_add');
    $frm_email_campaign_add.submit(function (e) {
        e.preventDefault();
        var workgroupid = $('#social-campaign-workgroup').val();
        if (!workgroupid) {
            cleanBookNotification.error(_L("ERROR_MSG_168"), "Sales Marketing");
            $('.admintabs a[href=#social-overview]').tab('show');
            return;
        }
        var folder = $('#social-campaign-folders').val();
        if (!folder || (folder === "0" && !$('#social-newfolder-name').val())) {
            cleanBookNotification.error(_L("ERROR_MSG_169"), "Sales Marketing");
            $('.admintabs a[href=#social-overview]').tab('show');
            return;
        }
        var cpbrand = $('#slBrandCampaign').val();
        if (!cpbrand) {
            cleanBookNotification.error(_L("ERROR_MSG_170"), "Sales Marketing");
            $('.admintabs a[href=#social-overview]').tab('show');
            return;
        }
        if ($frm_email_campaign_edit.valid()) {
            $.LoadingOverlay("show");
            var frmData = new FormData($frm_email_campaign_edit[0]);
            var opwgSelect = $('#social-campaign-workgroup option:selected');
            var qbicleId = opwgSelect.attr("qbicleid");
            var topicid = opwgSelect.attr("topicid");
            frmData.append("qbicleFolderId", qbicleId);
            frmData.append("topicid", topicid);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: 'multipart/form-data',
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $('app-marketing-email-campaign-edit').modal('hide');
                        isBusy = false;
                        cleanBookNotification.success(_L("ERROR_MSG_171"), "Sales Marketing");
                        location.reload();
                    } else if (data.msg) {
                        cleanBookNotification.error(data.msg, "Sales Marketing");
                        isBusy = false;
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            });
        } else {
            $('.admintabs a[href=#social-overview]').tab('show');
            return;
        }
    });

    $(".previewimg").change(function () {
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });


    $(".previewimgresource").change(function () {
        var target = $(this).data('target');
        if (isFileImage(this) > 0) {
            readImgURL(this, target);
            $(target).fadeIn();
        } else {
            $(target).hide();
        }
    });



    $('#textfield').keyup(function () {
        var maxtw = 460;
        var maxin = 2200;

        var len = $(this).val().length;

        if (len >= maxtw) {
            $('#charNum').text(' Twitter limit exceeded');
        } else {
            //var char = max - len;
            $('#charNum').text(len + '/' + maxtw);
        }

        if (len >= maxin) {
            $('#charNum2').text(' Instagram limit exceeded');
        } else {
            //var char = max - len;
            $('#charNum2').text(len + '/' + maxin);
        }
    });
    $('#texteditfield').keyup(function () {
        var maxtw = 460;
        var maxin = 2200;

        var len = $(this).val().length;

        if (len >= maxtw) {
            $('#EditcharNum').text(' Twitter limit exceeded');
        } else {
            //var char = max - len;
            $('#EditcharNum').text(len + '/' + maxtw);
        }

        if (len >= maxin) {
            $('#EditcharNum2').text(' Instagram limit exceeded');
        } else {
            //var char = max - len;
            $('#EditcharNum2').text(len + '/' + maxin);
        }
    });

    // Cycle app nav tabs with button triggers
    $frm_social_post.submit(function (e) {
        e.preventDefault();
        if ($frm_social_post.valid()) {
            $.LoadingOverlay("show");
            var frmData = new FormData($frm_social_post[0]);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: 'multipart/form-data',
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $('#app-marketing-social-post-add').modal('hide');
                        isBusy = false;
                        cleanBookNotification.success(_L("ERROR_MSG_174"), "Sales Marketing");
                        $frm_social_post.trigger("reset");
                        $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                        LoadCountQueue();
                    } else if (data.msg) {
                        cleanBookNotification.error(data.msg, "Sales Marketing");
                        isBusy = false;
                    }

                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    LoadingOverlayEnd();
                }

            });
        }
    });
    var Block = Quill.import('blots/block');
    Block.tagName = 'div';
    Quill.register(Block);
    $quill = new Quill('#editor', {
        modules: {
            toolbar: '#toolbar-container'
        },
        placeholder: 'Enter your email\'s main content here...',
        theme: 'snow'
    });

    $("#previewEmail").click(function (e) {
        e.preventDefault();
        var formData = new FormData();
        formData.append("Headline", $("input[name=Headline]").val());
        formData.append("ButtonLink", $("input[name=ButtonLink]").val());
        if (formData.get('ButtonLink')) {
            formData.set('ButtonLink', "https://" + formData.get('ButtonLink'))
        }
        formData.append("TemplateId", $("select[name=templateId]").val());
        formData.append("ButtonText", $("input[name=ButtonText]").val());
        formData.append("BodyContent", escape($("#editor .ql-editor").html()));
        formData.append("promotionalImgFile", $('input[type=file][name=promotionalImgFile]')[0].files[0]);
        formData.append("adImgFile", $('input[type=file][name=adImgFile]')[0].files[0]);
        formData.append("promotionalImg", $("input[name=promotionalImg]").val());
        formData.append("adImg", $("input[name=adImg]").val());


        $.LoadingOverlay("show");

        $.ajax({
            type: 'post',
            url: '/SalesMarketing/RenderPreviewEmail',
            enctype: 'multipart/form-data',
            cache: false,
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {

                openWindowWithPost("/SalesMarketing/PreviewEmail", {
                    id: 0,
                    preview: data.Object
                });

                //window.open(
                //    '/SalesMarketing/PreviewEmail/0',
                //    '_blank'
                //);
                LoadingOverlayEnd();
            },
            error: function (err) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                cleanBookNotification.error(err.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    })

    if ($('#slBrandCampaign').val()) {
        $('#slBrandCampaign').trigger('change');
    }


});

function changeListSegments() {
    $.LoadingOverlay("show");
    var lstSegmentsId = $('#campaignsegments').val()
    if (!lstSegmentsId) {
        var options = $('#campaignsegments option');
        var lstSegmentsId = $.map(options, function (option) {
            return option.value;
        });
    }
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/CountContacts',
        datatype: 'json',
        data: { lstSegments: lstSegmentsId },
        success: function (data) {
            
            $("#includedSegments").text(data.Object.lstSegments);
            $("#totalRecipients").text(data.Object.totalContacts);
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function ProcessSMEmailPost() {
    if (!$frm_email_post.valid()) {
        cleanBookNotification.error(_L("ERROR_MSG_179"), "Sales Marketing");
        return;
    }
    if ($("#promotionalImg").val() == 0 && $('#sm-email-post-promotional-upload-media').get(0).files.length === 0) {
        cleanBookNotification.error(_L("ERROR_MSG_175"), "Sales Marketing");
        return;
    } else if ($(".ql-editor").html() == "<p><br></p>") {
        cleanBookNotification.error(_L("ERROR_MSG_176"), "Sales Marketing");
        return;
    }
    ProcessSMEmailPostFeature();

}

ProcessSMEmailPostFeature = function () {
    $.LoadingOverlay("show");
    var featureMediaFiles = document.getElementById("sm-email-post-feature-upload-media").files;

    if (featureMediaFiles && featureMediaFiles.length > 0) {

        UploadMediaS3ClientSide("sm-email-post-feature-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-email-post-feature-image-object-key").val(mediaS3Object.objectKey);
                $("#sm-email-post-feature-image-object-name").val(mediaS3Object.fileName);
                $("#sm-email-post-feature-image-object-size").val(mediaS3Object.fileSize);
                ProcessSMEmailPostPromotional();
            }
        });


    } else {
        $("#sm-email-post-feature-image-object-key").val("");
        $("#sm-email-post-feature-image-object-name").val("");
        $("#sm-email-post-feature-image-object-size").val("");
        ProcessSMEmailPostPromotional();
    }
};
ProcessSMEmailPostPromotional = function () {
    LoadingOverlay();
    var promotionalMediaFiles = document.getElementById("sm-email-post-promotional-upload-media").files;

    if (promotionalMediaFiles && promotionalMediaFiles.length > 0) {

        UploadMediaS3ClientSide("sm-email-post-promotional-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {


                $("#sm-email-post-promotional-image-object-key").val(mediaS3Object.objectKey);
                $("#sm-email-post-promotional-image-object-name").val(mediaS3Object.fileName);
                $("#sm-email-post-promotional-image-object-size").val(mediaS3Object.fileSize);
                ProcessSMEmailPostAd();
            }
        });




    } else {
        $("#sm-email-post-promotional-image-object-key").val("");
        $("#sm-email-post-promotional-image-object-name").val("");
        $("#sm-email-post-promotional-image-object-size").val("");
        ProcessSMEmailPostAd();
    }
    LoadingOverlayEnd();
};
ProcessSMEmailPostAd = function () {
    var adMediaFiles = document.getElementById("sm-email-post-ad-upload-media").files;

    if (adMediaFiles && adMediaFiles.length > 0) {

        UploadMediaS3ClientSide("sm-email-post-ad-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {


                $("#sm-email-post-ad-image-object-key").val(mediaS3Object.objectKey);
                $("#sm-email-post-ad-image-object-name").val(mediaS3Object.fileName);
                $("#sm-email-post-ad-image-object-size").val(mediaS3Object.fileSize);
                SubmitSMEmailPost();
            }
        });


    } else {
        $("#sm-email-post-ad-image-object-key").val("");
        $("#sm-email-post-ad-image-object-name").val("");
        $("#sm-email-post-ad-image-object-size").val("");
        SubmitSMEmailPost();
    }
};

SubmitSMEmailPost = function () {
    {
        var frmData = new FormData($frm_email_post[0]);
        if (frmData.get('ButtonLink')) {
            frmData.set('ButtonLink', "https://" + frmData.get('ButtonLink'))
        }
        frmData.append("BodyContent", escape($("#editor .ql-editor").html()));
        $.ajax({
            type: "post",
            cache: false,
            url: "/SalesMarketing/SaveEmailPost",
            enctype: 'multipart/form-data',
            data: frmData,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                isBusy = true;
            },
            success: function (data) {
                if (data.result) {
                    if ($("#Id").val() == 0) {
                        $('#app-marketing-email-post-add').modal('hide');
                        cleanBookNotification.success(_L("ERROR_MSG_177"), "Sales Marketing");
                    } else {
                        $('#app-marketing-email-post-edit').modal('hide');
                        cleanBookNotification.success(_L("ERROR_MSG_369"), "Sales Marketing");
                        location.reload();
                    }

                    $frm_email_post.trigger("reset");
                    $('.nav-marketing a[data-target="#network1-content-approvals"]').trigger('click');
                    LoadCountEmailQueue();
                } else if (data.msg) {
                    cleanBookNotification.error(data.msg, "Sales Marketing");
                }

            },
            error: function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                LoadingOverlayEnd();
            }

        }).always(function () {
            LoadingOverlayEnd();
        });
    }
}


function getEmailTemplate(elm) {
    var id = $(elm).val();
    if (id == 0) {
        $('#frm_marketing-email-post input[name=Headline]').val('');
        $quill.root.innerHTML = '';
        $('#frm_marketing-email-post input[name=ButtonText]').parent().parent().parent().show();
        $('#frm_marketing-email-post input[name=ButtonLink]').parent().parent().parent().show();
        $('#frm_marketing-email-post select[name=slAdvImage]').parent().parent().parent().show();
        $('#frm_marketing-email-post input[name=ButtonText]').val('');
        $('#frm_marketing-email-post input[name=ButtonLink]').val('');
        $('#initFeaturedImage').val('');
        $('#promotionalImg').val(0);
        $('#initAdvImg').val('');
        $('#adImg').val(0);
        $('#frm_marketing-email-post select[name=slpromotionalImg]').val('2').change();
        $('#frm_marketing-email-post select[name=slAdvImage]').val('2').change();
    } else {
        $.get("/SalesMarketing/GetEmailTemplate?id=" + id, function (response) {
            $('#frm_marketing-email-post input[name=Headline]').val(response.HeadlineText);
            $quill.root.innerHTML = '<div>' + response.BodyContent + '</div>';
            $('#initFeaturedImage').val(response.FeaturedImage);
            $('#frm_marketing-email-post select[name=slpromotionalImg]').val(1).change();
            if (response.ButtonIsHidden) {
                $('#frm_marketing-email-post input[name=ButtonText]').parent().parent().parent().show();
                $('#frm_marketing-email-post input[name=ButtonLink]').parent().parent().parent().show();
                $('#frm_marketing-email-post input[name=ButtonText]').val(response.ButtonText);
                $('#frm_marketing-email-post input[name=ButtonLink]').val(response.ButtonLink.replace('https://', ''));
            } else {
                $('#frm_marketing-email-post input[name=ButtonText]').parent().parent().parent().hide();
                $('#frm_marketing-email-post input[name=ButtonLink]').parent().parent().parent().hide();
            }
            if (response.AdvertImgiIsHidden) {
                $('#initAdvImg').val(response.AdvertImage);
                $('#frm_marketing-email-post select[name=slAdvImage]').val(1).change();
                $('#frm_marketing-email-post select[name=slAdvImage]').parent().parent().parent().show();
            } else {
                $('#initAdvImg').val('');
                $('#adImg').val(0);
                $('#frm_marketing-email-post select[name=slAdvImage]').val('2').change();
                $('#frm_marketing-email-post select[name=slAdvImage]').parent().parent().parent().hide();
                $('.rowadvres').hide();
            }
        });
    }
}