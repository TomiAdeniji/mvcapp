var _b2cQbiceId = "0";
var loadCountActivity = 0;
var isFirstLoad = 0;
var previousShown = false;
var isBusy = false;

var $totalConnected = 0;

$(document).ready(function () {
    timeline();
    initSearch();

    initB2CQbicleEventClick();
    triggerClickB2CQbicleActive(false);
    initPlugin();

    //get data in this, if empty then hiden add/filter..
    $totalConnected = document.getElementById("b2c-connected").getElementsByTagName("li").length;
    if ($totalConnected > 0)
        $('.connected-action').show();
    else
        $('.connected-action').hide();

});
function initPlugin() {
    var _format = $dateFormatByUser.toUpperCase();
    var currentDate = moment($('#txtFilterDaterange').data("maxdate"), _format).toDate();
    $('.daterange').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        maxDate: currentDate,
        locale: {
            cancelLabel: 'Clear',
            format: _format
        }
    });
    $('.daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
    });

    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
    });
}
function initSearch() {
    //Filters B2C Qbicle
    $('#filters-contacts input[name=search]').keyup(delay(function () {
        _b2cQbiceId = 0;
        loadB2CQbicleContent(true);
    }, 500));
    $('#filters-contacts select[name=orderBy]').change(function () {
        loadB2CQbicleContent();//not reset b2cQbiceId because only orderby
    });
    $('#connection-shown-type li').click(function () {
        if (!$(this).hasClass("active")) {
            _b2cQbiceId = 0;
            $('#connection-shown-type li').removeClass("active");
            $(this).addClass("active");

            loadB2CQbicleContent(true);
        }
    });
    //end
}
function loadB2CQbicleContent(eventclick) {
    var paramaters = {
        keyword: $('#filters-contacts input[name=search]').val(),
        orderby: $('#filters-contacts select[name=orderBy]').val(),
        typeShown: $('#connection-shown-type .active').attr('connectiontype'),
        b2cQbiceKey: _b2cQbiceId,
    };
    $("#dashboard-page-display").html('');
    $('#dashboard-page-display').LoadingOverlay('show');
    $('.widget-contacts').empty();
    $('.widget-contacts').LoadingOverlay('show');


    $('.widget-contacts').load("/B2C/LoadB2CQbiclesContent", paramaters, function () {
        initB2CQbicleEventClick();
        updateB2CConnectionTypeNum();
        $('.widget-contacts').LoadingOverlay('hide');
        $('#dashboard-page-display').LoadingOverlay('hide');

        if (eventclick)
            $('ul.widget-contacts > li.active > a').click();
        else
            $("ul.widget-contacts > li:first-child > a").click();
    });
}

function updateB2CConnectionTypeNum() {
    var _url = "/B2C/GetB2CConnectionNumberByTypes";
    $.ajax({
        method: 'GET',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            $("#all-connection-tab a").text("All (" + response.NonBlockedConnectionNumber + ")");
            $("#new-connection-tab a span label").text(response.NewConnectionNumber);
            $("#blocked-connection-tab a").text("Blocked (" + response.BlockedConnectionNumber + ")")
        }
    })
}
var $commonName = "";

function initB2CQbicleEventClick() {

    $('ul.widget-contacts > li > a').click(function (e) {

        $('ul.widget-contacts li').removeClass('active');
        $(this).closest('li').addClass('active');
        $('html').scrollTop(0);


        $('.b2bcommsmain').show();
        var $itemactive = $('ul.widget-contacts li.active');
        _b2cQbiceId = $itemactive.data("b2cqbicleid");
        $('#hdfCurrentQbicleId').val(_b2cQbiceId);
        var status = $itemactive.data("status");
        if (status == 'Approved') {
            $('.b2c-not-blocked').show();
        } else if (status == 'Blocked') {
            $('.b2c-not-blocked').hide();
        }
        $commonName = $itemactive.data("forename");
        $.ajax({
            type: 'post',
            url: '/B2C/CheckExistB2cOrders',
            data: { qbicleKey: _b2cQbiceId },
            dataType: 'json',
            success: function (response) {
                if (response == true) {
                    $('.order-context-flyout-div').show();
                    //$(".order-context-flyout-div").attr('data-tooltip', 'Orders');
                }
            },
            error: function (er) {

            }
        }).always(function () {
        });

        updateAsViewedConnection(_b2cQbiceId);
        loadDataDashboardB2B(true);
        viewProfileByB2CQbicle($itemactive);

    });

}
/**
 * 
 * @param {any} isFirst = true or false
 */
