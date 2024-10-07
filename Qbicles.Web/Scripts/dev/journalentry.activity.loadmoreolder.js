
var loadCountPost = 1, loadCountMedia = 1;
function LoadMorePosts(activityId, pageSize,divId) {

    $.ajax({
        url: '/BKJournalEntries/LoadMoreJournalEntryPosts',
        data: {
            activityId: activityId,
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
            console.log(_L("ERROR_MSG_EXCEPTION_SYSTEM"));
        }
    });

}

function LoadMoreMedias(activityId, pageSize, divId) {
    $.ajax({
        url: '/BKJournalEntries/LoadMoreJournalEntryMedias',
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
            console.log(_L("ERROR_MSG_EXCEPTION_SYSTEM"));
        }
    });

}