
LearnMoreApp = function (appId) {
    $("#app-detail-image").attr("src", $("#app-image-" + appId).text());
    $("#app-detail-name").text($("#app-name-" + appId).text());
    $("#app-detail-description").text($("#app-description-" + appId).text());
    $("#app-detail-adpage").text($("#app-adpage-" + appId).text());

    $("#app-detail").modal('show');
}
function opentrader() {
    $.ajax({
        type: 'get',
        url: '/Trader/CheckTrader',
        datatype: 'json',
        success: function (response) {
            if (response) {
                window.location.href = '/Trader/AppTrader';
            } else {
                window.location.href = '/Trader/TraderSetup';
            }
        },
        error: function (err) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
});
}
$(document).ready(function () {
    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: DBformatOptions,
        templateSelection: DBformatSelected,
        dropdownCssClass: 'withdrop'
    });
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