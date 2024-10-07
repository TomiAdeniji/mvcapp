var isBusy = false;
var $tblLeadingIndicators;
var $tblGoalMeasures;
$(document).ready(function () {
   
    initModalMedia();
    createDiscussion();
    initDateRange();
    LoadGoalMeasures();
    $('#slTimeframe').change(searchThrottle(function () {
        $('.text-timeframe').text($("#slTimeframe option:selected").text());
        if ($('#tabNavGoal li.active a[data-target=#goal-overview]').length>0)
            LoadGoalMeasures();
        else
            loadTabMeasuresContent();
    }));
    $('#txtCustomDate').change(searchThrottle(function () {
        LoadGoalMeasures();
    }));
    $('#txtSearchMeasures').change(searchThrottle(function () {
        loadTabMeasuresContent();
    }));
});
function initDateRange() {
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('#txtCustomDate').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('#txtCustomDate').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $(this).trigger("change");
        
    });
}
function LoadMedias(qid) {
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/Operator/LoadMedias',
        datatype: 'json',
        data: { qid: qid, fileType: fileType == "All" ? "" : fileType },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#goal-resources .flex-grid-thirds-lg');
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
function initModalMedia()
{
   var $form_media_addedit = $("#form_media_smresource");
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
            var files = document.getElementById("goal-resource-file-input").files;
            if (files.length > 0) {
                UploadMediaS3ClientSide("goal-resource-file-input").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#goal-resource-file-object-key").val(mediaS3Object.objectKey);
                        $("#goal-resource-file-object-name").val(mediaS3Object.fileName);
                        $("#goal-resource-file-object-size").val(mediaS3Object.fileSize);

                        var frmData = new FormData($form_media_addedit[0]);
                        $.ajax({
                            type: "POST",
                            cache: false,
                            url: "/Operator/SaveResourceGoal",
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
                                    LoadMedias($('#qbicleId').val());
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
            }
            else {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_154"), "Qbicles");
            }

        }
    });
}
function animatestat(value) {
    $('#goalChartProcess').circleProgress({
        max: 100,
        value: value,
        textFormat: 'percent',
        animation: 'easeInOutExpo',
        animationDuration: 2600
    });
    animatedprogress();
}
function animatedprogress() {

    var delay = 500;
    $(".progress-bar").each(function (i) {
        $(this).delay(delay * i).animate({
            width: $(this).attr('aria-valuenow') + '%'
        }, delay);
    });

}
function initChart(data) {
    // Padding fix for chart examples
    Chart.plugins.register({
        id: 'paddingBelowLegends',
        beforeInit: function (chart, options) {
            chart.legend.afterFit = function () {
                this.height = this.height + 50;
            };
        }
    });

    // Sample bar chart for MyDesk
    var ctx = document.getElementById('myChart').getContext('2d');

    var gradblue = ctx.createLinearGradient(0, 0, 0, 400);
    var gradaqua = ctx.createLinearGradient(0, 0, 0, 400);

    // Aqua
    gradaqua.addColorStop(0, "#05e4b1");
    gradaqua.addColorStop(1, "#1dc7e6");
    // Blue
    gradblue.addColorStop(0, "#05a8f4");
    gradblue.addColorStop(1, "#049ef4");

    var _labels = [];
    var _data = [];
    var _bgColor = [];
    $.each(data, function (index, item) {
        if(item.key!="01/01")
        {
            _labels.push(item.key);
            _data.push(item.totalweight);
            if (data.length == (index + 1))
                _bgColor.push(gradaqua);
            else
                _bgColor.push(gradblue);
        }
    });
    
    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: _labels,
            datasets: [{
                label: '% progress towards Goal',
                data: _data,
                backgroundColor: _bgColor,
                borderWidth: 0
            }]
        },
        options: {
            legend: {
                display: false
            },
            scales: {
                xAxes: [{
                    gridLines: {
                        color: 'rgba(0, 0, 0, 0)',
                    }
                }],
                yAxes: [{
                    display: true,
                    ticks: {
                        beginAtZero: true,
                        max: 100
                    }
                }]
            }
        }
    });
}
function createDiscussion() {

    $('#frm-create-discussion').submit(function (e) {
        e.preventDefault();
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Discussions/SaveDiscussionForGoal',
            datatype: 'json',
            data: { goalId: $('#goalId').val(), openingmessage: $('#ds_openingmessage').val(), isexpiry: $('#ds_isexpiry').prop('checked'), expirydate: $('#ds_expirydate').val() },
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
function LoadGoalMeasures() {
    $('.text-timeframe').text($("#slTimeframe option:selected").text());
    //Validate Date 
    var isDay=true;
    if($('#slTimeframe').val()=="2")
    {
        $('.text-timeframe').text($('#txtCustomDate').val());
        if (!$('#txtCustomDate').val())
            return;
        var startDaterange = $('#txtCustomDate').data('daterangepicker').startDate.toDate();
        var endDaterange = $('#txtCustomDate').data('daterangepicker').endDate.toDate();
        var max12month = new moment(startDaterange).add(12, 'months').toDate();
        if (endDaterange > max12month)
        {
            cleanBookNotification.error(_L("ERROR_MSG_706"), "Operator");
            return;
        }
        var max31day= new moment(startDaterange).add(31, 'days').toDate()
        if (endDaterange > max31day)
        {
            isDay = false;
        }
    }
    
    //end validate
    var parmaters = {
        goalId:$('#goalId').val(),
        timeframe: $('#slTimeframe').val(),
        customDate: $("#txtCustomDate").val(),
        isDay: isDay
    }
    $.ajax({
        type: 'post',
        url: '/Operator/LoadGoalMeasuresForChart',
        datatype: 'json',
        data: parmaters,
        success: function (response) {
            if (response) {
                animatestat(response.TotalProgressGoal);
                if (response.ChartProcessMeasures)
                {
                    $('#pagination-container').pagination({
                        dataSource: response.ChartProcessMeasures,
                        prevText: '<',
                        nextText: '>',
                        pageSize: 5,
                        callback: function (data, pagination) {
                            var html = measuresTemplating(data);
                            $('.records-h').html(html);
                        }
                    })
                }
                initChart(response.CharMonitorMeasures);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
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

                        SaveOperatorGoal();
                    }
                });
            }
            else {
                $("#operator-goal-feature-image-object-key").val("");
                $("#operator-goal-feature-image-object-name").val("");
                $("#operator-goal-feature-image-object-size").val("");

                SaveOperatorGoal();
            }
        } 
    });

}


