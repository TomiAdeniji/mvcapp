var $contactId = $("#contact-id").val();
var reader = new FileReader();
var selectInvoice = {};
var selectSale = {};
var selectPurchase = {};
var mode = "Allocation";
var contactmanage = 'contactmanage';

function initContactMaster() {
    if ($(".accountInfo").text().length > 0) {
        $(".addbtnaccount").attr("style", "display:none;");
        $(".editbtnaccount").removeAttr("style");
    } else if ($(".accountInfo").text().length === 0) {
        $(".editbtnaccount").attr("style", "display:none;");
        $(".addbtnaccount").removeAttr("style");
    }
    if ($($('.domain_name')[0]).text().replace(/\s/g, "") === "") {
        selectWorkGroupContact();
    }
    if ($("#numberRef").text() === "" || $("#numberRef").text() === "0") {
        getNewContactRef();
    }

    $("#form_contact_add").validate({
        rules: {
            Name: {
                required: true,
                maxlength: 71
            }
        }
    });
}

function selectTab() {
    var activeTab = getLocalStorage(contactmanage);
    if (activeTab)
        $('a[href="#' + activeTab + '"]').tab('show');
    removeLocalStorage(contactmanage);
}
$(function () {
    selectTab();
    //$('#all_value').on("cut copy paste", function (e) {
    //    e.preventDefault();
    //});
    // Select your input element.
    var number = document.getElementById('all_value');

    // Listen for input event on numInput.
    number.onkeydown = function (e) {
        if (!((e.keyCode > 95 && e.keyCode < 106) || (e.keyCode > 47 && e.keyCode < 58) || e.keyCode === 8)) {
            return false;
        }
    };
});

// get selected account
var BKAccount = {};
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    BKAccount = { Id: id, Name: name };
    $("#accountId").val(id);
    closeSelected();
    SaveContact();

    $("#app-bookkeeping-treeview").modal("toggle");

};

function closeSelected() {
    if (BKAccount.Id) {
        $(".accountInfo").empty();
        $(".accountInfo").append(BKAccount.Name);
    } else {
        $(".accountInfo").empty();
    }

    if ($(".accountInfo").text().length > 0) {
        $(".addbtnaccount").attr("style", "display:none;");
        $(".editbtnaccount").removeAttr("style");
    } else if ($(".accountInfo").text().length === 0) {
        $(".editbtnaccount").attr("style", "display:none;");
        $(".addbtnaccount").removeAttr("style");
    }
    //SaveContact();
};

// showChangeAccount buton edit account selected
function showChangeAccount() {
    setTimeout(function () {
        CollapseAccount();
    },
        1);

};

function CollapseAccount() {
    $(".jstree").jstree("close_all");
};


function initSelectedAccount() {
    setTimeout(function () {
        $(".selectaccount").removeClass("selectaccount");
        $(".accountid-" + BKAccount.Id).addClass("selectaccount");
    },
        1);
};

// end account tree

//save contact
function changeContactManageImageLogo(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#trader-contact-avatar-upload");
        var imageResult = "";
        reader.onload = function (e) {
            imageResult = e.target.result;
            ChangeContactLogo(imageResult);
        }
        reader.readAsDataURL(image[0].files[0]);
        
    }
};

function ChangeContactLogo(image) {
    var files = document.getElementById("trader-contact-avatar-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("trader-contact-avatar-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            
            $.ajax({
                type: 'post',
                url: '/TraderContact/ChangeContactLogo?id=' + $("#contact-id").val() + '&mediaObjectKey=' + mediaS3Object.objectKey,
               
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                dataType: 'json',
                success: function () {
                    cleanBookNotification.updateSuccess();
                    $(".contact-avatar").attr("src", image);
                    $(".avatar").css("background-image", "url(" + image + ")");
                },
                error: function (er) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        });

    }
        
};

$("#group-workgroup-id").on("change",
    function () {
        SaveContact();
    });
$("#group-contact-id").on("change",
    function () {
        SaveContact();
    });
$("#CountryName").on("change",
    function () {
        SaveContact();
    });

function SaveContact() {
    $.LoadingOverlay("show");
    if ($("#update-right").val() === "True")
        UpdateContact();
    else
        CheckStatus($("#contact-id").val(), 'Contact').then(function (res) {

            if (res.result && res.Object !== "ContactApproved") {
                UpdateContact();
            } else if (res.result && res.Object === "ContactApproved") {
                cleanBookNotification.error("The Contact has been Approved. Cannot update", "Qbicles");
                setLocalStorage(contactmanage, $('ul.app_subnav li.active a').attr('href').replace('#', ''));
                setTimeout(function () { window.location.reload(); }, 1000);
            } else if (res.result === false) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });


};

