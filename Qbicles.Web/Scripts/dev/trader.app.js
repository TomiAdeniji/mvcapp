$(document).ready(function () {
    ValidateCurrentLocation();
    getCurrencySettings();
});
//Currency Settings
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
function initCurrency() {
    $("#frmCurrencyConfiguration").submit(function (event) {
        event.preventDefault();
        var cSymbol = $('select[name=CurrencySymbol]').val();
        var sDisplay = $('select[name=SymbolDisplay]').val();
        var dPlace = $('select[name=DecimalPlace]').val();
        $.ajax({
            type: 'post',
            url: this.action,
            data: {
                CurrencySymbol: cSymbol,
                SymbolDisplay: sDisplay,
                DecimalPlace: dPlace,
            },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
                    var _event = "resetSettings('" + cSymbol + "','" + sDisplay + "','" + dPlace + "')";
                    $('#btnCurrencyReset').attr("onclick", _event);
                } else
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    });
}
function resetSettings(cSymbol, sDisplay, dPlace) {
    $('select[name=CurrencySymbol]').val(cSymbol).trigger('change');
    $('select[name=SymbolDisplay]').val(sDisplay).trigger('change');
    $('select[name=DecimalPlace]').val(dPlace).trigger('change');
}
//End Currency
               
