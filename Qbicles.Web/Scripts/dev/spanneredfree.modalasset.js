var currentItemData = null;
var _tblAssociatedItems;
var _tableAssetInventory;
var _table;
function loadModalAsset(id) {
    $('#app-spannered-asset-add').empty();
    $('#app-spannered-asset-add').modal("show");
    $("#app-spannered-asset-add").load("/Spanneredfree/LoadModalAsset", { id: id, lid: $slLocation.val() }, function () {
        initModalAsset();
    });
}
function initModalAsset() {
    var $frmSpanneredAsset = $('#frmSpanneredAsset');
    $('select.select2').select2({ placeholder: 'Please select' });
    // Cycle app nav tabs with button triggers
    $('#frmSpanneredAsset .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('#tabNavAsset .active').next('li').find('a').trigger('click');
    });

    $('#frmSpanneredAsset .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('#tabNavAsset .active').prev('li').find('a').trigger('click');
    });
    //Tab Meters
    _table = $('#tblMeters').DataTable({
        "destroy": true,
        "columnDefs": [
            {
                "targets": 0,
                "visible": false
            },
            {
                "render": function (data, type, row) {
                    var _htmlOptions = '<button type="button" class="btn btn-warning btn-edit-meter"><i class="fa fa-pencil"></i></button> <button type="button" class="btn btn-danger btn-delete-meter"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                },
                "targets": 4
            }
        ],
        rowId: function (a) {
            return 'meter-id-' + UniqueId();
        },
    });
    $('#tblMeters tbody').on('click', 'button.btn-edit-meter', function () {
        var $row = $(this).parents('tr');
        var data = _table.row($row).data();
        //Bind data from current row into form Meter
        $('#asset-4 input[name=rid]').val($row.attr('id'));
        $('#asset-4 input[name=mid]').val(data[0]);
        $('#asset-4 input[name=mname]').val(data[1]);
        $('#asset-4 input[name=munit]').val(data[2]);
        $('#asset-4 input[name=mdesc]').val(data[3]);
        //Update UI Meter button to status edit
        $('#btnUpdateMeter').show();
        $('#btnAddMeter').hide();
        //end
    });
    $('#tblMeters tbody').on('click', 'button.btn-delete-meter', function () {
        _table.row($(this).parents('tr')).remove().draw();
    });
    $('#btnAddMeter').on('click', function () {
        var meterName = $('#asset-4 input[name=mname]').val();
        var meterUnit = $('#asset-4 input[name=munit]').val();
        var meterDesc = $('#asset-4 input[name=mdesc]').val();
        if (meterName && meterUnit && meterDesc) {
            _table.row.add([
                0,
                meterName,
                meterUnit,
                meterDesc,
                UniqueId(),
            ]).draw(false);
            //reset form Meter
            $('#btnAddMeter').attr("disabled", true);
            $('#asset-4 input[name=mname]').val('');
            $('#asset-4 input[name=munit]').val('');
            $('#asset-4 input[name=mdesc]').val('');
            //end reset
        }
    });
    //End Meters
    //Tab Associated Trader item
    _tblAssociatedItems = $('#tblTraderItem').DataTable({
        destroy: true,
        serverSide: true,
        processing: true,
        paging: true,
        searching: false,
        deferLoading: 30,
        ajax: {
            "url": "/Spanneredfree/GetListAssociatedTraderItem",
            "data": function (d) {
                return $.extend({}, d, {
                    "locationId": $slLocation.val(),
                    "keyword": $('#asset-2 input[name=search]').val(),
                    "groupId": $('#asset-2 select[name=group]').val(),
                    "itemLink": $('#hdfTraderItemIdLink').val(),
                    "spwgid": ($('#asset-1 select[name=WorkgroupId]').val() ? $('#asset-1 select[name=WorkgroupId]').val() : 0)
                });
            }
        },
        columns: [
            { "title": "Id", "data": "Id", "searchable": false },
            { "title": "Item", "data": "Item", "searchable": true },
            { "title": "Barcode", "data": "Barcode", "searchable": true },
            { "title": "SKU", "data": "SKU", "searchable": true },
            { "title": "Group", "data": "Group", "searchable": true },
            null
        ],
        columnDefs: [
            {
                "targets": 0,
                "visible": false
            },
            {
                "targets": 1,
                "data": "Item",
                "render": function (data, type, row, meta) {
                    return '<a href="#">' + data + '</a>';
                }
            }, {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (row.Islink)
                        return '<button class="btn btn-danger" type="button" onclick="linkTraderItemWithAsset(false,0)"><i class="fa fa-unlink"></i> &nbsp; Unlink</button>';
                    else
                        return '<button class="btn btn-success" type="button" onclick="linkTraderItemWithAsset(true,' + data + ')" style="display: inline-block;"><i class="fa fa-link"></i> &nbsp; Link</button>';
                }
            }]
        //drawCallback: function (data) {
        //    $('.chktoggle').bootstrapToggle();
        //}
    });
    $('#asset-2 input[name=search]').keyup(searchThrottle(function () {
        $('#hdfTraderItemIdLink').val(0);
        _tblAssociatedItems.ajax.reload();
    }));
    $('#asset-2 select[name=group]').change(function () {
        $('#hdfTraderItemIdLink').val(0);
        _tblAssociatedItems.ajax.reload();
    });
    //End Associated Trader item
    var $select_traderitem = $('#select-traderitems').select2({
        ajax: {
            url: '/Spanneredfree/GetListTraderItem',
            delay: 250,
            data: function (params) {
                var query = {
                    search: params.term,
                    locationId: $slLocation.val(),
                    spwgid: ($('#asset-1 select[name=WorkgroupId]').val() ? $('#asset-1 select[name=WorkgroupId]').val() : 0)
                }
                return query;
            },
            cache: true,
            processResults: function (data) {
                return {
                    results: data.Object
                };
            }
        },
        minimumInputLength: 1
    });
    $select_traderitem.on("select2:select", function (e) {
        var data = e.params.data;
        $('#asset-3 select[name=unit]').empty();
        $('#asset-3 select[name=unit]').select2({
            data: (data.units ? data.units : [])
        });
    });
    //Tab Consumables, Parts & Services
    _tableAssetInventory = $('#tblAssetInventory').DataTable({
        "destroy": true,
        "columnDefs": [
            {
                "targets": 6,
                "visible": false
            },
            {
                "targets": 7,
                "visible": false
            },
            {
                "targets": 8,
                "visible": false
            },
            {
                "targets": 9,
                "visible": false
            },
            {
                "render": function (data, type, row) {
                    var _htmlOptions = '<button type="button" class="btn btn-warning btn-edit-assetinventory"><i class="fa fa-pencil"></i></button> <button type="button" class="btn btn-danger btn-delete-assetinventory"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                },
                "targets": 5
            },
            {
                "render": function (data, type, row) {
                    var _htmlOptions = '<span class="label label-lg label-info">' + data + '</span>';
                    return _htmlOptions;
                },
                "targets": 4
            }
        ],
        rowId: function (a) {
            return 'ain-id-' + UniqueId();
        },
    });
    $('#tblAssetInventory tbody').on('click', 'button.btn-edit-assetinventory', function () {
        var $row = $(this).parents('tr');
        var data = _tableAssetInventory.row($row).data();
        ////Bind data from current row into form Meter
        $('#asset-3 input[name=rid]').val($row.attr('id'));
        $('#asset-3 input[name=atid]').val(data[9]);
        $('#select-traderitems').val(data[6]).change();
        $('#asset-3 select[name=unit]').val(data[7]).change();
        $('#asset-3 select[name=purpose]').val(data[8]).change();
        //Update UI Meter button to status edit
        $('#btnUpdateAssetInventory').show();
        $('#btnAddAssetInventory').hide();
        //end
        $.ajax({
            type: 'GET',
            url: '/Spanneredfree/GetListTraderItem',
            data: {
                search: '',
                locationId: $slLocation.val(),
                spwgid: ($('#asset-1 select[name=WorkgroupId]').val() ? $('#asset-1 select[name=WorkgroupId]').val() : 0),
                itemId: data[6]
            }
        }).then(function (data) {
            // create the option and append to Select2
            data = data.Object;
            currentItemData = data;
            var option = new Option(data.text, data.id, true, true);
            $('#select-traderitems').empty().append(option).trigger('change');
            // manually trigger the `select2:select` event
            $('#select-traderitems').trigger({
                type: 'select2:select',
                params: {
                    data: data
                }
            });

            inventoryInfoValid();
        });
    });
    $('#tblAssetInventory tbody').on('click', 'button.btn-delete-assetinventory', function () {
        _tableAssetInventory.row($(this).parents('tr')).remove().draw();
    });
    $('#btnAddAssetInventory').on('click', function () {
        var _dataInventoryCPS = getDataInventoryCPS();
        if (_dataInventoryCPS.item && checkItemInventoryCPS(_dataInventoryCPS.item.id)) {
            cleanBookNotification.error(_L('ERROR_DATA_EXISTED', [_dataInventoryCPS.item.text]), "Spannered");
            return;
        }
        if (_dataInventoryCPS.item && _dataInventoryCPS.unitid && _dataInventoryCPS.purposeid) {
            _tableAssetInventory.row.add([
                _dataInventoryCPS.item.itemname,
                _dataInventoryCPS.item.barcode,
                _dataInventoryCPS.item.sku,
                _dataInventoryCPS.unittext,
                _dataInventoryCPS.purposetext,
                null,
                _dataInventoryCPS.item.id,
                _dataInventoryCPS.unitid,
                _dataInventoryCPS.purposeid,
                _dataInventoryCPS.id
            ]).draw(false);
            resetInventoryCPS();
        }
    });

    //End Consumables, Parts & Services
    //Form Asset init 
    $frmSpanneredAsset.validate({
        ignore: "",
        rules: {
            Title: {
                required: true,
                maxlength: 200
            },
            Identification: {
                required: true,
                maxlength: 200
            },
            Description: {
                required: true,
                maxlength: 500
            },
            WorkgroupId: {
                required: true
            },
            FeaturedImage: {
                filesize: true
            },
        }
    });
    
}

