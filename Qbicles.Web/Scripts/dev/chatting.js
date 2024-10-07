/**
 * Type of chat: Qbicle, Order, B2C, B2B, Community
 * */
var $chattingType = $("#chatting-type").val();
/**
 * Button send chat in Qbicle/B2C/B2B/Community dashboard
 * */
var $messageButton = $("#button-post-submit");
/**
 * Chat box element in Qbicle/B2C/B2B/Community dashboard
 * */
var $messageChat = $('#post-text');
/**
 * Order Discussion Id
 * if chat from Qbicle/Community/B2C,B2B then DiscussionId=0
 * */
var $discussionId = $("#order-discussion-id").val();
/**
 * 5 seconds will send
 * */
var $throttleTime = 5000;
/**
 * call to signalR if is true
 * */
var $canSend2SignalR = true;
/**
 * timer identifier to stop typing
 * */
var $typingTimer;
/**
 * time in waiting typing stop (30 seconds)
 * */
var $doneTypingInterval = 20000;

$messageChat.keyup(function () {
    var _message = $messageChat.val();
    if (_message && _message.length > 0) {
        if (_message.length <= 1000) {
            $messageButton.attr('disabled', false);
        } else {
            $messageButton.attr('disabled', true);
        }
    } else {
        $messageButton.attr('disabled', true);
    }



    if ($chattingType != "C2C") {
        return;
    }
    clearTimeout($typingTimer);

    if (_message && _message.length > 0) {
        if ($canSend2SignalR) {
            TypingNotification(true);
        }
    } else {
        TypingNotification(false);
    }

    if (!$canSend2SignalR)
        $typingTimer = setTimeout(StopTyping, $doneTypingInterval);


});


/**
 * user is "stop typing,"
 * */
function StopTyping() {

    TypingNotification(false);
    clearTimeout($typingTimer);
}
/**
 * Send a notification while user typing on chat box
 * @param {boolean} typing true: chatting - false: end chat
 */
function TypingNotification(typing) {

    if (typing) {
        $canSend2SignalR = false;
        setTimeout(function () { $canSend2SignalR = true; }, $throttleTime);
    }

    var toUsers = "";
    if ($chattingType == "Order")
        toUsers = $("#order-chat-to-users").val();
    else
        toUsers = $connectedC2C;

    $.ajax({
        type: 'GET',
        url: '/QbicleComments/SignalRTyping?toUsers=' + toUsers + '&typing=' + typing + '&discussionId=' + $discussionId,
        dataType: 'json',
        success: function (response) {
            
        },
        error: function (er) {
            
        }
    }).always(function () {
    });
}


$messageChat.keypress(function (e) {
    var key = e.which;
    if (key === 13) // the enter key code
    {
        clearTimeout($typingTimer);

        if ($chattingType == 'Qbicle')
            ChatQbicle();
        else
            ChatCommunity();
    }
});


/**
 * Chat from community dashboard
 * */
function ChatCommunity() {
    var message = $('#post-text').val();
    var validTag = /(\<\w*)((.*\>)|(.*\<\/\w*\>)|(\>))/gm;
    if (validTag.test(message)) {
        cleanBookNotification.error(_L("ERROR_MSG_INVALID_POST"), "Qbicles");
        return;
    }
    isDisplayFlicker(true);
    resetPostInput();
    var topicName = 'General';
    $.ajax({
        url: "/QbicleComments/AddPostToTopic",
        data: { message: message, topicName: topicName },
        type: "POST",
        success: function (data) {
            if (data.result) {
                if (data.msg != '') {
                    isDisplayFlicker(false);
                    htmlActivityRender(data.msg, 0);
                }
                resetPostInput();
                qbicleStreamToTop();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_133"), "Qbicles");

            }
        },
        error: function (error) {
            cleanBookNotification.error(error, "Qbicles");
            $('.general-state').show();
            $('.loading-state').hide();
            isDisplayFlicker(false);
        }
    });
}

/**
 * Add post chat from Qbicles Dashboard
 * */
function ChatQbicle() {

    var message = $('#post-text').val();
    var validTag = /(\<\w*)((.*\>)|(.*\<\/\w*\>)|(\>))/gm;
    if (validTag.test(message)) {
        cleanBookNotification.error(_L("ERROR_MSG_INVALID_POST"), "Qbicles");
        return;
    }
    isDisplayFlicker(true);
    resetPostInput();
    var topicId = $("#toppic-value").val();
    $.ajax({
        url: "/QbicleComments/AddPostWithTopicId",
        data: { message: message, topicId: topicId },
        type: "POST",
        success: function (data) {
            if (data.result) {
                if (data.msg != '') {
                    isDisplayFlicker(false);
                    htmlActivityRender(data.msg, 0);
                }
                resetPostInput();
                SetTopic(topicId);
                qbicleStreamToTop();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_133"), "Qbicles");

            }
        },
        error: function (error) {
            cleanBookNotification.error(error, "Qbicles");
            $('.general-state').show();
            $('.loading-state').hide();
            isDisplayFlicker(false);
        }
    });
};

