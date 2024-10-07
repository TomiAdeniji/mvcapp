var $viewLoaded = false;
// For domain tab
$(function () {
    if ($('#selectRolesDomain').val()) {
        $('#changeAvailableApps').removeClass("hidden");
    } else {
        $('#changeAvailableApps').addClass("hidden");
    }

    $("#selectRolesDomain").select2();
    $("#select-role-app-use").select2();
    $("#user-for-rules").select2();



    $("#selectRolesDomain").on("change", function () {
        RoleManageChange();
    });
    $("#select-role-app-use").on("change", function () {
        reloadUsersByRole();
    });
    initTblUsersByRole();
});

function OpenTraderFromAdministrator() {
    $.ajax({
        type: 'get',
        url: '/Trader/CheckTrader',
        datatype: 'json',
        success: function (response) {
            if (response) {
                window.location.href = '/Trader/AppTrader';
            } else {
                window.location.href = '/Trader/TraderSetup';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function AddRoleDomain() {
    if (!$("#add-role-domain").valid()) {
        return;
    }

    $.ajax({
        type: 'post',
        url: '/DomainRole/AddRoleDomainByName',
        datatype: 'json',
        async: false,
        data: { roleDomainName: $('#txtRoleDomainName').val() },
        success: function (refModel) {
            if (refModel.result) {
                var option = '<option value="' + refModel.Object.Id + '">' + refModel.Object.Name + '</option>';
                $('#selectRolesDomain').append(option);
                $('#selectRolesDomain').val(refModel.Object.Id).change();

                $('#select-role-app-use').append(option);
                $('#changeAvailableApps').removeClass("hidden");

                $('#add-role').modal('hide');
            }
            else {
                $("#add-role-domain").validate().showErrors({ roleDomainName: refModel.msg });
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};


RoleManageChange = function () {
    if (!$viewLoaded) return;

    $('#tbl-body-roles').empty();
    GetAppByRoleId($("#selectRolesDomain").val());
};


$('#add-role').on('shown.bs.modal', function () {
    $("#txtRoleDomainName").val("");
    $("#txtRoleDomainName").focus();
});



if ($('#selectRolesDomain').val()) {
    $('#selectRolesDomain').val($('#selectRolesDomain').val()).trigger('change');
}


if ($('#select-role-app-use').val()) {
    $('#select-role-app-use').val($('#select-role-app-use').val()).trigger('change');
}


function ChangePermissions(appId, rightId, isCheck) {
    var roleId = $('#selectRolesDomain').val();
    var data = { appId: appId, rightId: rightId, isCheck: isCheck, roleId: roleId };
    $.ajax({
        type: 'post',
        url: '/Apps/ChangePermissions',
        datatype: 'json',
        async: false,
        data: data,
        success: function (refModel) {
            if (refModel.result) {
                if (refModel.msgId !== "") {
                    $('#row-' + refModel.msgId).remove();
                }
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};

function RemoveUserFromRoles(rolesId, userId, fullName) {
    var data = { rolesId: rolesId, userId: userId };
    $.ajax({
        type: 'post',
        url: '/DomainRole/RemoveUserFromRoles',
        datatype: 'json',
        async: false,
        data: data,
        success: function (refModel) {
            if (refModel.result === false) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            } else {
                reloadUsersByRole();
                loadUsersNotExistInRole();
                cleanBookNotification.removeSuccess();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};

function AddUserToRoles() {
    var selectedValues = $("#user-for-rules").val();
    if (selectedValues === null || selectedValues === "") {
        cleanBookNotification.error(_L("ERROR_MSG_12"), "Qbicles");
        return;
    }
    var roleId = $('#select-role-app-use').val();
    $.ajax({
        type: 'POST',
        url: '/DomainRole/AddUserToRoles',
        dataType: 'JSON',
        data: JSON.stringify({ listUserId: selectedValues, roleId: roleId }),
        contentType: "application/json",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                $('#add-user').modal('hide');
                reloadUsersByRole();
                cleanBookNotification.updateSuccess();
                loadUsersNotExistInRole();

            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });

};

function initTblUsersByRole() {
    var $tblUsersByRole = $('#tblUsersByRole');
    $tblUsersByRole.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblUsersByRole.LoadingOverlay("show");
        } else {
            $tblUsersByRole.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        autoWidth: true,
        pageLength: 10,
        deferLoading: 30,
        order: [[1, "asc"]],
        ajax: {
            "url": "/DomainRole/GetDomainUsersByRoleId",
            "data": function (d) {
                return $.extend({}, d, {
                    "roleId": $("#select-role-app-use").val()
                });
            }
        },
        columns: [
            {
                "data": "AvatarUri",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlBusiness = "<div style=\"background-image: url(" + data + "&size=T)\" class=\"table-avatar mini\">&nbsp;</div>";
                    return _htmlBusiness;
                }
            },
            { "data": "FullName", "orderable": true },
            { "data": "RoleName", "orderable": false },
            {
                "data": "Id",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-danger" onclick="RemoveUserFromRoles(\'' + row.RoleId + '\', \'' + row.Id + '\', \'' + fixQuoteCode(row.FullName) + '\' )"><i class="fa fa-trash"></i></button>';
                    if (row.IsCurrentUser == true)
                        _htmlOptions = '<button class="btn btn-danger" disabled><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            }
        ]
    });

}
function reloadUsersByRole() {
    if ($.fn.DataTable.isDataTable('#tblUsersByRole')) {
        $('#tblUsersByRole').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblUsersByRole').DataTable().ajax.reload();
        }, 1500);
    }
}
function loadUsersNotExistInRole() {
    var $elusers = $('#user-for-rules');
    $.get("/DomainRole/GetUsersNotExistInRole?roleId=" + $("#select-role-app-use").val(), function (data) {
        $elusers.select2('destroy');
        $elusers.empty();
        $elusers.select2({
            placeholder: "Please select",
            data: data
        });
    });
}

function ClearUserSelect() {
    loadUsersNotExistInRole();    
};

function GenerateRows2TableAppByArr(arr) {
    var row = '';
    $.each(arr, function (index, app) {
        row += '<tr id="row-' + app.Id + '">';
        row += '  <td>' + app.Name + '</td>';
        row += '<td>';
        $.each(app.listRight, function (idx, right) {

            row += ' <div class="checkbox toggle">';
            row += ' <label>';
            row += '<input data-toggle="toggle" class="app-right" onchange="ChangePermissions(\'' + app.Id + '\', \'' + right.Id + '\', this.checked)"' + right.Ischeck + ' data-onstyle="success"  type="checkbox">';
            row += right.Name + '</label>';
            row += '</div>';
        });
        row += ' </td>';
        row += ' </tr>';
    });
    $('#tbl-body-roles').prepend(row);
    $('.app-right').bootstrapToggle('destroy');
    $('.app-right').bootstrapToggle();
};

function GetAppByRoleId(roleId) {
    $.ajax({
        type: 'POST',
        url: '/Application/GetAppByRoleId',
        dataType: 'JSON',
        data: { roleId: roleId },
        async: false,
        success: function (refModel) {
            if (refModel) {
                // genarate rows for table
                GenerateRows2TableAppByArr(refModel);

                $('input:checkbox.apps-account').each(function () {
                    $(this).bootstrapToggle('off');
                });

                $.each(refModel, function (index, app) {
                    $('#app-account-' + app.Id).bootstrapToggle('on');
                });
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });

};


LearnMoreApp = function (appId) {
    $("#app-detail-image").attr("src", $("#app-image-" + appId).text());
    $("#app-detail-name").text($("#app-name-" + appId).text());
    $("#app-detail-description").text($("#app-description-" + appId).text());
    $("#app-detail-adpage").text($("#app-adpage-" + appId).text());

    $("#app-detail").modal('show');
}

SubcribeApp = function (appId) {
    $.ajax({
        url: "/Apps/SubscribeApp",
        type: "POST",
        dataType: "JSON",
        data: { appId: appId },
        success: function (refModel) {
            if (refModel.result) {
                $("#subscribed-label-" + appId).text("Subscribed");
                $("#subscribed-label-" + appId).attr('style', 'display:block;');
                $("#learn-more-button-" + appId).attr('style', 'display:none;;margin-top: 5px');
                $("#manage-app-title-" + appId).text("Unsubscribe");
                $("#manage-app-button-" + appId).removeClass("btn-success");
                $("#manage-app-button-" + appId).addClass("btn-danger");
                $("#manage-app-button-" + appId).attr('onclick', 'UnSubcribeApp(' + appId + ')');
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error("Error: " + refModel.msg, "Qbicles");
            }
        },
        error: function (ex) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

UnSubcribeApp = function (appId) {
    $.ajax({
        url: "/Apps/UnSubscribeApp",
        type: "POST",
        dataType: "JSON",
        data: { appId: appId },
        success: function (refModel) {
            if (refModel.result) {
                $("#subscribed-label-" + appId).text("");
                $("#subscribed-label-" + appId).attr('style', 'display:none;');
                $("#learn-more-button-" + appId).attr('style', 'display:block;;margin-top: 5px');
                $("#manage-app-title-" + appId).text("Subscribe");
                $("#manage-app-button-" + appId).removeClass("btn-danger");
                $("#manage-app-button-" + appId).addClass("btn-success");
                $("#manage-app-button-" + appId).attr('onclick', 'SubcribeApp(' + appId + ')');
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error("Error: " + refModel.msg, "Qbicles");
            }
        },
        error: function (ex) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

ChangeAvailableApps = function () {

    $.ajax({
        url: "/Apps/ChangeAvailableApps",
        type: "POST",
        dataType: "JSON",
        data: { roleId: $('#selectRolesDomain').val() },
        success: function (refModel) {
            if (refModel.result) {
                $("#add-app").empty().append(refModel.msg);
                $('.apps-account').bootstrapToggle();
                $('input:checkbox.apps-account').each(function () {
                    var appReged = $(this).attr('appRed');
                    if (appReged === false)
                        $(this).bootstrapToggle('off');
                });
                $('#add-app').modal('show');
            }
        },
        error: function (ex) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"),
                "Qbicles");
        }
    });

};

function SuspendOrActive(userId, type) {
    $.ajax({
        type: 'POST',
        url: '/Qbicles/SuspendOrActive',
        dataType: 'JSON',
        async: false,
        data: { userId: userId, type: type },
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.updateSuccess();
                $("#tbdatatable").DataTable().ajax.reload();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
};


ManagePermission = function () {
    $viewLoaded = true;
    RoleManageChange();
};
var BKAccount = {
    Id: 0,
    Name: ""
};
function checkBankmateAccountSetup(el) {
    if ($(el).prop('checked')) {
        $.get("/Bankmate/CheckBankmateAccountSetup", function (response) {
            if (!response) {
                BKAccount.Id = 0;
                $('#mbmaccount').empty();
                $('#mbmaccount').modal('show');
                $('#mbmaccount').load("/Bankmate/LoadModalAccountSetup?elId=" + $(el).attr('id'), function () {
                    $('#frmBankmateAccountSetup').validate({
                        ignore: [],
                        // any other options and/or rules
                    });
                    $("#frmBankmateAccountSetup").submit(function (e) {
                        e.preventDefault();
                        if ($("#frmBankmateAccountSetup").valid()) {
                            $.LoadingOverlay("show");
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
                                success: function (response) {
                                    $.LoadingOverlay("hide");
                                    if (response.actionVal === 1) {
                                        $("#mbmaccount").modal("hide");
                                        cleanBookNotification.createSuccess();
                                    } else if (response.actionVal === 2) {
                                        $("#mbmaccount").modal("hide");
                                        cleanBookNotification.updateSuccess();
                                    } else if (response.actionVal === 3) {
                                        cleanBookNotification.error(response.msg, "Qbicles");
                                    }
                                },
                                error: function (er) {
                                    $.LoadingOverlay("hide");
                                    cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
                                }
                            });
                        }
                    });
                });
            }
        });
    }
}
function initSelectedAccount() { }
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    $("#accountId").val(id);
    BKAccount.Id = id;
    BKAccount.Name = name;
    $("#accountId").val(id);
    $("#accountId").valid();
    closeSelected();
    $("#app-bookkeeping-treeview").modal("hide");
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
};

function UpdateDomain() {
    if ($('#form-domain-update').valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("domain-logo").files;

        if (files && files.length > 0) {
            UploadMediaS3ClientSide("domain-logo").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd('hide');
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    $("#domain-logo-current").val(mediaS3Object.objectKey);
                    SubmitDomain();
                }
            });
        } else {
            SubmitDomain();
        }
    }
}

function SubmitDomain() {

    $.ajax({
        type: 'POST',
        url: '/Domain/UpdateDateDomainSetting',
        dataType: 'JSON',
        async: false,
        data: { domainKey: $("#current-domain-key").val(), name: $("#domain-name").val(), image: $("#domain-logo-current").val() },
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.updateSuccess();
                $('.tab-settings-button').attr('disabled');
                $("#domain-name-current").val($("#domamin-name").val());
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function CancelChangeDomain() {
    $("#domain-logo-preview").attr('src', apiDocRetrievalUrl + $("#domain-logo-current").val());
    $("#domain-name").val($("#domain-name-current").val());
    document.getElementById("domain-logo").value = null;

    $('.tab-settings-button').attr('disabled', 'disabled');
}

$("#domain-logo").change(function () {
    var target = $(this).data('target');
    readImgURL(this, target);
    $(target).fadeIn();

    $('.tab-settings-button').removeAttr('disabled');
});
function readImgURL(input, target) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

//Extension Request
function updateExtensionRequestStatus(requestId, domainKey, status, type, note) {
    var _url = "/DomainExtension/ChangeExtensionRequestStatus";
    LoadingOverlay();
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            requestId: requestId,
            domainKey: domainKey,
            status: status,
            type: type,
            note: note
        },
        success: function (response) {
            if (response.result) {
                var $colcontainer = $("#col-type-" + type);
                if (status == 2) {
                    //Pending
                    $colcontainer.find('.last-updated').remove();
                    $colcontainer.prepend('<span class="last-updated" style="background: #f0ad4e;">Pending</span>');
                    $colcontainer.find('button').removeClass().addClass('btn btn-warning community-button').text('Pending').attr("disabled", "disabled");
                } else if (status == 3) {
                    //Approved
                    $colcontainer.find('.last-updated').remove();
                    $colcontainer.prepend('<span class="last-updated">Subscribed</span>');
                    $colcontainer.find('button').removeClass().addClass('btn btn-danger community-button').text('Cancel');
                } else if (status == 1 || status == 4) {
                    //Unsubscribe or Rejected
                    $colcontainer.find('.last-updated').remove();
                    $colcontainer.find('button').removeClass().addClass('btn btn-success community-button').text('Request').attr('onClick', 'updateExtensionRequestStatus(' + response.Object + ', \'' + domainKey + '\', 2, ' + type + ', "")');
                }

                cleanBookNotification.updateSuccess();
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

initExtensionRequestHistoryDtTable = function () {
    var dataTable = $("#extension-request-list")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#extension-request-list').LoadingOverlay("show");
            } else {
                $('#extension-request-list').LoadingOverlay("hide", true);
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
                "url": '/DomainExtension/LoadExtensionRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#extension-keysearch").val(),
                        "createdUserIdSearch": $("#extension-usersearch").val(),
                        "dateRange": $("#extension-datesearch").val(),
                        "extensionTypeSearch": 0,
                        "lstRequestStatusSearch": [3, 4],
                        "domainKey": $("#current-domain-key").val()
                    });
                }
            },
            "columns": [
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "RequestedByName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var creatorName = row.RequestedByName;
                        var creatorLogoUri = row.RequestedByLogoUri;
                        var creatorId = row.RequestById;
                        var htmlStr = "";
                        htmlStr += '<a href="/Community/UserProfilePage?uId=' + creatorId + '">';
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + creatorLogoUri + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + creatorName + '</div>'
                        htmlStr += '<div class="clearfix"></div></a>';
                        return htmlStr;
                    }
                },
                {
                    data: "TypeName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<label class="label label-lg label-info">Highlights</label>';
                        htmlStr += '&nbsp; ' + row.TypeName + '</td>';
                        return htmlStr;
                    }
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return row.StatusLabel;
                    }
                },
                {
                    data: "Note",
                    orderable: true
                }
            ]
        });
}
//END Extension Request

//Domain Request
function upgradeDomainToPremium(dmKey) {
    var _url = "/Domain/UpgradeBusinessDomain";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            domainKey: dmKey
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#upgrade-domain-container button").addClass("disabled").text("Upgrade pending").attr("onclick", "");
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

//END Domain Request


// Subscriptions Activities
function updateDomainTotalSlotNumber() {
    var newAmountSlot = Number($("#slot-required-number").val());
    var levelSlotNumber = $("#current-level-default-slot-number").val();
    var _url = "/Domain/UpdateDomainSlotNumber";
    var _data = {
        newTotalSlotNumber: newAmountSlot
    };

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        data: _data,
        url: _url,
        beforeSend: function () {
            LoadingOverlay();
        },
        success: function (response) {
            if (response.result) {
                // Update the UI
                var currentDomainUserNumber = Number($('#current-domain-user-number').val());
                $("#current-slot-number").text(currentDomainUserNumber + "/" + newAmountSlot + " used");
                var extraUserNumber = newAmountSlot - levelSlotNumber;
                var costPerExtra = $("#current-level-cost-per-extra-user").val();
                var extraCost = Number(costPerExtra) * extraUserNumber;
                var currentPlanCost = $("#current-level-cost").val();
                var totalCost = Number(extraCost) + Number(currentPlanCost);
                var currencySymbol = $("#current-level-currency-symbol").val();

                // Subscription plan table
                $("#extra-user-number-row").text(extraUserNumber);
                $("#extra-user-money-row").text(toCurrencyDecimalPlace(extraCost) + currencySymbol);
                $("#total-cost-row").text(toCurrencyDecimalPlace(totalCost) + currencySymbol);
                $("#extra-slot-money-row").text(toCurrencyDecimalPlace(extraCost) + currencySymbol);

                // Disable the update slots button
                $("#newslots").attr("disabled", "true");

                cleanBookNotification.success('Total number slots of the domain updated!', 'Qbicles');
            } else {
                cleanBookNotification.error(response.msg, 'Qbicles');
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, 'Qbicles');
            LoadingOverlayEnd();
        }
    })
}

function validateOnChangingDomainPlanLevel(newLevelId) {
    var _url = "/Administration/ValidateOnChangingDomainPlanLevel";
    var _data = {
        "newDomainLevelId": newLevelId
    }
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: _data,
        success: function (response) {
            if (response.result) {
                if (response.Object) {
                    // Show confirmation popup
                    $("#domchange-entry").load("/Administration/ShowChangeDomainPlanLevelConfirmationPopup?newDomainLevelId=" + newLevelId);
                    $("#domchange-entry").modal("show");
                    LoadingOverlayEnd();
                } else {
                    // Show error popup
                    $("#downgrade-slots").load("/Administration/ShowChangeDomainPlanLevelErrorPopup?newDomainLevelId=" + newLevelId);
                    $("#downgrade-slots").modal("show");
                    LoadingOverlayEnd();
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error("Validate on changing domain plan level fails", "Qbicles");
            LoadingOverlayEnd();
        }
    })
}

function updateDomainPlanLevel(newLevelId) {
    var _url = "/Administration/ChangeDomainPlanLevel";
    var _data = {
        "newDomainLevelId": newLevelId
    }

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: _data,
        beforeSend: function () {
            LoadingOverlay();
        },
        success: function (response) {
            if (response.result) {
                if (response.Object.RedirectionNeeded) {
                    window.location = response.Object.RedirectionUrl
                } else {
                    $("#domchange-entry").modal("hide");
                    $("#packagesuccess").modal("show");
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        LoadingOverlayEnd();
    })
}

function updateSubAccInfo() {
    var businessName = $("#subacc-business-name").val();
    var bankCode = $("#subacc-bank-code").val();
    var accountNumber = $("#subacc-account-number").val();

    var _data = {
        "businessName": businessName,
        "bankCode": bankCode,
        "accountNumber": accountNumber
    };
    var _url = "/Domain/UpdatePaystackSubAccountInformation";

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: _data,
        beforeSend: function () {
            LoadingOverlay();
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("SubAccount information updated", "Qbicles");
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).then(function () {
        LoadingOverlayEnd();
    })
}


// END: Subscriptions Activities
