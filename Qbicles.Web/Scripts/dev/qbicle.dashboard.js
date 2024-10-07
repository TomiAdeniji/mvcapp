//init emoji
initEmoji('#post-text');
//Qbicle Settings Dashboard v2
var loadCountActivity = 0;
var previous_shown = false;
var $isFilter = false;
var isServerBusy = false;
var popUp = false;
var tab = 1;
var isFirstLoad = 0;
var _calYear = null;
var _calMonth = null;

function SetTopic(topicName) {
    if (topicName === "") {
        topicName = "General";
    }
    setCookie("Qbicle-" + getCookieClient("CurrentQbicleId"), topicName);
};



function addTopicToFilter(topicId, topicName) {
    //Topic Filter
    if ($('#select-topic option[value="' + topicId + '"]').length <= 0) {
        $('#select-topic').append($('<option>', {
            value: topicId,
            text: topicName
        })).select2({ placeholder: 'Please select' });
        $('#select-topic').on('change.select2', function (e) {
            LoadDataDashboard(true);
        });
    }
    //Topic QbicleStream
    if ($('#toppic-value option[value="' + topicId + '"]').length <= 0) {
        $('#toppic-value').append($('<option>', {
            value: topicId,
            text: topicName
        })).select2();
    }

};

function SetTabIndex(tabindex) {
    tab = tabindex;
    if (tabindex == 1)
        loadUserContactData();
}
function Timeline() {
    $(window).scrollTop(0);
    $(window).scroll(function () {
        if ($(window).scrollTop() >= ($(document).height() - $(window).height() - 100) && $("ul.community-v2-nav li.active.qbicle-home").length > 0) {
            if (previous_shown == false && !popUp) {
                loadingNew();
                setTimeout(function () {
                    LoadMoreActivities();
                }, 100);

                previous_shown = true;
                return previous_shown;
            }
        }
    });
}
function LoadDataDashboard(isFilter) {
    if (isFilter) {
        $(window).scrollTop(0);
        loadCountActivity = 0;
        isFirstLoad = 0;
        $('#dashboard-page-display').hide();
        $('#first-load-icon').show();
    }
    setTimeout(function () {
        LoadMoreActivities(isFilter);
    }, 100);
}
function loadingNew() {
    if ($('#previous div.text-center'))
        $('#previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
}
function showQbicleStream() {
    $('#first-load-icon').hide();
    $("#latch").show();
    $('#dashboard-page-display').show();
}
function LoadMoreActivities(isFilter) {
    if (isServerBusy) {
        return;
    }
    var _activityTypes = $("#select-activity").val();
    var _topicIds = $("#select-topic").val()
    var _apps = $("#AppTypes").val();
    var url = "/Qbicles/LoadMoreActivities/";
    $.ajax({
        url: url,
        data: {
            size: loadCountActivity * qbiclePageSize,
            ActivityTypes: _activityTypes ? _activityTypes : [],
            TopicIds: _topicIds ? _topicIds : [],
            Apps: _apps ? _apps : []
        },
        type: "POST",
        async: false,
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (data) {
            $('#previous').empty();
            if (isFirstLoad == 0) {
                isFirstLoad = 1;
                if ($('#first-load-icon:visible').length > 0) {
                    showQbicleStream();
                    $(window).scrollTop(0);
                }
            }
            if (data.length !== 0) {
                //Convert Unicode emoji to image
                data.ModelString = convertUnicodeEmojiToImg(data.ModelString);
                if (isFilter) {
                    $("#dashboard-page-display").html('');
                    $("#dashboard-page-display").append('<div id="previous"></div>');
                    $(data.ModelString).insertBefore("#previous").fadeIn(250);
                }
                else {
                    $(data.ModelString).insertBefore("#previous").fadeIn(250);
                }
                var $dayfirstdate = $('#dashboard-date-today');
                if ($dayfirstdate.length > 0) {
                    $("#dashboard-date-today .day-date").first().addClass("day-date-first");
                    $dayfirstdate.addClass("day-block-first");
                }
                RemoveDom();
            }
            else {
                if (isFilter) {
                    $("#dashboard-page-display").html('');
                    $("#dashboard-page-display").append('<div id="previous"></div>');
                }
                previous_shown = true;
            }

            if (data.ModelCount) {
                var ajaxModelCount = data.ModelCount - (loadCountActivity * qbiclePageSize);
                if (ajaxModelCount <= 0)
                    previous_shown = true;
                else
                    previous_shown = false;
            }
            isServerBusy = false;
        },
        error: function (xhr, status, error) {
            isServerBusy = false;
            showQbicleStream();
        }
    });
    loadCountActivity = loadCountActivity + 1;
};
function SearchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 500);
    };
}
function ApplyFilter() {
    $isFilter = true;
    loadCountActivity = 0;
    $("#dashboard-page-display").empty();
    $("#dashboard-page-display").html("<div id='previous'></div>");
    LoadMoreActivities(true);
};
function ClearFilter() {
    $("#select-topic").val('').trigger("change");
    $("#select-activity").val('').trigger("change");
    loadCountActivity = 0;
    $("#dashboard-page-display").empty();
    $("#dashboard-page-display").html("<div id='previous'></div>");
    LoadMoreActivities(true);
};
function CalPinned(Id, IsPost) {

    var url = "/MyDesks/PinnedActivity/";
    $.ajax({
        url: url,
        data: { ActivityId: Id, IsPost: IsPost },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                var elfunc = $('#textPin-' + Id);
                if (elfunc.text() == " Pin this") {
                    elfunc.text(" Unpin this");
                } else {
                    elfunc.text(" Pin this");
                }
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });

}
function CalFilterReset() {
    $('#cal-filter-orderby').val($("#cal-filter-orderby option:first").val()).trigger('change');
    $('#cal-filter-activitytype').val('').trigger('change');
    $('#cal-filter-topic').val('').trigger('change');
    $('#cal-filter-status').val(0).trigger('change');
    $('#cal-filter-people').val('').trigger('change');
    $('#cal-filter-apps').val('').trigger('change');
}
function GetTotalPageByCal(targetID, currentDate, type) {

    $('#options-calendar').LoadingOverlay("show");
    $('#activity-today').LoadingOverlay("show");
    $('#activity-week').LoadingOverlay("show");
    $('#activity-month').LoadingOverlay("show");
    var data = $('#frm-calendar-filters').serializeArray();
    data.push({ name: "keyword", value: $('#calendar input[name=search]').val() });
    data.push({ name: "type", value: type });
    data.push({ name: "pageSize", value: calPageSize })
    data.push({ name: "pageIndex", value: 0 })
    var currentDate = $('#calendar input[name=day]').val();
    if (currentDate != null)
        data.push({ name: "day", value: currentDate });
    $.ajax({
        type: "POST",
        url: "/Qbicles/GenerateCalendarActivities",
        data: data,
        async: true,
        success: function (refModel) {
            $("#paginateTemplate").css("display", "none");
            $('#calendar .tab-content article').remove();
            var totalRecord = refModel.Object.totalRecord;
            if (totalRecord > 0) {
                initPagination(targetID, currentDate, type, totalRecord);
            } else {
                $('#activity-today').LoadingOverlay("hide", true);
                $('#activity-week').LoadingOverlay("hide", true);
                $('#activity-month').LoadingOverlay("hide", true);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        $('#options-calendar').LoadingOverlay("hide", true);
    });
}

function CalLoadContent(targetID, currentDate, type, currentPage) {
    
    var data = $('#frm-calendar-filters').serializeArray();
    data.push({ name: "keyword", value: $('#calendar input[name=search]').val() });
    data.push({ name: "type", value: type });
    data.push({ name: "pageSize", value: calPageSize })
    data.push({ name: "pageIndex", value: currentPage })
    var currentDate = $('#calendar input[name=day]').val();
    if (currentDate != null)
        data.push({ name: "day", value: currentDate });

    $.ajax({
        type: "POST",
        url: "/Qbicles/GenerateCalendarActivities",
        data: data,
        success: function (refModel) {
            if (refModel.result) {
                $('#calendar .tab-content article').remove();
                $(targetID + ' .records-h').prepend(refModel.Object.strResult);
                $("#paginateTemplate").css("display", "block");
            }
        }
    }).always(function () {
        $('#activity-today').LoadingOverlay("hide", true);
        $('#activity-week').LoadingOverlay("hide", true);
        $('#activity-month').LoadingOverlay("hide", true);
    });
}
function CalTabActive(Year, Month) {
    var mParams = { Year: Year, Month: Month };
    $('#calendar [data-target=#activity-today]').trigger('click');
    $.getJSON("/Qbicles/LoadDotActivities", mParams, function (data) {
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
                    html: date.getDate() + datedot.color
                }
            }
        }
        var calendar = $('.calendar-view-dashboard').datepicker().data('datepicker');
        calendar.update('onRenderCell', cellFunc);
    });
}
function CalApplyFilter() {

    var targ = $("#calendar ul.activity-tabs li.active a");
    var targetID = targ.attr("data-target");
    var type = targetID.replace("#activity-", "");
    var templatePagination = '<br><div id="paginateTemplate" style="display: none"></div ><div class="clearfix"></div>';
    $(".records-h").html('');
    $(targetID + ' .records-h').html(templatePagination);
    GetTotalPageByCal(targetID, null, type);
}
function CalSearchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 600);
    };
}

