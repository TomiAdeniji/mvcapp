jQuery(function ($) {

    $('.select2avatarDomain').select2({
        placeholder: 'Please select',
        templateResult: formatOptions,
        templateSelection: formatSelected,
        dropdownCssClass: 'withdrop'
    });

});

function formatOptions(state) {

    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}

function formatSelected(state) {
    if (!state.id) { return state.text; }
    var $state = $(
        '<div class="select2imgwrap"><div class="select2img mini" style="background-image: url(\'' + ($(state.element).attr('LogoUri') == '' ? '/Content/DesignStyle/img/icon_domain_default.png' : ($(state.element).attr('api') + '' + $(state.element).attr('LogoUri'))) + '\');"></div> <span class="select2text">' + state.text + '</span></div>'
    );
    return $state;
}
function OnchangeEmail() {
    if ($("#UserEmail").val() != "") {
        $('.confirm-add').removeAttr('disabled');
    }
    else {
        $('.confirm-add').attr('disabled', 'disabled');
    }
}
function SearchPeopleData() {
    var peopleName = $("#searchPeople").val();
    var roleLevel = $("#ddlRoleLevel :selected").val();
    var domainRole = $("#ddlDomainRole").val() == null ? [] : $("#ddlDomainRole").val();
    var url = "/OurPeople/SearchPeopleData/";
    $("#Peoplelist").LoadingOverlay('show')
    $.ajax({
        url: url,
        data: { peopleName: peopleName, roleLevel: roleLevel, domainRole: domainRole },
        cache: false,
        type: "POST",
        async: true,
        success: function (data) {
            if (data) {
                $("#content-peoplelist").html(data);
                $("#Peoplelist").DataTable();
            }
            $("#Peoplelist").LoadingOverlay('hide')
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            $("#Peoplelist").LoadingOverlay('hide')
        }
    });
}
function InvitedUser() {
    var email = $("#UserEmail").val();
    if (validateEmail(email)) {
        $.LoadingOverlay("show");
        setTimeout(function () {
            var loadStyle = $(".loadingoverlay").attr("style");
            if (loadStyle.indexOf('opacity') > -1)
                loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

            $(".loadingoverlay").attr("style", loadStyle);
            var url = "/OurPeople/Invitation/";
            $.ajax({
                url: url,
                data: { email: email },
                cache: false,
                type: "POST",
                async: false,
                success: function (refModel) {
                    if (refModel.result) {
                        cleanBookNotification.success(_L("ERROR_MSG_499", [email]), $("#people-user-add").find('.modal-title').html());
                        $("#manage-invites").click();                        
                        SearchInvitationPeople();
                        $("#UserEmail").val('');
                        $("#people-user-add").modal("hide");
                    }
                    else {
                        if (refModel.msg != '') {
                            cleanBookNotification.warning(refModel.msg, $("#people-user-add").find('.modal-title').html());
                        }
                        else {
                            cleanBookNotification.error(_L("ERROR_MSG_495"), $("#people-user-add").find('.modal-title').html());
                        }
                        
                    }
                    LoadingOverlayEnd();
                },
                error: function (xhr, status, error) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        }, 200);
    }
    else {
        cleanBookNotification.error(_L("ERROR_MSG_496"), $("#people-user-add").find('.modal-title').html());
        LoadingOverlayEnd();
    }
}
function reSendEmail(invitationId, email, RecipientName) {
   
    $.LoadingOverlay("show");
    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        var url = "/OurPeople/ReSendInvitation/";
        $.ajax({
            url: url,
            data: { Id: invitationId, email: email, RecipientName: RecipientName},
            cache: false,
            type: "POST",
            async: false,
            success: function (refModel) {
                if (refModel.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_500", [email]));
                    SearchInvitationPeople();
                }
                else {
                    cleanBookNotification.error(_L("ERROR_MSG_497"));
                }
                LoadingOverlayEnd();
            },
            error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}

function ConfirmRemoveUserFromDomain(userId, userName) {
    $("#hduserId").val(userId);   
    $("#lblUserName").html(userName);
    $("#lblDomainName").html($('.select2avatarDomain :selected').html());
    $("#PromoteOrDemoteConfirm").modal('show');   
}
function RemoveUserFromDomain() {
    RemovedUserFromDomain($("#hduserId").val(), $("#lblUserName").html(), $("#lblDomainName").html());
}
function PromoteOrDemoteUser(userId, PromoteOrDemoteTo, currentPossition) {
    var url = "/OurPeople/PromoteOrDemoteUser/";
    $.LoadingOverlay("show");
    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        $.ajax({
            url: url,
            data: { userId: userId, PromoteOrDemoteTo: PromoteOrDemoteTo, currentPossition: currentPossition },
            cache: false,
            type: "POST",
            async: false,
            success: function (refModel) {
                var str = "";
                if (PromoteOrDemoteTo > currentPossition)
                    str += "Promote " + (PromoteOrDemoteTo == 3 ? "to Domain Admin" : (PromoteOrDemoteTo == 2 ? "to Qbicle Creator" : ""));
                else
                    str += "Demote " + (PromoteOrDemoteTo == 2 ? "to Qbicle Creator" : (PromoteOrDemoteTo == 2 ? "to Domain User" : ""));
                if (refModel.result) {

                    cleanBookNotification.success(str + " success");
                    // SearchPeopleData();
                    initPeopleList();
                    $("#PromoteOrDemoteConfirm").modal('hide');
                }
                else {
                    cleanBookNotification.error(str + " fail");
                }
                LoadingOverlayEnd();
            },
            error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}
function SearchInvitationPeople() {
    var peopleName = $("#searchInvitation").val();
    var status = $("#ddlStatus :selected").val();
    var url = "/OurPeople/SearchInvitationPeople/";
    $("#lstInvitation").LoadingOverlay("show");
    $.ajax({
        url: url,
        data: { peopleName: peopleName, status: status },
        cache: false,
        type: "POST",
        async: true,
        success: function (data) {
            if (data) {
                $("#content-invites").empty();
                $("#content-invites").html(data);
                $("#lstInvitation").DataTable({
                    destroy: true,
                    searching: false,
                    order: [[3, 'desc']],
                    //paging: false,
                    //info: false,
                    //ordering: false,
                });
            }
            $("#lstInvitation").LoadingOverlay("hide");
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
            $("#lstInvitation").LoadingOverlay("hide");
        }
    });
}
function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
}
function RemovedUserFromDomain(userId, userName,domainName) {
    var url = "/OurPeople/RemovedUserFromDomain/";
    $.LoadingOverlay("show");
    setTimeout(function () {
        var loadStyle = $(".loadingoverlay").attr("style");
        if (loadStyle.indexOf('opacity') > -1)
            loadStyle = loadStyle.substring(0, loadStyle.indexOf('opacity'));

        $(".loadingoverlay").attr("style", loadStyle);
        $.ajax({
            url: url,
            data: { userId: userId },
            cache: false,
            type: "POST",
            async: false,
            success: function (refModel) {               
                if (refModel.result) {
                    cleanBookNotification.success(_L("ERROR_MSG_501", [userName, domainName]));
                    // SearchPeopleData();                    
                    GetCurrentMembers();
                    initPeopleList();
                    $("#PromoteOrDemoteConfirm").modal('hide');
                }
                else {
                    cleanBookNotification.error(_L("ERROR_MSG_498", [userName, domainName]));
                }
                LoadingOverlayEnd();
            },
            error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                LoadingOverlayEnd();
            }
        });
    }, 200);
}

function GetCurrentMembers(){
    $.ajax({
        type: "GET",
        url: "/OurPeople/GetCurrentUserAllowed",
        data: "data",
        success: function (data) {
            $(".user-slots").text(data);
        }
    });
}

function UpdatePeopleData() {
    initPeopleList();
    GetCurrentMembers();
}

function initPeopleList(){
    var currentUserID = $("#info-people-datatable").attr('currentuserid');
    var levelAdministrator = $("#info-people-datatable").attr('administrators');
    var levelManager = $("#info-people-datatable").attr('manager');
    var levelUser = $("#info-people-datatable").attr('user');
    var peoplelist2 = $("#Peoplelist");
    peoplelist2.DataTable().clear();
    peoplelist2.DataTable({
        "destroy": true,
        "serverSide": true,
        "paging": true,
        "pagingTag": 'button',
        "searching": false,
        "ordering" : false,
        "responsive": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "pageLength": 10,
        "aaSorting" : [],
        "ajax": {
            "url": '/OurPeople/SearchPeopleDataTable',
            "type": 'POST',
            "data": function (d) {
                $("#Peoplelist").LoadingOverlay("show");
                return $.extend({},d,{
                    "peopleName" : $("#searchPeople").val(),
                    "roleLevel" : $("#ddlRoleLevel :selected").val(),
                    "domainRole": $("#ddlDomainRole").val() == null ? [] : $("#ddlDomainRole").val()
                })
            },
            "complete" : function(){
                $("#Peoplelist").LoadingOverlay("hide");
            } 
        },
        "columns" : [
            {
                render : function(data, type, row){
                    html = `<td><div class="table-avatar mini" style="background-image: url('`+row.ImageUri+`"&size=T")');">&nbsp;</div></td>`
                    return html;
                }
            },
            {
                render : function(data, type, row){
                        html = ` <td><a href="/Community/UserProfilePage?uId=`+row.Id+`">`+row.FullName+`</a></td>`
                    return html;
                }
            },
            {data: 'CreatedDate'},
            {
                render : function(data, type, row){
                    html = `<td><a href="mailto:`+row.Email+`">`+row.Email+`</a></td>`
                    return html;
                }
            },
            {
                render : function(data, type, row){
                    html = `<td><span class="label label-lg label-primary">`+row.TypeUser+`</span></td>`
                    return html;
                }
            },
            {
                render : function(data, type, row){
                    html = ``;
                    if (row.lstRole.length > 0){
                        row.lstRole.forEach(element => {
                            html +=`<span class="label label-lg label-info" style="margin-bottom:3px;display: inline-flex;">`+element+`</span><br />`
                        });
                    }
                    return html;
                }
            },
            {data: 'QbiclesCount'},
            {
                render : function(data, type, row){
                    html = ``;
                    if (!(row.TypeUserId == levelAdministrator && row.Id == currentUserID)){
                        if (row.TypeUserId == levelAdministrator)
                        {
                            html2 = `<li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelManager+`, `+levelAdministrator+`)"><i class="fa fa-arrow-down"></i> &nbsp; Demote to Qbicle Creator</a></li>
                                    <li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelUser+`, `+levelAdministrator+`)"><i class="fa fa-arrow-down"></i> &nbsp; Demote to Domain User</a></li>
                                    <li><a href="#" onclick="ConfirmRemoveUserFromDomain('`+row.Id+`','`+row.FullName+`')"><i class="fa fa-trash"></i> &nbsp; Remove from Domain</a></li>`
                        }
                        else if (row.TypeUserId == levelManager)
                        {
                            html2 = `<li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelAdministrator+`, `+levelManager+`)"><i class="fa fa-arrow-up"></i> &nbsp; Promote to Domain Admin</a></li>
                                    <li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelUser+`, `+levelManager+`)"><i class="fa fa-arrow-down"></i> &nbsp; Demote to Domain User</a></li>
                                    <li><a href="#" onclick="ConfirmRemoveUserFromDomain('`+row.Id+`','`+row.FullName+`')"><i class="fa fa-trash"></i> &nbsp; Remove from Domain</a></li>`
                        }
                        else
                        {
                            html2 = `<li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelAdministrator+`, `+levelUser+`)"><i class="fa fa-arrow-up"></i> &nbsp; Promote to Domain Admin</a></li>
                                    <li><a href="#" onclick="PromoteOrDemoteUser('`+row.Id+`', `+levelManager+`, `+levelUser+`)"><i class="fa fa-arrow-up"></i> &nbsp; Promote to Qbicle Creator</a></li>
                                    <li><a href="#" onclick="ConfirmRemoveUserFromDomain('`+row.Id+`','`+row.FullName+`')"><i class="fa fa-trash"></i> &nbsp; Remove from Domain</a></li>`
                        }
                        
                        html = `<div class="btn-group options">
                                    <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                        <i class="fa fa-cog"></i>
                                    </button>
                                    <ul class="dropdown-menu dropdown-menu-right" style="right: 0;">
                                    `+html2+`
                                    </ul>
                                </div>`
                    }
                    return html;
                    }
            }
        ]
    })
}
//debounce input
$('#searchPeople').keyup(delay(function () {
    initPeopleList();
}, 500));
//init datatable
initPeopleList();