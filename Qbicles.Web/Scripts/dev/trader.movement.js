function initAlertSettingPage() {
    FilterMovementDataTable();
    FilterMovementAlertSettings();
    $('#alert-settings-daterange').daterangepicker({
        autoUpdateInput: false,
        timePicker: true,
        timePicker24Hour: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });

    $("#alert-setting_status").select2();

    $("#key-search-alert-setting").keyup(delay(function () {
        $("#alert-settings_table").DataTable().ajax.reload();
    }, 1000));


    $("#alert-setting_status").change(function () {
        $("#alert-settings_table").DataTable().ajax.reload();
    });

}


function FilterMovementAlertSettings() {
    var _url = "/TraderMovement/GetListMovementAlertSettings";

    $("#alert-settings_table").on("processing.dt", function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#alert-settings_table").LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $("#alert-settings_table").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "language": {
            "infoFiltered": ""
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "ajax": {
            "url": _url,
            "type": "GET",
            "dataType": "json",
            "data": function (d) {
                return $.extend({}, d, {
                    key: $("#key-search-alert-setting").val(),
                    dateRange: $("#alert-settings-daterange").val(),
                    statusShown: $("#alert-setting_status").val()
                });
            }
        },
        "columns": [
            {
                data: "Reference",
                name: "Reference",
                orderable: true,
                visible: true
            },
            {
                data: "Date",
                name: "Date",
                orderable: true,
                visible: true
            },
            {
                data: "NoMvnAlert",
                name: "NoMvnAlert",
                orderable: true,
                visible: true,
                render: function (value, type, row) {
                    var str = '';
                    if (row.NoMvnAlert) {
                        str += "<td><span class='label label-lg label-success'>Enabled</span></td>";
                    } else {
                        str += '<td><span class="label label-lg label-danger">Disabled</span></td>';
                    }
                    return str;
                }
            },
            {
                data: "MinMaxAlert",
                name: "Min/max alerts",
                orderable: true,
                visible: true,
                render: function (value, type, row) {
                    var str = '';
                    if (row.MinMaxAlert) {
                        str += "<td><span class='label label-lg label-success'>Enabled</span></td>";
                    } else {
                        str += '<td><span class="label label-lg label-danger">Disabled</span></td>';
                    }
                    return str;
                }
            },
            {
                data: "AccumulationAlert",
                name: "Accumulation alerts",
                orderable: true,
                visible: true,
                render: function (value, type, row) {
                    var str = '';
                    if (row.AccumulationAlert) {
                        str += "<td><span class='label label-lg label-success'>Enabled</span></td>";
                    } else {
                        str += '<td><span class="label label-lg label-danger">Disabled</span></td>';
                    }
                    return str;
                }
            },
            {
                name: "Options",
                orderable: false,
                visible: true,
                render: function (value, type, row) {
                    var str = '';
                    str += "<button class='btn btn-warning' onclick=" + "window.location.href='/TraderMovement/TraderMovementSetting?alertGroupId=" + row.alertGroupId + "';" + "><i class='fa fa-pencil'></i></button>";

                    return str;
                }
            }
        ],
        "columnDefs": [
            {
                "targets": [0, 2],
                "visible": false
                //"searchable": false
            }],
        "order": [[1, "desc"]]
    });
}

