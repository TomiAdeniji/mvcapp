function ShowFeaturedProductTable() {
    var _url = "/Administration/FeaturedProductDTData";
    var $featuredProductTable = $("#featured-product-table");

    var productTable = $featuredProductTable.on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $featuredProductTable.LoadingOverlay("show");
        } else {
            $featuredProductTable.LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": false,
        "paging": false,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "rowReorder": {
            dataSrc: "DisplayOrder"
        },
        "bPaginate": false,
        "select": true,
        "ajax": {
            "url": _url,
            "type": 'POST'
        },
        "columns": [
            {
                data: "DisplayOrder",
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    return '<i class="fa fa-arrows"></i> &nbsp; <span class="dorder">' + row.DisplayOrder + '</span>';
                }
            },
            {
                data: "ProductId",
                orderable: false,
                render: function (value, type, row) {
                    var htmlString = '';

                    htmlString += '<div class="flex-grid-fluid">';
                    htmlString += '<div class="col">';
                    htmlString += '<br>';
                    htmlString += '<label class="label label-lg label-primary">' + row.ProductTypeLabelName + '</label>';
                    htmlString += '<br><br>';
                    htmlString += '<div class="productimg" style="background-image: url(\'' + row.ItemImageUri + '\'); border-radius: 5px; overflow: hidden; height: 200px;"></div>';

                    if (row.ProductTypeLabelName == "Product feature") {
                        htmlString += '<div class="whom" style="margin: 0; padding: 10px 0 5px 0;">';
                        htmlString += '<div class="avatarc" style="background-image: url(\'' + row.BusinessLogUri + '\');">&nbsp;</div>';
                        htmlString += '<div class="whominfo">';
                        htmlString += '<h5 style="color: #333;">' + row.CatalogItemName + '</h5>';
                        htmlString += '<small>' + row.BusinessName + '</small>';
                        htmlString += '</div>';
                        htmlString += '</div>';
                        htmlString += '</div>';
                    }

                    htmlString += '</div>';

                    return htmlString;
                }
            },
            {
                name: "Link",
                data: "Link",
                orderable: false,
                render: function (value, type, row) {
                    var htmlString = '';

                    if (row.ProductTypeLabelName == "Image") {
                        htmlString += '<a href="' + row.Link + '" target="blank">' + row.Link + '</a>';
                    } else {
                        htmlString += row.Link;
                    }

                    htmlString += '</div>';

                    return htmlString;
                }
            },
            {
                name: "DomainName",
                data: "DomainName",
                orderable: false
            },
            {
                data: "ProductId",
                orderable: false,
                class: 'text-right',
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<button class="btn btn-warning community-button sm w-auto" onclick="ShowAddEditFeaturedProductModal(' + row.ProductId + ')"><i class="fa fa-pencil"></i> &nbsp; Edit</button>';
                    htmlString += ' ';
                    htmlString += '<button class="btn btn-danger community-button sm w-auto" onclick="DeleteProductItem(' + row.ProductId + ')"><i class="fa fa-trash"></i> &nbsp; Delete</button>';
                    return htmlString;
                }
            }
        ]
    });

    // Init row reordering events
    productTable.on('row-reorder', function (e, diff, edit) {
        var updatedList = []

        for (var i = 0, ien = diff.length; i < ien; i++) {
            var rowData = productTable.row(diff[i].node).data();

            var rowProductId = rowData.ProductId;
            var newProductDisplayOrder = diff[i].newData;

            var updatedObj = {
                'ProductId': rowProductId,
                'DisplayOrder': newProductDisplayOrder
            }

            updatedList.push(updatedObj);

            //result += rowData[1] + ' updated to be in position ' +
            //    diff[i].newData + ' (was ' + diff[i].oldData + ')<br>';
        }

        if (updatedList.length > 0) {
            var _url = '/Administration/UpdateFeaturedProductDisplayOrder';
            $.ajax({
                'method': 'POST',
                'dataType': 'JSON',
                'data': {
                    'lstProduct': updatedList
                },
                'url': _url,
                'success': function (response) {
                    if (response.result) {
                        cleanBookNotification.success("The order of the featured product item is updated successfully!", "Qbicles");
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                    $("#featured-product-table").DataTable().ajax.reload();
                },
                'error': function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                    $("#featured-product-table").DataTable().ajax.reload();
                }
            })
        }
    });
}
function ShowFeaturedStoreTable() {

    var _url = "/Administration/FeaturedStoreDTData";
    var $featuredStoreTable = $("#featured-store-table");

    var storeTable = $featuredStoreTable.on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $featuredStoreTable.LoadingOverlay("show");
        } else {
            $featuredStoreTable.LoadingOverlay("hide", true);
        }
    }).DataTable({
        serverSide: true,
        rowReorder: {
            dataSrc: "DisplayOrder"
        },
        searching: false,
        bPaginate: false,
        select: true,
        responsive: true,
        "ajax": {
            "url": _url,
            "type": 'POST'
        },
        "columns": [
            {
                data: "DisplayOrder",
                width: "100px",
                render: function (value, type, row) {
                    return '<i class="fa fa-arrows"></i> &nbsp; <span class="dorder">' + row.DisplayOrder + '</span>';
                }
            },
            {
                data: "StoreId",
                orderable: false,
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="whom" style="margin: 0; padding: 10px 0 5px 0;">';
                    htmlString += '     <div class="avatarc" style="background-image: url(\'' + row.DomainImageUri + '\');">&nbsp;</div>';
                    htmlString += '     <div class="whominfo">';
                    htmlString += '         <h5 style="color: #333;">' + row.DomainName + '</h5>';
                    htmlString += '         <small>' + row.DomainPlanLevelName + '</small>';
                    htmlString += '     </div>';
                    htmlString += '</div>';

                    return htmlString;
                }
            },
            {
                data: "DomainPlanLevelName",
                orderable: false,
                class: 'text-right',
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<button class="btn btn-warning community-button sm w-auto" onclick="ShowAddEditFeaturedStoreModal(' + row.StoreId + ')"><i class="fa fa-pencil"></i> &nbsp; Edit</button>';
                    htmlString += ' ';
                    htmlString += '<button class="btn btn-danger community-button sm w-auto" onclick="DeleteStoreItem(' + row.StoreId + ')"><i class="fa fa-trash"></i> &nbsp; Delete</button>';
                    return htmlString;
                }
            }
        ]
    });

    // Init row reordering events
    storeTable.on('row-reorder', function (e, diff, edit) {
        var updatedList = []

        for (var i = 0, ien = diff.length; i < ien; i++) {
            var rowData = storeTable.row(diff[i].node).data();

            var rowStoreId = rowData.StoreId;
            var newStoreDisplayOrder = diff[i].newData;

            var updatedObj = {
                'StoreId': rowStoreId,
                'DisplayOrder': newStoreDisplayOrder
            }

            updatedList.push(updatedObj);

            //result += rowData[1] + ' updated to be in position ' +
            //    diff[i].newData + ' (was ' + diff[i].oldData + ')<br>';
        }

        if (updatedList.length > 0) {
            var _url = '/Administration/UpdateStoreDisplayOrder';
            $.ajax({
                'method': 'POST',
                'dataType': 'JSON',
                'data': {
                    'lstStores': updatedList
                },
                'url': _url,
                'success': function (response) {
                    if (response.result) {
                        cleanBookNotification.success("The order of the store item is updated successfully!", "Qbicles");
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                    $("#featured-store-table").DataTable().ajax.reload();
                },
                'error': function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                    $("#featured-store-table").DataTable().ajax.reload();
                }
            })
        }
    });

}
function ShowAddEditFeaturedProductModal(productId) {
    var _url = "/Administration/ShowAddEditFeaturedProductModal?productId=" + productId;

    LoadingOverlay();
    $("#admin-featured-product-add").empty();
    $("#admin-featured-product-add").load(_url, function () {
        $("#admin-featured-product-add").modal('show');
        LoadingOverlayEnd();
    })

}
function ShowAddEditFeaturedStoreModal(storeId) {
    var _url = "/Administration/ShowAddEditFeaturedStoreModal?storeId=" + storeId;

    LoadingOverlay();
    $("#admin-featured-store-add").empty();
    $("#admin-featured-store-add").load(_url, function () {
        $("#admin-featured-store-add").modal('show');
        LoadingOverlayEnd();
    })

}
function InitFeaturedProductItemAddEditForm() {
    var $formSaveFeaturedProduct = $("#feat-item-form");
    $formSaveFeaturedProduct.validate({
        rules: {
            domain: {
                required: true
            },
            catalog: {
                required: true
            },
            itemSKU: {
                required: true
            }
        }
    });

    $formSaveFeaturedProduct.submit(function (e) {
        e.preventDefault();
        if ($formSaveFeaturedProduct.valid()) {
            SaveProduct();
        }
        else {
            LoadingOverlayEnd();
            var itemSKU = $("#product-item-sku").val();
            if (itemSKU.length <= 0)
                cleanBookNotification.error("Item SKU is required.", "Qbicles");
        }
    });
}
function InitFeaturedProductImageAddEditForm() {
    var $tableSaveFeaturedProductImage = $("#feat-img-form");
    $tableSaveFeaturedProductImage.validate({
        rules: {
            domain: {
                required: true
            },
            itemSKU: {
                required: true
            }
        }
    });

    $tableSaveFeaturedProductImage.submit(function (e) {
        e.preventDefault();

        var imageId = $("#feature-product-id").val();

        LoadingOverlay();
        if ($tableSaveFeaturedProductImage.valid()) {
            //Process with uploading image
            var files = document.getElementById("img-file-uri").files;

            if (files && files.length > 0) {
                UploadMediaS3ClientSide("img-file-uri").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize,
                        };
                        SaveImage(s3Object);
                    }
                });
            } else {
                if (imageId > 0) {
                    SaveImage(null);
                } else {
                    $tableSaveFeaturedProductImage.validate().showErrors({
                        imgfile: "This field is required."
                    });
                    LoadingOverlayEnd();
                }
            }

            //End
        }
        else {
            LoadingOverlayEnd();
        }
    });
}
function InitFeaturedStoreAddEditForm() {
    var $tableSaveFeaturedStore = $("#feat-store-form");
    $tableSaveFeaturedStore.validate({
        rules: {
            domain: {
                required: true
            }
        }
    });

    $tableSaveFeaturedStore.submit(function (e) {
        e.preventDefault();
        if ($tablelayoutfrm.valid()) {
            SaveFeaturedStore();
        }
        else {
            LoadingOverlayEnd();
        }
    });
}
function SaveProduct() {
    var productDomainKey = $("#product-domain").val();
    var productCatalogId = $("#product-catalog").val();
    var itemSKU = $("#product-item-sku").val();
    var productId = $("#feature-product-id").val();

    var trader_item = {
        SKU: $("#form_inventory_SKU").val(),
        AdditionalInfos: []
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

    var _url = "/Administration/SaveFeaturedProduct";
    var _data = {
        'productId': productId,
        'domainKey': productDomainKey,
        'catalogId': productCatalogId,
        'itemSKU': itemSKU,
        'item': trader_item
    }
    LoadingOverlay();
    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'data': _data,
        'url': _url,
        'success': function (data) {
            if (data.result) {
                cleanBookNotification.success('Save featured product information successfully!', 'Qbicles');
                $("#admin-featured-product-add").modal('hide');
                $("#featured-product-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        LoadingOverlayEnd();
    })
}
function SaveImage(s3UploadModel) {
    var _url = '/Administration/SaveFeaturedImage';
    var _data = {
        'imageId': $("#feature-product-id").val(),
        'domainKey': $("#image-domain").val(),
        'imageUrl': $("#image-hyperlink").val(),
        'uploadModel': s3UploadModel
    };

    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'data': _data,
        'success': function (data) {
            if (data.result) {
                cleanBookNotification.success('Save featured image information successfully!', 'Qbicles');
                $("#admin-featured-product-add").modal('hide');
                $("#featured-product-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, 'Qbicles');
        }
    }).then(function () {
        LoadingOverlayEnd();
    })
}
function SaveFeaturedStore() {
    var domainKey = $("#featured-store-domain").val();
    var _data = {
        'domainKey': domainKey,
        'featuredStoredId': $("#featured-store-id").val()
    };
    var _url = "/Administration/SaveFeaturedStore";
    LoadingOverlay();
    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'data': _data,
        'url': _url,
        'success': function (data) {
            if (data.result) {
                cleanBookNotification.success("Save Store information successfully!", "Qbicles");
                $("#admin-featured-store-add").modal('hide');
                $("#featured-store-table").DataTable().ajax.reload();
            }
            else {
                cleanBookNotification.error(data.msg, "Qbicles");
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        LoadingOverlayEnd();
    })

}
function UpdateCatalogList(domainElmId, selectorElm) {
    var domainElm = $("#" + domainElmId);
    var data = {
        'domainKey': (domainElm).val()
    }
    var _url = '/Administration/GetListCatalogByDomain';

    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'data': _data,
        'success': function (data) {
            selectorElm.select2('destroy').empty;
            $.each(data, function (key, val) {
                $selector.append('<option value="' + val.Id + '">' + val.Text + '</option>');
            })
            selectorElm.select2();
        },
        'error': function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_570"), "Qbicles");
        }
    })
}


