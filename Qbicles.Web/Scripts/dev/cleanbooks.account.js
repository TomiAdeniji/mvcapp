var $selectFilter = $("#filter-group"), $selectOrder = $("#filter-order"),
    $select_user_role = $("#select_user_role"),
    $button_add_group = $("#button_add_group"),
    $button_add_account = $("button[name='button_add_account']"),
    $trDelete = null;


var $modal_group = $("#modal_group"),
    $modal_group_title = $("#modal_group [class='modal-title']"),
    $input_group_id = $("#input_group_id"),
    $input_group_name = $("#input_rectaskgroup_name"),
    $input_group_createddate = $("#input_group_createddate"),
    $input_group_createdbyid = $("#input_group_createdbyid");

var $input_account_id = $("#input_account_id"),
    $input_account_name = $("#input_account_name"),
    $input_account_number = $("#input_account_number"),
    $input_account_isactive = $("#input_account_isactive"),
    $input_account_createddate = $("#input_account_createddate"),
    $input_account_createdbyid = $("#input_account_createdbyid"),
    $input_account_lastbalance = $("#input_account_lastbalance"),
    $select_UpdateFrequency_type = $("#select_Frequency_type"),
    $select_datamanager_type = $("#select_datamanager_type"),
    $select_account_group = $("#select_account_group"),
    $modal_account_remove = $("#modal-delete-account"),
    $modal_account = $("#modal_account"),
    $createByDiv = $("#create-by-div"),
    $createDateDiv = $("#create-date-div"),
    $createdBy = $("#CreatedByName"),
    $createDate = $("#CreatedDateValue"),
    $modal_account_title = $("#modal_account [class='modal-title']"),
    $accountEditId = 0,
    $accountGroupId = 0;

var $modal_transaction_edit = $("#modal_transaction_edit"),
    $modal_transaction_preview = $("#modal_transaction_preview");

var
    $modal_transaction_preview_title = $("#modal_transaction_preview [class='modal-title']"),
    $button_transaction_preview = $("a[name='button_transaction_preview']"),
    $button_transaction_delete = $("a[name='button_transaction_delete']"),
    $modal_upload_remove = $("#modal-delete"),
    $stepv3edit_tableNew = $("#tabletransactionNew");
$stepv4display_table = $("#tableConfirmDisplay");
$stepv4AnalysedData_table = $("#tableAnalysedData");
$ReadSheetData = $("#ReadSheetData"),
    $DefineColumnsData = $("#DefineColumnsData"),
    $buttonProceed4 = $("#buttonProceed4"),
    $buttoncheckscan = $("#buttoncheckscan"),
    $lblcheckfilefeedback = $("#lblcheckfilefeedback"),
    $select_sheetnameFile = $("#select_sheetnameFile"),
    $tablePreview = $("#tablePreview")
$input_transaction_upload = $("#input_transaction_upload"),
    $dlsheetname = $("#dlsheetname"),
    $select_upload_accountid = 0,
    $tabledataCofirm = $("#tabledataCofirm"),
    $accountIdSelected = $("#accountIdSelected"),
    $accountLastBalanceSelected = 0;
$lblfromtodate = $("#lblfromtodate"),
    $label_upload_name = $("#label_upload_name"),
    $selectedAccountId = 0,
    $selectedGroupAccountId = 0,
    $dateFromatSelected = "",
    _headersSelected = [],
    $uploadIdDelete = 0
    $uploadNameDelete = "",

    $uploadFormatId = 0,
    $accountName = "";


var $accountIdDelete = 0,
    $accountNameDelete = "",
    $accountNumberDelete = "",
    $accountCreateDate = null,
    $accountCreateBy = "",
    $accountDataManage = "",
    $liDelete = null;


var CheckExistsAccount = false;
var colCount = 0; // count number columns
var colHeadIndex = 1;

function format(item, container) {

    var originalOption = item.element;
    var originalText = item.text;
    return $('<div title="' + originalText + " " + $(originalOption).data('mytxt') + '">' + originalText + ' ' + '<span class="newTxt">' + $(originalOption).data('mytxt') + '</span></div>');
}
function resetTitlle() {

    $('#alertConfigureColumns1').empty().append("<p>Below you will find the data from your imported file. Above each column is a " +
        "select box which you can use to specify what each column is in your import. If " +
        "a column isn't required, simply select <strong><i>ignore</i></strong></p></br>");
    //$('select').not('.multi-select').select2();
}

