var $form_media_addedit = $("#form_media_sm_idea_resource"), isBusyAddTaskForm = false;
function SocialIdeaAdd() {
    $(".previewimgidea").change(function () {
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });
    var $frm_idea_theme = $('#frm-idea-theme');
    $frm_idea_theme.validate({
        rules: {
            Name: {
                required: true,
                minlength: 5,
                maxlength: 35
            },
            Explanation: {
                required: true,
                maxlength: 200
            },
            Url: {
                url: true
            }
        }
    });

}
function SocialIdeaAutoGenerateFolderName() {
    $.getJSON("/SalesMarketingIdea/AutoGenerateFolderName", function (data) {
        if (data) {
            $('#ideaFolderName').val(data);
        }
    });
}
function SocialMarketingIdeaMediaSave(qbicleId) {
   
    if ($form_media_addedit.valid()) {
        $.ajax({
            url: "/Medias/DuplicateMediaNameCheck",
            data: { qbicleId: qbicleId, mediaId: 0, MediaName: $('#form_media_smresource input[name=name]').val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                $form_media_addedit.validate().showErrors({ name: "Name of Media already exists." });
            else {
                ProcessSaleMarketingIdeaResource();
            }
        }).fail(function () {
            $form_media_addedit.validate().showErrors({ name: _L("ERROR_MSG_75") });
        })
    }
};

function ProcessSaleMarketingIdeaResource() {
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-idea-resource-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-idea-resource-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-idea-resource-object-key").val(mediaS3Object.objectKey);
                $("#sm-idea-resource-object-name").val(mediaS3Object.fileName);
                $("#sm-idea-attribute-object-size").val(mediaS3Object.fileSize);

                SubmitSocialMediaBranchResourceSaveMedia();
            }
        });

    }
    else {
        $("#sm-idea-resource-object-key").val("");
        $("#sm-idea-resource-object-name").val("");
        $("#sm-idea-resource-object-size").val("");
        SubmitSocialMediaBranchResourceSaveMedia();
    }
};