function UpdateFeaturedProductSelector() {
    var domainKey = $("#product-domain").val();
    var $selector = $("#product-catalog");
    var _data = {
        'domainKey': domainKey
    }
    var _url = '/Administration/GetListCatalogByDomain';
    $(".itemsku").fadeOut();
    if (domainKey !== '') {
        $.ajax({
            'method': 'POST',
            'dataType': 'JSON',
            'url': _url,
            'data': _data,
            'success': function (data) {
                $selector.select2('destroy').empty();
                $selector.append('<option value=""></option>');
                $.each(data, function (key, val) {
                    $selector.append('<option value="' + val.Id + '">' + val.Text + '</option>');
                })
                $selector.select2();
                $('.catalogue').fadeIn();
            },
            'error': function (err) {
                cleanBookNotification.error(_L("ERROR_MSG_5"), "Qbicles");
            }
        })
    }
}

function UpdateFeaturedImageSelector() {

}

function ShowFeaturedProductItemFilteringModal() {
    var catalogId = $("#product-catalog").val();
    var _url = '/Administration/ShowProductItemFilteringModal?catalogId=' + catalogId;
    var skuKeySearch = $("#product-item-sku").val();

    $("#app-trader-pos-itemlist").load(_url, { keySearch: skuKeySearch }, function () {
        LoadAdminDataTraderItemInventory();

        LoadAdminDataTraderItemNonInventory();

        // Select2
        $("#app-trader-pos-itemlist").find('.select2').select2();

        // Assign the events to the input and selector
        // The Inventory tab
        $("#inventory-key-search").keyup(delay(function () {
            $('#inventory-trader-item-table').DataTable().ajax.reload();
        }, 500));

        $("#inventory-product-group").on('change', function () {
            $('#inventory-trader-item-table').DataTable().ajax.reload();
        })

        // The Non Inventory tab
        $("#non-inventory-key-search").keyup(delay(function () {
            $('#non-inventory-trader-item-table').DataTable().ajax.reload();
        }, 500));

        $("#non-inventory-product-group").on('change', function () {
            $('#non-inventory-trader-item-table').DataTable().ajax.reload();
        })
    });
}

