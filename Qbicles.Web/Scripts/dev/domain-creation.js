var freePlanLevel = 1;
var starterPlanLevel = 2;
var standardPlanLevel = 3;
var expertPlanLevel = 4;
var existingPlanLevel = 5;

var selectedPlanType = freePlanLevel;
var planUserNumber = 0;
var planCostPerExtraUser = 0;
var planCost = 0;
var planCurrencySymbol = "NGN";
var planName = "";


function updateAsChosenPlanLevel() {
    switch (selectedPlanType) {
        case freePlanLevel:
            planUserNumber = 0;
            planCostPerExtraUser = 0;
            planCost = 0;
            planName = "Free plan";
            break;
        case starterPlanLevel:
            planUserNumber = 3;
            planCostPerExtraUser = 5000;
            planCost = 10000;
            planName = "Starter plan";
            break;
        case standardPlanLevel:
            planUserNumber = 5;
            planCostPerExtraUser =  5000;
            planCost = 20000;
            planName = "Standard plan";
            break;
        case expertPlanLevel:
            planUserNumber = 5;
            planCostPerExtraUser = 5000;
            planCost = 40000;
            planName = "Expert plan";
            break;
        case existingPlanLevel:
            planUserNumber = 1000;
            planCostPerExtraUser = 0;
            planCost = 0;
            planName = "Existing plan";
            break;
    }
}


function setup() {
    setTimeout(function () {
        $('.setup .btn-loading').hide();
        $('.setup .btn-loaded').show();
    }, 3000);
}
$(" .checkmulti").multiselect({
    includeSelectAllOption: true,
    selectAllJustVisible: true,
    includeResetOption: false,
    enableFiltering: false,
    buttonWidth: '100%',
    maxHeight: 400,
    enableClickableOptGroups: true,
    enableFiltering: true,
    enableCaseInsensitiveFiltering: true
});
$('#setavatar').on('change', function (e) {
    var url = "dist/img/logos/003.png";
    $("#avatar").css("background-image", "url(" + url + ")");
});
$('.stat').circleProgress({
    max: 100,
    value: 0,
    textFormat: 'percent',
    animation: 'easeInOutExpo',
    animationDuration: 2600
});
var step = 0;
function recount() {
    step = step + 25;
    $('.stat').circleProgress({
        max: 100,
        value: step,
        textFormat: 'percent',
        animation: 'easeOutExpo',
        animationDuration: 2600
    });
    var update = step + '%';
    $('#step1p').css('width', update);
}
function decount() {
    step = step - 17;
    $('.stat').circleProgress({
        max: 100,
        value: step,
        textFormat: 'percent',
        animation: 'easeOutExpo',
        animationDuration: 2600
    });
    var update = step + '%';
    $('#step1p').css('width', update);
}
$('#itemtype a').bind('click', function (e) {
    e.preventDefault();
    $('.theformtype').hide();
    var itemtype = $(this).data('value');
    if (itemtype != 'comp') {
        $('#prodorserv').fadeIn();
        $('#formulae-sell').hide();
    } else {
        $('#prodorserv').hide();
        $('#formulae-sell').show();
        $('#recipe').show();
    }
    if (itemtype == 'buy') {
        $('#item-buy').show();
    }
    if (itemtype == 'sell') {
        $('#item-sell').show();
    }
    if (itemtype == 'buys') {
        $('#item-buysell').show();
    }
    if (itemtype == 'comp') {
        $('#item-compound').show();
        $('.wizard-start').show();
    }
});
$('#productorservice a').bind('click', function (e) {
    e.preventDefault();
    var prodorserv = $(this).data('value');
    $('.wizard-start').show();
    if (prodorserv == 'service') {
        $('.inventorytab').hide();
        $('.finishonfirst').show();
        $('.normalfinish').hide();
        $('.barcode').hide();
    } else {
        $('.barcode').show();
        $('.finishonfirst').hide();
        $('.normalfinish').show();
        $('.inventorytab').show();
    }
})
function switchpromos(type) {
    $('.promotype').hide();
    $(type).show();
}
$('#areasdrop').bind('change', function (e) {
    var c = $(this).val();
    if (c == '0') {
        $('.currency').show();
    } else {
        $('.countrychoice').hide();
        $('.unsupported').show();
    }
});


