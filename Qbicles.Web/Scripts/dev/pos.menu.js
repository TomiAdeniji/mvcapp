$('#search-item-list').keyup(function () {
    $("#item-list").DataTable().ajax.reload();
});
var categorys = [];
var traderItemsFilter = [];
var $locationManager = $('#location-manager').val();
$('#subfilter-group').on('change', function () {
    $("#item-list").DataTable().ajax.reload();
});
var $disableButton = "";
var imgApi = $("#img-api").val();

var $menuId = 0;
UpdateMenu = function (id) {

    $.LoadingOverlay("show");
    $("#menu-name").val('');
    $("#menu-summary").val('');
    $('#report_filters').val('');
    $('#report_filters').select2({ placeholder: 'Please select' });
    $("#menu-modal-title").text('Edit catalog');
    $menuId = id;
    $.ajax({
        url: "/PointOfSaleMenu/GetById?id=" + id,
        type: "post",
        dataType: "json",
        success: function (rs) {
            if (rs.actionVal > 0) {
                $("#menu-name").val(rs.Object.Name);
                var $menusalechannel = $("#menu-salechannel");
                $menusalechannel.prop("disabled", true);
                $menusalechannel.val(rs.Object.SalesChannel).trigger("change");
                $("#menu-summary").val(rs.Object.Description);
                $('#report_filters').val(rs.Object.Dimensions);
                $('#report_filters').select2({ placeholder: 'Please select' });

                if (rs.Object.Type == 1) {
                    $(".display-locations").hide();
                    $(".display-report-filters").hide();
                } else {
                    $(".display-locations").show();
                    $(".display-report-filters").show();
                }
                $('#catalog-type-value').val(rs.Object.Type);
                $('#app-trader-pos-menu-modal').modal('show');
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


$("#item_featuredimg").change(function () {
    var target = $(this).data('target');
    readImgURL(this, target);
    $(target).fadeIn();
});

function readImgURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function initItemTable() {
    var catalogId = $("#menu_id").val();
    var catalogStatus = $("#catalog_status").val();
    $("#item-list").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#item-list').LoadingOverlay("show");
        } else {
            $('#item-list').LoadingOverlay("hide", true);
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
                "url": '/PointOfSale/LoadCatalogItemTableContent',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "catalogId": catalogId,
                        "keySearch": $("#search-item-list").val(),
                        "categoryIdSearch": $("#subfilter-group").val(),
                    });
                }
            },
            "columns": [
                {
                    data: "ImageUri",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        htmlString += '<a href="javascript:" data-toggle="tab" data-target="#item-detail">';
                        htmlString += '<div class="table-avatar mini" style="background-image: url(\'' + row.ImageUri + '\');">&nbsp;</div>';
                        htmlString + '</a>';
                        return htmlString;
                    }
                },
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "SKU",
                    orderable: true
                },
                {
                    data: "CategoryName",
                    orderable: true
                },
                {
                    data: "Price",
                    orderable: true
                },
                {
                    data: "Level",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        var htmlString = "";
                        htmlString += row.Level;
                        htmlString += row.InStockLabel;
                        return htmlString;
                    }
                },
                {
                    //Options
                    orderable: false,
                    render: function (data, type, row, data) {
                        var htmlString = "";
                        htmlString += "<div class='btn-group options table-button'>";
                        htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                        htmlString += '<i class="fa fa-cog"></i> &nbsp; Options';
                        htmlString += '</button>';
                        htmlString += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                        htmlString += '<li><a class="viewdetail_' + row.Id + '" href="#item-detail-basic" onclick="ViewPoscategoryItemDetail(\'' + row.Id + '\')" data-toggle="tab">View &amp; edit</a></li>';
                        if (catalogStatus == "Inactive") {                           
                           
                            var iName = encodeURIComponent(row.Name);     
                            iName = iName.replace(/'/g, "Ⓞ");
                                                        
                            htmlString += "<li><input hidden id='item-row-id-" + row.Id + "' value='" + iName + "'/> <a href='#' data-toggle='modal' data-target='#menu-item-delete-confirm' onclick='ConfirmDeleteMenuItem(" + row.Id + ")'>Remove item</a></li>";

                        }
                        return htmlString;
                    }
                }
            ]
        })
}


// Edit menu in Menu detail page
function SavePosMenu() {

    if (!$("#pos-menu-form").valid())
        return;
    $.LoadingOverlay("show");
    // Upload images
    var files = document.getElementById("expert-catalog-img").files;
    if (files && files.length > 0) {
        UploadMediaS3ClientSide("expert-catalog-img").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                menuSave(mediaS3Object.objectKey);
            }
        });
    } else {
        menuSave(null);
    }

};