// validation data on columns selected
function configureColumns(colIndex) {
    cleanBookNotification.clearmessage();
    $DefineColumnsData.attr('disabled', '');
    // validation data date, number success then = true
    var validColValue = false;
    // validation selected minimum columns require
    var validColSelected = false;
    //[1- get selected columns]
    _headersSelected = [];
    var _headersName = [];
    $("#tabletransactionNew  tbody  tr").find('.columnSelected').each(function (i, selected) {

        selectedVal = $(this).val().replace(' ', '');
        _headersSelected[i] = selectedVal;
        if (selectedVal.toString().startsWith('Ignore')) {
            _headersName[i] = selectedVal;
        }
        else {
            var pos = selectedVal.lastIndexOf('_');
            _headersName[i] = (pos <= 0) ? '' : selectedVal.substring(pos, 0);
        }
    });
    //[2] validate duplicate columns selected
    var duplicates = _headersName.reduce(function (acc, el, i, arr) {
        if (arr.indexOf(el) !== i && acc.indexOf(el) < 0)
            acc.push('It is not possible to select two <strong>' + el + '</strong> columns. Please change your selection.</br>');
        return acc;
    }, []);

    var _headercol = [];
    var _valcookie = getCookie('headercol');
    if (_valcookie)
        _headercol = _valcookie.replace(" ", "").split(',');
    var _status = true;
    var _status_same = false;
    if (duplicates.length === 0) {
        if (_headercol.length > 0 && _headersName.length === 0) {
            _status = true;
        }
        else
            for (var x = 0; x < _headercol.length; x++) {
                _status_same = false;
                for (var y = 0; y < _headersName.length; y++) {
                    if (_headercol[x].replace(" ", "") === _headersName[y].replace(" ", "")) {
                        _status_same = true;
                        break;
                    }
                }
                if (!_status_same) {
                    _status = false;
                    break;
                }
            }

        if (_status)
            $DefineColumnsData.removeAttr('disabled', '');
        else
            $DefineColumnsData.attr('disabled', '');
    }
    else
        $DefineColumnsData.attr('disabled', '');
    if (duplicates.length > 0) {
        cleanBookNotification.warning($.unique(duplicates), 'CleanBooks');
        return;
    }
}

$(document).ready(function () {
    //upload
    $modal_transaction_edit.on('hidden.bs.modal', function () {
        //$('select').not('.multi-select').select2();
    });
    $("#opening_balance").number(true, 2);
    //validation if select file upload is null
    $("input[type=file]").bind("change", function () {
        $lblcheckfilefeedback.text("");
        var selected_file_name = $(this).val();
        if (selected_file_name.length > 0) {
            $buttoncheckscan.removeAttr('disabled', '');
        }
        else {
            $ReadSheetData.attr('disabled', '');
            $buttoncheckscan.attr('disabled', '');
            RemoveSheetSelect();
            $dlsheetname.hide();
        }
    });


    //end upload
    //$select_user_role.on("select2:unselecting",
    //    function (e) {
    //        if (e.params.args.data.id === 'Date' || e.params.args.data.id === 'Description')
    //            e.preventDefault();
    //    });
    LoadAccounts('init');

    $selectFilter.change(function () {
        $("#account-page-display").empty();
        $("#account-page-display").html("<div id='account-content'></div>");

        LoadAccounts('');
    });

    $selectOrder.change(function () {
        $("#account-page-display").empty();
        $("#account-page-display").html("<div id='account-content'></div>");

        LoadAccounts('');
    });

    $button_add_group.bind('click',
        function (event) {
            if (this.classList.contains('isDisabled')) {
                event.preventDefault();
                return;
            }
            ClearError();
            $input_group_id.val(0);
            $input_group_name.val("");
            $input_group_createddate.val("");
            $input_group_createdbyid.val("");
            $modal_group_title.text("Add a Group");
            $("#save-group").text("Add now");
            $modal_group.modal('toggle');
        });

    $(".fileinput").on("change.bs.fileinput", function () {
        $dlsheetname.hide();
        $ReadSheetData.attr('disabled', '');
        $DefineColumnsData.attr('disabled', '');
        $lblcheckfilefeedback.text("");
    });
});

// Account Group
$("#form_group").submit(function (e) {
    e.preventDefault();
    $.ajax({
        type: this.method,
        cache: false,
        url: this.action,
        enctype: 'multipart/form-data',
        data: new FormData(this),
        processData: false,
        contentType: false,
        success: function (refModel) {
            if (refModel.result) {
                $modal_group.modal('toggle');
                if (refModel.actionVal === 1) {
                    $("#list_view").prepend(refModel.Object.toString());
                    $("#grid_view").prepend(refModel.msg);
                    $selectFilter.append("<option value='" + refModel.msgId + "'>" + refModel.msgName + "</option>");
                    $select_account_group.prepend("<option value='" + refModel.msgId + "'>" + refModel.msgName + "</option>");
                    cleanBookNotification.success(_L("ERROR_MSG_467", [$input_group_name.val()]), "Qbicles");
                }
                else if (refModel.actionVal === 2) {
                    $("#account-group-name-grid-" + $input_group_id.val()).text(refModel.msg);
                    $("#account-group-name-list-" + $input_group_id.val()).text(refModel.msg);

                    $('#select_account_group [value="' + refModel.msgId + '"]').text(refModel.msgName);
                    $('#filter-group [value="' + refModel.msgId + '"]').text(refModel.msgName);

                    cleanBookNotification.success(_L("ERROR_MSG_468", [$input_group_name.val()]), "Qbicles");
                }
            }
            else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });
});
function SaveGroup() {
    Save_Group();
}
function Save_Group() {
    if ($("#form_group").valid()) {
        $.ajax({
            url: "/ManageAccounts/DuplicateAccountGroup",
            data: {
                Id: $input_group_id.val(),
                Name: $input_group_name.val()
            },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel)
                $("#form_group").validate().showErrors({
                    Name: "Account Group name of '" + $input_group_name.val() + "' already exists."
                });
            else {
                $("#form_group").trigger("submit");
            }
        }).fail(function () {
            $("#form_group").validate()
                .showErrors({ Name: "Error checking existing name of group in the current Domain!" });
        });
    }
}
function EditGroup(groupId, controlId) {

    if ($("#" + controlId).hasClass('isDisabled')) {
        return;
    }
    ClearError();
    if (groupId && groupId > 0) {
        $.ajax({
            type: 'GET',
            url: "/ManageAccounts/GetAccountGroup",
            datatype: 'json',
            data: { id: groupId },
            success: function (refModel) {
                if (refModel.Id) {
                    $("#form_group").validate().resetForm();
                    $input_group_id.val(refModel.Id);
                    $input_group_name.val(refModel.Name);
                    $input_group_createddate.val(refModel.CreatedDate
                        ? new Date(parseInt(refModel.CreatedDate.substr(6))).toJSON()
                        : null);
                    $input_group_createdbyid.val(refModel.CreatedById);

                    $modal_group_title.text("Edit Group");
                    $("#save-group").text("Confirm");

                    $modal_group.modal('toggle');
                }
            },
            error: function (data) {
                cleanBookNotification.error("Have an error, detail: " + data.error, "Qbicles");
            }
        });

    }
}

