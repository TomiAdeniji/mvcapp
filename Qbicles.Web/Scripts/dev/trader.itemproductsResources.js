var activeSubTab = "tab-images";
var resourceKey = "subResourceTab";
var locationId = $('#local-manage-select').val();
function getDefaultSidebarTab() {
    if (getLocalStorage(resourceKey))
        activeSubTab = getLocalStorage(resourceKey);
    switch (activeSubTab.toLocaleLowerCase()) {
        case "tab-images":
            selectImagesTab();
            break;
        case "tab-documents":
            selectDocumentsTab();
            break;
        case "tab-brands":
            selectBrandsTab();
            break;
        case "tab-needs":
            selectNeedsTab();
            break;
        case "tab-quality":
            selectQuantityTab();
            break;
        case "tab-extratags":
            selectExtratagsTab();
            break;
        case "tab-access":
            selectAccessTab();
            break;
        default:
            selectImagesTab();
            break;
    }
    $('a[href="#' + activeSubTab + '"]').tab('show');
}
function selectImagesTab() {
    setLocalStorage(resourceKey, 'tab-images');

    var ajaxUri = '/TraderResource/ResourceImagesTab';
    $('#tab-images').LoadingOverlay("show");
    $('#tab-images').empty();
    $('#tab-images').load(ajaxUri, function () {
        $('#tab-images').LoadingOverlay("hide");
    });
}
function selectDocumentsTab() {
    setLocalStorage(resourceKey, 'tab-documents');
    var ajaxUri = '/TraderResource/ResourceDocumentsTab?locationId=' + $('#local-manage-select').val();
    $('#tab-documents').LoadingOverlay("show");
    $('#tab-documents').empty();
    $('#tab-documents').load(ajaxUri, function () {
        $('#tab-documents').LoadingOverlay("hide");
    });
}
function selectBrandsTab() {
    setLocalStorage(resourceKey, 'tab-brands');
    var ajaxUri = '/TraderResource/ResourceBrandsTab?type=Brand';
    $('#tab-brands').LoadingOverlay("show");
    $('#tab-brands').empty();
    $('#tab-brands').load(ajaxUri, function () {
        $('#tab-brands').LoadingOverlay("hide");
    });
}
function selectNeedsTab() {
    setLocalStorage(resourceKey, 'tab-needs');
    var ajaxUri = '/TraderResource/ResourceBrandsTab?type=Need';
    $('#tab-brands').LoadingOverlay("show");
    $('#tab-brands').empty();
    $('#tab-brands').load(ajaxUri, function () {
        $('#tab-brands').LoadingOverlay("hide");
    });
}
function selectQuantityTab() {
    setLocalStorage(resourceKey, 'tab-quality');
    var ajaxUri = '/TraderResource/ResourceBrandsTab?type=QualityRating';
    $('#tab-brands').LoadingOverlay("show");
    $('#tab-brands').empty();
    $('#tab-brands').load(ajaxUri, function () {
        $('#tab-brands').LoadingOverlay("hide");
    });
}
function selectExtratagsTab() {
    setLocalStorage(resourceKey, 'tab-extratags');
    var ajaxUri = '/TraderResource/ResourceBrandsTab?type=ProductTag';
    $('#tab-brands').LoadingOverlay("show");
    $('#tab-brands').empty();
    $('#tab-brands').load(ajaxUri, function () {
        $('#tab-brands').LoadingOverlay("hide");
    });
}
function selectAccessTab() {
    setLocalStorage(resourceKey, 'tab-access');
    var ajaxUri = '/TraderResource/ResourceAccessTab?locationId=' + $('#local-manage-select').val();
    $('#tab-access').LoadingOverlay("show");
    $('#tab-access').empty();
    $('#tab-access').load(ajaxUri, function () {
        $('#tab-access').LoadingOverlay("hide");
    });
}
// Image Resource

