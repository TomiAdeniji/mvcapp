$(document).on('click', '.unready', function () {
    alert('This section has not been implemented yet.');
})

function ApplyDomainByActivity() {
    $.ajax({
        type: 'post',
        url: '/Commons/GetUserDomainChangeActive',
        datatype: 'json',
        success: function (refModel) {
            if (refModel.result) {
                $("#currenDomainId").val(refModel.msgId);
            }
        }
    });
}
function DomainSelectedChange() {
    DomainSelected($('.select2avatarDomain :selected').val(), '');
}
function DomainSelected(domainkey, domainName) {
    //check business profile wizard
    //if done go to list qbicle, else go to wizard
    $("#qbicles-list-v2").LoadingOverlay('show');
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

};


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

$(document).ready(function () {
    ApplyDomainByActivity();
    CheckUnreadNotifications();
    CheckPendingRequest();
    CheckPendingWaitlist();
});

//var $userIdCurrent = "";
//$.get('/Commons/getCurrentUserId', function (data) { $userIdCurrent = data.currentUserId }).done(function () {
//    $('#selectTaskAssign').on("select2:unselecting", function (e) {
//        if (e.params.args.data.id === $userIdCurrent)
//            e.preventDefault();
//    });
//    $("#selectUserDiscussion").on("select2:unselecting", function (e) {
//        if (e.params.args.data.id === $userIdCurrent)
//            e.preventDefault();
//    });
//    $('#selectLinkAlertTo').on("select2:unselecting", function (e) {
//        if (e.params.args.data.id === $userIdCurrent)
//            e.preventDefault();
//    });
//    $('#selectSendInvites').on("select2:unselecting", function (e) {
//        if (e.params.args.data.id === $userIdCurrent)
//            e.preventDefault();
//    });
//});

function openPopupChangeDomain() {
    $('#domain-change').modal('show');
};

function gotoMediaFolder() {
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Qbicles/SetGoBackForMediaFolder',
        datatype: 'json',
        data: { gobackurl: goBack },
        success: function (refModel) {
            window.location.href = '/Qbicles/MediaFolder';
        }
    });

    setCookie("PreviousPageOfMediaFolder", goBack);
    window.location.href = "/Qbicles/MediaFolder";
};

function gotoUserProfile() {
    var url = window.location.href;
    setCookie("PreviousPageOfProfile", url);
    window.location.href = "/Administration/UserProfile";
};


