var wto;
//Init DataTable Listing Type
initPropertyTypeTable = function () {
    var dataTable = $("#propertyType-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#propertyType-table').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#propertyType-table').LoadingOverlay("hide", true);
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
                "url": '/HighlightSetup/LoadPropertyType',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "propertyNameSearch": $("#keysearch").val(),
                        "creatorId": $("#propertycreator").val()
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "CreatorName",
                    orderable: true
                },
                {
                    data: "Options",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<button class="btn btn-warning" onclick="ShowAddEditPropertyTypeModal(' + row.Id + ')"><i class="fa fa-pencil"></i></button> ';
                        htmlStr += '<button class="btn btn-danger" onclick="DeletePropertyType(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
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

//Init DataTable Listing Extras
initPropertyExtraTable = function () {
    var dataTable = $("#propertyExtra-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#propertyExtra-table').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#propertyExtra-table').LoadingOverlay("hide", true);
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
                "url": '/HighlightSetup/LoadPropertyExtra',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "propertyNameSearch": $("#keysearch").val(),
                        "creatorId": $("#propertycreator").val()
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "CreatorName",
                    orderable: true
                },
                {
                    data: "Name",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<button class="btn btn-warning" onclick="ShowAddEditPropertyExtraModal(' + row.Id + ')"><i class="fa fa-pencil"></i></button> ';
                        htmlStr += '<button class="btn btn-danger" onclick="DeletePropertyExtra(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                        return htmlStr;
                    }
                }
            ]
        });
}

//Init DataTable Countries
initCountriesTable = function () {
    $("#countries-list-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#countries-list-table').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#countries-list-table').LoadingOverlay("hide", true);
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
                "url": '/HighlightSetup/LoadCountries',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keySearch": $("#groupname-search").val(),
                    });
                }
            },
            "columns": [
                {
                    data: "CommonName",
                    orderable: true
                }
            ]
        });
}

//Init DataTable Listing Location
initListingLocationTable = function () {
    $("#listing-location-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#listing-location-table').LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $('#listing-location-table').LoadingOverlay("hide", true);
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
                "url": '/HighlightSetup/LoadHLLocations',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationNameString": $("#location-search-key").val(),
                        "countryName": $("#locationgroup").val().trim()
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "Group",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return JSON.stringify(row.GroupListRenderString).slice(1, -1);
                    }
                },
                {
                    data: "Options",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<button class="btn btn-warning" onclick="ShowAddEditLocationModal(' + row.Id + ')"><i class="fa fa-pencil"></i></button> ';
                        htmlStr += '<button class="btn btn-danger" onclick="DeleteListingLocation(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                        return htmlStr;
                    }
                }
            ],
            "drawCallback": function (settings) {
                $(".locationselect2").select2();
            }
        });
}

//Init DataTable Business Categories
initBusinessCatalogiesTable = function () {
    $("#tblBusinessCategories")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $('#tblBusinessCategories').LoadingOverlay("show");
            } else {
                $('#tblBusinessCategories').LoadingOverlay("hide", true);
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
            "ajax": {
                "url": '/HighlightSetup/LoadBusinessCategories',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "keyword": $("#business-categories input[name=Search]").val()
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<button class="btn btn-warning" onclick="ShowAddEditBusinessCategoryModal(' + row.Id + ')"><i class="fa fa-pencil"></i></button> ';
                        htmlStr += '<button class="btn btn-danger" onclick="DeleteBusinessCategory(' + row.Id + ')"><i class="fa fa-trash"></i></button>';
                        return htmlStr;
                    }
                }
            ]
        });
}

function LoadPropertyTypeContent() {
    $('.subnavr a').removeClass('active');
    $('#propertytype-tab').addClass('active');

    var _url = "/HighlightSetup/ListPropertyTypePartialShow";
    if ($("#tab-lists").is(':visible')) {
        $("#tab-lists").LoadingOverlay('show');
    }
    $("#tab-lists").empty();
    $("#tab-lists").load(_url);
    $("#tab-lists").LoadingOverlay('hide');
}

