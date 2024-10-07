var $qbicleModal = $('#edit-qbicle');
var $form = $("#form_qbicle_addedit");
function SaveQbicleSettingsEdit() {

    if (!$form.valid())
        return;
    var $qbicId = $("#qbicId"),
        $qbicName = $('#qbicName');
    $.ajax({
        url: "/Qbicles/DuplicateQbicleNameCheck",
        data: { key: $qbicId.val(), qbicName: $qbicName.val()},
        type: "GET",
        dataType: "json",
        async: false
    }).done(function (refModel) {
        if (refModel.result)
            $form.validate().showErrors({ Name: _L("ERROR_MSG_50") });
        else {
            ProcessQbicleLogoEdit();
        }

    }).fail(function () {
        $form.validate().showErrors({ Name: _L("ERROR_MSG_42") });
    })
}
function QbiclesSettingsSubmitEdit() {
    var formData = new FormData(document.getElementById("form_qbicle_addedit"));

    $.ajax({
        type: "post",
        cache: false,
        url: "/Qbicles/SaveQbicle",
        enctype: 'multipart/form-data',
        data: formData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
        },
        success: function (data) {
            if (data.result) {
                $qbicleModal.modal("toggle");
                cleanBookNotification.updateSuccess();
                location.reload();
                //ApplySearch();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    }).always(function () {
        LoadingOverlayEnd();
    });
}

function ProcessQbicleLogoEdit() {
    $.LoadingOverlay("show");
    var files = document.getElementById("qbicle-upload-logo").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("qbicle-upload-logo").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#qbicle-logo-uri").val(mediaS3Object.objectKey);
                QbiclesSettingsSubmitEdit();
            }
        });

    } else
        QbiclesSettingsSubmitEdit();
};
function GetQbicle(key) {
    return $.ajax({
        url: "/Qbicles/GetQbicle",
        type: "GET",
        dataType: "json",
        data: { key: key }
    });
}
$(document).ready(function () {
    function readURL(input, target) {

        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $(target).attr('src', e.target.result);
            }

            reader.readAsDataURL(input.files[0]);
        }

    }
    $(".previewimg").change(function () {
        var target = $(this).data('target');
        readURL(this, target);
        $(target).fadeIn();
    });
    // Shoehorn avatar support into select2 plugin
    function SformatOptions(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    function SformatSelected(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    $('.select2avatar-setting').select2({
        placeholder: 'Please select',
        templateResult: SformatOptions,
        templateSelection: SformatSelected
    });
    $qbicleModal.on('show.bs.modal', function (e) {


        var $qbicId = $("#qbicId"),
            $qbicName = $('#qbicName'),
            $qbicleImg = $('#qbicleImg'),
            $qbicDescription = $('#qbicDescription'),
            $qbicleTitle = $('.qbicle-title'),
            $qbicUserDomainSelect = $('#qbicUserDomainSelect'),
            $qbicOwnerSelect = $('#qbicOwnerSelect'),
            $loadingSettings = $('#loadingSettings');


        $qbicName.hide();
        $qbicDescription.hide();
        $("#q-name").hide();
        $("#q-des").hide();
        $("#qbicle-upload-logo").hide();
        $("#q-i-change").hide();
        $("#q-i-pre").hide();
        //waiting icon
        $qbicUserDomainSelect.next().hide();
        $loadingSettings.show();
        //end

        GetQbicle($qbicId.val()).done(function (cube) {
            if (cube.QbicleKey) {

                if (cube.IsMemberQbicle) {
                    $qbicName.show();
                    $qbicDescription.show();
                    $("#qbicle-upload-logo").show();
                    $("#q-i-change").show();
                }
                else {
                    $("#q-name").show();
                    $("#q-i-pre").show();
                    $("#q-des").show();
                }
                $("#q-name").text(cube.Name);
                $("#q-des").text(cube.Description);

                $qbicName.val(cube.Name);
                $qbicDescription.val(cube.Description);
                $qbicleImg.attr('src', cube.LogoUri);
                $qbicleTitle.text(cube.Name);

                var arrayUser = cube.CubeUser.split(',');
                $qbicUserDomainSelect.val(arrayUser).change();
                var api = $('#api-uri').val();
                var lstManager = cube.QbicManager;
                if (lstManager.length > 0) {
                    var selected = "";
                    $('#qbicOwnerSelect option').remove();
                    var strManager = '<option value=""></option>';
                    for (var i = 0; i < lstManager.length; ++i) {
                        selected = lstManager[i].Id == cube.Manager ? "selected" : "";
                        strManager += '<option ' + selected + ' avatarUrl="' + api + '' + lstManager[i].ProfilePic + '" value="' + lstManager[i].Id + '">' + lstManager[i].Forename + " " + lstManager[i].Surname + '</option>';
                    }
                    $qbicOwnerSelect.append(strManager);
                }

                //$qbicOwnerSelect.val(cube.Manager).trigger('change');
                $qbicOwnerSelect.val(cube.Manager).change();
                //loaded
                $qbicUserDomainSelect.next().show();
                $loadingSettings.hide();
                $form.valid();
                $form.validate().resetForm();
                //end
            }
        });
    })
});