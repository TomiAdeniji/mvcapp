
$.connection.hub.url = $("#connection-hub-uri").val();
var stocks = $.connection.broadcastUpdate;
var signalRParmaters = {};

var $queueNotify = [];

$(document).ready(function () {

    signalRParmaters = {
        $userPlaySound: $("#userPlaySound").val(),
        $userNotificationMethod: $("#UserNotificationMethod").val(),
        $NonNotification: $("#NonNotification").val(),
        $SoundNotification: $("#SoundNotification").val(),
        $dashboardpagedisplay: $("#dashboard-page-display"),
        $currentQbicleId: $('#hdfCurrentQbicleId'),
        $currentDomainId: $('#hdfCurrentDomainId')
    };

    var refreshIntervalId = 0;
    var timeToRefreshPage = 5000;
    refreshIntervalId = window.setInterval(function () {
        if (isEmpty() == false) {

            if (this.$queueNotify.length == 1)
                NewAlertNotification(dequeue(), true);
            else
                NewAlertNotification(dequeue(), false);

        }
    }, timeToRefreshPage);



});

var notificationEvent = {
    QbicleCreation: 1,
    QbicleUpdate: 2,
    DiscussionCreation: 3,
    DiscussionUpdate: 4, //This includes when any Post, Task, Event, Alert or File is added to a discussion
    TaskCreation: 5,
    TaskCompletion: 6,
    AlertCreation: 7,
    EventCreation: 8,
    EventWithdrawl: 9, // When a user indicates that they are not going to attend an event
    MediaCreation: 10,
    PostCreation: 11,
    ApprovalCreation: 12,
    CreateMember: 13,
    InvitedMember: 14,
    AlertUpdate: 15,
    ApprovalUpdate: 16,
    TaskUpdate: 17,
    EventUpdate: 18,
    TopicPost: 19,
    MediaUpdate: 20,
    ApprovalReviewed: 21,
    ApprovalApproved: 22,
    ApprovalDenied: 23,
    JournalPost: 24,
    TransactionPost: 25,
    LinkCreation: 26,
    RemoveUserOutOfDomain: 27,
    ReminderCampaignPost: 28,
    RemoveQueue: 29,
    QbicleInvited: 30,
    AssignTask: 31,
    C2CConnectionIssued: 32,
    C2CConnectionAccepted: 33,
    B2CConnectionCreated: 34,
    LinkUpdate: 35,
    ActivityComment: 36,
    ListingInterested: 37,
    RemoveUserOutOfQbicle: 38,
    AddUserParticipants: 39,
    RemoveUserParticipants: 40,
    AddUserToQbicle: 41,
    TypingChat: 66,
    EndTypingChat: 88,
    CreateRequest: 90,
    ProcessExtensionRequest: 91,
    B2COrderUpdated: 92,
    PostEdit: 93,
    B2COrderInvoiceCreationCompleted: 94,
    B2COrderBeginProcess: 95,
    B2COrderCompleted: 96,
    B2COrderPaymentApproved: 97,
    MediaRemoveVersion: 98,
    MediaAddVersion: 99,
    EventNotificationPoints: 42,
    TaskNotificationPoints: 43,
    TaskStart: 44,
    TaskComplete: 45,
    JoinToWaitlist: 102,
    ApprovalSubscriptionAndCustomWaitlist: 103,
    ApprovalSubscriptionWaitlist: 104,
    ApprovalCustomWaitlist : 105,
    RejectWaitlist: 106,
    UpdateMembersList: 108,
    B2BOrderCompleted: 110,
};

/**
  Object of chatting
 */
var $chatting = {
    NotConnected: "",
    Connected: "",
    ChatFrom: "",
    ChatFromId: "",
    Chatting: false,
    ChatElement: ""
}


function removeTypingElement() {
    var typeingOld = document.getElementsByClassName($chatting.ChatElement);
    while
        (typeingOld.length > 0) typeingOld[0].remove();
}

/**
  Render chatting after changed connect to another community
 */
function onCommunityConnected() {

    if (!$chatting.Chatting) return;
    removeTypingElement();
    if ($chatting.ChatFrom != $connectedC2C) {
        $('#' + $chatting.ChatElement).append($chatting.NotConnected);
    }
    else {
        htmlActivityRender($chatting.Connected, 0);
    }
}

