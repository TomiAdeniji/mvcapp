
var loadingoverlay_value = "<div style=\"background-color: rgba(255, 255, 255, 0.8);position: fixed;display: flex;flex-direction: column;align-items: center;justify-content: center;z-index: 2147483647;background-image: url('/Content/DesignStyle/img/loadingnew.gif');background-position: center center;background-repeat: no-repeat;top: 0px;left: 0px;width: 100%;height: 100%;background-size: 100px;\"></div>";
var showLog = false;
var currencySetting;

function checkfile(fileName) {
    var validExts = JSON.parse($("#listIDAccept").val());
    var fileExt = fileName.substring(fileName.lastIndexOf(".")).replace(".", "").toLowerCase();
    var rs = { err: _L("ERROR_MSG_2"), stt: false };
    if (validExts.indexOf(fileExt) < 0) {
        rs.err = _L("ERROR_MSG_3", [validExts.join(", ")]);
        rs.stt = false;
    } else {
        rs.err = "Success";
        rs.stt = true;
    }
    return rs;
}

function getDateTimeFormat() {
    return $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser;
}

function convertStringToDate(value, format) {
    if (!format) format = $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser;
    else {
        var date = format.split(' ')[0].toUpperCase();
        var time = format.split(' ').length > 1 ? (' ' + format.split(' ')[1]) : '';
        format = date + time;
    }
    return moment(value, format).format($systemFormat);
}

function convertStringToDateFormat(value, formatInput, formatOutput) {
    var _sdate = value;
    if (value)
        value = value.split(' ');
    var lastcharacter = value[0].indexOf(",") > 1 ? "," : "";
    if (value[0].includes("Today"))
        return _sdate;
    if (formatOutput)
        return moment(value[0], formatInput).format(formatOutput) + lastcharacter + (value[1] ? " " + value[1] : "") + (value[2] ? " " + value[2] : "");
    else
        return moment(value[0], formatInput).format($systemFormat) + lastcharacter + (value[1] ? " " + value[1] : "") + (value[2] ? " " + value[2] : "");
}

function checkVersionfileUpload(fileName, filetype) {
    var fileExt = fileName.substring(fileName.lastIndexOf(".")).toLowerCase();
    var rs = { err: _L("ERROR_MSG_2"), stt: false };
    if (fileExt !== filetype) {
        rs.err = _L("ERROR_MSG_3", [filetype]);
        rs.stt = false;
    } else {
        rs.err = "Success";
        rs.stt = true;
    }
    return rs;
}

function getLimitFileSize(fileName) {
    var _fileInfo;
    if (fileName) {
        var ext = fileName.substr((fileName.lastIndexOf('.') + 1));
        $.ajax({
            type: "GET",
            url: "/Qbicles/getFileTypeInfo",
            data: { ext: ext.toLowerCase() },
            dataType: 'json',
            cache: false,
            async: false,
            success: function (data) {
                _fileInfo = data;
            }
        });
    }
    return _fileInfo;
}

function validateEmail(email) {
    var re =
        /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

// pin or unpin the activity
function PinnedActivity(Id, IsPost, event) {
    if (event)
        event.stopPropagation();
    var url = "/MyDesks/PinnedActivity/";
    $.ajax({
        url: url,
        data: { ActivityId: Id, IsPost: IsPost },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                var pinPerfix = "-";
                if (IsPost === true) {
                    pinPerfix = "Post-";
                }
                if ($("#pinIcon" + pinPerfix + Id).hasClass("pinned")) {
                    $("#pinIcon" + pinPerfix + Id).removeClass("pinned");
                } else {
                    $("#pinIcon" + pinPerfix + Id).addClass("pinned");
                }
                $("#pinIconNotification" + pinPerfix + Id).toggleClass("fa-thumb-tack");
                $("#pinIconNotification" + pinPerfix + Id).toggleClass("fa-check green");
                $("#pinTitleNotification" + pinPerfix + Id).toggleText("Pin this", "Unpin this");
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");

        }
    });

}

