function setRequestStatus(apprKey, status, userId) {

    if (status === '') {
        status = $("#action_status").val();
    }
    LoadingOverlay();
    $.ajax({
        url: "/Qbicles/SetRequestStatusForApprovalRequest",
        type: "GET",
        dataType: "json",
        data: { appKey: apprKey, status: status },
        success: function (rs) {
            if (rs.actionVal > 0) {
                setLocalStorage("isUpdate", "1");
                location.reload();
                return;
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            LoadingOverlayEnd();
        }

    }).always(function() {
        
    });
};

$(document).ready(function () {
    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: DBformatOptions,
        templateSelection: DBformatSelected,
        dropdownCssClass: 'withdrop'
    });
    function DBformatOptions(state) {

        if (!state.id) { return state.text; }
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }

    function DBformatSelected(state) {
        if (!state.id) { return state.text; }
        var $state = $(
            '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
        );
        return $state;
    }
});
