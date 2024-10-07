$(function () {
    $("#location-select").select2();
    $("#location-filter").select2();
    $("#search-dt").keyup(delay(function () {
        reloadDataItemImports();
    }, 1000));

    $('.item-import-daterange').daterangepicker({
        autoUpdateInput: false,
        timePicker: true,
        cancelClass: "btn-danger",
        opens: "right",
        //startDate: new Date($("#fromDateTime").val()),
        //endDate: new Date($("#toDateTime").val()),
        locale: {
            cancelLabel: 'Clear',
            format: $dateTimeFormatByUser
        }
    });
    $('.item-import-daterange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        $('.item-import-daterange').html(picker.startDate.format($dateTimeFormatByUser) + ' - ' + picker.endDate.format($dateTimeFormatByUser));
        filter.DateRange = $(".item-import-daterange").val();
        reloadDataItemImports();
    });
    $('.item-import-daterange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        filter.DateRange = $(".item-import-daterange").val();
        $('.item-import-daterange').html('full history');
        reloadDataItemImports();
    });
    LoadItemImports();
});

function reloadDataItemImports() {
    $("#table-item-import").DataTable().ajax.reload(null, false);
}

function UploadNew() {
    $('#button-import-now').attr('disabled', 'disabled');
    $("#item-upload-file").val("");
    $("#location-select").val(0).trigger("change");
    ShowImport();
    $('#import-items-modal').modal('toggle');
}


function OnSelectLocation() {
    if ($("#location-select").val() == "0")
        $('#button-import-now').attr('disabled', 'disabled');
    else
        $('#button-import-now').removeAttr('disabled');
}


function uploading() {

    ShowImporting();

    var files = document.getElementById("item-upload-file").files;
    if (files <= 0 || $("#item-upload-file").val() == "") {
        cleanBookNotification.error("Spreadsheet file is required!", "Qbicles");
        ShowImport();
        return;
    }
    if ($("#location-select").val() == "0") {
        cleanBookNotification.error("Location will items be added is required!", "Qbicles");
        ShowImport();
        return;
    }


    var mediaFiles = document.getElementById("item-upload-file").files;
    var file = mediaFiles[0];
    $.ajax({
        type: 'post',
        url: '/TraderItemImport/FileNameAndExtensionVerify',
        data: {
            fileName: file.name,
            locationId: parseInt($("#location-select").val())
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                SaveItemImport()
            } else {
                if (response.actionVal == 2)
                    ShowImportFailed();
                else {
                    ShowImport();
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            }
        },
        error: function (er) {
            ShowImport();
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
    });
}


function ShowImporting() {
    $('#import').hide();
    $('#importing').show();
    $('#imported').hide();
    $('#importfailed').hide();
}

function ShowImport() {
    $('#import').show();
    $('#importing').hide();
    $('#imported').hide();
    $('#importfailed').hide();
    $('#button-import-now').show();
}

function ShowImportFailed() {
    $('#import').show();
    $('#importing').hide();
    $('#imported').hide();
    $('#importfailed').show();
    $('#button-import-now').hide();

}

function SaveItemImport() {
    ShowImporting();
    UploadMediaS3ClientSide("item-upload-file").then(function (mediaS3Object) {

        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            LoadingOverlayEnd('hide');
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            return;
        }
        else {
            var itemImport = {
                Id: 0,
                Location: { Id: parseInt($("#location-select").val()) },
                Domain: { Id: 0 },
                SpreadsheetKey: mediaS3Object.objectKey,
                Spreadsheet: mediaS3Object.fileName
            };
            
            $.ajax({
                type: 'post',
                url: '/TraderItemImport/SaveItemImport',
                data: { itemImport: itemImport },
                dataType: 'json',
                success: function (response) {
                    if (response.result) {
                        $('#import').hide();
                        $('#importing').hide();
                        $('#imported').show();

                    } else {
                        $('#import').show();
                        $('#importing').hide();
                        $('#imported').hide();
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }

                },
                error: function (er) {
                    cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
                }
            }).always(function () {
                //$.LoadingOverlay("hide");
            });;
        }
    });
}

