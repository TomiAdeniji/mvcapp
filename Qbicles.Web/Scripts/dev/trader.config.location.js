$(document).ready(function () {
    InitLocationDataTable();
    $('#location-tab-geolocated').select2();
    // Init onchange events
    $('#location-tab-key-search').keyup(delay(function () {
        $("#tbllocations").DataTable().ajax.reload();
    }, 500));

    $('#location-tab-geolocated').change(delay(function () {
        $("#tbllocations").DataTable().ajax.reload();
    }, 500));
});
function clickAddlocation() {
    $('#app-location-add').load('/TraderConfiguration/LocationAddEdit');
};
function saveLocation() {


    if ($("#location-edit-name").val() == "") {
        $("#location_form").validate().showErrors({ Name: "This field is required." });
        return false;
    }


    var long = parseFloat($("#Longitude").val());
    if ($("#Longitude").val() != "") {
        if (long < -180 || long > 180) {
            $("#location_form").validate().showErrors({ Longitude: "Longitude value as a decimal value between -180 and 180" });
            return false;
        }
    }

    var lat = parseFloat($("#Latitude").val());
    if ($("#Latitude").val() != "") {
        if (lat < -90 || lat > 90) {
            $("#location_form").validate().showErrors({ Latitude: "Latitude value as a decimal value between -90 and 90" });
            return false;
        }
    }

    var country = $('#CountryName').val();
    var location = {
        Id: $('#location-id').val(),
        Name: $('#location-edit-name').val(),
        Address: {
            Id: $('#location-address-id').val(),
            AddressLine1: $('#AddressLine1').val(),
            AddressLine2: $('#AddressLine2').val(),
            City: $('#City').val(),
            State: $('#State').val(),
            PostCode: $('#PostCode').val(),
            Email: $('#Email').val(),
            Phone: $('#Phone').val(),
            Longitude: long.toFixed(7),
            Latitude: lat.toFixed(7)

        }
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderConfiguration/Savelocation?country=' + country,
        data: { location: location },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.result === true) {
                if (response.actionVal === 1) {
                    $("#tbllocations").DataTable().ajax.reload();
                    $('#app-location-add').modal('toggle');
                    cleanBookNotification.createSuccess();
                } else if (response.actionVal === 2) {
                    $("#tbllocations").DataTable().ajax.reload();
                    $('#app-location-add').modal('toggle');
                    cleanBookNotification.updateSuccess();
                } else if (response.actionVal === 3) {
                    //cleanBookNotification.error(response.msg, "Qbicles");
                    $("#location_form").validate().showErrors({ Name: response.msg });
                }
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_location_add');
        LoadingOverlayEnd();
    });
}
function Editlocation(locationId) {
    $('#app-location-add').load('/TraderConfiguration/LocationAddEdit?id=' + locationId);
};

function ConfirmDeletelocation(id, name) {
    $('#label-confirm-location').text("Do you want delete location: " + name);
    $('#id-itemlocation-delete').val(id);
};
function deletelocation() {
    $.ajax({
        type: 'delete',
        url: '/TraderConfiguration/Deletelocation',
        data: { id: $('#id-itemlocation-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                $("#tbllocations").DataTable().ajax.reload();
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            $('.modal-cancel').click();
        }
    });
};

function reloadLocation() {
    $('#comfig-content').load('/TraderConfiguration/TraderConfigurationContent?value=location');
}
function setDefaultAddress(elm) {
    $.post("/Commerce/SetDefaultAddress", { locationId: $(elm).val() }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}


function InitLocationDataTable() {
    var keySearchElm = $("#location-tab-key-search");
    var geoLocatedElem = $("#location-tab-geolocated");

    $("#tbllocations").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#tbllocations').LoadingOverlay("show");
        } else {
            $('#tbllocations').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [5, 10, 20, 50, 100]],
            "pageLength": 10,
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            },
            "ajax": {
                "url": '/Commerce/GetLocationTableData',
                "type": 'POST',
                "data": function (d) {
                    keySearch = "";
                    geoLocated = null;

                    if (keySearchElm.length > 0)
                        keySearch = keySearchElm.val();
                    if (geoLocatedElem.length > 0)
                        if (geoLocatedElem.val() == 1)
                            geoLocated = true;
                        else if (geoLocatedElem.val() == 2)
                            geoLocated = false;

                    return $.extend({}, d, {
                        "keySearch": keySearch,
                        "isGeoLocated": geoLocated
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "Address",
                    orderable: true
                },
                {
                    data: "Lat",
                    orderable: true
                },
                {
                    data: "Long",
                    orderable: true
                },
                {
                    data: "GeoText",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlString = "";

                        htmlString += '<i class="' + row.GeoClass + '"></i><span hidden>' + row.GeoText + '</span>'

                        return htmlString;
                    }
                },
                {
                    data: "Creator",
                    orderable: true
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";

                        htmlString += '<input type="radio" name="DefaultAddress" onchange="setDefaultAddress(this);" ' + row.DefaultAddressClass + ' value="' + row.Id + '">';

                        return htmlString;
                    }
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";

                        htmlString += '<button class="btn btn-warning" onclick="Editlocation(' + row.Id + ')" data-toggle="modal" data-target="#app-location-add"><i class="fa fa-pencil"></i></button> ';
                        htmlString += '<button ' + row.CanDelText + ' class="btn btn-danger" data-toggle="modal" data-target="#app-location-confirm" onclick="ConfirmDeletelocation(' + row.Id + ', \'' + row.Name + '\')"><i class="fa fa-trash"></i></button>';

                        return htmlString;
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
// end location functions