// End Account group
//  add account
function AddAccount(groupId, controlId) {
    var ajaxUri = '/ManageAccounts/GetAccount2AddEdit?id=0&groupId='+groupId;
    AjaxElementShowModal(ajaxUri, 'modal_account');
}
// Edit account
function EditCbAccount(accountId) {
    var ajaxUri = '/ManageAccounts/GetAccount2AddEdit?id=' + accountId;
    AjaxElementShowModal(ajaxUri, 'modal_account');
};





function DeleteAccount(id, name, accountNumber, createBy, createdDate, dataManage, ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }

    $.ajax({
        type: 'post',
        url: '/ManageAccounts/ValidDeleteAccount',
        dataType: 'json',
        data: { accountId: id },
        success: function (ref) {

            if (ref.status === true) {
                $("#permission-del-account").show();
                $("#confirm-del-account").hide();
                $('#task-name-permission-account').text(name);
            } else {

                $trDelete = $("#account-tr-" + id);
                $liDelete = $("#account-" + id);
                $accountIdDelete = id;
                $accountNameDelete = name;
                $accountNumberDelete = accountNumber;
                $accountCreateDate = createdDate;
                $accountCreateBy = createBy;
                $accountDataManage = dataManage;

                $("#permission-del-account").hide();
                $("#confirm-del-account").show();
                $('#task-name-confirm-account').text(name);
            }
            $modal_account_remove.modal('toggle');
        }
    })

};
function AccountDelete() {
    if ($accountIdDelete <= 0)
        return;

    var model = {
        Id: $accountIdDelete,
        Name: $accountNameDelete,
        Number: $accountNumberDelete,
        CreatedDate: $accountCreateDate,
        CreatedById: $accountCreateBy,
        DataManagerId: $accountDataManage
    };
    $.ajax({
        type: 'post',
        url: '/ManageAccounts/DeleteAccount',
        dataType: 'json',
        data: model,
        success: function (res) {
            if (res.status) {
                $modal_account_remove.modal('toggle');
                $($trDelete).css("background-color", "#FF3700");
                $($trDelete).fadeOut(1500, function () {
                    $($trDelete).remove();
                });
                $($liDelete).css("background-color", "#FF3700");
                $($liDelete).fadeOut(1500, function () {
                    $($liDelete).remove();
                });
                $accountIdDelete = 0;
                cleanBookNotification.success(_L("ERROR_MSG_469"), "Qbicles");
            }
            else
                cleanBookNotification.error(_L("ERROR_MSG_458", [$accountNameDelete]), "Qbicles");
        }
    })
}

