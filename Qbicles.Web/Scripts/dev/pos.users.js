IncludeInPosPool = function (memberId, posUserType) {

    var posUser = {
        Id: 0,
        User: { Id: memberId },
        UserType: posUserType
    };

    console.log({ posUser });

    var url = "/PointOfSaleDevice/CreatePosUser";

    var action = $("#check-"+memberId).is(':checked');
    if (action)
        url = "/PointOfSaleDevice/DeletePosUser";
   
    $.ajax({
        url: url,
        type: "post",
        dataType: "json",
        data: { posUser: posUser },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.createSuccess();
                $('.pin-' + memberId).toggle();
                if (url === "/PointOfSaleDevice/DeletePosUser") {

                    $('#generate-pin-button-' + memberId + ' div.general-pin').text("Generate PIN for till user");

                    $(".pin-" + memberId).removeClass('btn-info').addClass('btn-primary');
                }
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {            
            cleanBookNotification.error(err,"Qbicles");
        }
    }).always(function () {
        
    });
};


GeneratePin = function (memberId, posUserType) {
 
    $('#generate-pin-button-' + memberId + ' div.loading-pin').removeClass('hidden');
    $('#generate-pin-button-' + memberId + ' div.general-pin').addClass('hidden');
    $('#generate-pin-button-' + memberId).addClass('animated shake disabled');

    $('.check-pool-column').addClass('disabled');
  
    var posUser = {
        Id: 0,
        User: { Id: memberId },
        UserType: posUserType
    };

    var url = "/PointOfSaleDevice/GeneratePin";

    $.ajax({
        url: url,
        type: "POST",
        dataType: "json",
        data: { posUser: posUser },
        success: function (rs) {
            if (rs.result) {
                cleanBookNotification.createSuccess();

                $('#generate-pin-button-' + memberId + ' div.general-pin').text("Regenerate PIN");

                $(".pin-" + memberId).removeClass('btn-primary').addClass('btn-info');

                //$('#generate-pin-button-' + memberId).attr('onClick', 'ReGeneratePin("' + memberId + '")');

                $('#generate-pin-button-' + memberId).removeClass('animated shake disabled');

                $('.check-pool-column').removeClass('disabled');

                $('#generate-pin-button-' + memberId + ' div.loading-pin').addClass('hidden');
                $('#generate-pin-button-' + memberId + ' div.general-pin').removeClass('hidden');
            } else {
                cleanBookNotification.error(rs.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err, "Qbicles");
            
        }
    }).always(function () {
        
        $('#generate-pin-button-' + memberId).removeClass('animated shake disabled');

        $('.check-pool-column').removeClass('disabled');

        $('#generate-pin-button-' + memberId + ' div.loading-pin').addClass('hidden');
        $('#generate-pin-button-' + memberId + ' div.general-pin').removeClass('hidden');
    });

};
