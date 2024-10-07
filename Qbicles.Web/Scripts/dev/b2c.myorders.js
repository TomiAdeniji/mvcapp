$(function () {
    loadDataOrders();
    initSearchs();
    $('.filter-daterange').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: $dateFormatByUser.toUpperCase()
        }
    });
    $('.filter-daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $(this).trigger("change");
    });

    $('.filter-daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $(this).trigger("change");
    });
});
function initSearchs() {
    $('#subapp-tracking input[name=search]').keyup(delay(function () {
        $('#tblOrders').DataTable().ajax.reload();
    }, 500));
    $('#subapp-tracking input[name=daterange]').change(function () {
        $('#tblOrders').DataTable().ajax.reload();
    });
    $('#subapp-tracking select[name=channel]').change(function () {
        $('#tblOrders').DataTable().ajax.reload();
    });
    $('#subapp-tracking input[name=isHideCompleted]').change(function () {
        $('#tblOrders').DataTable().ajax.reload();
    });
}
function loadDataOrders() {
    var $tblOrders = $('#tblOrders');
    $tblOrders.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        responsive: true,
        order: [[3, "asc"]],
        ajax: {
            "url": "/C2C/GetMyOrders",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#subapp-tracking');
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            {
                "data": "FullRef",
                "searchable": true,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    return '<a href="#my-order" data-toggle="modal" onclick="loadOrderInfo(' + row.OrderId + ')">' + data + '</a>';
                }
            },
            {
                "data": "BusinessName",
                "searchable": true,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _businessHtml='<a href="#" target="_blank">';
                    _businessHtml+='<div class="table-avatar mini pull-left" style="background-image: url(\''+row.BusinessLogoUri+'&size=T\');"></div>';
                    _businessHtml+='<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">'+data+'</div>';
                    _businessHtml += '<div class="clearfix"></div></a>';
                    return _businessHtml;
                }
            },
            { "data": "Placed", "searchable": true, "orderable": true },
            { "data": "Channel", "searchable": true, "orderable": true },
            {
                "name": "Status",
                "data": "Status",
                "searchable": false,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _statusHTML = "";
                    switch (data) {
                        case "Draft":
                            _statusHTML = "<span class=\"label label-lg label-info\">Draft</span>";
                            break;
                        case "AwaitingProcessing":
                        case "Awaiting processing":
                            _statusHTML = "<span class=\"label label-lg label-primary\">Awaiting processing</span>";
                            break;
                        case "InProcessing":
                        case "Processing":
                            _statusHTML = "<span class=\"label label-lg label-primary\">In processing</span>";
                            break;
                        case "Processed":
                        case "Completed":
                            _statusHTML = "<span class=\"label label-lg label-success\">Completed</span>";
                            break;
                        case "ProcessedWithProblems":
                        case "Completed with problems":
                            _statusHTML = "<span class=\"label label-lg label-danger\">Completed with problems</span>";
                            break;
                        case "Ready for payment":
                            _statusHTML = "<span class=\"label label-lg label-primary\">Ready for payment</span>";
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                "data": "OrderId",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-info" data-toggle="modal" data-target="#my-order" onclick="loadOrderInfo(' + data + ')">Order info</button>';
                    _htmlOptions += '&nbsp<a href="/C2C/ContactStore?orderId=' + data + '" class="btn btn-primary" target="_blank" style="text-decoration: none !important;">Contact store</a>';
                    return _htmlOptions;
                }
            },
        ],
        createdRow: function (row, data, dataIndex) {
            // Set the data-status attribute, and add a class
            if (data.Status == "Processed" || data.Status == "Completed")
            {
                $(row).addClass('completed');
            }
        }
    });
    $('#tblOrders_filter').hide();
}
function getParamaters(tabId) {
    var _paramaters = {};
    var $keyword = $(tabId + ' input[name=search]');
    var $datetime = $(tabId + ' input[name=daterange]');
    var $isHideCompleted = $(tabId + ' input[name=isHideCompleted]');
    var $channel = $(tabId + ' select[name=channel]');
    if ($keyword.length > 0)
        _paramaters.keyword = $keyword.val();
    if ($datetime.length > 0)
        _paramaters.daterange = $datetime.val();
    if ($isHideCompleted.length > 0)
        _paramaters.isHideCompleted = $isHideCompleted.prop('checked');
    if ($channel.length > 0)
        _paramaters.channel = $channel.val();
    return _paramaters;
}
function loadOrderInfo(id) {
    var $myorder = $('#my-order');
    $.LoadingOverlay('show');
    $myorder.empty();
    $myorder.load("/B2C/B2COrderInfoModal", { id: id }, function () {
        LoadingOverlayEnd();
    });
}