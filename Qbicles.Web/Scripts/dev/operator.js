var isBusy = false;
var isHideLoading = false, goalPageSize = 8, perfomanceTrackingPageSize = 8, isInitpagination = false;
var $tblLeadingIndicators;
var $tblGoalMeasures;
function LoadOperatorConfigs() {
    $.LoadingOverlay("show");
    $("#app-config").load("/Operator/LoadOperatorConfigs", function () {
        LoadingOverlayEnd();
    });
}
function LoadOperatorGoals(skip, isLoad) {
    if (isLoad)
        isInitpagination = false;
    var isHide = $('#chkGoalIsHidden').prop('checked') ? true : false;
    var _tags = $('#slGoalTags').val();
    var _param = {
        skip: skip,
        take: goalPageSize,
        keyword: $('#txtGoalSearch').val(),
        isHide: isHide,
        tags: _tags ? _tags : []
    };
    $.get("/Operator/LoadGoals", _param, function (data) {
        if (data.result) {
            if (data.Object) {
                $('#goals-goals div.flex-grid-quarters-lg').html(data.Object.strResult);
                if (!isInitpagination)
                    initPagination(data.Object.totalRecord, goalPageSize, '#GoalPaginateTemplate');
                if (data.Object.totalRecord == 0)
                    $('#GoalPaginateTemplate').hide();
            }
        }
    });
}
function LoadOperatorMeasures() {
    initMeasuresTable();
}
function LoadOperatorTags() {
    $.get("/Operator/getTagsAll", function (data) {
        if (data.length > 0) {
            $('#slGoalTags').empty();
            $('#slGoalTags').append("<option selected>Show all</option>");
            data.forEach(function (item) {
                $('#slGoalTags').append("<option value=\"" + item.Id + "\">" + item.Name + "</option>");
            });
            $('#slGoalTags').select2({ placeholder: 'Please select' });
        }
    });
}
function initMeasuresTable() {
    // Tags
    $tblMeasures = $('#tblMeasures');
    $tblMeasures.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblMeasures.LoadingOverlay("show");
        } else {
            $tblMeasures.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Operator/SearchMeasures",
            "data": function (d) {
                return $.extend({}, d, {
                    "measureName": $('#txtmeasureSearch').val(),
                });
            }
        },
        columns: [
            { "title": "Measure", "data": "Measure", "searchable": true, "orderable": true },
            { "title": "Summary", "data": "Summary", "searchable": true, "orderable": true },
            { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 2,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<div class="btn-group options"><button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> &nbsp; Options</button>';
                    _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="#" onclick="loadModalMeasure(' + data + ');">Edit</a></li>' + (row.AllowDelete ? '<li><a href="#" onclick="deleteMeasure(' + data + ')">Delete</a></li>' : '') + '</ul></div>';
                    return _htmlOptions;
                }
            }]
    });
}
function loadModalGoal(id) {
    $('#app-operator-goal-addedit').modal("show");
    $("#app-operator-goal-addedit").load("/Operator/LoadModalGoal", { id: id }, function () {
        initGoalModal();
    });
}
function initGoalModal() {
    var $frmGoal = $('#frmGoal');
    $tblLeadingIndicators = $('#tblLeadingIndicators').DataTable({
        "destroy": true,
        "order": [[4, "desc"]],
        "columnDefs": [
            { "visible": false, "targets": 0 },
            { "visible": false, "targets": 1 },
            {
                "targets": 4,
                "render": function (data, type, row, meta) {
                    if (typeof data == "number")
                        return data + '%';
                    else
                        return data.replace("%", "") + '%';
                }
            },
            {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-danger" type="button" onclick="deleteRowMeasure(\'tblLeadingIndicators\',this)"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            }
        ]
    });
    $tblGoalMeasures = $('#tblGoalMeasures').DataTable({
        "destroy": true,
        "order": [[4, "desc"]],
        "columnDefs": [
            { "visible": false, "targets": 0 },
            { "visible": false, "targets": 1 },
            {
                "targets": 4,
                "render": function (data, type, row, meta) {
                    if (typeof data == "number")
                        return data + '%';
                    else
                        return data.replace("%", "") + '%';
                }
            },
            {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-danger" onclick="deleteRowMeasure(\'tblGoalMeasures\',this)"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            }
        ]
    });
    $('#frmGoal select.select2').select2({ placeholder: 'Please select' });
    $("#frmGoal .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('.btnNext').click(function () {
        var parent = $(this).closest('.modal');
        if ($frmGoal.valid()) {
            $(parent).find('#tabNavGoals .active').next('li').find('a').trigger('click');
        } else {
            $(parent).find('#tabNavGoals .active').find('a').trigger('click');
        }
    });
    $('.btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('#tabNavGoals .active').prev('li').find('a').trigger('click');
    });
    $('#btnLMAdd').click(function () {
        if ($('#frmGoal select[name=lm_measure]').val() != '' && $("#frmGoal input[name=lm_weight]").valid()) {
            var $option = $('#frmGoal select[name=lm_measure] option:selected');
            var measureId = $option.val();
            var measureName = $option.text();
            var measureDesc = $option.attr("desc");
            var measureWeight = parseInt($('#frmGoal input[name=lm_weight]').val());
            //Validate total data LeadingIndicators
            var dataIndicators = $tblLeadingIndicators.rows().data();
            var _totalLeadingIndicators = measureWeight;
            $.each(dataIndicators, function (key, value) {
                if (typeof value[4] == "number")
                    _totalLeadingIndicators += value[4];
                else {
                    _totalLeadingIndicators += parseInt(value[4].replace("%", ""));
                }
            });
            if (_totalLeadingIndicators > 100) {
                cleanBookNotification.error(_L("ERROR_MSG_703"), "Operator");
                return;
            }
            //end
            $tblLeadingIndicators.row.add({
                0: 0,
                1: measureId,
                2: measureName,
                3: measureDesc,
                4: measureWeight,
                5: 0,
            }).draw(false);
            //reset form Meter
            $option.remove();
            $('#frmGoal select[name=lm_measure]').select2({ placeholder: 'Please select' });
            $('#btnLMAdd').attr("disabled", true);
            $('#frmGoal select[name=lm_measure]').val('').trigger("change");
            $('#frmGoal input[name=lm_weight]').val('0');
            //end reset
        }
    });
    $('#btnGMAdd').click(function () {
        if ($('#frmGoal select[name=gm_measure]').val() != '' && $("#frmGoal input[name=gm_weight]").valid()) {
            var $option = $('#frmGoal select[name=gm_measure] option:selected');
            var measureId = $option.val();
            var measureName = $option.text();
            var measureDesc = $option.attr("desc");
            var measureWeight = parseInt($('#frmGoal input[name=gm_weight]').val());
            //Validate total data LeadingIndicators
            var dataMeasures = $tblGoalMeasures.rows().data();
            var _totalWeightGoalMeasures = measureWeight;
            var isObjectCellWeight = false;
            $.each(dataMeasures, function (key, value) {
                if (typeof value[4] == "number")
                    _totalWeightGoalMeasures += value[4];
                else {
                    _totalWeightGoalMeasures += parseInt(value[4].replace("%", ""));
                }

            });
            if (_totalWeightGoalMeasures > 100) {
                cleanBookNotification.error(_L("ERROR_MSG_703"), "Operator");
                return;
            }
            //end
            $tblGoalMeasures.row.add({
                0: 0,
                1: measureId,
                2: measureName,
                3: measureDesc,
                4: measureWeight,
                5: 0,
            }).draw(false);
            //reset form Meter
            $option.remove();
            $('#frmGoal select[name=gm_measure]').select2({ placeholder: 'Please select' });
            $('#btnGMAdd').attr("disabled", true);
            $('#frmGoal select[name=gm_measure]').val('').trigger("change");
            $('#frmGoal input[name=gm_weight]').val('0');
            //end reset
        }
    });
    $frmGoal.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 100
            },
            Summary: {
                required: true,
                maxlength: 500
            },
            Tags: {
                required: true,
            }
        }
    });
    $frmGoal.submit(function (e) {
        e.preventDefault();
        if ($frmGoal.valid()) {
            if ($frmGoal.valid()) {
                $.LoadingOverlay("show");
                var files = document.getElementById("goal-featured-image").files;
                if (files.length > 0) {
                    UploadMediaS3ClientSide("goal-featured-image").then(function (mediaS3Object) {

                        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                            LoadingOverlayEnd('hide');
                            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                            return;
                        }
                        else {
                            $("#operator-goal-feature-image-object-key").val(mediaS3Object.objectKey);
                            $("#operator-goal-feature-image-object-name").val(mediaS3Object.fileName);
                            $("#operator-goal-feature-image-object-size").val(mediaS3Object.fileSize);

                            SaveGoalOnOperator();
                        }
                    });
                }
                else {
                    $("#operator-goal-feature-image-object-key").val("");
                    $("#operator-goal-feature-image-object-name").val("");
                    $("#operator-goal-feature-image-object-size").val("");

                    SaveGoalOnOperator();
                }
            } 
        } 
    });

}

