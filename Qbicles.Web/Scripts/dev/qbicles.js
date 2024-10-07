$.fn.modal.prototype.constructor.Constructor.DEFAULTS.backdrop = 'static';
$.fn.modal.prototype.constructor.Constructor.DEFAULTS.keyboard = false;

var $qbicleCreateModal = $("#create-qbicle"),
    $modal_group_title = $("#create-qbicle [class='modal-title']"),
    $form_qbicle_addedit2 = $("#form_qbicle_addedit2"),
    $qbicName = $('#qbicName2'),
    $qbicImage = $('#qbicImage2'),
    $qbicDescription = $('#qbicDescription2'),
    $qbicScopeSelect = $('#qbicScopeSelect'),
    $qbicUserDomainSelect = $('#qbicUserDomainSelect2'),
    //$qbicId = $("#qbicId2"),
    $selectDomain = $("#domainId");
var $open_close = 0;
var $cubeOpen = true,
    $cubeClosed = true,
    $cubePublic = true,
    $cubePrivate = true,
    $cubeSearch = "";

var $checkPublic = $("#checkPublic"),
    $checkPrivate = $("#checkPrivate"),
    $checkOpen = $("#checkOpen"),
    $checkClosed = $("#checkClosed");
var $selectPeople = $("#filter-people"),
    $selectTopic = $("#filter-topics"),
    $nameSearch = $("#searchQbicles");

var $qbicleGuests = $("#qbicleGuests");
jQuery(function ($) {
    $('.td-scroll-up').click(function () {
        $("html, body").animate({ scrollTop: "0px" }, 'slow');
    });
    var triggers = setInterval(function () {
        if (document.readyState === "complete") {
            setTimeout(function () {
                $('.td-scroll-up').click();
            }, 200);

            clearInterval(triggers);
            return;
        }

    }, 200);

    //$qbicUserDomainSelect.select2();
    $qbicUserDomainSelect.on("select2:unselecting", function (e) {
        if (e.params.args.data.id === currentUserId)
            e.preventDefault();
    });
    ////get list users associated with the Domain has selected in the dropdown
    $selectDomain.change(function () {
        $("#loadingAddEditGif").show();
        $("#selectUser").hide();
        var domainSelected = $(this).find("option:selected");
        $.ajax({
            type: 'post',
            url: '/Domain/GetUsersByDomainId',
            datatype: 'json',
            data: {
                domainId: domainSelected.val()
            },
            success: function (refModel) {
                if (refModel.result) {
                    $qbicUserDomainSelect.empty().append(refModel.Object);
                }
                $("#loadingAddEditGif").hide();
                $("#selectUser").show();
            }
        });
    });

    // set name,logo for Domain selected
    $('#qbicleNameSelected').text('@Model.Name');
    $("#qbicleLogoSelect").attr("src", "");
    //input guests
    $('#qbicleGuests').on('beforeItemAdd', function (event) {
        var tag = event.item;
        // Do some processing here
        if (!event.options || !event.options.preventPost) {
            if (validateEmail(tag)) {
                $.ajax({
                    type: 'post',
                    url: '/Account/CheckUserEmailInSystem',
                    datatype: 'json',
                    data: {
                        userEmail: event.item
                    },
                    success: function (refModel) {
                        if (refModel.result) {
                            $('#qbicleGuests').tagsinput('remove', tag, { preventPost: true });
                            event.item = refModel.msg;//exist user in the system, set by user's name
                            $('#qbicleGuests').tagsinput('add', event.item, { preventPost: true });
                        }
                    }
                });
                return event;
            }
            else {
                event.cancel = true;
            }
        }
    });



    // Shoehorn avatar support into select2 plugin

    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: formatOptions,
        templateSelection: formatSelected,
        dropdownCssClass: 'withdrop'
    });
    $('.select2avatar').select2({
        placeholder: 'Please select',
        templateResult: formatOptions,
        templateSelection: formatSelected
    });
    $('#searchQbicles').keyup(delay(function () {
        ApplySearch();
    }, 1000));
    $("#order").on("change", function () {
        ApplySearch();
    });
    $("#isShowHidden").on("change", function () {
        ApplySearch();
    });
});

