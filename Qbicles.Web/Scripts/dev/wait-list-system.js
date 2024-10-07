var $waitlistIds = [];
var $waitlistBulkIds = [];
var is_dataTable_reloading = true;
//var lst_ignored_page_ids_on_selecting_all = []
var waitlist_table = '';
function LoadWaitlistRequest() {

    $("#table-waitlist-request").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing && $('.loadingoverlay').length === 0) {
            $('#table-waitlist-request').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#table-waitlist-request').LoadingOverlay("hide", true);
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
        "pageLength": 20, preDrawCallback: function () {
            is_dataTable_reloading = true;
        },
        "ajax": {
            "url": '/Waitlist/GetWaitListRequests',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var $paramaters = {};
                var $keyword = $("#waitlist-keyword").val();
                var $daterange = $("#waitlist-daterange-search").val();
                var $employees = $("#waitlist-employee-search").val();
                var $discoveredVia = $("#waitlist-discovered-search").val();
                var $countries = $("#waitlist-country-search").val();

                if ($keyword.length > 0)
                    $paramaters.keyword = $keyword;

                if ($daterange.length > 0)
                    $paramaters.daterange = $daterange;

                if ($employees != null && $employees.length > 0)
                    $paramaters.employees = $employees;

                if ($discoveredVia != null && $discoveredVia.length > 0)
                    $paramaters.discoveredVia = $discoveredVia;

                if ($countries != null && $countries.length > 0)
                    $paramaters.countries = $countries;

                return $.extend({}, d, $paramaters);
            }
        },
        "columns": [
            {
                "name": "Id",
                "data": "Id",
                "searchable": false,
                orderable: false,
                render: function (value, type, row) {
                    var strStatus = '<td><input type="checkbox" class="dt-checkboxes" waitlist-id="' + row.Id + '"></td>';
                    return strStatus;
                },
                "checkboxes": {
                    'selectRow': true,
                    'selectCallback': function (nodes, selected) {
                        
                        var selected_waitlist_id = $(nodes[0]).find('.dt-checkboxes').attr('waitlist-id');
                        var selected_obj = {
                            'waitlist_id': selected_waitlist_id,
                        };

                        var index = $waitlistIds.findIndex(v => v.waitlist_id == selected_waitlist_id);

                        if (index <= -1 && selected) {
                            $waitlistIds.push(selected_obj);
                        } else if (index >= 0 && !selected) {
                            $waitlistIds.splice(index, 1);
                        }
                        if (!is_dataTable_reloading) {
                            toggleWaitlistBulk();
                            //lst_ignored_page_ids_on_selecting_all.push(waitlist_table.page.info().page);
                        }
                    },
                    'selectAllCallback': function (nodes, selected, indeterminate) {
                        
                        if (indeterminate === false) {
                            var checkboxes = $(".dt-checkboxes");
                            _.forEach(checkboxes, function (checkbox_item) {
                                var selected_waitlist_id = $(checkbox_item).attr('waitlist-id');
                                var selected_obj = {
                                    'waitlist_id': selected_waitlist_id,
                                };

                                var index = $waitlistIds.findIndex(v => v.waitlist_id == selected_waitlist_id);

                                if (index <= -1 && selected) {
                                    $waitlistIds.push(selected_obj);
                                } else if (index >= 0 && !selected) {
                                    $waitlistIds.splice(index, 1);
                                }
                            })
                            if (!is_dataTable_reloading) {
                                toggleWaitlistBulk();
                                //lst_ignored_page_ids_on_selecting_all.push(waitlist_table.page.info().page);
                            }
                        }
                    }
                }
            },
            {
                name: "DateTime",
                data: "DateTime",
                width: "150px",
                orderable: true
            },
            {
                name: "User",
                data: "User",
                width: "250px",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = " <a href='my-contact-profile.php'>";
                    strStatus += "<div class='table-avatar mini pull-left' style='background-image: url(\"" + row.CreatorUri + "\");'></div>"
                    strStatus += "<div class='avatar-name pull-left' style='color: #333; line-height: 4; padding-left: 15px;'>" + row.User + "</div>";
                    strStatus += "<div class='clearfix'></div>";
                    strStatus += "</a>";
                    return strStatus;
                }
            },
            {
                name: "Country",
                data: "Country",
                orderable: true
            },
            {
                name: "Business",
                data: "Business",
                orderable: true
            },
            {
                name: "Discovered",
                data: "Discovered",
                orderable: true
            },
            {
                name: "Categories",
                data: "Categories",
                orderable: false
            },            
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = "<div class='btn-group'>";
                    str += "<button type='button' class='btn btn-primary dropdown-toggle' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>";
                    str += "Actions &nbsp; <i class='fa fa-angle-down'></i></button>";
                    str += "<ul class='dropdown-menu dropdown-menu-right primary'>";
                    if (row.IsApprovedForSubsDomain == false && row.IsApprovedForCustomDomain == false) {
                        str += "<li><a href='#' onclick='ApprovalSubscriptionDomain(" + row.Id + ")'>Allow Subscription Domains</a></li>";
                        str += "<li><a href='#' onclick='ApprovalSubscriptionAndCustomDomain(" + row.Id + ")'><i class='fa fa-exclamation'></i>Allow Subscription &amp; Custom Domains</a></li>";
                    }                    
                    str += "<li><a href='#' onclick='RejectWaitlistDomain(" + row.Id + ")'>Reject</a></li>";
                    str += "</ul>";
                    str += "</div>";
                    return str;
                }
            }
        ],
        "order": [[1, "desc"]]
    });

    $('#table-waitlist-request').on('draw.dt', function () {
        is_dataTable_reloading = false;
        //if (!lst_ignored_page_ids_on_selecting_all.includes(waitlist_table.page.info().page)) {
        //    setTimeout(function () {
        //        var is_all_current_prices_checked = $("#table-waitlist-request > thead input[type=checkbox]").is(':checked');
        //        if (!is_all_current_prices_checked) {
        //            $("#table-waitlist-request > thead input[type=checkbox]").click();
        //        }
        //    }, 200);
        //}
    })
};
$("#waitlist-keyword").keyup(searchThrottle(function () {
    $('#table-waitlist-request').DataTable().ajax.reload();
}));

