var loadCountPost = 1, loadCountMedia = 1,busycomment=false;
function LoadMorePosts(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: loadCountPost * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadPosts').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            loadCountPost = loadCountPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function LoadMoreMedias(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: loadCountMedia * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            loadCountMedia = loadCountMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function validateAddComment() {
    var message = $('#txt-comment-event').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function addCommentToEvent(eventkey) {
    if (busycomment)
        return;
    var message = $('#txt-comment-event');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-event');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentToEvent",
            data: { message: message.val(), eventKey: eventkey },
            type: "POST",
            success: function (response) {
                
                if (response.result) {
                    message.val("");

                    if (response.msg != '') {
                        $('#list-comments-event').prepend(response.msg);
                        isDisplayFlicker(false);
                    }
                }
                busycomment = false;
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                isPlaceholder(false, '');
                busycomment = false;
            }
        });
    }
    
}
function Attending(currentPeobleId,isPresent)
{
    $.ajax({
        url: "/Events/UpdateAttend",
        data: { peopleId: currentPeobleId, isPresent: isPresent },
        type: "POST",
        success: function (data) {
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_92"), "Qbicles");
                if (isPresent)
                {
                    $('#going').show();
                    $('#p_' + currentPeobleId).show();
                    $('.att_no').removeClass('active');
                    $('.att_yes').addClass('active');
                }
                else
                {
                    $('#going').hide();
                    $('#p_' + currentPeobleId).hide();
                    $('.att_no').addClass('active');
                    $('.att_yes').removeClass('active');
                }
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}