var $originatingCreationId = "";
stocks.client.updateNotifications = function (broadcast) {
    //check only show on community chat..... NotificationId
    console.log('--------------broadcast-dev------------------');
    console.log(broadcast);
    console.log('--------------end broadcast-dev------------------');
    setTimeout(function () {
        $originatingCreationId = broadcast.OriginatingCreationId;

        if (broadcast.BroadcastEvent == notificationEvent.TypingChat) {

            var typeingOld = document.getElementsByClassName($chatting.ChatElement);
            if (typeingOld.length > 0)
                return;

            $chatting.ChatFromId = broadcast.CreatedId;
            $chatting.ChatElement = "typing-chat-" + $chatting.ChatFromId;
            $chatting.Chatting = true;
            $chatting.ChatFrom = broadcast.CreatedEmail;

            var typingHtml = "";
            if ($chattingType && $chattingType == "Order") {
                if ($discussionId != broadcast.NotificationId) return;
                typingHtml += "<article class='" + $chatting.ChatElement + " activity event_snippet animated fadeInUp' style='margin-bottom: 25px;'>";
                typingHtml += "<div class='activity-avatar tiny' style=\"background-image: url('" + broadcast.CreatedByIcon + "'); \"></div>";
                typingHtml += "<div class='activity-detail'>";
                typingHtml += "<a href='#'>";
                typingHtml += "<div class='activity-overview plain' style='padding: 0;'>";
                typingHtml += "<img src='/Content/DesignStyle/img/ellipsis.gif' class='typing tiny'>";
                typingHtml += "</div>";
                typingHtml += "</a>";
                typingHtml += "</div>";
                typingHtml += "<div class='clearfix'></div>";
                typingHtml += "</article>";
                $chatting.Connected = typingHtml;
                $('#list-comments-discussion').prepend($chatting.Connected);
            }
            else {
                $chatting.NotConnected = "<img class='" + $chatting.ChatElement + "' src='/Content/DesignStyle/img/ellipsis.gif' style='width: 50%;height: 50%;'>";

                typingHtml = "<article id='typing-" + $chatting.ChatElement + "' class='" + $chatting.ChatElement + " activity event_snippet animated bounceIn' style='margin-bottom: 35px;'>"
                typingHtml += "<div class='activity-avatar' style=\"background-image: url('" + broadcast.CreatedByIcon + "'); \"></div>";
                typingHtml += "<div class='activity-detail'>";
                typingHtml += "<a href='#'>";
                typingHtml += "<div class='activity-overview plain' style='padding: 0;'>";
                typingHtml += "<img src='/Content/DesignStyle/img/ellipsis.gif' class='typing'>";
                typingHtml += "</div>";
                typingHtml += "</a>";
                typingHtml += "</div>";
                typingHtml += "<div class='clearfix'></div>";
                typingHtml += "</article>";

                $chatting.Connected = typingHtml;
                if ($chatting.ChatFrom != $connectedC2C) {
                    $('#' + $chatting.ChatElement).append($chatting.NotConnected);
                }
                else {
                    htmlActivityRender($chatting.Connected, 0);
                }
            }
        }
        else if (broadcast.BroadcastEvent == notificationEvent.EndTypingChat) {
            removeTypingElement();
            $chatting = {
                NotConnected: "",
                Connected: "",
                ChatFrom: "",
                ChatFromId: "",
                Chatting: false,
                ChatElement: ""
            }
        }
        else if (broadcast.BroadcastEvent == notificationEvent.CreateRequest) {
            CheckPendingRequest();
        } else {

            getNotificationById(broadcast.NotificationId);
        }
    });


};