function MarkAsReadNotification(notificationId, event) {

    //if (event) {
    //    event.preventDefault();
    //    event.stopPropagation();
    //}

    var url = "/Notifications/MarkAsReadNotification/";
    $.ajax({
        url: url,
        data: { notificationId: notificationId },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                InitNotificationsPagination();
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
}

function MarkAsReadActivity(activityKey, event) {

    //if (event) {
    //    event.preventDefault();
    //    event.stopPropagation();
    //}


    var url = "/Notifications/MarkAsReadNotification/";
    $.ajax({
        url: url,
        data: { notificationId: 0, activityKey: activityKey },
        cache: false,
        type: "POST",
        async: true,
        success: function (refModel) {
            if (refModel.result) {
                if (refModel.Object == true) {
                    $("#CheckUnReadNotificationExist").css("display", "");
                } else {
                    $("#CheckUnReadNotificationExist").css("display", "none");
                    $("#notification-modal-show").html('<div class="soft_tan">You have no new messages!</div>');
                }
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
}

function MarkAsMultipleReadNotification(strObj) {
    var url = "/Notifications/MarkAsMultipleReadNotification/";
    $.ajax({
        url: url,
        data: { strType: strObj },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
            if (refModel.result) {
                InitNotificationsPagination();

                $(".checkall").text("Check visible");
                $(".removeall").prop("disabled", true);
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            $.LoadingOverlay("hide");
        }
    });
}

function ordinalSuffix(number) {
    var suffix = "";

    if (number / 10 % 10 === 1) {
        suffix = "th";
    } else if (number > 0) {
        switch (number % 10) {
            case 1:
                suffix = "st";
                break;
            case 2:
                suffix = "nd";
                break;
            case 3:
                suffix = "rd";
                break;
            default:
                suffix = "th";
                break;
        }
    }
    return suffix;
}

function Loading() {
    var str =
        '<div id="modal-loading-parent"  class="modal fade" role="dialog" style="opacity: 0.6;background: #000;display: block;">' +
        " </div>" +
        '<div id="modal-loading"  class="modal fade" style="margin-top: 14%;">' +
        '<div class="modal-dialog">' +
        '  <div class="modal-content">' +
        '  <div class="modal-body text-center">' +
        "   <br />" +
        ' <i class="fa fa-spinner fa-4x fa-spin" style="margin-bottom: 20px;"></i>' +
        " <h4>Processing Data, please wait...</h4>" +
        " <br />" +
        "<br />" +
        " </div>" +
        " </div>" +
        " </div>" +
        " </div>" +
        '<input type="button" data-toggle="modal" data-target="#modal-loading" id="popdialogloading" style="display:none" />';
    $("body").append(str);
    $("#popdialogloading").click();
}

function Loading(message) {
    if (typeof message === "undefined" || !message)
        message = "Processing Data, please wait...";
    var str =
        '<div id="modal-loading-parent"  class="modal fade" role="dialog" style="opacity: 0.6;background: #000;display: block;">' +
        " </div>" +
        ' <div id="modal-loading" class="modal fade" role="dialog" style="margin-top: 15%;">' +
        '<div class="modal-dialog">' +
        '  <div class="modal-content">' +
        '  <div class="modal-body text-center">' +
        "   <br />" +
        ' <i class="fa fa-spinner fa-4x fa-spin" style="margin-bottom: 20px;"></i>' +
        " <h4>" +
        message +
        "</h4>" +
        " <br />" +
        " <br /><br />" +
        " </div>" +
        " </div>" +
        " </div>" +
        " </div>" +
        '<input type="button" data-toggle="modal" data-target="#modal-loading" id="popdialogloading" style="display:none" />';
    $("body").append(str);
    $("#popdialogloading").click();
}

function EndLoading() {

    setTimeout(function () {

        $("body").find("div#modal-loading").remove();
        $("body").find("div#modal-loading-parent").remove();
        $(".modal-backdrop").remove();
        $("#popdialogloading").remove();
        $("body").removeClass("modal-open");
        $("body").removeAttr("style");
    },
        500);
}

function EndLoading2() {
    setTimeout(function () {
        $("body").find("div#modal-loading").remove();
        $("body").find("div#modal-loading-parent").remove();
        $(".modal-backdrop").remove();
        var array = $(".modal-backdrop.fade.in").toArray();
        for (var x = 0; x < array.length - 1; x++) {
            array[x].remove();
        }
        $("#popdialogloading").remove();
    },
        500);
}

function ResetFormControl(formId) {
    ClearError();
    $("#" + formId)[0].reset();
    ($("#" + formId).validate()).resetForm();

};

function UniqueId(idStrLen) {
    if (!idStrLen) {
        idStrLen = 20;
    }
    var idStr = (Math.floor((Math.random() * 25)) + 10).toString(36);
    // add a timestamp in milliseconds (base 36 again) as the base
    idStr += (new Date()).getTime().toString(36);
    // similar to above, complete the Id using random, alphanumeric characters
    do {
        idStr += (Math.floor((Math.random() * 35))).toString(36);
    } while (idStr.length < idStrLen);

    return (idStr);
};

function readURLImage(input, elementId) {
    if (input.files && input.files[0]) {
        var $elm;
        if (elementId.indexOf('.') > -1)
            $elm = $(elementId);
        else
            $elm = $('#' + elementId.replace("#", ""));
        // check file image
        if (!input.files[0]["type"] || input.files[0]["type"].split('/')[0] !== 'image') {
            $elm.hide();
            return;
        }
        var reader = new FileReader();
        reader.onload = function (e) {
            $elm.attr('src', e.target.result);
            $elm.show();
        }

        reader.readAsDataURL(input.files[0]);
    } else {
        $elm.hide();

    }
}

//cookies
function setCookie(key, value) {
    $.ajax({
        url: "/Commons/SaveCookie",
        data: { key: key, value: value, expireTime: 5 },
        type: "POST",
        async: false,
        success: function (data) {

        },
        error: function (error) {
        }
    });
}

function getCookie(key) {
    var result = null;
    $.ajax({
        url: "/Commons/GetCookie",
        data: { key: key },
        type: "POST",
        async: false,
        success: function (data) {
            result = data.Object;
        }
    });
    return result;
}

function getCookieClient(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function removeCookie(key) {
    $.ajax({
        url: "/Commons/RemoveCookie",
        data: { key: key },
        type: "DELETE",
        success: function (data) {
        },
        error: function (error) {
        }
    });
};

function LoadingOverlay() {
    $.LoadingOverlay("show");
};

function LoadingOverlayEnd() {
    $.LoadingOverlay("hide");
};

function toDataURL(url, callback) {
    var xhr = new XMLHttpRequest();
    xhr.onload = function () {
        var reader = new FileReader();
        reader.onloadend = function () {
            callback(reader.result);
        }
        reader.readAsDataURL(xhr.response);
    };
    xhr.open('GET', url);
    xhr.responseType = 'blob';
    xhr.send();
};

var lstTraderItems = [];

function ResetItemSelected(tableId, selectId, workgroupId, select2WorkGroupId, select2LocationId = 0,
    hasShowAllItems = false, isSold = true, isBought = true) {

    if (typeof workgroupId !== "undefined")
        if (workgroupId === null) return;
    var isWg = !isNaN(workgroupId);
    if ($('#' + tableId) && $('#' + selectId)) {
        var trs = $('#' + tableId + ' tbody tr');
        var tds = $('#' + tableId + ' tbody tr td');
        var lstId = [];
        if (trs.length > 0 && tds.length > 1) {
            for (var i = 0; i < trs.length; i++) {
                lstId.push($($(trs[i]).find(' td input.traderItem')).val().split(':')[0]);
            }
        }
        var traderItemsSelected = jQuery.map(lstTraderItems, function (item) {
            if (lstId.indexOf(item.Id.toString()) === -1) {
                return item;
            }
        });
        if (isWg) {
            traderItemsSelected = jQuery.map(traderItemsSelected,
                function (item) {
                    if (item.WgIds && (item.WgIds.join(',') + ',').indexOf(workgroupId + ',') > -1) {
                        return item;
                    }
                });
        }

        // Init Select 2
        if (select2WorkGroupId == null) {
            select2WorkGroupId = $("select[name=workgroup]").val();
        }
        if (select2WorkGroupId == null) {
            select2WorkGroupId = 0;
        }
        var select2AjaxUrl = '/Select2Data/GetTraderItemsByWorkgroup?workGroupId=' + select2WorkGroupId + "&locationId=" + select2LocationId;
        //Get default set of options for select2
        var $defaultResults = $("#" + selectId + " option:not([selected])");
        var defaultResults = [];
        $defaultResults.each(function () {
            var $option = $(this);
            defaultResults.push({
                id: $option.attr('value'),
                text: $option.text()
            });
        });
        //Initialize select2 object
        var parameters = {};
        $("#" + selectId).not('.multi-select').select2({
            ajax: {
                url: select2AjaxUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    parameters['keySearch'] = params.term;
                    parameters['isSold'] = isSold;
                    parameters['isBought'] = isBought;
                    return parameters;
                },
                cache: true,
                processResults: function (data) {
                    var lstData = [];
                    if (hasShowAllItems == true) {
                        lstData.push({
                            "id": 0,
                            "text": "Show all"
                        })
                    };
                    lstData = lstData.concat(data.Object);
                    data.Object.forEach(function (item) {
                        lstData.push({
                            "text": item.Name,
                            "itemId": item.Id,
                            "itemName": item.Name,
                            "itemImage": item.ImageUri,
                            "taxName": item.TaxRateName,
                            "taxRate": item.TaxRateValue,
                            "costUnit": item.CostUnit,
                            "value": item.Id,
                            "id": item.Id,
                            "newTag": true
                        });
                    })
                    return {
                        results: lstData
                    };
                }
            },
            //minimumInputLength: 1,
            defaultResults: defaultResults,
        }).on('select2:selecting', function (e) {
            var data = e.params.args.data;
            var htmlStr = "<option itemId='" + data["itemId"] + "' selected value='" + data["itemId"] + "' itemName='" + data["itemName"] + "' itemImage='" + data["itemImage"]
                + "' taxName='" + data["taxName"] + "' taxRate='" + data["taxRate"] + "' costUnit='" + data["costUnit"] + "'>" + data["text"] + "</option>";
            $("#" + selectId).html(htmlStr);
            e.preventDefault();
            $("#" + selectId).select2('close').trigger('change');
        }).val(0).trigger('change');
        qbicleLog('ResetItemSelected');
    }
};

function formatRepo(repo) {
    var $container = $(
        "<div class='select2-result-repository clearfix'>" +
        "</div>"
    );

    $container.find(".select2-result-repository__title").text(repo.full_name);
    $container.find(".select2-result-repository__description").text(repo.description);
    $container.find(".select2-result-repository__forks").append(repo.forks_count + " Forks");
    $container.find(".select2-result-repository__stargazers").append(repo.stargazers_count + " Stars");
    $container.find(".select2-result-repository__watchers").append(repo.watchers_count + " Watchers");

    return $container;
}

function ResetBudgetItemSelected(tableId, selectId) {

    if ($('#' + tableId) && $('#' + selectId)) {
        var trs = $('#' + tableId + ' tbody tr');
        var tds = $('#' + tableId + ' tbody tr td');
        var lstId = [];
        if (trs.length > 0 && tds.length > 1) {
            for (var i = 0; i < trs.length; i++) {
                lstId.push($($(trs[i]).find(' td input.traderItem')).val().split(':')[0]);
            }
        }
        var traderItemsSelected = jQuery.map(lstTraderItems, function (item) {
            if (lstId.indexOf(item.Id.toString()) === -1)
                return item;
        });
        var html = "<option value='0'></option>";
        for (var j = 0; j < traderItemsSelected.length; j++) {
            html +=
                "<option itemId='" + traderItemsSelected[j].Id + "'" +
                " itemName='" + traderItemsSelected[j].Name + "'" +
                " itemImage='" + traderItemsSelected[j].ImageUri + "'" +
                " itemUnit='" + traderItemsSelected[j].Unit + "'" +
                "value=\"" + traderItemsSelected[j].Id +
                "\">" + traderItemsSelected[j].Name + "</option>";
        }
        $("#" + selectId).empty();
        $("#" + selectId).append(html);
        $("#" + selectId).not('.multi-select').select2();
    }
};

function resetFormTransfer() {
    $('#item_selected select').empty();
    $('#item_selected select').append("<option></option>");
    $('#item_selected select').not('.multi-select').select2({ placeholder: "Please select" });

    $('#form_item_quantity').val("");
    $('#item_fee').val("");
};

function numberString(number, currency) {
    if (!currency) currency = "";

    if (!number) { number = 0; }
    else { number = parseFloat(number.toString()); }

    if (isNaN(number)) number = 0;
    return currency + number.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}

// Update menu - reload page
UpdateTitle = function () {


    switch (currentTraderTab.toLocaleLowerCase()) {
        case "apptrader":
            $('.mod_title h4').text('Sales');
            break;
        case "purchases":
            $('.mod_title h4').text('Purchases');
            break;
        case "transfers":
            $('.mod_title h4').text('Transfers');
            break;
        case "contacts":
            $('.mod_title h4').text('Contacts');
            break;
        case "cashbank":
            $('.mod_title h4').text('Cash & Bank');
            break;
        case "itemsproducts":
            $('.mod_title h4').text('Items & Products');
            break;
        case "budget":
            $('.mod_title h4').text('Budget');
            break;
        case "reports":
            $('.mod_title h4').text('Reports');
            break;
        case "config":
            $('.mod_title h4').text('App Configuration');
            break;
        case "manufacturing":
            $('.mod_title h4').text('Manufacturing');
            break;
        case "pointofsale":
            $('.mod_title h4').text('Devices');
            break;
        case "saleschannels":
            $('.mod_title h4').text('Sales Channels');
            break;
        case "shiftmanagement":
            $('.mod_title h4').text('Shift Management');
            break;
        case "orderdisplaysystem":
            $('.mod_title h4').text('Order Display System');
            break;
        default: $('.mod_title h4').text('Sales');
            break;
    }
}

function UpdateMenuActive(ev) {

    $('.app_main_nav li').removeClass('active');
    if (ev) {
        $(ev.parentElement).addClass('active');
    }
    switch (currentTraderTab.toLocaleLowerCase()) {
        case "apptrader":
            $('.trader-sale').addClass('active');
            break;
        case "purchases":
            $('.trader-purchase').addClass('active');
            break;
        case "transfers":
            $('.trader-transfer').addClass('active');
            break;
        case "contacts":
            $('.trader-contact').addClass('active');
            break;
        case "cashbank":
            $('.trader-bank').addClass('active');
            break;
        case "itemsproducts":
            $('.trader-item').addClass('active');
            break;
        case "budget":
            $('.trader-budget').addClass('active');
            break;
        case "reports":
            $('.trader-report').addClass('active');
            break;
        case "config":
            $('.trader-config').addClass('active');
            break;
        case "manufacturing":
            $('.trader-Manufacturing').addClass('active');
            break;
        case "pointofsale":
            $('.trader-pos').addClass('active');
            break;
        case "shiftmanagement":
            $('.trader-shiftManagement').addClass('active');
            break;
        case "orderdisplaysystem":
            $('.trader-pos-pds').addClass('active');
            break;
        case "system":
            $('.admin-system').addClass('active');
            break;
        case "domains":
            $('.admin-domain').addClass('active');
            break;
        case "bankmatetrans":
            $('.admin-bankmate-transactions').addClass('active');
            break;
        case "datarecovery":
            $('.admin-data-recovery').addClass('active');
            break;
        case "accountstatement":
            $('.admin-account-statement').addClass('active');
            break;
        case "adminskills":
            $('.admin-skill').addClass('active');
            break;
        case "saleschannels":
            $('.trader-channel').addClass('active');
            break;
        case "hlsetup":
            $('.hlsetup').addClass('active');
            break;
        case "domainrequest":
            $('.domainrequest').addClass('active');
            break;
        case "waitlistrequest":
            $('.waitlistrequest').addClass('active');
            break;
        case "extensionrequest":
            $('.extensionrequest').addClass('active');
            break;
        case "communityfeature":
            $('.communityfeature').addClass('active');
            break;
        case "monibackpromotion":
            $('.monibackpromotion').addClass('active');
            break;
        case "bulkdeal":
            $('.bulkdeal').addClass('active');
            break;
        default: $('.trader-sale').addClass('active');
            $('#mobile-tab-active').val("apptrader");
            break;
    }
}

var currentTraderTab = "";
var activeTab = "";

ValidateCurrentLocation = function () {
    var locationId = getCookie("CurrentLocationManage");
    currentTraderTab = getTabTrader().TraderTab;
    if (locationId === null || locationId === "0" || locationId === 0) {
        UpdateCurrentLocationManage(currentTraderTab ? currentTraderTab : "");
    }
    else {
        if (currentTraderTab && currentTraderTab !== "") {
            $('#mobile-tab-active').val(currentTraderTab.toLowerCase());
            UpdateMenuActive();
            GetContentMobile(currentTraderTab.toLowerCase());
        }
        else
            ShowContent();
    }
};

UpdateCurrentLocationManage = function (reload) {
    if (!reload) {
        var currentTab = getTabTrader().TraderTab;
        reload = currentTab ? currentTab : 'Sale';
    }
    if (window.location.href.indexOf('Trader/ItemsProducts') > 0) {
        UpdateTab();
    }
    //$.LoadingOverlay("show");
    var locationId = $("#local-manage-select").val();
    $.ajax({
        type: 'get',
        url: '/Trader/UpdateCurrentLocationManage?id=' + locationId,
        dataType: 'json',
        success: function (response) {
            //LoadingOverlayEnd();
            $('div.app_topnav.restyle').load('/Trader/NavigationTraderPartial', function () {
                if (reload === "sale")
                    $('#menu_traderapp a[href="#AppTrader"]').click();
                else {
                    var _tabactive = $('#menu_traderapp a[href="#' + reload + '"]');
                    if (_tabactive.length > 0) {
                        _tabactive.click();
                    } else
                        $('#menu_traderapp a[href="#AppTrader"]').click();
                }
            });
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
        }
    });
};

ShowContent = function (ev) {
    $("#local-manage-div").show();
    var url = window.location.href;
    if (ev) {
        url = $(ev).attr('href');
        url = url.substr(url.indexOf('#') + 1, url.length);
    } else if (url.indexOf('#') >= 0) {
        url = url.substr(url.indexOf('#') + 1, url.length);
    }
    if (url) {
        url = url.split('#');
    }
    currentTraderTab = url[0];
    setValueSelected(currentTraderTab);
    UpdateMenuActive(ev);
    switch (currentTraderTab.toLocaleLowerCase()) {
        case "apptrader":
            ShowSaleContent();
            break;
        case "purchases":
            ShowPurchaseContent();
            break;
        case "transfers":
            ShowTransfersContent();
            break;
        case "contacts":
            ShowContactContent();
            break;
        case "cashbank":
            ShowCashBankContent();
            break;
        case "itemsproducts":
            ShowItemProductContent();
            break;
        case "budget":
            ShowBudgetContent();
            break;
        case "reports":
            ShowReportsContent();
            break;
        case "config":
            ShowConfigContent();
            break;
        case "manufacturing":
            ShowManufacturingContent();
            break;
        case "pointofsale":
            ShowPointOfSaleContent();
            break;
        case "shiftmanagement":
            ShowShiftManagementContent();
            break;
        case "orderdisplaysystem":
            ShowOrderDisplaySystemContent();
            break;
        case "system":
            ShowAdminSystems();
            break;
        case "domains":
            ShowAdminDomains();
            break;
        case "bankmatetrans":
            ShowAdminBankmatetrans();
            break;
        case "datarecovery":
            ShowAdminDataRecovery();
            break;
        case "accountstatement":
            ShowAdminAccountStatement();
            break;
        case "adminskills":
            ShowAdminSkills();
            break;
        case "saleschannels":
            ShowChannelsContent();
            break;
        case "hlsetup":
            ShowHLSetupContent();
            break;
        case "communityfeature":
            ShowAdminCommunityFeatureContent();
            break;
        case "domainrequest":
            ShowAdminDomainRequest();
            break;
        case "waitlistrequest":
            ShowAdminWaitlistRequest();
            break;
        case "extensionrequest":
            ShowAdminExtensionRequest();
            break;
        case "monibackpromotion":
            ShowAdminMonibackPromotionTabContent();
            break;
        case "bulkdeal":
            ShowBulkDealTabContent();
            break;
        default: ShowSaleContent();
            break;
    }
}

UpdateTab = function () {
    if (activeTab === "pricebook-tab") {
        SelectPriceBookTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "adjuststock-tab") {
        SelectAdjustStockTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "waste-report-tab") {
        $('a[href="#adjuststock-tab"]').tab('show');
        SelectAdjustStockWasteTab();
    }
    else if (activeTab === "inventoryaudit-tab") {
        SelectInventoryAuditTab();
        $('a[href="#' + activeTab + '"]').tab('show');
    }
    else {
        isChangeItem = true;
        selectTraderItemsTab();
    }
}

ShowSaleContent = function () {
    document.title = "Trader > Sales";
    setTabTrader(currentTraderTab);
    $('#trader-sale-content').empty();
    $('#trader_content').load('/Trader/TraderSaleTab', function () {
        UpdateTitle();
    });
};

ShowPurchaseContent = function () {
    document.title = "Trader > Purchases";
    setTabTrader(currentTraderTab);
    $('#trader_content').load('/Trader/TraderPurchasesTab', function () {
        UpdateTitle();
    });
};

ShowTransfersContent = function () {
    document.title = "Trader > Transfers";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/TransfersTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowContactContent = function () {
    document.title = "Trader > Contacts";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ContactTab";
    $('#trader_content').load(ajaxUri, function () {
    });
};

ShowCashBankContent = function () {
    document.title = "Trader > Cash & Bank";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/CashBankTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowItemProductContent = function () {
    document.title = "Trader > Items & Products";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ItemsProductsTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowManufacturingContent = function () {
    document.title = "Trader > Manufacturing";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ManufacturingTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowBudgetContent = function () {
    document.title = "Trader > Budget";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/BudgetTab";
    $('#trader_content').load(ajaxUri, function () {

        var subTraderTab = getTabTrader().SubTraderTab;

        if (subTraderTab === 'scenario') {
            $('.budget-tab li').removeClass('active');
            $('#budget-groups').removeClass('active');
            $('.budget-scenario-li').addClass('active');
            $('#budget-scenarios').addClass('in active');
            localStorage.removeItem("SubTraderTab");
        }
        UpdateTitle();
    });
};

ShowReportsContent = function () {
    document.title = "Trader > Reports";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ReportsTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowPointOfSaleContent = function () {
    document.title = "Trader > Devices";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/DeviceTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowSalesChannelContent = function () {
    document.title = "Trader > Sales Channels";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/SalesChannelTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowShiftManagementContent = function () {
    document.title = "Trader > ShiftManagement";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ShiftManagementTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
}

ShowOrderDisplaySystemContent = function () {
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/OrderDisplaySystemTab";
    $('#trader_content').load(ajaxUri, function () {

        UpdateTitle();
    });
};

ShowChannelsContent = function () {
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ChannelsTab";
    $('#trader_content').load(ajaxUri, function () {

        UpdateTitle();
    });
};

ShowConfigContent = function () {
    document.title = "Trader > App Configuration";
    $("#local-manage-div").hide();
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Trader/ConfigTab";
    $('#trader_content').load(ajaxUri, function () {
        UpdateTitle();
    });
};

ShowAdminSystems = function () {
    document.title = "Administration - System";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminSystemsTab";
    $('#administration-content').load(ajaxUri, function () {

    });
};

ShowAdminDomainRequest = function () {
    document.title = "Administration - Domain Request";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminDomainRequestTab";
    $('#administration-content').load(ajaxUri, function () {

    });
}

ShowAdminWaitlistRequest = function () {
    document.title = "Administration - Waitlist";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminWaitlistRequestTab";
    $('#administration-content').load(ajaxUri, function () {

    });
}

ShowAdminExtensionRequest = function () {
    document.title = "Administration - Extension Request";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminExtensionRequestTabContent";
    $('#administration-content').load(ajaxUri, function () {

    });
}

ShowAdminCommunityFeatureContent = function () {
    document.title = "Administration - Community Features";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminCommunityFeatureTabContent";
    $('#administration-content').load(ajaxUri, function () {

    });
}

ShowAdminMonibackPromotionTabContent = function () {
    document.title = "Administration - Moniback Promotions";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminMonibackPromotionTabContent";
    $('#administration-content').load(ajaxUri, function () {

    });
}

ShowBulkDealTabContent = function () {
    document.title = "Administration - Bulk Deal";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/BulkDealTabContent";
    $('#administration-content').load(ajaxUri, {}, function () {

    });
}

ShowHLSetupContent = function () {
    document.title = "Administration - Categories & lists";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminHighlightSetup";
    $('#administration-content').load(ajaxUri, function () {
    });
}

function LoadListAdministratorDomain() {
    if ($.fn.DataTable.isDataTable('#tbl-administrator-domains')) {
        $("#tbl-administrator-domains").DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $("#tbl-administrator-domains").DataTable().ajax.reload();
        }, 1000);
    }
}

function ShowAdminDomains() {
    document.title = "Administration - Domains";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminDomainsTab";
    $('#administration-content').load(ajaxUri, function () {
        $("#tbl-administrator-domains")
            .on('processing.dt', function (e, settings, processing) {
                $('#processingIndicator').css('display', 'none');
                if (processing) {
                    $('#tbl-administrator-domains').LoadingOverlay("show");
                } else {
                    $('#tbl-administrator-domains').LoadingOverlay("hide", true);
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
                "deferLoading": 30,
                "order": [[1, "asc"]],
                "ajax": {
                    "url": '/Domain/GetListDomainByAccount',
                    "type": 'POST',
                    "data": function (d) {
                        return $.extend({}, d, {
                            "name": $("#domainSearch").val(),
                            "dateRange": $("#domainDateRange").val(),
                            "status": $("#domainStatus").val(),
                            "type": "Administrator"
                        });
                    }
                },
                "columns": [
                    {
                        data: "LogoUri",
                        orderable: false,
                        width: "80px",
                        render: function (value, type, row) {
                            if (row.LogoUri) {
                                return ' <div id="avatar-' + row.Id + '" class="table-avatar" style="background-image: url(\'' + apiDocRetrievalUrl + row.LogoUri + '&size=T\');">&nbsp;</div>';
                            }
                            else
                                return '<div id="avatar-' + row.Id + '" class="table-avatar" style="background-image: url(\'/Content/DesignStyle/img/icon_domain_default.png\');">&nbsp;</div>';
                        }
                    },
                    {
                        data: "Name",
                        orderable: true,
                        render: function (value, type, row) {
                            return row.Name;
                        }
                    },
                    {
                        data: "CreatedDate",
                        orderable: true,
                        render: function (value, type, row) {
                            return row.CreatedDate;
                        }
                    },
                    {
                        data: "Status",
                        orderable: true,
                        render: function (value, type, row) {
                            if (row.Status === domainStatusClose)
                                return '<div id="status-' + row.Id + '">Closed</div>';
                            else
                                return '<div id="status-' + row.Id + '">Active</div>';
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        width: "80px",
                        render: function (value, type, row) {
                            var str = ' <div class="btn-group options">';
                            str += ' <button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"> Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                            str += '<ul class="dropdown-menu">';
                            str += '  <li>';
                            str += '<a href="javascript:void(0)" id="edit-anchor-' + row.Id + '" onclick="editDomain(\'' + row.Key + '\', \'' + fixQuoteCode(row.Name) + '\', \'' + apiDocRetrievalUrl + row.LogoUri + '\')">Edit Domain</a>';
                            str += '</li>';
                            str += ' <li>';
                            str += ' <a href="javascript:void(0)" id="openOrClose-' + row.Id + '" onclick="openOrCloseDomainById(\'' + row.Key + '\', \'' + row.Id + '\')">';
                            str += row.Status === domainStatusOpened ? "Close Domain" : "Open Domain";
                            str += ' </a>';
                            str += ' </li>';
                            str += ' </ul>';
                            str += '</div>';
                            return str;
                        }
                    }
                ]
            });
        $("#domainStatus").select2();
        $('#domainDateRange').daterangepicker({
            autoUpdateInput: false,
            cancelClass: "btn-danger",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: 'DD-MM-YYYY'
            }
        });
        $('#domainDateRange').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
            LoadListAdministratorDomain();
        });

        $('#domainDateRange').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            LoadListAdministratorDomain();
        });

        $('#domainSearch').keyup(searchThrottle(function () {
            LoadListAdministratorDomain();
        }));
        $("#domainStatus").change(function () {
            LoadListAdministratorDomain();
        });
        LoadListAdministratorDomain();
    });
};

ShowAdminBankmatetrans = function () {
    document.title = "Administration - Bankmate transactions";
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminBankmatetransTab";
    $('#administration-content').load(ajaxUri, function () {
        //initBankmatetransTab();
    });
};

ShowAdminDataRecovery = function () {
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminDataRecoveryTab";
    $('#administration-content').load(ajaxUri, function () {

    });
};

ShowAdminAccountStatement = function () {
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminAccountStatementTab";
    $('#administration-content').load(ajaxUri, function () {

    });
};

ShowAdminSkills = function () {
    setTabTrader(currentTraderTab);
    var ajaxUri = "/Administration/AdminSkillsTab";
    $('#administration-content').load(ajaxUri, function () {

    });
};

function setTabTrader(tab, subTab) {
    if ($('#page').val() == "Administration")//Page is Adminnistrator
    {
        if (tab)
            window.localStorage.AdminTab = tab;
        if (subTab)
            window.localStorage.SubAdminTab = subTab;
    } else//Page is Trader
    {
        if (tab)
            window.localStorage.TraderTab = tab;
        if (subTab)
            window.localStorage.SubTraderTab = subTab;
    }
}

function setTabLevel(index, value) {
    if (index) {
        window.localStorage["level" + index] = value;
        if (index === 1) {
            window.localStorage.TraderTab = value;
        } else if (index === 2) {
            window.localStorage.SubTraderTab = value;
        }
    }
}

function getTabLevel(index, old) {
    if (index) {
        if (index === 1 && old) {
            return window.localStorage.TraderTab;
        } else if (index === 2 && old) {
            return window.localStorage.SubTraderTab;
        } else return window.localStorage["level" + index];
    } else return null;
}

function openTab(index, old) {
    var activeTab = null;
    if (index) {
        activeTab = getTabLevel(index, old);
        if (activeTab) {
            $('a[href="#' + activeTab + '"]').tab('show');
        }
    } else {
        var loop = true;
        var level = 1;
        while (loop) {

            activeTab = getTabLevel(level, old);
            if (activeTab) {
                $('a[href="#' + activeTab + '"]').tab('show');
                level++;
            } else loop = false;
        }
    }
}

function getTabTrader() {
    var traderMenuTab = {
        TraderTab: window.localStorage.TraderTab,
        SubTraderTab: window.localStorage.SubTraderTab
    };
    if ($('#page').val() == "Administration") {
        traderMenuTab.TraderTab = window.localStorage.AdminTab;
        traderMenuTab.SubTraderTab = window.localStorage.SubAdminTab;
    }
    return traderMenuTab;
}

function removeTabTrader() {
    localStorage.removeItem("TraderTab");
    localStorage.removeItem("SubTraderTab");
}

CheckStatus = function (id, type) {
    var dfd = new $.Deferred();
    $.ajax({
        type: 'get',
        url: '/Trader/CheckStatusTrader?type=' + type + '&id=' + id,
        async: false,
        contentType: false, // Not to set any content header  
        processData: false, // Not to process data  
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                dfd.resolve(response);
            } else if (response.actionVal === 3 || response.actionVal === 2) {
                dfd.resolve(response);
            }
        },
        error: function (er) {
            dfd.resolve("");
        }
    });

    return dfd.promise();
}

CheckStatusApproval = function (id) {
    var dfd = new $.Deferred();
    $.ajax({
        type: "GET",
        url: '/Trader/CheckStatusApprovalReq',
        data: { appId: id },
        dataType: 'json',
        success: function (response) {
            if (response.result === true) {
                dfd.resolve(response);
            } else if (response.actionVal === 3 || response.actionVal === 2) {
                dfd.resolve(response);
            }
        },
        error: function () {
            dfd.resolve("");
        }
    });

    return dfd.promise();
}

// mobile
function setValueSelected(currentTab) {
    switch (currentTab.toLocaleLowerCase()) {
        case "apptrader":
        case "purchases":
        case "transfers":
        case "contacts":
        case "cashbank":
        case "itemsproducts":
        case "budget":
        case "reports":
        case "config":
        case "manufacturing":
        case "pointofsale":
        case "saleschannels":
        case "shiftmanagement":
        case "accountstatement":
        case "adminaccount":
        case "system":
        case "bankmatetrans":
        case "domains":
        case "datarecovery":
        case "adminskills":
        case "orderdisplaysystem":
        case "hlsetup":
        case "communityfeature":
        case "monibackpromotion":
        case "bulkdeal":
        case "saleschannels":
        case "domainrequest":
        case "waitlistrequest":
        case "extensionrequest":
            $('#mobile-tab-active').val(currentTab.toLocaleLowerCase());
            break;
        default: $('#mobile-tab-active').val("apptrader");
            currentTraderTab = "AppTrader";
            setTabTrader(currentTraderTab);
            break;
    }
}

function onSelectedMenuChange() {
    currentTraderTab = $('#mobile-tab-active').val();
    UpdateMenuActive();
    GetContentMobile(currentTraderTab);
}

function GetContentMobile(currentTraderTab) {
    switch (currentTraderTab.toLocaleLowerCase()) {
        case "apptrader":
            ShowSaleContent(true);
            break;
        case "purchases":
            ShowPurchaseContent(true);
            break;
        case "transfers":
            ShowTransfersContent(true);
            break;
        case "contacts":
            ShowContactContent(true);
            break;
        case "cashbank":
            ShowCashBankContent(true);
            break;
        case "itemsproducts":
            ShowItemProductContent(true);
            break;
        case "budget":
            ShowBudgetContent(true);
            break;
        case "reports":
            ShowReportsContent(true);
            break;
        case "config":
            ShowConfigContent(true);
            break;
        case "manufacturing":
            ShowManufacturingContent(true);
            break;
        case "pointofsale":
            ShowPointOfSaleContent(true);
            break;
        case "shiftmanagement":
            ShowShiftManagementContent(true);
            break;
        case "orderdisplaysystem":
            ShowOrderDisplaySystemContent(true);
            break;
        case "system":
            ShowAdminSystems();
            break;
        case "domains":
            ShowAdminDomains();
            break;
        case "bankmatetrans":
            ShowAdminBankmatetrans();
            break;
        case "datarecovery":
            ShowAdminDataRecovery();
            break;
        case "accountstatement":
            ShowAdminAccountStatement();
            break;
        case "adminskills":
            ShowAdminSkills();
            break;
        case "saleschannels":
            ShowChannelsContent();
            break;
        case "hlsetup":
            ShowHLSetupContent();
            break;
        case "communityfeature":
            ShowAdminCommunityFeatureContent();
            break;
        case "domainrequest":
            ShowAdminDomainRequest();
            break;
        case "waitlistrequest":
            ShowAdminWaitlistRequest();
            break;
        case "extensionrequest":
            ShowAdminExtensionRequest();
            break;
        case "monibackpromotion":
            ShowAdminMonibackPromotionTabContent();
            break;
        case "bulkdeal":
            ShowBulkDealTabContent();
            break;
        default: ShowSaleContent(true);
            break;
    }
}

// Save message when reload page

function setMessageReload(mess) {
    window.localStorage[window.location.href.toLocaleLowerCase()] = mess;
}

function showMessageReload() {
    var mess = window.localStorage[window.location.href.toLocaleLowerCase()];
    if (mess) {
        cleanBookNotification.error(mess, "Qbicles");
        LoadingOverlayEnd();
    }
    window.localStorage.removeItem(window.location.href.toLocaleLowerCase());
}

// set key value localStoga
function setLocalStorage(key, value) {
    window.localStorage[key] = value;
}

function getLocalStorage(key) {
    return window.localStorage[key];
}

function removeLocalStorage(key) {
    localStorage.removeItem(key);
}
//

function setFilterByDateTime(inputDateId, tableId, column, formatDate) {
    if (!formatDate) {
        formatDate = 'DD/MM/YYYY';
    }
    $('#' + tableId).dataTable().fnDestroy();
    if ($.fn.dataTable.ext.search && $.fn.dataTable.ext.search.length > 0)
        $.fn.dataTable.ext.search = [];

    $('#' + inputDateId).daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: formatDate
        }
    });
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            if (!$('#' + inputDateId).val() || ($('#' + inputDateId).val() && $('#' + inputDateId).val().trim() === '')) return true;
            var min = moment(($('#' + inputDateId).val().split('-')[0] + '').trim(), formatDate);;
            var max = moment(($('#' + inputDateId).val().split('-')[1] + '').trim(), formatDate);;
            var startDate = moment((data[column] + '').trim(), formatDate);
            if (min === null && max === null) { return true; }
            if (min === null && startDate <= max) { return true; }
            if (max === null && startDate >= min) { return true; }
            if (startDate <= max && startDate >= min) { return true; }
            return false;
        }
    );
    $('#' + inputDateId).on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(formatDate) + ' - ' + picker.endDate.format(formatDate));
        $('#' + inputDateId).html(picker.startDate.format(formatDate) + ' - ' + picker.endDate.format(formatDate));
        $('#' + tableId).DataTable().draw();
    });
    $('#' + inputDateId).on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#' + inputDateId).html('full history');
        $('#' + tableId).DataTable().draw();
    });
    $('#' + inputDateId).on('keyup', function (event) {
        $('#' + tableId).DataTable().draw();
    });
    $('#' + tableId).DataTable().draw();
}

