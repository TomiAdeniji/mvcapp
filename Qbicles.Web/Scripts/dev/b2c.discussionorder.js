var CountPost = 1; busycomment = false;
var currencySetting = {
    CurrencySymbol: 0,
    DecimalPlace: 0
};
var $cartbusy = false;
$(document).ready(function () {
    loadCurrencySettings();
    $(".select2modal").select2();
    $('.toggle-switch').bootstrapToggle();

    initFormSaveAddress();

    $('#user-address-add').on('hidden.bs.modal', function () {
        var val = $('#slDeliveryAddress').val();
        if (val == 0) {
            $('#slDeliveryAddress').val('').trigger('change');
        }
    });

    $('#orer-item-search-text').keyup(delay(function () {
        initOrderItemsShow();
    }, 500));
    $('#orer-item-search-categories').change(function () {
        initOrderItemsShow();
    });

    $('#order-search-key').keyup(delay(function () {
        //initOrderItemsTable();
        $('#order-list').DataTable().ajax.reload();
    }, 500));
    $('#order-search-categories').change(function () {
        //initOrderItemsTable(); 
        $('#order-list').DataTable().ajax.reload();
    });
    $("#b2c-order-tab-menu li").click(function () {
        $("#tab4").removeAttr("style");
    });
});
initCustomerDiscussionPage = function () {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    initOrderItemsShow();
    showOrderCartB2C();
};

initBusinessDiscussionPage = function () {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });

    $(document).ready(function () {
        initOrderItemsTable();
    });

    initOrderItemsShow();
}