function qbicleStreamToTop() {
    var elDashboard = $('#dashboard-page-display');
    if ($(document).width() > 768 && elDashboard) {
        $(window).scrollTop(0);
    }
};

//------------------------ discussion order -----------------

/**
 * Chat box element in Order
 * */
var $messageOrderChat = $('#txt-comment-order');
/**
 * Button send chat element in Order
 * */
var $messageOrderButton = $('#message-order-button');



$messageOrderChat.keyup(function () {

    clearTimeout($typingTimer);

    if ($messageOrderChat.val() && $messageOrderChat.val().length > 0) {
        if ($messageOrderChat.val().length > 1500) {
            $('#addcomment-error').show();
            $messageOrderButton.attr('disabled', true);
        }
        else {
            $('#addcomment-error').hide();
            $messageOrderButton.attr('disabled', false);
        }

        if ($canSend2SignalR) {
            TypingNotification(true);
        }
    } else {
        $messageOrderButton.attr('disabled', true);
        TypingNotification(false);
    }

    if (!$canSend2SignalR)
        $typingTimer = setTimeout(StopTyping, $doneTypingInterval);
});

/**
 * Send chat/comment from B2C order
 * @param {string} discussionKey Key of discussion
 */
function ChatFromOrder(discussionKey) {

    if ($messageOrderChat.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');

        $messageOrderButton.attr('disabled', true);

        $.ajax({
            url: "/QbicleComments/AddComment2DiscussionOrder",
            data: { message: $messageOrderChat.val(), disKey: discussionKey },
            type: "POST",
            success: function (res) {
                if (res.result) {
                    if (res.msg != '') {
                        isDisplayFlicker(false);
                        //htmlActivityRender(res.msg, 0);
                        $('#list-comments-discussion').prepend(res.msg);
                    }
                    $messageOrderChat.val("");
                }
                $messageOrderButton.attr('disabled', false);
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                isPlaceholder(false, '');

            }
        }).always(function () {
            $messageOrderButton.attr('disabled', false);
        });;
    }
}
function initEmoji(elmselector) {


    var el = $(elmselector).emojioneArea({
        inline: true,
        pickerPosition: "bottom",
        autocomplete: true,
        events: {
            keyup: function (editor, event) {
                let message = $(elmselector)[0].emojioneArea.getText();
                console.log(message);
                $(elmselector).val(message).trigger('keyup');
                if (event.which === 13) {
                    $('#button-post-submit').trigger('click');
                }
            },
        }
    });

    el[0].emojioneArea.on("emojibtn.click", function (btn, event) {
        $messageButton.attr('disabled', false);
        //console.log(btn.html());
    });
}

function resetPostInput() {
    let $inputPost = $messageChat[0].emojioneArea;
    if ($inputPost) {
        $messageChat[0].emojioneArea.setText('');
        $messageChat.trigger('keyup');
    } else
         $messageChat.val('');
}

//------------------------ discussion item associted order -----------------

function ChatFromProviderB2B(discussionKey) {
    var $messageItemChat = $("#txt-comment-item");
    var $messageItemButton = $("#btn-comment-item");
    var $headingComment = $(".heading-comment");
    var $bulkMessage = [];
    $bulkMessage.push($headingComment.text(),$messageItemChat.val());
    if ($messageItemChat.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');

        $messageItemButton.attr('disabled', true);

        $.ajax({
            url: "/QbicleComments/AddBulkComment2DiscussionOrder",
            data: { messages: $bulkMessage, disKey: discussionKey },
            type: "POST",
            success: function (res) {
                if (res.result) {
                    if(res.Object != null){
                        isDisplayFlicker(false);
                        res.Object.forEach(element => {
                            $('#list-comments-discussion').prepend(element);
                        });
                    }
                    $messageItemChat.val("");
                }
                $messageItemButton.attr('disabled', false);
                $("#b2b-seller-add-item").modal("hide");
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                isPlaceholder(false, '');
            }
        }).always(function () {
            $messageItemButton.attr('disabled', false);
        });;
    }
}