$.connection.hub.logging = true;
$.connection.hub.start({ transport: 'longPolling' }).done(function (refModel) {
    $.ajax({
        type: 'post',
        url: '/Qbicles/SetHubConnect?id=' + $.connection.hub.id,
        dataType: 'json',
        success: function (response) {
        }
    });
    // console.log('conn id ' + $.connection.hub.id);
}).fail(function (ex) {
    // console.log('connection.hub.start failed; ' + ex);
});
$.connection.hub.disconnected(function () {
    setTimeout(function () {
        $.connection.hub.start({ transport: 'longPolling' }).fail(function (ex) {
            // console.log('connection.hub.disconnected failed:' + ex);
        });
    }, 10000); // Restart connection after 10 seconds.
});
$.connection.hub.error(function (error) {
    //if (error.context && error.context.status == 401)
    //    location.reload();
    //console.log('SignalR connection.hub.error: ' + error);
});
function htmlActivityRender(htmlAppend, elementId) {
    if (htmlAppend) {
        //Remove Activity or Post when hangfire job come after qbiclestream page refresh
        var activityOld = document.getElementById("activity-" + elementId);
        if (activityOld)
            activityOld.remove();

        var postOld = document.getElementById("post-" + elementId);
        if (postOld)
            postOld.remove();
        //end
        var appendDashboard = document.getElementById("dashboard-date-today");
        //Convert Unicode emoji to image
        htmlAppend = convertUnicodeEmojiToImg(htmlAppend);
        if (appendDashboard !== null) {
            $('#dashboard-date-today-sub').remove();
            $("#dashboard-date-today").prepend("<div class=\"clearfix\"></div>" + htmlAppend + "<div class=\"clearfix\"></div>");
            $("#dashboard-date-today").prepend("<div id='dashboard-date-today-sub' class='day-date day-date-first'><span class='date'>Today</span></div>");
        }
        else {
            //$(".day-date").removeClass("day-date-first");          
            $(".day-block").removeClass("day-block-first");
            //var dashboardToday = "<div id=\"dashboard-date-today\" class=\"day-block day-block-first\" style=\"margin-bottom: 0;\"><div class=\"day-date\" style=\"display:block !important;border-top: 1px solid #e5e8e9 !important;\"><span class=\"date\">Today</span></div><div class=\"clearfix\"></div>" + "<div class=\"clearfix\"></div>" + htmlAppend + "<div class=\"clearfix\"></div></div>";
            var dashboardToday = "<div id=\"dashboard-date-today\" class=\"day-block day-block-first\" style=\"margin-bottom: 0;\"><div id='dashboard-date-today-sub' class=\"day-date day-date-first\"><span class=\"date\">Today</span></div>" + "<div class=\"clearfix\"></div>" + htmlAppend + "<div class=\"clearfix\"></div></div>";
            signalRParmaters.$dashboardpagedisplay.prepend(dashboardToday + "<div class=\"clearfix\"></div>");
        }
        //QBIC-2064: Remove Forward Option in post management("Discuss, Edit and Delete only.")
        setTimeout(function () {
            if ($('#forward-to-qbicle').length == 0) {
                $('.op-forward').remove();
            }
        }, 100);
    }
}
function getNotificationById(notifyId) {
    //console.log('notification', notifyId);
    if (!notifyId)
        return false;
    $.ajax({
        type: "get",
        dataType: 'json',
        //xhrFields: {
        //    withCredentials: true
        //},
        contentType: "application/json; charset=utf-8",
        url: '/Notifications/GetNotificationById',
        data: {
            id: notifyId
        }
    }).done(function (data) {

        var elementId = data.ElementId;

        if (data.CreatorTheQbcile != 1 || data.IsAlertDisplay) {//if notification not associcate from Qbicle, then show alert            
            enqueue(notifyId);
        }
        if (data.Event == notificationEvent.JoinToWaitlist) {
            CheckPendingWaitlist();
            return;
        }
        if (data.Event == notificationEvent.ApprovalSubscriptionAndCustomWaitlist || data.Event == notificationEvent.ApprovalSubscriptionWaitlist
            || data.Event == notificationEvent.ApprovalCustomWaitlist || data.Event == notificationEvent.RejectWaitlist) {
            CheckApprovalWaitlist(data.AssociatedById);
            return;
        }



        UpdateOrderStatus(data.Event, elementId);

        if (data.Event == notificationEvent.TaskStart || data.Event == notificationEvent.TaskComplete) {
            if (data.IsCurrentQbicle && data.IsCurrentTask) {
                setTimeout(function () {
                    window.location.reload();
                }, 1500);
                return;
            }
        }
        if (data.Event == notificationEvent.RemoveUserOutOfDomain && data.IsCurrentAssociator) {
            cleanBookNotification.error('Administrator have been removed you from the Domain', 'Qbicles');
            setTimeout(function () {
                window.location.href = '/';
            }, 1000);

            return;
        }
        else if (data.Event == notificationEvent.RemoveUserOutOfQbicle && data.IsCurrentAssociator) {
            cleanBookNotification.error('Administrator have been removed you from the Qbicle', 'Qbicles');
            setTimeout(function () {
                window.location.href = '/Qbicles';
            }, 1000);

            return;
        }

        else if (data.Event == notificationEvent.UpdateMembersList){
            UpdatePeopleData();
        }
        else if (data.Event == notificationEvent.B2BOrderCompleted) {
            UpdateB2BOrderStatus();
        }
        else {
            var htmlAppend = data.HtmlNotification;
            var appendToPageName = data.AppendToPageName;


            var playSound = false;
            if (appendToPageName === "Qbicle" && signalRParmaters.$dashboardpagedisplay.length > 0 && signalRParmaters.$currentQbicleId.val() == data.CurrentQbicleKey) {
                isDisplayFlicker(false);
                htmlActivityRender(htmlAppend, elementId);
                qbicleStreamToTop();
                playSound = true;
                return;
            }
            // append Qbicle page Domain in case Create QBicle with member/Edit Qbicle add member
            else if (appendToPageName == "Domain" && signalRParmaters.$currentPage === "Qbicles"
                && data.IsCurrentDomain
            ) {
                var qbicleOld = document.getElementById("domain-qbicle-" + elementId);
                if (qbicleOld)
                    qbicleOld.remove();

                isDisplayFlicker(false);
                $("#qbicles-dash-grid").prepend(htmlAppend);
                playSound = true;
            }
            // append Activity, chat on dashboard qbicle/b2b/b2c to Qbicle page stream
            else if (appendToPageName == "Activities" && signalRParmaters.$dashboardpagedisplay.length > 0 && data.IsCurrentQbicle
            ) {
                removeTypingElement(data.CreatedById);
                //console.log('remove by signalR');
                isDisplayFlicker(false);
                htmlActivityRender(htmlAppend, elementId);
                qbicleStreamToTop();
                playSound = true;
            }
            // append reload page till detail
            else if (appendToPageName == "Activities" && $("#isReloadPageNeeded").length > 0 && data.IsCurrentQbicle && window.location.href.includes("app-trader-pos-cash-device")
            ) {
                var queryString = window.location.search;
                var urlParams = new URLSearchParams(queryString);
                var tillId = urlParams.get('tillId');
                ShowTillDetailTable(tillId)
            }
            // append Activity, chat on dashboard qbicle/b2b/b2c to Qbicle page stream
            else if (appendToPageName == "bookkeeping") {
                //transaction-comments
                if (htmlAppend.indexOf("appentTransaction") !== -1) {
                    //var id = readCookie('transaction-cookie');
                    $('#transaction-comments-' + elementId).prepend(htmlAppend);
                    var currentCount = $("#comment-" + elementId).text();
                    currentCount = parseInt(currentCount) + 1;
                    $("#comment-" + elementId).text(currentCount);
                }
                else
                    $('#list-comments-journal').prepend(htmlAppend);
                isPlaceholder(false, '');
            }
            else if (appendToPageName == "Activity") {
                //if (data.Event == notificationEvent.MediaCreation) {
                //    $('#list-medias').prepend(data.HtmlNotification);
                //    playSound = true;
                //}
                if ((data.Event == notificationEvent.MediaAddVersion || data.Event == notificationEvent.MediaRemoveVersion) && data.IsCurrentMedia) {
                    playSound = true;
                    setTimeout(function () {
                        window.location.reload();
                    }, 5000);

                }
                else {
                    $('#list-medias').prepend(data.HtmlNotification);
                    playSound = true;
                }
            }
            else if (appendToPageName == "B2C Order" || appendToPageName == "B2B Order") {
                UpdateOrderStatus(data.Event, elementId);
            }
            // ----------- Activity Comment ---------------------
            else {
                if (appendToPageName == "Activities") return;
                var postOld = document.getElementById("post-" + elementId);
                if (postOld)
                    postOld.remove();

                if (data.IsCurrentTask) {
                    $('#list-comments-task').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentAlert) {
                    $('#list-comments-alert').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentEvent) {
                    $('#list-comments-event').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentMedia) {
                    $('#list-comments-media').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentLink) {
                    $('#list-comments-link').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentApproval) {
                    $('#list-comments-approval').prepend(htmlAppend);
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentDiscussion) {
                    $('#list-comments-discussion').prepend(htmlAppend);
                    //Check to handle Action with attribute on discussion order (B2C/C2C/B2B)
                    //Using value of an attribute named discussionType
                    var discussionPageType = $('#list-comments-discussion').attr("discussionType");
                    //if (discussionPageType && data.HasActionToHandle && !data.IsCurrentCreator) {
                    if (discussionPageType && data.HasActionToHandle && $originatingCreationId != data.CurrentConnectionId) {
                        if (discussionPageType == "orderdiscussion-business") {
                            reloadDataTable();
                        }
                        else if (discussionPageType == "orderdiscussion-customer") {
                            showOrderCartB2C();
                        }
                        else if (discussionPageType == 'b2b-buyingdomain' || discussionPageType == 'b2b-sellingdomain') {
                            reloadOrderStatus();
                        }
                    }
                    //b2b Check to handle Action with attribute
                    if (data.HasActionToHandle && $('#hdfRelationshipId').length > 0 && !data.IsCurrentCreator) {
                        refreshLogisticsView();
                    }
                    //end
                    isPlaceholder(false, '');
                }
                else if (data.IsCurrentJournalEntry) {
                    //transaction-comments
                    if (htmlAppend.indexOf("appentTransaction") !== -1) {
                        //var id = readCookie('transaction-cookie');
                        $('#transaction-comments-' + elementId).prepend(htmlAppend);
                        var currentCount = $("#comment-" + elementId).text();
                        currentCount = parseInt(currentCount) + 1;
                        $("#comment-" + elementId).text(currentCount);
                    }
                    else
                        $('#list-comments-journal').prepend(htmlAppend);
                }
                playSound = true;
            }


            if (signalRParmaters.$userNotificationMethod !== signalRParmaters.$NonNotification) {
                if (data.IsCurrentCreator) {
                    if (data.Event !== notificationEvent.MediaCreation) {
                        CheckUnreadNotifications();
                        playSound = true;
                    }

                    if (data.Event == notificationEvent.QbicleInvited) {
                        var count = $("#lblCountInvitation-" + appendToPageName).text();
                        var countNew = parseInt(count) + 1;
                        $("#lblCountInvitation-" + appendToPageName).text(countNew);

                        $('#lblCountInvitation-' + appendToPageName).show();
                    }
                    //notification sound play
                    if (signalRParmaters.$userPlaySound === signalRParmaters.$SoundNotification && playSound === true)
                        $('#notify-sound')[0].play();
                }
            }
            //b2b
            //b2c move updated qbicles to the top - B2C Communications
            b2COrderByQbicle(data.CurrentQbicleKey);
            if (!$("li[data-b2cqbicleid=" + data.CurrentQbicleKey + "]").hasClass("active")) {
                $("li[data-b2cqbicleid=" + data.CurrentQbicleKey + "] a .comms-newstuff").removeAttr("hidden");
            }
        }
    });
}
/**
 * b2c move updated qbicles to the top - B2C Communications
 * @param {any} qbicleKeyOfNotification
 */
