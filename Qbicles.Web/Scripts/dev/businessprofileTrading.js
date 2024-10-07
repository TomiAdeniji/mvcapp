var isBusy = false;
var isDraft = false;
var recipes = [];
var BKAccount = { Id: 0, Name: "" };
var wto;
var dataVehicles = [];
var BKAccount = { Id: 0, Name: "" };
var tradinglogisticstabs = {
    istablogisticsgeneralload: false,
    istablogisticschargesload: false,
    istablogisticsvehiclesload: false,
    istablogisticsdriversload: false,
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
    istabgeneralorderdefaultsload: false
};
var tradingitemscataloguestabs = {
    istabitemsload: false,
    istabitemspricingload: false,
    istabitemscatalogues: false,
    istabitemscataloguesdistribution: false,
    istabitemsimport: false,
    istabitemresourcefirstload: false
};

$('ul.trading-logistics-tabs li a').click(function () {
    var elid = $(this).attr('href');
    setTimeout(function () {
        switch (elid) {
            case "#logistics-general":
                initPluginsOfTabGarenal(tradinglogisticstabs.istablogisticsgeneralload);
                tradinglogisticstabs.istablogisticsgeneralload = true;
                break;
            case "#logistics-charges":
                getLocationsDomain($('#logistics-charges select[name=locations]'), tradinglogisticstabs.istablogisticschargesload);
                loadContentPriceList();
                initFormClonePriceList(tradinglogisticstabs.istablogisticschargesload);
                tradinglogisticstabs.istablogisticschargesload = true;
                break;
            case "#logistics-vehicles":
                initPluginsOfTabVehicles(tradinglogisticstabs.istablogisticsvehiclesload);
                tradinglogisticstabs.istablogisticsvehiclesload = true;
                break;
            case "#logistics-drivers":
                getLocationsDomain($('#logistics-drivers select[name=locations]'), tradinglogisticstabs.istablogisticsdriversload);
                initPluginsOfTabDrivers(tradinglogisticstabs.istablogisticsdriversload);
                tradinglogisticstabs.istablogisticsdriversload = true;
                break;
        }
    }, 200);
});

var tradinggrouptabs = {
    istabgroupsitemsload: false,
    istabgroupscontactsload: false
};
var tradingpromotionstabs = {
    istabactiveload: false,
    istabarchivedload: false
};

