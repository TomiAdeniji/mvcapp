var element = document.getElementById("user-avata-2");
var _isBusy = false;
var wto;
$(document).ready(function () {
    getUserProfilePages();
    var _tabactive = getQuerystring('tab');
    switch (_tabactive) {
        case 'profile-pages':
            reloadUserProfilePages();
            $('a[href=#' + _tabactive + ']').tab('show');
            break;
        default:
            break;
    }
    $('#profile-pages input[name=keyword]').keyup(delay(function () {
        reloadUserProfilePages();
    }, 1000));
    $('#profile-pages select[name=status]').change(function () {
        reloadUserProfilePages();
    });
    $('select[name=interests]').change(function () {
        clearTimeout(wto);
        wto = setTimeout(function () {
            saveInterests();
        }, 1000);
    });
});
//check upload file here
function avatarChange(sender) {
    readURL(sender);
}
function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#imgAvatar').show();
            $('#imgAvatar').attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function initFormEditProfile() {
    var $frm_profile = $('#form-profile-general-edit');
    $frm_profile.validate({
        rules: {
            DisplayUserName: {
                maxlength: 71
            },
            forename: {
                required: true,
                maxlength: 35
            },
            surname: {
                required: true,
                maxlength: 35
            },
            Company: {
                maxlength: 500
            },
            JobTitle: {
                maxlength: 500
            },
            tagline: {
                maxlength: 50
            },
            description: {
                required: true,
                maxlength: 500
            }
        }
    });
    $frm_profile.submit(function (e) {
        e.preventDefault();
        if (_isBusy)
            return;

        if ($frm_profile.valid()) {
            $.LoadingOverlay("show");
            //var fullName = $("#FullName").val();
            //var _forename = fullName.split(' ')[0];
            //var _surname = fullName.substring(_forename.length).trim();
            //if (_forename.trim() == "" || _surname.trim() == "") {
            //    $frm_profile.validate().showErrors({ FullName: "Forename and Surname of the user are required" });
            //    LoadingOverlayEnd();
            //}

            //$.ajax({
            //    url: "/Administration/DuplicateEmail",
            //    data: { Email: $("#txtEmail").val() },
            //    type: "GET",
            //    dataType: "json",
            //    async: false,
            //}).done(function (refModel) {
            //    if (!refModel.result) {
            //        switch (refModel.msgId) {
            //            case "2":
            //                $frm_profile.validate().showErrors({ Email: "Email '" + $("#txtEmail").val() + "' already exists." });
            //                break;
            //        }
            //        return;
            //    }
            //}).fail(function () {
            //    $("#FullName").validate().showErrors({ FullName: _L("ERROR_MSG_120") });
            //})
            var _url = "/Administration/SaveGeneralProfile";

            var userInfor = {
                DisplayUserName: $('#txtDisplayUserName').val(),
                Forename: $('#txtforename').val(),
                Surname: $('#txtsurname').val(),
                Tagline: $("#txtTagline").val(),
                Company: $("#txtCompanyName").val(),
                Tell: $("#Telnumber").val(),
                JobTitle: $("#JobTitle").val(),
                Description: $("#UserDescription").val(),
                FacebookLink: $("#facebooklink").val(),
                InstagramLink: $("#instagramlink").val(),
                LinkedlnLink: $("#linkedInlink").val(),
                TwitterLink: $("#twitterlink").val(),
                WhatsApp: $("#whatsapplink").val()
            };

            $.ajax({
                method: 'POST',
                cache: false,
                dataType: 'JSON',
                url: _url,
                data: {
                    user: userInfor
                },
                beforeSend: function (xhr) {
                    _isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        cleanBookNotification.success(_L("ERROR_MSG_121"), "Qbicles");
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Qbicles");
                    }
                    _isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    _isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}

function initFormEditSettings() {
    var $frm_settings = $('#save-settings-form');
    $frm_settings.validate({
        rules: {
            Timezone: {
                required: true,
            },
            TimeFormat: {
                required: true,
            },
            dateformat: {
                required: true,
            },
            ChosenNotificationMethod: {
                required: true
            },
            NotificationSound: {
                required: true
            }
        }
    });
    $frm_settings.submit(function (e) {
        e.preventDefault();
        if (_isBusy)
            return;

        if ($frm_settings.valid()) {
            $.LoadingOverlay("show");
            var _url = "/Administration/SaveUserSettings";

            var userInfor = {
                Timezone: $("#timezone-setting").val(),
                TimeFormat: $("#timeformat-setting").val(),
                DateFormat: $("#dateformat-setting").val(),
                ChosenNotificationMethod: $("#ChosenNotificationMethod").val(),
                NotificationSound: $("#NotificationSound").val(),
                PreferredDomain_Key: $("#sl_PreferredDomain").val(),
                PreferredQbicle_Id: $("#sl_PreferredQbicle").val()
            };

            $.ajax({
                method: 'POST',
                cache: false,
                dataType: 'JSON',
                url: _url,
                data: {
                    user: userInfor
                },
                beforeSend: function (xhr) {
                    _isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        cleanBookNotification.success(_L("ERROR_MSG_121"), "Qbicles");
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Qbicles");
                    }
                    _isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    _isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}


function loadPreferredQbicles() {
    $('#sl_PreferredQbicle').empty();
    $.getJSON("/Administration/loadPreferredQbicles", { domainKey: $('#sl_PreferredDomain').val() }, function (data) {
        if (data && data.length > 0) {
            $('.prefqbicle').fadeIn();
            $('#sl_PreferredQbicle').select2({
                data: data,
                placeholder: "Please select"
            });
        } else {
            $('.prefqbicle').fadeOut();
        }
    });
}
function SetSelectedAvata(URI, el) {
    $("#userAvataUrl").val(URI);
    $('a.avt-elm').removeClass('selected');
    $('a.avt-elm').addClass('options');
    $(el).addClass('selected');
    $(el).removeClass('options');
}
function SetAvatar() {
    $.LoadingOverlay("show");
    $.post("/Administration/SetAvatar", { userAvatar: $("#userAvataUrl").val() }, function (data) {
        if (data.result) {
            $('#manage-avatar').modal("hide");
            var avatarUrl = $('#hdfApiFile').val() + data.Object + '&size=S';
            $('#dv-avatar').css('background-image', 'url(' + avatarUrl + ')');
            var iconUrl = $('#hdfApiFile').val() + data.Object + '&size=T';
            $("#profile-icon").css('background-image', 'url(' + iconUrl + ')');
        } else {
            cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_502"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}
function CancelChosseAvata() {
    $("#userAvataUrl").val('');
}

function UpdatePass() {
    $('#password-reset').submit(function (e) {
        e.preventDefault();
        if (_isBusy)
            return;
        if ($('#msg-criteria .invalid').length == 0 && $('#password-reset').valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    _isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        cleanBookNotification.success(_L("ERROR_MSG_123"), "Qbicles");
                        $('#change-password').modal('hide');
                    } else {
                        cleanBookNotification.error(data.msg, "Qbicles");
                    }
                    _isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    _isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}
function CancelUpdatePass() {
    $('#password-reset').trigger("reset");
    $('#password-reset').resetForm();
}
function privacyOptions() {
    $('#frm-privacyOptions').submit(function (e) {
        e.preventDefault();
        if (_isBusy)
            return;
        $.LoadingOverlay("show");
        $.ajax({
            type: this.method,
            cache: false,
            url: this.action,
            data: $(this).serialize(),
            beforeSend: function (xhr) {
                _isBusy = true;
            },
            success: function (data) {
                if (data.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_124"), "Qbicles");
                    $('#profile-permissions').modal('hide');
                } else {
                    cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Qbicles");
                }
                _isBusy = false;
                LoadingOverlayEnd();
            },
            error: function (data) {
                _isBusy = false;
                LoadingOverlayEnd();
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    });
}
function loadEmloymentHistory() {
    $('#tbl-profile-history').dataTable({
        destroy: true,
        ajax: "/Administration/GetEmploymentHistory",
        ordering: false,
        columns: [
            { "title": "Employer", "data": "Employer", "searchable": true },
            { "title": "Role", "data": "Role", "searchable": true },
            { "title": "Dates", "data": "Dates", "searchable": true },
            { "title": "Summary", "data": "Summary", "searchable": true },
            null,
        ],
        columnDefs: [{
            "targets": 4,
            "data": "Id",
            "render": function (data, type, row, meta) {
                var _elmactions = '<button class="btn btn-warning" data-toggle="modal" onclick="loadDataEmploymentEdit(' + data + ')" data-target="#app-community-add-work-history"><i class="fa fa-pencil"></i></button>';
                _elmactions += '&nbsp;<button class="btn btn-danger" onclick="deleteEmploymentHistoryConfirm(' + data + ')"><i class="fa fa-trash"></i></button>';
                return _elmactions;
            }
        }]
    });
}

//Address
function loadUserAddresses() {
    $('#tbl-address-list').dataTable({
        destroy: true,
        ajax: "/Administration/GetUserAddresses",
        ordering: false,
        columns: [
            { "title": "Address", "data": "AddressFull", "searchable": true },
            null,
            null,
        ],
        columnDefs: [{
            "targets": 1,
            "data": "IsDefault",
            "render": function (data, type, row, meta) {
                var isChosen = "";
                if (row.IsDefault) {
                    isChosen = "checked";
                }
                var isDefaultStr = '<input onClick="SetUserDefaultLocation(\'' + row.Key + '\')" type="radio" ' + isChosen + ' name = "default' + row.Id + '" > ';
                return isDefaultStr;
            }
        },
        {
            "targets": 2,
            "data": "",
            "render": function (data, type, row, meta) {
                var _editButtonStr = '<button class="btn btn-warning" onclick="CreateEditUserAddressShow(\'' + row.Key + '\')"><i class="fa fa-pencil"></i></button>';
                var _deleteButtonStr = '&nbsp;<button class="btn btn-danger" onclick="DeleteAddress(\'' + row.Key + '\')"><i class="fa fa-trash"></i></button>';
                var str = _editButtonStr + _deleteButtonStr;
                return str;
            }
        }]
    });
}
CreateEditUserAddressShow = function (addressKey) {
    LoadingOverlay();

    $('#create-address').empty();
    var _url = '/UserInformation/TraderAddressEditViewShow?addressKey=' + addressKey;
    AjaxElementShowModal(_url, "create-address");
    $('#create-address').fadeIn();

    LoadingOverlayEnd();
}

function SetUserDefaultLocation(addressKey) {
    LoadingOverlay();

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: '/UserInformation/SetUserDefaultLocation?addressKey=' + addressKey,
        success: function (response) {
            if (response.result) {
                var tbl = $("#tbl-address-list");
                if (tbl != null) {
                    tbl.DataTable().ajax.reload();
                    cleanBookNotification.updateSuccess();
                }

                var addrListWizard = $("#wizard-addresses");
                if (addrListWizard != null) {
                    showListWizardAddresses();
                }
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    })
}

DeleteAddress = function (addressKey) {
    LoadingOverlay();

    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: '/UserInformation/DeleteUserAddress?addressKey=' + addressKey,
        success: function (response) {
            if (response.result) {
                var tbl = $("#tbl-address-list");
                if (tbl != null) {
                    tbl.DataTable().ajax.reload();
                    cleanBookNotification.updateSuccess();
                }

                var addrListWizard = $("#wizard-addresses");
                if (addrListWizard != null) {
                    showListWizardAddresses();
                }

            } else {
                cleanBookNotification.error("The Address is linking to something else and can not be deleted.", "Qbicles");
            }
            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    })
}
function SaveAddress() {
    LoadingOverlay();

    var add1 = $("#add1").val();
    var add2 = $("#add2").val();
    var city = $("#city").val();
    var state = $("#state").val();
    var country = $("#country").val();
    var postcode = $("#postcode").val();
    var isDefault = $("#isDefault").prop("checked");

    var addressModel = {
        Key: $('#address-id').val(),
        AddressLine1: add1,
        AddressLine2: add2,
        City: city,
        State: state,
        PostCode: postcode,
        IsDefault: isDefault
    }

    $.ajax({
        type: 'post',
        datatype: 'json',
        url: '/UserInformation/SaveAddress?country=' + country,
        data: { mdAddress: addressModel},
        success: function (response) {
            $('#receiveMethod').find('option[value="existing-addresses"]').prop('selected', 'true').trigger('change');
            var tbl = $("#tbl-address-list");
            if (tbl != null) {
                tbl.DataTable().ajax.reload();
                cleanBookNotification.updateSuccess();
            }

            var noAddressBlock = $("#noaddresses");
            if (noAddressBlock != null) {
                listUserAddressesShow();
            }

            var addrListWizard = $("#wizard-addresses");
            if (addrListWizard != null) {
                $("#noaddress").hide();
                showListWizardAddresses();
                $("#address").show();
            }

            LoadingOverlayEnd();
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
            LoadingOverlayEnd();
        }
    })

    LoadingOverlayEnd();
}
//End Address

function deleteEmploymentHistoryConfirm(id) {
    $('#emp-delete-confirm').modal("show");
    $('#emp_deleteId').val(id);
}
function deleteEmploymentHistory() {
    var id = $('#emp_deleteId').val();
    $.LoadingOverlay("show");
    $.post("/Administration/DeleteEmploymentById", { id: id }, function (data) {
        if (data.result) {
            $('#tbl-profile-history').DataTable().ajax.reload();
            cleanBookNotification.success(_L("ERROR_MSG_125"), "Qbicles");
        } else {
            cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_504"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}
function deleteMyFileConfirm(id) {
    $('#file-delete-confirm').modal("show");
    $('#file_deleteId').val(id);
}
function deleteMyFile() {
    var id = $('#file_deleteId').val();
    $.LoadingOverlay("show");
    $.post("/Administration/DeleteProFileById", { id: id }, function (data) {
        if (data.result) {
            $('#tbl-my-files').DataTable().ajax.reload();
            cleanBookNotification.success(_L("ERROR_MSG_127"), "Qbicles");
        } else {
            cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_505"), "Qbicles");
        }
        LoadingOverlayEnd();
    });
}
function loadDataEmploymentEdit(id) {
    $.post("/Administration/getEmploymentById", { id: id }, function (data) {
        if (data) {
            $('#app-community-add-work-history h5.modal-title').text("Edit employment history entry");
            $('#frm-employment-history input[name=Employer]').val(data.Employer);
            $('#frm-employment-history input[name=Id]').val(data.Id);
            $('#frm-employment-history input[name=Role]').val(data.Role);
            $('#frm-employment-history input[name=Role]').val(data.Role);
            $('#frm-employment-history textarea[name=Summary]').val(data.Summary);
            $('#hdf-StartDate').val(data.StartDate);
            $('#hdf-EndDate').val(data.EndDate);
            $('#workhistory-dates').val(data.StartDate + ' - ' + data.EndDate);
        }
    });
}
function loadMyFiles() {
    $('#tbl-my-files').dataTable({
        destroy: true,
        ajax: "/Administration/GeMyfiles",
        columns: [
            { "title": "Title", "data": "Title", "searchable": true },
            { "title": "Added", "data": "Added", "searchable": true },
            { "title": "Type", "data": "Type", "searchable": true },
            { "title": "Description", "data": "Description", "searchable": true },
            null,
        ],
        columnDefs: [{
            "targets": 4,
            "data": "Id",
            "render": function (data, type, row, meta) {
                var _elmactions = '<button class="btn btn-danger" onclick="deleteMyFileConfirm(' + data + ')"><i class="fa fa-trash"></i></button>';
                return _elmactions;
            }
        }]
    });
}
function SaveEmployment() {
    var $frm_employment_history = $('#frm-employment-history');
    $frm_employment_history.validate({
        rules: {
            Employer: {
                required: true,
                maxlength: 500,
                minlength: 5
            },
            Role: {
                required: true,
                maxlength: 500
            },
            Summary: {
                required: true,
                minlength: 5,
                maxlength: 1500
            }
        }
    });
    $frm_employment_history.submit(function (e) {
        e.preventDefault();
        if (_isBusy)
            return;
        if ($frm_employment_history.valid()) {
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: $(this).serialize(),
                beforeSend: function (xhr) {
                    _isBusy = true;
                },
                success: function (data) {
                    if (data.result) {
                        $('#tbl-profile-history').DataTable().ajax.reload();
                        $('#app-community-add-work-history').modal('hide');
                        resetFormEmployment();
                        var id = $('#frm-employment-history input[name=Id]').val();
                        if (id === '0')
                            cleanBookNotification.success(_L("ERROR_MSG_365"), "Qbicles");
                        else
                            cleanBookNotification.success(_L("ERROR_MSG_366"), "Qbicles");
                    } else {
                        cleanBookNotification.error(data.msg ? data.msg : "Have an error!", "Qbicles");
                    }
                    _isBusy = false;
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    _isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            });
        }
    });
}

function resetFormEmployment() {
    $('#frm-employment-history input[name=Id]').val("0");
    $('#app-community-add-work-history h5.modal-title').text("Add an employment history entry");
    $('#frm-employment-history')[0].reset();
    $('#frm-employment-history').validate().resetForm();
}
function onloadProfilePage() {
    $('#workhistory-dates').daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: 'DD/MM/YYYY'
        }
    });
    $('#workhistory-dates').on('apply.daterangepicker', function (ev, picker) {
        var _startDate = picker.startDate.format('DD/MM/YYYY');
        var _endDate = picker.endDate.format('DD/MM/YYYY');
        $(this).val(_startDate + ' - ' + _endDate);
        $('#hdf-StartDate').val(_startDate);
        $('#hdf-EndDate').val(_endDate);
    });
    //formProfile();
    UpdatePass();
    privacyOptions();

    SaveEmployment();
    loadEmloymentHistory();
    loadMyFiles();
    loadUserAddresses();

    $('.toggle-pw').bind('click', function (e) {
        e.preventDefault();

        var type = $('#new_password').attr('type');

        if (type == "password") {
            $('#new_password').prop('type', 'text');
        } else {
            $('#new_password').prop('type', 'password');
        }

        $('i', this).toggleClass('fa-eye').toggleClass('fa-eye-slash');
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
        if (pswd.match(/\W+/g)) {
            $('#alpha').removeClass('invalid').addClass('valid');
            validation++;
        } else {
            $('#alpha').removeClass('valid').addClass('invalid');
        }
    });
};


function UploadUserMyFile() {
    var $frm_my_files = $('#frm-my-files');
    if ($frm_my_files.valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("my-file-upload").files;

        if (files && files.length > 0) {
            UploadMediaS3ClientSide("my-file-upload").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {

                    $("#user-my-file-object-key").val(mediaS3Object.objectKey);
                    $("#user-my-file-object-name").val(mediaS3Object.fileName);
                    $("#user-my-file-object-size").val(mediaS3Object.fileSize);

                    SaleUserMyFile();
                }
            });


        } else {
            cleanBookNotification.error("Missing file upload!", "Qbicles");
        }
    }
};

function SaleUserMyFile() {
    $.ajax({
        type: "post",
        cache: false,
        url: "/Administration/SaleUserMyFile",
        enctype: 'multipart/form-data',
        data: new FormData($('#frm-my-files')[0]),
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            _isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#tbl-my-files').DataTable().ajax.reload();
                $('#profile-file-upload').modal('hide');
                resetFormMyFile();
                cleanBookNotification.success(_L("ERROR_MSG_128"), "Qbicles");
            } else {
                cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_506"), "Qbicles");
            }
            _isBusy = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            _isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
};

function resetFormMyFile() {
    $('#frm-my-files')[0].reset();
    $('#frm-my-files').validate().resetForm();
};


UploadUserAvatar = function () {
    var $frmavatarupload = $('#frm-avatar-upload');
    if ($frmavatarupload.valid()) {
        $.LoadingOverlay("show");
        var files = document.getElementById("user-avatar-upload").files;

        if (files && files.length > 0) {
            UploadMediaS3ClientSide("user-avatar-upload").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    console.log(mediaS3Object);
                    $("#user-avatar-upload-object-key").val(mediaS3Object.objectKey);
                    $("#user-avatar-upload-object-name").val(mediaS3Object.fileName);
                    $("#user-avatar-upload-object-size").val(mediaS3Object.fileSize);

                    SaveUserAvatatar();
                }
            });

        }
    }
}


function SaveUserAvatatar() {

    $.ajax({
        type: "post",
        cache: false,
        url: "/Administration/AvatarUpload",
        enctype: 'multipart/form-data',
        data: new FormData($('#frm-avatar-upload')[0]),
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            _isBusy = true;
        },
        success: function (data) {
            if (data.result) {
                $('#manage-avatar').modal("hide");
                var avatarUrl = $('#hdfApiFile').val() + data.Object;
                $('#dv-avatar').css('background-image', 'url(' + avatarUrl + ')');
                $('a.avt-elm').removeClass('selected');
                $('a.avt-elm').addClass('options');
                var _avatar_el = '<div id="avata-' + data.Object + '" class="col"><a onclick="SetSelectedAvata(\'' + data.Object + '\',this)" class="avt-elm article-feature selected">';
                _avatar_el += '<div class="article-feature-img" style="background: url(\'' + avatarUrl + '\');"></div></a></div>';
                $('#avatar-empty').before(_avatar_el);
            } else {
                cleanBookNotification.error(data.msg ? data.msg : _L("ERROR_MSG_503"), "Qbicles");
            }
            _isBusy = false;
            LoadingOverlayEnd();
        },
        error: function (data) {
            _isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error("Have an error, detail: " + data.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });;
};


//Showcase
function showListShowcases() {
    if (!$('.loadingoverlay').is(':visible')) {
        LoadingOverlay();
    }
    var searchKey = "";
    var searchInput = $("#showcase-key-search");
    if (searchInput && searchInput.length > 0) {
        searchKey = searchInput.val();
    }
    var _url = "/UserInformation/ListShowCasesPartial?keySearch=" + searchKey;
    $("#showcases").empty();
    $("#showcases").load(_url);
    LoadingOverlayEnd();
}

function modalSaveShowcaseShow(showcaseKey) {
    LoadingOverlay();
    var _url = "/UserInformation/AddEditShowcaseView?showcaseKey=" + showcaseKey;
    $("#profile-showcase-add").empty();
    $("#profile-showcase-add").load(_url);
    $("#profile-showcase-add").modal("show");
    LoadingOverlayEnd();
}

function deleteShowcase(showcaseKey) {
    if (confirm("Are you sure you want to delete this Showcase from your profile?")) {
        LoadingOverlay();
        var _url = "/UserInformation/DeleteShowCase?showcaseKey=" + showcaseKey;
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success("Deleted Showcase successfully!", "Qbicles");

                    var $lstshowcaseprofile = $("#showcases");
                    if ($lstshowcaseprofile != null) {
                        showListShowcases();
                    }

                    var $lstshowcasewizard = $("#list-showcase-wizard");
                    if ($lstshowcasewizard != null) {
                        showListWizardShowcase();
                    }
                } else {
                    cleanBookNotification.error(response.msg);
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg);
            }
        });
        LoadingOverlayEnd();
    }
}

function intiAddingShowcaseForm() {
    var $addeditscfrm = $("#add-edit-sc-frm");
    $addeditscfrm.validate({
        rules: {
            title: {
                required: true
            },
            cap: {
                required: true,
                maxlength: 100
            }
        }
    });

    $addeditscfrm.submit(function (e) {
        e.preventDefault();
        if ($addeditscfrm.valid()) {
            LoadingOverlay();
            var files = document.getElementById("sc-img").files;

            //Processing with uploaded files
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("sc-img").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize
                        };
                        saveShowcase(s3Object);
                    }
                });
            } else {
                var scId = $("#showcase-id").val();
                if (scId === "0") {
                    LoadingOverlayEnd();
                    $addeditscfrm.validate().showErrors({ "image": "This field is required!" });
                } else {
                    saveShowcase(null);
                }
            }
        }
    });
}

function saveShowcase(uploadModel) {
    var sc = {
        Key: $("#showcase-id").val(),
        Title: $("#sc-title").val(),
        Caption: $("#sc-caption").val(),
    }

    var _url = "/UserInformation/SaveShowCase";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            sc: sc,
            uploadedFile: uploadModel
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success("Save Showcase successfully!", "Qbicles");

                var $lstshowcaseprofile = $("#showcases");
                if ($lstshowcaseprofile != null) {
                    showListShowcases();
                }

                var $lstshowcasewizard = $("#list-showcase-wizard");
                if ($lstshowcasewizard != null) {
                    showListWizardShowcase();
                }

                $("#profile-showcase-add").modal("hide");
            } else {
                cleanBookNotification.error(response.msg);
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg);
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
//End Showcase


//Education & qualifications tab == Skills block
function showListSkills() {
    var _url = "/UserInformation/UserSkillList";
    $("#userskills").empty();
    $("#userskills").load(_url);
}

function modalSaveSkillsShow(skillKey) {
    LoadingOverlay();
    var _url = "/UserInformation/AddEditSkillView?skillKey=" + skillKey;
    $("#profile-skill-add").empty();
    $("#profile-skill-add").load(_url);
    $("#profile-skill-add").modal("show");
    LoadingOverlayEnd();
}

function deleteSkills(skillKey) {
    if (confirm('Are you sure you want to remove this skill from your profile?')) {
        LoadingOverlay();
        var _url = "/UserInformation/DeleteUserSkill?skillKey=" + skillKey;
        $.ajax({
            method: 'POST',
            dataType: 'JSON',
            url: _url,
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.success("Delete Skill successfully.", "Qbicles");
                    showListSkills();
                } else {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (err) {
                cleanBookNotification.error(err.msg, "Qbicles");
            }
        });
        LoadingOverlayEnd();
    }
}

function initAddingSkillsForm() {
    var $skilladdfrm = $("#skillform");
    $skilladdfrm.validate({
        rules: {
            skillname: {
                required: true
            },
            skillpoint: {
                required: true
            }
        }
    });

    $skilladdfrm.submit(function (e) {
        e.preventDefault();
        LoadingOverlay();
        if ($skilladdfrm.valid()) {
            var _url = "/UserInformation/SaveUserSkill";
            var skillObj = {
                Name: $("#skillname").val(),
                Proficiency: $("#skillpoint").val()
            };
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    userSkill: skillObj
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.success("Save Skill successfully.", "Qbicles");
                        showListSkills();
                        $("#profile-skill-add").modal("hide");
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            });
        }
        LoadingOverlayEnd();
    });
}

//END Education & qualifications tab == Skills block

//Education & qualifications tab == Work Exp block
function showListWorkExpList() {
    var _url = "/UserInformation/UserWorkExpList";
    $("#work-exp-list").empty();
    $("#work-exp-list").load(_url);
}

function modalSaveWorkExpShow(expKey) {
    LoadingOverlay();
    var _url = "/UserInformation/AddEditWorkExpView?expKey=" + expKey;
    $("#profile-employment-add").empty();
    $("#profile-employment-add").load(_url);
    $("#profile-employment-add").modal("show");
    LoadingOverlayEnd();
}

function initAddingWorkExp() {
    var $addworkexpform = $("#add-workexp-form");
    $addworkexpform.validate({
        rules: {
            company: {
                required: true
            },
            role: {
                required: true
            },
            startdate: {
                required: true
            }
        }
    });
    $addworkexpform.submit(function (e) {
        e.preventDefault();
        if ($addworkexpform.valid()) {
            LoadingOverlay();
            
            var workExp = {
                Key: $("#workId").val(),
                Company: $("#company-name").val(),
                Role: $("#role-name").val(),
                Summary: $("#wsummary").val()
            };
            var startDate = $("#wstartdate").val();
            var iscurrenton = $("#wcurrent-on").prop("checked");
            var endDate = $("#wenddate").val();

            var _url = "/UserInformation/SaveWorkExp";
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    work: workExp,
                    startDate: startDate,
                    endDate: endDate,
                    isCurrentStillWork: iscurrenton
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.success("Save Work Experience Successfully.", "Qbicles");
                        showListWorkExpList();
                        $("#profile-employment-add").modal("hide");
                    } else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            })
            LoadingOverlayEnd();
        }
    })
}
//END Education & qualifications tab == Work Exp block

//Education & qualifications tab == Edu Exp block
function showListEduExpList() {
    var _url = "/UserInformation/UserEduExpList";
    $("#edu-exp-list").empty();
    $("#edu-exp-list").load(_url);
}

function modalSaveEduExpShow(expKey) {
    LoadingOverlay();
    var _url = "/UserInformation/AddEditEduExpView?expKey=" + expKey;
    $("#profile-education-add").empty();
    $("#profile-education-add").load(_url);
    $("#profile-education-add").modal("show");
    LoadingOverlayEnd();
}

function initAddEduExp() {
    var $addeduexpform = $("#add-eduexp-form");
    $addeduexpform.validate({
        rules: {
            ins: {
                required: true
            },
            course: {
                required: true
            },
            startdate: {
                required: true
            }
        }
    });
    $addeduexpform.submit(function (e) {
        e.preventDefault();
        if ($addeduexpform.valid()) {
            LoadingOverlay();
            var eduExp = {
                Key: $("#eduId").val(),
                Institution: $("#ins-name").val(),
                Course: $("#course-name").val(),
                Summary: $("#esummary").val()
            };
            var startDate = $("#estartdate").val();
            var iscurrenton = $("#ecurrent-on").prop("checked");
            var endDate = $("#eenddate").val();

            var _url = "/UserInformation/SaveEduExp";
            $.ajax({
                method: 'POST',
                dataType: 'JSON',
                url: _url,
                data: {
                    exp: eduExp,
                    startdate: startDate,
                    enddate: endDate,
                    isStillWorkHere: iscurrenton
                },
                success: function (response) {
                    if (response.result) {
                        cleanBookNotification.success("Save Education Experience successfully.", "Qbicles");
                        showListEduExpList();
                        $("#profile-education-add").modal("hide");
                    }
                    else {
                        cleanBookNotification.error(response.msg, "Qbicles");
                    }
                },
                error: function (err) {
                    cleanBookNotification.error(err.msg, "Qbicles");
                }
            })

            LoadingOverlayEnd();
        }
    })
}
//END Education & qualifications tab == Edu Exp block

//User Profile Files
function listProfileFilesShow(keySearch) {
    //LoadingOverlay();
    var _url = "/UserInformation/UserProfileFileList";

    $("#profile-files").empty();
    $("#profile-files").load(_url, { keySearch: keySearch });
    //LoadingOverlayEnd();
}

function modalSaveProfileFileShow(fileKey) {
    LoadingOverlay();
    var _url = "/UserInformation/AddEditProfileFileView?fileKey=" + fileKey;
    $("#profile-file-add").empty();
    $("#profile-file-add").load(_url);
    $("#profile-file-add").modal("show");
    LoadingOverlayEnd();
}

function deleteProfileFile(fileKey) {
    LoadingOverlay();
    var _url = "/UserInformation/DeleteUserProfileFile?fileKey=" + fileKey;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        success: function (response) {
            if (response.result) {
                cleanBookNotification.removeSuccess();
                listProfileFilesShow('');
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    })
    LoadingOverlayEnd();
}

function initSaveProfileFileForm() {
    var $savefilefrm = $("#save-profilefile-form");
    $savefilefrm.validate({
        rules: {
            title: {
                required: true
            },
            description: {
                required: true
            }
        }
    })

    $savefilefrm.submit(function (e) {
        e.preventDefault();
        var files = document.getElementById("file-upload").files;
        if ($savefilefrm.valid()) {
            LoadingOverlay();
            if (files && files.length > 0) {
                UploadMediaS3ClientSide("file-upload").then(function (mediaS3Object) {

                    if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                        LoadingOverlayEnd('hide');
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    }
                    else {
                        var s3Object = {
                            FileName: mediaS3Object.fileName,
                            FileKey: mediaS3Object.objectKey,
                            FileSize: mediaS3Object.fileSize,
                            FileType: mediaS3Object.fileType,
                        };
                        saveProfileFile(s3Object);
                    }
                });
            } else {
                saveProfileFile(null);
            }
        }
    })
}