function b2COrderByQbicle(qbicleKeyOfNotification) {
    var $orderBy = $("select[name='orderBy']");
    if ($orderBy != null) {
        if ($orderBy.val() == 0) {
            var _listQbicleKey = [];
            $('li[data-b2cqbicleid]').each(function (index) {
                _listQbicleKey.push($(this).data("b2cqbicleid"));
            });
            $('li[data-c2cqbicleid]').each(function (index) {
                _listQbicleKey.push($(this).data("c2cqbicleid"));
            });

            $.post("/B2C/MapQbicleKeyIsUpdated", { currentKey: qbicleKeyOfNotification, listQbicleKey: _listQbicleKey }, function (data) {
                if (data) {
                    var $b2cqb = $("ul li[data-b2cqbicleid=" + data + "]");
                    if ($b2cqb != null && $b2cqb.index() != 0) {
                        $b2cqb.slideToggle(500, function () {
                            $b2cqb.prependTo($b2cqb.parent()).slideToggle(500);
                        });
                    }

                    if ($b2cqb.length > 0 && !$b2cqb.hasClass('active')) {
                        $b2cqb.find(".comms-newstuff").removeAttr("hidden");
                        //var messNum = Number($b2cqb.find(".counter").text());
                        //$b2cqb.find(".counter").text(messNum + 1);
                    };

                    var $c2cqb = $('li[data-c2cqbicleid="' + data + '"]');
                    if ($c2cqb.length > 0 && !$c2cqb.hasClass('active')) {
                        $c2cqb.find(".comms-newstuff").removeAttr("hidden");
                    }
                }
            });
        }
    }
}