$(document).ready(function () {
    initNavClick();

    if (location.hash === "#profile-vouchers") {
        $('a[href="#profile-vouchers"]').trigger('click');
        return;
    }

    var _tabactive = getQuerystring('tab');
    if (_tabactive == 'general-contacts') {
        $('a[href=#profile-info]').click();
        setTimeout(function () {
            $('a[href=#' + _tabactive + ']').click();
        }, 500);
    }
    else if (_tabactive == 'items-catalogues') {
        $('a[href=#profile-items]').click();
        setTimeout(function () {
            $('a[href=#' + _tabactive + ']').click();
        }, 500);
    } else if (_tabactive == 'general-candb') {
        $('a[href=#trading-general]').click();
        setTimeout(function () {
            $('a[href=#' + _tabactive + ']').click();
        }, 500);
    }
    else if (_tabactive == 'items-catalogues-distribution') {
        $('a[href=#profile-items]').click();
        setTimeout(function () {
            $('a[href=#items-catalogues-distribution]').click();
        }, 500);
    } else if (_tabactive == 'items-import') {
        $('a[href=#profile-items]').click();
        setTimeout(function () {
            $('a[href=#items-import]').click();
        }, 500);
    }
    else
        $('a[href=#profile-info]').click();

    //init b2b subtab general Logistic
    initPluginsOfTabGarenal(tradinglogisticstabs.istablogisticsgeneralload);
    tradinglogisticstabs.istablogisticsgeneralload = true;
});
function initNavClick() {
    $('ul.profilenav li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {
                case "#profile-info":
                    $('a[href="#general-locations"]').trigger('click');
                    break;
                case "#profile-items":
                    $('a[href="#items"]').trigger('click');
                    break;
                case "#profile-vouchers":
                    //$('a[href="#promos-active"]').trigger('click');
                    initPluginsOfTabActive(tradingpromotionstabs.istabactiveload);
                    tradingpromotionstabs.istabactiveload = true;
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
                case "#general-contacts":
                    loadTabContent(elid, tradinggeneraltabs.istabcontactsload);
                    tradinggeneraltabs.istabcontactsload = true;
                    break;
                default:
                    loadTabContent(elid, tradinggeneraltabs.istabgeneralorderdefaultsload);
                    tradinggeneraltabs.istabgeneralorderdefaultsload = true;
                    break;
            }
        }, 200);
    });
    $('ul.trading-items-catalogues-tabs li a').click(function () {
        var elid = $(this).attr('href');
        setTimeout(function () {
            switch (elid) {
                case "#items":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemsload);
                    tradingitemscataloguestabs.istabitemsload = true;
                    break;
                case "#item-resources":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemresourcefirstload);
                    tradingitemscataloguestabs.istabitemresourcefirstload = true;
                    break;
                case "#items-pricing":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemspricingload);
                    tradingitemscataloguestabs.istabitemspricingload = true;
                    break;
                case "#items-catalogues":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemscatalogues);
                    tradingitemscataloguestabs.istabitemscatalogues = true;
                    break;
                case "#items-catalogues-distribution":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemscataloguesdistribution);
                    tradingitemscataloguestabs.istabitemscataloguesdistribution = true;
                    break;
                case "#items-import":
                    loadTabContent(elid, tradingitemscataloguestabs.istabitemsimport);
                    tradingitemscataloguestabs.istabitemsimport = true;
                    break;
            }
        }, 200);
    });

    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });

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
    $('input[name=b2b-service-toggle]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            updateB2BUsabilty();
        }, 1000);
    });
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
function loadTabContent(tabid, isfirstload) {
    if (!isfirstload) {
        tabid = tabid.replace('#', '');
        var $content;
        switch (tabid) {
            case 'general-locations':
                $content = $('#general-locations');
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
        if (tabid != 'general-candb' && tabid != 'general-groups')
            if (!$(".loadingoverlay").is(":visible")) {
                $content.LoadingOverlay("show");
            }

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
            break;
        case "general-workgroups":
            //init event customize filters for Location tab
            $('#wg-key-search').keyup(delay(function () {
                $('#wg-table').DataTable().ajax.reload();
            }, 500));

            $('#wg-processes-filter').change(function () {
                $('#wg-table').DataTable().ajax.reload();
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

                    if ($('#general-currency input[name=search]').val().length > 0)
                        $('#tblTaxrates').DataTable().search($('#general-currency input[name=search]').val(), true, false, false).draw();

                    $content.LoadingOverlay("hide");
                });
            };

            //init event customize filters for Taxes & Currency tab
            $('#general-currency input[name=search]').keyup(delay(function () {
                $('#tblTaxrates').DataTable().search($(this).val(), true, false, true).draw();
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
        case 'items':
            $("#search_dt").keyup(delay(function () {
                CallBackFilterDataItemOverViewServeSide();
            }, 1000));
            $('#slLocation_items').change(function () {
                applyItemOverviewFilter();
                CallBackFilterDataItemOverViewServeSide();
            });
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

                if ($('#tab-groups-items input[name=search]').val().length > 0)
                    $('#tab-groups-items .dataTable').DataTable().search($('#tab-groups-items input[name=search]').val(), true, false, false).draw();
            }
            $('#tab-groups-items input[name=search]').keyup(delay(function () {
                $('#tab-groups-items .dataTable').DataTable().search($(this).val(), true, false, true).draw();
            }, 500));
            break;
        case 'tab-groups-contacts':
            reloadTableContactGroup = function () {
                tradinggrouptabs.istabgroupscontactsload = false;
                tradinggeneraltabs.istabcontactsload = false;
                $('ul.nav-subgroups li a[href=#tab-groups-contacts]').click();

                if ($('#tab-groups-contacts input[name=search]').val().length > 0)
                    $('#tab-groups-contacts .dataTable').DataTable().search($('#tab-groups-contacts input[name=search]').val(), true, false, false).draw();
            }
            $('#tab-groups-contacts input[name=search]').keyup(delay(function () {
                $('#tab-groups-contacts .dataTable').DataTable().search($(this).val(), true, false, true).draw();
            }, 500));
            break;
        case 'items-pricing':
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
        case 'items-catalogues':
            var catalogType = $("a[href='#items-catalogues']").attr("catalogType");
            moveModalIntoBody('#items-catalogues .mn-modal-content');
            //this function in the pos.product.js

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
                var catalogType = getCatalogType();//this function in the pos.product.js
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
                var quickCatalogType = getCatalogType();//this function in the pos.product.js
                if (quickCatalogType == '1') {
                    SearchCatalogDistribution();
                } else {
                    SearchMenu();
                }
            };
            //init plugin
            $('#items-catalogues select.select2').select2({ placeholder: 'Please select' });
            $("#items-catalogues select.checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
            $('.mn-modal-content select.select2').select2({ placeholder: 'Please select' });
            var timeout = null;
            $("#items-catalogues input[name=search]").keyup(function () {
                clearTimeout(timeout);
                timeout = setTimeout(function () { SearchMenu(); }, 1000);
            });
            break;
        case 'items-catalogues-distribution':
            var catalogType = $("a[href='#items-catalogues-distribution']").attr("catalogType");
            moveModalIntoBody('#items-catalogues-distribution .mn-modal-content');
            //Override SuccessAction funtion in file pos.products.js
            SearchCatalogDistribution = function () {
                $('#items-catalogues-distribution #pos-menu-list').remove();
                var $content = $('#catalog-distribution-list');
                $content.empty();
                $content.LoadingOverlay("show");
                $content.load("/PointOfSaleMenu/LoadPosMenu",
                    {
                        catalogSearchType: catalogType,
                        salesChannel: $('#items-catalogues-distribution select[name=salechannel]').val(),
                        keyword: $('#items-catalogues-distribution input[name=search]').val(),
                        status: $('#items-catalogues-distribution select[name=status]').val()
                    }, function (response) {
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
                var catalogType = getCatalogType();//this function in the pos.product.js
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
                var quickCatalogType = $(".trading-items-catalogues-tabs > .active > a").attr('catalogType');
                if (quickCatalogType == '1') {
                    SearchCatalogDistribution();
                } else {
                    SearchMenu();
                }
            };
            //init plugin
            $('#items-catalogues-distribution select.select2').select2({ placeholder: 'Please select' });
            $("#items-catalogues-distribution select.checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
            $('.mn-modal-content select.select2').select2({ placeholder: 'Please select' });
            var timeout = null;
            $("#items-catalogues-distribution input[name=search]").keyup(function () {
                clearTimeout(timeout);
                timeout = setTimeout(function () { SearchCatalogDistribution(); }, 1000);
            });
            $('#items-catalogues-distribution .filter-tab select').attr('onchange', 'SearchCatalogDistribution()');
            break;
        case 'items-import':
            moveModalIntoBody('#' + tabid + ' .pr-modal-content');
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
            LoadingOverlayEnd();
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
                maxlength: 150,
                //minlength: 4
            },
            description: {
                required: true,
                //maxlength: 200,
            },
            extratags: {
                required: true,
                //maxlength: 200,
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
            return;
        }

        //if ($("#form_specifics_description").val() == "") {
        //    $("#frmTradingItem").validate().showErrors({ form_specifics_description: "Item description is required." });
        //   return;
        //}
        if (!$('#item_protags').val()) {
            $("#frmTradingItem").validate().showErrors({ extratags: "This field is required." });
            $.LoadingOverlay("hide");
            return;
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
        // Description: escape(tinymce.get("area3").getContent()),
        Description: $("#form_specifics_description").text(),
        DescriptionText: $("#form_specifics_description_text").val(),
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
        AdditionalInfos: [],//TODO: Save item from business profile
        AssociatedRecipes: [],
        VendorsPerLocation: [],
        PurchaseAccount: [],
        SalesAccount: [],
        TaxRates: [],
        InventoryAccount: [],
        IsCommunityProduct: false,
        IsBought: false,
        IsSold: false,
        IsActiveInAllLocations: true,
        GalleryItems: GetGalleryItems()
    };

    if ($('#item_brand').val() !== "" && $('#item_brand').val() !== null) {
        trader_item.AdditionalInfos.push({ Id: $('#item_brand').val(), Type: 1 });
    }

    if ($('#item_protags').val()) {
        var $tagNames = $("#item_protags").val();
        var $name = JSON.parse($tagNames);
        for (var i = 0; i < $name.length; i++) {
            trader_item.AdditionalInfos.push({ Id: 0, Name: $name[i].value, Type: 4 });
        }
    }

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

    // Additional information
    if ($('#item_brand').val() !== "" && $('#item_brand').val() !== null) {
        trader_item.AdditionalInfos.push({ Id: $('#item_brand').val() });
    }

    if ($("textarea[name='extratags']").val() !== "" && $("textarea[name='extratags']").val() !== null) {
        var lstProductTags = JSON.parse($("textarea[name='extratags']").val());
        for (var i = 0; i < lstProductTags.length; i++) {
            trader_item.AdditionalInfos.push({ Id: lstProductTags[i]['id'] })
        }
    }

    var _ivquantity = $('#ivquantity').val();
    var _ivunitcost = $('#ivunitcost').val();
    var createInventory = {
        LocationId: locationId,
        Quantity: _ivquantity ? _ivquantity : 0,
        Unitcost: _ivunitcost ? _ivunitcost : 0
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
    var _isSubscribe = $('#items').attr("SubscribeTrader");
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
            //data: "Description",
            render: function (value, type, row) {
                return '<a data-toggle="modal" href="#app-trader-item-description" onclick="showTraderItemDescription(\'' + row.Id + '\')">' + row.Description + '</a>';
            },
            orderable: true
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
    if ($.fn.DataTable.isDataTable("#tbldrivers")) {
        setTimeout(function () {
            $('#tbldrivers').DataTable().ajax.reload();
        }, 100);
    }
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
function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    }, 1);
};

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
function loadContentPriceList() {
    var $content_pricelist = $('#b2b-charges-content');
    $content_pricelist.empty();
    $content_pricelist.LoadingOverlay("show");
    $content_pricelist.load("/TraderChannels/LoadContentPriceList", { keyword: $("#txtplkeyword").val(), locationId: $('#logistics-charges select[name=locations]').val() }, function () {
        $content_pricelist.LoadingOverlay("hide");
    });
}
function loadVehicles() {
    $.get("/TraderChannels/GetVehiclesForSelect2", function (data) {
        dataVehicles = data;
    });
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
                                _htmlOptions += '<li><a href="javascript:void(0)" onclick="setOnOffDuty(' + data + ',2)">Set off duty</a></li>';
                            else
                                _htmlOptions += '<li><a href="javascript:void(0)" onclick="setOnOffDuty(' + data + ',1)">Set on duty</a></li>';
                            _htmlOptions += '<li><a href="javascript:void(0)" onclick="loadModalLocationChange(' + data + ')">Change location</a></li>';
                            _htmlOptions += '<li><a href="javascript:void(0)" onclick="deleteDriver(' + data + ')">Delete</a></li>';
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
function updateB2BUsabilty() {
    var $profileId = $('#hdfProfileId').val();
    var $activedB2B = $('input[name=b2b-service-toggle]').prop('checked');

    var _url = "/Commerce/ChangeB2BServiceAcessibilityStatus";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            profileId: $profileId,
            IsActive: $activedB2B
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).done(function () {
        LoadingOverlayEnd()
    });
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
    //Add a Contact
    if ($("#hdfcontactaccountId").length > 0)
        $("#hdfcontactaccountId").val(id);
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
function initPluginsOfTabArchived(isFirstLoad) {
    if (!isFirstLoad) {
        getArchivedPromotions();
        $('#tblArchivedPromotions').DataTable().ajax.reload();
        $('#promos-archive input[name=search]').keyup(delay(function () {
            $('#tblArchivedPromotions').DataTable().ajax.reload();
        }, 1000));
        $('#promos-archive select[name=type]').change(function () {
            $('#tblArchivedPromotions').DataTable().ajax.reload();
        });
        $('#promos-archive input[name=scheduleddate]').change(function () {
            $('#tblArchivedPromotions').DataTable().ajax.reload();
        });
    }
}
function getArchivedPromotions() {
    var $tblArchivedPromotions = $('#tblArchivedPromotions');
    $tblArchivedPromotions.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblArchivedPromotions.LoadingOverlay("show");
        } else {
            $tblArchivedPromotions.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        autoWidth: true,
        searchDelay: 800,
        pageLength: 10,
        deferLoading: 30,
        order: [[0, "asc"]],
        ajax: {
            "url": "/Commerce/GetArchivedPromotions",
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $('#promos-archive input[name=search]').val(),
                    "daterange": $('#promos-archive input[name=scheduleddate]').val(),
                    "type": $('#promos-archive select[name=type]').val(),
                });
            }
        },
        columns: [
            { "data": "Name", "orderable": true },
            { "data": "Type", "orderable": true },
            { "data": "StartDate", "orderable": true },
            { "data": "EndDate", "orderable": true }
        ]
    });
}

