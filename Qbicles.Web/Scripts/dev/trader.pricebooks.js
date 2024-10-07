var $isEditVersionName = false;

var $priceBookId = $("#pricebook-id").val();
var $priceBookInfoId = 0;
var $groupId = 0;
var $instanceId = 0;

$(document).ready(function () {
    if (!currencySetting)
        getCurrencySettings();
    if ($priceBookId > 0) {
        $('#versions-tab').show();
        $('#versioning').show();
    }

});
function getCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
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
function SaleChannelChange() {
    var ajaxUri = '/TraderPriceBooks/ProductGroupByChannel?id=' + $priceBookId + '&channel=' + $('#price-book-sale-channel').val();
    $.LoadingOverlay("show");
    $("#product-group-channel").empty();
    $('#product-group-channel').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};

function CreatePriceBook(id) {

    
    GetCookieAsync("CurrentLocationManage").then(function (locationId) {
        
        var pbProductItemGroup = [];
        var groups = $("#price-book-product-group").val();
        if (groups !== null && groups.length > 0) {
            groups = groups.toString().split(",");
            $.each(groups,
                function (index, itemId) {
                    var pr = {
                        Id: itemId
                    };
                    pbProductItemGroup.push(pr);
                });
        }

        return priceBook = {
            Id: id,
            Name: $("#price-book-name").val(),
            SalesChannel: $("#price-book-sale-channel").val(),
            Location: {
                Id: locationId
            },
            AssociatedProductGroups: pbProductItemGroup,
            Description: $("#price-book-description").val()
        };
    });



    //var locationId = null;
    //setTimeout(function () {
    //    locationId = getCookie("CurrentLocationManage");
    //}, 1000);
    



    //var pbProductItemGroup = [];
    //var groups = $("#price-book-product-group").val();
    //if (groups !== null && groups.length > 0) {
    //    groups = groups.toString().split(",");
    //    $.each(groups,
    //        function (index, itemId) {
    //            var pr = {
    //                Id: itemId
    //            };
    //            pbProductItemGroup.push(pr);
    //        });
    //}

    //return priceBook = {
    //    Id: id,
    //    Name: $("#price-book-name").val(),
    //    SalesChannel: $("#price-book-sale-channel").val(),
    //    Location: {
    //        Id: locationId
    //    },
    //    AssociatedProductGroups: pbProductItemGroup,
    //    Description: $("#price-book-description").val()
    //};
};

SavePriceBook = function () {
    if (!$('#form-pricebook').valid()) {
        return;
    }
    var groups = $("#price-book-product-group").val();
    if (groups === null || groups.length === 0) {
        cleanBookNotification.error(_L("ERROR_MSG_634"), "Qbicle");
        return;
    }
    $.ajax({
        url: "/TraderPriceBooks/CheckExistName",
        data: { priceBookId: $priceBookId, priceBookName: $("#price-book-name").val() },
        type: "GET",
        dataType: "json"
    }).done(function (refModel) {
        if (refModel.result) {
            $("#form-pricebook").validate().showErrors({ pricebookname: _L("ERROR_MSG_283") });
            return;
        }

        $.ajax({
            type: "POST",
            url: "/Commons/GetCookie",
            data: { key: "CurrentLocationManage" },
            dataType: 'json',
            success: function (response) {
                
                 locationId = response.Object;
                var pbProductItemGroup = [];
                var groups = $("#price-book-product-group").val();
                if (groups !== null && groups.length > 0) {
                    groups = groups.toString().split(",");
                    $.each(groups,
                        function (index, itemId) {
                            var pr = {
                                Id: itemId
                            };
                            pbProductItemGroup.push(pr);
                        });
                }

                var priceBook = {
                    Id: $priceBookId,
                    Name: $("#price-book-name").val(),
                    SalesChannel: $("#price-book-sale-channel").val(),
                    Location: {
                        Id: locationId
                    },
                    AssociatedProductGroups: pbProductItemGroup,
                    Description: $("#price-book-description").val()
                };

                //var priceBook = CreatePriceBook($priceBookId);
                $.ajax({
                    type: 'post',
                    url: '/TraderPriceBooks/SavePriceBook',
                    data: { priceBook: priceBook },
                    datatype: 'json',
                    success: function (res) {
                        if (res.actionVal === 3) {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                            return;
                        } else if (res.actionVal === 1) {
                            cleanBookNotification.createSuccess();
                            $priceBookId = res.msgId;
                        } else if (res.actionVal === 2) {
                            cleanBookNotification.updateSuccess();
                        }

                        $('#versions-tab').show();
                        $('#versioning').show();
                    },
                    error: function (err) {
                       
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                });
            },
            error: function (err) {
               
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });



        
        
    }).fail(function () {
        $("#form-pricebook").validate().showErrors({ pricebookname: _L("ERROR_MSG_284") });
        return;
    });
};

GoToVersionTab = function (ev) {
    var ajaxUri = '/TraderPriceBooks/PriceBookVersion?pricebookId=' + $priceBookId;
    $.LoadingOverlay("show");
    $('#pb-ver').empty();
    $("#title-pricebook").text("Edit");
    $('#pb-ver').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });


    if (ev === 'button') {
        $('[href="#pb-ver"]').tab('show');
    }
       //$('#versions-tab').trigger('click');
};