initOrderItemsTable = function () {
    if (!currencySetting) {
        loadCurrencySettings();
    }
    $("#order-list").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#order-list').LoadingOverlay("show");
        } else {
            $('#order-list').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[5, 10, 20, 50, 100], [5, 10, 20, 50, 100]],
            "pageLength": 5,
            "ajax": {
                "url": '/B2C/LoadB2COrderItems',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "discussionKey": $("#discussionKey").val(),
                        "searchKey": $("#order-search-key").val()
                        /*"catIds": $("#order-search-categories").val(),*/
                    });
                }
            },
            "columns": [
                {
                    data: "ItemName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        if (row.SourceName != "") {
                            htmlString += row.ItemName;
                            htmlString += " <i class='fa fa-info-circle blue' data-tooltip='Source item: " + row.SourceName + "'></i>";

                            //variant
                            htmlString += '<div class="row cartitemxtra" style="margin-top: 10px; padding: 0;">';

                            if (row.CatItemHasMultipleVariants) {
                                htmlString += '<div class="col-xs-6">';
                                htmlString += '<h6>' + row.Variant.Name + '</h6>';
                                htmlString += '</div>';
                            } else {
                                htmlString += '<div class="col-xs-6">';
                                htmlString += '<h6></h6>';
                                htmlString += '</div>';
                            }

                            htmlString += '<div class="col-xs-6 text-right">';
                            htmlString += '<h6>' + currencySetting.CurrencySymbol + ' ' + row.Variant.AmountInclTax.toFixed(currencySetting.DecimalPlace) + '</h6></div>';
                            htmlString += '</div>';
                            htmlString += '</div>';

                            if (row.Extras != null) {
                                for (var i = 0; i < row.Extras.length; i++) {
                                    htmlString += '<div class="row cartitemxtra" style="margin-top: 10px; padding: 0;">';
                                    htmlString += '<div class="col-xs-6">';
                                    htmlString += '<h6> \+ ' + row.Extras[i].Name + '</h6>';
                                    htmlString += '</div>';
                                    htmlString += '<div class="col-xs-6 text-right">';
                                    htmlString += '<h6>' + currencySetting.CurrencySymbol + ' ' + row.Extras[i].AmountInclTax.toFixed(currencySetting.DecimalPlace) + '</h6></div>';
                                    htmlString += '</div>';
                                    htmlString += '</div>';
                                }
                            }
                        } else {
                            htmlString += row.ItemName;
                        }
                        return htmlString;
                    }
                },
                {
                    data: "CategoryName",
                    orderable: true
                },
                {
                    data: "Quantity",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        htmlString = "\<input type=\'number\' min='1' itemId='" + row.ItemId
                            + "' " + _disalbed + "  id='quantity" + row.ItemId
                            + "' class=\'form-control quantity-item\' value=\'"
                            + row.Quantity + "\'\ onchange=\"updateOrderItemQuantity(" + row.ItemId + ");\">";
                        return htmlString;
                    }
                },
                {
                    data: "ItemInitialPrice",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = row.ItemInitialPrice.toFixed(currencySetting.DecimalPlace);
                        return htmlString;
                    }
                },
                {
                    //Discount
                    data: "Discount",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        htmlString = "\<input type=\'number\' min='0' max='100' itemId='" + row.ItemId + "' " + _disalbed + "  id='itemdiscount" + row.ItemId + "' class=\'form-control itemdiscount\' value=\'" + row.Discount + "\'\>";
                        return htmlString;
                    }
                },
                {
                    //Taxes
                    data: "TaxInfo",
                    orderable: false,
                },
                {
                    data: "PriceString",
                    orderable: false,
                    className: "price"
                },
                {
                    data: "ItemId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var itemId = row.IsAllowEdit ? row.ItemId : " ";
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        return '<button '+_disalbed+' " class="btn btn-danger" onclick="businessRemoveB2COrderItem(' + itemId + ')"><i class="fa fa-trash"></i></button>';
                    }
                }
            ],
            "drawCallback": function (settings) {
                var handleDiscountChange = delay(function (e) {
                    $("#recalculatebtn").removeAttr("disabled");
                    var $elm = $(this);
                    var $row = $elm.parents("tr");
                    var $table = $('#order-list').DataTable();
                    //var rowData = $table.row($row).data();
                    var itemId = $($row).find("input[type=number]").attr("itemId");
                    $("#taxes" + itemId).parent().addClass('taxItem' + itemId);

                    //Update Tax row data
                    var tradeOrderId = $("#tradeorderid").val();
                    var discount = $("#itemdiscount" + itemId).val();
                    discount = discount ? discount : 0;

                    if (discount < 0) {
                        cleanBookNotification.error("The discount must be greater or equal to 0.", "Qbicles");
                        discount = 0;
                        $("#itemdiscount" + itemId).val(0);
                    }
                    else if (discount > 100) {
                        cleanBookNotification.error("The discount must be less than or equal to 100.", "Qbicles");
                        discount = 0;
                        $("#itemdiscount" + itemId).val(0);
                    }

                    var _lstTaxes = $("#taxes" + itemId);
                    //var htmlString = "";
                    //var priceDisable = "disabled";
                    if (_lstTaxes != null) {
                        LoadingOverlay();
                        $.ajax({
                            method: 'POST',
                            dataType: 'JSON',
                            url: "/B2C/ReCalculateTax",
                            data: {
                                tradeOrderId: tradeOrderId,
                                discount: discount,
                                itemId: itemId
                            },
                            success: function (response) {
                                if (response.result) {
                                    //if (response.Object != null && response.Object.length > 0) {
                                    //    htmlString += "<ul id='taxes" + itemId + "' class='unstyled'>";
                                    //    for (var i = 0; i < response.Object.length; i++) {
                                    //        htmlString += '<li>';
                                    //        htmlString += currencySetting.CurrencySymbol + response.Object[i].AmountTax.toFixed(currencySetting.DecimalPlace);
                                    //        htmlString += "<small><i>(";
                                    //        htmlString += response.Object[i].TaxName;
                                    //        htmlString += ")</i></small></li>";
                                    //    };
                                    //    htmlString += "</ul>";
                                    //    $table.cell('.taxItem' + itemId).data(htmlString);
                                    //    //actionval > 0 has voucher applied
                                    //    if (response.actionVal == 0)
                                    //        priceDisable = "";

                                    //    //Update Price for Item
                                    //    var itemPrice = parseFloat($("#pureprice" + itemId).val());
                                    //    var newPrice = itemPrice * (1 - discount / 100);
                                    //    $(".itemprice" + itemId).parent().addClass('pricecontainer' + itemId);

                                    //    var priceString = "\<input " + priceDisable + " type=\'number\' id='itemprice" + itemId + "' class='form-control itemprice" + itemId + "' value=\'" + newPrice.toFixed(currencySetting.DecimalPlace) + "\'>";
                                    //    priceString += "<input type='hidden' value='" + $("#pureprice" + itemId).val() + "' id='pureprice" + itemId + "'>";
                                    //    $table.cell(".pricecontainer" + itemId).data(priceString);
                                    //    UpdateOrderItemInfo(itemId);
                                    //}
                                    UpdateOrderItemInfo(itemId);
                                } else {
                                    LoadingOverlayEnd()
                                    cleanBookNotification.error(response.msg, "Qbicles");
                                }
                            },
                            error: function (err) {
                                LoadingOverlayEnd()
                                cleanBookNotification(err.msg, "Qbicles");
                            }
                        });
                    } else {
                        $table.cell('.taxItem' + itemId).data("--");
                    };

                }, 1000);

                var handlePriceChange = delay(function (e) {
                    $("#recalculatebtn").removeAttr("disabled");
                    var $elm = $(this);
                    var $row = $elm.parents("tr");
                    var $table = $('#order-list').DataTable();
                    //var rowData = $table.row($row).data();
                    var itemId = $($row).find("input[type=number]").attr("itemId");
                    $("#itemdiscount" + itemId).parent().addClass('discountContainer' + itemId);

                    var itemPurePrice = parseFloat($("#pureprice" + itemId).val());
                    var itemTotalPrice = parseFloat($("#totalprice" + itemId).val());
                    var newPrice = parseFloat($("#itemprice" + itemId).val());
                    var newDiscount = parseFloat(100 - (newPrice / itemTotalPrice * 100));
                    if (newDiscount < 0) {
                        cleanBookNotification.error('The updated price must not cause the discount to be less than 0.', 'Qbicles');
                        newDiscount = 0;
                        $("#itemprice" + itemId).val(itemTotalPrice);
                    } else if (newDiscount > 100) {
                        cleanBookNotification.error('The updated price must not cause the discount to be greater than 100.', 'Qbicles');
                        newDiscount = 0;
                        $("#itemprice" + itemId).val(itemTotalPrice);
                    }
                    $table.cell('.discountContainer' + itemId).data(newDiscount);
                    LoadingOverlay();
                    UpdateOrderItemInfo(itemId);
                }, 1000);

                $(".itemdiscount").on("change", handleDiscountChange);
                $(".price").on("change", handlePriceChange);
            }
        });
}

