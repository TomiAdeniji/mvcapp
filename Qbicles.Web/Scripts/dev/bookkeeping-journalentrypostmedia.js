var $qbiclesFileTypes = [];
var $traderJournalDetailBkTransactionAttachments = [];
var $traderJournalDetailBkTransactionAttachmentExisted = {
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





var $tranComments = $("#comments-view"),
    $tranAttachments = $("#attachments-view");
function ShowTransactionComment(id) {

    $.LoadingOverlay("show");
    $('#transaction-attachments').empty();
    var ajaxUri = '/BKJournalEntries/ShowTransactionComment?id=' + id;
    $('#transaction-comments-view').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $tranComments.modal('toggle');
    });

}

function ShowTransactionAttachments(id) {

    $.LoadingOverlay("show");
    $traderJournalDetailBkTransactionAttachments = [];
    $traderJournalDetailBkTransactionAttachmentExisted = {
        AssociatedFiles: []
    };

    $('#transaction-comments-view').empty();
    var ajaxUri = '/BKJournalEntries/ShowTransactionAttachments?id=' + id;
    $('#transaction-attachments').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $tranAttachments.modal('toggle');
    });
}




function JournalEntryDetailAddAnother(transactionId) {
    var idFile = GenerateUUID();

    var inputFiles = $(".attachments_" + transactionId + " input.inputfile");
    var attInput = "<div id='manage-id-" + idFile + "' class=\"row attachment_row\"> <div class=\"col-xs-12\">";
    attInput += "<div class=\"form-group\"> <label for=\"name\">Name</label> <input type=\"text\" name=\"name\" class=\"form-control inputfilename" + (inputFiles.length + 1) + "\">";
    attInput += "</div> </div> <div class=\"col-xs-12\"> <div class=\"form-group\"> <label for=\"file\">File</label>";
    attInput += "<input type=\"file\" name=\"file\" onchange=\"JournalEntryDetailChangeFile(this," + (inputFiles.length + 1) + "," + transactionId + ")\" class=\"form-control inputfile\">  </div>  </div> </div>";
    $(".attachments_" + transactionId + " div.repeater_wrap").append(attInput);
};

function JournalEntryDetailChangeFile(evt, index, indexform) {
    var fileName = $(".attachments_" + indexform + " input.inputfilename" + index).val();
    if ($(evt).length > 0 && $(evt)[0].files.length > 0 && fileName.lenght == 0)
        fileName = $(evt)[0].files[0].name;
    var files = $(".attachments_" + indexform + " input.inputfile");
    for (var i = 0; i < files.length; i++) {
        if (files[0] !== evt && $(files[0]).val() == $(evt).val()) {
            $(evt).val('');
            cleanBookNotification.error(_L("ERROR_MSG_351", [fileName]), "Qbicles");
            fileName = '';
        }
    }
    $('.attachments_' + indexform + ' .inputfilename' + index).val(fileName);
};





function JournalEntryDetailConfirmAddViewer(transactionId, addToview = false) {

    $traderJournalDetailBkTransactionAttachmentExisted = {
        AssociatedFiles: []
    };
    $traderJournalDetailBkTransactionAttachments = [];

    var inputFiles = $(".attachments_" + transactionId + " input.inputfile");
    for (var i = 0; i < inputFiles.length; i++) {
        // check input file name and set file name
        if ($(".attachments_" + transactionId + " .inputfilename" + (i + 1)).val() == "" && inputFiles[i].files.length > 0)
            $(".attachments_" + transactionId + " .inputfilename" + (i + 1)).val(inputFiles[i].files[0].name.substr(0, inputFiles[i].files[0].name.lastIndexOf('.')));
        if (inputFiles[i].files.length > 0) {

            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if ($("div.attachments_" + transactionId + " .inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("div.attachments_" + transactionId + " .inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $traderJournalDetailBkTransactionAttachments.push(attachmentAdd);
        }

        else if ($("#fileid-" + transactionId + "-" + (i + 1)).length > 0) {
            $traderJournalDetailBkTransactionAttachmentExisted.AssociatedFiles.push(
                {
                    Id: parseInt($("#fileid-" + transactionId + "-" + (i + 1)).val()),
                    Name: $(".inputfilename" + (i + 1)).val(),
                    IconPath: $("#inputiconpath_edit" + (i + 1)).val()
                });

        }
    }
    if (addToview)
        JournalEntryDetailLoadAttachmentListViewer();

}

function JournalEntryDetailLoadAttachmentListViewer() {

    $("#transaction-attachments ul.domain-change-list").empty();

    _.forEach($traderJournalDetailBkTransactionAttachmentExisted.AssociatedFiles, function (file) {

        var li = " <li> <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });

    _.forEach($traderJournalDetailBkTransactionAttachments, function (file) {

        var li = " <li> <a href='javascript:void(0)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("#transaction-attachments ul.domain-change-list").append(li);
    });


}

function ProcessBKTransactionMedia(transactionId) {
    $.LoadingOverlay("show");
    JournalEntryDetailConfirmAddViewer(transactionId, false);

    if ($traderJournalDetailBkTransactionAttachments.length > 0) {
        UploadBatchMediasS3ClientSide($traderJournalDetailBkTransactionAttachments).then(function () {
            SubmitBKTransactionMedia(transactionId);
        });
    }
    else {
        SubmitBKTransactionMedia(transactionId);
    }
}


function SubmitBKTransactionMedia(id) {

    if ($traderJournalDetailBkTransactionAttachmentExisted.length === 0 && $traderJournalDetailBkTransactionAttachments.length === 0) {
        LoadingOverlayEnd();
        return;
    }

    var files = [];
    _.forEach($traderJournalDetailBkTransactionAttachments, function (file) {
        file.File = {};
        files.push(file);
    });


    $.ajax({
        type: 'post',
        url: '/BKJournalEntries/SaveBKTransactionMedia?id=' + id,
        data: {
            bKTransactionAssociatedFiles: $traderJournalDetailBkTransactionAttachmentExisted,
            bkTransactionAttachments: files
        },
        dataType: 'json',
        success: function (refModel) {
            $tranAttachments.modal('toggle');
            if (refModel.result) {
                cleanBookNotification.updateSuccess();
                $("#attachment-" + id).text($traderJournalDetailBkTransactionAttachmentExisted.AssociatedFiles.length + $traderJournalDetailBkTransactionAttachments.length);
            }
            else
                cleanBookNotification.error(refModel.msg, "Qbicles");
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_56"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function ShowBkTransactionDetail(id) {
    var ajaxUri = '/Bookkeeping/ShowBkTransactionDetail?id=' + id;
    AjaxElementShowModal(ajaxUri, 'je-more');
}