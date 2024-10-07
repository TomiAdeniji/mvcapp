var $paymentTypeCash = "Cash";
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

$('#search-device').keyup(searchThrottle(function () {
    ApplySearch();
}));

//$("#search-device").on("keyup", function () {
//    if ($('#search-device').val().length > 3 || $('#search-device').val().length === 0)
//        ApplySearch();
//});
$("#order-device").on("change", function () {
    ApplySearch();
});

function ApplySearch() {
    var ajaxUri = '/PointOfSaleDevice/SearchDevice?name=' + $('#search-device').val() + '&order=' + $("#order-device").val();
    $('#pos-devices-list').LoadingOverlay("show");
    $('#pos-devices-list').empty();
    $('#pos-devices-list').load(ajaxUri, function () {
        $('#pos-devices-list').LoadingOverlay("hide");
    });
};
function checkExistsTabelPrefix(tablePrefix, deviceId) {
    var dfd = new $.Deferred();
    $.ajax({
        type: 'get',
        url: '/PointOfSaleDevice/CheckExistsTabletPrefix?tabletPrefix=' + tablePrefix + '&deviceId=' + deviceId,
        async: false,
        contentType: false, // Not to set any content header  
        processData: false, // Not to process data
        success: function (response) {
            dfd.resolve(response);
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            dfd.resolve(true);
        }
    });
    return dfd.promise();
}
CreateDevice = function () {
    if (!$("#add-device-form").valid()) return;
    $.LoadingOverlay("show");
    var device = {
        Name: $("#device-name").val(),
        PosDeviceType: { Id: $("#devicetype").val() },
        TabletPrefix: $("#device-prefix").val(),
        SerialNumber: $("#device-serial").val(),
        Summary: $("#device-summary").val(),
        PreparationQueue: { Id: $("#device-queue-id").val() },
        Location: {
            Domain: {
                Key: $("#device-domain-key").val()
            }
        }
    };
    var isExists = false;
    checkExistsTabelPrefix(device.TabletPrefix, 0).then(function (response) {
        if (response) {
            $("#add-device-form").validate().showErrors({ prefix: _L("ERROR_MSG_411") });
            LoadingOverlayEnd();
            isExists = response;
        }
    });
    if (isExists) {
        return;
    }
    var url = "/PointOfSaleDevice/CreateDevice";


    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#pos-devices-list').append(rs.msg);
                $('#app-trader-pos-device-add').modal('hide');
                $("#device-name").val("");
                $("#device-serial").val("");
                $("#device-summary").val("");
            } else {
                $("#add-device-form").validate().showErrors({ devicename: _L("ERROR_MSG_337") });
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

// Device manager


$('#search-user-list').keyup(searchThrottle(function () {
    $('#user-device-list').DataTable().search($(this).val()).draw();
}));
$('#search-name').keyup(searchThrottle(function () {
    SearchUserModal();
}));
//$('#subfilter-group').on('change', function () {
//    var group = $(this).val();
//    $('#user-device-list').DataTable().search(group, false, false, false).draw();
//});

var table = $('#user-device-list').DataTable({
    'columnDefs': [
        { 'orderable': false, 'targets': [0, 5, 6] },
        { 'visible': false, 'targets': [6] },
        { 'targets': 4, 'orderDataType': 'dom-select' }
    ],
    'order': [[1, 'asc']]
});

// Custom filtering function for user role
$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var selectedRole = $('#subfilter-group').val();
        var userRole = $(table.row(dataIndex).node()).find('select[id^="user-roles"]').val();
        var rowCheckbox = $(table.row(dataIndex).node()).find('input[type="checkbox"]').is(':checked');

        
        if (selectedRole === " " || userRole === selectedRole || rowCheckbox) {
            return true;
        }
         
        return false;
    }
);

// Event listener for role filter dropdown
$('#subfilter-group').on('change', function () {
    table.draw();
});