function saveProfileFile(uploadModel) {
    if (_isBusy)
        return;
    var profileFile = {
        Key: $("#file-id").val(),
        Title: $("#file-title").val(),
        Description: $("#file-description").val()
    };
    var _url = "/UserInformation/SaveProfileFile";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            pfile: profileFile,
            uploadModel: uploadModel
        },
        beforeSend: function (xhr) {
            _isBusy = true;
        },
        success: function (response) {
            _isBusy = false;
            if (response.result) {
                cleanBookNotification.success("Save User Profile File successfully.", "Qbicles");
                listProfileFilesShow('');
                $("#profile-file-add").modal("hide");
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            _isBusy = false;
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    })

    LoadingOverlayEnd();
}

//END User Profile Files

//User Trader Addresses
function listUserAddressesShow() {
    var _url = "/UserInformation/ListUserTraderAddressesShow";
    $("#profile-addresses").empty();
    $("#profile-addresses").load(_url);
}
//END User Trader Addresses

//User profile Wizard
function avatarWizardChange(sender) {
    readURLWizard(sender);
}

function readURLWizard(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#avatar').attr('style', 'background-image: url("' + e.target.result + '")');
        }
        reader.readAsDataURL(input.files[0]);
    }
}

function recount(finished_step) {
    stepFinished = finished_step;
    step = (100 / stepNum) * finished_step;
    $('.stat').circleProgress({
        max: 100,
        value: Math.ceil(step),
        textFormat: 'percent',
        animation: 'easeOutExpo',
        animationDuration: 2600
    });
    updateWizardStep();
}


