var CountPost = 1, CountMedia = 1, busycomment = false;
function validateAddComment() {
    var message = $('#txt-comment-link').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function AddCommentToDiscussion(discussionKey) {
    if (busycomment)
        return;
    $('.newcomment').addClass('reprisedcomments');

    var message = $('#txt-comment-link');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddComment2Discussion",
            data: { message: message.val(), disKey: discussionKey },
            type: "POST",
            success: function (response) {
                if (response.result) {
                    message.val("");

                    if (response.msg != '') {
                        $('#list-comments-discussion').prepend(response.msg);
                        isDisplayFlicker(false);
                    }

                }
                busycomment = false;
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                isPlaceholder(false, '');
                busycomment = false;
            }
        });
    }
}
function LoadMorePostsDiscussion(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: CountPost * pageSize
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
            $('#' + divId).append(response).fadeIn(250);
            CountPost = CountPost + 1;

        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function LoadMoreMediasDiscussion(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: CountMedia * pageSize
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
            $('#' + divId).append(response).fadeIn(250);
            CountMedia = CountMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });
}
function ContactsParticipants() {
    $.get("/Qbicles/DiscussionParticipants", { disId: $('#hdfDisId').val(), keyword: $('#txtSearchContacts').val() }, function (data) {
        $('#contacts-participants').html(data);
    });
}
function ContactsQbicle()
{
    $.get("/Qbicles/SearchQbicleUsersInviteDiscussion", { disId: $('#hdfDisId').val(), keyword: $('#txtSearchUsersQbicle').val() }, function (data) {
        $('#contacts-participants-add').html(data);
    });
}
function ParticipantsDetail(uId) {
    $.get("/Qbicles/DisContactDetail", { disId: $('#hdfDisId').val(), uId: uId }, function (data) {
        $('#dis-contact-detail').html(data);
    });
}
function ParticipantsDetailAdd(uId) {
    $.get("/Qbicles/DisContactDetailAdd", { disId: $('#hdfDisId').val(), uId: uId }, function (data) {
        $('#dis-contact-detail-add').html(data);
    });
}
function RemoveMemberDis(disId,uId)
{
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("ERROR_MSG_395"),
        callback: function (result) {
            if (result) {
                $.post("/Qbicles/RemoveContactDiscussion", { disId: disId, uId: uId }, function (data) {
                    if(data.result)
                    {
                        ContactsParticipants();
                        $('.contact').hide();
                        $('.contact-list').fadeIn();
                    }else
                    {
                        cleanBookNotification.error(_L("ERROR_MSG_394")); 
                    }
                });
                return;
            }
        }
    });
    
}
function AddMemberDis(disId, uId) {
    $.post("/Qbicles/AddContactDiscussion", { disId: disId, uId: uId }, function (data) {
        if (data.result) {
            $('#btnAddedInviteUser').show();
            ContactsParticipants();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_396"));
        }
    });
}
function SearchThrottle(f, delay) {
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
$(document).ready(function () {
    $('#txtSearchContacts').keyup(SearchThrottle(function () {
        ContactsParticipants();
    }));
    $('#txtSearchUsersQbicle').keyup(SearchThrottle(function () {
        ContactsQbicle();
    }));
});