// Event listener for search input
$('#search-user-list').on('keyup', function () {
    table.search(this.value).draw();
});

// Custom sort function for select elements
$.fn.dataTable.ext.order['dom-select'] = function (settings, col) {
    return this.api().column(col, { order: 'index' }).nodes().map(function (td, i) {
        return $('select', td).val();
    });
};


function SearchUserModal() {
    var input, filter, ul, li, a, i, txtValue;
    input = document.getElementById("search-name");
    filter = input.value.toUpperCase();
    if (filter.length > 0 && filter.length < 3) return;
    ul = document.getElementById("pos-user-UL");
    li = ul.getElementsByTagName("article");
    for (i = 0; i < li.length; i++) {

        a = li[i].getElementsByTagName("h5");
        if (a.length === 0) {
            li[i].style.display = "none";
            continue;
        };
        txtValue = a[0].textContent || a[0].innerText;
        if (txtValue.toUpperCase().indexOf(filter) > -1) {
            {
                li[i].style.display = "";
                document.getElementsByClassName('group-' + a[0].className)[0].setAttribute("style", "display: block");
            }
        } else {
            li[i].style.display = "none";
        }
    }
};

DeactivateDevice = function (deviceId) {

    $.LoadingOverlay("show");
    var device = {
        Id: deviceId,
        Status: 1
    };

    var url = "/PointOfSaleDevice/ActiveDevice";


    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (rs.actionVal > 0) {
                cleanBookNotification.updateSuccess();
                $('.deactivate').toggle();
                $('.deactivate-label').toggle();
                $('.activate').toggle();
                $('.activate-label').toggle();
                $("#device-form :input").attr('disabled', false);
                $("#pos-menu").attr('disabled', false);
                $("#pos-queue").attr('disabled', false);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

ActivateDevice = function (deviceId) {
    $.LoadingOverlay("show");
    var device = {
        Id: deviceId,
        Status: 2
    };

    var url = "/PointOfSaleDevice/ActiveDevice";


    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (rs.actionVal > 0) {
                cleanBookNotification.updateSuccess();
                $('.deactivate').toggle();
                $('.deactivate-label').toggle();
                $('.activate').toggle();
                $('.activate-label').toggle();
                $("#device-form :input").attr('disabled', true);
                $("#pos-menu").attr('disabled', true);
                $("#pos-queue").attr('disabled', true);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });


};

UpdateDeviceMenu = function (deviceId) {
    $.LoadingOverlay("show");
    var device = {
        Id: deviceId,
        Menu: {
            Id: $("#pos-menu").val()
        }
    };

    var url = "/PointOfSaleDevice/UpdateDeviceMenu";


    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (rs.actionVal > 0) {
                cleanBookNotification.updateSuccess();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

UpdateDeviceQueue = function (deviceId) {
    $.LoadingOverlay("show");
    var device = {
        Id: deviceId,
        preparationQueue: {
            Id: $("#pos-queue").val()
        }
    };

    var url = "/PointOfSaleDevice/UpdateDeviceQueue";


    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { device: device },
        success: function (rs) {
            if (rs.actionVal > 0) {
                cleanBookNotification.updateSuccess();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};
var delayTimer;
function searchKeyWork() {
    clearTimeout(delayTimer);

}
UpdateDevice = function (deviceId) {
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        if ($("#device-name").val() === "") {
            $("#device-form").validate().showErrors({ devicename: "Device name is required." });
            return;
        }
        $.LoadingOverlay("show");
        var device = {
            Id: deviceId,
            Name: $("#device-name").val(),
            TabletPrefix: $("#device-prefix").val(),
            SerialNumber: $("#device-serial").val(),
            PosDeviceType: { Id: $("#devicetype").val() },
            Summary: $("#device-summary").val(),
            Location: {
                Domain: {
                    Key: $("#device-domain-key").val()
                }
            }
        };
        if (!device.TabletPrefix || (device.TabletPrefix && device.TabletPrefix.length === "")) {
            $("#device-form").validate().showErrors({ prefix: "The prefix is required." });
            LoadingOverlayEnd();
            return;
        }
        var isExists = false;
        checkExistsTabelPrefix(device.TabletPrefix, deviceId).then(function (response) {
            if (response) {
                cleanBookNotification.error("TabletPrefix is unique for a device at the current location", "Qbicles");
                LoadingOverlayEnd();
                isExists = response;
            }
        });
        if (isExists) {
            return;
        }
        var url = "/PointOfSaleDevice/UpdateDevice";

        $.ajax({
            url: url,
            type: "post",
            dataType: "json",
            data: { device: device },
            success: function (rs) {
                if (rs.actionVal === 1) {
                    cleanBookNotification.updateSuccess();
                    $("#device-name-label").text(device.Name);
                    $("#device-summary-label").text(device.Summary);
                } else {
                    $("#device-form").validate().showErrors({ devicename: "Device name already exists." });
                }
            },
            error: function (err) {
                cleanBookNotification.error(err, "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });

    }, 1000); // Will do the ajax stuff after 1500 ms, or 1 s
};

var $isUser = false;
var $isManager = false;
var $isAdmin = false;

RemoveUser = function (posUserId, deviceId, userId, group, name, avatar) {

    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        className: "my-modal",
        title: "Qbicles",
        message: _L("ERROR_MSG_339"),
        callback: function (result) {
            if (!result) {
                $('body').css('overflow-y', 'hidden');
                return;
            }
            $('body').css('overflow-y', 'auto');
            $.LoadingOverlay("show");
            var $trDelete = $("#user-tr-" + posUserId);

            var model = {
                Id: 0,
                DeviceId: deviceId,
                PosUserId: posUserId,
                UserId: userId,
                IsDelete: true,
                Group: group,
                Name: name,
                Avatar: avatar
            };
            $.ajax({
                url: "/PosUser/DeletePosUserDevice",
                type: "post",
                dataType: "json",
                data: { model: model },
                success: function (rs) {
                    if (rs.result) {
                        cleanBookNotification.removeSuccess();

                        ReinitUserModal(model, rs.msg);


                        $($trDelete).css("background-color", "#FF3700");
                        $($trDelete).fadeOut(1500,
                            function () {
                                var t = $('#user-device-list').DataTable();
                                t.row($trDelete).remove();
                                t.draw();
                            });

                    } else {
                        cleanBookNotification.error(rs.msg, "Qbicles");
                    }
                },
                error: function (err) {

                    cleanBookNotification.error(err, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        }
    });
};

UpdateUser = function (deviceId, posUserId, userId, group, name, avatar,) {
    var action = $('.user-' + posUserId).is(':checked');
    var isDelete = false;
    if (action === true) {
        isDelete = false;
    } else {
        isDelete = true;
    }
    var isDeleteAll = false;
    action = $('.user-admin-' + posUserId).is(':checked');
    if (action === true) {
        isDeleteAll = false;
    } else {
        isDeleteAll = true;
    }

    var model = {
        Id: $('.user-' + posUserId).attr('objectId'),
        DeviceId: deviceId,
        PosUserId: posUserId,
        UserId: userId,
        IsDelete: isDelete,
        Group: group,
        Name: name,
        Avatar: avatar
    };

    if (!isDeleteAll)
        UpdateUserConfirm(model, isDeleteAll);
    else {
        bootbox.confirm({
            show: true,
            backdrop: true,
            closeButton: true,
            animate: true,
            className: "my-modal",
            title: "Qbicles",
            message: _L("ERROR_MSG_339"),
            callback: function (result) {
                if (!result) {
                    $('.user-' + posUserId).prop('checked', true);
                    $('body').css('overflow-y', 'hidden');
                    return;
                }

                $('body').css('overflow-y', 'auto');
                //$('.user-manager-' + posUserId).toggle();
                UpdateUserConfirm(model, isDeleteAll);

            }
        });
    }
};

UpdateUserConfirm = function (model, isDeleteAll) {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/PosUser/UpdatePosUser?isDeleteAll=" + isDeleteAll,
        type: "post",
        dataType: "json",
        data: { model: model },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                if (!isDeleteAll) {
                    $('.user-' + model.PosUserId).attr('objectId', rs.msgId);
                    $('.user-cashier-' + model.PosUserId).prop('checked', false);
                    $('.user-cashier-' + model.PosUserId).toggle();
                    $('.user-supervisor-' + model.PosUserId).prop('checked', false);
                    $('.user-supervisor-' + model.PosUserId).toggle();
                    $('.user-manager-' + model.PosUserId).prop('checked', false);
                    $('.user-manager-' + model.PosUserId).toggle();

                } else {
                    var $trDelete = $("#user-tr-" + model.PosUserId);
                    $($trDelete).css("background-color", "#FF3700");
                    $($trDelete).fadeOut(1500,
                        function () {
                            var t = $('#user-device-list').DataTable();
                            t.row($trDelete).remove();
                            t.draw();
                        });
                    ReinitUserModal(model, rs.msg);
                }

            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {

            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};
 

UpdateUserAdmin = function (deviceId, posUserId, userId, group, name, avatar) {
    var action = $('.user-admin-' + posUserId).is(':checked');
    var isDelete = false;
    if (action === true) {
        isDelete = false;
    } else {
        isDelete = true;
    }
    var model = {
        Id: $('.user-admin-' + posUserId).attr('objectId'),
        DeviceId: deviceId,
        PosUserId: posUserId,
        UserId: userId,
        IsDelete: isDelete,
        Group: group,
        Name: name,
        Avatar: avatar
    };
    UpdateUserManagerAdmin(model, "/PosUser/UpdatePosUserAdminDevice", '.user-admin-', posUserId);
};

UpdateUserManagerAdmin = function (model, api, select, posUserId) {
    $.LoadingOverlay("show");
    var url = api;
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { model: model },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                $(select + posUserId).attr('objectId', rs.msgId);

                if (select === '.user-admin-' && selectedRole =="none") {
                    var $trDelete = $("#user-tr-" + posUserId);
                    $($trDelete).css("background-color", "#FF3700");
                    $($trDelete).fadeOut(1500,
                        function () {
                            var t = $('#user-device-list').DataTable();
                            t.row($trDelete).remove();
                            t.draw();
                        });
                    ReinitUserModal(model, rs.msg);
                }
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {

            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

var selectedRole;
var previousRole = {};

function StorePreviousRole(selectElement, modelId, posUserId, userId, forenameGroup, userName, avatar,pin) {
    var key = posUserId; 
    previousRole[key] = selectElement.value;
    selectedRole = selectElement.value;
    if (selectedRole == "none") {
        $('#user-roles-' + posUserId).prop('disabled', true);
    }
    else {
        $('#user-roles-' + posUserId).prop('disabled', false);
    }
}
function UpdateUsersRole(selectElement, deviceId, posUserId, userId, forenameGroup, userName, avatar,pin) {

   
    // Get the selected role
    selectedRole = $(selectElement).val();     
    var model = {
        Id: $('#user-roles-' + posUserId).attr('objectId'),
        DeviceId: deviceId,
        PosUserId: posUserId,
        UserId: userId,
        CurrentRole: selectedRole,
        PreviousRole: previousRole[posUserId],
        Group: forenameGroup,
        Name: userName,
        Pin: pin,
        Avatar: avatar
    };
    
    if (selectedRole == "none") {
        bootbox.confirm({
            show: true,
            backdrop: true,
            closeButton: true,
            animate: true,
            className: "my-modal",
            title: "Qbicles",
            message: _L("ERROR_MSG_339"),   
            callback: function (result) {
                if (!result) {
                    var key = posUserId;
                    selectElement.value = previousRole[key];
                    return;
                }
                else {
                    $('body').css('overflow-y', 'auto');
                   
                    UpdateUserRole(model.DeviceId, model.PosUserId, model.UserId, model.CurrentRole, model.PreviousRole, model.Group, model.Name, model.Avatar,model.Pin);
                    $('#user-roles-' + posUserId).prop('disabled', true);
                }
            }
        });
    }
    else {
        UpdateUserRole(model.DeviceId, model.PosUserId, model.UserId, model.CurrentRole, model.PreviousRole, model.Group, model.Name, model.Avatar, model.Pin);
        $('#user-roles-' + posUserId).prop('disabled', false);
    }
}
UpdateUserRole = function (deviceId, posUserId, userId, currentRole, previousRole, group, name, avatar,pin) {
    $.LoadingOverlay("show");
    var url = "/PosUser/UpdatePosUser?isDeleteAll=false";
    var model = {
        Id: $('.user-' + posUserId).attr('objectId'),
        DeviceId: deviceId,
        PosUserId: posUserId,
        UserId: userId,
        CurrentRole: currentRole,
        PreviousRole: previousRole,
        Group: group,
        Name: name,
        Avatar: avatar,
        Pin:pin
    };

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: {
            model: model
        },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                var adminIsChecked = $('.user-admin-' + posUserId).is(':checked');
                if (!adminIsChecked && currentRole == "none") {
                    var $trDelete = $("#user-tr-" + posUserId);
                    $($trDelete).css("background-color", "#FF3700");
                    $($trDelete).fadeOut(1500,
                        function () {
                            var t = $('#user-device-list').DataTable();
                            t.row($trDelete).remove();
                            t.draw();
                        });
                    ReinitUserModal(model, rs.msg);
                }
                if (currentRole == "none") {
                    $('#user-roles-' + posUserId).prop('disabled', true);
                }

            } else {
                if (previousRole[posUserId] != "none") {
                    $('#user-roles-' + posUserId).prop('disabled', false);
                }
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            if (previousRole[posUserId] != "none") {
                $('#user-roles-' + posUserId).prop('disabled', false);
            }
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        if (previousRole[posUserId] != "none") {
            $('#user-roles-' + posUserId).prop('disabled', false);
        }
        LoadingOverlayEnd();
    });
}
function getAllowedValues(selectId) {
    var allowedValues = [];
    $("#" + selectId + " option").each(function () {
        if ($(this).text() === 'None' ||
            $(this).text() === 'User' ||
            $(this).text() === 'Cashier' ||
            $(this).text() === 'Supervisor' ||
            $(this).text() === 'Manager' ||
            $(this).text() === 'Admin') {
            allowedValues.push($(this).val());
        }
    });
    return allowedValues;
}
$('.add-user-pool').on('select2:select', function (e) {
    var data = e.params.data;
    var userId = data.element.className.split('#')[0];
    var validNumbers = ["2", "3", "4", "5"]
    if (data.id === '1') {
        $("#select-" + userId).val(null).trigger("change");
        $("#select-" + userId).val(1).trigger("change");
    } else {
        var arr = $("#select-" + userId).val();
        arr = arr.filter(function (item) {
            return item !== "1";
        });
      
        if (data.id !== "6" || !arr.includes("6")) {
            validNumbers = validNumbers.filter(function (item) {
                return item !== data.id;
            })
            arr = arr.filter(function (ele) {
                return !validNumbers.includes(ele);
            })
        }
        $("#select-" + userId).val(null).trigger("change");
        $("#select-" + userId).val(arr).trigger("change");
    }
});

$('.add-user-pool').on("select2:unselect", function (e) {

    var data = e.params.data;
    var userId = data.element.className.split('#')[0];

    var arr = $("#select-" + userId).val();
    if (arr === null) {
        $("#select-" + userId).val(null).trigger("change");
        $("#select-" + userId).val(1).trigger("change");
    }
});

ConfirmAddUsers = function (deviceId) {

    var pooledUsers = [];

    var lists = $("#pos-user-UL article");


    var donInclude = ["1"];

    if (lists.length > 0) {
        for (var i = 0; i < lists.length; i++) {
            var userId = $(lists[i]).find("div.contact-info input").val();

            var pooled = $($(lists[i]).find("div.contact-info select")).val();

            if (typeof userId !== "undefined" && typeof pooled !== "undefined" && !_.isEqual(pooled, donInclude)) {
                var item = {
                    UserId: userId,
                    Pools: pooled
                };
                pooledUsers.push(item);
            }
        }
    }
    $.LoadingOverlay("show");

    $.ajax({
        url: "/PosUser/CreatePosUserDevice?deviceId=" + deviceId,
        type: "post",
        dataType: "json",
        data: { pooledUsers: pooledUsers },
        success: function (rs) {
            if (rs.actionVal > 0) {
                $('#app-trader-pos-user-add').modal('hide');
                cleanBookNotification.updateSuccess();
                setTimeout(function () {
                    window.location.reload();
                }, 100);

            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {

            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

ReinitUserModal = function (model, content) {
    var theDiv = document.getElementById("group-" + model.Group);
    if (theDiv === null) {
        var groupHtml = "<article id='group-'" +
            model.Group +
            " class='letters group-'" +
            model.Group +
            "  style='padding-top: 0;'>" +
            model.Group +
            " </article>";
        var rootDiv = document.getElementById("pos-user-UL");
        rootDiv.insertAdjacentHTML('beforeend', groupHtml);
        rootDiv.insertAdjacentHTML('beforeend', content);
    } else {
        theDiv.insertAdjacentHTML('beforeend', content);
    }




    $("#select-" + model.UserId).select2();

    $("#select-" + model.UserId).on('select2:select', function (e) {
        var data = e.params.data;
        var userId = data.element.className.split('#')[0];
        if (data.id === "None") {
            $("#select-" + userId).val(null).trigger("change");
            $("#select-" + userId).val("None").trigger("change");
        } else {
            var arr = $("#select-" + userId).val();
            arr = arr.filter(function (item) {
                return item !== "None";
            });

            $("#select-" + userId).val(null).trigger("change");
            $("#select-" + userId).val(arr).trigger("change");
        }
    });

    $("#select-" + model.UserId).on("select2:unselect", function (e) {

        var data = e.params.data;
        var userId = data.element.className.split('#')[0];

        var arr = $("#select-" + userId).val();
        if (arr === null) {
            $("#select-" + userId).val(null).trigger("change");
            $("#select-" + userId).val(1).trigger("change");
        }
    });
}
// Payment account

function ConfirmAddPaymentMethod() {
    if (!ValidationDisplayName("table_displayname_temp"))
        return;

    if ($("#select2-table_paymentmethod_temp-container").text().toLowerCase() == $paymentTypeCash.toLowerCase()) {
        var safeAssociatedCashAndBankAccountId = $("#account-of-safe").val();

        if (safeAssociatedCashAndBankAccountId === '-1') {
            cleanBookNotification.error("No safe found. Create a safe for this location first.", "Qbicles");
            LoadMethodAccountTable();
            return;
        }
    }

    var posDevice = {
        Id: $('#posDevice_id').val(),
        MethodAccount: [{
            Id: 0,
            TabletDisplayName: $('#table_displayname_temp').val(),
            PaymentMethod: { Id: $('#table_paymentmethod_temp').val() },
            CollectionAccount: { Id: $('#table_account_temp').val() }
        }],
        IsAdd: true
    }
    SavePaymentAccount(posDevice);
}
function PaymentAccountRowChanged(id) {
    if (!ValidationDisplayName('table_paymentmethod_' + id))
        return;

    if ($('#table_paymentmethod_' + id + " option:selected").text().toLowerCase() == $paymentTypeCash.toLowerCase()) {
        var safeAssociatedCashAndBankAccountId = $("#account-of-safe").val();

        if (safeAssociatedCashAndBankAccountId === '-1') {
            cleanBookNotification.error("No safe found. Create a safe for this location first.", "Qbicles");
            LoadMethodAccountTable();
            return;
        }
    }

    CheckPaymentTypeSelected(id);
    var posDevice = {
        Id: $('#posDevice_id').val(),
        MethodAccount: [{
            Id: id,
            TabletDisplayName: $('#table_displayname_' + id).val(),
            PaymentMethod: { Id: $('#table_paymentmethod_' + id).val() },
            CollectionAccount: { Id: $('#table_account_' + id).val() }
        }],
        IsAdd: false
    }
    SavePaymentAccount(posDevice);
}

ValidationDisplayName = function (eId) {
    if ($('#' + eId).val() !== "")
        return true;
    cleanBookNotification.error(_L("ERROR_VALUE_REQUIRED", ["Display name"]), "Qbicles");
    return false;
}
function DeletePaymentAccount(id) {
    $.LoadingOverlay("show");
    var posDevice = {
        Id: $('#posDevice_id').val(),
        MethodAccount: [{
            Id: id
        }]
    }
    $.ajax({
        url: "/PointOfSale/DeletePaymentMethod",
        type: "delete",
        dataType: "json",
        data: { posDevice: posDevice },
        success: function (rs) {
            if (rs.actionVal === 2) {
                LoadMethodAccountTable();
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            console.log(err);
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function SavePaymentAccount(posDevice) {
    if (posDevice.MethodAccount[0].PaymentMethod.Id === null || posDevice.MethodAccount[0].PaymentMethod.Id === "0") {
        cleanBookNotification.error(_L("ERROR_MSG_356"), "Qbicles");
        return false;
    }
    if (posDevice.MethodAccount[0].CollectionAccount.Id === null || posDevice.MethodAccount[0].CollectionAccount.Id === "0") {
        cleanBookNotification.error(_L("ERROR_MSG_357"), "Qbicles");
        return false;
    }

    $.LoadingOverlay("show");
    $.ajax({
        url: "/PointOfSale/CreatePaymentMethod",
        type: "post",
        dataType: "json",
        data: { posDevice: posDevice },
        success: function (rs) {

            if (rs.actionVal > 2) {
                cleanBookNotification.error(rs.msg, "Qbicles");
                if (!posDevice.IsAdd)
                    LoadMethodAccountTable();
                return;
            }

            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('.add-row').hide();
                $('#addpayment').show();
                $("#table_methodAccount tr.row_template").remove();

            } else if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            } LoadMethodAccountTable();
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");

        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function LoadMethodAccountTable() {
    var id = $('#posDevice_id').val();
    $.LoadingOverlay("show");
    $("#table_methodAccount").empty();
    $("#table_methodAccount").load("/PointOfSale/GetPaymentAccountTable?idDevice=" + id, function () {
        LoadingOverlayEnd();
    });
}
function addEditPosDevice(id) {
    LoadingOverlay();
    $("#app-trader-pos-device-add").empty();
    $("#app-trader-pos-device-add").load("/PointOfSale/AddEditPosDevice?id=" + id, function () {
        LoadingOverlayEnd();
    });
}


var $deviceIdDelete = 0;
function ConfirmDeleteDevice(id, name) {
    $("#name-delete").text(name.replace('|', '\''));
    $deviceIdDelete = id;
}

function DeleteDevice() {
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/PointOfSaleDevice/DeleteDevice",
        data: { id: $deviceIdDelete },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                $("#pos-device-" + $deviceIdDelete).remove();
                $('#confirm-delete').modal('hide');
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_349", [er.error]), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function CheckPaymentTypeSelected(id) {
    var collectionAccountId = $("#collectionAccountId").val();
    var collectionAccountText = $("#collectionAccountText").val();
    if (id === -1) {
        var paymentType = $("#select2-table_paymentmethod_temp-container").text().toLowerCase();
        if (paymentType == $paymentTypeCash.toLowerCase()) {
            $("#table_displayname_temp").val($paymentTypeCash).addClass("disabled");
            var safeAssociatedCashAndBankAccountId = $("#account-of-safe").val();

            if (safeAssociatedCashAndBankAccountId === '-1') {
                cleanBookNotification.error("No safe found. Create a safe for this location first.", "Qbicles");
                return;
            } else {
                //add collection account
                $("#table_account_temp").append($("<option selected></option>")
                    .attr("value", collectionAccountId)
                    .text(collectionAccountText));

                $("#table_account_temp").prop("disabled", "disabled");
                $("#table_account_temp").val(collectionAccountId).trigger("change");
                
            }
        } else {
            //remove collection account
            $("#table_account_temp option[value=" + collectionAccountId + "]").remove();
            $("#table_displayname_temp").val("").removeClass("disabled");
            $("#table_account_temp").prop("disabled", false);
            $("#table_account_temp").val(collectionAccountId).trigger("change");
            
        }
    } else {
        var paymentType = $("#table_paymentmethod_" + id + " option:selected").text().toLowerCase();
        if (paymentType == $paymentTypeCash.toLowerCase()) {
            $("#table_displayname_" + id).val($paymentTypeCash).addClass("disabled");
            var safeAssociatedCashAndBankAccountId = $("#account-of-safe").val();

            if (safeAssociatedCashAndBankAccountId === '-1') {
                cleanBookNotification.error("No safe found. Create a safe for this location first.", "Qbicles");
                return;
            } else {
                $("#table_account_" + id).append($("<option selected></option>")
                    .attr("value", collectionAccountId)
                    .text(collectionAccountText));
                $("#table_account_" + id).select2("destroy").select2();

                $("#table_account_" + id).prop("disabled", "disabled");
            }
            return;
        } else {
            $("#table_account_" + id).removeClass("disabled");
            $("#table_account_" + id).prop("disabled", false);
            $("#table_displayname_" + id).removeClass("disabled").removeAttr("readonly");

            //remove collection account
            $("#table_account_" + id + " option[value=" + collectionAccountId + "]").remove();
            $("#table_account_" + id).select2("destroy").select2();
            return;
        }
    }
}


function CancelAddPaymentMethod() {
    $('.add-row').hide(); $('#addpayment').show();
    $('.table_content tr.row_template').remove();
    $('.dataTables_empty').removeClass('hidden');
    //$('#table_methodAccount tbody tr#tr_id_' + idBuild).remove();
    //$('.table_content tr.row_template').remove();
    //$('#table_templage tbody tr td.row_select span').remove();
    $('#table_templage tbody tr.row_template span').remove();
}

function RemovePaymentMethodRowItem() {
    //$('#table_methodAccount tbody tr' + id).remove();
    //$('#table_templage tbody tr td.row_select span').remove();
    $('#table_templage tbody tr.row_template span').remove();
    $('.table_content tr.row_template').remove();
    $('.dataTables_empty').removeClass('hidden');
    $('.add-row').hide();
    $('#addpayment').show();
}
var idBuild = UniqueId();
function AddPaymentMethodRow(ev) {

    //$('#table_templage tbody tr td.row_select span').remove();
    $('#table_templage tbody tr.row_template span').remove();
    $(".dataTables_empty").addClass("hidden");
    $('.payrow2').show();
    $('.add-row').show();
    $(ev).hide();
    var clone = $('#table_templage tbody tr.row_template').clone();
    $("#table_methodAccount table tbody").append(clone[0]);
    $('#table_methodAccount tbody tr.row_template td select').select2({ placeholder: 'Please select' });
    CheckPaymentTypeSelected(-1);
}