function initPluginsOfTabActive(isFirstLoad) {
    if (!isFirstLoad) {
        loadActivePromotions();
        $('#promos-active input[name=search]').keyup(delay(function () {
            loadActivePromotions();
        }, 1000));
        $('#promos-active select[name=type]').change(function () {
            loadActivePromotions();
        });
        $('#promos-active input[name=scheduleddate]').change(function () {
            loadActivePromotions();
        });
        $('#promos-active select[name=status]').change(function () {
            loadActivePromotions();
        });
    }
}

function loadActivePromotions(isHideSpinner) {
    var _paramaters = {
        keyword: $('#promos-active input[name=search]').val(),
        daterange: $('#promos-active input[name=scheduleddate]').val(),
        type: $('#promos-active select[name=type]').val(),
        status: $('#promos-active select[name=status]').val(),
    };

    var $content = $('#promos-active .flex-grid-quarters-lg');
    if (!isHideSpinner)
        $content.LoadingOverlay("show");
    $content.load("/Commerce/LoadActivePromotions", { filterModel: _paramaters }, function () {
        $content.LoadingOverlay("hide");
    });
}
function OpenPromotion(promotionKey) {
    $.LoadingOverlay("show");
    window.location.href = '/Commerce/Promotion?key=' + promotionKey;
}

