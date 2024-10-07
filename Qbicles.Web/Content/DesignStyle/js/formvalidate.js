jQuery.fn.extend({
    toggleText: function (a, b) {
        var that = this;
        if (that.text() !== a && that.text() !== b) {
            that.text(a);
        }
        else
            if (that.text() === a) {
                that.text(b);
            }
            else
                if (that.text() === b) {
                    that.text(a);
                }
        return this;
    }
});

$(function () {

    $('#form-profile-edit').validate(
        {
            rules: {
                Forename: {
                    required: true,
                    maxlength: 35
                },
                Surname: {
                    required: true,
                    maxlength: 35
                },
                Email: {
                    required: true,
                    email: true,
                    maxlength: 300
                },
                profile: {
                    required: true
                },
                UserName: {
                    required: true,
                    maxlength: 300
                }
                //password: {
                //    required: true,
                //    maxlength: 300
                //},
                //password2: {
                //    required: true,
                //    maxlength: 300,
                //    equalTo: '#password'
                //}
            }
        });
    // Login form validation
    $('#form_Login').validate(
        {
            rules: {
                email: {
                    required: true,
                    email: true,
                    maxlength: 300
                },
                password: {
                    required: true,
                    maxlength: 300
                }
            }
        });
    // Change password validation
    $('#form_changepassword').validate(
        {
            rules: {
                email: {
                    required: true,
                    minlength: 6,
                    maxlength: 254,
                    email: true
                }
            }
        });
    // Add/Edit Qbicle validation
    $('#form_qbicle_addedit').validate(
        {
            rules: {
                Name: {
                    required: true,
                    minlength: 5,
                    maxlength: 50
                },
                qbicleLogo: {
                    filesize: true
                },
                Description: {
                    maxlength: 350
                }
            }
        });
    // Add/Edit Task validation
    $('#general-seting-account').validate({
        rules: {
            account_name: {
                required: true,
                maxlength: 300
            }
        }
    });

    $('#fromSearchQbicle').validate(
        {
            rules: {
                search: {
                    //spacesonly: true,
                    minlength: 3
                }
            }
        });
    //Developer End validate

    // Reset password
    $('#password-reset').validate(
        {
            rules: {
                password: {
                    required: true,
                    maxlength: 300
                },
                confirm_password: {
                    required: true,
                    maxlength: 300,
                    equalTo: '#password'
                }
            }
        });

    // Create task
    $('#form_task_create').validate(
        {
            ignore: 'input[type=hidden]',
            errorClass: "error",
            errorPlacement: function (error, element) {
                var elem = $(element);
                error.insertAfter(element);
            },
            rules: {
                task_title: {
                    required: true,
                    maxlength: 300
                },
                assign_to: {
                    required: true
                }
            },
            highlight: function (element, errorClass, validClass) {

                var elem = $(element);

                elem.addClass(errorClass);

            },
            unhighlight: function (element, errorClass, validClass) {
                var elem = $(element);

                if (elem.hasClass('sl')) {
                    elem.siblings('.sl').find('.select2-choice').removeClass(errorClass);
                } else {
                    elem.removeClass(errorClass);
                }
            }
        });
    $('#form-create-folder').validate(
        {
            rules: {
                Name: {
                    required: true,
                    //spacesonly: true,
                    minlength: 5
                }
            }
        });
    $('#form-update-folder').validate(
        {
            rules: {
                Name: {
                    required: true,
                    //spacesonly: true,
                    minlength: 5
                }
            }
        });


    $.validator.addMethod(
        "validatePassword",
        function (value, element) {
            var validation = 0;
            //validate the length
            if (value.length < 8) {
                $('#length').removeClass('green').addClass('red');
                validation++;
            } else {
                $('#length').removeClass('red').addClass('green');
            }

            //validate letter
            if (value.match(/[a-z]/)) {
                $('#letter').removeClass('red').addClass('green');
            } else {
                $('#letter').removeClass('green').addClass('red');
                validation++;
            }

            //validate uppercase letter
            if (value.match(/[A-Z]/)) {
                $('#capital').removeClass('red').addClass('green');
            } else {
                $('#capital').removeClass('green').addClass('red');
                validation++;
            }

            //validate number
            if (value.match(/\d/)) {
                $('#number').removeClass('red').addClass('green');
            } else {
                $('#number').removeClass('green').addClass('red');
                validation++;
            }

            //matching Non-alphanumeric
            if (!value.match(/\W+/g)) {
                $('#alpha').removeClass('green').addClass('red');
                validation++;
            } else {
                $('#alpha').removeClass('red').addClass('green');
            }

            if (validation > 0) {
                return false;
            } else {
                return true;
            }

        },
        "The entered password does not meet the acceptance criteria!"
    );

    $('#create-account-main').validate(
        {
            rules: {
                company: {
                    required: true,
                    maxlength: 300
                },
                username: {
                    required: true,
                    maxlength: 150
                },
                accountname: {
                    required: true,
                    maxlength: 150
                },
                domain: {
                    required: true,
                    maxlength: 150
                },
                forename: {
                    required: true,
                    maxlength: 35
                },
                surname: {
                    required: true,
                    maxlength: 35
                },
                email: {
                    required: true,
                    maxlength: 300
                },
                password: {
                    //required: true,
                    //pwcheck: true,
                    //minlength: 8,
                    //maxlength: 300
                    validatePassword: true
                },
                password_repeat: {
                    equalTo: '#password',
                    required: true,
                    minlength: 8,
                    maxlength: 300
                }
            },
            messages: {
                //password: {
                //    pwcheck: "The entered password does not meet the acceptance criteria!",
                //}
            },
            submitHandler: function (form) {
                if (grecaptcha.getResponse()) {
                    //form.submit();
                } else {
                    //form.submit();
                    alert('Please complete the reCaptcha to proceed');
                }
            }
        });

    $('#create-user-main').validate(
        {
            rules: {

                username: {
                    required: true,
                    maxlength: 150
                },
                email: {
                    required: true,
                    maxlength: 300
                },
                password: {
                    required: true,
                    maxlength: 300,
                    minlength: 8
                },
                password_repeat: {
                    required: true,
                    maxlength: 300,
                    equalTo: '#password',
                    minlength: 8
                },
                messages: {
                    password: {
                        pwcheck: "Das Passwort entspricht nicht den Kriterien!",
                    }
                }
            }
        });

    $('#create-participant-domain').validate(
        {
            rules: {
                'usersDomainAssign[]': {
                    required: true
                }
            }
        });

    $('#create-guest-discussion-main').validate(
        {
            rules: {

                emailGuest: {
                    required: true,
                    maxlength: 300
                }
            }
        });

    $('#create-task-form').validate(
        {
            rules: {
                name: {
                    required: true
                },
                description: {
                    required: true
                }
            }
        });

    $('#form-process-group').validate(
        {
            rules: {
                name: {
                    required: true,
                    maxlength: 300
                }
            }
        });

    $('#form_process_step_addedit').validate(
        {
            rules: {
                Title: {
                    required: true,
                    maxlength: 300
                },
                Description: {
                    required: true,
                    maxlength: 3000
                }
            }
        });

    $('#form_process_related_addedit').validate(
        {
            rules: {
                Document: {
                    required: true,
                    maxlength: 1500
                }
            }
        });

    $('#form_process_addedit').validate(
        {
            rules: {
                Name: {
                    required: true,
                    maxlength: 300
                },
                Description: {
                    required: true,
                    maxlength: 3000
                },
                StartState: {
                    required: true,
                    maxlength: 300
                },
                EndState: {
                    required: true,
                    maxlength: 300
                }
            }
        });
    $('#form_Document_addedit').validate(
        {
            rules: {
                Document: {
                    required: true,
                    maxlength: 1500
                }
            }
        });

    $('#form_DocumentTask_addedit').validate(
        {
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                },
                Description: {
                    required: true
                }

            }
        });
    $('#form-approval-group').validate(
        {
            rules: {
                name: {
                    required: true,
                    maxlength: 300
                }
            }
        });

    $('#form_approval_app_addedit').validate(
        {
            rules: {
                Title: {
                    required: true,
                    maxlength: 300
                },
                Description: {
                    required: true,
                    maxlength: 3000
                }
            }
        });

    $('#form-create-media-folder').validate(
        {
            rules: {
                Name: {
                    required: true,
                    minlength: 5
                }
            }
        });

    $('#form-update-del-media-folder').validate(
        {
            rules: {
                nameMFedit: {
                    required: true,
                    minlength: 5
                }
            }
        });


    $.validator.addMethod("pwcheck", function (value) {
        return /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^\w\s]).{8,}$/.test(value) // consists of only these
        //&& /[a-z]/.test(value) // has a lowercase letter
        //&& /\d/.test(value)); // has a digit
    });
    var _msgerror;
    $.validator.addMethod('filesize', function (value, element, arg) {
        var _file = element.files ? element.files[0] : null;
        var _filename = _file ? _file.name : "";
        var _uploadInfo = getLimitFileSize(_filename);
        if (_file && _file.size > parseInt(_uploadInfo.MaxFileSize)) {
            var cusMsg = $(element).attr("cus-msg-filesize");
            if (cusMsg)
                _msgerror = _L(cusMsg, [bytesToSize(parseInt(_uploadInfo.MaxFileSize))]);
            else
                _msgerror = _L("ERROR_MSG_399", [_uploadInfo.Extension.toUpperCase(), bytesToSize(parseInt(_uploadInfo.MaxFileSize))]);
            return false;
        } else {
            return true;
        }
    }, function () { return _msgerror; });
    //accountgroup validate
    $("#form_group").validate({
        ignore: 'input[type="button"],input[type="submit"]',
        rules: {
            Name: {
                required: true,
                maxlength: 150
            }
        }
    });

    $("#form_accout").validate({
        ignore: 'input[type="button"],input[type="submit"]',
        rules: {
            Name: {
                required: true,
                maxlength: 150
            },
            Number: {
                required: true,
                maxlength: 45
            },
            "uploadfields[]": {
                required: true
            }
        }
    });
});


//Developer js
function ClearError() {
    $("label.error").hide();
    $(".error").removeClass("error");
    $("label.valid").hide();
    $(".valid").removeClass("valid");
}

function ValidateFileImage(ev) {
    var extension = $(ev).val().substr($(ev).val().lastIndexOf(".") + 1, $(ev).val().length).toLowerCase();
    if (extension === "jpg" || extension === "jpeg" || extension === "png" || extension === "gif" || extension === "tif" || extension === "tiff" || extension === "bmp") {
        //TO DO
        return true;
    } else {
        $(ev).val("");
        cleanBookNotification.error(_L("ERROR_MSG_570"), "Qbicles");
        return false;
    }
}