function LoadAdminDataTraderItemNonInventory() {
    $("#non-inventory-trader-item-table").on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $("#non-inventory-trader-item-table").LoadingOverlay("show");
        } else {
            $("#non-inventory-trader-item-table").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/Administration/FilterTraderItemServerSide',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    search: $("#non-inventory-key-search").val(),
                    catalogId: $("#product-catalog").val(),
                    nonInventory: true,
                    productGroupId: $("#non-inventory-product-group").val()
                });
            }
        },
        "columns": [
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    return "<div class='table-avatar mini' style='background-image: url(\"" + $("#api-base-url").val() + row.ImageUri + "\");'></div>";
                }
            },
            {
                name: "Name",
                data: "Name",
                orderable: true,
            },
            {
                name: "SKU",
                data: "SKU",
                orderable: true,
            },
            {
                name: "Group",
                data: "Group",
                orderable: true
            },
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    //1- Add new item, 2 - add extra, 3 - add 
                    return "<button class='btn btn-success' data-dismiss='modal' onclick='selectItemToFeaturedProductAddingForm(this)' itemsku='" + row.SKU + "' itemid='" + row.Id + "' ><i class='fa fa-check'></i></button>";
                }
            }
        ]
    });
}

function LoadAdminDataTraderItemInventory() {
    $("#inventory-trader-item-table").on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $("#inventory-trader-item-table").LoadingOverlay("show");
        } else {
            $("#inventory-trader-item-table").LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": false,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/Administration/FilterTraderItemServerSide',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    search: $("#inventory-key-search").val(),
                    catalogId: $("#product-catalog").val(),
                    nonInventory: false,
                    productGroupId: $("#inventory-product-group").val()
                });
            }
        },
        "columns": [
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    return "<div class='table-avatar mini' style='background-image: url(\"" + $("#api-base-url").val() + row.ImageUri + "\");'></div>";
                }
            },
            {
                name: "Name",
                data: "Name",
                orderable: true,
            },
            {
                name: "SKU",
                data: "SKU",
                orderable: true,
            },
            {
                name: "Group",
                data: "Group",
                orderable: true
            },
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    //1- Add new item, 2 - add extra, 3 - add 
                    return "<button class='btn btn-success' data-dismiss='modal' onclick='selectItemToFeaturedProductAddingForm(this)' itemsku='" + row.SKU + "' itemid='" + row.Id + "' ><i class='fa fa-check'></i></button>";
                }
            }
        ]
    });
}