function FilterMovementDataTable() {
    var ajaxUri = '/Trader/ListItemInventoryBatch?locationId=' + $('#local-manage-select').val() + '&callback=true' + '&datestring=' + ($('#movement_daterange').val() + "").trim().replace(/\s/g, "");
    $('#movement-table').LoadingOverlay('show');
    $('.table-movement').removeClass('manage-columns');
    $('#movement-table').empty();
    $('#movement-table').load(ajaxUri, function () {
        $('.table-movement input[type="checkbox"]').on('change', function () {
            var table = $('#movement_table').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('.table-movement').addClass('manage-columns');
        $('#movement-table').LoadingOverlay('hide');
        $("#movement_table").DataTable().draw();

    });
}

function SetProductGroups(alertGroupId) {

    $('#applyc').attr('disabled', 'disabled');

    if (alertGroupId == 0) return;
    var lstProductGroupIds = $("#product-groups").val();
    $('#ProductEmpty-error').remove();
    if (lstProductGroupIds == null) {
        $("#productgroups-selector")
            .append("<label id='ProductEmpty-error' class='error' for='ProductGroups'>No Product Groups selected.</label>");
    } else {
        LoadingOverlay();
        var _url = "/TraderMovement/SetAlertProductGroups";
        $.ajax({
            method: "POST",
            dataType: "JSON",
            url: _url,
            data: { alertGroupId: alertGroupId, lstProductGroupIds: lstProductGroupIds },
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    window.location = "/TraderMovement/TraderMovementSetting?alertGroupId=" + response.Object;
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }
}

function ConfirmProductGroups(alertGroupId) {
    var lstProductGroupIds = $("#product-groups").val();
    $('#PosDevices-error').remove();
    if (lstProductGroupIds == null) {
        $("#productgroups-selector")
            .append("<label id='PosDevices-error' class='error' for='PosDevices'>No Product Groups selected.</label>");
    } else {
        LoadingOverlay();
        var _url = "/TraderMovement/SetAlertProductGroups";
        $.ajax({
            method: "POST",
            dataType: "JSON",
            url: _url,
            data: { alertGroupId: alertGroupId, lstProductGroupIds: lstProductGroupIds },
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    setTimeout(function () {
                        window.location.href = '/TraderMovement/TraderMovementSetting?alertGroupId=' + response.msgId;
                    }, 3000);
                    //window.location = "/Trader/AppTrader";
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }
}

function GenerateNoMovementThresholds(alertGroupId) {
    //daterange value
    var benchmarking = $("#nomovement-daterange").val();
    $('#nomvnt-daterange-error').remove();
    if (benchmarking == "") {
        $("#nomovement-inputcontainer")
            .append("<label id='nomvnt-daterange-error' class='error' for='NoMovementAlert'>The daterange must be selected first.</label>");
        return;
    }

    $('.no-movement-alerts').LoadingOverlay("show");
    var _url = "/TraderMovement/SetNoMovementThresholds?alertGroupId=" + alertGroupId;

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { daterangeString: benchmarking },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                ReloadActiveProductGroup();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                ReloadActiveProductGroup();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
        }
    }).always(function () {
        $('.no-movement-alerts').LoadingOverlay("hide", true);
    });
}

function GenerateMinMaxThresholds(alertGroupId) {
    //daterange value
    var benchmarking = $("#minmax-daterange").val();
    $('#minmax-daterange-error').remove();
    if (benchmarking == "") {
        $("#minmax-inputcontainer")
            .append("<label id='minmax-daterange-error' class='error' for='MinMaxAlert'>The daterange must be selected first.</label>");
        return;
    }
    $('.inventory-min-max-alerts').LoadingOverlay("show");
    //inventory-min-max-alerts
    var _url = "/TraderMovement/SetMinMaxThresholds";
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            alertGroupId: alertGroupId,
            daterangeString: benchmarking
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();

            } else {
                cleanBookNotification.error(response.msg, "Qbicles");

            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");

        }
    }).always(function () {
        $('.inventory-min-max-alerts').LoadingOverlay("hide", true);
    })
    ReloadActiveProductGroup();
}

function GenerateAccumulationThresholds(alertGroupId) {
    //daterange value
    var benchmarking = $("#accumulation-daterange").val();
    $('#accumulation-daterange-error').remove();
    if (benchmarking == "") {
        $("#accumulation-inputcontainer")
            .append("<label id='accumulation-daterange-error' class='error' for='AccumulationAlert'>The daterange must be selected first.</label>");
        return;
    }

    var checkPeriod = $("#accumulation_check-event").val();
    var _url = "/TraderMovement/SetAccumulationThresholds";
    $('.inventory-accumulation-alerts').LoadingOverlay("show");
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { alertGroupId: alertGroupId, daterangeString: benchmarking, checkPeriod: checkPeriod },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(response.msg);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
        }
    }).always(function () {
        $('.inventory-accumulation-alerts').LoadingOverlay("hide", true);
    })
    ReloadActiveProductGroup();
}

