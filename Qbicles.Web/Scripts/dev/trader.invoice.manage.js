var $invoiceKey = $("#invoice-key").val();
var $paymentId = 0;

function DownloadInvoice(id) {
    $.LoadingOverlay("show");
    var fileName = 'invoice-' + id + '.pdf';

    $.ajax({
        type: 'post',
        url: '/TraderInvoices/DownloadFile',
        datatype: 'json',
        data: { invoiceId :id},
        success: function (refModel) {
            LoadingOverlayEnd();
            var link = document.createElement("a");
            link.download = fileName;
            link.href = refModel;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        }, error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
};

function IssueInvoice(id) {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/TraderInvoices/IssueInvoice",
        type: "POST",
        dataType: "json",
        data: { id: id},
        success: function (rs) {
            if (rs.result) {
                //$("#invoice-guid").val(rs.msg);
                LoadingOverlayEnd();
                cleanBookNotification.updateSuccess();
                document.getElementById("download-1").style.cursor = "pointer";
                document.getElementById("download-2").disabled = false;
                $("#download-3").removeClass("disabled");
                $("#invoice-issue").text("Yes");
            }
        },
        error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

//invoice payment
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#payment-workgroup-select").val());
};

function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    var id = $("#workgroup-select").val();
    if (id !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + id,
            dataType: "json",
            success: function (response) {
                $.LoadingOverlay("hide");
                if (response.result) {
                    $(".preview-workgroup table tr td.location_name").text(response.Object.Location);
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member span").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.location_name").text('');
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member span").text('');
                }
            },
            error: function (er) {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};



function ReloadInvoicePayment() {
    $("#invoice-payments").empty();
    $("#invoice-payments").load("/TraderPayments/InvoicePaymentContent?key=" + $invoiceKey,
        function () {
            LoadingOverlayEnd();
        });
}

//id : invoice id
function AddInvoicePayment(invoiceId) {
    $.LoadingOverlay("show");
    $("#app-trader-invoice-payment").empty();
    $('#app-trader-invoice-payment').load("/TraderPayments/TraderInvoicePaymentAdd?invoiceId=" + invoiceId,
        function () {
            LoadingOverlayEnd();
            $("#app-trader-invoice-payment").modal("toggle");
        });
    $paymentId = 0;
};


//id: payment id
function EditInvoicePayment(paymentId) {
    $.LoadingOverlay("show");
    $("#app-trader-invoice-payment").empty();
    $('#app-trader-invoice-payment').load("/TraderPayments/TraderInvoicePaymentEdit?paymentId=" + paymentId,
        function () {
            LoadingOverlayEnd();
            $("#app-trader-invoice-payment").modal("toggle");
        });
    $paymentId = paymentId;
};

function ClearInvoicePayment() {
    $("#amount").val(0);
    $("#description").val("");
}

