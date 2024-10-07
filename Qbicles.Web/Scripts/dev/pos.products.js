
var $menuAction = 0;
var $menuId = 0;
jQuery(document).ready(function () {
    $('.validate-control select').on('change', function (e) {
        if ($('.validate-control select').val() != null && $('.validate-control select').val() !== "") {
            $('.validate-control select + label.error').remove();
        }
    });
    $('#search-name').keyup(delay(function () {
        SearchMenu();
    }, 500));
});
function SearchMenu() {
    var catalogType = $(".trading-items-catalogues-tabs li.active a").attr("catalogtype");
    var $content = $('#pos-menu-list');
    if (catalogType == 1) {
        $content = $("#catalog-distribution-list");
    }
    $content.LoadingOverlay("show");
    $content.empty();

    var _locIds = $('#items-catalogues select[name=locations]').val();
    $content.load("/PointOfSaleMenu/LoadPosMenu", { catalogSearchType: catalogType, locationIds: (_locIds ? _locIds : []), keyword: $('#search-name').val(), salesChannel: $('#search-salechannel').val() }, function (response) {
        $content.LoadingOverlay("hide");
    });
};


resetValidateMenuForm = function () {
    $(".display-locations").hide();
    $("#menu-name").removeClass('error valid');
    $("#pos-menu-form").validate().resetForm();
};

CreateMenu = function () {

    $menuAction = 1;
    $("#menu-name").val('');
    $("#menu-summary").val('');
    $("#menu-salechannel").prop("disabled", false);
    $("#menu-salechannel").val('POS').change();
    $('#report_filters').val('');
    $('#report_filters').select2({ placeholder: 'Please select' });
    $("#menu-modal-title").text('Add a Catalog');
    resetValidateMenuForm();
    $menuId = 0;
    $('#app-trader-pos-menu-modal').modal('show');

    //set value for distribution saleChannel
    var catalogType = getCatalogType();
    if (catalogType == 1) {
        $("#menu-salechannel").val('B2B').change();
        $("#menu-salechannel").attr("disabled", "disabled");

        $(".display-locations").hide();
        $(".display-report-filters").hide();
    } else {
        $(".display-locations").show();
        $(".display-report-filters").show();
    }
};

UpdateMenu = function (id) {
    $menuAction = 2;
    $menuId = id;
    $("#menu-modal-title").text('Edit catalog');
    resetValidateMenuForm();
    EditMenuOpen();
};

CloneMenu = function (id) {
    $menuAction = 3;
    $menuId = id;
    $("#menu-modal-title").text('Clone this catalog');
    resetValidateMenuForm();
    $(".display-locations").show();
    EditMenuOpen(true);
};


function ConfirmDeleteMenu(id) {
    var name = $("#input_" + id).val();
    $("#name-delete").text(name + " catalog");
    $menuId = id;
}