function SaveOperatorGoal() {
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
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                location.reload();
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
function loadModalGoal(id) {
    $('#app-operator-goal-addedit').modal("show");
    $("#app-operator-goal-addedit").load("/Operator/LoadModalGoal", { id: id }, function () {
        initGoalModal();
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
function measuresTemplating(data) {
    var html ='';
    $.each(data, function (index, item) {
        var classColor = 'started';
        if (item.Score == 3) {
            classColor = 'pending';
        } else if (item.Score <= 2)
        {
            classColor = 'overdue';
        }

        html += '<article class="' + classColor + '"><div class="record">';
        html += '<div class="col titleblock"><a href="#"> <p class="tasktitle" style="padding-left: 15px;">' + item.MeasureName + '</p></a></div>';
        html += '<div class="col text-right">';
        for (var i = 0; i < item.Score; i++) {
            html += '<i class="fa fa-star yellow-text"></i>';
        }
        html += '</div>';
        html += '</div></article>';
    });
    return html;
}
function loadTabMeasuresContent() {
    $.LoadingOverlay("show");
    //Validate Date 
    if ($('#slTimeframe').val() == "2") {
        if (!$('#txtCustomDate').val())
            return;

        var drp = $('#txtCustomDate').data('daterangepicker');
        var maxdate = drp.startDate.add(12, 'months').toDate();
        if (drp.endDate.toDate() > maxdate) {
            cleanBookNotification.error(_L("ERROR_MSG_706"), "Operator");
            return;
        }
        var max31day = drp.startDate.add(31, 'days').toDate()
        if (drp.endDate.toDate() > max31day) {
        }
    }

    //end validate
    var parmaters = {
        goalId: $('#goalId').val(),
        timeframe: $('#slTimeframe').val(),
        customDate: $("#txtCustomDate").val(),
        search:$("txtSearchMeasures").val()
    }
    $.ajax({
        type: 'post',
        url: '/Operator/LoadTabGoalMeasures',
        datatype: 'json',
        data: parmaters,
        success: function (response) {
            if (response) {
                var $divcontain = $('#goal-measures');
                $divcontain.html(response);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function deleteGoalMeasure(id) {
    $.post("/Operator/DeleteGoalMeasure", { id: id }, function (data) {
        if(data.result)
        {
            $('#gm_' + id).remove();
        }else if(!data.result&&data.msg)
        {
            cleanBookNotification.error(_L(data.msg), "Operator");
        }else
        {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}