$(document).ajaxError(function (event, jqXHR, settings, thrownError) {
    if (jqXHR.status === 405) {
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        setTimeout(function () { location.reload(); }, 3000);
    } else if (jqXHR.status === 401) {
        location.reload();
    }
});

$(document).ready(function () {
    var modal = $(".modal");
    for (var i = 0; i < modal.length; ++i) {
        if ($(modal[i]).hasClass("fade")) {
            $("#" + $(modal[i]).attr("id")).on('show.bs.modal', function () {
                var options = $(this).data('bs.modal').options;
                if ($(this).data('backdrop') != 'static')
                    options.backdrop = true;
                else
                    options.backdrop = 'static';
                options.keyboard = true;
                options.focus = "input:first";
            });

        }
    }
});

function destroyTableById(tableId) {
    $("#" + tableId).dataTable().fnClearTable();
    $("#" + tableId).dataTable().fnDraw();
    $("#" + tableId).dataTable().fnDestroy();
}

function destroyTableByClass(tableClass) {

    $("table." + tableClass).dataTable().fnClearTable();
    $("table." + tableClass).dataTable().fnDraw();
    $("table." + tableClass).dataTable().fnDestroy();
}

function AjaxElementLoadPost(ajaxUri, data, elementId) {
    $('#' + elementId).LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).load(ajaxUri, data, function () {
        $('#' + elementId).LoadingOverlay("hide", true);
    });
};

