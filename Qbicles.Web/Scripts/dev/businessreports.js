var isBusy = false;
var tabs = {
    isordersload: false,//Order status
    issalesload: false,//Sales
    ispurchaseload: false,//purchase
    isinvoicesload: false,//Invoices
    ispaymentsload: false,//Payments
    istranfersload: false,//Transfers & delivery
    isbookkeepingload: false,//bookkeeping
    isinventoryload: false,//Inventory
};

var bookkeepingSubTabs = {
    isManufacturingLoad: false,
    isPaymentLoad: false,
    isInventoryPurchaseLoad: false,
    isNonInventoryPurchaseLoad: false,
    isSaleInvoiceLoad: false,
    isSaleTransferLoad: false
};
$(document).ready(function () {
    initNavClick();
    var _tabactive = getQuerystring('tab');
    switch (_tabactive) {
        case 'sales':
            $('a[href=#sales]').click();
            break;
        case 'purchase':
            $('a[href=#purchase]').click();
            break;
        case 'invoices':
            $('a[href=#invoices]').click();
            break;
        case 'payments':
            $('a[href=#payments]').click();
            break;
        case 'transfers':
            $('a[href=#transfers]').click();
            break;
        case 'bookkeeping':
            $('a[href=#bookkeeping]').click();
        default:
            $('a[href=#orders]').click();
            break;
    }
    //if (_tabactive == 'general-contacts') {
    //    $('a[href=#trading-general]').click();
    //    setTimeout(function () {
    //        $('a[href=#' + _tabactive + ']').click();
    //    }, 500);
    //}
});
function initNavClick() {
    $('ul.nav-stacked li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {
                case "#sales":
                    loadTabContent(elid, tabs.issalesload);
                    tabs.issalesload = true;
                    break;
                case "#purchase":
                    loadTabContent(elid, tabs.ispurchaseload);
                    tabs.ispurchaseload = true;
                    break;
                case "#invoices":
                    loadTabContent(elid, tabs.isinvoicesload);
                    tabs.isinvoicesload = true;
                    break;
                case "#payments":
                    loadTabContent(elid, tabs.ispaymentsload);
                    tabs.ispaymentsload = true;
                    break;
                case "#transfers":
                    loadTabContent(elid, tabs.istranfersload);
                    tabs.istranfersload = true;
                    break;
                case "#orders":
                    loadTabContent(elid, tabs.isordersload);
                    tabs.isordersload = true;
                    break;
                case "#bookkeeping":
                    loadTabContent(elid, tabs.isbookkeepingload);
                    tabs.isbookkeepingload = true;
                case "#inventory":
                    loadTabContent(elid, tabs.isinventoryload);
                    tabs.isinventoryload = true;
            }
        }, 200);
    });
}
function loadTabContent(tabid, isfirstload) {
    if (!isfirstload) {
        tabid = tabid.replace('#', '');
        var $content;
        switch (tabid) {
            default:
                $content = $('#' + tabid);
                break;
        }
        $content.empty();
        $content.LoadingOverlay("show");
        $content.load("/BusinessReports/LoadBusinessReportTab", { tab: tabid }, function (response) {
            $content.LoadingOverlay("hide");
            initPlugins(tabid);
            overrideFunctions(tabid);
            $('.dataTables_filter').hide();
        });
    }
}
function overrideFunctions(tabid) {
    switch (tabid) {
        case 'orders':
            loadDataOrdersSale();
            loadDataOrdersPurchase();
            $('#orders-sale input[name=search]').keyup(delay(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            }, 500));
            $('#orders-sale input[name=daterange]').change(delay(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            }, 500));
            $('#orders-sale select[name=location]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });
            $('#orders-sale select[name=channel]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });
            $('#orders-sale select[name=contacts]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });
            $('#orders-sale select[name=filterInvoice]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });
            $('#orders-sale select[name=filterPayment]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });
            $('#orders-sale select[name=filterDelivery]').change(function () {
                $("#tblOrdersSale").DataTable().ajax.reload();
            });

            $('#orders-purchase input[name=search]').keyup(delay(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            }, 500));
            $('#orders-purchase input[name=daterange]').change(delay(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            }, 500));
            $('#orders-purchase select[name=location]').change(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            });
            $('#orders-purchase select[name=contacts]').change(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            });
            $('#orders-purchase select[name=filterInvoice]').change(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            });
            $('#orders-purchase select[name=filterPayment]').change(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            });
            $('#orders-purchase select[name=filterDelivery]').change(function () {
                $("#tblOrdersPurchase").DataTable().ajax.reload();
            });
            break;
        case 'purchase':
            loadDataPurchase();
            //init event customize filters for sales tab
            $('#purchase input[name=search]').keyup(delay(function () {
                $("#tblPurchase").DataTable().ajax.reload();
            }, 500));
            $('#purchase input[name=daterange]').change(delay(function () {
                $("#tblPurchase").DataTable().ajax.reload();
            }, 500));
            $('#purchase select[name=location]').change(function () {
                $("#tblPurchase").DataTable().ajax.reload();
            });
            $('#purchase select[name=channel]').change(function () {
                $("#tblPurchase").DataTable().ajax.reload();
            });
            $('#purchase select[name=contacts]').change(function () {
                $("#tblPurchase").DataTable().ajax.reload();
            });
            break;
        case 'sales':
            loadDataSales();
            loadDashBoardSales();
            //init event customize filters for sales tab
            $('#sales input[name=search]').keyup(delay(function () {
                $("#tblSales").DataTable().ajax.reload();
                loadDashBoardSales();
            }, 500));
            $('#sales input[name=daterange]').change(delay(function () {
                $("#tblSales").DataTable().ajax.reload();
                loadDashBoardSales();
            }, 500));
            $('#sales select[name=location]').change(function () {
                $("#tblSales").DataTable().ajax.reload();
                loadDashBoardSales();
            });
            $('#sales select[name=channel]').change(function () {
                $("#tblSales").DataTable().ajax.reload();
                loadDashBoardSales();
            });
            $('#sales select[name=contacts]').change(function () {
                $("#tblSales").DataTable().ajax.reload();
                loadDashBoardSales();
            });
            var accordionsMenu = $('.cd-accordion-menu');

            if (accordionsMenu.length > 0) {
                accordionsMenu.each(function () {
                    var accordion = $(this);
                    //detect change in the input[type="checkbox"] value
                    accordion.on('change', 'input[type="checkbox"]', function () {
                        var checkbox = $(this);

                        (checkbox.prop('checked')) ? checkbox.siblings('ul').attr('style', 'display:none;').slideDown(300) : checkbox.siblings('ul').attr('style', 'display:block;').slideUp(300);
                    });
                });
            }

            $('.dash-showhide').bind('click', function (e) {
                e.preventDefault();
                collapseSummary();
            });
            break;
        case 'invoices':
            loadDataInvoices();
            //init event customize filters for invoices tab
            $('#invoices input[name=search]').keyup(delay(function () {
                $("#tblInvoices").DataTable().ajax.reload();
            }, 500));
            $('#invoices input[name=daterange]').change(delay(function () {
                $("#tblInvoices").DataTable().ajax.reload();
            }, 500));
            $('#invoices select[name=contacts]').change(function () {
                $("#tblInvoices").DataTable().ajax.reload();
            });
            break;
        case 'payments':
            loadDataPayments();
            //init event customize filters for invoices tab
            $('#payments input[name=search]').keyup(delay(function () {
                $("#tblPayments").DataTable().ajax.reload();
            }, 500));
            $('#payments input[name=daterange]').change(delay(function () {
                $("#tblPayments").DataTable().ajax.reload();
            }, 500));
            $('#payments select[name=contacts]').change(function () {
                $("#tblPayments").DataTable().ajax.reload();
            });
            break;
        case 'transfers':
            loadDataTransfers();
            //init event customize filters for invoices tab
            $('#transfers input[name=search]').keyup(delay(function () {
                $("#tblTransfers").DataTable().ajax.reload();
            }, 500));
            $('#transfers input[name=daterange]').change(delay(function () {
                $("#tblTransfers").DataTable().ajax.reload();
            }, 500));
            break;
        case 'bookkeeping':
            loadDataManufacturing();
            initBKSubTab();
            break;
        case 'inventory':
            loadDataInventoryTbl();

            //Init envent for input fields

            break;
        default:
            break;
    }
}
function initPlugins(tabid) {
    $("#" + tabid + " .checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $("#" + tabid + " .select2").select2();
    var $daterange = $("#" + tabid + " .daterange");
    $daterange.daterangepicker({
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        },
        timePicker: true
    });
    $daterange.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $(this).change();
    });
    $daterange.on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $(this).change();
    });
}
function loadDataPurchase() {
    $("#tblPurchase").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#tblPurchase').LoadingOverlay("show");
        } else {
            $('#tblPurchase').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": ""
        },
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/BusinessReports/GetPurchase',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#purchase');
                return $.extend({}, d, _paramaters);
            }
        },
        "columns": [
            {
                name: "FullRef",
                data: "FullRef",
                orderable: true
            },
            {
                name: "Location",
                data: "Location",
                orderable: true
            },
            {
                name: "Channel",
                data: "PurchaseChannel",
                orderable: true
            },
            { name: "Contact", data: "Contact", orderable: true },
            {
                name: "Total",
                data: "PurchaseTotal",
                orderable: true
            },
            {
                name: "Status",
                data: "Status",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = '<span class="label label-lg ' + row.LabelStatus + '">' + row.Status + '</span>';
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var _htmlOptions = '<button class="btn btn-primary" disabled><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    if (row.AllowEdit)
                        _htmlOptions = '<button class="btn btn-primary" onclick="window.location.href = \'/TraderPurchases/PurchaseMaster?id=' + row.Id + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    return _htmlOptions;
                }
            }
        ],
        "order": [[0, "asc"]]
    });
}
function loadDataSales() {
    $("#tblSales").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#tblSales').LoadingOverlay("show");
        } else {
            $('#tblSales').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": ""
        },
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/BusinessReports/GetSales',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#sales');
                return $.extend({}, d, _paramaters);
            }
        },
        "columns": [
            {
                name: "FullRef",
                data: "FullRef",
                orderable: true
            },
            {
                name: "Location",
                data: "Location",
                orderable: true
            },
            {
                name: "SalesChannel",
                data: "SalesChannel",
                orderable: true
            },
            { name: "Contact", data: "Contact", orderable: true },
            {
                name: "SaleTotal",
                data: "SaleTotal",
                orderable: true
            },
            {
                name: "Status",
                data: "Status",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = '<span class="label label-lg ' + row.LabelStatus + '">' + row.Status + '</span>';
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var _htmlOptions = '<button class="btn btn-primary" disabled><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    if (row.AllowEdit)
                        _htmlOptions = '<button class="btn btn-primary" onclick="window.location.href = \'/TraderSales/SaleMaster?key=' + row.Key + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    return _htmlOptions;
                }
            }
        ],
        "order": [[0, "asc"]]
    });
}
function loadDashBoardSales() {
    if ($('.loadingoverlay').length === 0)
        $.LoadingOverlay("show");
    var _paramaters = getParamaters('#sales');
    $('#section_dashboard').load("/TraderSales/SaleReportGetDataDashBoard", _paramaters, function () {
        $('.dash-concise').show();
        LoadingOverlayEnd();
    });
}
function loadDataInvoices() {
    var $tblInvoices = $('#tblInvoices');
    $tblInvoices.on('processing.dt', function (e, settings, processing) {
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
        order: [[0, "asc"]],
        ajax: {
            "url": "/BusinessReports/GetInvoices",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#invoices');
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "data": "Ref", "searchable": true, "orderable": true },
            { "name": "Contact", "data": "Contact", "orderable": true },
            { "data": "Date", "searchable": true, "orderable": true },
            { "data": "Amount", "searchable": true, "orderable": true },
            {
                "data": "Status",
                "searchable": false,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _statusHTML = "";
                    switch (data) {
                        case 0:
                            _statusHTML = '<span class="label label-lg label-info">Draft</span>';
                            break;
                        case 1:
                            _statusHTML = '<span class="label label-lg label-warning">Pending Review</span>';
                            break;
                        case 2:
                            _statusHTML = '<span class="label label-lg label-primary">Pending Approval</span>';
                            break;
                        case 3:
                            _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                            break;
                        case 4:
                            _statusHTML = '<span class="label label-lg label-success">Approved</span>';
                            break;
                        case 5:
                            _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                            break;
                        default:
                            _statusHTML = '<span class="label label-lg label-success">Issued</span>';
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    if (row.AllowEdit && !row.IsBill)
                        return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderInvoices/InvoiceManage?key=' + row.Key + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    else if (row.AllowEdit && row.IsBill)
                        return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderBill/BillManage?key=' + row.Key + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    else
                        return '<button class="btn btn-primary" disabled=""><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                }
            },
        ]
    });
}
function loadDataPayments() {
    var $tblPayments = $('#tblPayments');
    $tblPayments.on('processing.dt', function (e, settings, processing) {
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
        order: [[2, "desc"]],
        ajax: {
            "url": "/BusinessReports/GetPayments",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#payments');
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "data": "Ref", "searchable": true, "orderable": true },
            { "name": "Contact", "data": "Contact", "orderable": true },
            { "data": "Date", "searchable": true, "orderable": true },
            { "data": "Amount", "searchable": true, "orderable": true },
            {
                "data": "Status",
                "searchable": false,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _statusHTML = "";
                    switch (data) {
                        case 0:
                            _statusHTML = '<span class="label label-lg label-info">Draft</span>';
                            break;
                        case 1:
                            _statusHTML = '<span class="label label-lg label-warning">Pending Review</span>';
                            break;
                        case 2:
                            _statusHTML = '<span class="label label-lg label-primary">Pending Approval</span>';
                            break;
                        case 3:
                            _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                            break;
                        case 4:
                            _statusHTML = '<span class="label label-lg label-success">Approved</span>';
                            break;
                        default:
                            _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    if (!row.AllowEdit)
                        return '<button class="btn btn-primary" disabled=""><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    else
                        return '<button class="btn btn-primary" onclick="window.location.href =\'/TraderPayments/PaymentManage?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                }
            }
        ]
    });
}
function loadDataTransfers() {
    var $tblTransfers = $('#tblTransfers');
    $tblTransfers.on('processing.dt', function (e, settings, processing) {
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
        order: [[3, "desc"]],
        ajax: {
            "url": "/BusinessReports/GetTransfers",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#transfers');
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "name": "FullRef", "data": "FullRef", "searchable": true, "orderable": true },
            { "name": "From", "data": "From", "searchable": true, "orderable": true },
            {
                name: "To",
                data: "To",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.Route)
                        _html = '<a href="/Community/UserProfilePage?uId=' + row.Route + '" target="_blank">' + value + '</a>';
                    else
                        _html = value;
                    return _html;
                }
            },
            { "name": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "name": "Reason", "data": "Reason", "searchable": true, "orderable": true },
            {
                "name": "Status",
                "data": "Status",
                "searchable": false,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _statusHTML = "";
                    switch (data) {
                        case "Initiated":
                            _statusHTML = "<span class=\"label label-lg label-info\">Initiated</span>";
                            break;
                        case "PendingPickup":
                            _statusHTML = "<span class=\"label label-lg label-primary\">Pending Pickup</span>";
                            break;
                        case "PickedUp":
                            _statusHTML = "<span class=\"label label-lg label-success\">Picked up</span>";
                            break;
                        case "Delivered":
                            _statusHTML = "<span class=\"label label-lg label-success\">Delivered</span>";
                            break;
                        case "Draft":
                            _statusHTML = "<span class=\"label label-lg label-primary\">Draft</span>";
                            break;
                        case "Denied":
                            _statusHTML = "<span class=\"label label-lg label-danger\">Denied</span>";
                            break;
                        case "Discarded":
                            _statusHTML = "<span class=\"label label-lg label-danger\">Discarded</span>";
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-primary" disabled><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    if (row.AllowEdit)
                        _htmlOptions = '<button class="btn btn-primary" onclick="window.location.href=\'/TraderTransfers/TransferMaster?key=' + row.Key + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function loadDataOrdersSale() {
    var $tblOrders = $('#tblOrdersSale');
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
        aaSorting: [],
        ajax: {
            "url": "/BusinessReports/GetOrders",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#orders-sale');
                _paramaters['isSaleOrderShow'] = true;
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "name": "Ref", "data": "Ref", "searchable": true, "orderable": true },
            { "name": "CreatedDate", "data": "CreatedDate", "searchable": true, "orderable": true },
            { "name": "Location", "data": "Location", "searchable": true, "orderable": true },
            { "name": "Channel", "data": "Channel", "searchable": true, "orderable": true },
            { "name": "Contact", "data": "Contact", "orderable": true },
            { "name": "Total", "data": "Total", "searchable": true, "orderable": true },
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
                            _statusHTML = "<span class=\"label label-lg label-primary\">Awaiting processing</span>";
                            break;
                        case "InProcessing":
                            _statusHTML = "<span class=\"label label-lg label-primary\">In processing</span>";
                            break;
                        case "Processed":
                            _statusHTML = "<span class=\"label label-lg label-success\">Processed</span>";
                            break;
                        case "ProcessedWithProblems":
                            _statusHTML = "<span class=\"label label-lg label-danger\">Processed with problems</span>";
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                name: "Status",
                data: "Sale",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.SaleKey) {
                        _html += '<a href="/TraderSales/SaleMaster?key=' + row.SaleKey + '" target="_blank">' + value;
                        switch (row.SaleStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "PendingReview":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                break;
                            case "PendingApproval":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                break;
                            case "SaleApproved":
                                _html += "<br /><span class=\"label label-success\">Approved</span>";
                                break;
                            case "SaleDenied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "SaleDiscarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            case "SalesOrderedIssued":
                                _html += "<br /><span class=\"label label-danger\">Ordered Issued</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                name: "Invoice",
                data: "Invoice",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.InvoiceId) {
                        _html = '<a href="/TraderInvoices/InvoiceManage?key=' + row.InvoiceKey + '" target="_blank">' + value;
                        switch (row.InvoiceStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "PendingReview":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                break;
                            case "PendingApproval":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                break;
                            case "InvoiceApproved":
                                _html += "<br /><span class=\"label label-success\">Approved</span>";
                                break;
                            case "InvoiceDenied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "InvoiceDiscarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            case "InvoiceIssued":
                                _html += "<br /><span class=\"label label-success\">Issued</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                name: "Payment",
                data: "Payments",
                orderable: false,
                render: function (items, type, row) {
                    var _html = '';
                    if (items.length > 0) {
                        items.forEach(function (item) {
                            if (item.Id)
                                _html += '<a href="/TraderPayments/PaymentManage?id=' + item.Id + '" target="_blank">' + item.Ref;
                            if (item.Status) {
                                switch (item.Status) {
                                    case "Draft":
                                        _html += "<br /><span class=\"label label-info\">Draft</span>";
                                        break;
                                    case "PendingReview":
                                        _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                        break;
                                    case "PendingApproval":
                                        _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                        break;
                                    case "PaymentApproved":
                                        _html += "<br /><span class=\"label label-success\">Approved</span>";
                                        break;
                                    case "PaymentDenied":
                                        _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                        break;
                                    case "PaymentDiscarded":
                                        _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                        break;
                                }
                            }
                            _html += "</a><br/>";
                        });
                    } else {
                        _html += '--';
                    }
                    return _html;
                }
            },
            {
                name: "Transfer",
                data: "Transfer",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.TransferId) {
                        _html = '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferKey + '" target="_blank">' + value + '</a>';
                        switch (row.TransferStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "Initiated":
                                _html += "<br /><span class=\"label label-primary\">Initiated</span>";
                                break;
                            case "PendingPickup":
                                _html += "<br /><span class=\"label label-warning\">Pending Pickup</span>";
                                break;
                            case "PickedUp":
                                _html += "<br /><span class=\"label label-warning\">Picked Up</span>";
                                break;
                            case "Delivered":
                                _html += "<br /><span class=\"label label-success\">Delivered</span>";
                                break;
                            case "Denied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "Discarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var showOption = false;
                    var _htmlOptions = '<div class="btn-group">';
                    _htmlOptions += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown">Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                    _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    if (row.Channel == 'B2C') {
                        _htmlOptions += '<li><a href="/B2C/ManageOrder?id=' + data + '">Manage</a></li>';
                        showOption = true;
                    }
                    if (row.Status == 'AwaitingProcessing' || (row.Status == 'InProcessing' && row.OrderProblem != 'Non')) {
                        _htmlOptions += '<li><a href="javascript:void(0);" onclick="reProcessOrder(' + data + ')">Process</a></li>';
                        showOption = true;
                    } else if (row.Status == 'InProcessing') {
                        _htmlOptions += '<li><a href="javascript:void(0);" onclick="reProcessOrderInProcessing(' + data + ')">Process</a></li>';
                        showOption = true;
                    }
                    _htmlOptions += ' </ul></div>';
                    if (showOption)
                        return _htmlOptions;
                    else
                        return '';
                }
            },
        ]
    });
}
function loadDataOrdersPurchase() {
    var $tblOrders = $('#tblOrdersPurchase');
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
        aaSorting: [],
        searching: true,
        responsive: true,
        ajax: {
            "url": "/BusinessReports/GetOrders",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = getParamaters('#orders-purchase');
                _paramaters['isSaleOrderShow'] = false;
                _paramaters['channel'] = 3;
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "name": "Ref", "data": "Ref", "searchable": true, "orderable": true },
            { "name": "CreatedDate", "data": "CreatedDate", "searchable": true, "orderable": true },
            { "name": "Location", "data": "DestinationLocation", "searchable": true, "orderable": true },
            { "name": "Contact", "data": "Contact", "orderable": true },
            { "name": "Total", "data": "Total", "searchable": true, "orderable": true },
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
                            _statusHTML = "<span class=\"label label-lg label-primary\">Awaiting processing</span>";
                            break;
                        case "InProcessing":
                            _statusHTML = "<span class=\"label label-lg label-primary\">In processing</span>";
                            break;
                        case "Processed":
                            _statusHTML = "<span class=\"label label-lg label-success\">Processed</span>";
                            break;
                        case "ProcessedWithProblems":
                            _statusHTML = "<span class=\"label label-lg label-danger\">Processed with problems</span>";
                            break;
                    }
                    return _statusHTML;
                }
            },
            {
                name: "Status",
                data: "Purchase",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.PurchaseId) {
                        _html += '<a href="/TraderPurchases/PurchaseMaster?id=' + row.PurchaseId + '" target="_blank">' + value;
                        switch (row.PurchaseStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "PendingReview":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                break;
                            case "PendingApproval":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                break;
                            case "PurchaseApproved":
                                _html += "<br /><span class=\"label label-success\">Approved</span>";
                                break;
                            case "PurchaseDenied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "PurchaseDiscarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            case "PurchaseOrderIssued":
                                _html += "<br /><span class=\"label label-danger\">Ordered Issued</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                name: "Bill",
                data: "Bill",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.BillId) {
                        _html = '<a href="/TraderBill/BillManage?id=' + row.BillId + '" target="_blank">' + row.Bill;
                        switch (row.BillStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "PendingReview":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                break;
                            case "PendingApproval":
                                _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                break;
                            case "InvoiceApproved":
                                _html += "<br /><span class=\"label label-success\">Approved</span>";
                                break;
                            case "InvoiceDenied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "InvoiceDiscarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            case "InvoiceIssued":
                                _html += "<br /><span class=\"label label-danger\">Issued</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                name: "Payment",
                data: "Payments",
                orderable: false,
                render: function (items, type, row) {
                    var _html = '';
                    if (items.length > 0) {
                        items.forEach(function (item) {
                            if (item.Id)
                                _html += '<a href="/TraderPayments/PaymentManage?id=' + item.Id + '" target="_blank">' + item.Ref;
                            if (item.Status) {
                                switch (item.Status) {
                                    case "Draft":
                                        _html += "<br /><span class=\"label label-info\">Draft</span>";
                                        break;
                                    case "PendingReview":
                                        _html += "<br /><span class=\"label label-warning\">Awaiting Review</span>";
                                        break;
                                    case "PendingApproval":
                                        _html += "<br /><span class=\"label label-warning\">Awaiting Approval</span>";
                                        break;
                                    case "PaymentApproved":
                                        _html += "<br /><span class=\"label label-success\">Approved</span>";
                                        break;
                                    case "PaymentDenied":
                                        _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                        break;
                                    case "PaymentDiscarded":
                                        _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                        break;
                                }
                            }
                            _html += "</a><br/>";
                        });
                    } else {
                        _html += '--';
                    }
                    return _html;
                }
            },
            {
                name: "Transfer",
                data: "Transfer",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.TransferId) {
                        _html = '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferKey + '" target="_blank">' + row.Transfer + '</a>';
                        switch (row.TransferStatus) {
                            case "Draft":
                                _html += "<br /><span class=\"label label-info\">Draft</span>";
                                break;
                            case "Initiated":
                                _html += "<br /><span class=\"label label-primary\">Initiated</span>";
                                break;
                            case "PendingPickup":
                                _html += "<br /><span class=\"label label-warning\">Pending Pickup</span>";
                                break;
                            case "PickedUp":
                                _html += "<br /><span class=\"label label-warning\">Picked Up</span>";
                                break;
                            case "Delivered":
                                _html += "<br /><span class=\"label label-success\">Delivered</span>";
                                break;
                            case "Denied":
                                _html += "<br /><span class=\"label label-danger\">Denied</span>";
                                break;
                            case "Discarded":
                                _html += "<br /><span class=\"label label-danger\">Discarded</span>";
                                break;
                            default:
                                break;
                        }
                        _html += '</a>';
                    }
                    else
                        _html = '--';
                    return _html;
                }
            },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var showOption = false;
                    var _htmlOptions = '<div class="btn-group">';
                    _htmlOptions += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown">Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                    _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    if (row.Channel == 'B2C') {
                        _htmlOptions += '<li><a href="/B2C/ManageOrder?id=' + data + '">Manage</a></li>';
                        showOption = true;
                    }
                    if (row.Status == 'AwaitingProcessing' || (row.Status == 'InProcessing' && row.OrderProblem != 'Non')) {
                        _htmlOptions += '<li><a href="javascript:void(0);" onclick="reProcessOrder(' + data + ')">Process</a></li>';
                        showOption = true;
                    } else if (row.Status == 'InProcessing') {
                        _htmlOptions += '<li><a href="javascript:void(0);" onclick="reProcessOrderInProcessing(' + data + ')">Process</a></li>';
                        showOption = true;
                    }
                    _htmlOptions += ' </ul></div>';
                    if (showOption)
                        return _htmlOptions;
                    else
                        return '';
                }
            },
        ]
    });
}
function getParamaters(tabId, modalId = '') {
    var _paramaters = {};
    var $keyword = $(tabId + ' input[name=search]');
    var $datetime = $(tabId + ' input[name=daterange]');
    var $channel = $(tabId + ' select[name=channel]');
    var $contact = $(tabId + ' select[name=contacts]');
    var $location = $(tabId + ' select[name=location]');
    var $filterInvoice = $(tabId + ' select[name=filterInvoice]');
    var $filterPayment = $(tabId + ' select[name=filterPayment]');
    var $filterDelivery = $(tabId + ' select[name=filterDelivery]');
    if ($keyword.length > 0)
        _paramaters.keyword = $keyword.val();
    if ($datetime.length > 0)
        _paramaters.datetime = $datetime.val();
    if ($location.length > 0)
        _paramaters.locationId = $location.val();
    if ($channel.length > 0)
        _paramaters.channel = $channel.val();
    if ($contact.length > 0) {
        _paramaters.contactId = $contact.val();
    }
    if ($filterInvoice.length > 0)
        _paramaters.filterInvoice = $filterInvoice.val();
    if ($filterPayment.length > 0)
        _paramaters.filterPayment = $filterPayment.val();
    if ($filterDelivery.length > 0)
        _paramaters.filterDelivery = $filterDelivery.val();

    _paramaters.modalId = modalId;
    return _paramaters;
}
function collapseSummary() {
    $('.dash-collapse').slideToggle();
    $('.dash-notify').slideToggle();
}