function LoadPropertyExtraContent() {
    $('.subnavr a').removeClass('active');
    $('#propertyextra-tab').addClass('active');

    var _url = "/HighlightSetup/ListPropertyExtraPartialShow";
    if ($("#tab-lists").is(':visible')) {
        $("#tab-lists").LoadingOverlay('show');
    }
    $("#tab-lists").empty();
    $("#tab-lists").load(_url);
    $("#tab-lists").LoadingOverlay('hide');
}

function LoadCountriesListTabContent() {
    $('.subnavr a').removeClass('active');
    $('#countries-tab').addClass('active');

    var _url = "/HighlightSetup/ListCountriesPartial";
    if ($("#countries-location").is(':visible')) {
        $("#countries-location").LoadingOverlay('show');
    }
    $("#countries-location").empty();
    $("#countries-location").load(_url);
    $("#countries-location").LoadingOverlay('hide');
}

function LoadHLLocationContent() {
    $('.subnavr a').removeClass('active');
    $('#locationlist-tab').addClass('active');

    var _url = "/HighlightSetup/ListHLLocationPartialShow";
    if ($("#countries-location").is(':visible')) {
        $("#countries-location").LoadingOverlay('show');
    }
    $("#countries-location").empty();
    $("#countries-location").load(_url);
    $("#countries-location").LoadingOverlay('hide');
}


//Showing Modal
function ShowAddEditPropertyTypeModal(propertyTypeId) {
    var ajaxUri = '/HighlightSetup/AddEditPropertyTypeModalShow?propertyTypeId=' + propertyTypeId;
    AjaxElementShowModal(ajaxUri, 'add-edit-propertytype_modal');
}

function ShowAddEditPropertyExtraModal(propertyExtraId) {
    var ajaxUri = '/HighlightSetup/AddEditPropertyExtraModalShow?propertyExtraId=' + propertyExtraId;
    AjaxElementShowModal(ajaxUri, 'add-edit-propertyextra_modal');
}

function ShowAddEditLocationGroupModal(groupId) {
    var ajaxUri = "/HighlightSetup/AddEditLocationGroupModalShow?groupId=" + groupId;
    AjaxElementShowModal(ajaxUri, 'add-edit-locationgroup_modal');
}

function ShowAddEditLocationModal(locationId) {
    var ajaxUri = "/HighlightSetup/AddEditLocationsModalShow?locationId=" + locationId;
    AjaxElementShowModal(ajaxUri, 'add-edit-location_modal');
}

function ShowAddEditBusinessCategoryModal(businessCategoryId) {
    var ajaxUri = '/HighlightSetup/AddEditBusinessCategoryModalShow?id=' + businessCategoryId;
    AjaxElementShowModal(ajaxUri, 'add-edit-businesscategory_modal');
}

//Business Rules
function initPropertyTypeFormToSave() {
    var $frmaddproperty = $("#form-propertytype");
    $frmaddproperty.validate({
        rule: {
            Name: {
                required: true
            }
        }
    });

    $frmaddproperty.submit(function (e) {
        e.preventDefault();
        if ($frmaddproperty.valid()) {
            var propertyTypeId = $("#propertyTypeId").val();
            var propertyTypeModel = {
                Id: propertyTypeId,
                Name: $("#property-type-name").val()
            };
            var _url = '/HighlightSetup/AddEditPropertyType';
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    propertyType: propertyTypeModel
                },
                success: function (response) {
                    if (response.result) {
                        if (Number(propertyTypeId) > 0) {
                            cleanBookNotification.updateSuccess();
                        } else {
                            cleanBookNotification.success("Added PropertyType successfully.", "Qbicles");
                        }
                        $("#propertyType-table").DataTable().ajax.reload();
                        $("#add-edit-propertytype_modal").modal('toggle');
                    } else {
                        $frmaddproperty.validate().showErrors({ "Name": response.msg });
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            })
        }
    });
}

