function opensalesmarketing() {
    $.ajax({
        type: 'get',
        url: '/SalesMarketing/CheckSMSetup',
        datatype: 'json',
        success: function (response) {
            if (response) {
                window.location.href = '/SalesMarketing/SMApps';
            } else {
                window.location.href = '/SalesMarketing/SMSetup';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