function loadDataManufacturing() {
    var $tblManufacturing = $('#bk-manufacturing-table');
    $tblManufacturing.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblManufacturing.LoadingOverlay("show");
        } else {
            $tblManufacturing.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            "url": "/BusinessReports/GetBkManufacturing",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-manufacturing input[name=textsearch]").val(),
                    daterange: $("#bk-manufacturing input[name=daterange]").val(),
                    locationId: $("#bk-manufacturing select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "JobRef",
                data: "JobRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Manufacturing/ManuJobReview?id=' + row.JobId + '" target="_blank">' + row.JobRef + '</a>';
                    return _html;
                }
            },
            {
                name: "SaleRef",
                data: "SaleRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderSales/SaleMaster?key=' + row.SaleKey + '" target="_blank">' + row.SaleRef + '</a>';
                    return _html;
                }
            },
            {
                name: "TransferInRef",
                data: "TransferInRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferInKey + '" target="_blank">' + row.TransferInRef + '</a>';
                    return _html;
                }
            },
            {
                name: "TransferOutRef",
                data: "TransferOutRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferOutKey + '" target="_blank">' + row.TransferOutRef + '</a>';
                    return _html;
                }
            }
        ]
    });
}
function loadDataPayment() {
    var $tblPayment = $('#bk-payment-table');
    $tblPayment.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblPayment.LoadingOverlay("show");
        } else {
            $tblPayment.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            "url": "/BusinessReports/GetBkPaymentTblData",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-payment input[name=textsearch]").val(),
                    daterange: $("#bk-payment input[name=daterange]").val(),
                    locationId: $("#bk-payment select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "PaymentRef",
                data: "PaymentRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderPayments/PaymentManage?id=' + row.PaymentId + '" target="_blank">' + row.PaymentRef + '</a>';
                    return _html;
                }
            }
        ]
    });
}
function loadDataInventoryPurchase() {
    var $tblInvPurchase = $('#bk-invpurchase-table');
    $tblInvPurchase.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblInvPurchase.LoadingOverlay("show");
        } else {
            $tblInvPurchase.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            "url": "/BusinessReports/GetBkInvPurchase",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-invent input[name=textsearch]").val(),
                    daterange: $("#bk-invent input[name=daterange]").val(),
                    locationId: $("#bk-invent select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "PurchaseRef",
                data: "PurchaseRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderPurchases/PurchaseMaster?id=' + row.PurchaseId + '" target="_blank">' + row.PurchaseRef + '</a>';
                    return _html;
                }
            },
            {
                name: "TransferRef",
                data: "TransferRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferKey + '" target="_blank">' + row.TransferRef + '</a>';
                    return _html;
                }
            }
        ]
    });
}
function loadDataNonInventoryPurchase() {
    var $tblNonInvPurchase = $('#bk-noninvpurchase-table');
    $tblNonInvPurchase.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblNonInvPurchase.LoadingOverlay("show");
        } else {
            $tblNonInvPurchase.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            "url": "/BusinessReports/GetBkNonInvPurchase",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-noninv input[name=textsearch]").val(),
                    daterange: $("#bk-noninv input[name=daterange]").val(),
                    locationId: $("#bk-noninv select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "PurchaseRef",
                data: "PurchaseRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderPurchases/PurchaseMaster?id=' + row.PurchaseId + '" target="_blank">' + row.PurchaseRef + '</a>';
                    return _html;
                }
            },
            {
                name: "BillRef",
                data: "BillRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderBill/BillManage?id=' + row.BillId + '" target="_blank">' + row.BillRef + '</a>';
                    return _html;
                }
            }
        ]
    });
}
function loadDataSaleInvoice() {
    var $tblSInvoice = $('#bk-sinvoice-table');
    $tblSInvoice.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblSInvoice.LoadingOverlay("show");
        } else {
            $tblSInvoice.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        order: [[0, 'desc']],
        searching: false,
        ajax: {
            "url": "/BusinessReports/GetBkSInvoice",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-sinvoice input[name=textsearch]").val(),
                    daterange: $("#bk-sinvoice input[name=daterange]").val(),
                    locationId: $("#bk-sinvoice select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "SaleRef",
                data: "SaleRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderSales/SaleMaster?key=' + row.SaleKey + '" target="_blank">' + row.SaleRef + '</a>';
                    return _html;
                }
            },
            {
                name: "InvoiceRef",
                data: "InvoiceRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderInvoices/InvoiceManage?key=' + row.InvoiceKey + '" target="_blank">' + row.InvoiceRef + '</a>';
                    return _html;
                }
            }
        ]
    });
}
function loadDataSaleTransfer() {
    var $tblSTransfer = $('#bk-stransfer-table');
    $tblSTransfer.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblSTransfer.LoadingOverlay("show");
        } else {
            $tblSTransfer.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        order: [[0, 'desc']],
        searching: false,
        ajax: {
            "url": "/BusinessReports/GetBkSTransfer",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $("#bk-stransfer input[name=textsearch]").val(),
                    daterange: $("#bk-stransfer input[name=daterange]").val(),
                    locationId: $("#bk-stransfer select[name=loc]").val()
                });
            }
        },
        columns: [
            {
                "name": "CreatedDate",
                "data": "CreatedDate",
                "searchable": true,
                "orderable": true
            },
            {
                name: "CreatedBy",
                data: "CreatedBy",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Community/UserProfilePage?uId=' + row.CreatedById + '" target="_blank">' + row.CreatedBy + '</a>';
                    return _html;
                }
            },
            {
                name: "JournalEntryNumber",
                data: "JournalEntryNumber",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/Bookkeeping/JournalEntry?id=' + row.JournalEntryId + '" target="_blank">' + row.JournalEntryNumber + '</a>';
                    return _html;
                }
            },
            {
                name: "TransferRef",
                data: "TransferRef",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="/TraderTransfers/TransferMaster?key=' + row.TransferKey + '" target="_blank">' + row.TransferRef + '</a>';
                    return _html;
                }
            },
            {
                name: "Type",
                data: "Type",
                orderable: true,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.Type != "") {
                        _html += '<span class="label label-lg label-info">' + row.Type + '</span>';
                    }
                    return _html;
                }
            }
        ]
    });
}

