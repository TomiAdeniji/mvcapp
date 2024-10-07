var $qbiclesFileTypes = [];
var $traderCashBankAttachments = [];
var $traderCashBankAttachmentExisted = {
    AssociatedFiles: []
};

var $cashAccountId = 0;
var BKAccount = {
    Id: 0,
    Name: ""
};

// filter
$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var _dateFormat = $dateFormatByUser.toUpperCase();
        if ($('#date_range').val().toString().trim() === '') return true;
        var min = moment(($('#date_range').val().split('-')[0] + '').trim(), _dateFormat);;
        var max = moment(($('#date_range').val().split('-')[1] + '').trim(), _dateFormat);;
        var startDate = moment((data[1] + '').trim(), _dateFormat);
        if (min == null && max == null) { return true; }
        if (min == null && startDate <= max) { return true; }
        if (max == null && startDate >= min) { return true; }
        if (startDate <= max && startDate >= min) { return true; }
        return false;
    }
);
$('.daterange').on('apply.daterangepicker', function (ev, picker) {
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
    $('#date_range').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
    $('#community-list').DataTable().ajax.reload();
});

function initTable() {
    var domainId = $("#accountdomain_id").val();

    $("#community-list")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#table_transaction').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#table_transaction').LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
            "processing": true,
            "serverSide": true,
            "info": false,
            "stateSave": false,
            "bLengthChange": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "scrollX": false,
            "autoWidth": true,
            "deferLoading": 30,
            "pageLength": 10,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "ajax": {
                "url": '/TraderCashBank/LoadPaymentTransaction',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "search": $("#search_trans").val(),
                        "fromDate": $('#date_range').val() !== "" ? $('#date_range').val().split(" - ")[0] : "",
                        "toDate": $('#date_range').val() !== "" ? $('#date_range').val().split(" - ")[1] : "",
                        "id": $("#tradercashaccount_id").val()
                    });
                }
            },
            "columns": [
                {
                    data: "Id",
                    orderable: true
                },
                {
                    data: "Date",
                    orderable: true
                },
                {
                    data: "Reference",
                    orderable: true
                },
                {
                    data: "Source",
                    orderable: true
                },
                {
                    data: "PaymentMethod",
                    orderable: true
                },
                {
                    data: "Destination",
                    orderable: true
                },
                {
                    data: "Type",
                    orderable: true,
                    render: function (value, type, row) {
                        var str = '<span class="label label-lg label-primary">' + value + '</span>';
                        return str;
                    }
                },
                {
                    data: "InOut",
                    orderable: true
                },
                {
                    data: "For",
                    orderable: true,
                    render: function (value, type, row) {
                        // var str = '<a href="javascript:void(0)">' + value + '</a>';
                        var str = value;
                        return str;
                    }
                },
                {
                    data: "Amount",
                    orderable: true
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (value, type, row) {
                        var str = '';
                        if (value == "Pending Review") {
                            str = '<span class="label label-lg label-info">' + value + '</span>';
                        } else if (value === "Pending Approval") {
                            str = '<span class="label label-lg label-primary">' + value + '</span>';
                        } else if (value === "Approved") {
                            str = '<span class="label label-lg label-success">' + value + '</span>';
                        } else if (value === "Denied") {
                            str = '<span class="label label-lg label-danger">' + value + '</span>';
                        } else if (value === "Draft") {
                            str = '<span class="label label-lg label-primary">' + value + '</span>';
                        } else if (value === "Discarded") {
                            str = '<span class="label label-lg label-danger">' + value + '</span>';
                        }
                        return str;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    width: "250px",
                    render: function (value, type, row) {
                        var str = '';
                        if (row.Type !== "Transfer") {
                            if (row.Status === "Draft") {
                                str = '<button class="btn btn-info" onclick="EditPayment(' + $('#tradercashaccount_id').val() + ',' + row.Id + ')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                            } else {
                                str += str = '<button class="btn btn-primary" onclick="window.location.href=\'/TraderPayments/PaymentManage?id=' + row.Id + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                            }
                        } else {
                            if (row.Status === "Draft") {
                                str = '<button class="btn btn-info" onclick="EditTranfer(' + $('#tradercashaccount_id').val() + ',' + row.Id + ')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                            } else {
                                str += str = '<button class="btn btn-primary" onclick="window.location.href=\'/TraderPayments/PaymentManage?id=' + row.Id + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                            }
                        }
                        return str;
                    }
                },
            ],
            "initComplete": function (settings, json) {
                $('#community-list').DataTable().ajax.reload();
            },
            "order": [[1, "asc"], [2, "asc"], [3, "asc"], [4, "asc"], [5, "asc"], [6, "asc"], [7, "asc"], [8, "asc"], [9, "asc"], [10, "asc"], [11, "asc"]]
        });
}

