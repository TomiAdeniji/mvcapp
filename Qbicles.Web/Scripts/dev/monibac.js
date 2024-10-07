var businessTotalLoad = 0,
    promotionTotalRecords = 0,
    promotionTotalLoad = 0,
    promotionfirstLoad = true,
    promotionPagenumber = 6,
    loadCountVourcher = 0,
    vourcherPagenumber = 4,
    vourcherTotalRecords = 0,
    previousShown = false;
var currencySetting, isBusy = false;
var wto;
var isSettingLikingUser = false;
var isSettingIsLike = false;

function initMonibacManagePage() {
    loadConnectedBusinesses(true);

    $("#business-keysearch").keyup(delay(function () {
        loadConnectedBusinesses(false);
    }, 1000));
    initPromotionFilterEvent();
    promotionsLoadMore();
    $('#moniback-get-code').on('hidden.bs.modal', function (e) {
        promotionsLoadMore();
        loadConnectedBusinesses(false);
    })
}

function loadConnectedBusinesses(isLoadMore) {
    var $lstBusinessTab = $("#connected-businesses");
    var keysearch = $("#business-keysearch").val();
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    if (isLoadMore) {
        businessTotalLoad += 10;
    }
    var _url = "/Monibac/ShowListConnectedBusiness?totalLoad=" + businessTotalLoad + "&keysearch=" + keysearch;
    $lstBusinessTab.load(_url);
    $leftsideTab.LoadingOverlay("hide");
}

function showBusinessContent(contactkey) {
    var _url = "/Monibac/GetMonibacInfor?contactKey=" + contactkey;
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    $("#store-detail").empty();
    $("#store-detail").load(_url);
    leftSideShows("#store-detail");
    $leftsideTab.LoadingOverlay("hide");
}

function showPointExchangePartial(contactkey) {
    var _url = "/Monibac/ShowPointExchangePartialView?contactKey=" + contactkey;
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    var $exchangePartial = $("#point-exchange-detail");
    $exchangePartial.empty();
    $exchangePartial.load(_url);
    leftSideShows("#point-exchange-detail");
    $leftsideTab.LoadingOverlay("hide");
}

function inputPointExchangeOnChange() {
    if (!currencySetting) {
        loadCurrencySettings();
    }
    var exchangeRate = $("#exchange-rate").val();
    var pointBalance = Number($("#point-balance").val());
    var inputPoint = Number($("#exchange-point").val());
    if (inputPoint > pointBalance) {
        $("#exchange-point").val(pointBalance);
        inputPoint = pointBalance;
    };
    var pointReceived = Number(inputPoint * exchangeRate);
    var storePointAccountBalance = $("#credit-balance").val();
    var newCreditBalance = pointReceived + Number(storePointAccountBalance);
    $("#amount-after-exchange").text(currencySetting.CurrencySymbol + FormatNumber(newCreditBalance), currencySetting.CurrencySymbol);
    $("#credit-received").text('+' + FormatNumber(pointReceived), "");

}

function loadCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettingsByDomain",
        type: "get",
        async: false,
        data: {
            domainId: $("#contact-domainid").val()
        },
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
                    currencyGroupSeparator: ',',
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

function FormatNumber(number, currency) {
    if (!currencySetting) {
        loadCurrencySettings();
    }

    if (!currency) currency = "";

    if (!number) { number = 0; }
    else { number = parseFloat(number.toString()); }

    if (isNaN(number)) number = 0;

    return currency + number.toFixed(currencySetting.DecimalPlace).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}

function showAccountBalanceExchangePartial(contactkey) {
    var _url = "/Monibac/ShowAccountBalanceExchangePartialView?contactKey=" + contactkey;
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    var $exchangePartial = $("#point-exchange-detail");
    $exchangePartial.empty();
    $exchangePartial.load(_url);
    leftSideShows("#point-exchange-detail");
    $leftsideTab.LoadingOverlay("hide");
}

function inputAccBalanceExchangeOnChange() {
    if (!currencySetting) {
        loadCurrencySettings();
    }
    var accBalance = Number($("#account-balance").val());
    var inputBalance = Number($("#balance-exchange").val());
    if (inputBalance > accBalance) {
        $("#balance-exchange").val(accBalance);
        inputBalance = accBalance;
    };
    var balanceReceived = Number(inputBalance);
    var storePointAccountBalance = $("#credit-balance").val();
    var newCreditBalance = balanceReceived + Number(storePointAccountBalance);
    $("#amount-after-exchange").text(currencySetting.CurrencySymbol + FormatNumber(newCreditBalance), currencySetting.CurrencySymbol);
    $("#credit-received").text('+' + FormatNumber(balanceReceived), "");

}