function loadCurrencySettings() {
    var domainId = $("#domainid").val();
    var currencyUrl = "/Qbicles/GetCurrencySettings"
    if (domainId != null) {
        currencyUrl = "/Qbicles/GetCurrencySettingsByDomain?domainId=" + domainId;
    }
    $.ajax({
        url: currencyUrl,
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

function initOrderItemsShow() {
    var $data_container_items = $('#items-container');
    var $pagination_container = $('#pagiation-items');
    $pagination_container.pagination({
        dataSource: '/B2C/SearchB2COrderItems',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_items.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 6,
        ajax: {
            data: {
                scatids: JSON.stringify($('#orer-item-search-categories').val()),
                keyword: $('#orer-item-search-text').val(),
                bdomainKey: $("#domain-key").val()
            },
            beforeSend: function () {
                $data_container_items.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var extraCol = (count % 3 == 0 ? 0 : 3) - count % 3;
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += itemTemplate(item);
            });
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<div class="col">&nbsp;</div>';
            }
            $data_container_items.html(dataHtml);
        }
    })
}

function itemTemplate(data) {
    var $domainkey = $('#domain-key').val();
    var _html = '<div class="col"><a href="#product-more-menu" data-toggle="modal" onclick="showItemContent(' + data.Id + ',\'' + $domainkey + '\')">';
    _html += '<div class="productimg" style="background-image: url(\'' + data.ImageUri + '\');"></div >';
    _html += '<div class="priceblock">';
    _html += '<p>' + data.Name + '</p>';
    _html += '<label class="label label-lg label-soft">' + data.CategoryName + '</label> &nbsp; <span>' + data.Price + '</span></div>';
    _html += '</a></div>';
    return _html;
}

function showOrderCartB2C(cartbusy) {
    if (cartbusy != null || cartbusy != undefined)
        $cartbusy = cartbusy;
    if ($cartbusy == false) {

        $('#cart').LoadingOverlay('show');
        $("#order-cart-tab").empty();
        $cartbusy = true;
        var disKey = $("#discussionKey").val();
        var $domainkey = $('#domain-key').val();
        var _url = "/B2C/B2COrderCartShow?disKey=" + disKey + "&domainKey=" + $domainkey;
        $("#order-cart-tab").load(_url, function () {
            //setTimeout(function () { $('#cart').LoadingOverlay('hide', true); }, 2000);
            $('#cart').LoadingOverlay('hide', true);
            $cartbusy = false;
            OrderProcessedCheck();

            //$("#tab4").removeAttr("style");

            //$('#cart-li-tab a').trigger('click');

            //$('#cart-li-tab-hidden').click();

            //for (var li of document.querySelectorAll("li.active")) {
            //    li.classList.remove("active");
            //}

            //$('li a').removeClass("active");
            //$(this).addClass("active");

        });
    }
}

function showItemContent(itemId, domainkey) {
    var oId = $("#tradeorderid").val();
    var _url = "/B2C/B2COrderItemContentShow?itemId=" + itemId + "&domainKey=" + domainkey + "&orderId=" + oId;
    LoadingOverlay();
    $("#item-content-modal").empty();
    $("#item-content-modal").load(_url);
    $("#item-content-modal").modal("show");
    LoadingOverlayEnd();
}



function AddItemToB2COrder(posItemId) {
    $('#add-new-item-to-order-modal').LoadingOverlay('show');

    var b2cOrderId = $("#tradeorderid").val();
    var isAddedByCustomer = $("#isCustomerDiscussion").val();

    var b2cOrder = {
        Id: b2cOrderId
    };

    //selected variant
    var variant = {
        Id: $posVariantSelected.Id,
        Price: _.toNumber($posVariantSelected.Price),
        ImageUri: _.isEmpty($posVariantSelected.ImageUri) ? $("#variant-image").val() : $posVariantSelected.ImageUri
    }

    //selected extra
    var listExtras = [];
    var extraItems = $('#item-extras div label div input');
    _.forEach(extraItems, function (input) {
        if ($('#' + input.id).prop('checked')) {
            var extraItemId = $('#' + input.id).attr("extra-id");
            var extraItemInclTaxAmount = parseFloat($('#' + input.id).val());
            var selectedExtraItem = {
                Id: extraItemId,
                Price: extraItemInclTaxAmount
            }
            listExtras.push(selectedExtraItem);
        }
    });

    //quantity
    var quantity = parseInt($("#item-quantity").val());
    var includedTaxAmount = parseFloat($("#order-total-prices").attr("value"));
    // 

    $.ajax({
        type: 'POST',
        //contentType: 'application/JSON; charset=utf-8',
        url: '/B2C/AddItemToB2COrder',
        data: {
            disKey: $("#discussionKey").val(),
            b2cOrder: b2cOrder,
            variant: variant,
            extras: listExtras,
            quantity: quantity,
            includedTaxAmount: includedTaxAmount,
            posItemId: posItemId,
            voucherId: $('#voucher-seleced-id').val()
        },
        datatype: 'JSON',
        success: function (response) {
            if (response.result == false) {
                cleanBookNotification.error(response.msg, "Qbicles");
                return;
            }
            if (response.result == true) {
                cleanBookNotification.success('Add item to B2C Order successfully', "Qbicles");
            }

            $("#item-content-modal").modal("hide");
            if (isAddedByCustomer == "true") {
                //increase highlighted above cart icon
                var itemNumber = Number($("#item-count").text());
                $("#item-count").text(itemNumber + 1);
            } else {
                reloadDataTable();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('#add-new-item-to-order-modal').LoadingOverlay('hide', true);
    });
}

function removeItemFromB2COrder(itemId) {
    $('#cart').LoadingOverlay('show');
    var disKey = $("#discussionKey").val();

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: "/B2C/RemoveItemFromB2COrder?disKey=" + disKey + "&itemId=" + itemId,
        success: function (response) {
            showOrderCartB2C();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        $('#cart').LoadingOverlay('hide', true);
    })
}


function businessRemoveB2COrderItem(itemId) {
    if(confirm("Do you want to completely remove this item from the cart?")){
        $('#cart').LoadingOverlay('show');
        var disKey = $("#discussionKey").val();
        $('#order-list').LoadingOverlay('show', true);
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: "/B2C/RemoveItemFromB2COrder?disKey=" + disKey + "&itemId=" + itemId,
            success: function (response) {
                reloadDataTable();
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        }).always(function () {
            $('#order-list').LoadingOverlay('hide', true);
        })
    }
    
}

function customerAcceptOrder(ev) {
    $('#frmCustomerDelivery').LoadingOverlay('show');
    var disKey = $("#discussionKey").val();
    //LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        data: $('#frmCustomerDelivery').serialize(),
        url: "/B2C/setB2COrderAcceptedByCustomer?disKey=" + disKey,
        success: function (response) {
            //LoadingOverlayEnd();

            if (response.result) {
                cleanBookNotification.updateSuccess();
                $(ev).css("display", "none");
                $("#processing").css("display", "");
                //UpdateB2COrderStatus(0);
                $('#customer-order-processing-modal').modal('show');


            } else if (!response.result && response.msg) {
                cleanBookNotification.error(_L(response.msg), "Qbicles");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");

        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            $('#btnAgreeTerms').removeAttr('disabled');
        }
    }).always(function () {
        $('#frmCustomerDelivery').LoadingOverlay('hide', true);
    });
}


