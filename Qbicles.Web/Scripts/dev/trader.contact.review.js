var $contactid = $('#contact-id').val();
var contactmanage = 'contactmanage';
function selectTab() {
    var activeTab = getLocalStorage(contactmanage);
    if (activeTab)
        $('a[href="#' + activeTab + '"]').tab('show');
    removeLocalStorage(contactmanage);
}
$(function () {
    selectTab();
});

//approval update
function UpdateStatusApproval(apprKey, mode) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");

    var contact = {
        Name: $("#contact-name").val(),
        Id: $("#contact-id").val(),
        Email: $("#contact-email").val()
    };

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: "/TraderContact/CheckExistedApprovedContact",
        data: {
            contact: contact
        }
    }).done(function (refModel) {
        if (refModel.result) {
            CheckStatusApproval(apprKey).then(function (res) {
                LoadingOverlayEnd();
                if (res.result && res.Object.toLocaleLowerCase() == statusOld.toLocaleLowerCase()) {
                    // apply 
                    $.LoadingOverlay("show");
                    $.ajax({
                        url: "/Qbicles/SetRequestStatusForApprovalRequest",
                        type: "GET",
                        dataType: "json",
                        data: { appKey: apprKey, status: $("#action_approval").val() },
                        success: function (rs) {
                            $.LoadingOverlay("hide");
                            if ($('#pagemode').val()) {
                                window.location.reload();
                                return false;
                            }
                            if (rs.actionVal > 0) {
                                RenderContentUpdated(true);
                            }
                        },
                        error: function (err) {
                            $.LoadingOverlay("hide");
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    });
                    // and apply
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
                    if (mode && mode === 'CreditNote') {
                        window.location.reload(true);
                    }
                    setTimeout(function () {
                        RenderContentUpdated();
                    }, 1500);
                }
            });
        } else {
            cleanBookNotification.error(refModel.msg, "Qbicles");
        }
    }).fail(function (er) {
        cleanBookNotification.error("Error checking existing Approved Trader Contact", "Qbicles");
    }).always(function () {
        LoadingOverlayEnd();
    });

};
function RenderContentUpdated(showMess) {
    LoadingOverlay();
    $contactid = $('#contact_id').val();
    $('#contact-content').empty();
    $('#contact-content').load('/TraderContact/ContactReview?id=' + $contactid + '&isReload=true', function () {
        LoadingOverlayEnd();
        if (showMess)
            setTimeout(function () { cleanBookNotification.updateSuccess(); }, 400);

    });
}
// get selected account
var BKAccount = {};
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    BKAccount = { Id: id, Name: name };
    $("#accountId").val(id);
    closeSelected();
    SaveContact();

    $("#app-bookkeeping-treeview").modal("toggle");
};

function closeSelected() {
    if (BKAccount.Id) {
        $(".accountInfo").empty();
        $(".accountInfo").append(BKAccount.Name);
    } else {
        $(".accountInfo").empty();
    }

    if ($(".accountInfo").text().length > 0) {
        $(".addbtnaccount").attr("style", "display:none;");
        $(".editbtnaccount").removeAttr("style");
    } else if ($(".accountInfo").text().length === 0) {
        $(".editbtnaccount").attr("style", "display:none;");
        $(".addbtnaccount").removeAttr("style");
    }
    //SaveContact();
};

// showChangeAccount buton edit account selected
function showChangeAccount() {
    setTimeout(function () {
        CollapseAccount();
    },
        1);

};

function CollapseAccount() {
    $(".jstree").jstree("close_all");
};


function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    },
        1);
};

// end account tree
var reader = new FileReader();
//save contact
function changeImageLogo(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#trader-contact-avatar-upload");
        reader.onload = function (e) {

            $(".contact-avatar").attr("src", e.target.result);
        }
        reader.readAsDataURL(image[0].files[0]);
        ChangeContactLogo();
    }
};

function ChangeContactLogo() {
    var files = document.getElementById("trader-contact-avatar-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("trader-contact-avatar-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $.ajax({
                type: 'post',
                url: '/TraderContact/ChangeContactLogo?id=' + $("#contact-id").val() + '&mediaObjectKey=' + mediaS3Object.objectKey,

                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                dataType: 'json',
                success: function () {
                    cleanBookNotification.updateSuccess();
                },
                error: function (er) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        });

    }

};

$("#group-workgroup-id").on("change",
    function () {
        SaveContact();
    });
