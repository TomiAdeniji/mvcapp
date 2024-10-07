function updateStatus(invitationId, partnershipId, status ) {
    $.post("/Commerce/UpdateLogisticsRelationshipStatus", { invitationId: invitationId, partnershipId: partnershipId, status: status }, function (response) {
        if (response.result) {
            $('#groupbtn' + partnershipId).remove();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}