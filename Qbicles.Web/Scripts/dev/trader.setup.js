var $locationCompleted = $("#locationCompleted").val();
var $productGroupCompleted = $("#productGroupCompleted").val();
var $contactGroupCompleted = $("#contactGroupCompleted").val();
var $workGroupCompleted = $("#workGroupCompleted").val();
var $accountingCompleted = $("#accountingCompleted").val();
var $isBookkeeping = $("#isBookkeeping").val();

//////    Location     /////////////////
function ValidateFormLocation() {
    var valid = false;
    if ($('#tradersetup_addlocation_name').val() === "") {
        valid = true;
        $('#tradersetup_addlocation_form').validate().showErrors({ location: "Location name is required." });
    }
    return valid;
};
function showprocessLocation(val) {
    $('#show-locations .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-locations .btn_process').removeClass("disabled");
        $locationCompleted = "True";
    } else {
        $('#show-locations .btn_process').addClass("disabled");
        UpdateSelectedStep("location");
        $locationCompleted = "False";
    }
};

function onclickAddLocation() {
    ModalToggle("app-trader-location-add", "/TraderConfiguration/LocationAddEdit");
};
function editLocation(id) {
    ModalToggle("app-trader-location-add", "/TraderConfiguration/LocationAddEdit?id=" + id);
};