ShowAddPricebookVersion = function () {
    ClearError();
    $("#version-name").val("");
    $("#app-trader-pricebook-version-add").modal('toggle');
}

SavePricebookVersion = function () {
    if (!$('#form-add-version').valid()) {
        return;
    }
    var version = {
        VersionName: $('#version-name').val(),
        ParentPriceBook: {
            Id: $priceBookId
        }
    };

    $.ajax({
        type: 'post',
        url: '/TraderPriceBooks/SavePricebookVersion',
        data: { version: version },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $("#app-trader-pricebook-version-add").modal("hide");
                $("body").removeClass("modal-open");
                $('.modal-backdrop').remove();
                cleanBookNotification.createSuccess();
                $.LoadingOverlay("show");
                var ajaxUri = '/TraderPriceBooks/PriceBookVersion?pricebookId=' + $priceBookId;
                $('#pb-ver').empty();
                $("#title-pricebook").text("Edit");
                $('#pb-ver').load(ajaxUri, function () {

                    LoadingOverlayEnd();
                });
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form-add-version');
        LoadingOverlayEnd();
    });
};

//Version management list/ Add/Edit version
EditVersionManagement = function () {
    $.LoadingOverlay("show");
    var ajaxUri = '/TraderPriceBooks/PricebookVersionsManagement?pricebookId=' + $priceBookId;
    $('#price-book-version-panel').empty();
    $('#price-book-version-panel').load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};

// Show Prices panel
EditPricesInfo = function (instanceId) {
    $.LoadingOverlay("show");
    var ajaxUri = '/TraderPriceBooks/PricebookVersionsSummerPrices?instanceId=' + instanceId;
    $('#price-book-version-panel').empty();
    $('#price-book-version-panel').load(ajaxUri, function () {
        $('#versions-help').hide();
        $('#manage').show();
        LoadingOverlayEnd();
    });
};

CopyPricesInfo = function (instanceId) {
    $.LoadingOverlay("show");
    var ajaxUri = '/TraderPriceBooks/CopyPricebookVersion?instanceId=' + instanceId;
    $('#price-book-version-panel').empty();
    $('#price-book-version-panel').load(ajaxUri, function () {
        $('#versions-help').hide();
        $('#manage').show();
        LoadingOverlayEnd();
    });
};


SearchItemOnTable = function (groupId) {
    var searchText = $("#input-search-table-info-" + groupId).val();
    $("#table-info-" + groupId).DataTable().search(searchText).draw();
};