function SaveCBAccount() {
    if ($("#role_grant").val() === null) {
        cleanBookNotification.error(_L("ERROR_MSG_459"), "Qbicles");
        return;
    }
    Save_Account();
}
function Save_Account() {
    if ($("#form_accout").valid()) {

        var upFieldsCheck = [];
        var uploadfieldsData = [];
        uploadfieldsData = $("#select_user_role").select2('data');
        for (var i = 0; i < uploadfieldsData.length; i++) {
            upFieldsCheck[i] = uploadfieldsData[i].text;
        }
        var checkField = false;
        $.ajax({
            type: 'post',
            url: '/ManageAccounts/CheckUploadFields',
            dataType: 'json',
            data: { uploadfields: JSON.stringify(upFieldsCheck) },
            success: function (ref) {

                if (!ref) {
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.warning(
                        "Date and Description fields are mandatory fields. At least a Debit or Credit field must also be selected.",
                        "Qbicles");

                    return;
                }
                $.ajax({
                    type: 'post',
                    url: '/ManageAccounts/DupplicateAccount',
                    dataType: 'json',
                    data: {
                        Id: $input_account_id.val(),
                        Name: $input_account_name.val(),
                        GroupId: $select_account_group.val()
                    },
                    success: function (refModel) {

                        if (!refModel) {
                            $.LoadingOverlay("show");
                            var bkAccount = {
                                Id: $("#input_account_id").val(),
                                IsActive: $("#input_account_isactive").val(),
                                CreatedDate: $("#input_account_createddate").val(),
                                CreatedById: $("#input_account_createdbyid").val(),
                                //Id: $("#isEditLastbalance").val(),//check
                                Name: $("#input_account_name").val(),
                                GroupId: $("#select_account_group").val(),
                                Number: $("#input_account_number").val(),
                                LastBalance: $("#input_account_lastbalance").val(),
                                //uploadfields: [],//check
                                UpdateFrequencyId: $("#select_Frequency_type").val(),
                                DataManagerId: $("#select_datamanager_type").val(),
                                BookkeepingAccount: { id: $("#selected-bookkeeping-account-id").val() },
                                WorkGroup: { Id: $("#select_account_workgroup").val() }
                            };
                            //upload fields
                            var fieldsSelected = $("#select_user_role").val();
                            
                            var rolesSelected = $("#role_grant").val();
                            

                            $.ajax({
                                type: "post",
                                dataType: 'json',
                                url: "/ManageAccounts/SaveCBAccount?isEditLastbalance=" + $("#isEditLastbalance").val() + "&uploadFields=" + fieldsSelected.join(), // + "&rolesGrant=" + rolesSelected.join(),
                                data: {
                                    account: bkAccount
                                },
                                //processData: false,
                                //contentType: false,
                                success: function (refModel) {
                                    if (refModel.result) {

                                        $("#account-page-display").empty();
                                        $("#account-page-display").html("<div id='account-content'></div>");
                                        LoadAccounts('');
                                        $modal_account.modal('toggle');
                                        if (refModel.actionVal === 1)
                                            cleanBookNotification.success(_L("ERROR_MSG_470"), "Qbicles");
                                        else if (refModel.actionVal === 2)
                                            cleanBookNotification.success(_L("ERROR_MSG_471"), "Qbicles");
                                    }
                                    else
                                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                                },
                                error: function (data) {
                                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                                }

                            }).always(function () {
                                LoadingOverlayEnd();
                            });
                        }
                        //$("#form_accout").trigger("submit");
                        else {
                            $("#form_accout").validate().showErrors({
                                Name: "Account name of '" + $input_account_name.val() + "' already exists."
                            });

                        }
                    },
                    error: function (data) {
                        cleanBookNotification.error('Have an error when save account!' +
                            $input_group_name.val() +
                            ": " +
                            data.error,
                            "Qbicles");
                    }
                });
            }
        });
    }
}

