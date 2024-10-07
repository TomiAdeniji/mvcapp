var isBusy = false, frmProfileId ="#frmB2BProfile";
var wto;

$(function () {
    getBusinessLocations();
    getBusinessProfilePages();

    $('#profile-b2c select[name=UserIdB2CRelationshipManagers], #profile-b2b select[name=UserIdB2BRelationshipManagers], #profile-b2b input[name=IsDisplayedInB2BListings], #profile-b2c input[name=IsDisplayedInB2CListings]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            updateDefaultManagers();
        }, 1000);
    });

});
$(document).ready(function () {
    reloadPosts(true, false);
    initFormPost();
    initEventStoreProfile();
    $('.select2tag').select2({
        tags: true
    });
    var _tabactive = getQuerystring('tab');
    switch (_tabactive) {
        case 'profile-pages':
            $('a[href=#' + _tabactive+']').click();
            break;
        default:
            break;
    };
    $(frmProfileId).validate({ validClass: "valid-no-border" });
});
function changeavatar() {
    if (!$(frmProfileId).valid()) {
        $('#commerce-logo').val(null);
        return;
    }
    var fileOstoreImg = document.getElementById("commerce-logo").files;
    var objectKey = $("#commerce-logo-object-key").val();
    $('.box-shadow-dark').LoadingOverlay('show');
    if (!objectKey) {
        if (fileOstoreImg && fileOstoreImg.length > 0) {
            UploadMediaS3ClientSide("commerce-logo").then(function (mediaS3Object) {
                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    $('.business-logo').LoadingOverlay('hide', true);
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    $("#commerce-logo-object-key").val(mediaS3Object.objectKey);
                    saveProfileB2B();
                }
            });
        }
    } else {
        saveProfileB2B();
    }
}
function saveProfileB2B() {
    if ($(frmProfileId).valid()) {
        var frmData = new FormData($(frmProfileId)[0]);
        LoadingOverlay();
        $.ajax({
            type: "post",
            cache: false,
            url: "/Commerce/SaveProfileB2B",
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            dataType: "json",
            data: frmData,
            beforeSend: function (xhr) {
                isBusy = true;
            },
            success: function (data) {
                isBusy = false;
                if (data.result) {
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
                    var logoobjectkey = $("#commerce-logo-object-key").val();
                    if (logoobjectkey) {
                        var api = $('#api-uri').val();
                        $('.business-logo').css('background-image', "url(" + api + logoobjectkey + ")");
                        $("#commerce-logo-object-key").val('');
                        $("#commerce-logo").val(null);
                    }
                    $("#business-name-display").text(data.msgName);
                    $("#domain-name-navigation").text(data.msgName);
                    $('#hdfProfileId').val(data.Object);
                    $('div.social-links input, select[name=tags]').removeAttr('disabled');
                    if ($("#domainType").val() == "Premium") {
                        $('div.business-options a').removeAttr('disabled');
                    }
                    $('#btnAddFeauturedPost').removeAttr('disabled');
                } else if (!data.result && data.msg) {
                    cleanBookNotification.error(_L(data.msg), "Commerce");
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                LoadingOverlayEnd();
                $('.box-shadow-dark').LoadingOverlay('hide', true);
            },
            error: function (data) {
                isBusy = false;
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
        LoadingOverlayEnd();
    }
}
function initFormPost() {
    var $frmCommerePost = $('#frmCommerePost');
    $frmCommerePost.validate({
        ignore: "",
        rules: {
            Title: {
                required: true,
                maxlength: 150
            },
            Content: {
                required: true,
                maxlength: 500
            }
        }
    });
    $frmCommerePost.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmCommerePost.valid()) {
            $.LoadingOverlay("show");
            var files = document.getElementById("commerce-post-feature").files;
            if (files.length > 0) {
                UploadMediaS3ClientSide("commerce-post-feature").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        $("#commerce-post-feature-object-key").val(mediaS3Object.objectKey);

                        SavePostB2B();
                    }
                });
            }
            else {
                $("#commerce-post-feature-object-key").val("");
                SavePostB2B();
            }
        }
    });
    $('#txtFilterFeaturedSearch').keyup(delay(function () {
        reloadPosts(true);
    }, 800));
    $('#txtFilterOtherSearch').keyup(delay(function () {
        reloadPosts(false);
    }));
    $('#frmB2BProfile input,#frmB2BProfile textarea').change(function () {
        $('#btnConfigCancel').attr('disabled', false);
    });
}
function SavePostB2B() {
    var profileId = $('#frmB2BProfile input[name=Id]').val();
    if (profileId > 0) {
        var frmData = new FormData($('#frmCommerePost')[0]);
        frmData.append("ProfileId", profileId);
        $.ajax({
            type: "post",
            cache: false,
            url: "/Commerce/SavePostB2B",
            enctype: 'multipart/form-data',
            processData: false,
            contentType: false,
            dataType: "json",
            data: frmData,
            beforeSend: function (xhr) {
                isBusy = true;
            },
            success: function (data) {
                isBusy = false;
                if (data.result) {
                    $('#app-commerce-post-add').modal('hide');
                    reloadPosts($('#hdfIsFeatured').val() == 'true' ? true : false);
                    jumpTo('content-featured-posts');
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
                } else if (!data.result && data.msg) {
                    cleanBookNotification.error(_L(data.msg), "Commerce");
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                LoadingOverlayEnd();
            },
            error: function (data) {
                isBusy = false;
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
    }
}
function initModalPost(id, isfeatured) {
    //Reset Modal
    $("#frmCommerePost").validate().resetForm();
    $('#hdfPostId').val(0);
    $('#hdfIsFeatured').val(isfeatured ? 'true' : 'false');
    $('#frmCommerePost input[name=Title]').val('');
    $('#frmCommerePost textarea[name=Content]').val('');
    $('#frmCommerePost input[name=FeaturedImage]').val('');
    if (id && id > 0) {
        $.get("/Commerce/GetPostById", { id: id }).done(function (response) {
            if (response.result) {
                var post = response.Object;
                $('#frmCommerePost input[name=Title]').val(post.Title);
                $('#frmCommerePost textarea[name=Content]').val(post.Content);
                $('#hdfPostId').val(post.Id);
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
    }
    if (id && id > 0 && isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Edit Featured Post");
    } else if (id && id > 0 && !isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Edit Profile Post");
    } else if (!id && isfeatured) {
        $('#app-commerce-post-add h5.modal-title').text("Add a Featured Post");
    } else {
        $('#app-commerce-post-add h5.modal-title').text("Add a Profile Post");
    }
    $('#app-commerce-post-add').modal('show');
}
function reloadPosts(isFeatured, firstload) {
    if (!firstload) {
        var _paramaters = {
            profileId: $('#frmB2BProfile input[name=Id]').val(),
            search: '',
            isfeatured: isFeatured
        };
        if (isFeatured) {
            _paramaters.search = $('#txtFilterFeaturedSearch').val();
            setTimeout(function () {
                var $content1 = $('.featured-posts');
                $content1.LoadingOverlay("show");
                $content1.load("/Commerce/LoadPostsContent", _paramaters, function () {
                    $content1.LoadingOverlay("hide");
                });
            }, 100);
        } else {
            _paramaters.search = $('#txtFilterOtherSearch').val();
            setTimeout(function () {
                var $content2 = $('#b2b-profile-2 .from-community');
                $content2.LoadingOverlay("show");
                $content2.load("/Commerce/LoadPostsContent", _paramaters, function () {
                    $content2.LoadingOverlay("hide");
                });
            }, 100);
        }
    }
}
function deletePost(id, isFeatured) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        centerVertical: true,
        closeButton: true,
        animate: true,
        title: "Posts",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Commerce/DeletePostById", { id: id }, function (response) {
                    if (response.result) {
                        reloadPosts(isFeatured);
                        jumpTo('content-featured-posts');
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                });
                return;
            }
        }
    });

}
function initEventStoreProfile() {
    $('div.social-links input').change(function () {
        saveSocialLinks();
    });
    $('#frmB2BProfile textarea[name=BusinessSummary], #frmB2BProfile select[name=BusinessEmail]').change(delay(function () {
        saveProfileB2B();
    }, 1000));
    $('#frmB2BProfile select[name=AreasOfOperation]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            saveProfileB2B();
        }, 1500);
    });
    $('select[name=interests]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            saveCategories();
        }, 1000);
    });
    $('select[name=tags]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            saveTags();
        }, 2500);
    });
    $('#frmsociallinks').validate(
        {
            rules: {
                FacebookUrl: {
                    url: true
                },
                LinkedInUrl: {
                    url: true
                },
                TwitterUrl: {
                    url: true
                },
                YoutubeUrl: {
                    url: true
                },
                InstagramUrl: {
                    url: true
                }
            }
        });
    $('#profile-pages input[name=keyword]').keyup(delay(function () {
        reloadBusinessProfilePages();
    }, 1000));
    $('#profile-pages select[name=status]').change(function () {
        reloadBusinessProfilePages();
    });
}
function saveSocialLinks() {
    var profileId = $('#hdfProfileId').val();
    if (profileId > 0 && $('#frmsociallinks').valid()) {
        var $facebook = $('#frmsociallinks input[name=FacebookUrl]');
        var $instagram = $('#frmsociallinks input[name=InstagramUrl]');
        var $linkedIn = $('#frmsociallinks input[name=LinkedInUrl]');
        var $twitter = $('#frmsociallinks input[name=TwitterUrl]');
        var $youtube = $('#frmsociallinks input[name=YoutubeUrl]');
        var b2BProfile = { Id: profileId };
        var facebookUrl = { Url: $facebook.val(), Type: $facebook.data('type'), B2BProfile: b2BProfile };
        var instagramUrl = { Url: $instagram.val(), Type: $instagram.data('type'), B2BProfile: b2BProfile };
        var linkedInUrl = { Url: $linkedIn.val(), Type: $linkedIn.data('type'), B2BProfile: b2BProfile };
        var twitterUrl = { Url: $twitter.val(), Type: $twitter.data('type'), B2BProfile: b2BProfile };
        var youtubeUrl = { Url: $youtube.val(), Type: $youtube.data('type'), B2BProfile: b2BProfile };
        var paramaters = [];
        paramaters.push(facebookUrl);
        paramaters.push(instagramUrl);
        paramaters.push(linkedInUrl);
        paramaters.push(twitterUrl);
        paramaters.push(youtubeUrl);
        $.post("/Commerce/UpdateSocialLinks", { socialLinks: paramaters }, function (Response) {
            if (Response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function saveTags() {
    var profileId = $('#hdfProfileId').val();
    if (profileId > 0 && $('#frmB2BProfile').valid()) {
        var paramaters = $('select[name=tags]').val();
        $.post("/Commerce/UpdateTags", { tags: (paramaters ? paramaters : []), profileId: profileId }, function (Response) {
            if (Response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function saveCategories() {
    var profileId = $('#hdfProfileId').val();
    if (profileId > 0 && $('#frmB2BProfile').valid()) {
        var paramaters = $('select[name=interests]').val();
        $.post("/Commerce/UpdateCategories", { categories: (paramaters ? paramaters : []), profileId: profileId }, function (Response) {
            if (Response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function jumpTo(id) {
    var e = document.getElementById(id);
    $(window).scrollTop(e.offsetTop);
}
function getBusinessLocations() {
    var $tblBusinessLocations = $('#tblBusinessLocations');
    $tblBusinessLocations.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblBusinessLocations.LoadingOverlay("show");
        } else {
            $tblBusinessLocations.LoadingOverlay("hide", true);
            var info = $('#tblBusinessLocations').DataTable().page.info();
            if (info && info.recordsTotal > 0)
                $('.locations-highlighted-text').hide();
            else
                $('.locations-highlighted-text').show();
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        autoWidth: true,
        searchDelay: 800,
        pageLength: 10,
        deferLoading: 30,
        order: [[0, "asc"]],
        ajax: { "url": "/Commerce/GetBusinessLocations" },
        columns: [
            { "data": "Name", "orderable": true },
            { "data": "Address", "orderable": true },
            { "data": "Geolocated", "orderable": true, "render": function (data, type, row, meta) { return data ? 'Yes' : 'No'; } },
            {
                "data": "IsIncludeInProfile",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    return '<input class="chktoggle" data-toggle="toggle" data-onstyle="success" onchange="includeLocationInProfile(' + row.Id + ',this)" ' + (data ? "checked" : "") + '  type="checkbox">';
                }
            }
        ],
        drawCallback: function (settings) {
            $('.chktoggle').bootstrapToggle();
        }
    });
}
function reloadBusinessLocations() {
    if ($.fn.DataTable.isDataTable("#tblBusinessLocations"))
        $("#tblBusinessLocations").DataTable().ajax.reload();
    else {
        wto = setInterval(function () {
            if ($.fn.DataTable.isDataTable("#tblBusinessLocations")) {
                $("#tblBusinessLocations").DataTable().ajax.reload();
                clearInterval(wto);
            }
        }, 1000);
    }

}
function includeLocationInProfile(lid, elm) {
    $.post("/Commerce/SetLocationIncludeInProfile", { locationId: lid, isIncludeInProfile: $(elm).prop('checked') }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Profile & config");
        } else if (!response.result && response.msg) {
            cleanBookNotification.success(_L(response.msg), "Profile & config");
        } else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Profile & config");
    });
}
function loadB2CBusinessCatalogues(dmkey) {
    var $businessCatalogues = $('#business-catalogues');
    $businessCatalogues.empty();
    $businessCatalogues.LoadingOverlay('show');
    $businessCatalogues.load("/B2C/LoadB2CBusinessCatalogues", { domainKey: dmkey, isLoadAll: true, view: "_TableBusinesCatalogues" }, function () {
        $('#tblBusinessCatalogues').DataTable({
            responsive: true,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            },
            drawCallback: function (settings) {
                $('#tblBusinessCatalogues input[data-toggle="toggle"]').bootstrapToggle();
            }
        });

        //$('#tblBusinessCatalogues').DataTable().columns(0).search($(this).val()).draw();


        $businessCatalogues.LoadingOverlay('hide', true);
        var info = $('#tblBusinessCatalogues').DataTable().page.info();
        if (info && info.recordsTotal > 0)
            $('.catalogues-highlighted-text').hide();
        else
            $('.catalogues-highlighted-text').show();
    });
}
function includeCatalogInProfile(catid, elm) {
    $.post("/Commerce/SetCatalogIncludeInProfile", { catId: catid, isIncludeInProfile: $(elm).prop('checked') }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Profile & config");
        } else if (!response.result && response.msg) {
            cleanBookNotification.success(_L(response.msg), "Profile & config");
        }
        else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Profile & config");
    });
}
function getBusinessProfilePages() {
    var $tblBusinessProfilePages = $('#tblBusinessProfilePages');
    $tblBusinessProfilePages.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblBusinessProfilePages.LoadingOverlay("show");
        } else {
            $tblBusinessProfilePages.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        searchDelay: 800,
        pageLength: 10,
        deferLoading: 30,
        ajax: {
            "url": "/ProfilePage/GetBusinessProfilePages",
            "type": 'Get',
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = {
                    keyword: $('#profile-pages input[name=keyword]').val(),
                    status: $('#profile-pages select[name=status]').val()
                };
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "data": "PageTitle", "orderable": false },
            { "data": "Created", "orderable": false },
            { "data": "DisplayOrder", "orderable": true },
            {
                "data": "Status",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '';
                    if (data == 'IsActive')
                        _htmlStatus += '<span class="label label-lg label-success">Active</span>';
                    else if (data == 'IsInActive')
                        _htmlStatus += '<span class="label label-lg label-warning">Inactive</span>';
                    else
                        _htmlStatus += '<span class="label label-lg label-info">Draft</span>';
                    return _htmlStatus;
                }
            },
            {
                "data": "Key",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '<div class="btn-group"><button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    _htmlStatus += 'Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                    _htmlStatus += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    _htmlStatus += '<li><a href="/ProfilePage/BusinessPageBuilder?pageKey=' + data + '">Edit</a></li>';
                    if (meta.row > 0) {
                        _htmlStatus += '<li class="dtMoveUp"><a href="javascript:void(0)">Move up</a></li>';
                    }
                    _htmlStatus += '<li class="dtMoveDown"><a href="javascript:void(0)">Move down</a></li>';
                    if (row.Status == 'IsActive') {
                        _htmlStatus += '<li><a href="/ProfilePage/BusinessPagePreview?pageKey=' + data + '" target="_blank">Preview</a></li>';
                        _htmlStatus += '<li><a href="javascript:void(0)" onclick="setBusinessPageStatus(\'' + data + '\',\'IsInActive\')">Unpublish</a></li>';
                    }
                    else
                        _htmlStatus += '<li><a href="javascript:void(0)" onclick="setBusinessPageStatus(\'' + data + '\',\'IsActive\')">Publish</a></li>';
                    _htmlStatus += '<li><a href="javascript:void(0)" onclick="deleteProfilePage(\'' + data +'\');">Delete</a></li>';
                    _htmlStatus += '</div>';
                    return _htmlStatus;
                }
            }
        ],
        drawCallback: function (settings) {
            $('#tblBusinessProfilePages tr:last .dtMoveDown').remove();

            // Remove previous binding before adding it
            $('.dtMoveUp').unbind('click');
            $('.dtMoveDown').unbind('click');

            // Bind clicks to functions
            $('.dtMoveUp').click(moveUp);
            $('.dtMoveDown').click(moveDown);
        }
    });
}
// Move the row up
function moveUp() {
    var tr = $(this).parents('tr');
    moveRow(tr, 'up');
}

// Move the row down
function moveDown() {
    var tr = $(this).parents('tr');
    moveRow(tr, 'down');
}

// Move up or down (depending...)
function moveRow(row, direction){
    var table = $("#tblBusinessProfilePages").DataTable();
    var index = table.row(row).index();

    var order = -1;
    if (direction === 'down') {
        order = 1;
    }

    var data1 = table.row(index).data();
    //data1.order += order;

    //var data2 = table.row(index + order).data();
    //data2.order += -order;

    //table.row(index).data(data2);
    //table.row(index + order).data(data1);
    //if (data1.DisplayOrder == data2.DisplayOrder) {
    //    data2.DisplayOrder = parseInt(data2.DisplayOrder) + order;
    //}
    //var _pages = [
    //    { Key: data1.Key, DisplayOrder: data2.DisplayOrder },
    //    { Key: data2.Key, DisplayOrder: data1.DisplayOrder }
    //];
    $.post("/ProfilePage/UpdateDisplayOrderPageBuilder", { key: data1.Key, direction: order }, function (Response) {
        if (Response.result) {
            table.page(0).draw(false);
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function reloadBusinessProfilePages() {
    if ($.fn.DataTable.isDataTable("#tblBusinessProfilePages"))
        $("#tblBusinessProfilePages").DataTable().ajax.reload();
    else {
        wto = setInterval(function () {
            if ($.fn.DataTable.isDataTable("#tblBusinessProfilePages")) {
                $("#tblBusinessProfilePages").DataTable().ajax.reload();
                clearInterval(wto);
            }
        }, 2000);
    }

}
function setBusinessPageStatus(pageKey, status) {
    $.post("/ProfilePage/SetBusinessPageStatus", { pageKey: pageKey, status: status }, function (Response) {
        if (Response.result) {
            reloadBusinessProfilePages();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function deleteProfilePage(pageKey) {
    var result = confirm('Are you sure you want to delete this page?');
    if (result) {
        $.post("/ProfilePage/DeleteProfilePage", { pageKey: pageKey }, function (Response) {
            if (Response.result) {
                reloadBusinessProfilePages();
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}

function updateDefaultManagers() {
    var profileId = $('#hdfProfileId').val();
    var _valmanagerb2b = $('select[name=UserIdB2BRelationshipManagers]').val();
    var _valmanagerb2c = $('select[name=UserIdB2CRelationshipManagers]').val();
    var activedB2b = $('input[name=IsDisplayedInB2BListings]').prop('checked');
    var activedB2c = $('input[name=IsDisplayedInB2CListings]').prop('checked');
    var paramaters = {
        Id: profileId,
        IsDisplayedInB2BListings: activedB2b,
        IsDisplayedInB2CListings: activedB2c,
        UserIdB2BRelationshipManagers: _valmanagerb2b ? _valmanagerb2b : [],
        UserIdB2CRelationshipManagers: _valmanagerb2c ? _valmanagerb2c : []
    };
    if (profileId > 0) {
        $.LoadingOverlay("show");
        $.post("/Commerce/UpdateDefaultManagers", paramaters, function (response) {
            LoadingOverlayEnd();
            if (response.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Profile & config");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Profile & config");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Profile & config");
        });
    } else {
        cleanBookNotification.warning(_L('WARNING_MSG_SAVEBUSINESSPROFILE'), "Profile & config");
    }
}
//Tree view selectaccount
function initSelectedAccount() { }
function closeSelected() {
    if (BKAccount.Id) {
        $(".accountInfo").empty();
        $(".accountInfo").append(BKAccount.Name);
    } else {
        $(".accountInfo").empty();
    }
    if ($(".accountInfo").text().length > 0) {
        $(".addbtnaccount").attr("style", "display:none;");
        $(".editbtnaccount").removeAttr("style");
    } else if ($(".accountInfo").text().length === 0) {
        $(".editbtnaccount").attr("style", "display:none;");
        $(".addbtnaccount").removeAttr("style");
    }
};
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    BKAccount.Id = id;
    BKAccount.Name = name;
    //Add cash/bank
    if ($("#accountId").length > 0)
        $("#accountId").val(id);
    //Add a Driver
    if ($("#hdfAccountId").length > 0)
        $("#hdfAccountId").val(id);
    //Add a Contact
    if ($("#hdfcontactaccountId").length > 0)
        $("#hdfcontactaccountId").val(id);
    closeSelected();
    $("#app-bookkeeping-treeview").modal("hide");
};
function showChangeAccount() {
    setTimeout(function () {
        CollapseAccount();
    },
        1);

};
function CollapseAccount() {
    $(".jstree").jstree("close_all");
};
function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    }, 1);
};
//end


function PublishCatalogInProfile(catid, elm) {
    $.post("/Commerce/PublishCatalogInProfile", { catId: catid, isPublish: $(elm).prop('checked') }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Profile & config");
        } else if (!response.result && response.msg) {
            cleanBookNotification.success(_L(response.msg), "Profile & config");
        }
        else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Profile & config");
    });
}