function AjaxElementLoad(ajaxUri, elementId) {
    $('#' + elementId).LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).load(ajaxUri, function () {
        $('#' + elementId).LoadingOverlay("hide", true);
    });
};

function AjaxElementShowModal(ajaxUri, elementId) {
    $('#' + elementId).LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).load(ajaxUri, function () {
        $('#' + elementId).LoadingOverlay("hide", true);
        $("#" + elementId).modal('show');
    });
};

function AjaxElementShowModalPostData(ajaxUri, data, elementId) {
    $('#' + elementId).LoadingOverlay("show");
    $('#' + elementId).empty();
    $('#' + elementId).load(ajaxUri, data, function () {
        $('#' + elementId).LoadingOverlay("hide", true);
        $("#" + elementId).modal('show');
    });
};

function removeValidateSelect(input) {
    $(input).removeClass('valid-select');
}

function initValidate() {
    $(document).ready(function () {
        $(".valid-select").on('change',
            function (e) {
                if ($(this).val() !== null)
                    $(this).next().removeClass('error');
            });
    });
}

function delay(callback, ms) {
    var timer = 0;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}

function bytesToSize(bytes) {
    var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes === 0) return '0 Byte';
    var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
    return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
}

function LoadTableData(tableid, url, jsonfiltervalue, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    if (!jsonfiltervalue) jsonfiltervalue = {};
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#" + tableid).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $("#" + tableid).LoadingOverlay("hide", true);
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
                return $.extend({}, d, jsonfiltervalue);
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}