function LoadMorePostsDiscussionOrder(activityKey, pageSize, divId) {
    $('#' + divId).LoadingOverlay("show");
    $('#btnLoadPosts').hide();

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: CountPost * pageSize,
            isDiscussionOrder: true
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
            } else {
                $('#btnLoadPosts').show();
            }

            $('#' + divId).append(response).fadeIn(250);
            CountPost = CountPost + 1;

            var element = $(".mdv2-col-user");
            element.animate({
                scrollTop: element.prop("scrollHeight")
            }, 500);

        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    }).always(function () {
        $('#' + divId).LoadingOverlay("hide", true);
    });

}

function UpdateOrderItemInfo(itemId) {
    var tradeOrderId = $("#tradeorderid").val();
    var _item = {
        Id: itemId,
        Variant: {
            Discount: parseFloat($("#itemdiscount" + itemId).val()),
        }
    }

    //LoadingOverlay();

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: "/B2C/UpdateItemInfor",
        data: JSON.stringify({
            disKey: $("#discussionKey").val(),
            tradeOrderId: tradeOrderId,
            updatedItem: _item
        }),
        contentType: 'application/json',
        success: function (response) {
            if (response.result) {
                reloadDataTable();
                $("#totalpricestr").text(currencySetting.CurrencySymbol + Number(response.Object).toFixed(currencySetting.DecimalPlace));

            } else {
                cleanBookNotification.error(response.msg);
            }

        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function B2COrderProcessingModalShow() {
    var disKey = $("#discussionKey").val();

    if ($("#used-default-settings").val() == 'false') {
        ShowWorkgroupsModal(disKey);
    }
    else {
        $('#business-order-manage').LoadingOverlay('show');
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: "/B2C/ProcessB2COrderUseDefaultWorkgroupSettings",
            data: {
                tradeOrderId: $("#tradeorderid").val(),
                disKey: disKey
            },
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    $(".submit-options").attr("style", "display: none");
                    $(".submit-note").show();
                    //$(".payment-tab").show();
                    OrderProcessedCheck();
                    $(".datatable input[type=number]").prop('disabled', 'true');
                } else {
                    ShowWorkgroupsModal(disKey);
                    cleanBookNotification.warning("Missing default workgroup settings", "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        }).always(function () {
            $('#business-order-manage').LoadingOverlay('hide', true);
        });
    }
}

function ShowWorkgroupsModal(disKey) {
    var _url = "/B2C/OrderProcessingModal?disKey=" + disKey;
    LoadingOverlay();
    $("#order-processing-modal").empty();
    $("#order-processing-modal").load(_url);
    $("#order-processing-modal").modal("show");
    LoadingOverlayEnd();
}


function initOrderProcessingForm() {
    var $frmorderprocessing = $("#orderprocessing-frm");

    $frmorderprocessing.validate({
        rule: {
            paymentacc: {
                required: true
            },
            salewg: {
                required: true
            },
            invoicewg: {
                required: true
            },
            paymentwg: {
                required: true
            },
            transferwg: {
                required: true
            }
        }
    });

    $frmorderprocessing.submit(function (e) {
        e.preventDefault();

        if ($frmorderprocessing.valid()) {
            $("#order-processing-modal").modal("hide");
            $('#business-order-manage').LoadingOverlay('show');
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: "/B2C/ProcessB2COrder",
                data: {
                    tradeOrderId: $("#tradeorderid").val(),
                    PaymentAccId: $("#paymentacc").val(),
                    disKey: $("#discussionKey").val(),
                    SaleWGId: $("#salewg").val(),
                    InvoiceWGId: $("#invoicewg").val(),
                    PaymentWGId: $("#paymentwg").val(),
                    TransferWGId: $("#transferwg").val()
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        $(".submit-options").attr("style", "display: none");
                        $(".submit-note").show();

                        //$(".payment-tab").show();
                        $("#b2c-order-status").text("Processing");
                        $("#b2c-order-status").removeClass().addClass('label label-lg label-info');

                        $("#add-invoice-button").removeAttr("disabled");
                        $(".datatable input[type=number]").prop('disabled', 'true');
                        OrderProcessedCheck();
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                $('#business-order-manage').LoadingOverlay('hide', true);
            });
        }
    })
}

