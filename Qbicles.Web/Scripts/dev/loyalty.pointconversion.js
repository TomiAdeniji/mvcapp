var isLoyaltySettingFormSubmit;
function loadProfileTradingPage() {
    isLoyaltySettingFormSubmit = true;
    initSaveConversionForm();
    loadArchievedConversionDataTable();
    initDomainLoyaltySettingForm();

    if (!$("#allow-debitpayment-btn").prop("checked")) {
        $("#toggle-status").text("disabled");
        $(".not-allow").hide();
    } else {
        $("#toggle-status").text("now enabled");
        $(".not-allow").show();
    }

    $("#toggle-container").on("change", function (ev) {
        var $settingfrm = $("#loyalty-setting-form");
        var isDebitPaymentAllowed = $("#allow-debitpayment-btn").prop("checked");
        var wgId = $("#debitwg").val();

        if (wgId <= 0 || wgId == "") {
            if (isDebitPaymentAllowed == true) {
                isLoyaltySettingFormSubmit = false;
                $settingfrm.validate().showErrors({ debitwg: "Please select a WorkGroup with the CreditNote process." });
                $("#allow-debitpayment-btn").bootstrapToggle("off");
                ev.preventDefault();
            } else {
                if (isLoyaltySettingFormSubmit) {
                    $("#loyalty-setting-form").submit();
                } else {
                    isLoyaltySettingFormSubmit = true;
                }
            }
        }
        else {
            $("#loyalty-setting-form").submit();
        }
    });
}

function initSaveConversionForm() {
    var $addconversionform = $("#addconversion-form");
    $addconversionform.validate({
        rules: {
            amount: {
                required: true
            },
            point: {
                required: true,
                digits: "This field only accepts integer"
            }
        }
    });

    jQuery.extend(jQuery.validator.messages, {
        digits: "This field only accepts integer."
    });


    $addconversionform.submit(function (e) {
        e.preventDefault();
        var amount = $("#conversion-amount").val();
        var pt = $("#conversion-point").val()

        if ($addconversionform.valid()) {
            if (amount < 0) {
                $addconversionform.validate().showErrors({
                    amount: "Conversion Amount must be greater than or equal to 0."
                });
            } else if (pt < 1) {
                $addconversionform.validate().showErrors({
                    point: "Conversion Amount must be greater than or equal to 1."
                });
            } else {
                savePointConversion();
            }
        }
    });
}

function savePointConversion() {
    var _conversion = {
        Amount: $("#conversion-amount").val(),
        Points: $("#conversion-point").val(),
        ConversionType: 1
    };
    var _url = "/LoyaltyPointConversion/AddConversion";

    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        data: {
            conversion: _conversion
        },
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess("Qbicles");
                $("#confirm").attr("disabled", "disabled");
                $('#archieved-conversions').DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
    LoadingOverlayEnd();
}

function loadArchievedConversionDataTable() {
    $("#archieved-conversions").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#archieved-conversions').LoadingOverlay("show");
        } else {
            $('#archieved-conversions').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/LoyaltyPointConversion/LoyaltyPaymentConversionDTContent',
            "type": 'GET',
            "dataType": 'json',
            "data": {
                conversiontye: 1
            }
        },
        "columns": [
            {
                name: "Amount",
                data: "Amount",
                orderable: false
            },
            {
                name: "Points",
                data: "Points",
                orderable: false
            },
            {
                name: "ArchievedDate",
                data: "ArchievedDate",
                orderable: false
            },
            {
                name: "ArchievedBy",
                data: "ArchievedBy",
                orderable: false
            }]
    });

}

function initDomainLoyaltySettingForm() {
    var $settingfrm = $("#loyalty-setting-form");

    $settingfrm.submit(function (e) {
        LoadingOverlay();
        e.preventDefault();
        var isDebitPaymentAllowed = $("#allow-debitpayment-btn").prop("checked");
        var wgId = $("#debitwg").val();
        var _url = "/LoyaltyPointConversion/UpdateDomainLoyaltySetting";

        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            data: {
                workGroupId: wgId,
                isDebitPaymentAllowed: isDebitPaymentAllowed
            },
            success: function (response) {
                if (response.result) {
                    if (!isDebitPaymentAllowed) {
                        $("#toggle-status").text("disabled");
                        $(".not-allow").hide();
                    } else {
                        $("#toggle-status").text("now enabled");
                        $(".not-allow").show();
                    }
                    cleanBookNotification.updateSuccess();
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        });

        LoadingOverlayEnd();
    });
}