function saveLocation() {

    if ($("#location-edit-name").val() == "") {
        $("#location_form").validate().showErrors({ Name: "This field is required." });
        return false;
    }

    var long = parseFloat($("#Longitude").val());
    if ($("#Longitude").val() != "") {
        if (long < -180 || long > 180) {
            $("#location_form").validate().showErrors({ Longitude: "Longitude value as a decimal value between -180 and 180" });
            return false;
        }
    }

    var lat = parseFloat($("#Latitude").val());
    if ($("#Latitude").val() != "") {
        if (lat < -90 || lat > 90) {
            $("#location_form").validate().showErrors({ Latitude: "Latitude value as a decimal value between -90 and 90" });
            return false;
        }
    }
    var country = $('#CountryName').val();



    var location = {
        Id: $('#location-id').val(),
        Name: $('#location-edit-name').val(),
        Address: {
            Id: $('#location-address-id').val(),
            AddressLine1: $('#AddressLine1').val(),
            AddressLine2: $('#AddressLine2').val(),
            City: $('#City').val(),
            State: $('#State').val(),
            PostCode: $('#PostCode').val(),
            Email: $('#Email').val(),
            Phone: $('#Phone').val(),
            Longitude: long.toFixed(7),
            Latitude: lat.toFixed(7)
        }
    }
    $.ajax({
        type: 'post',
        url: '/TraderConfiguration/Savelocation?country=' + country,
        data: { location: location },
        dataType: 'json',
        success: function (response) {

            if (response.actionVal === 1) {
                showprocessLocation(true);
                ReloadTable("show-locations .table_location", "app-trader-location-add", "/Trader/AddTraderTableLocation", response.actionVal);
            } else if (response.actionVal === 2) {
                showprocessLocation(true); 
                ReloadTable("show-locations .table_location", "app-trader-location-add", "/Trader/AddTraderTableLocation", response.actionVal);
            } else if (response.actionVal === 3) {
                showprocessLocation(false); 
                $("#tradersetup_addlocation_form").validate().showErrors({ Name: response.msg });
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_location_add');
    });
};
function deleteLocation(id) {
    $.ajax({
        type: 'delete',
        url: '/TraderSetup/DeleteLocation?id=' + id,
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                ReloadTable("show-locations .table_location", "app-trader-location-add", "/Trader/AddTraderTableLocation", 4);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_290"), "Qbicles");
            }
            // show button process
            showprocessLocation(res.result);
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function CancelLocation() {
    $('#app-trader-location-add').modal('toggle');
};


///////////    Trader Group         ////////////////////
function ValidateFormProductGroup() {
    var valid = false;
    if ($('#trader_group_add_name').val() === "") {
        valid = true;
        $('#trader_add_group_form').validate().showErrors({ title: "Group name is required." });
    }
    return valid;
}
function ValidateFormContactGroup() {
    var valid = false;
    if ($('#trader_contact_group_add_name').val() === "") {
        valid = true;
        $('#trader_add_contact_group_form').validate().showErrors({ title: "Group name is required." });
    }
    return valid;
}
function onclickAddProductGroup() {
    ModalToggle("app-trader-product-group-add", "/Trader/AddTraderProductGroup?id=" + 0);
}
function onclickAddContactGroup() {
    ModalToggle("app-trader-contact-group-add", "/Trader/AddTraderContactGroup?id=" + 0);
}
function editProductGroup(id) {
    ModalToggle("app-trader-product-group-add", "/Trader/AddTraderProductGroup?id=" + id);
};
function editContactGroup(id) {
    ModalToggle("app-trader-contact-group-add", "/Trader/AddTraderContactGroup?id=" + id);
};
function showprocessGroup(val) {
    $('#show-product-groups .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-product-groups .btn_process').removeClass("disabled");
        $productGroupCompleted = "True";
    } else {
        $('#show-product-groups .btn_process').addClass("disabled");
        UpdateSelectedStep("group");
        $productGroupCompleted = "False";
    }
};
function SaveProductGroup() {
    if (ValidateFormProductGroup()) {
        return;
    }
    var group = {
        Id: $('#trader_group_add_id').val(),
        Name: $('#trader_group_add_name').val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderSetup/SaveGroup',
        data: { group: group },
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");
            }
            if (res.actionVal !== 3) { ReloadTable("show-product-groups .table_group", "app-trader-product-group-add", "/Trader/AddTraderTableProductGroup", res.actionVal); }

            // show button process
            showprocessGroup(res.result);
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function showprocessContactGroup(val) {
    $('#show-contact-groups .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-contact-groups .btn_process').removeClass("disabled");
        $contactGroupCompleted = "True";
    } else {
        $('#show-contact-groups .btn_process').addClass("disabled");
        UpdateSelectedStep("contact");
        $contactGroupCompleted = "False";
    }
};
function SaveContactGroup(type) {
    if (ValidateFormContactGroup()) {
        return;
    }
    var contactGroup = {
        Id: $('#trader_contact_group_add_id').val(),
        Name: $('#trader_contact_group_add_name').val(),
        saleChannelGroup: type
    }
    $.ajax({
        type: 'post',
        url: '/TraderSetup/SaveContactGroup',
        data: { group: contactGroup },
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [res.msg]), "Qbicles");
            }
            if (res.actionVal !== 3) { ReloadTable("show-contact-groups .table_group", "app-trader-contact-group-add", "/Trader/AddTraderTableContactGroup", res.actionVal); }

            // show button process
            showprocessContactGroup(res.result);
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function deleteGroup(id) {
    $.ajax({
        type: 'delete',
        url: '/TraderSetup/DeleteGroup?id=' + id,
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                ReloadTable("show-product-groups .table_group", "app-trader-product-group-add", "/Trader/AddTraderTableProductGroup", 4);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_292"), "Qbicles");
            }
            // show button process
            showprocessGroup(res.result);
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function deleteContactGroup(id) {
    $.ajax({
        type: 'delete',
        url: '/TraderSetup/DeleteContactGroup?id=' + id,
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                ReloadTable("show-contact-groups .table_group", "app-trader-contact-group-add", "/Trader/AddTraderTableContactGroup", 4);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_292"), "Qbicles");
            }
            // show button process
            showprocessContactGroup(res.result);
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function CancelProductGroup() {
    $('#app-trader-product-group-add').modal('toggle');
};

var $_members = []; var $_reviewers = []; var $_approvers = []; var $wgId = 0;

function onclickAddWorkGroup() {
    ModalToggle("app-trader-group-add", "/Trader/AddTraderWorkGroup?id=" + 0);
};
function editWorkGroup(id) {
    ModalToggle("app-trader-group-add", "/Trader/AddTraderWorkGroup?id=" + id);
};

function showprocessWorkGroup(val) {
    $('#show-workgroups .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-workgroups .btn_process').removeClass("disabled");
        $workGroupCompleted = "True";
    } else {
        $('#show-workgroups .btn_process').addClass("disabled");
        UpdateSelectedStep("wg");
        $workGroupCompleted = "False";
    }
};
function SaveWorkGroup() {
    if (ValidateWorkgroupForm("add") === false)
        return;
    var wg = CreateWorkgroupData(0);
    
    $.ajax({
        type: "post",
        url: "/TraderWorkGroups/Create",
        datatype: "json",
        data: {
            wg: wg
        },
        success: function (refModel) {
            if (refModel.result) {
                ReloadTable("show-workgroups .table_workgroup", "app-trader-group-add", "/Trader/AddTraderTableWorkGroup", 1);
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_380",[refModel.msg]), "Qbicles");
            }

            showprocessWorkGroup(refModel.actionVal === 1 ? true : false);
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};
function CreateWorkgroupData(id) {

    var action = "add";
    if (id > 0)
        action = "edit";
    var location = {
        Id: $("#wg-location-" + action).val()
    };

    var qbicle = {
        Id: $("#wg-qbicle-" + action).val()
    };

    var topic = {
        Id: $("#wg-topic-" + action).val()
    };

    var itemCategories = [];
    if ($("#wg-group-" + action).val().length > 0) {
        var categories = $("#wg-group-" + action).val().toString().split(",");
        $.each(categories,
            function (index, itemId) {
                var gr = {
                    Id: itemId
                };
                itemCategories.push(gr);
            });
    }
    var wgProcesses = [];
    var processes = $("#wg-process-" + action).val();
    if (processes !== null && processes.length > 0) {
        processes = processes.toString().split(",");
        $.each(processes,
            function (index, itemId) {
                var pr = {
                    Id: itemId
                };
                wgProcesses.push(pr);
            });
    }

    var workGroup = {
        Id: id,
        Name: $("#wg-name-" + action).val(),
        Processes: wgProcesses,
        Location: location,
        Qbicle: qbicle,
        Topic: topic,
        ItemCategories: itemCategories,
        Members: $_members,
        Reviewers: $_reviewers,
        Approvers: $_approvers
    };
    return workGroup;
};

function ClearForm(id) {
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    $wgId = 0;
    ReInitUsersEdit(0);
    ResetFormControl(id);
    var parent = $(".btnNextAdd").closest('.modal');
    if ($(parent).find('.app_subnav .active')[0].id === "li-roles-add")
        $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');

    var table = $("#wg-table-members-add").DataTable();
    table.clear();
    table.destroy();
    $("#wg-group-add").val('').trigger("change");
};
function ReInitUsersEdit(id) {
    var table = $("#user-list").DataTable();

    table.clear();
    table.destroy();
    $.ajax({
        type: "post",
        url: "/TraderWorkGroups/ReInitUsersEdit",
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
                cleanBookNotification.error(_L("ERROR_MSG_380",[refModel.msg]), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

};
function AddUsersToMembers(isCheck, userId, userName, userPic) {

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
function AddMemberToWorkgroup() {
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
function AddUsersToReviewers(isCheck, userId, spanKey) {
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
function AddUsersApprovers(isCheck, userId, spanKey) {
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
function Edit(id) {
    $('#workgroup-content-edit').empty();
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    PreviousEdit();
    $wgId = id;
    $.LoadingOverlay("show");
    $.ajax({
        type: "post",
        url: "/TraderWorkGroups/GetWorkGroupUser",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {
            $_members = refModel.Members;
            $_approvers = refModel.Approvers;
            $_reviewers = refModel.Reviewers;
            ReInitUsersEdit(id);
            $('#workgroup-content-edit').load('/TraderWorkGroups/Edit?id=' + id);
            $.LoadingOverlay("hide");
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

};

function NextEdit() {

    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-edit").val()
    };

    if ($("#form-wg-edit").valid()) {
        $.ajax({
            url: "/TraderWorkGroups/ValidateName",
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

                $("#step_1").removeClass('active').addClass('disabled disabledTab');
                $("#edit-specifics").removeClass('in active').addClass('disabled disabledTab');

                $("#step_2").removeClass('disabled disabledTab').addClass('active');
                $("#edit-members").removeClass('disabled disabledTab').addClass('in active');

                $("#step2-vtab").css({ 'color': '#337ab7' });
                $("#step1-vtab").removeAttr('style');
            }

        }).fail(function (er) {
            $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Error" + er });
        });
    }
};

function PreviousEdit() {
    $("#step_2").removeClass('active').addClass('disabled disabledTab');
    $("#edit-members").removeClass('in active').addClass('disabled disabledTab');

    $("#step_1").removeClass('disabled disabledTab').addClass('active');
    $("#edit-specifics").removeClass('disabled disabledTab').addClass('in active');

    $("#step1-vtab").css({ 'color': '#337ab7' });
    $("#step2-vtab").removeAttr('style');
};

function UpdateWorkgroup() {
    
    if (ValidateWorkgroupForm("edit") === false)
        return;
    var wg = CreateWorkgroupData($wgId);
    
    if (wg.Processes.length === 0) {
        cleanBookNotification.error("Process is required!", "Qbicles");
        return;
    }
    $.ajax({
        type: "post",
        url: "/TraderWorkGroups/Update",
        datatype: "json",
        data: {
            wg: wg
        },
        success: function (refModel) {
            if (refModel.result) {
                
                ReloadTable("show-workgroups .table_workgroup", "app-trader-group-edit", "/Trader/AddTraderTableWorkGroup", 2);
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_380",[refModel.msg]), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};
function Delete(tableId, id) {
    $.ajax({
        type: "delete",
        url: "/TraderSetup/DeleteWorkgroup",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {
            if (refModel.result) {
                ReloadTable("show-workgroups .table_workgroup", "app-trader-group-edit", "/Trader/AddTraderTableWorkGroup", 4);
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_293"), "Qbicles");
            }
            showprocessWorkGroup(refModel.result);
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

};

ValidateWorkgroupForm = function (form) {

    var isValid = true;

    if (($("#wg-group-" + form).val() == null || $("#wg-group-" + form).val().length <= 0)) {
        isValid = false;
        if (form === "add")
            $("#form-wg-" + form).validate().showErrors({ wgProductsNameAdd: "This field is required." });
        else
            $("#form-wg-" + form).validate().showErrors({ wgProductsNameEdit: "This field is required." });
    }
    if ($("#wg-name-" + form).val() === "") {
        if (form === "add")
            $("#form-wg-" + form).validate().showErrors({ wgNameAdd: "This field is required." });
        else
            $("#form-wg-" + form).validate().showErrors({ wgNameEdit: "This field is required." });
        isValid = false;
    }

    var processes = $("#wg-process-" + form).val();
    if (processes === null || processes.length === 0) {
        cleanBookNotification.error(_L("ERROR_MSG_653"), "Qbicles");
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
///////    Setup Account Config     ////////
function ValidateFormTaxrate() {
    var valid = false;
    if ($('#form_taxrate_add_name').val() === "") {
        valid = true;
        $('#trader_form_taxrate_add').validate().showErrors({ Name: "TaxRate name is required." });
    }
    return valid;
};

function showprocessSetting(val) {
    $('#show-accounting .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-accounting .btn_process').removeClass("disabled");
        $accountingCompleted = "True";
    } else {
        $('#show-accounting .btn_process').addClass("disabled");
        UpdateSelectedStep("accounting");
        $accountingCompleted = "False";
    }
};

function onclickAddTaxRate() {
    ModalToggle("app-trader-tax-add", "/Trader/AddTraderTaxRate?id=" + 0);
};
function editTaxRate(id) {
    ModalToggle("app-trader-tax-add", "/Trader/AddTraderTaxRate?id=" + id);
};
function deleteTaxRate(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'delete',
        url: '/TraderSetup/DeleteTaxRate?id=' + id,
        datatype: 'json',
        success: function (refModel) {
            ReloadTable("show-accounting .table_group", "app-trader-tax-add", "/Trader/AddTraderTableTaxRate", 4);
            $.LoadingOverlay("hide");
            showprocessSetting(refModel.result);
        },
        error: function (err) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}


function UpdateJournalGroupDefault(traderSettingId) {
    var data = { journalGroupId: $("#journal-group-default").val(), traderSettingId: traderSettingId };
    $.ajax({
        type: 'post',
        url: '/TraderConfiguration/UpdateJournalGroupDefault',
        datatype: 'json',
        async: false,
        data: data,
        success: function (refModel) {
            if (refModel.result) {
                $.LoadingOverlay("hide");
                $('#show-accounting .btn_process').removeClass("disabled");
                cleanBookNotification.updateSuccess();
                showprocessSetting(refModel.result);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}

function SaveTaxRate() {
    if (ValidateFormTaxrate()) {
        return;
    }
    var taxrate = {
        Id: $('#trader_form_taxrate_add_id').val(),
        Name: $('#form_taxrate_add_name').val(),
        Rate: $('#trader_form_taxrate_add_Rate').val(),
        Description: $('#trader_form_taxrate_add_description').val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderSetup/SaveTaxRate',
        data: { taxrate: taxrate },
        datatype: 'json',
        success: function (res) {
            if (res.actionVal === 1) {
                $('#bookkeeping-connected table_trader_taxrate').removeClass("hidden");
            } else if (res.actionVal === 2) {
                $('#bookkeeping-connected table_trader_taxrate').removeClass("hidden");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_380",[res.msg]), "Qbicles");
            }
            if (res.actionVal !== 3) {
                $('#app-trader-tax-add').modal('toggle');
                SelectAccounting('accounting');
            }

            showprocessSetting(res.result);
            // show button process 
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
function CancelTaxrate() {
    $('#app-trader-tax-add').modal('toggle');
};


function ModalToggle(modal, ajaxUri) {
    $('#' + modal).empty();
    $.LoadingOverlay("show");
    $('#' + modal).load(ajaxUri, function () {
        $('#' + modal).modal('toggle');
        $.LoadingOverlay("hide");
    });
}

function ReloadTable(elementId, modalToggle, ajaxUri, actionAjax) {

    $.LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).removeClass("hidden");
    $('#' + elementId).load(ajaxUri, function () {
        if (actionAjax <= 2)
            $('#' + modalToggle).modal('toggle');
        if (actionAjax === 1)
            cleanBookNotification.createSuccess();
        else if (actionAjax === 2)
            cleanBookNotification.updateSuccess();
        else
            cleanBookNotification.removeSuccess();
        $.LoadingOverlay("hide");
    });
};




function SelectLocation(name) {
    SelectedStep(name, "Location", "/TraderSetup/ShowLocation");
};
function SelectGroup(name) {
    SelectedStep(name, "ProductGroup", '/TraderSetup/ShowProductGroup');
};
function SelectContact(name) {
    SelectedStep(name, "Contact", '/TraderSetup/ShowContactGroup');
};
function SelectWorkGroup(name) {
    SelectedStep(name, "Workgroup", '/TraderSetup/ShowWorkGroup');
};
function SelectAccounting(name) {
    SelectedStep(name, "Accounting", '/TraderSetup/ShowAccounting');
};
function SelectComplete(name) {
    SelectedStep(name, "Complete", '/TraderSetup/ShowComplete');
};


function UpdateTraderIsSettingComplete(isComplete, goToTrader) {

    if (goToTrader === "TraderApp")
        isComplete = "TraderApp";

    $.ajax({
        type: 'post',
        url: '/TraderSetup/UpdateTraderIsSettingComplete',
        data: { isComplete: isComplete },
        datatype: 'json',
        success: function (res) {
            if (goToTrader === 'TraderApp') {
                if (res === true) {
                    window.location.href = "/Trader/AppTrader";
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            } else {
                UpdateSelectedStep(goToTrader);
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function UpdateTraderSettingCompleted(goToTrader) {


    $.ajax({
        type: 'post',
        url: '/TraderSetup/UpdateTraderIsSettingComplete',
        data: { isComplete: goToTrader },
        datatype: 'json',
        success: function (res) {
            if (res === true) {
                window.location.href = "/Trader/AppTrader";
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function LocationProcess() {
    switch_trader_setup('#show-locations', '#show-product-groups');
    ShowStep("ProductGroup", "show_content", "/TraderSetup/ShowProductGroup");
};
function GroupProcess() {
    switch_trader_setup('#show-product-groups', '#show-contact-groups');
    ShowStep("Contact", "show_content", "/TraderSetup/ShowContactGroup");
};
function ContactGroupProcess() {
    switch_trader_setup('#show-contact-groups', '#show-workgroups');
    ShowStep("Workgroup", "show_content", "/TraderSetup/ShowWorkGroup");
};
function WorkGroupProcess() {
    switch_trader_setup('#show-workgroups', '#show-accounting');
    ShowStep("Accounting", "show_content", "/TraderSetup/ShowAccounting");
};
function AccountProcess() {
    switch_trader_setup('#show-accounting', '#show-success');
    ShowStep("Complete", "show_content", "/TraderSetup/ShowComplete");
};

function ShowStep(stepName, stepId, stepUri) {
    $.LoadingOverlay("show");
    UpdateTraderIsSettingComplete(stepName, 'setup');
    $('#' + stepId).empty();
    $('#' + stepId).load(stepUri, function () {
        $.LoadingOverlay("hide");
    });
};

function switch_trader_setup(from, to) {
    $(from).hide();
    $(to).fadeIn();

    $('.wizard-steps li').each(function () {
        if ($('a', this).data('form') === from) {
            $(this).removeClass('active').addClass('complete');
        }
        if ($('a', this).data('form') === to) {
            $(this).addClass('active').removeClass('complete').addClass('incomplete');
        }
    });
};



function UpdateSelectedStep(stepName) {
    $(".tradersetup_" + stepName).removeClass('complete').addClass('incomplete');
};


function SelectedStep(name, step, traderUri) {
    $('ul.tradersetup_ul li').removeClass('active');
    $('.tradersetup_' + name).addClass('active');


    UpdateTraderIsSettingComplete(step, name);
    $.LoadingOverlay("show");
    if (name !== "location")
        if ($locationCompleted === "True" || $isBookkeeping) {
            $(".tradersetup_location").removeClass('incomplete').addClass('complete');
        } else {
            $(".tradersetup_location").removeClass('complete').addClass('incomplete');
        }


    if (name !== "group")
        if ($productGroupCompleted === "True") {
            $(".tradersetup_group").removeClass('incomplete').addClass('complete');
        } else {
            $(".tradersetup_group").removeClass('complete').addClass('incomplete');
        }
    if (name !== "contact")
        if ($contactGroupCompleted === "True") {
            $(".tradersetup_contact").removeClass('incomplete').addClass('complete');
        } else {
            $(".tradersetup_contact").removeClass('complete').addClass('incomplete');
        }
    if (name !== "wg")
        if ($workGroupCompleted === "True") {
            $(".tradersetup_wg").removeClass('incomplete').addClass('complete');
        } else {
            $(".tradersetup_wg").removeClass('complete').addClass('incomplete');
        }

    if (name !== "accounting")
        if ($accountingCompleted === "True") {
            $(".tradersetup_accounting").removeClass('incomplete').addClass('complete');
        } else {
            $(".tradersetup_accounting").removeClass('complete').addClass('incomplete');
        }
    $('#show_content').empty();
    $('#show_content').load(traderUri);
};