function UpdateOrderStatus(event, elementId) {

    //console.log('UpdateOrderStatus event:' + event + 'event id: ' + elementId + 'trader id: ' + $("#tradeorderid").val());
    var currentOrder = _.toNumber($("#tradeorderid").val()) == _.toNumber(elementId);
    if (!currentOrder) return;

    try {
        $("#order-context-flyout-table").DataTable().ajax.reload(null, false);
    } catch (e) {

    }
    //OrderProcessedCheck();
    showOrderCartB2C(false);
    reloadStatusSubmit();

    if (event == notificationEvent.B2COrderInvoiceCreationCompleted) {
        //UpdateB2COrderStatus(2);

        //console.log('DisplayPaymentTabToCustomer: B2COrderInvoiceCreationCompleted-', event);
        //console.log('showOrderCartB2C: B2COrderInvoiceCreationCompleted-', event);

        //$("#order-li-tab").remove();
        //$("#tab0").remove();
        DisplayPaymentTabToCustomer();
    }
    else if (event == notificationEvent.B2COrderBeginProcess) {
        //UpdateB2COrderStatus(1);
        //remove order tab from Customer

        //console.log('B2COrderBeginProcess: Remove order tab-', event);

        //$("#order-li-tab").remove();
        //$("#tab0").remove();
         //remove order tab from Customer
        $("#order-li-tab").remove();
        $("#tab0").remove();
        //$("#tab0").removeClass().addClass('hidden');
        //$("#order-li-tab").removeClass().addClass('hidden');
        //$("#order-a-tab").removeClass().addClass('hidden');

        $("#cart-li-tab").addClass('active');
        $("#order-cart-tab").addClass('in active');
    }
    else if (event == notificationEvent.B2COrderCompleted) {
        //UpdateB2COrderStatus(3);
        //console.log('showOrderCartB2C: B2COrderCompleted-', event);
        

    }
    else if (event == notificationEvent.B2COrderPaymentApproved) {

        //console.log('DisplayPaymentTabToCustomer: B2COrderPaymentApproved-', event);
        //console.log('showOrderCartB2C: B2COrderPaymentApproved-', event);

        //$("#order-li-tab").remove();
        //$("#tab0").remove();
        DisplayPaymentTabToCustomer();

        //OrderProcessedCheck(true);
        //reloadStatusSubmit();
    }
    else {
        //console.log('UpdateOrderStatus event - false: ', event);
    }


}

function convertUnicodeEmojiToImg(val) {

    //Convert Unicode emoji to image
    try {
        if (jQuery().emojioneArea && typeof emojione !== 'undefined') {
            return emojione.unicodeToImage(val);
        } else
            return val;
    } catch {
        return val;
    }



    //for (var i = 1; i <= 5; i++) {
    //    q.enqueue(i);
    //}
}



/**
 * add an element to the end of the Queue
 * @param {any} item
 */
function enqueue(item) {
    this.$queueNotify.push(item);
}
/**
 * get an element at the beginning of the Queue
 * */
function dequeue() {
    return this.$queueNotify.shift();
}

//function front() {
//    if (this.$queueNotify.length == 0) return undefined;

//    return this.$queueNotify[0];
//}

//function rear() {
//    if (this.$queueNotify.length == 0) return undefined;

//    return this.$queueNotify[this.size - 1];
//}

function isEmpty() {
    return this.$queueNotify.length == 0;
}