var $folderId = 0;

$('#create-media').on('shown.bs.modal', function (e) {
    $('#add-media-select-media-folder').val($folderId).trigger('change');    
})
function QbicleLoadMediasByFolder(id) {
    $('#media').LoadingOverlay("show");
    if (id > 0)
        $folderId = id;
    $.ajax({
        type: 'post',
        url: '/Qbicles/LoadMediasByFolderId',
        datatype: 'json',
        data: { folderId: $folderId, name: $("#qbicles-media-filter").val(), fileType: $("#qbicles-media-file-type").val() },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#media .media-contents');
                $divcontain.empty();
                $divcontain.html(listMedia);
                totop();

                $('#media').LoadingOverlay("hide");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            $('#media').LoadingOverlay("hide");
        }
    });
}
function QbicleReloadMediaFolder(isReloadMedias) {
    $.getJSON("/Medias/LoadFoldersByQbicle", { search: "" }, function (data) {
        if (data && data.length > 0) {
            $('#add-media-select-media-folder').empty();
            $('#add-media-select-media-folder').select2({
                data: data
            });
            if (isReloadMedias)
                QbicleLoadMediasByFolder($folderId);
        }
    });
}
function QbicleResetMediaFormFolder() {
    $('#frm-folder-qbicle input[name=title]').val('');
}
function QbicleEditMediaFolder(id, name) {
    $folderId = id;
    /*var data_select = $('#media-folder-qbicle').select2('data')[0];*/
    $('#frm-manager-folder-qbicle input[name=title]').val(name);
    $('#frm-manager-folder-qbicle input[name=folderId]').val(id);
    if (name == "General") {
        $('#media-btn-del-folder').hide();
        $('#media-btn-del-confirm').hide();
    } else {
        $('#media-btn-del-folder').show();
        $('#media-btn-del-confirm').show();
    }
}
function QbicleAddMediaFolder() {
    if (!$("#frm-folder-qbicle").valid())
        return;
    $.LoadingOverlay("show");
    var media = {
        mediaFolderId: 0,
        mediaFolderName: $('#frm-folder-qbicle input[name=title]').val()
    }
    $.ajax({
        type: 'post',
        url: '/Qbicles/InsertOrUpdateMediaFolder',
        datatype: 'json',
        data: media,
        success: function (refModel) {
            if (refModel.result) {

                var newFolder = "<li id='folder-li-" + refModel.Object.Id + "'>";
                newFolder += '<a href="#" onclick="ChangeIconFolder(this);QbicleLoadMediasByFolder(' + refModel.Object.Id + ');" data-toggle="tab">';
                newFolder += '<i class="folder fa fa-folder"></i> &nbsp;<span id="folder-name-' + refModel.Object.Id + '">' + refModel.Object.Name + '</span>';
                newFolder += "<button onclick='QbicleEditMediaFolder(" + refModel.Object.Id + ", \"" + refModel.Object.Name + "\");' class='btn btn-transparent' data-toggle='modal' data-target='#manage-folder'><i class='fa fa-ellipsis-h'></i></button>";
                newFolder += '</a>';
                newFolder += '</li>';
                $("#qbicle-media-folder li:last").before(newFolder);


                $('#create-folder').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_87"), "Medias");
                QbicleReloadMediaFolder(null);
                QbicleResetMediaFormFolder();
            } else if (!refModel.result && refModel.msg) {
                cleanBookNotification.error(_L(refModel.msg), "Medias");
            } else {
                $("#frm-folder-qbicle").validate().showErrors({ title: refModel.msg });
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function QbicleManagerMediaFolder() {
    if (!$("#frm-manager-folder-qbicle").valid())
        return;
    $.LoadingOverlay("show");
    mFolder = {
        mediaFolderId: $('#frm-manager-folder-qbicle input[name=folderId]').val(),
        mediaFolderName: $('#frm-manager-folder-qbicle input[name=title]').val()
    };
    $.ajax({
        type: 'post',
        url: '/Qbicles/InsertOrUpdateMediaFolder',
        datatype: 'json',
        data: mFolder,
        success: function (refModel) {

            if (refModel.result) {
                $("#folder-name-" + mFolder.mediaFolderId).text(mFolder.mediaFolderName);
                $('#manage-folder').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_88"), "Medias");
                QbicleReloadMediaFolder(null);
            } else if (!refModel.result && refModel.msg) {
                cleanBookNotification.error(_L(refModel.msg), "Medias");
            } else {
                $("#frm-manager-folder-qbicle").validate().showErrors({ title: refModel.msg });
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function QbicleDeleteMediaFolder() {
    $.LoadingOverlay("show");
    var folderId = $('#frm-manager-folder-qbicle input[name=folderId]').val();
    $.ajax({
        type: 'post',
        url: '/Qbicles/DeleteMediaFolderById',
        datatype: 'json',
        data: {
            mFolderId: folderId
        },
        success: function (refModel) {
            if (refModel.result) {
                $("#folder-li-" + folderId).remove();
                $('#manage-folder').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_89"), "Medias");
                QbicleReloadMediaFolder(true);
            } else {
                $("#form-update-del-media-folder").validate().showErrors({ title: refModel.msg });
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}
function QbicleLoadMoveMediaFolders(cFolderId, cMediaId) {
    $('#media-move-currentKey-qbicle').val(cMediaId);
    $.getJSON("/Medias/LoadMoveFoldersByQbicle", { cFolderId: cFolderId }, function (data) {
        if (data && data.length > 0) {
            $('#media-move-folders-qbicle').select2({
                data: data
            });
        }
    });
}
function QbicleSaveMoveMediaFolder() {
    $.LoadingOverlay("show");
    var mediaKey = $('#media-move-currentKey-qbicle').val();
    var folderId = $('#media-move-folders-qbicle').val();
    $.ajax({
        type: 'post',
        url: '/Medias/MediaMoveFolderById',
        datatype: 'json',
        data: {
            mediaKey: mediaKey,
            nFolderId: folderId
        },
        success: function (refModel) {
            if (refModel.result) {
                $('#move-media').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_90"), "Medias");
                QbicleLoadMediasByFolder($folderId);
            } else {
                cleanBookNotification.error(refModel.msg, "Medias");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            LoadingOverlayEnd();
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}



$(document).ready(function () {
    //Set active Tab
    var url_string = window.location.href;
    var url = new URL(url_string);
    var _querytab = url.searchParams.get("tab");
    if (_querytab) {
        //var _fid = url.searchParams.get("folder");
        //$('#media-folder-qbicle').val(_fid).change();
        $('ul.community-v2-nav a[data-target="#media"]').click();
    }
    $('#frm-folder-qbicle').validate(
        {
            rules: {
                title: {
                    required: true,
                    minlength: 5
                }
            }
        });
    $('#frm-manager-folder-qbicle').validate(
        {
            rules: {
                title: {
                    required: true,
                    minlength: 5
                }
            }
        });
    var $picker = $('.calendar-view-dashboard');
    $picker.datepicker({
        language: 'en',
        dateFormat: $dateFormatByUser.toLowerCase(),
        onSelect: function (formattedDate) {
            var $ele = $('#calendar [data-target="#activity-today"]');
            if (formattedDate == "" || $ele.attr("crd") === formattedDate)
                $ele.text("Today");
            else
                $ele.text(formattedDate);
            $('#calendar input[name=day]').val(formattedDate);
            $ele.trigger("click");
        },
        onChangeMonth: function (month, year) {
            var calendar = $('.calendar-view-dashboard').datepicker().data('datepicker');
            var date = calendar.currentDate;
            _calYear = date.getFullYear();
            _calMonth = date.getMonth() + 1;
            CalTabActive(_calYear, _calMonth)
        },
        onChangeView: function (view) {
            if (view == "days") {
                var calendar = $('.calendar-view-dashboard').datepicker().data('datepicker');
                var date = calendar.currentDate;
                _calYear = date.getFullYear();
                _calMonth = date.getMonth() + 1;
                CalTabActive(_calYear, _calMonth)
            }
        },
    });

    //Calendar load tab active
    $('#calendar [data-toggle="tab"]').click(function (e) {
        var $this = $(this),
            loadurl = $this.attr('href'),
            targ = $this.attr('data-target'),
            type = targ.replace("#activity-", "");
        var templatePagination = '<br><div id="paginateTemplate" style="display: none"></div ><div class="clearfix"></div>';
        $(".records-h").html('');
        $(targ + ' .records-h').html(templatePagination);
        GetTotalPageByCal(targ, null, type);
        $this.tab('show');
        return false;
    });
    $('#calendar input[name=search]').keyup(CalSearchThrottle(function () {
        // do the search if criteria is met
        CalApplyFilter();
    }));
    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: DBformatOptions,
        templateSelection: DBformatSelected,
        dropdownCssClass: 'withdrop'
    });
    Timeline();
    //$('#select-activity,#select-topic,#AppTypes').on('change.select2', function (e) {
    //    LoadDataDashboard(true);
    //});
    latchformobile();
    initForwardPostModal();
});

function DBformatOptions(state) {

    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function DBformatSelected(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
//End

function CallPopUpQbicMember() {
    popUp = true;
}
function ClosePopUpQbicMember() {
    popUp = false;
}
function UserDetail(userId, fullName) {

    if (tab == 1) {
        $('.contact-list').hide();
        $('.contact').fadeIn();
    }
    else {
        $('.contact-list-found').hide();
        $('.contact-invite').hide();
        $('.contact-add').hide();
        $('.contact-add').fadeIn();
    }
    $("#hdQbicMemberId").val(userId);
    $("#lblQbicMemberName").html(fullName);
    loadUserContactProfile(userId);
}
function ClosePopup(obj) {
    $("#" + obj).modal("hide");
}
function loadUserContactProfile(UserId) {
    if (tab === 1)
        $('#lblProfile').html('');
    else
        $('#lblProfileInvite').html('');
    $.ajax({
        type: "GET",
        url: "/Dashboard/LoadUserProfile",
        data: {
            UserId: UserId
        },
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                if (tab == 1) {
                    $('#lblProfile').html(refModel.Object.strResult);
                }
                else {
                    $('#lblProfileInvite').html(refModel.Object.strResult);
                    if (refModel.Object2) {
                        $("#btnInviteQbicMember").hide();
                        $("#btnInviteQbicMember").siblings('.added').show();
                    }
                    else {
                        $("#btnInviteQbicMember").show();
                        $("#btnInviteQbicMember").siblings('.added').hide();
                    }
                }
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}
function loadUserContactData() {
    var ulHeight = $("#lstDashboardContact").height();
    if (ulHeight == 0)
        ulHeight = '100%';
    else
        ulHeight = ulHeight + "px";
    $("#lstDashboardContact").css({ "height": ulHeight });
    $('#lstDashboardContact li').remove();
    $.ajax({
        type: "GET",
        url: "/Dashboard/DashboardLoadUserContact",
        data: {
            searchName: $("#txtSearchContact").val()
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#lstDashboardContact').html(refModel.Object.strResult);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}
function DeleteQbicMember() {
    $.LoadingOverlay("show");
    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        var url = "/Dashboard/RemoveQbicMember/";
        $.ajax({
            url: url,
            data: { userId: $("#hdQbicMemberId").val() },
            cache: false,
            type: "POST",
            async: false,
            success: function (refModel) {
                if (refModel.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_492", [$("#lblQbicMemberName").html()]));
                    ClosePopup("Qbic-member-remove");
                    $("#contact-list-back").click();
                    loadUserContactData();
                }
                else {
                    cleanBookNotification.error("Not Remove " + $("#lblQbicMemberName").html() + "!");
                }
                LoadingOverlayEnd();
            },
            error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}
function loadInviteUserContactData() {
    var ulHeight = $("#lstDashboardContact").height();
    if (ulHeight == 0)
        ulHeight = '100%';
    else
        ulHeight = ulHeight + "px";
    $("#lstDashboardInviteContact").css({ "height": ulHeight });
    $('#lstDashboardInviteContact li').remove();
    $('.existing-member').show();
    $.ajax({
        type: "GET",
        url: "/Dashboard/InviteLoadUserContact",
        data: {
            searchName: $("#txtSearchInviteContact").val()
        },
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                $('#lstDashboardInviteContact').html(refModel.Object.strResult);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}
function AddQbicMember() {
    $.LoadingOverlay("show");
    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        var url = "/Dashboard/AddMemberToQbicle/";
        $.ajax({
            url: url,
            data: { userId: $("#hdQbicMemberId").val() },
            cache: false,
            type: "POST",
            async: false,
            success: function (refModel) {
                if (refModel.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_494", [$("#lblQbicMemberName").html()]));
                    $("#btnInviteQbicMember").hide();
                    $("#btnInviteQbicMember").siblings('.added').show();
                }
                else {
                    cleanBookNotification.error("Not Add " + $("#lblQbicMemberName").html() + " to Qbicle!");
                }
                LoadingOverlayEnd();
            },
            error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}
function RemoveDom() {
    $(".day-block").each(function () {
        if ($(this).find("article").length == 0)
            $(this).remove();
    });
}
function latchformobile() {
    if ($(document).width() < 1200) {
        $('.interact').waypoint(function (direction) {
            if (direction == "down") {
                $('.interact').addClass('mobile-top');
                $('.block-container').addClass('compensate-sticky');
            }
            if (direction == "up") {
                $('.block-container').removeClass('compensate-sticky');
                $('.interact').removeClass('mobile-top');
            }
        }, { offset: '25%' });
    }
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
}

function initPagination(targetID, currentDate, type, totalRecord) {
    if (totalRecord != 0) {
        var container = $('#paginateTemplate');
        var sources = function () {
            var result = [];
            for (var i = 1; i <= totalRecord; i++) {
                result.push(i);
            }
            return result;
        }();

        var options = {
            prevText: 'Prev',
            nextText: 'Next',
            currentPage: 1,
            pageSize: calPageSize,
            dataSource: sources,
            callback: function (response, pagination) {
                CalLoadContent(targetID, currentDate, type, pagination.pageNumber - 1)
            }
        };
        container.pagination(options);
    }
}
function deletePost(elmId,key) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("WARNING_MSG_DELETEPOST"),
        callback: function (result) {
            if (result) {
                $.LoadingOverlay("show");
                $.post("/Posts/DeletePost", { key: key }, function (response) {
                    $.LoadingOverlay("hide");
                    if (response.result) {
                        $('#' + elmId).remove();
                        cleanBookNotification.success("Your post was successfully deleted.");
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error("There was an issue deleting this post. Please try again", "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                });
                return;
            }
        }
    });
    
}
function initForwardPostModal() {
    $('#forward-to-qbicle select.select2avatar').select2({
        placeholder: 'Please select',
        templateResult: formatOptions2,
        templateSelection: formatSelected2
    });
    $("#frmForwardPost").submit(function (event) {
        event.preventDefault();
        if ($(this).valid()) {
            var _paramaters = {
                key: $("#forward-to-qbicle input[name=PostKey]").val(),
                qbicleKey: $("#forward-to-qbicle select[name=destinationQbicle]").val()
            };
            $.post('/Posts/ForwardPost', _paramaters, function (response) {
                if (response.result) {
                    $('#forward-to-qbicle').modal('hide');
                    var _qbicleName = $("#forward-to-qbicle select[name=destinationQbicle] option:selected").text();
                    //This post has been added to the Qbicle <Qbicle name>
                    cleanBookNotification.success(_L("SUCCESS_MSG_ADDEDPOST", [_qbicleName]));
                } else if (!response.result && response.msg) {
                    cleanBookNotification.error(_L(response.msg), "Qbicles");
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else
            return;
        
    });
}
function formatOptions2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function formatSelected2(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + $(state.element).data("iconurl") + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function addPostToDiscuss(key) {
    $.LoadingOverlay("show");
    $.get("/Posts/GetMessageOfPost?key="+key, function (response) {
        $('#create-discussion-qb').modal('show');
        $('#discussion-summary').val(response.message);
        $.LoadingOverlay("hide");
    });
}
function showForwardPostModal(key) {
    $("#forward-to-qbicle input[name=PostKey]").val(key);
    $("#forward-to-qbicle select[name=destinationQbicle]").val(null).change();
    $('#forward-to-qbicle').modal('show');
}
function loadEditPostModal(elmId,key) {
    $('#edit-post').modal('show');
    $.LoadingOverlay("show");
    $("#edit-post").load("/Qbicles/GenerateEditPostModal", { postKey: key }, function () {
        $('#edit-post select[name=topic]').select2({ placeholder: 'Please select' });
        $("#frmEditPost").submit(function (event) {
            event.preventDefault();
            console.log('edit dashboard');
            if ($(this).valid()) {
                $.LoadingOverlay("show");
                var _paramaters = {
                    key: $('#frmEditPost input[name=PostKey]').val(),
                    message: $("#frmEditPost textarea[name=postcontent]").val(),
                    topicId: $("#frmEditPost select[name=topic]").val()
                };
                $.post('/QbicleComments/UpdatePost', _paramaters, function (response) {
                    if (response.result) {
                        $('#edit-post').modal('hide');
                        var topicname = $("#frmEditPost select[name=topic] option:selected").text();
                        $('#' + elmId + ' .topic-label').html('<span class="label label-info">' + fixQuoteCode(topicname) + '</span>');
                        _paramaters.message = convertUnicodeEmojiToImg(_paramaters.message);
                        $('#' + elmId + ' .activity-overview p').html(_paramaters.message);
                        $('#' + elmId + ' .post-event').text("Edited post");
                        cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Qbicles");
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                    $.LoadingOverlay("hide");
                });
            } else
                return;

        });
        $('#edit-post textarea[name=postcontent]').emojioneArea({
            inline: false,
            pickerPosition: "bottom",
            autocomplete: true
        });
        $.LoadingOverlay("hide");
    });
}

function convertUnicodeEmojiToImg(val) {

    //Convert Unicode emoji to image
    try {
        if (jQuery().emojioneArea && typeof emojione !== 'undefined') {
            return emojione.unicodeToImage(val);
        } else
            return val;
    } catch {
        return val;
    }
}


function ShowMediaFolders(target, firstFolderId) {

    var ajaxUri = '/Qbicles/ShowMediaFolderPanelOnDashboard?folderId=' + firstFolderId;
    
    $('#media-folder-div').empty();
    $('#media-folder-div').load(ajaxUri, function () {
        totop();
        manage_options('#options-media');
        $('.qbicle-overview').hide();
        $('.qbicle-snippet').fadeIn();
        QbicleLoadMediasByFolder(firstFolderId);

        $('.sidebar-options').hide();
        $(target).show();
    });

    
}