function initUserProfileWizard() {
    var st = stepFinished + 1;
    $("#s" + st).addClass("in active");

    //Loading List Businesses to Connect
    var lstChosenInterestIds = $('.myinterests .col .active').map(function () { return $(this).attr("bid"); }).get();
    var _loadBusinessUrl = '/UserInformation/ShowBusinessByInterests';
    $("#recommended-businesses").empty();
    $("#recommended-businesses").load(_loadBusinessUrl, { interestIds: lstChosenInterestIds });

    showListWizardAddresses();
    showListWizardShowcase();

    $('.stat').circleProgress({
        max: 100,
        value: Math.ceil(step),
        textFormat: 'percent',
        animation: 'easeInOutExpo',
        animationDuration: 2600
    });

    $('.myinterests .col a').bind('click', function (e) {
        $('#interests').removeAttr('disabled');
        e.preventDefault();
        $(this).toggleClass('active');
    });


    $('.owl-carousel-2').owlCarousel({
        loop: false,
        autoplay: true,
        autoplayHoverPause: true,
        autoplayTimeout: 3000,
        dots: true,
        nav: false,
        margin: 10,
        responsiveClass: true,
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 2
            }

        }
    });
}

function showListWizardAddresses() {
    var _url = "/UserInformation/UserListAddressWizard";
    $("#wizard-addresses").empty();
    $("#wizard-addresses").load(_url);
}

