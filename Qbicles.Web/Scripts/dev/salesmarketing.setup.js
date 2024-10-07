var $contactsCompleted = $("#contactsCompleted").val();
var $traderContactsCompleted = $("#traderContactsCompleted").val();
var $qbileCompleted = $("#qbileCompleted").val();
var $workgroupCompleted = $("#workgroupCompleted").val();

$(document).ready(function () {
    if (contact === 'True') {
        $('.contacts-data').show();
        LoadContacts();
    }
    if (traderContacts === 'True') {
        // $('.contacts-data').show();
        //  SyncTrader();
    }
    if (workgroup === 'True') {
        $('.contacts-data').show();
        $('.workgroup-data').show();
        $('#wg-proceed').attr('disabled', false);
        LoadTableWorkgroup();
    }
});

function showprocesscontacts(val) {
    $('#show-contacts .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-contacts .btn_process').removeClass("disabled");
        $contactsCompleted = "True";
    } else {
        $('#show-contacts .btn_process').addClass("disabled");
        UpdateSelectedStep("contacts");
        $contactsCompleted = "False";
    }
};

function showprocesstradercontacts(val) {
    $('#show-contacts .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-contacts .btn_process').removeClass("disabled");
        $traderContactsCompleted = "True";
    } else {
        $('#show-contacts .btn_process').addClass("disabled");
        UpdateSelectedStep("tradercontacts");
        $traderContactsCompleted = "False";
    }
};

function showprocessqbicle(val) {
    $('#show-qbicle .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-qbicle .btn_process').removeClass("disabled");
        $qbicleCompleted = "True";
    } else {
        $('#show-qbicle .btn_process').addClass("disabled");
        UpdateSelectedStep("qbicle");
        $qbicleCompleted = "False";
    }
};

function showprocessworkgroup(val) {
    $('#show-qbicle .btn_process').attr("disabled", !val);
    if (val) {
        $('#show-qbicle .btn_process').removeClass("disabled");
        $qbicleCompleted = "True";
    } else {
        $('#show-qbicle .btn_process').addClass("disabled");
        UpdateSelectedStep("qbicle");
        $qbicleCompleted = "False";
    }
};


function LoadContacts() {
    LoadingOverlay();
    $("#show-contacts .contacts-data").load("/SalesMarketing/LoadContact", function () {
        $('#age-ranges-table').DataTable({
            destroy: true,
            responsive: true,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });
        $('#sm-add-contact').remove();

        var idCus = $('#customCriteriaDefinitionId').val();
        if (idCus !== undefined) {
            if (idCus !== '-1') {
                $contactsCompleted = 'True';
                $('#show-contacts .btn_process').attr("disabled", false);
                $('#show-contacts .btn_process').removeClass("disabled");
            } else {
                $contactsCompleted = 'False';
                $('#show-contacts .btn_process').attr("disabled", true);
                $('#show-contacts .btn_process').addClass("disabled");
            }
        }
        LoadingOverlayEnd();
    });
}

