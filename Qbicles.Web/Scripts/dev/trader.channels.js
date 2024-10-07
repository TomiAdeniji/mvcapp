var firstload = false;
var isBusy = false;
var $b2bModals = {
    $modal_pricelist: $('#delivery-charge-pricelist-add'),
    $modal_deliverychargeframework: $('#delivery-charge-framework-add'),
    $modal_deliveryvehicle: $('#delivery-vehicle-add'),
    $modal_driverclocationchange: $('#b2b-driver-location-change'),
    $modal_deliverydriveradd: $('#delivery-driver-add'),
    $modal_chargeframeworkclone: $('#b2b-charge-framework-clone'),
    $modal_chargeframeworkport: $('#b2b-charge-framework-port')
};
var $b2bcontent = {
    $content_pricelist: $('#b2b-charges-content'),
    $content_framework: $('#framework-1'),
    $content_contactadd: $('.contact-add')
};
var $b2btables = {
    $table_vehicle: $('#tblvehicles'),
    $table_drivers: $('#tbldrivers')
};
var dataVehicles = [];
var BKAccount = {
    Id: 0,
    Name: ""
};
$(document).ready(function () {
    $('div.channel .select2').select2({ placeholder: "Please select" });
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    initDataTable();
    $('#txtplkeyword').keyup(delay(function () {
        loadContentPriceList();
    }, 400));
    $('#txtvehiclekeyword').keyup(delay(function () {
        reloadTableVehicles();
    }, 400));
    $('#txtmembersearch').keyup(delay(function () {
        searchDeliveryDrivers();
    }, 400));
    $('#txtSearchDriver').keyup(delay(function () {
        reloadTableDrivers();
    }, 400));
    loadVehicles();
    initFormClonePriceList();
    $('#select-traderitems').select2({
        ajax: {
            url: '/TraderItem/Select2TraderItemsByLocationId',
            delay: 250,
            data: function (params) {
                var query = {
                    keyword: params.term,
                    page: params.page || 1
                }
                return query;
            },
            cache: true
        },
        minimumInputLength: 1
    });
});

function reloadDefaultTopic(currentTopicId) {
    var qbicleId = $('#wg-qbicle').val();
    $('#wg-topic').empty();
    $.getJSON('/Operator/GetTopicByQbicle', { qbicleId: (qbicleId ? qbicleId : 0), currentTopicId: currentTopicId }, function (result) {
        $('#wg-topic').select2({
            placeholder: "Select a state",
            data: result
        });
        if (firstload) {
            $('#wg-topic').trigger('change');
        }
        firstload = true;
    });
}


function loadModalPriceList(id) {
    $b2bModals.$modal_pricelist.empty();
    $b2bModals.$modal_pricelist.modal('show');
    $b2bModals.$modal_pricelist.load("/TraderChannels/LoadModalPriceList?id=" + id, function () {
        var $frmb2bpricelist = $("#frmb2bpricelist");
        $frmb2bpricelist.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                Icon: {
                    filesize: true
                },
                Summary: {
                    required: true,
                    minlength: 3,
                    maxlength: 500
                }
            }
        });
        $frmb2bpricelist.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmb2bpricelist.valid()) {
                $.LoadingOverlay("show");
                var files = document.getElementById("b2b-price-icon-input").files;
                if (files.length > 0) {
                    UploadMediaS3ClientSide("b2b-price-icon-input").then(function (mediaS3Object) {

                        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                            LoadingOverlayEnd('hide');
                            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                            return;
                        }
                        else {
                            $("#b2b-price-icon-object-key").val(mediaS3Object.objectKey);
                            $("#b2b-price-icon-object-name").val(mediaS3Object.fileName);
                            $("#b2b-price-icon-object-size").val(mediaS3Object.fileSize);

                            SaveB2bPriceList();
                        }
                    });
                }
                else {
                    $("#b2b-price-icon-object-key").val("");
                    $("#b2b-price-icon-object-name").val("");
                    $("#b2b-price-icon-object-size").val("");

                    SaveB2bPriceList();
                }
            }
        });
    });
}