UpdateContact = function () {
    if ($("#group-contact-id").val() === null) {
        cleanBookNotification.error(_L("ERROR_MSG_262"), "Qbicles");
        $("#contact-group-label").addClass("text-red");
    }
    if ($("#group-workgroup-id").val() === null) {
        cleanBookNotification.error(_L("ERROR_MSG_263"), "Qbicles");
    }

    var group = {
        Id: $("#group-contact-id").val()
    };
    var workgroup = {
        Id: $("#group-workgroup-id").val()
    };
    var contact = {
        ContactGroup: group,
        Workgroup: workgroup,
        Name: $("#contact-name").val(),
        Id: $("#contact-id").val(),
        Email: $("#contact-email").val()
    };
    if ($("#form_contact_add").valid()) {
        if ($("#group-contact-id").val() === null) {
            return;
        } if ($("#group-workgroup-id").val() === null) {
            return;
        }
        $.ajax({
            url: "/TraderContact/TraderContactNameCheck",
            data: { contact: contact },
            type: "POST",
            dataType: "json"
        }).done(function (refModel) {
            if (refModel.result)
                $("#form_contact_add").validate()
                    .showErrors({ Name: _L("ERROR_MSG_251") });
            else {
                $.ajax({
                    method: "POST",
                    dataType: "JSON",
                    url: "/TraderContact/CheckExistedApprovedContact",
                    data: {
                        contact: contact
                    }
                }).done(function (checkResultModel) {
                    if (checkResultModel.result) {
                        $("#form_contact_add").trigger("submit");
                    } else {
                        cleanBookNotification.error(checkResultModel.msg, "Qbicles");
                    }
                }).fail(function (er) {
                    cleanBookNotification.error("Error checking existing Approved Trader Contact", "Qbicles");
                }).always(function () {
                    LoadingOverlayEnd();
                });
            }
        }).fail(function () {
            $("#form_contact_add").validate()
                .showErrors({ Name: _L("ERROR_MSG_251") });
        }).always(function () {
            LoadingOverlayEnd();
        });
    } else {
        LoadingOverlayEnd();
    }
};


$("#form_contact_add").submit(function (e) {
    e.preventDefault();
    document.getElementById("form_contact_add").style.cursor = "not-allowed";
    var address = $("#AddressLine1").val() + ' ' + $("#AddressLine2").val() + ' ' + $("#City").val() + ' ' + $("#CountryName").val();
    fetch("https://maps.googleapis.com/maps/api/geocode/json?address=" + address + '&key=' + $("#map-key").val())
        .then(response => response.json())
        .then(data => {
            $("#contact-Latitude").val(data.results[0].geometry.location.lat);
            $("#contact-longitude").val(data.results[0].geometry.location.lng);
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                enctype: "multipart/form-data",
                data: new FormData(this),
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    if (data.result) {
                        ClearError();
                        cleanBookNotification.updateSuccess();
                        document.getElementById("form_contact_add").style.cursor = "default";
                        setLocalStorage(contactmanage, $('ul.app_subnav li.active a').attr('href').replace('#', ''));
                        //setTimeout(function () { window.location.reload(); }, 1000);
                    }
                },
                error: function (data) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    
                    document.getElementById("form_contact_add").style.cursor = "default";
                }
            });
        });

   


});



var strValueOld = "";
var strValueChanged = "";
function OnFocusOutControl(value) {

    strValueChanged = value;
    if (strValueChanged !== strValueOld)
        SaveContact();
};

