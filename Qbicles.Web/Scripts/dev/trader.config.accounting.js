
var BKAccount = {};
// begin tax rate functions
function clickAdd() {
    $('.accountInfo').text('');
    $('.editbtnaccount').attr("style", "display:none;");
    $('.addbtnaccount').removeAttr("style");
    ResetFormControl('form_taxrate_add');
}
function addTaxRate() {
    if ($("#form_taxrate_add").valid()) {
        $('.app-coa-tax-add').LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveTaxRate',
            data: {
                Name: $('#add-taxrate-name').val(),
                Rate: $('#add-taxrate-rate').val(),
                AssociatedAccount: { Id: (BKAccount.Id ? BKAccount.Id : 0) },
                IsAccounted: $('#add-isAccounted').val() == "1" ? true : false,
                IsPurchaseTax: $('#add-isPurchaseTax').val() == "1" ? true : false,
                IsCreditToTaxAccount: $('#add-isCreditToTaxAccount').val() == "1" ? true : false,
                Description: $('#add-taxrate-description').val()
            },
            dataType: 'json',
            success: function (response) {
                if (response.result == true) {
                    if (response.actionVal == 1) {
                        $('#app-coa-tax-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        reloadAccounting();

                    } else if (response.actionVal == 2) {
                        $('#app-coa-tax-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        reloadAccounting();
                    } else if (response.actionVal == 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            $('.app-coa-tax-add').LoadingOverlay("hide", true);
        });
    };
}
function editTaxRate(taxRateId) {
    ResetFormControl('form_taxrate_edit');
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/GetEditTaxRateById',
        data: {
            id: taxRateId
        },
        dataType: 'json',
        success: function (response) {
            $('#edit-taxrate-id').val(response.Id);
            $('#edit-taxrate-name').val(response.Name);
            $('#edit-taxrate-rate').val(response.Rate);
            $('#edit-taxrate-description').val(response.Description);
            $("#edit-isAccounted").val(response.IsAccounted ? "1" :"0").trigger('change');
            $("#edit-isPurchaseTax").val(response.IsPurchaseTax ? "1" : "0").trigger('change');
            $("#edit-isCreditToTaxAccount").val(response.IsCreditToTaxAccount ? "1" : "0").trigger('change');
            if (response.AssociatedAccount) {
                BKAccount = { Id: response.AssociatedAccount.Id, Name: response.AssociatedAccount.Name };
                $('.accountInfo').text(BKAccount.Name);
            } else {
                BKAccount = {};
                $('.accountInfo').text('');
            }
            if ($('.accountInfo').text().length > 0) {
                $('.addbtnaccount').attr("style", "display:none;");
                $('.editbtnaccount').removeAttr("style");
            } else if ($('.accountInfo').text().length === 0) {
                $('.editbtnaccount').attr("style", "display:none;");
                $('.addbtnaccount').removeAttr("style");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function updateTaxrate() {

    if ($("#form_taxrate_edit").valid()) {
        $('.app-coa-tax-edit').LoadingOverlay("show");
        var isAcc = $('#edit-isAccounted').val() == "1" ? true : false;
        var data = {
            Id: $('#edit-taxrate-id').val(),
            Name: $('#edit-taxrate-name').val(),
            Rate: $('#edit-taxrate-rate').val(),
            AssociatedAccount: { Id: (BKAccount.Id && isAcc ? BKAccount.Id : 0) },
            IsAccounted: isAcc,
            IsPurchaseTax: $('#edit-isPurchaseTax').val() == "1" && isAcc ? true : false,
            IsCreditToTaxAccount: $('#edit-isCreditToTaxAccount').val() == "1" && isAcc ? true : false,
            Description: $('#edit-taxrate-description').val()
        };

        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveTaxRate',
            data: data,
            dataType: 'json',
            success: function (response) {
                if (response.result == true) {
                    if (response.actionVal == 1) {
                        $('#app-coa-tax-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        reloadAccounting();
                    } else if (response.actionVal == 2) {
                        $('#app-coa-tax-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        reloadAccounting();
                    } else if (response.actionVal == 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            $('.app-coa-tax-edit').LoadingOverlay("hide", true);
        });
    }
}
function confirmDeleteTaxRate(id, name) {
    $('#label-confirm-taxrate').text("Do you want delete tax rate : " + name);
    $('#id-itemtaxrate-delete').val(id);
}
function deleteTaxRate() {
    $.ajax({
        type: 'delete',
        url: '/Bookkeeping/DeleteTaxRate',
        data: {
            id: $('#id-itemtaxrate-delete').val()
        },
        dataType: 'json',
        success: function (response) {
            if (response == "OK") {
                cleanBookNotification.removeSuccess();
                reloadAccounting();
            } else if (response == "Fail") {
                cleanBookNotification.removeFail();
            }

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}


function initSelectedAccount() {
    setTimeout(function () {
        $('.selectaccount').removeClass("selectaccount");
        $('.accountid-' + BKAccount.Id).addClass("selectaccount");
    }, 1);
}
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $('.selectaccount').removeClass('selectaccount');
    $(ev).addClass("selectaccount");
    BKAccount = { Id: id, Name: name };
    closeSelected();
    $("#app-bookkeeping-treeview").modal('toggle');
    //return BKAccount;
}
function closeSelected() {
    if (BKAccount.Id) {
        $('.accountInfo').empty();
        $('.accountInfo').append(BKAccount.Name);
    }
    else {
        $('.accountInfo').empty();
    }

    if ($('.accountInfo').text().length > 0) {
        $('.addbtnaccount').attr("style", "display:none;");
        $('.editbtnaccount').removeAttr("style");
    }
    else if ($('.accountInfo').text().length === 0) {
        $('.editbtnaccount').attr("style", "display:none;");
        $('.addbtnaccount').removeAttr("style");
    }
}

function CollapseAccount() {
    $('.jstree').jstree('close_all');
}
// end tax rate functions
reloadAccounting = function () {
    $('#comfig-content').LoadingOverlay('show');
    setTimeout(function () {
        $('#comfig-content').load('/TraderConfiguration/TraderConfigurationContent?value=accounting');
        $('#comfig-content').LoadingOverlay('hide');
    }, 500);
};

$(document).ready(function () {
    $('#tblTaxrates').DataTable({
        destroy: true,
        responsive: true,
        "lengthChange": true,
        "pageLength": 10,
        "columnDefs": [{
            "targets": 3,
            "orderable": false
        }],
        "order": []
    });
    $('.apps-account').bootstrapToggle();
    $('#tblTaxrates').show();
    $('select.select2').select2({
        placeholder: 'Please select'
    });
    //initJsTree();
});


function UpdateJournalGroupDefault(traderSettingId) {
    var data = { journalGroupId: $("#journal-group-default").val(), traderSettingId: traderSettingId };
    $.ajax({
        type: 'post',
        url: '/TraderConfiguration/UpdateJournalGroupDefault',
        datatype: 'json',
        async: false,
        data: data,
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}