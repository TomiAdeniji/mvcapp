function editDomain(key, name, logoUrl) {


    $('#domain-logo').attr('src', logoUrl);
    $('#domainId-UI').val("");
    $('#domainName-UI').val("");
    $('#logoDomain-UI').val("");
    ClearError();

    $('#title-add-update-domain').text("Edit Domain");
    $('#title-btn-addEdit-domain').text("Save Domain");
    $('#create-domain').modal('show');
    $('#domainId-UI').val(key);
    $('#domainName-UI').val(name);
};

function addDomain() {

    $('#domainId-UI').val("");
    $('#domainName-UI').val("");
    $('#logoDomain-UI').val("");
    ClearError();
    $('#title-add-update-domain').text("Add a Domain");
    $('#title-btn-addEdit-domain').text("Add Domain");
    $('#domain-logo').attr('src', "");
    $('#create-domain').modal('show');
};

function openOrCloseDomainById(key, id) {
    $.ajax({
        type: 'post',
        url: '/Domain/OpenOrCloseDomainById',
        datatype: 'json',
        data: { domainKey: key },
        success: function (refModel) {

            if (refModel.result) {
                $('#status-' + id).text(refModel.msg);
                $('#openOrClose-' + id).text(refModel.Object.Label);
            }
            else {
                cleanBookNotification.error(refModel.msg, "Qbicles");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};


//  end of Applications tab
function ListUserBySysTemAdmin() {
    $("#tbdatatable")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#tbdatatable').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#tbdatatable').LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": true,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100, 200], [10, 20, 50, 100, 200]],
            "pageLength": 10,
            "order": [[1, "desc"]],
            "ajax": {
                "url": '/Qbicles/LoadAllUserBySystemAdmin',
                "type": 'POST',
            },
            "columns": [
                {
                    data: "ProfilePic",
                    orderable: false,
                    render: function (value, type, row) {
                        if (row.ProfilePic) {
                            return ' <div class="table-avatar mini" style="background-image: url(\'' + apiDocRetrievalUrl + row.ProfilePic + '&size=T\');">&nbsp;</div>';
                        }
                        else
                            return ' <div class="table-avatar mini" style="background-image: url(\'../Content/DesignStyle/img/icon_contact.png\');">&nbsp;</div>';
                    }
                },
                {
                    data: "UserName",
                    orderable: true,
                    render: function (value, type, row) {
                        return row.UserName;
                    }
                },
                { "data": "Email", "class": "Email", "orderable": true },
                {
                    data: "Roles",
                    orderable: false,
                    render: function (value, type, row) {
                        var roleSelect = RenderSystemRolesColumn(row.Id, row.SystemRoles);
                        return roleSelect;
                    }
                },
                {
                    data: "Domain",
                    orderable: false,
                    render: function (value, type, row) {
                        return ' <ul>' + row.Domain + '</ul>';
                    }
                },
                {
                    data: "IsEnabled",
                    orderable: true,
                    render: function (value, type, row) {
                        if (row.IsEnabled)
                            return '<span class="label label-success label-lg">Active</span>';
                        else
                            return '<span class="label label-danger label-lg">Blocked</span>';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    width: "80px",
                    render: function (value, type, row) {
                        if (!row.IsEnabled) {
                            return '<button class="btn btn-primary" onclick="SuspendOrActive(\'' + row.Id + '\',1)"><i class="fa fa-lock-open" style="color: #fff;"></i> &nbsp; Unblock</button>';
                        }
                        else {
                            return '<button class="btn btn-danger" onclick="SuspendOrActive(\'' + row.Id + '\',0)"><i class="fa fa-user-lock" style="color: #fff;"></i> &nbsp; Block</button>';
                        }
                    }
                }
            ],
            drawCallback: function (settings) {
                $('#tbdatatable').DataTable().columns.adjust().responsive.recalc();

                $(".role-multi").multiselect({
                    includeSelectAllOption: true,
                    enableFiltering: true,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                
            }
        });
};

function RenderSystemRolesColumn(userId, roleIds) {    
    var roles = "<select id='user-" + userId + "' " +
        "onchange='UpdateSystemUserRoles(\"" + userId + "\")'"
        + "class='form-control role-multi' multiple>";

    _.forEach($domanRoles, function (role) {
        var selected = "";
        var match = _.find(roleIds, (r) => {
            return r.Id === role.Id;
        });
        if (match != null) {
            selected = "selected";
        }
        roles += "<option value='" + role.Name + "' " + selected + ">" + role.Name + "</option>"

    });
    roles += "</select>";
    return roles;
};


function UpdateSystemUserRoles(userId) {
      
    $.ajax({
        type: 'POST',
        url: '/Administration/UpdateSystemUserRoles',
        dataType: 'JSON',
        async: false,
        data: { userId: userId, roles: $("#user-" + userId).val() },
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.updateSuccess();
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");                
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        
    });
}

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
}

function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 500);
    };
}

function triggerSessionId(el) {
    $("#sessionId").val($(el).text());
    changeFilterLog();
}

function InitLogTable() {
    var dataTable = $("#tblLogs")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#tblLogs').LoadingOverlay("show");
            } else {
                $('#tblLogs').LoadingOverlay("hide", true);
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
            "order": [[0, "desc"]],
            "ajax": {
                "url": '/Administration/LoadLogs',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "searchAll": $("#searchAll").val(),
                        "dateRangeLog": $("#dateRangeLog").val(),
                        "sessionId": $("#sessionId").val(),
                        "domainKey": $("#domainKey").val(),
                        "appType": $("#appType").val(),
                        "action": $("#action").val()
                    });
                }
            },
            "columns": [
                {
                    data: "StrCreatedDate",
                    orderable: true
                },
                {
                    data: "SessionID",
                    orderable: true,
                    render: function (value, type, row) {
                        return '<a href="javascript:void(0)" onclick="triggerSessionId(this)" title="Click to filter by this ID">' + row.SessionID + '</a>';
                    }
                },
                {
                    data: "Domain",
                    orderable: true
                },
                {
                    data: "Qbicle",
                    orderable: true
                },
                {
                    data: "User",
                    orderable: true
                },
                {
                    data: "IPAddress",
                    orderable: true
                },
                {
                    data: "App",
                    orderable: true
                },
                {
                    data: "Action",
                    orderable: true
                },
            ]
        });
    $('#filterColumn input[type="checkbox"]').on('change', function (e) {
        var col = dataTable.column($(this).data('column'));
        col.visible(!col.visible());
    });
}

