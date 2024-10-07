var reader = new FileReader();

// Page function
function saveAndFinish() {
    updateCoverImage(true);
    updateAvatar(true);
    updateProfileText(true);
    saveProfileFiles(true);
    // skills
    var skills = [];
    var elementSkillUpdate = $("#skill-list div.community-competencies");
    if (elementSkillUpdate.length > 0) {
        for (var i = 0; i < elementSkillUpdate.length; i++) {
            var tagValue = $(elementSkillUpdate[i]).find("input.skill_tags").val().split(",");
            var tagSkill = [];
            if (tagValue.length > 0) {
                for (var j = 0; j < tagValue.length; j++) {
                    tagSkill.push({ Id: tagValue[j] });
                }
            }
            skills.push(
                {
                    Id: $(elementSkillUpdate[i]).find("input.skill_id").val(),
                    Name: $(elementSkillUpdate[i]).find("p.skill_name").text(),
                    Tags: tagSkill,
                    Level: $(elementSkillUpdate[i]).find("span.skill_level").text(),
                    Description: $(elementSkillUpdate[i]).find("p.skill_description").text()
                }
            );
        }
    }
    // end skill
    // userprofile tags
    var tagVal = $("#userProfile_tags").val();
    var tags = [];
    if (tagVal.length > 0) {
        for (var i = 0; i < tagVal.length; i++) {
            tags.push({ Id: tagVal[i] });
        }
    }
    // end userprofile tags
    // employer
    var employs = $("#listEmployer div.portlet");
    var lstEmploy = [];
    if (employs.length > 0) {
        for (var k = 0; k < employs.length; k++) {
            if ($(employs[k]).find("input.employer_dates").val() != null) {
                var dates = $(employs[k]).find("input.employer_dates").val().split("-");
                var emp = {
                    Id: $(employs[k]).find("input.employer_id").val(),
                    Employer: $(employs[k]).find("div.employer_name").text().trim(),
                    Role: $(employs[k]).find("p.employer_role").text()
                };
                //var start = dates[0].trim().split("/");
                //var end = dates[1].trim().split("/");
                emp.StartDate = dates[0].trim();// new Date(start[2], start[1], start[0]);
                emp.EndDate = dates[1].trim();//new Date(end[2], end[1], end[0]);
                lstEmploy.push(emp);
            }
        }
    }
    // end employer
    var profile = {
        StrapLine: $("#profile_strapline").val(),
        Skills: skills,
        Tags: tags
    };

    $.ajax({
        type: "post",
        url: "/UserProfile/SaveAndFinish",
        data: {
            userProfile: profile,
            employments: lstEmploy
        },
        dataType: "json",
        success: function(response) {
            if (response.actionVal == 1) {
                cleanBookNotification.updateSuccess();
                location.href = "/Community/UserProfilePage?id=" + response.msgId;
            } else if (response.actionVal == 3 || response.actionVal == 2) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function(er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });
}

function updateCoverImage(value) {
    var lstFiles = new FormData();
    var coverFile = $("#image_featured");
    lstFiles.append("Cover-profile", coverFile[0].files[0]);
    if (coverFile.length > 0 && coverFile[0].files.length > 0)
        $.ajax({
            type: "post",
            url: "/UserProfile/CommunityChangeFeatureUserImage",
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: "json",
            success: function(response) {
                if (!value)
                    if (response.actionVal == 1) {
                        cleanBookNotification.updateSuccess();
                    } else if (response.actionVal == 3 || response.actionVal == 2) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
            },
            error: function(er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
}

// End Page function
// Cover avatar profile
function changeImageCover(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#image_featured");
        reader.onload = function(e) {
            $("div.community-profile-upper").attr("style", "background-image: url('" + e.target.result + "');");
        };
        reader.readAsDataURL(image[0].files[0]);
    }

}

function changeImageAvatar(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#upload_image_avatar");
        reader.onload = function(e) {
            $("div.community-side div.community-avatar")
                .attr("style", "background-image: url('" + e.target.result + "');");
        };
        reader.readAsDataURL(image[0].files[0]);
    }
}

function previewUserProfile() {
    // preview tags
    var tags = $("#userProfile_tags option:selected");
    $("#profile_preview_tags").empty();
    if (tags.length > 0) {
        for (var i = 0; i < tags.length; i++) {
            var tag = "<a class=\"topic-label\"> <span class=\"label label-info\">" +
                $(tags[i]).text() +
                "</span> </a>";
            $("#profile_preview_tags").append(tag);
        }
    }
    // preview strapline
    $("#preview_trapline").text($("#profile_strapline").val());

    $(".previewUserProfile").removeClass("hidden");
    $(".editProfile").addClass("hidden");
}

function returnEdit() {
    $(".editProfile").removeClass("hidden");
    $(".previewUserProfile").addClass("hidden");
}

function changeProfileDescription() {
    $(".profile_description").text($("#txtprofile_description").val());
}

function updateAvatar(value) {
    var lstFiles = new FormData();
    var avatarFile = $("#upload_image_avatar");
    lstFiles.append("avatar-profile", avatarFile[0].files[0]);
    if (avatarFile.length > 0 && avatarFile[0].files.length > 0)
        $.ajax({
            type: "post",
            url: "/UserProfile/CommunityChangeUserAvatar",
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: "json",
            success: function(response) {
                if (!value)
                    if (response.actionVal == 1) {
                        cleanBookNotification.updateSuccess();
                    } else if (response.actionVal == 3 || response.actionVal == 2) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
            },
            error: function(er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
}

function updateProfileText(value) {
    if ($("#txtprofile_description").val().length > 0)
        $.ajax({
            type: "post",
            url: "/UserProfile/UpdateProfileText?profileText=" + $("#txtprofile_description").val(),
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: "json",
            success: function(response) {
                if (!value)
                    if (response.actionVal == 1) {
                        cleanBookNotification.updateSuccess();
                    } else if (response.actionVal == 2) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
            },
            error: function(er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
    else {

    }
}
// End Cover avatar profile

// Skill list
var divSkillClass = null;

function addSkill() {
    var tags = [];
    var skillId = UniqueId();
    var skillCount = $("#skill-list div.community-competencies").length + 1;
    if ($("#input_skill_tags").val() && $("#input_skill_tags").val().length > 0)
        for (var i = 0; i < $("#input_skill_tags").val().length; i++) {
            tags.push({ Id: $("#input_skill_tags").val()[i] });
        }
    var skill = {
        Name: $("#input_skill_name").val(),
        Level: $("#input_skill_level").val(),
        Tags: tags,
        Description: $("#input_skill_description").val()
    };

    var skillStr = "<div class=\"panel-group community-competencies skill-class-" +
        skillId +
        " newskill\" role=\"tablist\" aria-multiselectable=\"true\" style=\"margin-bottom: 15px;\">";
    skillStr +=
        "<input type=\"hidden\" class=\"skill_id\" value=\"\"/> <input type=\"hidden\" class=\"skill_tags\" value=\"\" />";
    skillStr += "<div class=\"panel panel-default\"> <div class=\"panel-heading\" role=\"tab\" id=\"heading-" +
        skillCount +
        "\">";
    skillStr += "<h4 class=\"panel-title\">";
    skillStr +=
        "<a role=\"button\"class=\"button_skill\" data-toggle=\"collapse\" data-parent=\"#accordion-competencies\" href=\"#collapse-" +
        skillCount +
        "\" aria-expanded=\"false\" aria-controls=\"collapse-" +
        skillCount +
        "\">";
    skillStr += "<i class=\"more-less fa fa-plus\"></i> <p class=\"skill_name\">" +
        skill.Name +
        "</p><br /> <span class=\"skill_level\">" +
        skill.Level +
        "</span> ";
    skillStr += "</a> </h4> </div> <div id=\"collapse-" +
        skillCount +
        "\" class=\"panel-collapse collapse\" role=\"tabpanel\" aria-labelledby=\"heading-" +
        skillCount +
        "\">";
    skillStr += "<div class=\"panel-body\" style=\"padding: 20px;\"> <p class=\"skill_description\"> " +
        skill.Description +
        " </p>  <br />";
    skillStr += "<a href=\"#\" class=\"btn btn-warning editProfile\" onclick=\"editSkill('skill-class-" +
        skillId +
        "')\" data-toggle=\"modal\" data-target=\"#app-community-edit-skill\"><i class=\"fa fa-pencil\"></i></a>  ";
    skillStr += "<a href=\"#\" class=\"btn btn-danger editProfile\" onclick=\"deleteSkill('skill-class-" +
        skillId +
        "')\"><i class=\"fa fa-trash\"></i></a>";
    skillStr += "</div> </div> </div>  </div>";

    $("#skill-list").append(skillStr);
    $("#skill-list div.skill-class-" + skillId + " input.skill_tags").val($("#input_skill_tags").val().join(","));
    // reset form add skill
    $("#input_skill_tags").val([]);
    $("#input_skill_name").val("");
    $("#input_skill_level").val("");
    $("#input_skill_description").val("");

    $("select").not(".multi-select").select2();
}

function editSkill(classSkill) {
    divSkillClass = classSkill;
    var tags = $("." + classSkill + " input.skill_tags").val().split(",");
    var name = $("." + classSkill + " p.skill_name").text();
    var level = $("." + classSkill + " span.skill_level").text();
    var description = $("." + classSkill + " p.skill_description").text();
    $("#modal_skill_name").val(name);
    $("#modal_skill_level").val(level);
    $("#modal_skill_tags").val(tags);
    $("select").not(".multi-select").select2();
    $("#modal_skill_description").val(description.trim());

}

function deleteSkill(classSkill) {
    $("." + classSkill).remove();
}

function updateSkill() {
    $("." + divSkillClass + " input.skill_tags").val($("#modal_skill_tags").val().join(","));
    $("." + divSkillClass + " p.skill_name").text($("#modal_skill_name").val());
    $("." + divSkillClass + " span.skill_level").text($("#modal_skill_level").val());
    $("." + divSkillClass + " p.skill_description").text($("#modal_skill_description").val());
    if ($("." + divSkillClass + ".newskill").length == 0)
        $("." + divSkillClass).addClass("skill_update");
    divSkillClass = null;

    $("#modal_skill_name").val("");
    $("#modal_skill_level").val("");
    $("#modal_skill_tags").val([]);
    $("select").not(".multi-select").select2();
    $("#modal_skill_description").val("");

    $("#app-community-edit-skill").modal("toggle");
}

function cancelEditSkill() {
    $("#modal_skill_name").val("");
    $("#modal_skill_level").val("");
    $("#modal_skill_tags").val([]);
    $("select").not(".multi-select").select2();
    $("#modal_skill_description").val("");
}


// Employment 
var employGuidID = null;

function addEmployment() {
    var empGuidId = UniqueId();
    var employer = {
        Name: $("#form_employ_name").val(),
        Role: $("#form_employ_role").val(),
        Dates: $("#form_employ_dates").val()
    };
    // valid employer
    if (employer.Name == "" || employer.Role == "" || employer.Dates == "") return false;

    var employStr = "<div class=\"portlet portlet_" +
        empGuidId +
        "\"> <input type=\"hidden\" class=\"employer_id\" value=\"0\"/>";
    employStr += "<input type=\"hidden\" class=\"employer_dates\" value=\"" + employer.Dates + "\"/>";
    employStr += "<div class=\"portlet-header employer_name\">" +
        employer.Name +
        " </div> <div class=\"portlet-content\">";
    employStr +=
        "<div class=\"row\"> <div class=\"col-xs-6 associated-mini\"> <p class=\"employer_role\" style=\"position: relative; top: 7px;\">" +
        employer.Role +
        "</p>";
    employStr += "</div> <div class=\"col-xs-6 text-right\"> <div class=\"btn-group options\">";
    employStr +=
        "<button type=\"button\" class=\"btn btn-success dropdown-toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
    employStr += "<i class=\"fa fa-cog\"></i> </button> <ul class=\"dropdown-menu dropdown-menu-right\">";
    employStr += "<li><a href=\"#\" data-toggle=\"modal\" onclick=\"editEmployer('portlet_" +
        empGuidId +
        "')\" data-target=\"#app-community-edit-work-history\">Edit</a></li>";
    employStr += "<li><a href=\"#\" onclick=\"deleteEmployer('portlet_" +
        empGuidId +
        "')\">Delete</a></li>  </ul> </div> </div> </div> </div> </div>";

    $("#listEmployer").append(employStr);
    $("#form_employ_name").val("");
    $("#form_employ_role").val("");
    $("#form_employ_dates").val("");
}

function editEmployer(classEmploy) {
    employGuidID = classEmploy;
    var name = $("." + classEmploy + " div.employer_name").text().trim();
    var role = $("." + classEmploy + " p.employer_role").text();
    var dates = $("." + classEmploy + " input.employer_dates").val();
    $("#employer_name").val(name);
    $("#employer_role").val(role);
    $("#employer_dates").val(dates);
    //var dateSub = dates.trim().split('-');

    //var start = dateSub[0].trim().split('/');
    //var end = dateSub[1].trim().split('/');
    //var startDate = start[1] + "/" + start[0] + "/" + start[2];
    //var endDate = end[1] + "/" + end[0] + "/" + end[2];

    //$('#employer_dates').data('daterangepicker').setStartDate(startDate);
    //$('#employer_dates').data('daterangepicker').setEndDate(endDate);

    //alert(endDate);
}

function deleteEmployer(classEmploy) {
    $("." + classEmploy).remove();
}

function employerConfirmChange() {
    $("." + employGuidID + " div.employer_name").text($("#employer_name").val());
    $("." + employGuidID + " p.employer_role").text($("#employer_role").val());
    $("." + employGuidID + " input.employer_dates").val($("#employer_dates").val());

    employGuidID = null;
    $("#employer_name").val("");
    $("#employer_role").val("");
    $("#employer_dates").val("");

    $("#app-community-edit-work-history").modal("toggle");
}

function cancelChangeEmployer() {
    $("#employer_role").val("");
    $("#employer_name").val("");
    $("#employer_dates").val("");
}

// end Employer
// My files
function LoadFile(file) {
    var guidID = UniqueId();
    var li = "<li class=\"newFile " +
        guidID +
        " \"> <span class=\"btn btn-danger delete_item\" onclick=\"myFileDelete(this.parentElement, '" +
        file.Name.replace(/\./g, "_") +
        "')\"><i class=\"fa fa-trash\"></i></span>";
    li += "<a> <img src=\".." +
        file.Type.IconPath +
        "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\"> " +
        file.Name +
        " </a>";
    li += "<input type=\"file\" name=\"file\" class=\"form-control myfile hidden\"><input type=\"text\" value=\"" +
        file.Name +
        "\" class=\"form-control hidden\"></li>";
    $("#ulMyfile").append(li);


    var article = "<article class=\"activity media " +
        file.Name.replace(/\./g, "_") +
        "\" style=\"margin-bottom: 0;\">";
    article +=
        "<div class=\"activity-detail\" style=\"padding-left: 0; max-width: 100%;\"> <div class=\"activity-overview media\">";
    article += "<div class=\"row\"> <div class=\"col-xs-12 col-sm-5 col-md-6 col-lg-5\"> <a href=\"#\">";
    article += "<img src=\".." +
        file.Type.IconPath +
        "\" class=\"img-responsive\"> </a> </div> <div class=\"col-xs-12 col-sm-7 col-md-6 col-lg-7 description\">";
    article += "<h5>" +
        file.Name +
        "</h5> <p></p> <small> </small> </div> </div> </div> </div> <div class=\"clearfix\"></div> </article>";
    $("#listMyFile").append(article);
    var file = $("#myfile_input_file").clone(true);
    $("#ulMyfile li." + guidID + " input.myfile")[0].files = file[0].files;

    $("#input_file_name").val("");
    $("#myfile_input_file").val("");
}

function addFile() {
    var lstFiles = new FormData();
    var fileName = $("#input_file_name").val();
    var file = $("#myfile_input_file");
    if (file.length > 0 && file[0].files.length > 0) {
        if (fileName.length == 0) {
            fileName = file[0].files[0].name;
            $("#input_file_name").val(fileName);
        }
        lstFiles.append(fileName, file[0].files[0]);
        LoadingOverlay();
        $.ajax({
            type: "post",
            url: "/UserProfile/CheckProfileFiles",
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: "json",
            success: function(response) {
                setTimeout(function() {
                        $.LoadingOverlay("hide");
                    },
                    2000);
                if (response.actionVal == 1) {
                    if (response.Object != null && response.Object.length > 0) {
                        LoadFile(response.Object[0]);
                    }

                } else if (response.actionVal == 3 || response.actionVal === 2) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function(er) {
                setTimeout(function() {
                        $.LoadingOverlay("hide");
                    },
                    2000);
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
    } else {
        cleanBookNotification.warning("Please selected a file.", "Qbicles");
    }
}

function myFileDelete(ev, id) {
    if ($("." + id + " input.file_profile").length > 0) {
        $("." + id + " input.file_profile").addClass("fileDelete");
        $("." + id).addClass("hidden");
    } else $("." + id).remove();
}

function saveProfileFiles(value) {
    // delete old file
    var lisOld = $("#ulMyfile input.fileDelete");
    if (lisOld.length > 0) {
        var lstId = [];
        for (var i = 0; i < lisOld.length; i++) {
            lstId.push(parseInt($(lisOld[i]).val()));
        }
        $.ajax({
            type: "post",
            url: "/UserProfile/DeleteFiles",
            data: { lstId: lstId },
            dataType: "json",
            success: function(response) {
                if (response.actionVal === 1) {
                    for (var i = 0; i < lisOld.length; i++) {
                        $(lisOld[0].parentElement).remove();
                    }
                }
                if (!value)
                    if (response.actionVal === 1) {
                        cleanBookNotification.updateSuccess();
                    } else if (response.actionVal === 3 || response.actionVal === 2) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
            },
            error: function(er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
    }

    // add new file
    var lis = $("#ulMyfile li.newFile");
    var lstFiles = new FormData();
    if (lis.length > 0) {
        for (var i = 0; i < lis.length; i++) {
            if ($(lis[i]).find("input.myfile")[0].files.length > 0)
                lstFiles.append($($(lis[i]).find("input")[1]).val(), $(lis[i]).find("input.myfile")[0].files[0]);
        }
        $.ajax({
            type: "post",
            url: "/UserProfile/CommunityUserAddNewFiles",
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: "json",
            success: function(response) {
                if (response.actionVal == 1) {
                    cleanBookNotification.updateSuccess();
                } else if (response.actionVal == 3 || response.actionVal === 2) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function(er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            }
        });
    }


}


function DownLoadUserProfileFile(fileUri, fileName, fileExtension) {
    var Name = fileName + "." + fileExtension;
    var fileModel = {
        Uri: fileUri,
        Name: Name
    };

    $.ajax({
        type: "post",
        url: "/Medias/DownloadFile",
        datatype: "json",
        data: fileModel,
        success: function(refModel) {
            var link = document.createElement("a");
            link.download = fileName;
            link.href = refModel;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        },
        error: function(err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}

function ShowAddUserToSlGroupCandidateView() {
    LoadingOverlay();
    var _url = "/Network/ShowAddCandidateView?candidateId=" + $("#candidate-id").val();
    $("#shortlist-candidate").empty();
    $("#shortlist-candidate").load(_url);
    $("#shortlist-candidate").modal('show');

    LoadingOverlayEnd();
}

function showChangeSlGroupView(candidateId) {
    LoadingOverlay();
    var slGroupId = $("#active-group-id").val();
    var _url = "/Network/ShowAddCandidateView?candidateId=" + candidateId + "&slGroupId=" + slGroupId;
    $("#shortlist-candidate").empty();
    $("#shortlist-candidate").load(_url);
    $("#shortlist-candidate").modal('show');

    LoadingOverlayEnd();
}

function loadUserShortlistGroupByDomain(el) {
    var $selector = $('#add-candidate-form select[name=shortlistGroup]');
    if ($(el).val != "0") {
        $("#slgroups-container").fadeIn();
    } else {
        $("#slgroups-container").fadeOut();
    }
    $.get("/Network/GetUserSlGroupByDomain?domainId=" + $(el).val(), function (data) {
        $selector.select2('destroy');
        $selector.empty();
        $.each(data, function (key, val) {
            $selector.append('<option value="' + val.id + '">' + val.text + '</option>');
        })
        var selectedSlGroupId = $("#active-group-id").val();
        $("option[value=" + selectedSlGroupId + "]").attr("selected", "true");
        $selector.select2();
    });
}

function initAddCandidateForm() {
    var $addcandidatefrm = $("#add-candidate-form");
    $addcandidatefrm.validate({
        rules: {
            domain: {
                required: true
            },
            shortlistGroup: {
                required: true
            }
        }
    });

    $addcandidatefrm.submit(function (e) {
        e.preventDefault();
        if ($addcandidatefrm.valid()) {
            var _url = "/Network/AddSlGroupCandidate";
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    userId: $("#candidate-id").val(),
                    slGroupId: $("#slgroups").val(),
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.success("Add User to Shortlist Group successfully.", "Qbicles");
                        $("#shortlist-candidate").modal('hide');
                        $("#shortlist").fadeOut();
                        $("#shortlisted").fadeIn();

                        var $candidateListTable = $("#candidate-list");
                        if ($candidateListTable != null) {
                            $candidateListTable.DataTable().ajax.reload();
                        }

                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            })
        }
    })
}

//Connect C2C
function connectC2CProfilePage(uId, fullName) {
    $.LoadingOverlay('show');
    $.post("/C2C/ConnectC2C", { linkId: uId, type: 2 }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            cleanBookNotification.success("Connection request sent. " + fullName + " will need to approve your request, and will appear as a pending contact in your Community hub until then.")
            $('#pending').show();
            $("#connect").hide();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}


function connectC2CPublicProfilePage(uId, fullName, isRedirect) {
    $.LoadingOverlay('show');
    $.post("/C2C/ConnectC2C", { linkId: uId, type: 2 }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            if (isRedirect) {
                window.location = "/C2C";
            } else {
                cleanBookNotification.success("Connection request sent. You can track the status in your Community hub.");
                var $candidateListTable = $("#candidate-list");
                if ($candidateListTable != null) {
                    $candidateListTable.DataTable().ajax.reload();
                }
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}

//Shared Qbicles
function initSharedQbicleShow() {
    var $data_container = $('#qbicles-container');
    var $pagination_container = $('#pagiation-qbicles');
    $pagination_container.pagination({
        dataSource: '/UserInformation/GetSharedQbicles',
        locator: 'items',
        totalNumberLocator: function (response) {
            //$data_container.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 20,
        ajax: {
            data: {
                userId: $("#candidate-id").val()
            },
            beforeSend: function () {
                $data_container.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += sharedQbicleTemplate(item);
            });
            $data_container.html(dataHtml);
            $data_container.LoadingOverlay('hide');
        }
    })
}


function sharedQbicleTemplate(data) {
    var logoUri = data.LogoUri + "&size=T";
    var qName = data.QbicleName;
    var domainName = data.DomainName;
    var domainOwner = data.DomainOwner;

    var _html = '<li>';
    _html += '<a href="javascript:void(0)" onclick="QbicleSelected(\'' + data.QbicleKey + '\', \'Dashboard\')">';
    _html += '<div class="owner-avatar">';
    _html += '<div class="avatar-sm" style="background: url(\'' + logoUri + '\');"></div>';
    _html += '</div>';
    _html += '<h5>' + qName + '<br><small>' + domainName + ' ' + domainOwner + '</small></h5>';
    _html += '</a></li>';
    return _html;
}