function DeletePropertyType(propertyTypeId) {
    var _url = "/HighlightSetup/DeletePropertyType?propertyTypeId=" + propertyTypeId;
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                $("#propertyType-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function initPropertyExtraFormToSave() {
    var $frmaddproperty = $("#form-propertyextra");
    $frmaddproperty.validate({
        rule: {
            Name: {
                required: true
            }
        }
    });

    $frmaddproperty.submit(function (e) {
        e.preventDefault();
        if ($frmaddproperty.valid()) {
            var propertyExtraId = $("#propertyExtraId").val();
            var propertyExtraModel = {
                Id: propertyExtraId,
                Name: $("#property-extra-name").val()
            };
            var _url = '/HighlightSetup/AddEditPropertyExtra';
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    propertyExtra: propertyExtraModel
                },
                success: function (response) {
                    if (response.result) {
                        if (Number(propertyExtraId) > 0) {
                            cleanBookNotification.updateSuccess();
                        } else {
                            cleanBookNotification.success("Added PropertyExtra successfully.", "Qbicles");
                        }
                        $("#propertyExtra-table").DataTable().ajax.reload();
                        $("#add-edit-propertyextra_modal").modal('toggle');
                    } else {
                        $frmaddproperty.validate().showErrors({ "Name": response.msg });
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            })
        }
    });
}