function _L(keyname) {
    try {
        var value = _errormessages[keyname];
        if (value)
            return value;
        else
            return keyname;
    } catch (e) {
        return keyname;
    }
}

function _L(keyname, args) {
    try {
        //args=["sea", "sells", "shells", "shore"]
        var value = _errormessages[keyname];
        if (value) return value.format(args); else return keyname;
    } catch (e) {
        return keyname;
    }

}
String.prototype.format = function (args) {
    var str = this;
    return str.replace(String.prototype.format.regex, function (item) {
        var intVal = parseInt(item.substring(1, item.length - 1));
        var replace;
        if (intVal >= 0) {
            replace = args[intVal];
        } else if (intVal === -1) {
            replace = "{";
        } else if (intVal === -2) {
            replace = "}";
        } else {
            replace = "";
        }
        return replace;
    });
};
String.prototype.format.regex = new RegExp("{-?[0-9]+}", "g");


var itemOverViewFilter = {
    GroupIds: [],
    Types: [],
    Brands: [],
    Needs: [],
    Rating: [],
    Tags: []
};

function ReLoadComment(idLoading, type, id, page) {
    $('#' + idLoading).load('/Trader/LoadPostComment?type=' + type + '&id=' + id + '&page=' + page, function () {
    });
};
function showTaxRatesDetail(pricePerUnit, quantity, discount, sTaxRates) {
    try {
        if (!currencySetting) {
            loadCurrencySettings();
        }

        var _defaultVal = currencySetting.SymbolDisplay == 0 ? (currencySetting.CurrencySymbol + "0") : ("0" + currencySetting.CurrencySymbol);
        if (!sTaxRates || sTaxRates === _defaultVal)
            return "0";
        var _sNewTaxRates = '<ul class="unstyled">';
        var _taxrates = sTaxRates.split(",");
        _taxrates.forEach(function (item) {
            var val_rate = item.split("-");
            var rate = parseFloat(val_rate[0]);
            var priceRate = (pricePerUnit * quantity) * (1 - (parseFloat(discount) / 100)) * (rate / 100);
            _defaultVal = toCurrencySymbol(priceRate, false);
            _sNewTaxRates += ("<li>" + _defaultVal + "<small> &nbsp; <i>(" + val_rate[1] + ")</i></small></li>");
        });
        _sNewTaxRates += '</ul><input type="hidden" value="' + sTaxRates + '" class="txt-taxname">'
        return _sNewTaxRates;
    } catch (e) {
        return sTaxRates;
    }
}
function toCurrencySymbol(value, isSymbol) {
    if (!currencySetting)
        loadCurrencySettings();
    if (currencySetting) {
        value = parseFloat(value);
        value = value.toFixed(currencySetting.DecimalPlace);
        value = value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, (currencySetting.currencyGroupSeparator ? currencySetting.currencyGroupSeparator : ','));
        if ((typeof (isSymbol) == "undefined") || isSymbol)
            value = (currencySetting.SymbolDisplay == 0 ? (currencySetting.CurrencySymbol + value) : (value + currencySetting.CurrencySymbol))
    }
    return value;
}
function toCurrencyDecimalPlace(value) {
    if (typeof value != 'number')
        value = parseFloat(value);
    if (currencySetting) {
        return value.toFixed(currencySetting.DecimalPlace);
    } else {
        loadCurrencySettings();
        return value.toFixed(currencySetting.DecimalPlace);
    }
    return value.toFixed($decimalPlace);
}
function stringToNumber(value) {
    if (typeof value == 'number')
        return value;
    if (!currencySetting) {
        loadCurrencySettings();
    }
    var re = new RegExp("\\" + currencySetting.currencyGroupSeparator, "g")
    return parseFloat(value.replace(currencySetting.CurrencySymbol, '').replace(re, '').trim());
}
function loadCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
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
jQuery(document).ready(function () {
    $('.validate-control select').on('change', function (e) {
        if ($(this).val() && $(this).next()[0].localName === 'label' && $($(this).next()).hasClass("error")) {
            $(this).next().remove();
        } else if ($(this).val() && $(this).next()[0].localName === 'span') {
            $(this).next().next().remove();
        }
    });

});
function qbicleLog(log) {
    if (showLog) { }
}

