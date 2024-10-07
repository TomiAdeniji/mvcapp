var $qbiclesFileTypes = [];
var $bkAccountAttachments = [];
var $bkAccountAttachmentExisted = {
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
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
});

var selectedNote = 0;
var lstParent = [];
function BKChartOfAccountContent(id, selected) {
    var ajaxUri = '/Bookkeeping/BKChartOfAccountContent?value=' + id;
    $.LoadingOverlay("show");

    $('#content').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });

}
$('.jstree').on("select_node.jstree", function (evt, data) {
    //$('#content').html("<div class='text-center' style='margin-top: 50px;'><img src='/Content/DesignStyle/img/loading.gif' class='loader'></div>");
    //var type = data.node.data.node;
    var value = data.node.data.value;
    selectedNote = data.node.data;
    if (typeof (selectedNote.parent) === "number") {
        lstParent.push(selectedNote.parent);
    } else if (typeof (selectedNote.parent) === "string") {
        lstParent = selectedNote.parent.split(',');
    }
    BKChartOfAccountContent(value);

    //$('.datatable').dataTable();



    if ($(document).width() < 978) {
        $('html, body').animate({
            scrollTop: $('#content').offset().top - 120
        }, 'slow');
    }

});
var lstNumberExists = [];
function GetNumber() {
    $.ajax({
        type: 'post',
        url: '/Bookkeeping/GetNumber',
        data: { id: selectedNote.value },
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                $('#add_ref_group').text(response.msgId + '.');
                $('#add_ref_account').text(response.msgId + '.');
                $('#add_suggestion_number').val(response.actionVal);
                $('#add_suggestion_number_account').val(response.actionVal);
                lstNumberExists = response.Object;
            } else {
                $('#app-coa-subgroup-add').modal("toggle");
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
// clear error
function BKResetForm(formId) {
    ClearError();
    $("#" + formId)[0].reset();
    ($("#" + formId).validate()).resetForm();
};
var formAddGroup = "#form_group_add", formEditGroup = "#form_group_edit",
    add_group_name = "#addnew_group_name", edit_group_name = "#edit_group_name";
function selectNode(arrayId, loadedContent) {
    $('.jstree').jstree("deselect_all");
    for (var i = 0; i < arrayId.length; i++) {
        if (i === (arrayId.length - 1)) {
            $("div#jstree_id").jstree("select_node", ".groupaccount_" + arrayId[i]);
            if (loadedContent === null) {
                var ajaxUri = '/Bookkeeping/BKChartOfAccountContent?value=' + arrayId[i];
                $.LoadingOverlay("show");
                $('#content').load(ajaxUri, function () {
                    LoadingOverlayEnd();
                });
            }
        } else {
            $("div#jstree_id").jstree("open_node", ".groupaccount_" + arrayId[i]);
        }
    }
}
function addNewSubGroup() {
    var newNumber = $('#add_suggestion_number').val() + "";
    if (newNumber.length === 0) newNumber = "0";
    if (lstNumberExists.indexOf(newNumber) === -1) {
        if ($(formAddGroup).valid()) {
            var number = $('#add_ref_group').text() + newNumber;
            $.ajax({
                type: 'post',
                url: '/Bookkeeping/SaveSubGroup',
                data: { Name: $(add_group_name).val(), Parent: { Id: selectedNote.value }, Number: number },
                dataType: 'json',
                success: function (response) {
                    if (response.result === true) {
                        if (response.actionVal === 1) {
                            cleanBookNotification.createSuccess();
                            var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                            $.LoadingOverlay("show");
                            $('#bkaccount-tree').load(ajaxUri, function () {
                                LoadingOverlayEnd();
                                selectNode(response.Object);
                                $("#app-coa-subgroup-add").modal('hide');
                                $(".modal-backdrop").remove();
                            });
                        }
                        else if (response.actionVal === 3) {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                },
                error: function (er) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    } else {
        cleanBookNotification.error(_L("ERROR_MSG_6"), "Qbicles");
    }
}
function showModalRename() {
    $.ajax({
        type: 'post',
        url: '/Bookkeeping/GetGroupById',
        data: { id: selectedNote.value },
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                lstNumberExists = response.Object2 ? response.Object2 : [];
                var subGroup = response.Object;
                if (subGroup.Number === null) subGroup.Number = '';
                $('#edit_suggestion_number').val(subGroup.Number.split('.')[subGroup.Number.split('.').length - 1]);
                $('#edit_ref_group').text(subGroup.Number.substr(0, subGroup.Number.lastIndexOf('.') + 1));
                $('#edit_group_name').val(subGroup.Name);
                if (response.actionVal === 1)
                    $('#edit_suggestion_number').prop("readonly", true);
                else
                    $('#edit_suggestion_number').prop("readonly", false);
            } else {
                $('#app-coa-subgroup-edit').modal('toggle');
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}
function renameSubGroup() {
    var newNumber = $('#edit_suggestion_number').val() + "";
    if (newNumber.length === 0) newNumber = "0";
    if (lstNumberExists.indexOf(newNumber) === -1) {
        if ($(formEditGroup).valid()) {
            var number = $('#edit_ref_group').text() + newNumber;
            $.ajax({
                type: 'post',
                url: '/Bookkeeping/SaveSubGroup',
                data: { Name: $(edit_group_name).val(), Id: selectedNote.value, Number: number },
                dataType: 'json',
                success: function (response) {
                    if (response.result === true) {
                        if (response.actionVal === 2) {
                            cleanBookNotification.updateSuccess();
                            var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                            $.LoadingOverlay("show");
                            $('#bkaccount-tree').load(ajaxUri, function () {
                                LoadingOverlayEnd();
                                selectNode(response.Object);
                                $("#app-coa-subgroup-edit").modal('toggle');
                                $(".modal-backdrop").remove();
                            });
                        } else if (response.actionVal === 3) {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                },
                error: function (er) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    } else {
        cleanBookNotification.error(_L("ERROR_MSG_6"), "Qbicles");
    }
}

var addAccName = "#add_account_name", addAccNumber = "#add_account_number",
    addAccBalance = "#add_account_balance", addAccDebits = "#add_account_debits", addAccCredits = "#add_account_credits",
    add_account_description = "#add_account_description", add_account_balance = "#add_account_balance", add_account_debit = "#add_account_debits",
    add_account_credit = "#add_account_credits",
    edit_account_Id = "#edit_account_Id", edit_account_name = "#edit_account_name", edit_account_number = "#edit_account_number",
    edit_account_description = "#edit_account_description", edit_account_balance = "#edit_account_balance", edit_account_debit = "#edit_account_debit", edit_account_credit = "#edit_account_credit";

function LoadBKAccountAttachmentList() {

    $("ul.domain-change-list").empty();
    var attachmentCount = $bkAccountAttachmentExisted.AssociatedFiles.length + $bkAccountAttachments.length;
    $("#bk-account-attachment-manage").text(" Attachments (" + attachmentCount + ")");

    if (attachmentCount > 0)
        $("#bk-account-attachment-icon").removeClass("fa fa-plus").addClass("fa fa-paperclip");

    _.forEach($bkAccountAttachmentExisted.AssociatedFiles, function (file) {
        var li = " <li> <a href=\"\">";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("ul.domain-change-list").append(li);
    });

    _.forEach($bkAccountAttachments, function (file) {
        var li = " <li> <a href=\"\">";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("ul.domain-change-list").append(li);
    });


}

function checknumber(e, evt) {
    var keyCode = evt.which;
    /*
      8 - (backspace)
      32 - (space)
      48-57 - (0-9)Numbers
    */
    if ((keyCode !== 8 || keyCode === 32) && (keyCode < 48 || keyCode > 57)) {
        evt.preventDefault();
    }

}
function CreateBKAccount() {
    $.LoadingOverlay("show");
    var newNumber = $('#add_suggestion_number_account').val() + "";
    if (newNumber.length === 0) newNumber = "0";
    if (lstNumberExists.indexOf(newNumber) === -1) {
        if ($("#bk-worgroup-select").val() === "") {
            $.LoadingOverlay("show");
            cleanBookNotification.error(_L("ERROR_MSG_452"), "Qbicles");
            return;
        }
        var number = $('#add_ref_account').text() + newNumber;

        if ($("#form_account_add_edit").valid()) {
            //upload media
            if ($bkAccountAttachments.length > 0) {
                UploadBatchMediasS3ClientSide($bkAccountAttachments).then(function () {
                    ConfirmCreateBkAccount(number);
                });
            }
            else
                ConfirmCreateBkAccount(number);
        } else {
            $.LoadingOverlay("show");
            cleanBookNotification.error(_L("ERROR_MSG_6"), "Qbicles");
        }
    }
}

ConfirmCreateBkAccount = function (number) {

    var files = [];
    _.forEach($bkAccountAttachments, function (file) {
        file.File = {};
        files.push(file);
    });
    var bkAccount = {
        Name: $(addAccName).val(),
        Code: $(addAccNumber).val(),
        Number: number,
        Balance: $(addAccBalance).val(),
        Credit: $(addAccCredits).val(),
        Debit: $(addAccDebits).val(),
        Description: $(add_account_description).val(),
        WorkGroup: { Id: $("#bk-worgroup-select").val() },
        Parent: { Id: selectedNote.value }
    };
    $.ajax({
        type: 'post',
        url: '/Bookkeeping/SaveBKAccount',
        data: {
            bkAccount: bkAccount,
            bkAccountAssociatedFiles: $bkAccountAttachmentExisted,
            bkAccountAttachments: files
        },
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                if (response.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                    var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                    $('#bkaccount-tree').load(ajaxUri, function () {
                        $("#app-coa-account-add").modal('hide');
                        selectNode(response.Object);
                    });
                }
                else if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}



function UpdateBKAccount() {
    var newNumber = $('#edit_suggestion_number_account').val() + "";
    if (newNumber.length === 0) newNumber = "0";
    if (lstNumberExists.indexOf(newNumber) === -1) {

        if ($("#bk-worgroup-select").val() === "") {
            //$('#form-work-group-select').validate().showErrors({ bkworgroupselect: "This field is required." });
            cleanBookNotification.error(_L("ERROR_MSG_452"), "Qbicles");
            return;
        }
        if ($("#form_account_add_edit").valid()) {

            var number = $('#edit_ref_account').text() + newNumber;
            //upload media
            UploadBatchMediasS3ClientSide($bkAccountAttachments).then(function () {

                ConfirmUpdateBKAccount(number);
            });
        }
    } else {
        cleanBookNotification.error(_L("ERROR_MSG_6"), "Qbicles");
    }
}

ConfirmUpdateBKAccount = function (number) {

    $.LoadingOverlay("show");
    var files = [];
    _.forEach($bkAccountAttachments, function (file) {
        file.File = {};
        files.push(file);
    });

    var bkAccount = {
        Id: $(edit_account_Id).val(),
        Name: $(edit_account_name).val(),
        Code: $(edit_account_number).val(),
        Number: number,
        Balance: $(edit_account_balance).val(),
        Credit: $(edit_account_credit).val(),
        Debit: $(edit_account_debit).val(),
        Description: $(edit_account_description).val(),
        WorkGroup: { Id: $("#bk-worgroup-select").val() },
        Parent: { Id: selectedNote.value }
    };
    $.ajax({
        type: 'post',
        url: '/Bookkeeping/SaveBKAccount',
        data: {
            bkAccount: bkAccount,
            bkAccountAssociatedFiles: $bkAccountAttachmentExisted,
            bkAccountAttachments: files
        },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.result === true) {
                if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                    return;
                }

                if (response.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                }
                else if (response.actionVal === 2) {
                    cleanBookNotification.updateSuccess();
                }
                $("#app-coa-account-edit").modal('hide');
                var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                $('#bkaccount-tree').load(ajaxUri, function () {
                    selectNode(response.Object);
                });

            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};




function AddBkAccount() {
    $.LoadingOverlay("show");

    var ajaxUri = '/Bookkeeping/BKAccountAddPartial?id=' + 0;
    $bkAccountAttachmentExisted = {
        AssociatedFiles: []
    };
    $bkAccountAttachments = [];
    $('#app-coa-account-add').empty();
    $('#app-coa-account-add').load(ajaxUri, function () {
        GetNumber();
        LoadingOverlayEnd();
        $("#app-coa-account-add").modal('show');
    });
};

var subgroupId = '';
function DeleteSubNode(id) {
    subgroupId = id;
    $(".subgroup-1-name").text($("#group-name").text());
    $("#confirm-delete-bksubgroup").modal('show');
};

function EditBkAccount(accountId) {
    $.LoadingOverlay("show");

    var ajaxUri = '/Bookkeeping/BKAccountEditPartial?id=' + accountId;
    $bkAccountAttachmentExisted = {
        AssociatedFiles: []
    };
    $bkAccountAttachments = [];

    $('#app-coa-account-edit').empty();
    $('#app-coa-account-edit').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $('#app-coa-account-edit').modal('show');
    });
}
function calcBalance() {
    var _debit = parseFloat($(add_account_debit).val());
    var _credit = parseFloat($(add_account_credit).val());
    var _balance = parseFloat($(add_account_balance).val());
    var _init = ($(add_account_credit).prop('readonly') || $(add_account_balance).prop('readonly') || $(add_account_debit).prop('readonly'));
    if (_debit && _credit && _debit != 0 && _credit != 0 && ($(add_account_balance).prop('readonly') || !_init)) {
        $(add_account_balance).val(_credit - _debit);
        $(add_account_debit).prop('readonly', false);
        $(add_account_credit).prop('readonly', false);
        $(add_account_balance).prop('readonly', true);
    } else if (_balance && _debit && _balance != 0 && _debit != null && ($(add_account_credit).prop('readonly') || !_init)) {
        $(add_account_credit).val(_balance + _debit);
        $(add_account_debit).prop('readonly', false);
        $(add_account_balance).prop('readonly', false);
        $(add_account_credit).prop('readonly', true);
    } else if (_balance && _credit && _balance != 0 && _credit != 0 && ($(add_account_debit).prop('readonly') || !_init)) {
        $(add_account_debit).val(_debit - _balance);
        $(add_account_credit).prop('readonly', false);
        $(add_account_balance).prop('readonly', false);
        $(add_account_debit).prop('readonly', true);
    } else {
        if ($(add_account_debit).prop('readonly'))
            $(add_account_debit).val(0);
        else if ($(add_account_credit).prop('readonly'))
            $(add_account_credit).val(0);
        else if ($(add_account_balance).prop('readonly'))
            $(add_account_balance).val(0);
        $(add_account_debit).prop('readonly', false);
        $(add_account_credit).prop('readonly', false);
        $(add_account_balance).prop('readonly', false);
    }
}
function calcBalanceEdit() {
    var _debit = parseFloat($(edit_account_debit).val());
    var _credit = parseFloat($(edit_account_credit).val());
    var _balance = parseFloat($(edit_account_balance).val());
    var _init = ($(edit_account_credit).prop('readonly') || $(edit_account_balance).prop('readonly') || $(edit_account_debit).prop('readonly'));
    if (_debit && _credit && _debit != 0 && _credit != 0 && ($(edit_account_balance).prop('readonly') || !_init)) {
        $(edit_account_balance).val(_credit - _debit);
        $(edit_account_debit).prop('readonly', false);
        $(edit_account_credit).prop('readonly', false);
        $(edit_account_balance).prop('readonly', true);
    } else if (_balance && _debit && _balance != 0 && _debit != null && ($(edit_account_credit).prop('readonly') || !_init)) {
        $(edit_account_credit).val(_balance + _debit);
        $(edit_account_debit).prop('readonly', false);
        $(edit_account_balance).prop('readonly', false);
        $(edit_account_credit).prop('readonly', true);
    } else if (_balance && _credit && _balance != 0 && _credit != 0 && ($(edit_account_debit).prop('readonly') || !_init)) {
        $(add_account_debit).val(_debit - _balance);
        $(add_account_credit).prop('readonly', false);
        $(add_account_balance).prop('readonly', false);
        $(add_account_debit).prop('readonly', true);
    } else {
        if ($(edit_account_debit).prop('readonly'))
            $(edit_account_debit).val(0);
        else if ($(edit_account_credit).prop('readonly'))
            $(edit_account_credit).val(0);
        else if ($(edit_account_balance).prop('readonly'))
            $(edit_account_balance).val(0);
        $(edit_account_debit).prop('readonly', false);
        $(edit_account_credit).prop('readonly', false);
        $(edit_account_balance).prop('readonly', false);
    }
}
function changeData() {
    var balance = $(add_account_credit).val() - $(add_account_debit).val();
    $(add_account_balance).val(balance);
    if (balance === 0) {
        $(add_account_balance).prop('readonly', false);
    } else {
        $(add_account_balance).prop('readonly', true);
    }
}
function changeDataEdit() {
    var balance = $(edit_account_credit).val() - $(edit_account_debit).val();
    $(edit_account_balance).val(balance);
    if (balance === 0) {
        $(edit_account_balance).prop('readonly', false);
    } else {
        $(edit_account_balance).prop('readonly', true);
    }
}
function changeBalance() {
    var _balance = $(add_account_balance).val();
    if (_balance === "" || _balance === "0") {
        $(add_account_debit).prop('readonly', false);
        $(add_account_credit).prop('readonly', false);
    }
    else {
        $(add_account_debit).val(0);
        $(add_account_credit).val(0);
        $(add_account_debit).prop('readonly', true);
        $(add_account_credit).prop('readonly', true);

    }
}
function changeBalanceEdit() {
    var _balance = $(edit_account_balance).val();
    if (_balance === "" || _balance === "0") {
        $(edit_account_debit).prop('readonly', false);
        $(edit_account_credit).prop('readonly', false);
    }
    else {
        $(edit_account_debit).val(0);
        $(edit_account_credit).val(0);
        $(edit_account_debit).prop('readonly', true);
        $(edit_account_credit).prop('readonly', true);
    }
}
function ConfirmBKAccountAttachment(isEdit, closeId) {

    $bkAccountAttachmentExisted = {
        AssociatedFiles: []
    };
    $bkAccountAttachments = [];
    var fileExtension = "";

    if (!isEdit) {
        var inputFiles = $(".add_attachment_row input.inputfile");
        for (var i = 0; i < inputFiles.length; i++) {

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
            if ($("#inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("#inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $bkAccountAttachments.push(attachmentAdd);

        }
    }
    else {
        var inputFiles2 = $(".edit_attachment_row input.inputfile");
        for (var j = 0; j < inputFiles2.length; j++) {
            if (inputFiles2[j].files.length > 0) {
                var fileEdit = inputFiles2[j].files[0];
                fileExtension = fileEdit.name.split('.').pop();
                var fileTypeEdit = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });//_.filter($qbiclesFileTypes, { 'Extension': fileExtension });
                var attachmentEdit = {
                    Id: GenerateUUID(),
                    Name: fileEdit.name,
                    Extension: fileExtension,
                    Size: fileEdit.size,
                    IconPath: fileTypeEdit.IconPath,
                    File: fileEdit
                };
                if ($("#inputfilename_edit" + (j + 1)).val() !== "") {
                    attachmentEdit.Name = $("#inputfilename_edit" + (j + 1)).val() + "." + fileExtension;
                }
                $bkAccountAttachments.push(attachmentEdit);
            }
            else {
                //edit account attachment
                if ($("#file_id_" + (j + 1)).length > 0) {
                    $bkAccountAttachmentExisted.AssociatedFiles.push(
                        {
                            Id: parseInt($("#file_id_" + (j + 1)).val()),
                            Name: $("#inputfilename_edit" + (j + 1)).val(),
                            IconPath: $("#inputiconpath_edit" + (j + 1)).val()
                        });
                }
            }
        }
    }
    LoadBKAccountAttachmentList();

    if (closeId) {
        $("#" + closeId).modal("toggle");
    }

};


// ----------- workgroup ---------
var $workgroupId = 0;
ChangeBKWorkgroup = function () {
    $workgroupId = $("#bk-worgroup-select").val();
    if ($workgroupId !== "") {
        $(".submit-for-review").empty();
        $.ajax({
            type: "get",
            url: "/Bookkeeping/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                $('.preview-workgroup').show();
                if (response.result) {
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member").text('');
                }
                $(".submit-for-review").empty().append(response.msg);
            },
            error: function (er) {
                $('.preview-workgroup').hide();
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    } else {
        $('.preview-workgroup').hide();
    }
};

//------------ Merge Account ---------------

ShowAccountDetail = function () {
    $.LoadingOverlay("show");
    $('.account-detail-preview').fadeOut();
    $('.account-detail-content').empty();
    document.getElementById("confirm-check").checked = false;
    $('#merge-button').addClass('hidden');
    var accountSelected = $("#merge-account-select").val();
    if (accountSelected === "") {
        LoadingOverlayEnd();
        return;
    }
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/GetAccountAssociatedDetail',
        data: {
            accountId: $("#merge-account-select").val()
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $('.account-detail-content').append(response.msg);
                $('.account-detail-preview').fadeIn();

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
};
OnchangeConfirmMerge = function (event) {
    if (!event.checked)
        $('#merge-button').addClass('hidden');
    else
        $('#merge-button').removeClass('hidden');
};


var $accountMasterId = 0;
MergeAccount = function (accountMasterId) {
    //
    $accountMasterId = accountMasterId;
    $(".account-1-name").text($("#account-name-title").text());
    $.LoadingOverlay("show");
    var ajaxUri = '/Bookkeeping/GetListMergeAccount?accountMasterId=' + accountMasterId;
    $('#merge-account-select-manage').empty();
    $('#merge-account-select-manage').load(ajaxUri, function () {
        $(".account-1-name").text($("#account-name-title").text());
        LoadingOverlayEnd();
        $("#app-coa-account-merge").modal('toggle');
    });



};

MergeAccountNow = function () {
    $(".account-1-name").text($("#account-name-title").text());
    $(".account-2-name").text($("#merge-account-select option:selected").html());
    $("#confirm-merge").modal('toggle');
};



ConfirmMergeAccount = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/BKMergeAccount',
        data: {
            accountMergeId: $("#merge-account-select").val(),
            accountMasterId: $accountMasterId
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                $('#bkaccount-tree').load(ajaxUri, function () {
                    selectNode(response.Object);

                    $("#confirm-merge").modal('toggle');
                    $("#app-coa-account-merge").modal('toggle');
                    cleanBookNotification.updateSuccess();
                });

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
};


var $accountDelId = 0;
DeleteAccount = function (accountId) {
    $accountDelId = accountId;
    $(".account-1-name").text($("#account-name-title").text());
    $("#confirm-delete").modal('show');
};


ConfirmDeleteAccount = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/BKDeleteAccount',
        data: {
            accountId: $accountDelId
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                $('#bkaccount-tree').load(ajaxUri, function () {
                    selectNode(response.Object);
                    $("#confirm-delete").modal('hide');
                    cleanBookNotification.removeSuccess();
                });
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

ConfirmDeleteGroup = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/BKDeleteSubGroup',
        data: {
            groupId: subgroupId
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                var ajaxUri = '/Bookkeeping/TreeViewGroupChartPartial?callback=' + true;
                $('#bkaccount-tree').load(ajaxUri, function () {
                    if (response.Object)
                        selectNode(response.Object);
                    $("#confirm-delete-bksubgroup").modal('hide');
                    cleanBookNotification.removeSuccess();
                });
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
};

var regExp = /([a-zA-Z0-9]+\-?\|?\.?\¦?\\?\/?)+/;
function VerifyAccountNumber(e, event) {
    if (!regExp.test(event.key)) {
        if (event.keyCode !== 46 &&
            event.keyCode !== 45 &&
            event.keyCode !== 124 &&
            event.keyCode !== 92 &&
            event.keyCode !== 47) {
            event.preventDefault();
        }
    }
};


CloseBook = function () {
    var ajaxUri = '/Bookkeeping/CloseBook';
    AjaxElementShowModal(ajaxUri, 'app-coa-account-close');
};
CloseBookConfirm = function () {
    $("#app-coa-account-close").modal('hide');
    //$("#close-date-confirm").text($("#close-date-select").text());
    $("#app-coa-account-close-confirm").modal('show');
};

CloseBookSave = function () {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/CloseBookSave',
        data: {
            closureDate: $("#close-date-confirm").text()
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $("#close-date-home").text($("#close-date-confirm").text());
                $("#app-coa-account-close-confirm").modal('hide');
                cleanBookNotification.success();
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
};