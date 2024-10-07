var CountPost = 1;
var $orderUI = {
    $discussionKey: $("#discussionKey"),
    $hdfOrderKey: $("#hdfOrderKey"),
    $providerDomainKey: $("#providerDomainKey"),
    $choosewg: $('#choose-wg'),
    $slDeliveryTo: $('#slDeliveryTo'),
    $amountBuyer: $('input[name=AmountBuyer]')
};

function initConsumerDiscussionPage() {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#orer-item-search-text').keyup(delay(function () {
        initOrderItemsShow(true);
    }, 500));
    $('#orer-item-search-categories').change(function () {
        initOrderItemsShow(true);
    });
    initOrderItemsShow(true);
    initBuyOrderItemsTable();
    $('input[name=AmountSeller],input[name=AmountBuyer]').change(delay(function () {
        updateExchangeRate();
    }, 1000));
};
function initProviderDiscussionPage() {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#orer-item-search-text').keyup(delay(function () {
        initOrderItemsShow(false);
    }, 500));
    $('#orer-item-search-categories').change(function () {
        initOrderItemsShow(false);
    });
    initSellOrderItemsTable();
    initOrderItemsShow(false);
}
function loadProductMoreContent(itemId) {
    var oId = $orderUI.$hdfOrderKey.val();
    var _url = "/Commerce/LoadProductMoreContent?itemId=" + itemId + "&orderKey=" + oId;
    LoadingOverlay();
    $("#product-more-b2border").empty();
    $("#product-more-b2border").modal("show");
    $("#product-more-b2border").load(_url, function () {
        //var _sku = $('#item-variant option:selected').data('sku');
        var $select_traderitem = $('#item-associated').select2({
            placeholder: "Please select",
            ajax: {
                url: '/Commerce/GetListTraderItem',
                delay: 250,
                data: function (params) {
                    var query = {
                        search: params.term
                    }
                    return query;
                },
                cache: true,
                processResults: function (data) {
                    return {
                        results: data.Object
                    };
                }
            },
            minimumInputLength: 1
        });
        var $unit = $('#unit-associated');
        $unit.select2({ placeholder: "Please select"});
        $select_traderitem.on("select2:select", function (e) {
           
            var data = e.params.data;
            $unit.empty();
            if (data.units.length == 0)
                $unit.attr("disabled", true);
            else
                $unit.attr("disabled", false);
            $unit.select2({
                placeholder: "Please select",
                data: (data.units ? data.units : [])
            });

            b2bMatchTaxes();
        });
        $('#primary-vendor').bootstrapToggle();
        showAmountConverted('input[name=showpriceconverted]');
       
        LoadingOverlayEnd();
    });
    
}

function loadProductMoreContentInProvider(itemId) {
    var oId = $orderUI.$hdfOrderKey.val();
    var _url = "/Commerce/LoadProductMoreContent?itemId=" + itemId + "&orderKey=" + oId;
    LoadingOverlay();
    $("#b2b-seller-add-item").empty();
    $("#b2b-seller-add-item").modal("show");
    $("#b2b-seller-add-item").load(_url, function () {
        LoadingOverlayEnd();
    });
}

function itemTemplateToProvider(data){
    var _html = '<div class="col"><a href="#product-more-menu" data-toggle="modal" onclick="loadProductMoreContentInProvider(' + data.Id + ')">';
    _html += '<div class="productimg" style="background-image: url(\'' + data.ImageUri + '\');"></div>';
    _html += '<div class="priceblock">';
    _html += '<p>' + data.Name + '</p>';
    _html += '<label class="label label-lg label-soft">' + data.CategoryName + '</label> &nbsp; <span>' + data.Price + '</div>';
    _html += '</a></div>';
    return _html;
}

