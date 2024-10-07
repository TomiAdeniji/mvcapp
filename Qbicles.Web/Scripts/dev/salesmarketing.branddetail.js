var isBusyAddTaskForm = false, brandCountRecord = 1,
    $form_media_addedit = $("#form_media_smresource");
function readImgURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function SocialRefreshProductVPropositon(Object) {
    if ($('#frm-value-prop select[name=ProductId] option[value="' + Object.id + '"]').length <= 0) {
        $('#frm-value-prop select[name=ProductId]').append($('<option>', {
            value: Object.id,
            text: Object.text
        })).select2();
    }
}


//Branch product ----------------------
function ProcessSaleMarketingBranchProduct() {

    if (!$('#frm-brand-product').valid()) {
        return;
    }
    var bpId = $('#frm-brand-product input[name=Id]').val();
    if (bpId == 0 && $('#frm-brand-product input[name=featuredimg]').get(0).files.length === 0) {
        $('#frm-brand-product').validate().showErrors({ featuredimg: "This field is required." })
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-brand-product-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-brand-product-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-brand-product-object-key").val(mediaS3Object.objectKey);
                $("#sm-brand-product-object-name").val(mediaS3Object.fileName);
                $("#sm-brand-product-object-size").val(mediaS3Object.fileSize);

                SubmitSaleMarketingBranchProduct();
            }
        });

    }
    else {
        $("#sm-brand-product-object-key").val("");
        $("#sm-brand-product-object-name").val("");
        $("#sm-brand-product-object-size").val("");
        SubmitSaleMarketingBranchProduct();
    }

};
SubmitSaleMarketingBranchProduct = function () {
    var frmData = new FormData($('#frm-brand-product')[0]);
    frmData.append('BrandId', $('#hdfBrandId').val());
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingBrand/SaveBrandProduct",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-brand-product-modal').modal('hide');
                if ($('#frm-brand-product input[name=Id]').val() === "0") {
                    cleanBookNotification.success(_L("ERROR_MSG_141"), "Sales Marketing");
                } else {
                    cleanBookNotification.success(_L("ERROR_MSG_142"), "Sales Marketing");
                }
                SocialBrandProductSearch();
                SocialBrandProductReset();
                SocialRefreshProductVPropositon(data.Object);
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusyAddTaskForm = false;
    });
}
function SocialBrandProductReset() {
    var $frm_brand_product = $('#frm-brand-product');
    $frm_brand_product.trigger("reset");
    $('#frm-brand-product img[name=productpreview]').hide();
    $('#frm-brand-product input[name=Id]').val('0');
    $('#frm-brand-product').validate().resetForm();
    $('#app-marketing-brand-product-modal .modal-title').text("Add a Brand Product");
}
function SocialBrandProductLoadEdit(id) {
    $.getJSON("/SalesMarketingBrand/LoadBrandProductById", { id: id }, function (data) {
        if (data) {
            $('#app-marketing-brand-product-modal .modal-title').text("Edit Brand Product");
            $('#frm-brand-product input[name=Id]').val(id);
            $('#frm-brand-product input[name=Name]').val(data.Name);
            $('#frm-brand-product textarea[name=Summary]').val(data.Summary);
            $('#frm-brand-product .productpreview').attr("src", data.featuredimg);
            $('#frm-brand-product .productpreview').show();
            $('#app-marketing-brand-product-modal').modal('show');
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_143", [id]), "Sales Marketing");
        }
    });
}
function SocialBrandProductLoad(pageSize, isSearch) {
    $.LoadingOverlay("show");
    $.ajax({
        url: '/SalesMarketingBrand/LoadBrandProducts',
        data: {
            brandId: $('#hdfBrandId').val(),
            size: brandCountRecord * pageSize,
            pageSize: pageSize,
            keyword: $('#txtBrandProductSearch').val(),
            isLoadingHide: $("#isLoadingHideBrandProduct").is(':checked') ? true : false
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            var contB = $('#brand-products .restyle');
            if (!isSearch) {
                if (contB)
                    contB.append(response).fadeIn(250);
                else
                    $('#brand-products').append(response).fadeIn(250);
            }
            else {
                $('#brand-products').html(response).fadeIn(250);
            }
            brandCountRecord = brandCountRecord + 1;
            var $result = $('#isHiddeLoadMore');
            if ($result.length > 0) {
                $('#btnLoadBands').remove();
                //return;
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function ShowOrHideBrandProduct(id) {
    $.ajax({
        url: "/SalesMarketingBrand/ShowOrHideBrandProduct",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                SocialBrandProductSearch();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SocialBrandProductSearch() {
    brandCountRecord = 0;
    SocialBrandProductLoad(brandPageSize, true);
}
//End branch product -----------



function SocialBrandAutoGenerateFolderName() {
    $.getJSON("/SalesMarketingBrand/AutoGenerateFolderName", function (data) {
        if (data) {
            $('#brandFolderName').val(data);
        }
    });
}

function SearchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}
function SocialMediasByBrand(fid, qid) {
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/SalesMarketingBrand/LoadMediasByBrand',
        datatype: 'json',
        data: { fid: fid, qid: qid, fileType: fileType == "All" ? "" : fileType },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#brand-resources .flex-grid-thirds-lg');
                $divcontain.html(listMedia);
                totop();
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function SocialBrandValuePropotionAdd() {
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    var $frm_value_prop = $('#frm-value-prop');
    $frm_value_prop.validate({
        rules: {
            ProductId: {
                required: true
            },
            WhoWantTo: {
                required: true,
                maxlength: 200
            },
            By: {
                required: true,
                maxlength: 200
            },
        }
    });
    $frm_value_prop.submit(function (e) {
        e.preventDefault();
        if ($frm_value_prop.valid()) {
            if (!$('#frm-value-prop select[name=CustomerSegment]').val()) {
                $frm_value_prop.validate().showErrors({ CustomerSegment: "This field is required." });
                return
            }
            $.LoadingOverlay("show");
            var frmData = $(this).serializeArray(); // convert form to array
            frmData.push({ name: "BrandId", value: $('#hdfBrandId').val() });
            $.ajax({
                type: this.method,
                url: this.action,
                data: frmData,
                beforeSend: function (xhr) {
                    isBusyAddTaskForm = true;
                },
                success: function (data) {
                    isBusyAddTaskForm = false;
                    if (data.result) {
                        $('#app-marketing-value-prop-add').modal('hide');
                        if ($('#frm-value-prop input[name=Id]').val() === "0")
                            cleanBookNotification.success(_L("ERROR_MSG_144"), "Sales Marketing");
                        else
                            cleanBookNotification.success(_L("ERROR_MSG_145"), "Sales Marketing");
                        SocialBrandValuePropotionReset();
                        socialBrandValueProLoad();
                    } else if (data.msg) {
                        cleanBookNotification.error(data.msg, "Sales Marketing");
                    }

                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusyAddTaskForm = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    LoadingOverlayEnd();
                }

            });
        }
    });
}
function SocialBrandValuePropotionReset() {
    var $frm_value_prop = $('#frm-value-prop');
    $frm_value_prop.trigger("reset");
    $('#frm-value-prop select[name=ProductId]').val('').change();
    $('#frm-value-prop select[name=CustomerSegment]').val('').change();
    $("#frm-value-prop select[name=CustomerSegment]").multiselect("refresh");
    $('#frm-value-prop input[name=Id]').val(0);
    $('#prop-action').text('');
    $('#prop-benefit').text('');
    $frm_value_prop.validate().resetForm();
    $('#app-marketing-value-prop-add .modal-title').text("Add a Value Proposition");
}
function SocialBrandValueProEdit(id) {
    $.getJSON("/SalesMarketingBrand/LoadBrandValueProposionById", { id: id }, function (data) {
        if (data) {
            $('#app-marketing-value-prop-add .modal-title').text("Edit Value Proposition");
            $('#frm-value-prop input[name=Id]').val(data.Id);
            $('#frm-value-prop select[name=ProductId]').val(data.productId).change();
            $('#frm-value-prop select[name=CustomerSegment]').val(data.segments).change();
            $("#frm-value-prop select[name=CustomerSegment]").multiselect("refresh");
            $('#frm-value-prop input[name=WhoWantTo]').val(data.WhoWantTo);
            $('#prop-action').text(data.WhoWantTo);
            $('#frm-value-prop input[name=By]').val(data.By);
            $('#prop-benefit').text(data.By);
            $('#app-marketing-value-prop-add').modal('show');
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_146", [id]), "Sales Marketing");
        }
    });
}
function socialBrandValueProLoad() {
    var brandId = $('#hdfBrandId').val();
    var productId = $('#valuefilter').val();
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/SalesMarketingBrand/LoadValuePropositionByBrand',
        dataType: 'html',
        data: { brandId: brandId, productId: productId, isLoadingHide: $("#isLoadingHideValueProp").is(':checked') ? true : false },
        success: function (data) {
            if (data) {
                var $divcontain = $('#brand-value .flex-grid-thirds-lg');
                $divcontain.html(data);
                totop();
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function ShowOrHideValueProp(id) {
    $.ajax({
        url: "/SalesMarketingBrand/ShowOrHideValueProp",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                socialBrandValueProLoad();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

$('#isLoadingHideValueProp').change(SearchThrottle(function () {
    // do the search if criteria is met
    socialBrandValueProLoad();
}));

//Branch Attribute group----------------------
function ProcessSaleMarketingBranchAttributeGroup() {
    if (!$('#frm-attribute-group').valid()) {
        return;
    }

    var brid = $('#hdfBrandId').val();
    if (brid === '0' && !$('#frm-attribute-group input[name=FeaturedImg]').val()) {
        $('#frm-attribute-group').validate().showErrors({ FeaturedImg: "This field is required." });
        return;
    }

    $.LoadingOverlay("show");
    var files = document.getElementById("sm-attribute-group-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-attribute-group-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-brand-attribute-group-object-key").val(mediaS3Object.objectKey);
                $("#sm-brand-attribute-group-object-name").val(mediaS3Object.fileName);
                $("#sm-brand-attribute-group-object-size").val(mediaS3Object.fileSize);

                SubmitSaleMarketingBranchAttributeGroup(brid);
            }
        });

    }
    else {
        $("#sm-brand-attribute-group-object-key").val("");
        $("#sm-brand-attribute-group-object-name").val("");
        $("#sm-brand-attribute-group-object-size").val("");
        SubmitSaleMarketingBranchAttributeGroup(brid);
    }
};

SubmitSaleMarketingBranchAttributeGroup = function (brid) {

    var frmData = new FormData($('#frm-attribute-group')[0]);
    frmData.append('BrandId', brid);
    $.ajax({
        type: "post",
        url: "/SalesMarketingBrand/SaveBrandAttrGroup",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-attribute-group-add').modal('hide');
                if ($('#frm-value-prop input[name=Id]').val() === "0")
                    cleanBookNotification.success(_L("ERROR_MSG_147"), "Sales Marketing");
                else
                    cleanBookNotification.success(_L("ERROR_MSG_148"), "Sales Marketing");
                SocialBrandAttributeGroupReset();
                SocialBrandAttributeGroupLoad();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }

        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
};

function SocialLoadAttrsForAttrGroup() {
    $.ajax({
        type: 'post',
        url: '/SalesMarketingBrand/LoadOptionAttributeByBrandId',
        dataType: 'html',
        data: { brandId: $('#hdfBrandId').val() },
        success: function (data) {
            if (data) {
                $('#frm-attribute-group select[name=Attributes]').html(data);
                $("#frm-attribute-group select[name=Attributes]").multiselect("destroy");
                $("#frm-attribute-group select[name=Attributes]").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                $('#frm-attribute-group select[name=Attributes]').hide();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}
function SocialBrandAttributeGroupEdit(id) {
    $.getJSON("/SalesMarketingBrand/GetBrandAttributegroupById", { id: id }, function (data) {
        if (data) {
            $('#app-marketing-attribute-group-add .modal-title').text("Edit Attribute Group");
            $('#frm-attribute-group input[name=Id]').val(data.Id);
            $('#frm-attribute-group input[name=Name]').val(data.Name);
            $('#frm-attribute-group textarea[name=Summary]').val(data.Summary);
            $('#frm-attribute-group img.groupimg').attr("src", data.IconUrl);
            $('#frm-attribute-group img.groupimg').show();
            $('#frm-attribute-group select[name=Attributes]').val(data.Attributes);
            $("#frm-attribute-group select[name=Attributes]").multiselect("refresh");
            $('#app-marketing-attribute-group-add').modal('show');
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_149", [id]), "Sales Marketing");
        }
    });
}
function SocialBrandAttributeGroupReset() {
    var $frm_attribute = $('#frm-attribute-group');
    $frm_attribute.trigger("reset");
    $('#frm-attribute-group select[name=Attributes]').val('').change();
    $("#frm-attribute-group select[name=Attributes]").multiselect("refresh");
    $('#frm-attribute-group input[name=Id]').val(0);
    $('#frm-attribute-group img.groupimg').hide();
    $('#frm-attribute-group input[name=FeaturedImg]').val();
    $frm_attribute.validate().resetForm();
    $('#app-marketing-attribute-group-add .modal-title').text("Add an Attribute Group");
}
function SocialBrandAttributeGroupLoad() {
    var brandId = $('#hdfBrandId').val();
    var keyword = $('#txtSearchAttrGroup').val();
    $.LoadingOverlay("show");
    var fileType = $('#sl-media-type').val();
    $.ajax({
        type: 'post',
        url: '/SalesMarketingBrand/LoadBrandAttrGroupsByBrandId',
        dataType: 'html',
        data: { brandId: brandId, keyword: keyword },
        success: function (data) {
            if (data) {
                var $divcontain = $('#brand-attribute-groups .flex-grid-quarters-lg');
                $divcontain.html(data);
                totop();
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}


//Branch Attribute----------------------

function ProcessSaleMarketingBranchAttribute() {
    if (!$('#frm-attribute').valid()) {
        return;
    }
    var brid = $('#hdfBrandId').val();
    if (brid === '0' && !$('#frm-attribute input[name=FeaturedImg]').val()) {
        $('#frm-attribute').validate().showErrors({ FeaturedImg: "This field is required." });
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-attribute-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-attribute-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-brand-attribute-object-key").val(mediaS3Object.objectKey);
                $("#sm-brand-attribute-object-name").val(mediaS3Object.fileName);
                $("#sm-brand-attribute-object-size").val(mediaS3Object.fileSize);

                SubmitSaleMarketingBranchAttribute(brid);
            }
        });


    }
    else {
        $("#sm-brand-attribute-object-key").val("");
        $("#sm-brand-attribute-object-name").val("");
        $("#sm-brand-attribute-object-size").val("");
        SubmitSaleMarketingBranchAttribute(brid);
    }
};

SubmitSaleMarketingBranchAttribute = function (brid) {
    var frmData = new FormData($('#frm-attribute')[0]);
    frmData.append('BrandId', brid);
    $.ajax({
        type: "post",
        url: "/SalesMarketingBrand/SaveBrandAttribute",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-attribute-add').modal('hide');
                if ($('#frm-attribute input[name=Id]').val() === "0")
                    cleanBookNotification.success(_L("ERROR_MSG_150"), "Sales Marketing");
                else
                    cleanBookNotification.success(_L("ERROR_MSG_151"), "Sales Marketing");
                SocialBrandAttributeReset();
                SocialBrandAttributeLoad();
                SocialLoadAttrsForAttrGroup();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }

            LoadingOverlayEnd();
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }

    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
};

function SocialBrandAttributeEdit(id) {
    $.getJSON("/SalesMarketingBrand/GetBrandAttributeById", { id: id }, function (data) {
        if (data) {
            $('#app-marketing-attribute-add .modal-title').text("Edit Attribute");
            $('#frm-attribute input[name=Id]').val(data.Id);
            $('#frm-attribute input[name=Name]').val(data.Name);
            $('#frm-attribute textarea[name=Summary]').val(data.Summary);
            $('#frm-attribute img.attrimg').attr("src", data.IconUrl);
            $('#frm-attribute img.attrimg').show();
            $('#app-marketing-attribute-add').modal('show');
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SocialBrandAttributeLoad() {
    var brandId = $('#hdfBrandId').val();
    var keyword = $('#txtAttributeSearch').val();
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/SalesMarketingBrand/LoadBrandAttributeByBrandId',
        dataType: 'html',
        data: { brandId: brandId, keyword: keyword, isLoadingHide: $("#isLoadingHideAttribute").is(':checked') ? true : false },
        success: function (data) {
            if (data) {
                var $divcontain = $('#brand-attribute-attributes .flex-grid-quarters-lg');
                $divcontain.html(data);
                totop();
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function ShowOrHideBrandAttribute(id) {
    $.ajax({
        url: "/SalesMarketingBrand/ShowOrHideBrandAttribute",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                SocialBrandAttributeLoad();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SocialBrandAttributeReset() {
    var $frm_attribute = $('#frm-attribute');
    $frm_attribute.trigger("reset");
    $('#frm-attribute input[name=Id]').val(0);
    $('#frm-attribute img.attrimg').hide();
    $('#frm-attribute input[name=FeaturedImg]').val();
    $frm_attribute.validate().resetForm();
    $('#app-marketing-attribute-add .modal-title').text("Add an Attribute");
}
//Branch Attribute End----------------------


//Branch Resource ------------

function SocialMediaBranchResourceSaveMedia(qbicleId) {

    if ($form_media_addedit.valid()) {
        $.ajax({
            url: "/Medias/DuplicateMediaNameCheck",
            data: { qbicleId: qbicleId, mediaId: 0, MediaName: $('#form_media_smresource input[name=name]').val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                $form_media_addedit.validate().showErrors({ name: "Name of Media already exists." });
            else {
                ProcessSaleMarketingBranchResource();
            }
        }).fail(function () {
            $form_media_addedit.validate().showErrors({ name: "Error checking existing name of Media" });
        })
    }
}

function ProcessSaleMarketingBranchResource() {
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-brand-resource-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-brand-resource-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-brand-resource-object-key").val(mediaS3Object.objectKey);
                $("#sm-brand-resource-object-name").val(mediaS3Object.fileName);
                $("#sm-brand-attribute-object-size").val(mediaS3Object.fileSize);

                SubmitSocialMediaBranchResourceSaveMedia();
            }
        });

    }
    else {
        $("#sm-brand-resource-object-key").val("");
        $("#sm-brand-resource-object-name").val("");
        $("#sm-brand-resource-object-size").val("");
        SubmitSocialMediaBranchResourceSaveMedia();
    }
};

SubmitSocialMediaBranchResourceSaveMedia = function () {
    var frmData = new FormData($form_media_addedit[0]);
    frmData.append("qbicleId", $('#media-qbicleId').val());
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketing/SaveCampaignResource",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#create-resource').modal('hide');
                isBusyAddTaskForm = false;
                cleanBookNotification.success(_L("ERROR_MSG_172"), "Sales Marketing");
                SocialMediasByBrand($('#hdfmediaFolderId').val(), $('#media-qbicleId').val());
                $form_media_addedit.trigger("reset");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
                isBusyAddTaskForm = false;
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            
        }

    }).always(function () {
        LoadingOverlayEnd();
        isBusyAddTaskForm = false;
    });

};
//End Modal
//branch ressource end




function LoadCampaignsInBrand() {
    $("#community-list").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#community-list").LoadingOverlay("show");
        } else {
            $("#community-list").LoadingOverlay("hide", true);
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
        "deferLoading": 30,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketingBrand/LoadCampaignsInBrand',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "brandId": $("#hdfBrandId").val(),
                    "types": $("#campaignType").val(),
                    "search": $("#searchCampaign").val()
                });
            }
        },
        "columns": [
            {
                data: "Name",
                orderable: false
            },
            {
                data: "Type",
                orderable: false
            },
            {
                data: "NumberOfQueue",
                orderable: false
            },
            {
                data: "NumberOfCompletedPost",
                orderable: false
            },
            {
                data: null,
                orderable: false,
                render: function (value, type, row) {
                    var str = '<button class="btn btn-info" data-toggle="modal" data-target="#app-marketing-brand-usage" onclick="LoadBrandElement(' + row.Id + ', \'' + row.Type + '\')"><i class="fa fa-list"></i> &nbsp; View</button>'
                    return str;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (value, type, row) {
                    var url = '';
                    if (row.Type == "Email") {
                        url = "/SalesMarketing/SMEmail?id=" + row.Id;
                    } else if (row.Type == "Automated Social") {
                        url = "/SalesMarketing/SMSocial?id=" + row.Id;
                    } else {
                        url = "/SalesMarketing/SMManualSocial?id=" + row.Id;
                    }
                    var str = '<button class="btn btn-primary" onclick="window.location.href=\'' + url + '\'"><i class="fa fa-eye"></i> &nbsp; View Campaign</button>'
                    return str;
                }
            }
        ],
        "initComplete": function (settings, json) {
            $('#community-list').DataTable().ajax.reload();
        }
    });
}

function LoadBrandElement(id, type) {
    $("#app-marketing-brand-usage").load("/SalesMarketingBrand/LoadBrandElement", { id: id, type: type }, function () {

    });
}



$(document).ready(function () {


    SocialBrandValuePropotionAdd();

    LoadCampaignsInBrand();


    $('#txtBrandProductSearch').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        SocialBrandProductSearch();
    }));
    $('#isLoadingHideBrandProduct').change(SearchThrottle(function () {
        // do the search if criteria is met
        SocialBrandProductSearch();
    }));
    //Branch Attribute group----------------------
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(".previewAttrGroupimg").change(function () {
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });
    $('#txtSearchAttrGroup').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        SocialBrandAttributeGroupLoad();
    }));

    $("#sm-brand-product-upload-image").change(function () {
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });

    //Branch Attribute 124----------------------
    $(".previewAttrimg").change(function () {
        var target = $(this).data('target');
        readImgURL(this, target);
        $(target).fadeIn();
    });
    $('#txtAttributeSearch').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        SocialBrandAttributeLoad();
    }));
    $('#isLoadingHideAttribute').change(SearchThrottle(function () {
        // do the search if criteria is met
        SocialBrandAttributeLoad();
    }));
});


function ProcessSaleMarketingBranch() {

    if (!$('#frm-marketing-brand').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-brand-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("m-brand-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-brand-object-key").val(mediaS3Object.objectKey);
                $("#sm-brand-object-name").val(mediaS3Object.fileName);
                $("#sm-brand-object-size").val(mediaS3Object.fileSize);

                SubmitSaleMarketingBranch();
            }
        });


    }
    else {
        $("#sm-brand-object-key").val("");
        $("#sm-brand-object-name").val("");
        $("#sm-brand-object-size").val("");
        SubmitSaleMarketingBranch();
    }

};
SubmitSaleMarketingBranch = function () {

    var folder = $('#frm-marketing-brand select[name=FeaturedImageUri]').val();
    if (!folder || (folder == "0" && !$('#brandFolderName').val())) {
        $frm_marketing_brand.validate().showErrors({ FolderName: "This field is required." });
        return;
    }
    var frmData = new FormData($('#frm-marketing-brand')[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingBrand/SaveSaleMarketingBrand",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-brand-add').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_140"), "Sales Marketing");
                getTotalPageBrand();
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                isBusyAddTaskForm = false;
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });

}