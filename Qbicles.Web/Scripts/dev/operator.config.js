var isBusy = false;
var taskMembers = [];
var teamMembers = [];
$(document).ready(function () {
    $('.select2').select2({ placeholder: "Please select" });
    initConfigModal();
    initConfigTable();
    initConfigSearch();
});

function initConfigModal() {
    // Tags
    var $tblOperatorTags = $('#tblOperatorTags');
    var $frmOperatorTag = $('#frmOperatorTag');

    $frmOperatorTag.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 50
            },
            Summary: {
                maxlength: 300
            }
        }
    });
    $frmOperatorTag.submit(function (e) {
        e.preventDefault();
        if ($frmOperatorTag.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-tag-addedit').modal('hide');
                        $tblOperatorTags.DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            return;
        }
    });

    // Roles
    var $tblOperatorRoles = $('#tblOperatorRoles');
    var $frmOperatorRole = $('#frmOperatorRole');

    $frmOperatorRole.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 100
            },
            Summary: {
                maxlength: 500
            }
        }
    });
    $frmOperatorRole.submit(function (e) {
        e.preventDefault();
        if ($frmOperatorRole.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-role-addedit').modal('hide');
                        $tblOperatorRoles.DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            return;
        }
    });

    // Locations
    var $tblOperatorLocations = $('#tblOperatorLocations');
    var $frmOperatorLocation = $('#frmOperatorLocation');

    $frmOperatorLocation.validate({
        ignore: "",
        rules: {
            Name: {
                required: true,
                maxlength: 200
            },
            AddressLine1: {
                required: true,
                maxlength: 200
            },
            AddressLine2: {
                maxlength: 200
            },
            City: {
                required: true,
                maxlength: 200
            },
            State: {
                maxlength: 200
            },
            Postcode: {
                maxlength: 200
            },
            CountryName: {
                required: true
            },
            Email: {
                maxlength: 350,
                email: true
            },
            Telephone: {
                maxlength: 25
            }
        }
    });
    $frmOperatorLocation.submit(function (e) {
        e.preventDefault();
        if ($frmOperatorLocation.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#app-operator-location-addedit').modal('hide');
                        $tblOperatorLocations.DataTable().ajax.reload();
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Operator");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                }
            });
        } else {
            return;
        }
    });
}
function initConfigTable() {
    // Tags
    var $tblOperatorTags = $('#tblOperatorTags');
    $tblOperatorTags.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblOperatorTags.LoadingOverlay("show");
        } else {
            $tblOperatorTags.LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            autoWidth: true,
            lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchTags",
                "data": function (d) {
                    return $.extend({}, d, {
                        "tagName": $('#tagNameSearch').val(),
                    });
                }
            },
            columns: [
                { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                { "title": "Summary", "data": "Summary", "searchable": true, "orderable": true },
                { "title": "Created", "data": "Created", "searchable": true, "orderable": true },
                { "title": "Creator", "data": "Creator", "searchable": true, "orderable": true },
                { "title": "Instances", "data": "Instances", "searchable": true, "orderable": true },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
                {
                    "targets": 3,
                    "data": "Creator",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<a href="/Community/UserProfilePage?uId=' + row.CreatorId + '">' + data + '</a>';
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 5,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<button class="btn btn-warning" onclick="loadModalTag(' + data + ');"><i class="fa fa-pencil"></i></button>';
                        return _htmlOptions;
                    }
                }]
        });

    // Roles
    var $tblOperatorRoles = $('#tblOperatorRoles');
    $tblOperatorRoles.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblOperatorRoles.LoadingOverlay("show");
        } else {
            $tblOperatorRoles.LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            autoWidth: true,
            lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchRoles",
                "data": function (d) {
                    return $.extend({}, d, {
                        "roleName": $('#roleNameSearch').val(),
                    });
                }
            },
            columns: [
                { "title": "Role", "data": "Name", "searchable": true, "orderable": true },
                { "title": "Description", "data": "Summary", "searchable": true, "orderable": true },
                { "title": "Status", "data": "Status", "searchable": true, "orderable": true, "width": "140px" },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false, "width": "140px" }
            ],
            columnDefs: [
                {
                    "targets": 2,
                    "data": "Status",
                    "render": function (data, type, row, meta) {
                        if (row.Status) {
                            var _htmlOptions = '<span class="label label-lg label-success">Active</span>';
                        } else {
                            var _htmlOptions = '<span class="label label-lg label-danger">Disabled</span>';
                        }
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 3,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions =
                            '<div class="btn-group options">' +
                            '<button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">' +
                            '<i class="fa fa-cog"></i> &nbsp; Options' +
                            '</button>' +
                            '<ul class="dropdown-menu dropdown-menu-right" style="right: 0;">' +
                            '<li><a href="javascript:void(0)" onclick="setOperatorStatus(' + data + ',' + !row.Status + ');">' + (row.Status == true ? 'Disable' : 'Enable') + '</a></li>' +
                            '<li><a href="javascript:void(0)" onclick="loadModalRole(' + data + ');">Edit</a></li>' +
                            '<li><a href="javascript:void(0)" onclick="removeOperatorRole(' + data + ');">Remove</a></li' +
                            '</ul>' +
                            '</div>';
                        return _htmlOptions;
                    }
                }]
        });

    // Locations
    var $tblOperatorLocations = $('#tblOperatorLocations');
    $tblOperatorLocations.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblOperatorLocations.LoadingOverlay("show");
        } else {
            $tblOperatorLocations.LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            autoWidth: true,
            lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchLocations",
                "data": function (d) {
                    return $.extend({}, d, {
                        "locationSearch": $('#locationSearch').val(),
                    });
                }
            },
            columns: [
                { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                { "title": "Address", "data": "Address", "searchable": true, "orderable": true },
                { "title": "Created", "data": "Created", "searchable": true, "orderable": true },
                { "title": "Creator", "data": "Creator", "searchable": true, "orderable": true },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
                {
                    "targets": 3,
                    "data": "Creator",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<a href="/Community/UserProfilePage?uId=' + row.CreatorId + '">' + data + '</a>';
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 4,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions =
                            '<button class="btn btn-warning" style="margin-right: 3px" data-toggle="modal" data-target="#app-operator-location-addedit" onclick="loadModalLocation(' + data + ');"><i class="fa fa-pencil"></i></button>' +
                            '<button class="btn btn-danger" onclick="removeOperatorLocation(' + data + ');"><i class="fa fa-trash"></i></button>';
                        return _htmlOptions;
                    }
                }]
        });

    // Workgroups
    var $tblOperatorWorkgroups = $('#tblOperatorWorkgroups');
    $tblOperatorWorkgroups.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblOperatorWorkgroups.LoadingOverlay("show");
        } else {
            $tblOperatorWorkgroups.LoadingOverlay("hide", true);
        }
    })
        .dataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            autoWidth: true,
            lengthMenu: [[10, 20, 50, 100], [10, 20, 50, 100]],
            pageLength: 10,
            order: [[0, "asc"]],
            ajax: {
                "url": "/Operator/SearchWorkgroups",
                "data": function (d) {
                    return $.extend({}, d, {
                        "name": $('#workgroupNameSearch').val(),
                        "type": $('#workgroupTypeSearch').val(),
                    });
                }
            },
            columns: [
                { "title": "Name", "data": "Name", "searchable": true, "orderable": true },
                { "title": "Creator", "data": "Creator", "searchable": true, "orderable": true },
                { "title": "Created", "data": "Created", "searchable": true, "orderable": true },
                { "title": "Type", "data": "Type", "searchable": true, "orderable": true },
                { "title": "Qbicle", "data": "Qbicle", "searchable": true, "orderable": true },
                { "title": "Members", "data": "Members", "searchable": true, "orderable": true },
                { "title": "Options", "data": "Id", "searchable": true, "orderable": false }
            ],
            columnDefs: [
                {
                    "targets": 3,
                    "data": "Type",
                    "render": function (data, type, row, meta) {
                        var htmlOptions = '';
                        if (data === "Team") {
                            _htmlOptions = '<span class="label label-lg label-primary">Team</span>';
                        } else {
                            _htmlOptions = '<span class="label label-lg label-info">Tasks</span>';
                        };
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 4,
                    "data": "Qbicle",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions = '<a href="/Qbicles/Dashboard">' + data + '</a>';
                        return _htmlOptions;
                    }
                },
                {
                    "targets": 6,
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var _htmlOptions =
                            '<button class="btn btn-warning" style="margin-right: 3px" data-toggle="modal" data-target="#app-operator-workgroup-addedit" onclick="LoadModalWorkgroup(' + data + ');"><i class="fa fa-pencil"></i></button>' +
                            '<button class="btn btn-danger" onclick="removeOperatorWorkgroup(' + data + ');"><i class="fa fa-trash"></i></button>';
                        return _htmlOptions;
                    }
                }]
        });
}
function initConfigSearch() {
    var $tblOperatorTags = $('#tblOperatorTags');
    $('#tagNameSearch').keyup(searchThrottle(function () {
        $tblOperatorTags.DataTable().ajax.reload();
    }));
    var $tblOperatorRoles = $('#tblOperatorRoles');
    $('#roleNameSearch').keyup(searchThrottle(function () {
        $tblOperatorRoles.DataTable().ajax.reload();
    }));
    var $tblOperatorLocations = $('#tblOperatorLocations');
    $('#locationSearch').keyup(searchThrottle(function () {
        $tblOperatorLocations.DataTable().ajax.reload();
    }));
    var $tblOperatorWorkgroups = $('#tblOperatorWorkgroups');
    $('#workgroupNameSearch').keyup(searchThrottle(function () {
        $tblOperatorWorkgroups.DataTable().ajax.reload();
    }));
    $('#workgroupTypeSearch').change(searchThrottle(function () {
        $tblOperatorWorkgroups.DataTable().ajax.reload();
    }));
}
function ReloadTopics() {
    var qid = $('#qbicleSetting').val();
    var topicSetting = $('#topicSetting');
    var btnwg = $('#btn-addWorkgroup');
    if (qid > 0) {
        $.getJSON("/Operator/LoadTopicsByQbicleId", { qid: qid }, function (data) {
            if (data && data.length > 0) {
                topicSetting.prop("disabled", false);
                btnwg.prop("disabled", false);
                topicSetting.empty();
                topicSetting.select2({
                    data: data,
                    placeholder: "Please select"
                });
                updateSetting();
            }
        });
    } else {
        topicSetting.prop("disabled", true);
        btnwg.prop("disabled", true);
    }
}
function updateSetting() {
    $.LoadingOverlay("show");
    $.ajax({
        url: "/Operator/UpdateSetting",
        data: { id: $('#settingId').val(), qId: $('#qbicleSetting').val(), tId: $('#topicSetting').val() },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_208"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}
function loadModalTag(id) {
    $('#frmOperatorTag').validate().resetForm();//remove error class on name elements and clear history
    if (id > 0) {
        $.get("/Operator/getTagById?id=" + id, function (data) {
            if (data) {
                $('#app-operator-tag-addedit h5.modal-title').text('Edit tag');
                $('#frmOperatorTag input[name="Id"]').val(data.Id);
                $('#frmOperatorTag input[name="Name"]').val(data.Name);
                $('#frmOperatorTag input[name="Summary"]').val(data.Summary);
                $('#app-operator-tag-addedit').modal('show');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_410", [id]), "Operator");
            }
        });
    } else {
        $('#app-operator-tag-add h5.modal-title').text('Add a Tag');
        $('#frmOperatorTag input[name="Id"]').val(0);
        $('#frmOperatorTag input[name="Name"]').val('');
        $('#frmOperatorTag input[name="Summary"]').val('');
        $('#app-operator-tag-addedit').modal('show');
    }
}

function loadModalRole(id) {
    $('#frmOperatorRole').validate().resetForm();//remove error class on name elements and clear history
    if (id > 0) {
        $.get("/Operator/getRoleById?id=" + id, function (data) {
            if (data) {
                $('#app-operator-role-addedit h5.modal-title').text('Edit role');
                $('#frmOperatorRole input[name="Id"]').val(data.Id);
                $('#frmOperatorRole input[name="Name"]').val(data.Name);
                $('#frmOperatorRole textarea[name="Summary"]').val(data.Summary);
                $('#app-operator-role-addedit').modal('show');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_410", [id]), "Operator");
            }
        });
    } else {
        $('#app-operator-role-add h5.modal-title').text('Add a Role');
        $('#frmOperatorRole input[name="Id"]').val(0);
        $('#frmOperatorRole input[name="Name"]').val('');
        $('#frmOperatorRole textarea[name="Summary"]').val('');
        $('#app-operator-role-addedit').modal('show');
    }
}

function loadModalLocation(id) {
    $('#frmOperatorLocation').validate().resetForm();//remove error class on name elements and clear history
    if (id > 0) {
        $.get("/Operator/getLocationById?id=" + id, function (data) {
            if (data) {
                $('#app-operator-location-addedit h5.modal-title').text('Edit location');
                $('#frmOperatorLocation input[name="Id"]').val(data.Id);
                $('#frmOperatorLocation input[name="Name"]').val(data.Name);
                $('#frmOperatorLocation input[name="AddressLine1"]').val(data.AddressLine1);
                $('#frmOperatorLocation input[name="AddressLine2"]').val(data.AddressLine2);
                $('#frmOperatorLocation input[name="City"]').val(data.City);
                $('#frmOperatorLocation input[name="State"]').val(data.State);
                $('#frmOperatorLocation input[name="Postcode"]').val(data.Postcode);
                $('#frmOperatorLocation select[name="CountryName"]').val(data.Country).trigger('change');
                $('#frmOperatorLocation input[name="Email"]').val(data.Email);
                $('#frmOperatorLocation input[name="Telephone"]').val(data.Telephone);
                $('#app-operator-location-addedit').modal('show');
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_410", [id]), "Operator");
            }
        });
    } else {
        $('#app-operator-location-addedit h5.modal-title').text('Add a Location');
        $('#frmOperatorLocation input[name="Id"]').val(0);
        $('#frmOperatorLocation input[name="Name"]').val('');
        $('#frmOperatorLocation input[name="AddressLine1"]').val('');
        $('#frmOperatorLocation input[name="AddressLine2"]').val('');
        $('#frmOperatorLocation input[name="City"]').val('');
        $('#frmOperatorLocation input[name="State"]').val('');
        $('#frmOperatorLocation input[name="Postcode"]').val('');
        $('#frmOperatorLocation select[name="CountryName"]').val('').trigger('change');
        $('#frmOperatorLocation input[name="Email"]').val('');
        $('#frmOperatorLocation input[name="Telephone"]').val('');
        $('#app-operator-location-addedit').modal('show');
    }
}

function setOperatorStatus(id, status) {
    $.LoadingOverlay("show");
    var $tblOperatorRoles = $('#tblOperatorRoles');
    $.ajax({
        url: "/Operator/SetOperatorStatus",
        data: { id: id, status: status },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $tblOperatorRoles.DataTable().ajax.reload();
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function removeOperatorRole(id) {
    $.LoadingOverlay("show");
    var $tblOperatorRoles = $('#tblOperatorRoles');
    $.ajax({
        url: "/Operator/RemoveOperatorRole",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $tblOperatorRoles.DataTable().ajax.reload();
                cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function removeOperatorLocation(id) {
    $.LoadingOverlay("show");
    var $tblOperatorLocations = $('#tblOperatorLocations');
    $.ajax({
        url: "/Operator/RemoveOperatorLocation",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $tblOperatorLocations.DataTable().ajax.reload();
                cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function removeOperatorWorkgroup(id) {
    $.LoadingOverlay("show");
    var $tblOperatorWorkgroups = $('#tblOperatorWorkgroups');
    $.ajax({
        url: "/Operator/RemoveOperatorWorkgroup",
        data: { id: id },
        type: "POST",
        success: function (data) {
            LoadingOverlayEnd();
            if (data.result) {
                $tblOperatorWorkgroups.DataTable().ajax.reload();
                cleanBookNotification.success(_L("REMOVE_MSG_SUCCESS"), "Operator");
            } else if (data.msg) {
                cleanBookNotification.error(data.msg, "Operator");
            }
        },
        error: function (error) {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
        }
    });
}

function LoadModalPeople(workgroupId) {
    $.LoadingOverlay("show");
    $("#app-operator-people-select").load("/Operator/LoadModalPeople", { workgroupId: workgroupId }, function () {
        $('#app-operator-people-select .select2').select2({ placeholder: "Please select" });
        var type = $("#frmOperatorWorkgroup select[name='Type']").val();
        if (type == 0) {
            $("#wgMembers .contact-sideoptions >button").show();
            $("#wgMembers .contact-sideoptions >div").hide();
            $("#tblTaskMembers button").each(function () {
                var id = $(this).data("id");
                $(".contact" + id + "-remove").show();
                $(".contact" + id + "-add").hide();
            });
        } else if (type == 1) {
            $("#wgMembers .contact-sideoptions >button").show();
            $("#wgMembers .contact-sideoptions >div").hide();
            $("#tblTeamMembers button").each(function () {
                var id = $(this).data("id");
                $(".contact" + id + "-remove").show();
                $(".contact" + id + "-add").hide();
            });
        }
        LoadingOverlayEnd();
    });
}

function LoadModalWorkgroup(id) {
    $.LoadingOverlay("show");
    $("#app-operator-workgroup-addedit").load("/Operator/LoadModalWorkgroup", { id: id }, function () {
        $('#app-operator-workgroup-addedit .select2').select2({ placeholder: "Please select" });
        $("input[data-toggle='toggle']").bootstrapToggle();
        //Workgroup
        var $frmOperatorWorkgroup = $('#frmOperatorWorkgroup');
        $frmOperatorWorkgroup.validate({
            ignore: "",
            rules: {
                Name: {
                    required: true,
                    maxlength: 150
                },
                QbicleId: {
                    required: true
                },
                TopicId: {
                    required: true
                },
                Type: {
                    required: true
                },
                LocationId: {
                    required: true
                }
            }
        });
        $('.btnNext').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('#tabNavWorkgroup .active').next('li').find('a').trigger('click');
        });

        $('.btnPrevious').click(function () {
            var parent = $(this).closest('.modal');
            $(parent).find('#tabNavWorkgroup .active').prev('li').find('a').trigger('click');
        });
        LoadingOverlayEnd();
    });
}

function filterMembers() {
    try {
        var kw = $('#opkeyword').val();
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

function reloadDefaultTopic(currentTopicId) {
    var qbicleId = $('#wg-qbicle').val();
    $.getJSON('/Operator/GetTopicByQbicle', { qbicleId: (qbicleId ? qbicleId : 0), currentTopicId: currentTopicId }, function (result) {
        $('#wg-topic').select2({
            data: result
        });
    });
}

function changeWorkgroupType() {
    var type = $("#frmOperatorWorkgroup select[name='Type']").val();
    $('.members').hide();
    if (type == 0) {
        $(".wg-task-setup").show();
    } else if (type == 1) {
        $(".wg-team-setup").show();
    }
}

function removeTeamMember(el) {
    $(el).closest("tr").remove();
    $(".contact" + $(el).data(id) + "-remove").hide();
    $(".contact" + $(el).data(id) + "-add").show();
}

function removeTaskMember(el) {
    $(el).closest("tr").remove();
    $(".contact" + $(el).data(id) + "-remove").hide();
    $(".contact" + $(el).data(id) + "-add").show();
}

function searchTeamMembers() {
    var teamKeyword = $("#teamKeyword").val().toLowerCase();
    var teamRole = $("#teamRole").val();
    $("#tblTeamMembers tbody tr").each(function () {
        var name = $(this).find("td").eq(1).text().trim();
        var role = $(this).find("select").first().val();
        if ((name == '' || name == undefined || name.toLowerCase().indexOf(teamKeyword) >= 0) && (teamRole == -1 || teamRole == role)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function searchTaskMembers() {
    var taskKeyword = $("#taskKeyword").val().toLowerCase();
    var permissionTask = $("#permissionTask").val();
    $("#tblTaskMembers tbody tr").each(function () {
        var name = $(this).find("td").eq(1).text().trim();
        var isTaskCreator = $(this).find("input").first().is(":checked");
        if ((name == '' || name == undefined || name.toLowerCase().indexOf(taskKeyword) >= 0) && (permissionTask == 0 || isTaskCreator == permissionTask)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function addMembers(el) {
    $(el).toggle();
    $(".contact" + $(el).data("id") + "-remove").toggle();
    var type = $("#workgroupType").val();
    if (type == 0) {
        $("#tblTaskMembers tbody").append(
            "<tr id=\"task-member-id-" + $(el).data("id") + "\">" +
            "<td><div class=\"table-avatar mini\" style=\"background-image: url('" + $(el).data("avatar") + "');\">&nbsp;</div></td>" +
            "<td><a href=\"/Community/UserProfilePage?uId=" + $(el).data("id") + "\">" + $(el).data("name") + "</a></td>" +
            "<td>" +
            "<input data-toggle=\"toggle\" data-onstyle=\"success\" data-on=\"<i class='fa fa-check'></i>\" data-off=\" \" type=\"checkbox\">" +
            "<td>" +
            "<button class=\"btn btn-danger\" data-id=\"" + $(el).data("id") + "\" onclick=\"removeTaskMember(this)\"><i class=\"fa fa-trash\"></i></button>" +
            "</td>" +
            "</tr>");
        $("#tblTaskMembers input[data-toggle='toggle']").last().bootstrapToggle();
    } else if (type == 1) {
        $("#tblTeamMembers tbody").append(
            "<tr id=\"team-member-id-" + $(el).data("id") + "\">" +
            "<td><div class=\"table-avatar mini\" style=\"background-image: url('" + $(el).data("avatar") + "');\">&nbsp;</div></td>" +
            "<td><a href=\"/Community/UserProfilePage?uId=" + $(el).data("id") + "\">" + $(el).data("name") + "</a></td>" +
            "<td>" +
            "<select name=\"role\" class=\"form-control select2\" style=\"width: 100%;\">" +
            "<option value=\"\"></option>" +
            "<option value=\"0\">Manager</option>" +
            "<option value=\"1\">Supervisor</option>" +
            "<option value=\"2\">Team member</option>" +
            "</select >" +
            "</td >" +
            "<td>" +
            "<button class=\"btn btn-danger\" data-id=\"" + $(el).data("id") + "\" onclick=\"removeTeamMember(this)\"><i class=\"fa fa-trash\"></i></button>" +
            "</td>" +
            "</tr >");
        $('#tblTeamMembers .select2').last().select2({ placeholder: "Please select" });
    }
}

function removeMembers(id) {
    $(".contact" + id + "-remove").hide();
    $(".contact" + id + "-add").toggle();
    var type = $("#workgroupType").val();
    if (type == 0) {
        $("#task-member-id-" + id).remove();
    } else {
        $("#team-member-id-" + id).remove();
    }
}

function getAllTeamMembers() {
    var lstTeamMembers = [];
    $("#tblTeamMembers tbody tr").each(function () {
        var id = $(this).find("button").first().data("id");
        var role = $(this).find("select").first().val();
        lstTeamMembers.push({ Id: id, Role: role });
    });
    return lstTeamMembers;
}

function getAllTaskMembers() {
    var lstTaskMembers = [];
    $("#tblTaskMembers tbody tr").each(function () {
        var id = $(this).find("button").first().data("id");
        var isTaskCreator = $(this).find("input").first().is(":checked");
        lstTaskMembers.push({ Id: id, IsTaskCreator: isTaskCreator });
    });
    return lstTaskMembers;
}

function saveWorkgroup() {
    var $tblOperatorWorkgroups = $('#tblOperatorWorkgroups');
    $frmOperatorWorkgroup = $("#frmOperatorWorkgroup");
    if ($frmOperatorWorkgroup.valid()) {
        $.LoadingOverlay("show");

        var formData = new FormData($frmOperatorWorkgroup[0]);
        if ($("#workgroupType").val() == 0) {
            formData.append("TaskMembers", JSON.stringify(getAllTaskMembers()));
        } else {
            formData.append("TeamMembers", JSON.stringify(getAllTeamMembers()));
        }
        $.ajax({
            type: 'POST',
            cache: false,
            url: '/Operator/SaveWorkgroup',
            enctype: 'multipart/form-data',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                isBusy = true;
            },
            success: function (data) {
                isBusy = false;
                if (data.result) {
                    $('#app-operator-workgroup-addedit').modal('hide');
                    $tblOperatorWorkgroups.DataTable().ajax.reload();
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Operator");
                } else if (!data.result && data.msg) {
                    cleanBookNotification.error(_L(data.msg), "Operator");
                }
                LoadingOverlayEnd();
            },
            error: function (data) {
                isBusy = false;
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
            }
        });
    } else {
        LoadingOverlayEnd();
        $('#app-operator-workgroup-addedit a[href=#add-specifics]').tab('show');
        return;
    }
}

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

