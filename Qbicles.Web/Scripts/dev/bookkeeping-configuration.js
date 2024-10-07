var BKReportConfiguration = [];
// get selected account
function selectAccount(ev, id) {
    //if ($(ev).hasClass('selectaccount')) {
    //    $(ev).removeClass('selectaccount');
    //    BKAccount = {};
    //} else {
    //    $('.selectaccount').removeClass('selectaccount');
    //    $(ev).addClass("selectaccount");
    //    BKAccount = { Id: id, Name: name };
    //}
    var name = $(".accountid-" + id).data("name");
    $('.selectaccount').removeClass('selectaccount');
    $(ev).addClass("selectaccount");
    BKAccount = { Id: id, Name: name };
    closeSelected();
    $("#app-bookkeeping-treeview").modal('toggle');
    //return BKAccount;
}
$(document).ready(function () {
    
    $('#approval_process_attachment_qbicles').change(function () {
        if ($('#approval_process_attachment_qbicles').val() > 0) {
            $.ajax({
                type: 'get',
                url: '/Bookkeeping/GetTopicsByQbicleId',
                data: { qbicleId: $('#approval_process_attachment_qbicles').val() },
                dataType: 'json',
                success: function (response) {
                    $("#journal_topic_attachment").empty().append('<option selected value="0">Select topic</option>');
                    $("#select2-journal_topic_attachment-container").text("Select topic");
                    $.each(response, function (i, v) {
                        $("#journal_topic_attachment").append($('<option></option>', { value: v.Id, text: v.Name }));
                    });
                },
                error: function (er) {
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        } else {
            $("#journal_topic_attachment").empty().append('<option selected value="0">Select topic</option>');
            $("#select2-journal_topic_attachment-container").text("Select topic");
        }

    });
    $('#defaultsavesettings').click(function () {

        var bkappsetting = {
            Id: $('#bkappsettingHiden').val(),
            AttachmentQbicle: { Id: $('#approval_process_attachment_qbicles').val() },
            AttachmentDefaultTopic: { Id: $('#journal_topic_attachment').val() }
        }

        if (bkappsetting.AttachmentQbicle.Id === "0") {
            cleanBookNotification.error("Default Qbicle to assign attachments is required!", "Qbicles");
            return;
        }
        if (bkappsetting.AttachmentDefaultTopic.Id === "0") {
            cleanBookNotification.error("Default topic to assign attachments is required!", "Qbicles");
            return;
        }

        $.LoadingOverlay("show");
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/DefaultSaveSetting',
            data: bkappsetting,
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1)
                        cleanBookNotification.createSuccess();
                    else if (response.actionVal === 2)
                        cleanBookNotification.updateSuccess();
                }
                else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    });
    initCurrency();
    addJournalGroup();
});

// clear error
function BKResetForm(formId) {
    ClearError();
    $("#" + formId)[0].reset();
    ($("#" + formId).validate()).resetForm();
};

// begin dimension funsions 
function clickAddDimension() {
    BKResetForm('form_dimension_add');
}
function EditDimension(dimensionId) {
    BKResetForm('form_dimension_edit');
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/GetEditDimensionById',
        data: { id: dimensionId },
        dataType: 'json',
        success: function (dimension) {
            $('#dimension-edit-id').val(dimension.Id);
            $('#dimension-edit-name').val(dimension.Name);
            $('#dimension-edit-createby').val(dimension.CreateBy);
            $('#dimension-edit-createdate').val(dimension.CreateDate);
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function addDimension() {
    if ($("#form_dimension_add").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveDimension',
            data: { Name: $('#addnew-dimension-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        setTimeout(function () {
                            $('#tab-dimensions').load('/Bookkeeping/DimensionPartial');
                        }, 500);
                        $('#app-dimension-add').modal('toggle');
                    }
                    else if (response.actionVal === 2) {

                        cleanBookNotification.updateSuccess();
                        setTimeout(function () {
                            $('#tab-dimensions').load('/Bookkeeping/DimensionPartial');
                        }, 500);
                        $('#app-dimension-edit').modal('toggle');
                    }
                    else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            BKResetForm();
        });
    }
}
function updateDimension() {
    if ($("#form_dimension_edit").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveDimension',
            data: { Id: $('#dimension-edit-id').val(), Name: $('#dimension-edit-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        $('#app-dimension-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        setTimeout(function () {
                            $('#tab-dimensions').load('/Bookkeeping/DimensionPartial');
                        }, 500);

                    } else if (response.actionVal === 2) {
                        $('#app-dimension-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        setTimeout(function () {
                            $('#tab-dimensions').load('/Bookkeeping/DimensionPartial');
                        }, 500);
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            BKResetForm();
        });
    }
}
function ConfirmDeleteDimension(id, name) {
    $('#label-confirm-dimension').text("Do you want delete dimension: " + name);
    $('#id-itemdimension-delete').val(id);
}
function deleteDimension() {
    $.ajax({
        type: 'delete',
        url: '/Bookkeeping/DeleteDimension',
        data: { id: $('#id-itemdimension-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                setTimeout(function () {
                    $('#tab-dimensions').load('/Bookkeeping/DimensionPartial');
                }, 500);
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            $('.modal-cancel').click();
        }
    });
}
// end dimension functions