function logAccess(type) {
    $.ajax({
        url: '/Commons/LogAccess?type=' + type,
        data: { type: type },
        cache: false,
        type: "POST",
        async: false,
        success: function (refModel) {
        },
        error: function (xhr, status, error) {
        }
    });
}


function GenerateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
};

//Lock sceen app while upload media
//function UploadStart() {
//    $("progress").attr('value', 0);
//    $('body').addClass('lock');
//    $('#upload-lock').fadeIn();

//};

//function UploadStop() {
//    $('#upload-lock').hide();
//    $('body').removeClass('lock');
//};


//AWS.config.update({
//    region: $s3BucketRegion,
//    credentials: new AWS.CognitoIdentityCredentials({
//        IdentityPoolId: $s3IdentityPoolId
//    })
//});

//var s3 = new AWS.S3({
//    apiVersion: '2006-03-01',
//    params: { Bucket: $s3BucketName }
//});



function UploadMediaS3ClientSide(idName) {
    var dfd = new $.Deferred();
    //


    var mediaS3Object = {
        objectKey: "no-image",
        fileName: "",
        fileSize: "",
    };

    var mediaFiles = document.getElementById(idName).files;

    if (mediaFiles.length > 0) {

        AWS.config.update({
            region: $s3BucketRegion,
            credentials: new AWS.CognitoIdentityCredentials({
                IdentityPoolId: $s3IdentityPoolId
            })
        });

        var s3 = new AWS.S3({
            apiVersion: '2006-03-01',
            params: { Bucket: $s3BucketName }
        });

        //UploadStart();
        var file = mediaFiles[0];

        mediaS3Object.fileName = file.name;
        mediaS3Object.fileSize = file.size;
        mediaS3Object.objectKey = GenerateUUID();

        var fileType = file.type;

        if (file.type.match('image.*')) {
            fileType = "image";
        } else if (file.type.match('video.*')) {
            fileType = "video";
        } else {
            fileType = "application";
        }
        //OK
        s3.upload({
            Key: mediaS3Object.objectKey,
            Body: file,
            ContentType: file.type,
            Metadata: {
                "file-name": mediaS3Object.fileName,
                "file-type": fileType,
                "file-extension": mediaS3Object.fileName.split('.').pop()
            },
            ACL: "bucket-owner-full-control"//'public-read'
        }, function (err, data) {
            //UploadStop();
            if (err) {
                mediaS3Object.objectKey = "no-image";
                cleanBookNotification.error(err, "Qbicles");
                dfd.resolve(mediaS3Object);
                return;
            }

            dfd.resolve(mediaS3Object);

        })
        //.on('httpUploadProgress', function (progress) {
        //     var uploaded = parseInt((progress.loaded * 100) / progress.total);
        //     $("progress").attr('value', uploaded);
        // });


    }
    else {
        dfd.resolve(mediaS3Object);
    }
    return dfd.promise();
}


