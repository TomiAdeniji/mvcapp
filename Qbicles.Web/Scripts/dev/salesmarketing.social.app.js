var skip = 0;
var take = 8;
var totalCamp = 0;
var totalArea = 0;
var totalPlace = 0;
var totalPipeline = 0;
var selectedPipelineContacts = [];
var selectedPipelineContact = [];
var uiEls = {
    $form_event_addedit: $("#form_event_addedit"),
    $eventName: $('#eventName'),
    $createEvent: $('#create-event'),
    $singleEventDate: $('#single-event-date'),
    $eventQbicleId: $('#eventQbicleId'),
};
var $frm_campaign_add = $('#frm_marketing-social-campaign_add'), isBusyAddTaskForm = false, isFacebookPage = false, brandCountRecord = 1, segmentCountRecord = 0;

function pad(num, size) {
    var s = num + "";
    while (s.length < size) s = "0" + s;
    return s;
}

function SocialCampaignAdd() {
    $.ajax({
        url: "/SalesMarketing/GetDependencyCheckManual",
        type: "GET",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.status) {
                var netwokType = data.netwokType;
                var brands = data.brands;
                var ideaThemes = data.ideaThemes;
                if (netwokType > 0 && brands > 0 && ideaThemes > 0) {
                    SMSocialCampaignAdd();
                }
                else {
                    $('#app-marketing-social-auto-dependencies').empty();
                    $('#app-marketing-social-auto-dependencies').load('/SalesMarketing/DependencyCheckPartial?type=0', function () {
                        $('#app-marketing-social-auto-dependencies').modal('show');
                    });
                }
            } else {
                LoadingOverlayEnd();
                cleanBookNotification.error(data.message, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });

}

function SMSocialCampaignAdd() {
    $('#app-marketing-manual-social-campaign-add').empty();
    $('#app-marketing-social-campaign-add').empty();
    $('#app-marketing-email-campaign-add').empty();
    $('#app-marketing-social-campaign-add').load('/SalesMarketing/SocialCampaignAdd', function () {
        $('#app-marketing-social-campaign-add').modal('show');
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#social-campaign-workgroup').select2({ placeholder: "Please select" });
        $('#slBrandCampaign').select2({
            placeholder: "Please select",
            allowClear: true
        });
        isValidWorkgroup();
        //load script init
        var $frm_campaign_add = $('#frm_marketing-social-campaign_add');
        $frm_campaign_add.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });

        $(".previewimg").change(function () {
            var target = $(this).data('target');
            readImgURL(this, target);
            $(target).fadeIn();
        });
        disableCampaignForm(true);
        //End load script
    });

}

function ManualSocialCampaignAdd() {
    $.ajax({
        url: "/SalesMarketing/GetDependencyCheckManual",
        type: "GET",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.status) {
                var brands = data.brands;
                var ideaThemes = data.ideaThemes;
                if (brands > 0 && ideaThemes > 0) {
                    SMManualSocialCampaignAdd();
                }
                else {
                    $('#app-marketing-social-auto-dependencies').empty();
                    $('#app-marketing-social-auto-dependencies').load('/SalesMarketing/DependencyCheckPartial?type=1', function () {
                        $('#app-marketing-social-auto-dependencies').modal('show');
                    });
                }
            } else {
                LoadingOverlayEnd();
                cleanBookNotification.error(data.message, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function SMManualSocialCampaignAdd() {
    $('#app-marketing-manual-social-campaign-add').empty();
    $('#app-marketing-social-campaign-add').empty();
    $('#app-marketing-email-campaign-add').empty();
    $('#app-marketing-manual-social-campaign-add').load('/SalesMarketing/ManualSocialCampaignAdd', function () {
        $('#app-marketing-manual-social-campaign-add').modal('show');
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#social-campaign-workgroup').select2({ placeholder: "Please select" });
        $('#slBrandCampaign').select2({
            placeholder: "Please select",
            allowClear: true
        });
        isValidWorkgroup();
        //load script init
        var $frm_campaign_add = $('#frm_marketing-social-campaign_add');
        $frm_campaign_add.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });

        $(".previewimg").change(function () {
            var target = $(this).data('target');
            readImgURL(this, target);
            $(target).fadeIn();
        });
        disableCampaignForm(true);
        //End load script
    });

}

function DPEmailCampaignAdd() {
    $.ajax({
        url: "/SalesMarketing/GetDependencyCheckManual",
        type: "GET",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.status) {
                var brands = data.brands;
                var ideaThemes = data.ideaThemes;
                var segments = data.segments;
                if (brands > 0 && ideaThemes > 0 && segments > 0) {
                    EmailCampaignAdd();
                }
                else {
                    $('#app-marketing-social-auto-dependencies').empty();
                    $('#app-marketing-social-auto-dependencies').load('/SalesMarketing/DependencyCheckPartial?type=3', function () {
                        $('#app-marketing-social-auto-dependencies').modal('show');
                    });
                }
            } else {
                LoadingOverlayEnd();
                cleanBookNotification.error(data.message, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function EmailCampaignAdd() {
    $('#app-marketing-manual-social-campaign-add').empty();
    $('#app-marketing-social-campaign-add').empty();
    $('#app-marketing-email-campaign-add').empty();
    $('#app-marketing-email-campaign-add').load('/SalesMarketing/EmailCampaignAdd', function () {
        $('#app-marketing-email-campaign-add').modal('show');
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#social-campaign-workgroup').select2({ placeholder: "Please select" });
        $('#slBrandCampaign').select2({
            placeholder: "Please select",
            allowClear: true
        });
        isValidWorkgroup();
        //load script init
        var $frm_email_campaign_add = $('#frm_marketing-email-campaign_add');
        $frm_email_campaign_add.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });

        $(".previewimg").change(function () {
            var target = $(this).data('target');
            readImgURL(this, target);
            $(target).fadeIn();
        });
        disableCampaignForm(true);
        $(".select2").select2();
        //End load script
    });

}

function SocialBrandOptionsLoad(el) {
    var brandId = $(el).val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/SocialBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            });
        }
    } else {
        $('#brandoptions').hide();
    }

}

function SocialBrandOptionsLoadForEdit(el) {
    var brandId = $(el).val();
    var cbrandId = $('#hdfBrandId').val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/SocialBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                if (cbrandId === brandId) {
                    var brandProducts = $('#hdfBrandProducts').val().split(",");
                    var valuePropositons = $('#hdfValuePropositons').val().split(",");
                    var brandAttributes = $('#hdfAttributes').val().split(",");
                    $('#brandoptions select[name=brandproducts]').val(brandProducts);
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val(brandAttributes);
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val(valuePropositons);
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                } else {
                    $('#brandoptions select[name=brandproducts]').val('');
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val('');
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val('');
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                }
            });
        }
    } else {
        $('#brandoptions').hide();
    }
}

function EmailBrandOptionsLoad(el) {
    var brandId = $(el).val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/EmailBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            });
        }
    } else {
        $('#brandoptions').hide();
    }
}

function EmailBrandOptionsLoadForEdit(el) {
    var brandId = $(el).val();
    var cbrandId = $('#hdfBrandId').val();
    if (brandId > 0) {
        $('#brandoptions').show();
        if (brandId) {
            $('#brandoptions').empty();
            $('#brandoptions').load('/SalesMarketing/EmailBrandOptions', { brandId: brandId }, function () {
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
                if (cbrandId === brandId) {
                    var brandProducts = $('#hdfBrandProducts').val().split(",");
                    var valuePropositons = $('#hdfValuePropositons').val().split(",");
                    var brandAttributes = $('#hdfAttributes').val().split(",");
                    $('#brandoptions select[name=brandproducts]').val(brandProducts);
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val(brandAttributes);
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val(valuePropositons);
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                } else {
                    $('#brandoptions select[name=brandproducts]').val('');
                    $('#brandoptions select[name=brandproducts]').multiselect('refresh');
                    $('#brandoptions select[name=attributes]').val('');
                    $('#brandoptions select[name=attributes]').multiselect('refresh');
                    $('#brandoptions select[name=valueprops]').val('');
                    $('#brandoptions select[name=valueprops]').multiselect('refresh');
                }
            });
        }
    } else {
        $('#brandoptions').hide();
    }
}