$('.daterange').on('cancel.daterangepicker', function (ev, picker) {
    $(this).val(null);
    $('#date_range').html('full history');
    $('#community-list').DataTable().ajax.reload();
});
function clearValue() {
    $('#search_invoice').val(null);
    $('#search_date_range').val(null);
}
//end filter
var $locationId = $("#manager_location").val();
// payment
function AddPayment(id) {
    $("#cashbank-transfer").empty();

    var $traderCashBankAttachments = [];
    var $traderCashBankAttachmentExisted = {
        AssociatedFiles: []
    };

    var url = "/TraderCashBank/AddPayment?id=" + id + "&locationid=" + $locationId;
    AjaxElementShowModal(url, "cashbank-payment");
}
function EditPayment(id, paymentId) {
    $("#cashbank-transfer").empty();

    var $traderCashBankAttachments = [];
    var $traderCashBankAttachmentExisted = {
        AssociatedFiles: []
    };

    var url = "/TraderCashBank/AddPayment?id=" + id + "&locationid=" + $locationId + "&transactionId=" + paymentId;
    AjaxElementShowModal(url, "cashbank-payment");
}
function AddTranfer(id) {
    $("#cashbank-payment").empty();

    var $traderCashBankAttachments = [];
    var $traderCashBankAttachmentExisted = {
        AssociatedFiles: []
    };

    var url = "/TraderCashBank/AddTransfer?id=" + id + "&locationid=" + $locationId;
    AjaxElementShowModal(url, "cashbank-transfer");
}
function EditTranfer(id, transferId) {
    $("#cashbank-payment").empty();

    var $traderCashBankAttachments = [];
    var $traderCashBankAttachmentExisted = {
        AssociatedFiles: []
    };

    var url = "/TraderCashBank/AddTransfer?id=" + id + "&locationid=" + $locationId + "&transactionId=" + transferId;
    AjaxElementShowModal(url, "cashbank-transfer");
}
function ShowTransactionAttachments(id) {
    setTimeout(function () {
        $("#attachments-view-payment").modal('toggle');
    }, 500);
};
function paymentforChange(ev) {
    var method = $(ev).val();
    $('#payment-specifics').show();
    if (method == "invoice") {
        $('#contact-selector').hide();
        $('#invoice-selector').fadeIn();
        $('#tblInvoices').DataTable().ajax.reload();
    } else {
        $('#invoice-selector').hide();
        $('#contact-selector').fadeIn();
    }
}
// treeview

//end treeview
// contact
CloseAddContact = function () {
    $("#app-trader-add-contact").modal("hide");
};

// end contact
// Invoice
function selectInvoice(elm) {
    var $row = $(elm).closest("tr");
    var _dataRow = $('#tblInvoices').DataTable().row($row).data();
    $('.invoice-select').hide();
    $('.selected-invoice').show();
    $("#invoice_id").text(_dataRow.Id);
    $("#invoice_date").text(_dataRow.Date);
    $("#invoice_contact").empty();
    let _contactHtml = '<a href="/TraderContact/ContactMaster?key=' + _dataRow.ContactKey + '">' + fixQuoteCode(_dataRow.Contact) + '</a>';
    $("#invoice_contact").append(_contactHtml);
    $("#invoice_amount").text(_dataRow.Amount);
    $("#invoice_paayment").text(_dataRow.Payments);
}
// End invoice