//  Common
function LoadAccounts(init) {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/ManageAccounts/LoadAccounts/",
        data: {
            groupId: $selectFilter.val(),
            orderBy: $selectOrder.val()
        },
        cache: false,
        type: "POST",
        success: function (data) {
            if (data.length !== 0) {
                $(data.ModelString).insertAfter("#account-content").hide().fadeIn(1000);
                if (init === 'init')
                    DisplayView('grid');
                else if (typeof (Storage) !== "undefined") {
                    var showType = sessionStorage.getItem('showType');
                    DisplayView(showType);
                } else {
                    DisplayView('grid');
                }
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function DisplayView(showType) {
    SetShowAccountType(showType);



    //$('.toggle_view').removeClass('active');
    $('.account-view').hide();
    if (showType === 'grid') {
        $("#grid-view").addClass('active');
        $("#grid_view").show();
    }
    else {
        $("#list-view").addClass('active');
        $("#list_view").show();
    }

}

function SetShowAccountType(showType) {
    if (typeof (Storage) !== "undefined") {
        sessionStorage.removeItem('showType');
        sessionStorage.setItem('showType', showType);
    } else {
        cleanBookNotification.warning('Your browser is too old. Please upgrade your browser!', "Qbicles");
    }
}

function swichPage() {
    var selectPage = document.getElementById("menu-select").value;
    if (selectPage === "Tasks")
        window.location = "/Apps/Tasks";
    else if (selectPage === "Accounts")
        window.location = "/Apps/Accounts";
    else if (selectPage === "Config")
        window.location = "/Apps/CleanBookConfig";
}
function ClearError() {
    $("label.error").hide();
    $(".error").removeClass("error");
    $("label.valid").hide();
    $(".valid").removeClass("valid");
}

// Upload
function ResizeAble() {
    setTimeout(function () {
        var width = $('#containerdiv').width();
        $('#resizediv').width(width);
        $("#resizediv").css({ 'height': '400px', 'overflow': 'auto' });
    }, 50);
}
function close_modal_upload() {
    $.ajax({
        type: 'post',
        url: '/Transactions/RemoveSession',
    })
}
function GetMaxUploads(accountId) {
    return $.ajax({
        url: "/ManageAccounts/GetMaxUploadToAccount",
        type: "GET",
        dataType: "json",
        data: { accountId: accountId }
    });
}

function AddNewUpload(groupId, accountId, accountName, accountNumber, accountLastBalance, ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }
    $dlsheetname.hide();
    $ReadSheetData.attr('disabled', '');
    $DefineColumnsData.attr('disabled', '');
    $buttonProceed4.attr('disabled', '');
    $buttoncheckscan.attr('disabled', '');
    $lblcheckfilefeedback.text("");
    $input_transaction_upload.val(null);
    $(".fileinput").fileinput("clear");


    $('.app_subnav li').removeClass('active');
    $('.tab-content div').removeClass('active');
    $("#modal_transaction_edit.tab-content").show();
    $("#step_2_edit").addClass('in active');
    $("#stepv2edit").addClass('in active');

    $selectedGroupAccountId = groupId;
    $accountName = accountName;
    $selectedAccountId = accountId;
    $accountLastBalanceSelected = accountLastBalance;
    var upName = accountName + " - " + accountNumber;
    // gen upload name
    GetMaxUploads($selectedAccountId).done(function (response) {
        var dateNow = new Date();
        $label_upload_name.text(upName + " - " +
            dateNow.getFullYear() + '/' + (dateNow.getMonth() + 1) + '/' + dateNow.getDate() + " - " + response.result
        );
        CheckExistsAccount = response.existsUpload;
        if (response.existsUpload) {
            $("#opening_balance").css({ "display": "none" });
            $("#lblOpenBalance").css({ "display": "block" });
            $("#lblOpenBalance").html($.number(response.lastbalance, 2));
            $("#opening_balance").val(response.lastbalance);
        }
        else {
            $("#opening_balance").css({ "display": "block" });
            $("#lblOpenBalance").css({ "display": "none" });
            $("#opening_balance").val(response.lastbalance);
            $("#lblOpenBalance").html($.number(response.lastbalance, 2));
        }
        $("#account-upload-content").css({ "display": "block" });
        $modal_transaction_edit.modal('toggle');
    }).error(function (response) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    }).fail(function (er) {
        cleanBookNotification.error(er.responseText, "Qbicles");
    });

}

function RemoveSheetSelect() {
    document.getElementById("select_sheetnameFile").innerHTML = "";
    $select_sheetnameFile.val(null).trigger("change");
}

function CheckFileTypeVirusScaning() {
    var fileName = $('[name="excelFile"]').val().trim();
    var pos = fileName.lastIndexOf('.');
    var extension = (pos <= 0) ? '' : fileName.substring(pos);
    var model = {
        fileName: "Excel",// default only upload excel
        fileExtension: extension
    };
    RemoveSheetSelect();
    $.ajax({
        type: 'post',
        url: '/Transactions/ValidationFileType',
        dataType: 'json',
        data: model,
        success: function (ref) {
            if (ref.valid === 1) {
                $lblcheckfilefeedback.text("");
                $buttoncheckscan.removeAttr('disabled', '');
                scaning_virus();
            }
            else if (ref.valid === 0) {
                $lblcheckfilefeedback.text(_L("ERROR_MSG_460"));
                return;
            }
            else {
                $lblcheckfilefeedback.text(_L("ERROR_MSG_461"));
                return;
            }
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    })

}
// scan virus if isvirus free then read sheets excel file, else delete file
function scaning_virus() {
    var file = $('#input_transaction_upload')[0].files;
    if (!file) {
        $lblcheckfilefeedback.text("Please browse a correct Excel file to upload");
        return;
    }
    var fd = new FormData();
    fd.append("file", file[0]);
    $.ajax({
        url: "/Transactions/VirusSacning",
        type: "POST",
        data: fd,
        processData: false,
        contentType: false
    }).done(function (data) {
        if (data.IsVirusFree) {
            $.ajax({
                url: "/Transactions/ReadSheets",
                dataType: 'json',
                type: "POST",
                data: fd,
                processData: false,
                contentType: false
            }).done(function (data) {
                if (!data.Status.Success) {
                    $lblcheckfilefeedback.text(data.Status.Message);
                    cleanBookNotification.error(_L("ERROR_MSG_462", [$('[name="excelFile"]').val().trim().replace("C:\\fakepath\\", "")]), "Qbicles");
                    return;
                }
                var sheetId;
                $.each(data.Sheets, function () {
                    sheetId = this.sheetId;
                    var newOption = new Option(this.sheetName, this.sheetId, false, false);
                    $select_sheetnameFile.append(newOption).trigger('change');
                })
                $dlsheetname.show();
                $lblcheckfilefeedback.text("");
                cleanBookNotification.success(_L("ERROR_MSG_472"), "Cleanbooks");
                $ReadSheetData.removeAttr('disabled', '');
            }).error(function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_462", [$('[name="excelFile"]').val().trim().replace("C:\\fakepath\\", "")]), "Qbicles");
                return false;
            }).fail(function (er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
            });
        }
        else {
            $lblcheckfilefeedback.text(data.Message);
            $ReadSheetData.attr('disabled', '');
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            //delete file
        }
    }).error(function (data) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        return false;
    }).fail(function (er) {
        cleanBookNotification.error(er.responseText, "Qbicles");
    });
}


function dateFormatSlected() {
    configureColumns(1);
}