function LoadLogFilter() {
    $('#tab-logs').load("/Administration/LoadLogFilter", function () {
        $("select.select2").select2({
            placeholder: 'Please select'
        });
        $('#log_filter .daterange').daterangepicker({
            autoUpdateInput: false,
            cancelClass: "btn-danger",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: 'DD-MM-YYYY'
            }
        });
        $('#log_filter .daterange').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
            changeFilterLog();
        });

        $('#log_filter .daterange').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            changeFilterLog();
        });

        InitLogTable();

        $('#searchAll, #sessionId').keyup(searchThrottle(function () {
            changeFilterLog();
        }));
        $("#domainKey, #appType, #action").change(function () {
            changeFilterLog();
        });
    })
};

function downloadFile(urlData) {
    window.location.href = urlData;
}

function ExportToCSV() {
    LoadingOverlay();
    var obj = new Object();
    obj.searchAll = $("#searchAll").val();
    obj.dateRangeLog = $("#dateRangeLog").val();
    obj.sessionId = $("#sessionId").val();
    obj.domainKey = $("#domainKey").val();
    obj.appType = $("#appType").val();
    obj.act = $("#action").val();
    downloadFile('/Administration/ExportToCSV?' + $.param(obj));
    LoadingOverlayEnd()
}

function changeFilterLog() {
    $("#tblLogs").DataTable().ajax.reload();
}