function EnableNoMovementCheckJob(alertGroupId, alertConstraintId, ev) {
    $('.no-movement-alerts').LoadingOverlay("show");
    var _url = "/TraderMovement/EnableNoMovementJob?alertGroupId=" + alertGroupId;
    var noMovementCheckEvent = $("#no-movement_check-event").val();

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { checkPeriod: noMovementCheckEvent },
        success: function (response) {
            if (response.result) {
                $(ev).toggleClass('btn-success btn-danger');
                $(ev).html() == 'Disable' ? $(ev).html('Enable') : $(ev).html('Disable');
                $(ev).attr("onclick", "DisableScheduleJob(" + alertGroupId + ", " + response.Object + ", 'noMovement', this)");
                cleanBookNotification.success("Enable Thresholds checking job successfully.", "Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {

            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('.no-movement-alerts').LoadingOverlay("hide", true);
    })
}

function EnableMinMaxCheckJob(alertGroupId, alertConstraintId, ev) {
    $('.inventory-min-max-alerts').LoadingOverlay("show");
    var _url = "/TraderMovement/EnableMinMaxJob?alertGroupId=" + alertGroupId;
    var minMaxCheckEvent = $("#minmax_check-event").val();

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { checkPeriod: minMaxCheckEvent },
        success: function (response) {
            if (response.result) {
                $(ev).toggleClass('btn-success btn-danger');
                $(ev).html() == 'Disable' ? $(ev).html('Enable') : $(ev).html('Disable');
                $(ev).attr("onclick", "DisableScheduleJob(" + alertGroupId + ", " + response.Object + ", 'minMax', this)");
                cleanBookNotification.success("Enable Thresholds checking job successfully.", "Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('.inventory-min-max-alerts').LoadingOverlay("hide", true);
    })
}

function EnableAccumulationJob(alertGroupId, alertConstraintId, ev) {
    var _url = "/TraderMovement/EnableAccumulationJob?alertGroupId=" + alertGroupId;
    var accumulationCheckEvent = $("#accumulation_check-event").val();
    $('.inventory-accumulation-alerts').LoadingOverlay("show");
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { checkPeriod: accumulationCheckEvent },
        success: function (response) {
            if (response.result) {
                $(ev).toggleClass('btn-success btn-danger');
                $(ev).html() == 'Disable' ? $(ev).html('Enable') : $(ev).html('Disable');
                $(ev).attr("onclick", "DisableScheduleJob(" + alertGroupId + ", " + response.Object + ", 'accumulation', this)");
                cleanBookNotification.success("Enable Thresholds checking job successfully.", "Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('.inventory-accumulation-alerts').LoadingOverlay("hide", true);
    });
}

function DisableScheduleJob(alertGroupId, alertConstraintId, typeName, ev) {
    if (typeName == "noMovement")
        $('.no-movement-alerts').LoadingOverlay("show");
    if (typeName == "minMax")
        $('.inventory-min-max-alerts').LoadingOverlay("show");
    if (typeName == "accumulation")
        $('.inventory-accumulation-alerts').LoadingOverlay("show");
    var _url = "/TraderMovement/DisableScheduleJob?alertConstraintId=" + alertConstraintId;

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                $(ev).toggleClass('btn-success btn-danger');
                console.log($(ev).html());
                $(ev).html() == 'Disable' ? $(ev).html('Enable') : $(ev).html('Disable');
                if (typeName == "noMovement") {
                    $(ev).attr("onclick", "EnableNoMovementCheckJob(" + alertGroupId + ", " + alertConstraintId + ", this)");
                } else if (typeName == "minMax") {
                    $(ev).attr("onclick", "EnableMinMaxCheckJob(" + alertGroupId + ", " + alertConstraintId + ", this)");
                } else if (typeName == "accumulation") {
                    $(ev).attr("onclick", "EnableAccumulationJob(" + alertGroupId + ", " + alertConstraintId + ", this)");
                }
                cleanBookNotification.success("Disable Thresholds checking job successfully.", "Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        if (typeName == "noMovement")
            $('.no-movement-alerts').LoadingOverlay("hide", true);
        if (typeName == "minMax")
            $('.inventory-min-max-alerts').LoadingOverlay("hide", true);
        if (typeName == "accumulation")
            $('.inventory-accumulation-alerts').LoadingOverlay("hide", true);
    })
}

function ShowProductGroupItems(productGroupId) {
    var _url = "/TraderMovement/ProductGroupItemsShow?productGroupId=" + productGroupId;
    $("#productgroup-items").empty();
    $("#productgroup-items").load(_url);
}

function ReloadActiveProductGroup() {
    $("#product-group-list .active a").trigger("click");
}

function UpdateItemAlertGroup(xId) {

    $('#productgroup-items').LoadingOverlay("show");
    var _url = "/TraderMovement/UpdateItemAlertGroup";
    var item = {
        id: xId,
        NoMovementInDaysThreshold: $("#NoMovementInDaysThreshold-" + xId).val(),
        NoMovementOutDaysThreshold: $("#NoMovementOutDaysThreshold-" + xId).val(),
        MinInventoryThreshold: $("#MinInventoryThreshold-" + xId).val(),
        MaxInventoryThreshold: $("#MaxInventoryThreshold-" + xId).val(),
        AccumulationTreshold: $("#AccumulationTreshold-" + xId).val()
    };


    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: { item: item },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('#productgroup-items').LoadingOverlay("hide", true);
    });
}