
function formatOptions(state) {

    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function formatSelected(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') === '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function getCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
                    CurrencySymbol: '',
                    SymbolDisplay: 0,
                    DecimalPlace: 2
                };
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
jQuery(function ($) {
    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: formatOptions,
        templateSelection: formatSelected,
        dropdownCssClass: 'withdrop'
    });
    //getCurrencySettings();
});