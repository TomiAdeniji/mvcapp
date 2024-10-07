var $form_media_smresource = $("#form_media_smresource");
var activeTaskTab = "Open";
var isBusy = false;
var $slLocation = $('#locationId');
$(document).ready(function () {
    getCurrencySettings();
    LoadAssetTasks();
    initFormMedia();
    initSpannered();
});
function getCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
                    CurrencySymbol: '',
                    SymbolDisplay: 0,
                    DecimalPlace: 2
                };
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function initSpannered() {
    $('a[data-target="#asset-tasks"]').trigger('click');
    $('#searchMeterName').keyup(searchThrottle(function () {
        searchMeters();
    }));
    checkPermissionAddEdit("Assets", $("#workgroupId").val());
    checkPermissionAddEdit("Asset Tasks", $("#workgroupId").val());
    checkPermissionAddEdit("Meters", $("#workgroupId").val());
    $('#txtFilterPurchaseSearch').keyup(function () {
        $('#tblRelatedPurchases').DataTable().search(this.value).draw();
    });
    $('#slFilterPurchaseStatus').change(function () {
        $('#tblRelatedPurchases').DataTable().search(this.value).draw();
    });
    $('#txtFilterInventorySearch').keyup(function () {
        $('#tblAssetInventories').DataTable().search(this.value).draw();
    });
    $('#slFilterInventoryPurpose').change(function () {
        var whatsSelected = $(this).val() ? $(this).val() : [];
        whatsSelected = whatsSelected.join('|');
        $('#tblAssetInventories').DataTable().columns(5).search(whatsSelected, true, false).draw();
    });
}
function checkPermissionAddEdit(process, workgroupId) {
    $.post("/Spanneredfree/CheckPermissionAddEdit", { process: process, workgroupId: workgroupId }, function (data) {
        if (data.result) {
            if (process === "Assets") {
                $("#options-assets button").show();
                $("a[href='#app-spannered-asset-edit']").show();
            } else if (process === "Asset Tasks") {
                $("#task-options button").show();
            } else if (process === "Meters") {
                $("#meter-options button").show();
            }
        } else {
            if (process === "Assets") {
                $("#options-assets button").hide();
                $("a[href='#app-spannered-asset-edit']").hide();
            } else if (process === "Asset Tasks") {
                $("#task-options button").hide();
            } else if (process === "Meters") {
                $("#meter-options button").hide();
            }
        }
    });
}
function initFormMedia() {
    $form_media_smresource.validate({
        rules: {
            name: {
                required: true,
                minlength: 5
            },
            description: {
                required: true
            }
        }
    });
}



SaveSpanneredResource = function () {
    if (!$form_media_smresource.valid())
        return;
    ProcessMediaSpanneredResource();
};




ProcessMediaSpanneredResource = function () {
    $.LoadingOverlay("show");
    var files = document.getElementById("spannered-resource-file-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("spannered-resource-file-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#spannered-resource-object-key").val(mediaS3Object.objectKey);
            $("#spannered-resource-object-name").val(mediaS3Object.fileName);
            $("#spannered-resource-object-size").val(mediaS3Object.fileSize);

            SubmitSaveSpanneredResource();
        });

    } else
        SubmitSaveSpanneredResource();
};




