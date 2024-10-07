// event modal
var isBusyAddEventForm = false;
var lstDate = [];
var uiEls = {
    $form_event_addedit: $("#form_event_addedit"),
    $eventName: $('#eventName'),
    $createEvent: $('#create-event'),
    $singleEventDate: $('.single-event-date'),
    $eventQbicleId: $('#eventQbicleId'),
};
function ChangeCheckItem(obj) {
    if ($(obj).is(":checked"))
        $(obj).attr("att-checked", 1);
    else
        $(obj).attr("att-checked", 0);
}

function SaveEvent() {
    if (isBusyAddEventForm) {
        return;
    }
    lstDate = [];
    $("#hdLastOccurrence").val($("input[name='recur-final-event']").val());
    var dayOrmonth = "", dayofweek = "";
    var type = $("#hdEventRecurrenceType").val();
    if (type == "0") {
        dayOrmonth = "";
        $(".dailyEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        dayOrmonth = "";
        $(".weeklyEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    else {
        dayOrmonth = "";
        $(".monthEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }

    $("#hdDayOrMonth").val(dayOrmonth);
    $("#hdEventDayOfWeek").val(dayofweek);
    if ($("#create-event-recurrence").find('input[name=isRecurs]').is(":checked")) {
        $("#lstEventDate tbody tr").find("input[type='checkbox']").each(function () {
            if ($(this).is(":checked")) {
                lstDate.push($(this).attr("att-date"));
            }
        });
    }
    if ($("#create-event-recurrence").find('input[name=isRecurs]').is(":checked") && lstDate.length <= 0) {
        cleanBookNotification.error(_L("ERROR_MSG_110"), "Event");
        return;
    }
    if (uiEls.$form_event_addedit.valid()) {
        $.ajax({
            url: "/Events/DuplicateEventNameCheck",
            data: { cubeId: uiEls.$eventQbicleId.val(), eventKey: $eventKey, EventName: $('#eventName').val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                uiEls.$form_event_addedit.validate().showErrors({ Name: _L("ERROR_MSG_321") });
            else {
                if ($('#eventAttachments').val()) {
                    var typeIsvalid = checkfile($('#eventAttachments').val());
                    if (typeIsvalid.stt) {
                        uiEls.$form_event_addedit.trigger("submit");
                    } else {
                        uiEls.$form_event_addedit.validate().showErrors({ eventAttachment: typeIsvalid.err });
                    }
                } else {
                    ProcessEventMedia();
                }
            }
        })
            .fail(function () {
                uiEls.$form_event_addedit.validate().showErrors({ Name: _L("ERROR_MSG_93") });
            })
    } else {
        if ($("#create-event-overview .error").length) {
            $('#eventTabs a[href="#create-event-overview"]').tab('show');
            return false;
        }
    }
};

function ProcessEventMedia() {
    $.LoadingOverlay("show");
    var files = document.getElementById("event-media-upload").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("event-media-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#event-object-key").val(mediaS3Object.objectKey);
                $("#event-object-name").val(mediaS3Object.fileName);
                $("#event-object-size").val(mediaS3Object.fileSize);

                EventSubmit();
            }
        });

    } else
        EventSubmit();
};

EventSubmit = function () {
    var frmData = new FormData(uiEls.$form_event_addedit[0]);
    frmData.append("listDate", lstDate);

    isDisplayFlicker(true);

    $.ajax({
        type: "post",
        cache: false,
        url: "/Events/SaveEvent",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddEventForm = true;
        },
        success: function (data) {
            if (data.result) {
                uiEls.$createEvent.modal('toggle');
                isBusyAddEventForm = false;
                
                if (data.msg != '') {
                    htmlActivityRender(data.msg, 0);
                    isDisplayFlicker(false);
                }

                if ($('#eventKey').val() != "") {
                    location.reload();
                } else {
                    ResetEvent();
                    var calendar = $('.calendar-view-dashboard').datepicker().data('datepicker');
                    if (calendar) {
                        var date = calendar.currentDate;
                        CalTabActive(date.getFullYear(), date.getMonth() + 1);
                    }
                }
            } else {
                isBusyAddEventForm = false;
                cleanBookNotification.error(data.msg, "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isDisplayFlicker(false);
            isBusyAddEventForm = false;
            
        }

    }).always(function () {
        LoadingOverlayEnd();
    });
};








//function AddNewEventClick(currentUserId) {
//    ClearEventInputModalAddEdit();
//    $('#selectSendInvites').select2().val([currentUserId]).change();
//}
function RecursCheck(obj) {
    if ($(obj).is(":checked")) {
        $(obj).prop('checked', true);
        $(obj).val(true);
        $('#recurrenceEvent').css({
            "display": "block"
        });
    }
    else {
        $(obj).prop('checked', false);
        $(obj).val(false);
        $('#recurrenceEvent').css({
            "display": "none"
        });
    }

}
function ResetEvent() {
    var St_time = $('#deadline').val();
    uiEls.$form_event_addedit.trigger("reset");
    $('#deadline').val(St_time);
    $('#create-event-overview select[name=EventType]').val($("select[name=EventType] option:first").val()).change();
    $('#create-event-people select[name=sendInvitesTo]').val('').change();
    $('#create-event-related select[name=ActivitiesRelate]').val('').change();
    $('#create-event-overview input[name=Name]').val('');
    $('#create-event-overview textarea[name=Description]').val('');
    $('#create-event-overview textarea[name=Location]').val('');
    $("#create-event-recurrence").find('input[name=isRecurs]').prop('checked', false).change();
    $("#create-event-recurrence").find('input[name=isRecurs]').val(false);
    $('#create-event-related .chkevtoggle').bootstrapToggle('off');
    ClearError();
}
function OnCheckType(obj, name) {
    var type = $("#hdEventRecurrenceType").val();
    if (type == 0) {
        if ($(obj).is(":checked")) {
            $(".dailyEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".dailyEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
    else if (type == 1) {
        if ($(obj).is(":checked")) {
            $(".weeklyEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".weeklyEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
    else {
        if ($(obj).is(":checked")) {
            $(".monthEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".monthEvent").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
}
function show_recurEvent(type) {
    if (type != "") {
        var elmentStart = $('input[name=event-recur-start]').val();
        var elmentEnd = $('input[name=recur-final-event]');
        var route = ".Event-recur-" + type;
        $('#event-recur').show();
        $('.event-recurtype').hide();
        $(route).show();
        var formatDate = $dateFormatByUser.toUpperCase();
        if (type == "daily") {
            $("#hdEventRecurrenceType").val(0);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(1, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: formatDate
                }
            });
        }
        else if (type == "weekly") {
            $("#hdEventRecurrenceType").val(1);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(6, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: formatDate
                }
            });
        }
        else if (type == "monthly") {
            $("#hdEventRecurrenceType").val(2);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(24, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: formatDate
                }
            });
        }
    }
    else {
        $('#event-recur').hide();
    }
    $(".exclusion-list-event").css({ "display": "none" });
    EventGenarateDateTable();
}
//end event modal
function NextTab_EventOverview() {
    if (!uiEls.$form_event_addedit.valid()) {
        return;
    }
    $('#eventTabs > .active').next('li').find('a').trigger('click');
}
function update_recurrenceEvent() {
    $('#event-restricted').hide();
    var duration = $('#Eventduration').val();
    var mode = $("#Eventmode :selected").text().trim().toLowerCase();
    $('#Eventdailyopt').removeAttr('disabled');
    $('#Eventweeklyopt').removeAttr('disabled');

    if (duration > 24 && mode == 'hours') {
        $('#event-indicated-duration').html(duration + " " + mode);
        $('#event-indicated-duration').val(duration + " " + mode);
        $('#event-restricted').show();
        $('#Eventdailyopt').attr('disabled', true);
    }

    if (duration > 1 && mode == 'days') {
        $('#event-indicated-duration').html(duration + " " + mode);
        $('#event-indicated-duration').val(duration + " " + mode);
        $('#event-restricted').show();
        $('#Eventdailyopt').attr('disabled', true);
    }

    if (duration > 7 && mode == 'days') {
        $('#event-indicated-duration').html(duration + " " + mode);
        $('#event-indicated-duration').val(duration + " " + mode);
        $('#event-restricted').show();
        $('#Eventdailyopt').attr('disabled', true);
        $('#Eventweeklyopt').attr('disabled', true);
    }

    if (duration > 1 && mode == 'weeks') {
        $('#event-indicated-duration').html(duration + " " + mode);
        $('#event-indicated-duration').val(duration + " " + mode);
        $('#event-restricted').show();
        $('#Eventdailyopt').attr('disabled', true);
        $('#Eventweeklyopt').attr('disabled', true);
    }
    $("input[name='event-recur-start']").val($("#deadline").val()).trigger("change");
    $("input[name='event-duration']").val(duration + " " + mode).trigger("change");
};

function GetListDateEvent(dayofweek, type, Pattern, customDate, LastOccurenceDate, firstOccurenceDate) {
    return $.ajax({
        url: "/Qbicles/GetListDate",
        type: "POST",
        dataType: "json",
        data: { dayofweek: dayofweek, type: type, Pattern: Pattern, customDate: customDate, LastOccurenceDate: LastOccurenceDate, firstOccurenceDate: firstOccurenceDate }
    });
}
function EventGenarateDateTable(isDisplayGrid) {
    var type = $("#hdEventRecurrenceType").val();
    var pattern = $("#patternEvent :selected").val();
    var LastOccurenceEventDate = $("input[name='recur-final-event']").val();
    var firstOccurenceDate = $("input[name='event-recur-start']").val();
    var customDate = "";
    if (pattern == "")
        pattern = "0";
    var dayofweek = "";
    var i = 0;

    if (pattern == "2")
        customDate = $("#month-dates-event :selected").val();
    else
        customDate = "0";
    if (type == "0") {
        $(".dailyEvent").each(function () {
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        $(".weeklyEvent").each(function () {
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    else {
        $(".monthEvent").each(function () {

            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    GetListDateEvent(dayofweek, type, pattern, customDate, LastOccurenceEventDate, firstOccurenceDate).done(function (data) {

        if (data != '') {
            var str = "";
            var strShortDate = "";
            $("#lstEventDate tbody tr").remove();
            for (var i = 0; i < data.length; i++) {
                str += '<tr>';
                str += '<td>' + data[i].Name + '</td>';
                str += '<td><input type="checkbox" ' + (data[i].selected ? "checked" : "") + ' value="' + data[i].Value + '" onclick="OnCheckType(this,\'' + data[i].ShortName + '\')"  att-date="' + data[i].Date + '"  ></td>';
                str += ' </tr>';
                strShortDate += data[i].ShortName + ",";
            }
            $("#lstEventDate tbody").append(str);
        }
        if (isDisplayGrid || $(".exclusion-list-event").attr("style").indexOf("display: block;") > 0)
            $(".exclusion-list-event").css({ "display": "block" });
        else
            $(".exclusion-list-event").css({ "display": "none" });
        if (type == 2) {
            $(".monthEvent").each(function () {

                if (strShortDate != null && strShortDate.indexOf($(this).val()) > -1) {
                    $(this).prop('checked', true);
                }
                else {
                    $(this).prop('checked', false);
                }
            });
        }
    });
}
function changePatternEvent() {
    if ($("#patternEvent :selected").val() == "2") {
        $('#customDateEvent').show();
        $("#hdcustomDate").val($("#month-dates-event :selected").val());
    }
    else {
        $('#customDateEvent').hide();
    }
    EventGenarateDateTable();
}
function AddRemoveAtvRelatedEv(id, name, thiss) {
    if ($(thiss).prop("checked")) {
        if ($('#create-event-related  select[name=ActivitiesRelate] option[value="' + id + '"]').length <= 0)
            $('#create-event-related  select[name=ActivitiesRelate]').append('<option selected value="' + id + '">' + name + '</option>').select2();
    } else {
        $('#create-event-related  select[name=ActivitiesRelate] option[value="' + id + '"]').remove();
        $('#create-event-related  select[name=ActivitiesRelate]').select2();
    }
}
function CountCanEventDelete(ceventId) {
    $.getJSON('/Events/CountCanEventsDelete', { ceventId: ceventId }, function (data) {
        var result = confirm(_L("ERROR_MSG_359", [data]));
        if (result) {
            $.LoadingOverlay("show");
            $.ajax({
                method: "POST",
                url: "/Events/RecurringEventsDelete",
                data: { ceventId: ceventId }
            }).done(function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    $('#stop-recurring-event').prop("disabled", true);
                    cleanBookNotification.success(_L("ERROR_MSG_94"), "Qbicles");
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}
$(document).ready(function () {
    $('#create-event-related  select[name=ActivitiesRelate]').on("select2:unselect", function (e) {
        var value = e.params.data.id;
        var selector = 'input[value=' + value + ']';
        $(selector).bootstrapToggle('off');
    });
    setTimeout(function () {
        $('#rleventActivities').dataTable({
            destroy: true,
            serverSide: true,
            processing: true,
            paging: true,
            searching: true,
            ajax: {
                "url": "/Tasks/AtivitiesRelated",
                "data": function (d) {
                    return $.extend({}, d, {
                        "currentQbicleId": $('#eventQbicleId').val(),
                        "currentActivityKey": $('#eventKey').val()
                    });
                }
            },
            columns: [
                { "title": "Name", "data": "Name", "searchable": true },
                { "title": "Type", "data": "Type", "searchable": true },
                null
            ],
            columnDefs: [{
                "targets": 2,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    return '<input class="chkevtoggle" data-toggle="toggle" data-on="Yes" data-off="No" data-onstyle="success" onchange="AddRemoveAtvRelatedEv\(' + data + ',\'' + row.Name.replace("'", '') + '\',this\)"  type="checkbox" value="' + data + '">';
                }
            }]
        });
    }, 2000);
    $('#rleventActivities').on('draw.dt', function () {
        //$('.chktoggle').bootstrapToggle();
        $.each($('.chkevtoggle'), function (index, value) {
            var listChecked = $('#create-event-related select[name=ActivitiesRelate]').val();
            var cr_chhk = $(value).val();
            if (listChecked != null && listChecked.indexOf(cr_chhk) >= 0) {
                $(value).bootstrapToggle('on');
            } else {
                $(value).bootstrapToggle('off');
            }
        });
    });
    function SformatOptions(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    function SformatSelected(state) {
        if (!state.id) { return state.text; }
        var urlAvatar = state.element.attributes["avatarUrl"].value;
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
    $('.select2avatar-event').select2({
        placeholder: 'Please select',
        templateResult: SformatOptions,
        templateSelection: SformatSelected
    });

    uiEls.$singleEventDate.daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        },
        minDate: moment()
    });
    $('.scroll-basket').slimScroll({
        height: '300px',
        railOpacity: 1,
        railVisible: true,
        alwaysVisible: true,
        wheelStep: 30,
        touchStep: 30,
        color: "#989696",
        railColor: "#e1e1e1"
    });
    var trigger = setInterval(function () {
        if (document.readyState == "complete") {
            $("input[name='event-recur-start']").val($("#deadline").val());
            $("input[name='event-duration']").val($("#Eventduration").val() + " " + $("#Eventmode :selected").text().trim().toLowerCase());
            clearInterval(trigger);
        }
    }, 200);

    uiEls.$form_event_addedit.validate(
        {
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    //spacesonly: true,
                    minlength: 5
                },
                Location: {
                    required: true
                }
            }
        });
});