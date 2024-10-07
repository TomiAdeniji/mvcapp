var isBusy = false;
var $_members = [];
var $_reviewers = [];
var $_approvers = [];
var loadCountActivity = 0;
var isFirstLoad = 0;
var previousShown = false;
var $wgId = 0;
$(document).ready(function () {
    timeline();
    initSearch();
    initFindBusinesses();
    $(".checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    var _dateFormat = $dateFormatByUser.toUpperCase();
    $('.daterange').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: _dateFormat
        }
    });
    $('.daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format(_dateFormat) + ' - ' + picker.endDate.format(_dateFormat));
        //var tabInvistions = $('#options-invitations ul.nav li.active a[href="#invitations-received"]');
        //if (tabInvistions.length > 0)
        //    reloadTblReceived();
        //else
        //    reloadTblSent();
    });
    $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        //var tabInvistions = $('#options-invitations ul.nav li.active a[href="#invitations-received"]');
        //if (tabInvistions.length > 0)
        //    reloadTblReceived();
        //else
        //    reloadTblSent();
    });
    //getCountNotifications();
    initPostStream();
    latchformobile();
    initFormB2bCreateOrder();
});
function loadWorkgroupContent() {
    setTimeout(function () {
        $('#config-2').LoadingOverlay('show');
        $('#config-2').load("/Commerce/LoadWorkgroupsContent", function () {
            initSelectModal('#config-2');
            initWorkgroupContent();
            $('#config-2').LoadingOverlay('hide');
        });
    }, 100);
}
function initWorkgroupContent() {
    $('#tbl-workgroups').DataTable({ "searching": false });
    $('#content-filters input[name="search"]').keyup(delay(function () {
        loadTableWorkgroups();
    }, 500));
    $('#content-filters select[name="location"],#content-filters select[name="process"]').change(function () {
        loadTableWorkgroups();
    });
}
function loadTableWorkgroups() {
    $('#wrapper-tbl').LoadingOverlay('show');
    var paramaters = {
        keyword: $('#content-filters input[name=search]').val(),
        lid: $('#content-filters select[name=location]').val(),
        process: $('#content-filters select[name=process]').val()
    };
    if (!paramaters.process)
        paramaters.process = [];
    $('#wrapper-tbl').load("/Commerce/LoadTableWorkgroups", paramaters, function () {
        $('#tbl-workgroups').DataTable({ "searching": false });
        $('#wrapper-tbl').LoadingOverlay('hide');
    });
}
function deleteWorkgroup(id) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Commerce",
        message: _L("ERROR_MSG_708"),
        callback: function (result) {
            if (result) {
                $.post("/Commerce/DeleteWorkgroup", { id: id }, function (response) {
                    if (response.result) {
                        loadTableWorkgroups();
                        if (response.Object.refresh) {
                            location.reload();
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
function loadWorkgroupModal(id) {
    $('#app-b2b-workgroup-add').empty();
    $('#app-b2b-workgroup-add').modal('show');
    $('#app-b2b-workgroup-add').load("/Commerce/LoadWorkgroupModal?id=" + (id ? id : 0), function () {
        initWorkgroup();
        initSelectModal('#app-b2b-workgroup-add');
    });
}
function loadRelationshipContent() {
    setTimeout(function () {
        $('#config-3').LoadingOverlay('show');
        $('#config-3').load("/Commerce/LoadRelationshipContent", function () {
            initSelectModal('#config-3');
            $('#config-3').LoadingOverlay('hide');
            $('#tblRelationships').DataTable();
            $('#txtSearchFilter').keyup(delay(function () {
                $('#tblRelationships').DataTable().search($(this).val()).draw();
            }, 500));
            $('#slRelationshipManagers').change(function () {
                var keyword = $(this).val();
                $('#tblRelationships').DataTable().search((keyword != "0" ? keyword : "")).draw();
            });
        });
    }, 100);
}
function initSelectModal(idparent) {
    $(idparent + " .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $(idparent + ' .select2').select2({ placeholder: "Please select" });
    $(idparent + ' input[data-toggle="toggle"]').bootstrapToggle();
}
function initWorkgroup() {
    $('#tblmembers').DataTable({
        "destroy": true,
        "autoWidth": false,
        "columns": [
            {
                "data": "img_url",
                "render": function (data, type, row, meta) {
                    return '<div class="table-avatar mini" style="background-image: url(\'' + data + '\');">&nbsp;</div></td>';
                }
            },
            { "data": "name" },
            {
                "data": "id",
                "render": function (data, type, row, meta) {
                    return '<input id="approval-user-id-' + data + '" class="trackInputapproval" data-toggle="toggle" data-onstyle="success" data-on="<i class=\'fa fa-check\'></i>" data-off=" " ' + (row.val_approver == 'Approver' ? 'checked' : "") + ' type="checkbox">';
                }
            },
            {
                "data": "id",
                "render": function (data, type, row, meta) {
                    return '<input id="reviewer-user-id-' + data + '" class="trackInputreviewer" data-toggle="toggle" data-onstyle="success" data-on="<i class=\'fa fa-check\'></i>" data-off=" " ' + (row.val_reviewer == 'Reviewer' ? 'checked' : "") + ' type="checkbox">';
                }
            },
            {
                "data": "id",
                "render": function (data, type, row, meta) {
                    return '<button type="button" class="btn btn-danger" onclick="removeRowTableMemberAdd(\'' + data + '\')"><i class="fa fa-trash"></i></button>';
                }
            },
            { "data": "val_approver", "visible": false, "searchable": true },
            { "data": "val_reviewer", "visible": false, "searchable": true },
        ],
        rowId: function (a) {
            return 'tr_user_' + a.id;
        },
        drawCallback: function (settings) {
            $(".trackInputapproval").on("change", function () {
                var $row = $(this).parents("tr");
                var rowData = $('#tblmembers').DataTable().row($row).data();
                if ($(this).prop('checked'))
                    rowData.val_approver = 'Approver';
                else
                    rowData.val_approver = '';
            })
            $(".trackInputreviewer").on("change", function () {
                var $row = $(this).parents("tr");
                var rowData = $('#tblmembers').DataTable().row($row).data();
                if ($(this).prop('checked'))
                    rowData.val_reviewer = 'Reviewer';
                else
                    rowData.val_reviewer = '';
            })
        }
    });
    var $frmb2bworkgroup = $('#frmb2bworkgroup');
    $frmb2bworkgroup.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 150,
                minlength: 3
            },
            Location: {
                required: true
            },
            Process: {
                required: true
            }
        },
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
        }
    });
    $frmb2bworkgroup.submit(function (e) {
        e.preventDefault();
        if (isBusy)
            return;
        if ($frmb2bworkgroup.valid()) {
            $.LoadingOverlay("show");
            getMembers();
            var process = [];
            var dataProcess = $('#frmb2bworkgroup select[name="Process"]').val();
            $.each(dataProcess, function (key, value) {
                process.push({ Id: value });
            });
            var members = [], reviewers = [], approvers = [];
            $.each($_members, function (key, value) {
                members.push({ Id: value.Id });
                if (value.IsApprover)
                    approvers.push({ Id: value.Id });
                if (value.IsReviewer)
                    reviewers.push({ Id: value.Id });
            });
            var wg = {
                Id: $('#frmb2bworkgroup input[name="Id"]').val(),
                Name: $('#frmb2bworkgroup input[name="Name"]').val(),
                Location: { Id: $('#frmb2bworkgroup select[name="Location"]').val() },
                SourceQbicle: { Id: $('#frmb2bworkgroup select[name="Qbicle"]').val() },
                DefaultTopic: { Id: $('#frmb2bworkgroup select[name="Topic"]').val() },
                Processes: process,
                Members: members,
                Reviewers: reviewers,
                Approvers: approvers
            };

            $.ajax({
                type: "post",
                url: "/Commerce/SaveWorgroup",
                datatype: "json",
                data: {
                    model: wg
                },
                success: function (refModel) {
                    if (refModel.result) {
                        $('#app-b2b-workgroup-add').modal('hide');
                        if (wg.Id == "0") {
                            cleanBookNotification.createSuccess();
                        } else {
                            cleanBookNotification.updateSuccess();
                        }
                        loadTableWorkgroups();
                        if (refModel.Object.refresh) {
                            location.reload();
                        }
                    } else if (!refModel.result && refModel.msg) {
                        cleanBookNotification.error(refModel.msg, "Commerce");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
                    }
                    $.LoadingOverlay("hide");
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                    $.LoadingOverlay("hide");
                }
            });
        }
    });
    initNextPreviousTab('#frmb2bworkgroup', '#tabb2bworkgroup');
    $('#txt-member-search').keyup(delay(function () {
        $('#tblmembers').DataTable().columns(1).search($(this).val()).draw();
    }, 500));
    $('#slTypeProcess').change(function () {
        var keyword = $(this).val();
        $('#tblmembers').DataTable().search((keyword == "0" ? "" : keyword)).draw();
    });
    $('#tblmembers_filter').hide();
}
function initNextPreviousTab(frmId, tabId) {
    $(frmId + ' .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').next('li').find('a').trigger('click');
    });

    $(frmId + ' .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').prev('li').find('a').trigger('click');
    });
}
function showMembersChoose() {
    getMembers();
    var rows = $('#user-list').DataTable().rows().data();
    $.each(rows, function (key, value) {
        var id = value[0]["@data-search"];
        if ($_members.filter(e => e.Id == id).length > 0)
            $('.user_' + id).bootstrapToggle('on');
        else
            $('.user_' + id).bootstrapToggle('off');
    });
    $('#app-people-select').modal('show');
}
function addUsersToMembers(checked, id, fullname, url) {
    if (checked) {
        if ($_members.filter(e => e.Id == id).length == 0)
            $_members.push({
                Id: id,
                Fullname: fullname,
                Url: url
            });
    } else {
        $_members = $_members.filter(function (el) { return el.Id != id; });
    }
}
function addMemberToWorkgroup() {
    var table = $('#tblmembers').DataTable();
    //Add items
    $.each($_members, function (key, value) {
        var row = table.rows('#tr_user_' + value.Id).data();
        if (row.length == 0) {
            table.row.add({
                id: value.Id,
                img_url: value.Url + '&size=T',
                name: value.Fullname,
                val_approver: '',
                val_reviewer: '',
            }).draw();
            $('#tr_user_' + value.Id + ' input[data-toggle="toggle"]').bootstrapToggle();

        }
    });
    //Remove items
    var rows = table.rows().data();
    $.each(rows, function (key, value) {
        var id = value.id
        if ($_members.filter(e => e.Id == (id ? id : "")).length == 0) {
            table.row('#tr_user_' + id).remove().draw();
        }
    });
}
function removeRowTableMemberAdd(id) {
    var table = $('#tblmembers').DataTable();
    table.row('#tr_user_' + id).remove().draw();
}
function getMembers() {
    $_members = [];
    var table = $('#tblmembers').DataTable();
    var dataMembers = table.rows().data();
    $.each(dataMembers, function (key, value) {
        $_members.push({
            Id: value.id,
            IsApprover: $('#approval-user-id-' + value.id).prop('checked'),
            IsReviewer: $('#reviewer-user-id-' + value.id).prop('checked'),
        });
    });
}
function initSearch() {
    //$('#search-member-all').keyup(delay(function () {
    //    $('#user-list').DataTable().columns(1).search($(this).val()).draw();
    //}, 500));
    //$('#search-right-all').change(function () {
    //    var keyword = $(this).val();
    //    $('#user-list').DataTable().search((keyword ? keyword : "")).draw();
    //});
    //$('#user-list_filter').hide();
    $('#options-comms input[name=search]').keyup(delay(function () {
        loadRelationships();
    }, 400));
    $('#txtbKeyword').keyup(delay(function () {
        initFindBusinesses();
    }, 400));
    $('#slbServices').change(function () {
        initFindBusinesses();
    });
    //$('#options-partners input[name=search]').keyup(delay(function () {
    //    $('#tblRelationships').DataTable().search($(this).val()).draw();
    //}, 500));
    //$('#options-partners select[name=partnership]').change(function () {
    //    var keyword = $(this).val();
    //    $('#tblRelationships').DataTable().columns(1).search(keyword == '0' ? '' : keyword).draw();
    //});
    //$('#options-partners select[name=managers]').change(function () {
    //    var whatsSelected = $(this).val() ? $(this).val() : [];
    //    whatsSelected = whatsSelected.join('|');
    //    $('#tblRelationships').DataTable().columns(2).search(whatsSelected, true, false).draw();
    //});
    //$('input[name=catalogue-filter-search]').keyup(delay(function () {
    //    reloadTblCatalogueItems();
    //}, 400));
    //$('select[name=catalogue-filter-group]').change(function () {
    //    reloadTblCatalogueItems();
    //});
}
function reloadDefaultTopic(currentTopicId) {
    $('#wg-topic').empty();
    var qbicleId = $('#wg-qbicle').val();
    $.getJSON('/Topics/GetTopicByQbicleId', { qbicleId: (qbicleId ? qbicleId : 0), currentTopicId: currentTopicId }, function (result) {
        $('#wg-topic').select2({
            placeholder: "Please select",
            data: result
        });
    });
}
//function loadBusinessesContent() {
//    $(window).scrollTop(0);
//    setTimeout(function () {
//        $('#businesses').LoadingOverlay('show');
//        var services = $('#slbServices').val();
//        $('#businesses').load("/Commerce/LoadBusinessesContent", { keyword: $('#txtbKeyword').val(), locationId: $('#slLocation').val(), services: (services ? services : []) }, function () {
//            $('#businesses').LoadingOverlay('hide');
//        });
//    }, 100);
//}
function initFindBusinesses() {
    var $data_container_business = $('#data-container-businesses');
    var $pagination_container = $('#pagiation-businesses');
    var services = $('#slbServices').val();
    $pagination_container.pagination({
        dataSource: '/Commerce/LoadBusinessesContent',
        locator: 'items',
        totalNumberLocator: function (response) {
            $data_container_business.LoadingOverlay('hide');
            // you can return totalNumber by analyzing response content
            return response.totalNumber;
        },
        pageSize: 8,
        ajax: {
            data: { keyword: $('#txtbKeyword').val(), locationId: $('#slLocation').val(), services: (services ? services : []) },
            beforeSend: function () {
                $data_container_business.LoadingOverlay('show');
            }
        },
        callback: function (data, pagination) {
            // template method of yourself
            var count = data.length;
            var extraCol = (count % 4 == 0 ? 0 : 4) - count % 4;
            var uri = $('#api-uri').val();
            var dataHtml = '';
            $.each(data, function (index, item) {
                dataHtml += businessTemplate(item, uri);
            });
            for (var i = 0; i < extraCol; i++) {
                dataHtml += '<article class="col" style="box-shadow: none; background: transparent;"></article>';
            }
            $data_container_business.html(dataHtml);
        }
    })
}
function businessTemplate(data, uri) {
    var _html = '<article class="col"><span class="last-updated">Logistics</span>';
    _html += '<a href="/Commerce/PublishBusinessProfile?id=' + data.Id + '"><div class="avatar" style="background-image: url(\'' + uri + data.LogoUri + '&size=T\');">&nbsp;</div>';
    _html += '<h1 style="color: #333;">' + data.BusinessName + '</h1>';
    _html += '</a>';
    _html += '<p class="qbicle-detail">' + data.BusinessSummary + '</p>'
    _html += '<div class="row" style="padding: 20px 20px 0 20px;">';
    if (data.HasB2BDefaultManager) {
        _html += '<a href="#" onclick="connectB2B(' + data.DomainId + ',\'' + fixQuoteCode(data.BusinessName) + '\')" class="btn btn-info community-button">Connect</a>';
    }
    _html += '</div></article>';
    return _html;
}
function connectB2B(uId, fullname) {
    $.LoadingOverlay('show');
    $.post("/Commerce/ConnectB2B", { partnerDomainId: uId}, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            _c2cQbiceId = response.Object;
            cleanBookNotification.success(_L("CONNECTED_SUCCESS", [fullname]), "Community");
            loadRelationships();
            initFindBusinesses();
            $('ul.subapps-nav a[href="#comms"]').removeClass("disabled");
            $('ul.subapps-nav a[href="#comms"]').click();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function loadRelationships() {
    setTimeout(function () {
        var $boxrl = $('#box-relationships');
        var keyword = $('#options-comms input[name=search]').val();
        $boxrl.LoadingOverlay('show');
        $boxrl.load("/Commerce/LoadRelationshipQbicles", { keyword: keyword}, function () {
            $('#box-relationships').LoadingOverlay('hide');
        });
    }, 100);
}
//function loadRelationshipsContent() {
//    $(window).scrollTop(0);
//    setTimeout(function () {
//        $('#partners').LoadingOverlay('show');
//        $('#partners').load("/Commerce/LoadRelationshipsContent", function () {
//            $('#tblRelationships').DataTable();
//            $('#tblRelationships_filter').hide();
//            $('#partners').LoadingOverlay('hide');
//        });
//    }, 100);
//}
//function initInvistions() {
//    var $tblReceived = $('#tblReceived');
//    $tblReceived.on('processing.dt', function (e, settings, processing) {
//        $('#processingIndicator').css('display', 'none');
//        if (processing) {
//            $tblReceived.LoadingOverlay("show");
//        } else {
//            $tblReceived.LoadingOverlay("hide", true);
//        }
//    }).dataTable({
//        destroy: true,
//        serverSide: true,
//        paging: true,
//        searching: false,
//        autoWidth: true,
//        pageLength: 10,
//        deferLoading: 30,
//        order: [[2, "desc"]],
//        ajax: {
//            "url": "/Commerce/GetB2bNotifications",
//            "data": function (d) {
//                return $.extend({}, d, {
//                    "isReceived": true,
//                    "keyword": $('#txt-search-received').val(),
//                    "status": $('#sl-status-received').val(),
//                    "dateRangeFilter": $('#txt-daterange-received').val()
//                });
//            }
//        },
//        columns: [
//            {
//                "data": "Business",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlBusiness = '<div class="avatarwtitle"><div class="theicon" style="background-image: url(\'' + row.BusinessLogoUri + '\');"></div>';
//                    _htmlBusiness += '<div class="thetitle">' + fixQuoteCode(data) + '</div></div>';
//                    return _htmlBusiness;
//                }
//            },
//            { "data": "Service", "orderable": true },
//            { "data": "Received", "orderable": true },
//            {
//                "data": "IssuedBy",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlIssuedBy = '<a href="/Community/UserProfilePage?uId=' + row.IssuedById + '">' + fixQuoteCode(data) + '</a>';
//                    return _htmlIssuedBy;
//                }
//            },
//            {
//                "data": "Status",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlStatus = '<label class="label label-lg label-warning">Pending</label>';
//                    if (data == 'Accepted')
//                        _htmlStatus = '<label class="label label-lg label-success">Accepted</label>';
//                    else if (data == 'Rejected')
//                        _htmlStatus = '<label class="label label-lg label-danger">Rejected</label>';
//                    return _htmlStatus;
//                }
//            },
//            {
//                "data": "Id",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlOptions = '';
//                    if (row.Service == 'Relationship') {
//                        _htmlOptions = '<div class="btn-group options">';
//                        if (row.Status == "Pending") {
//                            _htmlOptions += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> &nbsp; Options</button><ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="#" onclick="showModalAcceptRelationship(' + data + ',\'' + fixQuoteCode(row.Business) + '\');">Accept</a></li><li><a href="#" onclick="acceptRelationship(' + data + ',3)">Reject</a></li></ul>';
//                        } else {
//                            _htmlOptions += '<button type="button" disabled class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> &nbsp; Options</button>';
//                        }
//                        _htmlOptions += '</div>';
//                    } else if (row.Service == 'Logistics') {
//                        _htmlOptions = '<div class="btn-group options">';
//                        if (row.Status == "Pending") {
//                            _htmlOptions += '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> &nbsp; Options</button><ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="#" onclick="loadChargeFrameworkSelectionModal(' + data + ');">Accept</a></li><li><a href="#" onclick="rejectLogistics(' + data + ')">Reject</a></li></ul>';
//                        } else {
//                            _htmlOptions += '<button type="button" disabled class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-cog"></i> &nbsp; Options</button>';
//                        }
//                    } else if (row.Service == "Logistics partnership") {
//                        if (row.Status == "Rejected")
//                            _htmlOptions = '<button class="btn btn-info animated bounce" disabled><i class="fa fa-eye"></i> &nbsp; Review proposal</button>';
//                        else
//                            _htmlOptions = '<button class="btn btn-info animated bounce" onclick="window.location.href=\'/Commerce/B2BLogisticsproposal?invitationId=' + data + '\'"><i class="fa fa-eye"></i> &nbsp; Review proposal</button>';
//                    }
//                    return _htmlOptions;
//                }
//            }
//        ]
//    });
//    var $tblSent = $('#tblSent');
//    $tblSent.on('processing.dt', function (e, settings, processing) {
//        $('#processingIndicator').css('display', 'none');
//        if (processing) {
//            $tblSent.LoadingOverlay("show");
//        } else {
//            $tblSent.LoadingOverlay("hide", true);
//        }
//    }).dataTable({
//        destroy: true,
//        serverSide: true,
//        paging: true,
//        searching: false,
//        autoWidth: true,
//        pageLength: 10,
//        deferLoading: 30,
//        order: [[2, "desc"]],
//        ajax: {
//            "url": "/Commerce/GetB2bNotifications",
//            "data": function (d) {
//                return $.extend({}, d, {
//                    "isReceived": false,
//                    "keyword": $('#txt-search-sent').val(),
//                    "status": $('#sl-search-sent').val(),
//                    "dateRangeFilter": $('#txt-daterange-sent').val()
//                });
//            }
//        },
//        columns: [
//            {
//                "data": "Business",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlBusiness = '<div class="avatarwtitle"><div class="theicon" style="background-image: url(\'' + row.BusinessLogoUri + '\');"></div>';
//                    _htmlBusiness += '<div class="thetitle">' + fixQuoteCode(data) + '</div></div>';
//                    return _htmlBusiness;
//                }
//            },
//            { "data": "Service", "orderable": true },
//            { "data": "Received", "orderable": true },
//            {
//                "data": "IssuedBy",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlIssuedBy = '<a href="/Community/UserProfilePage?uId=' + row.IssuedById + '">' + fixQuoteCode(data) + '</a>';
//                    return _htmlIssuedBy;
//                }
//            },
//            {
//                "data": "Status",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlStatus = '<label class="label label-lg label-warning">Pending</label>';
//                    if (data == 'Accepted')
//                        _htmlStatus = '<label class="label label-lg label-success">Accepted</label>';
//                    else if (data == 'Rejected')
//                        _htmlStatus = '<label class="label label-lg label-danger">Rejected</label>';
//                    return _htmlStatus;
//                }
//            },
//            {
//                "data": "Id",
//                "orderable": true,
//                "render": function (data, type, row, meta) {
//                    var _htmlOptions = '';
//                    if (row.Status == 'Pending')
//                        _htmlOptions += '<button type="button" class="btn btn-danger" onclick="deleteRelationship(' + data + ')"><i class="fa fa-trash"></i> &nbsp; Cancel</button>';
//                    else
//                        _htmlOptions += '<button type="button" disabled class="btn btn-danger"><i class="fa fa-trash"></i> &nbsp; Cancel</button>';
//                    return _htmlOptions;
//                }
//            }
//        ]
//    });
//    initSearchInvistions();
//}
//function initSearchInvistions() {
//    $('#txt-search-sent').keyup(delay(function () {
//        reloadTblSent();
//    }, 500));
//    $('#sl-search-sent').change(function () {
//        reloadTblSent();
//    });
//    $('#txt-search-received').keyup(delay(function () {
//        reloadTblReceived();
//    }, 500));
//    $('#sl-status-received').change(function () {
//        reloadTblReceived();
//    });
//}
//function reloadTblReceived() {
//    if ($.fn.DataTable.isDataTable('#tblReceived')) {
//        $('#tblReceived').DataTable().ajax.reload();
//    } else {
//        setTimeout(function () {
//            $('#tblReceived').DataTable().ajax.reload();
//        }, 1500);
//    }
//    getCountNotifications();
//}
//function reloadTblSent() {
//    if ($.fn.DataTable.isDataTable('#tblSent')) {
//        $('#tblSent').DataTable().ajax.reload();
//    } else {
//        setTimeout(function () {
//            $('#tblSent').DataTable().ajax.reload();
//        }, 1500);
//    }
//}
//function acceptRelationship(ivId, status) {
//    var members = $('#slacceptassignee').val();
//    $.post("/Commerce/UpdateRelationshipStatus", { invistionId: ivId, status: status, members: (members ? members : []) }, function (response) {
//        if (response.result) {
//            $('#b2b-accept-partnership').modal('hide');
//            reloadTblReceived();
//            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
//        } else if (!response.result && response.msg) {
//            cleanBookNotification.error(_L(response.msg), "Commerce");
//        }
//        else {
//            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//        }
//    });
//}
//function loadRelationshipManagersModal(id) {
//    $('#b2b-form-relationship').empty();
//    $('#b2b-form-relationship').modal('show');
//    $('#b2b-form-relationship').load("/Commerce/LoadRelationshipManagersModal?id=" + (id ? id : 0), function () {
//        initSelectModal('#b2b-form-relationship');
//    });
//}
//function loadChargeFrameworkSelectionModal(id) {
//    $('#b2b-charge-framework-selection').empty();
//    $('#b2b-charge-framework-selection').modal('show');
//    $('#b2b-charge-framework-selection').load("/Commerce/LoadChargeFrameworkSelectionModal?id=" + (id ? id : 0), function () {
//        initSelectModal('#b2b-charge-framework-selection');
//        var $frmInvitationChargeFramework = $("#frmInvitationChargeFramework");
//        $frmInvitationChargeFramework.validate({
//            ignore: "",
//            rules: {
//                "InvitationId": {
//                    required: true
//                }
//            },
//            invalidHandler: function (e, validator) {
//                if (validator.errorList.length)
//                    $('a[href="#' + jQuery(validator.errorList[0].element).closest(".tab-pane").attr('id') + '"]').tab('show');
//            }
//        });
//        $frmInvitationChargeFramework.submit(function (e) {
//            e.preventDefault();
//            if (isBusy)
//                return;
//            if ($frmInvitationChargeFramework.valid()) {
//                $.LoadingOverlay("show");
//                reIndexElmProvider();
//                $.ajax({
//                    type: this.method,
//                    cache: false,
//                    url: this.action,
//                    data: $(this).serialize(),
//                    dataType: "json",
//                    beforeSend: function (xhr) {
//                        isBusy = true;
//                    },
//                    success: function (data) {
//                        if (data.result) {
//                            $('#b2b-charge-framework-selection').modal('hide');
//                            reloadTblReceived();
//                            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
//                        } else if (!data.result && data.msg) {
//                            cleanBookNotification.error(data.msg, "Trader");
//                        } else {
//                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
//                        }
//                        isBusy = false;
//                        LoadingOverlayEnd();
//                    },
//                    error: function (data) {
//                        isBusy = false;
//                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
//                        LoadingOverlayEnd();
//                    }
//                });
//            }
//        });
//        initNextPreviousTab('#frmInvitationChargeFramework', '#tabConsumer');
//    });
//}
//function updateRelationshipManagers(id) {
//    $.LoadingOverlay('show');
//    var members = $('#slassignee').val();
//    $.post("/Commerce/UpdateMembersRelationship", { relationshipId: id, members: (members ? members : []) }, function (response) {
//        $.LoadingOverlay('hide');
//        if (response.result) {
//            $('#b2b-form-relationship').modal('hide');
//            loadRelationshipContent();
//            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
//        } else if (!response.result && response.msg) {
//            cleanBookNotification.error(_L(response.msg), "Commerce");
//        }
//        else {
//            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//        }
//    });
//}
//function deleteRelationship(id) {
//    bootbox.confirm({
//        show: true,
//        backdrop: true,
//        closeButton: true,
//        animate: true,
//        title: "Commerce",
//        message: _L("ERROR_MSG_708"),
//        callback: function (result) {
//            if (result) {
//                $.post("/Commerce/DeleteRelationship", { invistionId: id }, function (response) {
//                    if (response.result) {
//                        reloadTblSent();
//                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
//                    } else if (!response.result && response.msg) {
//                        cleanBookNotification.error(_L(response.msg), "Commerce");
//                    }
//                    else {
//                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//                    }
//                });
//                return;
//            }
//        }
//    });
//}
//function getCountNotifications() {
//    $.get("/Commerce/GetCountNotifications", function (response) {
//        if (response > 0)
//            $('.count_notifications').text(response);
//        else
//            $('.count_notifications').text('');
//    });
//}
//function tabTblSentEventClick() {
//    $('#options-invitations ul.nav a[href="#invitations-sent"]').click();
//}
//function showModalAcceptRelationship(ivId, partnershipname) {
//    $('.partnership-request').text(partnershipname);
//    $('#hdfIvId').val(ivId);
//    $('#b2b-accept-partnership').modal('show');
//}
//function loadPriceListByLocationId($elm, rowindex) {
//    $('.priceloc_row' + rowindex).fadeIn();
//    $elm.valid();
//    var $elPricesList = $('.priceloc_row' + rowindex + ' select');
//    $.get("/Commerce/GetPricelistByLocationId?lid=" + $elm.val(), function (data) {
//        $elPricesList.select2('destroy');
//        $elPricesList.empty();
//        $elPricesList.select2({
//            placeholder: "Please select",
//            data: data
//        });
//    });
//}
//function addProposedProvider($elmPrepend, invitationId) {
//    var rowindex = getMaxIndexElmProvider() + 1;
//    $.get("/Commerce/ElementProposedProvider", { invitationId: invitationId, rowindex: rowindex }, function (response) {
//        $elmPrepend.before(response);
//    });
//}
//function rejectLogistics(invitationId) {
//    $.post("/Commerce/SaveInviLogisticsPartnershipReject", { invitationId: invitationId }, function (response) {
//        if (response.result) {
//            reloadTblReceived();
//            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
//        } else if (!response.result && response.msg) {
//            cleanBookNotification.error(_L(response.msg), "Commerce");
//        }
//        else {
//            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//        }
//    });
//}
//function endRelationship(id) {
//    bootbox.confirm({
//        show: true,
//        backdrop: true,
//        closeButton: true,
//        animate: true,
//        title: "Commerce",
//        message: _L("CONFIRM_MSG_ENDRELATIONSHIP"),
//        callback: function (result) {
//            if (result) {
//                $.post("/Commerce/EndRelationship", { relationshipId: id }, function (response) {
//                    if (response.result) {
//                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Commerce");
//                        loadRelationshipsContent();
//                    } else if (!response.result && response.msg) {
//                        cleanBookNotification.error(_L(response.msg), "Commerce");
//                    }
//                    else {
//                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//                    }
//                });
//                return;
//            }
//        }
//    });
//}
//function getMaxIndexElmProvider() {
//    var max = 0;
//    $('.proposed-provider').each(function () {
//        var value = parseInt($(this).data('index'));
//        max = (value > max) ? value : max;
//    });
//    return max;
//}
//function reIndexElmProvider() {
//    $('.proposed-provider').each(function (index) {
//        var oldIndex = parseInt($(this).data('index'));
//        $(this).data("index", index);
//        var $lblErrorProvider = $(this).find('label.error');
//        var $selectProvider = $(this).find('.proposedloc select');
//        var $divPriceLoc = $(this).find('.priceloc_row' + oldIndex);
//        var $selectPricelist = $(this).find('select.pricelistbyloc');
//        $lblErrorProvider.attr("id", "ProposedList[" + index + "].ProviderId-error");
//        $lblErrorProvider.attr("for", "ProposedList[" + index + "].ProviderId");
//        $selectProvider.attr("name", "ProposedList[" + index + "].ProviderId");
//        $selectProvider.attr("onchange", "loadPriceListByLocationId($(this)," + index + ");");
//        $divPriceLoc.removeClass('.priceloc_row' + oldIndex).addClass('.priceloc_row' + index);
//        $selectPricelist.attr("name", "ProposedList[" + index + "].PricelisId");
//    });
//}
//function loadCatalogueItemModal() {
//    $('#b2b-catalogue-item-add').empty();
//    $('#b2b-catalogue-item-add').modal('show');
//    $('#b2b-catalogue-item-add').load("/Commerce/LoadCatalogueItemModal", function () {
//        initSelectModal('#b2b-catalogue-item-add');
//        $('#select-traderitems').select2({
//            ajax: {
//                url: '/Commerce/Select2TraderItemsByDomainId',
//                delay: 250,
//                data: function (params) {
//                    var query = {
//                        keyword: params.term,
//                        page: params.page || 1
//                    }
//                    return query;
//                },
//                cache: true
//            },
//            minimumInputLength: 1
//        });
//        $('#select-traderitems').on("select2:select", function (e) {
//            var data = e.params.data;
//            $.get("/Commerce/ItemSelectedById?id=" + data.id, function (response) {
//                $('#itemunit').empty();
//                $('#itemunit').select2({
//                    data: (response.Units ? response.Units : [])
//                });
//                $('#itemunit').on("select2:select", function (e) {
//                    var unit = e.params.data;
//                    $('.cell_itemunit').text(unit.text);
//                    $('#itemunit').valid();
//                });
//                $('#item-locations').empty();
//                if (response.Locations && response.Locations.length > 0) {
//                    response.Locations.forEach(function (item) {
//                        $("#item-locations").append('<option value="' + item.id + '">' + fixQuoteCode(item.text) + '</option>');
//                    });
//                }
//                $('#item-locations').multiselect('destroy');
//                $("#item-locations").multiselect({
//                    includeSelectAllOption: false,
//                    enableFiltering: false,
//                    buttonWidth: '100%',
//                    maxHeight: 400,
//                    enableClickableOptGroups: true
//                });
//                //Load datatable item
//                var _htmlTaxRates = '<ul class="unstyled" style="padding-left: 0; margin-left: 0; margin-bottom: 0;">';
//                if (response.StringTaxRates && response.StringTaxRates.length>0) {
//                    response.StringTaxRates.forEach(function (item) {
//                        _htmlTaxRates += '<li>' + item + '</li>';
//                    });
//                } else {
//                    _htmlTaxRates += '<li>Tax free</li>';
//                }
//                _htmlTaxRates += '</ul>';
//                $('.cell_taxrates').html(_htmlTaxRates);
//                $('.cell_itemname').text(response.ItemName);
//                $('.txt_itemname').val(response.ItemName);
//                $('.txt_itemname').valid();
//                var unitdefault = response.Units.find(element => element.selected);
//                $('.cell_itemunit').text(unitdefault ? unitdefault.text : '');
//                if (unitdefault)
//                    $('#itemunit').valid();
//                //end
//            });
//        });
//        initFormCatalogueItemAdd();
//    });
//}
//function initCatalogueItems() {
//    var $tblCatalogueItems = $('#tblCatalogueItems');
//    $tblCatalogueItems.on('processing.dt', function (e, settings, processing) {
//        $('#processingIndicator').css('display', 'none');
//        if (processing) {
//            $tblCatalogueItems.LoadingOverlay("show");
//        } else {
//            $tblCatalogueItems.LoadingOverlay("hide", true);
//        }
//    }).dataTable({
//        destroy: true,
//        serverSide: true,
//        paging: true,
//        searching: false,
//        autoWidth: true,
//        pageLength: 10,
//        deferLoading: 30,
//        order: [[0, "asc"]],
//        ajax: {
//            "url": "/Commerce/GetCatalogueItems",
//            "data": function (d) {
//                return $.extend({}, d, {
//                    "keyword": $('input[name="catalogue-filter-search"]').val(),
//                    "itemgroupId": $('select[name="catalogue-filter-group"]').val(),
//                });
//            }
//        },
//        columns: [
//            { "data": "Item","orderable": true},
//            { "data": "SKU", "orderable": true },
//            { "data": "ProductGroup", "orderable": true },
//            { "data": "Unit","orderable": true},
//            { "data": "Locations","orderable": false},
//            {
//                "data": "Id",
//                "orderable": false,
//                "render": function (data, type, row, meta) {
//                    var _htmlOptions = '<div class="btn-group options">';
//                    _htmlOptions+='<button type="button" class="btn btn-success dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
//                    _htmlOptions+='Options &nbsp; <i class="fa fa-angle-down"></i></button>';
//                    _htmlOptions += '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;"><li><a href="javascript:removeCatalogueItem(' + data + ')">Remove from catalogue</a></li></ul>';
//                    _htmlOptions += '</div>';
//                    return _htmlOptions;
//                }
//            }
//        ]
//    });
//}
//function reloadTblCatalogueItems() {
//    if ($.fn.DataTable.isDataTable('#tblCatalogueItems')) {
//        $('#tblCatalogueItems').DataTable().ajax.reload();
//    } else {
//        setTimeout(function () {
//            $('#tblCatalogueItems').DataTable().ajax.reload();
//        }, 1500);
//    }
//}
//function initFormCatalogueItemAdd() {
//    var $frmcatalogueitem = $('#frmcatalogueitem');
//    $frmcatalogueitem.validate({
//        ignore: "",
//        rules: {
//            item: {required: true},
//            tradname: {required: true},
//            locs: {required: true},
//            unit: {required: true}
//        }
//    });
//    $frmcatalogueitem.submit(function (e) {
//        e.preventDefault();
//        if (isBusy)
//            return;
//        if ($frmcatalogueitem.valid()) {
//            $.LoadingOverlay("show");
//            var providerLocations = [];
//            var locations = $('#item-locations').val();
//            $.each(locations, function (key, value) {
//                providerLocations.push({ Id: value });
//            });
//            var b2BCatalogItem = {
//                Item: { Id: $('#select-traderitems').val() },
//                ProviderLocations: providerLocations,
//                ProviderUnit: { Id: $('#itemunit').val() }
//            };
//            var startingVisibility=$('#frmcatalogueitem select[name=visibility]').val()
//            $.ajax({
//                type: this.method,
//                url: this.action,
//                datatype: "json",
//                data: {
//                    model: b2BCatalogItem,
//                    tradingName: $('#frmcatalogueitem input[name=tradname]').val(),
//                    IsShown: (startingVisibility==0?false:true)
//                },
//                success: function (refModel) {
//                    if (refModel.result) {
//                        $('#b2b-catalogue-item-add').modal('hide');
//                        cleanBookNotification.createSuccess();
//                        reloadTblCatalogueItems();
//                    } else if (!refModel.result && refModel.msg) {
//                        cleanBookNotification.error(refModel.msg, "Commerce");
//                    } else {
//                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//                    }
//                    $.LoadingOverlay("hide");
//                },
//                error: function (xhr) {
//                    cleanBookNotification.error(xhr.responseText, "Qbicles");
//                    $.LoadingOverlay("hide");
//                }
//            });
//        }
//    });
//}
//function initProductGroups() {
//    $.get("/Commerce/Select2GroupsByDomainId", function (groups) {
//        groups.unshift({ id: 0, text: "Show all" });
//        $('select[name=catalogue-filter-group]').empty();
//        $('select[name=catalogue-filter-group]').select2({
//            data: (groups ? groups : [])
//        });
//    });   
//}
//function removeCatalogueItem(id) {
//    bootbox.confirm({
//        show: true,
//        backdrop: true,
//        closeButton: true,
//        animate: true,
//        title: "Commerce",
//        message: _L("ERROR_MSG_708"),
//        callback: function (result) {
//            if (result) {
//                $.post("/Commerce/RemoveCatalogueItemById", { id: id }, function (response) {
//                    if (response.result) {
//                        reloadTblCatalogueItems();
//                    } else if (!response.result && response.msg) {
//                        cleanBookNotification.error(_L(response.msg), "Commerce");
//                    } else {
//                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Commerce");
//                    }
//                });
//                return;
//            }
//        }
//    });
//}
//B2bQbicles Stream
function timeline() {
    $(window).scrollTop(0);
    $(window).scroll(function () {
        if ($(window).scrollTop() >= ($(document).height() - $(window).height() - 100) && $("ul.subapps-nav li.active.qbicle-home").length > 0) {
            if (previousShown == false) {
                loadingNewCommerce();
                previousShown = true;
                return previousShown;
            }
        }
    });
}
function loadMoreActivitiesCommerce(isFilter) {
    if (isBusy) {
        return;
    }
    var _activityTypes = [];
    var _topicIds = [];
    var _apps = [];
    if ($('#select-activity').val() != "0")
        _activityTypes.push($('#select-activity').val());

    var url = "/Commerce/LoadMoreActivities";
    $.ajax({
        url: url,
        data: {
            Key: $('#hdfCurrentQbicleId').val(),
            Size: loadCountActivity * qbiclePageSize,
            ActivityTypes: _activityTypes,
            TopicIds: _topicIds,
            Apps: _apps,
            Daterange: $('#txtFilterDaterange').val()
        },
        type: "POST",
        async: false,
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            $('#previous').empty();
            if (isFirstLoad == 0) {
                loadModalActivities();
                isFirstLoad = 1;
                if ($('#first-load-icon:visible').length > 0) {
                    showQbicleStream();
                    $(window).scrollTop(0);
                }
            }
            if (data.length !== 0) {
                if (isFilter) {
                    $("#dashboard-page-display").html('');
                    $("#dashboard-page-display").append('<div id="previous"></div>');
                    $(data.ModelString).insertBefore("#previous").fadeIn(250);
                }
                else {
                    $(data.ModelString).insertBefore("#previous").fadeIn(250);
                }
                var $dayfirstdate = $('#dashboard-date-today');
                if ($dayfirstdate.length > 0) {
                    $("#dashboard-date-today .day-date").first().addClass("day-date-first");
                    $dayfirstdate.addClass("day-block-first");
                }
                removeDom();
            }
            else {
                if (isFilter) {
                    $("#dashboard-page-display").html('');
                    $("#dashboard-page-display").append('<div id="previous"></div>');
                }
                previousShown = true;
            }

            if (data.ModelCount) {
                var ajaxModelCount = data.ModelCount - (loadCountActivity * qbiclePageSize);
                if (ajaxModelCount <= 0)
                    previousShown = true;
                else
                    previousShown = false;
            }
            isBusy = false;
            //QBIC-2064: Remove Forward Option in post management("Discuss, Edit and Delete only.")
            $('.op-forward').remove();
        },
        error: function (xhr, status, error) {
            isBusy = false;
            showQbicleStream();
        }
    });
    loadCountActivity = loadCountActivity + 1;
};
function showQbicleStream() {
    $('#first-load-icon').hide();
    $("#latch").show();
    $('#dashboard-page-display').show();
}
function loadingNewCommerce() {
    if ($('#previous div.text-center'))
        $('#previous').html('<div class="text-center"><img src="/Content/DesignStyle/img/loading-new.gif" style="width: 180px; height: auto;"></div><br />');
    setTimeout(function () {
        loadMoreActivitiesCommerce();
    }, 500);
}
function LoadDataDashboardCommerce(isFilter) {
    if (isFilter) {
        $(window).scrollTop(0);
        loadCountActivity = 0;
        isFirstLoad = 0;
        $('#dashboard-page-display').hide();
        $('#first-load-icon').show();
    }
    setTimeout(function () {
        loadMoreActivitiesCommerce(isFilter);
    }, 500);
}
function removeDom() {
    $(".day-block").each(function () {
        if ($(this).find("article").length == 0)
            $(this).remove();
    });
}
function addTopicToFilter(topicId, topicName) {
    //Topic Filter
    if ($('#select-topic option[value="' + topicId + '"]').length <= 0) {
        $('#select-topic').append($('<option>', {
            value: topicId,
            text: topicName
        })).select2({ placeholder: 'Please select' });
        $('#select-topic').on('change.select2', function (e) {
            LoadDataDashboardCommerce(true);
        });
    }
    //Topic QbicleStream
    if ($('#toppic-value option[value="' + topicId + '"]').length <= 0) {
        $('#toppic-value').append($('<option>', {
            value: topicId,
            text: topicName
        })).select2();
    }

};

function resetFilters() {
    $('.removefilters').hide();
    $('#select-activity').val('0').trigger('change');
    $('#txtFilterDaterange').val('');
    LoadDataDashboardCommerce(true);
}


function initPostStream() {    
    $('#txtSearchPartner').keyup(function () {
        filterB2bPartners();
    });
}
function updateManagers() {
    $.post("/Commerce/UpdateMembersRelationship", { key: $('#hdfCurrentQbicleId').val(), members: $('#slrelationshipmanagers').val() }, function (response) {
        if (response.result) {
            $('#b2b-managers').modal('hide');
            cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Community");
            delay(function () {
                loadModalActivities();
            }, 50);

        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}
function clickB2bQbicle(relationshipId, qbicleKey, el, partnerDomainKey ,partnerName) {
    $('#hdfCurrentQbicleId').val(qbicleKey);
    $('.tab-content ul.widget-contacts li.active').removeClass('active');
    var $elLi= $(el).parent();
    $elLi.addClass('active');
    var rltype = $elLi.data('rltype');
    var partnershipkey = $elLi.data('partnershipkey');
    //hide all button group
    $('#btnlnkHub').hide();
    $('.buy-option').hide();
    $('.sell-option').hide();
    $('.acquirelogistics-option').hide();
    $('#orderListInfo').hide();
    //end
    switch (rltype) {
        case "hud":
            var partnerKey = $elLi.attr('partnerKey');
            var _checkPortalAccessibiltyUrl = "/Commerce/CheckPartnershipPortalAccessibility";
            $.ajax({
                type: 'POST',
                url: _checkPortalAccessibiltyUrl,
                data: {
                    partnerDomainKey: partnerKey
                },
                success: function (response) {
                    if (response == "True") {
                        $('#btnlnkHub').show();
                        $('#btnlnkHub').children('button').attr('onclick', "window.location.href='/Commerce/DiscussionPartner?rlid=" + relationshipId + "';");
                    } else {
                        $('#btnlnkHub').hide();
                    }
                },
                error: function (error) {
                    $('#btnlnkHub').hide();
                }
            });
            break;
        case "buy":
            $('.buy-option').show();
            var _keyQbicles = $('#hdfCurrentQbicleId').val();
            var _checkExistOrder = "/Commerce/CheckExistB2BOrders";
            $.ajax({
                type: "POST",
                url: _checkExistOrder,
                data: {
                    qbicleKey: _keyQbicles
                },
                success: function (response) {
                    if(response){
                        $('#orderListInfo').css({"bottom":"175px"}).attr("data-intro","B2B Orders").show();
                    }
                }
            });
            $('#b2b-catalogues .modal-title').text(partnerName + ' catalogues');
            $('.buy-option a[href="#b2b-catalogues"],.buy-option button[data-target="#b2b-catalogues"]').attr('onclick', 'loadModalCatalogs("' + partnerDomainKey + '",' + relationshipId + ')');
            $('.buy-option a[href="#b2b-order-add"]').attr('onclick', 'initDataB2bForCreateOrder("' + partnershipkey+ '")');
            break;
        case "sell":
            var _keyQbicles = $('#hdfCurrentQbicleId').val();
            var _checkExistOrder = "/Commerce/CheckExistB2BOrders";
            $.ajax({
                type: "POST",
                url: _checkExistOrder,
                data: {
                    qbicleKey:_keyQbicles
                },
                success: function (response) {
                    if(response){
                        $('#orderListInfo').css({"bottom":"100px"}).attr("data-intro","Customer B2B Orders").show();
                    }
                }
            });
            $('.sell-option').show();
            $('.sell-option a[href="#b2b-order-add"]').attr('onclick', 'initDataB2bForCreateOrder("' + partnershipkey + '")');
            break;
        case "acquirelogistics":
            $('.acquirelogistics-option').show();
            break;
        default:
            break;
    }
    LoadDataDashboardCommerce(true);
}
function loadModalCatalogs(partnerDomainKey,relationshipId) {
    if (partnerDomainKey) {
        $('#qbicles-dash-grid-b2b-catalogues').load("/Commerce/LoadPartnerCatalogs?domainKey=" + partnerDomainKey + "&relationshipId=" + relationshipId, function () {
            if($(".distributor-catalog").hasClass("active")){
                $('#cat-distro').addClass("active").addClass("in");
                $('#cat-sales').removeClass("active").removeClass("in");
            }else{
                $('#cat-sales').addClass("active").addClass("in");
                $('#cat-distro').removeClass("active").removeClass("in");
            }
        });
    }
}
function filterB2bPartners() {
    var keyword = $('#txtSearchPartner').val();
    if (keyword) {
        $("#rlQbicles li").filter(function () {
            var reg = new RegExp(keyword, "ig");
            if (reg.test($(this).text()))
                $(this).closest('li').show();
            else
                $(this).closest('li').hide();
        });
    } else {
        $('#rlQbicles li').show();
    }
}
function loadModalActivities() {
    var qbicleKey = $('#hdfCurrentQbicleId').val();
    if (qbicleKey!="0") {
        $('#modal-activities').load("/Commerce/LoadModalActivities?qbicleKey=" + qbicleKey, function () {
            $('#modal-activities select.select2').not('select.select2-hidden-accessible').select2({
                placeholder: 'Please select'
            });
            $('#modal-activities input[data-toggle="toggle"]').bootstrapToggle();
            $('#modal-activities .checkmulti').multiselect({
                includeSelectAllOption: false,
                enableFiltering: false,
                buttonWidth: '100%',
                maxHeight: 400,
                enableClickableOptGroups: true
            });
        });
    }
}
function initNextPreviousTab(frmId, tabId) {
    $(frmId + ' .btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').next('li').find('a').trigger('click');
    });

    $(frmId + ' .btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find(tabId + ' .active').prev('li').find('a').trigger('click');
    });
}
function latchformobile() {
    if ($(document).width() < 1200) {
        $('.interact').waypoint(function (direction) {
            if (direction == "down") {
                $('.interact').addClass('mobile-top');
                $('.block-container').addClass('compensate-sticky');
            }
            if (direction == "up") {
                $('.block-container').removeClass('compensate-sticky');
                $('.interact').removeClass('mobile-top');
            }
        }, { offset: '25%' });
    }
}

function initDataB2bForCreateOrder(partnershipkey) {
    resetDataB2bOrder();
    $('#b2b-order-add').modal('show');
    var _url = "/Commerce/InitDataB2bForCreateOrder";
    $.post(_url, { partnershipKey: partnershipkey}, function (data) {
        $('#b2b-order-add input[name=OrderReferenceId]').val(data.reference.id);
        $('#b2b-order-add input[name=OrderFullRef]').val(data.reference.orderref);
        $('#b2b-order-add input[name=Partnershipkey]').val(partnershipkey);
        var $catalog = $('#b2b-order-add select[name=CatalogId]');
        $catalog.empty();
        data.catalogs.unshift({ id: "", text: "" });
        $catalog.select2({
            placeholder: "Please select",
            data: data.catalogs
        });
    });
}
function resetDataB2bOrder() {
    $('#b2b-order-add input[name=OrderReferenceId]').val('0');
    $('#b2b-order-add input[name=OrderFullRef]').val('');
    $('#b2b-order-add input[name=Partnershipkey]').val('');
    $('#b2b-order-add textarea[name=OrderNote]').val('');
    var $catalog = $('#b2b-order-add select[name=CatalogId]');
    $catalog.empty();
    $catalog.select2({
        placeholder: "Please select",
        data: []
    });
}
function initFormB2bCreateOrder() {
    var $frmordercreation = $('#frmb2bordercreation');
    $frmordercreation.submit(function (e) {
        e.preventDefault();
        if ($frmordercreation.valid()) {
            isDisplayFlicker(true);
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $('#b2b-order-add').modal('hide');
                        if (data.msg != '') {
                            isDisplayFlicker(false);
                            htmlActivityRender(data.msg, 0);
                        }
                        cleanBookNotification.success("Order created successfully", "Qbicles");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(data.msg, "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                    isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        }
    });
}
function deletePost(elmId, key) {
    bootbox.confirm({
        show: true,
        backdrop: true,
        closeButton: true,
        animate: true,
        title: "Qbicles",
        message: _L("WARNING_MSG_DELETEPOST"),
        callback: function (result) {
            if (result) {
                $.LoadingOverlay("show");
                $.post("/Posts/DeletePost", { key: key }, function (response) {
                    $.LoadingOverlay("hide");
                    if (response.result) {
                        $('#' + elmId).remove();
                        cleanBookNotification.success("Your post was successfully deleted.");
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error("There was an issue deleting this post. Please try again", "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                });
                return;
            }
        }
    });

}
function addPostToDiscuss(key) {
    $.LoadingOverlay("show");
    $.get("/Posts/GetMessageOfPost?key=" + key, function (response) {
        $('#create-discussion-qb').modal('show');
        $('#discussion-summary').val(response.message);
        $.LoadingOverlay("hide");
    });
}
function loadEditPostModal(elmId, key) {
    $('#edit-post').modal('show');
    $.LoadingOverlay("show");
    $("#edit-post").load("/Qbicles/GenerateEditPostModal", { postKey: key }, function () {
        $('#edit-post select[name=topic]').select2({ placeholder: 'Please select' });
        $("#frmEditPost").submit(function (event) {
            event.preventDefault();
            
            if ($(this).valid()) {
                $.LoadingOverlay("show");
                var _paramaters = {
                    key: $('#frmEditPost input[name=PostKey]').val(),
                    message: $("#frmEditPost textarea[name=postcontent]").val(),
                    topicId: $("#frmEditPost select[name=topic]").val()
                };
                $.post('/QbicleComments/UpdatePost', _paramaters, function (response) {
                    if (response.result) {
                        $('#edit-post').modal('hide');
                        var topicname = $("#frmEditPost select[name=topic] option:selected").text();
                        $('#' + elmId + ' .topic-label').html('<span class="label label-info">' + fixQuoteCode(topicname) + '</span>');
                        $('#' + elmId + ' .activity-overview p').text(_paramaters.message);
                        $('#' + elmId + ' .post-event').text("Edited post");
                        cleanBookNotification.success(_L('UPDATE_MSG_SUCCESS'), "Qbicles");
                    } else if (!response.result && response.msg) {
                        cleanBookNotification.error(_L(response.msg), "Qbicles");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                    $.LoadingOverlay("hide");
                });
            } else
                return;

        });
        $.LoadingOverlay("hide");
    });
}
function loadB2BPromoteCatalogModal(elmId) {
    $("#promote-catalog-modal").empty();
    LoadingOverlay();
    var _url = "/Qbicles/ShowB2BPromoteCatalogModal";
    $("#promote-catalog-modal").load(_url, function () {
        $("#list-catalog").select2();
        $("#promote-catalog-modal").modal('show');
        LoadingOverlayEnd();
    });
}

function saveB2BCatalogDiscussion() {
    var discussionModel = {
        'CatalogId': $("#promote-catalog-modal #list-catalog").val(),
        'CoveringNote': $("#promote-catalog-modal #covering-note").val()
    }
    var _url = "/Discussions/SaveB2BCatalogDiscussion";
    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            'discussionModel': discussionModel
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Create B2B Catalog discussion successfully!", "Qbicles");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            $("#promote-catalog-modal").modal('hide');
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    })
}

function OpenOrderContextFlyoutB2B() {   
    var ajaxUri = '/Commerce/OpenOrderContextFlyout';
    $('#b2borders-bview').LoadingOverlay("show");
    $('#b2borders-bview').empty();
    $('#b2borders-bview').load(ajaxUri, function () {
        $('#b2borders-bview').LoadingOverlay("hide", true);
        $("#b2borders-bview").modal('show');
    var titleOrder = $("#orderListInfo").attr("data-intro");
    $(".order-context-flyout-modal-title").text(titleOrder);
    });
}
//End B2bQbicle