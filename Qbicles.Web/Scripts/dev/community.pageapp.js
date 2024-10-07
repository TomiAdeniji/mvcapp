function FollowPage(id) {
    $.ajax({
        type: 'post',
        url: '/Community/FollowPage',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $("#follow-page").addClass('hidden');
                $("#unfollow-page").removeClass('hidden');
                cleanBookNotification.success(_L("ERROR_MSG_662"), "Qbicles");
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
function unFollowPage(id) {
    $.ajax({
        type: 'post',
        url: '/Community/UnFollowPage',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) { 
                $("#follow-page").removeClass('hidden');
                $("#unfollow-page").addClass('hidden');
                cleanBookNotification.success(_L("ERROR_MSG_661"), "Qbicles");
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