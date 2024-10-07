$(document).ready(function () {
    createDiscussion();
});
function createDiscussion() {
    $('#frm-create-discussion').submit(function (e) {
        e.preventDefault();
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Discussions/SaveDiscussionForComplianceTask',
            datatype: 'json',
            data: { taskId: $('#complianceTaskId').val(), openingmessage: $('#ds_openingmessage').val(), isexpiry: $('#ds_isexpiry').prop('checked'), expirydate: $('#ds_expirydate').val() },
            beforeSend: function (xhr) {
                isBusy = true;
            },
            success: function (data) {
                if (data.result) {
                    $('.new-discuss').hide();
                    var elbtnDis = $('#btnJoinDiscussion');
                    if (data.Object.Id > 0) {
                        var elhref = elbtnDis.attr("href") + "?disId=" + data.Object.Id;
                        elbtnDis.attr("href", elhref);
                        elbtnDis.show();
                    }
                    cleanBookNotification.success(_L("ERROR_MSG_196"), "Operator");
                    $('#create-discussion').modal('hide');
                } else if (data.msg) {
                    cleanBookNotification.error(data.msg, "Operator");
                }
                isBusy = false;
                LoadingOverlayEnd();
            },
            error: function (err) {
                isBusy = false;
                LoadingOverlayEnd();
            }
        });
    });
}