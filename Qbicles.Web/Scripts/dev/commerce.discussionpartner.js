var countPost = 1, countMedia = 1, busycomment = false;
var wto;
$(document).ready(function () {
    initPlugins();
});
function initPlugins() {
    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#contact-finder input[name=search]').keyup(delay(function () {
        searchTop10TraderContact();
    }, 1000));
    $('#frm-confirm-b2bcontact').validate({
        ignore: "",
        rules: {
            group: {
                required: true
            },
            workgroup: {
                required: true
            }
        }
    });
}
//type: buy or sell
function settingsPartnership(partnershipId, type, elm) {
    var members = $('#slTeams' + type + partnershipId + ' option:selected').map(function (i, v) {
        return this.value;
    }).get(); // result is array
    var menus = $('#slMenus' + type + partnershipId).val();
    var accountId = $('#slAccount' + type + partnershipId).val();
    var status = $('#chkParnership' + partnershipId).prop('checked');
    $.post("/Commerce/SettingsPartnership", { partnershipId: partnershipId, members: (members ? members : []), menus: (menus ? menus : []), accountId: accountId, type: type, status: status }, function (response) {
        if (response.result) {
            $(elm).attr('disabled', true);
            cleanBookNotification.updateSuccess();
            loadPartnershipsContent($('#hdfRelationshipId').val());
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Commerce");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function haltAll(relationshipId) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Commerce",
        message: _L("CONFIRM_MSG_HALTALL", ["all partnerships"]),
        callback: function (result) {
            if (result) {
                $.post("/Commerce/HaltAllPartnerships", { relationshipId: relationshipId }, function (response) {
                    if (response.result) {
                        $('#logistics').css("cursor", "not-allowed");
                        $('#logistics').css("pointer-events", "none");
                        location.reload();
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Commerce");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    }
                });
                return;
            }
        }
    });
}
function halt(partnershipId) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Commerce",
        message: _L("CONFIRM_MSG_HALTALL", ["a partnership"]),
        callback: function (result) {
            if (result) {
                $.post("/Commerce/HaltPartnership", { partnershipId: partnershipId }, function (response) {
                    if (response.result) {
                        loadPartnershipsContent($('#hdfRelationshipId').val());
                        $('#chkParnership' + partnershipId).bootstrapToggle('off');
                        if (response.Object && response.Object.isLogistics) {
                            $('#logistics').css("cursor", "not-allowed");
                            $('#logistics').css("pointer-events", "none");
                        }
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Commerce");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    }
                });
                return;
            }
        }
    });
}
function validateAddComment() {
    var message = $('#txt-comment-link').val();
    if (message.length > 1500)
        $('#addcomment-error').show();
    else
        $('#addcomment-error').hide();
}
function addCommentToDiscussion(discussionKey) {

    if (busycomment)
        return;
    var message = $('#txt-comment-link');
    if (message.val() && !$('#addcomment-error').is(':visible')) {
        isPlaceholder(true, '#list-comments-discussion');
        busycomment = true;
        $.ajax({
            url: "/QbicleComments/AddCommentB2BDiscussion",
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
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                isPlaceholder(false, '');
                busycomment = false;
            }
        });
    }
}
function loadMorePostsDiscussion(activityKey, pageSize, divId) {

    $.ajax({
        url: '/Qbicles/LoadMoreActivityPosts',
        data: {
            activityKey: activityKey,
            size: countPost * pageSize
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
            countPost = countPost + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });

}
function loadMoreMediasDiscussion(activityId, pageSize, divId) {
    $.ajax({
        url: '/Qbicles/LoadMoreActivityMedias',
        data: {
            activityId: activityId,
            size: countMedia * pageSize
        },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#' + divId).append(response).fadeIn(250);
            countMedia = countMedia + 1;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });
}
function loadPartnershipsContent(rlid) {
    setTimeout(function () {
        if ($('ul.nav li.active a[href=#tab0]').length > 0)
            $('#tab0').LoadingOverlay('show');
        $('#tab0').load("/Commerce/LoadPartnershipsContent", { rlid: rlid }, function () {
            $('#tab0').LoadingOverlay('hide');
        });
    }, 700);
}
function loadLogisticsPartnershipContent(pid, tabActive) {
    if (pid > 0) {
        var draftAgreementId = $('#logs-draft').data("draftagreementid");
        setTimeout(function () {
            if ($('ul.nav li.active a[href=#tab2]').length > 0)
                $('#tab2').LoadingOverlay('show');
            $('#tab2').load("/Commerce/LoadLogisticsPartnershipContent", { pid: pid }, function () {
                initPlugins();
                loadLogisticsModals(pid);
                var isExistLogisticsAgreement = $('#hdfIsExistLogisticsAgreement').val();
                if (isExistLogisticsAgreement == 1) {
                    $('#logistics').css("cursor", "");
                    $('#logistics').css("pointer-events", "");
                }
                var newAgreementId = $('#logs-draft').data("draftagreementid");
                if (tabActive || (draftAgreementId > 0 && draftAgreementId != newAgreementId)) {
                    $('#activetab').show();
                    $('#activetab a[href=#logs-active]').click();
                }
                $('#tab2').LoadingOverlay('hide');
            });
        }, 500);
    }
}
function loadLogisticsModals(pid) {
    $('#logictismodals').load("/Commerce/LogisticsPartnershipModals", { pid: pid }, function () {
        $('#logictismodals select.select2').select2({ placeholder: 'Please select' });
        $('#logictismodals input[data-toggle="toggle"]').bootstrapToggle();
    });
}
function loadProviderPriceList(pid) {
    $('.provider-pricelist').load("/Commerce/ProviderPriceList", { pid: pid }, function () { });
}
function loadLogisticsPartnershipButton(pid) {
    $('.action_button').load("/Commerce/LogisticsPartnershipButton", { pid: pid }, function () { });
}
function loadActiveContent(pid) {
    var $tabactive = $('#logs-active .qbicles-dash-grid');
    setTimeout(function () {
        if ($('ul.nav li.active a[href=#logs-active]').length > 0)
            $tabactive.LoadingOverlay('show');
        $tabactive.load("/Commerce/LoadActiveContent", { pid: pid }, function () {
            $tabactive.LoadingOverlay('hide');
        });
    }, 500);
}
function loadArchiveContent(pid) {
    var $tabactive = $('#logs-archive .qbicles-dash-grid');
    setTimeout(function () {
        if ($('ul.nav li.active a[href=#logs-archive]').length > 0)
            $tabactive.LoadingOverlay('show');
        $tabactive.load("/Commerce/LoadArchiveContent", { pid: pid }, function () {
            $tabactive.LoadingOverlay('hide');
        });
    }, 500);
}
function loadChargeListModal(logisticsAgreementId) {
    $('#b2b-archive-delivery-charge-list').load("/Commerce/ChargeListModal", { logisticsAgreementId: logisticsAgreementId }, function () { });
}
function tabActive(tabid, pid) {
    if (tabid === 'tradepartner')
        $('#tradepartner').trigger('click');
    else if (tabid == 'logistics') {
        //check Only one Logistics Agreement can be active at a time.
        $.post("/Commerce/CheckLogisticsAgreement", { relationshipId: $('#hdfRelationshipId').val(), partnershipId: pid }, function (data) {
            if (!data) {
                $('#logistics').trigger('click');
                loadLogisticsPartnershipContent(pid, false);
            } else
                cleanBookNotification.warning(_L('WARNING_MSG_EXISTAGREEMENT'), 'Commerce');
        });

    }
}
function updateLocations(pid, elm) {
    clearTimeout(wto);
    wto = setTimeout(function () {
        var locids = $(elm).val();
        if (locids.length === 0)
            return;
        $.post("/Commerce/UpdateLocationsPartnership", { partnershipId: pid, locids: (locids ? locids : []) }, function (response) {
            if (response.result) {
                loadProviderPriceList(pid);
                cleanBookNotification.updateSuccess();
                loadPartnershipsContent($('#hdfRelationshipId').val());
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(_L(response.msg), "Commerce");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
    }, 1500);
}
function addPriceListLogisticsAgreement(pid) {
    var priceListId = $('#slPriceList').val();
    $('#frmProviderPriceList').validate();
    if ($('#frmProviderPriceList').valid()) {
        $.post("/Commerce/AddPriceListLogisticsAgreement", { partnershipId: pid, priceListId: priceListId }, function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                loadProviderPriceList(pid);
                loadLogisticsModals(pid);
                $('#b2b-choose-prices').modal('hide');
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(_L(response.msg), "Commerce");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            }
        });
    }
}
function deletePriceListLogisticsAgreement(pid) {
    $.post("/Commerce/DeletePriceListLogisticsAgreement", { partnershipId: pid }, function (response) {
        if (response.result) {
            cleanBookNotification.updateSuccess();
            loadProviderPriceList(pid);
            loadLogisticsModals(pid);
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Commerce");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function updateProviderChargeFrameworks(pid) {
    var chargeFrameworks = [];
    $('#b2b-delivery-charge-mgmt article').each(function () {
        var $article = $(this);
        var cfk = {
            Id: $article.data('id'),
            IsActive: $article.find("input[name=IsActive]").prop('checked'),
            DistanceTravelPerKm: $article.find("input[name=DistanceTravelPerKm]").val(),
            DistanceTravelledFlatFee: $article.find("input[name=DistanceTravelledFlatFee]").val(),
            TimeTakenFlatFee: $article.find("input[name=TimeTakenFlatFee]").val(),
            TimeTakenPerSecond: $article.find("input[name=TimeTakenPerSecond]").val(),
            ValueOfDeliveryFlatFee: $article.find("input[name=ValueOfDeliveryFlatFee]").val(),
            ValueOfDeliveryPercentTotal: $article.find("input[name=ValueOfDeliveryPercentTotal]").val(),
        };
        chargeFrameworks.push(cfk);
    });
    $.post("/Commerce/UpdateProviderChargeFrameworks", { partnershipId: pid, chargeFrameworks: chargeFrameworks }, function (response) {
        if (response.result) {
            cleanBookNotification.updateSuccess();
            $('#b2b-delivery-charge-mgmt').modal('hide');
            //loadProviderPriceList(pid);
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Commerce");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function loadPriceListsByLocationId(el) {
    $('.plist').fadeIn();
    var $price = $('#slPriceList');
    $.get("/Commerce/GetPriceLists?lid=" + $(el).val(), function (data) {
        $price.select2('destroy');
        $price.empty();
        $price.select2({
            placeholder: "Please select",
            data: data
        });
    });
}
function agreeTerms(pid) {
    $.post("/Commerce/AgreeTerms", { partnershipId: pid }, function (response) {
        if (response.result) {
            cleanBookNotification.updateSuccess();
            var providername = $('.provider-name').text();
            var $agreeterms = $('#agreeterms');
            $agreeterms.removeClass('btn-success');
            $agreeterms.attr("disabled", true);
            $agreeterms.removeAttr("onclick");
            $agreeterms.addClass("btn-info")
            $agreeterms.text(providername + ' reviewing');
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Commerce");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function finaliseAgreement(pid) {
    $.post("/Commerce/FinaliseAgreement", { partnershipId: pid }, function (response) {
        if (response.result) {
            cleanBookNotification.updateSuccess();
            loadLogisticsPartnershipContent(pid, true);
            $('#logs-draft button').attr('disabled', true);
            loadPartnershipsContent($('#hdfRelationshipId').val());
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Commerce");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
        }
    });
}
function refreshLogisticsView() {
    var pid = $('#hdfParnershipId').val();
    if (pid) {
        loadLogisticsPartnershipContent(pid, false);
    }
    loadPartnershipsContent($('#hdfRelationshipId').val());
}
function moveModalIntoBody(selector) {
    var $source = $(selector);
    if ($source.length > 0) {
        $('body ' + selector).remove();
        $source.appendTo('body');
    }
}
function searchTop10TraderContact() {
    var $contactitems = $('#contact-finder .widget-contacts');
    $contactitems.LoadingOverlay('show');
    $contactitems.load("/TraderContact/SearchTop10ContactContent", { keyword: $('#contact-finder input[name=search]').val() }, function () {
        $contactitems.LoadingOverlay('hide', true);
    });
}
function loadContactDetail(contactId) {
    var $contactinfo = $('#contact-finder .welcome-info');
    $('#contact-finder .contact').fadeIn();
    $('#contact-finder .contact-list').hide();
    $contactinfo.LoadingOverlay('show');
    $contactinfo.load("/TraderContact/TradingContactInfoContent", { contactId: contactId }, function () {
        $contactinfo.LoadingOverlay('hide', true);
    });
}
function chooseContact() {
    var contact = JSON.parse($('#hdfContactJsonInfo').val());
    $('#contact-id').val(contact.Id);
    $('.contact-name').text(contact.Name);
    $('a.contact-email').attr('href', 'mailto:' + contact.Email);
    $('a.contact-email').text(contact.Email);
    $('.contact-tel').text(contact.Phone);
    $('.contact-address').text(contact.Address);
    $('.contact-avatar').css('background-image', 'url(\'' + contact.AvartarUri + '\')');
    $('#contact-finder').modal('hide');
    $('#btnConfirmContact').show();
    $('#btnConfirmNewContact').hide();
}
function setcontact() {
    if ($('#frm-confirm-b2bcontact').valid()) {
        var paramaters = {
            relationshipId: $('#hdfRelationshipId').val(),
            contactId: $('#contact-id').val(),
            groupId: $('#frm-confirm-b2bcontact select[name=group]').val(),
            workgroupId: $('#frm-confirm-b2bcontact select[name=workgroup]').val()
        };
        $.LoadingOverlay('show');
        $.post("/Commerce/SetContactForB2BRelationship", paramaters, function (response) {
            if (response.result) {
                $('#confirm-approval-modal').modal('hide');
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
                $('ul.app_main_nav a').removeClass('depend');
                $('.contactopts').remove();
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(_L(data.msg), "Commerce");
            } else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
            LoadingOverlayEnd();
        });
    }
}
function setExistContact() {
    var _contacId = $('#contact-id').val();
    if (_contacId == '0' || !_contacId) {
        cleanBookNotification.error('Please choose a Contact.', "Qbicles");
        return;
    }
    var paramaters = {
        relationshipId: $('#hdfRelationshipId').val(),
        contactId: _contacId,
        groupId: 0,
        workgroupId: 0
    };
    $.LoadingOverlay('show');
    $.post("/Commerce/SetContactForB2BRelationship", paramaters, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            $('ul.app_main_nav a').removeClass('depend');
            $('.contactopts').remove();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(data.msg), "Qbicles");
        } else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        LoadingOverlayEnd();
    });
}

function savecontactgroup(type) {  
    var id = 0;
    var name = $('#contact-group-edit-name').val() + "";
    if (name.trim() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_374"), "Qbicles");
        return;
    } else {
        $.ajax({
            type: 'post',
            url: '/TraderConfiguration/SaveContactGroup',
            data: {
                group: { Id: id, Name: name, saleChannelGroup: type }
            },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        reloadContactGroup(name);
                        $('#add-contact-group').modal('hide');
                    }
                    else if (response.actionVal === 2) {  

                        cleanBookNotification.updateSuccess();
                        reloadContactGroup(name);
                        $('#add-contact-group').modal('hide');
                    }
                    else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");

                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_375"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            //ResetFormControl('form_group_add');
        });
    }
}

function reloadContactGroup(name){
    var url = '/TraderContact/GetContactGroups'
    var newContact = null;
    var newContactId = null;
    $.get(url, function(data, status){
        newContact = data.find(o => o.text === name);
        if(newContact){
            $("#contact-groups-select").select2({
                data : data
            }).val(newContact.id).trigger('change');
        }else{
            $("#contact-groups-select").select2({
                data : data
            })
        }
    })
}