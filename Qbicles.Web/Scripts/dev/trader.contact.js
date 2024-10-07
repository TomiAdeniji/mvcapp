var BKAccount = {};
var filter = {
    Contactgroup: "",
    Workgroup: "",
    Key: ""
};



function onSelectWorkgroup(ev) {
    filter.Workgroup = $(ev).val();
    setTimeout(function () { searchOnTableContact(); }, 200);
}
function onSelectContactGroup(ev) {
    filter.Contactgroup = $(ev).val();
    setTimeout(function () { searchOnTableContact(); }, 200);
}
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTableContact(); }, 200);
}
function searchOnTableContact() {
    $('#trader-contact-list').DataTable().ajax.reload();
    //var listKey = [];
    //if ($('#filter-group').val() !== "" && $('#filter-group').val() !== null) {
    //    listKey.push($('#filter-group').val());
    //}
    //if ($('#filter-contact-group').val() !== "" && $('#filter-contact-group').val() !== null) {
    //    listKey.push($('#filter-contact-group').val());
    //}
    //var keys = $('#trader_search_contact').val().split(' ');
    //if ($('#trader_search_contact').val() !== "" && $('#trader_search_contact').val() !== null && keys.length > 0) {
    //    for (var i = 0; i < keys.length; i++) {
    //        if (keys[i] && keys[i].trim() !== "") listKey.push(keys[i]);
    //    }
    //}
    //$("#trader-contact-list").DataTable().search(listKey.join("|"), true, false, true).draw();
}
// get selected account
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    BKAccount = { Id: id, Name: name };
    $("#hdfcontactaccountId").val(id);
    closeSelected();
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

//
$("#filter-group").on("change",
    function () {
        var group = $(this).val();
        $("#trader-contact-list").DataTable().search(group, true, false, true).draw();
    });
$("#filter-contact-group").on("change",
    function () {
        var group = $(this).val();
        $("#trader-contact-list").DataTable().search(group, true, false, true).draw();
    });
var oTable = $("#trader-contact-list");
//$("#trader_search_contact").keyup(function () {
//    $("#trader-contact-list").DataTable().search($(this).val()).draw();
//});

$('.manage-columns input[type="checkbox"]').on("change",
    function () {
        var table = $("#trader-contact-list").DataTable();
        var column = table.column($(this).attr("data-column"));
        column.visible(!column.visible());
    });

$('.manage-columns.v2 input[type="checkbox"]').on("change",
    function () {
        var table = $.closest($("#trader-contact-list")).DataTable();
        var column = table.column($(this).attr("data-column"));
        column.visible(!column.visible());
    });

// begin group funsions 
function clickAddNew(form) {
    $("#contact-group-label").removeClass("text-red");
    if (form === "form_contact_add") {
        getNewContactRef();
        $("#modal-title-contact").text("Add a contact");
        BKAccount = {};
        $(".accountInfo").text("");
        if ($(".accountInfo").text().length > 0) {
            $(".addbtnaccount").attr("style", "display:none;");
            $(".editbtnaccount").removeAttr("style");
        } else if ($(".accountInfo").text().length === 0) {
            $(".editbtnaccount").attr("style", "display:none;");
            $(".addbtnaccount").removeAttr("style");
        }
    }

    ResetFormControl(form);
    $("#contact-id").val(0);
    $("#hdfcontactaccountId").val(0);
    $("#group-contact-id").trigger("change");
    $("#group-workgroup-id").trigger("change");
    $("#contact-address-id").val(0);

}
function selectWorkGroupContact() {
    var wgselectd = $('#group-workgroup-id option:selected');
    $('.domain_name').text(wgselectd.attr("domain")+"-");
}
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


