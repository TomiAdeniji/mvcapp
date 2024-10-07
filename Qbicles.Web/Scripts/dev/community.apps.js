
var idPage = 0;
var searchResultUserProfiles = [], searchResultDomainProfiles = [], searchResultCommunityPages = [];
var reloadTableFollower = false;
var reloadTablePage = false;
function getParamName() {
    searchResultDomainProfiles = $('#stringBusId').val().split(',');
    searchResultUserProfiles = $('#stringPeoId').val().split(',');
    searchResultCommunityPages = $('#stringPageId').val().split(',');
}
function confirmDeletePage(title, idValue) {
    idPage = idValue;
    $('#delete_name').text(title.trim());
    $('#confirm-delete').modal('toggle');
}
function deletePage() {
    if (idPage > 0) {
        $.ajax({
            type: 'post',
            url: '/CommunitySystem/ValidationDeletePage',
            data: { id: idPage },
            dataType: 'json',
            success: function (response) {
                if (response.result) {
                    cleanBookNotification.warning(_L("ERROR_MSG_80"), "Qbicles");
                    $('#confirm-delete').modal('toggle');
                }
                else {
                    $.ajax({
                        type: 'post',
                        url: '/CommunitySystem/DeletePage',
                        data: { id: idPage },
                        dataType: 'json',
                        success: function (response) {
                            if (response.result) {
                                $("#communityPage_table_" + idPage).remove();
                                cleanBookNotification.updateSuccess();
                                $('#confirm-delete').modal('toggle');
                                reloadTablePage = true;
                                $('#com_table').load("/Community/CommunityPageTable");
                            }
                            else {
                                cleanBookNotification.error(response.msg, "Qbicles");
                            }
                        },
                        error: function (er) {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    });
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}

function unFollower(id, type, showpage) {
    $.ajax({
        type: 'get',
        url: '/Community/UnFollower?id=' + id + '&type=' + type,
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $('#table_app_follower_' + id + '-' + type).remove();
                cleanBookNotification.success(_L("ERROR_MSG_661"), "Qbicles");
                if (showpage == 'showpage') {
                    var followed = parseInt($("#folowed-" + id).text());
                    $("#folowed-" + id).text(followed - 1);
                    $('#community-list tr.show_page_' + id + ' button.unfollower').addClass('hidden');
                    $('#community-list tr.show_page_' + id + ' button.follower').removeClass('hidden');
                }
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function follower(id, type, showpage) {
    $.ajax({
        type: 'get',
        url: '/Community/Follower?id=' + id + '&type=' + type,
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                cleanBookNotification.success(_L("ERROR_MSG_662"), "Qbicles");
                if (showpage == 'showpage') {
                var followed = parseInt($("#folowed-" + id).text());
                $("#folowed-" + id).text(followed + 1);
                    $('#community-list tr.show_page_' + id + ' button.unfollower').removeClass('hidden');
                    $('#community-list tr.show_page_' + id + ' button.follower').addClass('hidden');
                }
            }
            else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function showBodyText(showMore) {
    if (showMore) {
        $('.more_bodytext').addClass('hidden');
        $('.less_bodytext').removeClass('hidden');
    } else {
        $('.less_bodytext').addClass('hidden');
        $('.more_bodytext').removeClass('hidden');
    }
}

function search() {
    var keyWord = $('#com_search').val();
    keyWord = keyWord.replace(/\,/g, '.').replace(/\ /g, '.');
    keyWord = keyWord.split('.');
    $.ajax({
        type: 'post',
        url: '/Community/Search',
        data: { lstKeyWord: keyWord },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal == 1) {
                if (response.Object != null && response.Object.length > 0) {
                    for (var i = 0; i < response.Object.length; i++) {
                        if (response.Object[i].PageType == 0) { // domain
                            $('#result_businesses').text(response.Object[i].ListId.length);
                            searchResultDomainProfiles = response.Object[i].ListId;
                        } else if (response.Object[i].PageType == 1) { // user profile
                            $('#result_people').text(response.Object[i].ListId.length);
                            searchResultUserProfiles = response.Object[i].ListId;
                        } else if (response.Object[i].PageType == 2) { // pages
                            $('#result_page').text(response.Object[i].ListId.length);
                            searchResultCommunityPages = response.Object[i].ListId;
                        }
                    }
                } else {
                    $('#result_businesses').text('0');
                    $('#result_businesses').text('0');
                    $('#result_page').text('0');
                }
            }
            else if (response.actionVal == 2) {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function businesseClick() {
    window.location = "/Community/CommunityList?name=businesses&lstBusId=" + searchResultDomainProfiles.join(',') + '&lstPeoId=' + searchResultUserProfiles.join(',') + '&lstPageId=' + searchResultCommunityPages.join(',');
}
function peopleClick() {
    window.location = "/Community/CommunityList?name=people&lstBusId=" + searchResultDomainProfiles.join(',') + '&lstPeoId=' + searchResultUserProfiles.join(',') + '&lstPageId=' + searchResultCommunityPages.join(',');
}
function pageClick() {
    window.location = "/Community/CommunityList?name=pages&lstBusId=" + searchResultDomainProfiles.join(',') + '&lstPeoId=' + searchResultUserProfiles.join(',') + '&lstPageId=' + searchResultCommunityPages.join(',');
}

// page community list
function searchInValue(name) {
    setTimeout(function () {
        var tags = $('#tagselect').val() == null ? [] : $('#tagselect').val();
        var keyWord = $('#search_dt').val();
        if (name == 'businesses') {
            $('#businesse_table').load("/Community/BusinessesTable?lstId=" + $('#stringBusId').val() + '&callback=true&tagIds=' + tags.join(',') + '&key=' + keyWord);
        } else if (name == 'people') {
            $('#people_table').load("/Community/PeopleTable?lstId=" + $('#stringPeoId').val() + '&callback=true&tagIds=' + tags.join(',') + '&key=' + keyWord);
        } else if (name == 'pages') {
            $('#page_table').load("/Community/PagesTable?lstId=" + $('#stringPageId').val() + '&callback=true&tagIds=' + tags.join(',') + '&key=' + keyWord);
        }
    }, 1000);
}