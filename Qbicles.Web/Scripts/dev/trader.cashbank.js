
var $cashAccountId = 0;
var $locationManageId = $("#local-manage-select");
var BKAccount = {
    Id: 0,
    Name: ""
};
var filter = {
    Workgroup: "",
    Key: ""
}
function onKeySearchChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { searchOnTableCashBank(); }, 200);
}
function searchOnTableCashBank() {
    var listKey = [];

    var keys = $('#search_dt_cb').val().split(' ');
    if ($('#search_dt_cb').val() !== "" && $('#search_dt_cb').val() !== null && keys.length > 0) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#community-list").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#community-list").val("");
}
ClearAllModal = function () {
    $("#cashbank-transfer").empty();
    $("#app-trader-cashbank").empty();
    $("#cashbank-payment").empty();
};


AddEditTraderCashBank = function (id) {
    $.LoadingOverlay("show");
    ClearAllModal();
    setTimeout(function () {
        $("#app-trader-cashbank").load("/TraderCashBank/TraderCashBankAddEdit?id=" + id);
        $.LoadingOverlay("hide");

        $("#app-trader-cashbank").modal("toggle");
    }, 2000);
}

//account treeview
function initBKAccount(id, name) {
    BKAccount.Id = id;
    BKAccount.Name = name;
}
function selectAccount(ev, id) {
    var name = $(".accountid-" + id).data("name");
    $(".selectaccount").removeClass("selectaccount");
    $(ev).addClass("selectaccount");
    $("#accountId").val(id);
    BKAccount.Id = id;
    BKAccount.Name = name;
    $("#accountId").val(id);
    closeSelected();
    $("#app-bookkeeping-treeview").modal("hide");
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
    }, 1);
};
//end treeview account
SaveCashAccount = function () {
    if (!$("#form_cash_bank").valid())
        return;

    //if (BKAccount.Id === 0) {
    //    cleanBookNotification.error("Please select an account!", "Qbicles");
    //    return;
    //}

    if ($("#cash-bank-icon").val() === "" && $("#cash-bank-id").val() === "0") {
        cleanBookNotification.error(_L("ERROR_MSG_618"), "Qbicles");
        return;
    }
    var cashBank = {
        Id: $("#cash-bank-id").val(),
        Name: $("#cash-bank-name").val()
    };

    $.ajax({
        type: "post",
        url: "/TraderCashBank/TraderCashAccountNameCheck",
        data: { cashBank: cashBank },
        dataType: "json",
        success: function (response) {
            if (response.result === false) {
                $("#form_cash_bank").validate().showErrors({ Name: response.msg });
                return;
            } else {
                ProcessCashBankAccount();
            }
        },
        error: function (er) {
            return;
        }
    });
};


function ProcessCashBankAccount() {
    $.LoadingOverlay("show");
    var files = document.getElementById("cash-bank-icon").files;

    if (files && files.length > 0) {
        UploadMediaS3ClientSide("cash-bank-icon").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            $("#cash-bank-object-key").val(mediaS3Object.objectKey);
            $("#cash-bank-object-name").val(mediaS3Object.fileName);
            $("#cash-bank-object-size").val(mediaS3Object.fileSize);

            CashBankAccountSubmit();
        });

    } else
        CashBankAccountSubmit();
};

CashBankAccountSubmit = function () {
    var frmData = new FormData($("#form_cash_bank")[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderCashBank/SaveTraderCashAccount",
        enctype: "multipart/form-data",
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
        },
        success: function (response) {
            LoadingOverlayEnd();
            if (response.actionVal === 1) {
                $("#app-trader-cashbank").modal("hide");
                setTimeout(function () {
                    cleanBookNotification.createSuccess();
                    ShowTableCashBankValue();
                }, 100);
            } else if (response.actionVal === 2) {
                $("#app-trader-cashbank").modal("hide");
                setTimeout(function () {
                    cleanBookNotification.updateSuccess();
                    ShowTableCashBankValue();
                }, 100);
            } else if (response.actionVal === 3) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            $.LoadingOverlay("hide");
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
};


UploadImage = function (id, type) {
    UploadMediaS3ClientSide("form_specifics_icon").then(function (mediaS3Object) {
        return mediaS3Object.objectKey;
    });
}
DeleteTraderCashBank = function (id) {
    //alert('delete');
    //show confirm
    //controller DeleteTraderCashAccount(int id)
}




function manageonclick(id) {
    $.LoadingOverlay("show");
    window.location.href = '/TraderCashBank/TraderCashAccount?id=' + id + '&locationid=' + $locationManageId.val()
}


// show table
$(function () {
    ShowTableCashBankValue();
    ClearAllModal();
});
function ShowTableCashBankValue() {
    $('#cashbank-content').LoadingOverlay("show");
    $('#cashbank-content').load("/TraderCashBank/TraderCashBankContents", function () {
        $('.manage-columns input[type="checkbox"]').on('change', function () {
            var table = $('#community-list-cash-bank').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });
        $('#cashbank-content').LoadingOverlay("hide");
        setTimeout(function () {
            searchOnTableCashBank();
        }, 300);

    });
}

function LoadTableDataCashBank(tableid, url, columns, orderIndex) {
    if (!orderIndex) orderIndex = 1;
    $("#" + tableid).on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $(e.currentTarget).LoadingOverlay("show");
        } else {
            $(e.currentTarget).LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": "",
            "processing": loadingoverlay_value
        },
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": url,
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    keysearch: $('#search_dt_cb').val()
                });
            }
        },
        "columns": columns,
        "order": [[orderIndex, "asc"]]
    });
}
function FilterDataCashBankByServerSide() {
    var url = '/TraderCashBank/GetCashBankContent';
    var columns = [
        {
            name: "Image",
            data: "Image",
            render: function (value, type, row) {
                var str = '';
                str += '<div class="table-avatar" style="background-image: url(\'' + $("#api-uri").val() + row.Image + '&size=T\');">&nbsp;</div>';
                return str;
            }
        },
        {
            name: "Name",
            data: "Name",
            orderable: true,
            render: function (value, type, row) {
                var str = '';
                str += '<h5 style="margin: 0; padding: 0 0 3px 0;">' + row.Name + '</h5>';
                return str;
            }
        },
        {
            name: "BookkeepingAccount",
            data: "BookkeepingAccount",
            orderable: false
        },
        {
            name: "FundsIn",
            data: "FundsIn",
            orderable: false
        },
        {
            name: "FundsOut",
            data: "FundsOut",
            orderable: false
        },
        {
            name: "Charges",
            data: "Charges",
            orderable: false
        },
        {
            name: "Transactions",
            data: "Transactions",
            orderable: false
        },
        {
            data: null,
            orderable: false,
            render: function (value, type, row) {
                var str = '<div class="btn-group options">';
                str += '<button type="button" class="btn btn-success dropdown-toggle"  data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                str += '<i class="fa fa-cog"></i> &nbsp; Options </button>';
                str += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">';
                str += '<li><a href="#" onclick="manageonclick(' + row.Id + ')">Manage</a></li>';
                if (row.BankmateType == 0)
                    str += '<li><a href="#" data-toggle="modal" onclick="AddEditTraderCashBank(' + row.Id + ')" >Edit</a></li>';
                str += '</ul> </div>';
                if (row.AllowEdit)
                    return str;
                else return "";
            }
        }
    ];
    LoadTableDataCashBank('community-list-cash-bank', url, columns, 1);
    CallBackFilterDataCashBankServeSide();
}

function CallBackFilterDataCashBankServeSide() {
    $("#community-list-cash-bank").DataTable().ajax.reload();
}


