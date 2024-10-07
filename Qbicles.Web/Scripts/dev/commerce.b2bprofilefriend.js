var isBusy = false;
var $frmpartnershiplogistics = $("#frmpartnershiplogistics");
var modalUiSelector = {
    b2bproductmore: "#b2b-product-more",
    el_b2bproductmore_item: "#frmlinkconsumeritem select[name=ConsumerItemId]",
    el_b2bproductmore_locations: "#frmlinkconsumeritem select[name=ConsumerLocations]",
    el_b2bproductmore_unit: "#frmlinkconsumeritem select[name=ConsumerUnitId]",
    el_b2bproductmore_SKU: "#frmlinkconsumeritem input[name=SKU]",
};
var domainParnershipId=0;
$(document).ready(function () {
    domainParnershipId=$('#hdfdomainParnershipId').val();
    $(".checkmulti").multiselect({
        numberDisplayed: 1,
        includeSelectAllOption: true,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    if ($frmpartnershiplogistics.length > 0)
        initFormPartnershipLogistics();
    $('a[data-toggle="collapse"]').on('click', toggleCollapse);
    initTradingItems();
    initTradingItemsOfRelationship();
    initSearch();
});
function toggleCollapse() {
    var val = $(this).attr('aria-expanded');
    if (val == 'true')
    {
        $('a[data-toggle="collapse"]').attr('aria-expanded', 'false');
        $(this).attr('aria-expanded', 'false');
    }
    else
    {
        $('a[data-toggle="collapse"]').attr('aria-expanded', 'false');
        $(this).attr('aria-expanded', 'true');
    }
}
function initFormPartnershipLogistics() {
    //Clone current pricelist
    
    $frmpartnershiplogistics.validate({
        rules: {
            ConsumerLocations: {
                required: true
            },
            ProviderLocations: {
                required: true
            }
        }
    });
    $frmpartnershiplogistics.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmpartnershiplogistics.valid()) {
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
                        $('#b2b-form-partnership-logistics').modal('hide');
                        $('.aboutreq').hide();
                        $('.reqsent').fadeIn();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}
function initTradingItems() {
    var $tblB2bTradingItems = $('#tblB2bTradingItems');
    $tblB2bTradingItems.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblB2bTradingItems.LoadingOverlay("show");
        } else {
            $tblB2bTradingItems.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Commerce/GetTradingItems",
            "data": function (d) {
                var $groupvals = $('select[name="tradingitem-filter-groups"]').val();
                var groupIds = [];
                if (!$groupvals) {
                    groupIds.push(0);
                } else if ($groupvals.length != $('select[name="tradingitem-filter-groups"] option').length) {
                    groupIds = $groupvals
                }
                return $.extend({}, d, {
                    "relationshipId": $('#hdfrelationshipid').val(),
                    "keyword": $('input[name="tradingitem-filter-search"]').val(),
                    "groupIds": JSON.stringify(groupIds),
                    "status": $('select[name="tradingitem-filter-status"]').val()
                });
            }
        },
        columns: [
            { "data": "Item", "orderable": true },
            {
                "data": "TradingName",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    return '<input type="text" class="form-control" onchange="updateTradingName(' + row.Id + ',this)" value="' + fixQuoteCode(data) + '">';
                }
            },
            { "data": "SKU", "orderable": true },
            { "data": "ProductGroup", "orderable": true },
            { "data": "Unit", "orderable": true },
            { "data": "Locations", "orderable": false },
            {
                "data": "Status",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '<span class="label label-lg label-warning">Hidden</span>';
                    if (data)
                        _htmlStatus = '<span class="label label-lg label-success">Shown</span>';
                    return _htmlStatus;
                }
            },
            {
                "data": "Id",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<div class="btn-group options">';
                    _htmlOptions += '<button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    _htmlOptions += 'Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                    if (row.Status)
                        _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="javascript:updateTradingItemStatus(' + data + ',false)">Hide from catalogue</a></li></ul>';
                    else
                        _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="javascript:updateTradingItemStatus(' + data + ',true)">Show in catalogue</a></li></ul>';
                    _htmlOptions += '</div>';
                    return _htmlOptions;
                }
            }
        ]
    });
}
function reloadTblTradingItems() {
    if ($.fn.DataTable.isDataTable('#tblB2bTradingItems')) {
        $('#tblB2bTradingItems').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblB2bTradingItems').DataTable().ajax.reload();
        }, 1000);
    }
}
function initSearch() {
    $('input[name=tradingitem-filter-search]').keyup(delay(function () {
        reloadTblTradingItems();
    }, 400));
    $('select[name=tradingitem-filter-groups]').change(function () {
        reloadTblTradingItems();
    });
    $('select[name=tradingitem-filter-status]').change(function () {
        reloadTblTradingItems();
    });
    $('input[name=tradingitemparnership-filter-search]').keyup(delay(function () {
        loadTradingItemsParnership();
    }, 400));
    $('select[name=tradingitemparnership-filter-groups]').change(function () {
        loadTradingItemsParnership();
    });
    $('input[name=tradingitemrelationship-filter-search]').keyup(delay(function () {
        reloadTblTradingItemsOfRelationship();
    }, 400));
    $('select[name=tradingitemrelationship-filter-islinked]').change(function () {
        reloadTblTradingItemsOfRelationship();
    });
}
function updateTradingItemStatus(id, status) {
    $.post("/Commerce/UpdateTradingItemStatus", { id: id, isShown: status }, function (response) {
        if (response.result)
        {
            reloadTblTradingItems();
        }
        else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
    });
}
function updateTradingName(id, elm) {
    var tradingname=$(elm).val();
    if (tradingname) {
        $.post("/Commerce/UpdateTradingName", { id: id, tradingName: tradingname }, function (response) {
            if (response.result)
                cleanBookNotification.updateSuccess();
            else if(!response.result&&response.msg)
                cleanBookNotification.error(refModel.msg, "Commerce");
            else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        });
    }
}
function orderbyTblTradingItems(index, elm) {
    var logic = $(elm).data('order');
    var table = $('#tblB2bTradingItems').DataTable();
    // Sort by columns 1 and 2 and redraw
    table.order([[index, logic]]).draw();
    $(elm).data('order', logic == 'asc' ? 'desc' : 'asc');
}
function publishCatalogue(elm, relationshipId, isPublish) {
    $.LoadingOverlay("show");
    $.post("/Commerce/PublishCatalogue", { relationshipId: relationshipId, isPublish: isPublish }, function (response) {
        $.LoadingOverlay("hide",true);
        if (response.result)
        {
            cleanBookNotification.updateSuccess();
            if (isPublish)
            {
                $('.menu-catalogue-ispublish').show();
                $(elm).toggleClass('btn-success btn-danger').html('Unpublish');
                var sEvent = $(elm).attr('onclick');
                $(elm).attr('onclick', sEvent.replace(isPublish,'false'));
            }
            else
            {
                $('.menu-catalogue-ispublish').hide();
                $(elm).toggleClass('btn-danger btn-success').html('Publish now');
                var sEvent = $(elm).attr('onclick');
                $(elm).attr('onclick', sEvent.replace(isPublish, 'true'));

            }
        }
        else if (!response.result && response.msg)
            cleanBookNotification.error(refModel.msg, "Commerce");
        else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
    });
}
function loadTradingItemsParnership(orderByString) {
    var $groupvals = $('select[name="tradingitemparnership-filter-groups"]').val();
    var groupIds = [];
    if (!$groupvals) {
        groupIds.push(0);
    } else if ($groupvals.length != $('select[name="tradingitemparnership-filter-groups"] option').length) {
        groupIds = $groupvals
    }
    var paramaters = {
        relationshipId: $('#hdfrelationshipid').val(),
        keyword: $('input[name=tradingitemparnership-filter-search]').val(),
        groupIds: groupIds,
        domainParnershipId: domainParnershipId,
        orderByString:(!orderByString?"TradingName asc":"")
    };
    $('#content-tradingitems-parnership').empty();
    $('#content-tradingitems-parnership').LoadingOverlay("show");
    $('#content-tradingitems-parnership').load("/Commerce/GetTradingItemsPartnership", paramaters, function () {
        $('#content-tradingitems-parnership').LoadingOverlay("hide",true);
    });
}
function orderbyTblTradingItemsParnership(orderByString, elm) {
    var logic = $(elm).data('order');
    $(elm).data('order', logic == 'asc' ? 'desc' : 'asc');
    loadTradingItemsParnership(orderByString);
}
function showProductMore(tradingItemId) {
    var paramaters = {
        tradingItemId: tradingItemId
    };
    $(modalUiSelector.b2bproductmore).empty();
    $(modalUiSelector.b2bproductmore).modal('show');
    $(modalUiSelector.b2bproductmore).load("/Commerce/LoadProductMoreModal", paramaters, function () {
        initSelectModal(modalUiSelector.b2bproductmore);
        $(modalUiSelector.el_b2bproductmore_item).select2({
            ajax: {
                url: '/Commerce/Select2TraderItemsByDomainId',
                delay: 250,
                data: function (params) {
                    var query = {
                        keyword: params.term,
                        domainId: 0,
                        isSell:false,//I Buy
                        page: params.page || 1
                    }
                    return query;
                },
                cache: true
            },
            minimumInputLength: 1
        });
        $(modalUiSelector.el_b2bproductmore_item).on("select2:select", function (e) {
            var data = e.params.data;
            $.get("/Commerce/ItemSelectedById?id=" + data.id, function (response) {
                $(modalUiSelector.el_b2bproductmore_unit).empty();
                $(modalUiSelector.el_b2bproductmore_unit).select2({
                    data: (response.Units ? response.Units : [])
                });
                $(modalUiSelector.el_b2bproductmore_unit).on("select2:select", function (e) {
                    var unit = e.params.data;
                    //$('.cell_itemunit').text(unit.text);
                    $(modalUiSelector.el_b2bproductmore_unit).valid();
                });
                $(modalUiSelector.el_b2bproductmore_locations).empty();
                if (response.Locations && response.Locations.length > 0) {
                    response.Locations.forEach(function (item) {
                        $(modalUiSelector.el_b2bproductmore_locations).append('<option value="' + item.id + '">' + fixQuoteCode(item.text) + '</option>');
                    });
                }
                $(modalUiSelector.el_b2bproductmore_locations).multiselect('destroy');
                $(modalUiSelector.el_b2bproductmore_locations).multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                $(modalUiSelector.el_b2bproductmore_SKU).val(response.SKU);
                var unitdefault = response.Units.find(element => element.selected);
                if (unitdefault)
                    $(modalUiSelector.el_b2bproductmore_unit).valid();
            });
        });
        initFormLinkConsumerItem();
    });
}
function initFormLinkConsumerItem() {
    var $frmlinkconsumeritem = $('#frmlinkconsumeritem');
    $frmlinkconsumeritem.validate({
        ignore: "",
        rules: {
            ConsumerItemId: { required: true },
            TradingName: { required: true },
            ConsumerLocations: { required: true },
            ConsumerUnitId: { required: true }
        }
    });
    $frmlinkconsumeritem.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmlinkconsumeritem.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                url: this.action,
                datatype: "json",
                data: $(this).serialize(),
                success: function (refModel) {
                    if (refModel.result) {
                        $(modalUiSelector.b2bproductmore).modal('hide');
                        cleanBookNotification.createSuccess();
                        loadTradingItemsParnership();
                    } else if (!refModel.result && refModel.msg) {
                        cleanBookNotification.error(refModel.msg, "Commerce");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    }
                    $.LoadingOverlay("hide");
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                    $.LoadingOverlay("hide");
                }
            });
        }
    });
}
function initSelectModal(idparent) {
    $(idparent + " .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(idparent + ' .select2').select2({ placeholder: "Please select" });
}
function initTradingItemsOfRelationship() {
    var $tblTradingItemsOfRelationship = $('#tblTradingItemsOfRelationship');
    $tblTradingItemsOfRelationship.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblTradingItemsOfRelationship.LoadingOverlay("show");
        } else {
            $tblTradingItemsOfRelationship.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Commerce/GetTradingItemsOfRelationship",
            "data": function (d) {
                return $.extend({}, d, {
                    "relationshipId": $('#hdfrelationshipid').val(),
                    "keyword": $('input[name="tradingitemrelationship-filter-search"]').val(),
                    "isLinked": $('select[name="tradingitemrelationship-filter-islinked"]').val()
                });
            }
        },
        columns: [
            { "data": "TradingName", "orderable": true },
            { "data": "Unit", "orderable": true },
            { "data": "SKU", "orderable": true },
            {
                "data": "IsLinked",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '<i class="fa fa-check green"></i> &nbsp; Ready';
                    if (!data)
                        _htmlStatus = '<i class="fa fa-remove red"></i> &nbsp; Not ready';
                    return _htmlStatus;
                }
            },
            {
                "data": "Id",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '';
                    if (!row.IsLinked && row.DomainId==domainParnershipId)
                        _htmlOptions += '<button class="btn btn-success" onclick="showProductMore(' + data + ')"><i class="fa fa-link"></i> &nbsp; Link an item</button>';
                    return _htmlOptions;
                }
            }
        ]
    });
}
function reloadTblTradingItemsOfRelationship() {
    if ($.fn.DataTable.isDataTable('#tblTradingItemsOfRelationship')) {
        $('#tblTradingItemsOfRelationship').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblTradingItemsOfRelationship').DataTable().ajax.reload();
        }, 1000);
    }
}