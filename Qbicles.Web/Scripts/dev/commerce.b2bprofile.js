$(document).ready(function () {
    //$(".checkmulti").multiselect({
    //    includeSelectAllOption: false,
    //    enableFiltering: false,
    //    buttonWidth: '100%',
    //    maxHeight: 400,
    //    enableClickableOptGroups: true
    //});
});
function createB2COrderDiscussion(businessDomainKey, catalogId) {
    $.LoadingOverlay('show');
    $.post("/B2C/CreateB2COrderDiscussion", { businessDomainKey: businessDomainKey, catalogId: catalogId }, function (response) {
        if (response.result) {
            location.href = "/B2C/DiscussionOrder?disKey=" + response.msgId;
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}
function connectB2C(uId, elm, fullname) {
    $.LoadingOverlay('show');
    $.post("/C2C/ConnectC2C", { linkId: uId, type: 1 }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            $(elm).attr('disabled', true);
            $(elm).text('Connected');
            cleanBookNotification.success(_L("CONNECTED_SUCCESS", [fullname]), "Qbicles");
            location.href = "/C2C";
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}
