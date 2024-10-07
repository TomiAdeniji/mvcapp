var isBusy = false;

$(document).ready(function () {
    LoadMedias();
    $('#personResourceSearch').keyup(searchThrottle(function () {
        LoadMedias();
    }));

    $('#sl-media-type').change(searchThrottle(function () {
        LoadMedias();
    }));
})

function LoadMedias() {
    $.LoadingOverlay("show");
    var fid = $('#mediaFolderId').val();
    var qid = $('#operatorQbicleId').val();
    var rs = $('#personResourceSearch').val();
    var fileType = $('#sl-media-type').val();
    
    $.ajax({
        type: 'post',
        url: '/Operator/LoadMedia',
        datatype: 'json',
        data: { fid: fid, qid: qid, fileType: fileType == "All" ? "" : fileType, rs : rs },
        success: function (listMedia) {
            if (listMedia) {
                var $divcontain = $('#asset-resources');
                $divcontain.html(listMedia);
                totop();
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.responseText, "Qbicles");
            LoadingOverlayEnd();
        }
    });
}

function LoadResourceModal(id) {
    $("#create-resource").load("/Operator/LoadResourceModal", { folderId: id }, function () {
        $('#create-resource .select2').select2({ placeholder: "Please select" });
        $form_media_addedit = $("#form_media_addedit");
        $form_media_addedit.validate({
            rules: {
                name: {
                    required: true,
                    minlength: 5
                },
                description: {
                    required: true
                }
            }
        });
        $form_media_addedit.submit(function (e) {
            e.preventDefault();
            if (isBusy)
                return;
            if ($form_media_addedit.valid()) {
                $.LoadingOverlay("show");
                var frmData = new FormData($form_media_addedit[0]);
                frmData.append("qbicleId", $("#operatorQbicleId").val());
                $.ajax({
                    type: this.method,
                    cache: false,
                    url: this.action,
                    enctype: 'multipart/form-data',
                    data: frmData,
                    processData: false,
                    contentType: false,
                    beforeSend: function (xhr) {
                        isBusy = true;
                    },
                    success: function (data) {
                        if (data.result) {
                            $('#create-resource').modal('hide');
                            isBusy = false;
                            LoadMedias($('#mediaFolderId').val(), $('#operatorQbicleId').val());
                            cleanBookNotification.success(_L("ERROR_MSG_172"), "Operator");
                            $form_media_addedit.trigger("reset");
                        } else if (data.msg) {
                            cleanBookNotification.error(data.msg, "Operator");
                            isBusy = false;
                        }
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Operator");
                        LoadingOverlayEnd();
                    }
                });
            }
        }); 
    });
}

function totop() {
    $("html, body").animate({ scrollTop: 0 }, 0);
    return false;
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