function convertAccountBalanceToStoreCredit(contactkey) {
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");

    var balance = $("#balance-exchange").val();
    var _url = "/Monibac/GenerateStoreCreditFromAccountBalance";
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            contactKey: contactkey,
            exchangeBalance: balance
        },
        success: function (response) {
            if (response.result) {
                var newPIN = response.Object;
                $("#pin-num").text(newPIN);
                cleanBookNotification.updateSuccess();
                showAccountBalanceExchangePartial(contactkey);
                $leftsideTab.LoadingOverlay("hide");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                $leftsideTab.LoadingOverlay("hide");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
            $leftsideTab.LoadingOverlay("hide");
        }
    });
}

function convertStorePointToStoreCredit(contactkey) {
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");

    var point = $("#exchange-point").val();
    var _url = "/Monibac/GenerateStoreCreditFromStorePoint";
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            contactKey: contactkey,
            exchangePoint: point
        },
        success: function (response) {
            if (response.result) {
                var newPIN = response.Object;
                $("#pin-num").text(newPIN);
                cleanBookNotification.updateSuccess();
                showPointExchangePartial(contactkey);
                $leftsideTab.LoadingOverlay("hide");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                $leftsideTab.LoadingOverlay("hide");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
            $leftsideTab.LoadingOverlay("hide");
        }
    });
}

function generatePIN() {
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    var _url = "/Monibac/GenerateStoreCreditPIN?createdReason=0";
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                var newPIN = response.Object;
                $("#pin-num").text(newPIN);
                cleanBookNotification.updateSuccess();
                $leftsideTab.LoadingOverlay("hide");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                $leftsideTab.LoadingOverlay("hide");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            $leftsideTab.LoadingOverlay("hide");
        }
    });
}

