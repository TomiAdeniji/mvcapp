
var isBusy = false;
var previous_shown = false;
var $qbiclesFileTypes = [];
var $ReAttachments = [];
var $ReAttachmentExisted = [];
var $ReUpdatedAttachment = [];
$(document).ready(function () {

    $qbiclesFileTypes = [];
    $.ajax({
        type: 'post',
        url: '/FileTypes/GetFileTypes',
        dataType: 'json',
        success: function (response) {
            $qbiclesFileTypes = response;
        },
        error: function (er) {
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
});


function initHighlightBusiness() {
    initEventHighlightTable();
    initJobHighlightTable();
    initNewsHighlightTable();
    initArticleHighlightTable();
    initKnowledgeHighlightTable();
    initRealestateHighlightTable();
    initEventListingHLTable();
    initJobListingHLTable();
    initRealEstateListingHLTable();
}

function resetPageLoad() {
    $(window).scrollTop(0);
    $pageIndex = 0;
    $("#post-list").empty();
}

highlightPostAddShow = function (postId) {
    var _url = "/HighlightPost/HighlightAddView?highlightPostId=" + postId;
    LoadingOverlay();
    $("#highlight-add").empty();
    $("#highlight-add").load(_url);
    $("#highlight-add").modal("show");
    LoadingOverlayEnd();
}

function initPostAddShow() {
    var $addpostfrm = $("#addpostform");
    $addpostfrm.validate({
        rules: {
            type: {
                required: true
            },
            title: {
                required: true
            },
            content: {
                required: true
            }
        }
    });

    $addpostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;

        //Process with uploading image
        var files = document.getElementById("highlight-img").files;

        if ($addpostfrm.valid()) {
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("highlight-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        SaveHighlightPost(s3Object);
                    }
                });
            } else {
                SaveHighlightPost(null);
            }
        }
    });
}

function SaveHighlightPost(uploadModel) {
    //Get data
    var id = $("#post-id").val();
    var type = $("#highlight-type").val();
    var title = $("#highlight-title").val();
    var content = $("#highlight-content").val();
    var link = $("#highlight-link").val();
    var tags = $("#highlight-tags").val().trim().split(" ");

    var highlightpost = {
        Id: id,
        Type: type,
        Title: title,
        Content: content,
        HyperLink: link
    }

    var _url = "/HighlightPost/AddEditHighlightPost";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            highlightPost: highlightpost,
            tags: tags,
            uploadModel: uploadModel
        },
        success: function (response) {
            if (response.result) {
                if (id > 0) {
                    cleanBookNotification.updateSuccess();
                } else {
                    cleanBookNotification.createSuccess();
                    previous_shown = false;
                    $("#taken-total").val(Number($("#taken-total").val()) + 1);
                }

                LoadMoreHighlightPost(true);
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");

            }
            $("#highlight-add").modal("hide");
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            $("#highlight-add").modal("hide");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

DeleteHighlightPost = function (postId, tableId) {
    var deleteConfirmed = confirm('Are you sure you want to remove this Highlight?');
    if (deleteConfirmed) {
        var _url = "/HighlightPost/DeleteHighlightPost?highlightPostId=" + postId;
        LoadingOverlay();
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.removeSuccess();
                    reloadDataTable(tableId);
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
}

function CancelAdvancedSearch(ev) {
    var container = $(ev).closest('.adv-search');
    $(container).find('input :not(.multiselect)').each(function (index) { this.value = ""; });
    $(container).find('.select2').each(function (index) { $(this).val(" ").trigger("change"); });
    $(container).find("input[type=text]").each(function (index) { $(this).val("") });
    $('#re-adv-bedroomsearch, #re-adv-bathroomsearch').val("0").trigger("change");

    $("#re-adv-proptypesearch").val(['0']);
    $("#re-adv-proptypesearch").multiselect('refresh');
    $("#re-adv-propertysearch").val([]);
    $("#re-adv-propertysearch").multiselect('refresh');
    $('.adv-search').slideUp();
    LoadMoreHighlightPost(true);
}
//var $pageIndex = 1;
function LoadMoreHighlightPost(isLoadMore) {
    var filteringTypeActive = $(".ptags > li").find('.active');
    var tagStr = $('.followingblock > .cattags > li > .active').text().split("#");
    tagStr.shift();
    
    var hlShowType = 0;
    var lsShowType = 0;
    if (filteringTypeActive && filteringTypeActive.length > 0) {
        hlShowType = $(filteringTypeActive[0]).attr('hlType');
        lsShowType = $(filteringTypeActive[0]).attr('lsType');
    }

    var dmkey = "";
    var creatorFilterElement = $(".followed-domains").find(".filterby");
    if (creatorFilterElement && creatorFilterElement.length > 0) {
        dmkey = $(creatorFilterElement[0]).attr("domain-key");
    }

    var isBookmarkedShow = $(".highltabs > .bookmarked").hasClass("active");
    var isFlaggedShow = $(".highltabs > .flagged").hasClass("active");

    //Load params by type
    //Highlight type: 2: News, 3:Knowledge, 4:Listing
    //Listing type: 1: Event, 2: Job, 3: Real Estate
    var keyStr = "";
    var locationName = "";
    var eventDateRange = "";
    var newsPublishedDate = "";
    var rePropertyTypes = [];
    var reBedRoomNum = 0;
    var reBathRoomNum = 0;
    var reProperty = [];
    var areaId = 0;
    //Listing advance search params
    if (hlShowType == 4) {
        //Event advance search params
        if (lsShowType == 1 && $("#event").is(":visible")) {
            keyStr = $("#event-adv-keysearch").val();
            locationName = $("#event-adv-countrySearch").val();
            eventDateRange = $("#event-adv-searchdate").val();
            if ($("#event-adv-areaSearch").val() != null) {
                areaId = $("#event-adv-areaSearch").val()
            }
        } else if (lsShowType == 2 && $("#job").is(":visible")) {
            keyStr = $("#job-adv-searchkey").val();
            locationName = $("#job-adv-locationsearch").val();
            if ($("#job-adv-areaSearch").val() != null) {
                areaId = $("#job-adv-areaSearch").val();
            }
        } else if (lsShowType == 3 && $("#realestate").is(":visible")) {
            keyStr = $("#re-adv-keysearch").val();
            locationName = $("#re-adv-locationsearch").val();
            rePropertyTypes = $("#re-adv-proptypesearch").val();
            reBedRoomNum = $("#re-adv-bedroomsearch").val();
            reBathRoomNum = $("#re-adv-bathroomsearch").val();
            reProperty = $("#re-adv-propertysearch").val();
            if ($("#re-adv-areaSearch").val() != null) {
                areaId = $("#re-adv-areaSearch").val();
            }
        }
    } else if (hlShowType == 3 && $("#knowledge").is(":visible")) {
        //Knowledge advance search params
        keyStr = $("#knowledge-adv-searchkey").val();
        locationName = $("#knowledge-adv-locationsearch").val();
    } else if (hlShowType == 2 && $("#news").is(":visible")) {
        //News advance search params
        keyStr = $("#news-adv-keysearch").val();
        newsPublishedDate = $("#news-adv-datesearch").val();
    }


    var _url = "/HighlightPost/LoadMoreHighlightPost";

    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        data: {
            keySearch: keyStr,
            tagsSearch: tagStr,
            pageIndex: $pageIndex,
            typeShowed: hlShowType,
            lsTypeSearch: lsShowType,
            domainKey: dmkey,
            isBookmarked: isBookmarkedShow,
            isFlagged: isFlaggedShow,
            countryName: locationName,
            eventDateRange: eventDateRange,
            newsPublishedDate: newsPublishedDate,
            rePropTypeIds: rePropertyTypes,
            reBedroomNumber: reBedRoomNum,
            reBathroomNumber: reBathRoomNum,
            rePropertyIds: reProperty,
            areaId: areaId
        },
        async: false,
        beforeSend: function (xhr) {
            isServerBusy = true;
        },
        success: function (data) {
            $("#previous").html('');
            if ($("#previous").length === 0) {
                $("#post-list").append('<div id="previous"></div>');
            }
            $(data.ModelString).insertBefore("#previous");

            if (isLoadMore && $pageIndex < data.TotalPage) {
                $pageIndex += 1;
                previous_shown = false;
            } else if ($pageIndex >= data.TotalPage) {
                previous_shown = true;
            }
        }
    }).always(function () {
    });
}

