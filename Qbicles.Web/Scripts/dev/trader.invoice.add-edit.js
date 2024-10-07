
var $qbiclesFileTypes = [];
var $traderInvoiceAttachments = [];
var $traderInvoiceAttachmentExisted = {
    AssociatedFiles: []
};

$(document).ready(function () {
    $qbiclesFileTypes = [];
    $.ajax({
        type: 'post',
        url: '/FileTypes/GetFileTypes',
        dataType: 'json',
        success: function (response) {
            $qbiclesFileTypes = response;
        },
        error: function (er) {
            
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
});

InvoicepaymentMediaBind = function (index) {
    $traderInvoiceAttachmentExisted = {
        AssociatedFiles: []
    };
    $traderInvoiceAttachments = [];

    var inputFiles = $(".attachments_" + index + " input.inputfile");

    for (var i = 0; i < inputFiles.length; i++) {
     
        // check input file name and set file name
        if ($(".attachments_" + index + " .inputfilename" + (i + 1)).val() === "" && inputFiles[i].files.length > 0)
            $(".attachments_" + index + " .inputfilename" + (i + 1)).val(inputFiles[i].files[0].name.substr(0, inputFiles[i].files[0].name.lastIndexOf('.')));

        var fielId = $(".file-id-input-" + (i + 1)).val();

        if (inputFiles[i].files.length > 0) {
            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: fielId,//GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if ($("div.attachments_" + index + " .inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("div.attachments_" + index + " .inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $traderInvoiceAttachments.push(attachmentAdd);
        }
        else {
            //edit bull attachment
            if ($("#file_id_" + (i + 1)).length > 0) {
                $traderInvoiceAttachmentExisted.AssociatedFiles.push(
                    {
                        Id: parseInt($("#file_id_" + (i + 1)).val()),
                        Name: $(".inputfilename" + (i + 1)).val(),
                        IconPath: $("#inputiconpath_edit" + (i + 1)).val()
                    });
            }
        }
    }
}


function InvoicePaymentAddAnother(index) {
    var idFile = GenerateUUID();
   
    var inputFiles = $(".attachments_" + index + " input.inputfile");
    var attInput = "<div id='manage-id-" + idFile + "' class=\"row attachment_row att-id-" + idFile + "\"> <div class=\"col-xs-12\">";
    attInput += "<input type=\"hidden\" class=\"file-id-input-" + (inputFiles.length + 1) + "\" value=\"" + idFile + "\"/>";
    attInput += "<div class=\"form-group\"> <label for=\"name\">Name</label> <input type=\"text\" name=\"name\" class=\"form-control inputfilename" + (inputFiles.length + 1) + "\">";
    attInput += "</div> </div> <div class=\"col-xs-12\"> <div class=\"form-group\"> <label for=\"file\">File</label>";
    attInput += "<input type=\"file\" name=\"file\" onchange=\"InvoicePaymentChangeFile(this," + (inputFiles.length + 1) + "," + index + ")\" class=\"form-control inputfile\">  </div>  </div> </div>";
    $(".attachments_" + index + " div.repeater_wrap").append(attInput);
};

function InvoicePaymentChangeFile(evt, index, indexform) {
    var fileName = $(".attachments_" + indexform + " input.inputfilename" + index).val();
    if ($(evt).length > 0 && $(evt)[0].files.length > 0 && fileName.length === 0)
        fileName = $(evt)[0].files[0].name.split(".")[0];
    var files = $(".attachments_" + indexform + " input.inputfile");
    for (var i = 0; i < files.length; i++) {
        if (files[0] !== evt && $(files[0]).val() === $(evt).val()) {
            $(evt).val('');
            cleanBookNotification.error(_L("ERROR_MSG_351", [fileName]), "Qbicles");
            fileName = '';
        }
    }
    $('.attachments_' + indexform + ' .inputfilename' + index).val(fileName);
};


function InvoicePaymentConfirmAddViewer(index) {

    InvoicepaymentMediaBind(index);

    $("#invoice-attachments ul.domain-change-list").empty();

    var attachmentCount = $traderInvoiceAttachmentExisted.AssociatedFiles.length + $traderInvoiceAttachments.length;
    $("#trader-invoice-attachment-manage").text(" Attachments (" + attachmentCount + ")");

    if (attachmentCount > 0)
        $("#trader-invoice-attachment-icon").removeClass("fa fa-plus").addClass("fa fa-paperclip");

    _.forEach($traderInvoiceAttachmentExisted.AssociatedFiles, function (file) {
        
        var li = " <li id='att-" + file.Id + "'>" +
            "<button class='btn btn-danger' onclick=\"InvoicePaymentRemoveAttachment('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#invoice-attachments ul.domain-change-list").append(li);
    });

    _.forEach($traderInvoiceAttachments, function (file) {
        
        var li = " <li id='att-" + file.Id + "'>" +
            "<button class='btn btn-danger' onclick=\"InvoicePaymentRemoveAttachment('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#invoice-attachments ul.domain-change-list").append(li);
    });
};



function InvoicePaymentRemoveAttachment(id) {
    $("#att-" + id).remove();
    $("#manage-id-" + id).remove();


    _.remove($traderInvoiceAttachments, function (file) {
        return file.Id == id;
    });
    _.remove($traderInvoiceAttachmentExisted.AssociatedFiles, function (file) {
        return file.Id == id;
    });

    var attachmentCount = $traderInvoiceAttachmentExisted.AssociatedFiles.length + $traderInvoiceAttachments.length;
    $("#trader-invoice-attachment-manage").text(" Attachments (" + attachmentCount + ")");
};


function CloseModalMedia() {
    $("#attachments-view").modal("hide");
}
//status payment
// type sale/purchase/transfer
function SaveInvoicePayment(status, type) {

    if ($("#reference").val() === "") {
        $("#cash-account-payment").validate().showErrors({ reference: "Reference is required." });
        return;
    }

    if ($("#workgroup-select").val() === "" || $("#workgroup-select").val() == null) {
        cleanBookNotification.error("Work group is required", "Qbicles");
        return;
    }

    InvoiceProcessMedia(status, type);
};


InvoiceProcessMedia = function (status, type) {
    if ($traderInvoiceAttachments.length > 0) {

        UploadBatchMediasS3ClientSide($traderInvoiceAttachments).then(function () {
            SubmitInvoicePayment(status, type);
        });
    }
    else {
        SubmitInvoicePayment(status, type);
    }
}

SubmitInvoicePayment = function (status, type) {

    $.LoadingOverlay("show");
    InvoicepaymentMediaBind($paymentId);

    var files = [];
    _.forEach($traderInvoiceAttachments, function (file) {
        file.File = {};
        files.push(file);
    });



    var associatedSale = {};
    var destinationAccount = {};

    var associatedPurchase = {};
    var originatingAccount = {};

    switch (type) {
        case "PaymentIn":
            associatedSale = {
                Id: $("#model-id").val()
            };
            break;
        case "PaymentOut":
            associatedPurchase = {
                Id: $("#model-id").val()
            };
            break;
        default:
    }
    var cashorbank = $("#cashorbank").val();
    if (cashorbank != null) {
        var cash = cashorbank.split("-");
        if (cash[1] === "PaymentIn") {
            destinationAccount = { Id: cash[0] };
        } else if (cash[1] === "PaymentOut") {
            originatingAccount = { Id: cash[0] };
        }
    }
    var invoicePayment = {
        Id: $paymentId,
        Description: $("#description").val(),
        Amount: $("#amount").val(),
        AssociatedSale: associatedSale,
        AssociatedPurchase: associatedPurchase,
        Status: status,
        Workgroup: { Id: $("#workgroup-select").val() },
        AssociatedInvoice: { Id: $("#invoice-id").val() },
        Type: type,
        OriginatingAccount: originatingAccount,
        DestinationAccount: destinationAccount,
        PaymentMethod: { Id: $("#payment-method").val() },
        Reference: $("#reference").val()
    }

    $.ajax({
        type: 'post',
        url: '/TraderPayments/SaveInvoicePayment',
        data: {
            invoicePayment: invoicePayment,
            traderInvoiceAssociatedFiles: $traderInvoiceAttachmentExisted,
            traderInvoiceAttachments: files
        },
        dataType: 'json',
        success: function (response) {
            ReloadInvoicePayment();
            $("#app-trader-invoice-payment").modal('toggle');
            cleanBookNotification.updateSuccess();
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });



};