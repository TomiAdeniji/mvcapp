var reader = new FileReader();

var $modal = $("#app-community-edit-location");

$(function () {
    if ($("#domain-profile-id").val() === "0") {
        $(".button-update").attr("button-update", true);
    } else {
        $(".button-update").removeAttr("disabled");
    }
});
function changeImageCover(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#image_featured");
        reader.onload = function (e) {
            $("div.community-profile-upper").attr("style", "background-image: url('" + e.target.result + "');");
        }
        reader.readAsDataURL(image[0].files[0]);
    }

}

function changeImageLogo(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#upload_image_logo");
        reader.onload = function (e) {
            $("div.community-side div.community-avatar").attr("style", "background-image: url('" + e.target.result + "');");
        }
        reader.readAsDataURL(image[0].files[0]);
    }

}

function previewDomainProfile() {
    $(".previewDomainProfile").removeClass("hidden");
    $(".editProfile").addClass("hidden");

    $("#strapline-preview-1").text($('#profile_strapline').val());
    $("#strapline-preview-2").text($('#profile_strapline').val());

    $("#profile-preview").html($('#domain-profile').val().replace(/\n/g, "<br />"));

    var tags = $("#domain-tag option:selected");
    $('#profile_preview_tags').empty();
    if (tags.length > 0) {
        for (var i = 0; i < tags.length; i++) {
            var tag = "<a class=\"topic-label\"> <span class=\"label label-info\">" + $(tags[i]).text() + "</span> </a>";
            $('#profile_preview_tags').append(tag);
        }
        $('#profile_preview_tags').append("<br><br>");
    }
}

function returnEdit() {
    $(".editProfile").removeClass("hidden");
    $(".previewDomainProfile").addClass("hidden");
}

