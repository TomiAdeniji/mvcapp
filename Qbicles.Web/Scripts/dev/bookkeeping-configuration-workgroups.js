var $_members = []; var $_reviewers = []; var $_approvers = [];
var $wgId = 0;
$(".check-right").bootstrapToggle();
$(".chosen-multiple").select2({ multiple: true });
//$(".datatable").DataTable({
//    responsive: true,
//    "lengthChange": true,
//    "pageLength": 10,
//    "columnDefs": [
//        {
//            "targets": 3,
//            "orderable": true
//        }
//    ],
//    "order": []
//});

//$(".datatable").show();


$("#wg-qbicle-add").on("change",
    function () {
        var id = $(this).val();
        InitTopic(id, "wg-topic-add");
    });

InitTopic = function (qbicleId, topicId) {
    if (qbicleId === null) {
        console.error("qbicle Id is null.");
        return;
    }
    $.ajax({
        type: "post",
        url: "/Topics/GetTopic2SelectByQbicle",
        datatype: "json",
        data: {
            qbicleId: qbicleId
        },
        success: function (refModel) {

            if (refModel.result) {
                $("#" + topicId).empty().append(refModel.Object);
                $('#' + topicId).select2();
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};

ClearForm = function (id) {
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    $wgId = 0;
    ReInitUsersEdit(0);
    ResetFormControl(id);
    var parent = $(".btnNextAdd").closest('.modal');
    if ($(parent).find('.app_subnav .active')[0].id === "step_2_add")
        $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');

    var table = $("#wg-table-members-add").DataTable();
    table.clear();
    table.destroy();
    $("#wg-process-add").val('').trigger("change");


    $("#app-trader-group-add").modal('show');
};

// Cycle app nav tabs with button triggers
PreviousAdd = function () {
    var parent = $(".btnNextAdd").closest('.modal');
    $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
};

$('.btnPreviousAdd').click(function () {
    var parent = $(this).closest('.modal');
    $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
});
$('.btnNextAdd').click(function () {
    var wGroup = {
        Id: 0,
        Name: $("#wg-name-add").val()
    };

    if ($("#form-wg-add").valid()) {
        $.ajax({
            url: "/BkWorkGroups/ValidateName",
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
// End Cycle app nav tabs with button triggers


function RemoveRowTableMemberAdd(id) {
    var actionTable = "add";
    if ($wgId > 0)
        actionTable = "edit";
    var table = $("#wg-table-members-" + actionTable).DataTable();
    table.destroy();
    $("#wg-table-members-" + actionTable + " tbody tr#tr_" + actionTable + "_user_" + id).remove();
    $("#wg-table-members-" + actionTable).DataTable().draw();

    for (var a = $_approvers.length; a--;) {
        if ($_approvers[a].Id === id) $_approvers.splice(a, 1);
    }
    for (var r = $_reviewers.length; r--;) {
        if ($_reviewers[r].Id === id) $_reviewers.splice(r, 1);
    }
    for (var m = $_members.length; m--;) {
        if ($_members[m].Id === id) $_members.splice(m, 1);
    }
}






//search on table user add new
var addTable = $("#wg-table-members-add");
$("#search-right-add").on("change",
    function () {
        var group = $(this).val();
        addTable.DataTable().search(group, true, false, true).draw();
    });

$("#search-member-add").keyup(function () {
    addTable.DataTable().search($(this).val(), true, false, true).draw();
});

//
var allTable = $("#user-list");
$("#search-right-all").on("change",
    function () {
        var group = $(this).val();
        if (group === null)
            group = "";
        allTable.DataTable().search(group, true, false, true).draw();
    });

$("#search-member-all").keyup(function () {
    allTable.DataTable().search($(this).val(), true, false, true).draw();
});

//end search on table user add new

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

CreateWorkgroup = function () {
   

    if (!$("#form-wg-add").valid()) {
        PreviousAdd();
        return;
    }

    
    if ($("#wg-process-add").val() === null || $("#wg-process-add").val() === "") {
        PreviousAdd();
        cleanBookNotification.error(_L("ERROR_MSG_454"), "Qbicles");
        return;
    }
    $.LoadingOverlay("show");




    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-add").val()
    };

    $.ajax({
        url: "/BkWorkGroups/ValidateName",
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

            var wg = CreateWorkgroupData(0);

            $.ajax({
                type: "post",
                url: "/BkWorkGroups/Create",
                datatype: "json",
                data: {
                    wg: wg
                },
                success: function (refModel) {
                    if (refModel.result) {
                        cleanBookNotification.createSuccess();
                        $("#app-trader-group-add").modal('hide');
                        ReloadWorkgroup();
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
        $("#form-wg-add").validate().showErrors({ wgNameAdd: "Error" + er });
        
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function CreateWorkgroupData(id) {
    var action = "add";
    if (id > 0)
        action = "edit";
    

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
    var workGroup = {
        Id: id,
        Name: $("#wg-name-" + action).val(),
        Processes: wgProcesses,
        Qbicle: qbicle,
        Topic: topic,
        Members: $_members,
        Reviewers: $_reviewers,
        Approvers: $_approvers
    };
    return workGroup;
};


ReloadWorkgroup = function () {
    var ajaxUri = '/Bookkeeping/WorkGroupPartial';
    $('#tab-workgroups').empty();
    $('#tab-workgroups').load(ajaxUri, function () {
        ajaxUri = '/BKWorkGroups/BookkeepingNavigatePartial';
        $('#bk-navigate').load(ajaxUri, function () {
            $('.modal-backdrop').remove();
        });
        //$('.modal-backdrop').remove();
    });
};

//Edit feature
QbicleEditChange = function(cube) {
    var id = $(cube).val();
    InitTopic(id, "wg-topic-edit");
};

NextEdit = function () {
    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-edit").val()
    };

    if ($("#form-wg-edit").valid()) {
        $.ajax({
            url: "/BkWorkGroups/ValidateName",
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

PreviousEdit = function () {

    $("#step_2_edit").removeClass('active');
    $("#edit-members").removeClass('in active');

    $("#step_1_edit").addClass('active');
    $("#edit-specifics").addClass('in active');

};
// End Cycle app nav tabs with button triggers

Edit = function (id) {
    $.LoadingOverlay("show");
    $('#workgroup-content-edit').empty();
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    PreviousEdit();
    $wgId = id;
    $("#search-right-all").val('').trigger("change");
    
    $.ajax({
        type: "post",
        url: "/BkWorkGroups/GetWorkGroupUser",
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
    $('#workgroup-content-edit').empty();
    $('#workgroup-content-edit').load('/BkWorkGroups/Edit?id=' + id, function () {
        LoadingOverlayEnd();
        $("#app-trader-group-edit").modal('show');
    });
};


ReInitUsersEdit = function (id) {
    var table = $("#user-list").DataTable();

    table.clear();
    table.destroy();
    $.ajax({
        type: "post",
        url: "/BkWorkGroups/ReInitUsersEdit",
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


UpdateWorkgroup = function () {
    
    if (!$("#form-wg-edit").valid()) {
        PreviousAdd(); PreviousEdit();
        return;
    }

    if ($("#wg-process-edit").val() === null || $("#wg-process-edit").val() === "") {
        PreviousAdd(); PreviousEdit();
        cleanBookNotification.error(_L("ERROR_MSG_454"), "Qbicles");
        return;
    }

    $.LoadingOverlay("show");
    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-edit").val()
    };
    $.ajax({
        url: "/BkWorkGroups/ValidateName",
        data: { wg: wGroup },
        type: "POST",
        dataType: "json"
    }).done(function (refModel) {
        if (refModel.result) {
            $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Name of Workgroup already exists." });
            PreviousEdit();
            return;
        } else {
            var wg = CreateWorkgroupData($wgId);

            $.ajax({
                type: "post",
                url: "/BkWorkGroups/Update",
                datatype: "json",
                data: {
                    wg: wg
                },
                success: function (refModel) {
                    if (refModel.result) {
                        ReloadWorkgroup();
                        $("#app-trader-group-edit").modal('hide');
                        $('.modal-backdrop').remove();
                        cleanBookNotification.updateSuccess();
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
        
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    }).always(function () {
        LoadingOverlayEnd();
    });

};



var $wgIdDelete = 0;
function ConfirmDeleteWg(id) {
    $wgIdDelete = id;
    $("#name-delete").text($("#wg-name-main-" + $wgIdDelete).text());
    $("#confirm-delete").modal('show');
}


function CancelDelete() {
    $('#confirm-delete').modal('hide');
};
function DeleteWG() {
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/BkWorkGroups/Delete",
        datatype: "json",
        data: {
            id: $wgIdDelete
        },
        success: function (refModel) {

            if (refModel.result) {
                $('#confirm-delete').modal('hide');
                var table = $("#wg-table").DataTable();
                table.destroy();
                $("#wg-table tbody tr#tr_workgroup_user_" + $wgIdDelete).remove();
                $("#wg-table").DataTable().draw();
                cleanBookNotification.removeSuccess();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

}
