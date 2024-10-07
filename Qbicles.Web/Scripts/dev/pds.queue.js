
var $prepQueueIdDelete = 0;
var $queueType = 0;


function Search() {
    if ($('#queue-search').val().length > 3 || $('#queue-search').val().length === 0) {

        var ajaxUri = '/PDS/SearchQueue?name=' + $('#queue-search').val() + "&type=" + $("#filter-queue").val();
        AjaxElementLoad(ajaxUri, "queue-list");
    }
};


QueueAddEdit = function (id, type) {
    var ajaxUri = '/PDS/AddEditPrepQueue?id=' + id + '&type=' + type;
    AjaxElementShowModal(ajaxUri, "app-trader-ods-queue-add-edit");
};


function SavePrepQueue() {
    if (!$("#prep-queue-form").valid())
        return;

    $.LoadingOverlay("show");
    var $id = $("#prep-queue-id").val();
    var $prepQueueId = $("#prep-queue-select").val();

    $queueType = parseInt($("#queue-type").val());

    var url = "/DDS/CreateDdsQueue";
    
    if ($queueType === 2) {
        if ($prepQueueId === null || $prepQueueId === 0) {
            $("#prep-queue-form").validate().showErrors({ prepqueueselect: "This field is required." });
            LoadingOverlayEnd();
            return;
        }
        if ($id > 0)
            url = "/DDS/UpdateDdsQueue";
    } else {
        url = "/PDS/CreatePrepQueue";
        if ($id > 0)
            url = "/PDS/UpdatePrepQueue";
    }

    var prepQueue = {
        Id: $id,
        Name: $("#prep-queue-name").val(),
        PrepQueue: { id: $prepQueueId }
    };

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { queue: prepQueue },
        success: function (rs) {
            if (rs.actionVal === 1) {
                if ($id > 0) {
                    if ($queueType === 1)
                        $("#prep-queue-name-main-" + $id).text(prepQueue.Name);
                    else {
                        $("#dds-queue-name-main-" + $id).text(prepQueue.Name);
                        $("#dds-queue-main-" + $id).text($("#prep-queue-select option:selected").text());
                    }
                    cleanBookNotification.updateSuccess();
                }
                else {
                    $('#queue-list').append(rs.msg);
                    cleanBookNotification.createSuccess();
                }

                $('#app-trader-ods-queue-add-edit').modal('hide');

            } else {
                $("#prep-queue-form").validate().showErrors({ queuename: rs.msg });
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function ConfirmDeletePrepQueue(id, type) {
    $prepQueueIdDelete = id;
    $queueType = $queueType = parseInt(type);
    var name = "dds";
    if ($queueType === 1)
        name = "prep";


    $("#name-delete").text($("#" + name + "-queue-name-main-" + $prepQueueIdDelete).text());
    $("#confirm-delete").modal('show');
};

function CancelDelete() {
    $('#confirm-delete').modal('hide');
};

function DeletePrepQueue() {
    $.LoadingOverlay("show");
    var url = "/PDS/DeletePrepQueue";
    if ($queueType === 2)
        url = "/DDS/DeleteDdsQueue";


    $.ajax({
        type: "delete",
        url: url,
        data: { id: $prepQueueIdDelete },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                if ($queueType === 2)
                    $("#dds-queue-item-" + $prepQueueIdDelete).remove();
                else
                    $("#prep-queue-item-" + $prepQueueIdDelete).remove();
                $('#confirm-delete').modal('hide');
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


