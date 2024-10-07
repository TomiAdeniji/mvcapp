var $orderType = '';

function OpenOrderContextFlyout(type) {   
    $orderType = type;
    var ajaxUri = '/B2C/OpenOrderContextFlyout';
    $('#orders-bview').LoadingOverlay("show");
    $('#orders-bview').empty();
    $('#orders-bview').load(ajaxUri, function () {
        $('#orders-bview').LoadingOverlay("hide", true);
        $("#orders-bview").modal('show');
        if (type == 'c2c')
            $(".order-context-flyout-modal-title").text("Your orders");
        else
            $(".order-context-flyout-modal-title").text($commonName + "'s orders");
    });

}