function SocialTwitter() {
    //load script init
    var $frm_twitter = $('#frm-twitter');
    $frm_twitter.submit(function (e) {
        e.preventDefault();
        if ($frm_twitter.valid()) {
            $.LoadingOverlay("show");
            var frmData = new FormData($frm_twitter[0]);
            
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: 'multipart/form-data',
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                    isBusyAddTaskForm = true;
                },
                success: function (data) {
                    if (data !== null) {
                        window.location.href = data.AuthorizationURL;
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                        isBusyAddTaskForm = false;
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusyAddTaskForm = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            });
        }
    })
}
function SocialFacebookPage() {
    //load script init

    var $frm_facebook = $('#frm-facebook');
    $frm_facebook.submit(function (e) {

        e.preventDefault();
        if ($frm_facebook.valid()) {
            $.LoadingOverlay("show");
            var frmData = new FormData($frm_facebook[0]);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: 'multipart/form-data',
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                    isBusyAddTaskForm = true;
                },
                success: function (data) {
                    
                    if (data !== null) {
                        window.location.href = data.AuthorizationURL;
                        //window.open(data.AuthorizationURL)
                        //window.open(data.AuthorizationURL, 'fbShareWindow', 'height=450, width=550, top=' + ($(window).height() / 2 - 275) + ', left=' + ($(window).width() / 2 - 225) + ', toolbar=0, location=0, menubar=0, directories=0, scrollbars=0');
                        //return false;
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                        isBusyAddTaskForm = false;
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusyAddTaskForm = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            });
        }
    })
}
function SocialFacebookGroup() {
    //load script init
    isFacebookPage = false;
    var $frmg_facebook = $('#frm-facebook-group');

    $frmg_facebook.submit(function (e) {
        e.preventDefault();

        if ($frmg_facebook.valid()) {
            $.LoadingOverlay("show");
            var frmData = new FormData($frmg_facebook[0]);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: 'multipart/form-data',
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                    isBusyAddTaskForm = true;
                },
                success: function (data) {
                    
                    if (data !== null) {
                        window.location.href = data.AuthorizationURL;
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                        isBusyAddTaskForm = false;
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusyAddTaskForm = false;
                    LoadingOverlayEnd();
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            });
        }
    })
}
function AddProfileFacebookPage(id, name, picture, access_token) {
    $.LoadingOverlay("show");
    var model = {
        id: id,
        name: name,
        picture: picture,
        access_token: access_token
    }
    $.ajax({
        url: "/SalesMarketing/AddProfileFacebookPage",
        data: model,
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.status) {
                $('#fp_' + id).remove();
                manage_options('#options-config');
                $('.section-detail').hide();
                $('.intro-config').show();
                $('.appnav').hide();
                LoadSocicalConfigs();
                cleanBookNotification.success(_L("ERROR_MSG_207"), "Sales Marketing");
            } else {
                LoadingOverlayEnd();
                cleanBookNotification.error(data.message, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function AddProfileFacebookGroup(id, name, picture, access_token) {
    $.LoadingOverlay("show");
    var model = {
        id: id,
        name: name,
        picture: picture,
        access_token: access_token
    }
    $.ajax({
        url: "/SalesMarketing/AddProfileFacebookGroup",
        data: model,
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.status) {
                $('#fg_' + id).remove();
                manage_options('#options-config');
                $('.section-detail').hide();
                $('.intro-config').show();
                $('.appnav').hide();
                LoadSocicalConfigs();
                cleanBookNotification.success(_L("ERROR_MSG_207"), "Sales Marketing");
            } else {
                LoadingOverlayEnd();
                cleanBookNotification.error(data.message, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function LoadModalSocialNetworkFacebook(id) {
    $.getJSON("/SalesMarketing/GetSocialNetworkById", { id: id, provider: "Facebook" }, function (data) {
        if (data !== null) {
            SocialFacebookPage();
            $('#fbpClientId').val(data.ClientId);
            $('#fbpClientSecret').val(data.ClientId);
        }
    });
}
function LoadModalSocialNetworkTwitter(id) {
    $.getJSON("/SalesMarketing/GetSocialNetworkById", { id: id, provider: "Twitter" }, function (data) {
        if (data !== null) {
            SocialTwitter();
            $('#twConsumerKey').val(data.ConsumerKey);
            $('#twconsumerSecret').val(data.ConsumerSecret);
            $('#twUserAccessToken').val(data.UserAccessToken);
            $('#twUserAccessSecret').val(data.UserAccessSecret);
        }
    });
}
function readImgURL(input, target) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(target).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function ReloadTopics() {
    var qid = $('#setting_qbicle').val();
    var $setting_topic = $('#setting_topic');
    var $btnwg = $('#btn-addWorkgroup');
    if (qid > 0) {
        $.getJSON("/SalesMarketing/LoadTopicsByQbicleId", { qid: qid }, function (data) {
            if (data && data.length > 0) {
                $setting_topic.prop("disabled", false);
                $btnwg.prop("disabled", false);
                $setting_topic.empty();
                $setting_topic.select2({
                    data: data,
                    placeholder: "Please select"
                });
                updateSetting();
            }
        });
    } else {
        $setting_topic.prop("disabled", true);
        $btnwg.prop("disabled", true);
    }
}
function updateSetting() {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/UpdateSetting",
        data: { id: $('#settingId').val(), qId: $('#setting_qbicle').val(), tId: $('#setting_topic').val() },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_208"), "Sales Marketing");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function isValidWorkgroup() {
    var workgroupid = $('#social-campaign-workgroup').val();
    var _opselect = $('#social-campaign-workgroup option:selected');
    $('td.info-process').text(_opselect.attr('process'));
    $('td.info-members').text(_opselect.attr('members'));
    disableCampaignForm(workgroupid ? false : true);
}
function disableCampaignForm(isDisable) {
    var frm = "#frm_marketing-social-campaign_add";
    if (isDisable) {
        $(frm + " input[name=Name]").prop("disabled", true);
        $(frm + " input[name=foldername]").prop("disabled", true);
        $(frm + " input[name=featuredimg]").prop("disabled", true);
        $(frm + " textarea[name=Details]").prop("disabled", true);
        $('#social-campaign-folders').prop("disabled", true);
        $('#slBrandCampaign').prop("disabled", true);
        $(frm + " .checkmulti").multiselect('disable');
        $('.preview-workgroup').hide();
    } else {
        $(frm + " input[name=Name]").prop("disabled", false);
        $(frm + " input[name=foldername]").prop("disabled", false);
        $(frm + " input[name=featuredimg]").prop("disabled", false);
        $(frm + " textarea[name=Details]").prop("disabled", false);
        $('#social-campaign-folders').prop("disabled", false);
        $(frm + " .checkmulti").multiselect('enable');
        var wg = $('#social-campaign-workgroup');
        $('.preview-workgroup .info-process').text(wg.attr("process"));
        $('.preview-workgroup .info-members').text(wg.attr("members"));
        $('#slBrandCampaign').prop("disabled", false);
        $('.preview-workgroup').show();
        ReloadFolders();
    }
}
function socialNextTheme() {
    $("#v-campaigntitle").html($("#campaigntitle").val());
    $("#v-campaignsummary").html($("#campaignsummary").val());
    $('#article-feature-img').css('background-image', 'url(' + imageSrc + ')');
}
$("#load-camp").on("click", function () {
    LoadSocicalCampains(false);
});
function LoadSocicalCampains(isReset) {
    var keyword = $('#social-cmp-search').val();
    var targnetworks = $('#social-cmp-target-type').val();
    var campaigntype = $('#social-cmp-campaign-type').val();
    $.LoadingOverlay("show");
    if (isReset) {
        $.ajax({
            url: "/SalesMarketing/CountSocialCampains",
            data: {
                search: keyword, targnetworks: targnetworks, campaigntype: campaigntype
            },
            async: false,
            type: "POST",
            success: function (result) {
                totalCamp = result;
                skip = 0;
                if (totalCamp > take) {
                    $("#load-camp").show();
                }
                $('#campaigns-social div.from-community').html("");
            },
            error: function (error) {
                totalCamp = 0;
                skip = 0;
            }
        });
    }

    var html = $('#campaigns-social div.from-community').html();
    $('#campaigns-social div.from-community').load("/SalesMarketing/LoadSocialCampains", { search: keyword, targnetworks: targnetworks, campaigntype: campaigntype, skip: skip, take: take }, function () {
        $('#campaigns-social div.from-community').prepend(html);
        if (totalCamp == $('#campaigns-social div.from-community .col').length) {
            $("#load-camp").hide();
        } else {
            $("#load-camp").show();
        }
        skip += take;
        LoadingOverlayEnd();
    });
}
$("#load-email-camp").on("click", function () {
    LoadEmailCampaigns(false);
});
function LoadEmailCampaigns(isReset) {
    var keyword = $('#email-campaign-search').val();
    var targetsegments = $('#target-segment-search').val();
    $.LoadingOverlay("show");
    if (isReset) {
        $.ajax({
            url: "/SalesMarketing/CountEmailCampaigns",
            data: {
                search: keyword, targetsegments: targetsegments
            },
            async: false,
            type: "POST",
            success: function (result) {
                totalCamp = result;
                skip = 0;
                if (totalCamp > take) {
                    $("#load-email-camp").show();
                }
                $('#campaigns-email div.from-community').html("");
            },
            error: function (error) {
                totalCamp = 0;
                skip = 0;
            }
        });
    }
    var html = $('#campaigns-email div.from-community').html();
    $('#campaigns-email div.from-community').load("/SalesMarketing/LoadEmailCampaigns", { search: keyword, targetsegments: targetsegments, skip: skip, take: take }, function () {
        $('#campaigns-email div.from-community').prepend(html);
        if (totalCamp == $('#campaigns-email div.from-community .col').length) {
            $("#load-email-camp").hide();
        } else {
            $("#load-email-camp").show();
        }
        skip += take;
        LoadingOverlayEnd();
    });
    $.LoadingOverlay("hide");
}

function LoadSocicalConfigs() {
    $.LoadingOverlay("show");
    $("#app-config").load("/SalesMarketing/LoadSocicalConfigs", function () {
        $('#setting_qbicle,#setting_topic').select2({ placeholder: "Please select" });
        InitDatatableEmailTemplate();
        initSESIdentitiesTable();
        LoadingOverlayEnd();
    })
}
function addMembers(id) {
    var members = $('#slMembers').val();
    if (!members) {
        members = [];
    }
    members.push(id);
    $('#slMembers').val(members);
}
function removeMembers(id) {
    var members = $('#slMembers').val();
    if (members)
        members.remove(id);
    $('#slMembers').val(members);
    //remove approval 94b7decd-2740-4f80-9718-b6c5340be87e
    $("#apr" + id).prop("checked", false);
    $("#apr" + id).change();
}
function isApprover(id, thiss) {
    if ($(thiss).prop("checked")) {
        var ReviewersApprovers = $('#slReviewersApprovers').val();
        if (!ReviewersApprovers) {
            ReviewersApprovers = [];
        }
        ReviewersApprovers.push(id);
        $('#slReviewersApprovers').val(ReviewersApprovers);
    } else {
        var ReviewersApprovers = $('#slReviewersApprovers').val();
        if (ReviewersApprovers)
            ReviewersApprovers.remove(id);
        $('#slReviewersApprovers').val(ReviewersApprovers);
    }
}
function FilterMembers() {
    try {
        var kw = $('#smkeyword').val();
        var filterShow = $('#slShow').val();
        if (kw) {
            $("#wgMembers li").each(function () {
                var elLi = $(this);
                var name = elLi.attr("fullname");
                if (filterShow == "1") {
                    if (elLi.hasClass("ismember") && name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                } else {
                    if (name.toLowerCase().indexOf(kw.toLowerCase()) !== -1) {
                        elLi.show();
                    } else {
                        elLi.hide();
                    }
                }
            });
        } else {
            if (filterShow == "1") {
                $("ul.widget-contacts li.ismember").show();
                $("ul.widget-contacts li:not(.ismember)").hide();
            } else {
                $("ul.widget-contacts li").show();
            }
        }

    } catch (e) {
        return;
    }

}
function SyncTrader() {
    $("#app-marketing-trader-sync .affected").load("/SalesMarketing/LoadSyncTrader", function () {
        $('#trader-table').DataTable({
            destroy: true,
            responsive: true,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });
        $('.loading').hide();
        $('.affected').fadeIn();
    });
}
function LoadContacts() {
    $(".mdv2-col-dash").LoadingOverlay("show");
    $("#contacts-ages").load("/SalesMarketing/LoadContact", function () {
        $('#age-ranges-table').DataTable({
            destroy: true,
            responsive: true,
            searching: false,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });
        $(".mdv2-col-dash").LoadingOverlay("hide");
    });
}

function onChangeAgeRange(id, idCus, value, index) {
    var startValue = parseInt($('#ip-start-age-range-' + id).val());
    var endValue = parseInt($('#ip-end-age-range-' + id).val());
    if (startValue < 0) {
        cleanBookNotification.error(_L("ERROR_MSG_209"), "Sales Marketing");
    } else if (endValue < 0) {
        cleanBookNotification.error(_L("ERROR_MSG_210"), "Sales Marketing");
    } else if (startValue > endValue) {
        cleanBookNotification.error(_L("ERROR_MSG_211"), "Sales Marketing");
    } else {
        $.ajax({
            url: "/SalesMarketing/SaveAgeRange",
            data: {
                id: id,
                idCus: idCus,
                start: startValue,
                end: endValue
            },
            async: false,
            type: "POST",
            success: function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_212"), "Sales Marketing");
                } else if (data.msg) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        });
        LoadingOverlayEnd();
    }
}

function AddRowContacTable(idCus) {
    $.ajax({
        url: "/SalesMarketing/SaveAgeRange",
        data: {
            id: -1,
            idCus: idCus,
            start: 1,
            end: 10
        },
        async: false,
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                LoadContacts();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
    LoadingOverlayEnd();
}

function DeleteAgeRange(id) {
    var r = confirm("Do you want remove this record?");
    if (r == true) {
        $.ajax({
            url: "/SalesMarketing/DeleteAgeRange",
            data: {
                id: id,
            },
            async: false,
            type: "POST",
            success: function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    LoadContacts();
                    cleanBookNotification.success(_L("ERROR_MSG_213"), "Sales Marketing");
                } else if (data.msg) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            },
            error: function (error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        });
        LoadingOverlayEnd();
    }

}

function LoadModalWorkgroup(id) {
    $("#app-marketing-workgroup-add").load("/SalesMarketing/LoadModalWorkgroup", { id: id }, function () {
        $('#source-qbicle,#default-topic,#slShow').select2();
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $("#slProcess").change(function () {
            $frmworkgroupaddedit.valid();
        });
        var $frmworkgroupaddedit = $('#frm-workgroup-addedit');
        $frmworkgroupaddedit.validate({
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                },
                Process: {
                    required: true
                }
            }
        });
        // Cycle app nav tabs with button triggers
        $('.btnNext').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('.app_subnav .active').next('li').find('a').trigger('click');
        });

        $('.btnPrevious').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
        });
        $frmworkgroupaddedit.submit(function (e) {
            e.preventDefault();
            var domainId = $('#domainId-workgroup').val();
            if (domainId === '') {
                cleanBookNotification.error(_L("ERROR_MSG_214"), "Sales Marketing");
                return;
            }
            var defaultTopic = $('#default-topic').val();
            if (!defaultTopic) {
                cleanBookNotification.error(_L("ERROR_MSG_215"), "Sales Marketing");
                return;
            }
            if ($frmworkgroupaddedit.valid()) {
                $.LoadingOverlay("show");
                var form_data = $(this).serialize();
                $.ajax({
                    type: this.method,
                    url: this.action,
                    data: form_data,
                    beforeSend: function (xhr) {
                        isBusyAddTaskForm = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $("#app-marketing-workgroup-add").modal('hide');
                            var wgid = parseInt($('#wg-id').val());
                            if (wgid > 0) {
                                cleanBookNotification.success(_L("ERROR_MSG_216"), "Sales Marketing");
                            } else
                                cleanBookNotification.success(_L("ERROR_MSG_217"), "Sales Marketing");
                            $('#workgroups').click();
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                        }
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            }
            else {
                $('.app_subnav a[href="#add-specifics"]').click();
            }
        });
    });
}
function LoadTableWorkgroup() {
    $.LoadingOverlay("show");
    $("#content-workgroup-table").load("/SalesMarketing/LoadTableWorkgroup", function () {
        $('#workgroups-table').DataTable({
            destroy: true,
            responsive: true,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });
        LoadingOverlayEnd();
    });
}


function DeleteWorkgroup() {
    $.LoadingOverlay("show");
    var id = $('#wg-delete-id').val();
    $.ajax({
        url: "/SalesMarketing/DeleteWorkgroupById",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            $('#app-marketing-delete-workgroup').modal('hide');
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_218"), "Sales Marketing");
                $('#wg-delete-id').val(0);
                LoadTableWorkgroup();
            } else if (data.msg) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function DeleteSocialNetwork() {
    $.LoadingOverlay("show");
    var id = $('#sn-delete-id').val();
    $.ajax({
        url: "/SalesMarketing/DeleteSocialNetworkById",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            $('#app-marketing-delete-socialnetwork').modal('hide');

            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_219"), "Sales Marketing");
                $('#sn-delete-id').val(0);
                LoadSocicalConfigs();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SetDisableSocialNetwork(id) {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/SetDisableSocialNetworkById",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();

            if (data.result) {
                //cleanBookNotification.success("The social network successfully...", "Sales Marketing");
                LoadSocicalConfigs();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
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
function AutoGenerateFolderName() {
    var opwgSelect = $('#social-campaign-workgroup option:selected');
    var qbicleId = opwgSelect.attr("qbicleid");
    if (qbicleId) {
        $.getJSON("/SalesMarketing/AutoGenerateFolderName", { qbicleId: qbicleId }, function (data) {
            if (data) {
                $('#social-newfolder-name').val(data).change();
            }
        });
    }
}

function SocialBrandFormReset() {
    $('#frm-marketing-brand').trigger("reset");
    $('select[name=FeaturedImageUri]').val('').change();
    $('.foldername').hide();
    $('#frm-marketing-brand').validate().resetForm();
    $('#brandFolderName').removeClass('valid');
    $('#imgbrandpreview').hide();

}
function getTotalPageBrand() {
    $.LoadingOverlay("show");
    $.ajax({
        url: '/SalesMarketingBrand/LoadBrands',
        type: "GET",
        data: {
            skip: 0,
            take: 0,
            keyword: $('#brandSearch').val(),
            isLoadingHide: $("#isLoadingHideBrand").is(':checked') ? true : false
        },
        async: true,
        success: function (response) {
            if (response.result) {
                var totalRecord = response.Object.totalRecord;
                if (totalRecord > 0) {
                    initPagination(totalRecord, brandPageSize, '#brandPaginateTemplate');
                } else {
                    $('#app-brands .from-community').html('');
                    $("#brandPaginateTemplate").css("display", "none");
                }
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function SocialBrandLoad(skip, take) {
    $.LoadingOverlay("show");
    $.ajax({
        url: '/SalesMarketingBrand/LoadBrands',
        type: "GET",
        data: {
            skip: skip,
            take: take,
            keyword: $('#brandSearch').val(),
            isLoadingHide: $("#isLoadingHideBrand").is(':checked') ? true : false
        },
        async: true,
        success: function (response) {
            if (response.result) {
                $('#app-brands .from-community').html(response.Object.strResult);
                $("#brandPaginateTemplate").css("display", "block");
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function initPagination(totalRecord, pageSize, elementID) {
    if (totalRecord != 0) {
        var container = $(elementID);
        var sources = function () {
            var result = [];
            for (var i = 1; i <= totalRecord; i++) {
                result.push(i);
            }
            return result;
        }();

        var options = {
            prevText: '&nbsp; &laquo; Prev &nbsp;',
            nextText: '&nbsp; Next &raquo; &nbsp;',
            currentPage: 1,
            pageSize: pageSize,
            dataSource: sources,
            callback: function (response, pagination) {
                switch (elementID) {
                    case '#brandPaginateTemplate': SocialBrandLoad((pagination.pageNumber - 1) * pageSize, pageSize); break;
                    case '#segmentPaginateTemplate': SocialSegmentContent((pagination.pageNumber - 1) * pageSize, pageSize); break;
                    case '#placePaginateTemplate': LoadPlace((pagination.pageNumber - 1) * pageSize, pageSize); break;
                    case '#areaPaginateTemplate': LoadArea((pagination.pageNumber - 1) * pageSize, pageSize); break;
                    case '#pipelinePaginateTemplate': LoadPipeline((pagination.pageNumber - 1) * pageSize, pageSize); break;
                    case '#ideaPaginateTemplate': LoadIdeas((pagination.pageNumber - 1) * pageSize, pageSize); break;
                }
            }
        };
        container.pagination(options);
    }
}
function SocialBrandAutoGenerateFolderName() {
    $.getJSON("/SalesMarketingBrand/AutoGenerateFolderName", function (data) {
        if (data) {
            $('#brandFolderName').val(data);
        }
    });
}

function LoadModalIdea(id) {
    $("#app-marketing-idea-add").load("/SalesMarketingIdea/GenerateModalIdeaAddEdit", { ideaId: id }, function () {
        $(".previewimgidea").change(function () {
            var target = $(this).data('target');
            readImgURL(this, target);
            $(target).fadeIn();
        });
        var $frm_idea_theme = $('#frm-idea-theme');
        $frm_idea_theme.validate({
            rules: {
                Name: {
                    required: true,
                    minlength: 5,
                    maxlength: 35
                },
                Explanation: {
                    required: true,
                    maxlength: 200
                },
                Url: {
                    url: true
                }
            }
        });

    });
}

function ProcessSMIdea() {
    var $frm_idea_theme = $('#frm-idea-theme');
    var ideatype = $('#frm-idea-theme select[name=Type]').val();
    if (!ideatype) {
        $frm_idea_theme.validate().showErrors({ Type: "This field is required." });
        return;
    }
    var folder = $('#frm-idea-theme select[name=ResourcesFolder]').val();
    if (!folder) {
        $frm_idea_theme.validate().showErrors({ ResourcesFolder: "This field is required." });
        return;
    }
    if (folder == "0" && !$('#ideaFolderName').val()) {
        $frm_idea_theme.validate().showErrors({ FolderName: "This field is required." });
        return;
    }
    if (!$('#frm-idea-theme').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-idea-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-idea-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-idea-object-key").val(mediaS3Object.objectKey);
                $("#sm-idea-object-name").val(mediaS3Object.fileName);
                $("#sm-idea-object-size").val(mediaS3Object.fileSize);

                SubmitSMIdea();
            }
        });

    }
    else {
        $("#sm-idea-object-key").val("");
        $("#sm-idea-object-name").val("");
        $("#sm-idea-object-size").val("");
        SubmitSMIdea();
    }
}

function SubmitSMIdea() {
    var frmData = new FormData($('#frm-idea-theme')[0]);
    var lnks = [];
    $('#ideaLinks tr[lnkUrl]').each(function (index) {
        frmData.append("Links[]", $(this).attr("lnkUrl"));
    });
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingIdea/SaveIdea",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-idea-add').modal('hide');
                getTotalPageIdea();
                SocialIdeaFormReset();
                cleanBookNotification.success(_L("ERROR_MSG_198"), "Sales Marketing");
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");

            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}

function getTotalPageIdea() {
    $.LoadingOverlay("show");
    $.ajax({
        url: '/SalesMarketingIdea/LoadIdeas',
        type: "GET",
        data: {
            skip: 0,
            take: 0,
            keyword: $('#txtIdeaKeyword').val(),
            isActive: $('#slIdeaStatus').val(),
            isLoadingHide: $("#isLoadingHideIdea").is(':checked') ? true : false
        },
        async: true,
        success: function (response) {
            if (response.result) {
                var totalRecord = response.Object.totalRecord;
                if (totalRecord > 0) {
                    initPagination(totalRecord, ideaPageSize, '#ideaPaginateTemplate');
                } else {
                    $('#app-ideas .from-community').html('');
                    $("#ideaPaginateTemplate").css("display", "none");
                }
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadIdeas(skip, take) {
    $.LoadingOverlay("show");

    $.ajax({
        url: '/SalesMarketingIdea/LoadIdeas',
        type: "GET",
        data: {
            skip: skip,
            take: take,
            keyword: $('#txtIdeaKeyword').val(),
            isActive: $('#slIdeaStatus').val(),
            isLoadingHide: $("#isLoadingHideIdea").is(':checked') ? true : false
        },
        async: true,
        success: function (response) {
            if (response.result) {
                $('#app-ideas .from-community').html(response.Object.strResult);
                $("#ideaPaginateTemplate").css("display", "block");
            }
            LoadingOverlayEnd();
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function ShowOrHideIdea(id) {
    $.ajax({
        url: "/SalesMarketingIdea/ShowOrHideIdea",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPageIdea();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SocialIdeaAutoGenerateFolderName() {
    $.getJSON("/SalesMarketingIdea/AutoGenerateFolderName", function (data) {
        if (data) {
            $('#ideaFolderName').val(data);
        }
    });
}
function SocialIdeaFormReset() {
    $('#frm-idea-theme').trigger("reset");
    $('select[name=Type]').val('').change();
    $('select[name=ResourcesFolder]').val('').change();
    $('#ideaFolderName').hide();
    $('#frm-idea-theme').validate().resetForm();
    $('#ideaFolderName').removeClass('valid');
    $('#imgideapreview').hide();

}
function extractHostname(el) {
    var url = document.createElement('a');
    url.href = $(el).val();
    if (url.origin) {
        return url.origin;
    }
    return '';
}
function SocialIdeaAddLink() {
    var $elurl = $('#frm-idea-theme input[name=Url]');
    if ($elurl.valid()) {
        var idealnk = extractHostname($elurl);
        if ($('#ideaLinks tr[lnkUrl="' + idealnk + '"]').length == 0) {
            $('#ideaLinks').show();
            var $op = '<tr lnkUrl="' + idealnk + '"><td><a href="' + idealnk + '" target="_blank"><i class="fa fa-external-link"></i> &nbsp; ' + idealnk + '</a></td><td> <button class="btn btn-danger" onclick="SocialIdeaRemoveLink(this)"><i class="fa fa-trash"></i></button></td></tr>';
            $('#ideaLinks').append($op);
            $('.links-associate').show();
            $elurl.val('');
        } else {
            $elurl.val('');
        }
    }
}
function SocialIdeaRemoveLink(el) {
    $(el).parent().parent().remove();
    if ($('#ideaLinks tr').length == 1) $('#ideaLinks').hide();
}
function socialIdeaUseInCampaign(id, isuse) {
    if (isuse) {
        $('.other').hide();
        $('#rs-' + id).show();
        $('.usetheme' + id).hide();
        $('.change' + id).show();
        $('#ideaId').val(id);
    } else {
        $('.other').show();
        $('.change' + id).hide();
        $('.usetheme' + id).show();
    }
}
function SocialContactCriteriaAdd() {
    var $frmContactCriteria = $('#frmContactCriteria');
    $frmContactCriteria.validate(
        {
            rules: {
                Label: {
                    required: true,
                    maxlength: 150
                }
            }
        });
    $frmContactCriteria.submit(function (event) {
        event.preventDefault(); //prevent default action 
        if (isBusyAddTaskForm)
            return;
        if ($frmContactCriteria.valid()) {
            $.LoadingOverlay("show");
            SocialReIndexPost();
            var post_url = $(this).attr("action"); //get form action url
            var request_method = $(this).attr("method"); //get form GET/POST method
            var form_data = $(this).serialize();
            $.ajax({
                url: post_url,
                type: request_method,
                data: form_data,
                beforeSend: function (xhr) {
                    isBusyAddTaskForm = true;
                },
                error: function (data) {
                    isBusyAddTaskForm = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            }).done(function (response) { //
                isBusyAddTaskForm = false;
                if (response.result) {
                    $('#app-marketing-contact-criteria-add').modal("hide");
                    SocialContactCriteriaContent();
                    if ($('#hdfCriteriaId').val() == "0") {
                        cleanBookNotification.success(_L("ERROR_MSG_226"), "Sales Marketing");
                        SocialContactCriteriaReset();
                    } else {
                        cleanBookNotification.success(_L("ERROR_MSG_227"), "Sales Marketing");
                        SocialContactCriteriaEdit(0);
                    }
                } else {
                    cleanBookNotification.error(response.msg, "Sales Marketing");
                }
                LoadingOverlayEnd();
            });
        }
    });
}
function SocialContactCriteriaReset() {
    $('#frmContactCriteria')[0].reset();
    $('#frmContactCriteria').validate().resetForm();
    $('#chkmandatoryfield').bootstrapToggle('off');
    $('.box-custome-option .input-group').remove();
}
function SocialCustomOptionAdd() {
    var _index_val = $('.box-custome-option div.input-group').length + 2;
    var _optionVal = '<div class="input-group option' + (_index_val + 1) + '" style="margin-bottom: 15px;">';
    _optionVal += '<input type="hidden" class="ipId" name="Options[' + _index_val + '].Id" value="0" />';
    _optionVal += '<input type="text" name="Options[' + _index_val + '].Label" class="form-control ipLabel" placeholder="Option ' + (_index_val + 1) + '">';
    _optionVal += '<input type="hidden" class="ipDisplayOrder" name="Options[' + _index_val + '].DisplayOrder" value="' + (_index_val + 1) + '" />';
    _optionVal += '<span class="input-group-btn">';
    _optionVal += '<button class="btn btn-danger" onclick="$(this).parent().parent().remove();"><i class="fa fa-trash"></i></button></span></div>';
    $('#btnaddoption').before(_optionVal);
}
function SocialContactCriteriaContent() {
    $(".mdv2-col-dash").LoadingOverlay("show");
    $('#contacts-criteria').load("/SalesMarketingContact/LoadContentCriteria", function () {
        var $tblcriteria = $('#tblCriteriaDef').DataTable({
            'drawCallback': function (settings) {
                $('#tblCriteriaDef tr:last .dtMoveDown').remove();

                // Remove previous binding before adding it
                $('.dtMoveUp').unbind('click');
                $('.dtMoveDown').unbind('click');

                // Bind clicks to functions
                $('.dtMoveUp').click(tblcriteria_moveUp);
                $('.dtMoveDown').click(tblcriteria_moveDown);
            }
        });
        function tblcriteria_moveUp() {
            var tr = $(this).parents('tr');
            tblcriteria_moveRow(tr, 'up');
        }
        function tblcriteria_moveDown() {
            var tr = $(this).parents('tr');
            tblcriteria_moveRow(tr, 'down');
        }
        function tblcriteria_moveRow(row, direction) {
            var index = $tblcriteria.row(row).index();

            var order = -1;
            if (direction === 'down') {
                order = 1;
            }

            var data1 = $tblcriteria.row(index).data();
            data1[0] = parseInt(data1[0]) + order;
            var indexUp = index + order;
            var $data1 = $(data1[4]);
            if (indexUp > 0) {
                if ($data1.find(".dtMoveUp").length == 0)
                    $data1.find('ul.dropdown-menu').prepend("<li><a href=\"#\" class=\"dtMoveUp\">Move up</a></li>");
                if ($data1.find(".dtMoveDown").length == 0) {
                    if ($data1.find('ul.dropdown-menu .dtMoveUp').length == 0)
                        $data1.find('ul.dropdown-menu').prepend("<li><a href=\"#\" class=\"dtMoveDown\">Move down</a></li>");
                    else
                        $data1.find('ul.dropdown-menu .dtMoveUp').after("<li><a href=\"#\" class=\"dtMoveDown\">Move down</a></li>");
                }

            } else {
                $data1.find(".dtMoveUp").parent().remove();
            }
            data1[4] = "<div class=\"btn-group options\">" + $data1.html() + "</div>";

            var data2 = $tblcriteria.row(index + order).data();
            data2[0] = parseInt(data2[0]) - order;
            var indexdown = index;
            var $data2 = $(data2[4]);
            if (indexdown > 0) {
                if ($data2.find(".dtMoveUp").length == 0)
                    $data2.find('ul.dropdown-menu').prepend("<li><a href=\"#\" class=\"dtMoveUp\">Move up</a></li>");
                if ($data2.find(".dtMoveDown").length == 0) {
                    if ($data2.find('ul.dropdown-menu .dtMoveUp').length == 0)
                        $data2.find('ul.dropdown-menu').prepend("<li><a href=\"#\" class=\"dtMoveDown\">Move down</a></li>");
                    else
                        $data2.find('ul.dropdown-menu .dtMoveUp').after("<li><a href=\"#\" class=\"dtMoveDown\">Move down</a></li>");
                }
            } else {
                $data2.find(".dtMoveUp").parent().remove();
            }
            data2[4] = "<div class=\"btn-group options\">" + $data2.html() + "</div>";;

            $tblcriteria.row(index).data(data2);
            $tblcriteria.row(indexUp).data(data1);
            $tblcriteria.draw(false);
            SocialMoveUpDownOrderCriteria(data1[5], indexUp + 1, data2[5], index + 1);
        }
        $(".mdv2-col-dash").LoadingOverlay("hide");
    });
}
function SocialContactCriteriaEdit(criteriaId) {
    $('#app-marketing-contact-criteria-add').load("/SalesMarketingContact/GenerateModalCriteriaAddEdit", { criteriaId: criteriaId }, function () {
        $('#chkmandatoryfield').bootstrapToggle();
        SocialContactCriteriaAdd();
        if (criteriaId > 0)
            $('#app-marketing-contact-criteria-add').modal('show');
    });
}
function SocialContactCriteriaRemove(criteriaId) {
    if (isBusyAddTaskForm)
        return;
    $.ajax({
        url: '/SalesMarketingContact/RemoveCriteria',
        type: 'post',
        data: { criteriaId: criteriaId },
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        error: function (data) {
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).done(function (response) { //
        isBusyAddTaskForm = false;
        if (response.result) {
            $('#app-marketing-contact-criteria-add').modal("hide");
            SocialContactCriteriaContent();
            cleanBookNotification.success(_L("ERROR_MSG_228"), "Sales Marketing");
        } else {
            cleanBookNotification.error(response.msg, "Sales Marketing");
        }
        LoadingOverlayEnd();
    });
}
function SocialContactCriteriaStatus(criteriaId, status) {
    if (isBusyAddTaskForm)
        return;
    $.ajax({
        url: '/SalesMarketingContact/SetStatusContactCriteria',
        type: 'post',
        data: { criteriaId: criteriaId, status: status },
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        error: function (data) {
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).done(function (response) { //
        isBusyAddTaskForm = false;
        if (response.result) {
            SocialContactCriteriaContent();
            cleanBookNotification.success(_L("ERROR_MSG_229"), "Sales Marketing");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
        LoadingOverlayEnd();
    });
}
function SocialReIndexPost() {
    $('.box-custome-option .input-group').each(function (index) {
        var $ipid = $(this).find(".ipId");
        var $ipLabel = $(this).find(".ipLabel");
        var $ipDisplayOrder = $(this).find(".ipDisplayOrder");
        var _index_val = 2 + index;
        if ($ipid.length > 0) {
            var _name = $ipid.attr("name");
            var $name = $('input[name="' + _name + '"]');
            $name.attr('placeholder', 'Option ' + (_index_val + 1));
            $name.attr('name', 'Options[' + _index_val + '].Id');
        }
        if ($ipLabel.length > 0) {
            var _label = $ipLabel.attr("name");
            var $label = $('input[name="' + _label + '"]');
            $label.attr('placeholder', 'Option ' + (_index_val + 1));
            $label.attr('name', 'Options[' + _index_val + '].Label');
        }
        if ($ipDisplayOrder.length > 0) {
            var _displayOrder = $ipDisplayOrder.attr("name");
            var $displayOrder = $('input[name="' + _displayOrder + '"]');
            $displayOrder.attr('placeholder', 'Option ' + (_index_val + 1));
            $displayOrder.attr('name', 'Options[' + _index_val + '].DisplayOrder');
            $displayOrder.val(_index_val + 1);
        }
    });
}
function SocialMoveUpDownOrderCriteria(_moveupid, _moveuporder, _movedownid, _movedownorder) {
    if (isBusyAddTaskForm)
        return false;
    var _data = {
        MoveUpId: _moveupid,
        MoveUpOrder: _moveuporder,
        MoveDownId: _movedownid,
        MoveDownOrder: _movedownorder
    };
    $.ajax({
        url: '/SalesMarketingContact/MoveUpDownOrderCriteria',
        type: 'post',
        data: _data,
        async: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        error: function (data) {
            isBusyAddTaskForm = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            return false;
        }
    }).done(function (response) { //
        isBusyAddTaskForm = false;
        if (response.result) {
            cleanBookNotification.success(_L("ERROR_MSG_230"), "Sales Marketing");
            return true;
        } else {
            cleanBookNotification.error(response.msg, "Sales Marketing");
            return false;
        }
    });
}

function SocialSegmentFormReset() {
    $('#frm-segment').trigger("reset");
    $('#segment-1 select[name=Type]').val('').change();
    $('#segment-1 select[name=Areas]').val([]).change();
    $("#segment-1 select[name=Areas]").multiselect("refresh");
    $('#frm-segment').validate().resetForm();
    $('#more-criteria-content').empty();
    $('#tabSegment a[href="#segment-1"]').tab('show');
}
function SocialMoreCriteria() {
    var el_count = $('#more-criteria-content .criteria-el').length + 1;
    var _crs = [];
    $('.item-criterial').each(function (index) {
        var _vl = $(this).val();
        if (_vl)
            _crs.push(_vl);
        else {
            cleanBookNotification.error(_L("ERROR_MSG_370"), "Sales Marketing");
            return;
        }
    });
    $.ajax({
        url: '/SalesMarketingSegment/GenerateMoreCriteria',
        type: 'get',
        data: { index: el_count, criterias: JSON.stringify(_crs) },
        error: function (data) {
            isBusyAddTaskForm = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        },
        success: function (response) {
            if (response) {
                var $response = $(response);
                if ($('#more-criteria-content div.criteria-el').length == 0) {
                    $response.find('.lblAnd').remove();
                }
                $('#more-criteria-content').append($response);
                $('.select2').select2({
                    placeholder: 'Please select'
                });
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            }
        }
    });
}
function SocialGenerateList(c_contacts) {
    if ($('#frm-segment').valid()) {
        var _clauses = [];
        $('.criteria-el').each(function (index) {
            var _criteriaVal = $('.criteria-el select[name="Criterias[' + index + '].CriteriaId"]').val();
            var _criteriaOption = $('.criteria-el select[name="Criterias[' + index + '].CriteriaValues"]').val();
            if (_criteriaVal && _criteriaOption) {
                _clauses.push({ CriteriaId: _criteriaVal, CriteriaValues: _criteriaOption });
            }
        });
        var _area = $('#segment-1 select[name=Areas]').val();
        var data = {
            clauses: _clauses,
            areaIds: _area ? _area : [],
            cContacts: c_contacts
        };
        $('#tabSegment a[href="#segment-3"]').tab('show');
        $('#lst-contact-content').load("/SalesMarketingSegment/GenerateListContact", data, function () {
            $('#lst-contacts').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'asc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            $('#lst-contacts').on('draw.dt', function () {
                $('#lst-contacts tbody input:checkbox').bootstrapToggle();
            });
            $('#lst-contacts tbody input:checkbox').bootstrapToggle();
        });
    }
}
function bindValCheckbox(el) {
    var elchk = $(el);
    if (elchk.prop("checked")) {
        $("#hdfSelectContacts option[value='" + elchk.val() + "']").attr('selected', 'selected');
    } else {
        $("#hdfSelectContacts option[value='" + elchk.val() + "']").removeAttr('selected');
    }
}
function getTotalPageSegment() {
    $(".mdv2-col-dash").LoadingOverlay("show");
    var _type = $('#segmentTypes').val();
    var data = {
        skip: 0,
        take: 0,
        keyword: $('#segmentSearch').val(),
        types: _type ? _type : [],
        isLoadingHide: $("#isLoadingHideSegment").is(':checked') ? true : false
    };
    $.ajax({
        url: '/SalesMarketingSegment/LoadContentSegment',
        type: "GET",
        data: data,
        async: true,
        success: function (response) {
            if (response.result) {
                var totalRecord = response.Object.totalRecord;
                if (totalRecord > 0) {
                    initPagination(totalRecord, segmentPageSize, '#segmentPaginateTemplate');
                } else {
                    $('#app-brands .from-community').html('');
                    $("#segmentPaginateTemplate").css("display", "none");
                }
            }
            $(".mdv2-col-dash").LoadingOverlay("hide");
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            $(".mdv2-col-dash").LoadingOverlay("hide");
        }
    });
}
function SocialSegmentContent(skip, take) {
    $(".mdv2-col-dash").LoadingOverlay("show");
    var _type = $('#segmentTypes').val();
    var data = {
        skip: skip,
        take: take,
        keyword: $('#segmentSearch').val(),
        types: _type ? _type : [],
        isLoadingHide: $("#isLoadingHideSegment").is(':checked') ? true : false
    };
    $.ajax({
        url: '/SalesMarketingSegment/LoadContentSegment',
        type: "GET",
        data: data,
        async: true,
        success: function (response) {
            if (response.result) {
                $('#contacts-segments .from-community').html(response.Object.strResult);
                $("#segmentPaginateTemplate").css("display", "block");
            }
            $(".mdv2-col-dash").LoadingOverlay("hide");
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            $(".mdv2-col-dash").LoadingOverlay("hide");
        }
    });
}
function SocialLoadOptionVal(cel, elid) {
    var _vl = $(cel).val();
    if (!_vl)
        return;
    var _clauses = [];
    $('.criteria-el').each(function (index) {
        var _criteriaVal = $('.criteria-el select[name="Criterias[' + index + '].CriteriaId"]').val();
        if (_criteriaVal) {
            _clauses.push(_criteriaVal);
        }
    });
    var found = _clauses.filter(function (element) {
        return element == _vl;
    });
    if (found && found.length > 1) {
        cleanBookNotification.error(_L("ERROR_MSG_205"), "Sales Marketing");
        $(cel).val('').change();
        return;
    }
    $.ajax({
        url: '/SalesMarketingSegment/GetOptionValuesByCriteriaId',
        type: 'get',
        data: { criteriaId: $(cel).val() },
        error: function (data) {
            isBusyAddTaskForm = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        },
        success: function (data) {
            if (data.length > 0) {
                var $elid = $('#' + elid);
                $('#' + elid + ' option').remove();
                $.each(data, function (index, value) {
                    $elid.append('<option value="' + value.id + '">' + value.text + '</option>');
                });
                $elid.multiselect('destroy');
                $elid.hide();
                $elid.multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            }
        }
    });
}
function LoadModalArea(id) {
    $("#app-marketing-area-add").load("/SalesMarketing/LoadModalArea", { id: id }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });

    });
}

function ProcessSMArea() {
    if (!$('#frm-area-addedit').valid())
        return;
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-area-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-area-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {


                $("#sm-area-object-key").val(mediaS3Object.objectKey);
                $("#sm-area-object-name").val(mediaS3Object.fileName);
                $("#sm-area-object-size").val(mediaS3Object.fileSize);

                SubmitSMAreaPlace();
            }
        });
    } else
        SubmitSMAreaPlace();
};

function SubmitSMAreaPlace() {
    var form_data = new FormData($('#frm-area-addedit')[0]);
    $.ajax({
        type: "post",
        url: "/SalesMarketing/SaveSMPlaceArea",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#app-marketing-area-add").modal('hide');
                var areaId = parseInt($('#areaId').val());
                if (areaId > 0) {
                    cleanBookNotification.success(_L("ERROR_MSG_234"), "Sales Marketing");
                } else
                    cleanBookNotification.success(_L("ERROR_MSG_235"), "Sales Marketing");
                getTotalPageArea();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}

function ShowOrHideArea(id) {
    $.ajax({
        url: "/SalesMarketing/ShowOrHideArea",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPageArea();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function LoadModalPlace(id) {
    $("#app-marketing-place-add").load("/SalesMarketing/LoadModalPlace", { id: id }, function () {
        $("#app-marketing-place-add").modal('show');
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });


        $('#frm-place-addedit').validate({
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    minlength: 4,
                    maxlength: 50
                },
                Summary: {
                    maxlength: 200
                }
            }
        });


    });
}

ProcessPlaceAdd = function () {
    if (!$('#frm-place-addedit').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-place-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-place-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {

                $("#sm-place-object-key").val(mediaS3Object.objectKey);
                $("#sm-place-object-name").val(mediaS3Object.fileName);
                $("#sm-place-object-size").val(mediaS3Object.fileSize);

                SubmitPlaceAdd();
            }
        });

    }
    else {
        $("#sm-place-object-key").val("");
        $("#sm-place-object-name").val("");
        $("#sm-place-object-size").val("");
        SubmitPlaceAdd();
    }
};

function SubmitPlaceAdd() {
    var form_data = new FormData($('#frm-place-addedit')[0]);
    $.ajax({
        type: "post",
        url: "/SalesMarketing/SavePlace",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#app-marketing-place-add").modal('hide');
                var placeId = parseInt($('#placeId').val());
                if (placeId > 0) {
                    cleanBookNotification.success(_L("ERROR_MSG_200"), "Sales Marketing");
                } else
                    cleanBookNotification.success(_L("ERROR_MSG_237"), "Sales Marketing");
                getTotalPagePlace();
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
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

function ShowOrHidePlace(id) {
    $.ajax({
        url: "/SalesMarketing/ShowOrHidePlace",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPagePlace();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function getTotalPageArea() {
    var name = $("#txtArea").val();
    var isLoadingHide = $("#isLoadingHideArea").is(':checked') ? true : false
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/CountListArea",
        data: { name: name, isLoadingHide: isLoadingHide },
        async: false,
        type: "POST",
        success: function (result) {
            if (result > 0) {
                initPagination(result, areaPageSize, '#areaPaginateTemplate');
            } else {
                $('#content-area').html('');
                $("#areaPaginateTemplate").css("display", "none");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
        }
    });
}

function LoadArea(skip, take) {
    var name = $("#txtArea").val();
    var isLoadingHide = $("#isLoadingHideArea").is(':checked') ? true : false
    $.LoadingOverlay("show");

    var html = $('#content-area').html();
    $.ajax({
        url: "/SalesMarketing/LoadArea",
        data: { name: name, skip: skip, take: take, isLoadingHide: isLoadingHide },
        cache: false,
        type: "POST",
        async: false,
        success: function (data) {
            $('#content-area').html(data);
            $("#areaPaginateTemplate").css("display", "block");
            LoadingOverlayEnd();
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function getTotalPagePipeline() {
    var name = $("#txtPipeline").val();
    var isLoadingHide = $("#isLoadingHidePipeline").is(':checked') ? true : false;
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/CountListPipeline",
        data: { name: name, isLoadingHide: isLoadingHide },
        async: false,
        type: "POST",
        success: function (result) {
            if (result > 0) {
                initPagination(result, pipelinePageSize, '#pipelinePaginateTemplate');
            } else {
                $('#content-pipeline').html('');
                $("#pipelinePaginateTemplate").css("display", "none");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
        }
    });
}

function LoadPipeline(skip, take) {
    var name = $("#txtPlace").val();
    var isLoadingHide = $("#isLoadingHidePipeline").is(':checked') ? true : false
    $.LoadingOverlay("show");
    var html = $('#content-pipeline').html();
    $.ajax({
        url: "/SalesMarketing/LoadPipeline",
        data: { name: name, skip: skip, take: take, isLoadingHide: isLoadingHide },
        cache: false,
        type: "POST",
        async: true,
        success: function (data) {
            $('#content-pipeline').html(data);
            $("#pipelinePaginateTemplate").css("display", "block");
            LoadingOverlayEnd();
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function ShowOrHidePipeline(id) {
    $.ajax({
        url: "/SalesMarketing/ShowOrHidePipeline",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPagePipeline();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function LoadModalPipeline(id) {
    $("#app-marketing-pipeline-addedit").load("/SalesMarketing/LoadModalPipeline", { id: id }, function () {
        $("#app-marketing-pipeline-addedit").modal('show');
    });
}

function ProcessSMPipeline() {
    if (!$('#frm-pipeline-addedit').valid())
        return;
    var steps = [];
    var idSteps = [];
    var flag = true;
    $("#steps .fieldtitle").each(function (index) {
        if ($(this).val() == "") {
            flag = false;
        }
        steps.push($(this).val());
        idSteps.push($(this).attr("data-id"));
    });
    if (steps.length == 0) {
        cleanBookNotification.error(_L("ERROR_MSG_509"), "Sales Marketing");
        return;
    } else if (!flag) {
        cleanBookNotification.error(_L("ERROR_MSG_600"), "Sales Marketing");
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-pipeline-upload-media").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("sm-pipeline-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }


            $("#sm-pipeline-object-key").val(mediaS3Object.objectKey);
            $("#sm-pipeline-object-name").val(mediaS3Object.fileName);
            $("#sm-pipeline-object-size").val(mediaS3Object.fileSize);

            SubmitSMPipeline();

        });

    } else
        SubmitSMPipeline();
}

function SubmitSMPipeline() {


    var form_data = new FormData($('#frm-pipeline-addedit')[0]);
    var steps = [];
    var idSteps = [];
    var flag = true;
    $("#steps .fieldtitle").each(function (index) {
        if ($(this).val() == "") {
            flag = false;
        }
        steps.push($(this).val());
        idSteps.push($(this).attr("data-id"));
    });
    form_data.append("steps", JSON.stringify(steps));
    form_data.append("idSteps", JSON.stringify(idSteps));
    $.ajax({
        type: "post",
        url: "/SalesMarketing/SavePipeline",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#app-marketing-pipeline-addedit").modal('hide');
                var id = parseInt($('#Id').val());
                if (id > 0) {
                    cleanBookNotification.success(_L("ERROR_MSG_609"), "Sales Marketing");
                    location.reload();
                } else {
                    cleanBookNotification.success(_L("ERROR_MSG_610"), "Sales Marketing");
                    getTotalPagePipeline();
                }
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}

function LoadModalPipelineContactDetail(pipelineContactId, contactId) {
    $.LoadingOverlay("show");
    $(".pipelineContactId").val(pipelineContactId);
    $("#app-marketing-pipeline-contact-manage").load("/SalesMarketing/LoadModalPipelineContactDetail", { id: pipelineContactId }, function () {
        $("#app-marketing-pipeline-contact-manage").modal('show');

        LoadCampaignsOfContact(contactId);
        LoadPipelineTasks();
        LoadPipelineEvents();
        $('#tblPipelineTasks').DataTable().ajax.reload();
        LoadingOverlayEnd();
    });
}

function LoadModalPipelineContact(id) {
    $.LoadingOverlay("show");
    $("#app-marketing-pipeline-prospect-addedit").load("/SalesMarketing/LoadModalPipelineContact", { id: id }, function () {
        $("#app-marketing-pipeline-prospect-addedit").modal('show');
        LoadingOverlayEnd();
    });
}

function LoadExistPipelineContacts(name, id) {
    $.LoadingOverlay("show");
    $("#pipelineContacts").load("/SalesMarketing/LoadExistPipelineContacts", { name: name, id: id }, function () {
        LoadingOverlayEnd();
    });
}

function LoadNewPipelineContacts(name) {
    $.LoadingOverlay("show");
    $("#newContacts").load("/SalesMarketing/LoadNewPipelineContacts", { name: name, pipelineId: $("#pipelineId").val() }, function () {
        LoadingOverlayEnd();
    });
}

function showExistPipelineContact(id) {
    $.LoadingOverlay("show");
    $(".contact").load("/SalesMarketing/ShowExistPipelineContact", { id: id }, function () {
        $(".select2").select2({
            placeholder: "Please select"
        });
        LoadingOverlayEnd();
    });
}

function showNewPipelineContact(id) {
    $.LoadingOverlay("show");
    $(".contact-2").load("/SalesMarketing/ShowNewPipelineContact", { id: id }, function () {
        $(".select2").select2({
            placeholder: "Please select"
        });
        $frm_pipelinecontact_add = $("#frm-pipelinecontact-add");
        //$frm_pipelinecontact_add.validate({
        //    ignore: "",
        //    rules: {
        //        Rating: {
        //            required: true
        //        },
        //        PotentialValue: {
        //            required: true
        //        }
        //    }
        //});

        $frm_pipelinecontact_add.submit(function (e) {
            e.preventDefault();
            if ($frm_pipelinecontact_add.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($frm_pipelinecontact_add[0]);
                frmData.append("pipelineId", $("#pipelineId").val());
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusyAddTaskForm = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            //$('.contact-2').hide(); $('.contact-list-2').show();
                            //LoadNewPipelineContacts("");
                            cleanBookNotification.success(_L("ERROR_MSG_611"), "Sales Marketing");
                            location.reload();
                        } else {
                            cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                            isBusyAddTaskForm = false;
                        }
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusyAddTaskForm = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            }
        });
        LoadingOverlayEnd();
    });
}

function savePipelineContact(id) {
    var frmData = new FormData();
    frmData.append("id", $("#pipelineContactId").val());
    frmData.append("pipelineId", 0);
    frmData.append("contactId", 0);
    frmData.append("rating", $("#rating").val());
    frmData.append("potentialValue", $("#potentialValue").val());
    $.ajax({
        type: 'POST',
        cache: false,
        url: "/SalesMarketing/SavePipelineContact",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
        },
        error: function (data) {
            isBusyAddTaskForm = false;
        }
    });
}

function removePipelineContact(id) {
    var pipelineContactId = [];
    pipelineContactId.push(id);
    removeListPipelineContact(pipelineContactId);
}

function removeListPipelineContact(pipelineContactId) {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/RemovePipelineContact",
        data: { pipelineContactId: JSON.stringify(pipelineContactId) },
        async: false,
        type: "POST",
        success: function (data) {
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_612"), "Sales Marketing");
                location.reload();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            LoadingOverlayEnd();
        }
    });
}

function moveOnePipelineContact(id) {
    selectedPipelineContact.push(id);
}

$("#btnRemovePipelineContact").on("click", function () {
    removeListPipelineContact(selectedPipelineContacts);
})

$("#btnMovePipelineContact").on("click", function () {
    changePipelineContactsToStep(selectedPipelineContact.length != 0 ? selectedPipelineContact : selectedPipelineContacts, $("#stepId").val(), true);
})

function LoadCampaignsOfContact(id) {
    $("#campaignPipeline").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#campaignPipeline').LoadingOverlay("show");
        } else {
            $('#campaignPipeline').LoadingOverlay("hide", true);
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
        "pageLength": 12,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketing/LoadCampaignsOfContact',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "contactId": id
                });
            }
        },
        "columns": [
            {
                data: null,
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    return pad(row.Id, 3);
                }
            },
            {
                data: "Name",
                orderable: false
            },
            {
                data: "Type",
                orderable: false
            },
            {
                data: null,
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    var str = '<button class="btn btn-info" onclick="window.location.href=\'/SalesMarketing/SMEmail?id=' + row.Id + '\'"><i class="fa fa-eye"></i> &nbsp; View</button>'
                    return str;
                }
            }
        ],
        "order": [[1, "asc"]]
    });
}

//function LoadModalPipelineTask(pipelineContactId) {
//    $.LoadingOverlay("show");
//    $("#create-task").load("/SalesMarketing/GenerateModalTask", { taskId: 0, pipelineContactId: pipelineContactId }, function () {
//        $("#create-task").modal('show');
//        $(".select2").select2({
//            placeholder: "Please select"
//        });
//        LoadingOverlayEnd();
//    });
//}

function SubmitTaskSaleMarketing() {
    $.LoadingOverlay("show");
    var form_data = new FormData($form_task_addedit[0]);
    $.ajax({
        type: "POST",
        url: "/SalesMarketing/SaveSMQbicleTask",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#create-task").modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_613"), "Sales Marketing");
                $('#tblPipelineTasks').DataTable().ajax.reload();
                ResetTask();
                //$('#tblScheduledVisits').DataTable().ajax.reload();
            } else {
                cleanBookNotification.error("Have an error!", "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error("Have an error, detail: " + data.error, "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });
}

function SaveTaskSaleMarketing() {
    lstDate = [];
    if ($('#taskName').val() == "" || $('#taskProgrammedStart').val() == "" || $('#taskDuration').val() == "0" || $('#taskDescription').val() == "") {
        cleanBookNotification.error(_L("ERROR_MSG_601"), "Task");
        return;
    }
    var valid = task_validtabs();
    if (!valid)
        return;
    if (!ValidateStepsWeight()) {
        cleanBookNotification.error(_L("ERROR_MSG_602"), "Task");
        $('#taskTabs a[href="#create-task-checklist"]').tab('show');
        return;
    }
    if (isBusyAddTaskForm) {
        return;
    }
    $("#hdTaskLastOccurrence").val($("input[name='recur-final']").val());
    var dayOrmonth = "", dayofweek = "";
    var type = $("#hdTaskRecurrenceType").val();

    if (type == "0") {
        dayOrmonth = "";
        dayofweek = "";
        $(".daily").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        dayOrmonth = "";
        dayofweek = "";
        $(".weekly").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    else {
        dayOrmonth = "";
        dayofweek = "";
        $(".monthTask").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    if ($("#create-task-recurrence").find('input[name=isRecurs]').is(":checked")) {
        $("#lstDate tbody tr").find("input[type='checkbox']").each(function () {
            if ($(this).is(":checked")) {
                lstDate.push($(this).attr("att-date"));
            }
        });
    }
    if ($("#create-task-recurrence").find('input[name=isRecurs]').is(":checked") && lstDate.length <= 0) {
        cleanBookNotification.error(_L("ERROR_MSG_603"), "Task");
        return;
    }
    $("#hdTaskDayOrMonth").val(dayOrmonth);
    $("#hdTaskDayOfWeek").val(dayofweek);
    $form_task_addedit.data("validator").settings.ignore = $('input[name=isSteps]').prop('checked') ? "" : ":hidden";
    if ($form_task_addedit.valid()) {
        ReDataStep();
        $.ajax({
            url: "/Tasks/DuplicateTaskNameCheck",
            data: { cubeId: $("#taskQbicleId").val(), taskKey: $('#taskKey').val(), taskName: $("#taskName").val() },
            type: "GET",
            dataType: "json",
            async: false
        }).done(function (refModel) {
            if (refModel.result)
                $form_task_addedit.validate().showErrors({ Name: _L("ERROR_MSG_604") });
            else {
                if ($('#taskAttachments').val()) {
                    var typeIsvalid = checkfile($('#taskAttachments').val());
                    if (typeIsvalid.stt) {
                        SubmitTaskSaleMarketing();
                    } else {
                        $form_task_addedit.validate().showErrors({ taskAttachments: typeIsvalid.err });
                    }
                } else {
                    SubmitTaskSaleMarketing();
                }
            }

        }).fail(function () {
            $("#form_task_addedit").validate().showErrors({ Name: _L("ERROR_MSG_606") });
        });
    } else
        task_validtabs();
};

function ProcessSMEventMedia() {
    $.LoadingOverlay("show");
    var files = document.getElementById("event-media-upload").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("event-media-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }


            $("#event-object-key").val(mediaS3Object.objectKey);
            $("#event-object-name").val(mediaS3Object.fileName);
            $("#event-object-size").val(mediaS3Object.fileSize);

            SubmitSMEvent();

        });

    } else
        SubmitSMEvent();
};



function SubmitSMEvent() {
    var form_data = new FormData(uiEls.$form_event_addedit[0]);
    $.ajax({
        type: "POST",
        url: "/SalesMarketing/SaveEvent",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#create-event").modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_614"), "Sales Marketing");
                $('#tblPipelineEvents').DataTable().ajax.reload();
                ResetEvent();
                //$('#tblScheduledVisits').DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}
function SaveQbicleEvent() {
    if (isBusyAddEventForm) {
        return;
    }
    lstDate = [];
    $("#hdLastOccurrence").val($("input[name='recur-final-event']").val());
    var dayOrmonth = "", dayofweek = "";
    var type = $("#hdEventRecurrenceType").val();
    if (type == "0") {
        dayOrmonth = "";
        $(".dailyEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });

    }
    else if (type == "1") {
        dayOrmonth = "";
        $(".weeklyEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }
    else {
        dayOrmonth = "";
        $(".monthEvent").each(function () {
            dayOrmonth += $(this).attr("att-checked");
            if ($(this).is(":checked")) {
                if (dayofweek == "")
                    dayofweek = $(this).val();
                else
                    dayofweek += ',' + $(this).val();
            }
        });
    }

    $("#hdDayOrMonth").val(dayOrmonth);
    $("#hdEventDayOfWeek").val(dayofweek);
    if ($("#create-event-recurrence").find('input[name=isRecurs]').is(":checked")) {
        $("#lstEventDate tbody tr").find("input[type='checkbox']").each(function () {
            if ($(this).is(":checked")) {
                lstDate.push($(this).attr("att-date"));
            }
        });
    }
    if ($("#create-event-recurrence").find('input[name=isRecurs]').is(":checked") && lstDate.length <= 0) {
        cleanBookNotification.error(_L("ERROR_MSG_607"), "Event");
        return;
    }
    if (uiEls.$form_event_addedit.valid()) {
        $.ajax({
            url: "/Events/DuplicateEventNameCheck",
            data: { cubeId: uiEls.$eventQbicleId.val(), eventKey: $eventKey, EventName: $('#eventName').val() },
            type: "GET",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                uiEls.$form_event_addedit.validate().showErrors({ Name: _L("ERROR_MSG_608") });
            else {
                if ($('#eventAttachments').val()) {
                    var typeIsvalid = checkfile($('#eventAttachments').val());
                    if (typeIsvalid.stt) {
                        ProcessSMEventMedia();
                    } else {
                        uiEls.$form_event_addedit.validate().showErrors({ eventAttachment: typeIsvalid.err });
                    }
                } else {
                    ProcessSMEventMedia();
                }
            }
        })
            .fail(function () {
                uiEls.$form_event_addedit.validate().showErrors({ Name: _L("ERROR_MSG_EXCEPTION_SYSTEM") });
            })
    } else {
        if ($("#create-event-overview .error").length) {
            $('#eventTabs a[href="#create-event-overview"]').tab('show');
            return false;
        }
    }
}

function LoadPipelineTasks() {
    $("#tblPipelineTasks").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#tblPipelineTasks").LoadingOverlay("show");
        } else {
            $("#tblPipelineTasks").LoadingOverlay("hide", true);
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
        "deferLoading": 30,
        "pageLength": 12,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketing/LoadPipelineTasks',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "pipelineContactId": $("#pipelineContactId").val()
                });
            }
        },
        "columns": [
            {
                data: null,
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    return pad(row.Id, 3);
                }
            },
            {
                data: "Title",
                orderable: false
            },
            {
                data: "Summary",
                orderable: false
            },
            {
                data: "Deadline",
                orderable: false
            },
            {
                data: "Status",
                orderable: false,
                render: function (value, type, row) {
                    if (row.Status === "Pending")
                        return '<span class="label label-info label-lg">Pending</span>';
                    else if (row.Status === "In progress")
                        return '<span class="label label-warning label-lg">In progress</span>';
                    else if (row.Status === "Overdue")
                        return '<span class="label label-danger label-lg">Overdue</span>';
                    else
                        return '<span class="label label-success label-lg">Complete</span>';
                }
            }
        ]
    });

}

function LoadPipelineEvents() {
    $("#tblPipelineEvents").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $("#tblPipelineEvents").LoadingOverlay("show");
        } else {
            $("#tblPipelineEvents").LoadingOverlay("hide", true);
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
        "deferLoading": 30,
        "pageLength": 12,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/SalesMarketing/LoadPipelineEvents',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "pipelineContactId": $("#pipelineContactId").val()
                });
            }
        },
        "columns": [
            {
                data: null,
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    return pad(row.Id, 3);
                }
            },
            {
                data: "Title",
                orderable: false
            },
            {
                data: "StartDate",
                orderable: false
            }
        ]
    });

}
function getTotalPagePlace() {
    var name = $("#txtPlace").val();
    var areaId = $("#slArea").val() ? $("#slArea").val() : 0;
    var isLoadingHide = $("#isLoadingHidePlace").is(':checked') ? true : false;
    $(".mdv2-col-dash").LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/CountListPlace",
        data: { name: name, areaId: areaId, isLoadingHide: isLoadingHide },
        async: false,
        type: "POST",
        success: function (result) {
            if (result > 0) {
                initPagination(result, placePageSize, '#placePaginateTemplate');
            } else {
                $('#content-place').html('');
                $("#placePaginateTemplate").css("display", "none");
            }
            $(".mdv2-col-dash").LoadingOverlay("hide");
        },
        error: function (error) {
            $(".mdv2-col-dash").LoadingOverlay("hide");
        }
    });
}
function LoadPlace(skip, take) {
    var name = $("#txtPlace").val();
    var areaId = $("#slArea").val() ? $("#slArea").val() : 0;
    var isLoadingHide = $("#isLoadingHidePlace").is(':checked') ? true : false
    $.LoadingOverlay("show");
    var html = $('#content-place').html();
    $.ajax({
        url: "/SalesMarketing/LoadPlace",
        data: { name: name, areaId: areaId, skip: skip, take: take, isLoadingHide: isLoadingHide },
        cache: false,
        type: "POST",
        async: true,
        success: function (data) {
            $('#content-place').html(data);
            $("#placePaginateTemplate").css("display", "block");
            LoadingOverlayEnd();
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}
function LoadModelAddSMContact() {
    $("#app-marketing-contact-add").load("/SalesMarketing/LoadModalAddSMContact", {}, function () {
        $("#Places").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });

        $("#frm-smcontact-add .multi").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400
        });

        $("#frm-smcontact-add .select2").select2({
            placeholder: "Please select"
        });

    });
}

function ProcessContact() {

    if ($('#frm-smcontact-add').valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("sm-contact-upload-media").files;
        if (files && files.length > 0) {

            UploadMediaS3ClientSide("sm-contact-upload-media").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd('hide');
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }

                $("#sm-contact-object-key").val(mediaS3Object.objectKey);
                $("#sm-contact-object-name").val(mediaS3Object.fileName);
                $("#sm-contact-object-size").val(mediaS3Object.fileSize);

                SubmitSaleMarketingContact();

            });

        }

    }


}

SubmitSaleMarketingContact = function () {
    var form_data = new FormData($('#frm-smcontact-add')[0]);
    $.ajax({
        type: "post",
        url: "/SalesMarketing/SaveSMContact",
        data: form_data,
        cache: false,
        enctype: 'multipart/form-data',
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $("#app-marketing-contact-add").modal('hide');
                $("#tblSMContact").DataTable().ajax.reload();
                cleanBookNotification.success(_L("ERROR_MSG_239"), "Sales Marketing");
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(data.msg, "Sales Marketing");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
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

function LoadSMContacts() {
    $("#tblSMContact").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#tblSMContact').LoadingOverlay("show");
        } else {
            $('#tblSMContact').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        //"processing": true,
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
        "pageLength": 10,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "ajax": {
            "url": '/SalesMarketing/LoadSMContact',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "search": $("#smContactSearch").val()
                });
            }
        },
        "columns": [
            {
                data: "AvatarUri",
                orderable: false,
                width: "80px",
                render: function (value, type, row) {
                    var str = '<a href="/SalesMarketing/ShowEditSMContract?contactId=' + row.Id + '"><div class="table-avatar mini" style="background-image: url(' + $("#api").val() + row.AvatarUri + "&size=T" + ');"></div></a>'
                    return str;
                }
            },
            {
                data: "Name",
                orderable: true
            },
            {
                data: "Email",
                orderable: true
            },
            {
                data: "Phone",
                orderable: true
            },
            {
                data: "SourceName",
                orderable: true
            },
            {
                data: "ReceiveEmail",
                orderable: true,
                render: function (value, type, row) {
                    if (row.ReceiveEmail == "Yes") {
                        var str = '<span class="label label-lg label-success">Yes</span>';
                    } else {
                        var str = '<span class="label label-lg label-danger">No</span>';
                    }
                    return str;
                }
            },
            {
                data: null,
                orderable: false,
                width: "250px",
                render: function (value, type, row) {
                    var str = '<button class="btn btn-primary" style="margin-right:3px" onclick="window.location.href = \'../SalesMarketing/ShowEditSMContract?contactId=' + row.Id + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>'
                        + '<button class="btn btn-danger" onclick="DeleteSMContact(' + row.Id + ')"><i class="fa fa-trash"></i> &nbsp; Remove</button>'
                    return str;
                }
            }
        ],
        "order": [[1, "asc"], [2, "asc"], [3, "asc"], [4, "asc"], [5, "asc"]]
    });
}

function DeleteSMContact(id) {
    var r = confirm("Do you want remove this record?");
    if (r == true) {
        $.ajax({
            url: "/SalesMarketing/DeleteSMContact",
            data: {
                id: id,
            },
            async: false,
            type: "POST",
            success: function (data) {
                LoadingOverlayEnd();
                if (data.result) {
                    $("#tblSMContact").DataTable().ajax.reload();
                    cleanBookNotification.success(_L("ERROR_MSG_240"), "Sales Marketing");
                } else if (data.msg) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            },
            error: function (error) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
        });
        LoadingOverlayEnd();
    }

}

function changeCustomCriteria() {
    var selectedOption = [];
    $(".criteria.select2").each(function () {
        if ($(this).val() != null) selectedOption.push($(this).val());
    });
    $("#Options").val(selectedOption);
}

function showImageFromInputFile(input, output) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(output).attr('src', e.target.result);
            $(output).css({ "display": "block" });
        };
        reader.readAsDataURL(input.files[0]);
    }
}

function changeBirthDay() {
    var _val = $("#DateOfBirth").val();
    if (_val) {
        var arr = _val.split("-");
        var birthDay = new Date(arr[2], arr[1] - 1, arr[0]);
        var ageDifMs = Date.now() - birthDay.getTime();
        var ageDate = new Date(ageDifMs); // miliseconds from epoch
        var age = Math.abs(ageDate.getUTCFullYear() - 1970);
        var flag = false;
        $("#AgeRanges option").each(function () {
            var ageArr = $(this).text().split("-");
            if (!flag && ageArr && age >= parseInt(ageArr[0]) && age <= parseInt(ageArr[1])) {
                $('#AgeRanges').val($(this).val()).trigger('change');
                flag = true;
            }
        });
    }
}
function saveTabActive(tab) {
    $.post("/SalesMarketing/SaveTabActive", { tab: tab });
}

function changePipelineContactsToStep(pipelineContactId, stepId, isNotify) {
    $.ajax({
        url: "/SalesMarketing/ChangePipelineContactToStep",
        data: {
            pipelineContactId: JSON.stringify(pipelineContactId),
            stepId: stepId
        },
        async: false,
        type: "POST",
        success: function (data) {
            if (isNotify) {
                if (data.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_615"), "Sales Marketing");
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                }
            }
            $("#pipeline_step").load("/SalesMarketing/LoadSteps", { pipelineId: $("#pipelineId").val() }, function () {
                $(".column").sortable({
                    connectWith: ".column",
                    handle: ".portlet-content",
                    revert: 0,
                    cancel: ".portlet-toggle",
                    placeholder: "portlet-placeholder ui-corner-all",
                    receive: function (event, ui) {
                        var pipelineContactId = [];
                        pipelineContactId.push($(ui.item[0]).attr("data-id"));
                        var stepId = $(this).attr("data-id");
                        changePipelineContactsToStep(pipelineContactId, stepId, false);
                    }
                });

                $(".portlet")
                    .addClass("ui-widget ui-widget-content ui-helper-clearfix ui-corner-all")
                    .find(".portlet-header")
                    .addClass("ui-widget-header ui-corner-all")
                    .prepend("<span class='ui-icon ui-icon-minusthick portlet-toggle'></span>");
            });
        },
        error: function (error) {
            if (isNotify) {
                cleanBookNotification.error("Have a error", "Sales Marketing");
            }
        }
    });
}

$("#btnClearSelected").on("click", function () {
    selectedPipelineContacts = [];
    $('.alert_matches.projects p').text(selectedPipelineContacts.length + " contacts selected");
    $('.pipeline-block .portlet.rework input[type="checkbox"]').prop('checked', false);
    $('.alert_matches.projects').removeClass('active');
});

if (jQuery().sortable) {
    $(function () {
        $(".column").sortable({
            connectWith: ".column",
            handle: ".portlet-content",
            revert: 0,
            cancel: ".portlet-toggle",
            placeholder: "portlet-placeholder ui-corner-all",
            receive: function (event, ui) {
                var pipelineContactId = [];
                pipelineContactId.push($(ui.item[0]).attr("data-id"));
                var stepId = $(this).attr("data-id");
                changePipelineContactsToStep(pipelineContactId, stepId, false);
            }
        });

        $(".portlet")
            .addClass("ui-widget ui-widget-content ui-helper-clearfix ui-corner-all")
            .find(".portlet-header")
            .addClass("ui-widget-header ui-corner-all")
            .prepend("<span class='ui-icon ui-icon-minusthick portlet-toggle'></span>");

        $(".portlet-toggle").on("click", function () {
            var icon = $(this);
            icon.toggleClass("ui-icon-minusthick ui-icon-plusthick");
            icon.closest(".portlet").find(".portlet-content").toggle();
        });
    });
}

$('.pipeline-block .portlet.rework input[type="checkbox"]').bind('click', function () {
    $(this).closest('.portlet').toggleClass('toggled');
    var id = $(this).val();
    if ($(this).prop("checked") == true) {
        $('.alert_matches.projects').addClass('active');
        selectedPipelineContacts.push(id);
    } else {
        $('.alert_matches.projects').removeClass('active');
        var index = selectedPipelineContacts.indexOf(id);
        if (index !== -1) selectedPipelineContacts.splice(index, 1);
    }
    $('.alert_matches.projects p').text(selectedPipelineContacts.length + " contacts selected");
});
function DBformatOptions(state) {

    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function DBformatSelected(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function LoadModalBrandById(id) {
    $("#app-marketing-brand-add").load("/SalesMarketingBrand/LoadModalBrandById", { id: id }, function () {
        $('#featuredImageUri').select2({
            placeholder: 'Please select'
        });
        $(".previewimgbrand").change(function () {
            var target = $(this).data('target');
            readImgURL(this, target);
            $(target).fadeIn();
        });

    })
}

function ProcessSaleMarketingBranch() {

    if (!$('#frm-marketing-brand').valid()) {
        return;
    }
    var folder = $('#frm-marketing-brand select[name=FeaturedImageUri]').val();
    if (!folder || (folder == "0" && !$('#brandFolderName').val())) {
        LoadingOverlayEnd();
        $frm_marketing_brand.validate().showErrors({ FolderName: "This field is required." });
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-brand-upload-image").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-brand-upload-image").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }


            $("#sm-brand-object-key").val(mediaS3Object.objectKey);
            $("#sm-brand-object-name").val(mediaS3Object.fileName);
            $("#sm-brand-object-size").val(mediaS3Object.fileSize);

            SubmitSaleMarketingBranch();

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
                isBusyAddTaskForm = false;
                cleanBookNotification.success(_L("ERROR_MSG_140"), "Sales Marketing");
                getTotalPageBrand();
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
                isBusyAddTaskForm = false;
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            
            isBusyAddTaskForm = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });

}


function ShowOrHideBrand(id) {
    $.ajax({
        url: "/SalesMarketingBrand/ShowOrHideBrand",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPageBrand();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function LoadModalSegment(id) {
    $("#app-marketing-segment-add").load("/SalesMarketingSegment/GenerateModalSegmentAddEdit", { segmentId: id }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#segmentType').select2({
            placeholder: 'Please select'
        });
        var $frmsegment = $('#frm-segment');
        $frmsegment.validate(
            {
                ignore: [],
                rules: {
                    Name: {
                        required: true,
                        maxlength: 150,
                        minlength: 3
                    },
                    Summary: {
                        maxlength: 250
                    }
                },
                invalidHandler: function () {
                    if ($('#segment-1 label.error:not([style*="display: none"])').length != 0) {
                        $('#tabSegment a[href="#segment-1"]').tab('show');
                    } else if ($('#segment-2 label.error:not([style*="display: none"])').length != 0) {
                        $('#tabSegment a[href="#segment-2"]').tab('show');
                    }
                }
            });

    })
}

ProcessSegmentAdd = function () {
    if (!$('#frm-segment').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-segment-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-segment-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }


            $("#sm-segment-object-key").val(mediaS3Object.objectKey);
            $("#sm-segment-object-name").val(mediaS3Object.fileName);
            $("#sm-segment-object-size").val(mediaS3Object.fileSize);

            SubmitSegmentAdd();

        });


    }
    else {
        $("#sm-segment-object-key").val("");
        $("#sm-segment-object-name").val("");
        $("#sm-segment-object-size").val("");
        SubmitSegmentAdd();
    }
};

SubmitSegmentAdd = function () {

    var frmData = new FormData($('#frm-segment')[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingSegment/SaveSegment",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                $('#app-marketing-segment-add').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_204"), "Sales Marketing");
                getTotalPageSegment();
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
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

function ShowOrHideSegment(id) {
    $.ajax({
        url: "/SalesMarketingSegment/ShowOrHideSegment",
        data: { id: id },
        type: "POST",
        success: function (data) {
            if (data.result) {
                getTotalPageSegment();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
            }
            LoadingOverlayEnd();
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    });
}

function RemoveQueue() {
    $.LoadingOverlay("show");
    var type = $('#btnRemoveQueue').attr('data-type');
    if (type === 'SocialCampaign') {
        var url = '/SalesMarketing/RemoveQueueSchedule';
    } else {
        var url = '/SalesMarketing/RemoveEmailQueueSchedule';
    }
    $.ajax({
        type: 'post',
        url: url,
        datatype: 'json',
        data: { campaignId: $('#btnRemoveQueue').attr('data-id') },
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                if (type === 'SocialCampaign') {
                    LoadSocicalCampains(true);
                } else {
                    LoadEmailCampaigns(true);
                }
                cleanBookNotification.success(_L("ERROR_MSG_404"), "Sales Marketing");
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_405"), "Sales Marketing");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_405"), "Sales Marketing");
            LoadingOverlayEnd();
        }
    });
}

function LoadSocialCampaignEditModal(id) {
    $.LoadingOverlay("show");
    $("#app-marketing-manual-social-campaign-edit").html("");
    $("#app-marketing-email-campaign-edit").html("");
    $("#app-marketing-social-campaign-edit").load("/SalesMarketing/SocialCampaignEdit?id=" + id, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('.select2').select2({
            placeholder: "Please select",
        });
        var $frm_campaign_edit = $('#frm_marketing-social-campaign_addedit');
        $frm_campaign_edit.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });
        if ($('#slBrandCampaign').val()) {
            $('#slBrandCampaign').trigger('change');
        }
        isValidWorkgroup();
        $frm_campaign_edit.submit(function (e) {
            e.preventDefault();
            var workgroupid = $('#social-campaign-workgroup').val();
            if (!workgroupid) {
                cleanBookNotification.error(_L("ERROR_MSG_168"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var folder = $('#social-campaign-folders').val();
            if (!folder || (folder === "0" && !$('#social-newfolder-name').val())) {
                cleanBookNotification.error(_L("ERROR_MSG_169"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var cpbrand = $('#slBrandCampaign').val();
            if (!cpbrand) {
                cleanBookNotification.error(_L("ERROR_MSG_170"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            if ($frm_campaign_edit.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($frm_campaign_edit[0]);
                var opwgSelect = $('#social-campaign-workgroup option:selected');
                var qbicleId = opwgSelect.attr("qbicleid");
                var topicid = opwgSelect.attr("topicid");
                frmData.append("qbicleFolderId", qbicleId);
                frmData.append("topicid", topicid);
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $('#app-marketing-social-campaign-edit').modal('hide');
                            LoadSocicalCampains(true);
                            cleanBookNotification.success(_L("ERROR_MSG_167"), "Sales Marketing");
                        } else if (data.msg) {
                            cleanBookNotification.error(data.msg, "Sales Marketing");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            } else {
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
        });
        LoadingOverlayEnd();
    });
}

function LoadManualSocialCampaignEditModal(id) {
    $.LoadingOverlay("show");
    $("#app-marketing-email-campaign-edit").html("");
    $("#app-marketing-social-campaign-edit").html("");
    $("#app-marketing-manual-social-campaign-edit").load("/SalesMarketing/ManualSocialCampaignEdit?id=" + id, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('.select2').select2({
            placeholder: "Please select",
        });

        isValidWorkgroup();
        if ($('#slBrandCampaign').val()) {
            $('#slBrandCampaign').trigger('change');
        }

        var $frm_campaign_edit = $('#frm_marketing-social-campaign_addedit');
        $frm_campaign_edit.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });
        
        $frm_campaign_edit.submit(function (e) {
            e.preventDefault();
            var workgroupid = $('#social-campaign-workgroup').val();
            if (!workgroupid) {
                cleanBookNotification.error(_L("ERROR_MSG_168"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var folder = $('#social-campaign-folders').val();
            if (!folder || (folder === "0" && !$('#social-newfolder-name').val())) {
                cleanBookNotification.error(_L("ERROR_MSG_169"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var cpbrand = $('#slBrandCampaign').val();
            if (!cpbrand) {
                cleanBookNotification.error(_L("ERROR_MSG_170"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            if ($frm_campaign_edit.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($frm_campaign_edit[0]);
                var opwgSelect = $('#social-campaign-workgroup option:selected');
                var qbicleId = opwgSelect.attr("qbicleid");
                var topicid = opwgSelect.attr("topicid");
                frmData.append("qbicleFolderId", qbicleId);
                frmData.append("topicid", topicid);
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $('#app-marketing-manual-social-campaign-edit').modal('hide');
                            LoadSocicalCampains(true);
                            cleanBookNotification.success(_L("ERROR_MSG_167"), "Sales Marketing");
                        } else if (data.msg) {
                            cleanBookNotification.error(data.msg, "Sales Marketing");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            } else {
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
        });
        LoadingOverlayEnd();
    });
}

function LoadEmailCampaignEditModal(id) {
    $.LoadingOverlay("show");
    $("#app-marketing-social-campaign-edit").html("");
    $("#app-marketing-manual-social-campaign-edit").html("");
    $("#app-marketing-manual-social-campaign-add").html("");
    $("#app-marketing-email-campaign-edit").load("/SalesMarketing/EmailCampaignEdit?id=" + id, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('.select2').select2({
            placeholder: "Please select",
        });
        isValidWorkgroup();
        if ($('#slBrandCampaign').val()) {
            $('#slBrandCampaign').trigger('change');
        }
        var $frm_email_campaign_edit = $('#frm_marketing-email-campaign_edit');
        $frm_email_campaign_edit.validate({
            ignore: "#social-newfolder-name",
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });
        $frm_email_campaign_edit.submit(function (e) {
            e.preventDefault();
            var workgroupid = $('#social-campaign-workgroup').val();
            if (!workgroupid) {
                cleanBookNotification.error(_L("ERROR_MSG_168"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var folder = $('#social-campaign-folders').val();
            if (!folder || (folder === "0" && !$('#social-newfolder-name').val())) {
                cleanBookNotification.error(_L("ERROR_MSG_169"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            var cpbrand = $('#slBrandCampaign').val();
            if (!cpbrand) {
                cleanBookNotification.error(_L("ERROR_MSG_170"), "Sales Marketing");
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
            if ($frm_email_campaign_edit.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($frm_email_campaign_edit[0]);
                var opwgSelect = $('#social-campaign-workgroup option:selected');
                var qbicleId = opwgSelect.attr("qbicleid");
                var topicid = opwgSelect.attr("topicid");
                frmData.append("qbicleFolderId", qbicleId);
                frmData.append("topicid", topicid);
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $('#app-marketing-email-campaign-edit').modal('hide');
                            cleanBookNotification.success(_L("ERROR_MSG_171"), "Sales Marketing");
                            LoadEmailCampaigns(true);
                        } else if (data.msg) {
                            cleanBookNotification.error(data.msg, "Sales Marketing");
                        }
                        isBusy = false;
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        LoadingOverlayEnd();
                        
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
                    }
                });
            } else {
                $('.admintabs a[href=#social-overview]').tab('show');
                return;
            }
        });
        
        LoadingOverlayEnd();
    });
}

$(".previewimg").change(function () {
    var target = $(this).data('target');
    readImgURL(this, target);
    $(target).fadeIn();
});


$(".previewimgresource").change(function () {
    var target = $(this).data('target');
    readImgURL(this, target);
    $(target).fadeIn();
});

function ReloadFolders() {
    var opwgSelect = $('#social-campaign-workgroup option:selected');
    var qbicleId = opwgSelect.attr("qbicleid");
    if (qbicleId) {
        $.getJSON("/SalesMarketing/LoadFoldersByQbicle", { qbicleId: qbicleId }, function (data) {
            if (data && data.length > 0) {
                data.push({ id: 0, text: "Create a new folder" });
                $('#social-campaign-folders').empty();
                $('#social-campaign-folders').append('<option value=""></option><option value="0">Create a new folder</option>');
                $('#social-campaign-folders').select2({
                    data: data,
                    placeholder: "Please select"
                });
                $('#social-campaign-folders').val($('#CurrentFolderId').val()).trigger("change");
            }
        });
    }
}

function changeListSegments() {
    $.LoadingOverlay("show");
    var lstSegmentsId = $('#campaignsegments').val()
    if (!lstSegmentsId) {
        var options = $('#campaignsegments option');
        var lstSegmentsId = $.map(options, function (option) {
            return option.value;
        });
    }
    $.ajax({
        type: 'post',
        url: '/SalesMarketing/CountContacts',
        datatype: 'json',
        data: { lstSegments: lstSegmentsId },
        success: function (data) {
            $("#includedSegments").text(data.Object.lstSegments);
            $("#totalRecipients").text(data.Object.totalContacts);
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

$(document).ready(function () {
    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: DBformatOptions,
        templateSelection: DBformatSelected,
        dropdownCssClass: 'withdrop'
    });
    LoadSMContacts();
    if (window.location.pathname.includes("SMApps")) {
        switch (tabConfig) {
            case "True":
                $('ul#tab-apps a[href="#app-config"]').click();
                if (facebookPage === "True") {
                    if (facebookType === "1") {
                        SocialFacebookPage();
                        $('#app-marketing-social-facebook-page-add').modal('show');
                        var pc = parseInt(facebookPageCount);
                        if (pc === 0) {
                            cleanBookNotification.error(_L("ERROR_MSG_401"), "Sales Marketing")
                        }
                    } else if (facebookType === "2") {
                        SocialFacebookGroup();
                        $('#app-marketing-social-facebook-group-add').modal('show');
                        var gc = parseInt(facebookGroupCount);
                        if (gc === 0) {
                            cleanBookNotification.error(_L("ERROR_MSG_402"), "Sales Marketing")
                        }
                    }
                }
                break;
            case "Contacts":
                $('ul#tab-apps a[href="#sm-contacts"]').click();
                $('#tblSMContact').DataTable().ajax.reload();
                break;
            case "Contacts.Segments":
                $('ul#tab-apps a[href="#sm-contacts"]').click();
                $('ul#tab-contacts a[href="#contacts-segments"]').click();
                break;
            case "Contacts.Locations":
                $('ul#tab-apps a[href="#sm-contacts"]').click();
                $('ul#tab-contacts a[href="#contacts-locations"]').click();
                break;
            case "Contacts.Criteria":
                $('ul#tab-apps a[href="#sm-contacts"]').click();
                $('ul#tab-contacts a[href="#contacts-criteria"]').click();
                break;
            case "Contacts.Ages":
                $('ul#tab-apps a[href="#sm-contacts"]').click();
                $('ul#tab-contacts a[href="#contacts-ages"]').click();
                break;
            case "Campaigns.Social":
                $('ul#tab-apps a[href="#app-ideas"]').click();
                $('ul#tab-campaigns a[href="#campaigns-social"]').click();
                break;
            case "Campaigns.Ideas":
                $('ul#tab-apps a[href="#app-ideas"]').click();
                $('ul#tab-campaigns a[href="#app-ideas"]').click();
                break;
            case "Campaigns.Emails":
                $('ul#tab-apps a[href="#app-ideas"]').click();
                $('ul#tab-campaigns a[href="#campaigns-email"]').click();
                break;
            case "Pipelines":
                $('ul#tab-apps a[href="#app-pipelines"]').click();
                break;
            case "Loyalty":
                $('ul#tab-apps a[href="#app-loyalty"]').click();
                break;
            case "Configs":
                $('ul#tab-apps a[href="#app-config"]').click();
                break;
            case "Configs.EmailTemplates":
                $('ul#tab-apps a[href="#app-config"]').click();
                setTimeout(function () { $('ul#tab-configs a[href="#smconfig-templates"]').click(); }, 1000);
                break;
            case "Configs.Settings":
                $('ul#tab-apps a[href="#app-config"]').click();
                setTimeout(function () { $('ul#tab-configs a[href="#smconfig-0"]').click(); }, 1000);
                break;
            case "Configs.Workgroups":
                $('ul#tab-apps a[href="#app-config"]').click();
                setTimeout(function () { $('ul#tab-configs a[href="#smconfig-1"]').click(); }, 1000);
                break;
            case "Configs.EmailVerification":
                $('ul#tab-apps a[href="#app-config"]').click();
                setTimeout(function () { $('ul#tab-configs a[href="#smconfig-emails"]').click(); }, 1000);
                break;
            default:
                $('ul#tab-apps a[href="#app-brands"]').click();
                break;
        }
    }

    $("#txtSMContact").keyup(SearchThrottle(function () {
        // do the search if criteria is met
        //LoadArea(true);
    }));
    $("#txtArea").keyup(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageArea();
    }));
    $("#txtPlace").keyup(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPagePlace();
    }));
    $('#social-cmp-search').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        LoadSocicalCampains(true);
    }));
    $('#social-cmp-target-type').change(function () {
        LoadSocicalCampains(true);
    });
    $('#social-cmp-campaign-type').change(function () {
        LoadSocicalCampains(true);
    });
    $('#email-campaign-search').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        LoadEmailCampaigns(true);
    }));
    $('#target-segment-search').change(function () {
        LoadEmailCampaigns(true);
    });
    $('#brandSearch').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageBrand();
    }));
    $('#isLoadingHideBrand').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageBrand();
    }));
    $('#isLoadingHideSegment').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageSegment();
    }));
    $('#isLoadingHidePlace').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPagePlace();
    }));
    $('#isLoadingHideArea').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageArea();
    }));
    $('#isLoadingHidePipeline').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPagePipeline();
    }));
    $('#isLoadingHideIdea').change(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageIdea();
    }));
    $('#txtIdeaKeyword').keyup(SearchThrottle(function () {
        // do the search if criteria is met
        getTotalPageIdea();
    }));
    $('#txtCriteriaSearch').keyup(SearchThrottle(function () {
        $('#tblCriteriaDef').DataTable().search($('#txtCriteriaSearch').val()).draw(false);
    }));
    $("#txtPipeline").keyup(SearchThrottle(function () {
        getTotalPagePipeline();
    }));
    SocialContactCriteriaAdd();
    //This prototype function allows you to remove even array from array
    Array.prototype.remove = function (x) {
        var i;
        for (i in this) {
            if (this[i].toString() == x.toString()) {
                this.splice(i, 1)
            }
        }
    }
});