//account treeview
function initBKAccount(id, name) {
    BKAccount.Id = id;
    BKAccount.Name = name;
}
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    $("#accountId").val(id);
    BKAccount.Id = id;
    BKAccount.Name = name;
    $("#accountId").val(id);
    closeSelected();
    $("#app-bookkeeping-treeview").modal("hide");
};
function closeSelected() {
    $("#app-bookkeeping-treeview").modal("hide");
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
function showChangeAccount() {
    setTimeout(function () {
        CollapseAccount();
    },
        1);
};
function CollapseAccount() {
    $(".jstree").jstree("close_all");
};

function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    }, 1);
};
//end treeview account
//save payment
function SaveCashBankPayment(status) {
    if (!validatePayment()) {
        return;
    }
    var paymentType = $("#payment-for").val();

    var paymentTransaction = {
        Id: $("#cashaccounttransaction_id").val(),
        Workgroup: { Id: $("#transfer-workgroup-select").val() },
        Type: $('#payment_income').val() == 'in' ? 'PaymentIn' : 'PaymentOut',
        DestinationAccount: { Id: $('#payment_income').val() == 'in' ? $('#tradercashaccount_id').val() : 0 },
        OriginatingAccount: { Id: $('#payment_income').val() == 'out' ? $('#tradercashaccount_id').val() : 0 },
        Amount: $('#payment_amount').val().replace(/\,/g, ""),
        Description: $('#payment_description').val(),
        Status: status,
        Contact: { Id: $('#payment_contact').val() },
        AssociatedInvoice: { Id: $("#invoice_id").text() },
        PaymentMethod: { Id: $("#payment-method").val() },
        Reference: $("#reference").val()
    };
    if (paymentTransaction.AssociatedInvoice.Id == "" || paymentTransaction.AssociatedInvoice.Id == "0") {
        paymentTransaction.AssociatedInvoice = null;
    }
    if (paymentTransaction.Contact.Id == "" || paymentTransaction.Contact.Id == "0") {
        paymentTransaction.Contact = null;
    }
    if (paymentType !== 'poa') {
        paymentTransaction.Contact = {};
    } else {
        paymentTransaction.AssociatedInvoice = {};
    }
    if (paymentType === 'poa' && !paymentTransaction.Contact) {
        cleanBookNotification.error(_L("ERROR_MSG_619"), "Qbicles");
        return false;
    } else if (paymentType !== 'poa' && !paymentTransaction.AssociatedInvoice) {
        cleanBookNotification.error(_L("ERROR_MSG_620"), "Qbicles");
        return false;
    }
    CashBankPaymentTransferProcessMedia(paymentTransaction, "payment");
}
function validatePayment() {
    var valid = true;
    // check workgroup
    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id == null) {
        $("#form_workgroup").validate().showErrors({ workgroup: "Workgroup is required." });
        valid = false;
    }
    if ($("#payment_income").val() === "") {
        $("#form_content").validate().showErrors({ type: "Type of payment is required." });
        valid = false;
    }
    if ($("#reference").val() === "") {
        $("#form_content").validate().showErrors({ reference: "Reference is required." });
        valid = false;
    }
    if ($('#payment-for').val() === "poa" && (!$('#payment_contact').val() || $('#payment-for').val() === "")) {
        $("#form_content").validate().showErrors({ contact: "Contact is required." });
        valid = false;
    } else {
        $('#payment_contact + label').remove();
    }
    return valid;
}