function initOrderItemsShow(isConsumer) {
    var $data_container_items = $('#items-container');
    var $pagination_container = $('#pagiation-items');
    $pagination_container.pagination({
        dataSource: '/Commerce/SearchB2bOrderItems',
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
                bdomainKey: $("#providerDomainKey").val()
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
            if(isConsumer){
                $.each(data, function (index, item) {
                    dataHtml += itemTemplate(item);
                });
            }
            else{
                $.each(data, function (index, item) {
                    dataHtml += itemTemplateToProvider(item);
                });
            }
            
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<div class="col">&nbsp;</div>';
            }
            $data_container_items.html(dataHtml);
        }
    })
}
function itemTemplate(data) {
    var _html = '<div class="col"><a href="#product-more-menu" data-toggle="modal" onclick="loadProductMoreContent(' + data.Id + ')">';
    _html += '<div class="productimg" style="background-image: url(\'' + data.ImageUri + '\');"></div>';
    _html += '<div class="priceblock">';
    _html += '<p>' + data.Name + '</p>';
    _html += '<label class="label label-lg label-soft">' + data.CategoryName + '</label> &nbsp; <span>' + data.Price + '</span>' + calPriceByExchangeRate(data.PriceVal) + '</div>';
    _html += '</a></div>';
    return _html;
}
function addItemToB2BOrder(itemId) {
    if (!$('#frmAddItemB2bOrder').valid())
        return;
    //selected variant
    var variantId = $("#item-variant option:selected").attr("variant-id");
    var variantInclTaxAmount = parseFloat($("#item-variant option:selected").val());
    var variant = {
        Id: variantId,
        Price: variantInclTaxAmount
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

    LoadingOverlay();
    $.ajax({
        type: 'POST',
        //contentType: 'application/JSON; charset=utf-8',
        url: '/Commerce/AddItemToB2BOrder',
        data: {
            DisKey: $orderUI.$discussionKey.val(),
            OrderKey: $orderUI.$hdfOrderKey.val(),
            Variant: variant,
            Extras: listExtras,
            Quantity: quantity,
            IncludedTaxAmount: includedTaxAmount,
            ItemId: itemId,
            AssociatedItemId: $('#item-associated').val(),
            AssociatedUnitId: $('#unit-associated').val(),
            PrimaryVendor: $('#primary-vendor').prop('checked')
        },
        datatype: 'JSON',
        success: function (response) {
            $('#product-more-b2border').modal('hide');
            $("#b2b-seller-add-item").modal("hide");
            LoadingOverlayEnd();
            if (response.result) {
                reloadOrderStatus();
                cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Qbicles");
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(data.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
            
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function reloadDataTable() {
    //reloadStatusSubmit();
    $("#order-list").DataTable().ajax.reload(function (json) {
        $("#totalpricestr-buyer").empty();
        $('#totalpricestr').empty();
        if (json.data[0] != null) {
            $("#totalpricestr-buyer").append(currencySetting.BuyingCurrencySymbol + (json.data[0].TotalPrice * parseFloat($orderUI.$amountBuyer.data('exchangerateval'))).toFixed(currencySetting.DecimalPlace));
            $('#totalpricestr').append(currencySetting.CurrencySymbol + json.data[0].TotalPrice.toFixed(currencySetting.DecimalPlace));
        } else {
            $('#totalpricestr').append(currencySetting.CurrencySymbol + Number(0).toFixed(currencySetting.DecimalPlace));
            $("#totalpricestr-buyer").append(currencySetting.BuyingCurrencySymbol + Number(0).toFixed(currencySetting.DecimalPlace));
        }
    });
}
function initBuyOrderItemsTable() {
    var isConverted = $("#showPriceConverted").prop('checked');
    var thisTable = $("#order-list").on('processing.dt', function (e, settings, processing) {
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
                "url": '/Commerce/LoadB2BOrderItems',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "orderKey": $orderUI.$hdfOrderKey.val(),
                        "searchKey": $("#order-search-key").val(),
                        "catIds": $("#order-search-categories").val(),
                        "isViewBuy":true
                    });
                },
                "dataSrc": function (data) {
                    var $cartcount = $("#item-count");
                    var $totalpricestr = $("#totalpricestr");
                    var $totalpricestrconverted = $("#totalpricestrconverted");
                    $totalpricestr.empty();
                    $totalpricestrconverted.empty();
                    if (data.recordsTotal > 0) {
                        $totalpricestr.append(currencySetting.CurrencySymbol + data.data[0].TotalPrice.toFixed(currencySetting.DecimalPlace));
                        $totalpricestrconverted.append(" / "+currencySetting.BuyingCurrencySymbol + (data.data[0].TotalPrice * parseFloat($orderUI.$amountBuyer.data('exchangerateval'))).toFixed(currencySetting.DecimalPlace));
                        $cartcount.show();
                        $cartcount.text(data.recordsTotal);
                        $cartcount.removeClass("animated bounce");
                        $cartcount.addClass("animated bounce");
                        $('#cart-empty').hide();
                        $('#cart').show();
                    } else {
                        $cartcount.hide();
                        $('#cart-empty').show();
                        $('#cart').hide();
                    }
                    return data.data;
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
                            htmlString += '<div class="col-xs-6">';
                            htmlString += '<h6>' + row.Variant.Name + '</h6>';
                            htmlString += '</div>';
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
                    orderable: false
                },
                {
                    data: "Quantity",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        htmlString = "<input style=\"width: 80px;\" type=\'number\' itemId='" + row.ItemId + "' " + _disalbed + "  id='itemquantity" + row.ItemId + "' class=\'form-control itemquantity\' value=\'" + row.Quantity + "\'\ oninput=\"validity.valid||(value='0')\"; min=\"0\">";
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
                    orderable: false
                },
                {
                    //Taxes
                    data: "TaxInfo",
                    orderable: false,
                },
                {
                    data: "Price",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = row.Price.toFixed(currencySetting.DecimalPlace);
                        return htmlString;
                    }
                    //className: "price"
                },
                {
                    data: "Price",
                    orderable: false,
                    visible : isConverted,
                    render: function (data, type, row, meta) {
                        let pricebuyercurrency = row.Price * parseFloat($orderUI.$amountBuyer.data('exchangerateval'));
                        var htmlString = '<span class="red">' + pricebuyercurrency.toFixed(currencySetting.DecimalPlace);+'</span>';
                        return htmlString;
                    }
                },
                {
                    //data: "AssociatedItemId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var _idrandom = UniqueId(5);
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        var htmlString = '<div class="input-group"><select name="item-associated" ' + _disalbed+' id="item-associated' + _idrandom + '" data-unitidref="' + _idrandom+'" class="form-control select2 item-associated" style="width: 200px;">';
                        if (row.AssociatedItem) {
                            htmlString += '<option value="' + row.AssociatedItem.id + '">' + row.AssociatedItem.text + '</option>';
                        }
                        htmlString += '</select>';
                        htmlString += '<span class="input-group-btn">';
                        htmlString += '<select ' + _disalbed+' onchange="updateAssociatedItem(\'' + _idrandom + '\',' + row.Variant.TraderId+')" id="unit-associated' + _idrandom + '" data-unitidref="' + _idrandom +'" name="unit-associated" class="form-control select2 unit-associated" style="width: 100px;">';
                        htmlString += '<option value=""></option>';
                        
                        if (row.AssociatedUnits != null && row.AssociatedUnits.length > 0) {
                            row.AssociatedUnits.forEach(item => {
                                htmlString += '<option value="' + item.id + '" ' + (item.selected?"selected":"")+'>' + item.text + '</option>';
                            });
                        }
                        htmlString += '</select></span>';
                        htmlString += '</div>';
                        return htmlString;
                    }
                },
                {
                    data: "ItemId",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        return '<button ' + _disalbed+' class="btn btn-default cartb2b-remove-item" onclick="removeItemFromOrder(' + row.ItemId+')"><i class="fa fa-trash"></i></button>';
                    }
                }
            ],
            "drawCallback": function (settings) {
                var handleQuantityChange = delay(function (e) {
                    var $elm = $(this);
                    var _item = {
                        Id: $elm.attr("itemId"),
                        Quantity: $elm.val()
                    }
                    updateQuantityOrderItem(_item);
                }, 1000);
                $(".itemquantity").on("change", handleQuantityChange);
            }
        });
    $('#order-list').on('draw.dt', function () {
        $(document).ready(function () {
            setTimeout(function () {
                $('#order-list select.item-associated').select2({
                    placeholder: "Please select",
                    ajax: {
                        url: '/Commerce/GetListTraderItem',
                        delay: 250,
                        data: function (params) {
                            var query = {
                                search: params.term
                            }
                            return query;
                        },
                        cache: true,
                        processResults: function (data) {
                            return {
                                results: data.Object
                            };
                        }
                    },
                    minimumInputLength: 1
                });
                $('#order-list select.unit-associated').select2({placeholder: "Please select"});
                $('#order-list select.item-associated').on("select2:select", function (e) {
                    
                    var data = e.params.data;
                    var unitidref=$(this).data("unitidref");
                    var $unit = $('#unit-associated' + unitidref);
                    $unit.empty();
                    $unit.select2({
                        placeholder: "Please select",
                        data: (data.units ? data.units : [])
                    });
                    $unit.trigger('change');
                });
            }, 100);
        });
    });
}
function initSellOrderItemsTable() {
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
                "url": '/Commerce/LoadB2BOrderItems',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "orderKey": $orderUI.$hdfOrderKey.val(),
                        "searchKey": $("#order-search-key").val(),
                        "catIds": $("#order-search-categories").val(),
                        "isViewBuy": false
                    });
                },
                "dataSrc": function (data) {
                    var $totalpricestr = $("#totalpricestr");
                    if (data.recordsTotal > 0) {
                        $totalpricestr.text(currencySetting.CurrencySymbol + data.data[0].TotalPrice.toFixed(currencySetting.DecimalPlace));
                        $('#cart-empty').hide();
                        $('#cart').show();
                    } else {
                        $('#cart-empty').show();
                        $('#cart').hide();
                    }
                    return data.data;
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
                            htmlString += '<div class="col-xs-6">';
                            htmlString += '<h6>' + row.Variant.Name + '</h6>';
                            htmlString += '</div>';
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
                    orderable: false
                },
                {
                    data: "Quantity",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        htmlString = "<input style=\"width: 80px;\" type=\'number\' itemId='" + row.ItemId + "' " + _disalbed + "  id='itemquantity" + row.ItemId + "' class=\'form-control itemquantity\' value=\'" + row.Quantity + "\'\ oninput=\"validity.valid||(value='0')\"; min=\"0\">";
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
                        htmlString = "\<input type=\'number\' itemId='" + row.ItemId + "' " + _disalbed + "  id='itemdiscount" + row.ItemId + "' class=\'form-control itemdiscount\' real-discount-value=\'" + row.Discount + "\' value=\'" + row.Discount + "\'\ oninput=\"validity.valid || (value = '0')\" min=\"0\" max= \"100\" step=\"0.001\">";
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
                        var _disalbed = row.IsAllowEdit ? "" : "disabled";
                        return '<button ' + _disalbed+' class="btn btn-default cartb2b-remove-item" onclick="removeItemFromOrder(' + row.ItemId+')"><i class="fa fa-trash"></i></button>';
                    }
                }
            ],
            "drawCallback": function (settings) {
                var handleDiscountChange = delay(function (e) {
                    var $elm = $(this);
                    var $row = $elm.parents("tr");
                    //var $table = $('#order-list').DataTable();
                    var itemId = $($row).find("input[type=number]").attr("itemId");
                    $elm.attr("real-discount-value",$elm.val());
                    updateDiscountOrderItem(itemId);
                }, 1000);
                var hendlePriceChange = delay(function (e) {
                    var $elm = $(this);
                    var $row = $elm.parents("tr");
                    var $table = $('#order-list').DataTable();
                    var itemId = $($row).find("input[type=number]").attr("itemId");
                    $("#itemdiscount" + itemId).parent().addClass('discountContainer' + itemId);

                    var itemPurePrice = parseFloat($("#pureprice" + itemId).val());
                    var newPrice = parseFloat($("#itemprice" + itemId).val());

                    if (newPrice > itemPurePrice) {
                        $table.cell('.discountContainer' + itemId).data(0);
                    } else {
                        var newDiscount = (1 - newPrice / itemPurePrice) * 100;
                        $table.cell('.discountContainer' + itemId).data(newDiscount.toFixed(currencySetting.DecimalPlace));
                        $("#itemdiscount" +itemId).attr("real-discount-value", newDiscount);
                    }
                    updateDiscountOrderItem(itemId);
                }, 1000);
                var handleQuantityChange = delay(function (e) {
                    var $elm = $(this);
                    var _item = {
                        Id: $elm.attr("itemId"),
                        Quantity: $elm.val()
                    }

                    updateQuantityOrderItem(_item);
                }, 1000);
                $(".itemquantity").on("change", handleQuantityChange);
                $(".itemdiscount").on("change", handleDiscountChange);
                $(".price").on("change", hendlePriceChange);
            }
        });
    $('#order-list').on('draw.dt', function () {
        $("#order-list .select2").select2({
            placeholder: "Please select"
        });
        //$('#tbl-unallocated input[data-toggle=toggle]').bootstrapToggle();
    });
}
function updateDiscountOrderItem(itemId) {
    var _item = {
        Id: itemId,
        Variant: {
            // Discount: parseFloat($("#itemdiscount" + itemId).val()),
            Discount: parseFloat($("#itemdiscount" + itemId).attr("real-discount-value")),
        }
    }

    var _url = "/Commerce/UpdateDiscountOrderItem";
    $('#order-list').LoadingOverlay("show");

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: JSON.stringify({
            disKey: $orderUI.$discussionKey.val(),
            orderKey: $orderUI.$hdfOrderKey.val(),
            updatedItem: _item
        }),
        contentType: 'application/json',
        success: function (response) {
            if (response.result) {
                reloadOrderStatus();
            } else {
                cleanBookNotification.error(response.msg);
            }
            $('#order-list').LoadingOverlay("hide", true);
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
            $('#order-list').LoadingOverlay("hide", true);
        }
    });
}
function updateTotalOrder() {
    var currencySetting = {
        MoneyCurrency: $("#currencySymbol").val(),
        MoneyDecimalPlaces: $("#currencyDecimalPlace").val()
    };

    var quantity = parseFloat($("#item-quantity").val());
    var variant = parseFloat($("#item-variant").val());

    var items = $('#item-extras div label div input');

    var extra = 0;
    _.forEach(items, function (input) {

        if ($('#' + input.id).prop('checked'))
            extra += parseFloat(input.value);

    });

    var total = quantity * (variant + extra);
    var _priceBuyerCurrency = calPriceByExchangeRate(total,true);
    $("#order-total-prices").empty().append(total.toFixed(currencySetting.MoneyDecimalPlaces));
    $('.total-price-buyer-currency').text(_priceBuyerCurrency);
    $("#order-total-prices").attr("value", total);
}
function updateQuantityOrderItem(item) {
    var _url = "/Commerce/UpdateQuantityOrderItem";
    $('#order-list').LoadingOverlay("show");

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: JSON.stringify({
            disKey: $orderUI.$discussionKey.val(),
            orderKey: $orderUI.$hdfOrderKey.val(),
            updatedItem: item
        }),
        contentType: 'application/json',
        success: function (response) {
            if (response.result) {
                reloadOrderStatus();
            } else {
                cleanBookNotification.error(response.msg);
            }
            $('#order-list').LoadingOverlay("hide", true);
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
            $('#order-list').LoadingOverlay("hide", true);
        }
    });

    //LoadingOverlayEnd();
}
function removeItemFromOrder(itemId) {
    if(confirm("Do you want to completely remove this item from the cart?")){
        $('#order-list').LoadingOverlay('show', true);
        var disKey = $orderUI.$discussionKey.val();
        var _url = "/Commerce/RemoveItemFromB2BOrder?disKey=" + disKey + "&itemId=" + itemId;
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    reloadDataTable();
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            },
            always: function (){
                $('#order-list').LoadingOverlay('hide', true);
            }
        })
    }
    
}
function updateAssociatedItem(_refId, variantId) {
    var _url = "/Commerce/UpdateAssociatedItem";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            orderKey: $orderUI.$hdfOrderKey.val(),
            itemId: $('#item-associated' + _refId).val(),
            unitId: $('#unit-associated' + _refId).val(),
            variantId: variantId
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    })
}
function getDeliveryDetail(locationId) {
    $.get("/Commerce/GetDeliveryDetail?locationId=" + locationId, function (response) {
        $('.delivery-detail').fadeIn();
        $('.location-name').html('<strong>' + response.locationName + '</strong>');
        $('.address-text').html(response.address);
    });
}
function sellingDomainSubmitProposal() {
    LoadingOverlay();
    var _url = "/Commerce/SellingDomainSubmitProposal";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            disKey: $orderUI.$discussionKey.val()
        },
        success: function (response) {
            if (response.result) {
                reloadOrderButtonSubmit();
                reloadDataTable();
                cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    })
}
function reloadOrderButtonSubmit() {
    $('.btn-block-submit').load('/Commerce/LoadOrderSubmit?orderKey=' + $orderUI.$hdfOrderKey.val());
}
function sellingChooseWGModal() {
    $orderUI.$choosewg.find(".modal-body").load('/Commerce/SellingChooseWGModal?orderKey=' + $orderUI.$hdfOrderKey.val(), function () {
        $('#choose-wg .select2').select2();
        initSellingOrderProcessingForm();
    });
}
function buyingChooseWGModal() {
    if ($('#frmBuyingSubmmitProposal').valid()) {
        $orderUI.$choosewg.modal('show');
        $orderUI.$choosewg.find(".modal-body").load('/Commerce/BuyingChooseWGModal?orderKey=' + $orderUI.$hdfOrderKey.val() + '&locationId=' + $orderUI.$slDeliveryTo.val(), function () {
            $('#choose-wg .select2').select2();
            initBuyingOrderProcessingForm();
        });
    }
}
function initSellingOrderProcessingForm() {
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
            var paymentAcc = $("#paymentacc").val();
            var saleWG = $("#salewg").val();
            var invoiceWG = $("#invoicewg").val();
            var paymentWG = $("#paymentwg").val();
            var transferWG = $("#transferwg").val();
            var _url = "/Commerce/ProcessB2BOrder";
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    orderKey: $orderUI.$hdfOrderKey.val(),
                    paymentAccId: paymentAcc,
                    disKey: $orderUI.$discussionKey.val(),
                    saleWGId: saleWG,
                    invoiceWGId: invoiceWG,
                    paymentWGId: paymentWG,
                    transferWGId: transferWG
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        $("#choose-wg").modal("hide");
                        reloadOrderButtonSubmit();
                        reloadDataTable();
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                    LoadingOverlayEnd();
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        }
    })
}
function initBuyingOrderProcessingForm() {
    var $frmorderprocessing = $("#orderprocessing-frm");

    $frmorderprocessing.validate({
        rule: {
            paymentacc: {
                required: true
            },
            purchasewg: {
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
            var paymentAcc = $("#paymentacc").val();
            var purchaseWG = $("#purchasewg").val();
            var invoiceWG = $("#invoicewg").val();
            var paymentWG = $("#paymentwg").val();
            var transferWG = $("#transferwg").val();
            var _url = "/Commerce/BuyingDomainSubmitProposal";
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    orderKey: $orderUI.$hdfOrderKey.val(),
                    paymentAccId: paymentAcc,
                    disKey: $orderUI.$discussionKey.val(),
                    purchaseWGId: purchaseWG,
                    invoiceWGId: invoiceWG,
                    paymentWGId: paymentWG,
                    transferWGId: transferWG,
                    destinationLocationId: $orderUI.$slDeliveryTo.val()
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.updateSuccess();
                        $("#choose-wg").modal("hide");
                        reloadOrderButtonSubmit();
                        reloadDataTable();
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                    LoadingOverlayEnd();
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                    LoadingOverlayEnd();
                }
            });  
        }
    })
}
function checkForSellingSubmitAbility() {
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
function checkForBuyingSubmitAbility() {
    var paymentAcc = $("#paymentacc").val();
    var purchaseWG = $("#purchasewg").val();
    var invoiceWG = $("#invoicewg").val();
    var paymentWG = $("#paymentwg").val();
    var transferWG = $("#transferwg").val();

    if (paymentAcc != "" && purchaseWG != "" && invoiceWG != "" && paymentWG != "" && transferWG != "") {
        $("#orderprocessing-frm").find("button[type=submit]").removeAttr("disabled");
    } else {
        $("#orderprocessing-frm").find("button[type=submit]").attr("disabled", "disabled");
    }
}

function loadMorePostsDiscussion(activityKey, pageSize, divId) {

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
            }
            $('#' + divId).append(response).fadeIn(250);
            CountPost = CountPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function reloadOrderStatus() {
    reloadDataTable();
    reloadOrderButtonSubmit();
}
function updateExchangeRate() {
    var _paramaters = {
        id: $orderUI.$amountBuyer.data('id'),
        amountSeller: $('input[name=AmountSeller]').val(),
        amountBuyer: $orderUI.$amountBuyer.val(),
    };
    $.post("/Commerce/UpdateExchangeRateById", _paramaters, function (response) {
        if (response.result) {
            $orderUI.$amountBuyer.data('exchangerateval',response.Object);
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Qbicles");
            initOrderItemsShow();
            reloadDataTable();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function showAmountConverted(el) {
    var table = $("#order-list").DataTable();
    var convertedColumn = table.column(7);
    if ($(el).prop('checked')){
        $('.converted').fadeIn();
        convertedColumn.visible(true);
    }
    else{
        $('.converted').fadeOut();
        convertedColumn.visible(false);
    }
}
function calPriceByExchangeRate(price,isNotSpan) {
    var _amountBuyer;
    if ($orderUI.$amountBuyer.length > 0) {
        _amountBuyer = price * parseFloat($orderUI.$amountBuyer.data('exchangerateval'));
        if (isNotSpan)
            _amountBuyer ='/'+_amountBuyer.toFixed(currencySetting.DecimalPlace) + $orderUI.$amountBuyer.data('currency');
        else
            _amountBuyer = '<span class="converted" ' + ($('input[name=showpriceconverted]').prop('checked') ? '' : 'style = "display: none;"') + '>/' + _amountBuyer.toFixed(currencySetting.DecimalPlace) + $orderUI.$amountBuyer.data('currency') + '</span>';
    }
    return _amountBuyer;
};

function UpdateB2BOrderStatus(){
    var status = $("#b2b-order-status");
    var discussionOrderKey = $("#discussionKey").val();
    var _url = "/Commerce/CheckStatusB2BOrders"
    $.ajax({
        type: "GET",
        url: _url,
        data: {DiscussionOrderKey: discussionOrderKey},
        success: function (response) {
            if(response.result){
                status.text(response.Object.statusOrder);
                status.attr("class",response.Object.labelOrder);
            }
        }
    });
}