var _c2cQbiceId = "0";
var $domainKey = "";
var $connectedC2C = "";
var loadCountActivity = 0;
var isFirstLoad = 0;
var previousShown = false;
var isBusy = false;
var isShopDetailBeingShown = false;

$(document).ready(function () {
    //timeline();
    tabTimeLine();
    loadC2CQbicleContent(true, false);
    initFindPeoble();
    initSearch();
    initC2CQbicleEventClick();
    $('ul.widget-contacts > li.active > a').click();
    initC2cQbicleDefaultActive();
    intitDatatableAgain();
    initPlugin();
    var _tabactive = getQuerystring('tab');
    if (_tabactive == 'shopping') {
        var _shopId = getQuerystring('shopid');
        if (_shopId) {
            isShopDetailBeingShown = true;
        }

        $('a[data-target=#shopping]').trigger('click');
        setTimeout(function () {
            if (_shopId) {
                loadB2CBusinessCatalogues(_shopId);
                $('#linkshop').click();

                setTimeout(function () {
                    var _catalogKey = getQuerystring('catalogkey');
                    if (_catalogKey) {
                        loadB2CBusinessCatalogDetail(_catalogKey);
                        $("#business-catalog-detail").addClass("active fade in");
                        $('#business-catalogues').removeClass("active fade in");
                    }
                }, 200);
            }
        }, 200);
    } else if (_tabactive == 'contacts') {
        $('a[data-target=#all-contacts]').trigger('click');
    }
});
function initPlugin() {
    $('.checkmulti').multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true,
        onChange: function (element, checked) {
            if (checked === true) {
                updateTemporaryselectedProduct();
                updateTemporaryselectedStore();
            }
            else if (checked === false) {
                updateTemporaryselectedProduct();
                updateTemporaryselectedStore();
            }
        }

    });
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
    $('#txtFilterPeopleKeyword').keyup(delay(function () {
        initFindPeoble();
    }, 500));
    $('#slFilterType').change(function () {
        initFindPeoble();
    });
    $('#slFilterPeopleType').change(function () {
        initFindPeoble();
    });
    //Filters C2C Qbicle
    $('#filters-contacts input[name=search]').keyup(delay(function () {
        _c2cQbiceId = "0";
        isC2CAllContactShown = false;
        c2cContactTabPage = 0;

        loadC2CQbicleContent(true, false, true);
    }, 500));
    $('#filters-contacts select[name=orderBy]').change(function () {
        isC2CAllContactShown = false;
        c2cContactTabPage = 0;

        loadC2CQbicleContent(true, false, true);
    });
    $('#filters-contacts select[name=contactType]').change(function () {
        _c2cQbiceId = "0";

        isC2CAllContactShown = false;
        c2cContactTabPage = 0;

        loadC2CQbicleContent(true, false, true);
    });

    $("#connection-shown-type li").click(function () {
        if (!$(this).hasClass('active')) {
            _c2cQbiceId = "0";
            $("#connection-shown-type li").removeClass('active');
            $(this).addClass('active');
            $(".widget-contacts li").remove();
            loadC2CQbicleContent(true);
        }
    });
    //end

    $('.filter-stores input[name=search]').keyup(delay(function () {
        initFindBusinessStores();
    }, 500));

    $('.filter-stores select[name=locations]').change(function () {
        initFindBusinessStores();
    });

    // filter products community
    $('#psearching').keyup(delay(function () {
        initFindProducts();
    }, 500));

    $("#pcountry-selector").on('change', function () {
        initFindProducts();
    });
    //End
    $("#products select[name=\"loc\"]").change(function () {
        initFindProducts();
    })
}
function initFindPeoble() {
    var $data_container_people = $('#data-container-people');
    var $pagination_container = $('#pagiation-people');
    $pagination_container.pagination({
        dataSource: '/C2C/FindPeople',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_people.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 8,
        ajax: {
            data: { PeopleType: $('#slFilterPeopleType').val(), ContactType: $('#slFilterType').val(), keyword: $('#txtFilterPeopleKeyword').val() },
            beforeSend: function () {
                $data_container_people.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var extraCol = (count % 4 == 0 ? 0 : 4) - count % 4;
            var uri = $('#api-uri').val();
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += peopleTemplate(item, uri);
            });
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<article class="col" style="box-shadow: none; background: transparent;"></article>';
            }
            $data_container_people.html(dataHtml);
        }
    })
}
function peopleTemplate(data, uri) {

    var _html = '<article class="col"><span class="last-updated">' + (data.Type == 2 ? 'Individual' : 'Business') + '</span>';
    _html += '<a href="' + (data.Type == 2 ? '/Community/UserProfilePage?uId=' + data.UserId : '/Commerce/PublishBusinessProfile?id=' + data.UserId) + '"><div class="avatar" style="background-image: url(\'' + uri + data.AvatarUri + '&size=T\');">&nbsp;</div>';
    _html += '<h1 style="color: #333;">' + data.FullName + '</h1>';
    _html += '</a>';
    _html += '<div class="row" style="padding: 20px 20px 0 20px;">';
    if (data.HasConnected) {
        _html += '<a href="#" class="btn btn-primary community-button" onclick="connectC2C(\'' + data.UserId + '\',\'' + fixQuoteCode(data.FullName) + '\',' + data.Type + ', true)"><i class="fa fa-comments"></i> &nbsp; Chat</a>';
    } else {
        _html += '<a href="#" class="btn btn-info community-button" onclick="connectC2C(\'' + data.UserId + '\',\'' + fixQuoteCode(data.FullName) + '\',' + data.Type + ', false)"><i class="fa fa-heart-o"></i> &nbsp; Connect</a>';
    }
    _html += '</div></article>';
    return _html;
}
//type=1: Businesses, type=2: Individual
function connectC2C(uId, fullname, type, hasConnected) {
    $.LoadingOverlay('show');
    $.post("/C2C/ConnectC2C", { linkId: uId, type: type }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            _c2cQbiceId = response.Object;
            if (!hasConnected) {
                if (type == 1)
                    cleanBookNotification.success(_L("CONNECTED_SUCCESS", [fullname]), "Community");
                else
                    cleanBookNotification.success(_L("SENT_INVITE_MSG_SUCCESS", [fullname]), "Community");
            }
            initFindPeoble();

            loadC2CQbicleContent(true);
            $('a[data-target="#c2c-comms"]').click();
            var statusStr = response.Object2;
            if (statusStr == "Blocked") {
                $("#connection-shown-type a[href=#blocked]").click();
            } else {
                $("#connection-shown-type a[href=#all]").click();
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function setLikeBy(qId, linkId, type, isFavorite, event) {
    event.stopPropagation();
    var $likeStr = $($(event.target)[0]);
    var linkIdActive = $(".widget-contacts li.active").data('linkid');
    var qbicleTypeName = type == 2 ? "Individual" : "Business";
    $.post("/C2C/SetLikeBy", { qId: qId, linkId: linkId, type: type, isFavorite: isFavorite }, function (response) {
        if (response.result) {
            if (isFavorite) {
                $likeStr.text("Remove from Favourites");
                $likeStr.attr('onclick', 'setLikeBy(' + qId + ',\'' + linkId + '\',' + type + ',' + !isFavorite + ',event)');
                if (linkId == linkIdActive) {
                    $(".plainmenu a[name=favourite-option]").text("Remove from Favourites");
                }
                cleanBookNotification.success("Add to Favourite successfully", "Qbicles");
                $("li[data-linkid=" + linkId + "] a p").text(qbicleTypeName + ", Favourites");
            } else {
                $likeStr.text("Add to Favourites");
                $likeStr.attr('onclick', 'setLikeBy(' + qId + ',\'' + linkId + '\',' + type + ',' + !isFavorite + ',event)');
                if (linkId == linkIdActive) {
                    $(".plainmenu a[name=favourite-option]").text("Add to Favourites");
                }
                cleanBookNotification.success("Remove from Favourite successfully", "Qbicles");
                $("li[data-linkid=" + linkId + "] a p").text(qbicleTypeName);
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    }).done(function () {
        $likeStr.closest(".contactoptside").removeClass("open");
        if ($("#connection-shown-type a[name=favourite]").closest("li").hasClass('active')) {
            loadC2CQbicleContent(true);
        } else {
            getC2CTalkNum();
        }
    });
}


var c2cContactTabPage = 0;
var isC2CContactTabBusy = false;
var isC2CAllContactShown = false;
function loadC2CQbicleContent(eventclick, isLoadMore, isFilter) {

    if (isC2CContactTabBusy) {
        return;
    }
    // If not LoadMore, means types of loading (active/pending/...) changed, and need to be loaded from 1st page again
    //if (isLoadMore == null || isLoadMore == false) {
    //    c2cContactTabPage = 0;
    //}
    //$eventclick = eventclick;


    if (eventclick) {
        isC2CAllContactShown = false;
        c2cContactTabPage = 0;
    }

    if (isFilter)
        c2cContactTabPage = 0;

    var paramaters = {
        keyword: $('#filters-contacts input[name=search]').val(),
        orderby: $('#filters-contacts select[name=orderBy]').val(),
        contactType: $('#filters-contacts select[name=contactType]').val(),
        c2cQbicleKey: _c2cQbiceId,
        isAllShown: $("#connection-shown-type a[href=#all]").closest('li').hasClass('active'),
        isFavouriteShown: $("#connection-shown-type a[href=#fav]").closest('li').hasClass('active'),
        isRequestShown: $("#connection-shown-type a[href=#new]").closest('li').hasClass('active'),
        isSentShown: $("#connection-shown-type a[href=#sent]").closest('li').hasClass('active'),
        isBlockedShown: $("#connection-shown-type a[href=#blocked]").closest('li').hasClass('active'),
        PageIndex: c2cContactTabPage,
    };

    var _urlTabContentLoading = "/C2C/LoadC2CQbiclesContent";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        data: paramaters,
        url: _urlTabContentLoading,
        beforeSend: function () {
            isC2CContactTabBusy = true;
            $("#connection-shown-type li").css("pointer-events","none");
            if ($('#contact-tab-previous'))
                $('#contact-tab-previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
        },
        success: function (resp) {
            var data = resp.Data;
            if (isLoadMore == null || !isLoadMore) {
                $('.widget-contacts').LoadingOverlay('show');
                $(".widget-contacts").html('');
                $(".widget-contacts").append('<div id="contact-tab-previous"></div>');
                $(data.ModelString).insertBefore("#contact-tab-previous");
            } else {
                $(data.ModelString).insertBefore("#contact-tab-previous");
            }
            //console.log('c2cContactTabPage:' + c2cContactTabPage);
            //console.log('data.ModelCount:' + data.ModelCount);
            //console.log('isC2CAllContactShown:' + isC2CAllContactShown);
            //console.log('ul.subapps-nav li.active.tab-activities.length:' + $("ul.subapps-nav li.active.tab-activities").length);

            if (c2cContactTabPage >= data.ModelCount) {
                isC2CAllContactShown = true;
            } else {
                c2cContactTabPage = c2cContactTabPage + 1;
                isC2CAllContactShown = false;
            }
        },
        error: function (err) {
        }
    }).then(function () {
        isC2CContactTabBusy = false;
        $('#contact-tab-previous').empty();
        if (isLoadMore == null || !isLoadMore) {
            getC2CTalkNum();
            $('.widget-contacts').LoadingOverlay('hide');
            if (eventclick)
                $('ul.widget-contacts > li.active > a').click();
            else
                $("ul.widget-contacts > li:first-child > a").click();
        }
        $("#connection-shown-type li").css("pointer-events","");
    })
}

function tabTimeLine() {
    var $elm = $(".contained-sidebar");
    $($elm).scrollTop(0);
    $($elm).scroll(function () {
        
        //if (!isC2CAllContactShown) {
        //    setTimeout(function () {
        //        loadC2CQbicleContent(true, true);
        //    }, 100);
        //}
        if (//($($elm).scrollTop() >= ($(".contact-list").height() - $(window).height() - 400))
            !isC2CAllContactShown && $("ul.subapps-nav li.active.tab-activities").length > 0) {
            if (isC2CAllContactShown == false) {
                setTimeout(function () {
                    loadC2CQbicleContent(false, true);
                }, 100);

                return isC2CAllContactShown;
            }
        }
    });
    //scroll only this div - QBIC-4012
    $elm.css("overscroll-behavior", "contain");
}

function initC2CQbicleEventClick() {

    $(document).on('click', 'ul.widget-contacts > li > a', (function (e) {

        $('ul.widget-contacts li').removeClass('active');
        $(this).closest('li').addClass('active');
        $('html').scrollTop(0);
        setTimeout(function () {
            $("#post-text").val('');

            TypingNotification(false);

            initC2cQbicleDefaultActive();
            var $itemactive = $('ul.widget-contacts li.active');
            _c2cQbiceId = $itemactive.data("c2cqbicleid");
            $domainKey = $itemactive.data("c2cdomainid");
            //connected to user community
            $connectedC2C = $itemactive.data("communityemail");


            $('#hdfCurrentQbicleId').val(_c2cQbiceId);
            var status = $itemactive.data("status");
            var type = $itemactive.data("type");
            if (status == 'Approved') {
                $('.c2c-not-blocked').show();
                loadDataDashboardC2C(true);
                viewProfileByC2CQbicle($itemactive, type);
            } else if (status == 'Blocked' || status == "Cancel") {
                $('.c2c-not-blocked').hide();
                loadDataDashboardC2C(true);
                viewProfileByC2CQbicle($itemactive, type);
            } else {
                loadC2CStatusInfo();
            }
            setCommunityTalkViewed(_c2cQbiceId, type);

        }, 100)
    }));
}

/**
  setStatusBy
  type=1: Businesses, type=2: Individual
 */
function setC2CStatusBy(key, status, type) {
    $.post("/C2C/SetStatusBy", { key: key, status: status, type: type }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            if (status == 'Approved') {
                loadC2CQbicleContent(true);
            } else {
                loadC2CQbicleContent(false,false,true);
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function removeCommunityQbicleById(key, type) {
    var url = "";
    if (type == "c2c")
        url = "/C2C/RemoveC2CQbicleById";
    else if (type = "b2c")
        url = "/B2C/RemoveB2CQbicleById";
    $.post(url, { key: key }, function (response) {
        if (response.result) {
            _c2cQbiceId = "0";
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            loadC2CQbicleContent(false);
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}


//function triggerClickC2CQbicleActive() {
//    $('ul.widget-contacts li.active').click();

//    $("ul.widget-contacts li:first-child a").click();
//}
//type=1: Businesses, type=2: Individual

var $commonName = "";

function viewProfileByC2CQbicle(elm, type) {
    $commonName = elm.data("forename");
    var linkid = elm.data("linkid");
    var key = elm.data("c2cqbicleid");
    var status = elm.data("status");
    var logoUrl = elm.data("logourl");
    var isLiked = elm.data("isliked");
    var qId = elm.data("qid");
    var isCurrentUserRequest = elm.data("iscurrentreq");

    //set data for FAB
    $('.order-context-flyout-div').hide();
    $('.customer-action').hide();    
    $('.business-action').empty();


    $('.plainmenu a[name=req-accept-option], .plainmenu a[name=req-decline-option], .plainmenu a[name=sent-cancel-option]').hide();

    if (type == 1) {//Business
        
        $.ajax({
            type: 'post',
            url: '/B2C/CheckExistB2cOrders',
            data: { qbicleKey: _c2cQbiceId },
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

        $('.user-viewprofile').attr('href', '/Commerce/PublishBusinessProfile?id=' + linkid);
        $('.plainmenu button').css("background-image", "url('" + logoUrl + "')");
        if (status == 'Blocked') {
            $(".plainmenu a[name=block-option]").text("Unblock");
            $(".plainmenu a[name=favourite-option]").hide();
            $('.c2c-not-blocked').hide();
        } else {
            $(".plainmenu a[name=block-option]").text("Block");
            $(".plainmenu a[name=favourite-option]").show();
            $('.c2c-not-blocked').show();
            //$('.business-action').html("<button onclick='browseCatalog();' class='btn btn-success fab'><i class='fa fa-shopping-basket'></i></button>");
            $('.business-action').html("<button onclick='browseB2CCatalog();' class='btn btn-success fab'><i class='fa fa-shopping-basket'></i></button>");
        }
    } else {//Individual-customer 
        //$(".order-context-flyout-div").hide();
        $('.user-viewprofile').attr('href', '/Community/UserProfilePage?uId=' + linkid);
        $('.plainmenu button').css("background-image", "url('" + logoUrl + "')");
        if (status == 'Blocked') {
            $(".plainmenu a[name=block-option]").text("Unblock");
            $(".plainmenu a[name=favourite-option]").hide();
            $('.c2c-not-blocked').hide();
        } else {
            if (status == 'Pending') {
                if (isCurrentUserRequest) {
                    $('.plainmenu a[name=sent-cancel-option]').show();
                } else {
                    $('.plainmenu a[name=req-accept-option], .plainmenu a[name=req-decline-option]').show();
                }
            }

            $('.customer-action').show();
            $(".plainmenu a[name=block-option]").text("Block");
            $(".plainmenu a[name=favourite-option]").show();
            $('.c2c-not-blocked').show();
        }
    }

    if (isLiked) {
        $(".plainmenu a[name=favourite-option]").text("Remove from Favourites");
    } else {
        $(".plainmenu a[name=favourite-option]").text("Add to Favourites");
    }
}
function blockContact(key, type) {
    //var $itemactive = $('ul.widget-contacts li.active');
    var $qbicItem = $("ul.widget-contacts li[data-c2cqbicleid=" + key + "]");
    var r = confirm('Are you sure you want to block ' + $qbicItem.data("forename") + '?');
    if (r == true) {
        setC2CStatusBy(key, 'Blocked', type);
    }
}

function toggleBtnInStores() {
    var $buttonSelectCategory = $('#add-filter');
    var $buttonClearCategory = $('#clear-filter');
    var $valueButton = $('.filter-stores input[name=search]').val();
    var $storesResults = $('#stores-results');
    var $storesResultsInfo = $('#data-container-businesses');
    var $defaultStoreInfo = $('#stores-splash');
    var $filterStatus = $("#filter-store-info")[0].innerText;


    if (!$valueButton && $filterStatus == 0 && $("#country-search").val() == 0) {
        $buttonSelectCategory.hide();
        $buttonClearCategory.hide();
        $storesResults.hide();
        $storesResultsInfo.show();
        $defaultStoreInfo.show();
        $('#stores-subfilter').fadeOut();

    } else {
        $buttonSelectCategory.show();
        $buttonClearCategory.show();
        $storesResults.show();
        $storesResultsInfo.show();
        //$defaultStoreInfo.hide();
        $('#stores-subfilter').fadeIn();
    }
}

function searchproducts() {
    $('#products-splash').hide();
    $('#products-results').hide();
    $('#products-subfilter').hide();
    var $filterStatus = $("#filter-product-info")[0].innerText;
    if ($('#psearching').val() == "" && $("#products select[name=\"loc\"]").val() == 0 && $filterStatus == 0) {
        $('#products-splash').fadeIn();
        console.log("false")
    } else {
        $('#products-results').fadeIn();
        $('#products-subfilter').fadeIn();
        console.log("true")
    }
}

// button Filter Store
$('#shopping #add-filter').click(function () {
    toggleCagetorySelect();
})
$('#shopping #clear-filter').click(function () {
    TemporaryClearFilterStore();
    initFindBusinessStores();
    updateTotalSelectedCategory();
})
$('.filter-stores input[name=search]').focus(function () {
    toggleBtnInStores();
})

$('.filter-stores input[name=search]').focusout(function () {
    toggleBtnInStores();
})

$('#reset-filter-store').click(function () {
    TemporaryClearFilterStore();
    $("#reset-filter-store").attr('disabled', '');
})

$('#litmitMyConnections').click(delay(function () {
    updateTemporaryselectedStore();
}, 500))

function TemporaryClearFilterStore() {
    $('option', $('#community-stores-filter .checkmulti')).each(function (element) {
        $(this).removeAttr('selected').prop('selected', true);
    });
    $('.checkmulti').multiselect('refresh');

    $('#community-stores-filter input[name=LimitMyConnections]').prop("checked", false);
    $('#litmitMyConnections div[data-toggle="toggle"]').removeClass("btn-success").addClass("btn-default off");
    // updateTemporaryselectedStore();
}

function updateTemporaryselectedStore() {
    var listMultipleSelected = $('#categories-search option:selected').length;
    var listMultipleSelect = $('#categories-search option').length;
    var isMyConnectionSelected = $('#litmitMyConnections div[data-toggle="toggle"]').hasClass('btn-success');
    if (listMultipleSelected < listMultipleSelect || isMyConnectionSelected) {
        $("#reset-filter-store").removeAttr('disabled');
    }
    else {
        $("#reset-filter-store").attr('disabled', '');
    }
}

function toggleCagetorySelect() {
    $('.optional-form').toggle();
}

// button Filter Product
$('#shopping #add-filter-products').click(function () {
})
$('#shopping #clear-filter-products').click(function () {
    ClearFilterProduct();
})
$('#reset-filter-products').click(function () {
    TemporaryClearFilterProduct();
})



// Tagify
var inputTagify = document.querySelector('textarea[name="input-custom-dropdown"]');
// init Tagify script on the above inputs
var tagify = new Tagify(inputTagify, {
    enforceWhitelist: true,
    whitelist: [],
    maxTags: 20,
    dropdown: {
        maxItems: 5,           // <- mixumum allowed rendered suggestions
        classname: "tags-look", // <- custom classname for this dropdown, so it could be targeted
        enabled: 0,             // <- show suggestions on focus
        closeOnSelect: false,    // <- do not hide the suggestions dropdown once an item has been selected
        mode: 'mix',
        enforceWhitelist: true
    }
});

//triggle when add/remove tags
tagify.on('add', function (e) {
    // updateTemporaryselectedProduct();
    $('#reset-filter-products').removeAttr('disabled');
});

tagify.on('remove', function (e) {
    updateTemporaryselectedProduct();
});

function dynamicWhitelistTagify() {
    var list = [];
    var totalist = $("#productTag option");
    for (let index = 0; index < totalist.length; index++) {
        list.push(totalist[index].text);
    }
    distinctList = [...new Set(list)];
    tagify.whitelist = distinctList;
}

function FilterByListInTagify() {
    var listfromTagify = '';
    if (inputTagify.value) {
        listfromTagify = JSON.parse(inputTagify.value);
        for (let index = 0; index < JSON.parse(inputTagify.value).length; index++) {
            listfromTagify.push(JSON.parse(inputTagify.value)[index].value)
        }
    }
    return listfromTagify;
}

function backupFilterProduct() {
    previousChoiceTagify = inputTagify.value;
    previousChoiceSelect = $('#listBrands option:selected').map(function (a, item) { return item.value; });
}

function TemporaryClearFilterProduct() {
    backupFilterProduct();
    tagify.removeAllTags();
    $("#listBrands").multiselect('selectAll', false);
    $("#listBrands").multiselect('refresh');
    updateTemporaryselectedProduct();
    // triggle
    $('#reset-filter-products').attr('disabled', '');
}
function RestoreOptionFilterProduct() {
    inputTagify.value = previousChoiceTagify;
}

function ClearFilterProduct() {
    //remove tagify
    tagify.removeAllTags();
    // set value null
    inputTagify.value = null;
    $("#listBrands").multiselect('selectAll', false);
    $("#listBrands").multiselect('refresh');

    initFindProducts();
}

// draw the row results
function FilterDataTable(listword, brands) {
    // Search LV 1 : Use ogrinal information to get result
    $(".dtproducts").DataTable().columns(2).search(brands, true, false, false).draw();
    // Search LV2 : Use result LV1 to search    
    $(".dtproducts").DataTable().search(listword).draw();
    updateTotalSelectedProduct();
}

function updateTotalSelectedProduct() {
    var totalSelected = 0;
    var lstBrandsSelected2 = $("#listBrands + .btn-group .multiselect-container li.active").length;
    var fullSelected = $("#listBrands + .btn-group .multiselect-container li").length;
    if (lstBrandsSelected2 < fullSelected) totalSelected += 1;

    if (inputTagify.value) {
        var listfromTagify = JSON.parse(inputTagify.value);
        totalSelected = totalSelected + 1;
    }
    $("#add-filter-products .label.label-info").text(totalSelected);
    if (totalSelected > 0) {
        $('#clear-filter-products').removeAttr('disabled');
        $('#reset-filter-products').removeAttr('disabled');
    }
    else {
        $('#clear-filter-products').attr('disabled', '');
        $('#reset-filter-products').attr('disabled', '');
    }
}

function updateTemporaryselectedProduct() {
    var listTagify = inputTagify.value.length;
    var listMultipleSelect = $('#listBrands option:selected').length;
    if (listTagify + listMultipleSelect > 0) {
        $('#reset-filter-products').removeAttr('disabled');
    }
    else {
        $('#reset-filter-products').attr('disabled', '');
    }
}

function updateTotalSelectedCategory() {
    var totalSelected = 0;
    if ($("#categories-search").val()) {
        var selectedCategories = $("#categories-search").val().length;
        var allCategoryOptions = $("#categories-search")[0].options.length;
        if (selectedCategories != allCategoryOptions) {
            totalSelected = totalSelected + 1;
        }
    } else if ($("#categories-search")[0].options.length != 0) {
        totalSelected = totalSelected + 1;
    }

    var isOnlyMyConnectionChecked = $('#community-stores-filter input[name=LimitMyConnections]').prop("checked");
    if (isOnlyMyConnectionChecked) {
        totalSelected = totalSelected + 1;
    }

    $("#add-filter .label.label-info").text(totalSelected);
    if (totalSelected > 0)
        $('#clear-filter').removeAttr('disabled');
    else
        $('#clear-filter').attr('disabled', '');
}


function initFindBusinessStores() {
    // Micro items
    var $data_container_business = $('#data-container-businesses');
    var $shopping_tab_content = $('#shopping-container');
    var $feature_store = $("#stores-splash");
    var $pagination_container = $('#pagiation-businesses');
    var categoryIds = $('#categories-search').val();
    var $storesresults = $('#stores-results');
    var numberStores;
    $pagination_container.pagination({
        dataSource: '/B2C/GetBusinessStores',
        locator: 'items',
        totalNumberLocator: function (response) {
            $shopping_tab_content.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            numberStores = response.totalNumber;
            return response.totalNumber;
        },
        pageSize: 8,
        ajax: {
            data: {
                keyword: $('.filter-stores input[name=search]').val(),
                AreaOfOperation: $('#country-search').val(),
                categoryIds: (categoryIds ? categoryIds.toString() : ""),
                // will change limitMyconnections to comminity store-filter late bugx
                limitMyConnections: $('#community-stores-filter input[name=LimitMyConnections]').prop("checked"),
                IsAllPublicStoreShown: $("#show-all-store-btn").css('display') == 'none'
            },
            beforeSend: function () {
                $shopping_tab_content.LoadingOverlay('show');
                // $feature_store.LoadingOverlay('show');
                toggleBtnInStores();
            }
        },
        callback: function (data, pagination) {
            $('#stores').addClass("active fade in");

            var extraCol = (count % 4 == 0 ? 0 : 4) - count % 4;
            var uri = $('#api-uri').val();
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += businessTemplate(item, uri);
            });
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<article class="col" style="box-shadow: none; background: transparent;"></article>';
            }

            $data_container_business.html(dataHtml);
            var showStoreHtml = '';
            if (numberStores > 0) {
                var showStoreHtml = '<div><h5>' + numberStores + ' store(s) found</h5></div>';
            }
            $storesresults.html(showStoreHtml);

            // Hide the Catalog list tab if shop detail is shown initially
            if (isShopDetailBeingShown == true) {
                $("#stores").removeClass("active fade in");
                isShopDetailBeingShown = false;
            }

            toggleBtnInStores();
        }
    })
    updateTotalSelectedCategory();
}
function businessTemplate(data, uri) {
    var _html = '<article class="col">';
    _html += '<a href="#business-catalogues" data-toggle="tab" aria-expanded="true" onclick="loadB2CBusinessCatalogues(\'' + data.DomainKey + '\')"><div class="avatar" style="background-image: url(\'' + data.LogoUri + '&size=T\');">&nbsp;</div>';
    _html += '<h1 style="color: #333;">' + data.BusinessName + '</h1>';
    _html += '</a>';
    _html += '<p class="qbicle-detail">' + data.BusinessSummary + '</p>';
    _html += '<div style="padding: 10px 0;">';
    _.forEach(data.Tags, function (tag) {
        _html += '<label class="label label-lg label-soft">' + tag + '</label> ';
    });
    _html += '</div>';
    _html += '</article>';
    return _html;
}
function loadB2CBusinessCatalogues(dmkey) {
    var $businessCatalogues = $('#business-catalogues');
    $businessCatalogues.empty();
    $businessCatalogues.LoadingOverlay('show');
    $businessCatalogues.load("/B2C/LoadB2CBusinessCatalogues", { domainKey: dmkey, isLoadAll: false }, function () {
        $businessCatalogues.LoadingOverlay('hide');
    });
}

function loadB2CBusinessCatalogDetail(catalogKey) {
    var $businessCatalogDetail = $('#business-catalog-detail');
    $businessCatalogDetail.empty();
    $businessCatalogDetail.LoadingOverlay('show');
    $businessCatalogDetail.load("/B2C/LoadB2CBusinessCatalogDetail", { catalogKey: catalogKey }, function () {
        $businessCatalogDetail.LoadingOverlay('hide');
    });
}

function loadB2CBusinessCatalogDetailForProductTab(catalogKey) {
    var $businessCatalogDetail = $('#business-catalog-detail');

    $('#products').removeClass('active in');
    $businessCatalogDetail.addClass('active in');

    $businessCatalogDetail.empty();
    $businessCatalogDetail.LoadingOverlay('show');
    $businessCatalogDetail.load("/B2C/LoadB2CBusinessCatalogDetail", { catalogKey: catalogKey }, function () {
        $businessCatalogDetail.LoadingOverlay('hide');
        $("#catalog-detail-back-btn").attr('data-target', '#products');
    });
}

function createB2COrderDiscussion(businessDomainKey, catalogId) {
    $.LoadingOverlay('show');
    $.post("/B2C/CreateB2COrderDiscussion", { businessDomainKey: businessDomainKey, catalogId: catalogId }, function (response) {
        if (response.result) {
            location.href = "/B2C/DiscussionOrder?disKey=" + response.msgId;
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "B2C");
            LoadingOverlayEnd();
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2C");
            LoadingOverlayEnd();
        }
        //LoadingOverlayEnd();
    });
}
//C2C Qbicle stream
function timeline() {
    $(window).scrollTop(0);
    //if (previousShown == false) {
    //    loadingNewC2C();

    //    previousShown = true;
    //    return previousShown;
    //}
    $(window).scroll(function () {
       
        if (//$(window).scrollTop() >= ($(document).height() - $(window).height() - 100)
            !isC2CAllContactShown && $("ul.subapps-nav li.active.tab-activities").length > 0) {
            if (previousShown == false) {
                loadingNewC2C();

                previousShown = true;
                return previousShown;
            }
        }
    });
}
function loadMoreActivitiesC2C(isFilter) {

    if (isBusy) {
        return;
    }
    var _activityTypes = [];
    var _topicIds = [];
    var _apps = [];
    if ($('#select-activity').val() != "0")
        _activityTypes.push($('#select-activity').val());

    var fillterModel = {
        Key: _c2cQbiceId,
        Size: loadCountActivity * qbiclePageSize,
        ActivityTypes: _activityTypes,
        TopicIds: _topicIds,
        Apps: _apps,
        Daterange: $('#txtFilterDaterange').val()
    };
    var url = "/C2C/LoadMoreC2CActivities";
    var $c2citem = $('ul.widget-contacts li.active');
    var _type = $c2citem ? $c2citem.data('type') : 2;
    $.ajax({
        url: url,
        data: {
            fillterModel: fillterModel,
            type: _type
        },
        type: "POST",
        async: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.isHidden) {
                _c2cQbiceId = 0;
                loadC2CQbicleContent(true);
                cleanBookNotification.warning(_L('WARNING_MSG_REMOVECONTACT'), "Community");
                setTimeout(function () {
                    $("ul.widget-contacts > li:first-child > a").click();
                }, 500);
                return;
            }
            $('#previous').empty();
            if (isFirstLoad == 0) {
                loadModalC2CActivities();
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
    });
    loadCountActivity = loadCountActivity + 1;
};
function showQbicleStream() {
    $('#first-load-icon').hide();
    $("#latch").show();
    $('#dashboard-page-display').show();
}
function loadingNewC2C() {
    //alert('loadingNewC2C');
    if ($('#previous div.text-center'))
        $('#previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
    setTimeout(function () {
        loadMoreActivitiesC2C();
    }, 500);
}
function loadDataDashboardC2C(isFilter) {
    //alert('loadDataDashboardC2C');
    if (isFilter) {
        $(window).scrollTop(0);
        loadCountActivity = 0;
        isFirstLoad = 0;
        $('#dashboard-page-display').hide();
        $('#first-load-icon').show();
    }
    setTimeout(function () {
        loadMoreActivitiesC2C(isFilter);

        onCommunityConnected();
    }, 100);
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
    loadDataDashboardC2C(true);
}

function loadModalC2CActivities() {
    var $c2citem = $('ul.widget-contacts li.active');
    if ($c2citem.length > 0 && $c2citem.data('status') == 'Approved') {
        $('#modal-activities').load("/C2C/LoadModalActivities", function () {
            $('#modal-activities select.select2').not('select.select2-hidden-accessible').select2({
                placeholder: 'Please select'
            });
            $('#modal-activities input[data-toggle="toggle"]').bootstrapToggle();
            initNextPreviousTab('#form_task_addedit', '#taskTabs');
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
function loadC2CStatusInfo() {
    var $commswaitingapprove = $('#block-content-approve');
    //$commswaitingapprove.empty();
    $commswaitingapprove.load("/C2C/LoadC2CQbicleStatusInfo", { key: _c2cQbiceId }, function () {
        onCommunityConnected();
        //$commswaitingapprove.LoadingOverlay('hide');
    });
}
function initC2cQbicleDefaultActive() {
    var elmid = $('ul.widget-contacts > li.active > a').attr('href');
    if (elmid == "#comms-waiting-approve") {
        $('#comms-activities').removeClass('in active');
        $(elmid).addClass('in active');
    } else {
        $('#comms-waiting-approve').removeClass('in active');
        $(elmid).addClass('in active');
    }

}
function getC2CTalkNum() {
    var _url = "/C2C/GetCommunityTalkNum";
    var keyword = $('#filters-contacts input[name=search]').val();
    var contactType = $('#filters-contacts select[name=contactType]').val();

    $.ajax({
        type: 'GET',
        dataType: 'JSON',
        url: _url,
        data: {
            keyword: keyword,
            contactType: contactType
        },
        success: function (data) {
            $("#connection-shown-type a[name=all]").text('All (' + data.allNum + ')');
            $("#connection-shown-type a[name=favourite]").text('Favourites (' + data.favouriteNum + ')');
            $("#connection-shown-type a[name=request] #count").text(data.requestNum);
            $("#connection-shown-type a[name=sent]").text('Sent (' + data.sentNum + ')');
            $("#connection-shown-type a[name=blocked]").text('Blocked (' + data.blockedNum + ')');
        },
        error: function (err) {

        }
    });
}
function setCommunityTalkViewed(key, type) {
    var _url = "/C2C/SetC2CConnectionViewedStatusAndGetShoppingAbility";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            'key': key,
            'type': type
        },
        success: function (response) {
            if (response.result) {
                $('li[data-c2cqbicleid="' + key + '"] .comms-newstuff').attr("hidden", "hidden");

                // Check the Shopping Ability
                if (type == 1) {
                    var responseObj = response.Object;
                    if (responseObj == null || responseObj.IsB2CActive == false) {
                        $(".business-action").hide();
                    } else {
                        $(".business-action").show();
                    }

                    if (responseObj == null || !responseObj.HasPublicCatalog) {
                        $(".business-action > button").addClass('disabled');
                        $(".business-action").attr('data-tooltip', 'No shopping catalogs available');
                        html5tooltips.refresh();
                    } else {
                        $(".business-action > button").removeClass('disabled');
                        $(".business-action").removeAttr('data-tooltip');
                        html5tooltips.refresh();
                    }
                }
            }
        }
    });
}
function setQbicleRemoved(key) {
    var _url = "/C2C/RemoveQbicleFromUser?qKey=" + key;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                loadC2CQbicleContent(true);
                cleanBookNotification.updateSuccess("Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
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


function browseB2CCatalog() {
    var $b2cCatalogues = $('#b2c-catalogues');
    $b2cCatalogues.LoadingOverlay('show');
    $b2cCatalogues.empty();
    $b2cCatalogues.load("/B2C/LoadB2CCatalogues", { domainKey: $domainKey }, function () {
        $b2cCatalogues.LoadingOverlay('hide');
        $('#b2c-catalogues').modal('toggle');
        loadB2CCatalogues($domainKey);
    });
}

function loadB2CCatalogDetail(catalogKey) {
    var $b2cCataloguesDetail = $('#business-catalog-detail');
    $b2cCataloguesDetail.LoadingOverlay('show');
    $b2cCataloguesDetail.empty();
    $b2cCataloguesDetail.load("/B2C/LoadB2CBusinessCatalogDetail", { catalogKey: catalogKey }, function () {
        $("ul.subapps-nav li.active").removeClass("active");
        $('ul li.shopping-li').addClass("active");

        $('.options').hide();
        $('.options-shopping').fadeIn();

        $("#c2c-comms").removeClass("active fade in");

        $('#shopping').addClass("active fade in");

        $('#business-catalogues').removeClass("active fade in");

        $("#stores").removeClass("active fade in");

        $("#business-catalog-detail").addClass("active fade in");

        $('#b2c-catalogues').modal('toggle');
        $b2cCataloguesDetail.LoadingOverlay('hide');
    });
}

function loadB2CCatalogues(dmkey) {
    var $b2cCatalogues = $('#business-catalogues');
    $b2cCatalogues.empty();
    $b2cCatalogues.load("/B2C/LoadB2CBusinessCatalogues", { domainKey: dmkey, isLoadAll: false }, function () {

    });
}

//End C2C Qbicle
// search products
function productTemplate(data) {
    var _html = '<tr role="row"><td>';
    _html += '<a href="javascript:void(0)"><div class="dtitemimg" onclick="loadB2CBusinessCatalogDetailForProductTab(\'' + data.Key + '\')" style="background-image: url(\'' + data.ProductImage + '\');"></div></a>'
    _html += '</td>';
    _html += '<td class="sorting_1">'
    _html += '<h5><a href="javascript:void(0)" onclick="loadB2CBusinessCatalogDetailForProductTab(\'' + data.Key + '\')" >' + data.ProductName + '</a></h5>'
    _html += '<small>' + data.ProductSKU + '</small>'
    _html += '</td>';
    _html += '<td class="provider">'
    _html += '<a href="javascript:void(0)" onclock="loadB2CBusinessCatalogues(\'' + data.DomainKeyEncrypt + '\')">' + data.ProviderName + '</a>'
    _html += '<h5 class="hide">' + data.Brand + '</h5>'
    _html += '</td>';
    _html += '<td class="price">'
    _html += '' + data.ProductPrice + ''
    _html += '</td>';
    _html += '<td class="text-right">'
    _html += '<a href="javascript:void(0)" class="btn btn-success community-button sm w-auto" onclick="loadB2CBusinessCatalogDetailForProductTab(\'' + data.Key + '\')">Shop now</a>'
    _html += '</td>';
    _html += '</tr>';
    return _html;
}

function listBrandTemplate(data, index) {
    var _brand = '<option value="' + index + '">' + data.NameDomain + '</option>'
    return _brand;
}
// rewrite new funcs findProduct
function initFindProducts() {
    searchproducts();
    updateTotalSelectedProduct();
    // LoadShoppingProductListDT
    // Getting data for FPProduct DataTable
    var dataTable = $("#products-results-dttable")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#products-results-dttable').LoadingOverlay("show");
            } else {
                $('#products-results-dttable').LoadingOverlay("hide", true);
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
            "ajax": {
                "url": '/C2C/LoadShoppingProductListDT',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#psearching").val(),
                        "countryName": $("#pcountry-selector").val(),
                        "lstBrandIds": $("#listBrands").val(),
                        "lstProductTags": FilterByListInTagify()
                    });
                }
            },
            "columns": [
                {
                    data: "CategoryItemId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";

                        htmlStr += '<a href="/Commerce/PublishBusinessProfile?id=' + row.BusinessId + '">';
                        htmlStr += ' <div class="dtitemimg" style="background-image: url(\'' + row.CatItemImageUri + '\');">';
                        htmlStr += '</div>'
                        htmlStr += '</a>';

                        return htmlStr;
                    }
                },
                {
                    data: "CategoryItemId",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<h5>';
                        htmlStr += '<a href="/Commerce/PublishBusinessProfile?id=' + row.BusinessId + '">' + row.CategoryItemName + '</a>';
                        htmlStr += '</h5>';
                        htmlStr += '<small>' + row.SKU + '</small>';
                        return htmlStr;
                    }
                },
                {
                    data: "B2BProfileBusinessName",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<a href="/Commerce/PublishBusinessProfile?id=' + row.BusinessId + '">' + row.B2BProfileBusinessName + '</a>';
                        return htmlStr;
                    }
                },
                {
                    data: "Price",
                    orderable: true
                },
                {
                    data: "CatalogId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";

                        htmlStr += '<a href="javascript:void(0)" onclick="loadB2CBusinessCatalogDetailForProductTab(\'' + row.CatalogKey + '\')" class="btn btn-success community-button sm w-auto">Shop now</a>'

                        return htmlStr;
                    }
                }
            ]
        });

    $('#products-results').LoadingOverlay("hide");
    dynamicWhitelistTagify();
}
var backupBrands;


function intitDatatableAgain() {
    //can't find the original setting of Datatable, so destroy it and create another datatable with new settings
    $("#products-results .dtproducts").DataTable().destroy();
    $("#products-results .dtproducts").DataTable({
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "search": {
            // matching multiple keywords a|b|c|d
            regex: true,
            smart: false
        }
    })
}

function dynamicListBrandMultiSelect(brands) {
    var listBrands = '';
    $.each(brands, function (index, item) {
        listBrands += listBrandTemplate(item, index);
    })
    $("#community-products-filter #listBrands").html(listBrands);
    $("#community-products-filter #listBrands").multiselect('rebuild');
}

function InitSplashTab() {
    $("#products-splash").LoadingOverlay('show');

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: '/C2C/GetFeaturedProductForShoppingTab',
        success: function (data) {
            if (data != null) {
                $("#products-splash-content").empty();
                var contentStr = "";
                data.forEach(function (item) {
                    if (item.Type == 1) { // Image
                        contentStr += SplashFImageTemplate(item);
                    }
                    else if (item.Type == 2) { //Product
                        contentStr += SplashFProductTemplate(item);
                    }
                });
                $("#products-splash-content").html(contentStr);
            }
        }
    }).then(function () {
        $("#products-splash").LoadingOverlay('hide');
    })
}

function SplashFProductTemplate(item) {
    var htmlString = "";

    htmlString += '<div class="col">';
    htmlString += '<a href="javascript:void(0)" onclick="loadB2CBusinessCatalogDetailForProductTab(\'' + item.AssociatedCatalogKey + '\')">';
    htmlString += '<div class="productimg" style="margin-bottom: 5px; background-image: url(\'' + item.LogoImageUri + '\'); border-radius: 5px; overflow: hidden;"></div>';
    htmlString += '<div class="whom">';
    htmlString += '<div class="avatarc" style="background-image: url(\'' + item.BusinessLogo + '\');">&nbsp;</div>';
    htmlString += '<div class="whominfo text-left">';
    htmlString += '<h5 style="color: #333;">' + item.CategoryItemName + '</h5>';
    htmlString += '<small>' + item.BusinessName + '</small>';
    htmlString += '</div>';
    htmlString += '<div class="price">';
    htmlString += '<h5>' + item.PriceStr + '</h5>';
    htmlString += '</div>';
    htmlString += '</div>';
    htmlString += '</a>';
    htmlString += '</div>';

    return htmlString;
}

function SplashFImageTemplate(item) {
    var htmlString = "";
    htmlString += '<div class="col">';

    if (item.FeaturedImageURL == "") {
        htmlString += '<div class="commsimgfeature" style="background-image: url(\'' + item.LogoImageUri + '\');"></div>';
    }
    else {
        htmlString += '<a href="' + item.FeaturedImageURL + '" target="_blank">';
        htmlString += '<div class="commsimgfeature" style="background-image: url(\'' + item.LogoImageUri + '\');"></div>';
        htmlString += '</a>';
    }

    htmlString += '</div>';
    return htmlString;
}