function checkDomainNameAvailability() {
    var $domainName = $("#domain-name").val();

    if ($domainName != '') {
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: "/Domain/IsDuplicateDomainNameCheck",
            beforeSend: function () {
                $('#checking').show();
                $('#result').hide();
            },
            data: {
                domainKey: '',
                domainName: $domainName
            },
            success: function (data) {
                if (data) {
                    cleanBookNotification.error("The domain name has been taken. Please try another one", "Qbicles");
                    $('#result').hide();
                } else {
                    $('#result').show();
                }
            },
            error: function (err) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                $('#result').hide();
            }
        }).then(function () {
            $('#checking').hide();
        })
    } else {
        $('#result').hide();
    }
}

function reserveDomainRequestName(ev) {
    var _url = "/Domain/ReserveDomainRequestName";
    var $requestName = $("#domain-name").val();
    var $requestId = $("#domain-request-id").val();

    $.ajax({
        type: "POST",
        url: _url,
        dataType: 'JSON',
        data: {
            requestedName: $requestName,
            requestId: $requestId
        },
        success: function (data) {
            if (data.result) {
                recount();
                $('#tab1link').addClass('done').removeClass('active');
                $('#tab1link a i').show();
                $('#tab2link').addClass('active');

                $("#tab2-toggle-btn").click();

                $("#domain-request-id").val(data.Object);
                $("#domain-name-title").text("Your " + $requestName + " Business Domain has been successfully created");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            
        }
    });
}

function changeRequestPlan(planType) {
    selectedPlanType = planType;
    updateAsChosenPlanLevel();
    switch (planType) {
        case freePlanLevel:
            createDomainCreationPaymentPlan();
            break;
        case starterPlanLevel:
        case standardPlanLevel:
        case expertPlanLevel:
            $('#planselect').hide();
            $('#paidplanchosen').fadeIn();
            break;
    }

    // Fill the input according to the chosen plan
    $("#level-default-user-number").val(planUserNumber);
    $("#cost-per-extra-str").text(planCostPerExtraUser + planCurrencySymbol);

    // Set min value
    $("#user-number").attr("min", planUserNumber);

    // Update money string according to chosen plan
    var planCostString = planCost + planCurrencySymbol + " monthly";

    $(".chosen-plan-name").text(planName);
    $(".chosen-plan-cost").text(planCostString);
    $(".chosen-plan-user-number").text(planUserNumber).val(planUserNumber);
}

function planCostCalculation() {
    switch (selectedPlanType) {
        case freePlanLevel:
            break;
        case starterPlanLevel:
        case standardPlanLevel:
        case expertPlanLevel:
        case existingPlanLevel:
            var defaultUserNumberOfPlan = planUserNumber;
            var extraUesrNumber = Number($("#user-number").val()) - Number(defaultUserNumberOfPlan);
            if (extraUesrNumber < 0)
                extraUesrNumber = 0;
            var entryPlanCost = Number(planCost);
            var entryCostPerAdditionalUser = Number(planCostPerExtraUser);
            var extraCost = entryCostPerAdditionalUser * extraUesrNumber;
            var totalCost = entryPlanCost + extraCost;
            if (extraUesrNumber > 0) {
                $("#request-cost-table tbody tr").removeClass('hidden');
                $("#table-extra-cost").text(extraCost);
                $("#extra-number").text(extraUesrNumber + " extras");
            }
            $("#table-plan-name").text(planName);
            $("#table-default-cost").text(entryPlanCost);
            $("#total-cost").text(totalCost)

            recount();
            $('#tab2link').addClass('done').removeClass('active');
            $('#tab2link a i').show();
            $('#tab3link').addClass('active');
            break;
    }
}

var isPlanPaymentProcessing = false;
function createDomainCreationPaymentPlan() {
    if (isPlanPaymentProcessing == true)
        return;

    var $domainRequestId = $("#domain-request-id").val();
    var $userNumber = Number($("#user-number").val());

    var _url = "/Domain/CreateDomainRequestPlan";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        beforeSend: function () {
            isPlanPaymentProcessing = true;
            LoadingOverlayEnd()
        },
        data: {
            domainRequestId: $domainRequestId,
            userNumber: $userNumber,
            level: selectedPlanType
        },
        success: function (data) {
            if (data.result) {
                if (selectedPlanType != freePlanLevel) {
                    window.location = data.Object;
                    //$("#paystack-frame").attr('src', data.Object);
                    //$("#demotab-toggle-btn").click();
                }
            } else {
                if (data.msg) {
                    cleanBookNotification.error(data.msg, "Qbicles");
                } else {
                    cleanBookNotification.error("Something went wrong. Please contact to the administrator", "Qbicles");
                }
            }
        },
        error: function (err) {
            cleanBookNotification.error("Something went wrong. Please contact to the administrator", "Qbicles");
        }
    }).then(function () {
        if (selectedPlanType == freePlanLevel) {
            $('#tab5link').hide();
            $('#tabxlink').hide();
            recount();
            $('#paidplanchosen').hide();
            $('#tab2link').addClass('done').removeClass('active');
            $('#tab2link a i').show();
            $('#tab3link').addClass('active');
            $("#show-tab3-btn").click();
        }
        LoadingOverlayEnd();
    })
}