function DeletePropertyExtra(propertyExtraId) {
    var _url = "/HighlightSetup/DeletePropertyExtra?propertyExtraId=" + propertyExtraId;
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                $("#propertyExtra-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function initLocationGroupFormToSave() {
    var $frmaddlocationgroup = $("#form-locationgroup");
    $frmaddlocationgroup.validate({
        rule: {
            Name: {
                required: true
            }
        }
    });

    $frmaddlocationgroup.submit(function (e) {
        e.preventDefault();
        if ($frmaddlocationgroup.valid()) {
            var groupId = $("#locationGroupId").val();
            var _locationGroup = {
                Id: groupId,
                Name: $("#location-group-name").val()
            };
            var _url = '/HighlightSetup/AddEditLocationGroup';
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: { locationGroup: _locationGroup },
                success: function (response) {
                    if (response.result) {
                        if (Number(groupId) > 0) {
                            cleanBookNotification.updateSuccess();
                        } else {
                            cleanBookNotification.success("Add Location Group successfully.", "Qbicles");
                        }
                        $("#countries-list-table").DataTable().ajax.reload();
                        $("#add-edit-locationgroup_modal").modal("toggle");
                    } else {
                        $frmaddlocationgroup.validate().showErrors({ "Name": response.msg });
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        }
    })
}

function DeleteLocationGroup(groupId) {
    var _url = '/HighlightSetup/DeleteLocationGroup?groupId=' + groupId;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                $("#countries-list-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
}

function initListingLocationFormToSave() {
    var $frmaddlistinglocation = $("#form-listinglocation");
    $frmaddlistinglocation.validate({
        rule: {
            Name: {
                required: true
            },
            locationgroup: {
                required: true
            }
        }
    });

    $frmaddlistinglocation.submit(function (e) {
        e.preventDefault();
        if ($frmaddlistinglocation.valid()) {
            var locationId = $("#locationId").val();
            var listingLocatioModel = {
                Id: locationId,
                Name: $("#location-name").val()
            };
            var countryName = $("#location-group").val();
            var _url = '/HighlightSetup/AddEditHLLocation';
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    location: listingLocatioModel,
                    countryName: countryName
                },
                success: function (response) {
                    if (response.result) {
                        if (Number(locationId) > 0) {
                            cleanBookNotification.updateSuccess();
                        } else {
                            cleanBookNotification.success("Added ListingLocation successfully.", "Qbicles");
                        }
                        $("#listing-location-table").DataTable().ajax.reload();
                        $("#add-edit-location_modal").modal('toggle');
                    } else {
                        $frmaddlistinglocation.validate().showErrors({ "Name": response.msg });
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            })
        }
    });
}

function UpdateLocationCountry(ev) {
    var locationId = $(ev).attr("locationIdAttr");
    var countryName = $(ev).val();

    var _url = "/HighlightSetup/UpdateLocationCountry?locationId=" + locationId + "&countryName=" + countryName;
    LoadingOverlay();

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
                $("#listing-location-table").DataTable().ajax.reload();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            $("#listing-location-table").DataTable().ajax.reload();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function DeleteListingLocation(locationId) {
    var _url = "/HighlightSetup/DeleteListingLocation?locationId=" + locationId;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                $("#listing-location-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    })
}

function initBusinessCategoryFormToSave() {
    var $frmabusinesscategory = $("#form-businesscategory");
    $frmabusinesscategory.validate({
        rule: {
            Name: {
                required: true
            }
        }
    });

    $frmabusinesscategory.submit(function (e) {
        e.preventDefault();
        if ($frmabusinesscategory.valid()) {
            var businessCategoryId = $("#businessCategoryId").val();
            var businessCategoryModel = {
                Id: businessCategoryId,
                Name: $("#form-businesscategory input[name=Name]").val()
            };
            var _url = '/HighlightSetup/AddEditBusinessCategory';
            LoadingOverlay();
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    category: businessCategoryModel
                },
                success: function (response) {
                    if (response.result) {
                        if (businessCategoryId > 0) {
                            cleanBookNotification.updateSuccess();
                        } else {
                            cleanBookNotification.success(_L('ERROR_MSG_657'), "Qbicles");
                        }
                        reloadTableBusinessCategory();
                        $("#add-edit-businesscategory_modal").modal('hide');
                    } else {
                        $frmabusinesscategory.validate().showErrors({ "Name": response.msg });
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            })
        }
    });
}

function DeleteBusinessCategory(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                var _url = "/HighlightSetup/DeleteBusinessCategory?id=" + id;
                LoadingOverlay();
                $.ajax({
                    method: 'POST',
                    dataType: 'JSON',
                    url: _url,
                    success: function (response) {
                        if (response.result) {
                            cleanBookNotification.removeSuccess();
                            reloadTableBusinessCategory();
                        } else {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    },
                    error: function (err) {
                        cleanBookNotification.error(err.msg, "Qbicles");
                    }
                }).always(function () {
                    LoadingOverlayEnd();
                });
                return;
            }
        }
    });

}

function businessCatActive(el) {
    $(el).next().children('a').addClass('active');
    reloadTableBusinessCategory();
}
function reloadTableBusinessCategory() {
    if ($.fn.DataTable.isDataTable("#tblBusinessCategories"))
        $("#tblBusinessCategories").DataTable().ajax.reload();
    else {
        wto = setInterval(function () {
            if ($.fn.DataTable.isDataTable("#tblBusinessCategories")) {
                $("#tblBusinessCategories").DataTable().ajax.reload();
                clearInterval(wto);
            }
        }, 1000);
    }
}
$(document).ready(function () {
    initBusinessCatalogiesTable();
});


function SetProductDefaultActiveTab(elm) {
    $(elm).next().children('a')[0].click();
}

function showItemBrandsTab() {
    var _url = '/Administration/ShowBrandsTab';

    $('.subnavr a').removeClass('active');
    $('#item-brands-tab').addClass('active');

    if ($("#products-specifics").is(':visible')) {
        $("#products-specifics").LoadingOverlay('show');
    }
    $("#products-specifics").empty();
    $("#products-specifics").load(_url, function () {
        $("#brand-domain-dropdown").select2();
        initItemBrandsDT();

        // Init events for filtering elements
        $("#brand-domain-dropdown").on('change', function () {
            $("#item-brand-table").DataTable().ajax.reload();
        })
        $('#brand-text-search').keyup(delay(function () {
            $("#item-brand-table").DataTable().ajax.reload();
        }, 500));
    });
    $("#products-specifics").LoadingOverlay('hide');
}

function showItemProductTagsTab() {
    var _url = '/Administration/ShowProductTagsTab/';

    $('.subnavr a').removeClass('active');
    $('#item-product-tags-tab').addClass('active');

    if ($("#products-specifics").is(':visible')) {
        $("#products-specifics").LoadingOverlay('show');
    }
    $("#products-specifics").empty();
    $("#products-specifics").load(_url, function () {
        $("#product-tag-domain-dropdown").select2();
        initItemProductTagsDT();

        // Init events for filtering elements
        $("#product-tag-domain-dropdown").on('change', function () {
            $("#item-product-tag-table").DataTable().ajax.reload();
        })
        $('#product-tag-key-search').keyup(delay(function () {
            $("#item-product-tag-table").DataTable().ajax.reload();
        }, 500));
    });
    $("#products-specifics").LoadingOverlay('hide');
}

function initItemBrandsDT() {
    var keySearchElm = $("#brand-text-search");
    var domainElm = $("#brand-domain-dropdown");

    $("#item-brand-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#item-brand-table').LoadingOverlay("show");
        } else {
            $('#item-brand-table').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[5, 10, 20, 50, 100], [5, 10, 20, 50, 100]],
            "pageLength": 10,
            "ajax": {
                "url": '/Administration/GetAdditionalInforDTData',
                "type": 'POST',
                "data": function (d) {
                    keySearch = "";
                    domainKey = "";

                    if (keySearchElm.length > 0)
                        keySearch = keySearchElm.val();
                    if (domainElm.length > 0)
                        domainKey = domainElm.val();

                    return $.extend({}, d, {
                        "keySearch": keySearch,
                        "domainKey": domainKey,
                        "type": 1
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "DomainName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return '<a href="/Commerce/PublishBusinessProfile?id=' + row.DomainId +'&isDomainId=true" target="_blank">' + row.DomainName + '</a>';
                    }
                },
                {
                    data: "CreatedDate",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return row.CreatedDateStr + " " + "<a href='/Community/UserProfilePage?uId=" + row.UserId +"' target='_blank'>" + row.CreatedByName + '</a>';
                    }
                },
                {
                    data: "AssociatedNumber",
                    orderable: false
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = '<button class="btn btn-warning" onclick="openAddEditBrandModal(' + row.Id + ', \'' + row.Name + '\', ' + row.DomainId + ')"><i class="fa fa-pencil"></i></button> ';

                        if (row.AssociatedNumber != 0) return htmlString;
                        htmlString += '<button class="btn btn-danger" onclick="adminDeleteAdditional(' + row.Id + ', \'' + row.Name + '\', 1)"><i class="fa fa-trash"></i></button>';

                        return htmlString;
                    }
                }
            ]
        });
}