$(function () {
    LoadLogFilter();

    ListUserBySysTemAdmin();

    LoadApplicationAccess();
});


function LoadApplicationAccess() {

    $("#tbl-appaccess-domains")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            $('.batch').hide();
            $('.domain-all-chk').prop('checked', false);
            if (processing) {
                $('#tbl-appaccess-domains').LoadingOverlay("show");
            } else {
                $('#tbl-appaccess-domains').LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100, 200], [10, 20, 50, 100, 200]],
            "pageLength": 10,
            "order": [[2, "desc"]],
            "ajax": {
                "url": '/Domain/GetListDomainApplicationAccess',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "name": $("#appaccess-domain-name").val(),
                        "status": $("#appaccess-domain-status").val()
                    });
                }
            },
            "columns": [
                {
                    data: "Id",
                    orderable: false,
                    render: function (value, type, row) {
                        return '<input value=\'' + row.Key + '\' type="checkbox" onchange="CheckDomain()">';
                    }
                },
                {
                    data: "LogoUri",
                    width: "80px",
                    orderable: false,
                    render: function (value, type, row) {
                        if (row.LogoUri) {
                            return ' <div class="table-avatar" style="background-image: url(\'' + apiDocRetrievalUrl + row.LogoUri + '&size=T\');">&nbsp;</div>';
                        }
                        else
                            return '<div class="table-avatar" style="background-image: url(\'/Content/DesignStyle/img/icon_domain_default.png\');">&nbsp;</div>';
                    }
                },
                {
                    data: "Name",
                    orderable: true,
                    render: function (value, type, row) {
                        return row.Name;
                    }
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (value, type, row) {
                        var rowClass = "success";
                        if (row.Status != "Active")
                            rowClass = "danger";
                        return '<span class="label label-lg label-' + rowClass + '">' + row.Status + '</span>';

                    }
                },
                {
                    data: "AvailableApps",
                    orderable: true,
                    render: function (value, type, row) {
                        return row.AvailableApps;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    width: "80px",
                    render: function (value, type, row) {
                        return '<input hidden id="domain-name-' + row.Id + '" value="' + row.Name + '"/><button class="btn btn-warning" onclick="ShowManageDomainApps(\'' + row.Key + '\', ' + row.Id + ')"><i class="fa fa-pencil"></i> &nbsp; Manage</button>';
                    }
                }
            ]
        });




    $('#appaccess-domain-name').keyup(searchThrottle(function () {
        LoadListApplicationAccess();
    }));
    $("#appaccess-domain-status").change(function () {
        LoadListApplicationAccess();
    });




}

function LoadListApplicationAccess() {
    $("#tbl-appaccess-domains").DataTable().ajax.reload();
}


//function CheckAllDomain(elm) {
//    var isChecked = $(elm).prop("checked");
//    var rows = $('#tbl-appaccess-domains').DataTable().rows({ 'search': 'applied' }).nodes();
//    $('input[type="checkbox"]', rows).prop('checked', isChecked);
//    CheckDomain();
//}
function CheckDomain() {
    var countChecked = $('#tbl-appaccess-domains').DataTable().$('input[type="checkbox"]:checked').length;
    if (countChecked > 0)
        $('.batch').show();
    else
        $('.batch').hide();
}

function GetDomainsSelected() {
    var $domainsSelectedId = [];
    $('#tbl-appaccess-domains').DataTable().rows().every(function (index, element) {
        var row = $(this.node());
        var checked = $(row).find("input[type=checkbox]");
        if ($(checked).prop('checked'))
            $domainsSelectedId.push($(checked).val());
    });
    return $domainsSelectedId;
}

function GetAppsSelected() {
    return $("#app-add-remove-selected").val();
}

