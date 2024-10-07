var partnershipkey = $("#partnershipkey").val();

function InitB2BCatalogDiscussionPage() {
    var $data_container_items = $('#data-container-items');
    var $pagination_container = $('#pagiation-items');
    $pagination_container.pagination({
        dataSource: '/Commerce/SearchMenuItems',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_items.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 8,
        ajax: {
            data: { scatids: JSON.stringify($('select[name=groups]').val()), keyword: $('input[name=search]').val(), bdomainId: $('#selling-domain-id').val() },
            beforeSend: function () {
                $data_container_items.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var extraCol = (count % 4 == 0 ? 0 : 4) - count % 4;
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

    //Init for submitting form to create new order
    var $frmordercreation = $('#frmb2bordercreation');
    $frmordercreation.submit(function (e) {
        e.preventDefault();
        var requestData = $(this).serialize();
        requestData = requestData + '&CatalogId=' + $("#catalog-id").val();
        
        if ($frmordercreation.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: requestData,
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $('#b2b-order-add').modal('hide');
                        isDisplayFlicker(true);
                        cleanBookNotification.success("Order created successfully", "Qbicles");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}

function itemTemplate(data) {
    var bdomainId = $('#selling-domain-id').val();
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
    LoadingOverlay('show');
    $modalproductmore.load("/B2C/LoadProductMoreMenuContent", { menuId: menuId, businessDomainId: businessDomainId }, function () {
        LoadingOverlayEnd();
    });
}

function initDataB2bForCreateOrder() {
    resetDataB2bOrder();
    $('#b2b-order-add').modal('show');
    var _url = "/Commerce/InitDataB2bForCreateOrder";
    $.post(_url, { "partnershipKey": partnershipkey }, function (data) {
        $('#b2b-order-add input[name=OrderReferenceId]').val(data.reference.id);
        $('#b2b-order-add input[name=OrderFullRef]').val(data.reference.orderref);
        $('#b2b-order-add input[name=Partnershipkey]').val(partnershipkey);
    });
}

function resetDataB2bOrder() {
    $('#b2b-order-add input[name=OrderReferenceId]').val('0');
    $('#b2b-order-add input[name=OrderFullRef]').val('');
    $('#b2b-order-add input[name=Partnershipkey]').val('');
    $('#b2b-order-add textarea[name=OrderNote]').val('');
}

function AddCommentToDiscussion(discussionKey) {

    if (busycomment)
        return;
    $('.newcomment').addClass('reprisedcomments');

    var message = $('#txt-comment-link');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddComment2Discussion",
            data: { message: message.val(), disKey: discussionKey },
            type: "POST",
            success: function (result) {
                if (result) {
                    message.val("");
                    if (result.msg != '') {
                        $('#list-comments-discussion').prepend(result.msg);
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
