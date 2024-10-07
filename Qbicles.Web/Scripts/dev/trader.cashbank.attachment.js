
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

// attachment
CloseAddCashBankAttachment = function () {
    $("#attachments-view-payment").modal("hide");
};

function CloseCashBankAttachment() {
    //ConfirmCashBankAttachmentAddViewer(0);
    $("#attachments-view-payment").modal("hide");
}

function AddCashBankAttachmentAnother(index) {
    var idFile = GenerateUUID();

    var inputFiles = $(".attachments_" + index + " input.inputfile");
    var attInput = "<div class=\"row attachment_row att-id-" + idFile + "\"> <div class=\"col-xs-12\">" +
        "<input type=\"hidden\" class=\"file-id-input" + (inputFiles.length + 1) + "\" value=\"" + idFile + "\"/>";
    attInput += "<div class=\"form-group\"> <label for=\"name\">Name</label> <input type=\"text\" name=\"name\" class=\"form-control inputfilename" + (inputFiles.length + 1) + "\">";
    attInput += "</div> </div> <div class=\"col-xs-12\"> <div class=\"form-group\"> <label for=\"file\">File</label>";
    attInput += "<input type=\"file\" name=\"file\" onchange=\"ChangeFileCashBankAttachment(this," + (inputFiles.length + 1) + "," + index + ")\" class=\"form-control inputfile\">  </div>  </div> </div>";
    $(".attachments_" + index + " div.repeater_wrap").append(attInput);
};

function ChangeFileCashBankAttachment(evt, index, indexform) {
    var fileName = $(".attachments_" + indexform + " input.inputfilename" + index).val();
    if ($(evt).length > 0 && $(evt)[0].files.length > 0 && fileName.length == 0)
        fileName = $(evt)[0].files[0].name;
    var files = $(".attachments_" + indexform + " input.inputfile");
    for (var i = 0; i < files.length; i++) {
        if (files[0] !== evt && $(files[0]).val() === $(evt).val()) {
            $(evt).val('');
            cleanBookNotification.error(_L("ERROR_MSG_351", [fileName]), "Qbicles");
            fileName = '';
        }
    }
    $('.attachments_' + indexform + ' .inputfilename' + index).val(fileName);
}

function ConfirmCashBankAttachmentAddViewer(index) {
   
    CashBankAttachmentMediaBind(index);


    $("ul.domain-change-list").empty();
    var attachmentCount = $traderCashBankAttachmentExisted.AssociatedFiles.length + $traderCashBankAttachments.length;
    $("#span-attachment-count").text(" Attachments (" + attachmentCount + ")");
    //$("#trader-CashBank-attachment-manage").text(" Attachments (" + attachmentCount + ")");

    //if (attachmentCount > 0)
    //    $("#trader-CashBank-attachment-icon").removeClass("fa fa-plus").addClass("fa fa-paperclip");


    _.forEach($traderCashBankAttachmentExisted.AssociatedFiles, function (file) {
        var li = " <li id='att-" + file.Id + "'>" +
            "<input class=\"file-id\" type=\"hidden\" value=\"" + file.Id + "\" />" +
            "<button class='btn btn-danger' onclick=\"RemoveCashBankAttachment('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });

    _.forEach($traderCashBankAttachments, function (file) {
        var li = " <li id='att-" + file.Id + "'>" +
            "<input class=\"file-id\" type=\"hidden\" value=\"" + file.Id + "\" />" +
            "<button class='btn btn-danger' onclick=\"RemoveCashBankAttachment('" + file.Id + "')\"><i class='fa fa-trash'></i></button>" +
            " <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });

};


RemoveCashBankAttachment = function (id) {
    $("#att-" + id).remove();
    $(".att-id-" + id).remove();


    _.remove($traderCashBankAttachments, function (file) {
        return file.Id == id;
    });
    _.remove($traderCashBankAttachmentExisted.AssociatedFiles, function (file) {
        return file.Id == id;
    });

    $("#span-attachment-count").text($("#transaction-attachments ul.domain-change-list li").length);
    //var accTTId = $('#transfer_id').val();
    //if (!accTTId) accTTId = $("#cashaccounttransaction_id").val();
    //if (!isNaN(parseInt(id))) {
    //    $.ajax({
    //        type: 'delete',
    //        url: '/TraderCashBank/DeleteFile?fileId=' + id + "&accId=" + accTTId,
    //        contentType: false, // Not to set any content header  
    //        processData: false, // Not to process data  
    //        dataType: 'json',
    //        success: function (response) {
    //            if (response === "OK") {
    //                cleanBookNotification.removeSuccess();
    //                $("#att-" + id).remove();
    //                $(".att-id-" + id).remove();
    //                $("#span-attachment-count").text($("#transaction-attachments ul.domain-change-list li").length);
    //            } else {
    //                cleanBookNotification.removeFail();
    //            }

    //        },
    //        error: function (er) {
    //            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    //        }
    //    });
    //} else {
    //    $("#att-" + id).remove();
    //    $(".att-id-" + id).remove();
    //    $("#span-attachment-count").text($("#transaction-attachments ul.domain-change-list li").length);
    //}
};


CashBankAttachmentMediaBind = function (index) {

     $traderCashBankAttachments = [];
     $traderCashBankAttachmentExisted = {
        AssociatedFiles: []
    };
    var fileExtension = "";

    var inputFiles = $("#attachments-view-payment .attachments_" + index + " input.inputfile");

    for (var i = 0; i < inputFiles.length; i++) {
        // check input file name and set file name
        if ($(".attachments_" + index + " .inputfilename" + (i + 1)).val() === "" && inputFiles[i].files.length > 0)
            $(".attachments_" + index + " .inputfilename" + (i + 1)).val(inputFiles[i].files[0].name.substr(0, inputFiles[i].files[0].name.lastIndexOf('.')));

        if (inputFiles[i].files.length > 0) {
            var inputfileName = $("div.attachments_" + index + " .inputfilename" + (i + 1)).val();
            var inputfileId = $("div.attachments_" + index + " .file-id-input" + (i + 1)).val();

            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: inputfileId,//GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if (inputfileName !== "") {
                attachmentAdd.Name = inputfileName;
            }
            $traderCashBankAttachments.push(attachmentAdd);
        } else {
            //edit bull attachment
            if ($("#file_id_" + (i + 1)).length > 0) {
                $traderCashBankAttachmentExisted.AssociatedFiles.push(
                    {
                        Id: parseInt($("#file_id_" + (i + 1)).val()),
                        Name: $(".inputfilename" + (i + 1)).val(),
                        IconPath: $("#inputiconpath_edit" + (i + 1)).val()
                    });
            }
        }


    }
}

// end attachment