function Timeline() {
    $(window).scrollTop(0);
    $(window).scroll(function () {
        if ($(window).scrollTop() >= ($(document).height() - $(window).height() - 100)) {
            if (previous_shown == false) {
                loadingNew();
                setTimeout(function () {
                    LoadMoreHighlightPost(true);
                }, 100);

                previous_shown = true;
                return previous_shown;
            }
        }
    });
}

function loadingNew() {
    if ($("#previous").length === 0) {
        $("#post-list").append('<div id="previous"></div>');
    }
    if ($('#previous'))
        $('#previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
}

function PostLikeProcess(postId) {
    LoadingOverlay();

    var _url = "/HighlightPost/LikePostProcess?highlightPostId=" + postId;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                var heartIcon = $("#heart-icon-" + postId);
                if (heartIcon.hasClass("red")) {
                    heartIcon.removeClass("red").removeClass("fa-heart").addClass("fa-heart-o");
                } else {
                    heartIcon.addClass("red").removeClass("fa-heart-o").addClass("fa-heart");
                }

                $("#like-time-" + postId).text(response.Object + " people love this")
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

//Init DataTables
function initNewsHighlightTable() {
    $("#hlnews-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hlnews-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hlnews-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#news-searchkey").val(),
                    "searchStatus": $("#news-searchstatus").val(),
                    "searchType": $("#hlnews-type").val()
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel"
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showNewsAddView(' + row.Id + ')" data-toggle="modal">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hlnews-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hlnews-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}
function initArticleHighlightTable() {
    $("#hlarticle-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hlarticle-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hlarticle-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#article-searchkey").val(),
                    "searchStatus": $("#article-searchstatus").val(),
                    "searchType": $("#hlarticle-type").val()
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showArticleAddView(' + row.Id + ')">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hlarticle-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hlarticle-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}
function initKnowledgeHighlightTable() {
    $("#hlknowledge-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hlknowledge-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hlknowledge-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#knowledge-searchkey").val(),
                    "searchStatus": $("#knowledge-searchstatus").val(),
                    "searchType": $("#hlknowledge-type").val()
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showKnowledgeAddView(' + row.Id + ')" data-toggle="modal">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hlknowledge-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hlknowledge-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}

function initEventHighlightTable() {
    $("#hlevent-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hlevent-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hlevent-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',        
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                var _event = $('#listingevent-type').val();
                return $.extend({}, d, {
                    "searchKey": $("#event-searchkey").val(),
                    "searchStatus": $("#event-searchstatus").val(),
                    "searchType": $("#hllisting-type").val(),
                    "searchListingTypesStr": JSON.stringify([_event])
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Reference",
                data: "Reference",
                orderable: false
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showEventAddView(' + row.Id + ')" data-toggle="modal">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hlevent-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hlevent-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}
function initJobHighlightTable() {
    $("#hljob-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hljob-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hljob-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                var _job = $('#listingjob-type').val();
                return $.extend({}, d, {
                    "searchKey": $("#job-searchkey").val(),
                    "searchStatus": $("#job-searchstatus").val(),
                    "searchType": $("#hllisting-type").val(),
                    "searchListingTypesStr": JSON.stringify([_job])
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Reference",
                data: "Reference",
                orderable: false
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showJobAddView(' + row.Id + ')" data-toggle="modal">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hljob-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hljob-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}
function initRealestateHighlightTable() {
    $("#hlrealestate-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#hlrealestate-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#hlrealestate-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/LoadingHighlightPosts',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                var _realEstate = $('#listingrealestate-type').val();
                return $.extend({}, d, {
                    "searchKey": $("#realestate-searchkey").val(),
                    "searchStatus": $("#realestate-searchstatus").val(),
                    "searchType": $("#hllisting-type").val(),
                    "searchListingTypesStr": JSON.stringify([_realEstate])
                });
            }
        },
        "columns": [
            {
                name: "Title",
                data: "Title",
                orderable: true
            },
            {
                name: "Property ref",
                data: "Reference",
                orderable: false
            },
            {
                name: "Added",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Status",
                data: "StatusLabel",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<div class="btn-group">';
                    htmlString += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    htmlString += 'Options &nbsp; <i class="fa fa-angle-down"></i>';
                    htmlString += '</button>';
                    htmlString += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    htmlString += '<li><a href="#" onclick="showRealEstateAddView(' + row.Id + ')" data-toggle="modal">Edit</a></li>';
                    if (row.IsPublished) {
                        htmlString += '<li><a href="#" onclick="unpublishPost(' + row.Id + ', \'hlrealestate-table\')">Unpublish (can\'t be undone)</a></li>';
                    }
                    htmlString += '<li><a href="#" onclick="DeleteHighlightPost(' + row.Id + ', \'hlrealestate-table\')">Delete</a></li>';
                    htmlString += '</ul>';
                    htmlString += '</div>';
                    return htmlString;
                }
            }]
    });
}
function initEventListingHLTable() {
    $("#listing-event-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#listing-event-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#listing-event-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/ListingPostDataTableContent',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#event-searchkey-1").val(),
                    "searchType": $("#listingevent-type").val(),
                    "searchRef": $("#event-searchref").val(),
                });
            }
        },
        "columns": [
            {
                name: "Person",
                data:"PersonName",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<a href="my-contact-profile.php" target="_blank">';
                    htmlString += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + row.PersonImageUri + '\');"></div>';
                    htmlString += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + row.PersonName + '</div>';
                    htmlString += '<div class="clearfix"></div>';
                    htmlString += '</a>';
                    return htmlString;
                }
            },
            {
                name: "Title",
                data: "Title",
                orderable: true,
                visible: false
            },
            {
                name: "Event ref",
                data: "PostReference",
                orderable: true
            },
            {
                name: "Date & time",
                data: "CreateDateString",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    if (!row.HasB2CChat) {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId +'\')" class="btn btn-info">Connect</button>';
                    } else {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId +'\')" class="btn btn-primary">Chat in B2C</button>';
                    }
                    return htmlString;
                }
            }]
    });
}
function initJobListingHLTable() {
    $("#listing-job-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#listing-job-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#listing-job-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/ListingPostDataTableContent',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#job-searchkey-1").val(),
                    "searchType": $("#listingjob-type").val(),
                    "searchRef": $("#job-searchref").val(),
                });
            }
        },
        "columns": [
            {
                name: "Person",
                data: "PersonName",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<a href="my-contact-profile.php" target="_blank">';
                    htmlString += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + row.PersonImageUri + '\');"></div>';
                    htmlString += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + row.PersonName + '</div>';
                    htmlString += '<div class="clearfix"></div>';
                    htmlString += '</a>';
                    return htmlString;
                }
            },
            {
                name: "Title",
                data: "Title",
                orderable: true,
                visible: false
            },
            {
                name: "Property ref",
                data: "PostReference",
                orderable: true
            },
            {
                name: "Date & time",
                data: "CreateDateString",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    if (!row.HasB2CChat) {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId + '\')" class="btn btn-info">Connect</button>';
                    } else {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId + '\')" class="btn btn-primary">Chat in B2C</button>';
                    }
                    return htmlString;
                }
            }]
    });

}
function initRealEstateListingHLTable() {
    $("#listing-realestate-table").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#listing-realestate-table').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#listing-realestate-table').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "bDestroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "lengthMenu": "_MENU_ &nbsp; per page"
        },
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "createdRow": function (row, data, dataIndex) {
            if (data.isCheckpoint) {
                $(row).removeClass().addClass("checkpoint even");
            }
        },
        "ajax": {
            "url": '/HighlightPost/ListingPostDataTableContent',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "searchKey": $("#realestate-searchkey-1").val(),
                    "searchType": $("#listingrealestate-type").val(),
                    "searchRef": $("#realestate-searchref").val(),
                });
            }
        },
        "columns": [
            {
                name: "Person",
                data: "PersonName",
                render: function (value, type, row) {
                    var htmlString = "";
                    htmlString += '<a href="my-contact-profile.php" target="_blank">';
                    htmlString += '<div class="table-avatar mini pull-left" style="background-image: url(\'' + row.PersonImageUri + '\');"></div>';
                    htmlString += '<div class="avatar-name pull-left" style="color: #333; line-height: 4; padding-left: 15px;">' + row.PersonName + '</div>';
                    htmlString += '<div class="clearfix"></div>';
                    htmlString += '</a>';
                    return htmlString;
                }
            },
            {
                name: "Title",
                data: "Title",
                orderable: true,
                visible: false
            },
            {
                name: "Property ref",
                data: "PostReference",
                orderable: true
            },
            {
                name: "Date & time",
                data: "CreateDateString",
                orderable: true
            },
            {
                name: "Option",
                render: function (value, type, row) {
                    var htmlString = "";
                    if (!row.HasB2CChat) {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId + '\')" class="btn btn-info">Connect</button>';
                    } else {
                        htmlString += '<button onclick="connectB2CFromListing(\'' + row.FlaggedUserId + '\', \'' + row.BusinessProfileId + '\')" class="btn btn-primary">Chat in B2C</button>';
                    }
                    return htmlString;
                }
            }]
    });
}
//End: Init DataTables