function initBKSubTab() {
    $('#bookkeeping ul.nav-pills li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {
                case "#bk-manufacturing":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isManufacturingLoad);
                    bookkeepingSubTabs.isManufacturingLoad = true;
                    break;
                case "#bk-payment":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isPaymentLoad);
                    bookkeepingSubTabs.isPaymentLoad = true;
                    break;
                case "#bk-invent":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isInventoryPurchaseLoad);
                    bookkeepingSubTabs.isInventoryPurchaseLoad = true;
                    break;
                case "#bk-noninv":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isNonInventoryPurchaseLoad);
                    bookkeepingSubTabs.isNonInventoryPurchaseLoad = true;
                    break;
                case "#bk-sinvoice":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isSaleInvoiceLoad);
                    bookkeepingSubTabs.isSaleInvoiceLoad = true;
                    break;
                case "#bk-stransfer":
                    loadBKDataTableContent(elid, bookkeepingSubTabs.isSaleTransferLoad);
                    bookkeepingSubTabs.isSaleTransferLoad = true;
                    break;
            }
        }, 200);
    });

    //init key-change and daterange-change event
    //Manufacturing
    $("#bk-manufacturing input[name=textsearch]").keyup(delay(function () {
        $('#bk-manufacturing-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-manufacturing input[name=daterange]").change(delay(function () {
        $('#bk-manufacturing-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-manufacturing select[name=loc]").change(delay(function () {
        $('#bk-manufacturing-table').DataTable().ajax.reload();
    }, 500));

    //Payment
    $("#bk-payment input[name=textsearch]").keyup(delay(function () {
        $('#bk-payment-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-payment input[name=daterange]").change(delay(function () {
        $('#bk-payment-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-payment select[name=loc]").change(delay(function () {
        $('#bk-payment-table').DataTable().ajax.reload();
    }, 500));

    //Inventory Purchase
    $("#bk-invent input[name=textsearch]").keyup(delay(function () {
        $('#bk-invpurchase-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-invent input[name=daterange]").change(delay(function () {
        $('#bk-invpurchase-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-invent select[name=loc]").change(delay(function () {
        $('#bk-invpurchase-table').DataTable().ajax.reload();
    }, 500));

    //NonInventory Purchase
    $("#bk-noninv input[name=textsearch]").keyup(delay(function () {
        $('#bk-noninvpurchase-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-noninv input[name=daterange]").change(delay(function () {
        $('#bk-noninvpurchase-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-noninv select[name=loc]").change(delay(function () {
        $('#bk-noninvpurchase-table').DataTable().ajax.reload();
    }, 500));

    //Sale Invoice
    $("#bk-sinvoice input[name=textsearch]").keyup(delay(function () {
        $('#bk-sinvoice-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-sinvoice input[name=daterange]").change(delay(function () {
        $('#bk-sinvoice-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-sinvoice select[name=loc]").change(delay(function () {
        $('#bk-sinvoice-table').DataTable().ajax.reload();
    }, 500));

    //Sale Transfer
    $("#bk-stransfer input[name=textsearch]").keyup(delay(function () {
        $('#bk-stransfer-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-stransfer input[name=daterange]").change(delay(function () {
        $('#bk-stransfer-table').DataTable().ajax.reload();
    }, 500));
    $("#bk-stransfer select[name=loc]").change(delay(function () {
        $('#bk-stransfer-table').DataTable().ajax.reload();
    }, 500));
}
function loadBKDataTableContent(tabid, isFirstLoad) {
    if (!isFirstLoad) {
        switch (tabid) {
            case "#bk-manufacturing":
                loadDataManufacturing();
                break;
            case "#bk-payment":
                loadDataPayment();
                break;
            case "#bk-invent":
                loadDataInventoryPurchase()
                break;
            case "#bk-noninv":
                loadDataNonInventoryPurchase();
                break;
            case "#bk-sinvoice":
                loadDataSaleInvoice();
                break;
            case "#bk-stransfer":
                loadDataSaleTransfer();
                break;
            default:
                loadDataManufacturing();
                break;
        }
    }
}
function reProcessOrder(id) {
    $.LoadingOverlay('show');
    $.post("/BusinessReports/ReProcessOrder", { tradeOderId: id }, function (response) {
        $.LoadingOverlay("hide");
        if (response.result) {
            cleanBookNotification.success(_L('B2C_PROCESSED_ORDER'), "Qbicles");
            $("#tblOrdersSale").DataTable().ajax.reload();
            $("#tblOrdersPurchase").DataTable().ajax.reload();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function reProcessOrderInProcessing(id) {
    $.LoadingOverlay('show');
    $.post("/BusinessReports/ReProcessOrderInProcessing", { tradeOderId: id }, function (response) {
        $.LoadingOverlay("hide");
        if (response.result) {
            cleanBookNotification.success(_L('B2C_PROCESSED_ORDER'), "Qbicles");
            $("#tblOrdersSale").DataTable().ajax.reload();
            $("#tblOrdersPurchase").DataTable().ajax.reload();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function showDetailTraderItemByGroup(ids) {
    $.LoadingOverlay("show");
    var datetime = $("#sales input[name=daterange]").val();
    $('#app-trader-product-group').load("/TraderSales/TraderSaleGetDetailTraderItems?ids=" + ids, { datetime: datetime }, function () {
        LoadingOverlayEnd();
    });
}

function loadDataInventoryTbl() {
    var url = '/BusinessReports/GetInventoryTblData';

    var columns = [
        {
            name: "Item",
            data: "Item",
            orderable: true,
            render: function (value, type, row) {
                var htmlString = "";
                htmlString += '<a href="#" onclick=ShowInventoryItemDetailModal(' + row.UnitId + ') data-toggle="modal">' + row.Item + '</a>'

                return htmlString;
            }
        },
        {
            id: "",
            name: "Unit",
            data: "Unit",
            orderable: true,
            render: function (value, type, row) {
                return row.ListUnitHtmlString;
            }
        },
        {
            name: "CurrentInventory",
            data: "CurrentInventory",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-current-inventory-" + row.Id + "'>" + row.CurrentInventory + "</span>";
            }
        },
        {
            name: "DaysToLast",
            data: "DaysToLast",
            orderable: false,
            render: function (value, type, row) {
                if (row.DaysToLastHighlighted)
                    return "<span id='row-day-to-last-" + row.Id + "' class='label label-danger' data-tooltip='Reorder required' data-tooltip-color='#dd4b39'>" + row.DaysToLast + "</span>";
                return "<span id='row-day-to-last-" + row.Id + "'>" + row.DaysToLast + "</span>";
            }
        },
        {
            name: "InventoryTotal",
            data: "InventoryTotal",
            orderable: false,
            render: function (value, type, row) {
                return "<span id='row-inventory-total-" + row.Id + "'>" + row.InventoryTotal + "</span>";
            }
        },
        {
            name: "Associated",
            data: "Associated",
            orderable: false,
            render: function (value, type, row) {
                if (row.Associated === "0 item(s)") {
                    return "";
                } else {
                    return "<button onclick='ShowIngredientsItemAssociated(" + row.Id + ")' class='btn btn-info'><i class='fa fa-cube'></i> &nbsp; " + row.Associated + "</button>";
                }
            }
        }
    ];
    $tblInventory = $("#tbl-inventory");
    $tblInventory.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblInventory.LoadingOverlay("show");
        } else {
            $tblInventory.LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "serverSide": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "fnDrawCallback": function () {
            $("#tbl-inventory .select2").select2();
        },
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keySearch: $('#inventory_search_text').val(),
                    inventoryBasis: $('#inventory-basis-select').val(),
                    maxDayToLast: $('#max-day-to-last').val(),
                    days2Last: $('#filter_daterange').val(),
                    dayToLastOperator: $("#day-to-last-basis-select").val(),
                    locationId: $("#inventory-location").val(),
                    hasSymbol: false
                });
            }
        },
        "columns": columns
    });
}

function ShowIngredientsItemAssociated(id) {
    var locationId = $("#inventory-location").val();
    var ajaxUri = '/BusinessReports/ShowInventoryAssociatedIngredientsItem?itemId=' + id + '&locationId=' + locationId;
    AjaxElementShowModal(ajaxUri, 'inventory-associated-items-view');
};

function UpdateInventoryUnit(itemId) {
    var newUnitId = $("#" + itemId + "-unit").val();
    //var keySearch = $('#inventory_search_text').val();
    var inventoryBasis = $('#inventory-basis-select').val();
    var maxDayToLast = $('#max-day-to-last').val();
    var days2Last = $('#filter_daterange').val();
    var dayToLastOperator = $("#day-to-last-basis-select").val();
    var locationId = $("#inventory-location").val();

    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/BusinessReports/UpdateInventoryUnit',
        data: {
            unitId: newUnitId,
            itemId: itemId,
            inventoryBasis: inventoryBasis,
            maxDayToLast: maxDayToLast,
            days2Last: days2Last,
            dayToLastOperator: dayToLastOperator,
            locationId: locationId,
            hasSymbol: false
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                var $id = response.Object.Id;
                $("#row-unit-item-" + $id).text(response.Object.Unit);
                $("#row-current-inventory-" + $id).text(response.Object.CurrentInventory);
                $("#row-day-to-last-" + $id).text(response.Object.DaysToLast);
                $("#row-inventory-total-" + $id).text(response.Object.InventoryTotal);

                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error("Have an error, detail: " + response.msg, "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function ShowInventoryItemDetailModal(unitId) {
    var $inventoryItemModalElm = $("#inventory-report-item");
    var _url = "/BusinessReports/GetInventoryReportItemDetailModal/";

    var inventoryBasis = $('#inventory-basis-select').val();
    var maxDayToLast = $('#max-day-to-last').val();
    var days2Last = $('#filter_daterange').val();
    var dayToLastOperator = $("#day-to-last-basis-select").val();
    var locationId = $("#inventory-location").val();

    var data = {
        "unitId": unitId,
        "locationId": locationId,
        "inventoryBasis": inventoryBasis,
        "maxDayToLast": maxDayToLast,
        "days2Last": days2Last,
        "dayToLastOperator": dayToLastOperator
    }

    $inventoryItemModalElm.load(_url, data, function () {
        $inventoryItemModalElm.modal('show');        
    });
}

function ShowTopSell(modalId) {
    var _paramaters = getParamaters('#sales', modalId);

    $("#" + modalId).load("/TraderSales/ShowSaleDashboardReportDetail?modalId=" + modalId, _paramaters, function () {
        $("#" + modalId).removeClass().addClass('modal left fade in');
        $("#" + modalId).show();

        document.body.style.overflow = "hidden";
        $modalContent.addEventListener("scroll", preventScroll);
    });
}

function CloseTopSell(modalId) {
    $("#" + modalId).removeClass();
    $("#" + modalId).hide()

    $modal.style.display = "none";
    document.body.style.overflow = "auto";
    $modalContent.removeEventListener("scroll", preventScroll);
}

function preventScroll(event) {
    event.preventDefault();
}