function UploadBatchMediasS3ClientSide($mediaList) {
    //UploadStart();
    var dfd = new $.Deferred();
    var promises = [];
    var $index = 1;
    _.forEach($mediaList, function (fileUpload) {
        var file = fileUpload.File;
        var objectKey = fileUpload.Id;
        var fileType = file.type;


        if (file.type.match('image.*')) {
            fileType = "image";
        } else if (file.type.match('video.*')) {
            fileType = "video";
        } else {
            fileType = "application";
        }

        var s3Body = {
            Key: objectKey,
            Body: file,
            ContentType: file.type,
            Metadata: {
                "file-name": fileUpload.Name,
                "file-type": fileType,
                "file-extension": fileUpload.Extension
            },
            ACL: "bucket-owner-full-control"
        };

        promises.push(s3Body);


    });

    Promise.all(promises.map(function (s3Body) {
        AWS.config.update({
            region: $s3BucketRegion,
            credentials: new AWS.CognitoIdentityCredentials({
                IdentityPoolId: $s3IdentityPoolId
            })
        });
        var s3 = new AWS.S3({
            apiVersion: '2006-03-01',
            params: { Bucket: $s3BucketName }
        });
        s3.upload(s3Body, function (err, data) {
            if (err) {
                cleanBookNotification.error(err, "Qbicles");
            }
            if ($index === $mediaList.length) {
                //UploadStop();
                dfd.resolve();
            }
            $index++;
        }).on('httpUploadProgress', function (progress) {
            var uploaded = parseInt((progress.loaded * 100) / progress.total);
            $("progress").attr('value', uploaded);
        });
    }));

    return dfd.promise();
};
function initJsTree() {

    $(".jstree").bind('loaded.jstree', function (e, data) {
        $("div#jstree_id").jstree("_open_to", "#selected-account");
    }).jstree({
        "core": {
            "themes": {
                "dots": true,
                "stripes": false,
                "variant": "large"
            }
        },
        "search": {
            "case_insensitive": true,
            "show_only_matches": true
        },
        "plugins": ["themes", "search", "html_data", "ui", "wholerow"]
    });

    //$('.jstree').jstree('open_all');
    $(".jstree").on("select_node.jstree",
        function (e, data) {
            data.instance.toggle_node(data.node);
        }).on("select_node.jstree",
            function (e, data) {
                if (data.instance.get_node(data.node, true).children("a").attr("href") !== "#") {
                    document.location = data.instance.get_node(data.node, true).children("a").attr("href");
                }
            });

    var showtree = function () {
        $(".treeview").fadeIn(800);
    };
    setTimeout(showtree, 200);

    $(".search-tree").keyup(function () {
        var searchString = $(this).val();
        $(".jstree").jstree("search", searchString);
    });
};


function LoadAccountTree(linkedId) {
    LoadingOverlay();
    $("#app-bookkeeping-treeview").html("");
    $("#app-bookkeeping-treeview").load("/Bookkeeping/TreeViewAccountByNodeIdPartial?id=0&number=0&linkedId=" + linkedId, function () {
        initJsTree();
        LoadingOverlayEnd();
    });
}

function fixQuoteCode(val) {
    if (val)
        return val.replace(/'/g, "\\'").replace(/"/g, '&#34;');
    else
        return val;
}
function getQuerystring(key) {
    const params = new URLSearchParams(location.search);
    if (params)
        return params.get(key);
    return '';
}

function initPendingDomainRequestTable() {
    var dataTable = $("#admin-pending-domainrequest")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#admin-pending-domainrequest').LoadingOverlay("show");
            } else {
                $('#admin-pending-domainrequest').LoadingOverlay("hide", true);
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
                "url": '/Domain/LoadDomainRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#pending-request-search").val(),
                        "createdUserIdSearch": $("#pending-request-creator").val(),
                        "dateRange": $("#pending-request-daterange").val(),
                        "domainTypeSearch": $("#pending-request-type").val(),
                        "lstRequestStatusSearch": [1],
                    });
                }
            },
            "columns": [
                {
                    data: "requestId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var requestId = row.requestId;
                        var htmlStr = "";
                        htmlStr += '<input type="checkbox" requestId="' + requestId + '" style="position: relative; top: 0;" onclick="if($(\'#admin-pending-domainrequest input:checked\').length > 0){$(\'.withselected\').fadeIn();}else{$(\'.withselected\').fadeOut();};">';
                        return htmlStr;
                    }
                },
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "RequestedByName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var creatorName = row.RequestedByName;
                        var creatorLogoUri = row.RequestedByLogoUri;
                        var creatorId = row.RequestById;
                        var htmlStr = "";
                        htmlStr += '<a href="/Community/UserProfilePage?uId=' + creatorId + '">';
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + creatorLogoUri + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + creatorName + '</div>'
                        htmlStr += '<div class="clearfix"></div></a>';
                        return htmlStr;
                    }
                },
                {
                    data: "DomainName",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var domainLogo = row.DomainLogoUri;
                        var domainName = row.DomainName;
                        var htmlStr = "";
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + domainLogo + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + domainName + '</div>';
                        htmlStr += '<div class="clearfix"></div>';
                        return htmlStr;
                    }
                },
                {
                    data: "RequestTypeStr",
                    orderable: true
                },
                {
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<div class="btn-group">';
                        htmlStr += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                        htmlStr += 'Actions &nbsp; <i class="fa fa-angle-down"></i>';
                        htmlStr += '</button>';
                        htmlStr += '<ul class="dropdown-menu dropdown-menu-right primary">';
                        htmlStr += '<li><a href="#app-mbm-admin-transfers" onclick="triggerDomainRequest(' + row.requestId + ', 2)" data-toggle="modal">Approve</a></li>';
                        htmlStr += '<li><a href="#" onclick="triggerDomainRequest(' + row.requestId + ', 3)">Reject</a></li>';
                        htmlStr += '</ul>';
                        htmlStr += '</div>';
                        return htmlStr;
                    }
                }
            ]
        });
}

function initHistoryDomainRequestTable() {
    var dataTable = $("#admin-history-domainrequest")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#admin-history-domainrequest').LoadingOverlay("show");
            } else {
                $('#admin-history-domainrequest').LoadingOverlay("hide", true);
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
                "url": '/Domain/LoadDomainRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#history-request-search").val(),
                        "dateRange": $("#history-request-daterange").val(),
                        "createdUserIdSearch": $("#history-request-creator").val(),
                        "domainTypeSearch": $("#history-request-type").val(),
                        "lstRequestStatusSearch": [2, 3],
                    });
                }
            },
            "columns": [
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "RequestedByName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var creatorName = row.RequestedByName;
                        var creatorLogoUri = row.RequestedByLogoUri;
                        var creatorId = row.RequestById;
                        var htmlStr = "";
                        htmlStr += '<a href="/Community/UserProfilePage?uId=' + creatorId + '">';
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + creatorLogoUri + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + creatorName + '</div>'
                        htmlStr += '<div class="clearfix"></div></a>';
                        return htmlStr;
                    }
                },
                {
                    data: "DomainName",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var domainLogo = row.DomainLogoUri;
                        var domainName = row.DomainName;
                        var htmlStr = "";
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + domainLogo + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + domainName + '</div>';
                        htmlStr += '<div class="clearfix"></div>';
                        return htmlStr;
                    }
                },
                {
                    data: "RequestTypeStr",
                    orderable: true
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += row.RequestStatusLabel;
                        return htmlStr;
                    }
                }
            ]
        });
}

