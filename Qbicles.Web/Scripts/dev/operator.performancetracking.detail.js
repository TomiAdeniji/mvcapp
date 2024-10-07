var isBusy = false;
$(document).ready(function () {
    initModalMedia();
    createDiscussion();
    initDateRange();
    LoadPerfomanceMeasures();
    $('#slTimeframe').change(searchThrottle(function () {
        $('.text-timeframe').text($("#slTimeframe option:selected").text());
        if ($('#tabNavPerformance li.active a[data-target=#performance-overview]').length > 0)
            LoadPerfomanceMeasures();
        else
            loadTabMeasuresContent();
    }));
    $('#txtCustomDate').change(searchThrottle(function () {
        LoadPerfomanceMeasures();
    }));
    $('#txtSearchMeasures').keyup(searchThrottle(function () {
        loadTabMeasuresContent();
    }));
});

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
                                location.reload();
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

function getAllMeasureWithWeights() {
    var lst = [];
    $("#tblTrackingMeasures tbody tr").each(function () {
        var measureId = $(this).data("id");
        var weight = $(this).data("weight");
        lst.push({ MeasureId: measureId, Weight: weight });
    });
    return lst;
}

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
        //$('#txtCustomDate').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
    });
}

function LoadMedias(qid) {
    $.LoadingOverlay("show");
    var fid = $('#mediaFolderId').val();
    var qid = $('#qbicleId').val();
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/Operator/LoadMedia',
        datatype: 'json',
        data: { fid: fid, qid: qid, fileType: fileType == "All" ? "" : fileType, rs: '' },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#performance-resources .flex-grid-thirds-lg');
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
function initModalMedia() {
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
            var files = document.getElementById("performancetracking-resource-input").files;
            if (files.length > 0) {
                UploadMediaS3ClientSide("performancetracking-resource-input").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#performancetracking-resource-object-key").val(mediaS3Object.objectKey);
                        $("#performancetracking-resource-object-name").val(mediaS3Object.fileName);
                        $("#performancetracking-resource-object-size").val(mediaS3Object.fileSize);
                        var frmData = new FormData($form_media_addedit[0]);

                        $.ajax({
                            type: "POST",
                            cache: false,
                            url: "/Operator/SaveResource",
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
function createDiscussion() {

    $('#frm-create-discussion').submit(function (e) {
        e.preventDefault();
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Discussions/SaveDiscussionForPerformance',
            datatype: 'json',
            data: { perfomanceId: $('#performanceTrackingId').val(), openingmessage: $('#ds_openingmessage').val(), isexpiry: $('#ds_isexpiry').prop('checked'), expirydate: $('#ds_expirydate').val() },
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
                    cleanBookNotification.success(_L("ERROR_MSG_807"), "Operator");
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
function LoadPerfomanceMeasures() {
    $('.text-timeframe').text($("#slTimeframe option:selected").text());
    //Validate Date 
    var isDay = true;
    if ($('#slTimeframe').val() == "2") {
        $('.text-timeframe').text($('#txtCustomDate').val());
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
            isDay = false;
        }
    }

    //end validate
    var parmaters = {
        performanceId: $('#performanceTrackingId').val(),
        timeframe: $('#slTimeframe').val(),
        customDate: $("#txtCustomDate").val(),
        isDay: isDay
    }
    $.ajax({
        type: 'post',
        url: '/Operator/LoadPerfomanceMeasuresForChart',
        datatype: 'json',
        data: parmaters,
        success: function (response) {
            if (response) {
                animatestat(response.TotalProgressPerformance);
                if (response.ChartProcessMeasures) {
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
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function animatestat(value) {
    $('#performanceChartProcess').circleProgress({
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

    // Gradients for bars
    var gradaqua = ctx.createLinearGradient(0, 0, 0, 400);
    var gradblue = ctx.createLinearGradient(0, 0, 0, 400);

    // Aqua
    gradaqua.addColorStop(0, "#05e4b1");
    gradaqua.addColorStop(1, "#1dc7e6");

    // Blue
    gradblue.addColorStop(0, "#05a8f4");
    gradblue.addColorStop(1, "#049ef4");
    
    var _labels = [];
    var _data = [];
    var _backgroundColor = [];
    $.each(data, function (index, item) {
        if (item.key != "01/01") {
            _labels.push(item.key);
            _data.push(item.processPercent);
            if (index !== data.length - 1)
                _backgroundColor.push(gradblue);
            else
                _backgroundColor.push(gradaqua);
        }
    });

    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: _labels,
            datasets: [{
                label: '% progress towards Performace',
                data: _data,
                backgroundColor: _backgroundColor,
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
                        steps: 10,
                        stepValue: 5,
                        max: 100
                    }
                }]
            }
        }
    });
}

function measuresTemplating(data) {
    var html = '';
    $.each(data, function (index, item) {
        var classColor = 'started';
        if (item.Score == 3) {
            classColor = 'pending';
        } else if (item.Score <= 2) {
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
        performanceId: $('#performanceTrackingId').val(),
        timeframe: $('#slTimeframe').val(),
        customDate: $("#txtCustomDate").val(),
        search: $("#txtSearchMeasures").val()
    }
    $.ajax({
        type: 'post',
        url: '/Operator/LoadTabPerformanceMeasures',
        datatype: 'json',
        data: parmaters,
        success: function (response) {
            if (response) {
                var $divcontain = $('#performance-measures');
                $divcontain.html(response);
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function deletePerformanceMeasure(id) {
    $.post("/Operator/DeletePerformanceMeasure", { id: id }, function (data) {
        if (data.result) {
            $('#gm_' + id).remove();
        } else if (!data.result && data.msg) {
            cleanBookNotification.error(_L(data.msg), "Operator");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function ShowGroupMember() {
    $('#team-person-preview').load("/Operator/ShowListMemberForWorkGroup?performanceId=" + $("#performanceId").val() + "&wgId=" + $("#performanceWorkgroup").val());
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