function checkForSubmitAbility() {
    var paymentAcc = $("#paymentacc").val();
    var saleWG = $("#salewg").val();
    var invoiceWG = $("#invoicewg").val();
    var paymentWG = $("#paymentwg").val();
    var transferWG = $("#transferwg").val();

    if (paymentAcc != "" && saleWG != "" && invoiceWG != "" && paymentWG != "" && transferWG != "") {
        $("#orderprocessing-frm").find("button[type=submit]").removeAttr("disabled");
    } else {
        $("#orderprocessing-frm").find("button[type=submit]").attr("disabled", "disabled");
    }
}


var delayTimer;
function updateOrderItemQuantity(itemId) {
    $('#cart').LoadingOverlay('show');
    $('#order-list').LoadingOverlay('show');
    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        var tradeOrder = {
            Id: $("#tradeorderid").val()
        };

        var newItemQuantity = $("#quantity" + itemId).val();

        if (newItemQuantity <= 0) {
            cleanBookNotification
                .warning('The quantity of the item must be greater than 0. The wrong quantity number will be reset to 1');
        }

        var _url = "/B2C/ChangeOrderItemQuantity";
        //LoadingOverlay();
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            data: {
                disKey: $("#discussionKey").val(),
                b2cOrder: tradeOrder,
                newQuantity: newItemQuantity,
                itemId: itemId,
            },
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    showOrderCartB2C();
                    reloadDataTable();
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
                //setTimeout(function () { LoadingOverlayEnd(); }, 500);
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
                //LoadingOverlayEnd();
            }
        }).always(function () {
            $('#cart').LoadingOverlay('hide', true);
            $('#order-list').LoadingOverlay('hide', true);
        });
    }, 1000);
}