function ReadSheetData_Click() {
    _heads = [];
    colHeadIndex = 1;
    //Read data sheet
    var fd = new FormData();
    fd.append("sheetId", $select_sheetnameFile.select2('data')[0].id);
    fd.append("accountId", $selectedAccountId);
    fd.append("_SheetName", $select_sheetnameFile.select2('data')[0].text);

    fd.append("lastBalance", $("#opening_balance").val());
    $.ajax({
        url: "/Transactions/ExcelFileUpload",
        dataType: 'json',
        type: "POST",
        data: fd,
        processData: false,
        contentType: false
    }).done(function (data) {
        setCookie("headercol", data.headColRequire);
        if (data.bindFormat === "Done") {
            if (data.headColRequire !== "")
                $('#alertConfigureColumns1').empty().append("<p>Below you will find the data from your imported file. Above each column is a" +
                    " select box which you can use to specify what each column is in your import. If " +
                    "a column isn't required, simply select <strong><i>ignore</i></strong>")
                    .append("<strong>The previous format for uploading transactions to this account has been applied.</strong></p>")
                    .append("</br><div class='alert alert-danger' style='margin-bottom: 0;'>NOTE: Uploading transactions to this account requires the following fields: " + data.headColRequire + ".</div>");
            else
                $('#alertConfigureColumns1').empty().append("<p>Below you will find the data from your imported file. Above each column is a" +
                    " select box which you can use to specify what each column is in your import. If " +
                    "a column isn't required, simply select <strong><i>ignore</i></strong></p>")
                    .append("<p><strong>The previous format for uploading transactions to this account has been applied.</strong></p>");
        }
        else {
            if (data.headColRequire !== "")
                $('#alertConfigureColumns1').empty().append("<p>Below you will find the data from your imported file. Above each column is a" +
                    " select box which you can use to specify what each column is in your import. If " +
                    "a column isn't required, simply select <strong><i>ignore</i></strong></p>")
                    .append("</br><div class='alert alert-danger' style='margin-bottom: 0;'>NOTE: Uploading transactions to this account requires the following fields: " + data.headColRequire + ".</div>");
            else
                $('#alertConfigureColumns1').empty().append("<p>Below you will find the data from your imported file. Above each column is a" +
                    " select box which you can use to specify what each column is in your import. If " +
                    "a column isn't required, simply select <strong><i>ignore</i></strong></p>");
        }
        if (data.StartReadSheetData === 1) {
            return;
        }
        if (data.status === "Error") {
            cleanBookNotification.error(_L("ERROR_MSG_462", [$('[name="excelFile"]').val().trim().replace("C:\\fakepath\\", "")]), "Qbicles");
            return;
        }
        else if (data.status === "NoData") {
            cleanBookNotification.error(_L("ERROR_MSG_463"), "Qbicles");
            return;
        }
        else if (data.status === "Error") {
            cleanBookNotification.error(data.message, "Qbicles");
            return;
        }
        //bind datatransaction to table
        $stepv3edit_tableNew.find('tbody').empty();
        $stepv3edit_tableNew.find('tbody').append(data.dataTransactionsNew);
        ResizeAble();
        $('select').not('.single-select').select2({
            templateResult: format
        });
        colCount = data.colCount;
        if (!CheckExistsAccount)
            $accountLastBalanceSelected = $("#opening_balance").val();
        else {
            var tmp = $("#lblOpenBalance").html();
            for (var i = 0; i < tmp.length; ++i) {
                if (tmp[i] === ",")
                    tmp = tmp.replace(",", "");
            }
            $accountLastBalanceSelected = tmp;
        }
        configureColumns(1);
        $(document).find('.active').removeClass('active');
        $("#step_3_edit").addClass('in active');
        $("#stepv3edit").addClass('in active');


        return;
    }).error(function (data) {
        cleanBookNotification.error(_L("ERROR_MSG_462", [$('[name="excelFile"]').val().trim().replace("C:\\fakepath\\", "")]), "Qbicles");
        return false;
    }).fail(function (er) {
        cleanBookNotification.error(er.responseText, "Qbicles");
    });
    return false;
}

function ValidationSelectedHeadColumns(headsselected, accountIdSelected) {
    var heads = { values: JSON.stringify(headsselected) };
    return $.ajax({
        url: "/Transactions/ValidationSelectedHeadColumns",
        type: "GET",
        dataType: "json",
        data: { heads: JSON.stringify(headsselected), accountId: accountIdSelected }
    });
}