function LoadItemImports() {

    $("#table-item-import").on('processing.dt', function (e, settings, processing) {
        //$('#processingIndicator').css('display', 'none');
        if (processing) {
            $('#table-item-import').LoadingOverlay("show", { minSize: "70x60px" });
        } else {
            $('#table-item-import').LoadingOverlay("hide", true);
        }
    }).DataTable({
        "destroy": true,
        "serverSide": true,
        "paging": true,
        "searching": false,
        "responsive": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "ajax": {
            "url": '/TraderItemImport/GetItemImports',
            "type": 'POST',
            "dataType": 'json',
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $("#search-dt").val(),
                    "datetime": $("#input-datetime-range").val(),
                    "locationId": $("#location-filter").val(),
                });
            }
        },
        "columns": [
            {
                name: "Spreadsheet",
                data: "Spreadsheet",
                orderable: true
            },
            {
                name: "Location",
                data: "Location",
                orderable: true
            },
            {
                name: "CreatedDate",
                data: "CreatedDate",
                orderable: true
            },
            {
                name: "Uploader",
                data: "Uploader",
                orderable: true
            },
            {
                name: null,//"Status",
                data: null,//"Status",
                orderable: true,
                render: function (value, type, row) {
                    switch (row.Status) {
                        case 1:
                            return "<span class='label label-lg label-info'>Verifying file format</span>";
                        case 2:
                            return "<span class='label label-lg label-primary'>Uploading &amp; updating items</span>";
                        case 3:
                            return "<span class='label label-lg label-success'>Uploaded</span>";
                        case 4:
                            return "<span class='label label-lg label-danger'>Uploaded with errors</span>";
                        case 5:
                            return "<span class='label label-lg label-warning'>File error</span>";
                    }
                }
            },
            {
                name: null,//"Processed",
                data: null,//"Processed",
                orderable: true,
                render: function (value, type, row) {
                    var processedCol = "";
                    switch (row.Status) {
                        case 3:
                        case 4:
                            if (row.ItemsImported != null)
                                processedCol += "<span class='label label-lg label-success'> Imported: " + row.ItemsImported + "</span>&nbsp;";
                            if (row.ItemsUpdated != null)
                                processedCol += "<span class='label label-lg label-primary'> Updated: " + row.ItemsUpdated + "</span>&nbsp;";
                            if (row.ItemsError != null)
                                processedCol += "<span class='label label-lg label-warning'> Error: " + row.ItemsError + "</span>";
                            break;
                    }
                    return processedCol;
                }
            },
            {
                name: null,
                data: null,
                orderable: false,
                width: "100px",
                render: function (value, type, row) {
                    var optionCol = "";
                    if (row.Status == 1 || row.Status == 2)
                        optionCol += '<button class="btn btn-soft" data-tooltip="Refresh for the latest status update" onclick="RefreshStatus(\'' + row.Key + '\',' + row.Id + ');"><i class="fa fa-redo"></i></button>';
                    if (row.Status == 3)
                        optionCol += '<a class="btn btn-primary" href="javascript:void(0)" onclick="DownloadItemImport(\'done\',\'' + row.SpreadsheetKey + '\');"><i class="fas fa-cloud-download-alt"></i> &nbsp;Download</a>';
                    if (row.Status == 4 || row.Status == 5) {
                        optionCol += "<div class='btn-group'>";
                        optionCol += "<button type='button' class='btn btn-primary dropdown-toggle' data-toggle='dropdown' aria-haspopup='true' aria-expanded='true'>";
                        optionCol += "Options &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i class='fa fa-angle-down'></i>";
                        optionCol += "</button>";
                        optionCol += "<ul class='dropdown-menu primary dropdown-menu-right' style='right: 0;'>";
                        optionCol += '<li><a href="javascript:void(0)" onclick="DownloadItemImport(\'done\',\'' + row.SpreadsheetKey + '\');"> &nbsp;Download</a></li>';
                        optionCol += '<li><a href="javascript:void(0)" onclick="DownloadItemImport(\'error\',\'' + row.SpreadsheetKey + '\');"> &nbsp;Error report</a></li>';
                        optionCol += "</ul>";
                        optionCol += "</div>";
                    }

                    return optionCol;
                }
            }
        ],
        "drawCallback": function (settings) {
            $.getScript("/Content/DesignStyle/js/html5tooltips.js");
        },
        "order": [[2, "desc"]]
    });
};


function RefreshStatus(objKey, id) {
    reloadDataItemImports();
};

function DeleteItemImport(key) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TraderItemImport/DeleteItemImport',
        data: { key: key },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                reloadDataItemImports();
                cleanBookNotification.removeSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        $.LoadingOverlay("hide");
    });;
}
function DownloadItemImport(name, uri) {
    $.ajax({
        type: 'post',
        url: '/TraderItemImport/DownloadImportedFile',
        datatype: 'json',
        data: {
            Uri: uri,
            Name: name
        },
        success: function (refModel) {
            var link = document.createElement("a");
            //link.download = 'abc def.xlsx';
            link.target = '_blank';
            link.href = refModel;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            delete link;
        }, error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
        }
    });
}