function SaveGoalOnOperator() {
    var frmData = new FormData($('#frmGoal')[0]);
    var dataIndicators = $tblLeadingIndicators.rows().data();
    var dataMeasures = $tblGoalMeasures.rows().data();
    //get data LeadingIndicators
    var _leadingIndicators = [];
    $.each(dataIndicators, function (key, value) {
        _leadingIndicators.push({
            Id: value[0],
            MeasureId: value[1],
            Weight: (typeof value[4] == "number" ? value[4] : value[4].replace("%", ""))
        });
    });
    //end
    //get data GoalMeasures
    var _goalMeasures = [];
    $.each(dataMeasures, function (key, value) {
        _goalMeasures.push({
            Id: value[0],
            MeasureId: value[1],
            Weight: (typeof value[4] == "number" ? value[4] : value[4].replace("%", ""))
        });
    });
    //end
    if (_leadingIndicators.length > 0)
        frmData.append("sLeadingIndicators", JSON.stringify(_leadingIndicators));
    else {
        cleanBookNotification.error(_L('ERROR_MSG_704'), "Operator");
        $('#tabNavGoals a[href=#goal-2]').click();
        LoadingOverlayEnd();
        return;
    }

    if (_goalMeasures.length > 0)
        frmData.append("sGoalMeasures", JSON.stringify(_goalMeasures));
    else {
        cleanBookNotification.error(_L('ERROR_MSG_705'), "Operator");
        $('#tabNavGoals a[href=#goal-3]').click();
        LoadingOverlayEnd();
        return;
    }
    $.ajax({
        type: "post",
        cache: false,
        url: "/Operator/SaveGoal",
        data: frmData,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                $('#app-operator-goal-addedit').modal('hide');
                LoadOperatorGoals(0, true);
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Operator");
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });

}
function deleteRowMeasure(tableId, elm) {
    var crow = $(elm).parents('tr');
    if (crow) {
        var $dataRow = $('#' + tableId).DataTable().row(crow);
        var data = $dataRow.data();
        var measureId = data[1];
        var measureName = data[2];
        var measureDesc = data[3];

        if (tableId == "tblLeadingIndicators") {
            if ($('#frmGoal select[name=lm_measure] option[value=' + measureId + ']').length == 0) {
                var $option = $('#frmGoal select[name=lm_measure]').append("<option value=\"" + measureId + "\" desc=\"" + measureDesc.replace(/'/g, "\\'").replace(/"/g, '&#34;') + "\">" + measureName + "</option>");
                $('#frmGoal select[name=lm_measure]').select2({ placeholder: 'Please select' });
            }
        } else {
            if ($('#frmGoal select[name=lm_measure] option[value=' + measureId + ']').length == 0) {
                var $option = $('#frmGoal select[name=gm_measure]').append("<option value=\"" + measureId + "\" desc=\"" + measureDesc.replace(/'/g, "\\'").replace(/"/g, '&#34;') + "\">" + measureName + "</option>");
                $('#frmGoal select[name=gm_measure]').select2({ placeholder: 'Please select' });
            }
        }
        $dataRow.remove().draw();
    }


}
function loadModalMeasure(id) {
    $('#frmMeasure').validate().resetForm();//remove error class on name elements and clear history
    if (id > 0) {
        $.get("/Operator/getMeasureById?id=" + id, function (data) {
            if (data) {
                $('#app-operator-measure-addedit h5.modal-title').text('Edit a Measure');
                $('#frmMeasure input[name="Id"]').val(data.Id);
                $('#frmMeasure input[name="Name"]').val(data.Name);
                $('#frmMeasure textarea[name="Summary"]').val(data.Summary);
                $('#app-operator-measure-addedit').modal('show');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_410", [id]), "Operator");
            }
        });
    } else {
        $('#app-operator-measure-addedit h5.modal-title').text('Add a Measure');
        $('#frmMeasure input[name="Id"]').val(0);
        $('#frmMeasure input[name="Name"]').val('');
        $('#frmMeasure textarea[name="Summary"]').val('');
        $('#app-operator-measure-addedit').modal('show');
    }
}
function initMeasureModal() {
    // Measures
    var $frmMeasure = $('#frmMeasure');
    $frmMeasure.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 100
            },
            Summary: {
                required: true,
                maxlength: 250
            }
        }
    });
    $frmMeasure.submit(function (e) {
        e.preventDefault();
        if ($frmMeasure.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-measure-addedit').modal('hide');
                        $('#tblMeasures').DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            LoadingOverlayEnd();
            return;
        }
    });
}
function initPagination(totalRecord, pageSize, elementID) {
    var container = $(elementID);
    if (totalRecord != 0) {
        container.show();
        var sources = function () {
            var result = [];
            for (var i = 1; i <= totalRecord; i++) {
                result.push(i);
            }
            return result;
        }();

        var options = {
            prevText: '&nbsp; &laquo; Prev &nbsp;',
            nextText: '&nbsp; Next &raquo; &nbsp;',
            currentPage: 1,
            pageSize: pageSize,
            dataSource: sources,
            callback: function (response, pagination) {
                if (isInitpagination) {
                    switch (elementID) {
                        case '#GoalPaginateTemplate': LoadOperatorGoals((pagination.pageNumber - 1) * pageSize, false); break;
                        case '#PerformancePaginateTemplate': LoadPerformanceTrackings((pagination.pageNumber - 1) * pageSize, false); break;
                    }
                }
            }
        };
        container.pagination(options);
        isInitpagination = true;
    } else {
        container.hide();
        isInitpagination = false;
    }
}
function updateHidden(id, isHidden) {
    $.post("/Operator/UpdateHiddenGoal", { id: id, isHidden: isHidden }, function (data) {
        var _pageNum = $('#GoalPaginateTemplate').pagination("getSelectedPageNum");
        loadAssets((_pageNum - 1) * goalPageSize);
    });
}

function ShowOrHidePerformanceTracking(id, isHide) {
    $.post("/Operator/ShowOrHidePerformanceTracking", { id: id, isHide: isHide }, function (data) {
        LoadPerformanceTrackings(0, true);
    });
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
function initSearch() {
    //Tab Goal
    $('#txtGoalSearch').keyup(searchThrottle(function () {
        LoadOperatorGoals(0, true);
    }));
    $('#slGoalTags').change(searchThrottle(function () {
        LoadOperatorGoals(0, true);
    }));
    $('#chkGoalIsHidden').change(searchThrottle(function () {
        LoadOperatorGoals(0, true);
    }));
    //end Tab Goal
    //Tab Time & Attendance
    $('#txtSearchClock').keyup(searchThrottle(function () {
        $('#tblAttendances').DataTable().ajax.reload();
    }));
    $('#txtFilterDateClock,#slFilterPeople').change(searchThrottle(function () {
        $('#tblAttendances').DataTable().ajax.reload();
    }));
    //End Tab Time & Attendance
    //Tab Tasks & Forms
    $('#txtFormsSearch').keyup(searchThrottle(function () {
        $('#tblForms').DataTable().ajax.reload();
    }));
    $('#slFormsTags').change(searchThrottle(function () {
        $('#tblForms').DataTable().ajax.reload();
    }));
    //End tab Tasks & Forms
    //Tab Tasks
    $('#txtTaskKeyword').keyup(searchThrottle(function () {
        $('#tblComplianceTasks').DataTable().ajax.reload();
    }));
    $('#slTaskAssignee,#slTaskForms').change(searchThrottle(function () {
        $('#tblComplianceTasks').DataTable().ajax.reload();
    }));
    //End
}
function updateOptionGoal(id, isHide) {
    $.post("/Operator/UpdateOptionGoal", { id: id, isHide: isHide }, function (data) {
        var _pageNum = $('#GoalPaginateTemplate').pagination("getSelectedPageNum");
        LoadOperatorGoals((_pageNum - 1) * goalPageSize, false);
    });
}
function deleteMeasure(id) {
    $.post("/Operator/DeleteMeasure", { id: id }, function (data) {
        if (data.result) {
            $('#tblMeasures').DataTable().ajax.reload();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
        }
        else if (!data.result && data.msg) {
            cleanBookNotification.error(_L(data.msg), "Operator");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}
function checkMeasuresExist() {
    $.get("/Operator/CheckMeasuresExist", function (response) {
        if (!response.isExist) {
            $('#goal-help-text').show();
        } else {
            $('#goal-help-text').hide();
        }
    });
}
$(document).ready(function () {
    var url_string = window.location.href; // www.test.com?filename=test
    var url = new URL(url_string);
    var tabValue = url.searchParams.get("tab");
    if ($('#operatorQbicleId').val() == "0")
        tabValue = "app-config";
    if (tabValue) {
        $('#tabApp ' + 'a[href=#' + tabValue + ']').trigger('click');
    } else
        initTeamPerson();
    initSearch();
    initMeasureModal();
    $('#sl-media-type').change(searchThrottle(function () {
        LoadMedias();
    }));
    initModalClock();
    initModalSchedule();
});
function detailPerson(el) {
    $(".contact-add .activity-overview a").prop("href", "/Community/UserProfilePage?uId=" + $(el).data("id"));
    $(".contact-add .activity-overview h2").text($(el).data("fullname"));
    $(".contact-add .activity-overview p").text($(el).data("summary"));
    $(".contact-add .activity-overview .contact-avatar-profile").css('background-image', 'url(' + $(el).data("avatar") + ')');
    $(".contact-add input[name=email]").val($(el).data("email"));
    $(".contact-add input[name=tel]").val($(el).data("tel"));
    $(".contact-add input[name=memberId]").val($(el).data("id"));
    $('.contact-list-found').hide();
    $('.contact-add').hide();
    $('.contact-add').fadeIn();
    $('.search_user').hide();
}
function searchDomainUsers(el) {
    $(".existing-member").show();
    var searchVal = $(el).val() ? $(el).val().toLowerCase() : "";
    $(".contact-list-found li").hide();
    $(".contact-list-found li:not(.letters)").each(function () {
        if (searchVal === "" || $(this).find("h5").text().toLowerCase().includes(searchVal)) {
            $(this).show();
            $(this).prev().show();
        }
    });
}
function initTeamPerson() {
    var $tblTeamPersons = $('#tblTeamPersons');
    $tblTeamPersons.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblTeamPersons.LoadingOverlay("show");
        } else {
            $tblTeamPersons.LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            autoWidth: true,
            lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchTeamPersons",
                "data": function (d) {
                    return $.extend({}, d, {
                        "teamPersonSearch": $('#teamPersonSearch').val(),
                        "teamPersonLocationSearch": $('#teamPersonLocationSearch').val(),
                        "teamPersonRoleSearch": $('#teamPersonRoleSearch').val(),
                    });
                }
            },
            columns: [
                { "title": "Person", "data": "Name", "searchable": true, "orderable": false },
                { "title": "Location", "data": "Location", "searchable": true, "orderable": true },
                { "title": "Role", "data": "Role", "searchable": true, "orderable": true },
                { "title": "Workgroup", "data": "Workgroup", "searchable": true, "orderable": true },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
                {
                    "targets": 0,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<a href="/Operator/DetailTeamPerson?id=' + row.Id + '">' +
                            '<div class="table-avatar mini pull-left" style="background-image: url(\'' + row.Avatar + '\');"></div>' +
                            '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + row.Name + '</div>' +
                            '<div class="clearfix"></div>' +
                            '</a>';
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 4,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<div class="btn-group options">' +
                            '<button type = "button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" >' +
                            '<i class="fa fa-cog"></i> &nbsp; Options' +
                            '</button>' +
                            '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">' +
                            '<li><a href="/Operator/DetailTeamPerson?id=' + data + '">View</a></li>' +
                            '<li><a href="#" data-toggle="modal" data-target="#app-operator-person-addedit" onclick="loadTeamPersonModal(' + data + ',\'' + row.MemberId + '\')">Edit</a></li>' +
                            '<li><a href="#" onclick="removeTeamPerson(' + data + ')">Remove</a></li>' +
                            '</ul>' +
                            '</div >';
                        return _htmlOptions;
                    }
                }]
        });

    $('#teamPersonSearch').keyup(searchThrottle(function () {
        $tblTeamPersons.DataTable().ajax.reload();
    }));

    $('#teamPersonLocationSearch, #teamPersonRoleSearch').change(searchThrottle(function () {
        $tblTeamPersons.DataTable().ajax.reload();
    }));

    $('a[href="#people-people"]').click(function () {
        $tblTeamPersons.DataTable().ajax.reload();
    })
}

