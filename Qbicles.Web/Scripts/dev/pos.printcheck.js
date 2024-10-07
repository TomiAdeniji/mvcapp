$cancelKey = "";
$(function () {
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
    $('#order-cancel-search-text').keyup(searchThrottle(function () {
        $('#order-cancel-table').DataTable().ajax.reload();
    }));

    $('#order-cancel-cashier').change(function () {
        $('#order-cancel-table').DataTable().ajax.reload();
    });

    $("#order-cancel-manager").change(function () {
        $('#order-cancel-table').DataTable().ajax.reload();
    });

    $("#order-cancel-devices").change(function () {
        $('#order-cancel-table').DataTable().ajax.reload();
    });

    $("#order-cancel-cashier,#order-cancel-manager,#order-cancel-devices").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });

    $('#order-cancel-datetimerange').daterangepicker({
        autoUpdateInput: true,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        startDate: new Date($("#fromDateTime").val()),
        endDate: new Date($("#toDateTime").val()),
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
    $('#order-cancel-datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('#order-cancel-datetimerange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));

        $("#order-cancel-table").DataTable().ajax.reload();
    });
    $('#order-cancel-datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        $('#order-cancel-datetimerange').html('full history');
        $("#order-cancel-table").DataTable().ajax.reload();
    });

});

function LoadDataOrderPrintCheckServerSide() {

    $("#order-cancel-table").on('processing.dt', function (e, settings, processing) {
        if (processing && $('.loadingoverlay').length === 0) {
            $("#order-cancel-table").LoadingOverlay("show");
        } else {
            $("#order-cancel-table").LoadingOverlay("hide", true);
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
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderPosOrderPrintCheck/TraderPosOrderPrintChecks',
            "type": 'POST',
            "data": function (d) {
                return $.extend({}, d, {
                    keyword: $('#order-cancel-search-text').val(),
                    cashiers: $('#order-cancel-cashier').val(),
                    managers: $('#order-cancel-manager').val(),
                    devices: $('#order-cancel-devices').val(),
                    datetime: $("#order-cancel-datetimerange").val()
                });
            }
        },
        "columns": [
            {
                name: "Ref",
                data: "Ref",
                orderable: true,
            },
            {
                name: "Date",
                data: "Date",
                orderable: true,
                width: "150px",
            },
            {
                name: "SalesChannel",
                data: "SalesChannel",
                orderable: true,
                width: "100px",
            },
            {
                name: "Reason",
                data: "Reason",
                orderable: true
            },
            {
                name: "CancelledBy",
                data: "CancelledBy",
                orderable: true
            },
            {
                name: "Cashier",
                data: "Cashier",
                orderable: true
            },
            {
                name: "Customer",
                data: "Customer",
                orderable: true
            },
            {
                name: "TotalItems",
                data: null,
                orderable: false,
                width: "60px",
                render: function (value, type, row) {
                    if (row.TotalItems != "0")
                        return "<button class='btn btn-info' onclick=\"ShowOrderPrintCheckItemDetail('" + row.Key + "')\" ><i class='fa fa-eye'></i> &nbsp; " + row.TotalItems + "</button>";
                    return "<button class='btn btn-info'><i class='fa fa-eye-slash'></i> &nbsp; 0</button>";
                }
            },
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    if (_.isUndefined(row.PDSOrders))
                        return "";
                    var str = "<ul style='margin: 0; padding: 0 0 0 15px;'>";
                    $.each(row.PDSOrders, function (idx, pds) {                        
                        str += "<li><a href='javascript:void(0)' onclick=\"ShowPDSOrderItemDetail('" + pds.Id + "')\">" + pds.Name + "</a></li>";
                    });

                    str += "</ul>";
                    return str;
                }
            },
            {
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    console.log(row.DiscussionKey);
                    if (row.DiscussionKey == "")
                        return "<button class='btn btn-primary' onclick=\"AddPrintCheckDiscussion('" + row.Key + "')\" ><i class='fa fa-plus'></i>&nbsp; Discuss</button>";
                    return "<button class='btn btn-primary' onclick=\"OpenDiscussionDetail('" + row.DiscussionKey + "')\" ><i class='fa fa-comments'></i>&nbsp; Discuss</button>";
                }
            }
        ],
        "order": [[1, "desc"]],
        "initComplete": function (settings, json) {
            $('#order-cancel-table').DataTable().ajax.reload();
        }
    });
}

