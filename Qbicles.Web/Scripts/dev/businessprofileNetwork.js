var isBusy = false;
var recipes = [];
var dataVehicles = [];
var BKAccount = { Id: 0, Name: "" };
var tabs = {
    isprofile1load: false,//b2b-profile-1
    isprofile2load: false,//b2b-profile-2
    istradingitemsload: false,//trading-items
    istradingpricingload: false,//trading-pricing
    islogisticsgeneral: false,//logistics-general
    islogisticscharges: false,//logistics-charges
    islogisticsvehicles: false,//logistics-vehicles
    islogisticsdrivers: false,//logistics-drivers
    istradingmenuload: false,//trading-pricing = showing catalog type sales
    iscatalogdimenstionload: false//trading-pricing = showing catalog type Distribution
};
var tradinggeneraltabs = {
    istablocationsload: false,
    istabworkgroupsload: false,
    istabcandbload: false,
    istabcurrencyload: false,
    istabgroupsload: false,
    istabcontactsload: false,
    istabb2bload: false,
    istabb2cload: false,
    istabposload: false,
    istabstorefrontload: false,
};
var tradinggrouptabs = {
    istabgroupsitemsload: false,
    istabgroupscontactsload: false
};
$(document).ready(function () {
    //initFormProfile();
    reloadPosts(true, false);
    initFormPost();
    initNavClick();
    var _tabactive = getQuerystring('tab');
    if (_tabactive == 'general-contacts') {
        $('a[href=#trading-general]').click();
        setTimeout(function () {
            $('a[href=#' + _tabactive + ']').click();
        }, 500);
    } else if (_tabactive == 'trading-menus') {
        $('a[href=#trading-menus]').click();
    } else if (_tabactive == 'general-candb') {
        $('a[href=#trading-general]').click();
        setTimeout(function () {
            $('a[href=#' + _tabactive + ']').click();
        }, 500);
    } else if (_tabactive == 'trading-menus-distribution') {
        $('a[href=#trading-menus-distribution]').click();
    }
    initEventStoreProfile();
    $('.select2tag').select2({
        tags: true
    })
});
function changeavatar(target) {
    var fileOstoreImg = document.getElementById("commerce-logo").files;
    var objectKey = $("#commerce-logo-object-key").val();
    if (!objectKey) {
        if (fileOstoreImg && fileOstoreImg.length > 0) {
            UploadMediaS3ClientSide("commerce-logo").then(function (mediaS3Object) {
                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    LoadingOverlayEnd();
                    var api = $('#api-uri').val();
                    $("#commerce-logo-object-key").val(mediaS3Object.objectKey);
                    $(target).css('background-image', "url('" + api + mediaS3Object.objectKey + "&size=T')");
                    saveProfileB2B();
                }
            });
        }
    } else {
        saveProfileB2B();
    }
}
//function initFormProfile() {
//    var $frmB2BProfile = $('#frmB2BProfile');
//    $frmB2BProfile.validate({
//        ignore: "",
//        rules: {
//            BusinessName: {
//                required: true,
//                maxlength: 150
//            },
//            BusinessSummary: {
//                required: true,
//                maxlength: 500
//            }
//        }
//    });
//    $frmB2BProfile.submit(function (e) {
//        e.preventDefault();
//        if (isBusy)
//            return;
//        if ($frmB2BProfile.valid()) {
//            $.LoadingOverlay("show");
//            var fileOstoreImg = document.getElementById("commerce-logo").files;
//            var fileOstoreHerroImg = document.getElementById("commerce-banner-image").files;

//            if (fileOstoreImg && fileOstoreImg.length > 0 && fileOstoreHerroImg && fileOstoreHerroImg.length > 0) {
//                Promise.all([UploadMediaS3ClientSide("commerce-logo"), UploadMediaS3ClientSide("commerce-banner-image")]).then(mediaS3Object => {
//                    $("#commerce-logo-object-key").val(mediaS3Object[0].objectKey);
//                    $("#commerce-banner-object-key").val(mediaS3Object[1].objectKey);
//                    SaveProfileB2B();
//                }).catch(reason => {
//                    LoadingOverlayEnd();
//                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
//                    return;
//                });
//            }
//            else if (fileOstoreImg && fileOstoreImg.length > 0 && fileOstoreHerroImg.length == 0) {
//                UploadMediaS3ClientSide("commerce-logo").then(function (mediaS3Object) {

//                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
//                        LoadingOverlayEnd();
//                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
//                        return;
//                    }
//                    else {
//                        $("#commerce-logo-object-key").val(mediaS3Object.objectKey);
//                        SaveProfileB2B();
//                    }
//                });
//            }
//            else if (fileOstoreImg.length == 0 && fileOstoreHerroImg && fileOstoreHerroImg.length > 0) {
//                UploadMediaS3ClientSide("commerce-banner-image").then(function (mediaS3Object) {

//                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
//                        LoadingOverlayEnd();
//                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
//                        return;
//                    }
//                    else {