function DefineColumnsData_Click() {
    Loading_proceed();
    $.ajax({
        type: 'post',
        url: '/Transactions/DataAnalyse',
        dataType: 'json',
        data: {
            AccountId: $selectedAccountId,
            headerColumns: JSON.stringify(_headersSelected),
            accountName: $accountName,
            accountLastBalance: JSON.stringify($accountLastBalanceSelected)
        },
        success: function (ref) {
            if (ref.StartDataAnalyse === 1) {
                EndLoading2();
                return;
            }
            var _Divuploaderror = document.getElementById('Divuploaderror');
            var _Divuploadnoerror = document.getElementById('Divuploadnoerror');
            _Divuploaderror.style.display = "none";
            _Divuploaderror.style.height = "0px";
            _Divuploadnoerror.style.display = "block";
            $stepv4AnalysedData_table.find('tbody').empty();
            $stepv4AnalysedData_table.find('tbody').append(ref.dataAnalysedData);
            $stepv4display_table.find('tbody').empty();
            $stepv4display_table.find('tbody').append(ref.dataTransactions);
            if (ref.warningbalance !== "") {
                _Divuploaderror.style.display = "block";
                _Divuploaderror.style.height = "150px";
                _Divuploadnoerror.style.display = "none";
                cleanBookNotification.error(_L("ERROR_MSG_464"), "Qbicles");
                $("#linkerrorfile").attr("href", ref.Errorfile);
                $buttonProceed4.attr('disabled', '');
                EndLoading2();
                return;
            }
            if (ref.dataTransactions !== "") {
                $buttonProceed4.removeAttr('disabled', '');
                $("#list_dateformat").empty().append(ref.GenListdateFormat);
                $("#list_dateformat").trigger("change");
            }
            else {
                cleanBookNotification.warning(_L("ERROR_MSG_465"), "Qbicles");
                $buttonProceed4.attr('disabled', '');
            }
            EndLoading2();
        },
        error: function () {
            EndLoading2();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            $buttonProceed4.attr('disabled', '');
        }
    });
}
function ConfirmUpload() {
    Loading_save();
    //get filename
    var filepath = $('[name="excelFile"]').val().trim();
    var pos = filepath.lastIndexOf('\\');
    //bind UploadFormat modal to save
    var uploadformatmodal = {
        Name: $label_upload_name.text()
    };
    //bind upload modal to save
    var uploadmodal = {
        Name: $label_upload_name.text(),
        FileName: filepath.substring(pos + 1),
        AccountId: $selectedAccountId
    };
    var dateFormat = "";
    dateFormat = $('#list_dateformat').val();

    $.ajax({
        url: "/Transactions/Confirm2Save",
        type: "post",
        dataType: "json",
        data: {
            uploadModal: uploadmodal,
            uploadFormatModal: uploadformatmodal,
            dateFormat: dateFormat,
            headerColumns: JSON.stringify(_headersSelected),//_headsSelected
            selectedGroupAccountId: $selectedGroupAccountId
        }
    }).done(function (data) {
        if (data.actionVal === 1) {
            document.getElementById("last-update-" + $selectedAccountId).innerHTML = data.msgName;
            var balanceFirst = "";
            var balanceLast = "";
            if (data.msg.indexOf(".") > 0) {
                balanceFirst = data.msg.split(".")[0];
                balanceLast = data.msg.split(".")[1];
            }
            else {
                balanceFirst = data.msg;
            }
            document.getElementById("last-balance-before-" + $selectedAccountId).innerHTML = balanceFirst;
            document.getElementById("last-balance-after-" + $selectedAccountId).innerHTML = '.' + balanceLast;

            document.getElementById("last-update-tr-" + $selectedAccountId).innerHTML = data.msgName;
            document.getElementById("last-balance-tr-" + $selectedAccountId).innerHTML = data.msg;

            $modal_transaction_edit.modal('hide');
            cleanBookNotification.success(_L("ERROR_MSG_473"), "Qbicles");
        }
        else if (data.actionVal === 2 || data.actionVal === 0) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
        else if (data.actionVal === 3) {
            cleanBookNotification.error(_L("ERROR_MSG_466"), "Qbicles");
        }
        EndLoading2();
        close_modal_upload();

    }).error(function (data) {
        EndLoading2();
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    }).fail(function (er) {
        cleanBookNotification.error(er.responseText, "Qbicles");
    });
}

//  Preview Upload