function menuSave(imageUri) {
    var catalogTypeElement = $(".nav-stacked > .active > a");
    var catalogType = 0;
    if (catalogTypeElement) {
        catalogType = catalogTypeElement.attr('catalogType');
    }
    if (catalogType == null) {
        catalogType = $('#catalog-type-value').val();
    }
    var orderItemDimensions = [];
    if (catalogType == 1) {
        var dimensions = $('#report_filters').val();
        if (dimensions == null || dimensions === "") {
            $("#pos-menu-form").validate().showErrors({ filters: "Reporting Filter required" });
            return false;
        }
        if (dimensions && dimensions.length > 0) {
            for (var i = 0; i < dimensions.length; i++) {
                orderItemDimensions.push({ Id: dimensions[i] });
            }
        }
    }

    //$.LoadingOverlay("show");
    var posMenu = {
        Id: $menuId,
        Name: $("#menu-name").val(),
        SalesChannel: $("#menu-salechannel").val(),
        OrderItemDimensions: orderItemDimensions,
        Description: $("#menu-summary").val(),
        Type: catalogType,
        Image: imageUri
    };

    var url = "/PointOfSaleMenu/UpdatePosMenu";

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posMenu: posMenu },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                if (rs.Object) {
                    $("#menu-name-description").text(rs.Object.Description);
                    $("#menu-name-title").text(rs.Object.Name);
                    $(".txtsalechannel").text(rs.Object.SalesChannel + " Menu");
                    $("#lstDimension").text(rs.Object.Dimensions);
                    if (rs.Object.SalesChannel == "POS")
                        $('#btnUpdatePosmenu').show();
                    else
                        $('#btnUpdatePosmenu').hide();
                }

                $('#app-trader-pos-menu-modal').modal('hide');
            } else if (!rs.result && rs.msg) {
                $("#pos-menu-form").validate().showErrors({ menuname: rs.msg });
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


//find item when add new
function FindSKU() {
    $("#app-trader-pos-itemlist").empty();
    var sku = $('#item_sku').val();
    var locationId = $locationManager;
    var $catalogType = $("#catalog_type");
    if ($catalogType && $catalogType.val() == "1") {
        locationId = 0;
    };
    var url = "/PointOfSaleMenu/ShowPosItemModal?search=" + sku + "&locationid=" + locationId + "&selectItemType=1" + "&idposition=0";
    url = url.replace(/\s/g, "%20");
    $("#app-trader-pos-itemlist").load(url);

}

//find item when add Extra to detail
function FindSKUDetail() {
    $("#app-trader-pos-itemlist").empty();
    var sku = $('#poscategoryitem_detail_sku_search').val();
    var locationId = $locationManager;
    var $catalogType = $("#catalog_type");
    if ($catalogType && $catalogType.val() == "1") {
        locationId = 0;
    };
    var url = "/PointOfSaleMenu/ShowPosItemModal?search=" + sku + "&locationid=" + locationId + "&selectItemType=2" + "&idposition=0";
    url = url.replace(/\s/g, "%20");
    $("#app-trader-pos-itemlist").load(url);

}

//find item when add variant detail
function FindSKUVariants(id) {
    $("#app-trader-pos-itemlist").empty();
    var sku = $('#table_variants_sku_' + id).val();
    var locationId = $locationManager;
    var $catalogType = $("#catalog_type");
    if ($catalogType && $catalogType.val() == "1") {
        locationId = 0;
    };
    var url = "/PointOfSaleMenu/ShowPosItemModal?search=" + sku + "&locationid=" + locationId + "&selectItemType=3" + "&idposition=" + id;
    url = url.replace(/\s/g, "%20");
    $("#app-trader-pos-itemlist").load(url);
}

function LoadPosCategories(viewItem) {
    $("#item-list").DataTable().ajax.reload(null, false);
}

function categorychanged() {
    if ($("#item_category").val() === "" || !$("#item_category").val()) {
        //valid = false;
        $("#form_additem").validate().showErrors({ categoryitem_add_category: "Category is required." });
        //return false;
    } else {
        $('#item_category + label.error').remove();
    }

}

function validatePosCategoryItem() {
    var valid = true;
    if ($("#item_name").val() === "") {
        SaveProcessdCategoryitem
        valid = false;
        $("#form_additem").validate().showErrors({ categoryitem_add_name: "Name is required." });
    } else {
        $('#item_name + label.error').remove();
    }
    if ($("#item_category").val() === "" || !$("#item_category").val()) {
        valid = false;
        $("#form_additem").validate().showErrors({ categoryitem_add_category: "Category is required." });
    } else {
        $('#item_category + label.error').remove();
    }
    if ($('#item_novariant')[0].checked) {
        if ($("#traderitem").val() === "" || $("#traderitem").val() === "0") {
            valid = false;
            //cleanBookNotification.error("Have an error, detail: Item product empty, please find and select Item product", "Qbicles");
            $("#form_additem").validate().showErrors({ categoryitem_sku: "Sku of Item product is required." });

        }

        if ($("#item_product_unit").val() == null || $("#item_product_unit").val() == "" || $("#item_product_unit").val() == "0") {
            valid = false;
            cleanBookNotification.error("The selected product does not have any units. Please add units to product.", "Qbicles");
        }
    }
    return valid;
};


function ChangeItemVariant(imgUrlDefault) {
    $('.onlysold').toggle();
    $('#itemextras').hide();
    $('.newpreview').attr("style", "display:none");
    $('.newpreview').attr("src", "");
    $('#item_featuredimg').val(null);
    var isVariant = $('#item_novariant')[0].checked;
    if (!isVariant) {
        {
            $('.newpreview').attr("src", $api + imgUrlDefault);
            $('.newpreview').attr("style", "display:block");

            $('#traderitem').val(0);
            $('#item_sku').val("");
        }
    }
};




function selectItem(traderitemId, sku, imgUrl) {

    $('#itemextras').show();
    $('#traderitem').val(traderitemId);
    $('#item_sku').val(sku);
    $('#image-default').val(imgUrl);
    $('.newpreview').attr("src", $api + imgUrl);
    $('.newpreview').attr("style", "block");
    var salechannel = $('#menu_salechannel').val();

    // catalog type: 0-sales, 1-distribution
    $('#item_product_unit').empty();

    var $catalogtype = $("#catalog_type").val();
    // getprice
    $.ajax({
        url: "/PointOfSaleMenu/GetPriceValue?traderid=" + traderitemId + "&locationid=" + $locationManager + "&saleChannel=" + salechannel,
        type: "get",
        dataType: "json",
        success: function (rs) {
            if (!rs.result) {
                //Distribution catalog does not need price validation
                if (!$catalogtype || $catalogtype != 1) {
                    cleanBookNotification.error(_L("ERROR_MSG_343"), "Qbicles");
                }
                return;
            }

            $('#item_product_unit').append(rs.Object2);
            $('#item_product_unit').select2();

            $('#priceDefault').val(rs.Object.GrossPrice);
            $('#price_id').val(rs.Object.Id);
            $('#unitprice').val(rs.Object.GrossPrice);
            if ($("#traderitem").val() === "" || $("#traderitem").val() === "0") {

                //cleanBookNotification.error("Have an error, detail: Item product empty, please find and select Item product", "Qbicles");
                $("#form_additem").validate().showErrors({ categoryitem_sku: _L("ERROR_MSG_342") });

            }
        }
    });

}

function selectItemDetail(traderitemId, sku, imgUrl) {
    $('#poscategoryitem_detail_itemid').val(traderitemId);
    $('#poscategoryitem_detail_sku_search').val(sku);

}

function selectItemToTable(id, traderitemid, sku, imgUrl) {
    var salechannel = $('#menu_salechannel').val();
    $.ajax({
        url: "/PointOfSaleMenu/GetPriceValue?traderid=" + traderitemid + "&locationid=" + $locationManager + "&saleChannel=" + salechannel,
        type: "get",
        dataType: "json",
        success: function (rs) {
            if (!rs.result) {
                cleanBookNotification.error(_L("ERROR_MSG_343"), "Qbicles");
                return;
            }

            $('#table_variants_traderitem_' + id).val(traderitemid);
            $('#table_variants_sku_' + id).val(sku);
            $('#variant-image-preview-' + id).attr('src', imgApi + imgUrl);


            $('#table_variants_pricebase_' + id).val(rs.Object.GrossPrice);
            $('#table_variants_priceid_' + id).val(rs.Object.Id);
            $('#table_variants_price_' + id).val(rs.Object.GrossPrice);

            var unitHtml = "<select name=\"unit\" onchange=\"changeUnitTable('" + id + "')\" id=\"table_variants_unit_" + id + "\" class=\"form-control select2\" style=\"width: 100 %;\">";
            unitHtml += rs.Object2;
            unitHtml += "</select>";
            $('#table_variants_tdunit_' + id).empty();
            $('#table_variants_tdunit_' + id).append(unitHtml);

            $('#table_variants_tdunit_' + id + " select").select2({ placeholder: 'Please select' });

            var unit = $('#table_variants_unit_' + id).val();
            if (unit && unit !== "") {
                $('#table_variants_price_' + id).val((_.toNumber(rs.Object.GrossPrice) * _.toNumber(unit.split('|')[1])).toFixed(2));
            }
            if (rs.Object.Id === "0") {
                cleanBookNotification.error(_L("ERROR_MSG_344"), "Qbicles");
                return;
            }
            SaveVariantsTable(id);
        }
    });
}

//function getPriceBySKU(sku, id) {
//    //var salechannel = $('#menu_salechannel').val();
//    // getprice
//    $.ajax({
//        url: "/PointOfSaleMenu/GetTraderItemBySku?sku=" + sku + "&locationId=" + $locationManager,
//        type: "get",
//        dataType: "json",
//        success: function (rs) {
//            if (rs.result) {
//                listProductitems = rs.Object.Items;
//                selectItemToTable(id, rs.Object.Id, sku);
//            } else {
//                cleanBookNotification.error(rs.msg, "Qbicles");
//            }
//            LoadingOverlayEnd();
//        }
//    });
//}

function changeUnitTable(id) {
    var baseprice = _.toNumber($('#table_variants_pricebase_' + id).val());
    var quantityofbase = _.toNumber($('#table_variants_unit_' + id).val().split('|')[1]);
    $('#table_variants_price_' + id).val((baseprice * quantityofbase).toFixed(2));
    SaveVariantsTable(id);
}

function SaveVariantsTable(id) {
    $.LoadingOverlay("show");
    if ($('#table_variants_unit_' + id) == null || $('#table_variants_unit_' + id).val() == null
        || $('#table_variants_traderitem_' + id) == null || $('#table_variants_traderitem_' + id).val() == null
        || $('#table_variants_unit_' + id).val().split('|')[0] === "" || $('#table_variants_traderitem_' + id).val() === ""
        || $('#table_variants_traderitem_' + id).val() === "0") {
        LoadingOverlayEnd();
        cleanBookNotification.error(_L("ERROR_MSG_343"), "Qbicles");
        return;
    } else {
        if ($('#table_variants_image_' + id).val() === "" && $('#table_variants_file_' + id).val() !== "")
            UploadMediaS3ClientSide("table_variants_file_" + id).then(function (response) {
                if (response === "no_image") {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

                }
                $('#table_variants_image_' + id).val(response);

            });

        var options = $('#table_variants_options_' + id).val().split(',');
        var posVariantOption = [];
        _.forEach(options, function (value) {
            posVariantOption.push({ Id: value });
        });


        var variant = {
            Id: $('#table_variants_id_' + id).val(),
            Name: $('#table_variants_name_' + id).text(),
            TraderItem: {
                Id: $('#table_variants_traderitem_' + id).val()
            },
            ImageUri: $('#table_variants_image_' + id).val(),
            Unit: { Id: $('#table_variants_unit_' + id).val().split('|')[0] },
            BaseUnitPrice: { Id: $('#table_variants_priceid_' + id).val() },
            Price: {
                GrossPrice: $('#table_variants_price_' + id).val()
            },
            CategoryItem: { Id: $('#poscategoryitem_detail_id').val() },
            IsDefault: $('#table_variants_default_' + id)[0].checked,
            IsActive: $('#table_variants_isactive_' + id)[0].checked,
            VariantOptions: posVariantOption
        };
        if (variant.TraderItem.Id === "" || variant.TraderItem.Id === "0"
            || variant.Unit.Id === "" || variant.Unit.Id === "0") {
            cleanBookNotification.error(_L("ERROR_MSG_343"), "Qbicles");
            LoadingOverlayEnd();
            $('#table_variants_sku_' + id).val('');
            $('#table_variants_sku_' + id).css("border-color", "red");
            setTimeout(function () {
                $('#table_variants_sku_' + id).css("border-color", "#e1e1e1");
            }, 2000);
            return false;
        }

        if ($('#table_variants_image_' + id).val() === "" && $('#table_variants_file_' + id).val() !== "")
            UploadMediaS3ClientSide("table_variants_file_" + id).then(function (mediaS3Object) {
                if (mediaS3Object.objectKey === "no_image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    return;
                }
                $('#table_variants_image_' + id).val(mediaS3Object.objectKey);
                variant.ImageUri = mediaS3Object.objectKey;
                SubmitPOSMenuVariant(variant);
            });
        else {
            SubmitPOSMenuVariant(variant);
        }
    }
};

function UpdateVariantPriceTable(variantId, index) {
    var price = $("#table_variants_price_" + index).val();
    var _url = "/PointOfSaleMenu/UpdateCatalogItemVariantPrice";
    LoadingOverlay();
    $.ajax({
        type: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            variantId: variantId,
            variantGrossPrice: price
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Update the price of the variant successfully!", "Qbicles");
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    })
};

function SubmitPOSMenuVariant(variant) {
    $.ajax({
        url: "/PointOfSaleMenu/SaveVariant",
        type: "post",
        dataType: "json",
        data: { variant: variant },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#table_variants_id_' + variant.Id).val(rs.msgId);
                LoadVariantsPropertyTable($('#poscategoryitem_detail_id').val());
            } else if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}



function selectItemDetail(traderitemId, sku) {
    $('#poscategoryitem_detail_itemid').val(traderitemId);
    $('#poscategoryitem_detail_sku_search').val(sku);
}
function SelectProductUnitChanged() {
    var unit = $('#item_product_unit').val();
    if (unit && unit !== '') {
        $('#unitprice').val((_.toNumber(unit.split('|')[1]) * _.toNumber($('#priceDefault').val())).toFixed(2));
    } else {
        $('#unitprice').val($('#priceDefault').val());
        cleanBookNotification.error("The selected product does not have any units. Please add units to product.", "Qbicles");
    }
}
function SaveProcessdCategoryitem() {

    if (!validatePosCategoryItem()) return;
    $.LoadingOverlay("show");
    var itemCategory = {
        Id: 0,
        Name: $('#item_name').val(),
        Description: $('#item_description').text(),
        Category: {
            Id: $('#item_category').val(),
            Name: $('#item_category option:selected').text(),
            Menu: { Id: $('#menu_id').val() }
        },
        PosVariants: [],
        ImageUri: $('#image-default').val(),
        VariantProperties: []
    }

    if ($('#item_novariant')[0].checked) {
        itemCategory.PosVariants = [
            {
                Id: 0,
                Name: $('#item_name').val(),
                Unit: { Id: $('#item_product_unit').val().split('|')[0] },
                TraderItem: { Id: $('#traderitem').val() },
                Price: { GrossPrice: $('#unitprice').val() },
                BaseUnitPrice: { Id: $('#price_id').val() }
            }];
    }
    if ($('#item_featuredimg').val() != '') {
        UploadMediaS3ClientSide("item_featuredimg").then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image") {
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                LoadingOverlayEnd();
                return;
            }
            itemCategory.ImageUri = mediaS3Object.objectKey;
            SubmitItemCategory(itemCategory);
        });
    } else {
        SubmitItemCategory(itemCategory);
    }
};