function loadModalPromotionAddEdit(promotionKey) {
    $.LoadingOverlay("show");
    var $promodal = $('#moniback-promo-add');
    $promodal.empty();
    $promodal.modal('show');
    $promodal.load("/Commerce/LoadModalPromotionAddEdit", { promotionKey: promotionKey }, function () {
        $("#moniback-promo-add .checkmulti").multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#moniback-promo-add .select2').select2({ placeholder: "Please select" });
        $('#moniback-promo-add .singledateandtime').daterangepicker({
            singleDatePicker: true,
            timePicker: true,
            autoApply: true,
            showDropdowns: true,
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
            }
        });
        $('#moniback-promo-add .chktoggle').bootstrapToggle();
        initFormPromotionValidate();
        setTimeout(function () {
            $.LoadingOverlay("hide");
        }, 500);

    });
}

function initFormPromotionValidate() {
    var $frmAddEditPromotion = $('#frmAddEditPromotion');

    $frmAddEditPromotion.validate({
        ignore: "",
        invalidHandler: function (e, validator) {
            console.log(validator, 'validator');
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $frmAddEditPromotion.submit(function (e) {
        e.preventDefault();

        // run when turn off time range
        setMinTimeFromToTimePromotion();

        if (isBusy)
            return;
        if ($frmAddEditPromotion.valid()) {
            $.LoadingOverlay("show");
            var fileImg = document.getElementById("filefeaturedimg").files;
            var imgobjkey = $("#hdffeaturedimguri").val();
            if (!imgobjkey || (fileImg && fileImg.length > 0)) {
                UploadMediaS3ClientSide("filefeaturedimg").then(function (mediaS3Object) {
                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#hdffeaturedimguri").val(mediaS3Object.objectKey);
                        savePromotion();
                    }
                });
            }
            else
                savePromotion();
        }
    });
}

function switchpromos(elm) {
    $(elm).valid();
    $('.promotype').hide();
    var typeval = $(elm).val();
    if (typeval == "1") {
        //#itemdiscount
        $('#itemdiscount').show();
    } else if (typeval == "2") {
        //#orderdiscount
        $('#orderdiscount').show();
    }
    setValidateItemAndOrderByType(typeval);
}

var $promotionKey = "";
var $promotionName = "";
var $promotionStop = false;
var $promotionMessage = false;
function startPromotion(promotionKey) {
    stopStartPromotion(promotionKey, false);
}

function stopPromotion(promotionKey) {
    //show modal confirm
    stopStartPromotion(promotionKey, true);
}

/**
 * open Promotion Message Model
 * @param {any} promotionKey
 * @param {any} promotionName
 * @param {any} isStop {boolean} isStop (true if Stop/ false if Start and set to IsHalted)
 */
function openPromotionMessageModel(promotionKey, promotionName, totalClaimed, isStop) {
    $promotionKey = promotionKey;
    $promotionName = promotionName;
    $promotionStop = isStop;
    var $promotionMessageModal = $('#promotion-message-modal');
    $("#promotion-name").text($promotionName);
    $("#total-claimed").text(totalClaimed);
    $promotionMessage = $("#business-name").val() + " have stopped their \"" + $promotionName + "\" promotion."
        + "\nAny vouchers you have not redeemed for this promotion will be affected."
        + "\nIf you have any questions please leave us a message.";
    $("#message-title").text("Stopping");
    if (!$promotionStop) {
        $("#message-title").text("Restarting");
        $promotionMessage = $("#business-name").val() + " have restarted their \"" + $promotionName + "\" promotion."
            + "\nAny vouchers you have not redeemed for this promotion should now be available."
            + "\nIf you have any questions please leave us a message.";
    }
    $("#promotion-message-text").val($promotionMessage);
    $promotionMessageModal.modal('show');
}

/**
 * Start or Stop promotion
 * @param {string} promotionKey
 * @param {boolean} isStop (true if Stop/ false if Start andset to IsHalted)
 */
function stopStartPromotion() {
    $.LoadingOverlay("show");
    $.post("/Commerce/StopStartPromotion", { promotionKey: $promotionKey, isStop: $promotionStop, message: $("#promotion-message-text").val() }, function (response) {
        LoadingOverlayEnd();
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Promotions & vouchers");
            loadActivePromotions(true);
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Promotions & vouchers");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
        }
    });
}

function archivePromotion(promotionKey) {
    $.LoadingOverlay("show");
    $.post("/Commerce/ArchivePromotion", { promotionKey: promotionKey }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Promotions & vouchers");
            loadActivePromotions(true);
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Promotions & vouchers");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
        }
        LoadingOverlayEnd();
    });
}