function InitDatatableEmailTemplate() {
    $("#tblEmailTemplates").DataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        responsive: true,
        autoWidth: false,
        lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
        pageLength: 10,
        ajax: {
            "url": '/SalesMarketing/GetEmailTemplates'
        },
        columns: [
             { "title": "Name", "data": "TemplateName", "width": "20%", "searchable": true, "orderable": true },
             { "title": "Description", "data": "TemplateDescription", "width": "50%", "searchable": true, "orderable": true },
             { "title": "Created", "data": "CreateDate", "searchable": true, "orderable": true },
             { "title": "Options", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [
            {
                "targets": 2,
                "data": "CreateDate",
                "render": function (data, type, row, meta) {
                    return data + ' by <a href="/Community/UserProfilePage?uId=' + row.CreateById + '">' + row.CreateBy + '</a>';
                }
            },
            {
                "targets": 3,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<a href="/SalesMarketing/EmailBuilder?id='+data+'" class="btn btn-warning"><i class="fa fa-pencil"></i></a> ';
                    _htmlOptions += '<button type="button" onclick="DeleteEmailTemplate('+data+')" class="btn btn-danger"><i class="fa fa-trash"></i></button>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function DeleteEmailTemplate(id){
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Sales Marketing",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/SalesMarketing/DeleteEmailTemplateById", { id: id }, function (data) {
                    if (data.result) {
                        cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
                        $("#tblEmailTemplates").DataTable().ajax.reload();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg);
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"));
                    }
                });
                return;
            }
        }
    });
}

