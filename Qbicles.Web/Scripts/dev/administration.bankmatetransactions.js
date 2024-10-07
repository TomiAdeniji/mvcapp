function initBankmatetransTab() {
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $("#administration-content .select2").select2();
    $('.daterange').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('.daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        reloadTblHistoryBankmatetrans();
    });
    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        reloadTblHistoryBankmatetrans();
    });
    $('#tblPendingBankmateTransactions').on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#tblPendingBankmateTransactions').LoadingOverlay("show");
        } else {
            $('#tblPendingBankmateTransactions').LoadingOverlay("hide", true);
        }
    }).DataTable({
        destroy: true,
        serverSide: true,
        responsive: true,
        paging: true,
        searching: false,
        deferLoading: 30,
        order: [[3, "desc"]],
        ajax: {
            "url": "/Bankmate/GetBankmateTransactions",
            "data": function (d) {
                return $.extend({}, d, {
                    "userId": $('#slcreator').val(),
                    "cashAccountId": $('#sldestination').val(),
                    "keyword": $('#txtSearch').val()
                });
            }
        },
        columns: [
                {
                    "data": "Id",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        return '<input type="checkbox" class="trackInput" style="position: relative; top: 0;">';
                    }
                },
                { "data": "Domain", "orderable": true },
                {
                    "data": "CreatorName",
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlCreator = '<a href="#"><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.CreatorUri + '\');"></div>';
                        _htmlCreator += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + fixQuoteCode(data) + '</div>';
                        _htmlCreator += '<div class="clearfix"></div></a>';
                        return _htmlCreator;
                    }
                },
                { "data": "CreateDate", "orderable": true },
                {
                    "data": "Type",
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlType = 'Funds in';
                        if (data == 2)
                            _htmlType = 'Funds out';
                        return _htmlType;
                    }
                },
                {
                    "data": "ExternalAccountName",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlFormTo = '<a href=""><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.ExternalAccountUri + '\');"></div>';
                        _htmlFormTo += '<div class="avatar-name pull-left" style="color: #333; padding-left: 15px;">';
                        _htmlFormTo += '<div style="padding: 5px 0 3px 0; font-weight: 500;">' + fixQuoteCode(data) + '</div>';
                        if (row.ExternalNUBAN && row.ExternalIBAN)
                            _htmlFormTo += row.ExternalNUBAN + ' &nbsp; / &nbsp; ' + row.ExternalIBAN;
                        _htmlFormTo += '</div><div class="clearfix"></div></a>';
                        return _htmlFormTo;
                    }
                },
                { "data": "Amount", "orderable": true },
                {
                    "data": "Attachments",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlAttachments = [];
                        data.forEach(function (element, index, array) {
                            _htmlAttachments.push('<a href="' + element.Value + '">' + fixQuoteCode(element.Key) + '</a>');
                        });
                        return _htmlAttachments.join('<br/>');
                    }
                },
                {
                    "data": "Id",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<div class="btn-group"><button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                        _htmlOptions += 'Actions &nbsp; <i class="fa fa-angle-down"></i></button><ul class="dropdown-menu">';
                        _htmlOptions += '<li><a href="javascript:loadBankmateTransfersModal(' + data + ')">Approve</a></li>';
                        _htmlOptions += '<li><a href="javascript:rejectBankmateTrans(' + data + ')">Reject</a></li>';
                        _htmlOptions += '</ul></div>';
                        return _htmlOptions;
                    }
                }
        ],
        drawCallback: function (settings) {
            $('#tblPendingBankmateTransactions').DataTable().columns.adjust().responsive.recalc();
            $("#tblPendingBankmateTransactions .trackInput").on("change", function () {
                var $elm = $(this);
                var $row = $elm.parents("tr");
                var $table = $('#tblPendingBankmateTransactions').DataTable();
                var rowData = $table.row($row).data();
                if (rowData)
                    rowData.isChecked = $elm.prop('checked');
                var showBulkButton = false;
                var rowsData = $table.rows().data();
                for (var i = 0; i < rowsData.length; i++) {
                    var value = rowsData[i];
                    if (value.isChecked)
                    {
                        showBulkButton = true;
                        break;
                    }
                }
                if (showBulkButton)
                    $('.withselected').fadeIn();
                else
                    $('.withselected').fadeOut();
            });
        }
    });
    $('#tblHistoryBankmateTransactions').on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#tblHistoryBankmateTransactions').LoadingOverlay("show");
        } else {
            $('#tblHistoryBankmateTransactions').LoadingOverlay("hide", true);
        }
    }).DataTable({
        destroy: true,
        serverSide: true,
        responsive: true,
        paging: true,
        searching: false,
        deferLoading: 30,
        order: [[2, "desc"]],
        ajax: {
            "url": "/Bankmate/GetBankmateTransactions",
            "data": function (d) {
                return $.extend({}, d, {
                    "userId": $('#slHistoryCreator').val(),
                    "cashAccountId": $('#slHistoryDestination').val(),
                    "keyword": $('#txthistorySearch').val(),
                    "daterange": $('#txtHistoryDaterange').val(),
                    "isPendingStatus": false
                });
            }
        },
        columns: [
                { "data": "Domain", "orderable": true },
                {
                    "data": "CreatorName",
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlCreator = '<a href="#"><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.CreatorUri + '\');"></div>';
                        _htmlCreator += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + fixQuoteCode(data) + '</div>';
                        _htmlCreator += '<div class="clearfix"></div></a>';
                        return _htmlCreator;
                    }
                },
                { "data": "CreateDate", "orderable": true },
                {
                    "data": "Type",
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlType = 'Funds in';
                        if (data == 2)
                            _htmlType = 'Funds out';
                        return _htmlType;
                    }
                },
                {
                    "data": "ExternalAccountName",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlFormTo = '<a href=""><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.ExternalAccountUri + '\');"></div>';
                        _htmlFormTo += '<div class="avatar-name pull-left" style="color: #333; padding-left: 15px;">';
                        _htmlFormTo += '<div style="padding: 5px 0 3px 0; font-weight: 500;">' + fixQuoteCode(data) + '</div>';
                        if (row.ExternalNUBAN && row.ExternalIBAN)
                            _htmlFormTo += row.ExternalNUBAN + ' &nbsp; / &nbsp; ' + row.ExternalIBAN;
                        _htmlFormTo += '</div><div class="clearfix"></div></a>';
                        return _htmlFormTo;
                    }
                },
                { "data": "Amount", "orderable": true },
                {
                    "data": "Attachments",
                    "orderable": false,
                    "render": function (data, type, row, meta) {
                        var _htmlAttachments = [];
                        data.forEach(function (element, index, array) {
                            _htmlAttachments.push('<a href="#">' + fixQuoteCode(element.Key) + '</a>');
                        });
                        return _htmlAttachments.join('<br/>');
                    }
                },
                {
                    "data": "Status",
                    "orderable": true,
                    "render": function (data, type, row, meta) {
                        var _htmlStatus = '';
                        if (data == 4)
                            _htmlStatus = '<label class="label label-success label-lg">Approved</label>';
                        else
                            _htmlStatus = '<label class="label label-danger label-lg">Denied</label>';
                        return _htmlStatus;
                    }
                },
        ],
        drawCallback: function (settings) {
            $('#tblHistoryBankmateTransactions').DataTable().columns.adjust().responsive.recalc();
        }
    });
}
function reloadTblPendingBankmatetrans() {
    if ($.fn.DataTable.isDataTable('#tblPendingBankmateTransactions')) {
        $('#tblPendingBankmateTransactions').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblPendingBankmateTransactions').DataTable().ajax.reload();

        }, 1000);
    }
}
function reloadTblHistoryBankmatetrans() {
    if ($.fn.DataTable.isDataTable('#tblHistoryBankmateTransactions')) {
        $('#tblHistoryBankmateTransactions').DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $('#tblHistoryBankmateTransactions').DataTable().ajax.reload();

        }, 1000);
    }
}
function initSearch() {
    $('#txtSearch').keyup(delay(function () {
        reloadTblPendingBankmatetrans();
    }, 500));
    $('#slcreator').change(function () {
        reloadTblPendingBankmatetrans();
    });
    $('#sldestination').change(function () {
        reloadTblPendingBankmatetrans();
    });
    $('#txthistorySearch').keyup(delay(function () {
        reloadTblHistoryBankmatetrans();
    }, 500));
    $('#slHistoryCreator').change(function () {
        reloadTblHistoryBankmatetrans();
    });
    $('#slHistoryDestination').change(function () {
        reloadTblHistoryBankmatetrans();
    });
}
function loadBankmateTransfersModal(id) {
    $('#app-mbm-admin-transfers').empty();
    $('#app-mbm-admin-transfers').modal('show');
    $('#app-mbm-admin-transfers').load("/Bankmate/ConfirmBankmateTransferModal?transactionId=" + (id ? id : 0), function () {
        $("#app-mbm-admin-transfers .select2").select2();
    });
}
function confirmApprove(id,idExternalbank,inout) {
    $.LoadingOverlay("show");
    var approveIds = [];
    approveIds.push(id);
    $.post("/Bankmate/ApproveCashBankAccountTransactions", { ids: approveIds, status: 'PaymentApproved', externalBankAccountId: $(idExternalbank).val(), type: inout }, function (response) {
        if (response.result) {
            $('#app-mbm-admin-transfers').modal('hide');
            cleanBookNotification.updateSuccess();
            reloadTblPendingBankmatetrans();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        } else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        $.LoadingOverlay("hide");
    });
}
function rejectBankmateTrans(id) {
    var approveIds = [];
    approveIds.push(id);
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("CONFIRM_MSG_REJECT"),
        callback: function (result) {
            if (result) {
                $.post("/Bankmate/RejectCashBankAccountTransactions", { ids: approveIds, status: 'PaymentDenied' }, function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        reloadTblPendingBankmatetrans();
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Qbicles");
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                });
                return;
            }
        }
    });
}
function onChangeExternalBankInfo(elm) {
    var $elm = $(elm);
    $('#app-mbm-admin-transfers textarea[name=BankAddress]').val($elm.find(':selected').data('address'));
    $('#app-mbm-admin-transfers input[name=PhoneNumber]').val($elm.find(':selected').data('phonenumber'));
    $('#app-mbm-admin-transfers input[name=AccountName]').val($elm.find(':selected').data('accountname'));
    $('#app-mbm-admin-transfers input[name=IBAN]').val($elm.find(':selected').data('iban'));
    $('#app-mbm-admin-transfers input[name=NUBAN]').val($elm.find(':selected').data('nuban'));
}
function bulkApprove() {
    var approveIds = [];
    $('#tblPendingBankmateTransactions').DataTable().rows().every(function (index, element) {
        var row = $(this.node());
        var $table = $('#tblPendingBankmateTransactions').DataTable();
        var rowData = $table.row(row).data();
        var checked = $(row).find("input[type=checkbox]");
        if (rowData.isChecked) {//fix errors not get value when datatable paging
            approveIds.push(rowData.Id);
        }
    });
    if (approveIds.length == 0)
        return;
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("CONFIRM_MSG_BULKAPPROVE"),
        callback: function (result) {
            if (result) {
                $.post("/Bankmate/RejectCashBankAccountTransactions", { ids: approveIds, status: 'PaymentApproved', externalBankAccountId: 0, type: 0 }, function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        reloadTblPendingBankmatetrans();
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Qbicles");
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                });
                return;
            }
        }
    });
}
function bulkReject() {
    var rejectIds = [];
    $('#tblPendingBankmateTransactions').DataTable().rows().every(function (index, element) {
        var row = $(this.node());
        var $table = $('#tblPendingBankmateTransactions').DataTable();
        var rowData = $table.row(row).data();
        if (rowData.isChecked) {//fix errors not get value when datatable paging
            rejectIds.push(rowData.Id);
        }
    });
    if (rejectIds.length == 0)
        return;
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("CONFIRM_MSG_BULKREJECT"),
        callback: function (result) {
            if (result) {
                $.post("/Bankmate/RejectCashBankAccountTransactions", { ids: rejectIds, status: 'PaymentDenied' }, function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        reloadTblPendingBankmatetrans();
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Qbicles");
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                });
                return;
            }
        }
    });
}
$(document).ready(function () {
    initBankmatetransTab();
    reloadTblPendingBankmatetrans();
    initSearch();
});