function getDataImages(reload) {
    var ajaxUri = '/TraderResource/ResourceImageData?locationId=' + locationId;
    if (!reload)
        $('#resource_image_data').LoadingOverlay("show");
    $('#resource_image_data').empty();
    $('#resource_image_data').load(ajaxUri, function () {
        $('#resource_image_data').LoadingOverlay("hide");
        searchOnDataImages();
    });
}
function addEditImage(id) {
    var ajaxUri = '/TraderResource/ResourceImageAddEdit?id=' + id;
    $('#app-trader-resources-image-add').LoadingOverlay("show");
    $('#app-trader-resources-image-add').empty();
    $('#app-trader-resources-image-add').load(ajaxUri, function () {
        $('#app-trader-resources-image-add').LoadingOverlay("hide");
    });
}
function selectImageReImage(ev) {
    readURLImage(ev, 'preview');

}
function validateReImage() {
    var valid = true;
    // check title
    if ($('#re_image_name').val() === '') {
        $("#re_image_form").validate().showErrors({ re_image_name: "Title is required." });
        valid = false;
    }
    // check image feature
    if (($('#re_image_file')[0].files && $('#re_image_file')[0].files.length === 0) && $('#re_image_filevalue').val() == "") {
        $("#re_image_form").validate().showErrors({ featuredimg: "File image is required." });
        valid = false;
    }
    // category
    if ($('#re_image_category').val() === '') {
        $("#re_image_form").validate().showErrors({ re_image_category: "Category image is required." });
        valid = false;
    }
    return valid;
}
function saveImage() {
    if (!validateReImage()) {
        return;
    }
    LoadingOverlay();
    if ($('#re_image_file')[0].files.length > 0) {
        UploadMediaS3ClientSide("re_image_file").then(function (mediaS3Object) {
            $('#preview').show();
            $('#re_image_filevalue').val(mediaS3Object.objectKey);
            $('#re_image_qbliclefiletype').val(mediaS3Object.fileName.split('.').pop());
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                SubmitItemProductResourceImage();
            }
        });
    } else {
        $('#preview').hide();
        LoadingOverlayEnd('hide');
    }


};

