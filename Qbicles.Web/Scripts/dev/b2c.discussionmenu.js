var CountPost = 1, CountMedia = 1, busycomment = false;
function validateAddComment() {
    var message = $('#txt-comment-link').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function AddComment2DiscussionMenu(discussionKey) {
    if (busycomment)
        return;
    $('.newcomment').addClass('reprisedcomments');

    var message = $('#txt-comment-link');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddComment2DiscussionMenu",
            data: { message: message.val(), disKey: discussionKey },
            type: "POST",
            success: function (response) {
                if (response.result) {
                    message.val("");

                    if (response.msg != '') {
                        $('#list-comments-discussion').prepend(response.msg);
                        isDisplayFlicker(false);
                    }

                }
                busycomment = false;
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                isPlaceholder(false, '');
                busycomment = false;
            }
        });
    }
}
$(function () {
    $(".checkmulti-category").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    initSearchMenuItems();
    $('#search-item').keyup(delay(function () {
        initSearchMenuItems();
    }, 500));
    $('#search-item-category').change(function () {
        initSearchMenuItems();
    });
});

function LoadMorePostsDiscussion(activityKey, pageSize, divId) {

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
function initSearchMenuItems() {
    var $data_container_items = $('#data-container-items');
    var $pagination_container = $('#pagiation-items');
    $pagination_container.pagination({
        dataSource: '/B2C/SearchMenuItems',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_items.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 6,
        ajax: {
            data: { scatids: JSON.stringify($('#search-item-category').val()), keyword: $('#search-item').val(), bdomainId: $('#hdfBusinessDomainId').val() },
            beforeSend: function () {
                $data_container_items.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var extraCol = (count % 3 == 0 ? 0 : 3) - count % 3;
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += itemTemplate(item);
            });
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<div class="col">&nbsp;</div>';
            }
            $data_container_items.html(dataHtml);
        }
    })
}
function itemTemplate(data) {
    var bdomainId = $('#hdfBusinessDomainId').val();
    var _html = '<div class="col"><a href="#product-more-menu" data-toggle="modal" onclick="loadProductMoreMenuContent(' + data.Id + ',' + bdomainId + ')">';
    _html += '<div class="productimg" style="background-image: url(\'' + data.ImageUri + '\')"></div>';
    _html += '<div class="priceblock">';
    _html += '<p>' + data.Name + '</p>';
    _html += '<label class="label label-lg label-soft">' + data.CategoryName + '</label> &nbsp; <span>' + data.Price + '</span></div>';
    _html += '</a></div>';
    return _html;
}
function loadProductMoreMenuContent(menuId, businessDomainId) {
    var $modalproductmore = $('#product-more-menu');
    $modalproductmore.LoadingOverlay('show');
    $modalproductmore.load("/B2C/LoadProductMoreMenuContent", { menuId: menuId, businessDomainId: businessDomainId }, function () {
        $modalproductmore.LoadingOverlay('hide');
    });
}
function createB2COrderDiscussionFromMenuDiscussion(menuDiscussionKey) {
    $.LoadingOverlay('show');
    $.post("/B2C/CreateB2COrderDiscussionFromCatalogDiscussion", { catalogDiscussionKey: menuDiscussionKey }, function (response) {
        if (response.result) {
            location.href = "/B2C/DiscussionOrder?disKey=" + response.msgId;
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}