//TODO: optimise or remove
function savePromotionDraftOld() {
    isDraft = true;

    var $frmAddEditPromotion = $('#frmAddEditPromotion');

    $frmAddEditPromotion.validate({
        ignore: "",
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });

    // run when turn off time range
    setMinTimeFromToTimePromotion();

    if ($frmAddEditPromotion.valid()) {
        $.LoadingOverlay("show");
        var fileImg = document.getElementById("filefeaturedimg").files;
        var imgobjkey = $("#hdffeaturedimguri").val();
        if (!imgobjkey || (fileImg && fileImg.length > 0)) {
            UploadMediaS3ClientSide("filefeaturedimg").then(function (mediaS3Object) {
                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    $("#hdffeaturedimguri").val(mediaS3Object.objectKey);
                    savePromotion();
                }
            });
        }
        else {
            savePromotion();
        }
    }
}

function savePromotion() {
    var frmname = '#frmAddEditPromotion';
    var days = $(frmname + ' select[name=Days]').val();
    if (!days)
        days = [];
    var _paramaters = {
        model: {
            PromotionKey: $(frmname + ' input[name=PromotionKey]').val(),
            Name: $(frmname + ' input[name=Name]').val(),
            Description: $(frmname + ' textarea[name=Description]').val(),
            StartDateString: $(frmname + ' input[name=StartDate]').val(),
            EndDateString: $(frmname + ' input[name=EndDate]').val(),
            DisplayDateString: ($('#chkAdvertiseCustome').prop('checked') ? $(frmname + ' input[name=DisplayDate]').val() : $(frmname + ' input[name=StartDate]').val()),
            DaysOfweek: ($('#chkSpecificTime').prop('checked') ? days.join(',') : []),
            FromTime: $(frmname + ' input[name=FromTime]').val(),
            ToTime: $(frmname + ' input[name=ToTime]').val(),
            VoucherExpiryDateString: ($('#chkExpiryDate').prop('checked') ? $("#expiry-date").val() : ''),
            IsDraft: $(frmname + ' input[name=IsDraft]').val(),
            Audience: {},
        },
        itemDiscountVoucherInfo: {},
        orderDiscountVoucherInfo: {}
    }
    var locIds = $(frmname + ' select[name=Locations]').val();

    var _locations = [];
    $.each(locIds, function (index, value) {
        _locations.push({ Id: value });
    });
    var type = $(frmname + ' select[name=Type]').val();
    var plan = $(frmname + ' input[name=Plan]').val();
    var audience = $(frmname + ' select[name=locationVisibility]').val();

    _paramaters.featuredImageUri = $('#hdffeaturedimguri').val();
    _paramaters.model.Type = type;
    _paramaters.model.PlanType = {
        Id: plan
    };

    //alert('Selected Value: ' + audience);

    if (audience == "1") {
        _paramaters.model.Audience = {
            LocationVisibility: audience,
            Distance: $(frmname + ' input[name=distance]').val(),
            DistanceFactor: $(frmname + ' select[name=factor]').val(),
            BusinessLocation: $(frmname + ' select[name=location]').val(),
        };
    }
    else {
        _paramaters.model.Audience = {
            LocationVisibility: audience,
        };
    }

    if (type == "1") {
        _paramaters.itemDiscountVoucherInfo = {
            Id: 0,
            MaxVoucherCount: $(frmname + ' input[name=MaxVoucherCount]').val(),
            MaxVoucherCountPerCustomer: $(frmname + ' input[name=MaxVoucherCountPerCustomer]').val(),
            TermsAndConditions: $(frmname + ' textarea[name=TermsAndConditions]').val(),
            Locations: ($('#chkSpecificLocation').prop('checked') ? _locations : []),
            ItemSKU: $(frmname + ' input[name=hdfItemSKU]').val(),
            ItemDiscount: $(frmname + ' input[name=ItemDiscount]').val(),
            MaxNumberOfItemsPerOrder: $(frmname + ' input[name=MaxNumberOfItemsPerOrder]').val()
        };

        if (!_paramaters.itemDiscountVoucherInfo.MaxVoucherCount)//infinite claims
            _paramaters.itemDiscountVoucherInfo.MaxVoucherCount = -1;
        if (!_paramaters.itemDiscountVoucherInfo.MaxNumberOfItemsPerOrder)//infinite claims
            _paramaters.itemDiscountVoucherInfo.MaxNumberOfItemsPerOrder = -1;
    } else if (type == "2") {
        _paramaters.orderDiscountVoucherInfo = {
            Id: 0,
            MaxVoucherCount: $(frmname + ' input[name=MaxVoucherCount]').val(),
            MaxVoucherCountPerCustomer: $(frmname + ' input[name=MaxVoucherCountPerCustomer]').val(),
            TermsAndConditions: $(frmname + ' textarea[name=TermsAndConditions]').val(),
            Locations: ($('#chkSpecificLocation').prop('checked') ? _locations : []),
            OrderDiscount: $(frmname + ' input[name=OrderDiscount]').val(),
            MaxDiscountValue: $(frmname + ' input[name=MaxDiscountValue]').val()
        };
        if (!_paramaters.orderDiscountVoucherInfo.MaxVoucherCount)//infinite claims
            _paramaters.orderDiscountVoucherInfo.MaxVoucherCount = -1;
        if (!_paramaters.orderDiscountVoucherInfo.MaxDiscountValue)//infinite claims
            _paramaters.orderDiscountVoucherInfo.MaxDiscountValue = -1;
    }
    $.ajax({
        type: 'post',
        url: '/Commerce/SavePromotion',
        data: _paramaters,
        dataType: 'json',
        success: function (response) {
            console.log(response, 'res');
            if (response.result) {

                console.log(_paramaters.model, 'model');

                //Redirect for payment if promotion is not a draft or free promotion
                if (_paramaters.model.IsDraft == 'false' && plan !== '1' && response.Object !== null) {
                    
                    var { authorization_url, access_code, reference } = response.Object;
                    const popup = new PaystackPop();

                    //initialise payment
                    popup.newTransaction({
                        accessCode: access_code,

                        onLoad: (res) => {
                            console.log("onLoad: ", res);
                        },
                        onSuccess: (res) => {
                            //verify that the payment was successful at the backend
                            if (res?.status == "success") {

                                //ref,
                                //loyPromotionId
                                //Update the paymentTransaction for the reference

                                $.ajax({
                                    type: 'post',
                                    url: '/Commerce/UpdatePromotionPayment',
                                    data: {
                                        reference: res?.reference,
                                        status: "success"
                                    },
                                    dataType: 'json',
                                    success: function (response) {
                                        //console.log(response, 'res');
                                        if (response.result) {                                          
                                            //Show success modal
                                            cleanBookNotification.createSuccess();
                                        } else if (!response.result && response.msg) {
                                            cleanBookNotification.error(response.msg, "Promotions & vouchers");
                                        } else {
                                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                                        }
                                    },
                                    error: function (er) {
                                        cleanBookNotification.error(er.responseText, "Qbicles");
                                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                                    }
                                });                                
                            }
                            else {
                                cleanBookNotification.error("Error processing payment", "Promotions & vouchers");
                            }

                            console.log(res, 'res');
                        },
                        onCancel: (res) => {
                            console.log("onCancel", res);
                        },
                        onError: (err) => {

                            $.ajax({
                                type: 'post',
                                url: '/Commerce/UpdatePromotionPayment',
                                data: {
                                    reference: err?.reference,
                                    status: "error"
                                },
                                dataType: 'json',
                                success: function (response) {
                                    cleanBookNotification.error(response.msg, "Promotions & vouchers");
                                },
                                error: function (er) {
                                    cleanBookNotification.error(er.responseText, "Qbicles");
                                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                                }
                            });       

                            cleanBookNotification.error("Error processing payment", "Promotions & vouchers");
                            console.log("Error: ", err.message);
                        }
                    });               
                }
                else {
                    cleanBookNotification.createSuccess();
                }

                $('#moniback-promo-add').modal('hide');
                loadActivePromotions(false);

            } else if (!response.result && response.msg) {
                cleanBookNotification.error(response.msg, "Promotions & vouchers");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
        }
    }).always(function () {
        loadActivePromotions(false);
        LoadingOverlayEnd();
    });
}

function retryPromotionPayment(promotionKey) {
    $.LoadingOverlay("show");

    $.ajax({
        type: 'post',
        url: '/Commerce/RetryPromotionPayment',
        data: { promotionKey: promotionKey },
        dataType: 'json',
        success: function (response) {
            console.log(response, 'res');
            if (response.result) {

                var { authorization_url, access_code, reference } = response.Object;

                const popup = new PaystackPop();

                //initialise payment
                popup.newTransaction({
                    accessCode: access_code,

                    onLoad: (res) => {
                        console.log("onLoad: ", res);
                    },
                    onSuccess: (res) => {
                        //verify that the payment was successful at the backend
                        if (res?.status == "success") {

                            $.ajax({
                                type: 'post',
                                url: '/Commerce/UpdatePromotionPayment',
                                data: {
                                    reference: res?.reference,
                                    status: "success"
                                },
                                dataType: 'json',
                                success: function (response) {
                                    //console.log(response, 'res');
                                    if (response.result) {
                                        loadActivePromotions(false);
                                        //Show success modal
                                        cleanBookNotification.createSuccess();
                                    } else if (!response.result && response.msg) {
                                        cleanBookNotification.error(response.msg, "Promotions & vouchers");
                                    } else {
                                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                                    }
                                },
                                error: function (er) {
                                    cleanBookNotification.error(er.responseText, "Qbicles");
                                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                                }
                            });
                        }
                        else {
                            cleanBookNotification.error("Error processing payment", "Promotions & vouchers");
                        }

                        console.log(res, 'res');
                    },
                    onCancel: (res) => {
                        console.log("onCancel", res);
                    },
                    onError: (err) => {

                        $.ajax({
                            type: 'post',
                            url: '/Commerce/UpdatePromotionPayment',
                            data: {
                                reference: err?.reference,
                                status: "error"
                            },
                            dataType: 'json',
                            success: function (response) {
                                cleanBookNotification.error(response.msg, "Promotions & vouchers");
                            },
                            error: function (er) {
                                cleanBookNotification.error(er.responseText, "Qbicles");
                                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
                            }
                        });

                        cleanBookNotification.error("Error processing payment", "Promotions & vouchers");
                        console.log("Error: ", err.message);
                    }
                });

            } else if (!response.result && response.msg) {
                cleanBookNotification.error(response.msg, "Promotions & vouchers");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
            }

            loadActivePromotions(true);
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Promotions & vouchers");
        }
    })
    .always(function () {
        LoadingOverlayEnd();
    });
}

function setValidateItemAndOrderByType(type) {
    if (type == "1") {
        //#itemdiscount
        $('#itemdiscount input[name=ItemSKU]').attr("required", true);
        $('#itemdiscount input[name=ItemDiscount]').attr("required", true);
        //$('#itemdiscount input[name=MaxNumberOfItemsPerOrder]').attr("required", true);
        $('#orderdiscount input[name=OrderDiscount]').removeAttr("required");
        //$('#orderdiscount input[name=MaxDiscountValue]').removeAttr("required");
    } else if (type == "2") {
        //#orderdiscount
        $('#itemdiscount input[name=ItemSKU]').removeAttr("required", true);
        $('#itemdiscount input[name=ItemDiscount]').removeAttr("required", true);
        //$('#itemdiscount input[name=MaxNumberOfItemsPerOrder]').removeAttr("required", true);
        $('#orderdiscount input[name=OrderDiscount]').attr("required", true);
        //$('#orderdiscount input[name=MaxDiscountValue]').attr("required", true);
    }
}

function showModalFindSKU() {
    var sku = $('#item_sku').val();
    var url = "/Commerce/LoadModalFindTraderItem?sku=" + sku;
    url = url.replace(/\s/g, "%20");
    $("#app-trader-pos-itemlist").load(url);
}

function selectTraderItem(id, sku, imagUri) {
    $('#itemdiscount input[name=hdfItemSKU]').val(sku);
    $('#item_sku').val(sku);
    $('#app-trader-pos-itemlist').modal('hide');
}

function calculateDaysForVoucherUsed() {
    var startDate = $('#moniback-promo-add input[name=StartDate]').val();
    var endDate = $('#moniback-promo-add input[name=EndDate]').val();
    $.post('/Commerce/GetCalculateDatesForPromotions', { endDate: endDate, startDate: startDate }, function (response) {
        var $selectdays = $("#moniback-promo-add select[name=Days]");
        $("#moniback-promo-add select[name=Days] > option").each(function () {
            //alert(this.text + ' ' + this.value);
            if (response) {
                var objDayCal = response.find(o => o.ShortName === this.value);
                if (!objDayCal) {
                    $(this).attr("disabled", true);
                } else
                    $(this).attr("disabled", false);
            }
        });
        $selectdays.multiselect("refresh");
    });
}

function calculateDaysForVoucherUsedInBulkDeal() {
    var startDate = $('#moniback-promo-add input[name=StartDate]').val();
    var endDate = $('#moniback-promo-add input[name=EndDate]').val();
    $.post('/Commerce/GetCalculateDatesForPromotions', { endDate: endDate, startDate: startDate }, function (response) {
        var $selectdays = $("#moniback-promo-add select[name=Days]");
        $("#moniback-promo-add select[name=Days] > option").each(function () {
            //alert(this.text + ' ' + this.value);
            if (response) {
                var objDayCal = response.find(o => o.ShortName === this.value);
                if (!objDayCal) {
                    $(this).attr("disabled", true);
                } else
                    $(this).attr("disabled", false);
            }
        });
        $selectdays.multiselect("refresh");
    });
}

function setMinTimeFromToTimePromotion() {
    var isSetTime = $('#chkSpecificTime').prop('checked');
    var fromTimeVal = $('#frmAddEditPromotion input[name=FromTime]').val();
    var $totime = $('#frmAddEditPromotion input[name=ToTime]');
    if (fromTimeVal && isSetTime) {
        var listTime = fromTimeVal.split(":");
        listTime[1]++;
        listTime[0] = listTime[0].toLocaleString('en-US', { minimumIntegerDigits: 2, useGrouping: false });
        listTime[1] = listTime[1].toLocaleString('en-US', { minimumIntegerDigits: 2, useGrouping: false });
        fromTimeVal = listTime[0] + ":" + listTime[1];
        $totime.attr("min", fromTimeVal);
    } else {
        $totime.removeAttr("min");
    }
}

//customize Date-picker
function initDateTimeCustomize() {
    var currentDate = moment().format($dateTimeFormatByUser);
    var endCurrentDate = moment().endOf('day');
    //init Advertise time
    $('.singledateandtime-displaydate').daterangepicker({
        "minDate": currentDate,
        "maxDate": endCurrentDate,
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
        }
    })
    // init Start and End
    if ($('#frmAddEditPromotion input[name=StartDate], #frmAddEditPromotion input[name=EndDate]').length) {
        $('#frmAddEditPromotion input[name=StartDate], #frmAddEditPromotion input[name=EndDate]').daterangepicker({
            "alwaysShowCalendars": true,
            "minDate": currentDate,
            timePicker: true,
            autoApply: true,
            autoUpdateInput: false,
            showDropdowns: true,
            cancelClass: "btn-danger",
            locale:
            {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
            },
        }, function (start, end, label) {
            var selectedStartDate = start.format($dateTimeFormatByUser);
            var selectedEndDate = end.format($dateTimeFormatByUser);

            $checkinInput = $('#frmAddEditPromotion input[name=StartDate]');
            $checkoutInput = $('#frmAddEditPromotion input[name=EndDate]');

            $checkinInput.val(selectedStartDate);
            $checkoutInput.val(selectedEndDate);

            var checkOutPicker = $checkoutInput.data('daterangepicker');
            checkOutPicker.setStartDate(selectedStartDate);
            checkOutPicker.setEndDate(selectedEndDate);

            var checkInPicker = $checkinInput.data('daterangepicker');
            checkInPicker.setStartDate(selectedStartDate);
            checkInPicker.setEndDate(selectedEndDate);

            //limit end date of Advertise by re-init
            $('.singledateandtime-displaydate').daterangepicker({
                "minDate": currentDate,
                "maxDate": selectedEndDate,
                singleDatePicker: true,
                timePicker: true,
                autoApply: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale: {
                    cancelLabel: 'Clear',
                    format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
                }
            })
            calculateDaysForVoucherUsed();
        });
    }
    if ($('#frmAddEditDeal input[name=StartDate], #frmAddEditDeal input[name=EndDate]').length) {
        $('#frmAddEditDeal input[name=StartDate], #frmAddEditDeal input[name=EndDate]').daterangepicker({
            "alwaysShowCalendars": true,
            "minDate": currentDate,
            timePicker: true,
            autoApply: true,
            autoUpdateInput: false,
            showDropdowns: true,
            cancelClass: "btn-danger",
            locale:
            {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
            },
        }, function (start, end, label) {
            var selectedStartDate = start.format($dateTimeFormatByUser);
            var selectedEndDate = end.format($dateTimeFormatByUser);

            $checkinInput = $('#frmAddEditDeal input[name=StartDate]');
            $checkoutInput = $('#frmAddEditDeal input[name=EndDate]');

            $checkinInput.val(selectedStartDate);
            $checkoutInput.val(selectedEndDate);

            var checkOutPicker = $checkoutInput.data('daterangepicker');
            checkOutPicker.setStartDate(selectedStartDate);
            checkOutPicker.setEndDate(selectedEndDate);

            var checkInPicker = $checkinInput.data('daterangepicker');
            checkInPicker.setStartDate(selectedStartDate);
            checkInPicker.setEndDate(selectedEndDate);

            //limit end date of Advertise by re-init
            $('.singledateandtime-displaydate').daterangepicker({
                "minDate": currentDate,
                "maxDate": selectedEndDate,
                singleDatePicker: true,
                timePicker: true,
                autoApply: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale: {
                    cancelLabel: 'Clear',
                    format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
                }
            })
            calculateDaysForVoucherUsedInBulkDeal();
        });
    }

}