// end payment
// transfer
function saveTransfer(status) {
    var reference = $("#reference").val();
    if (reference == "" && status.toLowerCase() != 'draft') {
        $("#form-create-transfer").validate().showErrors({ reference: "Reference is required." });
        return;
    }

    var $workgroup = {
        Id: $("#transfer-workgroup-select").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id == null) {
        $("#form_workgroup").validate().showErrors({ workgroup: "Workgroup is required." });
        return;
    }
    var to = $('#transfer_destination').val();
    var accCb = $('#tradercashaccount_id').val();
    var transferTransaction = {
        Id: $("#transfer_id").val(),
        Workgroup: $workgroup,
        Type: $('#transfer_type').val(),
        DestinationAccount: { Id: to },
        OriginatingAccount: { Id: accCb },
        Amount: $('#transfer_amount').val().replace(/\,/g, ""),
        Charges: $('#transfer_charges').val().replace(/\,/g, ""),
        Description: $('#transfer_desciption').val(),
        Status: status,
        Reference: reference
    };

    CashBankPaymentTransferProcessMedia(transferTransaction, "transfer");
}
// end transfer
// workgroup
function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    var id = $("#transfer-workgroup-select").val();
    if (id && id !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + id,
            dataType: "json",
            success: function (response) {
                $.LoadingOverlay("hide");
                if (response.result) {
                    $('#workgroup-location-id').val(response.Object.LocationId);
                    $(".preview-workgroup table tr td.location_name").text(response.Object.Location);
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member span").text(response.Object.Members);
                } else {
                    $('#workgroup-location-id').val(0);
                    $(".preview-workgroup table tr td.location_name").text('');
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member span").text('');
                }
                $('#tblInvoices').DataTable().ajax.reload();
            },
            error: function (er) {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $('#workgroup-location-id').val(0);
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
}
function ShowGroupMember() {
    var url = "/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#transfer-workgroup-select").val();
    AjaxElementLoad(url, "app-trader-workgroup-preview");
};

var isProcessing = false;
CashBankPaymentTransferProcessMedia = function (data, type) {
    $.LoadingOverlay('show');
    if ($traderCashBankAttachments.length > 0) {
        UploadBatchMediasS3ClientSide($traderCashBankAttachments).then(function () {
            SubmitCashBankPaymentTransfer(data, type);
        });
    }
    else {
        SubmitCashBankPaymentTransfer(data, type);
    }
}

