
$('#general-seting-account').on('submit', function (e) {
    e.preventDefault();
    if ($("#general-seting-account").valid()) {
        changeNameAccount();
    }
});

function changeNameAccount() {
    $.ajax({
        type: 'post',
        url: '/Account/ChangeNameAccount',
        datatype: 'json',
        data: { accountName: $('#txtAccountNameChange').val() },
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.success(_L("ERROR_MSG_301"), "Qbicles");
            }
            else {
                $("#general-seting-account").validate().showErrors({ account_name: refModel.msg });
            }
        }
    });
};

function changeToPackage(idPackage) {
    $.ajax({
        type: 'post',
        url: '/Account/ChangeToPackage',
        datatype: 'json',
        data: { idPackage: idPackage },
        success: function (refModel) {
            if (refModel.result) {
                $('.hidden.currentpackage').removeClass("hidden currentpackage");
                $('#package-' + idPackage).addClass("hidden currentpackage");
                $('#name-package').text(refModel.Object.AccessLevel);
                $('#cost-package').html(refModel.Object.Cost);
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }
    });
};

function removeAccountAdmin(userId, level) {
    $.ajax({
        type: 'POST',
        url: '/Account/RemoveAccountAdmin',
        datatype: 'json',
        data: { userId: userId },
        success: function (refModel) {
            if (refModel.result) {
                // remove text level account admin
                $('#' + userId + '-' + level.replace(/\s/g, '')).remove();
                // remove option revove account admin
                $('#removeAccountAdmin-' + userId + '-' + level.replace(/\s/g, '')).remove();
                $('#setAsAccountOwner-li-' + userId).remove();
                // if exists level admin domain
                if ($('#' + userId + '-DomainAdministrator').length > 0) {
                    var optionAddAccountAdmin = '<li id="addAccountAdmin-li-' + userId + '"><a href="javascript:void(0)" onclick="addAccountAdmin(\'' + userId + '\')">Add account admin right</a></li>';
                    $('#options-' + userId).append(optionAddAccountAdmin);
                } else {
                    //if coount level == 0 => remove tr
                    if ($('#levels-' + userId).children("div").length === 0) {
                        $('#tr-' + userId).remove();
                    }
                }
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }
    });
};

function addAccountAdmin(userId) {
    $.ajax({
        type: 'POST',
        url: '/Account/AddAccountAdmin',
        datatype: 'json',
        data: { userId: userId },
        success: function (refModel) {
            if (refModel.result) {
                var obj = refModel.Object;
                $('#addAccountAdmin-li-' + userId).remove();
                var divDomain = '<div id="' + userId + '-' + obj.Level.replace(/\s/g, "") + '">' + obj.Level + '</div>';
                $('#levels-' + userId).append(divDomain);
                var optionRemoveAccountADmin = '<li id="removeAccountAdmin-' + userId + '-' + obj.Level.replace(/\s/g, "") + '"><a href="javascript:void(0)" onclick="removeAccountAdmin(\'' + obj.Id + '\', \'' + obj.Level + '\')">Remove account admin right</a></li>';
                $('#options-' + userId).append(optionRemoveAccountADmin);
                if ($('[data-owner="' + userId + '"]').length === 0) {
                    var optionSetAccountAdmin = '<li  id="setAsAccountOwner-li-' + userId + '"><a href="javascript:void(0)" onclick="setAsAccountOwner(\'' + userId + '\')">Set as account owner</a></li>';
                    $('#options-' + userId).append(optionSetAccountAdmin);
                }
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }
    });
};

function setAsAccountOwner(userId) {
    $.ajax({
        type: 'POST',
        url: '/Account/SetAsAccountOwner',
        datatype: 'json',
        data: { userId: userId },
        success: function (refModel) {
            if (refModel.result) {
                // remove owner current row
                var obj = refModel.Object;
                //1. remove option nut set owner from new ownre
                $('#setAsAccountOwner-li-' + obj.Id).remove();

                var rowOwnerOld = $("[data-owner]");
                var idOldOwner = rowOwnerOld.data("owner");
                //2. remove div text level old account owner
                $('#' + idOldOwner + '-AccountOwner').remove();
                //3. add option 'set as account owner' for old account owner
                var optionSetAccountAdmin = '<li  id="setAsAccountOwner-li-' + idOldOwner + '"><a href="javascript:void(0)" onclick="setAsAccountOwner(\'' + idOldOwner + '\')">Set as account owner</a></li>';
                $('#options-' + idOldOwner).append(optionSetAccountAdmin);
                //4. add div text to cell levels for new acount owner
                var levelDiv = '<div id="' + obj.Id + '-AccountOwner" data-owner="' + obj.Id + '">Account Owner</div>';
                $('#levels-' + obj.Id).append(levelDiv);
                //5. remove option setAsAccountOwner from new account owner
                $('#setAsAccountOwner-li-' + obj.Id).remove();
                //6. remove att data-owner from old owner
                $('#' + idOldOwner + '-AccountOwner').removeData("owner");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }
    });
};