function reloadDataTable() {

    if (!currencySetting)
        loadCurrencySettings();
    reloadStatusSubmit();

    $("#voucher-applied-info").hide();
    $("#voucher-applied-name").html("");
    $("#voucher-applied-code").html("");
    $("#voucher-seleced-id").val(0);
    $("#order-list").DataTable().ajax.reload(function (json) {
        if (json.data[0] != null) {
            $('#totalpricestr').text(currencySetting.CurrencySymbol + json.data[0].TotalPrice.toFixed(currencySetting.DecimalPlace));
            if (parseFloat(json.data[0].VoucherId) > 0) {
                $("#voucher-applied-info").show();
                $("#voucher-applied-name").html(json.data[0].VoucherName);
                $("#voucher-applied-code").html(json.data[0].VoucherCode);
                $("#voucher-seleced-id").val(parseFloat(json.data[0].VoucherId));
            }
        } else {
            $('#totalpricestr').text(currencySetting.CurrencySymbol + Number(0).toFixed(currencySetting.DecimalPlace));
        }
    });
}
function reloadStatusSubmit() {

    var oId = $("#tradeorderid").val();
    $.get("/B2C/GetIsAgreedByCustomer?oid=" + oId, function (response) {
        $("#proceed").prop("disabled", !response.isAgreedByCustomer);

        if (response.isAgreedByBoth) {
            $(".submit-options").attr("style", "display: none");
        }
        if (response.customerInfo) {
            $('.customer-info').html(response.customerInfo);
        }
        console.log('--------------reloadStatusSubmit-dev------------------');
        console.log(response.orderStatus);
        console.log('--------------End reloadStatusSubmit-dev------------------');

        $("#b2c-order-status").text(response.orderStatus);
        $("#b2c-order-status").removeClass().addClass('label label-lg label-' + response.orderStatusCss);
    });
}
function activeInteraction(disId, elm) {
    $.post("/B2C/ActiveInteraction", { disId: disId }, function (response) {
        if (response.result) {
            $('#store').addClass('mdlock');
            $('#talktoagent').show();
            $(elm).parent().hide();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function managedelivery(elm) {
    var value = $(elm).val();
    if (value == 1)//Deliver them to someone else
    {
        $('.address-them').show();
        $('.methods').show();
        $('.note-desc').text('Delivery note (optional)');
    }
    else if (value == 2)//I will pick them up
    {
        $('.address-them').hide();
        $('.methods').hide();
        $('.note-desc').text('Note: (enter notes for pickup)');
    } else if (value == 0)//Deliver them to me
    {
        $('.address-them').hide();
        $('.methods').show();
        $('.note-desc').text('Delivery note (optional)');
    }
    $('.note-box').show();
    validAgreeTerms();
}
function addAppendOptionAddress(data) {
    var $address = $('select[name=delivery]');
    if ($address.find("option[value='" + data.Id + "']").length) {
        $address.val(data.Id).trigger('change');
    } else {
        // Create a DOM Option and pre-select by default
        var newOption = new Option(data.Address, data.Id, true, true);
        // Append it to the select
        $('.add-new-address').before(newOption);
        $address.val(data.Id).trigger('change');
    }
}
function validAgreeTerms() {
    var $method = $('#receive-method-selector');
    var valmethod = $method.val();
    var valaddress = $('select[name=delivery]').val();
    var valSomeoneName = $('input[name=someoneName]').val();
    var countItems = $('li.basket-item').length;
    if (countItems > 0 && valmethod == 1 && valSomeoneName && valaddress) {
        $('#btnAgreeTerms').removeAttr('disabled');
    } else if (countItems > 0 && valmethod == 0 && valaddress) {
        $('#btnAgreeTerms').removeAttr('disabled');
    } else if (countItems > 0 && valmethod == 2)
        $('#btnAgreeTerms').removeAttr('disabled');
    else
        $('#btnAgreeTerms').attr('disabled', true);
}
function initFormSaveAddress() {
    var $frmuseraddressadd = $("#frmuseraddressadd");
    $frmuseraddressadd.submit(function (e) {
        e.preventDefault();
        if ($cartbusy)
            return;
        if ($frmuseraddressadd.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    $cartbusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        if (data.Object)
                            addAppendOptionAddress(data.Object);
                        $('#user-address-add').modal('hide');
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Community");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Community");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
                    }
                    $cartbusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    $cartbusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
                    LoadingOverlayEnd();
                }
            });
        }
    });

}