// begin journal group functions 
function addGroup() {
    BKResetForm('form_group_add');
}
function editJournalGroup(journalGroupId) {
    BKResetForm('form_group_edit');
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/GetEditJournalGroupById',
        data: { id: journalGroupId },
        dataType: 'json',
        success: function (response) {
            if (!response) {
                return false;
            }
            var journalGroup = response;
            $('#subgroup-edit-id').val(journalGroup.Id);
            $('#subgroup-edit-name').val(journalGroup.Name);
            var createDate = eval(("new " + journalGroup.CreatedDate).replace(/\//g, ""));
            $('#subgroup-edit-createby').val(journalGroup.CreatedBy);
            $('#subgroup-edit-createdate').val(moment(createDate).format('YYYY-MM-DD'));
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function addJournalGroup() {
    $("#form_group_add").submit(function (event) {
        event.preventDefault();
        if ($("#form_group_add").valid()) {
            $.ajax({
                type: 'post',
                url: '/Bookkeeping/SaveJournalGroup',
                data: { Name: $('#journalgroup-name').val() },
                dataType: 'json',
                success: function (response) {
                    if (response.result === true) {
                        if (response.actionVal === 1) {
                            $('#app-coa-subgroup-add').modal('toggle');
                            cleanBookNotification.createSuccess();
                            setTimeout(function () {
                                $('#tab-groups').load('/Bookkeeping/JournalGroupPartial', function () {
                                    addJournalGroup();
                                });
                            }, 500);

                        } else if (response.actionVal === 2) {
                            $('#app-coa-subgroup-edit').modal('toggle');
                            cleanBookNotification.updateSuccess();
                            setTimeout(function () {
                                $('#tab-groups').load('/Bookkeeping/JournalGroupPartial', function () {
                                    addJournalGroup();
                                });
                            }, 500);
                        } else if (response.actionVal === 3) {
                            cleanBookNotification.error(response.msg, "Qbicles");
                        }
                    } else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                },
                error: function (er) {
                    
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
    
}
function updateJournalGroup() {
    if ($("#form_group_edit").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveJournalGroup',
            data: { Id: $('#subgroup-edit-id').val(), Name: $('#subgroup-edit-name').val() },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        $('#app-coa-subgroup-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        setTimeout(function () {
                            $('#tab-groups').load('/Bookkeeping/JournalGroupPartial');
                        }, 500);
                    } else if (response.actionVal === 2) {
                        $('#app-coa-subgroup-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        setTimeout(function () {
                            $('#tab-groups').load('/Bookkeeping/JournalGroupPartial');
                        }, 500);
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function confirmDeleteJournalGroup(id, name) {
    $('#label-confirm-journalgroup').text("Do you want delete journal group : " + name);
    $('#id-itemjournalgroup-delete').val(id);
}
function deleteJournalGroup() {
    $.ajax({
        type: 'delete',
        url: '/Bookkeeping/DeleteJournalGroup',
        data: { id: $('#id-itemjournalgroup-delete').val() },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                setTimeout(function () {
                    $('#tab-groups').load('/Bookkeeping/JournalGroupPartial');
                }, 500);
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }

        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
// end journal group functions


// begin template functions
function addClickTemplate() {
    BKResetForm('form_template_add');
}
function addTemplate() {
    if ($("#form_template_add").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveTemplate',
            data: {
                Name: $('#add-template-name').val(),
                Description: $('#add-template-description').val()
            },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {

                    if (response.actionVal === 1) {
                        $('#app-template-add').modal('toggle');
                        cleanBookNotification.createSuccess();
                        setTimeout(function () {
                            $('#tab-templates').load('/Bookkeeping/TemplatePartial');
                        }, 500);
                    } else if (response.actionVal === 2) {
                        $('#app-template-add').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        setTimeout(function () {
                            $('#tab-templates').load('/Bookkeeping/TemplatePartial');
                        }, 500);
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }

                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function editTemplate(templateId) {
    BKResetForm('form_template_edit');
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/GetEditTemplateById',
        data: {
            id: templateId
        },
        dataType: 'json',
        success: function (response) {
            $('#edit-template-id').val(response.Id);
            $('#edit-template-name').val(response.Name);
            $('#edit-template-description').val(response.Description);
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function updateTemplate() {
    if ($("#form_template_edit").valid()) {
        $.ajax({
            type: 'post',
            url: '/Bookkeeping/SaveTemplate',
            data: {
                Id: $('#edit-template-id').val(),
                Name: $('#edit-template-name').val(),
                Description: $('#edit-template-description').val()
            },
            dataType: 'json',
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        $('#app-template-edit').modal('toggle');
                        cleanBookNotification.createSuccess();
                        setTimeout(function () {
                            $('#tab-templates').load('/Bookkeeping/TemplatePartial');
                        }, 500);
                    } else if (response.actionVal === 2) {
                        $('#app-template-edit').modal('toggle');
                        cleanBookNotification.updateSuccess();
                        setTimeout(function () {
                            $('#tab-templates').load('/Bookkeeping/TemplatePartial');
                        }, 500);
                    } else if (response.actionVal === 3) {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function confirmDeleteTemplate(id, name) {
    $('#label-confirm-template').text("Do you want delete template : " + name);
    $('#id-template-delete').val(id);
}
function deleteTemplate() {
    $.ajax({
        type: 'delete',
        url: '/Bookkeeping/DeleteTemplate',
        data: {
            id: $('#id-template-delete').val()
        },
        dataType: 'json',
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                $('#tab-templates').load('/Bookkeeping/TemplatePartial');
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
// end template functions
function loadManageTemplate(elmId) {
    $('#modal-content-repconfig').LoadingOverlay("show");
    $('#modal-content-repconfig').load("/Bookkeeping/ConfigReportIncome", function () {
        $('select.select2').select2({ placeholder: 'Please select'});
        $('#modal-content-repconfig').LoadingOverlay("hide", true);
    });
}
function saveConfigTemplate()
{
    if (BKReportConfiguration.length == 0) {
        $('#report-usage-income').modal("hide");
        return;
    }
    $.LoadingOverlay("show");
    $.ajax({
        method: "POST",
        url: "/Bookkeeping/SaveConfigReportIncome",
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(BKReportConfiguration)
    }).done(function (data) {
        $.LoadingOverlay("hide", true);
        if (data.result) {
            $('#report-usage-income').modal("hide");
            BKReportConfiguration = [];
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"),"Bookkeeping");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Bookkeeping");
        }
    });
}
function addRevenue(elm, frmId, revId) {
    var $frmsubtotal = $(frmId);
    $frmsubtotal.validate(
        {
            ignore: [],
            rules: {
                title: {
                    required: true,
                    maxlength: 150,
                    minlength: 3
                }
            }
        });
    if ($frmsubtotal.valid()) {
        var _itemr = $.grep(BKReportConfiguration, function (value) { return value.revId == revId; });
        if (_itemr.length > 0) {
            _itemr.title = $(frmId + ' input[name="title"]').val();
            _itemr.expId = $(frmId + ' select[name="expId"]').val();
            _itemr.action = 'Add';
        } else {
            _itemr = {
                title: $(frmId + ' input[name="title"]').val(),
                expId: $(frmId + ' select[name="expId"]').val(),
                revId: revId,
                action: 'Add'
            };
            BKReportConfiguration.push(_itemr);
        }
        //Display result Ui
        $(elm).parent().parent().parent().parent().hide();
        $('#success_expense_' + revId).text(" - " + $(frmId + ' select[name="expId"] option:selected').text());
        $('#success_subtitle_' + revId).text(" " + _itemr.title);
        $('#calcfirstitem_' + revId).show();
        $('.exp_' + _itemr.expId).remove();
        $('select.select2').select2({ placeholder: 'Please select' });
        //End
    }
}
function removeSubtotal(expId, revId) {
    $('#modal-content-repconfig').LoadingOverlay("show");
    var _itemr = $.grep(BKReportConfiguration, function (value) { return value.revId == revId; });
    if (_itemr.length>0) {
        _itemr.expId = expId;
        _itemr.action = 'Remove';
    } else {
        _itemr = {
            title: '',
            expId: expId,
            revId: revId,
            action: 'Remove'
        };
        BKReportConfiguration.push(_itemr);
    }
    //Save Submit
    $.ajax({
        method: "POST",
        url: "/Bookkeeping/SaveConfigReportIncome",
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(BKReportConfiguration)
    }).done(function (data) {
        if (data.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Bookkeeping");
            loadManageTemplate();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Bookkeeping");
        }
    });
    //end
    
}
//Currency Settings
function initCurrency()
{
    $("#frmCurrencyConfiguration").submit(function (event) {
        event.preventDefault();
        var cSymbol = $('select[name=CurrencySymbol]').val();
        var sDisplay = $('select[name=SymbolDisplay]').val();
        var dPlace = $('select[name=DecimalPlace]').val();
        $.ajax({
            type: 'post',
            url: this.action,
            data: {
                CurrencySymbol: cSymbol,
                SymbolDisplay: sDisplay,
                DecimalPlace: dPlace,
            },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
                    var _event = "resetSettings('" + cSymbol + "','" + sDisplay + "','" + dPlace + "')";
                    $('#btnCurrencyReset').attr("onclick", _event);
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    });
}
function resetSettings(cSymbol, sDisplay, dPlace) {
    $('select[name=CurrencySymbol]').val(cSymbol).trigger('change');
    $('select[name=SymbolDisplay]').val(sDisplay).trigger('change');
    $('select[name=DecimalPlace]').val(dPlace).trigger('change');
}
//End Currency