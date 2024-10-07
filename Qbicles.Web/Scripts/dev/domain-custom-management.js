
//create custom domain
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}

$("#domain-name").keyup(searchThrottle(function () {
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
}));

function validateAddDomainDescription() {
    var description = $("#domain-description").val();
    if (description.length > 500)
        $("#domain-description-error").show();
    else
        $("#domain-description-error").hide();
}


function SaveCustomDomain(saveAgain) {
    $('#custom-step2').LoadingOverlay("show");
    $("#btn-save-custom-domain").attr("disabled", "disabled");

    var files = document.getElementById("domain-logo").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("domain-logo").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                $('#custom-step2').LoadingOverlay("show");
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                SubmitCustomDomain(mediaS3Object.objectKey, saveAgain);
            }
        });
    } else {
        SubmitCustomDomain(null, saveAgain);
    }
}

function SubmitCustomDomain(s3ObjectKey, saveAgain) {
   
    var request_data = {
        Description: $("#domain-description").val(),
        LogoUri: s3ObjectKey,
        Name: $("#domain-name").val()
    };
    var _url = "/Domain/ProcessCustomDomainCreate";

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: { domainRequest: request_data },
        success: function (response) {

            if (response.result) {
                $("#custom-domain-name").text('"' + request_data.Name + '"');
                if (saveAgain == false) {
                    // Get a reference to all tab elements
                    var tabElements = document.querySelectorAll(".cutom-domain-add");

                    // Iterate over the tab elements
                    tabElements.forEach(function (tabElement) {
                        // Remove the "active" class from each tab element
                        tabElement.classList.remove("active");
                    });

                    // Get a reference to the current tab element you want to activate
                    var currentTabElement = document.getElementById("custom-step3");
                    // Add the "active" class to the current tab element
                    currentTabElement.classList.add("active");
                    // Remove the "fade" and "in" classes if present
                    currentTabElement.classList.remove("fade");
                    currentTabElement.classList.remove("in");

                }

                $('#errordemo').hide(); $('#successdemo').show();

                $domainKey = response.msgId;
                $(".qbicles-list-v2").prepend(response.msg);
            } else {
                $('#errordemo').show(); $('#successdemo').hide();

                $("#btn-save-custom-domain").attr("disabled", "");
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        $('#custom-step2').LoadingOverlay("hide", true);
    })
}



$domainKey = "";
function SetupDomainProfile() {
    window.location.href = "/Domain/Wizard?key=" + $domainKey;
}