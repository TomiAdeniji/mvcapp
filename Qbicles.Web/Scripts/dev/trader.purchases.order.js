function DownloadPurchaseOrder(id) {
    $.LoadingOverlay("show");
    var fileName = 'Purchase-Order-' + id + '.pdf';

    $.ajax({
        type: 'post',
        url: '/TraderPurchases/DownloadFile',
        datatype: 'json',
        data: { saleOrderId :id},
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

//function IssuePurchaseOrder(purchaseOrderId) {
//    $.LoadingOverlay("show");
//    $.ajax({
//        url: "/TraderPurchases/IssuePurchaseOrder",
//        type: "POST",
//        dataType: "json",
//        data: { id: purchaseOrderId},
//        success: function (rs) {
//            if (rs.result) {
//                document.getElementById("download-1").style.cursor = "pointer";
//                document.getElementById("download-2").disabled = false;
//                $("#download-3").removeClass("disabled");
//                $("#purchase-order-issue").text("Yes");
//                //$("#purchase-order-guid").val(rs.msg);
//                LoadingOverlayEnd();
//                cleanBookNotification.updateSuccess();
//            }
//        },
//        error: function (err) {
//            LoadingOverlayEnd(); cleanBookNotification.error(err.responseText, "Qbicles");
//            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
//        }
//    });
//};

function IssuePurchaseOrder(purchaseOrderId) {
    var emails = $("#mail-addresses").val();
    if (emails.length > 0) {
        var invalid = validateEmails(emails);
        if (invalid.length > 0) {
            cleanBookNotification.error("Email format is not correct: '\n'" + invalid, "Qbicles");

            return;
        }
    }
    $('#adding-other-mail-addresses').LoadingOverlay("show");
    $.ajax({
        url: "/TraderPurchases/IssuePurchaseOrder",
        type: "POST",
        dataType: "json",
        data: {
            id: purchaseOrderId,
            emails: emails
        },
        success: function (rs) {
            if (rs.result) {
                document.getElementById("download-1").style.cursor = "pointer";
                document.getElementById("download-2").disabled = false;
                $("#download-3").removeClass("disabled");
                $("#sale-order-issue").text("Yes");
                //$("#sale-order-guid").val(rs.msg);
                cleanBookNotification.updateSuccess();

                $("#adding-other-mail-addresses").modal('hide');
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            cleanBookNotification.error("Issue error: " + err, "Qbicles");
        }
    }).always(function () {
        $('#adding-other-mail-addresses').LoadingOverlay("hide");
    });
};


function OpenIssuePurchaseOrderModal() {
    $("#mail-addresses").text('');
    $("#mail-addresses").val('');
    $("#adding-other-mail-addresses").modal('show');
};