function loadTeamPersonModal(id, memberId) {
    $.LoadingOverlay("show");
    $("#app-operator-person-addedit").load("/Operator/LoadTeamPersonModal", { id: id }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: true,
            selectAllJustVisible: true,
            includeResetOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true,
            enableFiltering: true,
            enableCaseInsensitiveFiltering: true

        })
        if (id != 0) {
            $(".widget-contacts li a[data-convertedid='" + memberId.replace(/-/g, '') + "']").first().trigger("click");
        }
        var $tblTeamPersons = $('#tblTeamPersons');
        $frmOperatorTeamPerson = $("#frmOperatorTeamPerson");
        $frmOperatorTeamPerson.submit(function (e) {
            e.preventDefault();
            if ($frmOperatorTeamPerson.valid()) {
                $.LoadingOverlay("show");
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    data: $(this).serialize(),
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        isBusy = false;
                        if (data.result) {
                            $('#app-operator-person-addedit').modal('hide');
                            $tblTeamPersons.DataTable().ajax.reload();
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(_L(data.msg), "Operator");
                        }
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                    }
                });
            }
        });
        LoadingOverlayEnd();
    });
}

function removeTeamPerson(id) {
    $.LoadingOverlay("show");
    var $tblTeamPersons = $('#tblTeamPersons');
    $.ajax({
        url: "/Operator/RemoveTeamPerson",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $tblTeamPersons.DataTable().ajax.reload();
                cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function backToSearch() {
    $('.contact-add').hide();
    $('.contact-list-found').fadeIn();
    $('select[name=lstRoleIds]').val([]);
    $('select[name=lstLocationIds]').val([]);
    $("select[name=lstLocationIds]").multiselect("refresh");
    $('select[name=lstRoleIds]').multiselect("refresh");
    $('.search_user').show();
}

function LoadMedias() {
    $.LoadingOverlay("show");
    var fid = $('#mediaFolderId').val();
    var qid = $('#operatorQbicleId').val();
    var fileType = $('#sl-media-type').val();

    $.ajax({
        type: 'post',
        url: '/Operator/LoadMedia',
        datatype: 'json',
        data: { fid: fid, qid: qid, fileType: fileType == "All" ? "" : fileType, rs: "" },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#asset-resources .flex-grid-thirds-lg');
                $divcontain.html(listMedia);
                totop();
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadFolderInfor() {
    $.ajax({
        type: 'post',
        url: '/Operator/GetFolderInfor',
        datatype: 'json',
        data: {},
        success: function (response) {
            $('#operatorQbicleId').val(response.qbicleId);
            $('#mediaFolderId').val(response.folderId);
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}

function LoadTeam() {
    manage_options('#options-people');
    $('.section-detail').hide();
    $('.intro-people').show();
    $('.appnav').hide();
    $('.nav-people').show();
    $('.nav-people a[href="#people-people"]').trigger('click');
    LoadFolderInfor();
}

function LoadResourceModal(folderId) {
    $("#create-resource").load("/Operator/LoadResourceModal", { folderId: folderId }, function () {
        $('#create-resource .select2').select2({ placeholder: "Please select" });
        $form_media_addedit = $("#form_media_addedit");
        $form_media_addedit.validate({
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
        $form_media_addedit.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($form_media_addedit.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($form_media_addedit[0]);
                frmData.append("qbicleId", $("#operatorQbicleId").val());
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $('#create-resource').modal('hide');
                            isBusy = false;
                            LoadMedias($('#mediaFolderId').val(), $('#operatorQbicleId').val());
                            cleanBookNotification.success(_L("ERROR_MSG_172"), "Operator");
                            $form_media_addedit.trigger("reset");
                        } else if (data.msg) {
                            cleanBookNotification.error(data.msg, "Operator");
                            isBusy = false;
                        }
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
    });
}
function loadWorkgroupPreview(wgid,frmid) {
    $(frmid +' .preview-localtion').text('');
    $(frmid +' .preview-qbicle').text('');
    $(frmid +' .preview-members').text('0');
    if (wgid) {
        $.get("/Operator/LoadWorkgroupPreview?wgid=" + wgid, function (response) {
            if (response) {
                $(frmid +' .preview-localtion').text(response.localtion);
                $(frmid +' .preview-qbicle').text(response.qbicle);
                $(frmid + ' .preview-members').text(response.countmember);
                $(frmid + ' .preview-members').parent().attr("onclick", "ShowTeamMembers(" + wgid + ")");
                var slPeoples = (frmid == "#frmSchedule" ? $(frmid + ' select[name=Employees]'): $(frmid+' select[name=Peoples]'));
                slPeoples.empty();
                response.members.forEach(function (item) {
                    slPeoples.append("<option value=\"" + item.UserId + "\">" + item.Fullname + "</option>");
                });
                slPeoples.multiselect("destroy").multiselect({
                    includeSelectAllOption: true,
                    enableFiltering: true,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            }
        });
    } else {
        var slPeoples = (frmid == "#frmSchedule" ? $(frmid + ' select[name=Employees]') : $(frmid + ' select[name=Peoples]'));
        slPeoples.empty();
        slPeoples.multiselect("destroy").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
    }
}
function initModalClock() {
    var $frmClock = $('#frmClock');
    $frmClock.validate({
        ignore: "",
        rules: {
            Workgroup: {
                required: true,
            },
            Date: {
                required: true,
            },
            Time: {
                required: true,
            },
            Peoples: {
                required: true,
            },
            Notes: {
                maxlength: 300
            }
        }
    });
    $frmClock.submit(function (e) {
        e.preventDefault();
        if ($frmClock.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-clockin').modal('hide');
                        $('#tblAttendances').DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        resetFormClock();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            LoadingOverlayEnd();
            return;
        }
    });
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('.daterange').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('.daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#tblAttendances').DataTable().ajax.reload();
    });
    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
    });
}
function resetFormClock() {
    $('.preview-workgroup').hide();
    $('#frmClock input[name=Date]').val('');
    $('#frmClock input[name=Time]').val('');
    $('#frmClock select[name=Peoples]').multiselect("clearSelection");
    $('#frmClock input[name=Notes]').val('');
    $('#frmClock').validate().resetForm();
}
function initAttendancesTable() {
    // Tags
    var $tblAttendances = $('#tblAttendances');
    $tblAttendances.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Operator/SearchAttendances",
            "data": function (d) {
                return $.extend({}, d, {
                    "searchFullname": $('#txtSearchClock').val(),
                    "filterdate": $('#txtFilterDateClock').val(),
                    "peoples": $('#slFilterPeople').val() ? JSON.stringify($('#slFilterPeople').val()) : "",
                });
            }
        },
        columns: [
            { "title": "Person", "data": "Person", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Location", "data": "Location", "searchable": true, "orderable": true },
            { "title": "Time in", "data": "TimeIn", "searchable": true, "orderable": true },
            { "title": "Time out", "data": "TimeOut", "searchable": true, "orderable": true },
            { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 0,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<a href="/Community/UserProfilePage?uId=' + row.PersonId + '" target="_blank"><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.PersonUrl + '\');"></div>';
                    _htmlOptions += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + data + '</div><div class="clearfix"></div></a>';
                    return _htmlOptions;
                }
            },
            {
                "targets": 3,
                "data": "TimeIn",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<input type="time" class="form-control timeIn' + row.Id + '" onchange="$(\'.confirm' + row.Id + '\').fadeIn();" value="' + data + '">';
                    return _htmlOptions;
                }
            },
            {
                "targets": 4,
                "data": "TimeOut",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<input type="time" class="form-control timeOut' + row.Id + '" onchange="$(\'.confirm' + row.Id + '\').fadeIn();" value="' + data + '">';
                    return _htmlOptions;
                }
            },
            {
                "targets": 5,
                "data": "TimeOut",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = ' <div class="confirm' + row.Id + '" style="display: none;">';
                    _htmlOptions += '<button class="btn btn-success" onclick="updateClockout(' + row.Id + ')"><i class="fa fa-check"></i></button>';
                    _htmlOptions += '<button class="btn btn-danger" onclick="$(\'.confirm' + row.Id + '\').hide();"><i class="fa fa-remove"></i></button></div>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function updateClockout(id) {
    var paramaters = {
        id: id,
        clockIn: $('.timeIn' + id).val(),
        clockOut: $('.timeOut' + id).val()
    }
    $.post("/Operator/UpdateClock", paramaters, function (response) {
        if (response.result) {
            $('.confirm' + id).hide();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}
function initModalSchedule() {
    var _format = $dateFormatByUser.toUpperCase();
    //init for first time
    var startDate = moment($('#frmSchedule input[name=StartDate]').val(), _format).toDate();
    var endDate = moment($('#frmSchedule input[name=EndDate]').val(), _format).toDate();
    calcDaysByDates(startDate, endDate);
    //end init
    $('#frmSchedule input[name=StartDate],#frmSchedule input[name=EndDate]').change(searchThrottle(function () {
        var startDate = moment($('#frmSchedule input[name=StartDate]').val(), _format).toDate();
        var endDate = moment($('#frmSchedule input[name=EndDate]').val(), _format).toDate();
        var maxdate = moment($('#frmSchedule input[name=StartDate]').val(), _format).add(3, "month");
        if (endDate > maxdate.toDate()) {
            cleanBookNotification.error(_L('ERROR_MSG_707'), "Operator");
            $('#frmSchedule input[name=EndDate]').val(maxdate.format(_format));
            endDate = maxdate.toDate();
        }
        calcDaysByDates(startDate, endDate);
    }));
    //form add schedule
    var $frmSchedule = $('#frmSchedule');
    $frmSchedule.validate({
        ignore: "",
        rules: {
            Workgroup: {
                required: true,
            },
            Employees: {
                required: true,
            },
            StartDate: {
                required: true,
            },
            EndDate: {
                required: true,
            },
            Days: {
                required: true,
            },
            ShiftStart: {
                required: true,
            },
            ShiftEnd: {
                required: true,
            }
        }
    });
    $frmSchedule.submit(function (e) {
        e.preventDefault();
        if ($frmSchedule.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-schedule-add').modal('hide');
                        initTablesDailyWeeklyMonthlyOfSched();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        resetFormSchedule();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            LoadingOverlayEnd();
            return;
        }
    });
    $('#frmSchedule .singledate,#day .singledate,#sched-day .singledate').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: _format
        }
    });
    calcWeekFilterByDate_timetimesheet();
    calcMonthFilterByDate_timetimesheet();
    calcWeekFilterByDate_timeschedule();
    calcMonthFilterByDate_timeschedule();
    //form Update schedule
    var $frmScheduleEdit = $('#frmScheduleEdit');
    $frmScheduleEdit.validate({
        ignore: "",
        rules: {
            ShiftStart: {
                required: true,
            },
            ShiftEnd: {
                required: true,
            }
        }
    });
    $frmScheduleEdit.submit(function (e) {
        e.preventDefault();
        if ($frmScheduleEdit.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-schedule-individual-edit').modal('hide');
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        initTablesDailyWeeklyMonthlyOfSched();
                        loadScheduleData(0);
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            LoadingOverlayEnd();
            return;
        }
    });
    //Init filter tab Schedules
    $('#sched-day input[name=Day],#sched-week select[name=Week],#sched-month select[name=Month],#slSchedPeoples,#slSchedRoles,#slSchedLocations').change(searchThrottle(function () {
        initTablesDailyWeeklyMonthlyOfSched();
    }));
    $('#day input[name=Day],#week select[name=Week],#month select[name=Month],#slTimeshPeoples,#slTimeshRoles,#slTimeshLocations').change(searchThrottle(function () {
        initTablesDailyWeeklyMonthlyOfTimeSheets();
    }));
}
function calcDaysByDates(startDate, endDate) {
    var slDays = $('#frmSchedule select[name=Days]');
    slDays.empty();
    var _format = $dateFormatByUser.toUpperCase();
    var _startDate = new Date(startDate);
    while (_startDate <= endDate) {
        var _day = moment(_startDate);
        if (_day.weekday() == 6 || _day.weekday() == 0)
            slDays.append("<option value=\"" + _day.format(_format) + "\">" + _day.format("dddd " + _format) + "</option>");
        else
            slDays.append("<option value=\"" + _day.format(_format) + "\" selected>" + _day.format("dddd " + _format) + "</option>");
        _startDate.setDate(_startDate.getDate() + 1);
    }
    slDays.multiselect("destroy").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
}
function calcWeekFilterByDate_timetimesheet() {
    var slDay = $('#week select[name=Week]');
    slDay.empty();
    var _format = $dateFormatByUser.toUpperCase();
    var _currentDate = new Date();
    var _endDate = moment(_currentDate).day("Saturday").add(1, "day").toDate();
    var _startDate = moment(_endDate).add(-3, "month").day("Monday").toDate();
    while (_endDate >= _startDate) {
        var _tempday = moment(_endDate);
        var _day = moment(_endDate).add(-1, "week");
        var _daytemp = moment(_endDate).add(-1, "week").add(1, "day");
        _endDate = _day.toDate();
        var _value = _day.add(1,"day").format(_format) + "-" + _tempday.format(_format);
        if (_tempday.month() == _day.month()) {
            slDay.append("<option value=\"" + _value + "\">" + _daytemp.format("Do") + " - " + _tempday.format("Do ") + _daytemp.format("MMMM YYYY") + "</option>");
        } else {
            slDay.append("<option value=\"" + _value + "\">" + _daytemp.format("Do MMMM YYYY") + " - " + _tempday.format("Do MMMM YYYY") + "</option>");
        }
    }
    slDay.select2({ placeholder: 'Please select' });
}
function calcMonthFilterByDate_timetimesheet() {
    var slDays = $('#sched-month select[name=Month],#month select[name=Month]');
    slDays.empty();
    var _format = $dateFormatByUser.toUpperCase();
    var _currentDate = new Date();
    var _endDate = new Date(_currentDate.getFullYear(), _currentDate.getMonth() + 1, 0);
    var _maxMonth =1;
    while (_maxMonth<=3) {
        var _tempday = moment(_endDate);
        var _day = moment(_endDate).subtract(_endDate.getDate(),'day');
        _endDate = _day.toDate();
        slDays.append("<option value=\"" + _day.add(1, "day").format(_format) + "-" + _tempday.add(1, "day").format(_format) + "\">" + _day.format("MMMM YYYY") + "</option>");
        _maxMonth++;
    }
    slDays.select2({ placeholder: 'Please select' });
}
function calcWeekFilterByDate_timeschedule() {
    var slDay = $('#sched-week select[name=Week]');
    slDay.empty();
    var _format = $dateFormatByUser.toUpperCase();
    var _currentDate = new Date();
    var _endDate = moment(_currentDate).add(3, "month").day("Saturday").add(1, "day").toDate();
    var _startDate = moment(_currentDate).add(-3, "month").day("Monday").toDate();
    while (_endDate >= _startDate) {
        var _tempday = moment(_endDate);
        var _day = moment(_endDate).add(-1, "week");
        var _daytemp = moment(_endDate).add(-1, "week").add(1, "day");
        _endDate = _day.toDate();
        var _value = _day.add(1, "day").format(_format) + "-" + _tempday.format(_format);
        var _isSelected = false;
        if (_currentDate >= _day.toDate() && _currentDate <= _tempday.toDate())
            _isSelected = true;
        if (_tempday.month() == _day.month()) {
            slDay.append("<option value=\"" + _value + "\" " + (_isSelected?"selected":"") + ">" + _daytemp.format("Do") + " - " + _tempday.format("Do ") + _daytemp.format("MMMM YYYY") + "</option>");
        } else {
            slDay.append("<option value=\"" + _value + "\" " + (_isSelected ? "selected" : "") + ">" + _daytemp.format("Do MMMM YYYY") + " - " + _tempday.format("Do MMMM YYYY") + "</option>");
        }
    }
    slDay.select2({ placeholder: 'Please select' });
}
function calcMonthFilterByDate_timeschedule() {
    var slDay = $('#sched-month select[name=Month]');
    slDay.empty();
    var _format = $dateFormatByUser.toUpperCase();
    var _currentDate = new Date();
    var _endDate = new Date(_currentDate.getFullYear(), _currentDate.getMonth() + 4, 0);
    var _maxMonth = 1;
    while (_maxMonth <= 6) {
        var _tempday = moment(_endDate);
        var _day = moment(_endDate).subtract(_endDate.getDate(), 'day');
        var _isSelected = false;
        if (_currentDate.getMonth() == _endDate.getMonth())
            _isSelected = true;
        _endDate = _day.toDate();
        slDay.append("<option value=\"" + _day.add(1, "day").format(_format) + "-" + _tempday.add(1, "day").format(_format) + "\" " + (_isSelected ? "selected" : "") + ">" + _day.format("MMMM YYYY") + "</option>");
        _maxMonth++;
    }
    slDay.select2({ placeholder: 'Please select' });
}
function initTablesDailyWeeklyMonthlyOfSched(tab) {
    var _format = $dateFormatByUser.toUpperCase();
    var _peoples = $('#slSchedPeoples').val();
    var _roles = $('#slSchedRoles').val();
    var _localtions = $('#slSchedLocations').val();
    if (!tab)
        tab = $('#tabviewschedule li.active a').attr("href");

    if (tab == "#sched-view-day") {
        var _day = moment($('#sched-day input[name=Day]').val(), _format);
        $('.sched-title-day').text(_day.format("dddd Do MMMM YYYY"));
        //load table
        var $tblschedviewday = $('#tblsched-view-day');
        $tblschedviewday.dataTable({
            destroy: true,
            //serverSide: true,
            paging: true,
            searching: false,
            autoWidth: true,
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchSchedulesDaily",
                "data": function (d) {
                    return $.extend({}, d, {
                        "Day": $('#sched-day input[name=Day]').val(),
                        "sPeoples": _peoples ? JSON.stringify(_peoples) : "",
                        "sRoles": _roles ? JSON.stringify(_roles) : "",
                        "sLocations": _localtions ? JSON.stringify(_localtions) : ""
                    });
                }
            },
            columns: [
                { "title": "Person", "data": "PersonName", "searchable": true, "orderable": true },
                { "title": "Shift", "data": "Shift", "searchable": true, "orderable": true },
                { "title": "Location(s)", "data": "Location", "searchable": true, "orderable": true },
                { "title": "Manage", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
                {
                    "targets": 0,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<ul class="avatar-listing unstyled" style="margin-bottom: 0 !important;"><li>';
                        _htmlOptions += '<a href="/Community/UserProfilePage?uId=' + row.PersonId + '" style="text-decoration: none !important; background: transparent !important; padding: 0;"><div class="owner-avatar">';
                        _htmlOptions += '<div class="avatar-sm" style="background: url(\'' + row.PersonUrl + '\');"></div></div>';
                        _htmlOptions += '<h5>' + data + '<br><small>' + (row.PersonJobtile ? row.PersonJobtile : '') + '</small></h5></a></li></ul>';
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 3,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<button class="btn btn-warning" type="button" onclick="loadScheduleData(' + data + ')" data-toggle="modal" data-target="#app-operator-schedule-individual-edit"><i class="fa fa-pencil"></i></button>';
                        return _htmlOptions;
                    }
                }
            ]
        });

    } else if (tab == "#sched-view-week") {
        var _valWeek = $('#sched-week select[name=Week]').val();
        var arrayweek = _valWeek.split("-");;
        var _startday = moment(arrayweek[0], _format);
        var _endday = moment(arrayweek[1], _format);
        $('.sched-title-week').text(_startday.format("dddd Do MMMM YYYY") + ' - ' + _endday.format("dddd Do MMMM YYYY"));
        //load table
        $.post("/Operator/SearchSchedulesWeekly", {
            "Week": _valWeek,
            "Peoples": _peoples ? _peoples : [],
            "Roles": _roles ? _roles : [],
            "Locations": _localtions ? _localtions : []
        }, function (response) {
            if (response) {
                $('#sched-content-week').html(response);
                $('#tblsched-view-week').DataTable({
                    destroy: true, pageLength: 10, order: [[0, "asc"]], autoWidth: true, pageLength: 10, searching: false,
                });
            }
        });
    } else {
        var _valMonth = $('#sched-month select[name=Month] option:selected').text();
        $('.sched-title-month').text(_valMonth);
        $.post("/Operator/SearchSchedulesMonthly", {
            "Week": $('#sched-month select[name=Month]').val(),
            "Peoples": _peoples ? _peoples : [],
            "Roles": _roles ? _roles : [],
            "Locations": _localtions ? _localtions : []
        }, function (response) {
            if (response) {
                $('#sched-content-month').html(response);
                $('#tblsched-view-month').DataTable({
                    destroy: true, pageLength: 10, order: [[0, "asc"]], autoWidth: true, pageLength: 10, searching: false,
                });
            }
        });
    }
}
function initTablesDailyWeeklyMonthlyOfTimeSheets(tab) {
    var _format = $dateFormatByUser.toUpperCase();
    var _peoples = $('#slTimeshPeoples').val();
    var _roles = $('#slTimeshRoles').val();
    var _localtions = $('#slTimeshLocations').val();
    if (!tab)
        tab = $('#tabviewtimesheets li.active a').attr("href");
    if (tab == "#view-day") {
        var _day = moment($('#day input[name=Day]').val(), _format);
        $('.timesh-title-day').text(_day.format("dddd Do MMMM YYYY"));
        //load table
        var $tblschedviewday = $('#tbltimesh-view-day');
        $tblschedviewday.dataTable({
            destroy: true,
            //serverSide: true,
            paging: true,
            searching: false,
            autoWidth: true,
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchTimesheetsDaily",
                "data": function (d) {
                    return $.extend({}, d, {
                        "Day": $('#day input[name=Day]').val(),
                        "sPeoples": _peoples ? JSON.stringify(_peoples) : "",
                        "sRoles": _roles ? JSON.stringify(_roles) : "",
                        "sLocations": _localtions ? JSON.stringify(_localtions) : ""
                    });
                }
            },
            columns: [
                { "title": "Person", "data": "PersonName", "searchable": true, "orderable": true },
                { "title": "Shift", "data": "Shift", "searchable": true, "orderable": true },
                { "title": "Location(s)", "data": "Location", "searchable": true, "orderable": true }
            ],
            columnDefs: [
                {
                    "targets": 0,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<ul class="avatar-listing unstyled" style="margin-bottom: 0 !important;"><li>';
                        _htmlOptions += '<a href="/Community/UserProfilePage?uId=' + row.PersonId + '" style="text-decoration: none !important; background: transparent !important; padding: 0;"><div class="owner-avatar">';
                        _htmlOptions += '<div class="avatar-sm" style="background: url(\'' + row.PersonUrl + '\');"></div></div>';
                        _htmlOptions += '<h5>' + data + '<br><small>' + (row.PersonJobtile ? row.PersonJobtile : '') + '</small></h5></a></li></ul>';
                        return _htmlOptions;
                    }
                }
            ]
        });
    } else if (tab == "#view-week") {
        var _valWeek = $('#week select[name=Week]').val();
        var arrayweek = _valWeek.split("-");;
        var _startday = moment(arrayweek[0], _format);
        var _endday = moment(arrayweek[1], _format);
        $('.timesh-title-week').text(_startday.format("dddd Do MMMM YYYY") + ' - ' + _endday.format("dddd Do MMMM YYYY"));
        //load table
        $.post("/Operator/SearchTimesheetsWeekly", {
            "Week": _valWeek,
            "Peoples": _peoples ? _peoples : [],
            "Roles": _roles ? _roles : [],
            "Locations": _localtions ? _localtions : []
        }, function (response) {
            if (response) {
                $('#timesh-content-week').html(response);
                $('#tbltimesh-view-week').DataTable({
                    destroy: true, pageLength: 10, order: [[0, "asc"]], autoWidth: true, pageLength: 10, searching: false,
                });
            }
        });
    } else {
        var _valMonth = $('#month select[name=Month] option:selected').text();
        $('.timesh-title-month').text(_valMonth);
        $.post("/Operator/SearchTimesheetsMonthly", {
            "Week": $('#month select[name=Month]').val(),
            "Peoples": _peoples ? _peoples : [],
            "Roles": _roles ? _roles : [],
            "Locations": _localtions ? _localtions : []
        }, function (response) {
            if (response) {
                $('#timesh-content-month').html(response);
                $('#tbltimesh-view-month').DataTable({
                    destroy: true, pageLength: 10, order: [[0, "asc"]], autoWidth: true, pageLength: 10, searching: false,
                });
            }
        });
    }
}
function resetFormSchedule() {
    $('#frmSchedule select[name=Employees]').multiselect("clearSelection");
    $('#frmSchedule select[name=Days]').multiselect("clearSelection");
    $('#frmSchedule input[name=ShiftStart]').val('');
    $('#frmSchedule input[name=ShiftEnd]').val('');
    $('#frmSchedule').validate().resetForm();
}
function checkTeamPermission() {
    $.get("/Operator/CheckIsManagerOrSupervisor", function (response) {
        if (response.allow)
            $('.team-permission').show();
        else
            $('.team-permission').hide();
    })
}
function getTeamMembers() {
    $.get("/Operator/GetTeamMembers", function (response) {
        if (response) {
            //Load filter
            var $selectmembers = $('.teammembers');
            $selectmembers.empty();
            $.each(response, function (key, value) {
                $selectmembers.append("<option value=\"" + value.UserId + "\">" + value.Fullname + "</option>");
            });
            $selectmembers.multiselect("destroy").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
        }
    });
}
function loadScheduleData(id) {
    if (id == 0) {
        $('#frmScheduleEdit input[name=Id]').val('0');
        $('#frmScheduleEdit input[name=ShiftStart]').val('');
        $('#frmScheduleEdit input[name=ShiftEnd]').val('');
    } else {
        $.get("/Operator/GetScheduleById?id=" + id, function (response) {
            if (response) {
                $('#frmScheduleEdit input[name=Id]').val(response.Id);
                $('#frmScheduleEdit input[name=ShiftStart]').val(response.ShiftStart);
                $('#frmScheduleEdit input[name=ShiftEnd]').val(response.ShiftEnd);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
            }
        });
    }
}
function getWorkgroupsTypeMember(formId) {
    $.get("/Operator/GetWorkgroups?type=1", function (response) {
        if (response) {
            //Load filter
            var $selectwgs = $(formId+' select[name=Workgroup]');
            $selectwgs.empty();
            $selectwgs.append('<option value="" selected=""></option>');
            $.each(response, function (key, value) {
                $selectwgs.append("<option value=\"" + value.Id + "\">" + value.Name + "</option>");
            });
            $selectwgs.select2({ placeholder: 'Please select' });
        }
    });
}
function LoadPerformanceTrackings(skip, isLoad) {
    if (isLoad)
        isInitpagination = false;
    var isHide = $('#chkPerformanceIsHidden').prop('checked') ? true : false;
    $.LoadingOverlay("show");
    var _param = {
        skip: skip,
        take: perfomanceTrackingPageSize,
        keyword: $('#txtPerfomanceSearch').val(),
        isLoadingHide: isHide,
        locationId: $('#slLocation').val()
    };
    $.get("/Operator/LoadPerformanceTrackings", _param, function (data) {
        if (data.result) {
            if (data.Object) {
                $('#people-performance div.flex-grid-quarters-lg').html(data.Object.strResult);
                if (!isInitpagination)
                    initPagination(data.Object.totalRecord, perfomanceTrackingPageSize, '#PerformancePaginateTemplate');
                if (data.Object.totalRecord == 0)
                    $('#PerformancePaginateTemplate').hide();
            }
        }
        LoadingOverlayEnd();
    });
}
function formatOptions(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('avataruri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('avataruri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function formatSelected(state) {
    var $state = $(
        '<div class="select2imgwrap" style="padding: 0px; "><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('avataruri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('avataruri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function ChangeWorkgroup(el) {
    $('.preview-workgroup').show();
    $.get("/Operator/GetWorkgroupInfor", { id: $(el).val() }, function (response) {
        $('#WGLocation').text(response.Location);
        $('#WGProcess').text(response.Process);
        $('#WGMember').text(response.Members);
        $('#WGQbicle').text(response.Qbicle);
        var html = '<option value=""></option>';
        if (response.Persons) {
            for (var i = 0; i < response.Persons.length; i++) {
                html += '<option avataruri="' + response.Persons[i].ProfilePic + '&size=T" api="' + $('#api-uri').val() + '" value="' + response.Persons[i].Id + '">' + response.Persons[i].Name + '</option>';
            }
        }
        $('#trackingPersons').html(html);
        $('#trackingPersons').select2({
            placeholder: 'Please select',
            templateResult: formatOptions,
            templateSelection: formatSelected
        });
    });
}

function AddMeasure() {
    if ($('#slMeasureToAdd').val() && $('#txtWeightPercent').val()) {
        var el = $('#slMeasureToAdd option[value=' + $('#slMeasureToAdd').val() + ']').first();
        var id = $('#slMeasureToAdd').val();
        var name = el.text();
        var description = el.data('description');
        var weight = parseInt($('#txtWeightPercent').val());
        var totalWeight = weight;
        if (weight && weight != 0) {
            $('#tblTrackingMeasures tbody tr').each(function () {
                totalWeight += parseInt($(this).data('weight'));
            });
            if (totalWeight <= 100) {
                $('#tblTrackingMeasures tbody').append("<tr data-id=\"" + id + "\" data-name=\"" + name + "\" data-description=\"" + description + "\" data-weight=\"" + weight + "\"><td>" + name + "</td><td>" + description + "</td><td>" + weight + "%</td><td><button class=\"btn btn-danger\" onclick=\"RemoveMeasure(this)\"><i class=\"fa fa-trash\"></i></button></td></tr>");
                el.remove();
                $('#slMeasureToAdd').val('');
                $('#slMeasureToAdd').trigger('change.select2');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_703"), "Operator");
            }
        }
    }
}

function RemoveMeasure(el) {
    var el1 = $(el).parent().parent();
    $('#slMeasureToAdd').append('<option value="' + el1.data('id') + '" data-description="' + el1.data('description') + '">' + el1.data('name') + '</option>');
    $('#slMeasureToAdd').val('');
    $('#slMeasureToAdd').trigger('change.select2');
    el1.remove();
}

function LoadPerformanceTrackingModal(id) {
    $.LoadingOverlay("show");
    $("#app-operator-performance-addedit").load("/Operator/LoadPerformanceTrackingModal", { id: id }, function () {
        var $frmOperatorPerformance = $('#frmOperatorPerformance');
        $('#performanceWorkgroup, #slMeasureToAdd').select2({ placeholder: 'Please select' });
        $('#trackingPersons').select2({
            placeholder: 'Please select',
            templateResult: formatOptions,
            templateSelection: formatSelected
        });
        $frmOperatorPerformance.validate({
            ignore: "",
            rules: {
                TeamPersonId: {
                    required: true,
                },
                WorkgroupId: {
                    required: true
                }
            }
        });
        $frmOperatorPerformance.submit(function (e) {
            e.preventDefault();
            if ($frmOperatorPerformance.valid()) {
                $.LoadingOverlay("show");
                var lstMeasureWithWeights = getAllMeasureWithWeights();
                var totalWeight = lstMeasureWithWeights.reduce(function (total, item) {
                    total += item.Weight;
                    return total;
                }, 0);
                if (lstMeasureWithWeights.length > 0) {
                    var formData = new FormData($frmOperatorPerformance[0]);
                    formData.append("TrackingMeasures", JSON.stringify(lstMeasureWithWeights));
                    $.ajax({
                        type: 'POST',
                        cache: false,
                        url: '/Operator/SavePerformanceTracking',
                        enctype: 'multipart/form-data',
                        data: formData,
                        processData: false,
                        contentType: false,
                        beforeSend: function (xhr) {
                            isBusy = true;
                        },
                        success: function (data) {
                            isBusy = false;
                            if (data.result) {
                                $('#app-operator-performance-addedit').modal('hide');
                                LoadPerformanceTrackings(0, true);
                                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                            } else if (!data.result && data.msg) {
                                cleanBookNotification.error(_L(data.msg), "Operator");
                            }
                            LoadingOverlayEnd();
                        },
                        error: function (data) {
                            isBusy = false;
                            LoadingOverlayEnd();
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                        }
                    });
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_806"), "Operator");
                    LoadingOverlayEnd();
                }

            } else {
                $('li a[href=#perf-1]').trigger('click');
            }
            LoadingOverlayEnd();
        });
        $('.btnNext').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('.admintabs .active').next('li').find('a').trigger('click');
        });

        $('.btnPrevious').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('.admintabs .active').prev('li').find('a').trigger('click');
        });

        LoadingOverlayEnd();
    });
}

function ShowGroupMember() {
    $('#team-person-preview').load("/Operator/ShowListMemberForWorkGroup?performanceId=" + $("#performanceId").val() + "&wgId=" + $("#performanceWorkgroup").val());
}

function getAllMeasureWithWeights() {
    var lst = [];
    $("#tblTrackingMeasures tbody tr").each(function () {
        var measureId = $(this).data("id");
        var weight = $(this).data("weight");
        lst.push({ MeasureId: measureId, Weight: weight });
    });
    return lst;
}

function loadModalForm(id) {
    $('#app-operator-form-add').modal("show");
    $("#app-operator-form-add").load("/Operator/LoadModalForm", { id: id }, function () {
        initFormModal();
    });
}
function initFormModal() {
    var $frmForm = $('#frmForm');
    $("#sortable").sortable();
    $("#frmForm .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#frmForm input[data-toggle="toggle"]').bootstrapToggle();
    $('#frmForm .select2').select2({ placeholder: 'Please select' });
    $('#frmForm button.submit-status').click(function () {
        if ($(this).val() == 'true')
            $('#frmForm input[name=IsDraft]').prop("checked", true);
        else
            $('#frmForm input[name=IsDraft]').prop("checked", false);
        $frmForm.trigger("submit");
    });
    $frmForm.validate({
        ignore: "",
        rules: {
            Title: {
                required: true,
                maxlength: 150
            },
            Description: {
                required: true,
                maxlength: 300
            },
            EstimatedTime: {
                required: true,
            },
            Tags: {
                required: true,
            }
        }
    });
    $frmForm.submit(function (e) {
        e.preventDefault();
        if ($frmForm.valid()) {
            $.LoadingOverlay("show");
            var _formElements = getDataFormElement();
            var _errors = false;
            $.each(_formElements, function (index, item) {
                if (!item.Label || (!item.AllowPhotos && !item.AllowDocs && !item.AllowNotes && !item.AllowScore)) {
                    _errors = true;
                    return;
                }
            });
            if (_errors) {
                cleanBookNotification.error(_L("ERROR_MSG_710"), "Operator");
                LoadingOverlayEnd();
                return;
            }
            var _formModel = {
                Id: $('#frmForm input[name=Id]').val(),
                Title: $('#frmForm input[name=Title]').val(),
                Description: $('#frmForm textarea[name=Description]').val(),
                EstimatedTime: $('#frmForm input[name=EstimatedTime]').val(),
                IsDraft: $('#frmForm input[name=IsDraft]').prop("checked"),
                Tags: $('#frmForm select[name=Tags]').val(),
                FormElements: _formElements
            };

            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: { formModel: _formModel },
                dataType: 'json',
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-form-add').modal('hide');
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        initFormsTable();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            $('#tabForm li a[href="#form-0"]').trigger("click");
        }
    });
    $("#frmForm .fieldtypes li a").bind('click', function (e) {
        e.preventDefault();
        var i = 0;
        var type = $(this).data("type");

        var output = '<div id="{UNIQUEID}" class="ui-sortable-handle"><div class="row preview-area" onclick="$(this).next(\'.row-options\').slideToggle();">';
        output += '<div class="col-xs-12 col-sm-9"><h5><span>{FORM_TYPE}<input type="hidden" name="Type" value="{TYPE}" /></span> &nbsp; <i class="{UNIQUEID}_title">Click to configure</i></h5></div>';
        output += '<div class="col-xs-12 col-sm-3"><a href="#" class="remove" onclick="$(\'#{UNIQUEID}\').remove();"><i class="fa fa-remove"></i></a></div></div>';
        output += '<div class="row-options"><div class="row"><div class="col-xs-12">';
        output += '<textarea name="Label" class="form-control" onkeyup="$(\'.{UNIQUEID}_title\').html($(this).val());" placeholder="Enter a label or question"></textarea>';
        output += '</div></div><br /><div class="row"><div class="col-xs-12 col-sm-3"><div class="form-group checkbox toggle"><label style="font-weight: 500 !important; padding: 0;">Allow photo</label><br />';
        output += '<input name="AllowPhotos" data-toggle="toggle" data-onstyle="success" type="checkbox"></div></div><div class="col-xs-12 col-sm-3">';
        output += '<div class="form-group checkbox toggle"><label style="font-weight: 500 !important; padding: 0;">Allow document</label><br />';
        output += '<input name="AllowDocs" data-toggle="toggle" data-onstyle="success" type="checkbox"></div></div>';
        output += '<div class="col-xs-12 col-sm-3"><div class="form-group checkbox toggle"><label style="font-weight: 500 !important; padding: 0;">Allow score</label><br />';
        output += '<input name="AllowScore" onchange="getMeasuresFormElement(\'{UNIQUEID}\')" data-toggle="toggle" data-onstyle="success" type="checkbox" onchange="$(\'#{UNIQUEID} .scoring\').toggle();"></div></div>';
        output += '<div class="col-xs-12 col-sm-3"><div class="form-group checkbox toggle"><label style="font-weight: 500 !important; padding: 0;">Allow notes</label><br />';
        output += '<input name="AllowNotes" data-toggle="toggle" data-onstyle="success" type="checkbox" checked></div></div></div>';
        output += '<div class="scoring well custom" style="display: none; margin-top: 15px; margin-bottom: 0;"><label>Which Measure should be used?</label>';
        output += '<select name="AssociatedMeasureId" class="form-control select2" style="width: 100%;"></select></div></div></div>';
        var _uniqueId = UniqueId();
        output = output.replace(/{UNIQUEID}/g, _uniqueId);
        switch (type) {
            case "text":
                output = output.replace(/{FORM_TYPE}/g, 'Free text').replace('{TYPE}', 4);
                break;
            case "boolean":
                output = output.replace(/{FORM_TYPE}/g, 'True or false').replace('{TYPE}', 1);
                break;
            case "number":
                output = output.replace(/{FORM_TYPE}/g, 'Number').replace('{TYPE}', 2);
                break;
            case "date":
                output = output.replace(/{FORM_TYPE}/g, 'Date').replace('{TYPE}', 3);
                break;
        }
        $('#sortable').append(output);
        $('#frmForm input[data-toggle="toggle"]').bootstrapToggle();
    });
    $('#frmForm .btnNext').click(function () {
        var $parent = $('#app-operator-form-add');
        if ($frmForm.valid()) {
            $parent.find('#tabForm .active').next('li').find('a').trigger('click');
        } else {
            $parent.find('#tabForm .active').find('a').trigger('click');
        }
    });
    $('#frmForm .btnPrevious').click(function () {
        var $parent = $('#app-operator-form-add');
        $parent.find('#tabForm .active').prev('li').find('a').trigger('click');
    });
}
function getDataFormElement() {
    var rows = $('#sortable div.ui-sortable-handle');
    var formelements = [];
    for (var i = 0; i < rows.length; i++) {
        var _feid = $(rows[i]).find("input[name=FEId]");
        var _type = $(rows[i]).find("input[name=Type]");
        var _label = $(rows[i]).find("textarea[name=Label]");
        var _allowPhotos = $(rows[i]).find("input[name=AllowPhotos]");
        var _AllowDocs = $(rows[i]).find("input[name=AllowDocs]");
        var _allowNotes = $(rows[i]).find("input[name=AllowNotes]");
        var _allowScore = $(rows[i]).find("input[name=AllowScore]");
        var _associatedMeasureId = $(rows[i]).find("select[name=AssociatedMeasureId]");
        var frel = {
            Id: _feid ? $(_feid).val() : 0,
            DisplayOrder: i,
            Type: $(_type).val(),
            Label: $(_label).val(),
            AllowPhotos: $(_allowPhotos).prop("checked"),
            AllowDocs: $(_AllowDocs).prop("checked"),
            AllowNotes: $(_allowNotes).prop("checked"),
            AllowScore: $(_allowScore).prop("checked"),
            AssociatedMeasureId: $(_associatedMeasureId).val(),
        };
        formelements.push(frel);
    }
    return formelements;
}
function loadOperatorFormTags() {
    $.get("/Operator/getTagsAll", function (data) {
        if (data.length > 0) {
            $('#slFormsTags').empty();
            $('#slFormsTags').append("<option value=\"0\" selected>Show all</option>");
            data.forEach(function (item) {
                $('#slFormsTags').append("<option value=\"" + item.Id + "\">" + item.Name + "</option>");
            });
            $('#slFormsTags').select2({ placeholder: 'Please select' });
        }
    });
}
function initFormsTable() {
    var $tblForms = $('#tblForms');
    $tblForms.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        ajax: {
            "url": "/Operator/SearchForms",
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $('#txtFormsSearch').val(),
                    "tag": $('#slFormsTags').val() ? $('#slFormsTags').val() : 0,
                });
            }
        },
        columns: [
            { "title": "Title", "data": "Title", "searchable": true, "orderable": true },
            { "title": "Description", "data": "Description", "searchable": true, "orderable": true },
            { "title": "EstimatedTime", "data": "EstimatedTime", "searchable": true, "orderable": true },
            { "title": "Status", "data": "IsDraft", "searchable": false, "orderable": false },
            { "title": "Tags", "data": "Tags", "searchable": false, "orderable": false },
            { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<button class="btn btn-warning" onclick="loadModalForm(' + data + ')"><i class="fa fa-pencil"></i></button>&nbsp;';
                    _htmlOptions += '<button class="btn btn-danger" onclick="removeForm(' + data + ')"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function removeForm(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Operator",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Operator/RemoveForm", { id: id }, function (data) {
                    if (data.result) {
                        cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
                        initFormsTable();
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"));
                    }
                });
                return;
            }
        }
    });
}
function loadModalTask(id) {
    $('#app-operator-compliance-task-add').modal("show");
    $("#app-operator-compliance-task-add").load("/Operator/LoadModalTask", { id: id }, function () {
        initTaskModal();
    });
}
function initTaskModal() {
    var $frmTaskOperator = $('#frmTaskOperator');
    $('#frmTaskOperator .select2').select2({ placeholder: 'Please select' });
    $('#frmTaskOperator input[data-toggle="toggle"]').bootstrapToggle();
    $frmTaskOperator.validate({
        ignore: "",
        rules: {
            WorkgroupId: {
                required: true,
            },
            TaskName: {
                required: true,
                maxlength: 150
            },
            TaskDescription: {
                required: true,
                maxlength: 300
            },
            Forms: {
                required: true,
            },
            ExpectedEnd:{
                required: true,
            },
            Assignee: {
                required: true,
            },
            TaskType: {
                required: true,
            }
        }
    });
    $frmTaskOperator.submit(function (e) {
        e.preventDefault();
        //$frmForm.data("validator").settings.ignore = $('#recurring').prop('checked') ? "" : ":hidden";
        if ($frmTaskOperator.valid()) {
            $.LoadingOverlay("show");
            var files = document.getElementById("task-operator-file-input").files;
            if (files.length > 0) {
                UploadMediaS3ClientSide("task-operator-file-input").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#task-operator-file-object-key").val(mediaS3Object.objectKey);
                        $("#task-operator-file-object-name").val(mediaS3Object.fileName);
                        $("#task-operator-file-object-size").val(mediaS3Object.fileSize);

                        SaveTaskOperator();
                    }
                });
            }
            else {
                $("#task-operator-file-object-key").val("");
                $("#task-operator-file-object-name").val("");
                $("#task-operator-file-object-size").val("");

                SaveTaskOperator();
            }


        } 
    });
    $('.singledateandtime').daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        minDate: moment(),
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
        }
    });
    $('#frmTaskOperator .btnNext').click(function () {
        var $parent = $('#app-operator-compliance-task-add');
        if ($frmTaskOperator.valid()) {
            $parent.find('#tab-modal-task .active').next('li').find('a').trigger('click');
        } else {
            $parent.find('#tab-modal-task .active').find('a').trigger('click');
        }
    });
    $('#frmTaskOperator .btnPrevious').click(function () {
        var $parent = $('#app-operator-compliance-task-add');
        $parent.find('#tab-modal-task .active').prev('li').find('a').trigger('click');
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
}

function SaveTaskOperator() {

    var dayOrmonth = "", dayofweek = "", lstDate = [];
    var type = $("#TaskRecurtype").val();

    if (type == "0") {
        dayOrmonth = "";
        $(".daily-day").each(function () {
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
        $(".weekly-day").each(function () {
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
    $("#hdTaskDayOrMonth").val(dayOrmonth);
    //remove disabled
    var isDisabled = $('#frmTaskOperator select[name=WorkgroupId]').prop("disabled");
    if (isDisabled)
        $('#frmTaskOperator select[name=WorkgroupId]').prop("disabled", false);
    //end remove
    var frmData = new FormData($("#frmTaskOperator")[0]);
    var lstDateIsValid = false;
    var isRecur = $("#create-task-recurrence").find('input[name=isRecurs]').is(":checked");
    if (isRecur) {
        $("#lstDate tbody tr").find("input[type='checkbox']").each(function () {
            if ($(this).is(":checked")) {
                lstDateIsValid = true;
                frmData.append("ListDates[]", $(this).attr("att-date"));
            }
        });
    }
    if (isRecur && !lstDateIsValid) {
        if (isDisabled)
            $('#frmTaskOperator select[name=WorkgroupId]').prop("disabled", true);
        LoadingOverlayEnd();
        cleanBookNotification.error(_L("ERROR_MSG_110"), "Task");
        return;
    }
    if (isDisabled)
        $('#frmTaskOperator select[name=WorkgroupId]').prop("disabled", true);
    $.ajax({
        type: "POST",
        cache: false,
        url: "/Operator/SaveTaskOperator",
        data: frmData,
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                $('#app-operator-compliance-task-add').modal('hide');
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                initComplianceTasksTable();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Operator");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}


function taskRecursCheck(obj) {
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
function loadWorkgroupPreviewTask(wgid) {
    $('#frmTaskOperator select[name="WorkgroupId"]').valid();
    $('.preview-task-localtion').text('');
    $('.preview-task-qbicle').text('');
    $('.preview-task-members').text('0');
    if (wgid) {
        $.get("/Operator/LoadWorkgroupTaskPreview?wgid=" + wgid, function (response) {
            if (response) {
                $('.preview-task-localtion').text(response.localtion);
                $('.preview-task-qbicle').text(response.qbicle);
                $('.preview-task-members').text(response.countmember);
                $('.preview-task-members').parent().attr("onclick", "ShowTaskMembers(" + wgid + ")");
                var slPeoples = $('#frmTaskOperator select[name=Assignee]');
                slPeoples.removeAttr('disabled');
                slPeoples.empty();
                response.members.forEach(function (item) {
                    slPeoples.append("<option avatarUrl=\"" + item.AvatarUrl + "\" value=\"" + item.UserId + "\">" + item.Fullname + "</option>");
                });
                slPeoples.select2({
                    placeholder: 'Please select',
                    templateResult: slformatOptions,
                    templateSelection: slformatSelected
                });
            }
        });
    } else {
        var slPeoples = $('#frmTaskOperator select[name=Assignee]');
        slPeoples.empty();
        slPeoples.attr("disabled", true);
        slPeoples.select2({
            placeholder: 'Please select'
        });

    }
}
function slformatOptions(state) {
    if (!state.id) { return state.text; }
    var urlAvatar = state.element.attributes["avatarUrl"].value;
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function slformatSelected(state) {
    if (!state.id) { return state.text; }
    var urlAvatar = state.element.attributes["avatarUrl"].value;
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function getMeasuresFormElement(elId) {
    $.get("/Operator/GetMeasures", function (response) {
        if (response) {
            var slMeasures = $('#' + elId + ' select[name=AssociatedMeasureId]');
            slMeasures.empty();
            response.forEach(function (item) {
                slMeasures.append("<option value=\"" + item.Id + "\">" + item.Name + "</option>");
            });
            slMeasures.select2({
                placeholder: 'Please select'
            });
        }
    });
    $('#' + elId + ' .scoring').toggle();
}
function showRecur(type) {
    if (type != "") {
        //var elmentStart = $('input[name=RecurStart]').val();
        //var elmentEnd = $('input[name=RecurEnd]').val($('input[name=ExpectedEnd]').val());
        $('input[name=RecurEnd]').val($('input[name=ExpectedEnd]').val());
        $('input[name=RecurStartView]').val($('input[name=RecurStartView]').val());
        var route = '';
        $('#TaskRecur').show();
        $('.task-recurtype').hide();
        
        if (type == "0") {//Daily
            route = ".task-recur-daily";
        }
        else if (type == "1") {//Weekly
            route = ".task-recur-weekly";          
        }
        else if (type == "2") {//Monthly
            route = ".task-recur-monthly";
        }
        $(route).show();
    }
    else {
        $('#TaskRecur').hide();
    }
    $("#task-exclusion-list").css({ "display": "none" });
    genarateDateTable();
}
function updateRecur() {
    var duration = $('#taskDuration').val();
    var mode = $('#taskDurationUnit :selected').text().toLowerCase().trim();
    var $TaskRecurtype = $('#TaskRecurtype');
    $TaskRecurtype.empty();
    $TaskRecurtype.append('<option value="">Please select</option>');
    
    if (duration != "" && parseInt(duration) > 24 && mode == 'hours') {
        $('#task-indicated-duration').text(duration + " " + mode);
        $('#task-restricted').show();
        $TaskRecurtype.append('<option value="1">Weekly</option>');
        $TaskRecurtype.append('<option value="2">Monthly</option>');
    } else if (duration != "" && parseInt(duration) > 1 && mode == 'days') {
        $('#task-indicated-duration').text(duration + " " + mode);
        $('#task-restricted').show();
        $TaskRecurtype.append('<option value="1">Weekly</option>');
        $TaskRecurtype.append('<option value="2">Monthly</option>');
    } else if (duration != "" && parseInt(duration) > 7 && mode == 'days') {
        $('#task-indicated-duration').text(duration + " " + mode);
        $('#task-restricted').show();
        $TaskRecurtype.append('<option value="1">Weekly</option>');
    } else if (duration != "" && parseInt(duration) > 1 && mode == 'weeks') {
        $('#task-indicated-duration').text(duration + " " + mode);
        $('#task-restricted').show();
        $TaskRecurtype.append('<option value="2">Monthly</option>');
    } else
    {
        $TaskRecurtype.append('<option value="0">Daily</option>');
        $TaskRecurtype.append('<option value="1">Weekly</option>');
        $TaskRecurtype.append('<option value="2">Monthly</option>');
    }
    $('#TaskRecurtype').select2({ placeholder: 'Please select' });
    $("input[name='duration']").val(duration + " " + mode);
}
function genarateDateTable(isDisplayGrid) {
    var type = $("#TaskRecurtype").val();
    var pattern = $("#pattern :selected").val();
    var LastOccurenceTaskDate = $("input[name='RecurEnd']").val();
    var FirstOccurenceDate = $("input[name='RecurStart']").val();
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
        $(".daily-day").each(function () {
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        $(".weekly-day").each(function () {
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
    getListDate(dayofweek, type, pattern, customDate, LastOccurenceTaskDate, FirstOccurenceDate).done(function (data) {
        if (data != '') {
            var str = "";
            $("#lstDate tbody tr").remove();
            var strShortDate = "";
            for (var i = 0; i < data.length; i++) {
                str += '<tr>';
                str += '<td>' + data[i].Name + '</td>';
                str += '<td><input type="checkbox" ' + (data[i].selected ? "checked" : "") + ' value="' + data[i].Value + '"  att-date="' + data[i].Date + '"  ></td>';
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
function getListDate(dayofweek, type, Pattern, customDate, LastOccurenceDate, FirstOccurenceDate) {
    return $.ajax({
        url: "/Operator/GetListDate",
        type: "POST",
        dataType: "json",
        data: { dayofweek: dayofweek, type: type, Pattern: Pattern, customDate: customDate, LastOccurenceDate: LastOccurenceDate, FirstOccurenceDate: FirstOccurenceDate }
    });
}
function changePattern() {
    if ($("#pattern :selected").val() == "2") {
        $('#customDate').show();
        $("#hdTaskcustomDate").val($("#month-dates :selected").val());
    }
    else {
        $('#customDate').hide();
    }
    genarateDateTable();
}
function changeTaskCheckItem(obj) {
    if ($(obj).is(":checked"))
        $(obj).attr("att-checked", 1);
    else
        $(obj).attr("att-checked", 0);
}
function initComplianceTasksTable() {
    var tblComplianceTasks = $('#tblComplianceTasks');
    tblComplianceTasks.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        ajax: {
            "url": "/Operator/SearchTasks",
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $('#txtTaskKeyword').val(),
                    "assignee": $('#slTaskAssignee').val(),
                    "form": $('#slTaskForms').val() ? $('#slTaskForms').val():0
                });
            }
        },
        columns: [
            { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
            { "title": "Assignee", "data": "Assignee", "searchable": true, "orderable": true },
            { "title": "Forms", "data": "Forms", "searchable": false, "orderable": false },
            { "title": "Type", "data": "Type", "searchable": true, "orderable": true },
            { "title": "Due", "data": "Due", "searchable": true, "orderable": true },
            { "title": "Options", "data": "Id", "searchable": false, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 1,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    return '<a href="/Community/UserProfilePage?uId=' + row.AssigneeId + '" target="_blank">' + data + '</a>';
                }
            },
            {
                "targets": 5,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<div class="btn-group options"><button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> Options</button>';
                    _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                    if (row.Type == "Repeatable") {
                        _htmlOptions += '<li><a href="/Operator/ComplianceTask?id=' + data + '">View in Qbicle</a></li>';
                        _htmlOptions += '<li><a href="/Operator/ComplianceTaskInstances?id=' + data + '">View submissions</a></li>';
                    }
                    _htmlOptions += '<li><a href="#" onclick="loadModalTask(' + data+')">Edit</a></li>';
                    _htmlOptions += '<li><a href="#" onclick="deleteComplianceTask(' + data+')">Delete</a></li></ul></div>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function loadOperatorForms() {
    $.get("/Operator/GetFormsAll", function (data) {
        if (data.length > 0) {
            $('#slTaskForms').empty();
            $('#slTaskForms').append("<option value=\"0\" selected>Show all</option>");
            data.forEach(function (item) {
                $('#slTaskForms').append("<option value=\"" + item.Id + "\">" + item.Title + "</option>");
            });
            $('#slTaskForms').select2({ placeholder: 'Please select' });
            $('#task-help-text').hide();

        } else {
            $('#task-help-text').show();
        }
    });
}
function deleteComplianceTask(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Operator",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.LoadingOverlay("show");
                $.post("/Operator/DeleteTask", { id: id }, function (response) {
                    if (response.result) {
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                        $('#tblComplianceTasks').DataTable().ajax.reload();
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Operator");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                    }
                    LoadingOverlayEnd();
                });
                return;
            }
        }
    });
}
function ShowTeamMembers(wgid) {
    $('#app-operator-members').empty();
    $('#app-operator-members').modal("show");
    $('#app-operator-members').load("/Operator/LoadWorkgroupTeamMembers?wgid=" + wgid, function () {
        $('#tblWorkgroupMembers').DataTable({
            responsive: true,
            "lengthChange": true,
            "pageLength": 10,
            "columnDefs": [{
                "targets": 2,
                "orderable": false
            }],
            "order": []
        });
    });
};
function ShowTaskMembers(wgid) {
    $('#app-operator-members').empty();
    $('#app-operator-members').modal("show");
    $('#app-operator-members').load("/Operator/LoadWorkgroupTaskMembers?wgid=" + wgid, function () {
        $('#tblWorkgroupMembers').DataTable({
            responsive: true,
            "lengthChange": true,
            "pageLength": 10,
            "columnDefs": [{
                "targets": 2,
                "orderable": false
            }],
            "order": []
        });
    });
};