function initItemProductTagsDT() {
    var keySearchElm = $("#product-tag-key-search");
    var domainElm = $("#product-tag-domain-dropdown");

    $("#item-product-tag-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#item-product-tag-table').LoadingOverlay("show");
        } else {
            $('#item-product-tag-table').LoadingOverlay("hide", true);
        }
    })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[5, 10, 20, 50, 100], [5, 10, 20, 50, 100]],
            "pageLength": 10,
            "ajax": {
                "url": '/Administration/GetAdditionalInforDTData',
                "type": 'POST',
                "data": function (d) {
                    keySearch = "";
                    domainKey = "";

                    if (keySearchElm.length > 0)
                        keySearch = keySearchElm.val();
                    if (domainElm.length > 0)
                        domainKey = domainElm.val();

                    return $.extend({}, d, {
                        "keySearch": keySearch,
                        "domainKey": domainKey,
                        "type": 4
                    });
                }
            },
            "columns": [
                {
                    data: "Name",
                    orderable: true
                },
                {
                    data: "DomainName",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return '<a href="/Commerce/PublishBusinessProfile?id=' + row.DomainId + '&isDomainId=true" target="_blank">' + row.DomainName + '</a>';
                    }
                },
                {
                    data: "CreatedDate",
                    orderable: true,
                    render: function (data, type, row, meta) {
                        return row.CreatedDateStr + " " + "<a href='/Community/UserProfilePage?uId=" + row.UserId + "' target='_blank'>" + row.CreatedByName + '</a>';
                    }
                },
                {
                    data: "AssociatedNumber",
                    orderable: false
                },
                {
                    data: "Id",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlString = '<button class="btn btn-warning" onclick="openAddEditProductTagModal(' + row.Id + ', \'' + row.Name + '\', ' + row.DomainId + ')"><i class="fa fa-pencil"></i></button> ';

                        if (row.AssociatedNumber != 0) return htmlString;
                        htmlString += '<button class="btn btn-danger" onclick="adminDeleteAdditional(' + row.Id + ', \'' + row.Name + '\', 4)" ><i class="fa fa-trash"></i></button>';

                        return htmlString;
                    }
                }
            ]
        });
}



