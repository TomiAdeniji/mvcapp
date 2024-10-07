var isBusy = false;
$(document).ready(function () {
    initSelect2avatar();
    initModalAddBankMate();
});
function initModalAddBankMate() {
    var $frmaddbankmate = $("#frmaddbankmate");
    $frmaddbankmate.validate({
        rules: {
            bankId: {
                required: true
            },
            address: {
                required: true
            },
            countryCode: {
                required: true
            },
            phone: {
                required: true
            },
            accountName: {
                required: true
            }
        }
    });
    $frmaddbankmate.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmaddbankmate.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-mbm-account-add').modal('hide');
                        LoadingOverlayEnd();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "MyBankMate");
                        location.reload();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "MyBankMate");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "MyBankMate");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "MyBankMate");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}
function initSelect2avatar() {
    $('.select2avatar-delivery').select2({
        placeholder: 'Please select',
        templateResult: formatOptions2,
        templateSelection: formatSelected2
    });
}
function formatOptions2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function formatSelected2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function addFundModalShow(accountId) {
    LoadingOverlay();
    var _url = "/Bankmate/AddFundModalShow?accountId=" + accountId;
    $("#app-mbm-funds-in").empty();
    $("#app-mbm-funds-in").load(_url, function () {
        $("#payment-accounts").select2();
        $("#app-mbm-funds-in").modal("show");
        LoadingOverlayEnd();
    })
}

function addWithdrawFundModalShow(accountId) {
    var _url = "/Bankmate/WithdrawFundModalShow?accountId=" + accountId;
    $("#app-mbm-funds-out").empty();
    $("#app-mbm-funds-out").load(_url, function () {
        $("#app-mbm-funds-out").modal("show");
        $("#receiving-accounts").select2();
        LoadingOverlayEnd();
    })
}

function transactionDetailModalShow(transactionId, accountId) {
    var _url = "/Bankmate/TransactionDetailModalShow?transactionId=" + transactionId + "&associatedAccountId=" + accountId;
    LoadingOverlay();
    $("#mbm-statement-detail").empty();
    $("#mbm-statement-detail").load(_url);
    $("#mbm-statement-detail").modal("show");
    LoadingOverlayEnd();
}

function addFund(currentAccountId) {
    //Get data
    var transactionAmount = $("#transaction-amount").val();
    var paymentAccount = $("#payment-accounts").find("option:selected").val();
    var transactionReference = $("#transaction-reference").val();

    var $addfundfrm = $("#add-fund_form");
    //Validation
    $addfundfrm.validate({
        rules: {
            transactionAmount: {
                required: true
            },
            assignee: {
                required: true
            },
            transactionReference: {
                required: true
            }
        }
    });

    //Add Funds
    var originalAccount = {
        Id: paymentAccount
    };
    var destinationAccount = {
        Id: currentAccountId
    };

    $addfundfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($addfundfrm.valid()) {
            var files = document.getElementById("link-upload-file").files;

            if (files && files.length > 0) {
                UploadMediaS3ClientSide("link-upload-file").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        SaveDomainBankmateAccountTransfer(currentAccountId, transactionAmount, originalAccount, destinationAccount, transactionReference, "fund", "app-mbm-funds-in", mediaS3Object);
                    }
                });
            } else {
                SaveDomainBankmateAccountTransfer(currentAccountId, transactionAmount, originalAccount, destinationAccount, transactionReference, "fund", "app-mbm-funds-in", null);
            }
        }
    });
}

function addWithdraw(currentAccountId) {
    //Get data
    var transactionAmount = $("#transaction-amount").val();
    var paymentAccount = $("#receiving-accounts").find("option:selected").val();
    var transactionReference = $("#transaction-reference").val();

    var $addwithdrawfrm = $("#addWithdrawFrm");
    //Validation
    $addwithdrawfrm.validate({
        rules: {
            transactionAmount: {
                required: true
            },
            assignee: {
                required: true
            },
            transactionReference: {
                required: true
            }
        }
    });

    //Add Funds
    var destinationAccount = {
        Id: paymentAccount
    };
    var originalAccount = {
        Id: currentAccountId
    };

    $addwithdrawfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($addwithdrawfrm.valid()) {
            var files = document.getElementById("link-upload-file").files;

            if (files && files.length > 0) {
                UploadMediaS3ClientSide("link-upload-file").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        SaveDomainBankmateAccountTransfer(currentAccountId, transactionAmount, originalAccount, destinationAccount, transactionReference, "withdraw", "app-mbm-funds-out", mediaS3Object);
                    }
                });
            } else {
                SaveDomainBankmateAccountTransfer(currentAccountId, transactionAmount, originalAccount, destinationAccount, transactionReference, "withdraw", "app-mbm-funds-out", null);
            }


            
        }
    });
}

