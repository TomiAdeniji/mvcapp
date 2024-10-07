var skip = 0;
var take = 8;

function showImageFromInputFile(input, output) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(output).attr('src', e.target.result);
            $(output).css({ "display": "block" });
        };
        reader.readAsDataURL(input.files[0]);
    }
}
function ProcessTaskLocationPlace() {
    $.LoadingOverlay("show");
    var files = document.getElementById("taskAttachments").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("taskAttachments").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#sm-place-task-object-key").val(mediaS3Object.objectKey);
                $("#sm-place-task-object-name").val(mediaS3Object.fileName);
                $("#sm-place-task-object-size").val(mediaS3Object.fileSize);

                SubmitTaskLocationPlace();
            }
        });

                
    } else
        SubmitTaskLocationPlace();
};
function SubmitTaskLocationPlace() {
    var form_data = new FormData($form_task_addedit[0]);
    $.ajax({
        type: "POST",
        url: "/SalesMarketingLocation/SaveSMLocationQbicleTask",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#create-task").modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_199"), "Sales Marketing");
                ResetTask();
                $('#tblScheduledVisits').DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
            isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}

function SaveTaskLocationPlace() {
    lstDate = [];
    if ($('#taskName').val() === "" || $('#taskProgrammedStart').val() === "" || $('#taskDuration').val() === "0" || $('#taskDescription').val() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_361"), "Task");
        return;
    }
    var valid = task_validtabs();
    if (!valid)
        return;
    if (!ValidateStepsWeight()) {
        cleanBookNotification.error(_L("ERROR_MSG_360"), "Task");
        $('#taskTabs a[href="#create-task-checklist"]').tab('show');
        return;
    }
    if (isBusyAddTaskForm) {
        return;
    }
    $("#hdTaskLastOccurrence").val($("input[name='recur-final']").val());
    var dayOrmonth = "", dayofweek = "";
    var type = $("#hdTaskRecurrenceType").val();

    if (type === "0") {
        dayOrmonth = "";
        dayofweek = "";
        $(".daily").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek === "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type === "1") {
        dayOrmonth = "";
        dayofweek = "";
        $(".weekly").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek === "")
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
                if (dayofweek === "")
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
        return;
    }
    $("#hdTaskDayOrMonth").val(dayOrmonth);
    $("#hdTaskDayOfWeek").val(dayofweek);
    $form_task_addedit.data("validator").settings.ignore = $('input[name=isSteps]').prop('checked') ? "" : ":hidden";
    if ($form_task_addedit.valid()) {
        ReDataStep();
        $.ajax({
            url: "/Tasks/DuplicateTaskNameCheck",
            data: { cubeId: $("#taskQbicleId").val(), taskKey: $('#taskKey').val(), taskName: $("#taskName").val() },
            type: "GET",
            dataType: "json",
            async: false
        }).done(function (refModel) {
            if (refModel.result)
                $form_task_addedit.validate().showErrors({ Name: _L("ERROR_MSG_111") });
            else {
                if ($('#taskAttachments').val()) {
                    var typeIsvalid = checkfile($('#taskAttachments').val());
                    if (typeIsvalid.stt) {
                        ProcessTaskLocationPlace();
                    } else {
                        $form_task_addedit.validate().showErrors({ taskAttachments: typeIsvalid.err });
                    }
                } else {
                    ProcessTaskLocationPlace();
                }
            }

        }).fail(function () {
            $("#form_task_addedit").validate().showErrors({ Name: _L("ERROR_MSG_112") });
        });
    } else
        task_validtabs();
};

function LoadModalPlace(id) {
    $("#app-marketing-place-add").load("/SalesMarketing/LoadModalPlace", { id: id }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });


        $('#frm-place-addedit').validate({
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    minlength: 4,
                    maxlength: 50
                },
                Summary: {
                    maxlength: 200
                }
            }
        });

    });
}

function LoadModalVisit(placeId) {
    $("#app-marketing-visit-add").load("/SalesMarketingLocation/LoadModalVisit", { placeId: placeId }, function () {
        $('#reason').select2({ placeholder: "Please select" });
        $('#task').select2({ placeholder: "Please select" });
        $('.singledateandtime').daterangepicker({
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
            }
        });

        var $frmvisitlogsadd = $('#frm-visitlogs-add');
        $frmvisitlogsadd.validate({
            ignore: "",
            rules: {
                DateTimeOfVisit: {
                    required: true
                },
                Reasons: {
                    required: true
                },
                Notes: {
                    maxlength: 3000
                }
            }
        });

        $frmvisitlogsadd.submit(function (e) {
            e.preventDefault();
            if ($frmvisitlogsadd.valid()) {
                $.LoadingOverlay("show");
                var form_data = new FormData(this);
                $.ajax({
                    type: this.method,
                    url: this.action,
                    data: form_data,
                    cache: false,
                    enctype: 'multipart/form-data',
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusyAddTaskForm = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $("#app-marketing-visit-add").modal('hide');
                            cleanBookNotification.success(_L("ERROR_MSG_202"), "Sales Marketing");
                            $('#tblVisitLogs').DataTable().ajax.reload();
                            $('#visitLogs').trigger("click");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                        }
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            }
        });
    });
}

