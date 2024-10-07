function FollowDomain(id) {
    $.ajax({
        type: 'post',
        url: '/DomainProfile/FollowDomain',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result == 1) {
                $("#button-follow").remove();
                $("#button-follow-" + id).remove();
                cleanBookNotification.updateSuccess();
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}