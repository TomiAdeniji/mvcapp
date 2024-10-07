
function StartProgress(thiss) {
    $.ajax({
        url: "/Tasks/StartProgress",
        data: { taskKey: $('#taskKey').val() },
        type: "Post",
        dataType: "json",
        async: false,
    }).done(function (refModel) {
        if (refModel.result) {
            if (refModel.Object && refModel.Object.Complete) {
                cleanBookNotification.success(_L("ERROR_MSG_363"), "Task");
                $('#inprogress').hide();
                $('#notstarted').hide();
                $('#complete').show();
                $('#btn-task-edit').remove();
                //var ActualEnd= refModel.Object.ActualEnd;
                //$('#task-deadline').text(moment(ActualEnd).format('DD-MM-YYYY hh:mm'));
                $('#countdown_task').text(refModel.Object.Countdown);
                $('.performance-popup,.performance').show();
            } else {
                cleanBookNotification.success(_L("ERROR_MSG_114"), "Task");
                $('#inprogress').show();
                $('#notstarted').hide();
            }
            $('.completion').show();
            $(thiss).hide();
            $('.task-breakdown input[type=checkbox]').prop('disabled', false);
            $('#disabledchecks').hide();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
        }
    }).fail(function (data) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
    });
}
function ConfirmChanges(thiss) {
    $(thiss).hide();
    var steps = [];
    $.each($("input[name='StepInstance']:checked"), function () {
        steps.push($(this).val());
    });
    $.ajax({
        url: "/Tasks/StepComplete",
        data: { taskKey: $('#taskKey').val(), stepInstance: steps },
        type: "Post",
        dataType: "json",
        async: false,
    }).done(function (refModel) {
        if (refModel.result) {
            cleanBookNotification.success(_L("ERROR_MSG_115"), "Task");
            if (refModel.Object.length > 0) {
                var Percent = 0;
                $.each(refModel.Object, function (key, value) {
                    if (value.Complete) {
                        var step = $('#StepInstance_' + value.StepId);
                        var divStep = step.parent();
                        step.remove();
                        divStep.addClass('completed-info');
                        divStep.prepend('<i style="width: 25px;" class="fa fa-check-circle green"></i>');
                        Percent += value.Percent;
                    }
                });
                Percent = Percent > 100 ? 100 : Percent;
                if (Percent == 100 || (refModel.Object2 && refModel.Object2.Complete === true)) {
                    $('#inprogress').hide();
                    $('#notstarted').hide();

                    $('#complete').show();
                    var ActualEnd = refModel.Object2.ActualEnd;
                    $('#task-deadline').text(ActualEnd);
                    $('.performance-popup').show();
                    $('#btn-task-edit').remove();
                } else {
                    $('.performance-popup').hide();
                }
                var progress = $('.progress-bar');
                progress.css("width", Percent + "%");
                progress.attr('aria-valuenow', Percent);
                $('.percent-task').text(Percent + "%");
            }
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
        }
    }).fail(function (data) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
    });
}
function PerformanceReview() {
    var rate = $('select[name=stars]').val();
    if (!rate) {
        cleanBookNotification.error(_L("ERROR_MSG_364"), "Task");
        return;
    }

    $.ajax({
        url: "/Tasks/PerformanceReview",
        data: { taskKey: $('#taskKey').val(), rating: $('select[name=stars]').val() },
        type: "Post",
        dataType: "json",
        async: false,
    }).done(function (refModel) {
        if (refModel.result) {
            $('#performance-review').modal("toggle");
            cleanBookNotification.success(_L("ERROR_MSG_116"), "Task");
            if (refModel.Object) {
                var rating = refModel.Object.rating;
                if (rating) {
                    for (var i = 1; i < 6; i++) {
                        if (i <= rating) {
                            $('.star-' + i).css("color", "#fc8b02");
                        } else {
                            $('.star-' + i).css("color", "#f1f1f1");
                        }
                    }
                    $('.performance small').text(rating + " out of 5");
                }
                $('.performance').show();
                $('.performance-popup').hide();
                $('#inprogress').hide();
                $('#notstarted').hide();
                $('#complete').show();
            }
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
        }
    }).fail(function (data) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
    });
}

$(document).ready(function () {
    $('.task-breakdown input[type=checkbox]').bind('click', function (e) {
        if ($(".task-breakdown input:checkbox:checked").length > 0) {
            $('.confirm').show();
        } else {
            $('.confirm').hide();
        }
    });
    $("button.btn-addworklog").click(function () {
        var dataworklog = {
            ActivityKey: $('#taskKey').val(),
            Days: $("input[name='est-days']").val(),
            Hours: $("input[name='est-hours']").val(),
            Minutes: $("input[name='est-mins']").val()
        };
        if (dataworklog.Days == 0 && dataworklog.Hours == 0 && dataworklog.Minutes == 0) {
            cleanBookNotification.error(_L("ERROR_MSG_118"), "Task");
            return;
        }
        $.ajax({
            url: "/Tasks/AddWorkLog",
            data: dataworklog,
            type: "Post",
            dataType: "json",
            async: false,
        }).done(function (refModel) {
            if (refModel.result) {
                cleanBookNotification.success(_L("ERROR_MSG_119"), "Task");

                $('#inprogress').show();
                $('#notstarted').hide();

                var data = refModel.Object;
                if (data) {
                    var myhours = data.Days ? data.Days + 'd ' : '';
                    myhours += data.Hours ? data.Hours + 'h ' : '';
                    myhours += data.Minutes ? data.Minutes + 'm' : '';
                    var row = "<tr><td>{date}</td><td>{myhours}</td></tr>";
                    $('#task-time table tbody tr.row-nodata').remove();
                    $('#task-time table tbody').append(row.replace("{date}", refModel.Object.DateTime).replace("{myhours}", myhours));

                }
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
            }
        }).fail(function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
        });
    });
});