function promotionsLoadMore() {
    if (isBusy)
        return;
    var filterModel = {
        keyword: $('#promotion-filters input[name=search]').val(),
        businessKey: $('#promotion-filters select[name=business]').val(),
        pageSize: promotionTotalLoad * promotionPagenumber,
        pageNumber: promotionPagenumber,
        isMyFavourites: $('#promotion-filters input[name=ismyfavourites]').prop('checked'),
        isLoadTotalRecord: false,
        locationIds: $('#promotion-filters select[name=locations]').val(),
    };
    if (promotionfirstLoad) {
        filterModel.isLoadTotalRecord = true;
    }
    if (!filterModel.locationIds)
        filterModel.locationIds = [];
    $('#rewards').LoadingOverlay('show');
    $.ajax({
        url: "/Monibac/LoadPublishPromotions",
        data: {
            filterModel: filterModel
        },
        type: "POST",
        //async: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data) {
                $('#content-promotion').append(data.htmlContent);
                if (promotionfirstLoad) {
                    promotionTotalRecords = data.totalRecords;
                    promotionfirstLoad = false;
                }
                
                promotionTotalLoad = promotionTotalLoad + 1;
                var page = promotionTotalRecords - (promotionTotalLoad * promotionPagenumber);
                if (page <= 0)
                    $('.btn-loadmore').hide();
                else
                    $('.btn-loadmore').show();
                initCountDown();
            }
            $('#rewards').LoadingOverlay('hide',true);
        },
        error: function (xhr, status, error) {
            isBusy = false;
            $('#rewards').LoadingOverlay('hide', true);
        }
    });
}
function initPromotionFilterEvent() {
    $('#promotion-filters input[name=search]').keyup(delay(function () {
        resetPromotionLoad();
        promotionsLoadMore();
    }, 700));
    $('#promotion-filters select[name=business]').change(function () {
        resetPromotionLoad();
        loadLocationsByBusinessKey($(this).val());
    });
    $('#promotion-filters select[name=locations]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            resetPromotionLoad();
            promotionsLoadMore();
        }, 1000);
    });
    $('#promotion-filters input[name=ismyfavourites]').change(function () {
        resetPromotionLoad();
        promotionsLoadMore();
    });
    
}
function loadLocationsByBusinessKey(businessKey) {
    if (businessKey != "0") {
        $('.filter-local-box').show();
        $.get("/Monibac/LoadLocationsByBusinessKey?businessKey=" + businessKey, function (data) {
            $("#promotion-filters select[name=locations]").multiselect('dataprovider', data);
            promotionsLoadMore();
        });
    } else {
        $('.filter-local-box').hide();
        $("#promotion-filters select[name=locations]").multiselect('dataprovider', []);
        promotionsLoadMore();
    }
        
    
}
function claimPromotion(promotionKey, key, id) {
    $.post("/Monibac/ClaimPromotion", { promotionKey: promotionKey, businessKey: key}, function (response) {
        if (response.result) {
            var promoinfo = response.Object;
            if (promoinfo.canClaim == false) {
                $("#button-claim-" + promotionKey).attr('disabled', 'disabled');
                $("#button-claim-" + promotionKey).removeAttr("onclick");
            }
            var vouchersValid = parseInt($("#vouchers-valid-" + id).text()) + 1;
            $("#vouchers-valid-" + id).text(vouchersValid);

            $('#moniback-get-code').modal('show');
            $('.promotion-name').text(promoinfo.promoname);
            $('.promotion-code-received').text(promoinfo.promoreceived);
            $('.promotion-code').val(promoinfo.code);
            $('.promotion-locations').text(promoinfo.promolocations);
            $('.promotion-maxvoucher').text(promoinfo.promolimitvoucherpercus);
            if ($('#widget-codes-detail:visible').length > 0) {
                searchVouchers(true);
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Moniback");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Moniback");
        }
    });
}
function setLikingUser(elm,promotionKey, isLiked) {
    $.post("/Monibac/SetLikingUser", { promotionKey: promotionKey, IsLiked: isLiked }, function (response) {
        if (response.result) {
            if (isLiked) {
                $(elm).addClass("red");
                $(elm).attr('onclick', 'setLikingUser(this,\'' + promotionKey + '\',' + !isLiked+')');
            } else {
                $(elm).removeClass("red");
                $(elm).attr('onclick', 'setLikingUser(this,\'' + promotionKey + '\',' + !isLiked + ')');
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Monibac");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Monibac");
        }
    });
}
function markLikePromotion(elm, promotionKey, markedLiked) {
    if (isSettingIsLike == true)
        return;
    isSettingIsLike = true;

    $.post("/Monibac/MarkLikePromotion", { promotionKey: promotionKey, IsLiked: markedLiked }, function (response) {
        isSettingIsLike = false;
        var likedTime = $(elm).closest('.post-options').find('.liked-count').text();
        if (response.result) {
            if (markedLiked) {
                $(elm).removeClass("fa fa-heart-o");
                $(elm).addClass("fa fa-heart");
                $(elm).attr('onclick', 'markLikePromotion(this,\'' + promotionKey + '\',' + !markedLiked + ')');
                $(elm).closest('.post-options').find('.liked-count').text(Number(likedTime) + 1);
            } else {
                $(elm).removeClass("fa fa-heart");
                $(elm).addClass("fa fa-heart-o");
                $(elm).attr('onclick', 'markLikePromotion(this,\'' + promotionKey + '\',' + !markedLiked + ')');
                $(elm).closest('.post-options').find('.liked-count').text(Number(likedTime) - 1);
            }
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Monibac");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Monibac");
        }
    });
}
function resetPromotionLoad() {
    promotionfirstLoad = true;
    promotionTotalLoad = 0;
    $('#content-promotion').empty();
}
function copyToClipboard(val) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(val).select();
    document.execCommand("copy");
    $temp.remove();
}
function initCountDown() {
    $(".promotion-countdown").each(function () {
        var $currentElm = $(this);
        var finalDate = new Date($currentElm.data('timestamp'));
        $currentElm.countdown(finalDate, function (event) {
            var day = '';
            if (event.offset.totalDays > 0)
                day = '%-Dd, ';
            $(this).text(
                event.strftime(day+'%-Hh, %-Mm, %Ss remaining')
            );
        }).on('finish.countdown', function () {
            // render something
            var $button = $(this).parents('.deal-promo').find('button');
            if ($button.length > 0)
                $button.attr('disabled', true);
            $(this).parent().html('<span style="top:0;">Offer expired</span>');
            //alert('het hạn');
        });

    });

   
}
function showWidgetCodes(businessKey) {
    var _url = "/Monibac/ShowWidgetCodesPartialView?businessKey=" + businessKey;
    var $leftsideTab = $("#app-moniback-widget");
    $leftsideTab.LoadingOverlay("show");
    $("#widget-codes-detail").empty();
    $("#widget-codes-detail").load(_url, function () {
        leftSideShows('#widget-codes-detail');
        searchVouchers(true);
        initVoucherFilterEvent();
        $leftsideTab.LoadingOverlay("hide");
    });
   
}
function initVoucherFilterEvent() {
    $('#widget-codes-detail input[name=search]').keyup(delay(function () {
        searchVouchers(true);
    }, 700));
    $('#widget-codes-detail input[name=Redeemed]').change(function () {
        searchVouchers(true);
    });
    $('#widget-codes-detail input[name=Expired]').change(function () {
        searchVouchers(true);
    });
    $('#widget-codes-detail input[data-toggle="toggle"]').bootstrapToggle();
}
function showVoucherItemMore(voucherKey) {
    var _url = "/Monibac/GetVoucherItemMore?voucherKey=" + voucherKey;
    var $voucheritemmore = $("#voucher-item-more");
    $voucheritemmore.empty();
    $voucheritemmore.modal("show");
    $voucheritemmore.load(_url, function () {
        initCountDown();
    });
}
function removeVoucher(voucherKey,elm) {
    $.post("/Monibac/RemoveVoucher", { voucherKey: voucherKey }, function (response) {
        if (response.result) {
            $(elm).parent().parent().parent().remove();
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Moniback");
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Moniback");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Moniback");
        }
    });
}