function OpenDiscussionDetail(key) {
    $("#order-cancel-table").LoadingOverlay("show");
    window.location.href = '/Qbicles/DiscussionQbicle?disKey=' + key;
}
var $printCheckKey = "";
function ShowOrderPrintCheckItemDetail(printCheckKey) {
    $printCheckKey = printCheckKey;

    $("#order-cancel-table").LoadingOverlay("show");
    var ajaxUri = '/TraderPosOrderPrintCheck/OrderPrintCheckItemDetail?printCheckKey=' + printCheckKey;
    $('#cancel-order-item').empty();
    $('#cancel-order-item').load(ajaxUri, function () {
        $("#order-cancel-table").LoadingOverlay("hide", true);
        $("#cancel-order-item").modal('show');
    });
}

function ShowPDSOrderItemDetail(pdsOrderId) {
    $("#order-cancel-table").LoadingOverlay("show");
    $('#cancel-order-item').empty();
    $('#cancel-order-item').load('/TraderReports/ShowQueueOrderSummary?queueOrderId=' + pdsOrderId, function () {
        $("#order-cancel-table").LoadingOverlay("hide", true);
        $("#cancel-order-item").modal('show');
    });
}



function AddPrintCheckDiscussion(printCheckKey) {
    $printCheckKey = printCheckKey;
    $("#order-cancel-table").LoadingOverlay("show");
    var ajaxUri = '/TraderPosOrderPrintCheck/AddOrderPrintCheckDiscussion?printCheckKey=' + printCheckKey;
    $('#create-cancellation-discussion-pos').empty();
    $('#create-cancellation-discussion-pos').load(ajaxUri, function () {
        $("#order-cancel-table").LoadingOverlay("hide", true);
        $("#create-cancellation-discussion-pos").modal('show');
    });
}

function CreateOrderPrintCheckDiscussion() {

    if ($('#frm-discussion-qb').valid()) {
        $("#create-cancellation-discussion-pos").LoadingOverlay("show");
        var files = document.getElementById("discussion-image-upload").files;

        if (files && files.length > 0) {

            UploadMediaS3ClientSide("discussion-image-upload").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    $("#order-cancel-table").LoadingOverlay("hide", true);
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    SaveOrderPrintCheckDiscussion(mediaS3Object.objectKey);
                }
            });
        }
        else {
            SaveOrderPrintCheckDiscussion("");
        }
    }
}


function SaveOrderPrintCheckDiscussion(objectKey) {
    var discussionModel = {
        Key: $("#discussion-key").val(),
        Title: $("#discussion-title").val(),
        Summary: $("#discussion-summary").val(),
        ExpiryDate: $("#discussion-expiryDate").val(),
        IsExpiry: $("#discussion-is-expiry").prop('checked'),
        Topic: $("#discussion-topic").val(),
        Assignee: $("#discussion-assignee").val(),
        FeaturedOption: $("#discussion-featured-option").val(),
        MediaDiscussionUse: $("#mediaDiscussionUse").val(),
        UploadKey: objectKey
    };
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderPosOrderPrintCheck/SaveOrderPrintCheckDiscussion?printCheckKey=" + $printCheckKey,
        data: {
            model: discussionModel
        },
        beforeSend: function (xhr) {
        },
        success: function (data) {
            if (data.result) {
                $('#order-cancel-table').DataTable().ajax.reload();
                $('#create-cancellation-discussion-pos').modal('hide');
                cleanBookNotification.success(_L("ERROR_MSG_398"), "Qbicles");

                $('#frm-discussion-qb').validate().resetForm();
            } else if (!data.result && data.Object) {
                $('#frm-discussion-qb').validate().showErrors(data.Object);
            } else {
                cleanBookNotification.error(_L(data.msg), "Qbicles");
            }
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    }).always(function () {
        $("#create-cancellation-discussion-pos").LoadingOverlay("hide", true);
    });
}