function showListWizardShowcase() {
    var _url = "/UserInformation/UserListShowcaseWizard";

    $("#list-showcase-wizard").empty();
    $("#list-showcase-wizard").load(_url);
}

function initStep1WizardForm() {
    var $step1form = $("#wizard-step1-form");   

    $step1form.submit(function (e) {
        e.preventDefault();
        LoadingOverlay();

        var files = document.getElementById("setavatar").files;
        current_step = "s" + GeneralSettingsStep;
        next_step = "s" + AddressAndPhoneSettingStep;
        if (files && files.length > 0) {
            UploadMediaS3ClientSide("setavatar").then(function (mediaS3Object) {

                if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                    return;
                }
                else {
                    saveStepInforWizard(mediaS3Object.objectKey, current_step, next_step);
                }
            });


        } else {
            saveStepInforWizard("", current_step, next_step);
        }

        LoadingOverlayEnd();
    });
}

function saveStepInforWizard(uploadmodelKey, currentstep, nextstep) {
    var displayname = $("#winzard-displayname").val();
    var tagline = $("#wizard-tagline").val();
    var about = $("#wizard-about").val();

    var userprofile = {
        profilePic: uploadmodelKey,
        DisplayUserName: displayname,
        Tagline: tagline,
        Profile: about,
        Tell: $("#wizard-tel").val(),
        Timezone: $("#wizard-timezone").val(),
        NotificationSound: $("#wizard-notification-sound").val(),
        ChosenNotificationMethod: $("#wizard-chosen-notification-method").val(),
        TimeFormat: $("#wizard-timeformat").val(),
        DateFormat: $("#wizard-dateformat").val(),
    }

    var _url = "/Administration/SaveUserProfileWizard";
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: userprofile,
        success: function (resposne) {
            if (resposne.result) {
                $("#" + currentstep).removeClass('active in');
                $("#" + nextstep).addClass('active in');
                cleanBookNotification.updateSuccess();
                var finished_step = currentstep.substring(1);
                recount(finished_step);
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
}

function saveUserInterest() {
    var lstChosenInterestIds = $('.myinterests .col .active').map(function () { return $(this).attr("bid"); }).get();
    var _url = "/HighlightSetup/AddUserInterest";

    LoadingOverlay();
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url,
        data: {
            interestIds: lstChosenInterestIds
        },
        success: function (response) {
            if (response.result) {
                cleanBookNotification.updateSuccess();

                //Loading List Businesses to Connect
                var _loadBusinessUrl = '/UserInformation/ShowBusinessByInterests';
                $("#recommended-businesses").empty();
                $("#recommended-businesses").load(_loadBusinessUrl, { interestIds: lstChosenInterestIds });

                //Move to next Step
                $("#s5").removeClass('active in');
                $("#s6").addClass('active in');
                recount(InterestSettingsStep);
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (err) {
            cleanBookNotification.error(err.msg, "Qbicles");
        }
    });
    LoadingOverlayEnd();

}

function connectBusiness(uId, businessName, ev) {
    $.LoadingOverlay('show');
    $.post("/C2C/ConnectC2C", { linkId: uId, type: 1 }, function (response) {
        $.LoadingOverlay('hide');
        if (response.result) {
            _c2cQbiceId = response.Object;
            cleanBookNotification.success(_L("CONNECTED_SUCCESS", [businessName]), "Community");

            $(ev).toggleClass('btn-info btn-success').html('Connected').attr("disabled", "");


        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Community");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Community");
        }

    });
}

function checkValidInput(ev) {
    console.log($(ev).val());

    if ($(ev).val() != "") {
        $(ev).addClass("valid").removeClass("invalid");
        $(ev).next('i').addClass('fa-check green').removeClass('fa-remove red');
    } else {
        $(ev).removeClass("valid").addClass("invalid");
        $(ev).next('i').removeClass('fa-check green').addClass('fa-remove red');
    }
}

function checkValidTextArea(ev) {
    if ($(ev).val() != "") {
        $(ev).addClass("valid").removeClass("invalid");
    } else {
        $(ev).addClass("invalid").removeClass("valid");
    }
}

function updateWizardStep() {
    LoadingOverlay();
    var _url = "/UserInformation/UpdateWizardFinishedStep?step=" + stepFinished;
    $.ajax({
        method: 'POST',
        dataType: 'JSON',
        url: _url
    })
    LoadingOverlayEnd();
}
//END User profile Wizard
function getUserProfilePages() {
    var $tblUserProfilePages = $('#tblUserProfilePages');
    $tblUserProfilePages.on('processing.dt', function (e, settings, processing) {
        $('#processingIndicator').css('display', 'none');
        if (processing) {
            $tblUserProfilePages.LoadingOverlay("show");
        } else {
            $tblUserProfilePages.LoadingOverlay("hide", true);
        }
    }).dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        //searchDelay: 800,
        pageLength: 10,
        deferLoading: 30,
        ajax: {
            "url": "/ProfilePage/GetUserProfilePages",
            "type": 'Get',
            "dataType": 'json',
            "data": function (d) {
                var _paramaters = {
                    keyword: $('#profile-pages input[name=keyword]').val(),
                    status: $('#profile-pages select[name=status]').val()
                };
                return $.extend({}, d, _paramaters);
            }
        },
        columns: [
            { "data": "PageTitle", "orderable": false },
            { "data": "Created", "orderable": false },
            { "data": "DisplayOrder", "orderable": true },
            {
                "data": "Status",
                "orderable": true,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '';
                    if (data == 'IsActive')
                        _htmlStatus += '<span class="label label-lg label-success">Active</span>';
                    else if (data == 'IsInActive')
                        _htmlStatus += '<span class="label label-lg label-warning">Inactive</span>';
                    else
                        _htmlStatus += '<span class="label label-lg label-info">Draft</span>';
                    return _htmlStatus;
                }
            },
            {
                "data": "Key",
                "orderable": false,
                "render": function (data, type, row, meta) {
                    var _htmlStatus = '<div class="btn-group"><button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    _htmlStatus += 'Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                    _htmlStatus += '<ul class="dropdown-menu dropdown-menu-right primary" style="right: 0;">';
                    _htmlStatus += '<li><a href="/ProfilePage/UserPageBuilder?pageKey=' + data + '">Edit</a></li>';
                    if (meta.row > 0) {
                        _htmlStatus += '<li class="dtMoveUp"><a href="javascript:void(0)">Move up</a></li>';
                    }
                    _htmlStatus += '<li class="dtMoveDown"><a href="javascript:void(0)">Move down</a></li>';
                    if (row.Status == 'IsActive') {
                        _htmlStatus += '<li><a href="/ProfilePage/UserPagePreview?pageKey=' + data + '" target="_blank">Preview</a></li>';
                        _htmlStatus += '<li><a href="javascript:void(0)" onclick="setUserPageStatus(\'' + data + '\',\'IsInActive\')">Unpublish</a></li>';
                    }
                    else
                        _htmlStatus += '<li><a href="javascript:void(0)" onclick="setUserPageStatus(\'' + data + '\',\'IsActive\')">Publish</a></li>';
                    _htmlStatus += '<li><a href="javascript:void(0)" onclick="deleteProfilePage(\'' + data + '\');">Delete</a></li>';
                    _htmlStatus += '</div>';
                    return _htmlStatus;
                }
            }
        ],
        drawCallback: function (settings) {
            $('#tblUserProfilePages tr:last .dtMoveDown').remove();

            // Remove previous binding before adding it
            $('.dtMoveUp').unbind('click');
            $('.dtMoveDown').unbind('click');

            // Bind clicks to functions
            $('.dtMoveUp').click(moveUp);
            $('.dtMoveDown').click(moveDown);
        }
    });
}
// Move the row up
function moveUp() {
    var tr = $(this).parents('tr');
    moveRow(tr, 'up');
}

