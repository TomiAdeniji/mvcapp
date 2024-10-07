ClearForm = function (id) {
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    $wgId = 0;
    ReInitUsersEdit(0);
    ResetFormControl(id);
    $('a[href="#add-specifics"]').tab('show');
    var parent = $(".btnNextAdd").closest('.modal');
    if ($(parent).find('.app_subnav .active')[0].id === "step_1_add")
        $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');

    $('#wg-process-add').val(null);
    $(".chosen-multiple").select2({ multiple: true });
    var table = $("#wg-table-members-add").DataTable();
    table.clear();
    table.destroy();
    $("#wg-group-add").val('').trigger("change");
};

//search on table user add new
var addTable = $("#wg-table-members-add");
$("#search-right-add").on("change",
    function () {
        var group = $(this).val();
        addTable.DataTable().search(group).draw();
    });

$("#search-member-add").keyup(function () {
    addTable.DataTable().search($(this).val()).draw();
});
function processChange(ev) {
    if ($(ev).val() != null && $('#wg-process-add + label.error')) {
        $('#wg-process-add + label.error').remove();
    }
}
ReInitUsersEdit = function (id) {
    var table = $("#user-list").DataTable();

    table.clear();
    table.destroy();
    $.ajax({
        type: "post",
        url: "/Apps/ReInitUsersEdit",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {
            if (refModel) {
                $("table#user-list > tbody").prepend(refModel);
                $(".check-right").bootstrapToggle();
                $("#user-list").DataTable({
                    responsive: true,
                    "lengthChange": true,
                    "pageLength": 10,
                    "columnDefs": [
                        {
                            "targets": 3,
                            "orderable": false
                        }
                    ],
                    "order": []
                });

                $("#user-list").show();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

};
AddMemberToWorkgroup = function () {

    if ($_members.length > 0) {
        var actionTable = "add";
        if ($wgId > 0)
            actionTable = "edit";
        var table = $("#wg-table-members-" + actionTable).DataTable();

        table.clear();
        table.destroy();

        $.each($_members,
            function (index, user) {
                $("table#" + "wg-table-members-" + actionTable + " > tbody").prepend(CreateRowTable(user));
            });

        $(".check-right").bootstrapToggle();
        $("#wg-table-members-" + actionTable).DataTable({
            responsive: true,
            "lengthChange": true,
            "pageLength": 10,
            "columnDefs": [
                {
                    "targets": 3,
                    "orderable": false
                }
            ],
            "order": []
        });

        $("#wg-table-members-" + actionTable).show();
    };
};
function CreateRowTable(obj) {

    var actionTable = "add";
    if ($wgId > 0)
        actionTable = "edit";

    var isApprover = "";
    var isReviewer = "";
    var app = $_approvers.filter(ap => {
        return ap.Id === obj.Id;
    });
    if (app.length > 0)
        isApprover = "checked";

    var rev = $_reviewers.filter(ap => {
        return ap.Id === obj.Id;
    });
    if (rev.length > 0)
        isReviewer = "checked";


    var tr = "<tr id=\"tr_" + actionTable + "_user_" + obj.Id + "\">";
    tr += "<td>" +
        "<div class=\"table-avatar mini\" style=\"background-image: url('" + obj.Pic + "');\">&nbsp;</div>" +
        "</td>";
    tr += "<td>" + obj.Name
        + "<span id=\"span-approval-add-" + obj.Id + "\" class='hidden'></span>"
        + "<span id=\"span-review-add-" + obj.Id + "\" class='hidden'></span>"
        + "</td>";

    tr += "<td>" +
        "<input " + isApprover + " onchange=\"AddUsersApprovers(this.checked,'" + obj.Id + "','span-approval-add-')\" class=\"check-right\" data-toggle=\"toggle\" data-onstyle=\"success\" data-on=\"<i class='fa fa-check'></i>\" data-off=\" \" type=\"checkbox\"></td>";

    tr += "<td>" +
        "<input " + isReviewer + " onchange=\"AddUsersToReviewers(this.checked,'" + obj.Id + "','span-review-add-')\" class=\"check-right\" data-toggle=\"toggle\" data-onstyle=\"success\" data-on=\"<i class='fa fa-check'></i>\" data-off=\" \" type=\"checkbox\"></td>";

    tr += "<td><button class='btn btn-danger' onclick=\"RemoveRowTableMemberAdd('" + obj.Id + "')\"><i class='fa fa-trash'></i></button></td>";
    tr += "</tr>";
    return tr;
};
AddUsersToMembers = function (isCheck, userId, userName, userPic) {

    var user = {
        Id: userId,
        Name: userName,
        Pic: userPic
    };

    if (isCheck === true) {
        $_members.push(user);
    } else {
        for (var i = $_members.length; i--;) {
            if ($_members[i].Id === user.Id) $_members.splice(i, 1);
        }
    }
};
AddUsersToReviewers = function (isCheck, userId, spanKey) {
    var user = {
        Id: userId
    };

    if (isCheck === true) {
        $_reviewers.push(user);
        $("#" + spanKey + userId).text("Reviewers");
    } else {
        for (var i = $_reviewers.length; i--;) {
            if ($_reviewers[i].Id === user.Id) {
                $_reviewers.splice(i, 1);
                $("#" + spanKey + userId).text("");
            }
        }
    }
};
AddUsersApprovers = function (isCheck, userId, spanKey) {
    var user = {
        Id: userId
    };

    if (isCheck === true) {
        $_approvers.push(user);
        $("#" + spanKey + userId).text("Approvers");
    } else {
        for (var i = $_approvers.length; i--;) {
            if ($_approvers[i].Id === user.Id) {
                $_approvers.splice(i, 1);
                $("#" + spanKey + userId).text("");
            }
        }
    }
};

var topicValue = 0;
function qbicleChange(ev) {
    var id = $(ev).val();
    topicValue = $('#"wg-topic-add"').val();
    $.ajax({
        type: "post",
        url: "/Topics/GetTopic2SelectByQbicle",
        datatype: "json",
        data: {
            qbicleId: id
        },
        success: function (refModel) {

            if (refModel.result) {
                $("#wg-topic-add").empty().append(refModel.Object);
                if (topicValue > 0) {
                    $("#wg-topic-add").val(topicValue);
                }
                $('#wg-topic-add').select2();
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
}

$('.btnPreviousAdd').click(function () {
    var parent = $(this).closest('.modal');
    $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
});
$('.btnNextAdd').click(function () {
    var wGroup = {
        Id: $('#cbwgid').val(),
        Name: $("#wg-name-add").val()
    };
    if ($("#form-wg-add").valid()) {
        $.ajax({
            url: "/Apps/ValidateName",
            data: { wg: wGroup },
            type: "POST",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result) {
                $("#form-wg-add").validate().showErrors({ wgNameAdd: "Name of Workgroup already exists." });

            }
            else {
                var parent = $(".btnNextAdd").closest('.modal');
                $(parent).find('.app_subnav .active').next('li').find('a').trigger('click');
            }

        }).fail(function (er) {
            $("#form-wg-add").validate().showErrors({ wgNameAdd: "Error" + er });
        });
    }
});
PreviousAdd = function () {
    var parent = $(".btnNextAdd").closest('.modal');
    $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
};
PreviousEdit = function () {

    $("#step_2_edit").removeClass('active');
    $("#edit-members").removeClass('in active');

    $("#step_1_edit").addClass('active');
    $("#edit-specifics").addClass('in active');

};
NextEdit = function () {

    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-edit").val()
    };

    if ($("#form-wg-edit").valid()) {
        $.ajax({
            url: "/Apps/ValidateName",
            data: { wg: wGroup },
            type: "POST",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result) {
                $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Name of Workgroup already exists." });
            }
            else {
                var parent = $(".btnNextEdit").closest('.modal');
                $(parent).find('.app_subnav .active').next('li').find('a').trigger('click');

                $("#step_1_edit").removeClass('active');
                $("#edit-specifics").removeClass('in active');

                $("#step_2_edit").addClass('active');
                $("#edit-members").addClass('in active');

            }

        }).fail(function (er) {
            $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Error" + er });
        });
    }
};
Edit = function (id) {
    $('#workgroup-content-edit').empty();
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    PreviousEdit();
    $wgId = id;
    $("#search-right-all").val('').trigger("change");
    $.LoadingOverlay("show");
    $.ajax({
        type: "post",
        url: "/Apps/GetWorkGroupUser",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {
            $_members = refModel.Members;
            $_approvers = refModel.Approvers;
            $_reviewers = refModel.Reviewers;
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
    ReInitUsersEdit(id);
    setTimeout(function () {
        $('#workgroup-content-edit').load('/Apps/Edit?id=' + id);
            $.LoadingOverlay("hide");
        },
        2000);
};
SaveWorkgroup = function () {
    if (ValidateWorkgroupForm("add") === false)
        return;

    var wGroup = {
        Id: $('#cbwgid').val(),
        Name: $("#wg-name-add").val()
    };
    if ($("#form-wg-add").valid()) {

        $.ajax({
            url: "/Apps/ValidateName",
            data: { wg: wGroup },
            type: "POST",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result) {
                $("#form-wg-add").validate().showErrors({ wgNameAdd: "Name of Workgroup already exists." });
                PreviousAdd();
                return;
            }
            else {

                $("#app-trader-group-add").modal('hide');
                var wg = CreateWorkgroupData(0);

                $.ajax({
                    type: "post",
                    url: "/Apps/SaveWorkgroup",
                    datatype: "json",
                    data: {
                        wg: wg
                    },
                    success: function (refModel) {
                        if (refModel.result) {
                            if (wGroup.Id == "0") {
                                cleanBookNotification.createSuccess();
                            } else {
                                cleanBookNotification.updateSuccess();
                            }
                            
                            ReloadWorkgroup();
                            reloadMenu();
                        }
                        else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    },
                    error: function (xhr) {
                        cleanBookNotification.error(xhr.responseText, "Qbicles");
                    }
                });
            }

        }).fail(function (er) {
            $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Error" + er });
        });
    }
};
ReloadWorkgroup = function () {
    showCBSetting('workgroup');
};
function showCBSetting(settingVal, callback) {
    setCookie('cb_config_tab', settingVal);
    var ajaxUri = '/Apps/CleanBookConfig?value=' + settingVal;
    LoadingOverlay();
    $('#tab-content-config').empty();
    $('#tab-content-config').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $('a[href="#'+ settingVal + '"]').tab('show');
        //initCurrency();
        if (callback) {
            callback();
        }
    });
};
function CreateWorkgroupData(id) {
    var action = "add";

    var qbicle = {
        Id: $("#wg-qbicle-" + action).val()
    };

    var topic = {
        Id: $("#wg-topic-" + action).val()
    };

    var wgProcesses = [];

    var processes = $("#wg-process-" + action).val();
    if (processes !== null && processes.length > 0) {
        processes = processes.toString().split(",");
        $.each(processes,
            function (index, itemId) {
                var gr = {
                    Id: itemId
                };
                wgProcesses.push(gr);
            });
    }
    var lstItems = $('.wg-user-member');
    var members = [];
    if (lstItems && lstItems.length) {
        for (var i = 0; i < lstItems.length; i++) {
            if ($(lstItems[i]).val() === 'true') {
                var id = $(lstItems[i]).attr('userid');
                members.push({ Id: id });
            }
        }
    }
    var workGroup = {
        Id: $('#cbwgid').val(),
        Name: $("#wg-name-" + action).val(),
        Processes: wgProcesses,
        Qbicle: qbicle,
        Topic: topic,
        Members: members
    };
    return workGroup;
};

ValidateWorkgroupForm = function (form) {

    var isValid = true;


    if ($("#wg-name-" + form).val() === "") {
        if (form === "add")
            $("#form-wg-" + form).validate().showErrors({ wgNameAdd: "This field is required." });
        else
            $("#form-wg-" + form).validate().showErrors({ wgNameEdit: "This field is required." });
        isValid = false;
    }

    var processes = $("#wg-process-" + form).val();
    if (processes === null || processes.length === 0) {
        cleanBookNotification.error(_L("ERROR_MSG_377"), "Qbicles");
        isValid = false;
    }
    if (isValid === false) {
        if (form === "add") {
            PreviousAdd();
        } else {
            PreviousEdit();
        }
        return isValid;
    } else {
        return isValid;
    }


};

function Delete(tableId, id) {
    $.ajax({
        type: "delete",
        url: "/Apps/Delete",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {

            if (refModel.result) {
                ReloadWorkgroup();
                reloadMenu();
                cleanBookNotification.removeSuccess();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

}


function addMembers(id, ev) {
    $(ev).toggle(); $('.contact'+ id +'-remove').toggle();
    $('.wg-member-' + id).val(true);
    checkLatestItem();
}
function removeMembers(id) {
    $('.contact' + id + '-remove').hide(); $('.contact' + id +'-add').toggle();
    $('.wg-member-' + id).val(false);
    checkLatestItem();
}

function addEditWg(id) {
    var ajaxUri = '/Apps/CleanBookWorkGroupAddEdit?id=' + id;
    $.LoadingOverlay("show");
    $('#app-trader-group-add').empty();
    $('#app-trader-group-add').load(ajaxUri, function () {
        //InitTopic($("#wg-qbicle-add").val(), "wg-topic-add");
        LoadingOverlayEnd();
    });
}

function checkLatestItem() {
    var lstItems = $('.wg-user-member');
    var count = 0;
    if (lstItems && lstItems.length) {
        for (var i = 0; i < lstItems.length; i++) {
            if ($(lstItems[i]).val() === 'true') {
                count++;
            }
        }
        if (count === 1) {
            for (var i = 0; i < lstItems.length; i++) {
                if ($(lstItems[i]).val() === 'true') {
                    var id = $(lstItems[i]).attr('userid');
                    $('.contact' + id + '-remove').addClass('disabled');
                }
            }
        } else {
            for (var i = 0; i < lstItems.length; i++) {
                if ($(lstItems[i]).val() === 'true') {
                    var id = $(lstItems[i]).attr('userid');
                    $('.contact' + id + '-remove').removeClass('disabled');
                }
            }
        }
    }
}

function FilterMembers() {
    try {
        var kw = $('#smkeyword').val();
        var filterShow = $('#slShow').val();
        if (kw) {
            $("#wgMembers li").each(function () {
                var elLi = $(this);
                var name = elLi.attr("fullname");
                if (filterShow == "1") {
                    if (elLi.hasClass("ismember") && name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                } else {
                    if (name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                }
            });
        } else {
            if (filterShow == "1") {
                $("ul.widget-contacts li.ismember").show();
                $("ul.widget-contacts li:not(.ismember)").hide();
            } else {
                $("ul.widget-contacts li").show();
            }
        }

    } catch (e) {
        return;
    }

}

function reloadMenu() {
    var ajaxUri = '/Apps/NavigationCleanBooksPartial?tab=config';
    $.LoadingOverlay("show");
    $('#navigationcleanBook').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
}