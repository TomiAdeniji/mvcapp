
$(function () {

});

function loadBudgetScenariosContents() {
    $('#budget-scenarios').LoadingOverlay("show");
    $('#budget-scenarios').empty();
    $('#budget-scenarios').load('/TraderBudget/BudgetScenariosContent', function () {
        $('#budget-scenarios').LoadingOverlay("hide");
    });
}
function budgetScenariosSearch() {
    loadBudgetScenariosItem();
}
function loadBudgetScenariosItem() {
    var key = $('#budget_scenarios_search_key').val();
    var sortBy = $('#budget_scenarios_orderby_filter').val();
    $('#budget_scenarions_Items').LoadingOverlay("show");
    $('#budget_scenarions_Items').empty();
    $('#budget_scenarions_Items').load('/TraderBudget/BudgetScenariosItems?key=' + key + '&sortBy=' + sortBy, function () {
        $('#budget_scenarions_Items').LoadingOverlay("hide");
    });
}
function budgetScenatioAddEdit(id) {
    $('#app-trader-budgetscenario-add').LoadingOverlay("show");
    $('#app-trader-budgetscenario-add').empty();
    $('#app-trader-budgetscenario-add').load('/TraderBudget/BudgetScenarioAddEditModal?id=' + id, function () {
        $('#app-trader-budgetscenario-add').LoadingOverlay("hide");
    });
}
function ChangeImageItem(ev) {
    $.LoadingOverlay("show");
    UploadMediaS3ClientSide("budgetscenario_featuredimage").then(function (mediaS3Object) {
        LoadingOverlayEnd();
        if (ev.files && ev.files.length > 0) {
            $('#preview').css('display', 'block');
            readURLImage(ev, 'preview');
        }
        else $('#preview').css('display', 'none');
        $('#budgetscenario_featuredimage_value').val(mediaS3Object.objectKey);
        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            return;
        }

    });
}
function getMonths(startDate, endDate) {
    var months = 0;
    var monthStart = (new Date(startDate)).getMonth();
    var monthEnd = (new Date(endDate)).getMonth();
    var yearStart = (new Date(startDate)).getFullYear();
    var yearEnd = (new Date(endDate)).getFullYear();
    if (yearStart === yearEnd) {
        months = monthEnd - monthStart;
    } else if (yearEnd - yearStart === 1) {
        months = (12 - monthStart) + monthEnd;
    } else if (yearEnd - yearStart > 1) {
        months = (12 - monthStart) + monthEnd + (yearEnd - yearStart - 1) * 12;
    }
    return months;
}
function onPeriodChange(id) {
    var startPeriod = $('#budgetscenario_fiscalstart').val();
    var endPeriod = $('#budgetscenario_fiscalend').val();
    if (startPeriod !== '' && endPeriod !== '') {
        if ((startPeriod === endPeriod)
            || ((new Date(startPeriod)).getTime() >= (new Date(endPeriod)).getTime())) {
            $('#' + id).val('');
            return;
        }
        var lstStart = [0, 3, 6, 9];
        var lstEnd = [0, 3, 6, 9];
        var months = getMonths(startPeriod, endPeriod);
        if (months > 6) {
            $("#budgetscenario_reportingperiod option[value='Weekly']").remove();
        } else {
            $("#budgetscenario_reportingperiod option[value='Weekly']").remove();
            $("#budgetscenario_reportingperiod").append('<option value="Weekly">Weekly</option>');
        }
        if (months % 3 === 0 && lstStart.indexOf((new Date(startPeriod)).getMonth()) !== -1 && lstEnd.indexOf((new Date(endPeriod)).getMonth()) !== -1) {
            $("#budgetscenario_reportingperiod option[value='Quarterly']").remove();
            $("#budgetscenario_reportingperiod").append('<option value="Quarterly">Quarterly</option>');
        } else {
            $("#budgetscenario_reportingperiod option[value='Quarterly']").remove();
        }
    }
    $('#budgetscenario_reportingperiod').select2();
}
function smalllestClick() {
    var smalVal = $("#budgetscenario_reportingperiod").val();
    var startPeriod = $('#budgetscenario_fiscalstart').val();
    var endPeriod = $('#budgetscenario_fiscalend').val();
    if (startPeriod !== '' && endPeriod !== '') {
        var lstStart = [0, 3, 6, 9];
        var lstEnd = [0, 3, 6, 9];
        var months = getMonths(startPeriod, endPeriod);
        if (months > 6) {
            $("#budgetscenario_reportingperiod option[value='Weekly']").remove();
        } else {
            $("#budgetscenario_reportingperiod option[value='Weekly']").remove();
            $("#budgetscenario_reportingperiod").append('<option value="Weekly">Weekly</option>');
        }
        if (months % 3 === 0 && lstStart.indexOf((new Date(startPeriod)).getMonth()) !== -1 && lstEnd.indexOf((new Date(endPeriod)).getMonth()) !== -1) {
            $("#budgetscenario_reportingperiod option[value='Quarterly']").remove();
            $("#budgetscenario_reportingperiod").append('<option value="Quarterly">Quarterly</option>');
        } else {
            $("#budgetscenario_reportingperiod option[value='Quarterly']").remove();
        }
    }
    $("#budgetscenario_reportingperiod").val(smalVal);
    $('#budgetscenario_reportingperiod').select2();
}
function validateBudgetScenario() {
    var valid = true;
    // check title
    if ($('#budgetscenario_title').val() === '') {
        $("#budgetScenario_addedit").validate().showErrors({ scenario_title: "Title is required." });
        valid = false;
    }
    // check image feature
    if ($('#budgetscenario_featuredimage_value').val() === '' || $('#budgetscenario_featuredimage_value').val() === null) {
        $("#budgetScenario_addedit").validate().showErrors({ featuredimg: "Featured image is required." });
        valid = false;
    }
    // FiscalStartPeriod
    if ($('#budgetscenario_fiscalstart').val() === '') {
        $("#budgetScenario_addedit").validate().showErrors({ budgetscenario_fiscalstart: "Fiscal start period is required." });
        valid = false;
    }
    // FiscalEndPeriod
    if ($('#budgetscenario_fiscalend').val() === '') {
        $("#budgetScenario_addedit").validate().showErrors({ budgetscenario_fiscalend: "Fiscal end period is required." });
        valid = false;
    }
    // check payments
    if (!$('#budgetscenario_reportingperiod').val() || ($('#budgetscenario_reportingperiod').val() && $('#budgetscenario_reportingperiod').val() === '')) {
        $("#budgetScenario_addedit").validate().showErrors({ smallest: "Smallest reporting period is required." });
        valid = false;
    }
    return valid;
}
// return true is exists
function checkExistsTitle() {
    var dfd = new $.Deferred();
    $.ajax({
        type: 'get',
        url: '/TraderBudget/CheckExistsTitle?title=' + $('#budgetscenario_title').val() + '&id=' + $('#budget_scenarios_id').val(),
        async: false,
        contentType: false, // Not to set any content header  
        processData: false, // Not to process data  
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                dfd.resolve(response.result);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                dfd.resolve(response.result);
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            dfd.resolve(false);
        }
    });
    return dfd.promise();
};
function getDateString(date) {
    var newDate = new Date(date);
    // format dd/MM/yyyy
    return newDate.getDate() + '/' + (newDate.getMonth() + 1) + '/' + newDate.getFullYear();
}
function saveBudgetScenario() {
    checkExistsTitle().then(function (res) {
        if (!res) {
            $("#budgetScenario_addedit").validate().showErrors({ scenario_title: "Title  must be unique at a Location." });
            cleanBookNotification.error(_L("ERROR_MSG_250"), "Qbicles");
            return;
        }
        if (!validateBudgetScenario()) {
            return;
        }
        var budgetScenario = {
            Id: $('#budget_scenarios_id').val(),
            Title: $('#budgetscenario_title').val(),
            Description: $('#budgetscenario_description').val(),
            FeaturedImage: $('#budgetscenario_featuredimage_value').val(),
            FiscalStartPeriod: getDateString($('#budgetscenario_fiscalstart').val()),
            FiscalEndPeriod: getDateString($('#budgetscenario_fiscalend').val()),
            ReportingPeriod: $('#budgetscenario_reportingperiod').val(),
            BudgetGroups: []
        }
        var budgetGroups = $('#budgetscenario_budgetgroup').val();
        if (budgetGroups !== null && budgetGroups.length > 0) {
            for (var i = 0; i < budgetGroups.length; i++) {
                budgetScenario.BudgetGroups.push({
                    Id: budgetGroups[i]
                });
            }
        }
        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/TraderBudget/SaveBudgetScenario?startdate=' + getDateString($('#budgetscenario_fiscalstart').val()) + '&enddate=' + getDateString($('#budgetscenario_fiscalend').val()),
            data: { budgetScenaio: budgetScenario },
            dataType: 'json',
            success: function (response) {
                $.LoadingOverlay("hide");
                if (response.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                    $('#app-trader-budgetscenario-add').modal('toggle');
                    loadBudgetScenariosItem();
                } else if (response.actionVal === 2) {
                    cleanBookNotification.updateSuccess();
                    $('#app-trader-budgetscenario-add').modal('toggle');
                    loadBudgetScenariosItem();
                } else if (response.actionVal === 3) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }

            },
            error: function (er) {
                $.LoadingOverlay("hide");
                cleanBookNotification.error(_L("ERROR_MSG_267"), "Qbicles");
            }
        });
    });

}

function setActive(id) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'get',
        url: '/TraderBudget/setActive?id=' + id,
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1) {
                loadBudgetScenariosItem();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

		},
		error: function (er) {
			$.LoadingOverlay("hide");
			cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
		}
	});
}


