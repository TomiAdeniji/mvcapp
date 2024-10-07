var isBusyAddTaskForm = false;
var $form_task_addedit = $('#form_task_addedit');
var lstDate = [];
function show_recur(type) {
    if (type != "") {
        var elmentStart = $('input[name=recur-start]').val();
        var elmentEnd = $('input[name=recur-final]');
        var route = ".task-recur-" + type;
        $('#TaskRecur').show();
        $('.task-recurtype').hide();
        $(route).show();
        if (type == "daily") {
            $("#hdTaskRecurrenceType").val(0);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(1, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: $dateFormatByUser.toUpperCase()
                }
            });
        }
        else if (type == "weekly") {
            $("#hdTaskRecurrenceType").val(1);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(6, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: $dateFormatByUser.toUpperCase()
                }
            });
        }
        else if (type == "monthly") {
            $("#hdTaskRecurrenceType").val(2);
            elmentEnd.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                maxDate: moment(elmentStart, $dateTimeFormatByUser).add(24, 'months'),
                locale: {
                    cancelLabel: 'Clear',
                    format: $dateFormatByUser.toUpperCase()
                }
            });
        }
    }
    else {
        $('#TaskRecur').hide();
    }
    $("#task-exclusion-list").css({ "display": "none" });
    GenarateDateTable();
}
function update_recurrence() {
    $('#task-restricted').hide();
    var duration = $('#taskDuration').val();
    var mode = $('#taskDurationUnit :selected').text().toLowerCase().trim();
    $('#dailyopt').removeAttr('disabled');
    $('#weeklyopt').removeAttr('disabled');

    if (duration != "" && parseInt(duration) > 24 && mode == 'hours') {
        $('#task-indicated-duration').html(duration + " " + mode);
        $('#task-indicated-duration').val(duration + " " + mode);
        $('#task-restricted').show();
        $('#dailyopt').attr('disabled', true);
    }

    if (duration != "" && parseInt(duration) > 1 && mode == 'days') {
        $('#task-indicated-duration').html(duration + " " + mode);
        $('#task-indicated-duration').val(duration + " " + mode);
        $('#task-restricted').show();
        $('#dailyopt').attr('disabled', true);
    }

    if (duration != "" && parseInt(duration) > 7 && mode == 'days') {
        $('#task-indicated-duration').html(duration + " " + mode);
        $('#task-indicated-duration').val(duration + " " + mode);
        $('#task-restricted').show();
        $('#dailyopt').attr('disabled', true);
        $('#weeklyopt').attr('disabled', true);
    }

    if (duration != "" && parseInt(duration) > 1 && mode == 'weeks') {
        $('#task-indicated-duration').html(duration + " " + mode);
        $('#task-indicated-duration').val(duration + " " + mode);
        $('#task-restricted').show();
        $('#dailyopt').attr('disabled', true);
        $('#weeklyopt').attr('disabled', true);
    }

    $("input[name='recur-start']").val($("#taskProgrammedStart").val());
    $("input[name='duration']").val($("#taskDuration").val() + " " + $("#taskDurationUnit :selected").text().trim().toLowerCase());

}
function TaskRecursCheck(obj) {
    if ($(obj).is(":checked")) {
        $(obj).prop('checked', true);
        $(obj).val(true);
        $('#TaskRecurrence').css({
            "display": "block"
        });
    }
    else {
        $(obj).prop('checked', false);
        $(obj).val(false);
        $('#TaskRecurrence').css({
            "display": "none"
        });
    }

}
function task_validtabs() {
    
    if ($("#create-task-overview input.error").length > 0 || $("#create-task-overview textarea.error").length > 0) {
        $('#taskTabs a[href="#create-task-overview"]').tab('show');
        return false;
    } else if (($("#create-task-checklist input.error").length > 0 || $("#create-task-checklist textarea.error").length > 0) && $('input[name=isSteps]').prop('checked')) {
        $('#taskTabs a[href="#create-task-checklist"]').tab('show');
        return false;
    }
    return true;
}
function ValidateStepsWeight() {
    var sumWeight = SumStepsWeight();
    if (sumWeight > 100)
        return false;
    else if (sumWeight < 100)
        return false;
    cleanBookNotification.clearmessage();
    return true;
}
function ValidateSteps() {
    
    //Validate
    var valid = true;
    if ($('input[name=isSteps]').prop('checked')) {
        var rows = $('#sortable div.ui-sortable-handle');
        for (var i = 0; i < rows.length; i++) {
            var FieldName = $(rows[i]).find("div.row-options input.fieldName");
            if (FieldName.val() === "") {
                valid = false;
                break;
            }
            var FieldDescription = $(rows[i]).find("div.row-options textarea.fieldDescription");
            if (FieldDescription.val() === "") {
                valid = false;
                break;
            }
            var FieldWeight = $(rows[i]).find("div.row-options input.fieldWeight");
            if (FieldWeight.val() === "") {
                valid = false;
                break;
            }
        }
    }
    //End
    return valid;
}
function NextTab_Task() {
    
    var valid = ValidateSteps();
    if (!valid)
        $form_task_addedit.valid();
    else {
        if (!ValidateStepsWeight()) {
            cleanBookNotification.error(_L("ERROR_MSG_360"), "Task");
            return;
        }
        $('#taskTabs > .active').next('li').find('a').trigger('click');
    }
}
function PrevTab_Task() {
    $('#taskTabs > .active').prev('li').find('a').trigger('click');
}
function NextTab_Overview() {
    
    //$form_task_addedit[0].data("validator").settings.ignore = $('input[name=isSteps]').prop('checked') ? "" : ":hidden";
    if (!$form_task_addedit.valid()) {
        task_validtabs();
        return;
    }
    $('#taskTabs > .active').next('li').find('a').trigger('click');
}
function SumStepsWeight() {
    
    var sumWeight = 0;
    $('input.fieldWeight').each(function (index) {
        sumWeight += parseInt($(this).val());
    });
    return sumWeight;
}
function SaveQbicleTask() {   
    $('#form_task_addedit').LoadingOverlay("show");
    lstDate = [];
    if ($('#taskName').val() == "" || $('#taskProgrammedStart').val() == "" || $('#taskDuration').val() == "0") {
        cleanBookNotification.error(_L("ERROR_MSG_361"), "Task");
        $('#form_task_addedit').LoadingOverlay("hide", true);
        return;
    }
    var valid = task_validtabs();
    if (!valid)
        return;
    if (!ValidateStepsWeight()) {
        cleanBookNotification.error(_L("ERROR_MSG_360"), "Task");
        $('#taskTabs a[href="#create-task-checklist"]').tab('show');
        $('#form_task_addedit').LoadingOverlay("hide", true);
        return;
    }
  
    $("#hdTaskLastOccurrence").val($("input[name='recur-final']").val());
    var dayOrmonth = "", dayofweek = "";
    var type = $("#hdTaskRecurrenceType").val();

    if (type == "0") {
        dayOrmonth = "";
        dayofweek = "";
        $(".daily").each(function () {
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
        dayofweek = "";
        $(".weekly").each(function () {
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
        dayofweek = "";
        $(".monthTask").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    if ($("#create-task-recurrence").find('input[name=isRecurs]').is(":checked")) {
        $("#lstDate tbody tr").find("input[type='checkbox']").each(function () {
            if ($(this).is(":checked")) {
                lstDate.push($(this).attr("att-date"));
            }
        });
    }
    if ($("#create-task-recurrence").find('input[name=isRecurs]').is(":checked") && lstDate.length <= 0) {
        cleanBookNotification.error(_L("ERROR_MSG_110"), "Task");
        $('#form_task_addedit').LoadingOverlay("hide", true);
        return;
    }
    $("#hdTaskDayOrMonth").val(dayOrmonth);
    $("#hdTaskDayOfWeek").val(dayofweek);
    //$form_task_addedit[0].data("validator").settings.ignore = $('input[name=isSteps]').prop('checked') ? "" : ":hidden";
    if ($form_task_addedit.valid()) {
        ReDataStep();
        $.ajax({
            url: "/Tasks/DuplicateTaskNameCheck",
            data: { cubeId: $("#taskQbicleId").val(), taskKey: $('#taskKey').val(), taskName: $("#taskName").val() },
            type: "GET",
            dataType: "json",
            async: false
        }).done(function (refModel) {
            if (refModel.result) {
                $form_task_addedit.validate().showErrors({
                    Name: _L("ERROR_MSG_111")
                });
                task_validtabs();
                $('#form_task_addedit').LoadingOverlay("hide", true);
            }
            else {
                //if ($('#taskAttachments').val()) {
                //    var typeIsvalid = checkfile($('#taskAttachments').val());
                //    if (typeIsvalid.stt) {
                //        ProcessTaskMedia();
                //    } else {
                //        $form_task_addedit.validate().showErrors({ taskAttachments: typeIsvalid.err });
                //    }
                //} else {
                //    ProcessTaskMedia();
                //}
                TaskSubmit();
            }

        }).fail(function () {
            $("#form_task_addedit").validate().showErrors({ Name: _L("ERROR_MSG_112") });
            $('#form_task_addedit').LoadingOverlay("hide", true);
        });
    } else
        task_validtabs();
};


//function ProcessTaskMedia() {
//    $.LoadingOverlay("show");
//    var files = document.getElementById("taskAttachments").files;

//    if (files && files.length > 0) {
//        UploadMediaS3ClientSide("taskAttachments").then(function (mediaS3Object) {

//            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
//                LoadingOverlayEnd('hide');
//                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
//                return;
//            }
//            $("#task-object-key").val(mediaS3Object.objectKey);
//            $("#task-object-name").val(mediaS3Object.fileName);
//            $("#task-object-size").val(mediaS3Object.fileSize);

//            TaskSubmit();
//        });

//    } else
//        TaskSubmit();
//};


TaskSubmit = function () {
    var frmData = new FormData($form_task_addedit[0]);
    frmData.append("listDate", lstDate);
    $.ajax({
        type: "post",
        cache: false,
        url: "/Tasks/SaveQbicleTask",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            
        },
        success: function (data) {
            if (data.result) {

                $('#create-task').modal('toggle');

              
                if ($('#taskKey').val() != "") {
                    location.reload();
                } else {                    
                    ResetTask();
                    var calendar = $('.calendar-view-dashboard').datepicker().data('datepicker');
                    if (calendar) {
                        var date = calendar.currentDate;
                        CalTabActive(date.getFullYear(), date.getMonth() + 1);
                    }
                }

                if (data.msg != '') {
                    htmlActivityRender(data.msg, 0);
                    isDisplayFlicker(false);
                }
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Qbicles");
                
            }
        },
        error: function (data) {            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isDisplayFlicker(false);
        }
    }).always(function () {
        $('#form_task_addedit').LoadingOverlay("hide", true);
    });
    var trigger = setInterval(function () {
        if (document.readyState == "complete") {
            $("#taskDurationUnit").val($("#taskDurationUnit :selected").val()).trigger("change");
            var type = $("#hdTaskRecurrenceType").val();
            if (type == 0)
                $("#TaskRecurtype").val("daily").trigger("change");
            else if (type == 1)
                $("#TaskRecurtype").val("weekly").trigger("change");
            else
                $("#TaskRecurtype").val("monthly").trigger("change");
            clearInterval(trigger);
            return;
        }

    }, 200);

}





















function AddStep() {
    var count_step = $('#sortable div.ui-sortable-handle').length;
    var f_name = "Steplst[" + count_step + "].Name";
    var f_description = "Steplst[" + count_step + "].Description";
    var f_weight = "Steplst[" + count_step + "].Weight";
    var cal_step = 100 - SumStepsWeight();
    var step_clone = $('#sortable div:first').clone();
    step_clone.removeClass('first-step');
    var new_id = UniqueId() + "_title";
    var stepTitle = $(step_clone).find('h5 i.stepTitle');
    stepTitle.attr("id", new_id);
    stepTitle.text("Click to configure this step");
    var fieldName = $(step_clone).find("div.row-options input.fieldName");
    fieldName.attr("onkeyup", "$('#" + new_id + "').html($(this).val());");
    fieldName.attr('name', f_name);
    fieldName.removeClass('valid');
    fieldName.val('');
    var lblErrorName = $(step_clone).find("div.row-options label.lblErrorName");
    lblErrorName.attr('id', f_name + "-error");
    lblErrorName.attr('for', f_name);
    var FieldDescription = $(step_clone).find("div.row-options textarea.fieldDescription");
    FieldDescription.attr('name', f_description);
    FieldDescription.removeClass('valid');
    FieldDescription.val('');
    var lblErrorDescription = $(step_clone).find("div.row-options label.lblErrorDescription");
    lblErrorDescription.attr('id', f_description + "-error");
    lblErrorDescription.attr('for', f_description)
    var FieldWeight = $(step_clone).find("div.row-options input.fieldWeight");
    FieldWeight.attr('name', f_weight);
    FieldWeight.removeClass('valid');
    FieldWeight.val(cal_step > 0 ? cal_step : 0);
    var FieldId = $(step_clone).find("div.row-options input.fieldId");
    FieldId.val('0');
    $('#sortable #btnAddStep').before(step_clone);
}

function ReOrderStep(){
    //when user delete any step, re-order all steps left
    $("#sortable div.ui-sortable-handle + .row-options").each((index, element) => {
        $(element).find("[name]").each((id,els) => {
            var currentNameAttr = $(els).attr("name");
            currentNameAttr = currentNameAttr.replace(/[\d+]/g, index);
            $(els).attr("name",currentNameAttr);
        });
        $(element).find(".error").each((id,els) =>{
            var currentNameId = $(els).attr("id");
            var currentNameFor = $(els).attr("for");
            currentNameId = currentNameId.replace(/[\d+]/g, index);
            currentNameFor = currentNameFor.replace(/[\d+]/g, index);
            $(els).attr("id",currentNameId);
            $(els).attr("for",currentNameFor);
        })
    })
}


function RemoveStep(thiss) {
    if ($('#sortable div.ui-sortable-handle').length > 1 && confirm("Are you sure you want to delete this step?")) {
        $(thiss).parent().parent().parent().remove();
    }
    ReOrderStep();
}
function ReDataStep() {
    var rows = $('#sortable div.ui-sortable-handle');
    for (var i = 0; i < rows.length; i++) {
        var FieldOrder = $(rows[i]).find("div.row-options input.fieldOrder");
        FieldOrder.attr("name", "Steplst[" + i + "].Order");
        FieldOrder.val(i);
        var FieldName = $(rows[i]).find("div.row-options input.fieldName");
        FieldName.attr("name", "Steplst[" + i + "].Name");
        var FieldDescription = $(rows[i]).find("div.row-options textarea.fieldDescription");
        FieldDescription.attr("name", "Steplst[" + i + "].Description");
        var FieldWeight = $(rows[i]).find("div.row-options input.fieldWeight");
        FieldWeight.attr("name", "Steplst[" + i + "].Weight");
        var FieldId = $(rows[i]).find("div.row-options input.fieldId");
        FieldId.attr("name", "Steplst[" + i + "].Id");
    }
}
function ResetTask() {
    $('#taskTabs a[href="#create-task-overview"]').click();
    var St_time = $('#taskProgrammedStart').val();
    $form_task_addedit.trigger("reset");
    $('#taskProgrammedStart').val(St_time);
    $('select[name=Assignee]').val('').change();
    $('select[name=Watchers]').val('').change();
    //Step Tab
    $('input[name=isSteps]').bootstrapToggle('off');
    var rows = $('#sortable div.ui-sortable-handle');
    for (var i = 0; i < rows.length; i++) {
        var row = $(rows[i]);
        if (i != 0 && !row.hasClass('.first-step')) {
            row.remove();
        }
    }
    $('#q1title').text('Click to configure this step');
    $('#create-task-checklist .checklist').hide();
    $("#create-task-recurrence").find('input[name=isRecurs]').prop('checked', false).change();
    $("#create-task-recurrence").find('input[name=isRecurs]').val(false);
    $('#create-task-related select[name=ActivitiesRelate]').val('').change();
    $('#create-task-related .chktoggle').bootstrapToggle('off');
    $taskId = 0;
    ClearError();
}
function GetListDate(dayofweek, type, Pattern, customDate, LastOccurenceDate, firstOccurenceDate) {
    return $.ajax({
        url: "/Qbicles/GetListDate",
        type: "POST",
        dataType: "json",
        data: { dayofweek: dayofweek, type: type, Pattern: Pattern, customDate: customDate, LastOccurenceDate: LastOccurenceDate, firstOccurenceDate: firstOccurenceDate }
    });
}
function GenarateDateTable(isDisplayGrid) {
    var type = $("#hdTaskRecurrenceType").val();
    var pattern = $("#pattern :selected").val();
    var LastOccurenceTaskDate = $("input[name='recur-final']").val();
    var firstOccurenceDate = $("input[name='recur-start']").val();
    var customDate = "";
    if (pattern == "")
        pattern = "0";
    var dayofweek = "";
    var i = 0;

    if (pattern == "2")
        customDate = $("#month-dates :selected").val();
    else
        customDate = "0";
    if (type == "0") {
        $(".daily").each(function () {
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        $(".weekly").each(function () {
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    else {
        $(".monthTask").each(function () {

            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    GetListDate(dayofweek, type, pattern, customDate, LastOccurenceTaskDate, firstOccurenceDate).done(function (data) {
        if (data != '') {
            var str = "";
            $("#lstDate tbody tr").remove();
            var strShortDate = "";
            for (var i = 0; i < data.length; i++) {
                str += '<tr>';
                str += '<td>' + data[i].Name + '</td>';
                str += '<td><input type="checkbox" ' + (data[i].selected ? "checked" : "") + ' value="' + data[i].Value + '" onclick="OnCheckType(this,\'' + data[i].ShortName + '\')"  att-date="' + data[i].Date + '"  ></td>';
                str += ' </tr>';
                strShortDate += data[i].ShortName + ",";
            }
            $("#lstDate tbody").append(str);
        }
        if (isDisplayGrid || $("#task-exclusion-list").attr("style").indexOf("display: block;") > 0)
            $("#task-exclusion-list").css({ "display": "block" });
        else
            $("#task-exclusion-list").css({ "display": "none" });
        if (type == 2) {
            $(".monthTask").each(function () {

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
function OnCheckType(obj, name) {
    var type = $("#hdEventRecurrenceType").val();
    if (type == 0) {
        if ($(obj).is(":checked")) {
            $(".daily").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".daily").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
    else if (type == 1) {
        if ($(obj).is(":checked")) {
            $(".weekly").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".weekly").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
    else {
        if ($(obj).is(":checked")) {
            $(".monthTask").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', false);
                }
            });
        }
        else {
            $(".monthTask").each(function () {
                if ($(this).val() == name) {
                    $(this).prop('checked', true);
                }
            });
        }
    }
}
function ChangeTaskCheckItem(obj) {
    if ($(obj).is(":checked"))
        $(obj).attr("att-checked", 1);
    else
        $(obj).attr("att-checked", 0);
}
function changePattern() {
    if ($("#pattern :selected").val() == "2") {
        $('#customDate').show();
        $("#hdTaskcustomDate").val($("#month-dates :selected").val());
    }
    else {
        $('#customDate').hide();
    }
    GenarateDateTable();
}
function AddRemoveAtvRelated(id, name, thiss) {
    if ($(thiss).prop("checked")) {
        if ($('#create-task-related select[name=ActivitiesRelate] option[value="' + id + '"]').length <= 0)
            $('#create-task-related select[name=ActivitiesRelate]').append('<option selected value="' + id + '">' + name + '</option>').select2();
    } else {
        $('#create-task-related select[name=ActivitiesRelate] option[value="' + id + '"]').remove();
        $('#create-task-related select[name=ActivitiesRelate]').select2();
    }
}
function CountCanTasksDelete(ctaskId) {
    $.getJSON('/Tasks/CountCanTasksDelete', { ctaskId: ctaskId }, function (data) {
        var result = confirm(_L("ERROR_MSG_362", [data]));
        if (result) {
            $.LoadingOverlay("show");
            $.ajax({
                method: "POST",
                url: "/Tasks/RecurringTasksDelete",
                data: { ctaskId: ctaskId }
            }).done(function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    $('#stop-recurring').prop("disabled", true);
                    cleanBookNotification.success(_L("ERROR_MSG_94"), "Qbicles");
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}
$(document).ready(function () {
    $('#create-task-related select[name=ActivitiesRelate]').on("select2:unselect", function (e) {
        var value = e.params.data.id;
        var selector = 'input[value=' + value + ']';
        $(selector).bootstrapToggle('off');
    });
    setTimeout(function () {
        $('#rlActivities').dataTable({
            destroy: true,
            serverSide: true,
            processing: true,
            paging: true,
            searching: true,
            ajax: {
                "url": "/Tasks/AtivitiesRelated",
                "data": function (d) {
                    return $.extend({}, d, {
                        "currentQbicleId": $('#taskQbicleId').val(),
                        "currentActivityKey": $('#taskKey').val()
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
                    return '<input class="chktoggle" data-toggle="toggle" data-on="Yes" data-off="No" data-onstyle="success" onchange="AddRemoveAtvRelated(' + data + ',\'' + row.Name.replace("'", '') + '\',this)"  type="checkbox" value="' + data + '">';
                }
            }]
        });
    }, 2000);
    $('#rlActivities').on('draw.dt', function () {
        //$('.chktoggle').bootstrapToggle();
        $.each($('.chktoggle'), function (index, value) {
            var listChecked = $('#create-task-related select[name=ActivitiesRelate]').val();
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

    $('.select2avatartask').select2({
        placeholder: 'Please select',
        templateResult: SformatOptions,
        templateSelection: SformatSelected
    });
    $('.single-date').daterangepicker({
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
            $("input[name='recur-start']").val($("#taskProgrammedStart").val());
            $("input[name='duration']").val($("#taskDuration").val() + " " + $("#taskDurationUnit :selected").text().trim().toLowerCase());
            clearInterval(trigger);
        }
    }, 200);
});
