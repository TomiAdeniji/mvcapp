function ShowGroupMember(wgId, title) {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/Trader/ShowListMemberForWorkGroup?wgId=" + wgId + "&title=" + title.replace(/\s/g, "%20"));
    $('#app-trader-workgroup-preview').modal('toggle');
};

var oldClosingCount = "";
var oldvariance = "";
var busycomment = false;
function updateVariance(id, itemId) {
    var openCountMoven = parseFloat($('#tb_product tr.tr_' + id + ' td.td_expected').text().replace(/\s/g, ""));
    var closingCount = parseFloat($('#tb_product tr.tr_' + id + ' td.td_closing_count input').val());
    $('#tb_product tr.tr_' + id + ' td.td_variance span').text((openCountMoven - closingCount).toFixed(2));
    UpdateProduct(id, itemId);
}
function UpdateProduct(id, itemId) {
    var closingCount = parseFloat($('#tb_product tr.tr_' + id + ' td.td_closing_count input').val());
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderStockAudits/UpdateStockitem?id=' + itemId + '&closingCount=' + closingCount,
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
                $('#tb_product tr.tr_' + id + ' td.td_closing_count input').val(oldClosingCount);
                $('#tb_product tr.tr_' + id + ' td.td_variance span').text(oldvariance);
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            $('#tb_product tr.tr_' + id + ' td.td_closing_count input').val(oldClosingCount);
            $('#tb_product tr.tr_' + id + ' td.td_variance span').text(oldvariance);
        }
    }).always(function() {
        $.LoadingOverlay("hide");
    });
}
function focusChange(id, ev) {
    oldClosingCount = $(ev).val();
    oldvariance = $('#tb_product tr.tr_' + id + ' td.td_variance span').text();
}
var $shiftAuditId = $("#shiftaudit_id").val();
function RenderContentUpdated(reload, update) {
    $.LoadingOverlay("show");
    $('#shiftaudit_content').load('/TraderStockAudits/ShiftAuditReview?id=' + $shiftAuditId + '&content=' + reload, function () {
        $.LoadingOverlay("hide");
        if (update) {
            //cleanBookNotification.updateSuccess();
            $('#tb_product').DataTable().draw();
        }
            
    });
}
//approval update
function UpdateStatusApproval(apprKey) {
    var statusOld = $("#action_approval_default").val();
    $.LoadingOverlay("show");
    var sendStatus = $("#action_approval").val();
    CheckStatusApproval(apprKey).then(function (res) {
        if (res.result && res.Object.toLocaleLowerCase() === statusOld.toLocaleLowerCase()) {
            $.ajax({
                url: "/Qbicles/SetRequestStatusForApprovalRequest",
                type: "GET",
                dataType: "json",
                data: { appKey: apprKey, status: sendStatus.split('_')[0] },
                success: function (rs) {
                    $.LoadingOverlay("hide");
                    finishAudit(sendStatus);
                    setTimeout(function () {
                        RenderContentUpdated(true, true);
                    }, 100);

                },
                error: function (err) {
                    $.LoadingOverlay("hide");
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_247"), "Qbicles");
            setTimeout(function () {
                window.location.reload();
            }, 1000);
        }
    });
};
function finishAudit(status) {
    var stockAudit = {
        Id: $('#shiftaudit_id').val()
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderStockAudits/FinishStockAudit?status=' + status,
        data: { stockAudit: stockAudit },
        dataType: 'json',
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.createSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 2) {
                $('#app-trader-inventory-stock-audit').modal('toggle');
                cleanBookNotification.updateSuccess();
                showStockAuditTable();
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}


function LoadComment() {
    $('#list-comments-approval').load('/Trader/LoadPostComment?type=StockAudit&id=' + $shiftAuditId, function () {
        
    });
}


