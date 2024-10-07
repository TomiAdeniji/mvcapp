var loadCountPost = 1, loadCountAlert = 1, loadCountMedia = 1, loadCountEvent = 1;
var isServerBusy = false;


function LoadMoreAlerts(id) {
    $('button#btnLoadAlerts div.loading-state').removeClass('hidden');
    $('#btnLoadAlerts div.general-state').addClass('hidden');
    $.ajax({
        url: '/Community/LoadMoreAlerts',
        data: {
            id: id,
            size: loadCountAlert * 5
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (response) {
            $('button#btnLoadAlerts div.loading-state').addClass('hidden');
            $('#btnLoadAlerts div.general-state').removeClass('hidden');
            if (response === "") {
                $('#btnLoadAlerts').remove();
                return;
            }
            $('#previousAlerts').append(response).hide().fadeIn(250);
            loadCountAlert = loadCountAlert + 1;
        },
        error: function (er) {
            $('button#btnLoadAlerts div.loading-state').addClass('hidden');
            $('#btnLoadAlerts div.general-state').removeClass('hidden');
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}

function LoadMoreMedias(id) {
    $('button#btnLoadMedias div.loading-state').removeClass('hidden');
    $('#btnLoadMedias div.general-state').addClass('hidden');
    $.ajax({
        url: '/Community/LoadMoreMedias',
        data: {
            id: id,
            size: loadCountMedia * 5
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (response) {
            $('button#btnLoadMedias div.loading-state').addClass('hidden');
            $('#btnLoadMedias div.general-state').removeClass('hidden');
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#previousMedias').append(response).hide().fadeIn(250);
            loadCountMedia = loadCountMedia + 1;
        },
        error: function (er) {
            $('button#btnLoadMedias div.loading-state').addClass('hidden');
            $('#btnLoadMedias div.general-state').removeClass('hidden');
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}

function LoadMoreEvents(id) {
    $('button#btnLoadEvents div.loading-state').removeClass('hidden');
    $('#btnLoadEvents div.general-state').addClass('hidden');
    $.ajax({
        url: '/Community/LoadMoreEvents',
        data: {
            id: id,
            size: loadCountEvent * 5
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (response) {
            $('button#btnLoadEvents div.loading-state').addClass('hidden');
            $('#btnLoadEvents div.general-state').removeClass('hidden');
            if (response === "") {
                $('#btnLoadEvents').remove();
                return;
            }
            $('#previousEvents').append(response).hide().fadeIn(250);
            loadCountEvent = loadCountEvent + 1;
        },
        error: function (er) {
            $('button#btnLoadEvents div.loading-state').addClass('hidden');
            $('#btnLoadEvents div.general-state').removeClass('hidden');
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}

function LoadMorePosts(key) {
    $('#btnLoadPosts div.loading-state').removeClass('hidden');
    $('#btnLoadPosts div.general-state').addClass('hidden');
    $.ajax({
        url: '/Community/LoadMorePosts',
        data: {
            key: key,
            size: loadCountPost * 5
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (response) {
            $('#btnLoadPosts div.loading-state').addClass('hidden');
            $('#btnLoadPosts div.general-state').removeClass('hidden');
            if (response === "") {
                $('#btnLoadPosts').remove();
                return;
            }
            $('#previousPosts').append(response).hide().fadeIn(250);
            loadCountPost = loadCountPost + 1;
        },
        error: function (er) {
            $('#btnLoadPosts div.loading-state').addClass('hidden');
            $('#btnLoadPosts div.general-state').removeClass('hidden');
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}