function updateCoverImage() {
    var lstFiles = new FormData();
    var coverFile = $("#image_featured");
    lstFiles.append("Cover-profile", coverFile[0].files[0]);
    if (coverFile.length > 0 && coverFile[0].files.length > 0)
        $.ajax({
            type: 'post',
            url: '/DomainProfile/ComunityChangeFeatureDomainImage',
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                CommunityUpdateLogo();
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    else
        CommunityUpdateLogo();
}

function CommunityUpdateLogo() {
    var lstFiles = new FormData();
    var logoFile = $("#upload_image_logo");
    lstFiles.append("avatar-profile", logoFile[0].files[0]);
    if (logoFile.length > 0 && logoFile[0].files.length > 0)
        $.ajax({
            type: 'post',
            url: '/DomainProfile/ChangeLogo',
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                cleanBookNotification.updateSuccess();
                setTimeout(function () {
                    location.href = "/Community/DomainProfile";
                }, 500);
            },
            error: function (er) {
                //cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    cleanBookNotification.updateSuccess();
    setTimeout(function () {
        location.href = "/Community/DomainProfile";
    }, 500);
}

function updateProfileText(value) {
    if ($("#domain-profile").val().length > 0) {
        var domainProfile = JSON.stringify($("#domain-profile").val().trim());
        $.ajax({
            type: 'post',
            url: '/DomainProfile/UpdateProfileText?profileText=' + domainProfile,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                if (!value)
                    if (response.actionVal == 1) {
                        cleanBookNotification.updateSuccess();
                    } else if (response.actionVal == 2) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
    else {
        $("#form-domain-profile").validate().showErrors({ domainprofile: "This field is required." });
    }
}


// Location 
var $locationGuidID = null;
function addLocation() {

    var location = {
        Name: $('#form_location_name').val(),
        Address: $('#form_location_address').val()
    }
    var valid = true;
    // valid location
    if (location.Name === "") {
        valid = false;
        $("#form-domain-profile").validate().showErrors({ locationName: "This field is required." });
    }
    if (location.Address === "") {
        valid = false;
        $("#form-domain-profile").validate().showErrors({ locationAddress: "This field is required." });
    }

    if (!valid)
        return;

    $.ajax({
        type: 'post',
        url: '/DomainProfile/BuildLocation',
        data: { location: location },
        dataType: 'json',
        success: function (refModel) {
            if (refModel.result) {
                $("#listLocations").append(refModel.msg);
                $('#form_location_name').val("");
                $('#form_location_address').val("");
            }
            else {
                cleanBookNotification.error(refModel.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}

function editLocation(locationId) {
    $locationGuidID = locationId;

    $('#location_name').val($("#name-" + locationId).html().replace(/<br ?\/?>/g, "\n"));
    $('#location_address').val($("#address-" + locationId).html().replace(/<br ?\/?>/g, "\n"));
    $modal.modal('toggle');
}

function deleteLocation(locationId) {
    $('#location-' + locationId).remove();
}

function locationConfirmChange() {
    $('#name-' + $locationGuidID).html($('#location_name').val().replace(/\n/g, "<br />"));
    $('#address-' + $locationGuidID).html($('#location_address').val().replace(/\n/g, "<br />"));

    $locationGuidID = null;
    $('#location_name').val("");
    $('#location_address').val("");
    $modal.modal('toggle');
}

function cancelChangeLocation() {
    $('#form_location_name').val("");
    $('#form_location_address').val("");
    $modal.modal('toggle');
}


function saveAndFinish() {
    //valid form
    var valid = true;
    if ($("#profile_strapline").val() === "") {
        valid = false;
        $("#form-domain-profile").validate().showErrors({ strapline: "This field is required." });
    }
    if ($("#domain-profile").val() === "") {
        valid = false;
        $("#form-domain-profile").validate().showErrors({ domainprofile: "This field is required." });
    }


    if (!valid)
        return;
    //tags
    var tagVal = $('#domain-tag').val();
    var tags = [];
    if (tagVal != null && tagVal.length > 0) {
        for (var i = 0; i < tagVal.length; i++) {
            tags.push({ Id: tagVal[i] });
        }
    }
    //location
    var locationRoot = $("#listLocations div.office-location");
    var locations = [];
    if (locationRoot.length > 0) {
        for (var k = 0; k < locationRoot.length; k++) {

            var local = {
                Id: $(locationRoot[k]).find('input.location_id').val(),
                Name: $(locationRoot[k]).find('strong.location_name').text().trim(),
                //Address: $(locationRoot[k]).find('p.location_address').html().replace(/<br ?\/?>/g, "\n"),
                Address: JSON.stringify($(locationRoot[k]).find('p.location_address').html().replace(/<br ?\/?>/g, "\n"))
            };
            locations.push(local);
        }
    }
    //community pages
    var pageRoot = $(".page-community div.portlet");
    var pages = [];
    if (pageRoot.length > 0) {
        for (var k = 0; k < pageRoot.length; k++) {
            var pId = $(pageRoot[k]).find('input.page_id').val();
            var pCheck = $("#page-display-" + pId).prop('checked');
            
            var p = {
                Id: pId,
                IsDisplayedOnDomainProfile: pCheck,
                DisplayOrderOnDomainProfile: k + 1
            };
            pages.push(p);
        }
    }
    // end 
    var profile = {
        Id: $("#domain-profile-id").val(),
        StrapLine: $('#profile_strapline').val(),
        ProfileText: $("#domain-profile").val(),
        Tags: tags
    };
    $.ajax({
        type: 'post',
        url: '/DomainProfile/SaveAndFinish',
        data: { domainProfile: profile, locations: locations, pages: pages },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal == 1) {
                SaveAssociate();
            }
            else if (response.actionVal == 3 || response.actionVal == 2) {

                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {

            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}


function DeletePage(id) {
    $.ajax({
        type: 'post',
        url: '/CommunitySystem/DeletePage',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $("#page-community-" + id).remove();
                $("#preview-page-" + id).remove();
                cleanBookNotification.updateSuccess();
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function SaveAssociate() {
    if ($("#domain-profile").val().length > 0) {
        var domainProfile = JSON.stringify($("#domain-profile").val().trim());
        $.ajax({
            type: 'post',
            url: '/DomainProfile/UpdateProfileText?profileText=' + domainProfile,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                updateCoverImage();
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
    else {
        $("#form-domain-profile").validate().showErrors({ domainprofile: "This field is required." });
    }
}