$("#waitlist-employee-search,#waitlist-discovered-search,#waitlist-country-search").on('change', function () {
    $('#table-waitlist-request').DataTable().ajax.reload();
});

//function CheckBoxWaitlistChange() {
//    $('.withselected').fadeToggle();

//}

function toggleWaitlistBulk() {
    var checkedNumber = $waitlistIds.length;
    if (checkedNumber > 0) {
        $('.withselected').show();
    } else {
        $('.withselected').hide();
    }
}


function ApprovalSubscriptionAndCustomDomain(waitlistId) {
    if (confirm('Are you sure you want to allow this user to subscribe to Business Domains, and create Custom Domains?')) {
        $waitlistIds = [{ 'waitlist_id': waitlistId }];;
        ApprovalWaitlist($waitlistIds, 103);
    }
};

function ApprovalSubscriptionDomain(waitlistId) {
    if (confirm('Are you sure you want to allow this user to subscribe to Business Domains?')) {
        $waitlistIds = [{ 'waitlist_id': waitlistId }];;
        ApprovalWaitlist($waitlistIds, 104);
    }
};

function ApprovalCustomDomain(waitlistId) {
    if (confirm('Are you sure you want to allow this user to subscribe to Business Domains, and create Custom Domains?')) {
        $waitlistIds = [{ 'waitlist_id': waitlistId }];;
        ApprovalWaitlist($waitlistIds, 105);
    };
};

function RejectWaitlistDomain(waitlistId) {
    if (confirm('Are you sure you want to remove this user from the Waitlist? This operation cannot be undone.')) {
        $waitlistIds = [{ 'waitlist_id': waitlistId }];
        ApprovalWaitlist($waitlistIds, 106);
    };
}

function GetWaitlistBulk() {
    var waitlistBulkIds = $("#waitlist-bulk-ids").val();
    return waitlistBulkIds.split(",").map(Number);
}

function ApprovalSubscriptionAndCustomDomainBulk(selected) {
    if (confirm('Are you sure you wish to allow the listed users to create BOTH Subscription and Custom Domains?')) {
        if (selected)
            ApprovalWaitlist($waitlistIds, 103);
        else
            ApprovalWaitlist(GetWaitlistBulk(), 103, true);
    }
};

function ApprovalSubscriptionDomainBulk(selected) {
    if (confirm('Are you sure you want to allow the listed users to create Subscription Domains?')) {
        if (selected)
            ApprovalWaitlist($waitlistIds, 104);
        else
            ApprovalWaitlist(GetWaitlistBulk(), 104, true);
    }
};