function ShowUploadHistory(accountId) {
    $("#upload-history").empty();
    $.ajax({
        url: "/ManageAccounts/UploadPreview/",
        data: {
            accountId: accountId,
            isDelete: $("#right-delete-history").val()
        },
        cache: false,
        type: "POST",
        success: function (data) {

            if (data.length !== 0) {
                $("#upload-history").append(data).hide().fadeIn(1000);
            }

        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
    $modal_transaction_preview.modal('toggle');
}

// Delete upload
function DeleteAccountUploadImportTransaction(uploadId,ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }

    $uploadIdDelete = uploadId;
    $uploadNameDelete = $("#upload-name-" + uploadId).val();
    $accountIdDelete = $("#account-id-" + uploadId).val();
    $accountNameDelete = $("#account-name-" + uploadId).val();
    $uploadFormatId = $("#UploadFormatId-" + uploadId).val();

    var model = {
        Id: uploadId,
        Name: $uploadNameDelete,
        AccountId: $accountIdDelete,
        CreatedDate: $("#create-date-" + uploadId).val()
    };
    $.ajax({
        type: 'post',
        url: '/Transactions/ValidDelete',
        dataType: 'json',
        data: model,
        success: function (ref) {
            if (ref.resLastUpload === "" && ref.resTransaction === "") {
                cleanBookNotification.error("Have an error when Delete upload. Please contact the system administrator for help.", "Qbicles");
                return;
            }
            if (!ref.resLastUpload) {// not last upload
                $("#permission-del").show();
                $("#confirm-del").hide();
                $('#upload-name-permission').text($uploadNameDelete);
                $('#account-name-permission').text($accountNameDelete);
                $('#temp-name-permission').text("You may only select the last upload to the Account to be deleted.");
            }
            else if (ref.resLastUpload) { // last upload
                if (!ref.resTransaction)//not in taskinstance
                {
                    //delete
                    $("#permission-del").hide();
                    $("#confirm-del").show();
                    $('#upload-name-confirm').text($uploadNameDelete);
                    $('#account-name-confirm').text($accountNameDelete);
                }
                else {
                    $("#permission-del").show();
                    $("#confirm-del").hide();
                    $('#upload-name-permission').text($uploadNameDelete);
                    $('#account-name-permission').text($accountNameDelete);
                    $('#temp-name-permission').text("Transactions from this upload are currently being used in a task.");
                }
            }
            $modal_upload_remove.modal('toggle');
        },
        error: function () {
            cleanBookNotification.error("Have an error when Delete upload. Please contact the system administrator for help.", "Qbicles");
        }
    })
}

function ConfirmDeleteUpload() {

    if ($uploadIdDelete <= 0)
        return;
    var model = {
        Id: $uploadIdDelete,
        Name: $uploadNameDelete,
        UploadFormatId: $uploadFormatId,
        AccountId: $accountIdDelete
    };
    $.ajax({
        type: 'post',
        url: '/Transactions/DeleteUpload',
        dataType: 'json',
        data: model,
        success: function (res) {
            if (res.status) {
                $modal_upload_remove.modal('hide');
                cleanBookNotification.removeSuccess();
                //change the background color to red before removing
                $trDelete = $("#upload-preview-tr-" + $uploadIdDelete);
                $($trDelete).css("background-color", "#FF3700");
                $($trDelete).fadeOut(1000,
                    function () {
                        $($trDelete).remove();
                    });

                $("#upload_preview-" + $uploadIdDelete).remove();

                //$uploadIdDelete = 0;
                document.getElementById("last-update-" + res.AccountId).innerHTML = res.LastUpload;
                var balanceFirst = "";
                var balanceLast = "";
                if (res.LastBalance.indexOf(".") > 0) {
                    balanceFirst = res.LastBalance.split(".")[0];
                    balanceLast = res.LastBalance.split(".")[1];
                }
                else if (res.LastBalance === "") {
                    balanceFirst = "0";
                    balanceLast = "00";
                }
                else {
                    balanceFirst = res.LastBalance;
                    balanceLast = "00";
                }

                document.getElementById("last-balance-before-" + res.AccountId).innerHTML = balanceFirst;
                document.getElementById("last-balance-after-" + res.AccountId).innerHTML = '.' + balanceLast;

                document.getElementById("last-update-tr-" + res.AccountId).innerHTML = res.LastUpload;
                document.getElementById("last-balance-tr-" + res.AccountId).innerHTML = res.LastBalance;

            } else
                cleanBookNotification.error(res.msg,"Qbicles");
        }
    });
}


function Loading_proceed() {
    generateModalprocess('Processing');
}
function Loading_save() {
    generateModalprocess('Saving');
};

function generateModalprocess(message) {
    var str = '<div id="modal-loading-parent" class="modal fade" role="dialog" style="opacity: 0.6;background: #000;display: block;">' + ' </div>' +
        '<div id="modal-loading" class="modal fade" style="margin-top: 14%;">' +
        '<div class="modal-dialog">' +
        '  <div class="modal-content">' +
        '  <div class="modal-body text-center">' +
        '   <br />' +
        ' <i class="fa fa-spinner fa-4x fa-spin" style="margin-bottom: 20px;"></i>' +
        ' <h4>' + message + ' Data, please wait...</h4>' +
        ' <br />' +
        '<br />' +
        ' </div>' +
        ' </div>' +
        ' </div>' +
        ' </div>' +
        '<input type="button" data-toggle="modal" data-target="#modal-loading" id="popdialogloading" style="display:none" />';
    $('body').append(str);
    $('#popdialogloading').click();
}

function ShowImportFromBookkeeping(accountId) {
    $('.loading-placeholder').show();
    $('.imported-data-transaction').hide();

    $("#app-cb-account-import").modal("show");

    var ajaxUri = '/CleanBooksAccount/ShowImportFromBookkeeping?accountId=' + accountId;
    $('.imported-data-transaction').load(ajaxUri, function () {
        $('.loading-placeholder').hide();
        $('.imported-data-transaction').fadeIn();
    });
};

function ImportFromBookkeeping(accountId) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/CleanBooksAccount/ImportFromBookkeeping?accountId=' + accountId,
        dataType: 'json',
        //data: model,
        success: function (res) {
            if (res.result) {

                $("#app-cb-account-import").modal("hide");
                document.getElementById("last-update-" + accountId).innerHTML = res.msg;
                document.getElementById("last-update-tr-" + accountId).innerHTML = res.msg;

                cleanBookNotification.createSuccess();
            }
            else
                cleanBookNotification.error(res.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};