function OnFocusControl(value) {
    strValueOld = value;
};
$(function () {
    //Tab Invoice
    $('#search_invoice').keyup(searchThrottle(function () {
        $('#invoice-list').DataTable().search($(this).val()).draw();
    }));

    $('.manage-columns-invoice input[type="checkbox"]').on('change', function () {
        var table = $('#invoice-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //Tab Bill
    $('#search_bill').keyup(searchThrottle(function () {
        $('#bill-list').DataTable().search($(this).val()).draw();
    }));
    $('.manage-columns-bill input[type="checkbox"]').on('change', function () {
        var table = $('#bill-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //Tab Payment
    $('#search_payment').keyup(searchThrottle(function () {
        $('#payment-list').DataTable().search($(this).val()).draw();
    }));
    $('.manage-columns-payment input[type="checkbox"]').on('change', function () {
        var table = $('#payment-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //tab Allocation
    $('#search_dt_table_allocation').keyup(searchThrottle(function () {
        $('#table_allocation').DataTable().search($(this).val()).draw();
    }));
    $('.manage-columns-allocation input[type="checkbox"]').on('change', function () {
        var table = $('#table_allocation').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //tab Credit
    $('#search_dt_creditnote').keyup(searchThrottle(function () {
        $('#credit-list').DataTable().search($(this).val()).draw();
    }));
    $('.manage-columns-credit input[type="checkbox"]').on('change', function () {
        var table = $('#credit-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //tab Debit
    $('#search_dt_debitnote').keyup(searchThrottle(function () {
        $('#debit-list').DataTable().search($(this).val()).draw();
    }));
    $('.manage-columns-debit input[type="checkbox"]').on('change', function () {
        var table = $('#debit-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });
    //End search
    //Load data Table
    tablePaymentsLoad();
    tableInvoicesLoad();
    tableBillsLoad();
    tableAllocationLoad();
    tableCreditLoad();
    tableDebitLoad();
});

function getNewAllocation() {
    $('#all_confirmAdd').removeClass('disabled');
    mode = "Allocation";
    $.LoadingOverlay("show");
    selectInvoice = {};
    $.ajax({
        url: "/TraderContact/GetNewAllocation?contactid=" + $contactId,
        type: "GET",
        datatype: "json"
    }).success(function (res) {
        $.LoadingOverlay("hide");
        // balance
        $('#modal_balance').text(res.ContactBalanceBefore);
        // reference
        $('#all_reference_id').val(res.Reference.Id);
        $('#all_prefic').text(res.Reference.Prefix);
        $('#all_delimeter_first, #all_delimeter_last').text(res.Reference.Delimeter);
        $('#all_numberic').text(res.Reference.NumericPart);
        $('#all_suffic').text(res.Reference.Suffix);
        // Find Invoice, value
        $('#all_find_invoice').val("");
        $('#all_value').val("0");
        // note
        $('#all_note').val(res.Description);
    }).error(function (err) {

        $.LoadingOverlay("hide");
        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        $('#create-contact-allocation').modal("hide");
    });
}

function onchangeAllocationValue(ev) {
    var errMsg = _L("ERROR_MSG_267");
    var allCurrentBalenceContact = parseFloat($('#modal_balance').text());
    var valueAll = parseFloat($(ev).val());
    var invoicebalance = $('#all_invoicebalance').val();
    if (valueAll > allCurrentBalenceContact) {
        cleanBookNotification.error(errMsg, "Qbicles");

        $(ev).val($('#modal_balance').text());
    } else if (selectInvoice.Id && selectInvoice.Id > 0 && valueAll > invoicebalance) {
        errMsg = _L("ERROR_MSG_268");
        cleanBookNotification.error(errMsg, "Qbicles");

        $('#all_confirmAdd').addClass('disabled');
    } else {
        $('#all_confirmAdd').removeClass('disabled');
    }
}

function findInvoiceContact() {
    //$.LoadingOverlay("show");
    //$("#contact-invoice-finder").empty();
    //$('#all_community-list').DataTable().destroy();
    //var key = '';
    //if (mode === "CreditNote" || mode === "DebitNote") {
    //    key = $('#bit_invoice_key').val();
    //} else {
    //    key = $('#all_find_invoice').val();
    //}
    //var url = '/TraderContact/FindInvoiceContact?contactid=' + $contactId + '&reffull=' + key + '&type=' + mode;
    //$('#contact-invoice-finder').load(url, function () {
    //    searchdtInvoice();
    //    setFilterByDateTime('search_daterange_invoice', 'all_community-list', 1);
    //    $.LoadingOverlay("hide");
    //});



    $("#contact-invoice-finder").empty();
    var url = '/TraderContact/FindInvoiceContact?type=' + mode;
    $('#contact-invoice-finder').load(url, function () {
        
    });
}

//function searchdtInvoice(ev, key) {
//    var listKey = [];
//    if (ev)
//        listKey.push($(ev).val());
//    else if (key)
//        listKey.push(key);
//    else if ($('#search_dt_find_invoice').val()) {
//        listKey.push($('#search_dt_find_invoice').val());
//    }
//    $("#all_community-list").DataTable().search(listKey.join("|"), true, false, true).draw();
//}
//function searchdtSale(ev, key) {
//    var listKey = [];
//    if (ev)
//        listKey.push($(ev).val());
//    else if (key)
//        listKey.push(key);
//    else if ($('#search_dt_sale').val()) {
//        listKey.push($('#search_dt_sale').val());
//    }
//    $("#community_list_find_sale").DataTable().search(listKey.join("|"), true, false, true).draw();
//}

//function searchdtPurchase(ev, key) {
//    var listKey = [];
//    if (ev)
//        listKey.push($(ev).val());
//    else if (key)
//        listKey.push(key);
//    else if ($('#search_dt_purchase').val()) {
//        listKey.push($('#search_dt_purchase').val());
//    }
//    $("#community_list_find_purchase").DataTable().search(listKey.join("|"), true, false, true).draw();
//}

function selectInvoiceContact(id) {

    $('.invoice-select').hide(); $('.selected-invoice').show();
    selectInvoice = {
        Id: id,
        Ref: $('.findInvoice_ref' + id).text(),
        Createdate: $('.findInvoice_createdate' + id).text(),
        Amount: $('.findInvoice_totalamount' + id).text(),
        Amountpaid: $('.findInvoice_amountpaid' + id).text(),
        BalanceInvoice: $('#balanceInvoice_' + id).val()
    };
    $("#balance-invoice-value").text(selectInvoice.BalanceInvoice);
    $("#balance-invoice-lable").show();

    
    //$("#all_value").val(selectInvoice.BalanceInvoice);
    //selectInvoice.Ref = selectInvoice.Ref.substr(1);
    $('#balanceInvoiceSelected').val(selectInvoice.BalanceInvoice);
    $('#invoiceselected .ref').text(selectInvoice.Ref);
    $('#invoiceselected .createdate').text(selectInvoice.Createdate);
    $('#invoiceselected .totalamount').text(selectInvoice.Amount);
    $('#invoiceselected .amountpaid').text(selectInvoice.Amountpaid);
    $('.invoice-select').hide(); $('.selected-invoice').show();
}
function confirmSelectInvoice() {
    if (mode === 'CreditNote' || mode === 'DebitNote') {
        $('#bit_invoice_key').val(selectInvoice.Ref);
    } else {

        $('#all_invoice_id').val(selectInvoice.Id);
        $('#all_find_invoice').val(selectInvoice.Ref);
        $('#all_invoicebalance').val(selectInvoice.BalanceInvoice);
        if ($('#all_value').val() > parseFloat(selectInvoice.BalanceInvoice)) {
            cleanBookNotification.error(_L("ERROR_MSG_268"), "Qbicles");
            $('#all_confirmAdd').addClass('disabled');
        } else {
            $('#all_confirmAdd').removeClass('disabled');
        }
    }
}

function saveAllocation() { 
    var Reference = {
        Id: $('#all_reference_id').val(),
        NumericPart: parseFloat($('#all_numberic').text()),
        Prefix: $('#all_prefic').text(),
        Suffix: $('#all_suffic').text(),
        Delimeter: $('#all_delimeter_first').text()
    };
    var allocation = {
        id: $('#allocation_id').val(),
        Contact: { Id: $contactId },
        Invoice: { Id: $('#all_invoice_id').val() },
        Value: $('#all_value').val(),
        ContactBalanceBefore: $('#modal_balance').val(),
        Description: $('#all_note').val(),
        Reference: Reference
    };
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderContact/SaveAllocation',
        data: { allocation: allocation },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1 || response.actionVal === 2) {
                cleanBookNotification.createSuccess();
                $("#table_allocation").DataTable().ajax.reload();
            } if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }

        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function deleteAllocation(id) {
    var result = confirm("Do you want delete an item ?");
    if (result === false) {
        return;
    }
    var url = "/TraderContact/DeleteAllocation?id=" + id;
    $.LoadingOverlay("show");
    $.ajax({
        url: url,
        type: "delete",
        dataType: "json",
        success: function (rs) {
            if (rs.result) {
                $("#table_allocation").DataTable().ajax.reload();
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                LoadingOverlayEnd();
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            LoadingOverlayEnd();
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
function LoadFormAddCreditDebit(id, type) {
    if (type === "CreditNote") {
        mode = "CreditNote";
    } else {
        mode = "DebitNote";
    }
    var url = '/TraderContact/AddEditCreditDebitNote?id=' + id + '&type=' + type + '&contactId=' + $contactId;
    $('#create-contact-credit').load(url, function () {
    });
}
function WorkGroupSelectedChange() {
    $('.preview-workgroup').show();
    var id = $("#workgroup-select-credit").val();
    if (id !== "") {
        $.LoadingOverlay("show");
        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + id,
            dataType: "json",
            success: function (response) {
                if (response.result) {
                    $(".preview-workgroup table tr td.location_name").text(response.Object.Location);
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_item span").text(response.Object.GroupNames);
                    $(".preview-workgroup table tr td.workgroup_member span").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.location_name").text('');
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_item span").text('');
                    $(".preview-workgroup table tr td.workgroup_member span").text('');
                }
                LoadingOverlayEnd();
            },
            error: function (er) {
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $(".preview-workgroup table tr td.location_name").text('');
        $(".preview-workgroup table tr td.workgroup_process").text('');
        $(".preview-workgroup table tr td.workgroup_qbicle").text('');
        $(".preview-workgroup table tr td.workgroup_member span").text('');
    }
};
function ShowGroupMember() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $("#workgroup-select-credit").val());
};
//credit
function creditFindInvoiceBitContact() {
    findInvoiceContact();
}
function creditFindSaleBitContact() {
    //var saleKey = $('#bit_sale_value').val();
    //var url = '/TraderContact/FindSaleCredit?contactid=' + $contactId + '&key=' + saleKey;
    $('#contact-sale-finder').empty();
    $('#contact-sale-finder').load('/TraderContact/FindSaleCredit', function () {
        //searchdtSale();
        //setFilterByDateTime('search_sale_datetime', 'community_list_find_sale', 1);
    });
}
function selectChoiseSale(saleId, refId) {
    $('.invoice-select').hide(); $('.selected-invoice').show();

    // set item value
    $('#sale_choise_id').val(saleId);
    $('#sale_choise_ref input').val(refId);
    $('#sale_choise_ref span').text($('.find_sale_ref_' + saleId).text());
    $('#sale_choise_cretedate').empty();
    $('#sale_choise_cretedate').append($('.find_sale_createdate_' + saleId).clone());
    $('#sale_dimensions').empty();
    $('#sale_dimensions').append($('.find_sale_dimensions_' + saleId).clone());
    $('#sale_choise_amount').text($('.find_sale_amount_' + saleId).text());
}
function reSelectSale() {
    $('.selected-invoice').hide(); $('.invoice-select').show();
    // reset value
    $('#sale_choise_id').val(0);
    $('#sale_choise_ref input').val(0);
    $('#sale_choise_ref span').text("");
    $('#sale_choise_cretedate').empty();
    $('#sale_dimensions').empty();
    $('#sale_choise_amount').text();
}
function confirmSelectSale() {
    selectSale = {
        Id: $('#sale_choise_id').val(),
        RefId: $('#sale_choise_ref input').val(),
        RefName: $('#sale_choise_ref span').text().trim()
    }
    $('#bit_sale_value').val(selectSale.RefName);
}
function editCredit(id) {
    LoadFormAddCreditDebit(id, "CreditNote");
}
// debit
function debitFindIBillBitContact() {
    findInvoiceContact();
}
function debitFindSPurchaseBitContact() {
    //var purchaseKey = $('#bit_purchase_value').val();
    //var url = '/TraderContact/FindPurchaseCredit?contactid=' + $contactId + '&key=' + purchaseKey;
    //$('#contact-sale-finder').load(url, function () {
    //    searchdtPurchase();
    //    setFilterByDateTime('search_purchase_datetime', 'community_list_find_purchase', 1);
    //});

    $('#contact-sale-finder').empty();
    $('#contact-sale-finder').load('/TraderContact/FindPurchaseCredit', function () {
        //searchdtSale();
        //setFilterByDateTime('search_sale_datetime', 'community_list_find_sale', 1);
    });
}
function selectChoisePurchse(purchaseId, refId) {
    $('.invoice-select').hide(); $('.selected-invoice').show();

    // set item value
    $('#sale_choise_id').val(purchaseId);
    $('#sale_choise_ref input').val(refId);
    $('#sale_choise_ref span').text($('.find_sale_ref_' + purchaseId).text());
    $('#sale_choise_cretedate').empty();
    $('#sale_choise_cretedate').append($('.find_sale_createdate_' + purchaseId).clone());
    $('#sale_dimensions').empty();
    $('#sale_dimensions').append($('.find_sale_dimensions_' + purchaseId).clone());
    $('#sale_choise_amount').text($('.find_sale_amount_' + purchaseId).text());
}
function reSelectPurchase() {
    $('.selected-invoice').hide(); $('.invoice-select').show();
    // reset value
    $('#sale_choise_id').val(0);
    $('#sale_choise_ref input').val(0);
    $('#sale_choise_ref span').text("");
    $('#sale_choise_cretedate').empty();
    $('#sale_dimensions').empty();
    $('#sale_choise_amount').text();
}
function confirmSelectPurchase() {
    selectPurchase = {
        Id: $('#sale_choise_id').val(),
        RefId: $('#sale_choise_ref input').val(),
        RefName: $('#sale_choise_ref span').text().trim()
    }
    $('#bit_purchase_value').val(selectPurchase.RefName);
}
function editDebit(id) {
    LoadFormAddCreditDebit(id, "DebitNote");
}
// save credit debit
function validateCreditDebit() {
    var valid = true;
    // check workgroup
    var $workgroup = {
        Id: $("#workgroup-select-credit").val()
    };
    if ($workgroup.Id === "" || $workgroup.Id === null || $workgroup.Id === "0") {
        $("#form_creditdebit").validate().showErrors({ workgroup: "Workgroup is required." });
        valid = false;
    }
    return valid;
}
function saveDraft() {
    saveCreditDebit("Draft");
}
function savePreview() {
    saveCreditDebit("PendingReview");
}
function saveCreditDebit(status) {
    if (!validateCreditDebit()) {
        return false;
    }
    var reference = {
        Id: $('#bit_reference_id').val(),
        NumericPart: parseFloat($('#bit_numberic').text()),
        Prefix: $('#bit_prefic').text(),
        Suffix: $('#bit_suffic').text(),
        Delimeter: $('#bit_delimeter_first').text()
    }
    var credit = {
        Id: $('#credit_id').val(),
        Contact: { Id: $contactId },
        Notes: $('#bit_note').val(),
        Value: $('#bit_amount').val(),
        Invoice: {
            Id: selectInvoice.Id
        },
        Sale: {
            Id: selectSale.Id
        },
        Purchase: {
            Id: selectPurchase.Id
        },
        Reason: $('#bit_reason').val(),
        Reference: reference,
        WorkGroup: {
            Id: $('#workgroup-select-credit').val()
        },
        Status: status
    }
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderContact/SaveCreditDebitNote',
        data: { creditNote: credit },
        dataType: 'json',
        success: function (response) {
            $.LoadingOverlay("hide");
            if (response.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#create-contact-credit').modal('toggle');
                if (mode === "CreditNote")
                    $("#credit-list").DataTable().ajax.reload();
                else if (mode === "DebitNote")
                    $("#debit-list").DataTable().ajax.reload();
            } else if (response.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#create-contact-credit').modal('toggle');
                if (mode === "CreditNote")
                    $("#credit-list").DataTable().ajax.reload();
                else if (mode === "DebitNote")
                    $("#debit-list").DataTable().ajax.reload();
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

// approval
function UpdateStatusApproval(apprKey) {
    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey },
        success: function (rs) {
            $.LoadingOverlay("hide");
            if (rs.actionVal > 0) {
                cleanBookNotification.updateSuccess();
            }
        },
        error: function (err) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};
//Payments
function tablePaymentsLoad() {
    var $paymentTable = $('#payment-list');
    var domainId = $("#contact-domain-id").val();
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#payment-list')) {
        $paymentTable.DataTable().destroy();
    }
    $paymentTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($paymentTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($paymentTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderContact/PaymentsByContact",
            "data": function (d) {
                return $.extend({}, d, {
                    "id": $contactId
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Type", "data": "Type", "searchable": true, "orderable": false },
            { "data": "Amount", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 2,
            "data": "Type",
            "render": function (data, type, row, meta) {
                if (data)
                    return '<a href="/TraderInvoices/InvoiceManage?key=' + row.IvKey + '">Invoice #' + data+'</a>';
                else
                    return 'Payment on account';
            }
        },{
            "targets": 4,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 0:
                        _statusHTML = '<span class="label label-lg label-info">Draft</span>';
                        break;
                    case 1:
                        _statusHTML = '<span class="label label-lg label-warning">Pending Review</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-primary">Pending Approval</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-success">Approved</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                }
                return _statusHTML;
            }
        },
        {
            "targets": 5,
            "data": "Id",
            "render": function (data, type, row, meta) {
                if (row.Status == 0 || row.Status == 1)
                    return '<button class="btn btn-primary" disabled=""><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                else
                    return '<button class="btn btn-primary" onclick="window.location.href =\'/TraderPayments/PaymentManage?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
            }
        }]
    });///TraderInvoices/InvoiceManage?key='+200
    $('#payment-table_filter').hide();
}
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 500);
    };
}
//Invoices
function tableInvoicesLoad() {
    var $invoiceTable = $('#invoice-list');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#invoice-list')) {
        $invoiceTable.DataTable().destroy();
    }
    $invoiceTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($invoiceTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($invoiceTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderContact/InvoiceByContact",
            "data": function (d) {
                return $.extend({}, d, {
                    "id": $contactId,
                    "type":"Sale"
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "data": "Amount", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 3,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 0:
                        _statusHTML = '<span class="label label-lg label-info">Draft</span>';
                        break;
                    case 1:
                        _statusHTML = '<span class="label label-lg label-warning">Pending Review</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-primary">Pending Approval</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-success">Approved</span>';
                        break;
                    case 5:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-success">Issued</span>';
                        break;
                }
                return _statusHTML;
            }
        },
        {
            "targets": 4,
            "data": "Id",
            "render": function (data, type, row, meta) {
                if (row.Status != 0)
                    return '<button class="btn btn-primary" onclick="window.location.href =\'/TraderInvoices/InvoiceManage?key=' + row.Key + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                else
                    return '<button class="btn btn-primary" disabled=""><i class="fa fa-eye"></i> &nbsp; Manage</button>';
            }
        }]
    });
    $('#invoice-table_filter').hide();
}
//Bills
function tableBillsLoad() {
    var $billTable = $('#bill-list');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#bill-list')) {
        $billTable.DataTable().destroy();
    }
    $billTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($billTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($billTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderContact/InvoiceByContact",
            "data": function (d) {
                return $.extend({}, d, {
                    "id": $contactId,
                    "type": "Purchase"
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "data": "Amount", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 3,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 0:
                        _statusHTML = '<span class="label label-lg label-info">Draft</span>';
                        break;
                    case 1:
                        _statusHTML = '<span class="label label-lg label-warning">Pending Review</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-primary">Pending Approval</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-success">Approved</span>';
                        break;
                    case 5:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-success">Issued</span>';
                        break;
                }
                return _statusHTML;
            }
        },
        {
            "targets": 4,
            "data": "Id",
            "render": function (data, type, row, meta) {
                if (row.Status != 0)
                    return '<button class="btn btn-primary" onclick="window.location.href =\'/TraderInvoices/InvoiceManage?key=' + row.Key + '\'"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                else
                    return '<button class="btn btn-primary" disabled=""><i class="fa fa-eye"></i> &nbsp; Manage</button>';
            }
        }]
    });
    $('#bill-table_filter').hide();
}
//Allocations
function tableAllocationLoad() {
    var $allocationTable = $('#table_allocation');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#table_allocation')) {
        $allocationTable.DataTable().destroy();
    }
    $allocationTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($allocationTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($allocationTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[1, "desc"]],
        ajax: {
            "url": "/TraderContact/AllocationList",
            "data": function (d) {
                return $.extend({}, d, {
                    "contactid": $contactId
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Invoice ref", "data": "InvoiceRef", "searchable": true, "orderable": false },
            { "data": "Value", "searchable": true, "orderable": true },
            { "title": "Notes", "data": "Description", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [
        {
            "targets": 5,
            "data": "Id",
            "render": function (data, type, row, meta) {
                var _htmlActions= '<button class="btn btn-primary" onclick="window.location.href=\'/TraderContact/AllocationDetail?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; View</button>';
                _htmlActions += '<button class="btn btn-danger" onclick="deleteAllocation(' + data+')"> <i class="fa fa-trash"></i></button>';
                return _htmlActions;
            }
        }]
    });
    $('#table_allocation-table_filter').hide();
}
//Credit Notes
function tableCreditLoad() {
    var $creditTable = $('#credit-list');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#credit-list')) {
        $creditTable.DataTable().destroy();
    }
    $creditTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($creditTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($creditTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[6, "desc"]],
        ajax: {
            "url": "/TraderContact/CreditNoteList",
            "data": function (d) {
                return $.extend({}, d, {
                    "contactid": $contactId,
                    "type":"Credit"
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Reason", "data": "Reason", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Invoice reference", "data": "InvoiceRef", "searchable": true, "orderable": false },
            { "title": "Sale reference", "data": "SaleRef", "searchable": true, "orderable": false },
            { "data": "Value", "searchable": true, "orderable": true },
            { "title": "Finalised", "data": "Finalised", "searchable": true, "orderable": true },
            { "title": "Notes", "data": "Notes", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 8,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 1:
                        _statusHTML = '<span class="label label-lg label-primary">Draft</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-warning">Awaiting Review</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-primary">Reviewed</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-primary">Approved</span>';
                        break;
                    case 5:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                }
                return _statusHTML;
            }
        },
            {
                "targets": 9,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    if (row.Status == 1)
                        return '<button class="btn btn-info" data-toggle="modal" data-target="#create-contact-credit" onclick="editCredit(' + data+')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                    else
                        return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderContact/CreditNoteDetail?id=' + data +'\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                }
            }]
    });
    $('#credit-table_filter').hide();
}
//Debit Notes
function tableDebitLoad() {
    var $debitTable = $('#debit-list');
    //table.ajax.reload( null, false ); 
    if ($.fn.DataTable.isDataTable('#debit-list')) {
        $debitTable.DataTable().destroy();
    }
    $debitTable.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $($debitTable).LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $($debitTable).LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: true,
        order: [[6, "desc"]],
        ajax: {
            "url": "/TraderContact/CreditNoteList",
            "data": function (d) {
                return $.extend({}, d, {
                    "contactid": $contactId,
                    "type": "Debit"
                });
            }
        },
        columns: [
            { "title": "#", "data": "Ref", "searchable": true, "orderable": true },
            { "title": "Reason", "data": "Reason", "searchable": true, "orderable": true },
            { "title": "Date", "data": "Date", "searchable": true, "orderable": true },
            { "title": "Invoice reference", "data": "InvoiceRef", "searchable": true, "orderable": false },
            { "title": "Sale reference", "data": "SaleRef", "searchable": true, "orderable": false },
            { "data": "Value", "searchable": true, "orderable": true },
            { "title": "Finalised", "data": "Finalised", "searchable": true, "orderable": true },
            { "title": "Notes", "data": "Notes", "searchable": true, "orderable": true },
            { "title": "Status", "data": "Status", "searchable": false, "orderable": true },
            { "title": "Actions", "data": "Id", "searchable": false, "orderable": false },
        ],
        columnDefs: [{
            "targets": 8,
            "data": "Status",
            "render": function (data, type, row, meta) {
                var _statusHTML = "";
                switch (data) {
                    case 1:
                        _statusHTML = '<span class="label label-lg label-primary">Draft</span>';
                        break;
                    case 2:
                        _statusHTML = '<span class="label label-lg label-warning">Awaiting Review</span>';
                        break;
                    case 3:
                        _statusHTML = '<span class="label label-lg label-primary">Reviewed</span>';
                        break;
                    case 4:
                        _statusHTML = '<span class="label label-lg label-primary">Approved</span>';
                        break;
                    case 5:
                        _statusHTML = '<span class="label label-lg label-danger">Denied</span>';
                        break;
                    default:
                        _statusHTML = '<span class="label label-lg label-danger">Discarded</span>';
                        break;
                }
                return _statusHTML;
            }
        },
        {
            "targets": 9,
            "data": "Id",
            "render": function (data, type, row, meta) {
                if (row.Status == 1)
                    return '<button class="btn btn-info" data-toggle="modal" data-target="#create-contact-credit" onclick="editCredit(' + data + ')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                else
                    return '<button class="btn btn-primary" onclick="window.location.href=\'/TraderContact/CreditNoteDetail?id=' + data + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
            }
        }]
    });
    $('#debit-table_filter').hide();
}


//
function getNewContactRef() {
    $.ajax({
        type: "get",
        url: "/TraderContact/GetNewContactRef",
        dataType: "json",
        success: function (response) {
            if (response) {
                $('#contactReferenceId').val(response.Id);
                $('#numberRef').text(response.Reference);
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        //ResetFormControl('form_group_add');
    });
}
function selectWorkGroupContact() {
    var wgselectd = $('#group-workgroup-id option:selected');
    $('.domain_name').text(wgselectd.attr("domain") + "-");
}
