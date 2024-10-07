
var $_members = []; var $_reviewers = []; var $_approvers = [];
var $wgId = 0;

$(document).ready(function () {
    $('#wg-key-search').keyup(delay(function () {
        $('#wg-table').DataTable().ajax.reload();
    }, 500));

    $('#wg-processes-filter').change(function () {
        $('#wg-table').DataTable().ajax.reload();
    });
    $(".check-right").bootstrapToggle();
    $("#wg-table-members-add,#user-list").DataTable({
        "destroy": true,
        responsive: true,
        "pageLength": 10
    });
    $("#wg-qbicle-add").on("change",
        function () {
            var id = $(this).val();
            InitTopic(id, "wg-topic-add");
        });

    $("#wg-table_filter").show();
    setTimeout(function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: true,
            selectAllJustVisible: true,
            includeResetOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true,
            enableFiltering: true,
            enableCaseInsensitiveFiltering: true
        });
        $(".chosen-multiple").select2({ multiple: true });
        $('select.select2').select2();
    }, 500);

    InitTopic($("#wg-qbicle-add").val(), "wg-topic-add");
    //InitWorkgroupDataTable();
});
function InitTopic(qbicleId, topicId) {
    $.ajax({
        type: "post",
        url: "/Topics/GetTopic2SelectByQbicle",
        datatype: "json",
        data: {
            qbicleId: qbicleId
        },
        success: function (refModel) {

            if (refModel.result) {
                var $topic = $('#' + topicId);
                if ($topic.data('select2')) {
                    $topic.select2('destroy');
                }
                $topic.empty().append(refModel.Object);
                $topic.select2();
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};

function ClearForm(id) {
    $_members = [];
    $_approvers = [];
    $_reviewers = [];
    $wgId = 0;
    ReInitUsersEdit(0);
    ResetFormControl(id);
    var parent = $(".btnNextAdd").closest('.modal');
    if ($(parent).find('.app_subnav .active')[0].id === "step_1_add")
        $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');

    var table = $("#wg-table-members-add").DataTable();
    table.clear();
    table.destroy();
    $("#wg-group-add").val('').trigger("change");
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
            url: "/TraderWorkGroups/ValidateName",
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
        addTable.DataTable().search(group).draw();
    });

$("#search-member-add").keyup(function () {
    addTable.DataTable().search($(this).val()).draw();
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
    allTable.DataTable().search($(this).val()).draw();
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
    if (ValidateWorkgroupForm("add") === false)
        return;

    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-add").val()
    };
    if ($("#form-wg-add").valid()) {

        $.ajax({
            url: "/TraderWorkGroups/ValidateName",
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
                $.LoadingOverlay("show");
                $.ajax({
                    type: "post",
                    url: "/TraderWorkGroups/Create",
                    datatype: "json",
                    data: {
                        wg: wg
                    },
                    success: function (refModel) {
                        $.LoadingOverlay("hide");
                        if (refModel.result) {
                            cleanBookNotification.createSuccess();
                            $("#wg-table").DataTable().ajax.reload();
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
    if ($("#wg-group-" + action).val() !== null && $("#wg-group-" + action).val().length > 0) {
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

ValidateWorkgroupForm = function (form) {

    var isValid = true;

    if (($("#wg-group-" + form).val() === null || $("#wg-group-" + form).val().length <= 0)) {
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
        url: "/TraderWorkGroups/Delete",
        datatype: "json",
        data: {
            id: id
        },
        success: function (refModel) {

            if (refModel.result) {
                $("#" + tableId).DataTable().ajax.reload();
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

ReloadWorkgroup = function () {
    $("#comfig-content").LoadingOverlay("show");
    $("#comfig-content").empty();
    setTimeout(function () {
        $("#comfig-content").load("/TraderConfiguration/TraderConfigurationContent?value=Workgroups");
        $("#comfig-content").LoadingOverlay("hide");
    },
        500);
};

//Edit feature
QbicleEditChange = function (cube) {

    var id = $(cube).val();
    InitTopic(id, "wg-topic-edit");
}

NextEdit = function () {

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
        url: "/TraderWorkGroups/GetWorkGroupUser",
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
        $('#workgroup-content-edit').load('/TraderWorkGroups/Edit?id=' + id);
        $.LoadingOverlay("hide");
    },
        2000);
};


ReInitUsersEdit = function (id) {
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
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

};


UpdateWorkgroup = function () {
    if (ValidateWorkgroupForm("edit") === false)
        return;
    
    var wGroup = {
        Id: $wgId,
        Name: $("#wg-name-edit").val()
    };
    $.ajax({
        url: "/TraderWorkGroups/ValidateName",
        data: { wg: wGroup },
        type: "POST",
        dataType: "json"
    }).done(function (refModel) {
        if (refModel.result) {
            $("#form-wg-edit").validate().showErrors({ wgNameEdit: "Name of Workgroup already exists." });
            PreviousEdit();
            return;
        } else {
            $("#app-trader-group-edit").modal('toggle');
            var wg = CreateWorkgroupData($wgId);

            $.ajax({
                type: "post",
                url: "/TraderWorkGroups/Update",
                datatype: "json",
                data: {
                    wg: wg
                },
                success: function (refModel) {
                    if (refModel.result) {

                        $("#wg-table").DataTable().ajax.reload();
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

    });

};


function InitWorkgroupDataTable() {
    //alert(2);
    var dataTable = $("#wg-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#wg-table').LoadingOverlay("show");
        } else {
            $('#wg-table').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "ajax": {
                "url": '/Commerce/GetWorkgroupTableData',
                //"url": '/Trader/AddTraderTableWorkGroup',
                "type": 'POST',
                "dataType": 'json',
                "data": function (d) {
                    var keySearchElm = $("#wg-key-search");
                    var processFilterElm = $("#wg-processes-filter");

                    keySearch = "";
                    lstProcessIds = null;

                    if (keySearchElm.length > 0)
                        keySearch = keySearchElm.val();
                    if (processFilterElm.length > 0)
                        lstProcessIds = processFilterElm.val()

                    return $.extend({}, d, {
                        "keySearch": keySearch,
                        "lstProcessIds": lstProcessIds
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "Creator",
                    orderable: false
                },
                {
                    data: "CreatedDate",
                    orderable: false
                },
                {
                    data: "Location",
                    orderable: false
                },
                {
                    data: "Process",
                    orderable: false
                },
                {
                    data: "QbicleName",
                    orderable: false
                },
                {
                    data: "MemberNum",
                    orderable: false
                },
                {
                    data: "ProductGroupNum",
                    orderable: false
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        if (row.IsOptionBtnShown == false) {
                            return htmlString;
                        }

                        htmlString += '<button class="btn btn-warning" onclick="Edit(' + row.Id + ')" data-toggle="modal" data-target="#app-trader-group-edit"><i class="fa fa-pencil"></i></button> ';

                        if (row.CanBeDeleted == false) {
                            htmlString += '<button class="btn btn-danger" disabled=""><i class="fa fa-trash"></i></button>';
                        }
                        else {
                            htmlString += '<button class="btn btn-danger" onclick="Delete(\'wg-table\', \'' + row.Id + '\')"><i class="fa fa-trash"></i></button>';
                        }

                        return htmlString;
                    }
                }
            ],
            "drawCallback": function (settings) {
                //$('#wg-table').DataTable().columns.adjust().responsive.recalc();
                //    var handleDiscountChange = delay(function (e) {
                //        $("#recalculatebtn").removeAttr("disabled");
                //        var $elm = $(this);
                //        var $row = $elm.parents("tr");
                //        var $table = $('#order-list').DataTable();
                //        //var rowData = $table.row($row).data();
                //        var itemId = $($row).find("input[type=number]").attr("itemId");
                //        $("#taxes" + itemId).parent().addClass('taxItem' + itemId);

                //        //Update Tax row data
                //        var tradeOrderId = $("#tradeorderid").val();
                //        var discount = $("#itemdiscount" + itemId).val();
                //        discount = discount ? discount : 0;

                //        if (discount < 0) {
                //            cleanBookNotification.error("The discount must be greater or equal to 0.", "Qbicles");
                //            discount = 0;
                //            $("#itemdiscount" + itemId).val(0);
                //        }
                //        else if (discount > 100) {
                //            cleanBookNotification.error("The discount must be less than or equal to 100.", "Qbicles");
                //            discount = 0;
                //            $("#itemdiscount" + itemId).val(0);
                //        }

                //        var _lstTaxes = $("#taxes" + itemId);
                //        //var htmlString = "";
                //        //var priceDisable = "disabled";
                //        if (_lstTaxes != null) {
                //            LoadingOverlay();
                //            $.ajax({
                //                method: 'POST',
                //                dataType: 'JSON',
                //                url: "/B2C/ReCalculateTax",
                //                data: {
                //                    tradeOrderId: tradeOrderId,
                //                    discount: discount,
                //                    itemId: itemId
                //                },
                //                success: function (response) {
                //                    if (response.result) {
                //                        UpdateOrderItemInfo(itemId);
                //                    } else {
                //                        LoadingOverlayEnd()
                //                        cleanBookNotification.error(response.msg, "Qbicles");
                //                    }
                //                },
                //                error: function (err) {
                //                    LoadingOverlayEnd()
                //                    cleanBookNotification(err.msg, "Qbicles");
                //                }
                //            });
                //        } else {
                //            $table.cell('.taxItem' + itemId).data("--");
                //        };

                //    }, 1000);

                //    var handlePriceChange = delay(function (e) {
                //        $("#recalculatebtn").removeAttr("disabled");
                //        var $elm = $(this);
                //        var $row = $elm.parents("tr");
                //        var $table = $('#order-list').DataTable();
                //        //var rowData = $table.row($row).data();
                //        var itemId = $($row).find("input[type=number]").attr("itemId");
                //        $("#itemdiscount" + itemId).parent().addClass('discountContainer' + itemId);

                //        var itemPurePrice = parseFloat($("#pureprice" + itemId).val());
                //        var itemTotalPrice = parseFloat($("#totalprice" + itemId).val());
                //        var newPrice = parseFloat($("#itemprice" + itemId).val());
                //        var newDiscount = parseFloat(100 - (newPrice / itemTotalPrice * 100));
                //        if (newDiscount < 0) {
                //            cleanBookNotification.error('The updated price must not cause the discount to be less than 0.', 'Qbicles');
                //            newDiscount = 0;
                //            $("#itemprice" + itemId).val(itemTotalPrice);
                //        } else if (newDiscount > 100) {
                //            cleanBookNotification.error('The updated price must not cause the discount to be greater than 100.', 'Qbicles');
                //            newDiscount = 0;
                //            $("#itemprice" + itemId).val(itemTotalPrice);
                //        }
                //        $table.cell('.discountContainer' + itemId).data(newDiscount);
                //        LoadingOverlay();
                //        UpdateOrderItemInfo(itemId);
                //    }, 1000);

                //    $(".itemdiscount").on("change", handleDiscountChange);
                //    $(".price").on("change", handlePriceChange);
            }
        });
}