function GetGalleryItems() {
    var items = $('#itemgalerylist figure');
    var galleryItems = []

    if (items.length > 0) {
        for (var i = 0; i < items.length; i++) {
            var uri = $($(items[i]).find('span.image_row')).text();
            var galleryItem = {
                FileUri: uri,
                Order: i
            };
            galleryItems.push(galleryItem)
        }
    }
    return galleryItems;
}

function AddImage() {
    $('#image-galery-input').trigger('click');
}
function ItemGaleryDelete(id) {
    var check = confirm('Are you sure you want to delete this image?');
    if (check) {
        $("#figure-" + id).remove();

        var items = $('#itemgalerylist figure');
        if (items.length <= 0) {
            $(".gallerynone").show();
            $(".gallerystart").hide();
        }
    }
}

function changeOrderGalery(oldIndex, newIndex) {
    // get order = newIndex -> set order = oldIndex
    // get order = oldIndex -> set order = newIndex
    console.log(oldIndex);
}

function AddImageGalery() {
    $('#theform').LoadingOverlay("show");

    UploadMediaS3ClientSide("image-galery-input").then(function (mediaS3Object) {
        //trader_item.ImageUri = mediaS3Object.objectKey;
        //$('#form_specifics_icon_text').val(mediaS3Object.objectKey);
        //ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            $('#theform').LoadingOverlay("hide", true);
            return;
        }
        var newImage = "<figure id='figure-" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='flex-contain' id='" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='col img'>";
        newImage += "<div style=\"background-image: url('" + apiDoc + mediaS3Object.objectKey + "')\"></div>";
        newImage += "</div>";
        newImage += "<div class='col options'>";
        newImage += '<span class="image_row" style="display:none">' + mediaS3Object.objectKey + '</span>'
        newImage += "<div class='dropdown contactoptside'>";
        newImage += "<button class='btn btn-primary dropdown-toggle' type='button' data-toggle='dropdown'>";
        newImage += "<i class='fa fa-ellipsis-h'></i>";
        newImage += "</button>";
        newImage += "<ul class='dropdown-menu dropdown-menu-right'>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryEdit('" + mediaS3Object.objectKey + "')\">Edit</a></li>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryDelete('" + mediaS3Object.objectKey + "')\">Delete</a></li>";
        newImage += "</ul></div></div></div>";
        //<li><a onclick="ItemGaleryEdit('@image.FileUri')">Edit</a></li>
        //<li><a onclick="ItemGaleryDelete('@image.FileUri')">Delete</a></li>
        $("#itemgalerylist").append(newImage);
        $(".gallerynone").hide();
        $(".gallerystart").show();
        $('#theform').LoadingOverlay("hide", true);
    });
}