//Show Adding Views
function showNewsAddView(newsId) {
    var _url = "/HighlightPost/NewsHighlightAddView?newsPostId=" + newsId;
    var _modalId = "#highlight-news-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url);
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function showArticleAddView(articleId) {
    var _url = "/HighlightPost/ArticleHighlightAddView?articlePostId=" + articleId;
    var _modalId = "#highlight-article-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url);
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function showKnowledgeAddView(knowledgeId) {
    var _url = "/HighlightPost/KnowledgeHighlightAddView?knowledgePostId=" + knowledgeId;
    var _modalId = "#highlight-knowledge-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url);
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function showEventAddView(listingId) {
    var _url = "/HighlightPost/EventHighlightAddView?listingPostId=" + listingId;
    var _modalId = "#highlight-event-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url, function () {
        initPlugins(_modalId);
        initAddingListingEventForm();
    });
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function showJobAddView(listingId) {
    var _url = "/HighlightPost/JobHighlightAddView?listingPostId=" + listingId;
    var _modalId = "#highlight-job-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url, function () {
        initPlugins(_modalId);
        initAddingListingJobForm();
    });
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function showRealEstateAddView(listingId) {
    var _url = "/HighlightPost/RealEstateHighlightAddView?listingPostId=" + listingId;
    var _modalId = "#highlight-realestate-add";
    LoadingOverlay();
    $(_modalId).empty();
    $(_modalId).load(_url, function () {
        initPlugins(_modalId);
        initAddingListingRealEstateForm();
    });
    $(_modalId).modal("show");
    LoadingOverlayEnd();
}
function initPlugins(modalId) {
    $(modalId + " .select2").select2({ placeholder: "Please select" });
    $(modalId +" .select2tag").select2({
        placeholder: 'Please select',
        tags: true
    });
    $(modalId +' .checkmulti').multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(modalId +' .singledateandtime').daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
}
//End: Show Adding Views

//Init Adding Form
function initAddingNewsForm() {
    var $addnewspostfrm = $("#add-newspost-form");
    $addnewspostfrm.validate({
        rules: {
            title: {
                required: true
            },
            hyperlink: {
                required: true
            },
            summary: {
                required: true
            }
        }
    });

    $addnewspostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Process with uploading image
        var files = document.getElementById("news-img").files;

        if ($addnewspostfrm.valid()) {
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("news-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        LoadingOverlayEnd();
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveNewsPost(s3Object);
                    }
                });
            } else {
                saveNewsPost(null);
            }
        } else {
            LoadingOverlayEnd();
        }
    });
}
function initAddingArticleForm() {
    var $addarticlepostfrm = $("#add-articlepost-form");
    $addarticlepostfrm.validate({
        rules: {
            title: {
                required: true
            },
            summary: {
                required: true
            }
        }
    });

    $addarticlepostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Process with uploading image
        var files = document.getElementById("article-img").files;

        if ($addarticlepostfrm.valid()) {
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("article-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        LoadingOverlayEnd();
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveArticlePost(s3Object);
                    }
                });
            } else {
                saveArticlePost(null);
            }
        }
    });
}
function initAddingKnowledgeForm() {
    var $addknowledgepostfrm = $("#add-knowledgepost-form");
    $addknowledgepostfrm.validate({
        rules: {
            title: {
                required: true
            },
            content: {
                required: true
            }
        }
    });

    $addknowledgepostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Process with uploading image
        var files = document.getElementById("knowledge-img").files;

        if ($addknowledgepostfrm.valid()) {
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("knowledge-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        LoadingOverlayEnd();
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveKnowledgePost(s3Object);
                    }
                });
            } else {
                saveKnowledgePost(null);
            }
        } else {
            LoadingOverlayEnd();
        }
    });
}
function initAddingListingEventForm() {
    var $addeventpostfrm = $("#event-form");
    $addeventpostfrm.validate({
        rules: {
            reference: {
                required: true
            },
            title: {
                required: true
            },
            summary: {
                required: true
            }
        }
    });

    $addeventpostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Process with uploading image
        var files = document.getElementById("event-img").files;

        if ($addeventpostfrm.valid()) {
            // Start Date MUST < END Date
            var startTime = Date.parse($("#event-startdate").val());
            var endTime = Date.parse($("#event-enddate").val());
            if (startTime >= endTime) {
                $addeventpostfrm.validate().showErrors({ "start-date": "The start time must be less than the end time" });
                return;
            }

            // End date MUST > today
            var todayTime = Date.parse(new Date().toLocaleDateString());
            if (endTime < todayTime) {
                $addeventpostfrm.validate().showErrors({ "end-date": "The end time must be greater than today" });
                return;
            }


            //Check for reference duplication
            //if (checkRefDuplication($("#event-ref").val(), 1)) {
            //    $addeventpostfrm.validate().showErrors({ reference: "Event Post reference existed." });
            //    return false;
            //}
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("event-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        LoadingOverlayEnd();
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveListingEventPost(s3Object);
                    }
                });
            } else {
                saveListingEventPost(null);
            }
        }
    });
}
function initAddingListingJobForm() {
    var $addjobpostfrm = $("#job-form");
    $addjobpostfrm.validate({
        rules: {
            reference: {
                required: true
            },
            title: {
                required: true
            },
            summary: {
                required: true
            },
            skill: {
                required: true
            }
        }
    });

    $addjobpostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Check for reference duplication
        //if (checkRefDuplication($("#job-ref").val(), 2)) {
        //    $addjobpostfrm.validate().showErrors({ reference: "Job Post reference existed." });
        //    return false;
        //}

        //Process with uploading image
        var files = document.getElementById("job-img").files;

        if ($addjobpostfrm.valid()) {
            // Closing date must be > today
            var closingTime = Date.parse($("#job-closingdate").val());
            var todayTime = Date.parse(new Date().toLocaleDateString());
            if (closingTime < todayTime) {
                $addjobpostfrm.validate().showErrors({ "closing-date": "The closing time must be greater than today" });
                LoadingOverlayEnd();
                return;
            }

            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("job-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        LoadingOverlayEnd();
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveListingJobPost(s3Object);
                    }
                });
            } else {
                saveListingJobPost(null);
            }
        }
    });
}
function initAddingListingRealEstateForm() {
    var $addrealestatepostfrm = $("#realestate-form");
    $addrealestatepostfrm.validate({
        rules: {
            reference: {
                required: true
            },
            title: {
                required: true
            },
            summary: {
                required: true
            }
        }
    });

    $addrealestatepostfrm.submit(function (e) {
        e.preventDefault();
        if (isBusy) {
            LoadingOverlayEnd();
            return;
        }

        //Check for reference duplication
        //if (checkRefDuplication($("#realestate-ref").val(), 2)) {
        //    $addrealestatepostfrm.validate().showErrors({ reference: "Real Estate Post reference existed." });
        //    return false;
        //}

        //Process with uploading image
        if ($addrealestatepostfrm.valid()) {
            LoadingOverlay();
            if ($ReAttachments.length > 0) {
                UploadBatchMediasS3ClientSide($ReAttachments).then(function () {
                    saveListingRealEstatePost();
                });
            } else {
                saveListingRealEstatePost();
            }
        }
    });
}
//End: Init Adding Form

