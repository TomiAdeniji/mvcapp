var isBusyAddTaskForm = false;
function LoadModalSegment(id) {
    $("#app-marketing-segment-add").load("/SalesMarketingSegment/GenerateModalSegmentAddEdit", { segmentId: id }, function () {
        $(".checkmulti").multiselect({
            includeSelectAllOption: false,
            enableFiltering: false,
            buttonWidth: '100%',
            maxHeight: 400,
            enableClickableOptGroups: true
        });
        $('#segmentType').select2({
            placeholder: 'Please select'
        });
        
        $('#frm-segment').validate(
            {
                ignore: [],
                rules: {
                    Name: {
                        required: true,
                        maxlength: 150,
                        minlength: 3
                    },
                    Summary: {
                        maxlength: 250
                    }
                },
                invalidHandler: function () {
                    if ($('#segment-1 label.error:not([style*="display: none"])').length != 0) {
                        $('#tabSegment a[href="#segment-1"]').tab('show');
                    } else if ($('#segment-2 label.error:not([style*="display: none"])').length != 0) {
                        $('#tabSegment a[href="#segment-2"]').tab('show');
                    }
                }
            });
    })
}
function SocialLoadOptionVal(cel, elid) {
    var _vl = $(cel).val();
    if (!_vl)
        return;
    var _clauses = [];
    $('.criteria-el').each(function (index) {
        var _criteriaVal = $('.criteria-el select[name="Criterias[' + index + '].CriteriaId"]').val();
        if (_criteriaVal) {
            _clauses.push(_criteriaVal);
        }
    });
    var found = _clauses.filter(function (element) {
        return element == _vl;
    });
    if (found && found.length > 1) {
        cleanBookNotification.error(_L("ERROR_MSG_205"), "Sales Marketing");
        $(cel).val('').change();
        return;
    }
    $.ajax({
        url: '/SalesMarketingSegment/GetOptionValuesByCriteriaId',
        type: 'get',
        data: { criteriaId: $(cel).val() },
        error: function (data) {
            isBusyAddTaskForm = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        },
        success: function (data) {
            if (data.length > 0) {
                var $elid = $('#' + elid);
                $('#' + elid + ' option').remove();
                $.each(data, function (index, value) {
                    $elid.append('<option value="' + value.id + '">' + value.text + '</option>');
                });
                $elid.multiselect('destroy');
                $elid.hide();
                $elid.multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            }
        }
    });
}
function SocialMoreCriteria() {
    var el_count = $('#more-criteria-content .criteria-el').length + 1;
    var _crs = [];
    $('.item-criterial').each(function (index) {
        var _vl = $(this).val();
        if (_vl)
            _crs.push(_vl);
        else {
            cleanBookNotification.error(_L("ERROR_MSG_370"), "Sales Marketing");
            return;
        }
    });
    $.ajax({
        url: '/SalesMarketingSegment/GenerateMoreCriteria',
        type: 'get',
        data: { index: el_count, criterias: JSON.stringify(_crs) },
        error: function (data) {
            isBusyAddTaskForm = false;
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        },
        success: function (response) {
            if (response) {
                var $response = $(response);
                if ($('#more-criteria-content div.criteria-el').length == 0)
                {
                    $response.find('.lblAnd').remove();
                }
                $('#more-criteria-content').append($response);
                $('.select2').select2({
                    placeholder: 'Please select'
                });
                $(".checkmulti").multiselect({
                    includeSelectAllOption: false,
                    enableFiltering: false,
                    buttonWidth: '100%',
                    maxHeight: 400,
                    enableClickableOptGroups: true
                });
            }
        }
    });
}
function SocialGenerateList(c_contacts) {
    if ($('#frm-segment').valid()) {
        var _clauses = [];
        $('.criteria-el').each(function (index) {
            var _criteriaVal = $('.criteria-el select[name="Criterias[' + index + '].CriteriaId"]').val();
            var _criteriaOption = $('.criteria-el select[name="Criterias[' + index + '].CriteriaValues"]').val();
            if (_criteriaVal && _criteriaOption) {
                _clauses.push({ CriteriaId: _criteriaVal, CriteriaValues: _criteriaOption });
            }
        });
        var _area=$('#segment-1 select[name=Areas]').val();
        var data = {
            clauses: _clauses,
            areaIds: _area?_area:[],
            cContacts: c_contacts
        };
        $('#tabSegment a[href="#segment-3"]').tab('show');
        $('#lst-contact-content').load("/SalesMarketingSegment/GenerateListContact", data, function () {
            $('#lst-contacts').DataTable({
                destroy: true,
                responsive: true,
                order: [[0, 'asc']],
                "language": {
                    "lengthMenu": "_MENU_ &nbsp; per page"
                }
            });
            $('#lst-contacts').on('draw.dt', function () {
                $('#lst-contacts tbody input:checkbox').bootstrapToggle();
            });
            $('#lst-contacts tbody input:checkbox').bootstrapToggle();
        });
    }
}
function bindValCheckbox(el) {
    var elchk = $(el);
    if (elchk.prop("checked")) {
        $("#hdfSelectContacts option[value='" + elchk.val() + "']").attr('selected', 'selected');
    } else {
        $("#hdfSelectContacts option[value='" + elchk.val() + "']").removeAttr('selected');
    }
}
function SocialRefreshContacts() {
    $('#app-marketing-segment-add').modal("show");
    $('#btnContactsGenerateList').trigger('click');
    $('#tabSegment a[href="#segment-3"]').tab('show');
}


ProcessSegmentAdd = function () {
    if (!$('#frm-segment').valid()) {
        return;
    }
    $.LoadingOverlay("show");
    var files = document.getElementById("sm-segment-upload-media").files;

    if (files && files.length > 0) {

        UploadMediaS3ClientSide("sm-segment-upload-media").then(function (mediaS3Object) {

            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                LoadingOverlayEnd('hide');
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $("#sm-segment-object-key").val(mediaS3Object.objectKey);
                $("#sm-segment-object-name").val(mediaS3Object.fileName);
                $("#sm-segment-object-size").val(mediaS3Object.fileSize);

                SubmitSegmentDetail();
            }
        });

            
    }
    else {
        $("#sm-segment-object-key").val("");
        $("#sm-segment-object-name").val("");
        $("#sm-segment-object-size").val("");
        SubmitSegmentDetail();
    }
};

SubmitSegmentDetail = function () {
    var frmData = new FormData($('#frm-segment')[0]);
    $.ajax({
        type: "post",
        cache: false,
        url: "/SalesMarketingSegment/SaveSegment",
        enctype: 'multipart/form-data',
        data: frmData,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            isBusyAddTaskForm = true;
        },
        success: function (data) {
            if (data.result) {
                cleanBookNotification.success(_L("ERROR_MSG_204"), "Sales Marketing");
                location.reload();
            } else {
                cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Sales Marketing");
            }
        },
        error: function (data) {
            
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
        }
    }).always(function () {
        isBusyAddTaskForm = false;
        LoadingOverlayEnd();
    });;
};

$(document).ready(function () {
    LoadModalSegment($('#hdfIdSm').val());
});