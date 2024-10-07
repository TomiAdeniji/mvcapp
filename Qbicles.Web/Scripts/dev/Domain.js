var isBusy = false;
function LoadDomainList() {
    $.ajax({
        type: "POST",
        datatype: "json",
        cache: false,
        data: { title: $("#searchDomain").val(), order: $("#order :selected").val() },
        url: "/Domain/SearchDomain",
        success: function (data) {
            
            if (data.result) {
                if (data.Object) {
                    $(".qbicles-list-v2 li").remove();
                    $(".qbicles-list-v2").append(data.Object.strResult);
                }
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

            
        }

    });
}
$(function () {
    $("#searchDomain").on("keyup", function () {
        LoadDomainList();
    });
    $("#order").on("change", function () {
        LoadDomainList();
    });
    $('#form_domain_addedit').validate({
        ignore: "",
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $(".previewimg").change(function () {
        var target = $(this).data('target');
        ReadURL(this, target);
        $(target).fadeIn();
    });
})

function updateOrInserDomain() {
    if ($('#form_domain_addedit').valid()) {
        var $domainKey = $('#domainId-UI');
        $.ajax({
            url: "/Domain/IsDuplicateDomainNameCheck",
            data: { domainKey: ($domainKey.length > 0 ? $domainKey.val() : ""), domainName: $('#domainName-UI').val() },
            type: "POST",
        }).done(function (isDuplicate) {
            var $tabShow = $('a[href="#dom-1"]');
            if (isDuplicate === true) {
                $('#form_domain_addedit').validate().showErrors({ "domain.Name": _L("ERROR_MSG_9") });
                if ($tabShow.length > 0)
                    $tabShow.tab('show');
            }
            else {
                ProcessDomainLogo();
            }
        }).fail(function () {
            $('#form_domain_addedit').validate().showErrors({ "domain.Name": _L("ERROR_MSG_10") });
            var $tabShow = $('a[href="#dom-1"]');
            if ($tabShow.length > 0)
                $tabShow.tab('show');
        })
    }
}
function ProcessDomainLogo() {
    $.LoadingOverlay("show");
    var fileLogoDomain = document.getElementById("logoDomain-UI");
    var fileLogoQbicle = document.getElementById("logoQbicle-UI");
    if (fileLogoDomain && fileLogoDomain.files.length > 0 && fileLogoQbicle && fileLogoQbicle.files.length > 0) {
        Promise.all([UploadMediaS3ClientSide("logoDomain-UI"), UploadMediaS3ClientSide("logoQbicle-UI")]).then(mediaS3Object => {
            if ((mediaS3Object[0].objectKey === "no_image" || mediaS3Object[0].objectKey === "no-image") && (mediaS3Object[1].objectKey === "no_image" || mediaS3Object[1].objectKey === "no-image")) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            } else {
                $("#domain-logo-uri").val(mediaS3Object[0].objectKey);
                $("#qbicle-logo-uri").val(mediaS3Object[1].objectKey);
                $('#logoDomain-UI').val(null);
                $('#logoQbicle-UI').val(null);
                DomainSave();
            }
        }).catch(reason => {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            return;
        });
    } else if (fileLogoDomain && fileLogoDomain.files.length > 0 && (!fileLogoQbicle || fileLogoQbicle.files.length == 0)) {
        UploadMediaS3ClientSide("logoDomain-UI").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#domain-logo-uri").val(mediaS3Object.objectKey);
                $('#logoDomain-UI').val(null);
                DomainSave();
            }
        });
    } else if (fileLogoQbicle && fileLogoQbicle.files.length > 0 && (!fileLogoDomain || fileLogoDomain.files.length == 0)) {
        UploadMediaS3ClientSide("logoQbicle-UI").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#qbicle-logo-uri").val(mediaS3Object.objectKey);
                $('#logoQbicle-UI').val(null);
                DomainSave();
            }
        });
    } else
        DomainSave();
};
function DomainSave() {
    var model = new FormData(document.getElementById("form_domain_addedit"));
    $.ajax({
        type: "post",
        cache: false,
        url: "/Domain/UpdateOrInsertDomain",
        enctype: 'multipart/form-data',
        data: model,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.result) {

                //Delete this view
                if (location.href.indexOf('AdminAccount') > 0) {
                    $("#tbl-associated-domains").DataTable().ajax.reload();
                    $("#create-domain").modal('hide');
                }
                else {
                    //if ($(".qbicles-list-v2").length>0)
                    //    LoadDomainList();
                    clearDomainInputModalAddEdit();
                    $('#create-domain').modal('toggle');
                    var $domainKey = $('#domainId-UI');
                    if (($domainKey.length == 0 || $domainKey.val() == "") && data.Object)
                        UpdateCurrentDomain(data.Object, '');
                }

                cleanBookNotification.updateSuccess();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
            else {
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
function SetBusinessDomain(par) {
    $('#sl_hdf_domainType').val(par);
    clearDomainInputModalAddEdit();
}
function clearDomainInputModalAddEdit() {
    $('#domain-logo').attr('src', "");
    $('#domain-logo').css({ "display": "none" });
    $('#domainName-UI').val("");
    $('#logoDomain-UI').val("");
    $('#domain-logo-uri').val("");
    $('#domainlogo_preview').attr('src', "");
    $('#domainlogo_preview').css({ "display": "none" });
    $('input[name="initQbicle.Name"]').val("");
    $('textarea[name="initQbicle.Description"]').val("");
    $('#logoQbicle-UI').val("");
    $('#qbicle-logo-uri').val("");
    $('#qbiclelogo_preview').attr('src', "");
    $('#qbiclelogo_preview').css({ "display": "none" });
    ClearError();
}
function ReadURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }

}
function UpdateCurrentDomain(domainkey) {
    $.ajax({
        type: 'post',
        url: '/Commons/UpdateCurrentDomain',
        datatype: 'json',
        cache: false,
        data: {
            currentDomainKey: domainkey
        },
        success: function (refModel) {
            if (refModel.result) {
                window.location.href = '/Commerce/BusinessProfile';
            }
        }
    });
};

function CreateDomainRequest() {
    if (isBusy)
        return;

    if ($('#form_domain_addedit').valid()) {
        var $domainKey = $('#domainId-UI');
        $.ajax({
            url: "/Domain/IsDuplicateDomainNameCheck",
            data: { domainKey: ($domainKey.length > 0 ? $domainKey.val() : ""), domainName: $('#domainName-UI').val() },
            type: "POST",
            beforeSend: function (xhr) {
                isBusy = true;
            },
        }).done(function (isDuplicate) {
            var $tabShow = $('a[href="#dom-1"]');
            if (isDuplicate === true) {
                isBusy = false;
                $('#form_domain_addedit').validate().showErrors({ "domain.Name": _L("ERROR_MSG_9") });
                if ($tabShow.length > 0)
                    $tabShow.tab('show');
            }
            else {
                ProcessDomainRequestLogos();
            }
        }).fail(function () {
            isBusy = false;
            $('#form_domain_addedit').validate().showErrors({ "domain.Name": _L("ERROR_MSG_10") });
            var $tabShow = $('a[href="#dom-1"]');
            if ($tabShow.length > 0)
                $tabShow.tab('show');
        })
    }
}

function ProcessDomainRequestLogos() {
    $.LoadingOverlay("show");
    var fileLogoDomain = document.getElementById("logoDomain-UI");
    var fileLogoQbicle = document.getElementById("logoQbicle-UI");
    if (fileLogoDomain && fileLogoDomain.files.length > 0 && fileLogoQbicle && fileLogoQbicle.files.length > 0) {
        Promise.all([UploadMediaS3ClientSide("logoDomain-UI"), UploadMediaS3ClientSide("logoQbicle-UI")]).then(mediaS3Object => {
            if ((mediaS3Object[0].objectKey === "no_image" || mediaS3Object[0].objectKey === "no-image") && (mediaS3Object[1].objectKey === "no_image" || mediaS3Object[1].objectKey === "no-image")) {
                LoadingOverlayEnd();
                isBusy = false;
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            } else {
                $("#domain-logo-uri").val(mediaS3Object[0].objectKey);
                $("#qbicle-logo-uri").val(mediaS3Object[1].objectKey);
                $('#logoDomain-UI').val(null);
                $('#logoQbicle-UI').val(null);
                DomainRequestSave();
            }
        }).catch(reason => {
            LoadingOverlayEnd();
            isBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            return;
        });
    } else if (fileLogoDomain && fileLogoDomain.files.length > 0 && (!fileLogoQbicle || fileLogoQbicle.files.length == 0)) {
        UploadMediaS3ClientSide("logoDomain-UI").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                isBusy = false;
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#domain-logo-uri").val(mediaS3Object.objectKey);
                $('#logoDomain-UI').val(null);
                DomainRequestSave();
            }
        });
    } else if (fileLogoQbicle && fileLogoQbicle.files.length > 0 && (!fileLogoDomain || fileLogoDomain.files.length == 0)) {
        UploadMediaS3ClientSide("logoQbicle-UI").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                isBusy = false;
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#qbicle-logo-uri").val(mediaS3Object.objectKey);
                $('#logoQbicle-UI').val(null);
                DomainRequestSave();
            }
        });
    } else
        DomainRequestSave();
}
function DomainRequestSave() {
    var model = new FormData(document.getElementById("form_domain_addedit"));
    $.ajax({
        type: "post",
        cache: false,
        url: "/Domain/AddQbicleDomainRequest",
        enctype: 'multipart/form-data',
        data: model,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.result) {
                clearDomainInputModalAddEdit();
                $('#create-domain').modal('toggle');
                showDomainRequestHistory();
                cleanBookNotification.updateSuccess();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            
        }


    }).always(function () {
        isBusy = false;
        LoadingOverlayEnd();
    });
}

function initDomainIndexView() {
    $('#domain-request-daterange').daterangepicker({
        timePicker: false,
        showDropdowns: true,
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            dateFormat: $dateFormatByUser.toUpperCase()
        }
    });

    $('#domain-request-daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $("#domain-request-history").DataTable().ajax.reload();
    });
    $('#domain-request-daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        $("#domain-request-history").DataTable().ajax.reload();
    });

    $("#domain-request-key").keyup(delay(function () {
        $("#domain-request-history").DataTable().ajax.reload();
    }, 1000));

    $('.dash-carousel').on("initialize.owl.carousel", function (event) {
        $("#create-domain-block").removeAttr("hidden");
    });

    $('.dash-carousel').owlCarousel({
        items: 1,
        loop: true,
        animateIn: 'fadeIn',
        animateOut: 'fadeOut',
        dots: false,
        nav: false,
        autoplay: false,
        autoplayHoverPause: false,
        mouseDrag: false,
        autoplayTimeout: 5000,
        navText: ["<i class='fa fa-angle-left'></i>", "<i class='fa fa-angle-right'></i>"]
    });
    initUserDomainRequestTable();
}

