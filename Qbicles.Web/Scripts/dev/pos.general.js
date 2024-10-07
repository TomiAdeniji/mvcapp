
var $settingId = $('#setting_id').val();
var $locationId = $('#local-manage-select').val();


$(document).ready(function () {
    $('.select2Pos').select2({ placeholder: 'Please select' });
    $('.checkbox.toggle input').bootstrapToggle();

    $("#pos_placeholder_image").change(function () {
        var target = $(this).data('target');
        readURL(this, target);
        $(target).fadeIn();
    });

    function readURL(input, target) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $(target).attr('src', e.target.result);
            }

            reader.readAsDataURL(input.files[0]);
        }

    }
});

function SettingChanged() {

    var maxContactResults = $('#max-contact-result').val();
    if (maxContactResults > 100 || maxContactResults < 0) {
        cleanBookNotification.warning("POS Contact results return between 1 to 100.", "Qbicles");
        $('#max-contact-result').val(100);
        return;
    }
    $.LoadingOverlay("show");
    if ($('#pos_placeholder_image').val() !== '') {
        UploadMediaS3ClientSide("pos_placeholder_image").then(function (mediaS3Object) {

            $('#pos_placeholder_image_key').val(mediaS3Object.objectKey);
            ConfirmChanged();
        });
    }
    else
        ConfirmChanged();
}


function ConfirmChanged() {

    var setting = {
        Id: $('#setting_id').val(),
        Location: { Id: $locationId },
        DefaultWorkGroup: { Id: $('#setting_workgroup').val() },
        DefaultWalkinCustomer: { Id: $('#setting_contact').val() },
        RolloverTime: $('#rollover-time').val(),
        MaxContactResult: $('#max-contact-result').val(),
        MoneyCurrency: $('#pos_monney_currency').val(),
        MoneyDecimalPlaces: $('#pos_decimal_place').val(),
        ReceiptHeader: $('#pos_receipt_header').val(),
        ReceiptFooter: $('#pos_receipt_footer').val(),
        ProductPlaceholderImage: $('#pos_placeholder_image_key').val(),
        SourceQbicle: { Id: $('#wg-qbicle').val() },
        DefaultTopic: { Id: $('#wg-topic').val() },
        OrderStatusWhenAddedToQueue: $("#pos-order").val()
    }
    var isValid = true;
    if (!setting.ReceiptHeader || (setting.ReceiptHeader && setting.ReceiptHeader === "")) {
        $("#pos_general_form").validate().showErrors({ rhead: "Receipt header is Required." });
        isValid = false;
    }
    if (!setting.ReceiptFooter || (setting.ReceiptFooter && setting.ReceiptFooter === "")) {
        $("#pos_general_form").validate().showErrors({ rfoot: "Receipt footer is Required." });
        isValid = false;
    }
    if (!isValid) return;


    $.ajax({
        type: 'post',
        url: "/PointOfSale/SaveSetting",
        data: { posSetting: setting },
        dataType: 'json',

        success: function (rs) {

            if (rs.actionVal === 1) {
                cleanBookNotification.createSuccess();
                $('#setting_id').val(rs.msgId);
            } else if (rs.actionVal === 2) {
                cleanBookNotification.updateSuccess();
                $('#setting_id').val(rs.msgId);
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}