SubmitSocialMediaBranchResourceSaveMedia = function () {
    var frmData = new FormData($form_media_addedit[0]);
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
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#create-resource').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_197"), "Sales Marketing");
                SocialMediasByIdea($('#hdfmediaFolderId').val(), $('#media-qbicleId').val());
                $form_media_addedit.trigger("reset");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
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
//End Modal

function SocialMediasByIdea(fid, qid) {
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/SalesMarketingIdea/LoadMediasByIdea',
        datatype: 'json',
        data: { fid: fid, qid: qid },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#idea-content');
                $divcontain.html(listMedia);
                totop();
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function SocialIdeaAddLink() {
    var $elurl = $('#frm-idea-theme input[name=Url]');
    if ($elurl.valid()) {
        var idealnk = extractHostname($elurl);
        if ($('#ideaLinks tr[lnkUrl="' + idealnk + '"]').length == 0) {
            $('#ideaLinks').show();
            var $op = '<tr lnkUrl="' + idealnk + '"><td><a href="' + idealnk + '" target="_blank"><i class="fa fa-external-link"></i> &nbsp; ' + idealnk + '</a></td><td> <button class="btn btn-danger" onclick="SocialIdeaRemoveLink(this)"><i class="fa fa-trash"></i></button></td></tr>';
            $('#ideaLinks').append($op);
            $('.links-associate').show();
            $elurl.val('');
        } else {
            $elurl.val('');
        }
    }
}
function SocialIdeaRemoveLink(el) {
    $(el).parent().parent().remove();
    if ($('#ideaLinks tr').length == 1) $('#ideaLinks').hide();
}
function extractHostname(el) {
    var url = document.createElement('a');
    url.href = $(el).val();
    if (url.origin) {
        return url.origin;
    }
    return '';
}
function SocialCreateDiscussion() {
    if (isBusyAddTaskForm) {
        return;
    }
    $('#frm-create-discussion').submit(function (e) {
        e.preventDefault();
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Discussions/SaveDiscussion',
            datatype: 'json',
            data: { ideaId: $('#ideaId').val(), openingmessage: $('#ds_openingmessage').val(), isexpiry: $('#ds_isexpiry').prop('checked'), expirydate: $('#ds_expirydate').val() },
            beforeSend: function (xhr) {
                isBusyAddTaskForm = true;
            },
            success: function (data) {
                if (data.result) {
                    $('.new-discuss').hide();
                    var elbtnDis = $('#btnJoinDiscussion');
                    if (data.Object.Id > 0) {
                        var elhref = elbtnDis.attr("href") + "?disId=" + data.Object.Id;
                        elbtnDis.attr("href", elhref);
                        elbtnDis.show();
                    }
                    cleanBookNotification.success(_L("ERROR_MSG_196"), "Sales Marketing");
                    $('#create-discussion').modal('hide');

                } else if (data.msg) {
                    cleanBookNotification.error(data.msg, "Sales Marketing");
                }
                isBusyAddTaskForm = false;
                LoadingOverlayEnd();
            },
            error: function (err) {
                isBusyAddTaskForm = false;
                LoadingOverlayEnd();
            }
        });
    });
}
function LoadCampaignsInTheme() {
    $("#community-list").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#community-list").LoadingOverlay("show");
        } else {
            $("#community-list").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketingIdea/LoadCampaignsInTheme',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "themeId": $("#ideaId").val(),
                    "types": $("#campaignType").val(),
                    "search": $("#searchCampaign").val()
                });
            }
        },
        "columns": [
            {
                data: "PostTitle",
                orderable: false
            },
            {
                data: "CampaignName",
                orderable: false
            },
            {
                data: "Type",
                orderable: false
            },
            {
                data: "StrDateOfIssue",
                orderable: false
            },
            {
                data: null,
                orderable: false,
                render: function (value, type, row) {
                    if (row.Status == "Complete") {
                        var str = '<span class="label label-lg label-success">Complete</span>'
                        return str;
                    } else {
                        var str = '<span class="label label-lg label-primary">Queued</span>'
                    }
                    return str;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (value, type, row) {
                    var url = "";
                    if (row.Type == "Automated Social") {
                        url = "/SalesMarketing/SocialPostInApp?id=" + row.PostId;
                    } else if (row.Type == "Manual Social") {
                        url = "/SalesMarketing/ManualSocialPostInApp?id=" + row.PostId;
                    } else {
                        url = "/SalesMarketing/EmailPostInApp?id=" + row.PostId;
                    }

                    var str = '<button class="btn btn-info" onclick="window.location.href=\'' + url + '\';"><i class="fa fa-eye"></i> &nbsp; View Post</button>';

                    return str;
                }
            }
        ],
        "initComplete": function (settings, json) {
            $('#community-list').DataTable().ajax.reload();
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
    LoadCampaignsInTheme();

    SocialIdeaAdd();
    SocialCreateDiscussion();
});
function readImgURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function ProcessSMIdea() {
    var ideatype = $('#frm-idea-theme select[name=Type]').val();
    if (!ideatype) {
        $frm_idea_theme.validate().showErrors({ Type: "This field is required." });
        return;
    }
    var folder = $('#frm-idea-theme select[name=ResourcesFolder]').val();
    if (!folder) {
        $frm_idea_theme.validate().showErrors({ ResourcesFolder: "This field is required." });
        return;
    }
    if (folder == "0" && !$('#ideaFolderName').val()) {
        $frm_idea_theme.validate().showErrors({ FolderName: "This field is required." });
        return;
    }
    if (!$('#frm-idea-theme').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-idea-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-idea-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {


                $("#sm-idea-object-key").val(mediaS3Object.objectKey);
                $("#sm-idea-object-name").val(mediaS3Object.fileName);
                $("#sm-idea-object-size").val(mediaS3Object.fileSize);

                SubmitSMIdea();
            }
        });

    }
    else {
        $("#sm-idea-object-key").val("");
        $("#sm-idea-object-name").val("");
        $("#sm-idea-object-size").val("");
        SubmitSMIdea();
    }
}

function SubmitSMIdea() {
    var frmData = new FormData($('#frm-idea-theme')[0]);
    var lnks = [];
    $('#ideaLinks tr[lnkUrl]').each(function (index) {
        frmData.append("Links[]", $(this).attr("lnkUrl"));
    });
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingIdea/SaveIdea",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-idea-add').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_198"), "Sales Marketing");
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");

            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}