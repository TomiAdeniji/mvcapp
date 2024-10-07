$(document).ready(function () {
    initCustomerDiscussionPage();
});
function initCustomerDiscussionPage() {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    initOrderItemsShow();
    $('input[name=search]').keyup(delay(function () {
        initOrderItemsShow();
    }, 500));
    $('select[name=groups]').change(function () {
        initOrderItemsShow();
    });
};
function initOrderItemsShow() {
    var numPageSize = 6;
    var $data_container_items = $('#items-container');
    var $pagination_container = $('#pagiation-items');
    $pagination_container.pagination({
        dataSource: '/Commerce/SearchCatalogItemsNotTaxes',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_items.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: numPageSize,
        ajax: {
            data: {
                scatids: JSON.stringify($('select[name=groups]').val()), keyword: $('input[name=search]').val(), bdomainKey: $('#domain-key').val()
            },
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
    var $domainkey = $('#domain-key').val();
    var _html = '<div class="col"><a onclick="showItemContentB2B(' + data.Id + ',\'' + $domainkey + '\')" href=\"#\">';
    _html += '<div class="productimg" style=\"background-image:url(\''+data.ImageUri+'\');\"></div>';
    _html += '<div class="priceblock">';
    _html += '<p>' + data.Name + '</p>';
    _html += '<label class="label label-lg label-soft">' + data.CategoryName + '</label> &nbsp; <span>' + data.Price + '</span></div>';
    _html += '</a></div>';
    return _html;
}
function showItemContentB2B(itemId, dmkey) {
    var _url = "/Commerce/B2BOrderItemContentShow?itemId=" + itemId + "&domainKey=" + dmkey;
    $("#product-more-catalogue").empty();
    $("#product-more-catalogue").load(_url, function () {
        $('.select2').select2({
            placeholder:"Please select"
            });
    });
    $("#product-more-catalogue").modal("show");
}
function importingItemfromDistributorCatalog(frmId) {
    var name = $("#product_groups_select").val() + "";
    if (name.trim() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_489"), "Qbicles");
        return;
    }
    else if ($(frmId).validate().valid()) {
        $.LoadingOverlay("show");
        var model = {
            itemId: $(frmId + ' input[name=id]').val(),
            groupId: $(frmId + ' select[name=group]').val(),
            sku: $(frmId + ' input[name=sku]').val(),
            description: $(frmId + ' textarea[name=desc]').val(),
            isprimaryVendor: $(frmId + ' input[name=isprimaryVendor]').prop('checked'),
            b2bRelationshipId: $('#hdfrelationshipId').val()
        };
        $.post("/Commerce/ImportingItemfromDistributorCatalog", { model: model }, function (response) {
            if (response.result) {
                cleanBookNotification.success(_L('SUCCESS_MSG_IMPORT_DISTRIBUTORCATALOG'), "B2B");
                $("#product-more-catalogue").modal("hide");
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(_L(response.msg), "B2B");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "B2B");
            }
            $.LoadingOverlay("hide");
        });
    }
}

function addgroup() {
    var name = $("#form_group_add").val() + "";
    if (name.trim() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_489"), "Qbicles");
        return;
    }else{
        $.ajax({
            type: 'post',
            url: '/TraderConfiguration/SaveGroup',
            data: { Name: $('#form_group_add').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        reloadProductGroup(name);
                        $('#add-category').modal('toggle');
                    }
                    else if (response.actionVal === 2) {

                        cleanBookNotification.updateSuccess();
                        reloadProductGroup(name);
                        $('#add-category').modal('toggle');
                    }
                    else if (response.actionVal === 3) {
                        //cleanBookNotification.error(response.msg, "Qbicles");
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            //ResetFormControl('form_group_add');
        });
    }
}

function reloadProductGroup(name){
    var newProductGroups = null;
    var url = '/Commerce/GetProductGroups'
    $.get(url, function(data, status){
        newProductGroups = data.find(o => o.text === name);
        if(newProductGroups){
            $("#product_groups_select").select2({
                data : data
            }).val(newProductGroups.id).trigger('change');
        }else{
            $("#product_groups_select").select2({
                data : data
            })
        }
    })
}