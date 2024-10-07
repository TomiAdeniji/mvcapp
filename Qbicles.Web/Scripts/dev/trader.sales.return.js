
$(function () {
    $("#subfilter-group-return").select2();
    
    LoadDataSalesReturn();
    $("#subfilter-group-return").on("change", delay(function () {
        CallBackDataTableTraderSaleReturn();
    }, 0));
    $("#search_dt").keyup(delay(function () {
        CallBackDataTableTraderSaleReturn();
    },2000));
    $('.manage-columns input[type="checkbox"]').on('change',
        function () {
            var table = $('#tb_trader_sales_return').DataTable();
            var column = table.column($(this).attr('data-column'));
            column.visible(!column.visible());
        });

});
function OnSelectWorkgroupReturn(ev) {
    filter.Workgroup = $(ev).val();
    setTimeout(function () { SearchOnSaleReturnTable(); }, 200);
}

function OnKeySearchReturnChanged(ev) {
    filter.Key = $(ev).val();
    setTimeout(function () { SearchOnSaleReturnTable(); }, 200);
}
function SearchOnSaleReturnTable() {
    var listKey = [];
    if ($('#subfilter-group-return').val() !== "" && $('#subfilter-group-return').val() !== null) {
        listKey.push($('#subfilter-group-return').val());
    }
    var keys = $('#search_dt_return').val();
    if ($('#search_dt_return').val() !== "" && $('#search_dt_return').val() !== null && keys && keys.length > 0) {
        keys = keys.split(' ');
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#tb_trader_sales_return").DataTable().search(listKey.join("|"), true, false, true).draw();
    $("#tb_trader_sales_return_filter input").val("");
};



function CallBackDataTableTraderSaleReturn() {
    $("#tb_trader_sales_return").DataTable().ajax.reload();
};

function LoadDataSalesReturn() {
    $("#tb_trader_sales_return").on('processing.dt', function (e, settings, processing) {
        
        if (processing && $('.loadingoverlay').length === 0) {
            $('#tblwrapreturn').LoadingOverlay("show");
        } else {
            $('#tblwrapreturn').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "dom": '<"top"fl<"clear">>rt<"bottomtable"ip<"clear">>',
        "language": {
            "infoFiltered": ""
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderSalesReturn/GetDataTableSalesReturn',
            "type": 'GET',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search_dt_return").val(),
                    "workGroupId": $("#subfilter-group-return").val() ? $("#subfilter-group-return").val() : 0
                });
            }
        },
        "columns": [
            {
                name: "FullRef",
                data: "FullRef",
                orderable: true
            },
            {
                name: "WorkgroupName",
                data: "WorkgroupName",
                orderable: true
            },
            {
                name: "CreatedDate",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "SalesRef",
                data: "SalesRef",
                orderable: true,
                render: function (value, type, row) {
                    var str = "<a href='/TraderSales/SaleMaster?key="+row.SaleRefKey+"'>" + row.SalesRef +"</a>";
                    return str;
                }
            },
            {
                name: "Status",
                data: "Status",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = '';
                    switch (row.Status) {
                        case 'Draft':
                            strStatus += '<span class="label label-lg label-info">Draft</span>';
                            break;
                        case 'PendingReview':
                            strStatus += '<span class="label label-lg label-warning">Awaiting Review</span>';
                            break;
                        case 'PendingApproval':
                            strStatus += '<span class="label label-lg label-success">Awaiting Approval</span>';
                            break;
                        case 'Denied':
                            strStatus += '<span class="label label-lg label-danger">Denied</span>';
                            break;
                        case 'Approved':
                            strStatus += '<span class="label label-lg label-primary">Approved</span>';
                            break;
                        case 'Discarded':
                            strStatus += '<span class="label label-lg label-danger">Discarded</span>';
                            break;
                        case 'Reviewed':
                            strStatus += '<span class="label label-lg label-primary">Reviewed</span>';
                            break;
                        default:
                            strStatus += '<span class="label label-lg label-info">Draft</span>';
                            break;
                    }
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = '';
                    if (row.Status === 'Draft') {
                        if (row.AllowEdit) {
                            str += '<button class="btn btn-info" onclick="EditSaleReturn(\'' + row.Id + '\')"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                        } else {
                            str += '<button class="btn btn-info hidden"><i class="fa fa-pencil"></i> &nbsp; Continue</button>';
                        }

                    } else {
                        str += '<button class="btn btn-primary" onclick="window.location.href = \'/TraderSalesReturn/SaleReturnMaster?id=' + row.Id + '\';"><i class="fa fa-eye"></i> &nbsp; Manage</button>';
                    }
                    return str;
                }
            }
        ],
        "order": [[2, "desc"]]
    });
}

function ShowTableSaleReturnValue(isReload) {
    $('#trader-sale-return-content').empty();
    $('#trader-sale-return-content').load('/TraderSalesReturn/TraderSaleReturnTable', filter, function (reFilter) {
    });
};


// --------------- Add Edit Sale return --------------------

function AddSaleReturn() {
    var ajax = '/TraderSalesReturn/TraderSaleReturnAdd';
    AjaxElementShowModal(ajax, "app-trader-sale-return-add");
};


function EditSaleReturn(traderSaleReturnId) {
    var ajax = '/TraderSalesReturn/TraderSaleReturnAdd?traderSaleReturnId=' + traderSaleReturnId;
    AjaxElementShowModal(ajax, "app-trader-sale-return-add");
};


ChangeWorkgroupReturn = function (ev) {
    var $workgroupId = $("#trader_sale_return_add_workgroup").val();
    if ($workgroupId !== "") {

        $.ajax({
            type: "get",
            url: "/TraderTransfers/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                $('.preview-workgroup').show();
                if (response.result) {
                    $(".preview-workgroup table tr td.location_name").text(response.Object.Location);
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member span").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.location_name").text('');
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member span").text('');
                }

            },
            error: function (er) {
                $('.preview-workgroup').hide();
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    } else {

        $('.preview-workgroup').hide();
    }
};

function ShowGroupMemberReturn() {
    $('#app-trader-workgroup-preview').empty();
    $('#app-trader-workgroup-preview').load("/TraderTransfers/ShowListMemberForWorkGroup?wgId=" + $workgroupId);
    $('#app-trader-workgroup-preview').modal('toggle');
}