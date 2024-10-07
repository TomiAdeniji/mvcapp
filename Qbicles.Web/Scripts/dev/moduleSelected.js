//
function ShowDiscussionPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Discussions/SetDiscussionSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/DiscussionQbicle?disKey=' + key);
                else
                    window.location.href = '/Qbicles/DiscussionQbicle?disKey=' + key;
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function ShowTaskPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Tasks/SetTaskSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                var url = refModel.Object;
                if (newTab)
                    window.open(url);
                else
                    window.location.href = url;
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });

}

function ShowAlertPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Alerts/SetAlertSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/Alert');
                else
                    window.location.href = '/Qbicles/Alert';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function ShowMediaPage(key, newTab) {
    $.LoadingOverlay('show');
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Medias/SetMediaSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/Media?key=' + key);
                else
                    window.location.href = '/Qbicles/Media?key=' + key;
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    })
}


function ShowMediaPageB2C(key, newTab) {
    $.LoadingOverlay('show');
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Medias/SetMediaSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/Media?key=' + key);
                else
                    window.location.href = '/Qbicles/Media?key=' + key;
            }
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

function ShowEventPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Events/SetEventSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/Event');
                else
                    window.location.href = '/Qbicles/Event';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function ShowLinkPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;
    $.ajax({
        type: 'post',
        url: '/Links/SetLinkSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/Qbicles/Link');
                else
                    window.location.href = '/Qbicles/Link';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function RedirectToApprovalDetailPage(key, redirectLink) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Approvals/SetApprovalSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (resp) {
            window.location = redirectLink;
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function ShowApprovalPage(key, newTab, approvalType) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Approvals/SetApprovalSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                var url = "/Qbicles/Approval";
                switch (approvalType) {
                    case "approval":
                        url = "/Qbicles/Approval";
                        break;
                    case "journal":
                        url = "/Bookkeeping/ApprovalBookkeeping";
                        break;
                    case "sale":
                        url = "/TraderSales/SaleReview?key=" + key;
                        break;
                    case "purchase":
                        url = "/TraderPurchases/PurchaseReview?key=" + key;
                        break;
                    case "transfer":
                        url = "/TraderTransfers/TransferReview?key=" + key;
                        break;
                    case "contact":
                        url = "/TraderContact/ContactReview?key=" + key;
                        break;
                    case "invoice":
                        url = "/TraderInvoices/InvoiceReview?key=" + key;
                        break;
                    case "payment":
                        url = "/TraderPayments/PaymentReview?key=" + key;
                        break;
                    case "spotCount":
                        url = "/TraderSpotCount/SpotCountReview?key=" + key;
                        break;
                    case "wasteReport":
                        url = "/TraderWasteReport/WasteReportReview?key=" + key;
                        break;
                    case "manufacturingjobs":
                        url = "/Manufacturing/ManuJobReview?key=" + key;
                        break;
                    default:
                        url = approvalType;
                        break;
                }

                window.location.href = url;
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function SetFormDefinitionSelected(id) {
    if ($isMemberQbicles === 'False')
        return;
    var rs = false;
    $.ajax({
        type: 'GET',
        url: '/FormDefinition/SetFormDefinitionSelected',
        datatype: 'json',
        data: { id: id },
        async: false,
        success: function (refModel) {
            rs = refModel.result;
        }
    });
    return rs;
}

function ShowTopic(event, id) {
    event.stopPropagation();
    $("#select-topic").val(id).trigger("change");
    var data = $("#select-topic").select2('data');
    var value = data ? data[0] : null;
    if (value != null)
        SetTopic(value.id);
}

function ModuleClick(key, module, ref, topicId) {
    if ($isMemberQbicles === 'False')
        return;
    $.ajax({
        type: 'post',
        url: '/Commons/BindingQbicleParameter',
        dataType: 'json',
        data: {
            key: key,
            ModuleSelected: module
        },
        success: function (refModel) {
            if (refModel.result == true)
                if (typeof topicId !== 'undefined') {
                    setCookie("Qbicle-topic", topicId);
                }
            window.location.href = "/Qbicles/Dashboard?ActivityType=" + ref;
        }
    });
};

function ShowDiscussionB2CPage(key, newTab) {
    $.LoadingOverlay("show");
    var goBack = window.location.href;

    $.ajax({
        type: 'post',
        url: '/Discussions/SetDiscussionSelected',
        datatype: 'json',
        data: {
            key: key, goBack: goBack
        },
        success: function (refModel) {
            if (refModel.result) {
                MarkAsReadActivity(key, null);
                if (newTab)
                    window.open('/B2C/DiscussionOrder?disKey=' + key);
                else
                    window.location.href = '/B2C/DiscussionOrder?disKey=' + key;
            }
        }
    });
}