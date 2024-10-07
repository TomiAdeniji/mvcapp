var isBusy = false;
var $tblWorkgroups = $('#tblWorkgroups');
var $tblAssetTags = $('#tblAssetTags');
var $tblInventories = $('#tblInventories');
var $slLocation = $('#slLocations');
var frmConsumeinventory = "#frmConsumeinventory";
var assetPageSize = 8;
var isInitpagination = false;
var isHideLoading = false;
var unitId = 0;
$(document).ready(function () {
    getCurrencySettings();
    initDataTable();
    initModalTag();
    loadAssets(0);
    initSearch();
    checkPermissionAddEdit("Assets", 0);
});
function getCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
                    CurrencySymbol: '',
                    SymbolDisplay: 0,
                    DecimalPlace: 2
                };
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function initModalWorkgroup() {
    var $frmWorkgroup = $('#frmSpanneredWorkgroup');
    $frmWorkgroup.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 50
            },
            Processes: {
                required: true
            },
            QbicleId: {
                required: true
            },
            TopicId: {
                required: true
            }
        }
    });
    $frmWorkgroup.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmWorkgroup.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-spannered-workgroup-add').modal('hide');
                        $tblWorkgroups.DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Spannered");
                    }
                    checkPermissionAddEdit("Assets", 0);
                    isHideLoading = true;
                    loadAssets(0);
                    isHideLoading = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            });
        } else {
            $('#tabNavWorkgroup a[href=#add-specifics]').tab('show');
            return;
        }
    });
}
function initModalTag() {
    var $frmSpanneredTag = $('#frmSpanneredTag');
    $frmSpanneredTag.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 50
            },
            Summary: {
                required: true,
                maxlength: 500
            }
        }
    });
    $frmSpanneredTag.submit(function (e) {
        e.preventDefault();
        if ($frmSpanneredTag.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-spannered-tag-add').modal('hide');
                        $tblAssetTags.DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Spannered");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            });
        } else {
            return;
        }
    });
}
function initDataTable() {
    //Workgroup DataTable
    $tblWorkgroups.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#config-2').LoadingOverlay("show");
        } else {
            $('#config-2').LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: false,
            searching: false,
            deferLoading: 30,
            order: [[2, "desc"]],
            ajax: {
                "url": "/Spanneredfree/GetWorkgroupsAll",
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationId": $slLocation.val(),
                    });
                }
            },
            columns: [
                    { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                    { "title": "Creator", "data": "Creator", "searchable": true, "orderable": true },
                    { "title": "Created", "data": "Created", "searchable": true, "orderable": true },
                    { "title": "Process", "data": "Process", "searchable": true, "orderable": true },
                    { "title": "Qbicle", "data": "Qbicle", "searchable": true, "orderable": true },
                    { "title": "Members", "data": "Members", "searchable": true, "orderable": false },
                    { "title": "Product group(s)", "data": "ProductGroups", "searchable": true, "orderable": false },
                    { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
            {
                "targets": 4,
                "data": "Qbicle",
                "render": function (data, type, row, meta) {
                    return '<a href="javascript:void(0)" onclick="QbicleSelected(\'' + row.QbicleId + '\',\'Dashboard\')">' + data + '</a>';
                }
            }
            , {
                "targets": 7,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-warning" onclick="loadModalWorkgroup(' + data + ');"><i class="fa fa-pencil"></i></button>';
                    _htmlOptions += ' <button class="btn btn-danger" onclick="deleteConfirmWorkgroup(' + data + ');"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            }]
        });
    //Tag DataTable
    $tblAssetTags.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#config-2').LoadingOverlay("show");
        } else {
            $('#config-2').LoadingOverlay("hide", true);
        }
    })
         .dataTable({
             destroy: true,
             serverSide: true,
             paging: false,
             searching: false,
             deferLoading: 30,
             order: [[0, "asc"]],
             ajax: {
                 "url": "/Spanneredfree/GetTagsAll"
             },
             columns: [
                     { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                     { "title": "Summary", "data": "Summary", "searchable": true, "orderable": true },
                     { "title": "Created", "data": "Created", "searchable": true, "orderable": true },
                     { "title": "Creator", "data": "Creator", "searchable": true, "orderable": true },
                     { "title": "Instances", "data": "Instances", "searchable": true, "orderable": true },
                     { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
             ],
             columnDefs: [
             {
                 "targets": 5,
                 "data": "Id",
                 "render": function (data, type, row, meta) {
                     var _htmlOptions = '<button class="btn btn-warning" onclick="loadModalTag(' + data + ');"><i class="fa fa-pencil"></i></button>';
                     _htmlOptions += ' <button class="btn btn-danger" onclick="deleteConfirmTag(' + data + ');"><i class="fa fa-trash"></i></button>';
                     return _htmlOptions;
                 }
             }]
         });
    //Inventory Datatable
    $tblInventories.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Spanneredfree/GetSpanneredInventoryItems",
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "locationId": $slLocation.val(),
                    "keyword": $("#txtInventorySearch").val(),
                    "groupId": ($("#slInventoryGroup").val() ? $("#slInventoryGroup").val() : 0)
                });
            }
        },
        columns: [
                { "title": "Item", "data": "Item", "searchable": true, "orderable": true },
                { "title": "Unit", "data": "Unit", "searchable": true, "orderable": true },
                { "title": "Barcode", "data": "Barcode", "searchable": true, "orderable": true },
                { "title": "SKU", "data": "SKU", "searchable": true, "orderable": true },
                { "title": "Group", "data": "Group", "searchable": true, "orderable": true },
                { "title": "Current stock", "data": "CurrentStock", "searchable": true, "orderable": false },
                { "title": "Additional", "data": "ItemId", "searchable": true, "orderable": true },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
        ],
        columnDefs: [
         {
             "targets": 0,
             "data": "Item",
             "render": function (data, type, row, meta) {
                 return '<a href="#">' + data + '</a>';
             }
         },
         {
             "targets": 5,
             "data": "CurrentStock",
             "render": function (data, type, row, meta) {
                 return toCurrencySymbol(data, false);
             }
         },
         {
             "targets": 6,
             "data": "ItemId",
             "render": function (data, type, row, meta) {
                 var _htmlOptions = '<button class="btn btn-info" data-toggle="modal" data-target="#app-trader-item-additional" onclick="ShowTraderItemAdditional(' + data + ')"><i class="fa fa-list"></i> &nbsp; View</button>';
                 return _htmlOptions;
             }
         },
         {
             "targets": 7,
             "data": "Id",
             "render": function (data, type, row, meta) {
                 var _htmlOptions = '<div class="btn-group options"><button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                 _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                 _htmlOptions += '<li><a href="#" onclick="removeFromSpannered(' + data + ')">Remove from Spannered</a></li>';
                 _htmlOptions += '<li><a href="#" onclick="loadModalConsume(' + row.ItemId + ')">Consume</a></li>';
                 _htmlOptions += '<li><a href="#" onclick="loadModalTranferItems(' + data + ')">Transfer</a></li>';
                 _htmlOptions += '</ul></div>';
                 return _htmlOptions;
             }
         }
        ]
    });
    reloadTraderGroups();
}
function reloadTraderGroups() {
    $('#slInventoryGroup').empty();
    var qbicleId = $('#wg-qbicle').val();
    $.getJSON('/Spanneredfree/GetTraderGroupsByLocation', { lid: $slLocation.val() }, function (result) {
        result.unshift({ id: 0, text: 'Show all' });
        $('#slInventoryGroup').select2({
            placeholder: "Please select",
            data: result
        });
    });
}
function reloadDataTableConfig() {
    if ($.fn.DataTable.isDataTable('#tblWorkgroups')) {
        $tblWorkgroups.DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $tblWorkgroups.DataTable().ajax.reload();
        }, 1500);
    }
    if ($.fn.DataTable.isDataTable('#tblAssetTags')) {
        $tblAssetTags.DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $tblAssetTags.DataTable().ajax.reload();
        }, 1500);
    }
}
function initSearch() {
    $('#txtAssetSearch').keyup(searchThrottle(function () {
        isInitpagination = false;
        loadAssets(0);
    }));
    $('#sltags').change(searchThrottle(function () {
        isInitpagination = false;
        loadAssets(0);
    }));
    $('#chkIsHidden').change(searchThrottle(function () {
        isInitpagination = false;
        loadAssets(0);
    }));
    $('#slLocations').change(function () {
        isInitpagination = false;
        loadAssets(0);
        reloadTraderGroups();
        if ($('#tab-spannered li.active a[href=#app-config]').length > 0) {
            reloadDataTableConfig();
        }
        if ($('#tab-spannered li.active a[href=#app-main-inventory]').length > 0) {
            reloadDataTableInventory();
        }
        $.post("/Spanneredfree/SaveLocationSelected", { lid :$(this).val()});
    });
    $('#txtInventorySearch').keyup(searchThrottle(function () {
        reloadDataTableInventory();
    }));
    $('#slInventoryGroup').change(function () {
        reloadDataTableInventory();
    });
}
function reloadDefaultTopic(currentTopicId) {
    var qbicleId = $('#wg-qbicle').val();
    $.getJSON('/Topics/GetTopicByQbicleId', { qbicleId: (qbicleId ? qbicleId : 0), currentTopicId: currentTopicId }, function (result) {
        $('#wg-topic').select2({
            placeholder: "Select a state",
            data: result
        });
    });
}
function loadModalWorkgroup(id) {
    var locationId = $slLocation.val();
    $('#app-spannered-workgroup-add').modal("show");
    $("#app-spannered-workgroup-add").load("/Spanneredfree/LoadModalWorkgroup", { id: id, locationId: locationId }, function () {
        $('#txtLocationName').val($('#slLocations option:selected').text());
        $('#txtWGLocationId').val(locationId);
        $('select.select2').select2();
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        // Cycle app nav tabs with button triggers
        $('.btnNext').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('#tabNavWorkgroup .active').next('li').find('a').trigger('click');
        });

        $('.btnPrevious').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('#tabNavWorkgroup .active').prev('li').find('a').trigger('click');
        });
        initModalWorkgroup();
    });

}
function deleteConfirmWorkgroup(id) {
    $('#wg-delete-id').val(id);
    $('#wg-delete-modal').modal("show");
}
function deleteWorkgroup() {
    $.post("/Spanneredfree/DeleteWorkgroup", { id: $('#wg-delete-id').val() }, function (data) {
        if (data.result) {
            $('#wg-delete-id').val(0);
            $('#wg-delete-modal').modal('hide');
            $tblWorkgroups.DataTable().ajax.reload();
        } else if (!data.result && data.msg) {
            cleanBookNotification.error(_L(data.msg), "Spannered");
        }
    });
}
function loadModalTag(id) {
    $('#frmSpanneredTag').validate().resetForm();//remove error class on name elements and clear history
    if (id > 0) {
        $.get("/Spanneredfree/getTagById?id=" + id, function (data) {
            if (data) {
                $('#app-spannered-tag-add h5.modal-title').text('Edit a Tag');
                $('#frmSpanneredTag input[name="Id"]').val(data.Id);
                $('#frmSpanneredTag input[name="Name"]').val(data.Name);
                $('#frmSpanneredTag input[name="Summary"]').val(data.Summary);
                $('#app-spannered-tag-add').modal('show');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_410", [id]), "Spannered");
            }
        });
    } else {
        $('#app-spannered-tag-add h5.modal-title').text('Add a Tag');
        $('#frmSpanneredTag input[name="Id"]').val(0);
        $('#frmSpanneredTag input[name="Name"]').val('');
        $('#frmSpanneredTag input[name="Summary"]').val('');
        $('#app-spannered-tag-add').modal('show');
    }
}
function deleteConfirmTag(id) {
    $('#tag-delete-id').val(id);
    $('#tag-delete-modal').modal("show");
}
function deleteTag() {
    $.post("/Spanneredfree/DelectTag", { id: $('#tag-delete-id').val() }, function (data) {
        if (data.result) {
            $('#tag-delete-id').val(0);
            $('#tag-delete-modal').modal('hide');
            $tblAssetTags.DataTable().ajax.reload();
        } else if (!data.result && data.msg) {
            cleanBookNotification.error(_L(data.msg), "Spannered");
        }
    });
}
function addMembers(id) {
    var members = $('#slMembers').val();
    if (!members) {
        members = [];
    }
    members.push(id);
    $('#slMembers').val(members);
}
function removeMembers(id) {
    var members = $('#slMembers').val();
    if (members)
        members = $.grep(members, function (value) {
            return value != id;
        });
    var approvers = $('#slReviewersApprovers').val();
    if (approvers)
        approvers = $.grep(approvers, function (value) {
            return value != id;
        });
    $('#slReviewersApprovers').val(approvers);
    $('#slMembers').val(members);
    //remove approval 94b7decd-2740-4f80-9718-b6c5340be87e
    $("#apr" + id).prop("checked", false);
    $("#apr" + id).change();
}
function isApprover(id, thiss) {
    var approvers = $('#slReviewersApprovers').val();
    if ($(thiss).prop("checked")) {
        if (!approvers) {
            approvers = [];
        }
        approvers.push(id);
    } else {

        if (approvers)
            approvers = $.grep(approvers, function (value) {
                return value != id;
            });
    }
    $('#slReviewersApprovers').val(approvers);
}
function filterMembers() {
    try {
        var kw = $('#spkeyword').val();
        var filterShow = $('#slShow').val();
        if (kw) {
            $("#wgMembers li").each(function () {
                var elLi = $(this);
                var name = elLi.attr("fullname");
                if (filterShow == "1") {
                    if (elLi.hasClass("ismember") && name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                } else {
                    if (name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                }
            });
        } else {
            if (filterShow == "1") {
                $("ul.widget-contacts li.ismember").show();
                $("ul.widget-contacts li:not(.ismember)").hide();
            } else {
                $("ul.widget-contacts li").show();
            }
        }

    } catch (e) {
        return;
    }

}
function loadAssets(skip) {
    var option = $('#chkIsHidden').prop('checked') ? 2 : 3;
    if (!isHideLoading && $('#tab-spannered li.active a[href=#app-assets]').length > 0) $('#app-assets').LoadingOverlay("show");
    var _tags = $('#sltags').val();
    var _param = {
        lid: $slLocation.val(),
        skip: skip,
        take: assetPageSize,
        keyword: $('#txtAssetSearch').val(),
        option: option,
        tags: _tags ? _tags : []
    };
    $.ajax({
        url: '/Spanneredfree/LoadAssets',
        type: "POST",
        data: JSON.stringify(_param),
        contentType: 'application/json',
        dataType: "json",
        success: function (response) {
            if (response.result) {
                if (response.Object) {
                    $('#app-assets div.flex-grid-quarters-lg').html(response.Object.strResult);
                    if (!isInitpagination)
                        initPagination(response.Object.totalRecord, assetPageSize, '#AssetPaginateTemplate');
                }
            }
            $('#app-assets').LoadingOverlay("hide", true);
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            $('#app-assets').LoadingOverlay("hide", true);
        }
    });
}
function updateOptionAsset(id, op) {
    $.post("/Spanneredfree/UpdateOptionAsset", { id: id, option: op }, function (data) {
        if (!$('#chkIsHidden').prop('checked'))
        {
            isInitpagination = false;
            loadAssets(0);
        } else
        {
            var _pageNum = $('#AssetPaginateTemplate').pagination("getSelectedPageNum");
            loadAssets((_pageNum - 1) * assetPageSize);
        }
    });
}
function initPagination(totalRecord, pageSize, elementID) {
    var container = $(elementID);
    if (totalRecord != 0) {
        container.show();
        var sources = function () {
            var result = [];
            for (var i = 1; i <= totalRecord; i++) {
                result.push(i);
            }
            return result;
        }();

        var options = {
            prevText: '&nbsp; &laquo; Prev &nbsp;',
            nextText: '&nbsp; Next &raquo; &nbsp;',
            currentPage: 1,
            pageSize: pageSize,
            dataSource: sources,
            callback: function (response, pagination) {
                if (isInitpagination)
                    loadAssets((pagination.pageNumber - 1) * pageSize, pageSize);
            }
        };
        container.pagination(options);
        isInitpagination = true;
    } else {
        container.hide();
        isInitpagination = false;
    }
}
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}
function backupValueOfUnit(meterId) {
    $('#valueOfUnit_' + meterId).val($('#valueOfUnit_' + meterId).data('backup'));
}
function checkAndUpdateValueOfUnit(meterId) {
    if ($("#valueOfUnit_" + meterId).val()) {
        $.ajax({
            type: 'post',
            url: '/Spanneredfree/UpdateValueOfUnit',
            datatype: 'json',
            data: {
                meterId: meterId, valueOfUnit: $("#valueOfUnit_" + meterId).val()
            },
            success: function (refModel) {
                if (refModel.result) {
                    $('.addreading_' + meterId).hide(); $('.meter-options_' + meterId).fadeIn();
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
                    searchMeters();
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            },
            fail: function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
            },
        });
    } else {
        cleanBookNotification.error("Value of unit cannot be empty!", "Spannered");
    }
}
function checkPermissionAddEdit(process, workgroupId) {
    $.post("/Spanneredfree/CheckPermissionAddEdit", { process: process, workgroupId: workgroupId }, function (data) {
        if (data.result) {
            if (process === "Assets") {
                $("#options-assets button").show();
                $("a[href='#app-spannered-asset-edit']").show();
            } else if (process === "Asset Tasks") {
                $("#task-options button").show();
            } else if (process === "Meters") {
                $("#meter-options button").show();
            } else if (process == "Consumption Reports,Purchases,Transfers") {
                $('#btnAddCPT').show();
                if (data.Object && data.Object.indexOf("Consumption Reports") >= 0) {
                    $('.add-consume-report').show();
                }else{
                    $('.add-consume-report').hide();
                }
                if (data.Object && data.Object.indexOf("Purchases") >= 0) {
                    $('.add-consume-purchase').show();
                } else {
                    $('.add-consume-purchase').hide();
                }
                if (data.Object && data.Object.indexOf("Transfers") >= 0) {
                    $('.add-consume-transfer').show();
                } else {
                    $('.add-consume-transfer').hide();
                }
            }
        } else {
            if (process === "Assets") {
                $("#options-assets button").hide();
                $("a[href='#app-spannered-asset-edit']").hide();
            } else if (process === "Asset Tasks") {
                $("#task-options button").hide();
            } else if (process === "Meters") {
                $("#meter-options button").hide();
            } else if (process == "Consumption Reports,Purchases,Transfers") {
                $('#btnAddCPT').hide();
            }
        }
    });
}
function loadModalTransfer(assetId) {
    $('#app-spannered-asset-relocate').modal("show");
    $('#app-spannered-asset-relocate').load("/Spanneredfree/LoadModalTransfer", { assetId: assetId, locationId: $slLocation.val() }, function () {
        $('#app-spannered-asset-relocate select.select2').select2({ placeholder: 'Please select' });
        initFormTransferAsset(assetId);
    });
}
function initFormTransferAsset(assetId) {
    var $frmTransferAsset = $('#frmTransferAsset');
    $frmTransferAsset.validate({
        ignore: "",
        rules: {
            WorkgroupId: {
                required: true
            },
            DestinationLocationId: {
                required: true
            }
        }
    });
    $frmTransferAsset.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmTransferAsset.valid()) {
            $.LoadingOverlay("show");
            var transfer = {
                Status: 'PendingPickup',
                OriginatingLocation: { Id: $slLocation.val() },
                DestinationLocation: { Id: $('#frmTransferAsset select[name=DestinationLocationId]').val() },
                Workgroup: { Id: $('#frmTransferAsset select[name=WorkgroupId]').val() }
            };

            $.ajax({
                type: "post",
                url: "/Spanneredfree/SaveTransferAsset",
                data: { assetId: assetId, transfer: transfer },
                dataType: "json",
                success: function (response) {
                    $.LoadingOverlay("hide");
                    $('#app-spannered-asset-relocate').modal("hide");
                    if (response.result) {
                        cleanBookNotification.createSuccess();
                        var _pageNum = $('#AssetPaginateTemplate').pagination("getSelectedPageNum");
                        loadAssets((_pageNum - 1) * assetPageSize);
                    } else {
                        cleanBookNotification.error(response.msg, "Spannered");
                    }
                },
                error: function (er) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            });
        } else {
            $('#frmTransferAsset ul.app_subnav a[href=#relocate-1]').tab('show');
            return;
        }
    });
    initNextPreviousTab('#frmTransferAsset', '#tabtransferasset');

}
function previewLocation(el) {
    var option = $(el).children("option:selected");
    if (option) {
        $('#preview-destination-location-title').text('To ' + $(option).data("name"));
        $('#preview-destination-location-address').html($(option).data("address"));
    }
}
function reloadDataTableInventory() {
    if ($.fn.DataTable.isDataTable('#tblInventories')) {
        $tblInventories.DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $tblInventories.DataTable().ajax.reload();
        }, 1500);
    }
}
function ShowTraderItemAdditional(traderItemId) {
    var ajaxUri = '/TraderItem/GetTraderItem?id=' + traderItemId;
    LoadingOverlay();
    $('#app-trader-item-additional').empty();
    $('#app-trader-item-additional').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
}
function removeFromSpannered(aiId) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Spannered",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Spanneredfree/RemoveItemSpanneredByAIId", { aiId: aiId }, function (Response) {
                    if (Response.result) {
                        $tblInventories.DataTable().ajax.reload();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(response.msg, "Spannered");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                    }
                });
                return;
            }
        }
    });
}
function loadModalConsume(itemId) {
    $('#app-spannered-inventory-consume').empty();
    $('#app-spannered-inventory-consume').modal("show");
    $("#app-spannered-inventory-consume").load("/Spanneredfree/loadModalConsume", { itemId: itemId, lid: $slLocation.val() }, function () {
        if (itemId > 0)
            unitId = 0;
        initModalConsume(itemId);
    });
}
function initModalConsume(itemId) {
    $(frmConsumeinventory + ' select.select2').select2({ placeholder: 'Please select' });
    var $frmCI = $('#frmConsumeinventory');
    $frmCI.validate({
        ignore: "",
        rules: {
            SPWorkgroupId: {
                required: true
            },
            Name: {
                required: true,
                minlength: 5,
                maxlength: 150
            },
            Description: {
                required: true,
                maxlength: 500
            }
        },
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $frmCI.submit(function (e) {
        e.preventDefault();
        
        if (isBusy)
            return;
        if ($frmCI.valid()) {
            $.LoadingOverlay("show");
            var consumeReport = {
                Status: 'Pending',
                LocationId: $slLocation.val(),
                TaskId: $('#hdfAssocicatedLinkTaskId').val(),
                WorkgroupId: $(frmConsumeinventory + ' select[name=SPWorkgroupId]').val(),
                Name: $(frmConsumeinventory + ' input[name=Name]').val(),
                Description: $(frmConsumeinventory + ' textarea[name=Description]').val(),
                Items: []
            };
            var dataConsumedStock = $('#tblConsumedStock').DataTable().rows().data();
            $.each(dataConsumedStock, function (key, value) {
                var _used = 0;
                if (value.Used) {
                    _used = value.Used;
                    consumeReport.Items.push({
                        ItemId: value.ItemId,
                        UnitId: value.UnitId,
                        Allocated: stringToNumber(value.Allocated ? value.Allocated : 0),
                        Used: _used,
                        Note: value.Note
                    });
                }
            });
            if (consumeReport.Items.length == 0)
            {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MIN_ITEM_CONSUME"), "Spannered");
                return;
            }
            $.ajax({
                type: "post",
                url: this.action,
                data: { consumeReport: consumeReport },
                dataType: "json",
                success: function (response) {
                    $.LoadingOverlay("hide");
                    $('#app-spannered-inventory-consume').modal("hide");
                    if (response.result) {
                        cleanBookNotification.createSuccess();
                    } else {
                        cleanBookNotification.error(_L(response.msg), "Spannered");
                    }
                },
                error: function (er) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            });
        }
    });
    initNextPreviousTab(frmConsumeinventory, '#tabConsumeInventory');
    $('#tblAssetTasks').DataTable({ "destroy": true });
    initTableConsumeStock(itemId);
    
    
    $('#slCSGroups').select2({
        ajax: {
            url: '/Spanneredfree/GetTraderGroupsByLocation',
            delay: 250,
            data: function (params) {
                var query = {
                    search: params.term,
                    lid: $slLocation.val(),
                }
                return query;
            },
            cache: true,
            processResults: function (data) {
                data.unshift({ id: 0, text: 'Show all' });
                return {
                    results: data
                };
            }
        }
    });
    $('#slATGroups').select2();
    $('#txtATKeyword').keyup(searchThrottle(function () {
        $('#tblAssetTasks').DataTable().search($(this).val()).draw();
    }));
    $('#slATGroups').change(function () {
        $('#tblAssetTasks').DataTable().columns(2).search($(this).val()).draw();
    });
    $('#txtCSKeyword').keyup(searchThrottle(function () {
        loadConsumedStock();
    }));
    $('#slCSGroups').change(function () {
        loadConsumedStock();
    });
}
function initTableConsumeStock(itemId) {
    if(itemId==0)
    {
        $('#tblConsumedStock').DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            order: [[2, "asc"]],
            ajax: {
                "url": "/Spanneredfree/GetSpanneredInventoryItems",
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationId": $slLocation.val(),
                        "keyword": $("#txtCSKeyword").val(),
                        "groupId": ($("#slCSGroups").val() ? $("#slCSGroups").val() : 0),
                        "linkTaskId": $('#hdfAssocicatedLinkTaskId').val()
                    });
                }
            },
            columns: [
                    { "title": "ItemId", "data": "ItemId", "searchable": false, "orderable": false },
                    { "title": "UnitId", "data": "UnitId", "searchable": false, "orderable": false },
                    { "title": "Item", "data": "Item", "searchable": true, "orderable": true },
                    { "title": "Unit", "data": "Unit", "searchable": true, "orderable": true },
                    { "title": "Barcode", "data": "Barcode", "searchable": true, "orderable": true },
                    { "title": "SKU", "data": "SKU", "searchable": true, "orderable": true },
                    { "title": "Group", "data": "Group", "searchable": true, "orderable": true },
                    { "title": "In stock", "data": "CurrentStock", "searchable": true, "orderable": false },
                    { "title": "Allocated", "data": "Allocated", "searchable": true, "orderable": true },
                    { "title": "Used", "data": "Used", "searchable": false, "orderable": false },
            ],
            columnDefs: [
             {
                 "targets": 0,
                 "visible": false
             },
             {
                 "targets": 1,
                 "visible": false
             },
             {
                 "targets": 2,
                 "data": "Item",
                 "render": function (data, type, row, meta) {
                     return '<a href="#">' + data + '</a>';
                 }
             },
             {
                 "targets": 7,
                 "data": "CurrentStock",
                 "render": function (data, type, row, meta) {
                     return toCurrencySymbol(data, false);
                 }
             },
             {
                 "targets": 8,
                 "data": "Allocated",
                 "render": function (data, type, row, meta) {
                     return data ? toCurrencySymbol(data, false) : '';
                 }
             },
             {
                 "targets": 9,
                 "data": "Used",
                 "render": function (data, type, row, meta) {
                     var _htmlOptions = '<input type="number" class="form-control trackInput">';
                     return _htmlOptions;
                 }
             }
            ],
            drawCallback: function (settings) {
                $(".trackInput").on("change", function () {
                    var $row = $(this).parents("tr");
                    var rowData = $('#tblConsumedStock').DataTable().row($row).data();
                    rowData.Used = $(this).val();
                })
            }
        });
    }else
    {
        $('#tblConsumedStock').DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            order: [[2, "asc"]],
            ajax: {
                "url": "/Spanneredfree/GetSpanneredInventoryItems",
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationId": $slLocation.val(),
                        "keyword": $("#txtCSKeyword").val(),
                        "groupId": ($("#slCSGroups").val() ? $("#slCSGroups").val() : 0),
                        "linkTaskId": $('#hdfAssocicatedLinkTaskId').val(),
                        "itemId": itemId,
                        "unitId": unitId
                    });
                }
            },
            columns: [
                    { "title": "ItemId", "data": "ItemId", "searchable": false, "orderable": false },
                    { "title": "UnitId", "data": "UnitId", "searchable": false, "orderable": false },
                    { "title": "Item", "data": "Item", "searchable": true, "orderable": true },
                    { "title": "Unit", "data": "Unit", "searchable": true, "orderable": true },
                    { "title": "Barcode", "data": "Barcode", "searchable": true, "orderable": true },
                    { "title": "SKU", "data": "SKU", "searchable": true, "orderable": true },
                    { "title": "Group", "data": "Group", "searchable": true, "orderable": true },
                    { "title": "In stock", "data": "CurrentStock", "searchable": true, "orderable": false },
                    { "title": "Allocated", "data": "Allocated", "searchable": true, "orderable": true },
                    { "title": "Used", "data": "Used", "searchable": false, "orderable": false },
            ],
            columnDefs: [
             {
                 "targets": 0,
                 "visible": false
             },
             {
                 "targets": 1,
                 "visible": false
             },
             {
                 "targets": 2,
                 "data": "Item",
                 "render": function (data, type, row, meta) {
                     return '<a href="#">' + data + '</a>';
                 }
             },
             {
                 "targets": 3,
                 "data": "Unit",
                 "render": function (data, type, row, meta) {
                     var _units = '<select name="unit" onchange="changeUnitConsumeReport(this)" class="form-control select2" style="width: 100%;">';
                     if (row.Units) {
                         $.each(row.Units, function (key, value) {
                             _units += '<option value="' + value.id + '" ' + (value.id == row.UnitId ? "selected" : "") + '>' + fixQuoteCode(value.text) + '</option>';
                         });
                     }                                                                                                                    
                     _units +='</select>';
                     return _units;
                 }
             },
             {
                 "targets": 7,
                 "data": "CurrentStock",
                 "render": function (data, type, row, meta) {
                     return toCurrencySymbol(data, false);
                 }
             },
             {
                 "targets": 8,
                 "visible": false
             },
             {
                 "targets": 9,
                 "data": "Used",
                 "render": function (data, type, row, meta) {
                     var _htmlOptions = '<input type="number" class="form-control trackInput" style="border: 1px solid #e1e1e1;">';
                     return _htmlOptions;
                 }
             }
            ],
            drawCallback: function (settings) {
                $(".trackInput").on("change", function () {
                    var $row = $(this).parents("tr");
                    var rowData = $('#tblConsumedStock').DataTable().row($row).data();
                    rowData.Used = $(this).val();
                })
                $('#tblConsumedStock .select2').select2();
            }
        });
    }
}
function consumeWorkgroupSelect(el) {
    if ($(el).valid()) {
        var optionVal = $('option:selected', el).attr('detail-info');
        optionVal = optionVal.split("|");
        var groups = JSON.parse(optionVal[3]);
        $(frmConsumeinventory + ' .prv-Location').text($('option:selected', $slLocation).text());
        $(frmConsumeinventory + ' .prv-process').text(optionVal[2]);
        $(frmConsumeinventory + ' .prv-qbicle').text(optionVal[0]);
        $(frmConsumeinventory + ' .prv-members').text(optionVal[1]);
        $(frmConsumeinventory + ' .prv-members').closest("button").attr("onclick", "loadTeamsWorkgroupSpannered(" + $(el).val() + ")");
        if (groups) {
            var _htmlUl = "<ul class=\"unstyled\" style=\"margin: 0; padding: 8px 0;\">";
            groups.forEach(function (item) {
                _htmlUl += "<li>" + fixQuoteCode(item) + "</li>";
            });
            _htmlUl += "</ul>";
            $(frmConsumeinventory + ' .prv-groups').html(_htmlUl);
        }
        //Load filter asignee
        var params = {
            search: '',
            wgId: ($('#frmConsumeinventory select[name=SPWorkgroupId]').val() ? $('#frmConsumeinventory select[name=SPWorkgroupId]').val() : 0)
        }
        $.get("/Spanneredfree/GetMembersQbicleByWorkgroupId", params).done(function (data) {
            data.Object.unshift({ id: "0", text: "Show all" });
            $("#slATGroups").select2({
                data: data.Object
            })
        });
    }
}
function loadConsumedStock() {
    if ($.fn.DataTable.isDataTable('#tblConsumedStock')) {
        $('#tblConsumedStock').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblConsumedStock').DataTable().ajax.reload();
        }, 1500);
    }
}
function initNextPreviousTab(frmId, tabId) {
    $(frmId + ' .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId+' .active').next('li').find('a').trigger('click');
    });

    $(frmId + ' .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').prev('li').find('a').trigger('click');
    });
}
function associcatedConsumeLinkTask(isLink, taskId, elm) {
    if (isLink) {
        $('#hdfAssocicatedLinkTaskId').val(taskId);
        $('#tblAssetTasks').DataTable().columns(5).search(taskId).draw();
        $(elm).hide();
        $('#taskchange' + taskId).show();
        $('#optnext').removeAttr('disabled');
        $('#skip').hide();
        loadConsumedStock();
    } else {
        $('#tblAssetTasks').DataTable().columns(5).search('').draw();
        $('#hdfAssocicatedLinkTaskId').val(0);
        $(elm).hide();
        $('#taskuse' + taskId).show();
        $('#optnext').attr('disabled', true);
        $('#skip').show();
        loadConsumedStock();
    }
}
function changeUnitConsumeReport(elm) {
    var _unitId = $(elm).val();
    unitId = _unitId ? _unitId : 0;
    loadConsumedStock();
}
function loadTeamsWorkgroupSpannered(wgId) {
    $('#team-view').empty();
    $('#team-view').modal('show');
    $('#team-view').load("/Spanneredfree/LoadTeamsByWorkgroupId?wgId=" + wgId);
}
function loadConsumeItemsByTaskId(taskId) {
    $('#app-spannered-items-task').empty();
    $('#app-spannered-items-task').modal('show');
    $('#app-spannered-items-task').load("/Spanneredfree/LoadConsumeItemsByTaskId?taskId=" + taskId + "&lId="+$slLocation.val());
}
function inventoryPermissions() {
    checkPermissionAddEdit("Consumption Reports,Purchases,Transfers", 0);
}