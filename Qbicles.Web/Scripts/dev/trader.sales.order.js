
function DownloadSaleOrder(id) {
    $.LoadingOverlay("show");

    var fileName = 'Sale-Order-' + id + '.pdf';

    $.ajax({
        type: 'post',
        url: '/TraderSales/DownloadFile',
        datatype: 'json',
        data: { saleOrderId: id },
        success: function (refModel) {
            LoadingOverlayEnd();
            var link = document.createElement("a");
            link.download = fileName;
            link.href = refModel;
            link.target = "_blank";
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

function IssueSaleOrder(saleOrderId) {
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
        url: "/TraderSales/IssueSaleOrder",
        type: "POST",
        dataType: "json",
        data: {
            id: saleOrderId,
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


function OpenIssueSaleOrderModal() {
    $("#mail-addresses").text('');
    $("#mail-addresses").val('');
    $("#adding-other-mail-addresses").modal('show');
};