function openAddEditBrandModal(itemId, itemName, domainId) {

    resetForm("add-edit-brand");
    $("#editting-brand-id").val(itemId);
    $("#editting-brand-name-hiden").val(itemName);
    $("#editting-brand-name").val(itemName);
    $("#save-brand-modal-domain-selector").val(domainId).change();
    if (itemId > 0)
        $("#brand-modal-title").text('Edit Brand');
    else {
        $("#brand-modal-title").text('Add a Brand');
        $('#save-brand-modal-domain-selector option:eq(0)').prop('selected', true).change();
    }
    $('#save-brand-modal-domain-selector').not('.multi-select').select2();
    $("#app-trader-resources-brand-add").modal('show');
}

function openAddEditProductTagModal(itemId, itemName, domainId) {

    resetForm("add-edit-product-tag");
    $("#editting-product-tag-id").val(itemId);
    $("#editting-product-tag-name").val(itemName);
    $("#editting-product-tag-name-hiden").val(itemName);
    $("#save-product-tag-modal-domain-selector").val(domainId).change();
    if (itemId > 0)
        $("#product-tag-modal-title").text('Edit Product Tag');
    else {
        $("#product-tag-modal-title").text('Add a Product Tag');
        $('#save-product-tag-modal-domain-selector option:eq(0)').prop('selected', true).change();
    }
    $('#save-product-tag-modal-domain-selector').not('.multi-select').select2();
    $("#app-trader-resources-tag-add").modal('show');
}

function resetForm(formId) {
    ClearError();
    $("#" + formId)[0].reset();
    ($("#" + formId).validate()).resetForm();
}

function adminDeleteAdditional(id, name, type) {
    var message = "Are you sure you want delete this ";
    if (type == 1)
        message += "Brand: " + name + " ?";
    else
        message += "Product tag: " + name + " ?";
    var result = confirm(message);
    if (result === false) {
        return;
    }
    var url = "/TraderResource/DeleteAdditionalInfo?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "delete",
        dataType: "json",
        success: function (rs) {
            LoadingOverlayEnd();
            if (rs.actionVal === 1) {
                cleanBookNotification.removeSuccess();

                // Reload table
                switch (type) {
                    case 1:
                        $("#item-brand-table").DataTable().ajax.reload();
                    case 4:
                        $("#item-product-tag-table").DataTable().ajax.reload();
                }

            } else if (rs.actionVal === 3) {
                cleanBookNotification.removeFail();
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
