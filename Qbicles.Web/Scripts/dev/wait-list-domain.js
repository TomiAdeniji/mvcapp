function JoinTheWaitList() {
    $('#waitlist-modal-body').LoadingOverlay("show");
    var waitList = getWaitlistRequest();
    if (waitList.BusinessCategories.length == 0) {
        cleanBookNotification.error("Your categorise your business is required.", "Qbicles");
        $('#waitlist-modal-body').LoadingOverlay("hide", true);
        return;
    }
    $.ajax({
        type: 'post',
        url: '/Waitlist/JoinTheWaitlist?countryCode=' + $('#wait-country').val(),
        data: { waitList: waitList },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                //$('.waitlist-join-request').hide();
                //$('.waitlist-pending').show();
                CheckApprovalWaitlist($("#user-current-id").val());
                $("#waitlist-modal").modal('hide');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    }).always(function () {
        $('#waitlist-modal-body').LoadingOverlay("hide", true);
    });
}

function getWaitlistRequest() {

    var numberOfEmployees= $('#wait-employees').val();
    if (numberOfEmployees == -1)
        numberOfEmployees = null;

    var discoveredVia = $('#wait-discoveredVia').val();
    if (discoveredVia == -1)
        discoveredVia = null;

    var waitList = {
        CountryCode: $('#wait-country').val(),
        BusinessCategories: [],
        NumberOfEmployees: numberOfEmployees,
        DiscoveredVia: discoveredVia,
        IsApprovedForSubsDomain: false,
        IsApprovedForCustomDomain: false,
        IsRejected: false,
        //User:
    };
    var bCategoryIds = $('#wait-categories').val();
    if (bCategoryIds)
        jQuery.each(bCategoryIds, function (index, item) {
            waitList.BusinessCategories.push({ Id: item });
        });

    return waitList;
}