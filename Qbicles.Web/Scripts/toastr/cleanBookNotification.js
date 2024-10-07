(function () {
    cleanBookNotification = {
        options: {
            isAlert: 0,
            content: "update success",
            title: "update data",
            type: ''
        },
        clearmessage: function() {
            toastr.clear();
        },
        info: function (mes, title) {
            toastr.options = {
                showDuration: 400,
                timeOut: 5000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            toastr.info(mes, title);
        },
        success: function (mes, title) {
            toastr.options = {
                showDuration: 400,
                timeOut: 5000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            toastr.success(mes, title);
        },
        warning: function (mes, title) {
            toastr.options = {
                showDuration: 400,
                timeOut: 10000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            toastr.warning(mes, title);
        },
        error: function (mes, title) {
            toastr.options = {
                showDuration: 400,
                timeOut: 20000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            toastr.error(mes, title);
        },
        requiredData: function (message) {
            toastr.options = {
                showDuration: 400,
                timeOut: 5000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            if (message == null) {
                toastr.warning(_L("ERROR_MSG_658"), "Qbicles");
            } else {
                toastr.warning(message, "Qbicles");
            }
        },
        isvalidData: function (message) {
            toastr.options = {
                showDuration: 400,
                timeOut: 5000,
                progressBar: true,
                closeButton: true,
                newestOnTop: true
            };
            if (message == null) {
                toastr.warning(_L("ERROR_MSG_659"), "Qbicles");
            } else {
                toastr.warning(message, "Qbicles");
            }
        },
        updateSuccess: function () {
            cleanBookNotification.success(_L("ERROR_MSG_656"), "Qbicles");
        },
        createSuccess: function () {
            cleanBookNotification.success(_L("ERROR_MSG_657"), "Qbicles");
        },
        createFail: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        UpdateFail: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        removeSuccess: function () {
            cleanBookNotification.success(_L("ERROR_MSG_655"), "Qbicles");
        },
        removeFail: function () {
            cleanBookNotification.warning(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        dataError: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        errordateupload: function (mes, title) {
            toastr.options = {
                showDuration: 400,
                timeOut: 0,
                extendedTimeOut: 0,
                progressBar: false,
                closeButton: true,
                newestOnTop: true
            };
            toastr.error(mes, title);
        },
        //errorValue: 1: Insert success,2- Update succes,3 - Delete succes, 4 -bug error
        displayNotification: function (errorValue) {
            switch (errorValue) {
                case "1":
                    cleanBookNotification.success(_L("ERROR_MSG_657"), "Qbicles");
                    break;
                case "2":
                    cleanBookNotification.success(_L("ERROR_MSG_656"), "Qbicles");
                    break;
                case "3":
                    cleanBookNotification.warning(_L("ERROR_MSG_655"), "Qbicles");
                    break;
                case "4":
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    break;
            }
        },
       
    }
})()