function addgroup(channelgroup) {
    if ($("#form_group_add").valid()) {
        $.ajax({
            type: "post",
            url: "/TraderContact/SaveGroup",
            data: { Name: $("#addnew-group-name").val(), saleChannelGroup:channelgroup },
            dataType: "json",
            success: function (response) {
                if (response.result === true) {
                    if (response.actionVal === 1) {
                        cleanBookNotification.createSuccess();
                        reloadgroup(response);
                        $("#app-group-generic-add").modal("toggle");
                    } else if (response.actionVal === 3) {
                        $("#app-group-generic-add").validate().showErrors({ Name: response.msg });
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

reloadgroup = function (optionStr) {
    $("#filter-group").append(optionStr.msg);
    $("#group-contact-id").append(optionStr.msgName);
};
// end group functions

//contact 
function EditContact(id) {
    $("#modal-title-contact").text("Edit contact");
    ResetFormControl("form_contact_add");
    $('.domain_name').text("");
    $.ajax({
        type: "get",
        url: "/TraderContact/GetContactById",
        data: { id: id },
        dataType: "json",
        success: function (contact) {

            $("#contact-name").val(contact.Name);
            $("#contact-company").val(contact.CompanyName);
            $("#contact-job").val(contact.JobTitle);
            $("#contact-phone").val(contact.PhoneNumber);
           
            if (contact.ContactRef) {
                $("#contactReferenceId").val(contact.ContactRef.Id);
                $("#numberRef").text(contact.ContactRef.Reference);
                $("#newrefnum").val(contact.ContactRef.ReferenceNumber);
            }
            

            $("#contact-email").val(contact.Email);

            $("#group-contact-id").select2("destroy");
            $("#group-contact-id").val(contact.ContactGroup.Id);
            $("#group-contact-id").select2();
            $("#group-contact-id").trigger("change");

            $("#group-workgroup-id").select2("destroy");
            $("#group-workgroup-id").val(contact.Workgroup.Id);
            $("#group-workgroup-id").select2();
            $("#group-workgroup-id").trigger("change");

            $("#contact-id").val(contact.Id);

            $("#contact-address-id").val(contact.Address.Id);

            $("#PostCode").val(contact.Address.PostCode);
            if (contact.Address.Country !== null)
                $("#CountryName").val(contact.Address.Country.CommonName).trigger('change');;
            $("#State").val(contact.Address.State);
            $("#City").val(contact.Address.City);
            $("#AddressLine2").val(contact.Address.AddressLine2);
            $("#AddressLine1").val(contact.Address.AddressLine1);
            $("#contact-longitude").val(contact.Address.Longitude);
            $("#contact-Latitude").val(contact.Address.Latitude);
            
            
            if (contact.CustomerAccount.Id > 0) {
                BKAccount = { Id: contact.CustomerAccount.Id, Name: contact.CustomerAccount.Name };
                $("#hdfcontactaccountId").val(BKAccount.Id);
                $(".accountInfo").text(BKAccount.Name);
            } else {
                BKAccount = {};
                $("#hdfcontactaccountId").val(0);
                $(".accountInfo").text("");
            }
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
            
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function ConfirmDeleteContact(id, name) {
    $("#label-confirm-contact").text("Do you want delete contact: " + name);
    $("#id-itemcontact-delete").val(id);
}

function DeleteContact() {
    $.ajax({
        type: "POST",
        url: "/TraderContact/DeleteContact",
        data: { id: $("#id-itemcontact-delete").val() },
        dataType: "json",
        success: function (response) {
            if (response === "OK") {
                cleanBookNotification.removeSuccess();
                searchOnTableContact();
            } else if (response === "Fail") {
                cleanBookNotification.removeFail();
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function initContactForm() {
    $('#form_contact_add').validate(
        {
            rules: {
                Name: {
                    required: true,
                    maxlength: 71
                }
            }
        });
}

function SaveTraderContact(status) {
    
    
    $("#contact-status").val(status);
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
        Id: $("#contact-id").val()
    };

    if ($("#form_contact_add").valid()) {
        //if ($("#contact-name").val() === null || ($("#contact-name").val()+"").length > 70) {
        //    cleanBookNotification.error(_L("ERROR_MSG_261"), "Qbicles");
        //    $("#label_contact_name").addClass("text-red"); 
        //    return;
        //} else {
        //    $("#contact-group-label").removeClass("text-red");
        //}
        if ($("#group-contact-id").val() === null) {
            cleanBookNotification.error(_L("ERROR_MSG_262"), "Qbicles");
            $("#contact-group-label").addClass("text-red");
            return;
        }
        if ($("#group-workgroup-id").val() === null) {
            cleanBookNotification.error(_L("ERROR_MSG_263"), "Qbicles");
            return;
        }

        if ($("#contactAvatar").val() === "" && $("#contact-id").val() === "0") {
            cleanBookNotification.error(_L("ERROR_MSG_264"), "Qbicles");
            return;
        }
        if ($("#group-contact-id").val() === null) {
            return;
        } if ($("#group-workgroup-id").val() === null) {
            return;
        }
        $.ajax({
            url: "/TraderContact/TraderContactNameCheck",
            data: { contact: contact },
            type: "POST",
            dataType: "json",
        }).done(function (refModel) {
            if (refModel.result)
                $("#form_contact_add").validate()
                    .showErrors({ Name: _L("ERROR_MSG_251") });
            else {
                ProcessContactMedia();
            }
        }).fail(function () {
            $("#form_contact_add").validate()
                .showErrors({ Name: _L("ERROR_MSG_252") });
        });
    }
}




ProcessContactMedia = function () {
    $.LoadingOverlay("show");
    var files = document.getElementById("trader-contact-avatar-upload").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("trader-contact-avatar-upload").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#trader-contact-object-key").val(mediaS3Object.objectKey);
            $("#trader-contact-object-name").val(mediaS3Object.fileName);
            $("#trader-contact-object-size").val(mediaS3Object.fileSize);

            SubmitTraderContact();
        });

    } else
        SubmitTraderContact();
}

SubmitTraderContact = function () {
    var frmData = new FormData($("#form_contact_add")[0]);
    var contactObj = {
        Id: frmData.get("Id"),
        Email: frmData.get("Email")
    }
    $.ajax({
        method: "POST",
        dataType: "JSON",
        url: "/TraderContact/CheckExistedApprovedContact",
        data: {
            contact: contactObj
        }
    }).done(function (refModel) {
        if (refModel.result) {
            $.ajax({
                type: "post",
                cache: false,
                url: "/TraderContact/SaveTraderContact",
                enctype: "multipart/form-data",
                data: frmData,
                processData: false,
                contentType: false,
                beforeSend: function (xhr) {
                },
                success: function (data) {
                    $.LoadingOverlay("hide");
                    if (data.result) {
                        $("#app-trader-modal-contact").modal("toggle");

                        if (data.actionVal === 1) {
                            cleanBookNotification.createSuccess();
                        } else {
                            cleanBookNotification.updateSuccess();
                        }
                        searchOnTableContact();
                    } else {
                        if (data.msgId == 2) {
                            cleanBookNotification.error(data.msg, "Qbicles")
                        } else {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    }
                },
                error: function (data) {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    
                }

            }).always(function () {
                LoadingOverlayEnd();
            });
        } else {
            cleanBookNotification.error(refModel.msg, "Qbicles");
        }
    }).fail(function (er) {
        cleanBookNotification.error("Error checking existing Approved Trader Contact", "Qbicles");
    }).always(function () {
        LoadingOverlayEnd();
    });;
}

// show table
$(function () {
    //ShowTableContactValue();
    LoadContactContent();
});
function ShowTableContactValue() {
    $.LoadingOverlay("show");
    var ajaxUri = "/TraderContact/ShowContactContent";
    $('#trader-contact-content').load(ajaxUri, function () {
        LoadingOverlayEnd();
        searchOnTableContact();
    });
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function LoadContactContent() {
    $("#trader-contact-list").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#trader-contact-content').LoadingOverlay("show");
        } else {
            $('#trader-contact-content').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "serverSide": true,
        "info": true,
        "stateSave": false,
        "bLengthChange": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "scrollX": false,
        "autoWidth": true,
        "pageLength": 10,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderContact/LoadContactContent',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    "search": $("#trader_search_contact").val(),
                    "groupId": $("#filter-group").val().split(","),
                    "contactGroupId": $("#filter-contact-group").val()
                });
            }
        },
        "columns": [
            {
                data: "Reference",
                orderable: true
            },
            {
                data: "Avatar",
                orderable: false,
                render: function (value, type, row) {
                    var str = '<div class="table-avatar" style="background-image: url(' + $("#api-uri").val() + row.Avatar + "&size=T" + ');">&nbsp;</div>';
                    return str;
                }
            },
            {
                data: "Name",
                orderable: true
            },
            {
                data: "ContactGroup",
                orderable: true
            },
            {
                data: "Workgroup",
                orderable: true
            },
            {
                data: "AccountBalance",
                orderable: false,
            },
            {
                data: "BillBalance",
                orderable: false,
            },
            {
                data: "InvoiceBalance",
                orderable: false,
            },
            {
                data: "Status",
                orderable: true,
                render: function (value, type, row) {
                    var str = "";
                    switch (row.Status) {
                        case "Draft":
                            str = '<span class="label label-lg label-primary">Draft</span>';
                            break;
                        case "PendingReview":
                            str = '<span class="label label-lg label-warning">Awaiting Review</span>';
                            break;
                        case "PendingApproval":
                            str = '<span class="label label-lg label-warning">Awaiting Approval</span>';
                            break;
                        case "ContactDenied":
                            str = '<span class="label label-lg label-danger">Denied</span>';
                            break;
                        case "ContactApproved":
                            str = '<span class="label label-lg label-success">Approved</span>';
                            break;
                        case "ContactDiscarded":
                            str = '<span class="label label-lg label-danger">Discarded</span>';
                            break;
                    }
                    return str;
                }
            },
            {
                data: "Action",
                orderable: false,
                width: 200,
                render: function (value, type, row) {
                    var spAction = row.Action.split(",");
                    var str = "";
                    for (i = 0; i < spAction.length; i++) {
                        if (spAction[i] == '1') {
                            str += '<button class="btn btn-info" onclick="EditContact(' + row.Id + ')" data-toggle="modal" data-target="#app-trader-modal-contact" style="margin-right: 3px"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                        } else if (spAction[i] == '2') {
                            str += "<button class=\"btn btn-primary\" onclick=\"window.location.href='/TraderContact/ContactMaster?key=" + row.Key + "'\" style=\"margin-right:3px\"><i class=\"fa fa-eye\"></i> &nbsp; Manage</button>";
                        } else if (spAction[i] == '3') {
                            str += "<button class=\"btn btn-danger\"" + (row.IsDisabled ? 'disabled' : '') + " data-toggle=\"modal\" data-target=\"#app-contact-confirm\" onclick=\"ConfirmDeleteContact(" + row.Id + ", '" + row.Name + "')\" > <i class=\"fa fa-trash\"></i>&nbsp; Delete</button >";
                        }
                    }
                    return str;
                }
            }
        ],
        "order": [[0, "asc"], [2, "asc"], [3, "asc"], [4, "asc"]]
    });
}