SubmitSaveSpanneredResource = function () {
    var frmData = new FormData($form_media_smresource[0]);
    frmData.append("qbicleId", $("#taskQbicleId").val());
    $.ajax({
        type: "post",
        cache: false,
        url: "/Spanneredfree/SpanneredFreeSaveResource",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {

        },
        success: function (data) {
            if (data.result) {
                $('#create-resource').modal('hide');
                isBusy = false;
                LoadMedias($('#mediaFolderId').val(), $('#taskQbicleId').val());
                cleanBookNotification.success(_L("ERROR_MSG_172"), "Spannered");
                $form_media_smresource.trigger("reset");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Spannered");
                isBusy = false;
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

};

function LoadMedias(fid, qid) {
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/Spanneredfree/LoadMedias',
        datatype: 'json',
        data: { fid: fid, qid: qid, fileType: fileType == "All" ? "" : fileType },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#asset-resources .flex-grid-thirds-lg');
                $divcontain.html(listMedia);
                totop();
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function submitTask() {
    $.LoadingOverlay("show");
    var form_data = new FormData($form_task_addedit[0]);
    //get datas from table tblTaskInventoryCPS
    var dataInventoriescps = $('#tblTaskInventoryCPS').DataTable().rows().data();
    var _inventoriescps = [];
    $.each(dataInventoriescps, function (key, value) {
        var _allocated = 0;
        if (value.Allocated) {
            _allocated = value.Allocated;
        } else {
            var val = $(value[6]).val();
            _allocated = (val ? val : 0);
        }
        _inventoriescps.push({
            Id: value[0],
            AssetInventoryId: value[1],
            Allocated: _allocated
        });
    });
    if (_inventoriescps.length > 0)
        form_data.append("inventoriescps", JSON.stringify(_inventoriescps));
    //end
    $.ajax({
        type: "POST",
        url: "/Spanneredfree/SaveQbicleTask",
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
                $("#app-spannered-task-add").modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_199"), "Spannered");
                SearchAssetTask();
                ResetTask();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Spannered");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
            }
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
        }
    });
}
function saveTask() {
    if ($('#taskName').val() == "" || $('#taskProgrammedStart').val() == "" || $('#taskDuration').val() == "0") {
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
            if (refModel.result) {
                $form_task_addedit.validate().showErrors({ Name: _L("ERROR_MSG_111") });
                task_validtabs();
            }
            else {
                if ($('#taskAttachments').val()) {
                    var typeIsvalid = checkfile($('#taskAttachments').val());
                    if (typeIsvalid.stt) {
                        submitTask();
                    } else {
                        $form_task_addedit.validate().showErrors({ taskAttachments: typeIsvalid.err });
                    }
                } else {
                    submitTask();
                }

            }

        }).fail(function () {
            $("#form_task_addedit").validate().showErrors({ Name: _L("ERROR_MSG_112") });
        });
    } else
        task_validtabs();
};
function LoadAssetTasks() {
    $("#tblAssetTaskCompleted")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#tblAssetTaskCompleted').LoadingOverlay("show");
            } else {
                $('#tblAssetTaskCompleted').LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "order": [[0, "desc"]],
            "ajax": {
                "url": '/Spanneredfree/LoadAssetTasks',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "assetId": $("#assetId").val(),
                        "status": $("#assetTaskStatus").val(),
                        "assigneeId": $("#assetTaskAssignee").val(),
                        "searchName": $("#assetTaskSearch").val()
                    });
                }
            },
            "columns": [
                {
                    data: null,
                    orderable: true,
                    width: "200px",
                    render: function (value, type, row) {
                        return row.Id + " - " + row.Name;
                    }
                },
                {
                    data: "Created",
                    orderable: false
                },
                {
                    data: "Assignee",
                    orderable: false
                },
                {
                    data: "Due",
                    orderable: false
                },
                {
                    data: "MeterThreshold",
                    orderable: false,
                    width: "120px",
                    render: function (value, type, row) {
                        return value ? value.toLocaleString() : "";
                    }
                },
                {
                    data: "Status",
                    render: function (value, type, row) {
                        return '<span class="label label-success label-lg">Complete</span>';
                    }
                }
            ]
        });
    $("#tblAssetTaskUnCompleted")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#tblAssetTaskUnCompleted').LoadingOverlay("show");
            } else {
                $('#tblAssetTaskUnCompleted').LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "order": [[0, "desc"]],
            "ajax": {
                "url": '/Spanneredfree/LoadAssetTasks',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "assetId": $("#assetId").val(),
                        "status": $("#assetTaskStatus").val(),
                        "assigneeId": $("#assetTaskAssignee").val(),
                        "searchName": $("#assetTaskSearch").val()
                    });
                }
            },
            "columns": [
                {
                    data: null,
                    orderable: true,
                    width: "200px",
                    render: function (value, type, row) {
                        return row.Id + " - " + row.Name;
                    }
                },
                {
                    data: "Created",
                    orderable: false,
                    width: "120px"
                },
                {
                    data: "Assignee",
                    orderable: false
                },
                {
                    data: "Due",
                    orderable: false,
                    width: "120px"
                },
                {
                    data: "MeterThreshold",
                    orderable: false,
                    width: "120px",
                    render: function (value, type, row) {
                        return value ? value.toLocaleString() : "";
                    }
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
                        else return '';
                    }
                },
                {
                    data: null,
                    width: "110px",
                    orderable: false,
                    render: function (value, type, row) {
                        var str = '<div class="btn-group options">' +
                            '<button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">' +
                            'Options &nbsp; <i class="fa fa-angle-down"></i>' +
                            '</button>' +
                            '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">' +
                            '<li><a href="javascript:void(0)" onclick="ShowTaskPage(' + row.ActivityKey + ')">Manage</a></li>' +
                            '<li style="display: ' + (row.IsAllowEdit ? 'block' : 'none') + '"><a href="#app-spannered-task-add" data-toggle="modal" data-target="#app-spannered-task-add" onclick="LoadModalAssetTask(' + row.Id + ', ' + $("#assetId").val() + ')">Edit</a></li>' +
                            '</ul></div>';
                        return str;
                    }
                },
            ]
        });
}
function LoadMeterHistoryModal(id) {
    $("#app-spannered-meter-history").load("/Spanneredfree/LoadMeterHistoryModal", { id: id }, function () {
    })
}
function LoadModalMeter() {
    $("#app-spannered-meter-add").load("/Spanneredfree/LoadModalMeter", {}, function () {
        $('#form_meter_addedit').trigger("reset");
        $('#form_meter_addedit').validate({
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    maxlength: 250
                },
                Unit: {
                    required: true,
                    maxlength: 50
                },
                Description: {
                    required: true,
                    maxlength: 500
                }
            }
        });
        $('#form_meter_addedit').submit(function (e) {
            e.preventDefault();
            if ($('#form_meter_addedit').valid()) {
                $.LoadingOverlay("show");
                var form_data = new FormData($('#form_meter_addedit')[0]);
                form_data.append("assetId", $("#assetId").val());
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    data: form_data,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        isBusy = false;
                        if (data.result) {
                            $('#app-spannered-meter-add').modal('hide');
                            searchMeters();
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                        }
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                    }
                });
            } else {
                return;
            }
        });
    })
}
function searchMeters() {
    $.LoadingOverlay("show");
    $("#asset-meters").load("/Spanneredfree/LoadMeters", { name: $("#searchMeterName").val(), assetId: $("#assetId").val() }, function () {
        LoadingOverlayEnd();
    })
}
function ShowTaskPage(key) {
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Tasks/SetTaskSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                window.location.href = '/Qbicles/Task';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function SearchAssetTask(activeTab) {
    if (activeTab) {
        activeTaskTab = activeTab;
        if (activeTab === "History") {
            $("#assetTaskStatus").val("Complete").trigger("change");
            $('#assetTaskStatus').attr('disabled', 'disabled');
        } else {
            $("#assetTaskStatus").val(" ").trigger("change");
            $('#assetTaskStatus').removeAttr('disabled');
        }
    }
    if (activeTaskTab === "Open") {
        if ($("#assetTaskStatus").val() == "Complete") {
            activeTaskTab = "History";
            $('.tab-pane a[href="#tasks-1"]').tab('show');
            $("#assetTaskStatus").val("Complete").trigger("change");
            $('#assetTaskStatus').attr('disabled', 'disabled');
        } else {
            $("#tblAssetTaskUnCompleted").DataTable().ajax.reload();
        }
    } else {
        $("#tblAssetTaskCompleted").DataTable().ajax.reload();
    }
}
function checkAndUpdateValueOfUnit(meterId) {
    if ($("#valueOfUnit_" + meterId).val()) {
        $.ajax({
            type: 'post',
            url: '/Spanneredfree/UpdateValueOfUnit',
            datatype: 'json',
            data: {
                meterId: meterId, valueOfUnit: $("#valueOfUnit_" + meterId).val()
            },
            success: function (refModel) {
                if (refModel.result) {
                    $('.addreading_' + meterId).hide(); $('.meter-options_' + meterId).fadeIn();
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Spannered");
                    searchMeters();
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                }
            },
            fail: function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
            },
        });
    } else {
        cleanBookNotification.error("Value of unit cannot be empty!", "Spannered");
    }
}
function isOnToggle(id) {
    var result = "";
    $("select[name='ActivitiesRelate'] option").each(function () {
        if ($(this).val() == id) {
            result = "checked";
            return false;
        }
    });
    return result;
}
function LoadModalAssetTask(taskId, assetId) {
    $.LoadingOverlay("show");
    $("#app-spannered-task-add form").load("/Spanneredfree/LoadModalAssetTask", { taskId: taskId, assetId: assetId }, function () {
        LoadingOverlayEnd();
        $('select.select2').select2({ placeholder: 'Please select' });

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
            }
        });
        $("#sortable").sortable();
        $("#sortable").disableSelection();
        $('input[name="isSteps"],#chkIncludeOutOfStock').bootstrapToggle();
        $('#rlActivities')
            .on('processing.dt', function (e, settings, processing) {
                $('#processingIndicator').css('display', 'none');
                if (processing) {
                    $('#rlActivities').LoadingOverlay("show");
                } else {
                    $('#rlActivities').LoadingOverlay("hide", true);
                }
            })
            .dataTable({
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
                        return '<input class="chktoggle" data-toggle="toggle" data-on="Yes" ' + isOnToggle(data) + ' data-off="No" data-onstyle="success" onchange="AddRemoveAtvRelated(' + data + ',\'' + row.Name.replace("'", '') + '\',this)"  type="checkbox" value="' + data + '">';
                    }
                }],
                drawCallback: function (data) {
                    $('.chktoggle').bootstrapToggle();
                }
            });
        $('#tblTaskInventoryCPS').DataTable({
            "destroy": true,
            "autoWidth": false,
            "columnDefs": [{
                "targets": 0,
                "visible": false
            },
            {
                "targets": 1,
                "visible": false
            }
            ],
            "drawCallback": function (settings) {
                $(".trackInput").on("change", function () {
                    var $row = $(this).parents("tr");
                    var rowData = $('#tblTaskInventoryCPS').DataTable().row($row).data();
                    rowData.Allocated = $(this).val();
                })
            }
        });
        $('#txtFilterTaskCSPKeywork').keyup(function () {
            $('#tblTaskInventoryCPS').DataTable().search(this.value).draw();
        });
        $('#slFilterTaskCSPPurpose').change(function () {
            $('#tblTaskInventoryCPS').DataTable().columns(2).search($(this).val()).draw();
        });
        $.fn.dataTable.ext.search.push(
            function (settings, data, dataIndex) {
                if (settings.sTableId == 'tblTaskInventoryCPS') {
                    var value = stringToNumber(data[5]); // use data for the age column
                    if (!$('#chkIncludeOutOfStock').prop('checked') && value > 0) {
                        return true;
                    } else if ($('#chkIncludeOutOfStock').prop('checked')) {
                        return true;
                    }
                    return false;
                } else
                    return true
            }
        );
        $('#chkIncludeOutOfStock').change(function () {
            $('#tblTaskInventoryCPS').DataTable().draw();
        });

    })
}
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
function LoadPeopleTabByQbicleId(qbicleId) {
    $("#create-task-people").load("/Spanneredfree/LoadPeopleTabByQbicleId", { qbicleId: qbicleId }, function () {
        $('.select2avatartask').select2({
            placeholder: 'Please select',
            templateResult: SformatOptions,
            templateSelection: SformatSelected
        });
        if ($("#assigneeId").val()) {
            $("select[name='Assignee']").val($("#assigneeId").val());
            $("select[name='Assignee']").trigger("change");
        }
        if ($("#watcherId").val()) {
            $("select[name='Watchers']").val($("#watcherId").val());
            $("select[name='Watchers']").trigger("change");
        }
    })
}
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}
function LoadTopicsByQbicleId(topicId) {
    var option = $('select[name="WorkgroupId"] option:selected');
    var qbicleId = option.data('qbicleid');
    var topicId = topicId != 0 ? topicId : option.data('topicid');
    $.getJSON('/Topics/GetTopicByQbicleId', { qbicleId: (qbicleId ? qbicleId : 0), currentTopicId: topicId }, function (result) {
        $('#taskTopicId').select2({
            placeholder: "Please select",
            data: result
        });
    });
    $("#qbicleId").val(qbicleId);
    LoadPeopleTabByQbicleId(qbicleId);
}
function ShowTraderItemAdditional(traderItemId) {
    var ajaxUri = '/TraderItem/GetTraderItem?id=' + traderItemId;
    LoadingOverlay();
    $('#app-trader-item-additional').empty();
    $('#app-trader-item-additional').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
}
function ShowPurchaseItems(id) {
    var ajaxUri = '/Spanneredfree/LoadPurchaseItems?purchaseId=' + id;
    LoadingOverlay();
    $('#app-spannered-purchase-detail').empty();
    $('#app-spannered-purchase-detail').load(ajaxUri, function () {
        $('#app-spannered-purchase-detail .datatable').DataTable();
        LoadingOverlayEnd();
    });
}
function removeFromSpannered(aiId, elm) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Spannered",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Spanneredfree/RemoveItemSpanneredByAIId", { aiId: aiId }, function (Response) {
                    if (Response.result) {
                        $(elm).closest('tr').remove();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(response.msg, "Spannered");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Spannered");
                    }
                });
                return;
            }
        }
    });
}
function loadTeamsWorkgroupSpannered(wgId) {
    $('#team-view').empty();
    $('#team-view').modal('show');
    $('#team-view').load("/Spanneredfree/LoadTeamsByWorkgroupId?wgId=" + wgId);
}