//Saving post Functions
function saveNewsPost(uploadModel) {
    //Get data
    var newsPost = {
        Id: $("#news-id").val(),
        Title: $("#news-title").val(),
        NewsHyperLink: $("#news-link").val(),
        Content: $("#news-summary").val(),
        NewsCitation: $("#news-citation").val()
    }
    var tags = $("#news-tag").val();
    var _url = "/HighlightPost/SaveNewsPost";

    $.ajax({
        method: 'POST',
        url: _url,
        dataType: 'JSON',
        data: {
            newsPost: newsPost,
            tags: tags,
            uploadModel: uploadModel
        },
        beforeSend: function () {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save News post successfully.", "Qbicles");
                $("#highlight-news-add").modal("hide");
                reloadDataTable("hlnews-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
function saveArticlePost(uploadModel) {
    //Get data
    var articlePost = {
        Id: $("#article-id").val(),
        Title: $("#article-title").val(),
        Content: $("#article-summary").val(),
        ArticleBody: escape($("#newsletter-editor .ql-editor").html())
    }
    var tags = $("#article-tags").val();
    var _url = "/HighlightPost/SaveArticlePost";

    $.ajax({
        method: 'POST',
        url: _url,
        dataType: 'JSON',
        data: {
            articlePost: articlePost,
            tags: tags,
            content: JSON.stringify(escape($("#newsletter-editor .ql-editor").html())),
            uploadModel: uploadModel
        },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Article post successfully.", "Qbicles");
                $("#highlight-article-add").modal("hide");
                reloadDataTable("hlarticle-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
function saveKnowledgePost(uploadModel) {
    //Get data
    var post = {
        Id: $("#knowledge-id").val(),
        Title: $("#knowledge-title").val(),
        Content: $("#knowledge-content").val(),
        KnowledgeHyperlink: $("#knowledge-link").val(),
        KnowledgeCitation: $("#knowledge-citation").val()
    }
    var tags = $("#knowledge-tags").val();
    var _url = "/HighlightPost/SaveKnowledgePost";

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            knowledgePost: post,
            tags: tags,
            uploadModel: uploadModel,
            countryName: $("#knowledge-country").val()
        },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Knowledge post successfully.", "Qbicles");
                $("#highlight-knowledge-add").modal("hide");
                reloadDataTable("hlknowledge-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
//
function saveListingEventPost(uploadModel) {
    //Get data
    var post = {
        Id: $("#listing-event-id").val(),
        Reference: $("#event-ref").val(),
        Title: $("#event-title").val(),
        EventLocation: $("#event-location").val(),
        StartDate: $("#event-startdate").val(),
        EndDate: $("#event-enddate").val(),
        Content: $("#event-summary").val()
    }
    var tags = $("#event-tag").val();
    var _locationId = 0;
    if ($('#area-event').is(":visible")) {
        _locationId = $("#listing-event-location").val();
    };
    var _url = "/HighlightPost/SaveListingEventPost";
    
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            eventPost: post,
            uploadModel: uploadModel,
            tags: tags,
            locationId: _locationId,
            startDate: $("#event-startdate").val(),
            endDate: $("#event-enddate").val(),
            countryName: $("#listing-event-country").val()
        },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Listing Event successfully.", "Qbicles");
                $("#highlight-event-add").modal("hide");
                reloadDataTable("hlevent-table");
                reloadDataTable("listing-event-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
function saveListingJobPost(uploadModel) {
    //Get data
    var post = {
        Id: $("#listing-job-id").val(),
        Reference: $("#job-ref").val(),
        Title: $("#job-title").val(),
        Salary: $("#job-salary").val(),
        ClosingDate: $("#job-closingdate").val(),
        Content: $("#job-summary").val(),
        SkillRequired: $("#job-skillrequired").val()
    }
    var tags = $("#job-tag").val();
    var _locationId = 0;
    if ($('#area-job').is(":visible")) {
        _locationId = $("#listing-job-location").val();
    };

    var _url = "/HighlightPost/SaveListingJobPost";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            jobPost: post,
            uploadModel: uploadModel,
            tags: tags,
            locationId: _locationId,
            countryName: $("#listing-job-country").val()
        },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Listing Job successfully.", "Qbicles");
                $("#highlight-job-add").modal("hide");
                reloadDataTable("hljob-table");
                reloadDataTable("listing-job-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
function saveListingRealEstatePost() {
    
    //Get data
    var post = {
        Id: $("#listing-realestate-id").val(),
        Reference: $("#realestate-ref").val(),
        Title: $("#realestate-title").val(),
        Content: $("#realestate-summary").val(),
        BedRoomNum: $("#realestate-bedroom-number").val(),
        BathRoomNum: $("#realestate-bathroom-number").val(),
        PricingInfo: $("#realestate-pricing-info").val(),
    }
    var files = [];
    _.forEach($ReAttachments, function (file) {
        file.File = {};
        files.push(file);
    });
    var tags = $("#realestate-tag").val();
    var _locationId = 0;
    if ($('#area-realestate').is(":visible")) {
        _locationId = $("#listing-realestate-location").val();
    };

    var _url = "/HighlightPost/SaveListingRealEstatePost";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            realestatePost: post,
            countryName: $("#listing-realestate-country").val(),
            locationId: _locationId,
            tags: tags,
            propTypeId: $("#realestate-property-type").val(),
            propListIds: $("#realestate-properties").val(),
            postAttachments: files,
            existedAttachments: ($ReAttachmentExisted ? $ReAttachmentExisted:[]),
            updatedAttachments: ($ReUpdatedAttachment ? $ReUpdatedAttachment:[])
        },
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Real Estate Job successfully.", "Qbicles");
                $("#highlight-realestate-add").modal("hide");
                reloadDataTable("hlrealestate-table");
                reloadDataTable("listing-realestate-table");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
        isBusy = false;
    });
}
//End: Saving post Functions

function unpublishPost(postId, tableId) {
    var _url = "/HighlightPost/UnpublishHLPost?postId=" + postId;
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                reloadDataTable(tableId);
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

function showArticleContentModal(articleId) {
    var _url = "/HighlightPost/ArticleContentModal?articleId=" + articleId;
    $("#highlight-article").empty();
    $("#highlight-article").load(_url);
    $("#highlight-article").modal("show");
}

function showRealEstateInfoModal(rePostId) {
    var _url = "/HighlightPost/RealEstateInfoModal?rePostId=" + rePostId;
    $("#highlight-realestate-info").empty();
    $("#highlight-realestate-info").load(_url);
    $("#highlight-realestate-info").modal("show");
}

//Common functions
function reloadDataTable(dtId) {
    $("#" + dtId).DataTable().ajax.reload();
}
function checkRefDuplication(reference, lsType) {
    var _url = "/HighlightPost/CheckReferenceDuplication?reference=" + reference + "&lsType=" + lsType;
    var isDuplicated = false;
    //lsType: Event = 1, Job = 2,RealEstate = 3
    return $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (data) {
            if (data > 0)
                isDuplicated = true;
            else
                isDuplicated = false;
        },
        error: function () {
            cleanBookNotification.error("Check for reference duplication failed.", "Qbicles");
            isDuplicated = false;
        },
        complete: function () {
            return isDuplicated;
        }
    });
}
function updateDomainFollowStatus(dmkey, type) {
    LoadingOverlay();
    var _url = "/HighlightPost/UpdateHLDomainFollowStatus?domainKey=" + dmkey + "&type=" + type;
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                window.location.reload();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        }, error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function updateHLFlagStatus(hlPostId, type, ev) {
    LoadingOverlay();
    var _url = "/HighlightPost/UpdateHLFlagStatus?hlPostId=" + hlPostId + "&type=" + type;
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                var elements = $(ev).closest("article").find(".flag");
                var flaggedNumber = Number($('.flagged > a > span').text());
                if (type == 1) {
                    elements.each(function (index) {
                        if ($(elements[index]).hasClass('btn-primary')) {
                            $(elements[index]).removeClass('btn-primary').addClass('btn-danger').text('I\'m not interested');
                        } else {
                            $(elements[index]).addClass('red').text('I\'m not interested');
                        }
                        $(elements[index]).attr("onClick", "updateHLFlagStatus(" + hlPostId + ", 2, this)");
                    });
                    $('.flagged > a > span').text(Number(flaggedNumber + 1));
                } else if (type == 2) {
                    elements.each(function (index) {
                        if ($(elements[index]).hasClass('red')) {
                            $(elements[index]).removeClass('red').text('I\'m interested');
                        } else {
                            $(elements[index]).addClass('btn-primary').removeClass('btn-danger').text('I\'m interested');
                        }
                        $(elements[index]).attr("onClick", "updateHLFlagStatus(" + hlPostId + ", 1, this)");
                    });
                    if ($('.flagged').hasClass('active')) {
                        $(ev).closest("article").css('display', 'none');
                    }
                    $('.flagged > a > span').text(Number(flaggedNumber - 1));
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        }, error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        return false;
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function updateHLLikeStatus(hlPostId, type, ev) {
    LoadingOverlay();
    var _url = "/HighlightPost/UpdateHLLikeStatus?hlPostId=" + hlPostId + "&type=" + type;
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                var likeStr = $(ev).closest('article').find('.likecount').text();
                if (type == 1) {
                    $(ev).find("i").addClass("red").addClass("fa-heart").removeClass("fa-heart-o");
                    $(ev).attr("onClick", "updateHLLikeStatus(" + hlPostId + ", 2, this)");
                    if (!likeStr.startsWith('0')) {
                        $(ev).closest('article').find('.likecount').text("You and " + likeStr);
                    } else {
                        $(ev).closest('article').find('.likecount').text("You love this");
                    }
                } else if (type == 2) {
                    $(ev).find("i").removeClass("red").removeClass("fa-heart").addClass("fa-heart-o");
                    $(ev).attr("onClick", "updateHLLikeStatus(" + hlPostId + ", 1, this)");
                    if (likeStr.includes("You and")) {
                        likeStr = likeStr.substr(original.indexOf(" ") + 1).substr(original.indexOf(" ") + 1);
                        $(ev).closest('article').find('.likecount').text(likeStr);
                    } else {
                        $(ev).closest('article').find('.likecount').text("0 people love this");
                    }
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        }, error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        return false;
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function updateHLBookmarkStatus(hlPostId, type, ev) {
    LoadingOverlay();
    var _url = "/HighlightPost/UpdateHLBookmarkStatus?hlPostId=" + hlPostId + "&type=" + type;
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                var bookmarkedNum = Number($('.bookmarked > a > span').text());
                if (type == 1) {
                    $(ev).find("i").addClass("red").addClass("fa-bookmark").removeClass("fa-bookmark-o");
                    $(ev).attr("onClick", "updateHLBookmarkStatus(" + hlPostId + ", 2, this)");
                    $('.bookmarked > a > span').text(Number(bookmarkedNum + 1));
                } else if (type == 2) {
                    $(ev).find("i").removeClass("red").removeClass("fa-bookmark").addClass("fa-bookmark-o");
                    $(ev).attr("onClick", "updateHLBookmarkStatus(" + hlPostId + ", 1, this)");
                    if ($('.bookmarked').hasClass('active')) {
                        $(ev).closest("article").css('display', 'none');
                    }
                    $('.bookmarked > a > span').text(Number(bookmarkedNum - 1));
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        }, error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).then(function () {
        return false;
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function showAllTagsModal() {
    var _url = "/HighlightPost/AllTagsModalShow";
    var _chosenTags = $('.followingblock > .cattags > li > .active').text().split("#");
    _chosenTags.shift();
    $("#highlights-alltags").empty();
    $("#highlights-alltags").load(_url, { chosenTags: _chosenTags });
    $("#highlights-alltags").modal("show");
}
function filterTags() {
    var tagSearchKey = $("#tagSearchKey").val();
    var tagElements = $('#lstTags > li');
    tagElements.each(function () {
        if ($(this).find('span').text().includes(tagSearchKey)) {
            $(this).removeAttr('hidden');
        } else {
            $(this).attr('hidden', 'true');
        }
    });
}
function connectB2CFromListing(userId, profileId) {
    $.LoadingOverlay('show');
    $.post("/B2C/ConnectB2C", { userId: userId, profileId: profileId }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            _c2cQbiceId = response.Object;
            window.location.href = "/B2C";
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}

//Attachment Process
function CloseModalAttachment() {
    $("#attachments-view").modal("toggle");
}
function ConfirmAttachment(isEdit, closeId) {
    $ReAttachmentExisted = [];
    $ReUpdatedAttachment = [];
    $ReAttachments = [];
    var fileExtension = "";

    if (!isEdit) {
        var inputFiles = $(".add_attachment_row input.inputfile");
        for (var i = 0; i < inputFiles.length; i++) {

            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if ($("#inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("#inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $ReAttachments.push(attachmentAdd);

        }
    }
    else {
        var inputFiles2 = $(".edit_attachment_row input.inputfile");
        for (var j = 0; j < inputFiles2.length; j++) {
            if (inputFiles2[j].files.length > 0) {
                var fileEdit = inputFiles2[j].files[0];
                fileExtension = fileEdit.name.split('.').pop();
                var fileTypeEdit = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });//_.filter($qbiclesFileTypes, { 'Extension': fileExtension });
                var attachmentEdit = {
                    Id: GenerateUUID(),
                    Name: fileEdit.name,
                    Order: parseInt($("#file_id_" + (j + 1)).val()),
                };
                if ($("#inputfilename_edit" + (j + 1)).val() !== "") {
                    attachmentEdit.Name = $("#inputfilename_edit" + (j + 1)).val() + "." + fileExtension;
                }
                $ReUpdatedAttachment.push(attachmentEdit);

                var updatedAttach = {
                    Id: attachmentEdit.Id,
                    Name: attachmentEdit.Name,
                    Extension: fileExtension,
                    Size: fileEdit.size,
                    IconPath: fileTypeEdit.IconPath,
                    File: fileEdit,
                    Index: j + 1
                };
                $ReAttachments.push(updatedAttach);
            }
            else {
                //edit account attachment
                if ($("#file_id_" + (j + 1)).length > 0) {
                    $ReAttachmentExisted.push(
                        {
                            Order: parseInt($("#file_id_" + (j + 1)).val()),
                            Name: $("#inputfilename_edit" + (j + 1)).val(),
                            IconPath: $("#inputiconpath_edit" + (j + 1)).val(),
                            ImgKey: $("#imgkey_edit" + (j + 1)).val()
                        });
                }
            }
        }

        var inputFiles = $(".add_attachment_row input.inputfile");
        for (var i = 0; i < inputFiles.length; i++) {

            var fileAdd = inputFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: GenerateUUID(),
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd,
                Index: i + 1
            };
            if ($("#inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("#inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $ReAttachments.push(attachmentAdd);
        };
    }
    LoadAttachmentList();

    if (closeId) {
        $("#" + closeId).modal("toggle");
    }

};
function LoadAttachmentList() {
    var postId = $("#listing-id").val();
    $("ul.domain-change-list").empty();
    var attachmentCount = $ReAttachmentExisted.length + $ReAttachments.length;
    $("#add_attachment").text(" Attachments (" + attachmentCount + ")");

    if (attachmentCount > 0)
        $("#attachment-icon").removeClass("fa fa-plus").addClass("fa fa-paperclip");

    _.forEach($ReAttachmentExisted, function (file) {
        var li = " <li> <a onclick='RemoveReImage(\'" + file.ImgKey + "\'," + postId + ", this)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("ul.domain-change-list").append(li);
    });

    _.forEach($ReAttachments, function (file) {
        var li = " <li> <a onclick='RemoveReImage(\"" + file.Index + "\"," + postId + ", this)'>";
        li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
        li += file.Name + " </a> </li>";
        $("ul.domain-change-list").append(li);
    });


}

//Country onChange
function CheckShowingAreaSelection(ev, areaSelectionId) {
    var countryName = $(ev).val();
    var _url = "/HighlightPost/GetListHLLocationByCountry?countryName=" + countryName;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (data) {
            var $selector = $('#' + areaSelectionId + ' select[name=location]');
            if (data.length > 1) {
                $selector.multiselect('destroy');
                $selector.empty();
                $.each(data, function (key, val) {
                    $selector.append('<option value="' + val.id + '">' + val.text + '</option>');
                })

                $selector.select2({
                    enableFiltering: true,
                });
                $('#' + areaSelectionId).fadeIn();
            } else {
                $selector.multiselect('destroy');
                $selector.empty();
                $('#' + areaSelectionId).fadeOut();
            }
        },
        error: function (err) {
            $('#' + areaSelectionId).fadeOut();
        }
    });
}

function RemoveReImage(imgKey, postId, ev) {
    if (confirm("Are you sure you want to remove this image ?")) {
        var _url = "/HighlightPost/RemoveRestateImage?imgKey=" + imgKey + "&postId=" + postId;
        LoadingOverlay();
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    //remove items in UI
                    $(ev).remove();
                    $(".inputFile_" + imgKey).remove();
                    var htmlString = "";
                    htmlString += '<i id="attachment-icon"';

                    //Update image count
                    var $btnAttachmentCount = $("#add_attachment");
                    $btnAttachmentCount.empty();
                    if (response.Object > 0) {
                        htmlString += 'class="fa fa-paperclip"</i>';
                        htmlString += " Attachments " + response.Object;
                    } else {
                        htmlString += 'class="fa fa fa-plus"</i>';
                    }
                    $btnAttachmentCount.html(htmlString);
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
}

//HL Post Sharing
function ShowingSharingHLPostPartialView(postId) {
    LoadingOverlay();
    var _url = '/HighlightPost/ShowSharePostPartialView';
    $("#share-content").empty();
    $("#share-content").load(_url, {'sharedPostId': postId});
    $("#share-content").modal('show');
    LoadingOverlayEnd();
}

// type: 1 - Hide, 2 - Unhide
function ChangeHidingStatus(domainKey, type) {
    if (confirm('Are you sure?')) {
        LoadingOverlay();

        var _url = "/HighlightPost/HideDomainHLPost";
        var _data = {
            'domainKey': domainKey,
            'type': type
        };

        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            data: _data,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    window.location.reload();
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
}