function formatOptions(state) {

    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function formatSelected(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

var loadImg = function (event) {
    var output = document.getElementById('qbicleImg');
    output.src = URL.createObjectURL(event.target.files[0]);
};
var loadImgCreateQbicle = function (event) {
    $("#qbicleImg2").attr("src", URL.createObjectURL(event.target.files[0]));
    $("#qbicleImg2").css({ "display": "block" });
};
function AddnewQbicleClick() {

    $("#close-reopen-qbicle").hide();
    $selectDomain.removeAttr('disabled', '');
    ClearError();
    //$qbicId.val(0);
    $("#qbicleImg2").attr("src", "");
    $("#qbicleImg2").css({ "display": "none" });
    $("#btnPrevious").click();
    $('#form_qbicle_addedit2').find('[name="qbicName"]').focus();
    //$("#Scope").val($("#Scope option:first").val());
    $modal_group_title.text($("input[name='modal_add_title']").val());
};


function ClearInputModalAddEdit() {
    //$qbicId.val(0);
    $qbicName.val('');
    $qbicImage.val('');
    $("#qbicleImg2").attr("src", "");
    $qbicDescription.val('');
    $("#qbicUserDomainSelect2").val(currentUserId).trigger("change");
    $("#managerId").val(currentUserId).trigger("change");
    ClearError();
};
function SaveQbicleFromDomain() {
    if ($qbicName.val() === "") {
        $("#btnPrevious").click();
        $('#form_qbicle_addedit2').valid();
    }
    else {

        QbicleValidationForm();
    }

};
function QbicleValidationForm() {
    if ($('#form_qbicle_addedit2').valid()) {
        $.ajax({
            url: "/Qbicles/DuplicateQbicleNameCheck",
            data: { key: "0", qbicName: $qbicName.val()},
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result) {
                $("#btnPrevious").click();
                $("#form_qbicle_addedit2").validate().showErrors({ Name: _L("ERROR_MSG_41") });
            }
            else {
                ProcessQbicleDomainLogo();
            }
        }).fail(function () {
            $("#btnPrevious").click();
            setTimeout(function () {
                $("#form_qbicle_addedit2").validate().showErrors({ Name: _L("ERROR_MSG_42") });
            }, 100);

        });
    }
}

function ProcessQbicleDomainLogo() {
    $.LoadingOverlay("show");
    var files = document.getElementById("qbicImage2").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("qbicImage2").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#qbicle-domain-logo-uri").val(mediaS3Object.objectKey);
                QbiclesDomainSubmit();
            }
        });

    } else
        QbiclesDomainSubmit();
};
QbiclesDomainSubmit = function () {
    var formData = new FormData(document.getElementById("form_qbicle_addedit2"));
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
                $qbicleCreateModal.modal("toggle");
                cleanBookNotification.updateSuccess();
                ClearInputModalAddEdit();
                ApplySearch();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            setTimeout(function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                $.LoadingOverlay("hide");
            }, 2000);
        }

    }).always(function () {
        LoadingOverlayEnd();
    });
}

function ApplySearch() {
    $('#qbicles-list').LoadingOverlay("show");
    var cubeParameter = {
        Open: $checkOpen.is(':checked'),
        Closed: $checkClosed.is(':checked'),
        Name: $nameSearch.val(),
        Peoples: $selectPeople.val(),
        Topics: $selectTopic.val(),
        order: $("#order :selected").val(),
        IsShowHidden: $("#isShowHidden").is(':checked') ? true : false
    };
    $("#remove-filters").removeAttr("disabled");
    $.ajax({
        type: 'post',
        url: '/Qbicles/ApplyFilterQbicle',
        data: cubeParameter,
        dataType: 'html',
        success: function (response) {
            if (response !== "") {
                $("#qbicles-dash-grid").empty();
                $("#qbicles-dash-grid").append(response);

            }
        },
        error: function (er) {
           
        }
    }).always(function () {
        $('#qbicles-list').LoadingOverlay("hide", true);
    });
};

function ShowOrHideQbicle(key, isHidden) {
    $.ajax({
        type: 'post',
        url: '/Qbicles/ShowOrHideQbicle',
        data: { key: key, isHidden: !isHidden },
        dataType: "json",
        success: function (res) {
            if (res.result) {
                ApplySearch();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function ConfigQbicle(key) {
    $("#qbicId").val(key);
}

function RemoveFilter() {
    $checkOpen.prop("checked", true).trigger("change");
    $checkClosed.prop("checked", false).trigger("change");
    $nameSearch.val('');
    $selectPeople.val('').trigger("change");
    $selectTopic.val('').trigger("change");
    $("#order").val(4).trigger("change");
    $("#isShowHidden").bootstrapToggle("off");
    ApplySearch();
    $("#remove-filters").attr("disabled", "disabled");
}

/*Qbicle Filter*/
var $qbicleModal = $('#edit-qbicle');
var $form = $("#form_qbicle_addedit");
var previous_shown = false;
var $isFilter = false;
var isServerBusy = false;

function UpdateQbiclesSettings() {
    $.LoadingOverlay("show");
    var form = $form[0];
    $.ajax({
        type: form.method,
        cache: false,
        url: "/Qbicles/SaveQbicle",
        enctype: 'multipart/form-data',
        data: new FormData(form),
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
        },
        success: function (data) {
            if (data.result) {
                $qbicleModal.modal("toggle");
                cleanBookNotification.updateSuccess();
                location.reload(true);
                if (ApplySearch !== undefined)
                    ApplySearch();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
            setTimeout(function () {
                $.LoadingOverlay("hide");
            }, 500);
        },
        error: function (data) {
            setTimeout(function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                $.LoadingOverlay("hide");
            }, 500);
        }

    });
}
function GetQbicle(key) {
    return $.ajax({
        url: "/Qbicles/GetQbicle",
        type: "GET",
        dataType: "json",
        data: { key: key }
    });
}