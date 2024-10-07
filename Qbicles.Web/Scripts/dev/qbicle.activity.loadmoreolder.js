
var loadCountPost = 1, loadCountMedia = 1;
function LoadMorePosts(activityKey, pageSize,divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
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
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}


function LoadMoreMedias(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
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
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function validFormAlert() {
    var valid = true;
    if ($("#alert_edit_name").val() === "") {
        valid = false;
        $("#edit_alert_form").validate().showErrors({ name: "This field is required." });
    }
    if ($("#alert_edit_priority").val() === "") {
        valid = false;
        $("#edit_alert_form").validate().showErrors({ priority: "This field is required." });
    }
    if ($("#alert_edit_content").val() === "") {
        valid = false;
        $("#edit_alert_form").validate().showErrors({ content: "This field is required." });
    }
    return valid;
}
function updateAlert() {
    if (validFormAlert()) {
        var alert = 
        {
            Id: $('#alert_edit_id').val(),
            Priority: $('#alert_edit_priority').val(),
            Content: $('#alert_edit_content').val(),
            Name: $('#alert_edit_name').val()
        }

        $.ajax({
            type: 'post',
            url: '/Alerts/UpdateAlert',
            datatype: 'json',
            data: {
                alert: alert
            },
            success: function (refModel) {
                if (refModel.actionVal == 1) {
                    window.location.href = '/Qbicles/Alert';
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }
        });
    }
}
// Event
function validFormEvent() {
    var valid = true;
    if ($("#edit_event_name").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ name: "This field is required." });
    }
    if ($("#event_edit_priority").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ priority: "This field is required." });
    }
    if ($("#edit_eventStart").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ EventStart: "This field is required." });
    }
    if ($("#edit_eventEnd").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ EventEnd: "This field is required." });
    }
    if ($("#event_edit_location").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ location: "This field is required." });
    }
    if ($("#event_edit_description").val() === "") {
        valid = false;
        $("#form_event_edit").validate().showErrors({ Description: "This field is required." });
    }
    return valid;
}
function UpdateEvent() {
    if (validFormEvent()) {
        var event =
        {
            Id: $('#edit_event_id').val(),
            EventType: $('#event_edit_priority').val(),
            Description: $('#event_edit_description').val(),
            Name: $('#edit_event_name').val(),
            Location: $('#event_edit_location').val(),
            StartedDate: $('#edit_eventStart').val(),
            End: $('#edit_eventEnd').val()
        }

        $.ajax({
            type: 'post',
            url: '/Events/UpdateEvent',
            datatype: 'json',
            data: {
                qEvent: event
            },
            success: function (refModel) {
                if (refModel.actionVal == 1) {
                    window.location.href = '/Qbicles/Event';
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }
        });
    }
}

// Task
function ValidateTask() {
    var valid = true;
    if ($("#edit_task_name").val() === "") {
        valid = false;
        $("#form_edit_task").validate().showErrors({ name: "This field is required." });
    }
    if ($("#task_edit_priority").val() === "") {
        valid = false;
        $("#form_edit_task").validate().showErrors({ priority: "This field is required." });
    }
    if ($("#task_edit_recurring").val() === "") {
        valid = false;
        $("#form_edit_task").validate().showErrors({ recurring: "This field is required." });
    }
    if ($("#task_edit_deadline").val() === "") {
        valid = false;
        $("#form_edit_task").validate().showErrors({ deadline: "This field is required." });
    }
    if ($("#include_edit_form").val() === []) {
        valid = false;
        $("#form_edit_task").validate().showErrors({ location: "This field is required." });
    }
    if ($("#task_edit_description").val() === "") {
        valid = false;
        $("#form_edit_task").validate().showErrors({ Description: "This field is required." });
    }
    return valid;
}
function updateTask() {
    if (ValidateTask()) {
        var include = $('#include_edit_form').val();
        var formDef = [];
        if (include.length > 0) {
            for (var i = 0; i < include.length; i++) {
                formDef.push({Id: include[i]});
            }
        }
        var task =
        {
            Id: $('#task_edit_id').val(),
            Priority: $('#task_edit_priority').val(),
            Description: $('#task_edit_description').val(),
            Name: $('#edit_task_name').val(),
            Repeat: $('#task_edit_recurring').val(),
            TimeLineDate: $('#task_edit_deadline').val(),//new Date(moment($('#task_edit_deadline').val(), "DD/MM/YYYY hh:mm")),
            FormDefinitions: formDef
        }

        $.ajax({
            type: 'post',
            url: '/Tasks/UpdateTask',
            datatype: 'json',
            data: {
                task: task
            },
            success: function (refModel) {
                if (refModel.actionVal == 1) {
                    window.location.href = '/Qbicles/Task';
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }
        });
    }
}

// Media
function ValudateMedia() {
    var valid = true;
    if ($("#media_edit_name").val() === "") {
        valid = false;
        $("#form_media_edit").validate().showErrors({ title: "This field is required." });
    }
    if ($("#media_edit_description").val() === "") {
        valid = false;
        $("#form_media_edit").validate().showErrors({ Description: "This field is required." });
    }
    return valid;
}
function updateMedia() {
    if (ValudateMedia()) {

        var media =
        {
            Key: $('#media_edit_key').val(),
            Description: $('#media_edit_description').val(),
            Name: $('#media_edit_name').val()
        }

        $.ajax({
            type: 'post',
            url: '/Medias/UpdateMedia',
            datatype: 'json',
            data: {
                media: media
            },
            success: function (refModel) {
                if (refModel.result) {
                    window.location.href = '/Qbicles/Media?key=' + media.Key;
                } else {
                    cleanBookNotification.error(refModel.msg, "Qbicles");
                }
            }
        });
    }
}