ProductGroupsDefault = function (infoId, groupName, groupId, instanceId) {
    $.LoadingOverlay("show");
    $("#group-name-info").text(groupName);

    $priceBookInfoId = infoId;
    $groupId = groupId;
    $instanceId = instanceId;

    $.ajax({
        type: 'GET',
        url: '/TraderPriceBooks/GetProductGroupPriceDefaultId',
        data: { id: infoId },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $("#info-markup-type").text(response.Object.MarkupPercentage);

                $("#info-discount-type").text(response.Object.DiscountPercentage);

                $("#info-markup").val(response.Object.MarkUp);

                $("#info-discount").val(response.Object.Discount);

            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


GroupTabClick = function (groupId) {
    //alert(groupId);
};


ApplyMarkupDiscount = function (applyType) {
    if (!$("#form-pricebook-info").valid()) {
        return;
    }

    var markupDiscount = {
        Id: $priceBookInfoId,
        MarkupPercentage: $("#info-markup-type").text(),
        MarkUp: $("#info-markup").val(),
        DiscountPercentage: $("#info-discount-type").text(),
        Discount: $("#info-discount").val(),
        ApplyType: applyType
    }
    $.ajax({
        type: 'post',
        url: '/TraderPriceBooks/ApplyMarkupDiscount',
        data: { markupDiscount: markupDiscount },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                var markupPercentage = currencySetting.CurrencySymbol + " markup", discountPercentage = currencySetting.CurrencySymbol + " discount";
                if (markupDiscount.MarkupPercentage === "%") {
                    markupPercentage = "markup %";
                }
                if (markupDiscount.DiscountPercentage === "%") {
                    discountPercentage = "discount %";
                }
                $("#defaul-markup-info-" + $priceBookInfoId).text(markupPercentage.replace("markup", markupDiscount.MarkUp));
                $("#defaul-discount-info-" + $priceBookInfoId).text(discountPercentage.replace("discount", markupDiscount.Discount));

                cleanBookNotification.updateSuccess();

                RenderTablePriceBookPrices();

            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        ClearError();
    });

};

RenderTablePriceBookPrices = function () {
    $.LoadingOverlay("show");
    var ajaxUri = '/TraderPriceBooks/RenderTablePriceBookPricesByInstance?instanceId=' + $instanceId + '&groupId=' + $groupId;
    $('#table-price-book-prices-' + $priceBookInfoId).empty();
    $('#table-price-book-prices-' + $priceBookInfoId).load(ajaxUri, function () {
        LoadingOverlayEnd();
        $("#app-trader-pricebook-modifiers").modal('hide');
    });
};

RecalculatePrices = function (infoId, groupId, instanceId, type) {
    $.LoadingOverlay("show");
    $groupId = groupId;
    $instanceId = instanceId;
    $priceBookInfoId = infoId;
    var ajaxUri = '/TraderPriceBooks/RecalculatePrices?instanceId=' + $instanceId + '&groupId=' + $groupId + '&type=' + type;
    $('#table-price-book-prices-' + $priceBookInfoId).empty();
    $('#table-price-book-prices-' + $priceBookInfoId).load(ajaxUri, function () {
        LoadingOverlayEnd();
    });
};

EditVersonNameKeyUp = function (versionName) {
    var verNameNew = $("#input-version-name-edit").val();
    if (verNameNew === versionName)
        $isEditVersionName = false;
    else
        $isEditVersionName = true;
};


SavePriceBookPrices = function (status) {
    //get all prices in all table group... missing
    var groupIds = $("#group-ids").val().split(";");
    if (groupIds.length === 0) {
        cleanBookNotification.warning(_L("ERROR_MSG_285"), "Qbicles");
        return;
    }
    var prices = [];
    groupIds.forEach(function (groupId) {
        var itemPrices = $('#table-info-' + groupId + ' tbody tr');
        for (var j = 0; j < itemPrices.length; j++) {
            var pId = $($(itemPrices[j]).find('td input.price-book-price-id')).val();
            if (pId) {
                var pr = {
                    Id: $($(itemPrices[j]).find('td input.price-book-price-id')).val(),

                    MarkUp: $($(itemPrices[j]).find('td input.markup-value')).val(),
                    IsMarkupPercentage: $($(itemPrices[j]).find('td span.markup-percentage'))[0].innerText.trim() === '%' ? true : false,

                    Discount: $($(itemPrices[j]).find('td input.discount-value')).val(),
                    IsDiscountPercentage: $($(itemPrices[j]).find('td span.discount-percentage'))[0].innerText.trim() === '%' ? true : false,

                    Price: $($(itemPrices[j]).find('td input.price-value')).val()
                };
                prices.push(pr);
            }
        }

    });
    if (groupIds.length === 0) {
        cleanBookNotification.warning("Not existed Product item", "Qbicles");
        return;
    }
    var ajaxUri = "";
    if ($isEditVersionName)
        ajaxUri = '/TraderPriceBooks/SavePriceBookPrices?status=' + status + "&versionName ='" + $("#input-version-name-edit").val() + "'";
    else
        ajaxUri = '/TraderPriceBooks/SavePriceBookPrices?status=' + status + '&versionName =' + "";
    $.ajax({
        type: 'post',
        url: ajaxUri,
        data: { prices: prices },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $isEditVersionName = false;
                cleanBookNotification.updateSuccess();
                if (status === "Apply")
                    EditVersionManagement();

            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        ClearError();
    });

};


ShowHistoryModal = function (versionId) {
    $.LoadingOverlay("show");
    var ajaxUri = '/TraderPriceBooks/ShowHistoryModal?versionId=' + versionId;
    $('#app-trader-pricebook-history-table').empty();
    $('#app-trader-pricebook-history-table').load(ajaxUri, function () {

        LoadingOverlayEnd();
    });
};


DiscountTableChange = function(text, pbPriceId) {
    $('#info-discount-item-' + pbPriceId).html(text);
    ReCalculatePriceRow(pbPriceId);
};
MarkUpTableChange = function (text, pbPriceId) {
    $('#info-markup-item-' + pbPriceId).html(text);
    ReCalculatePriceRow(pbPriceId);
};

ReCalculatePriceRow = function (pbPriceId, isInclusiveTax) {
    $.LoadingOverlay("show");
    var isMarkupPercentage = false;
    var isDiscountPercentage = false;
    if ($("#info-markup-item-" + pbPriceId).text().includes("%"))
        isMarkupPercentage = true;
    if ($("#info-discount-item-" + pbPriceId).text().includes("%"))
        isDiscountPercentage = true;

    var priceBookPrice = {
        id: pbPriceId,
        MarkUp: $("#markup-value-" + pbPriceId).val(),
        Discount: $("#discount-value-" + pbPriceId).val(),
        Price: $("#price-value-" + pbPriceId).val(),
        IsMarkupPercentage: isMarkupPercentage,
        IsDiscountPercentage: isDiscountPercentage,
        FullPrice: isInclusiveTax ? $("#full-price-" + pbPriceId).val() : 0,
    };
    $.ajax({
        type: 'post',
        url: '/TraderPriceBooks/ReCalculatePriceRow',
        data: { priceBookPrice: priceBookPrice },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {

                cleanBookNotification.updateSuccess();
                $("#calculated-price-" + pbPriceId).html(response.Object.CalculatedPrice);
                $("#full-price-" + pbPriceId).val(toCurrencyDecimalPlace(response.Object.FullPrice));
                $("#price-tax-" + pbPriceId).html(response.Object.TaxValue);
                if(isInclusiveTax)
                    $("#price-value-" + pbPriceId).val(toCurrencyDecimalPlace(response.Object.Price));
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};



ApplyPriceBookPrices = function (instanceId) {
    $.LoadingOverlay("show");
    //get all prices in all table group... missing
    var groupIds = $("#group-ids").val().split(";");
    if (groupIds.length === 0) {
        cleanBookNotification.warning(_L("ERROR_MSG_285"), "Qbicles");
        return;
    }
    var versionName = "";
    if ($isEditVersionName)
        versionName = $("#input-version-name-edit").val() + "'";

    $.ajax({
        type: 'post',
        url: '/TraderPriceBooks/ApplyPriceBookPrices',
        data: {
            instanceId: instanceId ,
            versionName: versionName
        },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                $isEditVersionName = false;
                cleanBookNotification.updateSuccess();
               
                    EditVersionManagement();

            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_380", [response.msg]), "Qbicles");
                return;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        ClearError();
        LoadingOverlayEnd();
    });

};