function searchVouchers(isFilter) {
    if (isFilter) {
        $('#mobiback-left-sidebar').scrollTop(0);
        loadCountVourcher = 0;
    }
    setTimeout(function () {
        loadMoreVourchers(isFilter);
    }, 200);
}
function loadMoreVourchers(isFilter) {
    if (isBusy) {
        return;
    }
    var fillterModel = {
        isRedeemed: $('#widget-codes-detail input[name=Redeemed]').prop('checked'),
        isExpired: $('#widget-codes-detail input[name=Expired]').prop('checked'),
        domainkey: $('#hdfDomainKey').val(),
        keyword: $('#widget-codes-detail input[name=search]').val(),
        pageSize: loadCountVourcher * vourcherPagenumber,
        pageNumber: vourcherPagenumber,
        isCountRecords: false
    };
    if (isFilter) {
        loadCountVourcher = 0;
        fillterModel.isCountRecords= true;
    }
    var url = "/Monibac/GetVouchersByUserAndShop";

    $.ajax({
        url: url,
        data: {
            filterModel: fillterModel
        },
        type: "POST",
        async: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data) {
                if (isFilter) {
                    $('.voucherlist').empty();
                }
                $('.voucherlist').append(data.htmlContent);
                if (fillterModel.isCountRecords) {
                    vourcherTotalRecords = data.totalRecords;
                }

                loadCountVourcher = loadCountVourcher + 1;
                var currentItems = (loadCountVourcher * vourcherPagenumber);
                var page = vourcherTotalRecords - currentItems;
                if (page <= 0) {
                    previousShown = true;
                    $("#more-voucher-btn").addClass("hidden");
                }
                else {
                    previousShown = false;
                    $("#more-voucher-btn").removeClass("hidden");
                }
                $('.pager-info').html('Showing <strong>1-' + (page <= 0 ? vourcherTotalRecords:currentItems)+'</strong> of <strong>' + vourcherTotalRecords+'</strong>');
            }
        },
        error: function (xhr, status, error) {
            isBusy = false;
        }
    });
};
function leftSideShows(widgetselector) {
    $(".widget-content").hide();
    $(widgetselector).fadeIn();
}
function ShowSharingPromotionPartialView(promotionKey) {
    LoadingOverlay();
    var _url = '/Monibac/PromotionSharePartialView';
    $("#share-content").empty();
    $("#share-content").load(_url, { 'promotionKey': promotionKey });
    $("#share-content").modal('show');
    LoadingOverlayEnd();
}
function SharePromotion(type) {
    var sharedPromotionKey = $("#shared-promotion-key").val();
    
    var _url = "/Monibac/SharePromotion";
    var selectedUserIds = [];
    var email = "";
    if (type == 1) {
        $(".selected-contact").each(function () {
            selectedUserIds.push($(this).attr('userid'));
        })
    } else if (type == 2) {
        email = $("input[name=new-member]").val();
    }
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            'type': type,
            'email': email,
            'sharedUserIds': JSON.stringify(selectedUserIds),
            'promotionKey': sharedPromotionKey
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("This promotion has been successfully shared with your chosen contacts", "Qbicles");
                $("#share-content").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error("There was a problem sharing this promotion. Please try again", "Qbicles");
        }
    }).done(function () {
        LoadingOverlayEnd();
    })
}