function AddRowContactTable() {
    var idCus = $('#customCriteriaDefinitionId').val();
    $.ajax({
        url: "/SalesMarketing/SaveAgeRange",
        data: {
            id: -1,
            idCus: idCus !== undefined ? idCus : -1,
            start: 1,
            end: 10
        },
        async: false,
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $('.contacts-data').show();

                $('#show-contacts .btn_process').attr("disabled", false);
                $('#show-contacts .btn_process').removeClass("disabled");
                LoadContacts();
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

function SyncTrader() {
    $.LoadingOverlay("show");
    $("#sync-trader-table").load("/SalesMarketing/LoadSyncTraderSetup", function () {
        $('#trader-table').DataTable({
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
                        LoadTableWorkgroup();
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
    var swg = $('#swg').val();
    $("#content-workgroup-table").load("/SalesMarketing/LoadTableWorkgroup", function () {
        $('#workgroups-table').DataTable({
            destroy: true,
            responsive: true,
            order: [[0, 'asc']],
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });
        if (swg !== undefined) {
            if (swg !== -1) {
                $workgroupCompleted = 'True';
            } else {
                $workgroupCompleted = 'False';
            }
        }
        LoadingOverlayEnd();
    });
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

function updateSetting() {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/SalesMarketing/UpdateSetting",
        data: { id: $('#settingId').val(), qId: $('#setting_qbicle').val(), tId: $('#setting_topic').val() },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $('#in-qbicle').val('1');
                cleanBookNotification.success(_L("ERROR_MSG_208"), "Sales Marketing");
                $('#show-qbicle .bt-qbicle').attr("disabled", false);
                $('#show-qbicle .bt-qbicle').removeClass("disabled");

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

function ContactsProcess() {
    switch_sm_setup('#show-contacts', '#show-import');
    ShowStep("TraderContacts", "show_content", "/SalesMarketingSetup/ShowTraderContacts");
};

function TraderContactsProcess() {
    switch_sm_setup('#show-import', '#show-qbicle');
    ShowStep("Qbicle", "show_content", "/SalesMarketingSetup/ShowQbicle");

};

function QbicleProcess() {
    switch_sm_setup('#show-qbicle', '#show-workgroup');
    ShowStep("Workgroup", "show_content", "/SalesMarketingSetup/ShowWorkgroup");
};

function WorkgroupProcess() {
    switch_sm_setup('#show-workgroup', '#show-success');
    ShowStep("Complete", "show_content", "/SalesMarketingSetup/ShowComplete");
}


function ShowStep(stepName, stepId, stepUri) {
    $.LoadingOverlay("show");
    UpdateSMIsSettingComplete(stepName, 'setup');
    $('#' + stepId).empty();
    $('#' + stepId).load(stepUri, function () {
        if (contact === 'True') {
            $('.contacts-data').show();
            LoadContacts();
        }
        if (traderContacts === 'True') {
            //SyncTrader();
        }
        if (workgroup === 'True') {
            $('.contacts-data').show();
            $('.workgroup-data').show();
            $('#wg-proceed').attr('disabled', false);
            LoadTableWorkgroup();
        }
        $('#setting_qbicle,#setting_topic').select2({ placeholder: "Please select" });
        $.LoadingOverlay("hide");
    });
};


function UpdateSMSettingCompleted(goToTrader) {


    $.ajax({
        type: 'post',
        url: '/SalesMarketingSetup/UpdateSMIsSettingComplete',
        data: { isComplete: goToTrader },
        datatype: 'json',
        success: function (res) {
            if (res === true) {
                window.location.href = "/SalesMarketing/SMApps";
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};



function UpdateSMIsSettingComplete(isComplete, goToSM) {

    if (goToSM === "SMApps")
        isComplete = "SMApps";

    $.ajax({
        type: 'post',
        url: '/SalesMarketingSetup/UpdateSMIsSettingComplete',
        data: { isComplete: isComplete },
        datatype: 'json',
        success: function (res) {
            if (goToSM === 'SMApps') {
                if (res === true) {
                    window.location.href = "/SalesMarketing/SMApps";
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            } else {
                UpdateSelectedStep(goToSM);
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};


function UpdateSMSettingCompleted(goToSM) {
    $.ajax({
        type: 'post',
        url: '/SalesMarketingSetup/UpdateSMIsSettingComplete',
        data: { isComplete: goToSM },
        datatype: 'json',
        success: function (res) {
            if (res === true) {
                window.location.href = "/SalesMarketing/SMApps";
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function UpdateSelectedStep(stepName) {
    $(".smsetup_" + stepName).removeClass('complete').addClass('incomplete');
};

function switch_sm_setup(from, to) {
    $(from).hide();
    $(to).fadeIn();

    $('.wizard-steps li').each(function () {
        if ($('a', this).data('form') === from) {
            $(this).removeClass('active').addClass('complete');
        }
        if ($('a', this).data('form') === to) {
            $(this).addClass('active').removeClass('complete').addClass('incomplete');
        }
    });


};

function SelectContacts(name) {
    GetIconSetup();
    SelectedStep(name, "Contacts", "/SalesMarketingSetup/ShowContacts");
};
function SelectTraderContacts(name) {
    GetIconSetup();
    SelectedStep(name, "TraderContacts", '/SalesMarketingSetup/ShowTraderContacts');
};
function SelectQbicle(name) {
    GetIconSetup();
    SelectedStep(name, "Qbicle", '/SalesMarketingSetup/ShowQbicle');

};
function SelectWorkgroup(name) {
    GetIconSetup();
    SelectedStep(name, "Workgroup", '/SalesMarketingSetup/ShowWorkGroup');
};

function SelectCompleteSM(name) {
    GetIconSetup();
    SelectedStep(name, "Complete", '/SalesMarketingSetup/ShowComplete');
};

function GetIconSetup() {
    var contacts = $('#customCriteriaDefinitionId').val();
    var traderContacts = 1;
    var qbicle = $('#in-qbicle').val();
    var workgroup = $('#swg').val();
    if (contacts !== undefined) {
        if (contacts !== -1) {
            $contactsCompleted = 'True';
        } else {
            $contactsCompleted = 'False';
        }
    }
    if (qbicle !== undefined) {
        if (qbicle !== '-1') {
            $qbileCompleted = 'True';
        } else {
            $qbileCompleted = 'False';
        }
    }
    if (workgroup !== undefined) {
        if (workgroup !== -1) {
            $workgroupCompleted = 'True';
        } else {
            $workgroupCompleted = 'False';
        }
    }
}

function SelectedStep(name, step, traderUri) {
    $('ul.sm_setup_ul li').removeClass('active');
    $('.smsetup_' + name).addClass('active');
    UpdateSMIsSettingComplete(step, name);
    $.LoadingOverlay("show");
    if (name !== "Contacts") {
        if ($contactsCompleted === "True") {
            $(".smsetup_contacts").removeClass('incomplete').addClass('complete');
        } else {
            $(".smsetup_contacts").removeClass('complete').addClass('incomplete');
        }
    }
    if (name !== "TraderContacts")
        if ($traderContactsCompleted === "True") {
            $(".smsetup_tradercontacts").removeClass('incomplete').addClass('complete');
        } else {
            $(".smsetup_tradercontacts").removeClass('complete').addClass('incomplete');
        }
    if (name !== "Qbicle")
        if ($qbileCompleted === "True") {
            $(".smsetup_qbicle").removeClass('incomplete').addClass('complete');
        } else {
            $(".smsetup_qbicle").removeClass('complete').addClass('incomplete');
        }
    if (name !== "Workgroup")
        if ($workgroupCompleted === "True") {
            $(".smsetup_workgroup").removeClass('incomplete').addClass('complete');
        } else {
            $(".smsetup_workgroup").removeClass('complete').addClass('incomplete');
        }


    $('#show_content').empty();
    $('#show_content').load(traderUri, function () {

        if (contact === 'True') {
            $('.contacts-data').show();
            LoadContacts();
        }
        if (traderContacts === 'True') {
            // SyncTrader();
        }
        if (workgroup === 'True') {
            $('.contacts-data').show();
            $('.workgroup-data').show();
            $('#wg-proceed').attr('disabled', false);
            LoadTableWorkgroup();
        }
        $('#setting_qbicle,#setting_topic').select2({ placeholder: "Please select" });

    });


    LoadingOverlayEnd();
};

function removeMembers(id) {
    var members = $('#slMembers').val();

    if (members) {
        for (var i = 0; i < members.length; i++) {
            members.splice(i, 1);
        }
    }
    $('#slMembers').val(members);
    $("#apr" + id).prop("checked", false);
    $("#apr" + id).change();
}

function addMembers(id) {
    var members = $('#slMembers').val();
    if (!members) {
        members = [];
    }
    members.push(id);
    $('#slMembers').val(members);
}

function isValidWorkgroup() {
    var workgroupid = $('#social-campaign-workgroup').val();
    var _opselect = $('#social-campaign-workgroup option:selected');
    $('td.info-process').text(_opselect.attr('process'));
    $('td.info-members').text(_opselect.attr('members'));
    disableCampaignForm(workgroupid ? false : true);
}

function LoadingOverlay() {
    $.LoadingOverlay("show");
};

function LoadingOverlayEnd() {
    $.LoadingOverlay("hide");
};