function RevokeAllApps() {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: "/Administration/RevokeAllApps?all=" + $isAdllDomains,
        dataType: 'json',
        data: {
            domainIds: GetDomainsSelected()
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#tbl-appaccess-domains").DataTable().ajax.reload(null, false);
                $("#revoke-all-apps-modal").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

var $isAdllDomains = false;

function ConfirmRevokeApp(isAll) {
    $isAdllDomains = isAll;
    if (isAll)
        $("#confirm-revoke-title").text('Are you sure you want to remove all app access rights from all Domains?');
    else
        $("#confirm-revoke-title").text('Are you sure you want to remove all app access rights from the chosen Domains?');
}

function ShowAddRemoveApps(isAdd, isAll) {
    var ajaxUri = '/Administration/SysadminAppAddRemoveModalShow?add=' + isAdd + '&all=' + isAll;
    AjaxElementShowModal(ajaxUri, 'sysadmin-app-add-remove-modal');
}

function SaveAddRemoveAppsDomains() {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: "/Administration/SaveAddRemoveAppsDomains?add=" + $("#app-add-or-edit").val() + "&all=" + $("#app-to-all-domain").val(),
        dataType: 'json',
        data: {
            domainKeys: GetDomainsSelected(),
            appIds: GetAppsSelected()
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#tbl-appaccess-domains").DataTable().ajax.reload(null, false);
                $("#sysadmin-app-add-remove-modal").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function OnSelectApp() {
    var apps = $("#app-add-remove-selected").val();
    if (apps == null)
        $("#button-confirm-apps").attr("disabled", "disabled");
    else
        $("#button-confirm-apps").removeAttr("disabled");
}




function ShowManageDomainApps(key, id) {
    $.LoadingOverlay("show");
    var ajaxUri = '/Administration/SysadminAppAssignModalShow?domainkey=' + key;
    $('#sysadmin-app-assign').empty();
    $('#sysadmin-app-assign').load(ajaxUri, function () {

        $('.app-domain-access').bootstrapToggle();
        $("#domain-name").text($("#domain-name-" + id).val());

        $('#sysadmin-app-assign').modal('show');
        LoadingOverlayEnd();
    });
}

function SaveManageDomainApps(domainKey) {
    //TODO Save ManageDomainApps

    var addAppIds = [];
    $('input:checkbox.app-domain-access').each(function () {

        var appAccess = $(this).prop('checked');
        if (appAccess == true)
            addAppIds.push($(this).val());
    });

    $.ajax({
        type: 'post',
        url: "/Administration/SaveAppAssignModal",
        dataType: 'json',
        data: {
            addAppIds: addAppIds,
            domainKey: domainKey
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#tbl-appaccess-domains").DataTable().ajax.reload(null, false);
                $("#sysadmin-app-assign").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

//Loyalty - System Settings
function initLoyaltySystemSetting() {
    var $sysSettingFrm = $("#loyalty-syssetting-form");

    $sysSettingFrm.validate({
        rules: {
            amount: {
                required: true
            },
            point: {
                required: true
            }
        }
    });

    $sysSettingFrm.submit(function (e) {
        e.preventDefault();

        if ($sysSettingFrm.valid()) {
            var settingAmount = $("#setting-amount").val();
            var settingPoint = $("#setting-point").val();

            if (settingAmount < 0) {
                $sysSettingFrm.validate().showErrors({
                    amount: "Setting Amount must be greater than or equal to 0."
                });
            } else if (settingPoint < 0) {
                $sysSettingFrm.validate().showErrors({
                    point: "Setting Amount must be greater than or equal to 0."
                });
            } else {
                saveLoyaltySystemSetting();
            }
        }
    });
}

function saveLoyaltySystemSetting() {
    var settingAmount = $("#setting-amount").val();
    var settingPoint = $("#setting-point").val();
    var setting = {
        Points: settingPoint,
        Amount: settingAmount
    }

    $("#tab-loyalty").LoadingOverlay('show');
    var _url = "/LoyaltyPointConversion/AddLoyaltySystemSetting";
    $.ajax({
        method: 'POST',
        url: _url,
        data: {
            sysSettings: setting
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $('#confirm').attr('disabled', 'disabled');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
    $("#tab-loyalty").LoadingOverlay('hide');
}
//END: Loyalty - System Settings