function DeleteMenu() {
    $.LoadingOverlay("show");
    $.ajax({
        type: "delete",
        url: "/PointOfSaleMenu/DeleteMenu",
        data: { id: $menuId },
        dataType: "json",
        success: function (response) {
            if (response.result) {
                $("#pos-menu-" + $menuId).remove();
                $('#confirm-delete').modal('hide');
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

EditMenuOpen = function (isClone) {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/PointOfSaleMenu/GetById?id=" + $menuId,
        type: "post",
        dataType: "json",
        success: function (rs) {
            if (rs.actionVal > 0) {
                $("#menu-name").val(rs.Object.Name);
                var $menusalechannel = $("#menu-salechannel");
                var $location = $('.display-locations select[name=Locations]');
                if (!isClone) {
                    $menusalechannel.prop("disabled", true);
                    $menusalechannel.val(rs.Object.SalesChannel).trigger("change");
                    $location.prop("disabled", true);
                    $location.val(rs.Object.LocationId).trigger("change");
                } else {
                    $menusalechannel.prop("disabled", false);
                    $menusalechannel.val(rs.Object.SalesChannel).trigger("change");
                    $location.prop("disabled", false);
                    $location.change();
                    //Clone distribution Catalog
                    if (rs.Object.Type == 1) {
                        $menusalechannel.prop("disabled", true);
                    }
                }
                $("#menu-summary").val(rs.Object.Description);
                $('#report_filters').val(rs.Object.Dimensions);
                $('#report_filters').select2({ placeholder: 'Please select' });
                $('#app-trader-pos-menu-modal').modal('show');

                if (rs.Object.Type == 1) {
                    $(".display-locations").hide();
                    $(".display-report-filters").hide();
                } else {
                    $(".display-locations").show();
                    $(".display-report-filters").show();
                }
                $('#catalog-type-value').val(rs.Object.Type);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};

function SuccessAction(rs) {
    setTimeout(function () {
        window.location.href = "/PointOfSale/PoSMenu?id=" + rs.msgId;
    }, 100);
}
var $currentMenuId = 0;

function SetMenuId(id) {
    $currentMenuId = id;
}

function UpdatePosMenuProduct(id) {
    $.LoadingOverlay("show");

    $.ajax({
        url: "/PointOfSaleMenu/UpdatePosMenuProduct",
        type: "post",
        dataType: "json",
        data: { menuId: $currentMenuId },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.updateSuccess();
                $('#pos-menu-updadte-pos-menu-modal').modal('hide');
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

function CatalogueQuickAddShow() {
    var _type = 0;
    _type = getCatalogType();
    var _url = "/PointOfSale/CatalogueQuickAddShow?type=" + _type;
    $("#bprofile-menu-quick-add").empty();
    $("#bprofile-menu-quick-add").load(_url);
    $("#bprofile-menu-quick-add").modal("show");
}


function loadTraderGroupsByLocationId(el) {
    var $selector = $('#catalogue-quickadd-form select[name=pgroups]');
    $.get("/PointOfSale/GetTraderGroupByLocation?locaionId=" + $(el).val(), function (data) {
        $selector.multiselect('destroy');
        $selector.empty();
        $.each(data, function (key, val) {
            $selector.append('<option value="' + val.id + '">' + val.text + '</option>');
        })
        $selector.multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true,
            enableCaseInsensitiveFiltering: true
        });
    });
}


//Create new Menu, Update menu, clone menu in Menu list page
function SavePosMenu(isLocationFromLocal) {
    if ($menuAction === 3 && $("#menu-salechannel").val() === "B2B"){
        if(!confirm("B2B catalogs do not contain items with variants or extras. Only items that do not have variants and extras will be cloned")) return 0;
    }
    if (!$("#pos-menu-form").valid())
        return;
    else {
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
                    processSavingCatalogExpertMode(mediaS3Object.objectKey, isLocationFromLocal);
                }
            });
        } else {
            processSavingCatalogExpertMode(null, isLocationFromLocal);
        }
    }
};

function processSavingCatalogExpertMode(imageUri, isLocationFromLocal) {
    var orderItemDimensions = [];
    var catalogType = getCatalogType();
    if (catalogType != 1) {
        var dimensions = $('#report_filters').val();
        if (dimensions == null || dimensions === "") {
            $("#pos-menu-form").validate().showErrors({ filters: "Reporting Filter required" });
            LoadingOverlayEnd();
            return false;
        }

        if (dimensions && dimensions.length > 0) {
            for (var i = 0; i < dimensions.length; i++) {
                orderItemDimensions.push({ Id: dimensions[i] });
            }
        }
    }

    var posMenu = {
        Id: $menuId,
        Name: $("#menu-name").val(),
        SalesChannel: $("#menu-salechannel").val(),
        OrderItemDimensions: orderItemDimensions,
        Description: $("#menu-summary").val(),
        Location: { Id: $(".display-locations select[name=Locations]").val() },
        Type: getCatalogType(),
        Image: imageUri
    };

    var url = "/PointOfSaleMenu/CreatePosMenu";
    if ($menuAction === 2)
        url = "/PointOfSaleMenu/UpdatePosMenu";
    else if ($menuAction === 3)
        url = "/PointOfSaleMenu/ClonePosMenu";

    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posMenu: posMenu, isLocationFromLocal: (isLocationFromLocal ? isLocationFromLocal : false) },
        success: function (rs) {
            if (rs.result) {
                
                $('#app-trader-pos-menu-modal').modal('hide');
                //Create go to detail
                if ($menuAction === 1) {
                    cleanBookNotification.createSuccess();
                    SuccessAction(rs);
                }
                //update search again
                else if ($menuAction === 2) {
                    SearchMenu();
                    cleanBookNotification.updateSuccess();
                }
                //clone wait hangfire and append to page
                else if ($menuAction === 3) {
                    var cataType = getCatalogType();
                    if (cataType != 1) {
                        $("#pos-menu-list #new-menu-div").before(preparingCatalog(rs.msgId, posMenu.Name, rs.msgName));
                    } else {
                        $("#catalog-distribution-list #new-menu-div").before(preparingCatalog(rs.msgId, posMenu.Name, rs.msgName));
                    }

                    StartVerifyCatalogStatus(rs.msgId, cataType);

                }
            } else if (!rs.result && rs.msg) {
                if (rs.actionVal === 8)
                    cleanBookNotification.error(rs.msg, "Qbicles");
                else
                    $("#pos-menu-form").validate().showErrors({ menuname: rs.msg });
            } else if (!rs.result) {
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

//quick mode Catalog
function initQuickAddCatalogueForm() {
    var $quickaddfrm = $("#catalogue-quickadd-form");
    var cataType = getCatalogType();
    if (cataType != 1) {
        $quickaddfrm.validate({
            rules: {
                name: {
                    required: true
                },
                location: {
                    required: true
                },
                pgroups: {
                    required: true
                },
                channel: {
                    required: true
                },
                filters: {
                    required: true
                },
                summary: {
                    required: true
                }
            }
        });
    } else {
        $quickaddfrm.validate({
            rules: {
                name: {
                    required: true
                },
                pgroups: {
                    required: true
                },
                channel: {
                    required: true
                },
                summary: {
                    required: true
                }
            }
        });
    }


    $quickaddfrm.submit(function (e) {
        e.preventDefault();
        if ($quickaddfrm.valid()) {
            // Upload images
            var files = document.getElementById("quick-catalog-img").files;
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("quick-catalog-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        quickAddCatalog(mediaS3Object.objectKey, cataType);
                    }
                });
            } else {
                quickAddCatalog(null, cataType);
            }
        }
    });
}


function quickAddCatalog(ImageUri, cataType) {
    //Get data
    var catalogueItem = {
        Name: $("#catalogue-name").val(),
        Description: $("#catalogue-summary").val(),
        SalesChannel: $("#catalogue-channelid").val(),
        Type: getCatalogType(),
        Image: ImageUri
    };

    var locationId = 0;
    var filterIds = [];
    locationId = $("#catalogue-locationid").val();
    if (cataType != 1) {
        filterIds = $("#catalogue-dimensionIds").val();
    };
    var _url = "/PointOfSaleMenu/SaveCatalogueQuickMode";
    //Submit
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            catalogue: catalogueItem,
            productGroupIds: $("#catalogue-groupIds").val(),
            locationId: locationId,
            filterIds: filterIds
        },
        success: function (response) {
            if (response.result) {
                $("#bprofile-menu-quick-add").modal("hide");
                LoadingOverlayEnd();
                //quick mode wait hangfire and append to page
                if (cataType == 1) {
                    $("#catalog-distribution-list #new-menu-div").before(preparingCatalog(response.msgId, catalogueItem.Name, response.msgName));
                } else if (cataType == 0) {
                    $("#pos-menu-list #new-menu-div").before(preparingCatalog(response.msgId, catalogueItem.Name, response.msgName));
                } else {
                    $("#new-menu-div").before(preparingCatalog(response.msgId, catalogueItem.Name, response.msgName));
                }
                StartVerifyCatalogStatus(response.msgId, cataType);

            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}




function preparingCatalog(catalogId, name, date) {
    return "<article class='col nvinprogress' id='preparing-menu-" + catalogId + "'>"
        + "<span class='last-updated'>Added " + date + "</span>"
        + "<div class='nvloading'>"
        + "<img style='width:200px;height:auto;' src=\"/Content/DesignStyle/img/loading-new.gif\">"
        + "<p>Preparing <strong>" + name + "</strong> catalog</p>"
        + "</div>"
        + "</article>";
};

function NewCatalog2UI(catalogId, cataType, currentLocationId) {
   
    $.get("/PointOfSaleMenu/RenderCatalogUI?catalogId=" + catalogId + "&locationId=" + currentLocationId, function (data) {
        if (data.result) {
            cleanBookNotification.updateSuccess();
            $("#preparing-menu-" + catalogId).remove();
            if (cataType == 1) {
                $('#catalog-distribution-list #new-menu-div').after(data.msg);
            } else if (cataType == 0) {
                $('#pos-menu-list #new-menu-div').after(data.msg);
            } else {
                $('#new-menu-div').after(data.msg);
            }

        }
        else
            cleanBookNotification.error(data.msg, "Qbicles");
    });
}


function StartVerifyCatalogStatus(catalogId, cataType) {

    var $intervalId = setInterval(() => {
        var currentLocationId = $("#select2-local-manage-select-container").val();
        if (currentLocationId == undefined || (typeof currentLocationId == "undefined"))
            currentLocationId = 0;

        $.get("/PointOfSaleMenu/VerifyCatalogStatus?catalogId=" + catalogId + "&locationId=" + currentLocationId, function (rs) {
            if (rs.result == false) {
                StopInterval($intervalId);
                //render UI catalog $catalogId
                NewCatalog2UI(catalogId, cataType, currentLocationId);
            }
        });
    }, 10000);

};


StopInterval = function (intervalId) {
    console.log("stop interval id: " + intervalId);
    clearInterval(intervalId);
}
function getCatalogType() {
    var catalogTypeElement = $(".trading-items-catalogues-tabs > .active > a");
    var catalogType = 0;
    if (catalogTypeElement) {
        catalogType = catalogTypeElement.attr('catalogType');
    }
    if (catalogType == null) {
        catalogType = $('#catalog-type-value').val();
    }
    return catalogType;
}