jQuery(function ($)  {

    //you have to use keyup, because keydown will not catch the currently entered value
    $('#new_password_repeat').keyup(function () {
        var validation = 0;
        // set password variable
        var pswd = $(this).val();

        //validate the length
        if (pswd.length < 8) {
            $('#length_repeat').removeClass('valid').addClass('invalid');
        } else {
            $('#length_repeat').removeClass('invalid').addClass('valid');
            validation++;
        }

        //validate letter
        if (pswd.match(/[A-z]/)) {
            $('#letter_repeat').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#letter_repeat').removeClass('valid').addClass('invalid');
        }

        //validate uppercase letter
        if (pswd.match(/[A-Z]/)) {
            $('#capital_repeat').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#capital_repeat').removeClass('valid').addClass('invalid');
        }

        //validate number
        if (pswd.match(/\d/)) {
            $('#number_repeat').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#number_repeat').removeClass('valid').addClass('invalid');
        }

        if (!pswd.match(/\W+/g)) {
            $('#alpha_repeat').removeClass('valid').addClass('invalid');
        } else {
            $('#alpha_repeat').removeClass('invalid').addClass('valid');
            validation++;
        }

        //matching pass and confirm pass
        if ($('#new_password').val() == $('#new_password_repeat').val()) {
            $('#matching_repeat').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#matching_repeat').removeClass('valid').addClass('invalid');
        }
        //compare with old password
        if ($('#new_password_repeat').val() != $('#old_password').val()) {
            $('#compare_repass').removeClass('invalid').addClass('valid');
            validation++;
        }
        else {
            $('#compare_repass').removeClass('valid').addClass('invalid');
        }
        if (validation == 7 && $('#oldpassstatus').val() == 1)
            $('#set_password').removeAttr('disabled', '');
        else
            $('#set_password').attr('disabled', '');
    }).focus(function () {
        var pos = $('#login').offset();

        var height1 = $(this).outerHeight();
        $("#newpswdrepeat_info").css({
            position: "absolute",
            top: ((pos.top / 2) - 10) + "px",
            left: (pos.left) + "px"
            //top: (pos.top + height1) + "px",
            //left: (0) + "px",
        }).show();
        //matching pass and confirm pass
        if ($('#new_password').val() == $('#new_password_repeat').val()) {
            $('#matching_repeat').removeClass('invalid').addClass('valid');
        } else {
            $('#matching_repeat').removeClass('valid').addClass('invalid');
        }
    }).blur(function () {
        $('#newpswdrepeat_info').hide();
    });


    $('#new_password').keyup(function () {
        // set password variable
        var validation = 0;
        var pswd = $(this).val();
        //validate the length
        if (pswd.length < 8) {
            $('#length').removeClass('valid').addClass('invalid');
        } else {
            $('#length').removeClass('invalid').addClass('valid');
            validation++;
        }

        //validate letter
        if (pswd.match(/[A-z]/)) {
            $('#letter').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#letter').removeClass('valid').addClass('invalid');
        }

        //validate uppercase letter
        if (pswd.match(/[A-Z]/)) {
            $('#capital').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#capital').removeClass('valid').addClass('invalid');
        }

        //validate number
        if (pswd.match(/\d/)) {
            $('#number').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#number').removeClass('valid').addClass('invalid');
        }
        //matching Non-alphanumeric
        if (!pswd.match(/\W+/g)) {
            $('#alpha').removeClass('valid').addClass('invalid');
        } else {
            $('#alpha').removeClass('invalid').addClass('valid');
            validation++;
        }

        //matching pass and confirm pass
        if ($('#new_password').val() == $('#new_password_repeat').val()) {
            $('#matching').removeClass('invalid').addClass('valid');
            validation++;
        }
        else {
            $('#matching').removeClass('valid').addClass('invalid');
        }
        //compare with old password
        if ($('#new_password').val() != $('#old_password').val()) {
            $('#compare').removeClass('invalid').addClass('valid');
            validation++;
        }
        else {
            $('#compare').removeClass('valid').addClass('invalid');
        }
        if (validation == 7 && $('#oldpassstatus').val() == 1)
            $('#set_password').removeAttr('disabled', '');
        else
            $('#set_password').attr('disabled', '');
    }).focus(function () {
        var pos = $('#login').offset();

        var height1 = $(this).outerHeight();
        $("#newpswd_info").css({
            position: "absolute",
            top: ((pos.top/2) -10) + "px",
            left: (pos.left) + "px"
            //top: 'auto'

            //top: (pos.top + height1 / 2) + "px",
            ////left: (pos.left * 2) + "px",
        }).show();
        //matching pass and confirm pass
        if ($('#new_password').val() == $('#new_password_repeat').val()) {
            $('#matching').removeClass('invalid').addClass('valid');
        }
        else {
            $('#matching').removeClass('valid').addClass('invalid');
        }
    }).blur(function () {
        $('#newpswd_info').hide();
    });
});