function SubmitItemCategory(itemCategory) {
    $.ajax({
        url: "/PointOfSaleMenu/SaveCategoryItem",
        type: "post",
        dataType: "json",
        data: { posCategoryItem: itemCategory },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-pos-item-add').modal('hide');
                ViewPoscategoryItemDetail(rs.msgId);
            } else if (rs.actionVal === 2) {
                $('#catalog-price-table').DataTable().ajax.reload(null, false);
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

var refershCategorySelect = function (lstCategory, idElement, value) {
    if (lstCategory && lstCategory.length > 0) {
        categorys = lstCategory;
        $('#' + idElement).empty();
        var html = "<option value=\"\"></option>";
        for (var i = 0; i < lstCategory.length; i++) {
            if (lstCategory[i].Id === value) {
                html += "<option selected value=\"" + lstCategory[i].Id + "\">" + lstCategory[i].Name + "</option>";
            } else {
                html += "<option value=\"" + lstCategory[i].Id + "\">" + lstCategory[i].Name + "</option>";
            }

        }
        $('#' + idElement).append(html);
        $('#' + idElement).val(value);
        $('#' + idElement).select2({
            placeholder: 'Please select',
            tags: true
        });
    }

}
function AddAnItem(defaultImage) {
    $('#item_featuredimg + label.error').remove();
    $('#item_name + label.error').remove();
    $('#item_category + label.error').remove();    
    $('#item_name').val('');
    $('#item_category').val('');
    $('#item_category').select2({
        placeholder: 'Please select',
        tags: true
    });
    $('#item_sku').val('');
    $('#item_product_unit').val('');
    $('#item_product_unit').select2();
    $('#unitprice').val('');
    $('#item_featuredimg').val(null);
    $('#itemextras').hide();
    $('#item_description').text('');
    $('#image-default').val(defaultImage);

    //$('.newpreview').attr("style", "display:none");
    $('.newpreview').attr("src", $api + defaultImage);
    //$('#item_featuredimg').val("");

    GetListCategoryByMenu(refershCategorySelect, 'item_category', "");
}
function GetListCategoryByMenu(functionName, idElement, defaulValue) {
    var idMenu = $('#menu_id').val();
    var url = "/PointOfSaleMenu/GetCategoryByMenu?idMenu=" + idMenu;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "get",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                functionName(rs.Object.Categories, idElement, defaulValue);
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error("Have an error, detail: " + rs.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
// Switch list category item and Category item detail
function ViewPoscategoryItemDetail(id) {
    //$.LoadingOverlay("show");
    $('#item-detail-basic').empty();
    $('#item-detail-basic').load('/PointOfSaleMenu/GetPosCategoryItemView?id=' + id, function () {
        $('#items-list').css('display', 'none').removeClass('fade in');
        $('#item-detail-basic').css('display', 'block').addClass('fade in');
        //LoadingOverlayEnd();

    });

}


function ConfirmDeleteMenuItem(id) {
    var iName = $("#item-row-id-" + id).val();
    iName = iName.replace(/Ⓞ/g, "'");
    var iName = decodeURIComponent(iName);
    $('#label-confirm-item-delete').text(iName);
    $('#id-item-delete').val(id);
}

function deleteCategoryItem() { 
    var id = $('#id-item-delete').val();
    var url = "/PointOfSaleMenu/DeleteCategoryItem?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "GET",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                LoadPosCategories();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");                
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");            
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function BackToList() {
    $('#item-detail-basic').css('display', 'none');
    $('#items-list').css('display', 'block');
    $("#item-list").DataTable().ajax.reload(null, false);

}

// change Overview
function ChangeDetailItem(key) {
    var url = "/PointOfSaleMenu/UpdatePosCategoryItem";
    if ($('#poscategoryitem_detail_name').val() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_347"), "Qbicles");
        return;
    }

    var item = {
        Id: $('#poscategoryitem_detail_id').val(),
        Name: $('#poscategoryitem_detail_name').val(),
        SKU: $('#poscategoryitem_detail_sku').val(),
        Description: $('#poscategoryitem_detail_category_description').text(),
        Category: {
            Id: $('#poscategoryitem_detail_category').val(),
            Name: $('#poscategoryitem_detail_category option:selected').val(),
            Menu: { Id: $('#menu_id').val() }
        }
    }
    var _itemPrice = $("#poscategoryitem-price").val();
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posCategoryItem: item, itemPrice: _itemPrice },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                GetListCategoryByMenu(refershCategorySelect, 'poscategoryitem_detail_category', rs.msgId);
                LoadCategoryTable();
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

// Extras View
function cancelAddExtras() {
    $('#poscategoryitem_detail_itemid').val('');
    $('#poscategoryitem_detail_sku_search').val('');
}
function confirmAddExtras() {
    if (isNaN(parseInt($('#poscategoryitem_detail_itemid').val()))) {
        return false;
    }
    var extras = {
        TraderItem: { Id: $('#poscategoryitem_detail_itemid').val(), SKU: $("#poscategoryitem_detail_sku_search").val() },
        CategoryItem: { Id: $('#poscategoryitem_detail_id').val() }
    }
    var url = "/PointOfSaleMenu/SaveExtras?locationid=" + $locationManager;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { extras: extras },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.updateSuccess();
                $('#poscategoryitem_detail_itemid').val('');
                $('#poscategoryitem_detail_sku_search').val('');
                LoadExtrasTable();
                $('.addextras2').hide(); $('#addextras2').show();
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

}
function deletePosVariants(extrasId) {
    var url = "/PointOfSaleMenu/DeleteExtras?id=" + extrasId;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "GET",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                LoadExtrasTable();
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function LoadExtrasTable() {
    $('#item_extras').empty();
    $('#item_extras').load('/PointOfSaleMenu/GetPosCategoryItemPosExtras?idItem=' + $('#poscategoryitem_detail_id').val(), function () {

        $('#extras_table_view').DataTable({
            responsive: true
        });

        $('#extras_table_view').show();
    });
}
function changerow(id) {
    $.LoadingOverlay("show");
    var unit = $('#table_unit_' + id).val();
    var priceBase = $('#table_price_base_' + id).val();
    var url = "/PointOfSaleMenu/UpdateExtrasPosCategoryItem";
    var extras = {
        Id: id,
        Unit: {
            Id: $('#table_unit_' + id).val().split('|')[0],
            Quantity: $('#table_qty_' + id).val()
        },
        Price: {
            GrossPrice: $('#table_price_' + id).val()
        },
        CategoryItem: { Id: $('#poscategoryitem_detail_id').val() }
    }
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { extras: extras },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#table_qty_' + id).val(rs.Object.Quantity);
                $('#table_price_' + id).val(rs.Object.GrossPrice);
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


//Featured image
function ChangeImageItem() {
    $.LoadingOverlay("show");
    readURL('item_categorydetail_imageurl', 'preview');
    UploadMediaS3ClientSide("item_categorydetail_imageurl").then(function (mediaS3Object) {
        if (mediaS3Object.objectKey === "no_image") {

            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            LoadingOverlayEnd();
            return;
        }
        else {
            var url = "/PointOfSaleMenu/UpdatePosCategoryItemImage?idItem=" + $('#poscategoryitem_detail_id').val() + "&imageurl=" + mediaS3Object.objectKey;
            $.ajax({
                url: url,
                type: "post",
                dataType: "json",
                success: function (rs) {

                    if (rs.actionVal === 2) {
                        cleanBookNotification.updateSuccess();
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                },
                error: function (err) {


                    cleanBookNotification.error(err, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        }

    });
}
function readURL(elementFile, elementId) {

    var input = $('#' + elementFile);
    if (input.length > 0 && input[0].files.length > 0) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#' + elementFile + "_" + elementId).attr('src', e.target.result);
        }

        reader.readAsDataURL(input[0].files[0]);
        $("#item_categorydetail_imageurl").css("color", "");
    } else
        $("#item_categorydetail_imageurl").css("color", "transparent");
}

// no variats

//Configure Variants
function PropertyRowChanged(ev, itemId) {
    var result = confirm(_L("ERROR_MSG_348"));
    if (result === false) return false;

    var posVariantProperty = {
        Id: itemId,
        CategoryItem: { Id: $('#poscategoryitem_detail_id').val() },
        VariantOptions: []
    }
    var selects = $('#property_select_' + itemId).val();
    if (selects == null) {
        cleanBookNotification.error("Option can not be null!", "Qbicles");
        return;
    }


    for (var i = 0; i < selects.length; i++) {
        posVariantProperty.VariantOptions.push({ Name: selects[i] });
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSaleMenu/UpdateVariantOptions";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posVariantProperty: posVariantProperty },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $(ev).hide();
                LoadVariantsPropertyTable($('#poscategoryitem_detail_id').val());
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function PropertyRowDelete(id) {
    var result = confirm(_L("ERROR_MSG_348"));
    if (result === false) return false;
    var url = "/PointOfSaleMenu/DeleteProperty?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "GET",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                LoadPropertyTable();
                LoadVariantsPropertyTable($('#poscategoryitem_detail_id').val());
                LoadingOverlayEnd();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function addProperty() {
    $('#posPropertyAdd_options').val(null);
    $('#posPropertyAdd_name').val('');
    $('#posPropertyAdd_options').select2({
        placeholder: 'Please select',
        tags: true
    });
}
function confirmAddProperty() {
    var result = confirm(_L("ERROR_MSG_348"));
    if (result === true) {
        var itemProperty = {
            Name: $('#posPropertyAdd_name').val(),
            CategoryItem: { Id: $('#poscategoryitem_detail_id').val() },
            VariantOptions: []
        }
        var selects = $('#posPropertyAdd_options').val();
        if (selects !== null)
            for (var i = 0; i < selects.length; i++) {
                itemProperty.VariantOptions.push({ Name: selects[i] });
            }
        $.LoadingOverlay("show");
        var url = "/PointOfSaleMenu/UpdateVariantOptions";
        $.ajax({
            url: url,
            type: "post",
            dataType: "json",
            data: { posVariantProperty: itemProperty },
            success: function (rs) {
                if (rs.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                    LoadPropertyTable();
                    LoadVariantsPropertyTable($('#poscategoryitem_detail_id').val());
                    $('#app-trader-pos-property-add').modal('hide');
                    LoadingOverlayEnd();
                } else if (rs.actionVal === 2) {
                    cleanBookNotification.updateSuccess();
                    LoadPropertyTable();
                    LoadingOverlayEnd();
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                }
            },
            error: function (err) {
                cleanBookNotification.error(err, "Qbicles");
                LoadingOverlayEnd();
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }
}

function LoadPropertyTable() {
    $('#variants_properties_table').empty();
    $('#variants_properties_table').load('/PointOfSaleMenu/GetPosCategoryItemPosProperties?idItem=' + $('#poscategoryitem_detail_id').val(), function () {

        $('.table_properties').DataTable({
            responsive: true
        });

        $('.table_properties').show();
    });
}
//Generated Variants
function LoadVariantsPropertyTable(id) {
    //$.LoadingOverlay("show");

    $('#posCategoryItem_variants_table').empty();
    $('#posCategoryItem_variants_table').load('/PointOfSaleMenu/GetVariantsByProperty?categoryItemId=' + id, function () {
        $('.posCategoryItem_variants_table').DataTable({
            responsive: true, "pageLength": 25,
        });
        $('.posCategoryItem_variants_table').show();
        $('.checkbox.toggle input').bootstrapToggle();
        // LoadingOverlayEnd();
    });
}

// Managing Categories
function addCategory() {
    $.LoadingOverlay("show");

    var category = {
        Name: $('#item_category_add_name').val(),
        Menu: { Id: $('#menu_id').val() }
    }

    var url = "/PointOfSaleMenu/AddCategory";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { category: category },
        success: function (rs) {
            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('.newcat').toggle(); $('.addcat').toggle();
                LoadCategoryTable();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                ;
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");

        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function updateCategory(idCategory) {
    $('.table_category_itemdetailt').LoadingOverlay("show");

    var category = {
        Id: idCategory,
        IsVisible: $('#table_category_visible_' + idCategory)[0].checked,
        Name: $('#table_category_name_' + idCategory).val(),
        Menu: { Id: $('#menu_id').val() }
    }

    var url = "/PointOfSaleMenu/UpdateCategory";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { category: category },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        $('.table_category_itemdetailt').LoadingOverlay("hide", true);
    });
}
function deleteCategory(id) {
    var url = "/PointOfSaleMenu/DeleteCategory?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "GET",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                LoadCategoryTable();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function LoadCategoryTable(reload) {
    $('#table_category_categoryitem').load('/PointOfSaleMenu/GetCategoryByMenuId?idMenu=' + $('#menu_id').val(), function () {
        if (!reload) {
            $(".table_category_itemdetailt").DataTable().draw();
        };
        $('.checkbox.toggle input').bootstrapToggle();
        $('.select2.taginput').select2({
            placeholder: 'Please select',
            tags: true
        });
    });

}

function OpenPriceAdvance(type, element) {
    var ajaxUri = '/PointOfSaleMenu/OpenPriceAdvance?menuId=' + $('#menu_id').val() + '&type=' + type;
    AjaxElementShowModal(ajaxUri, element);
}


// Dimensions
function ManagerDimension() {
    $.LoadingOverlay("show");
    $('#app-trader-pos-menu-dimensions').load('/PointOfSale/GetDimensionManager?idMenu=' + $('#menu_id').val(), function () {
        LoadingOverlayEnd();
    });
}
function SaveDimension() {
    var posMenu = {
        Id: $('#menu_id').val(),
        OrderItemDimensions: []
    }
    var lstDimensionId = $('#dimension_select').val();
    for (var i = 0; i < lstDimensionId.length; i++) {
        posMenu.OrderItemDimensions.push({ Id: lstDimensionId[i] });
    }
    $.LoadingOverlay("show");
    var url = "/PointOfSale/SaveDimensions";
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posMenu: posMenu },
        success: function (rs) {
            if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-pos-menu-dimensions').modal('hide');
                $('#lstDimension').text(rs.msgId);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");

            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");

        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function GoToAllProduct(tab) {
    window.location = "/Trader/AppTrader#PointOfSale";
};

function RefreshPrices() {

    $.LoadingOverlay("show");

    $.ajax({
        url: "/PointOfSaleMenu/RefreshPrices",
        type: "post",
        dataType: "json",
        data: { categoryIds: $('#categories-refresh').val() },
        success: function (rs) {
            if (rs.result) {

                DisableButton('RefreshPrice', 'Refresh prices processing', true);
                DisableButton('UpdatePosmenu', 'Update POS Menu', false);

                StartVerifyCatalogStatus($('#menu_id').val(), 'Price');

                var divElement = document.getElementById("item-detail-basic");

                if (divElement.style.display === "block") {
                    $('#item-detail-basic').empty();
                    $('#item-detail-basic').css('display', 'none');

                    $("#li-categories").removeClass("active");
                    $("#li-items").addClass("active");

                    $("#itemlist-categories").removeClass("in active");
                    $("#itemlist-items").addClass("in active");

                    $('#items-list').css('display', 'block');
                }

                $('#pos-menu-refresh-prices-modal').modal('hide');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


function PushPrices() {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/PointOfSaleMenu/PushPricesToPricingPool",
        type: "post",
        dataType: "json",
        data: { categoryIds: $('#categories-push').val() },
        success: function (rs) {
            if (rs.result) {

                //DisableButton('RefreshPrice', 'Refresh prices processing', true);
                //DisableButton('UpdatePosmenu', 'Update POS Menu', false);

                //StartVerifyCatalogStatus($('#menu_id').val(), 'Price');

                //$('#item-detail-basic').empty();
                //$('#item-detail-basic').css('display', 'none');

                //$("#li-categories").removeClass("active");
                //$("#li-items").addClass("active");

                //$("#itemlist-categories").removeClass("in active");
                //$("#itemlist-items").addClass("in active");

                //$('#items-list').css('display', 'block');

                $('#push-prices').modal('hide');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

var $jobId = '';
var $reloadData = false;
function UpdatePosMenuProduct() {


    $.LoadingOverlay("show");

    $.ajax({
        url: "/PointOfSaleMenu/UpdatePosMenuProduct",
        type: "post",
        dataType: "json",
        data: { menuId: $('#menu_id').val() },
        success: function (rs) {
            if (rs.result) {
                LoadingOverlayEnd();
                $('#pos-menu-updadte-pos-menu-modal').modal('hide');

                DisableButton('UpdatePosmenu', 'Update POS Menu processing', true);
                StartVerifyCatalogStatus($('#menu_id').val(), 'Database');

            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function DisableButton(button, text, isSpinner) {
    $('.table-button').addClass('disabled');
    $disableButton = "disabled";
    $('#btn' + button).attr("disabled", "disabled");
    if (isSpinner == true)
        $('#icon-' + button).removeClass('fa-refresh').addClass('fa-spinner fa-spin');
    $('#span' + button).text(text);
    $('#btn' + button).removeAttr("data-target");
};


function EnableButton() {
    $('.table-button').removeClass('disabled');
    $disableButton = "";
    $('#btnUpdatePosmenu').removeAttr("disabled");
    $('#spanUpdatePosmenu').text("Update POS Menu");
    $('#icon-UpdatePosmenu').removeClass('fa-spinner fa-spin').addClass('fa-refresh');
    $('#btnUpdatePosmenu').attr("data-target", "#pos-menu-updadte-pos-menu-modal");


    $('#btnRefreshPrice').removeAttr("disabled");
    $('#spanRefreshPrice').text("Refresh prices");
    $('#icon-RefreshPrice').removeClass('fa-spinner fa-spin').addClass('fa-refresh');
    $('#btnRefreshPrice').attr("data-target", "#pos-menu-refresh-prices-modal");


    $('#catalog-price-table').DataTable().ajax.reload(null, false);
    $('#item-list').DataTable().ajax.reload(null, false);

};


//Verifing on initialisation

var $updateProcessing = false;
var $refreshProcessing = false;



function StartVerifyCatalogStatus(catalogId, processType) {
    $disableButton = "disabled";

    var $intervalId = setInterval(() => {
        var currentLocationId = $("#select2-local-manage-select-container").val();

        if (currentLocationId == undefined || (typeof currentLocationId == "undefined"))
            currentLocationId = 0;

        $.get("/PointOfSaleMenu/VerifyCatalogStatus?catalogId=" + catalogId + "&locationId=" + currentLocationId + "&type=" + processType, function (rs) {
            if (rs.result == false) {
                StopInterval($intervalId);
                EnableButton();
                if (processType == 'Price') {
                    $('#catalog-price-table').DataTable().ajax.reload(null, false);
                    $('#item-list').DataTable().ajax.reload(null, false);
                }
            }
        });
    }, 10000);

};


StopInterval = function (intervalId) {
    //console.log("stop interval id: " + intervalId);
    clearInterval(intervalId);
}


function variantImageChange(e, index) {
    var target = $(e).data('target');
    readImgURL(e, target, index);
    $(target).fadeIn();
    SaveVariantsTable(index);
}
function readImgURL(input, target, index) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
        $("#table_variants_image_" + index).css("color", "");
    }
    else
        $("#table_variants_image_"+index).css("color", "transparent");
}

var price_table = '';
var lst_selected_trader_item_ids = [];
var is_all_price_selected = false;
var is_dataTable_reloading = true;
var lst_ignored_page_ids_on_selecting_all = []

function LoadCatalogPriceDataTable() {
    var $tblCatalogPrice = $('#catalog-price-table');
    price_table = $tblCatalogPrice.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblCatalogPrice.LoadingOverlay("show");
        } else {
            $tblCatalogPrice.LoadingOverlay("hide", true);
        }
    }).DataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        responsive: true,
        preDrawCallback: function () {
            is_dataTable_reloading = true;
        },
        aaSorting : [],
        ajax: {
            "url": "/PointOfSale/GetListPriceItem",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    catalogKey: $("#catalog-key").val(),
                    traderItemSKU: $("#sku-search-price").val(),
                    lstCategoryId: $("#list-category-search-price").val(),
                    categoryItemNameKeySearch: $("#category-item-search-price").val(),
                    variantOrExtraName: $("#variant-extra-name-search-price").val(),
                    locationId: $("#location-manager").val(),
                    taxupdate: $("#tax-update-select").val(),
                    lastcostupdate: $("#last-cost-update-select").val(),
                });
            }
        },
        "columnDefs": [
            {
                //{ "width": "40px", "targets": 1 },
                "width": "80px", "targets": 9
            },
        ],
        columns: [
            {
                "name": "Id",
                "data": "Id",
                "searchable": false,
                "orderable": false,
                "render": function (value, type, row) {
                    var html = '';
                    html += '<input type="checkbox" class="dt-checkboxes" style="margin: 0;" ';
                    html += 'price-id-value="' + row.PriceId + '" item-id="' + row.TraderItemId + '" ';

                    if (row.IsVariant == true) {
                        html += 'variant-id="' + row.Id + '" extra-id="0" ';
                    }
                    else {
                        html += 'variant-id="0" extra-id="' + row.Id + '" ';
                    }

                    html += '"class="price-table-checkbox">';
                    return html;
                },
                "checkboxes": {
                    'selectRow': true,
                    'selectCallback': function (nodes, selected) {
                        var selected_item_id = $(nodes[0]).find('.dt-checkboxes').attr('item-id');
                        var selected_price_id = $(nodes[0]).find('.dt-checkboxes').attr('price-id-value');
                        var selected_variant_id = $(nodes[0]).find('.dt-checkboxes').attr('variant-id');
                        var selected_extra_id = $(nodes[0]).find('.dt-checkboxes').attr('extra-id');
                        var selected_obj = {
                            'item_id': selected_item_id,
                            'price_id': selected_price_id,
                            'variant_id': selected_variant_id,
                            'extra_id': selected_extra_id
                        };

                        var index = lst_selected_trader_item_ids.findIndex(v => v.item_id == selected_item_id && v.price_id == selected_price_id);

                        if (index <= -1 && selected) {
                            lst_selected_trader_item_ids.push(selected_obj);
                        } else if (index >= 0 && !selected) {
                            lst_selected_trader_item_ids.splice(index, 1);
                        }
                        if (!(is_dataTable_reloading && is_all_price_selected)) {
                            togglePriceActionPanel();
                            lst_ignored_page_ids_on_selecting_all.push(price_table.page.info().page);
                        }
                    },
                    'selectAllCallback': function (nodes, selected, indeterminate) {
                        if (indeterminate === false) {
                            var checkboxes = $(".dt-checkboxes");
                            _.forEach(checkboxes, function (checkbox_item) {
                                var selected_item_id = $(checkbox_item).attr('item-id');
                                var selected_price_id = $(checkbox_item).attr('price-id-value');
                                var selected_variant_id = $(checkbox_item).attr('variant-id');
                                var selected_extra_id = $(checkbox_item).attr('extra-id');
                                var selected_obj = {
                                    'item_id': selected_item_id,
                                    'price_id': selected_price_id,
                                    'variant_id': selected_variant_id,
                                    'extra_id': selected_extra_id
                                };

                                var index = lst_selected_trader_item_ids.findIndex(v => v.item_id == selected_item_id && v.price_id == selected_price_id);

                                if (index <= -1 && selected) {
                                    lst_selected_trader_item_ids.push(selected_obj);
                                } else if (index >= 0 && !selected) {
                                    lst_selected_trader_item_ids.splice(index, 1);
                                }
                            })
                            if (!(is_dataTable_reloading && is_all_price_selected)) {
                                togglePriceActionPanel();
                                lst_ignored_page_ids_on_selecting_all.push(price_table.page.info().page);
                            }
                        }
                    }
                }
            },
            {
                name: "ItemSKU",
                data: "ItemSKU",
                orderable: false,
            },
            {
                name: "CategoryId",
                data: "CategoryId",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="javascript:void(0)" onclick="SetCategoryFiltering(' + row.CategoryId + ')">' + row.CategoryName + '</a>';
                    return _html;
                }
            },
            {
                name: "CategoryItemId",
                data: "CategoryItemId",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="javascript:void(0)" onclick="SetCategoryItemFiltering(\'' + row.CategoryItemName + '\')" class="filtering">' + row.CategoryItemName + '</a>';
                    return _html;
                }
            },
            {
                name: "Id",
                data: "Id",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.IsVariant) {
                        _html += '<label class="label label-lg label-primary">' + row.Name + '</label>';
                    } else {
                        _html += '<label class="label label-lg label-info">' + row.Name + '</label>';
                    }
                    return _html;
                }
            },
            {
                name: "AverageAndLastestCost",
                data: "AverageAndLastestCost",
                orderable: false
            },
            {
                name: "MarginLatestCost",
                data: "MarginLatestCost",
                orderable: false,
            },
            {
                name: "MarginAverageCost",
                data: "MarginAverageCost",
                orderable: false,
            },
            {
                name: "NetPrice",
                data: "NetPrice",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<input type="number" class="form-control" onchange="updateValuePrice(' + row.CatalogId + ',' + row.PriceId + ', false, $(this).val())" min="0" value="' + row.NetPrice + '">';
                    return _html;
                }
            },
            {
                name: "ListTaxes",
                data: "ListTaxes",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<ul class="unstyled">';
                    if (row.ListTaxes.length == 0) {
                        _html += '--';
                    } else {
                        _.forEach(row.ListTaxes, function (taxitem) {
                            _html += '<li>' + taxitem.Amount + ' <small> &nbsp; <i>(' + taxitem.TaxName + ')</i></small></li>'
                        });
                    }
                    _html += '</ul>';
                    return _html;
                }
            },
            {
                name: "GrossPrice",
                data: "GrossPrice",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<input type="number" class="form-control" onchange="updateValuePrice(' + row.CatalogId + ',' + row.PriceId + ', true, $(this).val())" min="0" value="' + row.GrossPrice + '">';
                    return _html;
                }
            },
            {
                name: "FlaggedForTaxUpdate",
                data: "FlaggedForTaxUpdate",
                orderable: false,
                render: function (value, type, row) {
                    return buildInputFlagged(row.FlaggedForTaxUpdate);
                }
            },
            {
                name: "FlaggedForLatestCostUpdate",
                data: "FlaggedForLatestCostUpdate",
                orderable: false,
                render: function (value, type, row) {
                    return buildInputFlagged(row.FlaggedForLatestCostUpdate);
                }
            },
        ],
        select: {
            'style': 'multi'
        }
    });

    $('#catalog-price-table').on('draw.dt', function () {
        $(".dt-checkboxes").on("click", function(){
            var select = $(this).is(":checked");
            var selected_item_id = $(this).attr('item-id');
                var selected_price_id = $(this).attr('price-id-value');
                var selected_variant_id = $(this).attr('variant-id');
                var selected_extra_id = $(this).attr('extra-id');
                var selected_obj = {
                    'item_id': selected_item_id,
                    'price_id': selected_price_id,
                    'variant_id': selected_variant_id,
                    'extra_id': selected_extra_id
                };

            var index = lst_selected_trader_item_ids.findIndex(v => v.item_id == selected_item_id && v.price_id == selected_price_id);
            if (index <= -1 && select) {
                lst_selected_trader_item_ids.push(selected_obj);
            } else if (index >= 0 && !select) {
                lst_selected_trader_item_ids.splice(index, 1);
            }
            if (!(is_dataTable_reloading && is_all_price_selected)) {
                togglePriceActionPanel();
                lst_ignored_page_ids_on_selecting_all.push(price_table.page.info().page);
            }
        })
        is_dataTable_reloading = false;
        if (is_all_price_selected && !lst_ignored_page_ids_on_selecting_all.includes(price_table.page.info().page)) {
            setTimeout(function () {
                var is_all_current_prices_checked = $("#catalog-price-table > thead input[type=checkbox]").is(':checked');
                if (!is_all_current_prices_checked) {
                    $("#catalog-price-table > thead input[type=checkbox]").click();
                }
            }, 200);
        }
    })
}

function buildInputFlagged(flagged) {
    if (flagged)
        return "<i class='fa fa-check green'></i>";
    return "<i class='fa fa-remove red'></i>";
}

//Referenced from Trader App => Configuration Master => Setup Power
function updateValuePrice(catalogId, priceId, isInclusiveTax, price) {
    price = price ? price : 0;

    $.post("/PointOfSaleMenu/UpdateCatalogPriceItem", { catalogId: catalogId, id: priceId, isTaxIncluded: isInclusiveTax, value: price }, function (response) {
        if (response.result) {
            $("#catalog-price-table").DataTable().ajax.reload(null, false);
            $('#item-list').DataTable().ajax.reload(null, false);
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
};

function togglePriceActionPanel() {
    var checkedNumber = lst_selected_trader_item_ids.length;
    if (checkedNumber > 0) {
        $('.itemcount').html(checkedNumber);
        $('.alert_matches.projects').addClass('active');
    } else {
        $('.itemcount').html(checkedNumber);
        $('.alert_matches.projects').removeClass('active');
    }
}

function deselectAllPriceCheckBox() {
    var checkAllBtn = $("#check-all-btn");
    is_all_price_selected = false;
    $(checkAllBtn).html('Check all');
    lst_selected_trader_item_ids = [];
    price_table.column(0).checkboxes.deselectAll();

    isCheckAllBtnReady = true;
    togglePriceActionPanel();
}

function bulkApplyMargin() {
    //marginValue, discountValue, isAppliedToAverageCost, unitType, catalogId, isMarginUpdated, lstItemIds

    LoadingOverlay();
    var _url = '/PointOfSaleMenu/AppplyBulkMargin';

    var _isAppliedToAverageCost = true;
    var _unitType = 1;
    if ($('#margin-tab').hasClass('active')) {
        _isAppliedToAverageCost = $("#margin-average-cost").is(':checked');
        _unitType = $('#margin-unit').val();
    }
    else if ($('#discount-tab').hasClass('active')) {
        _isAppliedToAverageCost = $("#discount-average-cost").is(':checked');
        _unitType = $('#discount-unit').val();
    }

    var _data = {
        'marginValue': $("#margin-value").val(),
        'discountValue': $("#discount-value").val(),
        'isAppliedToAverageCost': _isAppliedToAverageCost,
        'unitType': _unitType,
        'catalogKey': $('#catalog-key').val(),
        'isMarginUpdated': $("#margin-tab").hasClass('active'),
        'lstExtraIds': lst_selected_trader_item_ids.map(v => v.extra_id),
        'lstVariantIds': lst_selected_trader_item_ids.map(v => v.variant_id)
    };

    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'data': _data,
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.success("The change is applied successfully.", "Qbicles");
                $('#catalog-price-table').DataTable().ajax.reload(null, false);
                $('#item-list').DataTable().ajax.reload(null, false);
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function toggleMarginTab() {
    var marginTab = $("#margin-tab");
    marginTab.addClass('active');
    $("#discount-tab").removeClass('active');
    $('.tmeth').hide();
    $('#tmargin').show();
}

function toggleDiscountTab() {
    var discountTab = $("#discount-tab");
    discountTab.addClass('active');
    $("#margin-tab").removeClass('active');
    $('.tmeth').show();
    $('#tmargin').hide();
}

function LoadListAffectedPriceModal() {
    $('#pricing-affected').modal('show');
    LoadAffectedPriceDataTable();
}

function LoadAffectedPriceDataTable() {
    var $tblCatalogPrice = $('#catalog-price-affected-table');
    $tblCatalogPrice.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $tblCatalogPrice.LoadingOverlay("show");
        } else {
            $tblCatalogPrice.LoadingOverlay("hide", true);
        }
    }).DataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        ajax: {
            "url": "/PointOfSaleMenu/GetListAffectedPriceItem",
            "type": "POST",
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    catalogKey: $("#catalog-key").val(),
                    lstVariants: lst_selected_trader_item_ids.map(v => v.variant_id),
                    lstExtras: lst_selected_trader_item_ids.map(v => v.extra_id),
                    keySearch: $("#affected-price-keysearch").val(),
                    locationId: $("#location-manager").val()
                });
            }
        },
        columns: [
            {
                name: "ItemSKU",
                data: "ItemSKU",
                orderable: false
            },
            {
                name: "CategoryId",
                data: "CategoryId",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="#">' + row.CategoryName + '</a>';
                    return _html;
                }
            },
            {
                name: "CategoryItemId",
                data: "CategoryItemId",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<a href="#" class="filtering">' + row.CategoryItemName + '</a>';
                    return _html;
                }
            },
            {
                name: "Id",
                data: "Id",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    if (row.IsVariant) {
                        _html += '<label class="label label-lg label-primary">' + row.Name + '</label>';
                    } else {
                        _html += '<label class="label label-lg label-info">' + row.Name + '</label>';
                    }
                    return _html;
                }
            },
            {
                name: "NetPrice",
                data: "NetPrice",
                orderable: false
            },
            {
                name: "ListTaxes",
                data: "ListTaxes",
                orderable: false,
                render: function (value, type, row) {
                    var _html = '';
                    _html += '<ul class="unstyled">';
                    _.forEach(row.ListTaxes, function (taxitem) {
                        _html += '<li>' + taxitem.Amount + ' <small> &nbsp; <i>(' + taxitem.TaxName + ')</i></small></li>'
                    });
                    _html += '</ul>';
                    return _html;
                }
            },
            {
                name: "GrossPrice",
                data: "GrossPrice",
                orderable: false
            }
        ]
    });
}

function SetCategoryFiltering(cateId) {
    $("#list-category-search-price").val([cateId]).multiselect('rebuild').change();
}

function SetCategoryItemFiltering(catItemName) {
    $("#category-item-search-price").val(catItemName).text(catItemName).keyup();
}

var isCheckAllBtnReady = true;
function ToggleCheckAllPriceButton() {
    if (!isCheckAllBtnReady) {
        return;
    }
    LoadingOverlay();
    lst_ignored_page_ids_on_selecting_all = [];
    isCheckAllBtnReady = false;
    var checkAllBtn = $("#check-all-btn");
    if (!is_all_price_selected) {
        // Get all ids of the price with the filtering conditions
        var _url = '/PointOfSaleMenu/GetFilteredPriceIds';
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            data: {
                catalogKey: $("#catalog-key").val(),
                traderItemSKU: $("#sku-search-price").val(),
                lstCategoryId: $("#list-category-search-price").val(),
                categoryItemNameKeySearch: $("#category-item-search-price").val(),
                variantOrExtraName: $("#variant-extra-name-search-price").val()
            },
            success: function (response) {
                if (response.result) {
                    is_all_price_selected = true;
                    $(checkAllBtn).html('Uncheck all');

                    lst_selected_trader_item_ids = response.Object;
                    var is_all_current_prices_checked = $("#catalog-price-table > thead input[type=checkbox]").is(':checked');
                    if (!is_all_current_prices_checked) {
                        $("#catalog-price-table > thead input[type=checkbox]").click();
                    }
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
                isCheckAllBtnReady = true;
                LoadingOverlayEnd();
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
                isCheckAllBtnReady = true;
                LoadingOverlayEnd();
            }
        }).done(function () {
            togglePriceActionPanel();
        })
    } else {
        is_all_price_selected = false;
        $(checkAllBtn).html('Check all');
        lst_selected_trader_item_ids = [];
        price_table.column(0).checkboxes.deselectAll();

        isCheckAllBtnReady = true;
        togglePriceActionPanel();
        LoadingOverlayEnd();
    }
}

//#region Importing new items to Catalogs
var currentListUnselectedProductGroups = [];
var listProductGroupsFromServer = [];
var listCategoryFromServer = [];
var unuseListProductGroups = [];
var isFinishLoading = false;
var domainId = $("#location-id-domain").text();
function getListProductGroup(){
    LoadingOverlay();
    $.get("/PointOfSale/GetTraderGroupByLocation?locaionId="+domainId, function (data) {
        listProductGroupsFromServer = data;
        if (!isFinishLoading) {
            isFinishLoading = true;
        } else {
            LoadingOverlayEnd();
            LoadingOverlayEnd();
        }
    });
}

function getListCategoryByMenuImport() {
    LoadingOverlay();
    var idMenu = $('#menu_id').val();
    var url = "/PointOfSaleMenu/GetCategoryByCatalog?idMenu=" + idMenu;
    $.ajax({
        url: url,
        type: "get",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                listCategoryFromServer = rs.Object.Categories;
                if (!isFinishLoading) {
                    isFinishLoading = true;
                } else {
                    LoadingOverlayEnd();
                    LoadingOverlayEnd();
                }
            } else {
                cleanBookNotification.error("Have an error, detail: " + rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    });
}

function setListProductGroup() {
    var $selector = $(".import-pgroup");
    $selector.empty();
    $selector.append('<option></option>');
    $.each(listProductGroupsFromServer, function (key, val) {
        // $selector.append('<option value="' + val.id + '">' + val.text + ' '+ val.id + '</option>');
        $selector.append('<option value="' + val.id + '">' + val.text + '</option>');
    })
    $selector.select2({
        placeholder: 'Select a Product group'
    });
}

function setListCategoryByMenuImport() {
    $selector = $(".import-cgroup");
    $selector.empty();
    $selector.append('<option></option>');
    $.each(listCategoryFromServer, function (key, val) {
        // $selector.append('<option value="' + val.Id + '">' + val.Name + ' '+ val.Id + '</option>');
        $selector.append('<option value="' + val.Id + '">' + val.Name + '</option>');
    })
    $selector.select2({
        placeholder: 'Select a category'
    });
}

function updateListProductGroup() {
    var listIdItemSelected = [];
    var list = $('.pgrouping')
    //find selected items
    list.each((index, item) => {
        idItemSelected = $(item).find('.import-pgroup').val();
        listIdItemSelected.push(idItemSelected);
    })
    //remove selected items
    currentListUnselectedProductGroups = listProductGroupsFromServer.filter(x => !listIdItemSelected.find(listSelected => (listSelected === x.id)));
    return currentListUnselectedProductGroups;
}

function reupdateTemplateRowProductGroups(listProductGroups, currentSelected) {
    var _html = '';
    _html += '<option value="' + currentSelected.id + '">' + currentSelected.text + '</option>';
    $.each(listProductGroups, function (key, val) {
        _html += '<option value="' + val.id + '">' + val.text + '</option>';
    })
    return _html
}

function templateRowImportMenu(listProductGroups, listCategoryFromServer) {
    var _html = '<tr>';
    _html += '<td class="tweak-select2-custom">';
    _html += '<div class="form-group pgrouping" style="margin: 0;">';
    _html += '<select name="pgroup" class="form-control select2 import-pgroup" style="width: 100%;">';
    _html += '<option></option>';
    $.each(listProductGroups, function (key, val) {
        // _html += '<option value="' + val.id + '">' + val.text + ' '+ val.id + '</option>';
        _html += '<option value="' + val.id + '">' + val.text + '</option>';
    })

    _html += '</select></div></td>';

    _html += '<td class="tweak-select2-custom">';
    _html += '<div class="form-group cgrouping" style="display: block; margin: 0;">';
    _html += '<select name="cgroup" class="form-control select2 import-cgroup" style="width: 100%;">'
   
    _html += '<option></option>';
    $.each(listCategoryFromServer, function (key, val) {
        // _html += '<option value="' + val.Id + '">' + val.Name + ' '+ val.Id + '</option>';
        _html += '<option value="' + val.Id + '">' + val.Name + '</option>';
    })
    _html += '</select></div></td>';

    _html += '<td>';
    _html += '<button class="btn btn-soft" onclick="$(this).closest(\'tr\').remove(); updateProductGroups();checkValidCategorryDropdown();" ><i class="fa fa-trash"></i></button>';
    _html += '</td>';

    _html += '</tr>';
    return _html;
}

function setValueSelectedCategory() {
    var result = $('.import-pgroup').val();
}

//remove all rows
function resetRow(){
    $('.proceedtoimport').hide();
    $('#import-table-product-groups tbody tr').remove();
}

// establish
function initImportProductFunction() {
    getListProductGroup();
    getListCategoryByMenuImport();
    initOtherSetting();
}

function initRow() {
    setListProductGroup();
    setListCategoryByMenuImport();
}

function initNewRow(){
    $('.proceedtoimport').hide();
    $("#import-table-product-groups").append(templateRowImportMenu(updateListProductGroup(),listCategoryFromServer));
    initOtherSetting();
}

//global setting dropdown
function initOtherSetting() {
    $(".import-cgroup:last-child").select2({
        placeholder: 'Select a category'
    }).on('select2:select', function(e){
        checkValidCategorryDropdown();
    });

    $(".import-pgroup:last-child").select2({
        placeholder: 'Select a product group'
    }).on('select2:select', function (e){
        checkValidCategorryDropdown();
        var currentContentProductGroups = e.params.data.text;
        //class Catalogue Category has same row level with Product Group
        currentDropdownCategory = $(this).parents('td').next().find('.import-cgroup');
        // if current Catalog isn't selected, it will set if matching Product Groups
        if (currentDropdownCategory.val() > 0) {

        }
        else {
            // all options of the Catalogue Category
            listOptionsCurrentDropDownCategory = $(this).parents('td').next().find('.import-cgroup option');
            listOptionsCurrentDropDownCategory.each(function (i, items) {
                // current content of Catalogue Category in dropdown
                var currentContentItems = $(items).text();
                var currentValueofItems = $(items).val();
                // if same string -> set option in i-th index.
                compare = currentContentItems.localeCompare(currentContentProductGroups);
                if (compare == 0) {
                    currentDropdownCategory.val(currentValueofItems).trigger('change');
                }
            })
        }

        //
        updateProductGroups();
    });
}

function updateProductGroups() {
    $(".import-pgroup").each((index, item) => {
        var idSelected = $(item).val();
        if (listProductGroupsFromServer.find(item => item.id === idSelected)===undefined){
            var textSelected = '';
        }
        else{
            var textSelected = listProductGroupsFromServer.find(item => item.id === idSelected).text;
        }
        var currentSelected = {
            id: idSelected,
            text: textSelected
        }
        $(item).html('');
        $(item).append(reupdateTemplateRowProductGroups(updateListProductGroup(),currentSelected));
        $(item).select2(
            {
                placeholder: 'Select a product group'
            }
        );
    })
}


// sent to backend
function importNow() {
    //format BE: 
    /*
    typedef data to push : ListProductGroupsWithCategory[] object[], int menuId, int locationId

    object
        public class ListProductGroupsWithCategory
    {
        public int CategoryId { get; set; }
        public int[] ProductGroupsId { get; set; }
    }
    */
    var listProductGroupsWithCategory = [];
    var object = [];
    var listCategoriesId = [];
    var isValidValue = true;
    $('.importing').show();
    $('.proceedtoimport').attr('disabled', true);

    $("#import-table-product-groups tbody tr").each((index, item) => {
        // 1 Category maybe link with 1 or many productgroups
        var categoryId = parseInt($(item).find('.import-cgroup').val());
        var productGroupId = parseInt($(item).find('.import-pgroup').val());
        if(isNaN(categoryId)){
            isValidValue = false;
            $('.importing').hide();
            $('.proceedtoimport').removeAttr('disabled');
        } {
            if (!listCategoriesId.includes(categoryId)) {
                listCategoriesId.push(categoryId);
                object[listCategoriesId.indexOf(categoryId)] = {
                    CategoryId: categoryId,
                    ProductGroupsId: [productGroupId]
                }
            } else {
                object[listCategoriesId.indexOf(categoryId)].ProductGroupsId.push(productGroupId);
            }
        }

    })
    object.forEach(e => listProductGroupsWithCategory.push(e));
    if (isValidValue) {
        $.ajax({
            url: "/PointOfSaleMenu/MappingProductGroupsWithCategories",
            type: "POST",
            dataType: 'JSON',
            data: {
                listProductGroups: listProductGroupsWithCategory,
                menuId: $menuId,
                locationId: $locationManager
            },
            success: function (rs) {
                location.reload();
            },
            error: function (err) {
                console.log(err);
            }
        });
    }else{
        alert("Null category");
    }
}

function checkValidCategorryDropdown(){
    var listProductGroupsWithCategory = [];
    var object = [];
    var listCategoriesId = [];
    var isValidValue = true;
    $("#import-table-product-groups tbody tr").each((index,item) => {
        var categoryId = parseInt($(item).find('.import-cgroup').val());
        var productGroupId = parseInt($(item).find('.import-pgroup').val());
        if(isNaN(categoryId) || isNaN(productGroupId)){
            isValidValue = false;
        }
        {
            if(!listCategoriesId.includes(categoryId)){
                listCategoriesId.push(categoryId);
                object[listCategoriesId.indexOf(categoryId)] = {
                    CategoryId: categoryId,
                    ProductGroupsId: [productGroupId]
                }
            }else{
                object[listCategoriesId.indexOf(categoryId)].ProductGroupsId.push(productGroupId);
            }
        }
        
    })
    object.forEach(e =>listProductGroupsWithCategory.push(e));
    if (isValidValue){
        $('.proceedtoimport').show();
    }else{
        $('.proceedtoimport').hide();
    }
}
//#endregion