function MethodChange() {
    var method = $("#pay-method").val();
    $("#pay-reference").val('');
    if (method == "Card" || method == "Electronic Transfer")
        $('#pay-card').show();
    else
        $('#pay-card').hide();
}

function ValidPaymentAmount() {
    var paymentTotal = parseFloat($("#payment-total").val());
    var saleTotal = parseFloat($("#b2c-payment-total").val());
    var paymentAmount = parseFloat($("#pay-amount").val());

    var acceptAmount = saleTotal - paymentTotal;

    if (saleTotal < paymentTotal + paymentAmount) {
        cleanBookNotification.warning("Amount to pay must be less than " + acceptAmount, "Qbicles");
        $("#pay-amount").val(acceptAmount);
        $("#confirm-payment").addClass('disabled');
        $("#confirm-payment").attr('disabled');
        return false;
    }
    else if (acceptAmount === 0) {
        $("#confirm-payment").addClass('disabled');
        $("#confirm-payment").attr('disabled');
        cleanBookNotification.warning("Paid out, can't create a new one", "Qbicles");
        return false;
    }
    else {
        $("#confirm-payment").removeAttr('disabled');
        $("#confirm-payment").removeClass('disabled');
        return true;
    }
}


function ShowPaystackPaymentPopup(orderKey) {
    if (!ValidPaymentAmount())
        return;

    $.ajax({
        type: 'post',
        url: '/B2C/GetB2COrerPaymentPaystackPopupData',
        data: {
            tradeOrderKey: orderKey
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                var data = response.Object;
                $("#pay-amount").val(data.amount);
                if (Number(data.chargeAmount) > 0) {
                    let handler = PaystackPop.setup({
                        key: data.publicKey, // Replace with your public key
                        email: data.userEmail,
                        amount: data.amount * 100,
                        subaccount: data.subAccCode,
                        transaction_charge: data.chargeAmount,
                        bearer: 'subaccount',
                        onClose: function () {
                            // pass
                        },
                        callback: function (response) {
                            CreateB2CPayment(orderKey, false, true);
                        }
                    });

                    handler.openIframe();
                } else {
                    let handler = PaystackPop.setup({
                        key: data.publicKey, // Replace with your public key
                        email: data.userEmail,
                        bearer: 'subaccount',
                        amount: data.amount * 100,
                        subaccount: data.subAccCode,
                        onClose: function () {
                            // pass
                        },
                        callback: function (response) {
                            CreateB2CPayment(orderKey, true, true);
                        }
                    });

                    handler.openIframe();
                }
            }
            else
                cleanBookNotification.error(response.msg, "Qbicles");

        },
        error: function (er) {
            cleanBookNotification.error(er, "Qbicles");
        }
    })
}