function triggerClickB2CQbicleActive(isFirst) {
    if (isFirst)
        $("ul.widget-contacts > li:first-child > a").click();
    else
        $('ul.widget-contacts > li.active > a').click();

    updateAsViewedConnection($('#hdfCurrentQbicleId').val());

}
function updateAsViewedConnection(key) {
    $('.order-context-flyout-div').hide();
    $.post("/B2C/SetB2BConnectionViewedStatus", { key: key }, function (response) {
        if (response.result) {
            var newCount = _.toNumber($("#count").text()) - 1;
            $("#count").text(newCount);

            $('li[data-b2cqbicleid="' + key + '"] span.newnots').remove();
            $('li[data-b2cqbicleid="' + key + '"] .comms-newstuff').attr("hidden", "hidden");
        }
    }).done(function () {
        if ($("#new-connection-tab").hasClass("active")) {
            $("#all-connection-tab").click();
        }
    });
}
//type=1: Businesses, type=2: Individual
function setStatusBy(key, status) {
    $.post("/B2C/SetStatusBy", { key: key, status: status }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            loadB2CQbicleContent();
            if ($('#hdfCurrentQbicleId').val() == key) {

                triggerClickB2CQbicleActive(true);
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
//type=1: Businesses, type=2: Individual
function viewProfileByB2CQbicle(elm) {
    var forename = elm.data("forename");
    $('span.txt-fullname').text(forename);
}
function blockB2CContact(key, name) {
    var r = confirm('Are you sure you want to block ' + name + '?');
    if (r == true) {
        setStatusBy(key, 'Blocked');
    }
}
function updateManagers() {
    $.post("/B2C/UpdateRelationshipManagers", { key: _b2cQbiceId, UserIds: $('#slrelationshipmanagers').val() }, function (response) {
        if (response.result) {
            $('#b2c-managers').modal('hide');
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            delay(function () {
                loadModalActivities();
            }, 50);

        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function removeB2CQbicleById(key, name) {
    var r = confirm('Are you sure you want to remove ' + name + '?');
    if (r !== true) return;

    $.post("/B2C/RemoveB2CQbicleById", { key: key }, function (response) {
        if (response.result) {
            _b2cQbiceId = "0";
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            loadB2CQbicleContent(true);
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function loadStatusInfo() {
    var $dashboardpagedisplay = $("#dashboard-page-display");
    //$commswaitingapprove.empty();
    $dashboardpagedisplay.load("/B2C/LoadB2CQbicleStatusInfo", { key: _b2cQbiceId }, function () {
        //$commswaitingapprove.LoadingOverlay('hide');
    });
}
function loadModalOrderCreation() {
    var $b2corderadd = $("#b2c-order-add");
    $b2corderadd.load("/B2C/LoadOrderCreationContent", function () {
        $("#b2c-order-add .select2").select2({ placeholder: "Please select" });
        var $frmordercreation = $('#frmordercreation');
        $frmordercreation.submit(function (e) {
            e.preventDefault();
            if ($frmordercreation.valid()) {

                isDisplayFlicker(true);
                $.LoadingOverlay("show");
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    data: $(this).serialize(),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $b2corderadd.modal('hide');

                            if (data.msg != '') {
                                isDisplayFlicker(false);
                                htmlActivityRender(data.msg, 0);
                            }
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
    });
}
//B2C Qbicle stream
function timeline() {
    $(window).scrollTop(0);
    $(window).scroll(function () {
        if ($(window).scrollTop() >= ($(document).height() - $(window).height() - 100) && $("ul.subapps-nav li.active.tab-activities").length > 0) {
            if (previousShown == false) {
                loadingNewB2B();

                previousShown = true;
                return previousShown;
            }
        }
    });
    var $elm = $(".contained-sidebar");
    $elm.css("overscroll-behavior", "contain");
}
function loadMoreActivitiesB2B(isFilter) {

    if (isBusy || $totalConnected <= 0) {
        return;
    }
    var _activityTypes = [];
    var _topicIds = [];
    var _apps = [];
    if ($('#select-activity').val() != "0")
        _activityTypes.push($('#select-activity').val());

    var fillterModel = {
        Key: _b2cQbiceId,
        Size: loadCountActivity * qbiclePageSize,
        ActivityTypes: _activityTypes,
        TopicIds: _topicIds,
        Apps: _apps,
        Daterange: $('#txtFilterDaterange').val()
    };
    var url = "/B2C/LoadMoreActivities";
    $.ajax({
        url: url,
        data: {
            fillterModel: fillterModel
        },
        type: "POST",
        async: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;

            if (data.isHidden) {
                _b2cQbiceId = "0";
                loadB2CQbicleContent(true);
                cleanBookNotification.warning(_L('WARNING_MSG_REMOVECONTACT'), "Community");
                return;
            }
            if (isFirstLoad == 0) {
                loadModalActivities();
                isFirstLoad = 1;
                if ($('#first-load-icon:visible').length > 0) {
                    showQbicleStream();
                    $(window).scrollTop(0);
                }
            }
            if (data.length !== 0) {
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
                removeDom();
            }
            else {
                if (isFilter) {
                    $("#dashboard-page-display").html('');
                    $("#dashboard-page-display").append('<div id="previous"></div>');
                }
                previousShown = true;
            }

            if (data.ModelCount) {
                var ajaxModelCount = data.ModelCount - (loadCountActivity * qbiclePageSize);
                if (ajaxModelCount <= 0)
                    previousShown = true;
                else
                    previousShown = false;
            }
            //QBIC-2064: Remove Forward Option in post management("Discuss, Edit and Delete only.")
            $('.op-forward').remove();
        },
        error: function (xhr, status, error) {
            isBusy = false;
            showQbicleStream();
        }
    }).always(function () {
        isBusy = false;
        $('#first-load-icon').hide();
    });
    loadCountActivity = loadCountActivity + 1;
};
function showQbicleStream() {
    $('#first-load-icon').hide();
    $("#latch").show();
    $('#dashboard-page-display').show();
}
function loadingNewB2B() {
    if ($('#previous div.text-center'))
        $('#previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
    setTimeout(function () {
        loadMoreActivitiesB2B();
    }, 500);
}
function loadDataDashboardB2B(isFilter) {
    if (isFilter) {
        $(window).scrollTop(0);
        loadCountActivity = 0;
        isFirstLoad = 0;
        $('#dashboard-page-display').hide();
        $('#first-load-icon').show();
    }
    setTimeout(function () {
        loadMoreActivitiesB2B(isFilter);
    }, 200);
}
function removeDom() {
    $("#comms-activities .day-block").each(function () {
        if ($(this).find("article").length == 0)
            $(this).remove();
    });
}
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
function resetFilters() {
    $('.removefilters').hide();
    $('#select-activity').val('0').trigger('change');
    $('#txtFilterDaterange').val('');
    loadDataDashboardB2B(true);
}



function loadModalActivities() {
    var $b2citem = $('ul.widget-contacts li.active');
    if ($b2citem.length > 0 && $b2citem.data('status') == 'Approved') {
        $b2citem.find('.counter').css("display", "none").text(0);
        $('#modal-activities').load("/B2C/LoadModalActivities", function () {
            $('#modal-activities select.select2').not('select.select2-hidden-accessible').select2({
                placeholder: 'Please select'
            });
            $('#modal-activities input[data-toggle="toggle"]').bootstrapToggle();
            if ($('#b2c-managers').length > 0)
                $('li.b2c-relationship-manager').show();
            else
                $('li.b2c-relationship-manager').hide();
            sharemenu();
        });
    } else {
        $('#modal-activities').empty();
    }
}
function initNextPreviousTab(frmId, tabId) {
    $(frmId + ' .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').next('li').find('a').trigger('click');
    });

    $(frmId + ' .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').prev('li').find('a').trigger('click');
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
function sharemenu() {
    var $frmShareMenu = $('#frmShareMenu');
    $frmShareMenu.validate({
        ignore: "",
        rules: {
            location: {
                required: true
            },
            menu: {
                required: true,
            },
            description: {
                required: true,
                minlength: 3,
                maxlength: 500
            }
        }
    });
    $frmShareMenu.submit(function (e) {
        e.preventDefault();

        if ($frmShareMenu.valid()) {
            isDisplayFlicker(true);

            $.LoadingOverlay("show");
            var paramaters = {
                MenuId: $('#frmShareMenu select[name=menu]').val(),
                OpeningComment: $('#frmShareMenu textarea[name=description]').val()
            };
            $.post(this.action, paramaters, function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    $('#b2c-menu-add').modal('hide');

                    if (data.msg != '') {
                        htmlActivityRender(data.msg, 0);
                        isDisplayFlicker(false);
                    }

                    cleanBookNotification.success(_L("ERROR_MSG_398"), "Qbicles");
                    $frmShareMenu.trigger("reset");
                    $('#frmShareMenu select[name=location]').val('').trigger('change');
                    $('#frmShareMenu select[name=menu]').val('').trigger('change');
                    $frmShareMenu.validate().resetForm();
                } else if (!data.result && data.msg) {
                    cleanBookNotification.error(_L(data.msg), "Qbicles");
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            });
        }
    });
}
function loadMenusByLocationId() {

    if (_.toNumber($("#menu-add-location-select").val()) <= 0)
        return;

    var $menu = $('#frmShareMenu select[name=menu]');
    $.get("/B2C/GetMenusByLocationId?lid=" + $("#menu-add-location-select").val(), function (data) {
        $menu.select2('destroy');
        $menu.empty();
        $menu.select2({
            placeholder: "Please select",
            data: data
        });
    });
}
function applyFilters() {
    $('#filter-b2b-stream').modal('hide');
    loadDataDashboardB2B(true);
}
function deletePost(elmId, key) {
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
function addPostToDiscuss(key) {
    $.LoadingOverlay("show");
    $.get("/Posts/GetMessageOfPost?key=" + key, function (response) {
        $('#create-discussion-qb').modal('show');
        $('#discussion-summary').val(response.message);
        $.LoadingOverlay("hide");
    });
}
function loadEditPostModal(elmId, key) {
    $('#edit-post').modal('show');
    $.LoadingOverlay("show");
    $("#edit-post").load("/Qbicles/GenerateEditPostModal", { postKey: key }, function () {
        $('#edit-post select[name=topic]').select2({ placeholder: 'Please select' });
        $("#frmEditPost").submit(function (event) {
            event.preventDefault();

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
                        $('#' + elmId + ' .activity-overview p').text(_paramaters.message);
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
        $.LoadingOverlay("hide");
    });
}
//End B2C Qbicle