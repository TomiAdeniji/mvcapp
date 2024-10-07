var reloadTableTags = false;
var $tagModel = $("#tag-modal");
var $tagKeywords = $("#tag-keywords");
var $tagName = $("#tag-name"),
    $tagCreatedby = $("#tag-createdby"),
    $tagCreatedDate = $("#tag-creadeddate"),
    $tagUpdatedDate = $("#tag-editeddate"),
    $tagId = $("#tag-id");
var lstPages = [];


function DeletePage(id) {
    $.ajax({
        type: 'post',
        url: '/CommunitySystem/ValidationDeletePage',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.warning(_L("ERROR_MSG_80"), "Qbicles");
            }
            else {
                $.ajax({
                    type: 'post',
                    url: '/CommunitySystem/DeletePage',
                    data: { id: id },
                    dataType: 'json',
                    success: function (response) {
                        if (response.result) {
                            $("#page-row-" + id).remove();
                            cleanBookNotification.updateSuccess();
                        }
                        else {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    },
                    error: function (er) {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                });
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}

function SuspendPage(id) {
    $.ajax({
        type: 'post',
        url: '/CommunitySystem/SuspendReinstatePage',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                if (response.actionVal === 1) {
                    $("#suspend-" + id).text("Suspended");
                    $("#page-status-" + id).text("Active");
                    $("#page-status-" + id).removeClass("label-danger");
                    $("#page-status-" + id).addClass("label-success");
                }
                else {
                    $("#suspend-" + id).text("Reinstate");
                    $("#page-status-" + id).text("Suspended");
                    $("#page-status-" + id).removeClass("label-success");
                    $("#page-status-" + id).addClass("label-danger");
                }
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}

function DeleteTag(id) {
    $.ajax({
        type: 'post',
        url: '/CommunitySystem/ValidationDeleteTag',
        data: { id: id },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.warning(_L("ERROR_MSG_81"), "Qbicles");
            }
            else {
                $.ajax({
                    type: 'post',
                    url: '/CommunitySystem/DeleteTag',
                    data: { id: id },
                    dataType: 'json',
                    success: function (response) {
                        if (response.result) {
                            $("#tag-row-" + id).remove();
                            cleanBookNotification.updateSuccess();
                        }
                        else {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    },
                    error: function (er) {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                });
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}

function EditTag(id) {
    $tagId.val(id);
    $tagKeywords.tagsinput('removeAll');
    GetTag(id).done(function (tag) {
        if (tag.Id) {
            $tagId.val(tag.Id);
            $tagName.val(tag.Name);
            $tagCreatedby.val(tag.CreatedBy);
            $tagCreatedDate.val(tag.CreatedDate);
            $tagUpdatedDate.val(tag.EditedDate);
            var arrayKeywords = tag.Keywords.split(',');
            $.each(arrayKeywords, function (index, value) {
                $tagKeywords.tagsinput('add', value, { preventPost: true });
            });
        }
    })
    $("#tag-title").text("Edit Tag");
    $("#save-title").text("Confirm");
    $tagModel.modal('toggle');
}
function GetTag(id) {
    return $.ajax({
        url: "/CommunitySystem/GetTagToEditView",
        type: "GET",
        dataType: "json",
        data: { id: id }
    });
}
function AddTag() {
    $tagId.val(0);
    $tagKeywords.tagsinput('removeAll');
    $tagName.val('');
    $("#tag-title").text("Add Tag");
    $("#save-title").text("Add Tag");
    $tagModel.modal('toggle');
}


function SaveTag() {
    ConfirmSaveTag();
};
function ConfirmSaveTag() {
    if ($("#form_tag").valid()) {
        $.ajax({
            url: "/CommunitySystem/DuplicateTagName",
            data: { id: $tagId.val(), Name: $tagName.val() },
            type: "GET",
            dataType: "json",
        }).done(function(refModel) {
            if (refModel.result)
                $("#form_tag").validate().showErrors({ Name: _L("ERROR_MSG_82") });
            else {
                $("#form_tag").submit();
            }

        }).fail(function() {
            $("#form_tag").validate().showErrors({ Name: _L("ERROR_MSG_355") });
        });
    }
}

$(document).ready(function () {

    $("#form_tag").submit(function (e) {
        e.preventDefault();
        $.ajax({
            type: this.method,
            cache: false,
            url: this.action,
            enctype: 'multipart/form-data',
            data: new FormData(this),
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {

            },
            success: function (data) {
                if (data.result) {
                    reloadTableTags = true;
                    $tagModel.modal('toggle');
                    cleanBookNotification.updateSuccess();
                    ClearError();
                    $('#tab1').load("/Community/SystemCommunityTags");
                }
            },
            error: function (data) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    });

    var loadFirst = true;
    $('#table_community_showpage').on('draw.dt', function () {
        if (loadFirst == true) loadFirst = false;
        else {
            setTimeout(function () {
                // update order
                UpdateOrder();
            }, 1000);
        }

    });
});
function UpdateOrder() {
    var trTable = $('#table_community_showpage tbody tr');
    var lstPageId = [];
    var lstPageOrder = [];
    if (trTable.length > 0) {
        for (var i = 0; i < trTable.length; i++) {
            lstPageId.push($(trTable[i]).find('td.td_checkbox input.row_id').val());
            lstPageOrder.push($(trTable[i]).find('td.td_order').text());
        }
    }
    $.ajax({
        type: 'post',
        url: '/Community/UpdateOrderpages',
        data: { lstId: lstPageId, lstOrder: lstPageOrder },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 2) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}
function selectedChange(ev) {
    if ($(ev).val() == "true") {
        $(ev).val(false);
        $(ev).removeClass("checked");
    } else {
        $(ev).val(true);
        $(ev).addClass("checked");
    }
    var pageId = $(ev.parentElement).find('input.row_id').val();
    // update isFeature
    $.ajax({
        type: 'post',
        url: '/Community/UpdateIsFeature',
        data: { id: pageId, value: $(ev).val() },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 2) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

    var all = $('#table_community_showpage .community_page_checked_show');
    var checked = 0;
    for (var j = 0; j < all.length; j++) {
        if ($(all[j]).val() === 'true') {
            checked += 1;
        }
    }
    if (checked >= 3) {
        // disable all
        for (var i = 0; i < all.length; i++) {
            if ($(all[i]).val() !== 'true') {
                $(all[i]).attr("disabled", "disabled");
            } else {
                $(all[i]).removeAttr("disabled");
            }
        }
    } else {
        // disable all
        for (var k = 0; k < all.length; k++) {
            $(all[k]).removeAttr("disabled");
        }
    }

}
