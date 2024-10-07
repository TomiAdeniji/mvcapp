
$(function () {
    var settingVal = getCookie('cb_config_tab');
    switch (settingVal) {
        case "workgroup":
            $('a[href="#' + settingVal + '"]').tab('show');
            showCBSetting(settingVal);
            break;
        case "bookkeeping":
            $('a[href="#' + settingVal + '"]').tab('show');
            showCBSetting(settingVal);
            break;
        default:
            $('a[href="#' + settingVal + '"]').tab('show');
            showCBSetting('workgroup');
            break;
    }

});

function showCBSetting(settingVal, callback) {
    setCookie('cb_config_tab', settingVal);
    var ajaxUri = '/Apps/CleanBookConfig?value=' + settingVal;
    LoadingOverlay();
    $('#tab-content-config').empty();
    $('#tab-content-config').load(ajaxUri, function () {
        LoadingOverlayEnd();
        $('a[href="#' + settingVal + '"]').tab('show');
        //initCurrency();
        if (callback) {
            callback();
        }
    });
};
function swichPage() {
    var selectPage = document.getElementById("menu-select").value;
    if (selectPage === "Tasks")
        window.location = "/Apps/Tasks";
    else if (selectPage === "Accounts")
        window.location = "/Apps/Accounts";
    else if (selectPage === "Config")
        window.location = "/Apps/CleanBookConfig";
}