function SaveB2bPriceList() {
    var frmData = new FormData($("#frmb2bpricelist")[0]);  
    $.ajax({        
        type: "post",
        cache: false,
        url: "/TraderChannels/SavePriceList",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $b2bModals.$modal_pricelist.modal('hide');
                loadContentPriceList();
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Trader");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
            }
            isBusy = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
            LoadingOverlayEnd();
        }
    });
}

function loadModalDeliveryChargeFramework(id, pricelistId) {
    $b2bModals.$modal_deliverychargeframework.empty();
    $b2bModals.$modal_deliverychargeframework.modal('show');
    $b2bModals.$modal_deliverychargeframework.load("/TraderChannels/LoadModalDeliveryChargeFramework?", { priceListId: pricelistId, id: id }, function () {
        var $frmChargeFramework = $("#frmChargeFramework");
        $frmChargeFramework.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                DistanceTravelledFlatFee: {
                    required: true
                },
                DistanceTravelPerKm: {
                    required: true
                },
                TimeTakenFlatFee: {
                    required: true
                },
                TimeTakenPerSecond: {
                    required: true
                },
                ValueOfDeliveryFlatFee: {
                    required: true
                },
                ValueOfDeliveryPercentTotal: {
                    required: true,
                    min: 0,
                    max: 100
                },
            }
        });
        $frmChargeFramework.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmChargeFramework.valid()) {
                $.LoadingOverlay("show");
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    data: $(this).serialize(),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $b2bModals.$modal_deliverychargeframework.modal('hide');
                            loadContentChargeFramework(pricelistId);
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
        initSelect2avatarDelivery();
    });
}
function initSelect2avatarDelivery() {
    $('.select2avatar-delivery').select2({
        placeholder: 'Please select',
        templateResult: formatOptions2,
        templateSelection: formatSelected2
    });
}
function searchDeliveryDrivers() {
    var keyword = $('#txtmembersearch').val();
    $('.contact-list-found').LoadingOverlay("show");
    $('.contact-list-found').load("/TraderChannels/LoadModalDeliveryDriver", { keyword: keyword }, function () {
        $('.existing-member').show();
        $('.contact-list-found').LoadingOverlay("hide");
    });
}
function loadModalDeliveryVehicle(id) {
    $b2bModals.$modal_deliveryvehicle.empty();
    $b2bModals.$modal_deliveryvehicle.modal('show');
    $b2bModals.$modal_deliveryvehicle.load("/TraderChannels/LoadModalDeliveryVehicle?id=" + id, function () {
        var $frmVehicle = $("#frmVehicle");
        $frmVehicle.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                RefOrRegistration: {
                    required: true,
                    maxlength: 50
                }
            }
        });
        $frmVehicle.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmVehicle.valid()) {
                $.LoadingOverlay("show");
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    data: $(this).serialize(),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $b2bModals.$modal_deliveryvehicle.modal('hide');
                            reloadTableVehicles();
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
        initSelect2avatarDelivery();
    });
}
function loadContentPriceList() {
    $b2bcontent.$content_pricelist.empty();
    $b2bcontent.$content_pricelist.LoadingOverlay("show");
    $b2bcontent.$content_pricelist.load("/TraderChannels/LoadContentPriceList", { keyword: $("#txtplkeyword").val() }, function () {
        $b2bcontent.$content_pricelist.LoadingOverlay("hide");
    });
}
function loadContentChargeFramework(priceId) {
    $('.pricelists').hide();
    $('#framework-1').fadeIn();
    $b2bcontent.$content_framework.empty();
    $b2bcontent.$content_framework.LoadingOverlay("show");
    $b2bcontent.$content_framework.load("/TraderChannels/LoadContentChargeFramework", { priceId: priceId }, function () {
        $b2bcontent.$content_framework.LoadingOverlay("hide");
    });
}
function formatOptions2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function formatSelected2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function deletePriceList(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeletePriceList", { id: id }, function (Response) {
                    if (Response.result) {
                        loadContentPriceList();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                });
                return;
            }
        }
    });
}
function deleteChargeFramework(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteChargeFramework", { id: id }, function (Response) {
                    if (Response.result) {
                        loadContentChargeFramework($('#hdfPriceListId').val());
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                });
                return;
            }
        }
    });
}
function initDataTable() {
    //verhicles DataTable
    $b2btables.$table_vehicle.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#logistics-vehicles').LoadingOverlay("show");
        } else {
            $('#logistics-vehicles').LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            deferLoading: 30,
            order: [[0, "asc"]],
            ajax: {
                "url": "/TraderChannels/SearchVehicles",
                "data": function (d) {
                    return $.extend({}, d, {
                        "keyword": $('#txtvehiclekeyword').val(),
                    });
                }
            },
            columns: [
                { "title": "Vehicle type", "data": "VehicleType", "searchable": true, "orderable": true },
                { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                { "title": "Reference or registration", "data": "RefOrRegistration", "searchable": true, "orderable": true },
                {
                    "title": "Options",
                    "data": "Id",
                    "searchable": true,
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<div class="btn-group"><button class="btn btn-primary" data-toggle="dropdown"><i class="fa fa-cog"></i></button>';
                        _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary">';
                        _htmlOptions += '<li><a href="#" onclick="loadModalDeliveryVehicle(' + data + ')">Edit</a></li>';
                        _htmlOptions += '<li><a href="#" onclick="deleteVehicle(' + data + ')">Delete</a></li>';
                        _htmlOptions += '</ul></div>';
                        return _htmlOptions;
                    }
                }
            ]
        });
    //drivers DataTable
    $b2btables.$table_drivers.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#logistics-drivers').LoadingOverlay("show");
        } else {
            $('#logistics-drivers').LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            deferLoading: 30,
            order: [[0, "asc"]],
            ajax: {
                "url": "/TraderChannels/SearchDrivers",
                "data": function (d) {
                    return $.extend({}, d, {
                        "keyword": $('#txtSearchDriver').val(),
                    });
                }
            },
            columns: [
                {
                    "title": "Driver",
                    "data": "Driver",
                    "searchable": true,
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlDriver = '<a href="#"><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.DriverIcon + '\');"></div>';
                        _htmlDriver += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + data + '</div>';
                        _htmlDriver += '<div class="clearfix"></div></a>';
                        return _htmlDriver;
                    }
                },
                {
                    "title": "Email",
                    "data": "Email",
                    "searchable": true,
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        return '<a href="mailto:' + data + '">' + data + '</a>';
                    }
                },
                {
                    "title": "Verhicle",
                    "data": "VehicleId",
                    "searchable": true,
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlVehicleSelect2 = '<select name="vehicle" onchange="updateVehicleDriver(' + row.Id + ',this)" class="form-control select2 dt-select2" style="width: 100%;">';
                        _htmlVehicleSelect2 += '<option></option>';
                        dataVehicles.forEach(function (item) {
                            _htmlVehicleSelect2 += '<optgroup label="' + fixQuoteCode(item.GroupName) + '">';
                            if (item.Items) {
                                item.Items.forEach(function (el) {
                                    _htmlVehicleSelect2 += '<option value="' + el.Id + '" ' + (el.Id == data ? 'selected' : '') + '>' + fixQuoteCode(el.Text) + '</option>';
                                });
                            }
                            _htmlVehicleSelect2 += '</optgroup>';
                        });
                        _htmlVehicleSelect2 += '</select>';
                        return _htmlVehicleSelect2;
                    }
                },
                {
                    "title": "Status",
                    "data": "Status",
                    "searchable": true,
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlStatus = '<label class="label label-lg label-success">Active</label>';
                        if (data != 1)
                            _htmlStatus = '<label class="label label-lg label-warning">Off duty</label>';
                        return _htmlStatus;
                    }
                },
                {
                    "title": "Options",
                    "data": "Id",
                    "searchable": true,
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<div class="btn-group"><button class="btn btn-primary" data-toggle="dropdown" aria-expanded="false"><i class="fa fa-cog"></i></button>';
                        _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary">';
                        if (row.Status == 1)
                            _htmlOptions += '<li><a href="#" onclick="setOnOffDuty(' + data + ',2)">Set off duty</a></li>';
                        else
                            _htmlOptions += '<li><a href="#" onclick="setOnOffDuty(' + data + ',1)">Set on duty</a></li>';
                        _htmlOptions += '<li><a href="#" onclick="loadModalLocationChange(' + data + ')">Change location</a></li>';
                        _htmlOptions += '<li><a href="#" onclick="deleteDriver(' + data + ')">Delete</a></li>';
                        _htmlOptions += '</ul></div>';
                        return _htmlOptions;
                    }
                }
            ],
            drawCallback: function () {
                $('.dt-select2').select2({ placeholder: "Please select", });
            }
        });
}
function reloadTableVehicles() {
    setTimeout(function () {
        $b2btables.$table_vehicle.DataTable().ajax.reload();
    }, 100);
    loadVehicles();
}
function reloadTableDrivers() {
    setTimeout(function () {
        $b2btables.$table_drivers.DataTable().ajax.reload();
    }, 100);

}
function loadVehicles() {
    $.get("/TraderChannels/GetVehiclesForSelect2", function (data) {
        dataVehicles = data;
    });
}
function deleteVehicle(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteVehicle", { id: id }, function (Response) {
                    if (Response.result) {
                        reloadTableVehicles();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                });
                return;
            }
        }
    });
}
function deleteDriver(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteDriver", { id: id }, function (Response) {
                    if (Response.result) {
                        reloadTableDrivers();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(_L(Response.msg), "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                });
                return;
            }
        }
    });
}
function setOnOffDuty(id, status) {
    $.post("/TraderChannels/UpdateStatusDriver", { id: id, status: status }, function (Response) {
        if (Response.result) {
            reloadTableDrivers();
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function loadModalLocationChange(id) {
    $b2bModals.$modal_driverclocationchange.empty();
    $b2bModals.$modal_driverclocationchange.modal('show');
    $b2bModals.$modal_driverclocationchange.load("/TraderChannels/LoadModalLocationChange?driverId=" + id, function () {
        var $frmVehicle = $("#frmLocationchange");
        $frmVehicle.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $b2bModals.$modal_driverclocationchange.modal('hide');
                        reloadTableDrivers();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    LoadingOverlayEnd();
                }
            });
        });
        $('#frmLocationchange .select2').select2();
    });
}
function updateVehicleDriver(id, elVehicle) {
    $.post("/TraderChannels/UpdateVehicleDriver", { id: id, vehicleId: $(elVehicle).val() }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function saveMember() {
    $.LoadingOverlay("show");
    $.post("/TraderChannels/AddDriver", {
        posUId: $('#hdfPosUid').val(),
        accountId: $('#hdfAccountId').val(),
        locationId: $("#slLocationIdForDriver").val(),
        driverUserId: $("#hdfUserid").val(),
    }, function (Response) {
        if (Response.result) {
            $b2bModals.$modal_deliverydriveradd.modal('hide');
            reloadTableDrivers();
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
function loadContentMemberDetail(posid) {
    $b2bcontent.$content_contactadd.empty();
    $b2bcontent.$content_contactadd.LoadingOverlay("show");
    $b2bcontent.$content_contactadd.load("/TraderChannels/LoadContentMemberDetail?posUId=" + posid, function () {
        $b2bcontent.$content_contactadd.LoadingOverlay("hide");
        $('.contact-list-found').hide();
        $('.contact-invite').hide();
        $('.contact-add').hide();
        $('.contact-add').fadeIn();
    });
}
function loadContentItemInfo() {
    var itemId = $('#select-traderitems').val();
    if (itemId) {
        var $content_iteminfo = $('#item-info');
        $content_iteminfo.empty();
        $content_iteminfo.LoadingOverlay("show");
        $content_iteminfo.load("/TraderChannels/LoadContentItemInfo?itemId=" + itemId, function () {
            $('#tbliteminfo').DataTable();
            $content_iteminfo.LoadingOverlay("hide");
        });
    }
}
function saveDeliverySettings(itemId) {
    $.LoadingOverlay("show");
    var paramaters = {
        Id: 0,
        DeliveryService: { Id: itemId },
    };
    $.post("/TraderChannels/SaveDeliverySettings", { settings: paramaters }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
        LoadingOverlayEnd();
    });
}
function initPriceCloneCurrentLocation(itemId, itemName) {
    $('#frmPriceListCLocationClone input[name=cloneId]').val(itemId);
    $('#frmPriceListCLocationClone input[name=cloneName]').val(itemName);
    $b2bModals.$modal_chargeframeworkclone.modal('show');
}
function initFormClonePriceList() {
    //Clone current pricelist
    var $frmPriceListCLocationClone = $("#frmPriceListCLocationClone");
    $frmPriceListCLocationClone.validate({
        rules: {
            cloneName: {
                required: true,
                minlength: 3,
                maxlength: 150
            }
        }
    });
    $frmPriceListCLocationClone.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmPriceListCLocationClone.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $b2bModals.$modal_chargeframeworkclone.modal('hide');
                        loadContentPriceList();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}
function initFormCloneOtherLocationPriceList() {
    //Clone current pricelist
    var $frmPriceListOtherLocationClone = $("#frmPriceListOtherLocationClone");
    $frmPriceListOtherLocationClone.validate({
        rules: {
            cloneName: {
                required: true,
                minlength: 3,
                maxlength: 150
            },
            locationId: {
                required: true
            }
        }
    });
    $frmPriceListOtherLocationClone.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;

        if ($frmPriceListOtherLocationClone.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $b2bModals.$modal_chargeframeworkport.modal('hide');
                        loadContentPriceList();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}

function loadModalLocationChange(priceId) {
    $b2bModals.$modal_chargeframeworkport.empty();
    $b2bModals.$modal_chargeframeworkport.modal('show');
    $b2bModals.$modal_chargeframeworkport.load("/TraderChannels/LoadModalChargeFrameworkPort?priceId=" + priceId, function () {
        initFormCloneOtherLocationPriceList();
        $('#frmPriceListOtherLocationClone .select2').select2();
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


BackToAllChannels = function () {
    $('.channels').show();
    $('.sale_channel_content').hide();
    $('.sale_channel_content_detail').empty();
};

ShowSalesChannelsContent = function (channel) {

    $('.sale_channel_content_detail').LoadingOverlay("show");
    setCookie('CurrentLocationManage', $('#local-manage-select').val());
    $('.channels').hide();
    $('.sale_channel_content_detail').empty();

    var ajaxUri = '/SalesChannels/SalesChannelB2BContent';

    switch (channel) {
        case "channel_b2b":
            ajaxUri = '/SalesChannels/SalesChannelB2BContent';
            break;
        case "channel_b2c":
            ajaxUri = '/SalesChannels/SalesChannelB2CContent';
            break;
        case "channel_pos":
            ajaxUri = '/PointOfSale/PointOfSaleContent?value=GeneralChannel';
            break;
    }

    

    $('.sale_channel_content_detail').load(ajaxUri, function () {
        $('.sale_channel_content_detail').LoadingOverlay("hide");
        $('.sale_channel_content').show();
    });
};



function saveB2BConfig() {
    var _paramaters = {
        Services: $('#slB2BServices').val(),
        SourceQbicleId: $('#wg-qbicle').val(),
        DefaultTopicId: $('#wg-topic').val(),
        SettingOrder: $("#b2b-order").val(),
        LocationId: $('#local-manage-select').val()
    };
    $.post("/TraderChannels/SaveB2BConfig", _paramaters, function (response) {
        if (response.result) {
            $('#wg-topic').data('topic', _paramaters.DefaultTopicId);
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            //Tab Logistics
            if (_paramaters.Services && _paramaters.Services.indexOf("Logistics") >= 0)
                $('#tabB2bConfig li.b2b-logistics').show();
            else
                $('#tabB2bConfig li.b2b-logistics').hide();
            //Tab Maintenance
            if (_paramaters.Services && _paramaters.Services.indexOf("Maintenance") >= 0)
                $('#tabB2bConfig li.b2b-maintenance').show();
            else
                $('#tabB2bConfig li.b2b-maintenance').hide();
        } else {
            cleanBookNotification.error(response.msg, "Trader");
        }
    });
}


function saveB2CConfig() {
    var _paramaters = {
        SettingOrder: $("#b2c-order").val(),
        LocationId: $('#local-manage-select').val(),
        DefaultSaleWorkGroupId: $('#wgsaledefault').val(),
        DefaultInvoiceWorkGroupId: $('#wginvoicedefault').val(),
        DefaultPaymentAccountId: $('#b2ccashbankaccount').val(),
        DefaultTransferWorkGroupId: $('#wgtransferdetault').val(),
        DefaultPaymentWorkGroupId: $('#wgpaymentdefault').val()
    };
    $.post("/TraderChannels/SaveB2CConfig", _paramaters, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            $('#wg-topic').data('topic', _paramaters.DefaultTopicId);
        } else {
            cleanBookNotification.error(response.msg, "Trader");
        }
    });
}
function LocationChange() {
    var locationId = $('#local-manage-select').val();
    $.getJSON('/SalesChannels/LoadB2C_WG_AC_ST_Default', { locationId: locationId },
        function (result) {
            //$('#b2b_order').empty();
            $('#b2c_order').empty();
            $('#wgsaledefault').empty();
            $('#wginvoicedefault').empty();
            $('#wgpaymentdefault').empty();
            $('#wgtransferdetault').empty();
            $('#b2ccashbankaccount').empty();
            //result.B2bOrderStatus.unshift({ id: "", text: "" });
            //$('#b2b_order').select2({
            //    placeholder: "Select a state",
            //    data: result.B2bOrderStatus
            //});
            result.B2cOrderStatus.unshift({ id: "", text: "" });
            $('#b2c_order').select2({
                placeholder: "Select a state",
                data: result.B2cOrderStatus
            });
            result.SaleWorkgroups.unshift({ id: "", text: "" });
            $('#wgsaledefault').select2({
                placeholder: "Choice a workgroup",
                data: result.SaleWorkgroups
            });
            result.InvoiceWorkgroups.unshift({ id: "", text: "" });
            $('#wginvoicedefault').select2({
                placeholder: "Choice a workgroup",
                data: result.InvoiceWorkgroups
            });
            result.PaymentWorkgroups.unshift({ id: "", text: "" });
            $('#wgpaymentdefault').select2({
                placeholder: "Choice a workgroup",
                data: result.PaymentWorkgroups
            });
            result.PaymentWorkgroups.unshift({ id: "", text: "" });
            $('#wgtransferdetault').select2({
                placeholder: "Choice a workgroup",
                data: result.TransferWorkgroups
            });
            result.PaymentAccounts.unshift({ id: "", text: "" });
            $('#b2ccashbankaccount').select2({
                placeholder: "Choice an account",
                data: result.PaymentAccounts
            });
        });
};