function saveSpanneredAsset(){
    if (!$('#frmSpanneredAsset').valid()) {
        $('#tabNavAsset a[href=#asset-1]').tab('show');
        return;
    }
    processMediaSpanneredAsset();
}
function processMediaSpanneredAsset() {
    $.LoadingOverlay("show");
    var files = document.getElementById("spannered-asset-image-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("spannered-asset-image-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#spannered-asset-object-key").val(mediaS3Object.objectKey);
            $("#spannered-asset-object-name").val(mediaS3Object.fileName);
            $("#spannered-asset-object-size").val(mediaS3Object.fileSize);

            confirmSaveSpanneredAsset();
        });

    } else
        confirmSaveSpanneredAsset();
};
function confirmSaveSpanneredAsset() {

    var $spanner = {
        Id: $("#spn-id").val(),
        Title: $("#spn-title").val(),
        Identification: $("#spn-identification").val(),
        Description: $("#spn-description").val(),
        WorkgroupId: $("#spn-workgroupId").val(),
        LocationId: $slLocation.val(),
        Tags: $("#spn-tags").val(),//!mutil
        OtherAssets: $("#spn-otherAssets").val(),//!mutil
        Meters: [],//!        
        LinkTraderItemId: "",//x!frmData.append("LinkTraderItemId", dataAssociatedTraderitem.Id);
        AssetInventories: [],//x_traderItemCPS to json
        MediaObjectKey: $("#spannered-asset-object-key").val(),
        MediaObjectName: $("#spannered-asset-object-name").val(),
        MediaObjectSize: $("#spannered-asset-object-size").val()
    };

    var dataMeters = _table.rows().data();

    $.each(dataMeters, function (key, value) {
        $spanner.Meters.push({
            Id: value[0],
            Name: value[1],
            Unit: value[2],
            ValueOfUnit: 0,
            Description: value[3]
        });
    });
   

    var dataAssociatedTraderitem = _tblAssociatedItems.rows().data();
    dataAssociatedTraderitem = (dataAssociatedTraderitem ? dataAssociatedTraderitem[0] : null);
    if (dataAssociatedTraderitem && dataAssociatedTraderitem.Islink)
        $spanner.LinkTraderItemId = dataAssociatedTraderitem.Id;

    var dataInventoryCPS = _tableAssetInventory.rows().data();
    $.each(dataInventoryCPS, function (key, value) {
        $spanner.AssetInventories.push({
            ItemId: value[6],
            UnitId: value[7],
            Purpose: value[8],
            Id: value[9]
        });
    });
    
    $.ajax({
        type: "post",
        cache: false,
        url: "/Spanneredfree/SpanneredFreeSaveAsset",
        //enctype: 'multipart/form-data',
        data: {
            asset: $spanner
        },
        //processData: false,
        //contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                $('#app-spannered-asset-add').modal("hide");
                if (window.location.href.includes("Asset")) {
                    location.reload();
                } else {
                    isInitpagination = false;
                    loadAssets(0);
                }

                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Spannered");
            }
        },
        error: function (data) {
            isBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function linkTraderItemWithAsset(islink, id) {
    if (islink) {
        $('#hdfTraderItemIdLink').val(id);
    } else {
        $('#hdfTraderItemIdLink').val(0);
    }
    $('#tblTraderItem').DataTable().ajax.reload();
}
function updateinventoryCPS() {
    var elm = document.getElementById($('#asset-3 input[name=rid]').val());
    var crow = $('#tblAssetInventory').DataTable().row(elm).data();
    var _dataInventoryCPS = getDataInventoryCPS();
    if (_dataInventoryCPS.item && checkItemInventoryCPS(_dataInventoryCPS.item.id) && crow[6] != _dataInventoryCPS.item.id) {
        cleanBookNotification.error(_L('ERROR_DATA_EXISTED', [_dataInventoryCPS.item.text]), "Spannered");
        return;
    }
    crow[0] = _dataInventoryCPS.item.itemname;
    crow[1] = _dataInventoryCPS.item.barcode;
    crow[2] = _dataInventoryCPS.item.sku;
    crow[3] = _dataInventoryCPS.unittext;
    crow[4] = _dataInventoryCPS.purposetext;
    crow[6] = _dataInventoryCPS.item.id;
    crow[7] = _dataInventoryCPS.unitid;
    crow[8] = _dataInventoryCPS.purposeid;
    $('#tblAssetInventory').DataTable().row(elm).data(crow).draw();
    resetInventoryCPS();

}
function resetInventoryCPS() {
    $('#asset-3 input[name=rid]').val('');
    $('#asset-3 input[name=atid]').val(0);
    $('#select-traderitems').val('').change();
    $('#asset-3 select[name=purpose]').val('').change();
    //Update UI Meter button to status edit
    $('#btnAddAssetInventory').attr("disabled", true);
    $('#btnUpdateAssetInventory').hide();
    $('#btnAddAssetInventory').show();
    //end
    currentItemData = null;
}
function getDataInventoryCPS() {
    var _id = $('#asset-3 input[name=atid]').val();
    var _itemdata = $('#select-traderitems').select2('data');
    if (currentItemData && (!_itemdata[0].itemname || !_itemdata[0].barcode || !_itemdata[0].sku)) {
        _itemdata[0].itemname = currentItemData.itemname;
        _itemdata[0].barcode = currentItemData.barcode;
        _itemdata[0].sku = currentItemData.sku;
    }
    var _unit = $('#asset-3 select[name=unit]').val();
    var _unittext = $('#asset-3 select[name=unit] option:selected').text();
    var _purpose = $('#asset-3 select[name=purpose]').val();
    var _purposetext = $('#asset-3 select[name=purpose] option:selected').text();
    return {
        id: _id,
        item: _itemdata[0],
        unitid: _unit,
        unittext: _unittext,
        purposeid: _purpose,
        purposetext: _purposetext
    };
}
function checkItemInventoryCPS(itemId) {
    var dataInventoryCPS = $('#tblAssetInventory').DataTable().rows().data();
    var isAny = false;
    $.each(dataInventoryCPS, function (key, value) {
        if (itemId == value[6]) {
            isAny = true;
            return;
        }
    });
    return isAny;
}
function inventoryInfoValid() {
    var itemId = $('#select-traderitems').val();
    var unit = $('#asset-3 select[name=unit]').val();
    var purpose = $('#asset-3 select[name=purpose]').val();
    if (itemId && unit && purpose)
        $('#asset-3 .btnvalid').removeAttr("disabled");
    else
        $('#asset-3 .btnvalid').attr("disabled", true);
}
function updateMeter() {
    var elm = document.getElementById($('#asset-4 input[name=rid]').val());
    var crow = $('#tblMeters').DataTable().row(elm).data();
    crow[1] = $('#asset-4 input[name=mname]').val();
    crow[2] = $('#asset-4 input[name=munit]').val();
    crow[3] = $('#asset-4 input[name=mdesc]').val();
    $('#tblMeters').DataTable().row(elm).data(crow).draw();
    //Reset after excecute Update row
    $('#asset-4 input[name=rid]').val('');
    $('#asset-4 input[name=mid]').val('');
    $('#asset-4 input[name=mname]').val('');
    $('#asset-4 input[name=munit]').val('');
    $('#asset-4 input[name=mdesc]').val('');
    //Update UI Meter button to status edit
    $('#btnUpdateMeter').hide();
    $('#btnAddMeter').show();
    //end
}
function meterInfoValid() {
    var meterName = $('#asset-4 input[name=mname]').val();
    var meterUnit = $('#asset-4 input[name=munit]').val();
    var meterDesc = $('#asset-4 input[name=mdesc]').val();
    if (meterName && meterUnit && meterDesc)
        $('#asset-4 .btnvalid').removeAttr("disabled");
    else
        $('#asset-4 .btnvalid').attr("disabled", true);
}
function workgroupSelect(el) {
    if ($(el).valid()) {
        var optionVal = $('option:selected', el).attr('detail-info');
        optionVal = optionVal.split(";");
        $('.prv-process').text(optionVal[2]);
        $('.prv-qbicle').text(optionVal[0]);
        $('.prv-members').text(optionVal[1]);
        $('.prv-members').closest("button").attr("onclick","loadTeamsWorkgroupSpannered(" + $(el).val() + ")");
    }
}