SubmitItemProductResourceImage = function () {
    var reImage = {
        Id: $("#re_image_id").val(),
        Name: $("#re_image_name").val(),
        FileUri: $("#re_image_filevalue").val(),
        Type: { Extension: $("#re_image_qbliclefiletype").val() },
        Category: { Id: $("#re_image_category").val() },
        Description: $("#re_image_description").val()
    }

    $.ajax({
        type: 'post',
        url: '/TraderResource/SaveImage',
        data: { reImage: reImage },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-resources-image-add').modal('toggle');
                getDataImages(true);
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-resources-image-add').modal('toggle');
                getDataImages(true);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function deleteResourceImage(id, name) {
    var result = confirm(_L("ERROR_MSG_281", [name]));
    if (result === false) {
        return;
    }
    var url = "/TraderResource/DeleteReImage?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "delete",
        dataType: "json",
        success: function (rs) {
            LoadingOverlayEnd();
            if (rs.actionVal === 1) {
                cleanBookNotification.removeSuccess();
                getDataImages();
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


// Document resource

function getDataDocuments(reload) {

    var ajaxUri = '/TraderResource/ResourceDocumentData?locationId=' + locationId;
    if (!reload)
        $('#resource_document_data').LoadingOverlay("show");
    $('#resource_document_data').empty();
    $('#resource_document_data').load(ajaxUri, function () {
        $('#resource_document_data').LoadingOverlay("hide");
        searchOnDataDocuments();
    });
}
function addEditDocument(id) {
    var ajaxUri = '/TraderResource/ResourceDocumentAddEdit?id=' + id;
    $('#app-trader-resource-document-add').LoadingOverlay("show");
    $('#app-trader-resource-document-add').empty();
    $('#app-trader-resource-document-add').load(ajaxUri, function () {
        $('#app-trader-resource-document-add').LoadingOverlay("hide");
    });
}
function selectImageReDocument(ev) {
    readURLImage(ev, 'preview');
}
function validateReDocument() {
    var valid = true;
    // check title
    if ($('#re_document_name').val() === '') {
        $("#re_document_form").validate().showErrors({ re_document_name: "Title is required." });
        valid = false;
    }
    // check image feature
    if (($('#re_document_file')[0].files && $('#re_document_file')[0].files.length === 0) && $('#re_document_filevalue').val() == "") {
        $("#re_document_form").validate().showErrors({ featuredimg: "File image is required." });
        valid = false;
    }
    // category
    if ($('#re_document_category').val() === '') {
        $("#re_document_form").validate().showErrors({ re_document_category: "Category image is required." });
        valid = false;
    }
    return valid;
}
function saveDocument() {
    if (!validateReDocument()) {
        return;
    }
    LoadingOverlay();
    if ($('#re_document_file')[0].files.length > 0) {
        UploadMediaS3ClientSide("re_document_file").then(function (mediaS3Object) {
            $('#re_document_filevalue').val(mediaS3Object.objectKey);
            $('#re_document_qbliclefiletype').val(mediaS3Object.fileName.split('.').pop());
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                cleanBookNotification.error(_L("ERROR_MSG_345"), "Qbicles");
                return;
            }
            SubmitItemProductResourceDocument();
        });
    }

}

SubmitItemProductResourceDocument = function () {
    var reDocument = {
        Id: $("#re_document_id").val(),
        Name: $("#re_document_name").val(),
        FileUri: $("#re_document_filevalue").val(),
        Type: { Extension: $("#re_document_qbliclefiletype").val() },
        Category: { Id: $("#re_document_category").val() },
        Description: $("#re_document_description").val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderResource/SaveDocument',
        data: { reDocument: reDocument },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-resource-document-add').modal('toggle');
                getDataDocuments(true);
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-resource-document-add').modal('toggle');
                getDataDocuments(true);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}


function deleteResourceDocument(id, name) {
    var result = confirm(_L("ERROR_MSG_281", [name]));
    if (result === false) {
        return;
    }
    var url = "/TraderResource/DeleteReDocument?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "delete",
        dataType: "json",
        success: function (rs) {
            LoadingOverlayEnd();
            if (rs.actionVal === 1) {
                cleanBookNotification.removeSuccess();
                getDataDocuments();
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
// Brands, need, quality, tag
function searchOnDataBrand() {
    var listKey = [];
    var keys = $('#key_search_brand').val();
    keys = $.trim(keys);
    if (keys !== "" && keys !== null && keys.length > 0) {
        listKey.push(keys);
    }
    $("#resource_brand_datatable").DataTable().search(listKey.join("|"), true, false, true).draw();
}
function getDataBrands(type) {
    var ajaxUri = '/TraderResource/ResourceBrandData?type=' + type;
    $('#resource_brand_data').LoadingOverlay("show");
    $('#resource_brand_data').empty();
    $('#resource_brand_data').load(ajaxUri, function () {
        $('#resource_brand_data').LoadingOverlay("hide");
        searchOnDataBrand();
    });
}
function addEditBrand(id, type) {
    var ajaxUri = '/TraderResource/ResourceBrandAddEdit?id=' + id + '&type=' + type;
    $('#app-trader-resources-brand-add').LoadingOverlay("show");
    $('#app-trader-resources-brand-add').empty();
    $('#app-trader-resources-brand-add').load(ajaxUri, function () {
        $('#app-trader-resources-brand-add').LoadingOverlay("hide");
    });
}
function validateBrand() {
    var valid = true;
    // check title
    if ($('#brand_name').val() == '') {
        $("#brand_form").validate().showErrors({ brand_title: "This field is required." });
        valid = false;
    }
    return valid;
}
function saveBrand(type) {

    $('#brand_form_div').LoadingOverlay("show");
    setTimeout(function () {

        if ($('#brand_name_hidden').val() == '') {
            $("#brand_form").validate().showErrors({ brand_title: "This field is required." });
            $('#brand_form_div').LoadingOverlay("hide", true);
            return;
        }

        var $name = JSON.parse($("#brand_name").val())[0].value;
        var brand = {
            Id: $("#brand_id").val(),
            Name: $name,
            Type: type
        }

        $.ajax({
            type: 'post',
            url: '/TraderResource/SaveBrand',
            data: { additional: brand },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        $('#app-trader-resources-brand-add').modal('toggle');
                        getDataBrands(type);
                    } else if (response.actionVal === 2) {
                        cleanBookNotification.updateSuccess();
                        $('#app-trader-resources-brand-add').modal('toggle');
                        getDataBrands(type);
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                } else {
                    $("#brand_form").validate().showErrors({ brand_title: response.msg });
                }

            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            $('#brand_form_div').LoadingOverlay("hide", true);
        });
    }, 1000);

}
function deleteAdditional(id, name, type) {
    var message = "";
    if (type == "Brand")
        message += "Brand: " + name + " ?";
    else if (type == "ProductTag")
        message += "Product tag: " + name + " ?";
    else if (type == "Need")
        message += "Need: " + name + " ?";
    else if (type == "QualityRating")
        message += "Quality rating: " + name + " ?";
    else
        message += name + " ?";

    var message = "Are you sure you want delete this " + message;

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
                getDataBrands(type);
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

// access
function getDataAccesss(value, reload) {
    if (!value)
        value = $('#key_search_access').val();
    var ajaxUri = '/TraderResource/ResourceAccessData?key=' + value.replace(/\s/g, '%20');
    if (!reload)
        $('#resource_access_data').LoadingOverlay("show");
    $('#resource_access_data').empty();
    $('#resource_access_data').load(ajaxUri, function () {
        $('#resource_access_data').LoadingOverlay("hide");
    });
}
function addEditAccess(id) {
    var ajaxUri = '/TraderResource/AccessAddEdit?id=' + id;
    $('#app-trader-resources-access-add').LoadingOverlay("show");
    $('#app-trader-resources-access-add').empty();
    $('#app-trader-resources-access-add').load(ajaxUri, function () {
        $('#app-trader-resources-access-add').LoadingOverlay("hide");
    });
}
function validateAccess() {
    var valid = true;
    // check title
    if ($('#access_name').val() === '') {
        $("#access_form").validate().showErrors({ access_title: "Name is required." });
        valid = false;
    }
    // check image feature
    if (($('#access_file')[0].files && $('#access_file')[0].files.length === 0) && $('#access_image').val() == "") {
        $("#access_form").validate().showErrors({ access_file: "Featured image is required." });
        valid = false;
    }
    return valid;
}
function saveAccess() {
    if (!validateAccess()) {
        return;
    }
    LoadingOverlay();
    if ($('#access_file')[0].files.length > 0) {
        UploadMediaS3ClientSide("access_file").then(function (mediaS3Object) {
            $('#access_image').val(mediaS3Object.objectKey);
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_345"), "Qbicles");
                return;
            }
            SubmitItemProductResourceAccess();
        });
    }


}

SubmitItemProductResourceAccess = function () {
    var access = {
        Id: $("#access_id").val(),
        AreaName: $("#access_name").val(),
        ImageUri: $("#access_image").val(),
        Type: $("#access_type").val(),
        Location: { Id: $("#access_location").val() },
        Description: $("#access_description").val()
    }
    $.ajax({
        type: 'post',
        url: '/TraderResource/SaveAccess',
        data: { access: access },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#app-trader-resources-access-add').modal('toggle');
                getDataAccesss(null, true);
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#app-trader-resources-access-add').modal('toggle');
                getDataAccesss(null, true);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }

        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};