$("#group-contact-id").on("change",
    function () {
        SaveContact();
    });
$("#CountryName").on("change",
    function () {
        SaveContact();
    });

function SaveContact() {
    $.LoadingOverlay("show");
    CheckStatus($("#contact-id").val(), 'Contact').then(function (res) {
        LoadingOverlayEnd();
        if (res.result && res.Object != "ContactApproved") {
            if ($("#group-contact-id").val() === null) {
                cleanBookNotification.error(_L("ERROR_MSG_262"), "Qbicles");
                $("#contact-group-label").addClass("text-red");
            }
            if ($("#group-workgroup-id").val() === null) {
                cleanBookNotification.error(_L("ERROR_MSG_263"), "Qbicles");
            }

            var group = {
                Id: $("#group-contact-id").val()
            };
            var workgroup = {
                Id: $("#group-workgroup-id").val()
            };
            var contact = {
                ContactGroup: group,
                Workgroup: workgroup,
                Name: $("#contact-name").val(),
                Id: $("#contact-id").val(),
                Email: $("#contact-email").val()
            };
            if ($("#form_contact_add").valid()) {
                if ($("#group-contact-id").val() === null) {
                    return;
                } if ($("#group-workgroup-id").val() === null) {
                    return;
                }
                $.ajax({
                    url: "/TraderContact/TraderContactNameCheck",
                    data: { contact: contact },
                    type: "POST",
                    dataType: "json",
                }).done(function (refModel) {
                    if (refModel.result)
                        $("#form_contact_add").validate()
                            .showErrors({ Name: "Name of Contact already exists." });
                    else {
                        $.ajax({
                            method: "POST",
                            dataType: "JSON",
                            url: "/TraderContact/CheckExistedApprovedContact",
                            data: {
                                contact: contact
                            }
                        }).done(function (checkResultModel) {
                            if (checkResultModel.result) {
                                $("#form_contact_add").trigger("submit");
                            } else {
                                cleanBookNotification.error(checkResultModel.msg, "Qbicles");
                            }
                        }).fail(function (er) {
                            cleanBookNotification.error("Error checking existing Approved Trader Contact", "Qbicles");
                        }).always(function () {
                            LoadingOverlayEnd();
                        });
                    }
                }).fail(function () {
                    $("#form_contact_add").validate()
                        .showErrors({ Name: "Error checking existing name of Contact." });
                });
            }
        } else if (res.result && res.Object == "ContactApproved") {
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setLocalStorage(contactmanage, $('ul.app_subnav li.active a').attr('href').replace('#', ''));
            setTimeout(function () { window.location.reload(); }, 1000);
        } else if (res.result == false) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });


};


$("#form_contact_add").submit(function (e) {
    e.preventDefault();
    
    document.getElementById("form_contact_add").style.cursor = "not-allowed";
    var address = $("#AddressLine1").val() + ' ' + $("#AddressLine2").val() + ' ' + $("#City").val() + ' ' + $("#CountryName").val();

    fetch("https://maps.googleapis.com/maps/api/geocode/json?address=" + address + '&key=' + $("#map-key").val())
        .then(response => response.json())
        .then(data => {
            $("#contact-Latitude").val(data.results[0].geometry.location.lat);
            $("#contact-longitude").val(data.results[0].geometry.location.lng);

            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: "multipart/form-data",
                data: new FormData(this),
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data.result) {
                        cleanBookNotification.updateSuccess();
                        document.getElementById("form_contact_add").style.cursor = "default";
                        ClearError();
                    }
                },
                error: function (data) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    
                    document.getElementById("form_contact_add").style.cursor = "default";
                }
            });
        });

});













var strValueOld = "";
var strValueChanged = "";
function OnFocusOutControl(value) {
    strValueChanged = value;
    if (strValueChanged !== strValueOld)
        SaveContact();
};

function OnFocusControl(value) {
    strValueOld = value;
};

//
function getNewContactRef() {
    $.ajax({
        type: "get",
        url: "/TraderContact/GetNewContactRef",
        dataType: "json",
        success: function (response) {
            if (response) {
                $('#contactReferenceId').val(response.Id);
                $('#numberRef').text(response.Reference);
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_group_add');
    });
}
function selectWorkGroupContact() {
    var wgselectd = $('#group-workgroup-id option:selected');
    $('.domain_name').text(wgselectd.attr("domain") + "-");
}