function LoadModalActivity(placeId) {
    $("#app-marketing-activity-add").load("/SalesMarketingLocation/LoadModalActivity", { placeId: placeId }, function () {
        $('.datetimerange').daterangepicker({
            autoUpdateInput: true,
            timePicker: true,
            cancelClass: "btn-danger",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: $dateTimeFormatByUser
            }
        });

        var $frmactivitylogsadd = $('#frm-activitylogs-add');
        $frmactivitylogsadd.validate({
            ignore: "",
            rules: {
                Date: {
                    required: true
                },
                Recorded: {
                    required: true
                },
                Notes: {
                    maxlength: 3000
                }
            }
        });

        $frmactivitylogsadd.submit(function (e) {
            e.preventDefault();
            if ($frmactivitylogsadd.valid()) {
                $.LoadingOverlay("show");
                var form_data = new FormData(this);
                $.ajax({
                    type: this.method,
                    url: this.action,
                    data: form_data,
                    cache: false,
                    enctype: 'multipart/form-data',
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusyAddTaskForm = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $("#app-marketing-activity-add").modal('hide');
                            cleanBookNotification.success(_L("ERROR_MSG_235"), "Sales Marketing");
                            $('#tblActivityLogs').DataTable().ajax.reload();
                            $('#activityLogs').trigger('click');
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                        }
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            }
        });
    })
}

function LoadScheduledVisits() {
    $("#tblScheduledVisits").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#tblScheduledVisits").LoadingOverlay("show");
        } else {
            $("#tblScheduledVisits").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketingLocation/LoadScheduledVisits',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "placeId": $("#id").val()
                });
            }
        },
        "columns": [
            {
                data: "DateTimeOfVisit",
                orderable: false
            },
            {
                data: "Duration",
                orderable: false
            },
            {
                data: "Agent",
                orderable: false
            },
            {
                data: "Status",
                orderable: false,
                render: function (value, type, row) {
                    if (row.Status === "Pending")
                        return '<span class="label label-info label-lg">Pending</span>';
                    else if (row.Status === "In progress")
                        return '<span class="label label-warning label-lg">In progress</span>';
                    else if (row.Status === "Overdue")
                        return '<span class="label label-danger label-lg">Overdue</span>';
                    else
                        return '<span class="label label-success label-lg">Complete</span>';
                }
            }
        ],
        "order": [[1, "asc"]]
    });

}

function LoadVisitLogs() {
    $("#tblVisitLogs").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#tblVisitLogs").LoadingOverlay("show");
        } else {
            $("#tblVisitLogs").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketingLocation/LoadVisitLogs',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "placeId": $("#id").val()
                });
            }
        },
        "columns": [
            {
                data: "DateTimeOfVisit",
                orderable: false
            },
            {
                data: "Agent",
                orderable: false
            },
            {
                data: "txtReason",
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    var str = "";
                    if (row.TaskId !== 0) {
                        str = '<a href="/Qbicles/Dashboard" target="_blank" style="color: #5bc0de; text-decoration: none;">' + value + '</>'
                    } else {
                        str = value;
                    }

                    return str;
                }
            },
            {
                data: "Leads",
                orderable: false
            },
            {
                data: "Notes",
                orderable: false
            },
        ],
        "order": [[1, "asc"]]
    });

}

function LoadActivityLogs() {
    $("#tblActivityLogs").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#tblActivityLogs").LoadingOverlay("show");
        } else {
            $("#tblActivityLogs").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketingLocation/LoadActivityLogs',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "placeId": $("#id").val()
                });
            }
        },
        "columns": [
            {
                data: "Date",
                orderable: false
            },
            {
                data: "Timeframe",
                orderable: false
            },
            {
                data: "Agent",
                orderable: false
            },
            {
                data: "Recorded",
                orderable: false
            },
            {
                data: "Notes",
                orderable: false
            },
        ],
        "order": [[1, "asc"]]
    });

}

function SocialCreateDiscussion() {
    if (isBusyAddTaskForm) {
        return;
    }
    $('#frm-create-discussion').submit(function (e) {
        e.preventDefault();
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Discussions/SaveDiscussionForPlace',
            datatype: 'json',
            data: { placeId: $('#placeId').val(), openingmessage: $('#ds_openingmessage').val(), isexpiry: $('#ds_isexpiry').prop('checked'), expirydate: $('#ds_expirydate').val() },
            beforeSend: function (xhr) {
                isBusyAddTaskForm = true;
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
                    cleanBookNotification.success(_L("ERROR_MSG_203"), "Sales Marketing");
                    $('#create-discussion').modal('hide');

                } else if (data.msg) {
                    cleanBookNotification.error(data.msg, "Sales Marketing");
                }
                isBusyAddTaskForm = false;
                LoadingOverlayEnd();
            },
            error: function (err) {
                isBusyAddTaskForm = false;
                LoadingOverlayEnd();
            }
        });
    });
}


$(function () {

    SocialCreateDiscussion();
    LoadScheduledVisits();
    LoadVisitLogs();
    LoadActivityLogs();
    $("#tblScheduledVisits").DataTable().ajax.reload();
});


ProcessPlaceAdd = function () {
    if (!$('#frm-place-addedit').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-place-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-place-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {


                $("#sm-place-object-key").val(mediaS3Object.objectKey);
                $("#sm-place-object-name").val(mediaS3Object.fileName);
                $("#sm-place-object-size").val(mediaS3Object.fileSize);

                SubmitPlaceDetail();
            }
        });
    }
    else {
        $("#sm-place-object-key").val("");
        $("#sm-place-object-name").val("");
        $("#sm-place-object-size").val("");
        SubmitPlaceDetail();
    }
};

function SubmitPlaceDetail() {
    var form_data = new FormData($('#frm-place-addedit')[0]);
    $.ajax({
        type: "post",
        url: "/SalesMarketing/SavePlace",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                var placeId = parseInt($('#placeId').val());
                if (placeId > 0) {
                    cleanBookNotification.success(_L("ERROR_MSG_200"), "Sales Marketing");
                    if (window.location.href.includes("PlaceDetail")) {
                        window.location.reload();
                    }
                } else
                    cleanBookNotification.success(_L("ERROR_MSG_237"), "Sales Marketing");
                LoadPlace(true);
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
};