SubmitCashBankPaymentTransfer = function (data, type) {
    if (isProcessing)
        return;
    isProcessing = true;
    CashBankAttachmentMediaBind(0);

    var files = [];
    _.forEach($traderCashBankAttachments, function (file) {
        file.File = {};
        files.push(file);
    });

    $.ajax({
        url: '/TraderCashBank/SaveCashAccountPayment',
        data: {
            payment: data,
            traderCashBankAssociatedFiles: $traderCashBankAttachmentExisted,
            traderCashBankAttachments: files
        },
        type: "POST",
        dataType: "json",
        success: function (refModel) {
            if (type == 'transfer') {
                var transferAmount = Number($('#transfer_amount').val().replace(/\,/g, ""));
                var transfeCharges = Number($('#transfer_charges').val().replace(/\,/g, ""));
                var availableAmount = $("#available-amount").val();
                if (data.Status.toLowerCase() != 'draft' && availableAmount < (transferAmount + transfeCharges)) {
                    cleanBookNotification.warning("The total of the Transfer Amount and Charges is larger than the Available amount.");
                }
            } else if (type == 'payment') {
                // Check if amount on creating outgoing payment greater than available amount
                var availableAmount = Number($("#available-amount").val());
                var isOutGoingPayment = $('#payment_income').val() == 'out';
                if (isOutGoingPayment && data.Status == 'PendingReview' && availableAmount < Number(data.Amount)) {
                    cleanBookNotification.warning("The Amount of the outgoing payment is larger than the Available amount.");
                }
            }

            $.LoadingOverlay("hide");
            $("#community-list").DataTable().ajax.reload();
            //if (refModel.result) {
            //    var url = "/TraderCashBank/CashAccountTransactionContents?id=" + $('#tradercashaccount_id').val();
            //    AjaxElementLoad(url, "table_transaction");
            //    if (refModel.actionVal == 1) {
            //        cleanBookNotification.createSuccess();
            //    } else if (refModel.actionVal == 2) {
            //        cleanBookNotification.updateSuccess();
            //    }
            if (type === "transfer")
                $("#cashbank-transfer").modal('hide');
            else
                $("#cashbank-payment").modal('hide');
            //}
            //else
            //    cleanBookNotification.error(refModel.msg, "Qbicles");
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        isProcessing = false;
        LoadingOverlayEnd();
    });
};
//QBIC-3176: Update by Thomas
function SaveCashBankContactAdd() {
    var group = {
        Id: $("#group-contact-id").val()
    };
    var contact = {
        ContactGroup: group,
        Name: $("#contact-name").val(),
        Id: $("#contact-id").val()
    };
    if ($("#form_contact_add").valid()) {
        $.ajax({
            url: "/TraderContact/TraderContactNameCheck",
            data: { contact: contact },
            type: "POST",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result)
                $("#form_contact_add").validate()
                    .showErrors({ Name: _L("ERROR_MSG_251") });
            else {
                SaveCashBankAddPaymentContact();
            }
        }).fail(function () {
            $("#form_contact_add").validate()
                .showErrors({ Name: _L("ERROR_MSG_252") });
        });
    }
}
function SaveCashBankAddPaymentContact() {
    $.LoadingOverlay("show");
    var files = document.getElementById("trader-contact-avatar-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("trader-contact-avatar-upload").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#trader-contact-object-key").val(mediaS3Object.objectKey);
            $("#trader-contact-object-name").val(mediaS3Object.fileName);
            $("#trader-contact-object-size").val(mediaS3Object.fileSize);

            SubmitCashBankAddPaymentContact();
        });
    } else
        SubmitCashBankAddPaymentContact();
};
function SubmitCashBankAddPaymentContact() {
    var frmData = new FormData($("#form_contact_add")[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderContact/SaveTraderContact",
        enctype: "multipart/form-data",
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
        },
        success: function (data) {
            if (data.result) {
                $("#app-trader-add-contact").modal("hide");
                cleanBookNotification.updateSuccess();
                $("#payment-contact-select").append(data.msg);
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};
function initModalAddPayment() {
    WorkGroupSelectedChange();
    $('#cashbank-payment .modal-body .select-modal').select2({
        placeholder: 'Please select'
    });
    var _format = $dateFormatByUser.toUpperCase();
    $('#search_date_range').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: _format
        }
    });
    $("#search_invoice").keyup(delay(function () {
        $('#tblInvoices').DataTable().ajax.reload();
    }, 700));
    $('#search_date_range').change(function () {
        $('#tblInvoices').DataTable().ajax.reload();
    });
    $('#search_date_range').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $(this).change();
    });
    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $(this).change();
    });
    loadDataInvoices();
}
function loadDataInvoices() {
    debugger;
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
        deferLoading: 30,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderCashBank/GetInvoicesInOutByLocation",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = {
                    keyword: $('#search_invoice').val(),
                    isIN: $('#payment_income').val() == 'in' ? true : false,
                    searchDate: $('#search_date_range').val(),
                    locationId: $('#workgroup-location-id').val(),
                };
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "data": "Ref", "searchable": true, "orderable": true },
            { "data": "Date", "searchable": true, "orderable": true },
            {
                "data": "Contact",
                "searchable": false,
                "orderable": true,
                "render": function (data, type, row, meta) {
                    return '<a href="/TraderContact/ContactMaster?key=' + row.ContactKey + '">' + fixQuoteCode(data) + '</a>';
                }
            },
            { "data": "Amount", "searchable": true, "orderable": true },
            { "data": "Payments", "searchable": false, "orderable": false },
            {
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (data, type, row, meta) {
                    return '<button type="button" class="btn btn-success" onclick="selectInvoice(this)"><i class="fa fa-check"></i></button>';
                }
            },
        ],
        initComplete: function (settings, json) {
            $('.invoice-select').show();
        }
    });
}
//End
function checkSendTransferToReviewCondition() {
    var reference = $("#reference").val();
    var transferDestination = $("#transfer_destination").val();
    var workgroup = $("#transfer-workgroup-select").val();
    var amountValue = $("#transfer_amount").val();

    if (reference == "" || transferDestination == "" || workgroup == "" || amountValue == "" || amountValue <= 0) {
        $("#sendToReviewBtn").attr("disabled", "disabled");
    } else {
        $("#sendToReviewBtn").removeAttr("disabled");
    }
}
$(document).ready(function () {
    initTable();
})