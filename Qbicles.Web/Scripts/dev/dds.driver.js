var BKAccount = {
    Id: 0,
    Name: ""
};
$(document).ready(function () {
    $('#txtmembersearch').keyup(delay(function () {
        searchDeliveryDrivers();
    }, 400));
});
function SearchDriver() {
    if ($('#driver-search').val().length > 3 || $('#driver-search').val().length === 0) {
        var ajaxUri = '/DDS/DdsDriverSearch?name=' + $('#driver-search').val() + "&statusId=" + $("#driver-status").val() + "&locationId=" + $("#driver-location").val() + "&currentLocationId=" + $("#current-location-id").val();
        AjaxElementLoad(ajaxUri, "driver-list");
    }
};
function searchDeliveryDrivers() {
    var keyword = $('#txtmembersearch').val();
    $('.contact-list-found').LoadingOverlay("show");
    $('.contact-list-found').load("/TraderChannels/LoadModalDeliveryDriver", { keyword: keyword }, function () {
        $('.existing-member').show();
        $('.contact-list-found').LoadingOverlay("hide");
    });
}
function loadContentMemberDetail(posid) {
    var $content_contactadd = $('.contact-add');
    $content_contactadd.empty();
    $content_contactadd.LoadingOverlay("show");
    $content_contactadd.load("/TraderChannels/LoadContentMemberDetailByUser?posUId=" + posid, function () {
        $content_contactadd.LoadingOverlay("hide");
        $('.contact-list-found').hide();
        $('.contact-invite').hide();
        $('.contact-add').hide();
        $('.contact-add').fadeIn();
    });
}
function saveMember() {
    $.LoadingOverlay("show");
    $.post("/TraderChannels/AddDriver",
        {
            posUId: $('#hdfPosUid').val(),
            accountId: $('#hdfAccountId').val(),
            locationId: $("#slLocationIdForDriver").val(),
            driverUserId: $("#hdfUserid").val(),
        }, function (Response) {
            if (Response.result) {
                $('#delivery-driver-add').modal('hide');
                SearchDriver();
                $('#lnkBackSearch').click();
                $('#txtmembersearch').val('');
                $('.contact-list-found').empty();
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Trader");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
            }
            LoadingOverlayEnd();
        });
}
function initSelectedAccount() { }
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    $("#hdfAccountId").val(id);
    BKAccount.Id = id;
    BKAccount.Name = name;
    $("#hdfAccountId").val(id);
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

var $deviceIdDelete = 0;
function ConfirmDeleteDriver(id) {
    $deviceIdDelete = id;

    $("#name-delete").text($("#driver-name-main-" + $deviceIdDelete).text());
    $("#confirm-delete").modal('show');
};

function CancelDelete() {
    $('#confirm-delete').modal('hide');
};

function DeleteDds() {
    $.LoadingOverlay("show");
    var url = "/DDS/DeleteDdsDriver";

    $.ajax({
        type: "delete",
        url: url,
        data: { id: $deviceIdDelete },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                $("#dds-driver-item-" + $deviceIdDelete).remove();
                cleanBookNotification.removeSuccess(); $('#confirm-delete').modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error delete driver, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function UpdateDriverLocation(driverId) {
    $("#dds-driver-item-" + driverId).LoadingOverlay("show");
    var locationId = $("#driver-location-" + driverId).val();
    var url = "/DDS/UpdateDriverLocation";
    var driver = {
        Id: driverId,
        EmploymentLocation: { Id: locationId }
    };
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { driver: driver },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $("#dds-driver-item-" + driverId).LoadingOverlay("hide", true);
    });
};

function UpdateDriverWorkLocation(driverId, locationId, isAdd) {

    $("#dds-driver-item-" + driverId).LoadingOverlay("show");
    var url = "/DDS/UpdateDriverWorkLocation?idAdd=" + isAdd;
    var driver = {
        Id: _.toNumber(driverId),
        WorkLocations: [{ Id: _.toNumber(locationId) }]
    };

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { driver: driver },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $("#dds-driver-item-" + driverId).LoadingOverlay("hide", true);
    });
}

function UpdateDriverShift(isCheck, id) {
    $("#dds-driver-item-" + id).LoadingOverlay("show");
    var url = "/DDS/UpdateDriverShift";
    var driver = {
        Id: id,
        AtWork: isCheck
    };

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { driver: driver },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.updateSuccess();

            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $("#dds-driver-item-" + id).LoadingOverlay("hide", true);
    });
}

function updateDeliveryDisplayRefresh(ev) {
    var deliveryDisplayRefreshInterval = $(ev).val();
    if (!isNaN(parseFloat(deliveryDisplayRefreshInterval)) && parseFloat(deliveryDisplayRefreshInterval) < 0) {
        deliveryDisplayRefreshInterval = 0;
        $(ev).val(0);
    }
    var setting = {
        Id: $('#setting_id').val(),
        DeliveryDisplayRefreshInterval: deliveryDisplayRefreshInterval
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveDeliveryDisplayRefreshSetting";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { setting: setting },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
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

function SaveDeliveryLingerTime(ev) {
    var lingerTime = $(ev).val();
    if (!isNaN(parseFloat(lingerTime)) && parseFloat(lingerTime) < 0) {
        lingerTime = 0;
        $(ev).val(0);
    }
    var setting = {
        Id: $('#setting_id').val(),
        LingerTime: lingerTime
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveDeliveryLingerTime";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { setting: setting },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
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

function SaveThresholdTimeInterval(ev) {
    var thresholdTimeInterval = $(ev).val();
    if (!isNaN(parseFloat(thresholdTimeInterval)) && parseFloat(thresholdTimeInterval) < 0) {
        thresholdTimeInterval = 0;
        $(ev).val(0);
    }
    var setting = {
        Id: $('#setting_id').val(),
        APICallThresholdTimeInterval: thresholdTimeInterval
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveDeliveryThresholdTimeInterval";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { setting: setting },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
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

function SaveSpeedDistance(ev) {
    var speedDistance = $("#spedd-distance").val();
    var setting = {
        Id: $('#setting_id').val(),
        SpeedDistance: speedDistance
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveSpeedDistance";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { setting: setting },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
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