function CreateB2CPayment(orderKey, isPaystackCallback, isCustomer) {
    if (isPaystackCallback == false) {
        if (!ValidPaymentAmount())
            return;
    }
    LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/B2C/CreateB2CPayment',
        data: {
            tradeOrderId: orderKey,
            isCutomer: isCustomer,
            /*invoiceId: $("#b2c-order-invoice-id").val(),*/
            payment: {
                Amount: $("#pay-amount").val(),
                Reference: $("#pay-reference").val(),
                PaymentMethod: {
                    Name: $("#pay-method").val()
                }
            }
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                ReloadPaymentTab();

                $("#cart-li-tab").removeClass();
                $("#order-cart-tab").removeClass().addClass('tab-pane fade');
                $('.payment-tab a').trigger('click');
                reloadStatusSubmit();
                var paymentTotal = parseFloat($("#payment-total").val());
                var paymentAmount = parseFloat($("#pay-amount").val());
                $("#payment-total").val(paymentTotal + paymentAmount);
                $('.search_dt_payment').show();
                cleanBookNotification.createSuccess();
            }
            else
                cleanBookNotification.error(response.msg, "Qbicles");

        },
        error: function (er) {
            cleanBookNotification.error(er, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

}


function SelectedVoucher(voucherId, name, code) {
    $('#choosevoucher').hide();
    $('#voucher-name').html(name);
    $('#voucher-code').html(code);
    $('#voucher-seleced-id').val(voucherId);
    $('#voucher').show();
    ApplyVoucherOrderB2C(voucherId);

}

function RemoveVoucher() {
    $('#voucher').hide();
    $('#choosevoucher').show();
    $('#voucher-name').html('');
    $('#voucher-code').html('');
    $('#voucher-seleced-id').val(0);
    ApplyVoucherOrderB2C(0);
}

function ApplyVoucherOrderB2C(voucherId) {
    $('#cart').LoadingOverlay('show');
    $.ajax({
        type: 'post',
        url: '/B2C/ApplyVoucherOrderB2C',
        data: {
            discussionKey: $("#discussionKey").val(),
            tradeOrderId: $("#tradeorder-key").val(),
            voucherId: voucherId
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                showOrderCartB2C();
            }
            else
                cleanBookNotification.error(response.msg, "Qbicles");

        },
        error: function (er) {
            cleanBookNotification.error(er, "Qbicles");
        }
    }).always(function () {
        $('#cart').LoadingOverlay('hide', true);
    });
}

function AddInvoicePayment() {
    $("#pay-amount").val(0);
    $("#pay-amount").removeClass('disabled');
    $('#pay-method').val("Cash").trigger('change');
    $("#confirm-payment").removeAttr('disabled');
    $("#confirm-payment").removeClass('disabled');
    $('#b2c-payment-add').modal('toggle');
}

var refreshIntervalId = 0;
function OrderProcessedCheck() {

    $.ajax({
        type: 'post',
        url: '/B2C/OrderProcessedCheck',
        data: { disKey: $("#discussionKey").val() },
        dataType: 'json',
        success: function (response) {
            if (!response.Processed) {
                return;
            }

            $(".payment-total-text").text(response.InvoiceTotalTxt);
            $("#payment-remain-amount").val(response.PaymentRemain);
            $("#pay-now-text").text(response.PaymentRemainTxt);

            $("#b2c-payment-total").val(response.SaleTotal);

            $("#b2c-order-invoice-id").val(response.InvoiceKey);
            $("#add-invoice-button").attr("disabled", false);
            $(".pay-tab-header").remove();
            $(".search_dt_payment").show();

            //ReloadPaymentTab();
            $("#tab4").removeAttr("style");
        },
        error: function (er) {
            cleanBookNotification.error(er, "Qbicles");
        }
    }).always(function () {

    });

}

/**
 * UpdateB2COrderStatus  (status) - Completed when payment is full
    Awaiting Processing: 0,
    In Processing: 1,
    InProcessing: 2, Processed: 3, (Invoice created - Ready for payment)
    order completed : 4 - payment full
 * */
function UpdateB2COrderStatus(status) {
    var disId = $("#discussionId").val();
    var statusText = "";
    var statusCss = "";
    if (status == 0) {
        statusText = "Awaiting processing";
        statusCss = "label label-lg label-primary";
    }
    if (status == 1) {

        statusText = "Processing";
        statusCss = "label label-lg label-info";

        //remove order tab from Customer
        //$("#order-li-tab").remove();
        //$("#tab0").remove();
        //$("#tab0").removeClass().addClass('hidden');
        //$("#order-li-tab").removeClass().addClass('hidden');
        //$("#order-a-tab").removeClass().addClass('hidden');

        //$("#cart-li-tab").addClass('active');
        //$("#order-cart-tab").addClass('in active');
        //$('#cart-li-tab-hidden').click();
        $('#cart-li-tab a').trigger('click');

    } else if (status == 2 || status == 3) {
        statusText = "Ready for payment";
        statusCss = "label label-lg label-warning";
        showOrderCartB2C();
    }
    else if (status == 4) {
        statusText = "Complete";
        statusCss = "label label-lg label-success";
        showOrderCartB2C();
    }

    $("#b2c-order-status").text(statusText);
    $("#b2c-order-status").removeClass().addClass(statusCss);

    $("#order-context-flyout-status-" + disId).text(statusText);
    $("#order-context-flyout-status-" + disId).removeClass().addClass(statusCss);
}

function DisplayPaymentTabToCustomer() {
    $(".payment-tab").show();
    //$(".payment-tab-hiden").hide();
}

function ReloadPaymentTab() {
    var ajaxUri = '/TraderPayments/InvoicePaymentContent?key=' + $("#b2c-order-invoice-id").val();
    $('#tab4').empty();
    $('#tab4').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $(".payment-tab").show();
    });
}