function VerifyEmailSES() {
    var email = $("#ses-email-identity").val();
    var _url = "/SalesMarketing/SendVerifyEmailSES";
    LoadingOverlay();
    $.ajax({
        'type': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'data': {
            'email': email
        },
        'success': function (response) {
            if (response.result) {
                $("#sm-verify-email").modal('hide');
                $("#ses-email-identity").val('');
                cleanBookNotification.success('Send verification email successfully!', 'Sales Marketing');
                $("#ses-identites-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, 'Sales Marketing');
            }
        },
        'error': function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        LoadingOverlayEnd();
    })
}

//Init SES Identities List Table
function initSESIdentitiesTable() {
    var dataTable = $("#ses-identites-table")
        .on('processing.dt', function (e, settings, processing) {
            $('#processingIndicator').css('display', 'none');
            if (processing) {
                $("#ses-identites-table").LoadingOverlay("show", { minSize: "70x60px" });
            } else {
                $("#ses-identites-table").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            "destroy": true,
            "serverSide": true,
            "paging": true,
            "searching": false,
            "responsive": true,
            "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
            "pageLength": 10,
            "ajax": {
                "url": '/SalesMarketing/LoadSESIdentities',
                "type": 'POST',
                "data": function (d) {
                    return $.extend({}, d, {
                        "key": '',
                        "status": 0
                    });
                }
            },
            "columns": [
                {
                    data: "Address",
                    orderable: true
                },
                {
                    data: "Added",
                    orderable: true
                },
                {
                    data: "StatusLabel",
                    orderable: true
                },
                {
                    data: "Options",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        var htmlStr = "";
                        htmlStr += '<button class="btn btn-danger" onclick="removeSESIdentity(\'' + row.Id + '\')"><i class="fa fa-trash"></i></button>';
                        return htmlStr;
                    }
                }
            ]
        });
}

function removeSESIdentity(identityId) {
    var _url = "/SalesMarketing/DeleteSESIdentity?identityId=" + identityId;
    $("#ses-identites-table").LoadingOverlay("show", { minSize: "70x60px" });
    $.ajax({
        'type': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.success('Delete identity successfully!', 'Sales Marketing');
                $("#ses-identites-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, 'Sales Marketing');
            }
        },
        'error': function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        $("#ses-identites-table").LoadingOverlay("hide");
    })
}

function UpdateIdentityStatus() {
    var _url = "/SalesMarketing/UpdateSESIdentityStatus";
    $("#ses-identites-table").LoadingOverlay("show", { minSize: "70x60px" });
    $.ajax({
        'type': 'POST',
        'dataType': 'JSON',
        'url': _url,
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.success('Update identities status successfully!', 'Sales Marketing');
                $("#ses-identites-table").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(response.msg, 'Sales Marketing');
            }
        },
        'error': function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        $("#ses-identites-table").LoadingOverlay("hide");
    })
}

function CheckEmailVerifyAvailable(){
    var email = $("#ses-email-identity").val();
    if (email != '') {
        $("#ses-identity-confirm-btn").removeAttr('disabled');
    } else {
        $("#ses-identity-confirm-btn").attr('disabled', 'disabled');
    }
}