function SaveDomainBankmateAccountTransfer(currentAccountId, amount, originalAccount, destinationAccount, reference, type, modalId, mediaS3Object) {
    LoadingOverlay();

    var s3ObjectUpload = null;
    if (mediaS3Object != null) {
        s3ObjectUpload = {
            FileKey: mediaS3Object.objectKey,
            FileName: mediaS3Object.fileName,
            FileType: mediaS3Object.fileType,
            FileSize: mediaS3Object.fileSize
        };
    }

    var _url = "/Bankmate/SaveBankmateTransaction";
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        data: {
            amount: amount,
            originalAccount: originalAccount,
            destinationAccount: destinationAccount,
            reference: reference,
            type: type,
            s3ObjectUpload: s3ObjectUpload
        },
        success: function (response) {
            isBusy = false;
            if (!response.result) {
                cleanBookNotification.error(response.msg, "Qbicles");
            } else {
                cleanBookNotification.createSuccess();
                ListDomainBMTransactionsShow(currentAccountId);
                $("#" + modalId).modal("toggle");
            }
        },
        error: function (err) {
            isBusy = false;
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });

    LoadingOverlayEnd();
}

function ListDomainBMTransactionsShow(accountId) {
    $("#list-transactions").LoadingOverlay('show');
    var searchKey = $("#searchkey").val();
    var searchBankIdList = "";
    if ($("#seachbankId").val() != null) {
        searchBankIdList = $("#seachbankId").val().toString();
    }
    var transactionTypeId = $("#transactiontypeId").val();
    var searchDateRange = $("#searchDateRange").val();
    var dateRangeArray = searchDateRange.split(" - ");
    if (dateRangeArray.length >= 2) {
        searchDateRange = dateRangeArray[0] + "-" + dateRangeArray[1];
    }

    var skip = 0;
    var takeNumber = 20;

    var _url = "/Bankmate/DomainBMTransactionsListShow?accountId=" + accountId + "&daterangeString=" + searchDateRange + "&keysearch=" + searchKey + "&searchBankIdList=" + searchBankIdList
        + "&showTypeId=" + transactionTypeId + "&takeNumber=" + takeNumber;
    //$.ajax({
    //    method: "GET",
    //    dataType: "JSON",
    //    url: _url,
    //    data: {
    //        daterangeString: searchDateRange,
    //        keysearch: searchKey,
    //        searchBankId: searchBankId,
    //        showTypeId: transactionTypeId,
    //        skip: skip,
    //        takeNumber: takeNumber
    //    },
    //    success: {

    //    }
    //})
    $("#list-transactions").empty();
    $("#list-transactions").load(_url);
    $("#list-transactions").LoadingOverlay('hide');
}

function LoadMoreTransaction(accountId, ev) {
    $("#list-transactions").LoadingOverlay('show');
    var searchKey = $("#searchkey").val();
    var searchBankIdList = "";
    if ($("#seachbankId").val() != null) {
        searchBankIdList = $("#seachbankId").val().toString();
    }
    var transactionTypeId = $("#transactiontypeId").val();
    var searchDateRange = $("#searchDateRange").val();
    var dateRangeArray = searchDateRange.split(" - ");
    if (dateRangeArray.length >= 2) {
        searchDateRange = dateRangeArray[0] + "-" + dateRangeArray[1];
    }

    var takeNumber = $(ev).attr("takeNum");
    var nextTimeTakenNumber = Number(takeNumber) + 20;
    var _url = "/Bankmate/LoadMoreDomainBMAccountTransaction";

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            accountId: accountId,
            daterangeString: searchDateRange,
            keysearch: searchKey,
            searchBankIdList: searchBankIdList,
            showTypeId: transactionTypeId,
            takeNumber: takeNumber
        },
        async: false,
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function(data) {
            $("#approvedList").html('');
            $("#approvedList").append(data.ModelString);

            if (data.ModelCount <= takeNumber) {
                $("#load-more-approved-btn").hide();
            } else {
                $("#load-more-approved-btn").attr("takeNum", nextTimeTakenNumber);
            }
        }
    })
    //$("#list-transactions").empty();
    //$("#list-transactions").load(_url);
    $("#list-transactions").LoadingOverlay('hide');
}