// Move the row down
function moveDown() {
    var tr = $(this).parents('tr');
    moveRow(tr, 'down');
}

// Move up or down (depending...)
function moveRow(row, direction) {
    var table = $("#tblUserProfilePages").DataTable();
    var index = table.row(row).index();

    var order = -1;
    if (direction === 'down') {
        order = 1;
    }

    var data1 = table.row(index).data();
    data1.order += order;

    var data2 = table.row(index + order).data();
    data2.order += -order;

    table.row(index).data(data2);
    table.row(index + order).data(data1);
    if (data1.DisplayOrder == data2.DisplayOrder) {
        data2.DisplayOrder = parseInt(data2.DisplayOrder)+order;
    }
    var _pages = [
        { Key: data1.Key, DisplayOrder: data2.DisplayOrder },
        { Key: data2.Key, DisplayOrder: data1.DisplayOrder }
    ];
    $.post("/ProfilePage/UpdateDisplayOrder", { pages: _pages }, function (Response) {
        if (Response.result) {
            table.page(0).draw(false);
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function reloadUserProfilePages() {
    if ($.fn.DataTable.isDataTable("#tblUserProfilePages"))
        $("#tblUserProfilePages").DataTable().ajax.reload();
    else {
        wto = setInterval(function () {
            if ($.fn.DataTable.isDataTable("#tblUserProfilePages")) {
                $("#tblUserProfilePages").DataTable().ajax.reload();
                clearInterval(wto);
            }
        }, 1000);
    }
}
function setUserPageStatus(pageKey, status) {
    $.post("/ProfilePage/SetUserPageStatus", { pageKey: pageKey, status: status }, function (Response) {
        if (Response.result) {
            reloadUserProfilePages();
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function deleteProfilePage(pageKey) {
    var result = confirm('Are you sure you want to delete this page?');
    if (result) {
        $.post("/ProfilePage/DeleteUserProfilePage", { pageKey: pageKey }, function (Response) {
            if (Response.result) {
                reloadUserProfilePages();
            } else if (!Response.result && Response.msg) {
                cleanBookNotification.error(Response.msg, "Qbicles");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
function saveInterests() {
    var paramaters = $('select[name=interests]').val();
    if (!paramaters || paramaters.length == 0) {
        cleanBookNotification.error(_L("ERROR_MSG_NOINTEREST"), "Qbicles");
        return;
    }
    $.post("/UserProfile/UpdateInterests", { interests: (paramaters ? paramaters : []) }, function (Response) {
        if (Response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
        } else if (!Response.result && Response.msg) {
            cleanBookNotification.error(Response.msg, "Qbicles");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function verificationNewEmail() {
    LoadingOverlay();
    var _email = $('#change-email input[name=email]').val();
    $.post("/Administration/VerificationNewEmail", { newEmailAddress: _email }, function (response) {
        LoadingOverlayEnd();
        if (response.result) {
            $('#change-email').modal('hide');
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
            $('#check-verify').modal('show');
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(_L(response.msg), "Qbicles");
        } else
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
    });
}
function checkNewEmailAvailable(elm) {
    var _email = $('#change-email input[name=email]').val();
    var isEmailAvailable = false;
    $.ajax({
        url: "/Administration/DuplicateEmail",
        data: { Email: _email },
        type: "GET",
        dataType: "json",
        async: false,
    }).done(function (refModel) {
        if (!refModel.result) {
            switch (refModel.msgId) {
                case "2":
                    $('#verify-box').hide();
                    $('#verify-box-unavailable').show();
                    break;
                default:
                    isEmailAvailable = true;
                    break;
            }
            return;
        } else {
            isEmailAvailable = true;
        }
    }).fail(function () {
        var $verifybox = $('#verify-box-unavailable');
        $verifybox.empty();
        $verifybox.removeClass('alert-success');
        $verifybox.addClass('alert-danger');
        $verifybox.children('p').text(_L("ERROR_MSG_EXCEPTION_SYSTEM"));
        $('#verify-box').hide();
        $verifybox.show();
    })
    if (isEmailAvailable) {
        $('#change-email input[name=email]').attr('disabled', true);
        $('#verify-box-unavailable').hide();
        $('#verify-box').show();
        $(elm).hide();
    }
}
function checkDisableBtnAvailable(elm) {
    $(elm).val() != '' ? $('#btncheck').removeAttr('disabled') : $('#btncheck').attr('disabled', true);
}