var eventDates = [], $tagsTable = $('#tags-table'), $frm_tag_addedit = $('#frm_tag_addedit');
var activeTab = 1;
var tagsTypeaHead = [];
var isBusy = false;
var isInitPagination = false;
function CalDotActive(Year, Month) {
    var mParams = { Year: Year, Month: Month };
    $.getJSON("/Qbicles/LoadDotMyDeskActivities", mParams, function (data) {
        const cellFunc = function (date, cellType) {
            var currentDate = moment(date).format("DD/MM/YYYY");
            var datedot = null;
            $.each(data, function (index, value) {
                if (value.date == currentDate) {
                    datedot = value;
                    return;
                }
            });
            if (cellType == 'day' && (datedot != null && datedot.date === currentDate)) {
                return {
                    html: date.getDate() + '<span class="' + datedot.color + '"></span>'
                }
            }
        }
        var calendar = $('.calendar-view').datepicker().data('datepicker');
        calendar.update('onRenderCell', cellFunc);
    });
}
$(document).ready(function () {
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(".mydesk-alt li.active a").click();
    SetActiveRemoveFilter();
    LoadQbicleData(1);
    LoadQbicleData(2);
    LoadQbicleData(3);
    LoadQbicleData(4);
    LoadQbicleData(5);
    LoadQbicleData(6);
    LoadQbicleData(7);
    if ($("#isHide1").val()==1) {
        $("#isHide1").bootstrapToggle('on');
    }
    if ($("#isHide2").val() == 1) {
        $("#isHide2").bootstrapToggle('on');
    }
    if ($("#isHide3").val() == 1) {
        $("#isHide3").bootstrapToggle('on');
    }
    if ($("#isHide6").val() == 1) {
        $("#isHide6").bootstrapToggle('on');
    }
    var $picker = $('.calendar-view');
    $picker.datepicker({
        language: 'en',
        onSelect: function (formattedDate) {
            var date = new Date();
            var today = moment(date).format('DD/MM/YYYY');
            $('.selected-date').html(today == formattedDate ? "today" : formattedDate);
            loadDataBySelectedDate(formattedDate);
        },
        onChangeMonth: function (month, year) {
            var calendar = $picker.datepicker().data('datepicker');
            var date = calendar.currentDate;
            CalDotActive(date.getFullYear(), date.getMonth() + 1)
        },
        onChangeView: function (view) {
            if (view == "days") {
                var calendar = $picker.datepicker().data('datepicker');
                var date = calendar.currentDate;
                CalDotActive(date.getFullYear(), date.getMonth() + 1)
            }
        },
    });
    $picker.datepicker().data('datepicker').selectDate(cdate);

    $('.select2avatar').select2({
        placeholder: 'Please select',
        templateResult: MyDeskformatOptions,
        templateSelection: MyDeskformatSelected
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
    $('.daterange-up').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        drops: "up",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });

    $('.daterange-up').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
    });

    $('.daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        $('#date_range').html(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        SetActiveRemoveFilter();
    });

    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        $('#date_range').html('full history');
    });
    $("#search_dt").keyup(TagSearchThrottle(function () {
        $tagsTable.DataTable().search($(this).val()).draw();
    }));
    $("#MyDeskSearchContact").keyup(TagSearchThrottle(function () {
        loadUserContactData();
    }));
    $("#txtSearchActivity1, #txtSearchActivity2, #txtSearchActivity3, #txtSearchActivity4, #txtSearchActivity5, #txtSearchActivity6, #txtSearchActivity7").keyup(TagSearchThrottle(function () {
        SetActiveRemoveFilter();
        SearchData();
    }));

    $("#ddlTags1, #ddlTags2, #ddlTags3, #ddlTags4, #ddlTags5, #ddlTags6, #ddlTags7, #isHide1, #isHide2, #isHide3, #isHide7, #ddlActType1").change(TagSearchThrottle(function () {
        SetActiveRemoveFilter();
    }));

    $frm_tag_addedit.validate(
        {
            rules: {
                Name: {
                    required: true,
                    minlength: 3
                }
            }
        });
    $frm_tag_addedit.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            return;
        }
        if ($frm_tag_addedit.valid()) {
            $.ajax({
                type: this.method,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                async: false,
                beforeSend: function (xhr) { isBusy = true; $.LoadingOverlay("show"); },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#tag-add').modal('hide');
                        $frm_tag_addedit.trigger("reset");
                        if (tagId == 0)
                            cleanBookNotification.success(_L("ERROR_MSG_134"), "Tags");
                        else
                            cleanBookNotification.success(_L("ERROR_MSG_135"), "Tags");
                        $('#tagId').val(0);
                        if (data.Object) {
                            $('.tag_' + data.Object.Id).text(data.Object.Name);
                        }
                        $tagsTable.DataTable().ajax.reload();
                        TagLoadDataTypeaHead();
                    } else if (data.result === false && data.msg) {
                        $frm_tag_addedit.validate().showErrors({ Name: data.msg });
                    } else {
                        if (tagId == 0)
                            cleanBookNotification.error(_L("ERROR_MSG_136"), "Tags");
                        else
                            cleanBookNotification.error(_L("ERROR_MSG_137"), "Tags");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Tags");
                    LoadingOverlayEnd();
                }
            });
        }
    });

    $(".select2,.daterange,.select2avatar").on("change", function () {

        if ($(this).val() != "" && $(this).val() != "0" && $(this).val() != null)
            SetActiveRemoveFilter();
    });
});
function ActiveTable(obj, tabIndex) {
    $(".filtering ").hide();
    $(".mydesk-alt li").removeClass("active");
    $(obj).parent().addClass("active");
    $(".mydesk-alt li").each(function () {
        $($(this).find("a").attr("data-target")).removeClass("in active");
    });
    //$($(obj).attr("data-target")).css({ "display": "block" });
    $($(obj).attr("data-target")).addClass("in active");
    $('#btnfiltermodal').attr("data-target", "#mydesk-filter-activities-" + tabIndex);
    activeTab = tabIndex;
    var templatePagination = '<br><div id="paginateTemplate" style="display: none"></div ><div class="clearfix"></div>';
    $('#lstPin').html("");
    $('#lstTask').html("");
    $('#lstEvent').html("");
    $('#lstMedia').html("");
    $('#lstProcess').html("");
    $('#lstLink').html("");
    $('#lstDiscussion').html("");
    if (activeTab == 1) {

        $('#lstPin').html(templatePagination);
    }
    else if (activeTab == 2) {

        $('#lstTask').html(templatePagination);
    }
    else if (activeTab == 3) {

        $('#lstEvent').html(templatePagination);
    }
    else if (activeTab == 4) {
        $('#lstMedia').html(templatePagination);
    }
    else if (activeTab == 6) {
        $('#lstProcess').html(templatePagination);
    }
    else if (activeTab == 5) {
        $('#lstLink').html(templatePagination);
    }
    else if (activeTab == 7) {
        $('#lstDiscussion').html(templatePagination);
    }
    parLoadPins = 0;
    isInitPagination = false;
    ReloadData(true, 0);
}
function MydeskPinnedActivity(taskid, isPost, event) {
    PinnedActivity(taskid, isPost, event);
    //parLoadPins = 0;
    //$('#content-Mypins').html("");
    //ReloadData();
    //event.preventDefault();
    //event.stopPropagation();
}
function MyDeskformatOptions(state) {
    if (!state.id) { return state.text; }
    var urlAvatar = state.element.attributes["avatarUrl"].value;
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function MyDeskformatSelected(state) {
    if (!state.id) { return state.text; }
    var urlAvatar = state.element.attributes["avatarUrl"].value;
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + urlAvatar.toLowerCase() + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function UnPinnedActivity(activityId, isPost, event) {
    PinnedActivity(activityId, isPost, event);
    //parLoadPins = 0;
    //$('#content-Mypins').html("");
    //ReloadData();
};
// pin or unpin the activity
function PinnedActivity(Id, IsPost, event) {
    //event.stopPropagation();
    $.LoadingOverlay("show");
    var url = "/MyDesks/PinnedActivity/";
    $.ajax({
        url: url,
        data: { ActivityId: Id, IsPost: IsPost, mydeskId: $('#MyDeskId').val() },
        cache: false,
        type: "POST",
        success: function (refModel) {
            if (refModel.result) {
                if (activeTab == 1) {
                    SearchData();
                } else {
                    //var iconp = $('.iconpin-' + Id);
                    //if (iconp.css("display") === "inline-block")
                    if ($('#textPin-' + Id).text().trim() == "Unpin this") {
                        //iconp.css("display", "none");
                        $('#textPin-' + Id).text("  Pin this");
                    }
                    else {
                        //iconp.css("display", "inline-block");
                        $('#textPin-' + Id).text("  Unpin this");
                    }

                    //if ($('#lstPin').length > 0) {
                    //    $('#lstPin .acid-' + Id).remove();
                    //}
                }
            }

            LoadingOverlayEnd();
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });

}
var parLoadPins = 0;
function loadMorePins() {
    $.LoadingOverlay("show");

    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        if (parLoadPins == 0)
            parLoadPins++;
        var obj = {};
        obj.SearchName = $("#txtSearchActivity").val();
        obj.order = $("#ddlOrderby" + activeTab + " :selected").val();
        obj.dateRange = $("#txtDaterange" + activeTab).val();
        obj.domainId = $("#dllDomainId" + activeTab + " :selected").val();
        if (parseInt(obj.domainId) > 0)
            obj.qbcileId = $("#ddlQbicle" + activeTab + " :selected").val();
        obj.tags = $("#ddlTags" + activeTab).val();
        obj.status = $("#ddlStatus" + activeTab + " :selected").val();
        obj.UserId = $("#ddlUser" + activeTab).val();

        $.ajax({
            type: "GET",
            url: "/MyDesks/MyDeskLoadMorePins",
            data: {
                model: JSON.stringify(obj),
                skip: parLoadPins * myDeskPageSize,
                myDeskId: $("#MyDeskId").val(),
                type: activeTab
            },
            async: false,
            success: function (refModel) {
                if (refModel.result) {
                    if (activeTab == 1) {
                        $('#lstPin').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreSpined').show();
                        else
                            $('#btnLoadMoreSpined').hide();
                    }
                    else if (activeTab == 2) {
                        $('#lstTask').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreTask').show();
                        else
                            $('#btnLoadMoreTask').hide();
                    }
                    else if (activeTab == 3) {
                        $('#lstEvent').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreEvent').show();
                        else
                            $('#btnLoadMoreEvent').hide();
                    }
                    else if (activeTab == 4) {
                        $('#lstMedia').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreMedia').show();
                        else
                            $('#btnLoadMoreMedia').hide();
                    }
                    else if (activeTab == 5) {
                        $('#lstLink').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreLink').show();
                        else
                            $('#btnLoadMoreLink').hide();
                    }
                    else if (activeTab == 6) {
                        $('#lstProcess').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreProcess').show();
                        else
                            $('#btnLoadMoreProcess').hide();
                    } else if (activeTab == 7) {
                        $('#lstDiscussion').append(refModel.Object.strResult);
                        if (refModel.Object.strResult.trim() != null && ((parLoadPins + 1) * myDeskPageSize) < refModel.Object.totalRecord)
                            $('#btnLoadMoreDiscussion').show();
                        else
                            $('#btnLoadMoreDiscussion').hide();
                    }
                    parLoadPins++;


                }
                LoadingOverlayEnd();
            },
            error: function (err) {
                cleanBookNotification.error(err.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}
function ReloadData(isLoading, currentPage) {
    SetActiveRemoveFilter();
    var selected_tab = '';
    if (activeTab == 1) {
        $('#lstPin').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstPin';
    }
    else if (activeTab == 2) {
        $('#lstTask').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstTask';
    }
    else if (activeTab == 3) {
        $('#lstEvent').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstEvent'
    }
    else if (activeTab == 4) {
        $('#lstMedia').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstMedia';
    }
    else if (activeTab == 5) {
        $('#lstLink').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstLink';
    }
    else if (activeTab == 6) {
        $('#lstProcess').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstProcess';
    }
    else if (activeTab == 7) {
        $('#lstDiscussion').LoadingOverlay('show', { minSize: "70x60px" });
        selected_tab = 'lstDiscussion';
    }
    if (isLoading || typeof myVar === 'undefined')
    var obj = {};
    obj.SearchName = $("#txtSearchActivity" + activeTab).val();
    obj.order = $("#ddlOrderby" + activeTab + " :selected").val();
    obj.dateRange = $("#txtDaterange" + activeTab).val();
    obj.domainId = $("#dllDomainId" + activeTab + " :selected").val();
    if (parseInt(obj.domainId) > 0)
        obj.qbcileId = $("#hdfQbicleId" + activeTab).val();
    obj.tags = $("#ddlTags" + activeTab).val();
    obj.isHide = $("#isHide" + activeTab).is(':checked') ? 1 : 0;
    obj.actType = $("#ddlActType" + activeTab).val();

    $.ajax({
        type: "GET",
        url: "/MyDesks/MyDeskLoadMorePins",
        data: {
            model: JSON.stringify(obj),
            skip: currentPage * myDeskPageSize,
            myDeskId: $("#MyDeskId").val(),
            type: activeTab
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                if(!isInitPagination)
                    initPagination(refModel.Object.totalRecord);
                if (activeTab == 1) {
                    $('.tab-content article').remove();
                    $('#lstPin').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 2) {
                    $('.tab-content article').remove();
                    $('#lstTask').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 3) {
                    $('.tab-content article').remove();
                    $('#lstEvent').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 4) {
                    $('.tab-content article').remove();
                    $('#lstMedia').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 5) {
                    $('.tab-content article').remove();
                    $('#lstLink').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 6) {
                    $('.tab-content article').remove();
                    $('#lstProcess').prepend(refModel.Object.strResult);
                }
                else if (activeTab == 7) {
                    $('.tab-content article').remove();
                    $('#lstDiscussion').prepend(refModel.Object.strResult);
                }
            }

            $('#' + selected_tab).LoadingOverlay('hide');
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            $('#' + selected_tab).LoadingOverlay('hide');
        }
    });
}
function initPagination(totalRecord) {
    var container = $('#paginateTemplate');
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
            pageSize: myDeskPageSize,
            dataSource: sources,
            callback: function (response, pagination) {
                if (isInitPagination)
                    ReloadData(true, pagination.pageNumber - 1)
            }
        };
        container.pagination(options);
        isInitPagination = true;
    }
    else
        container.hide();
}

function EditTags(activityId, IsPost) {
    $('select[name=assigneeTags]').val([]).change();
    $("#hdActivityId").val(activityId);
    $("#hdIsPost").val(IsPost);
    $("#assigneeTags option").remove();
    $.ajax({
        type: "GET",
        url: "/MyDesks/GetAllTagByMyDeskId",
        data: {
            myDeskId: $("#MyDeskId").val()
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                var data = refModel.Object;
                var str = ' <option value=""></option>';
                if (data != '') {
                    for (var i = 0; i < data.length; ++i) {
                        str += '<option value="' + data[i].Id + '" Desk-Id="' + data[i].Desk_Id + '">' + data[i].Name + '</option>';
                    }
                }
                $("#assigneeTags").append(str);
                var TagId = $("#hdTagIds-" + activityId).val();
                var arrTagId = TagId.split(',');
                if (arrTagId != '') {
                    $('select[name=assigneeTags]').val(arrTagId).change();
                }
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });

}
function loadDataBySelectedDate(SelectedDate) {
    if ($('.loadingoverlay').length == 0)
        $('.calendar-view').LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/MyDeskLoadByDateSelected",
        data: {
            selectedDate: SelectedDate,
            myDeskId: $("#MyDeskId").val()
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                if (refModel.Object.strResult != undefined && refModel.Object.strResult.trim() != '')
                    $('#lblToDay').html(refModel.Object.strResult);
                else {
                    var dt = new Date();
                    $('#lblToDay').html('<div class="mdv2-activity"> You have no to-dos ' + (SelectedDate != (dt.getDate() + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear()) ? SelectedDate : 'today') + '!</div>');
                }
            }

            $('.calendar-view').LoadingOverlay("hide");
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");

            $('.calendar-view').LoadingOverlay("hide");
        }
    });
}
function loadUserContactData() {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/MyDeskLoadUserContact",
        data: {
            searchName: $("#MyDeskSearchContact").val()
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#lstMyDeskContact').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            LoadingOverlayEnd();
        }
    });
}
function SearchData() {
    //getTotalPage();
    isInitPagination = false;
    ReloadData(true, 0);
}
function SetActiveRemoveFilter() {
    switch (activeTab) {
        case 1:
            if ($("#txtSearchActivity" + activeTab).val() == '' && $('#ddlOrderby' + activeTab).val() == "2" && $('#dllDomainId' + activeTab).val() == '0' && $('#ddlQbicle' + activeTab).val() == '0' && !$('#txtDaterange' + activeTab).val()
                && !$('#ddlTags' + activeTab).val() && $("#ddlActType" + activeTab).val() == 0 && !$("#isHide" + activeTab).is(':checked')) {
                $(".btnRemoveApplyfilters_" + activeTab).hide();
            } else {
                $(".btnRemoveApplyfilters_" + activeTab).show();
            }
            break;
        case 2:
        case 3:
            if ($("#txtSearchActivity" + activeTab).val() == '' && $('#ddlOrderby' + activeTab).val() == "0" && !$('#txtDaterange' + activeTab).val() && $('#dllDomainId' + activeTab).val() == '0' && $('#ddlQbicle' + activeTab).val() == '0'
                && !$('#ddlTags' + activeTab).val() && !$("#isHide" + activeTab).is(':checked')) {
                $(".btnRemoveApplyfilters_" + activeTab).hide();
            } else {
                $(".btnRemoveApplyfilters_" + activeTab).show();
            }
            break;
        case 4:
        case 5:
            if ($("#txtSearchActivity" + activeTab).val() == '' && $('#ddlOrderby' + activeTab).val() == "2" && !$('#txtDaterange' + activeTab).val() && $('#dllDomainId' + activeTab).val() == '0' && $('#ddlQbicle' + activeTab).val() == '0'
                && !$('#ddlTags' + activeTab).val()) {
                $(".btnRemoveApplyfilters_" + activeTab).hide();
            } else {
                $(".btnRemoveApplyfilters_" + activeTab).show();
            }
            break;
        case 6:
            if ($("#txtSearchActivity" + activeTab).val() == '' && $('#ddlOrderby' + activeTab).val() == "0" && !$('#txtDaterange' + activeTab).val() && $('#dllDomainId' + activeTab).val() == '0' && $('#ddlQbicle' + activeTab).val() == '0'
                && !$('#ddlTags' + activeTab).val()) {
                $(".btnRemoveApplyfilters_" + activeTab).hide();
            } else {
                $(".btnRemoveApplyfilters_" + activeTab).show();
            }
            break;
        case 7:
            if ($("#txtSearchActivity" + activeTab).val() == '' && $('#ddlOrderby' + activeTab).val() == "2" && !$('#txtDaterange' + activeTab).val() && $('#dllDomainId' + activeTab).val() == '0' && $('#ddlQbicle' + activeTab).val() == '0'
                && !$('#ddlTags' + activeTab).val() && !$("#isHide" + activeTab).is(':checked')) {
                $(".btnRemoveApplyfilters_" + activeTab).hide();
            } else {
                $(".btnRemoveApplyfilters_" + activeTab).show();
            }
            break;
    }
}
function ResetSearchDataActivity(obj) {
    $("#ddlOrderby" + activeTab).val($("#ddlOrderby" + activeTab + " option:nth-child(1)").val()).change();
    $("#txtDaterange" + activeTab).val('').change();
    $("#dllDomainId" + activeTab).val(0).change();
    $("#ddlQbicle" + activeTab).val(0).change();
    $("#ddlQbicle" + activeTab).prop("disabled", true);
    $("#hdfQbicleId" + activeTab).val(0);
    $('#ddlTags' + activeTab).multiselect('deselectAll', false);
    $('#ddlTags' + activeTab).multiselect('select', []);   
    $("#ddlStatus" + activeTab).val(0).change();
    $("#ddlUser" + activeTab).val([]).change();
    $("#isHide" + activeTab).bootstrapToggle('off');
    $("#ddlActType" + activeTab).val(0).change();
    $("#txtSearchActivity" + activeTab).val('').change();
    $(".btnRemoveApplyfilters_" + activeTab).hide();
    isInitPagination = false;
    ReloadData(true, 0);
}
function LoadQbicleData(tab) {
    SetActiveRemoveFilter();
    var domainId = $("#dllDomainId" + tab + " :selected").val();
    $("#ddlQbicle" + tab + " option:not(:nth-child(1))").remove();

    if (parseInt(domainId) > 0) {
        $('#ddlQbicle' + tab).prop("disabled", false);
        $.ajax({
            type: "GET",
            url: "/MyDesks/GetListQbicleByDomainId",
            data: {
                domainId: domainId
            },
            async: false,
            success: function (refModel) {
                var str = '';
                if (refModel.result) {

                    var data = refModel.Object;
                    for (var i = 0; i < data.length; ++i) {
                        str += '  <option value="' + data[i].Id + '">' + data[i].Name + '</option>';
                    }
                    if (data.length == 0) $("#ddlQbicle" + tab).val(0).change();
                }
                $("#ddlQbicle" + tab).append(str);
                $("#ddlQbicle" + tab).val($('#hdfQbicleId' + tab).val()).change();
                
            },
            error: function (err) {
                cleanBookNotification.error(err.responseText, "Qbicles");

                // LoadingOverlayEnd();
            }
        });
    }
    else {
        $("#ddlQbicle" + tab).val(0).change();
        $('#ddlQbicle' + tab).prop("disabled", true);
    }
       
}
function UpdateActivityTags() {
    $.LoadingOverlay("show");
    var tags = $("#assigneeTags").val();
    var strTags = "";
    if (tags != "" && tags != undefined)
        strTags = tags.join(',');
    var model = {
        myDeskId: $("#MyDeskId").val(),
        activityId: $("#hdActivityId").val(),
        TagsId: strTags,
        IsPost: $("#hdIsPost").val()
    };
    $.ajax({
        type: "GET",
        url: "/MyDesks/UpdateTagsByActivity",
        data: model,
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                SearchData();
                $('select[name=assigneeTags]').val([]).change();
            }
            $('#tags-manage').modal('toggle');
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");

            LoadingOverlayEnd();
        }
    });
}
function TagsTableLoad() {
    $tagsTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        ajax: {
            "url": "/MyDesks/GetDataTags",
            "data": function (d) {
                return $.extend({}, d, {
                    "myDeskId": $("#MyDeskId").val()
                });
            }
        },
        columns: [
            { "title": "Tag", "data": "Name", "searchable": true },
            { "title": "Created", "data": "CreatedDate", "searchable": true },
            { "title": "Creator", "data": "Creator", "searchable": true },
            { "title": "Instances", "data": "Instances", "searchable": true },
            null
        ],
        columnDefs: [{
            "targets": 4,
            "data": "Id",
            "render": function (data, type, row, meta) {
                var soption = '<button class="btn btn-warning" data-toggle="modal" data-target="#tag-add" onclick="TagLoadEdit(' + data + ',\'' + row.Name + '\',true)"><i class="fa fa-pencil"></i></button>';
                soption += ' <button class="btn btn-danger" data-toggle="modal" data-target="#tag-delete-confirm" onclick="TagDeleteConfirm(' + data + ',\'' + row.Name + '\')"><i class="fa fa-trash"></i></button>';
                return soption;
            }
        }]
    });
    $('#tags-table_filter').hide();
    if (jQuery().typeahead) {
        var substringMatcher = function () {
            return function findMatches(q, cb) {
                var matches, substringRegex;

                // an array that will be populated with substring matches
                matches = [];

                // regex used to determine if a string contains the substring `q`
                substrRegex = new RegExp(q, 'i');

                // iterate through the pool of strings and for any string that
                // contains the substring `q`, add it to the `matches` array
                $.each(tagsTypeaHead, function (i, str) {
                    if (substrRegex.test(str)) {
                        matches.push(str);
                    }
                });

                cb(matches);
            };
        };
        TagLoadDataTypeaHead();
        $('.typeaheadtag').typeahead('destroy');
        $('.typeaheadtag').typeahead({
            hint: true,
            highlight: true,
            minLength: 1
        },
            {
                name: 'tags',
                source: substringMatcher()

            });
    };
}
function TagLoadDataTypeaHead() {
    $.get('/MyDesks/GetTagNameForTypeahead', { deskId: $('#hd_deskId').val() }, function (data) {
        tagsTypeaHead = data;
    }, 'json');
}
function TagLoadEdit(id, name, isEdit) {
    if (isEdit) {
        $('#tag-add h5.modal-title').text("Edit Tag");
        $('#tag-add button.btn-success').text("Confirm");
    } else {
        $('#tag-add h5.modal-title').text("Add a Tag");
        $('#tag-add button.btn-success').text("Add now");
    }
    //clear error mesg
    $('#frm_tag_addedit input.valid').removeClass("valid");
    $('#frm_tag_addedit input.error').removeClass("error");
    $("#frm_tag_addedit label.error").hide();
    $("#frm_tag_addedit label.valid").hide();

    $('#tagId').val(id);
    $('#hd_deskId').val($("#MyDeskId").val());
    $('#tagName').val(name);
}
function TagDeleteConfirm(id, name) {
    $('#tag_deleteId').val(id);
    $('#label-confirm-tag').html('Do you want delete tag : <strong>' + name + '</strong>');
}
function TagDelete() {
    $.ajax({
        url: "/MyDesks/DeleteTag",
        data: { tagId: $('#tag_deleteId').val() },
        type: "GET",
        dataType: "json",
        beforeSend: function (xhr) {
            isBusy = true;
            $.LoadingOverlay("show");
        },
    }).done(function (refModel) {
        if (refModel.result) {
            cleanBookNotification.success(_L("ERROR_MSG_138"), "Tags");
            $tagsTable.DataTable().ajax.reload();
            TagLoadDataTypeaHead();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_139"), "Tags");
        }
        isBusy = false;
        LoadingOverlayEnd();
    }).fail(function () {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Tags");
        isBusy = false;
        LoadingOverlayEnd();
    });
}
function TagSearchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 1000);
    };
}
function LoadFilterByTab() {
    $('#frmFilterMyDesk').load("");
}
function LoadCommunityTab() {
    $('#tabs-community li a[data-target=#community-contacts]').trigger("click");
    loadUserContactData();
}
function LoadTaskMoreModal(taskKey) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadTaskMoreModal",
        data: {
            taskKey: taskKey
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#task-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadEventMoreModal(eventKey) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadEventMoreModal",
        data: {
            eventKey: eventKey
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#event-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadLinkMoreModal(linkId) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadLinkMoreModal",
        data: {
            linkId: linkId
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#link-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadMediaMoreModal(mediaKey) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadMediaMoreModal",
        data: {
            mediaKey: mediaKey
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#media-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadDiscussionMoreModal(discussionId) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadDiscussionMoreModal",
        data: {
            discussionId: discussionId
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#discussion-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadProcessMoreModal(processKey) {
    $.LoadingOverlay("show");
    $.ajax({
        type: "GET",
        url: "/MyDesks/LoadProcessMoreModal",
        data: {
            processKey: processKey
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#process-more').html(refModel.Object.strResult);
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
};