var $imageChanged = "";

function ItemGaleryEdit(id) {
    $imageChanged = id;
    $('#image-galery-edit').trigger('click');
}

function ChangeImageGalery() {
    $('#theform').LoadingOverlay("show");

    UploadMediaS3ClientSide("image-galery-edit").then(function (mediaS3Object) {
        //trader_item.ImageUri = mediaS3Object.objectKey;
        //$('#form_specifics_icon_text').val(mediaS3Object.objectKey);
        //ItemProductSubmit(trader_item, createInventory, currentLocationId, isCurrentLocation);

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            $('#theform').LoadingOverlay("hide", true);
            return;
        }

        $("#" + $imageChanged).remove();

        var newImage = "<div class='flex-contain' id='" + mediaS3Object.objectKey + "'>";
        newImage += "<div class='col img'>";
        newImage += "<div style=\"background-image: url('" + apiDoc + mediaS3Object.objectKey + "')\"></div>";
        newImage += "</div>";
        newImage += "<div class='col options'>";
        newImage += '<span class="image_row" style="display:none">' + mediaS3Object.objectKey + '</span>'
        newImage += "<div class='dropdown contactoptside'>";
        newImage += "<button class='btn btn-primary dropdown-toggle' type='button' data-toggle='dropdown'>";
        newImage += "<i class='fa fa-ellipsis-h'></i>";
        newImage += "</button>";
        newImage += "<ul class='dropdown-menu dropdown-menu-right'>";

        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryEdit('" + mediaS3Object.objectKey + "')\">Edit</a></li>";
        newImage += "<li><a style='cursor: pointer !important;' onclick=\"ItemGaleryDelete('" + mediaS3Object.objectKey + "')\">Delete</a></li>";
        newImage += "</ul></div></div>";

        $("#figure-" + $imageChanged).append(newImage);

        var figure = document.getElementById("figure-" + $imageChanged);
        figure.setAttribute("id", "figure-" + mediaS3Object.objectKey);

        $('#theform').LoadingOverlay("hide", true);
    });
}