//                        $("#commerce-banner-object-key").val(mediaS3Object.objectKey);
//                        SaveProfileB2B();
//                    }
//                });
//            }
//            else
//                SaveProfileB2B();
//        }
//    });
//}
function initNavClick() {
    $('ul.nav-stacked li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {
                case "#b2b-profile-1":
                    reloadPosts(true, tabs.isprofile1load);
                    tabs.isprofile1load = true;
                    break;
                case "#b2b-profile-2":
                    reloadPosts(false, tabs.isprofile2load);
                    tabs.isprofile2load = true;
                    break;
                case "#trading-general":
                    $('a[href="#general-locations"]').trigger('click');
                    break;
                case "#trading-items":
                    loadTabContent(elid, tabs.istradingitemsload);
                    tabs.istradingitemsload = true;
                    break;
                case "#trading-pricing":
                    loadTabContent(elid, tabs.istradingpricingload);
                    tabs.istradingpricingload = true;
                    break;
                case "#trading-menus":
                    loadTabContent(elid, tabs.istradingmenuload);
                    tabs.istradingmenuload = true;
                    break;
                case "#logistics-general":
                    initPluginsOfTabGarenal(tabs.islogisticsgeneral);
                    tabs.islogisticsgeneral = true;
                    break;
                case "#logistics-charges":
                    getLocationsDomain($('#logistics-charges select[name=locations]'), tabs.islogisticscharges);
                    loadContentPriceList();
                    initFormClonePriceList(tabs.islogisticscharges);
                    tabs.islogisticscharges = true;
                    break;
                case "#logistics-vehicles":
                    initPluginsOfTabVehicles(tabs.islogisticsvehicles);
                    tabs.islogisticsvehicles = true;
                    break;
                case "#logistics-drivers":
                    getLocationsDomain($('#logistics-drivers select[name=locations]'), tabs.islogisticsdrivers);
                    initPluginsOfTabDrivers(tabs.islogisticsdrivers);
                    tabs.islogisticsdrivers = true;
                    break;
                case "#trading-menus-distribution":
                    loadTabContent(elid, tabs.iscatalogdimenstionload);
                    tabs.iscatalogdimenstionload = true;
                    break;
            }
        }, 200);
    });
    $('ul.trading-general-tabs li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            var currentUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
            var tabName = elid.replace("#", "");
            var newUrl = currentUrl + "?tab=" + tabName;
            window.history.pushState({ path: newUrl }, '', newUrl);

            switch (elid) {
                case "#general-locations":
                    loadTabContent(elid, tradinggeneraltabs.istablocationsload);
                    tradinggeneraltabs.istablocationsload = true;
                    break;
                case "#general-workgroups":
                    loadTabContent(elid, tradinggeneraltabs.istabworkgroupsload);
                    tradinggeneraltabs.istabworkgroupsload = true;
                    break;
                case "#general-candb":
                    loadTabContent(elid, tradinggeneraltabs.istabcandbload);
                    tradinggeneraltabs.istabcandbload = true;
                    break;
                case "#general-currency":
                    loadTabContent(elid, tradinggeneraltabs.istabcurrencyload);
                    tradinggeneraltabs.istabcurrencyload = true;
                    break;
                case "#general-groups":
                    loadTabContent(elid, tradinggeneraltabs.istabgroupsload);
                    tradinggeneraltabs.istabgroupsload = true;
                    break;
                case "#general-b2b":
                    loadTabContent(elid, tradinggeneraltabs.istabb2bload);
                    tradinggeneraltabs.istabb2bload = true;
                    break;
                case "#general-b2c":
                    loadTabContent(elid, tradinggeneraltabs.istabb2cload);
                    tradinggeneraltabs.istabb2cload = true;
                    break;
                case "#general-pos":
                    loadTabContent(elid, tradinggeneraltabs.istabposload);
                    tradinggeneraltabs.istabposload = true;
                    break;
                case "#general-storefront":
                    loadTabContent(elid, tradinggeneraltabs.istabstorefrontload);
                    tradinggeneraltabs.istabstorefrontload = true;
                    break;
                default:
                    loadTabContent(elid, tradinggeneraltabs.istabcontactsload);
                    tradinggeneraltabs.istabcontactsload = true;
                    break;
            }
        }, 200);
    });

    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#profile-managers input[name=search]').keyup(delay(function () {
        $('#tblManagers').DataTable().search($(this).val()).draw();
    }, 500));
    $('#profile-managers select[name=filtermanagers]').change(function () {
        var keyword = $("#profile-managers select[name=filtermanagers] option:selected").text();
        keyword = (keyword != "Show all" ? keyword : "");
        $('#tblManagers').DataTable().search(keyword).draw();
    });
    $('#tblManagers_filter').hide();
    $('#itemtype a').bind('click', function (e) {
        e.preventDefault();
        $('.wizard-start').hide();
        $('#productorservice li').removeClass('active');
        $('.theformtype').hide();
        $('#itemtype li').removeClass('active');
        $(this).closest('li').addClass('active');
        var itemtype = $(this).data('value');
        $('#sltype').val(itemtype);
        if (itemtype != 'icompound') {
            $('#prodorserv').fadeIn();
            //$('#formulae-sell').hide();
        } else {
            $('#prodorserv').hide();
            //$('#formulae-sell').show();
            //$('#recipe').show();
        }

        if (itemtype == 'icompound') {
            $('.wizard-start').show();
        }
    });
    $('#productorservice a').bind('click', function (e) {
        e.preventDefault();
        $('#productorservice li').removeClass('active');
        $(this).closest('li').addClass('active');
        var prodorserv = $(this).data('value');
        $('#chkIsProduct').prop('checked', (prodorserv == 'service' ? false : true));
        $('.wizard-start').show();
        //if (prodorserv == 'service') {
        //    $('.inventorytab').hide();
        //    $('.finishonfirst').show();
        //    $('.normalfinish').hide();
        //    $('.barcode').hide();
        //} else {

        //    $('.barcode').show();
        //    $('.finishonfirst').hide();
        //    $('.normalfinish').show();
        //    $('.inventorytab').show();
        //}
    })
}
function initNavSubgroupsClick() {
    $('ul.nav-subgroups li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {

                case "#tab-groups-items":
                    loadTabContent(elid, tradinggrouptabs.istabgroupsitemsload);
                    tradinggrouptabs.istabgroupsitemsload = true;
                    break;
                default:
                    loadTabContent(elid, tradinggrouptabs.istabgroupscontactsload);
                    tradinggrouptabs.istabgroupscontactsload = true;
                    break;
            }
        }, 200);
    });
}
function saveProfileB2B() {
    var frmData = new FormData($('#frmB2BProfile')[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/Commerce/SaveProfileB2B",
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        dataType: "json",
        data: frmData,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
                $("#commerce-logo-object-key").val('');
                $('#hdfProfileId').val(returnJson.Object);
                $('div.social-links input, select[name=tags]').removeAttr('disabled');
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Commerce");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function initFormPost() {
    var $frmCommerePost = $('#frmCommerePost');
    $frmCommerePost.validate({
        ignore: "",
        rules: {
            Title: {
                required: true,
                maxlength: 150
            },
            Content: {
                required: true,
                maxlength: 500
            }
        }
    });
    $frmCommerePost.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmCommerePost.valid()) {
            $.LoadingOverlay("show");
            var files = document.getElementById("commerce-post-feature").files;
            if (files.length > 0) {
                UploadMediaS3ClientSide("commerce-post-feature").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#commerce-post-feature-object-key").val(mediaS3Object.objectKey);

                        SavePostB2B();
                    }
                });
            }
            else {
                $("#commerce-post-feature-object-key").val("");
                SavePostB2B();
            }
        }
    });
    $('#txtFilterFeaturedSearch').keyup(delay(function () {
        reloadPosts(true);
    }, 800));
    $('#txtFilterOtherSearch').keyup(delay(function () {
        reloadPosts(false);
    }));
    $('#frmB2BProfile input,#frmB2BProfile textarea').change(function () {
        $('#btnConfigCancel').attr('disabled', false);
    });
}
function SavePostB2B() {
    var frmData = new FormData($('#frmCommerePost')[0]);
    frmData.append("ProfileId", $('#frmB2BProfile input[name=Id]').val());
    $.ajax({
        type: "post",
        cache: false,
        url: "/Commerce/SavePostB2B",
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        dataType: "json",
        data: frmData,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                $('#app-commerce-post-add').modal('hide');
                reloadPosts($('#hdfIsFeatured').val() == 'true' ? true : false);
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Commerce");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function addArea() {
    var _htmlrow = '<tr class="add-area animated flipInY">';
    _htmlrow += '<td><input type="text" name="AreasOfOperation[]" maxlength="150" class="form-control" value="" placeholder="e.g. London" style="width: 100%;"></td>';
    _htmlrow += '<td><button type="button" class="btn btn-danger" onclick="$(this).parent().parent().hide();"><i class="fa fa-trash"></i></button></td>';
    _htmlrow += '</tr>';
    $('#tblAreas tbody').append(_htmlrow);
}
function initModalPost(id, isfeatured) {
    //Reset Modal
    $("#frmCommerePost").validate().resetForm();
    $('#hdfPostId').val(0);
    $('#hdfIsFeatured').val(isfeatured ? 'true' : 'false');
    $('#frmCommerePost input[name=Title]').val('');
    $('#frmCommerePost textarea[name=Content]').val('');
    $('#frmCommerePost input[name=FeaturedImage]').val('');
    if (id && id > 0) {
        $.get("/Commerce/GetPostById", { id: id }).done(function (response) {
            if (response.result) {
                var post = response.Object;
                $('#frmCommerePost input[name=Title]').val(post.Title);
                $('#frmCommerePost textarea[name=Content]').val(post.Content);
                $('#hdfPostId').val(post.Id);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
    }
    if (id && id > 0 && isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Edit Featured Post");
    } else if (id && id > 0 && !isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Edit Profile Post");
    } else if (!id && isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Add a Featured Post");
    } else {
        $('#app-commerce-post-add h5.modal-title').text("Add a Profile Post");
    }
    $('#app-commerce-post-add').modal('show');
}
function reloadPosts(isFeatured, firstload) {
    if (!firstload) {
        var _paramaters = {
            profileId: $('#frmB2BProfile input[name=Id]').val(),
            search: '',
            isfeatured: isFeatured
        };
        if (isFeatured) {
            _paramaters.search = $('#txtFilterFeaturedSearch').val();
            setTimeout(function () {
                var $content1 = $('.featured-posts');
                $content1.LoadingOverlay("show");
                $content1.load("/Commerce/LoadPostsContent", _paramaters, function () {
                    $content1.LoadingOverlay("hide");
                });
            }, 100);
        } else {
            _paramaters.search = $('#txtFilterOtherSearch').val();
            setTimeout(function () {
                var $content2 = $('#b2b-profile-2 .from-community');
                $content2.LoadingOverlay("show");
                $content2.load("/Commerce/LoadPostsContent", _paramaters, function () {
                    $content2.LoadingOverlay("hide");
                });
            }, 100);
        }
    }
}
function deletePost(id, isFeatured) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Commerce",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Commerce/DeletePostById", { id: id }, function (response) {
                    if (response.result) {
                        reloadPosts(isFeatured);
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                });
                return;
            }
        }
    });

}
function showIconProfileManager() {
    setTimeout(function () {
        var activedB2b = $('input[name=IsDisplayedInB2BListings]').prop('checked');
        var activedB2c = $('input[name=IsDisplayedInB2CListings]').prop('checked');
        if (!activedB2b && !activedB2c) {
            $('.managers').hide();
        } else
            $('.managers').show();
        if (activedB2b)
            $('.cell-b2b-action button').prop('disabled', false);
        else
            $('.cell-b2b-action button').prop('disabled', true);
        if (activedB2c)
            $('.cell-b2c-action button').prop('disabled', false);
        else
            $('.cell-b2c-action button').prop('disabled', true);
    }, 200);
}
//type:B2B||type:B2c
function showModalManagers(type, profileId, isEdit) {
    var textEdit = isEdit ? 'Edit' : 'Add';
    var $manager = $('#bprofile-add-managers select[name=managers]');
    var $actionconfirm = $('.btn-profileaction');
    if (type == 'B2B') {
        $('.modalprofile-type-text').text(type);
        $('#bprofile-add-managers .modal-title').text(textEdit + ' B2B default relationship managers');
        $manager.attr('onchange', 'bindManagersBusinessProfile(\'B2B\',this)');
        $actionconfirm.attr('onclick', 'updateDefaultManagers(\'B2B\',' + profileId + ')');
    } else {
        $('.modalprofile-type-text').text(type);
        $('#bprofile-add-managers .modal-title').text(textEdit + ' B2C default relationship managers');
        $manager.attr('onchange', 'bindManagersBusinessProfile(\'B2C\',this)');
        $actionconfirm.attr('onclick', 'updateDefaultManagers(\'B2C\',' + profileId + ')');
    }
    //Load data default managers
    if (profileId > 0) {
        $.get("/Commerce/LoadDefaultManagers?profileId=" + profileId, function (response) {
            if (type == 'B2B')
                $manager.val(response.defaultb2bmanagers);
            else
                $manager.val(response.defaultb2cmanagers);
            $manager.multiselect("refresh");
        });
    } else {
        $manager.val([]);
        $manager.multiselect("refresh");
    }
}
//type:B2B||type:B2c
function updateDefaultManagers(type, profileId) {
    var _valmanagerb2b = $('select[name=UserIdB2BRelationshipManagers]').val();
    var _valmanagerb2c = $('select[name=UserIdB2CRelationshipManagers]').val();
    var activedB2b = $('input[name=IsDisplayedInB2BListings]').prop('checked');
    var activedB2c = $('input[name=IsDisplayedInB2CListings]').prop('checked');
    var paramaters = {
        Id: profileId,
        IsDisplayedInB2BListings: activedB2b,
        IsDisplayedInB2CListings: activedB2c,
        UserIdB2BRelationshipManagers: _valmanagerb2b ? _valmanagerb2b : [],
        UserIdB2CRelationshipManagers: _valmanagerb2c ? _valmanagerb2c : []
    };
    if (profileId > 0) {
        $.LoadingOverlay("show");
        $.post("/Commerce/UpdateDefaultManagers", paramaters, function (response) {
            LoadingOverlayEnd();
            if (response.result) {
                $('.managers').hide();
                if (type == 'B2B') {
                    var _htmlCell = htmlManagersTemplate(response.Object.defaultb2bmanagers);
                    $('.cell-b2b-defaultmanagers').html(_htmlCell);
                    var _htmlButtonAction = '<button class="btn btn-warning" onclick="showModalManagers(\'B2B\',' + profileId + ',true)" data-toggle="modal" data-target="#bprofile-add-managers"><i class="fa fa-pencil"></i></button>';
                    $('.cell-b2b-action').html(_htmlButtonAction);
                } else {
                    var _htmlCell = htmlManagersTemplate(response.Object.defaultb2cmanagers);
                    $('.cell-b2c-defaultmanagers').html(_htmlCell);
                    var _htmlButtonAction = '<button class="btn btn-warning" onclick="showModalManagers(\'B2C\',' + profileId + ',true)" data-toggle="modal" data-target="#bprofile-add-managers"><i class="fa fa-pencil"></i></button>';
                    $('.cell-b2c-action').html(_htmlButtonAction);
                }
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Commerce");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        });
    } else {
        $('.managers').hide();
        cleanBookNotification.warning(_L('WARNING_MSG_SAVEBUSINESSPROFILE'), "Commerce");
    }
    $('#bprofile-add-managers').modal('hide');
}
function htmlManagersTemplate(managers) {
    var _htmlCell = 'None assigned yet.';
    if (managers.length > 0) {
        _htmlCell = '<ul style="padding-left: 15px;">';
        $.each(managers, function (index, value) {
            _htmlCell += ('<li>' + value + '</li>');
        });
        _htmlCell += '</ul>';
    }
    return _htmlCell;
}
function bindManagersBusinessProfile(type, elm) {
    var _managers = $(elm).val();
    if (type == 'B2B') {
        $('select[name=UserIdB2BRelationshipManagers]').val(_managers);
    } else
        $('select[name=UserIdB2CRelationshipManagers]').val(_managers);
}
function loadTabContent(tabid, isfirstload) {
    if (!isfirstload) {
        tabid = tabid.replace('#', '');
        var $content;
        switch (tabid) {
            case 'general-locations':
                $content = $('#general-locations .general-dynamiccontent');
                break;
            case 'tab-groups-items':
                $content = $('#tab-groups-items .groups-items-dynamiccontent');
                break;
            case 'tab-groups-contacts':
                $content = $('#tab-groups-contacts .groups-contacts-dynamiccontent');
                break;
            default:
                $content = $('#' + tabid);
                break;
        }
        $content.empty();
        if (tabid != 'general-candb')
            $content.LoadingOverlay("show");

        $content.load("/Commerce/LoadBusinessProfileTab", { tab: tabid }, function (response) {

            $content.LoadingOverlay("hide");
            overrideFunctions(tabid);
            $('.dataTables_filter').hide();
            html5tooltips.refresh();
        });
    }
}
function overrideFunctions(tabid) {
    switch (tabid) {
        case 'general-locations':
            //Override reloadLocation funtion in file trader.config.location.js
            reloadLocation = function (argument) {
                tradinggeneraltabs.istablocationsload = false;
                $('a[href="#general-locations"]').trigger('click');
            }
            //init event customize filters for Location tab
            $('#general-locations input[name=search]').keyup(delay(function () {
                $('#tbllocations').DataTable().search($(this).val()).draw();
            }, 500));
            $('#general-locations select[name=type]').change(function () {
                var keyword = $("#general-locations select[name=type]").val();
                switch (keyword) {
                    case "1":
                        keyword = "check";
                        break;
                    case "2":
                        keyword = "none";
                        break;
                    default:
                        keyword = "";
                        break;
                }
                $('#tbllocations').DataTable().search(keyword).draw();
            });
            break;
        case "general-workgroups":
            moveModalIntoBody('#' + tabid + ' .gp-modal-content');
            //Override ReloadWorkgroup funtion in file trader.config.workgroups.js
            ReloadWorkgroup = function () {
                var $content = $('.tbl-wg-content');
                $content.empty();
                $content.LoadingOverlay("show");
                $content.load("/Commerce/LoadBusinessProfileTab", { tab: 'general-workgroups', reload: true }, function (response) {
                    $("#wg-table").DataTable({
                        "destroy": true,
                        responsive: true,
                        "pageLength": 10
                    });
                    $content.LoadingOverlay("hide");
                });
            };
            //init event customize filters for Location tab
            $('#general-workgroups input[name=search]').keyup(delay(function () {
                $('#wg-table').DataTable().search($(this).val()).draw();
            }, 500));
            $('#processes-filter').change(function () {
                var $processes = $("#processes-filter option:selected");
                var selected = [];
                $($processes).each(function (index, brand) {
                    selected.push([$(this).text()]);
                });
                $('#wg-table').DataTable().search(selected.join('|'), true, false, true).draw();
            });
            break;
        case 'general-currency':
            initCurrency();
            //Override ReloadWorkgroup funtion in file trader.config.workgroups.js
            reloadAccounting = function () {
                var $content = $('.tbl-taxrates-content');
                $content.empty();
                $content.LoadingOverlay("show");
                $content.load("/Commerce/LoadBusinessProfileTab", { tab: 'general-currency', reload: true }, function (response) {
                    $("#tblTaxrates").DataTable({
                        "destroy": true,
                        responsive: true,
                        "pageLength": 10
                    });
                    $content.LoadingOverlay("hide");
                });
            };
            //init event customize filters for Taxes & Currency tab
            $('#general-currency input[name=search]').keyup(delay(function () {
                $('#tblTaxrates').DataTable().search($(this).val()).draw();
            }, 500));
            $('#general-currency select[name=transactiontype]').change(function () {
                var $transactiontypes = $("#general-currency select[name=transactiontype] option:selected");
                var selected = [];
                $($transactiontypes).each(function (index, brand) {
                    selected.push([$(this).text()]);
                });
                $('#tblTaxrates').DataTable().search(selected.join('|'), true, false, true).draw();
            });
            $('#general-currency select[name=type]').change(function () {
                var $types = $("#general-currency select[name=type] option:selected");
                var selected = [];
                $($types).each(function (index, brand) {
                    selected.push([$(this).text()]);
                });
                $('#tblTaxrates').DataTable().search(selected.join('|'), true, false, true).draw();
            });
            break;
        case 'trading-items':
            $("#search_dt").keyup(delay(function () {
                CallBackFilterDataItemOverViewServeSide();
            }, 1000));
            $('#itemoverview-filter-type').change(function () {
                applyItemOverviewFilter();
                CallBackFilterDataItemOverViewServeSide();
            });
            $('#itemoverview-filter-group').change(function () {
                applyItemOverviewFilter();
                CallBackFilterDataItemOverViewServeSide();
            });
            break;
        case 'general-contacts':
            moveModalIntoBody('#' + tabid + ' .ct-modal-content');
            $("#group-workgroup-id").select2();
            $("#group-contact-id").select2();
            $("#CountryName").select2();

            break;
        case 'general-groups':
            initNavSubgroupsClick();
            $('ul.nav-subgroups li a[href=#tab-groups-items]').click();
        case 'tab-groups-items':
            reloadgroup = function () {
                tradinggrouptabs.istabgroupsitemsload = false;
                $('ul.nav-subgroups li a[href=#tab-groups-items]').click();
                clickAddgroup();
            }
            $('#tab-groups-items input[name=search]').keyup(delay(function () {
                $('#tab-groups-items .dataTable').DataTable().search($(this).val()).draw();
            }, 500));
            break;
        case 'tab-groups-contacts':
            reloadTableContactGroup = function () {
                tradinggrouptabs.istabgroupscontactsload = false;
                tradinggeneraltabs.istabcontactsload = false;
                $('ul.nav-subgroups li a[href=#tab-groups-contacts]').click();
            }
            $('#tab-groups-contacts input[name=search]').keyup(delay(function () {
                $('#tab-groups-contacts .dataTable').DataTable().search($(this).val()).draw();
            }, 500));
            break;
        case 'trading-pricing':
            moveModalIntoBody('#' + tabid + ' .pr-modal-content');
            //Override addGroupTab funtion in file trader.config.mastersetup.js
            addGroupTab = function (groupid, groupname) {
                $("#tabs-pgroup").append(
                    "<li><a data-toggle=\"tab\" onclick='loadReloadPrices(" + groupid + ");' href='#group" + groupid + "'>" + groupname + "</a></li>"
                );
                $("#tabs-content").append(
                    "<div class=\"tab-pane fade\" id='group" + groupid + "'></div>"
                );
                $('#group' + groupid).load("/Commerce/LoadGroupConfigTab?groupid=" + groupid, function () {
                    $('#group' + groupid + ' select.select2').select2({ placeholder: 'Please select' });
                    $('#group' + groupid + ' .checkmulti').multiselect({
                        includeSelectAllOption: false,
                        enableFiltering: true,
                        buttonWidth: '100%',
                        maxHeight: 400,
                        enableClickableOptGroups: true
                    });
                    $("#tabs-pgroup a[href=#group" + groupid + "]").click();
                });
                $("#tabs-pgroup a[href=#group" + groupid + "]").tab("show");
            }
        case 'trading-menus':
            var catalogType = $("a[href='#" + tabid + "']").attr("catalogType");
            moveModalIntoBody('#' + tabid + ' .mn-modal-content');
            //Override SuccessAction funtion in file pos.products.js
            SearchMenu = function () {
                //Commerce/LoadPosMenu
                $('#trading-menus #catalog-distribution-list').remove();
                var $content = $('#pos-menu-list');
                $content.empty();
                $content.LoadingOverlay("show");
                $content.load("/PointOfSaleMenu/LoadPosMenu", { locationIds: $('#trading-menus select[name=locations]').val(), catalogSearchType: catalogType, salesChannel: $('#trading-menus select[name=salechannel]').val(), keyword: $('#trading-menus input[name=search]').val(), status: $('#trading-menus select[name=status]').val() }, function (response) {
                    $content.LoadingOverlay("hide");
                });
            };
            SearchMenu();
            resetValidateMenuForm = function () {
                $("#menu-name").removeClass('error valid');
                $("#pos-menu-form").validate().resetForm();
            };
            CreateMenu = function () {
                $menuAction = 1;
                $("#menu-name").val('');
                $("#menu-summary").val('');
                $("#pos-menu-form select[name=Locations]").prop("disabled", false);
                $("#menu-salechannel").prop("disabled", false);
                $("#menu-salechannel").val('B2C').change();
                $('#report_filters').val('');
                $('#report_filters').select2({ placeholder: 'Please select' });
                $("#menu-modal-title").text('Add a Catalog');
                resetValidateMenuForm();
                $menuId = 0;
                $('#app-trader-pos-menu-modal').modal('show');

                //set value for distribution saleChannel
                var catalogTypeElement = $(".nav-stacked > .active > a");
                var catalogType = 0;
                if (catalogTypeElement) {
                    catalogType = catalogTypeElement.attr('catalogType');
                }
                if (catalogType == "1") {
                    $("#menu-salechannel").val('B2B').change();
                    $("#menu-salechannel").attr("disabled", "disabled");

                    $(".display-locations").hide();
                    $(".display-report-filters").hide();
                } else {

                    $(".display-locations").show();
                    $(".display-report-filters").show();
                }
            };
            SuccessAction = function (rs) {
                var quickCatalogType = $(".nav-stacked > .active > a").attr('catalogType');
                if (quickCatalogType == '1') {
                    SearchCatalogDistribution();
                } else {
                    SearchMenu();
                }
            };
            //init plugin
            $('#trading-menus select.select2').select2({ placeholder: 'Please select' });
            $("#trading-menus select.checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
            $('.mn-modal-content select.select2').select2({ placeholder: 'Please select' });
            var timeout = null;
            $("#trading-menus input[name=search]").keyup(function () {
                clearTimeout(timeout);
                timeout = setTimeout(function () { SearchMenu(); }, 1000);
            });
            break;
        case 'trading-menus-distribution':
            var catalogType = $("a[href='#" + tabid + "']").attr("catalogType");
            moveModalIntoBody('#' + tabid + ' .mn-modal-content');
            //Override SuccessAction funtion in file pos.products.js
            SearchCatalogDistribution = function () {
                //Commerce/LoadPosMenu
                $('#trading-menus-distribution #pos-menu-list').remove();
                var $content = $('#catalog-distribution-list');
                $content.empty();
                $content.LoadingOverlay("show");
                $content.load("/PointOfSaleMenu/LoadPosMenu", { locationIds: $('#trading-menus-distribution select[name=locations]').val(), catalogSearchType: catalogType, salesChannel: $('#trading-menus-distribution select[name=salechannel]').val(), keyword: $('#trading-menus-distribution input[name=search]').val(), status: $('#trading-menus-distribution select[name=status]').val() }, function (response) {
                    $content.LoadingOverlay("hide");
                });
            };
            SearchCatalogDistribution();
            resetValidateMenuForm = function () {
                $("#menu-name").removeClass('error valid');
                $("#pos-menu-form").validate().resetForm();
            };
            CreateMenu = function () {
                $menuAction = 1;
                $("#menu-name").val('');
                $("#menu-summary").val('');
                $("#pos-menu-form select[name=Locations]").prop("disabled", false);
                $("#menu-salechannel").prop("disabled", false);
                $("#menu-salechannel").val('B2C').change();
                $('#report_filters').val('');
                $('#report_filters').select2({ placeholder: 'Please select' });
                $("#menu-modal-title").text('Add a Catalog');
                resetValidateMenuForm();
                $menuId = 0;
                $('#app-trader-pos-menu-modal').modal('show');

                //set value for distribution saleChannel
                var catalogTypeElement = $(".nav-stacked > .active > a");
                var catalogType = 0;
                if (catalogTypeElement) {
                    catalogType = catalogTypeElement.attr('catalogType');
                }
                if (catalogType == "1") {
                    $("#menu-salechannel").val('B2B').change();
                    $("#menu-salechannel").attr("disabled", "disabled");

                    $(".display-locations").hide();
                    $(".display-report-filters").hide();
                } else {

                    $(".display-locations").show();
                    $(".display-report-filters").show();
                }
            };
            SuccessAction = function (rs) {
                var quickCatalogType = $(".nav-stacked > .active > a").attr('catalogType');
                if (quickCatalogType == '1') {
                    SearchCatalogDistribution();
                } else {
                    SearchMenu();
                }
            };
            //init plugin
            $('#trading-menus-distribution select.select2').select2({ placeholder: 'Please select' });
            $("#trading-menus-distribution select.checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
            $('.mn-modal-content select.select2').select2({ placeholder: 'Please select' });
            var timeout = null;
            $("#trading-menus-distribution input[name=search]").keyup(function () {
                clearTimeout(timeout);
                timeout = setTimeout(function () { SearchCatalogDistribution(); }, 1000);
            });
            $('#trading-menus-distribution .filter-tab select').attr('onchange', 'SearchCatalogDistribution()');
            break;
        case 'general-candb':
            showTableCashBankValue();
            clearAllModal();
            break;
        default:
            break;
    }
}
//Cash & bank accounts
var $cashAccountId = 0;
var filter = { Workgroup: "", Key: "" };
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTableCashBank(); }, 200);
}
function searchOnTableCashBank() {
    var listKey = [];

    var keys = $('#search_dt_cb').val().split(' ');
    if ($('#search_dt_cb').val() !== "" && $('#search_dt_cb').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#community-list").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#community-list").val("");
}
function clearAllModal() {
    $("#cashbank-transfer").empty();
    $("#app-trader-cashbank").empty();
    $("#cashbank-payment").empty();
};
function AddEditTraderCashBank(id) {
    $.LoadingOverlay("show");
    clearAllModal();
    setTimeout(function () {
        $("#app-trader-cashbank").load("/TraderCashBank/TraderCashBankAddEdit?id=" + id);
        $.LoadingOverlay("hide");

        $("#app-trader-cashbank").modal("toggle");
    }, 2000);
}
function initBKAccount(id, name) {
    BKAccount.Id = id;
    BKAccount.Name = name;
}
function SaveCashAccount() {
    if (!$("#form_cash_bank").valid())
        return;

    //if (BKAccount.Id === 0) {
    //    cleanBookNotification.error("Please select an account!", "Qbicles");
    //    return;
    //}

    if ($("#cash-bank-icon").val() === "" && $("#cash-bank-id").val() === "0") {
        cleanBookNotification.error(_L("ERROR_MSG_618"), "Qbicles");
        return;
    }
    var cashBank = {
        Id: $("#cash-bank-id").val(),
        Name: $("#cash-bank-name").val()
    };

    $.ajax({
        type: "post",
        url: "/TraderCashBank/TraderCashAccountNameCheck",
        data: { cashBank: cashBank },
        dataType: "json",
        success: function (response) {
            if (response.result === false) {
                $("#form_cash_bank").validate().showErrors({ Name: response.msg });
                return;
            } else {
                ProcessCashBankAccount();
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            return;
        }
    });
}
function ProcessCashBankAccount() {
    $.LoadingOverlay("show");
    var files = document.getElementById("cash-bank-icon").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("cash-bank-icon").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#cash-bank-object-key").val(mediaS3Object.objectKey);
            $("#cash-bank-object-name").val(mediaS3Object.fileName);
            $("#cash-bank-object-size").val(mediaS3Object.fileSize);

            CashBankAccountSubmit();
        });

    } else
        CashBankAccountSubmit();
}
function CashBankAccountSubmit() {
    var frmData = new FormData($("#form_cash_bank")[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderCashBank/SaveTraderCashAccount",
        enctype: "multipart/form-data",
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
        },
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $("#app-trader-cashbank").modal("hide");
                setTimeout(function () {
                    cleanBookNotification.createSuccess();
                    showTableCashBankValue();
                }, 100);
            } else if (response.actionVal === 2) {
                $("#app-trader-cashbank").modal("hide");
                setTimeout(function () {
                    cleanBookNotification.updateSuccess();
                    showTableCashBankValue();
                }, 100);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
}
function UploadImage(id, type) {
    UploadMediaS3ClientSide("form_specifics_icon").then(function (mediaS3Object) {
        return mediaS3Object.objectKey;
    });
}
function manageonclick(id) {
    $.LoadingOverlay("show");
    window.location.href = '/TraderCashBank/TraderCashAccount?id=' + id + '&locationid=0'
}
function showTableCashBankValue() {
    $('#cashbank-content').LoadingOverlay("show");
    $('#cashbank-content').load("/TraderCashBank/TraderCashBankContents", function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#community-list-cash-bank').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#cashbank-content').LoadingOverlay("hide");
        setTimeout(function () {
            searchOnTableCashBank();
        }, 300);

    });
}
function LoadTableDataCashBank(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": "",
            "processing": loadingoverlay_value
        },
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keysearch: $('#search_dt_cb').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function FilterDataCashBankByServerSide() {
    var url = '/TraderCashBank/GetCashBankContent';
    var columns = [
        {
            name: "Image",
            data: "Image",
            render: function (value, type, row) {
                var str = '';
                str += '<div class="table-avatar" style="background-image: url(\'' + $("#api-uri").val() + row.Image + '&size=T\');">&nbsp;</div>';
                return str;
            }
        },
        {
            name: "Name",
            data: "Name",
            orderable: true,
            render: function (value, type, row) {
                var str = '';
                str += '<h5 style="margin: 0; padding: 0 0 3px 0;">' + row.Name + '</h5>';
                return str;
            }
        },
        {
            name: "BookkeepingAccount",
            data: "BookkeepingAccount",
            orderable: false
        },
        {
            name: "FundsIn",
            data: "FundsIn",
            orderable: false
        },
        {
            name: "FundsOut",
            data: "FundsOut",
            orderable: false
        },
        {
            name: "Charges",
            data: "Charges",
            orderable: false
        },
        {
            name: "Transactions",
            data: "Transactions",
            orderable: false
        },
        {
            data: null,
            orderable: false,
            render: function (value, type, row) {
                var str = '<div class="btn-group options">';
                str += '<button type="button" class="btn btn-success dropdown-toggle"  data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                str += '<i class="fa fa-cog"></i> &nbsp; Options </button>';
                str += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                str += '<li><a href="#" onclick="manageonclick(' + row.Id + ')">Manage</a></li>';
                if (row.BankmateType == 0)
                    str += '<li><a href="#" data-toggle="modal" onclick="AddEditTraderCashBank(' + row.Id + ')" >Edit</a></li>';
                str += '</ul> </div>';
                if (row.AllowEdit)
                    return str;
                else return "";
            }
        }
    ];
    LoadTableDataCashBank('community-list-cash-bank', url, columns, 1);
    CallBackFilterDataCashBankServeSide();
}
function CallBackFilterDataCashBankServeSide() {
    $("#community-list-cash-bank").DataTable().ajax.reload();
}
//End Cash & bank accounts
function moveModalIntoBody(selector) {
    var $source = $(selector);
    if ($source.length > 0) {
        $('body ' + selector).remove();
        $source.appendTo('body');
    }
}
//This is funtion for Cash & Bank tab
function initCurrency() {
    $("#frmCurrencyConfiguration").submit(function (event) {
        event.preventDefault();
        var cSymbol = $('select[name=CurrencySymbol]').val();
        var sDisplay = $('select[name=SymbolDisplay]').val();
        var dPlace = $('select[name=DecimalPlace]').val();
        $.ajax({
            type: 'post',
            url: this.action,
            data: {
                CurrencySymbol: cSymbol,
                SymbolDisplay: sDisplay,
                DecimalPlace: dPlace,
            },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
                    var _event = "resetSettings('" + cSymbol + "','" + sDisplay + "','" + dPlace + "')";
                    $('#btnCurrencyReset').attr("onclick", _event);
                    tabs.istradingitemsload = false;
                    tradinggeneraltabs.istabcandbload = false;
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(er.responseText, "Qbicles");
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    });
}
function resetSettings(cSymbol, sDisplay, dPlace) {
    $('select[name=CurrencySymbol]').val(cSymbol).trigger('change');
    $('select[name=SymbolDisplay]').val(sDisplay).trigger('change');
    $('select[name=DecimalPlace]').val(dPlace).trigger('change');
}
//type: ibuy,isell,ibuysell,icompound
function loadModalTradingItemAdd() {
    $('#wizard').hide();
    var $type = $('#sltype');
    var $isProduct = $('#chkIsProduct');
    $('#app-trader-inventory-item-add').empty();//fix conflict
    var $content = $('#theform');
    $content.fadeIn();
    $content.empty();
    $content.LoadingOverlay("show");
    $content.load("/Commerce/LoadTradingItemAdd", { type: $type.val(), isProduct: $isProduct.prop('checked') }, function (response) {
        $('#theform .select2').select2({ placeholder: 'Please select' });
        $("#theform .checkmulti").multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $content.LoadingOverlay("hide");
        initNextPreviousTab('#theform', '.app_subnav');
        initFormTradingItem();
    });
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
function initFormTradingItem() {
    recipes = [];
    var $frmTradingItem = $('#frmTradingItem');
    $frmTradingItem.validate({
        ignore: "",
        rules: {
            name: {
                required: true,
                maxlength: 50,
                minlength: 4
            },
            description: {
                required: true,
                maxlength: 200,
                minlength: 4
            },
            sku: {
                required: true,
                maxlength: 50
            },
            barcode: {
                maxlength: 50
            }
        },
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $frmTradingItem.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        var type = $('#sltype').val();
        if (type == 'icompound' && recipes.length === 0) {
            cleanBookNotification.error(_L("ERROR_MSG_628"), "Qbicles");
            $.LoadingOverlay("hide");
            return false;
        }
        if ($frmTradingItem.valid() && checkUniqueSKUandBarcode()) {
            $.LoadingOverlay("show");
            var fileImg = document.getElementById("form_specifics_icon").files;
            var imgobjkey = $("#itemimg-object-key").val();
            if (!imgobjkey || (fileImg && fileImg.length > 0)) {
                UploadMediaS3ClientSide("form_specifics_icon").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {

                        $("#itemimg-object-key").val(mediaS3Object.objectKey);
                        saveTradingItem();
                    }
                });
            }
            else
                saveTradingItem();
        }
    });
}

function saveTradingItem() {
    var trader_item = {
        Id: $('#traderItem_id').val(),
        Group: { Id: $("#frmTradingItem " + " select[name=groups]").val() },
        Name: $("#frmTradingItem " + " input[name=name]").val(),
        Description: $("#frmTradingItem " + " textarea[name=description]").val(),
        ImageUri: $("#itemimg-object-key").val(),
        Locations: [],
        Units: [],
        Barcode: $("#frmTradingItem " + " input[name=barcode]").val(),
        SKU: $("#frmTradingItem " + " input[name=sku]").val(),
        IsCompoundProduct: false,
        //update new model
        PrimaryVendors: [],
        InventoryDetails: [],
        ResourceDocuments: [],
        AdditionalInfos: [],
        AssociatedRecipes: [],
        VendorsPerLocation: [],
        PurchaseAccount: [],
        SalesAccount: [],
        TaxRates: [],
        InventoryAccount: [],
        IsCommunityProduct: false,
        IsBought: false,
        IsSold: false,
        IsActiveInAllLocations: true
    };
    var type = $('#sltype').val();
    var isProduct = $('#chkIsProduct').prop('checked');
    switch (type) {
        case 'isell':
            trader_item.IsSold = true;
            break;
        case 'ibuysell':
            trader_item.IsSold = true;
            trader_item.IsBought = true;
            break;
        case 'icompound':
            trader_item.IsSold = true;
            trader_item.IsBought = false;
            trader_item.IsCompoundProduct = true;
            break;
        default:
            trader_item.IsBought = true;
            break;
    }
    var purchase_taxrates = $('#sl_taxrate_purchase').val();
    if (purchase_taxrates) {
        for (var i = 0; i < purchase_taxrates.length; i++) {
            trader_item.TaxRates.push({ Id: purchase_taxrates[i] });
        }
    }
    var sale_taxrates = $('#sl_taxrate_sale').val();
    if (sale_taxrates) {
        for (var i = 0; i < sale_taxrates.length; i++) {
            trader_item.TaxRates.push({ Id: sale_taxrates[i] });
        }
    }
    if (recipes.length > 0) {
        for (var i = 0; i < recipes.length; i++) {
            trader_item.AssociatedRecipes.push(recipes[i]);
        }
    }
    if (isProduct) {
        var locationId = $('#local-manage-select').val();
        if (locationId) {
            trader_item.InventoryDetails.push({
                Location: { Id: locationId },
                MinInventorylLevel: $('#form_inventory_mininv').val().replace(/\,/g, ""),
                MaxInventoryLevel: $('#form_inventory_maxinv').val().replace(/\,/g, ""),
                CurrentInventoryLevel: 0
            });
        }
    }
    var _ivquantity = $('#ivquantity').val();
    var _ivunitcost = $('#ivunitcost').val();
    var createInventory = {
        ivquantity: _ivquantity ? _ivquantity : 0,
        ivunitcost: _ivunitcost ? _ivunitcost : 0
    };
    if ($('#unit_conversions tbody tr#add-unit-row').length > 0) {
        $('#unit_conversions tbody tr#add-unit-row').remove();
    }
    if ($('#unit_conversions tbody tr').length > 0 && $('#unit_conversions tbody tr td').length > 1) {
        var units = $('#unit_conversions tbody tr');
        if (units.length > 0) {
            var unitsConversions = [];
            for (var j = 0; j < units.length; j++) {
                var unitTemp = $($(units[j]).find('td.row_componentUnit input')).val();
                var uC = {
                    Id: $($(units[j]).find('td.row_name .unitIds')).val(),
                    Name: $($(units[j]).find('td.row_name .unitName')).val(),
                    IsBase: $($(units[j]).find('td.row_name p')).text() === 'true' ? true : false,
                    MeasurementType: $('#item_measure_type').val(),
                    Quantity: $($(units[j]).find('td.row_quantity')).text(),
                    QuantityOfBaseunit: $($(units[j]).find('td.row_name span')).text(),
                    IsActive: $(units[j]).find('td.row_active input')[0].checked,
                    //IsPrimary: $(units[j]).find('td.row_primary input')[0].checked,
                    ParentUnit: (unitTemp && unitTemp !== "") ? { Id: unitTemp } : null
                };
                unitsConversions.push(uC);
            }
            if (unitsConversions.length > 0) {
                for (var m = 0; m < unitsConversions.length; m++) {
                    unitsConversions[m].ParentUnit = selectedChild(unitsConversions[m], unitsConversions)
                }

                for (var k = 0; k < unitsConversions.length; k++) {
                    var strId = unitsConversions[k].Id.toString();
                    if (isNaN(parseInt(unitsConversions[k].Id.toString()))) {
                        if (trader_item.AssociatedRecipes && trader_item.AssociatedRecipes.length > 0) {
                            for (var z = 0; z < trader_item.AssociatedRecipes.length; z++) {
                                if (trader_item.AssociatedRecipes[z].Ingredients && trader_item.AssociatedRecipes[z].Ingredients.length > 0)
                                    for (var n = 0; n < trader_item.AssociatedRecipes[z].Ingredients.length; n++) {
                                        if (trader_item.AssociatedRecipes[z].Ingredients[n].Unit.Id === unitsConversions[k].Id) {
                                            trader_item.AssociatedRecipes[z].Ingredients[n].Unit.Id = int;

                                        }
                                    }
                            }
                        }
                        unitsConversions[k].Id = int;
                        for (var l = 0; l < unitsConversions.length; l++) {
                            if (k !== l && unitsConversions[l].Id === strId) {
                                unitsConversions[l].Id = int;
                            }
                        }
                        int--;
                    }
                }
                trader_item.Units = unitsConversions;
            }
        }
    }
    $.ajax({
        type: 'post',
        url: '/TraderItem/SaveItemProduct?currentLocationId=' + 0 + '&isCurrentLocation=' + false,
        data: { item: trader_item, createInventory: createInventory },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#wizard-add-item').modal('hide');
                CallBackFilterDataItemOverViewServeSide(true);
            }
            else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function checkUniqueSKUandBarcode() {
    var sKU = $('#frmTradingItem input[name=sku]').val();
    var barcode = $('#frmTradingItem input[name=barcode]').val();
    var validateObj = $("#frmTradingItem").validate();
    var valid = true;
    if (!sKU) {
        valid = false;
        validateObj.showErrors({ sku: "The Product SKU is required!" });
    }
    if (sKU) {
        $.ajax({
            method: "POST",
            data: { traderId: $('#traderItem_id').val(), SKU: sKU, Barcode: barcode },
            url: "/TraderItem/validateUniqueSKUandBarcode",
            dataType: "json",
            async: false
        }).done(function (data) {
            if (data.sku) {
                valid = false;
                validateObj.showErrors({ sku: "The Product SKU already exists!" });
            }
            if (data.barcode) {
                valid = false;
                validateObj.showErrors({ barcode: "The Product Barcode already exists!" });
            }
            $('a[href="#item-details"]').click();
        });
    }
    return valid;
}
function filterTradingItemByServerSide() {
    var url = '/Trader/GetDataItemOverView';
    var _isSubscribe = $('#trading-items').attr("SubscribeTrader");
    var columns = [
        {
            data: "ImageUri",
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                str += '<a href="' + apiDoc + row.ImageUri + '" class="table-avatar image-pop" rel="resources" style="display: block; background-image: url(\'' + apiDoc + row.ImageUri + '&size=T\');">&nbsp;</a>';
                return str;
            }
        },
        {
            name: "ItemName",
            data: "ItemName",
            orderable: true,
            render: function (value, type, row) {
               
                return row.ItemName;
            }
        },
        {
            name: "SKU",
            data: "SKU",
            orderable: true
        },
        {
            name: "Barcode",
            data: "Barcode",
            orderable: true
        },
        {
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                if (row.IsBought) {
                    str += '<span class="label label-lg label-warning">Item I buy</span> ';
                }
                if (row.IsSold) {
                    str += '<span class="label label-lg label-success">Item I sell</span> ';
                }
                if (row.IsCompoundProduct) {
                    str += '<span class="label label-lg label-primary">Compound</span> ';
                }
                return str;
            }
        },
        {
            name: "GroupName",
            data: "GroupName",
            orderable: true
        },
        {
            name: "Description",
            data: "Description",
            orderable: false
        },
        {
            name: "Vendor",
            data: "Vendor",
            orderable: false
        },
        {
            orderable: false,
            render: function (value, type, row) {
                var str = '';
                if (_isSubscribe)
                    str += '<button class="btn btn-info" title="Subscribe to Trader to unlock this feature" data-toggle="modal" onclick="showTraderItemAdditional(\'' + row.Id + '\')" data-target="#app-trader-item-additional"><i class="fa fa-list"></i> &nbsp; View</button>';
                else
                    str += '<button class="btn btn-info" title="Subscribe to Trader to unlock this feature" disabled><i class="fa fa-list"></i> &nbsp; View</button>';
                return str;
            }
        },
        {
            name: "IsActive",
            data: "IsActive",
            orderable: true,
            render: function (value, type, row) {
                var str = '';
                if (row.IsActive) {
                    str += '<label><i class="fa fa-check green" style="width: 15px;"></i> &nbsp; Active</label>';
                } else {
                    str += '<label><i class="fa fa-remove red" style="width: 15px;"></i> &nbsp; Inactive</label>';
                }
                return str;
            }
        },
        {
            data: null,
            orderable: false,
            width: "100px",
            render: function (value, type, row) {
                var str = '';
                str += '<div class="btn-group options">';
                str += '<button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                str += '<i class="fa fa-cog"></i> &nbsp; Options </button>';
                str += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';

                if (row.IsBought) {
                    str += '<li><a href="javascript:void(0)" onclick="editTraderItem(1, ' + row.Id + ', \'item-tab\')">Edit</a></li>';
                } else {
                    str += '<li><a href="javascript:void(0)" onclick="editTraderItem(2, ' + row.Id + ', \'item-tab\')">Edit</a></li>';
                }
                //str += '<li><a href="javascript:void(0)">Delete</a></li></ul></div>';
                return str;
            }
        }
    ];
    LoadTableDataItemOverView('tb_trader_items', url, columns);
    CallBackFilterDataItemOverViewServeSide(false);
}
function initIVCreateModal() {
    var $itemname = $("#frmTradingItem " + " input[name=name]");
    if (!$itemname.valid()) {
        $("#app-trader-inventory-create").modal("hide");
        $('a[href="#item-details"]').click();
        return;
    }
    $('.iv-item-name').text($itemname.val());
    var trbaseunit = $("#unit_conversions tr td.row_name p:contains('true')");
    if (trbaseunit.text() === 'true') {
        var unitname = trbaseunit.closest('tr').find('td.row_name .unitName').val();
        if (unitname) {
            $('.iv-unit-base-name').text(unitname);
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_700"), "Qbicles");
            return;
        }

    } else {
        cleanBookNotification.error(_L("ERROR_MSG_700"), "Qbicles");
        return;
    }
    //var _txtLocation = $('#local-manage-select option:selected').text();
    //$('.iv-locations').text(_txtLocation);
    $('#frmCreateInventory').validate(
        {
            rules: {
                ivquantity: {
                    required: true,
                    min: 0
                },
                ivunitcost: {
                    required: true,
                    min: 0
                }
            }
        });
    $("#frmCreateInventory").submit(function (event) {
        event.preventDefault(); //prevent default action 
        if ($('#frmCreateInventory').valid()) {
            var val = parseFloat($('#ivquantity').val());
            $('#form_inventory_instock').val(val);
            $("#app-trader-inventory-create").modal("hide");
        } else {
            $('#ivquantity').val(0);
            $('#ivunitcost').val(0);
        }
    });
    $("#app-trader-inventory-create").modal("show");
}
function saveOverNewRecipe(tbid) {
    if (validateRecipeForm(tbid)) {
        // get item recipe
        var recipeName = "";
        var recipeId = 0;
        var isCurrent = false;
        if (tbid === 'tb_add_recipe') {
            recipeName = $('#form_add_recipe_name').val();
            recipeId = int;
            isCurrent = $('#form_add_recipe_iscurrent')[0].checked;
            int--;
        }
        else if (tbid === 'tb_edit_recipe') {
            recipeName = $('#edit_recipe_name').val();
            recipeId = $('#edit_recipe_form_id').val();
            isCurrent = $('#edit_recipe_iscurrent')[0].checked;
        }
        var recipeItem = {
            Id: recipeId,
            Name: recipeName,
            Ingredients: [],
            IsActive: true,
            IsCurrent: isCurrent
        }

        var trnews = $('#' + tbid + ' tbody tr');
        if (trnews.length > 0) {
            for (var i = 0; i < trnews.length; i++) {
                if (!$($(trnews[i]).find('td.item_name select')).val()) {
                    $($(trnews[i]).find('td.item_name select')).val('');
                }
                recipeItem.Ingredients.push({
                    Id: $($(trnews[i]).find('td.item_name input')).val(),
                    SubItem: { Id: $($(trnews[i]).find('td.item_name select')).val().split('|')[0] },
                    Quantity: $($(trnews[i]).find('td.item_quantity input')).val(),
                    Unit: { Id: $($(trnews[i]).find('td.item_selected select')).val() },
                    AverageCost: $($(trnews[i]).find('td.item_averagecost span')).text(),
                    LatestCost: $($(trnews[i]).find('td.item_latestcost span')).text()
                });
            }
        }
        addObjectToRecipeTable(recipeItem);
        $('#newrecipe').hide();
        $('#editrecipe').hide();
    }
}
function validateRecipeForm(tbid) {
    var valid = true;
    if (tbid === 'tb_add_recipe') {
        if ($("#form_add_recipe_name").val().trim() === "") {
            valid = false;
            $("#frmTradingItem").validate().showErrors({ recipe_name: "Recipe name is required." });
        }
    } else {
        if ($("#edit_recipe_name").val().trim() === "") {
            valid = false;
            $("#frmTradingItem").validate().showErrors({ recipe_name_edit: "Recipe name is required." });
        }
    }
    //Validate Recipe Items
    var rows = $('#' + tbid + " tr");
    for (var i = 0; i < rows.length; i++) {
        var FieldName = $(rows[i]).find("td.item_name select");
        if (FieldName.val() === "") {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_623"), "Recipe items");
            break;
        }
        var FieldQuantity = $(rows[i]).find("td.item_quantity input");
        if (FieldQuantity.val() <= 0) {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_624"), "Recipe items");
            break;
        }
        var FieldUnit = $(rows[i]).find("td.item_selected select");
        if (FieldUnit.val() === "") {
            valid = false;
            cleanBookNotification.error(_L("ERROR_MSG_625"), "Recipe items");
            break;
        }
    }
    //End Validate Recipe
    return valid;
}
function checkInventoryChooseBP(el) {
    $(el).valid();
    var value = $(el).val();
    if (value) {
        $('.iv-locations').text($(el).find(":selected").text());
        $('.btnaddinventory').prop('disabled', false);
    }
    else
        $('.btnaddinventory').prop('disabled', true);
}
//B2B Logistics
function initPluginsOfTabGarenal(isFirstLoad) {
    if (!isFirstLoad) {
        getLocationsDomain($('#logistics-general select[name=locations]'), isFirstLoad);
        $('#logistics-general #select-traderitems').select2({
            ajax: {
                url: '/TraderItem/Select2TraderItemsByLocationId',
                delay: 250,
                data: function (params) {
                    var query = {
                        keyword: params.term,
                        page: params.page || 1,
                        locationId: $('#logistics-general select[name="locations"]').val()
                    }
                    return query;
                },
                cache: true
            },
            minimumInputLength: 1
        });
    }
}
function initPluginsOfTabVehicles(isFirstLoad) {
    if (!isFirstLoad) {
        $('#tblvehicles').on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#logistics-vehicles').LoadingOverlay("show");
            } else {
                $('#logistics-vehicles').LoadingOverlay("hide", true);
            }
        })
            .dataTable({
                destroy: true,
                serverSide: true,
                paging: true,
                searching: false,
                deferLoading: 30,
                order: [[0, "asc"]],
                ajax: {
                    "url": "/TraderChannels/SearchVehicles",
                    "data": function (d) {
                        return $.extend({}, d, {
                            "keyword": $('#txtvehiclekeyword').val(),
                        });
                    }
                },
                columns: [
                    { "title": "Vehicle type", "data": "VehicleType", "searchable": true, "orderable": true },
                    { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                    { "title": "Reference or registration", "data": "RefOrRegistration", "searchable": true, "orderable": true },
                    {
                        "title": "Options",
                        "data": "Id",
                        "searchable": true,
                        "orderable": false,
                        "render": function (data, type, row, meta) {
                            var _htmlOptions = '<div class="btn-group"><button class="btn btn-primary" data-toggle="dropdown"><i class="fa fa-cog"></i></button>';
                            _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary">';
                            _htmlOptions += '<li><a href="#" onclick="loadModalDeliveryVehicle(' + data + ')">Edit</a></li>';
                            _htmlOptions += '<li><a href="#" onclick="deleteVehicle(' + data + ')">Delete</a></li>';
                            _htmlOptions += '</ul></div>';
                            return _htmlOptions;
                        }
                    }
                ]
            });
        $('#tblvehicles').DataTable().ajax.reload();
        $('#txtvehiclekeyword').keyup(delay(function () {
            reloadTableVehicles();
        }, 400));
    }
}
function initPluginsOfTabDrivers(isFirstLoad) {
    if (!isFirstLoad) {
        loadVehicles();
        var $table_drivers = $('#tbldrivers');
        $table_drivers.on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#logistics-drivers').LoadingOverlay("show");
            } else {
                $('#logistics-drivers').LoadingOverlay("hide", true);
            }
        })
            .dataTable({
                destroy: true,
                serverSide: true,
                paging: true,
                searching: false,
                deferLoading: 30,
                order: [[0, "asc"]],
                ajax: {
                    "url": "/TraderChannels/SearchDrivers",
                    "data": function (d) {
                        return $.extend({}, d, {
                            "keyword": $('#txtSearchDriver').val(),
                            "locationId": $('#logistics-drivers select[name=locations]').val()
                        });
                    }
                },
                columns: [
                    {
                        "title": "Driver",
                        "data": "Driver",
                        "searchable": true,
                        "orderable": true,
                        "render": function (data, type, row, meta) {
                            var _htmlDriver = '<a href="#"><div class="table-avatar mini pull-left" style="background-image: url(\'' + row.DriverIcon + '\');"></div>';
                            _htmlDriver += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + data + '</div>';
                            _htmlDriver += '<div class="clearfix"></div></a>';
                            return _htmlDriver;
                        }
                    },
                    {
                        "title": "Email",
                        "data": "Email",
                        "searchable": true,
                        "orderable": true,
                        "render": function (data, type, row, meta) {
                            return '<a href="mailto:' + data + '">' + data + '</a>';
                        }
                    },
                    {
                        "title": "Verhicle",
                        "data": "VehicleId",
                        "searchable": true,
                        "orderable": true,
                        "render": function (data, type, row, meta) {
                            var _htmlVehicleSelect2 = '<select name="vehicle" onchange="updateVehicleDriver(' + row.Id + ',this)" class="form-control select2 dt-select2" style="width: 100%;">';
                            _htmlVehicleSelect2 += '<option></option>';
                            dataVehicles.forEach(function (item) {
                                _htmlVehicleSelect2 += '<optgroup label="' + fixQuoteCode(item.GroupName) + '">';
                                if (item.Items) {
                                    item.Items.forEach(function (el) {
                                        _htmlVehicleSelect2 += '<option value="' + el.Id + '" ' + (el.Id == data ? 'selected' : '') + '>' + fixQuoteCode(el.Text) + '</option>';
                                    });
                                }
                                _htmlVehicleSelect2 += '</optgroup>';
                            });
                            _htmlVehicleSelect2 += '</select>';
                            return _htmlVehicleSelect2;
                        }
                    },
                    {
                        "title": "Status",
                        "data": "Status",
                        "searchable": true,
                        "orderable": true,
                        "render": function (data, type, row, meta) {
                            var _htmlStatus = '<label class="label label-lg label-success">Active</label>';
                            if (data != 1)
                                _htmlStatus = '<label class="label label-lg label-warning">Off duty</label>';
                            return _htmlStatus;
                        }
                    },
                    {
                        "title": "Options",
                        "data": "Id",
                        "searchable": true,
                        "orderable": false,
                        "render": function (data, type, row, meta) {
                            var _htmlOptions = '<div class="btn-group"><button class="btn btn-primary" data-toggle="dropdown" aria-expanded="false"><i class="fa fa-cog"></i></button>';
                            _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right primary">';
                            if (row.Status == 1)
                                _htmlOptions += '<li><a href="#" onclick="setOnOffDuty(' + data + ',2)">Set off duty</a></li>';
                            else
                                _htmlOptions += '<li><a href="#" onclick="setOnOffDuty(' + data + ',1)">Set on duty</a></li>';
                            _htmlOptions += '<li><a href="#" onclick="loadModalLocationChange(' + data + ')">Change location</a></li>';
                            _htmlOptions += '<li><a href="#" onclick="deleteDriver(' + data + ')">Delete</a></li>';
                            _htmlOptions += '</ul></div>';
                            return _htmlOptions;
                        }
                    }
                ],
                drawCallback: function () {
                    $('.dt-select2').select2({ placeholder: "Please select", });
                }
            });
        reloadTableDrivers();
        $('#txtmembersearch').keyup(delay(function () {
            searchDeliveryDrivers();
        }, 400));
        $('#txtSearchDriver').keyup(delay(function () {
            reloadTableDrivers();
        }, 400));
    }
}
function loadContentItemInfo() {
    var itemId = $('#logistics-general #select-traderitems').val();
    if (itemId) {
        var $content_iteminfo = $('#item-info');
        $content_iteminfo.empty();
        $content_iteminfo.LoadingOverlay("show");
        $content_iteminfo.load("/TraderChannels/LoadContentItemInfo?itemId=" + itemId, function () {
            $('#tbliteminfo').DataTable();
            $content_iteminfo.LoadingOverlay("hide");
        });
    }
}
function saveDeliverySettings(itemId) {
    $.LoadingOverlay("show");
    var paramaters = {
        Id: 0,
        DeliveryService: { Id: itemId },
        Location: { Id: $('#logistics-general select[name=locations]').val() }
    };
    $.post("/TraderChannels/SaveDeliverySettings", { settings: paramaters }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
        }
        LoadingOverlayEnd();
    });
}
function loadModalPriceList(id) {
    var $modal_pricelist = $('#delivery-charge-pricelist-add');
    $modal_pricelist.empty();
    $modal_pricelist.modal('show');
    $modal_pricelist.load("/TraderChannels/LoadModalPriceList?id=" + id, function () {
        $('#delivery-charge-pricelist-add select.select2').select2({ placeholder: "Please select" });
        var $frmb2bpricelist = $("#frmb2bpricelist");
        $frmb2bpricelist.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                Icon: {
                    filesize: true
                },
                Summary: {
                    required: true,
                    minlength: 3,
                    maxlength: 500
                }
            }
        });
        $frmb2bpricelist.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmb2bpricelist.valid()) {
                $.LoadingOverlay("show");
                var files = document.getElementById("b2b-price-icon-input").files;
                if (files.length > 0) {
                    UploadMediaS3ClientSide("b2b-price-icon-input").then(function (mediaS3Object) {

                        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                            LoadingOverlayEnd('hide');
                            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                            return;
                        }
                        else {
                            $("#b2b-price-icon-object-key").val(mediaS3Object.objectKey);
                            $("#b2b-price-icon-object-name").val(mediaS3Object.fileName);
                            $("#b2b-price-icon-object-size").val(mediaS3Object.fileSize);

                            saveB2bPriceList();
                        }
                    });
                }
                else {
                    $("#b2b-price-icon-object-key").val("");
                    $("#b2b-price-icon-object-name").val("");
                    $("#b2b-price-icon-object-size").val("");

                    saveB2bPriceList();
                }
            }
        });
    });
}
function saveB2bPriceList() {
    var frmData = new FormData($("#frmb2bpricelist")[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderChannels/SavePriceList",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#delivery-charge-pricelist-add').modal('hide');
                loadContentPriceList();
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Trader");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
            }
            isBusy = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
            LoadingOverlayEnd();
        }
    });
}
function loadContentPriceList() {
    var $content_pricelist = $('#b2b-charges-content');
    $content_pricelist.empty();
    $content_pricelist.LoadingOverlay("show");
    $content_pricelist.load("/TraderChannels/LoadContentPriceList", { keyword: $("#txtplkeyword").val(), locationId: $('#logistics-charges select[name=locations]').val() }, function () {
        $content_pricelist.LoadingOverlay("hide");
    });
}
function loadContentChargeFramework(priceId) {
    $('.pricelists').hide();
    var $content_framework = $('#framework-1');
    $content_framework.fadeIn();
    $content_framework.empty();
    $content_framework.LoadingOverlay("show");
    $content_framework.load("/TraderChannels/LoadContentChargeFramework", { priceId: priceId }, function () {
        $content_framework.LoadingOverlay("hide");
    });
}
function loadModalDeliveryChargeFramework(id, pricelistId) {
    var $modal_deliverychargeframework = $('#delivery-charge-framework-add');
    $modal_deliverychargeframework.empty();
    $modal_deliverychargeframework.modal('show');
    $modal_deliverychargeframework.load("/TraderChannels/LoadModalDeliveryChargeFramework?", { priceListId: pricelistId, id: id }, function () {
        var $frmChargeFramework = $("#frmChargeFramework");
        $frmChargeFramework.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                DistanceTravelledFlatFee: {
                    required: true
                },
                DistanceTravelPerKm: {
                    required: true
                },
                TimeTakenFlatFee: {
                    required: true
                },
                TimeTakenPerSecond: {
                    required: true
                },
                ValueOfDeliveryFlatFee: {
                    required: true
                },
                ValueOfDeliveryPercentTotal: {
                    required: true,
                    min: 0,
                    max: 100
                },
            }
        });
        $frmChargeFramework.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmChargeFramework.valid()) {
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
                            $modal_deliverychargeframework.modal('hide');
                            loadContentChargeFramework(pricelistId);
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
        initSelect2avatarDelivery();
    });
}
function initPriceCloneCurrentLocation(itemId, itemName) {
    $('#frmPriceListCLocationClone input[name=cloneId]').val(itemId);
    $('#frmPriceListCLocationClone input[name=cloneName]').val(itemName);
    $('#frmPriceListCLocationClone input[ name=locationId]').val($('#location_item_' + itemId).val());
    $('#b2b-charge-framework-clone').modal('show');
}
function initFormClonePriceList(isFirstLoad) {
    //Clone current pricelist
    if (!isFirstLoad) {
        var $frmPriceListCLocationClone = $("#frmPriceListCLocationClone");
        $frmPriceListCLocationClone.validate({
            rules: {
                cloneName: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                }
            }
        });
        $frmPriceListCLocationClone.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmPriceListCLocationClone.valid()) {
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
                            $('#b2b-charge-framework-clone').modal('hide');
                            loadContentPriceList();
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
        $('#txtplkeyword').keyup(delay(function () {
            loadContentPriceList();
        }, 400));
    }
}
function initFormCloneOtherLocationPriceList() {
    //Clone current pricelist
    var $frmPriceListOtherLocationClone = $("#frmPriceListOtherLocationClone");
    $frmPriceListOtherLocationClone.validate({
        rules: {
            cloneName: {
                required: true,
                minlength: 3,
                maxlength: 150
            },
            locationId: {
                required: true
            }
        }
    });
    $frmPriceListOtherLocationClone.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;

        if ($frmPriceListOtherLocationClone.valid()) {
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
                        $('#b2b-charge-framework-port').modal('hide');
                        loadContentPriceList();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}
function loadModalLocationChange(priceId) {
    var $modal_chargeframeworkport = $('#b2b-charge-framework-port')
    $modal_chargeframeworkport.empty();
    $modal_chargeframeworkport.modal('show');
    $modal_chargeframeworkport.load("/TraderChannels/LoadModalChargeFrameworkPort?priceId=" + priceId, function () {
        initFormCloneOtherLocationPriceList();
        $('#frmPriceListOtherLocationClone .select2').select2({ placeholder: "Please select" });
    });
}
function deletePriceList(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "B2B Logistics",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeletePriceList", { id: id }, function (Response) {
                    if (Response.result) {
                        loadContentPriceList();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "B2B Logistics");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    }
                });
                return;
            }
        }
    });
}
function deleteChargeFramework(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "B2B Logistics",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteChargeFramework", { id: id }, function (Response) {
                    if (Response.result) {
                        loadContentChargeFramework($('#hdfPriceListId').val());
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "B2B Logistics");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    }
                });
                return;
            }
        }
    });
}
function loadModalDeliveryVehicle(id) {
    var $modal_deliveryvehicle = $('#delivery-vehicle-add');
    $modal_deliveryvehicle.empty();
    $modal_deliveryvehicle.modal('show');
    $modal_deliveryvehicle.load("/TraderChannels/LoadModalDeliveryVehicle?id=" + id, function () {
        var $frmVehicle = $("#frmVehicle");
        $frmVehicle.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 3,
                    maxlength: 150
                },
                RefOrRegistration: {
                    required: true,
                    maxlength: 50
                }
            }
        });
        $frmVehicle.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($frmVehicle.valid()) {
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
                            $modal_deliveryvehicle.modal('hide');
                            reloadTableVehicles();
                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                        } else if (!data.result && data.msg) {
                            cleanBookNotification.error(data.msg, "Trader");
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                        LoadingOverlayEnd();
                    }
                });
            }
        });
        initSelect2avatarDelivery();
    });
}
function initSelect2avatarDelivery() {
    $('.select2avatar-delivery').select2({
        placeholder: 'Please select',
        templateResult: formatOptions2,
        templateSelection: formatSelected2
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
function reloadTableVehicles() {
    setTimeout(function () {
        $('#tblvehicles').DataTable().ajax.reload();
    }, 100);
    loadVehicles();
}
function reloadTableDrivers() {
    setTimeout(function () {
        $('#tbldrivers').DataTable().ajax.reload();
    }, 100);

}
function deleteVehicle(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteVehicle", { id: id }, function (Response) {
                    if (Response.result) {
                        reloadTableVehicles();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(Response.msg, "B2B Logistics");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    }
                });
                return;
            }
        }
    });
}
function deleteDriver(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Trader",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/TraderChannels/DeleteDriver", { id: id }, function (Response) {
                    if (Response.result) {
                        reloadTableDrivers();
                    } else if (!Response.result && Response.msg) {
                        cleanBookNotification.error(_L(Response.msg), "B2B Logistics");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
                    }
                });
                return;
            }
        }
    });
}
function loadVehicles() {
    $.get("/TraderChannels/GetVehiclesForSelect2", function (data) {
        dataVehicles = data;
    });
}
function searchDeliveryDrivers() {
    var keyword = $('#txtmembersearch').val();
    $('.contact-list-found').LoadingOverlay("show");
    $('.contact-list-found').load("/TraderChannels/LoadModalDeliveryDriver", { keyword: keyword }, function () {
        $('.existing-member').show();
        $('.contact-list-found').LoadingOverlay("hide");
    });
}
function setOnOffDuty(id, status) {
    $.post("/TraderChannels/UpdateStatusDriver", { id: id, status: status }, function (Response) {
        if (Response.result) {
            reloadTableDrivers();
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function loadModalLocationChange(id) {
    var $modal_driverclocationchange = $('#b2b-driver-location-change');
    $modal_driverclocationchange.empty();
    $modal_driverclocationchange.modal('show');
    $modal_driverclocationchange.load("/TraderChannels/LoadModalLocationChange?driverId=" + id, function () {
        var $frmVehicle = $("#frmLocationchange");
        $frmVehicle.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
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
                        $modal_driverclocationchange.modal('hide');
                        reloadTableDrivers();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    LoadingOverlayEnd();
                }
            });
        });
        $('#frmLocationchange .select2').select2();
    });
}
function updateVehicleDriver(id, elVehicle) {
    $.post("/TraderChannels/UpdateVehicleDriver", { id: id, vehicleId: $(elVehicle).val() }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function loadContentMemberDetail(posid) {
    var $content_contactadd = $('.contact-add');
    $content_contactadd.empty();
    $content_contactadd.LoadingOverlay("show");
    $content_contactadd.load("/TraderChannels/LoadContentMemberDetail?posUId=" + posid, function () {
        $content_contactadd.LoadingOverlay("hide");
        $('#slLocationIdForDriver').select2({ placeholder: "Please select" });
        $('.contact-list-found').hide();
        $('.contact-invite').hide();
        $('.contact-add').hide();
        $('.contact-add').fadeIn();
    });
}
function saveMember() {
    var locationId = $('#slLocationIdForDriver').val();
    if (locationId == 0) {
        cleanBookNotification.error(_L("ERROR_MSG_622"), "B2B Logistics");
        return;
    }
    $.LoadingOverlay("show");
    $.post("/TraderChannels/AddDriver", {
        posUId: $('#hdfPosUid').val(),
        accountId: $('#hdfAccountId').val(),
        locationId: locationId,
        driverUserId: $("#hdfUserid").val(),
    }, function (Response) {
        if (Response.result) {
            $('#delivery-driver-add').modal('hide');
            reloadTableDrivers();
            $('#lnkBackSearch').click();
            $('#txtmembersearch').val('');
            $('.contact-list-found').empty();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "B2B Logistics");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "B2B Logistics");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B Logistics");
        }
        LoadingOverlayEnd();
    });
}
function getLocationsDomain($elm, isFirstLoad) {
    if (!isFirstLoad) {
        $.get("/Commerce/getLocationsOfCurrentDomain", function (data) {
            $elm.select2('destroy');
            //$elm.empty();
            $elm.select2({
                placeholder: "Please select",
                data: data
            });
        });
    }
}
//end B2B Logistics
//Tree view selectaccount
function initSelectedAccount() { }
function closeSelected() {
    if (BKAccount.Id) {
        $(".accountInfo").empty();
        $(".accountInfo").append(BKAccount.Name);
    } else {
        $(".accountInfo").empty();
    }
    if ($(".accountInfo").text().length > 0) {
        $(".addbtnaccount").attr("style", "display:none;");
        $(".editbtnaccount").removeAttr("style");
    } else if ($(".accountInfo").text().length === 0) {
        $(".editbtnaccount").attr("style", "display:none;");
        $(".addbtnaccount").removeAttr("style");
    }
};
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    BKAccount.Id = id;
    BKAccount.Name = name;
    //Add cash/bank
    if ($("#accountId").length > 0)
        $("#accountId").val(id);
    //Add a Driver
    if ($("#hdfAccountId").length > 0)
        $("#hdfAccountId").val(id);
    closeSelected();
    $("#app-bookkeeping-treeview").modal("hide");
};
function showChangeAccount() {
    setTimeout(function () {
        CollapseAccount();
    },
        1);

};
function CollapseAccount() {
    $(".jstree").jstree("close_all");
};
function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    }, 1);
};
//end
function initEventStoreProfile() {
    $('div.social-links input').keyup(delay(function () {
        saveSocialLinks();
    }, 1000));
    $('#frmB2BProfile input[name=BusinessName], #frmB2BProfile textarea[name=BusinessSummary]').keyup(delay(function () {
        saveProfileB2B();
    }, 1000));
    $('select[name=tags]').change(function () {
        delay(function () {
            saveTags();
        }, 1000)
    });
    $('#frmsociallinks').validate(
        {
            rules: {
                FacebookUrl: {
                    url: true
                },
                LinkedInUrl: {
                    url: true
                },
                TwitterUrl: {
                    url: true
                },
                YoutubeUrl: {
                    url: true
                },
                InstagramUrl: {
                    url: true
                }
            }
        });
}
function saveSocialLinks() {
    var profileId = $('#hdfProfileId').val();
    if (profileId && $('#frmsociallinks').valid()) {
        var $facebook = $('#frmsociallinks input[name=FacebookUrl]');
        var $instagram = $('#frmsociallinks input[name=InstagramUrl]');
        var $linkedIn = $('#frmsociallinks input[name=LinkedInUrl]');
        var $twitter = $('#frmsociallinks input[name=TwitterUrl]');
        var $youtube = $('#frmsociallinks input[name=YoutubeUrl]');
        var b2BProfile = { Id: profileId };
        var facebookUrl = { Url: $facebook.val(), Type: $facebook.data('type'), B2BProfile: b2BProfile };
        var instagramUrl = { Url: $instagram.val(), Type: $instagram.data('type'), B2BProfile: b2BProfile };
        var linkedInUrl = { Url: $linkedIn.val(), Type: $linkedIn.data('type'), B2BProfile: b2BProfile };
        var twitterUrl = { Url: $twitter.val(), Type: $twitter.data('type'), B2BProfile: b2BProfile };
        var youtubeUrl = { Url: $youtube.val(), Type: $youtube.data('type'), B2BProfile: b2BProfile };
        var paramaters = [];
        paramaters.push(facebookUrl);
        paramaters.push(instagramUrl);
        paramaters.push(linkedInUrl);
        paramaters.push(twitterUrl);
        paramaters.push(youtubeUrl);
        $.post("/Commerce/UpdateSocialLinks", { socialLinks: paramaters }, function (Response) {
            if (Response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function saveTags() {
    var profileId = $('#hdfProfileId').val();
    if (profileId) {
        var paramaters = $('select[name=tags] option:selected').text();
        $.post("/Commerce/UpdateTags", { tags: paramaters, profileId: profileId }, function (Response) {
            if (Response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}