function CheckUnreadNotifications() {
    var url = "/Notifications/CheckUnReadNotification/";
    $.ajax({
        url: url,
        type: "GET",
        success: function (result) {
            if (result != 'False') {
                $("#CheckUnReadNotificationExist").css("display", "");
            } else {
                $("#CheckUnReadNotificationExist").css("display", "none");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
}

function CheckPendingRequest() {
    var _url = "/Domain/CheckPendingRequest";
    $.ajax({
        method: 'POST',
        url: _url,
        dataType: 'JSON',
        success: function (result) {
            var domainRequestNum = result.pendingDomainRequestNum;
            var extensionRequestNum = result.pendingExtensionRequestNum;
            var pendingVerifyNewEmail = result.pendingVerifyNewEmail;
            if (Number(domainRequestNum) > 0 || Number(extensionRequestNum) > 0) {
                if (Number(domainRequestNum) > 0) {
                    $("#domain-request-count").css("display", "").text(domainRequestNum);
                } else {
                    $("#domain-request-count").css("display", "none").text(0);
                }
                if (Number(extensionRequestNum) > 0) {
                    $("#extension-request-count").css("display", "").text(extensionRequestNum);
                } else {
                    $("#extension-request-count").css("display", "none").text(0);
                }

                $("#administration-has-noti").css("display", "");
            } else {
                $("#domain-request-count").css("display", "none").text(0);
                $("#administration-has-noti").css("display", "none");
            }
            if (pendingVerifyNewEmail) {
                $('#check-verify-box').show();
                var _checkNewEmailModal = '<div class="modal fade right" id="check-verify" role="dialog" aria-labelledby="check-verify" style="display: none;"></div>';
                $('body').append(_checkNewEmailModal);
                $('#check-verify').load('/Administration/LoadCheckVerify');
            } else
                $('#check-verify-box').hide();
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    })
}
function CheckPinNewEmailAvailable() {
    var _pinInput = $('#check-verify input[name=pin]').val();
    $.post("/Administration/CheckPinNewEmailAvailable", { pin: _pinInput }, function (data) {
        /// 1: This PIN is not available
        /// 2: This PIN is expired time
        /// 0: This PIN is available
        /// 3: Only wrong PIN is allowed 5 times
        if (data == 0) {
            $('#check-verify-box').remove();
            $('#verify-doing').hide();
            $('#result-done').show();
        } else if (data == 1) {
            $('#verify-doing').hide();
            $('#result-fail').show();
        } else if (data == 2) {
            $('#verify-doing').hide();
            $('#result-invalidtoken').show();
        } else if (data == 3) {
            $('#check-verify-box').remove();
            $('#result-fail').html('<p>The PIN you’ve entered doesn’t match the one we emailed. Only wrong PIN is allowed 5 times</p>');
            $('#result-fail').show();
        }

    });
}
function ReVerificationNewEmail(email) {
    $.post("/Administration/VerificationNewEmail", { newEmailAddress: email }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            $('#result-invalidtoken').hide();
            $('#verify-doing').show();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        } else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    });
}

function checkShowHideAppsMenu() {
    $.ajax({
        url: '/DomainRole/CountAllUserByAllRoleOfDomain',
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
            if (refModel === 0) {
                if ($('#apps-menu').hasClass("hidden") === false) {
                    $('#apps-menu').addClass("hidden");
                }
            } else {
                $('#apps-menu').removeClass("hidden");
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
};



function isDisplayFlicker(val) {
    if (val && $('.animate-flicker').length === 0) {
        $("#dashboard-page-display").prepend("<div class=\"activity-detail animate-flicker\" style=\"padding-left: 0; margin-bottom: 30px; float: right \"> <img src=\"/Content/DesignStyle/img/stream-placeholder.png\" class=\"img-responsive\" ></div ><div class=\"clearfix\"></div>");
    } else {
        $(".animate-flicker").remove();
    }
}
function isPlaceholder(val, elId) {
    if (val && $('.animate-flicker').length === 0) {
        $(elId).prepend("<div class=\"activity-detail animate-flicker\" style=\"padding-left: 0; margin-bottom: 30px;\"> <img src=\"/Content/DesignStyle/img/stream-placeholder.png\" class=\"img-responsive\" ></div ><div class=\"clearfix\"></div>");
    } else {
        $(".animate-flicker").remove();
        removeTypingElement();
    }
}
function ShowDomanInvited() {
    var ajaxUri = '/Domain/InvitationApproverByUser';
    AjaxElementShowModal(ajaxUri, 'domain-invites');
}

function ShowAdminJoinToWaitlist() {
    window.location.href = '/Administration/AdminSysManage?tabActive=WaitlistRequest';
}

function RejectInvitationConfirm(Id, status, domainName, statusName, DomainId) {
    $("#hdInvitationId").val(Id);
    $("#hdInvitationStatus").val(status);
    $("#hdInvitationDomainName").val(domainName);
    $("#hdInvitationStatusName").val(statusName);
    $("#hdInvitationDomainId").val(DomainId);
    $("#domain-invite-rejection").css({ "z-index": "10000" });
}
function RejectInvitation() {
    ApproverInvitation($("#hdInvitationId").val(), $("#hdInvitationStatus").val(), $("#hdInvitationDomainName").val(), $("#hdInvitationStatusName").val(), $("#hdInvitationDomainId").val());
}
function ApproverInvitation(Id, status, domainName, statusName, DomainId) {

    $.LoadingOverlay("show");
    $.ajax({
        url: "/OurPeople/ApproverOrRejectInvitation/",
        data: { Id: Id, status: status, DomainId: DomainId, Note: $("#txtInvitationNote").val() },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {

            if (refModel.result) {
                var userId = $("#user-current-id").val();
                var count = $("#lblCountInvitation-" + userId).text();
                var countNew = parseInt(count) - 1;
                if (countNew <= 0)
                    $('#lblCountInvitation-' + userId).hide();

                $("#lblCountInvitation-" + userId).text(countNew);

                if (status == "3") {//reject
                    cleanBookNotification.warning(_L("WARNING_MSG_REJECT_INVITATION", [domainName]));
                    $("#domain-invite-rejection").modal("hide");
                    $("#txtInvitationNote").val("");
                }
                if (status == "2") {//accept
                    cleanBookNotification.success(_L("ERROR_MSG_491", [domainName]));
                    LoadDomainList();
                }
                var $trDelete = $("#tr-invitation-join-domain-" + Id);
                $($trDelete).css("background-color", "#FF3700");
                $($trDelete).fadeOut(1000,
                    function () {
                        var t = $('#lstInvitationJoinDomain').DataTable({
                            destroy: true,
                            searching: false,
                            paging: false,
                            info: false,
                            ordering: false,
                        });
                        t.row($trDelete).remove();
                        t.draw();
                    });
            }
            else {
                cleanBookNotification.warning(refModel.msg);
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function CheckPendingWaitlist() {
    var _url = "/Waitlist/CheckPendingWaitlist";
    $.ajax({
        method: 'POST',
        url: _url,
        dataType: 'JSON',
        success: function (result) {
            if (Number(result.actionVal) > 0) {
                $("#waitlist-request-count").css("display", "").text(result.actionVal);
            } else {
                $("#waitlist-request-count").css("display", "none").text(0);
            }
            $("#table-waitlist-request").DataTable().ajax.reload();
            $("#table-waitlist-history").DataTable().ajax.reload();
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    })
};

function CheckApprovalWaitlist(userId) {
    var _url = "/Waitlist/CheckApprovalWaitlist?userId=" + userId;
    $.ajax({
        method: 'POST',
        url: _url,
        dataType: 'JSON',
        success: function (result) {
            $('.domain-without-custom').hide();
            $('.waitlist-pending').hide();
            $('.waitlist-join-request').hide();
            $('.both-domain-and-custom').hide();
            $('.wait-join-custom').hide();

            //join waitlist
            if (result.waitRequest == "block") {
                $('.waitlist-join-request').show();
            }
            //Pending join Custom
            if (result.waitJoinCustom == "block") {
                $('.wait-join-custom').show();
            }
            //Pending
            if (result.waitPending == "block") {
                $('.waitlist-pending').show();
            }
            //done All
            if (result.allDomainCustom == "block") {
                $('.both-domain-and-custom').show();
            }
            //Done (without Custom Domains)
            if (result.domainWithoutCustom == "block") {
                $('.domain-without-custom').show();
            }
        },
        error: function (xhr) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    })
};