function RejectWaitlistDomainBulk(selected) {
    if (confirm('Are you sure to want to remove the ' + $waitlistIds.length + ' selected users from the waitlist? This cannot be undone.')) {
        if (selected)
            ApprovalWaitlist($waitlistIds, 106);
        else
            ApprovalWaitlist(GetWaitlistBulk(), 106, true);
    };
}

function FilterWaitlistByIds() {
    var ids = $waitlistIds.map(obj => obj.waitlist_id).join();
    var ajaxUri = '/Waitlist/FilterWaitlistByIds?ids=' + ids;
    AjaxElementShowModal(ajaxUri, 'waitlist-results');
}



/**
 * 
 * @param {any} waitlistIds
 * @param {any} approvalType 103-Subscription and custom; 104-Subscription; 105-custom; 106-reject
 */
function ApprovalWaitlist(waitlistIds, approvalType, bulk) {
    var _url = '/Waitlist/ApprovalWaitlist';
    if (bulk != true)
        waitlistIds = waitlistIds.map(obj => obj.waitlist_id);
    $.ajax({
        'method': 'POST',
        'dataType': 'JSON',
        'data': {
            waitlistIds: waitlistIds,
            approvalType: approvalType
        },
        'url': _url,
        'success': function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();
                $("#table-waitlist-request").DataTable().ajax.reload();
                $("#table-waitlist-history").DataTable().ajax.reload();
                $("#table-waitlist-revoke").DataTable().ajax.reload();
                $waitlistIds = [];
                $("#waitlist-bulk").modal('hide');
                $("#waitlist-results").modal('hide');

                $("#bulk-category").val(null).trigger("change");
                $("#bulk-country").val(null).trigger("change");
            } else {
                cleanBookNotification.error(response.msg, 'Qbicles');
            }
        },
        'error': function (err) {
            cleanBookNotification.error(err.msg, 'Qbicles');
        }
    })
}



var $bulkType = 0;
function ShowUserWaitlist() {
    var ajaxUri = '/Waitlist/FilterWaitlist?country=' + $("#bulk-country").val() + '&categoryId=' + $("#bulk-category").val() + '&type=' + $bulkType;
    AjaxElementShowModal(ajaxUri, 'waitlist-results');
}

$("#history-keyword").keyup(searchThrottle(function () {
    $('#table-waitlist-history').DataTable().ajax.reload();
}));
$("#history-employee-search,#history-discovered-search,#history-rights-search").on('change', function () {
    $('#table-waitlist-history').DataTable().ajax.reload();
});

function searchThrottle(f, delay) {
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


function LoadWaitlistHistory() {

    $("#table-waitlist-history").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        //if (processing && $('.loadingoverlay').length === 0) {
        //    $('#table_show').LoadingOverlay("show", { minSize: "70x60px" });
        //} else {
        //    $('#table_show').LoadingOverlay("hide", true);
        //}
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
        "pageLength": 20,
        "ajax": {
            "url": '/Waitlist/GetDomainCreationRightsLog',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var $paramaters = {};
                var $keyword = $("#history-keyword").val();
                var $daterange = $("#history-daterange-search").val();
                var $employees = $("#history-employee-search").val();
                var $discoveredVia = $("#history-discovered-search").val();
                var $rights = $("#history-rights-search").val();

                if ($keyword.length > 0)
                    $paramaters.keyword = $keyword;

                if ($daterange.length > 0)
                    $paramaters.daterange = $daterange;

                if ($employees != null && $employees.length > 0)
                    $paramaters.employees = $employees;

                if ($discoveredVia != null && $discoveredVia.length > 0)
                    $paramaters.discoveredVia = $discoveredVia;


                if ($rights != null && $rights.length > 0)
                    $paramaters.rights = $rights;

                return $.extend({}, d, $paramaters);
            }
        },
        "columns": [
            {
                name: "DateTime",
                data: "DateTime",
                orderable: true
            },
            {
                name: "User",
                data: "User",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = " <a href='my-contact-profile.php'>";
                    strStatus += "<div class='table-avatar mini pull-left' style='background-image: url(\"" + row.CreatorUri + "\");'></div>"
                    strStatus += "<div class='avatar-name pull-left' style='color: #333; line-height: 4; padding-left: 15px;'>" + row.User + "</div>";
                    strStatus += "<div class='clearfix'></div>";
                    strStatus += "</a>";
                    return strStatus;
                }
            },
            {
                name: "Country",
                data: "Country",
                orderable: true
            },
            {
                name: "Business",
                data: "Business",
                orderable: true
            },
            {
                name: "Discovered",
                data: "Discovered",
                orderable: true
            },
            {
                name: "Categories",
                data: "Categories",
                orderable: false
            },
            {
                name: null,
                data: null,
                orderable: false,
                //width: "100px",
                render: function (value, type, row) {
                    var str = "<td>";
                    if (row.IsApprovedForSubsDomain)
                        str += '<span class="label label-lg label-success">Subscription</span> ';
                    if (row.IsApprovedForCustomDomain)
                        str += '<span class="label label-lg label-info">Custom</span> ';
                    if (row.IsRejected)
                        str += '<span class="label label-lg label-danger">Reject</span> ';
                    str += "</td>";
                    return str;
                }
            }
        ],
        "order": [[0, "desc"]]
    });
};



