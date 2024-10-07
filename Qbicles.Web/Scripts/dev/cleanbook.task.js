function ViewTransactionMatchingReport(accountName, accountId, accountName2, accountId2, taskid, taskname, transactionMatchingTypeId, transactionmatchingtaskId, taskInstanceId, userName, date, time, countTransactionsMatched, countTransactionsManual, countTransactionsUnMatched) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/Tasks/RedirectTransactionMatchingReport',
        dataType: 'json',
        data: {
            accountName: accountName,
            accountId: accountId,
            accountName2: accountName2,
            accountId2: accountId2,
            taskid: taskid,
            taskname: taskname,
            transactionMatchingTypeId: transactionMatchingTypeId,
            transactionmatchingtaskId: transactionmatchingtaskId,
            taskInstanceId: taskInstanceId,
            userName: userName,
            date: date,
            time: time,
            countTransactionsMatched: countTransactionsMatched,
            countTransactionsManual: countTransactionsManual,
            countTransactionsUnMatched: countTransactionsUnMatched
        },
        success: function (response) {
            if (response === true)
                window.location.href = "/TransactionMatching/TransactionMatchingReport";
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function ViewExcuteTransaction(accountName, accountId, accountName2, accountId2, taskid, taskname, transactionMatchingTypeId, transactionmatchingtaskId, taskInstanceId) {

    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/Tasks/RedirectTransactionMatching',
        dataType: 'json',
        data: {
            accountName: accountName,
            accountId: accountId,
            accountName2: accountName2,
            accountId2: accountId2,
            taskid: taskid,
            taskname: taskname,
            transactionMatchingTypeId: transactionMatchingTypeId,
            transactionmatchingtaskId: transactionmatchingtaskId,
            taskInstanceId: taskInstanceId
        },
        success: function (response) {
            if (response === true) {
                window.location.href = "/TransactionMatching/TransactionMatchingRecords";
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