function selectItemToFeaturedProductAddingForm(elm) {
    $(".featured-item-div").LoadingOverlay("show");

    var itemSku = $(elm).attr('itemsku');
    $("#product-item-sku").val(itemSku);

    //show alert missing brand/tags
    $(".shoutout-brand-tag").empty();
    $('.shoutout-brand-tag').show();

    $(".shoutout-brand-tag").load("/Administration/ShowProductItemBrandTagModal", { itemId: $(elm).attr('itemid') }, function () {
        $('.shoutout-brand-tag').show();
        $(".featured-item-div").LoadingOverlay("hide", true);
    });

}

function DeleteStoreItem(storeId) {
    if (confirm("Are you sure you want to delete this feature?")) {
        var _url = '/Administration/DeletedFeaturedStore?storeId=' + storeId;
        $.ajax({
            'method': 'POST',
            'dataType': 'JSON',
            'url': _url,
            'success': function (response) {
                if (response.result) {
                    cleanBookNotification.success('Remove the Featured Store successfully!', 'Qbicles');
                    $("#featured-store-table").DataTable().ajax.reload();
                } else {
                    cleanBookNotification.error(response.msg, 'Qbicles');
                }
            },
            'error': function (err) {
                cleanBookNotification.error(err.msg, 'Qbicles');
            }
        })
    }
}

function DeleteProductItem(productId) {
    if (confirm("Are you sure you want to delete this feature?")) {
        var _url = '/Administration/DeleteFeaturedProduct?productId=' + productId;
        $.ajax({
            'method': 'POST',
            'dataType': 'JSON',
            'url': _url,
            'success': function (response) {
                if (response.result) {
                    cleanBookNotification.success('Remove the Featured Product successfully!', 'Qbicles');
                    $("#featured-product-table").DataTable().ajax.reload();
                } else {
                    cleanBookNotification.error(response.msg, 'Qbicles');
                }
            },
            'error': function (err) {
                cleanBookNotification.error(err.msg, 'Qbicles');
            }
        })
    }
}