$("#revoke-keyword").keyup(searchThrottle(function () {
    $('#table-waitlist-revoke').DataTable().ajax.reload();
}));
$("#revoke-employee-search,#revoke-discovered-search,#revoke-rights-search").on('change', function () {
    $('#table-waitlist-revoke').DataTable().ajax.reload();
});
function LoadWaitlistRevoke() {

    $("#table-waitlist-revoke").on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        //if (processing && $('.loadingoverlay').length === 0) {
        //    $('#table_show').LoadingOverlay("show", { minSize: "70x60px" });
        //} else {
        //    $('#table_show').LoadingOverlay("hide", true);
        //}
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
        "pageLength": 20,
        "ajax": {
            "url": '/Waitlist/GetDomainCreationRightsToRevoke',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                var $paramaters = {};
                var $keyword = $("#revoke-keyword").val();
                var $daterange = $("#revoke-daterange-search").val();
                var $employees = $("#revoke-employee-search").val();
                var $discoveredVia = $("#revoke-discovered-search").val();
                var $rights = $("#revoke-rights-search").val();

                if ($keyword.length > 0)
                    $paramaters.keyword = $keyword;

                if ($daterange.length > 0)
                    $paramaters.daterange = $daterange;

                if ($employees != null && $employees.length > 0)
                    $paramaters.employees = $employees;

                if ($discoveredVia != null && $discoveredVia.length > 0)
                    $paramaters.discoveredVia = $discoveredVia;


                if ($rights != null && $rights.length > 0)
                    $paramaters.rights = $rights;

                return $.extend({}, d, $paramaters);
            }
        },
        "columns": [
            {
                name: "DateTime",
                data: "DateTime",
                orderable: true
            },
            {
                name: "User",
                data: "User",
                orderable: true,
                render: function (value, type, row) {
                    var strStatus = " <a href='my-contact-profile.php'>";
                    strStatus += "<div class='table-avatar mini pull-left' style='background-image: url(\"" + row.CreatorUri + "\");'></div>"
                    strStatus += "<div class='avatar-name pull-left' style='color: #333; line-height: 4; padding-left: 15px;'>" + row.User + "</div>";
                    strStatus += "<div class='clearfix'></div>";
                    strStatus += "</a>";
                    return strStatus;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                //width: "100px",
                render: function (value, type, row) {
                    var str = "<td>";
                    if (row.IsApprovedForSubsDomain)
                        str += '<span class="label label-lg label-success">Subscription</span> ';
                    if (row.IsApprovedForCustomDomain)
                        str += '<span class="label label-lg label-info">Custom</span> ';
                    
                    str += "</td>";
                    return str;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var str = "<button type='button'onclick='RevokeWaitlist(" + row.Id + ")' class='btn btn-danger'>";
                    str += "<i class='fa fa-ban'></i> &nbsp; Revoke &nbsp; </button>";
                    return str;
                }
            }
        ],
        "order": [[0, "desc"]]
    });
};
function RevokeWaitlist(id) {
    if (confirm('Are you sure? This action cannot be undone.')) {
        $.ajax({
            'method': 'POST',
            'dataType': 'JSON',
            'data': {
                waitlistId: id
            },
            'url': '/Waitlist/RevokeWaitlist',
            'success': function (response) {
                if (response.result) {
                    cleanBookNotification.updateSuccess();
                    $("#table-waitlist-history").DataTable().ajax.reload();
                    $("#table-waitlist-revoke").DataTable().ajax.reload();
                } else {
                    cleanBookNotification.error(response.msg, 'Qbicles');
                }
            },
            'error': function (err) {
                cleanBookNotification.error(err.msg, 'Qbicles');
            }
        })
    }
};