function updatePricingInfor() {

}

function processDomainLogoSaving() {
    var description = $("#domain-description").val();
    if (description.length > 500)
        return;
    $.LoadingOverlay("show");
    var files = document.getElementById("domain-logo").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("domain-logo").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                //$("#domain-logo-current").val(mediaS3Object.objectKey);
                updateDomainSpecifics(mediaS3Object.objectKey);
            }
        });
    } else {
        updateDomainSpecifics(null);
    }
}

function updateDomainSpecifics(s3ObjectKey) {
    var request_data = {
        description: $("#domain-description").val(),
        uploadKey: s3ObjectKey,
        requestId: $("#domain-request-id").val()
    };
    var _url = "/Domain/SaveDomainRequestSpecifics";

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: request_data,
        success: function (responseReturned) {
            var response = responseReturned;
            if (response.result) {
                if (selectedPlanType == freePlanLevel) {
                    recount();
                    $('#tab3link').addClass('done').removeClass('active');
                    $('#tab3link a i').show();
                    $('#callback').addClass('active');
                    $('#domain-created-key').val(response.Object2);

                    $("#free-plan-btn").click();
                } else {
                    recount();
                    $('#tab3link').addClass('done').removeClass('active');
                    $('#tab3link a i').show();

                    $("#tabxlink").addClass('active')
                    $("#open-receive-payment-btn").click();
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
        }
    })
}

function saveSubAccountInfo() {
    var businessName = $("#subacc-business-name").val();
    var bankCode = $("#subacc-bank-code").val();
    var accountNumber = $("#subacc-account-number").val();
    var requestId = $("#domain-request-id").val();
    var _url = "/Domain/SaveSubAccountInfoToDomainRequest";
    var _data = {
        'businessName': businessName,
        'bankCode': bankCode,
        'accountNumber': accountNumber,
        'domainRequestId': requestId
    }

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: _data,
        beforeSend: function () {
            LoadingOverlay();
        },
        success: function (response) {
            if (response.result) {
                $('#tabxlink').addClass('done').removeClass('active');
                $('#tabxlink a i').show();

                $("#paid-plan-btn").click();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).then(function () {
        LoadingOverlayEnd();
    });
}


function LaunchCreatedDomain() {
    //check business profile wizard
    //if done go to list qbicle, else go to wizard
    LoadingOverlay();
    var domainkey = $('#domain-created-key').val();
    setTimeout(function () {
        $.ajax({
            type: 'post',
            url: '/Domain/DomainBusinessProfileWirad',
            datatype: 'json',
            cache: false,
            data: {
                key: domainkey
            },
            success: function (refModel) {
                if (refModel.msgId == '') {
                    GoToQbicles(domainkey);
                }
                else {
                    window.location.href = '/Domain/Wizard?key=' + refModel.msgId;
                }
            }
        });
    }, 500);
}
function DomainRequestSave() {
    var request_data = {

    }
    $.ajax({
        type: "post",
        cache: false,
        url: "/Domain/AddQbicleDomainRequest",
        enctype: 'multipart/form-data',
        data: model,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.result) {
                clearDomainInputModalAddEdit();
                $('#create-domain').modal('toggle');
                showDomainRequestHistory();
                cleanBookNotification.updateSuccess();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            
        }


    }).always(function () {
        isBusy = false;
        LoadingOverlayEnd();
    });
}

function GoToQbicles(domainkey) {
    $.ajax({
        type: 'post',
        url: '/Commons/UpdateCurrentDomain',
        datatype: 'json',
        cache: false,
        data: {
            currentDomainKey: domainkey
        },
        success: function (refModel) {
            if (refModel.result) {
                window.location.href = '/Qbicles';
            }
            else {
                console.log(domainName);
            }
        }
    });
}

function updateSelectedBusinessDomainLevel(newLevel) {
    selectedPlanType = newLevel;
}