function initUserDomainRequestTable() {
    var dataTable = $("#domain-request-history")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#domain-request-history').LoadingOverlay("show");
            } else {
                $('#domain-request-history').LoadingOverlay("hide", true);
            }
        })
        .dataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "order": [[0, "desc"]],
            "ajax": {
                "url": '/Domain/LoadDomainRequestTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#domain-request-key").val(),
                        "dateRange": $("#domain-request-daterange").val(),
                        "createdUserIdSearch": "",
                        "domainTypeSearch": 0,
                        "lstRequestStatusSearch": [1, 2, 3],
                        "isSearchingForCurrentUser": true
                    });
                }
            },
            "columns": [
                {
                    data: "RequestedDate",
                    orderable: true
                },
                {
                    data: "DomainName",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var domainLogo = row.DomainLogoUri;
                        var domainName = row.DomainName;
                        var htmlStr = "";
                        htmlStr += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + domainLogo + '\');"></div>';
                        htmlStr += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + domainName + '</div>';
                        htmlStr += '<div class="clearfix"></div>';
                        return htmlStr;
                    }
                },
                {
                    data: "RequestTypeStr",
                    orderable: true
                },
                {
                    data: "Status",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += row.RequestStatusLabel;
                        return htmlStr;
                    }
                },
                {
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        if (row.Status == 1 || row.Status == 3) {
                            htmlStr += '<a href="mailto:support@qbicles.com" class="btn btn-primary">Contact us</a>';
                        }
                        return htmlStr;
                    }
                },
                //{
                //    data: "Creator",
                //    orderable: true,
                //    "render": function (data, type, row, meta) {
                //        //var _htmlStr = row.CreatedBy.Surname + " " + row.CreatedBy.Forename;
                //        var _htmlStr = "namme";
                //        return _htmlStr;
                //    }
                //}
            ]
        });
}

function triggerDomainRequest(requestId, status) {
    $("#admin-pending-domainrequest").LoadingOverlay("show");

    var _url = "/Domain/ProcessDomainRequest";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            domainRequestId: requestId,
            status: status
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#admin-pending-domainrequest").DataTable().ajax.reload();
                $("#admin-history-domainrequest").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });

    $("#admin-pending-domainrequest").LoadingOverlay("hide");
}

function processMultipleDomainRequest(status) {
    LoadingOverlay();
    var selectedRequest = [];
    $("#admin-pending-domainrequest input:checked").each(function () {
        selectedRequest.push($(this).attr('requestid'));
    });

    var _url = "/Domain/ProcessMultipleDomainRequests";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            lstRequestId: selectedRequest,
            status: status
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#admin-pending-domainrequest").DataTable().ajax.reload();
                $("#admin-history-domainrequest").DataTable().ajax.reload();
                $('.withselected').fadeOut();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });

    LoadingOverlayEnd();
}

function showDomainRequestHistory() {
    initUserDomainRequestTable();

    $('#domain-request-daterange').on('apply.daterangepicker', function (ev, picker) {

        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        $("#domain-request-history").DataTable().ajax.reload();
    });
    $('#domain-request-daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        $("#domain-request-history").DataTable().ajax.reload();
    });

    $("#domain-request-key").keyup(delay(function () {
        $("#domain-request-history").DataTable().ajax.reload();
    }, 1000));

    $("#domain-history").modal('show');
}

function showDomainExtensionHistoryTab(dmKey) {
    var _url = "/Domain/UpdateCurrentDomain?domainKey=" + dmKey;
    $.ajax({
        method: 'POST',
        url: _url,
        success: function (response) {
            if (response.result) {
                window.location = "/Administration/AdminPermissions?tabActive=ExtensionHistory";
            } else {
                cleanBookNotification.error(response.msg);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
        }
    })
}

function updateURL(param, value) {
    if (history.pushState) {
        var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + '?' + param + '=' + value;
        window.history.pushState({ path: newurl }, '', newurl);
    }
}

function updateNavBarOpenStatus() {
    var _url = "/UserProfile/UpdateUserNavBarOpenStatus";
    var _isNavTabClosed = $("body").hasClass('sidebar-collapse');
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            'isClosed': _isNavTabClosed
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    })
}
//The purpose of the solution to this problem: the client side will "silently" send request, 
//frequent requests to the server to tell the server that it is still "alive"
$(function () {
    setInterval(function () {
        $.get("/Domain/HeartBeat")
    }, 1000 * 240); //4 minutes sent request one time
});

/**
 * Open new tab with post method
 * @param {any} url
 * @param {any} data
 */
function openWindowWithPost(url, data) {
    var form = document.createElement("form");
    form.target = "_blank";
    form.method = "POST";
    form.action = url;
    form.style.display = "none";

    for (var key in data) {
        var input = document.createElement("input");
        input.type = "hidden";
        input.name = key;
        input.value = data[key];
        form.appendChild(input);
    }

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
}

//Approval Page

var loadCountPost = 1, loadCountMedia = 1, busycomment = false;
//Add comment to Activity Approval page
function validateAddComment() {
    var message = $('#txt-comment-approval').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}

function addCommentForApproval(apprKey) {
    if (busycomment)
        return;
    var message = $('#txt-comment-approval');
    if (message.val()) {
        isPlaceholder(true, '#list-comments-approval');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentToApproval",
            data: { message: message.val(), approvalKey: apprKey },
            type: "POST",
            success: function (result) {
                if (result) {
                    message.val("");
                }
                busycomment = false;
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                busycomment = false;
            }
        }).always(function () {
            isPlaceholder(false, '');
        });
    }
};
// Load more comment on Activity approval page
function LoadMorePosts(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: loadCountPost * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadPosts').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            loadCountPost = loadCountPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
// Load more media on Activity approval page
function LoadMoreMedias(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: loadCountMedia * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#' + divId).append(response).hide().fadeIn(250);
            loadCountMedia = loadCountMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}


function validateAddCommentTask() {
    var message = $('#txt-comment-task').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function AddCommentToTask(taskKey) {
    if (busycomment)
        return;
    var message = $('#txt-comment-task');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-task');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentToTask",
            data: { message: message.val(), taskKey: taskKey },
            type: "POST",
            success: function (response) {
                if (response.result) {
                    message.val("");

                    if (response.msg != '') {
                        $('#list-comments-task').prepend(response.msg);
                        isDisplayFlicker(false);
                    }
                }
                busycomment = false;
            },
            error: function (error) {
                isPlaceholder(false, '');
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Task");
            }
        }).always(function () {
            busycomment = false;
        });
    }
}

function AddCommentTransaction(id) {

    var message = $('#txt-comment-transaction');
    isPlaceholder(true, '#transaction-comments-' + id);
    busycomment = true;
    $.ajax({
        url: "/QbicleComments/AddCommentTransaction",
        data: { message: message.val(), id: id },
        type: "POST",
        success: function (result) {
            if (result) {
                message.val("");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isPlaceholder(false, '');
        }
    }).always(function () {
        busycomment = false;
    });
};


function AddCommentJournalEntry(id) {
    var message = $('#txt-comment-task');
    isPlaceholder(true, '#list-comments-journal');
    busycomment = true;
    $.ajax({
        url: "/QbicleComments/AddCommentJournalEntry",
        data: { message: message.val(), id: id },
        type: "POST",
        success: function (result) {
            if (result) {
                message.val("");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            isPlaceholder(false, '');
        }
    }).always(function () {
        busycomment = false;
    });
};

function RemoveOptionSelect2(elementId) {
    $('#' + elementId).select2('destroy');
    $("#" + elementId + " option:selected").remove();

    $('#' + elementId).not('.multi-select').select2({
        placeholder: 'Please select'
    });
}
function AddOptionSelect2(elementId, value, text) {
    $('#' + elementId).select2('destroy');
    $('#' + elementId).append("<option value='" + value + "'>" + text + "</option>");
    $('#' + elementId).not('.multi-select').select2({
        placeholder: 'Please select'
    });
}

function initSelect2MethodAJAX(elementId, url = '', parameters = {}, hasShowAll = false) {
    if (url == '') {
        cleanBookNotification.error(_L("ERROR_MSG_5"), "Qbicles");
        return;
    }

    //Get default set of options for select2
    var $defaultResults = $("#" + elementId + " option:not([selected])");
    var defaultResults = [];
    $defaultResults.each(function () {
        var $option = $(this);
        defaultResults.push({
            id: $option.attr('value'),
            text: $option.text()
        });
    });

    //Initialize select2 object
    $("#" + elementId).select2({
        ajax: {
            url: url,
            dataType: 'json',
            delay: 250,
            data: function (params) {
                parameters['keySearch'] = params.term;
                return parameters;
            },
            cache: true,
            processResults: function (data) {
                var lstData = [];
                if (hasShowAll == true) {
                    lstData.push({
                        "id": 0,
                        "text": "Show all"
                    })
                };
                lstData = lstData.concat(data.Object);
                return {
                    results: lstData
                };
            }
        },
        //minimumInputLength: 1,
        defaultResults: defaultResults
    })
}

function validateEmails(emails) {
    var invalid = "";

    const regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

    const emailArray = emails.split(/[;,]/);

    for (var i = 0; i < emailArray.length; i++) {
        var email = emailArray[i].trim();
        if (email.length > 0 && !regex.test(email)) {
            invalid += email + '\n';
        }
    }
    return invalid;
}