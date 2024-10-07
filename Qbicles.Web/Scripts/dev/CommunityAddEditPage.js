
var tagsOrigin = null;
var reader = new FileReader();
var table = null;


function getTagsValue() {
    tagsOrigin = $.parseJSON($('#tagsValue').val());
}
function SaveProcessd(value) {

    if (!Validtion()) return;
    LoadingOverlay();

    setTimeout(function () {
        SaveCreate(value);
        $.LoadingOverlay("hide");
    }, 2000);
};

SaveCreate = function (value) {

    var page = {
        Id: $('#form_id').val(),
        Title: $('#new_title').val(),
        BodyText: $('#new_bodyText').val().trim(),
        FeaturedImageCaption: $('#new_ImageCaption').val(),
        FeaturedImage: $('#new_page_featureimage').val(),
        Qbicle: { Id: $('#new_qbicle_id').val() },
        AlertsDisplayStatus: $('#new_page_alerts').val(),
        FilesDisplayStatus: $('#new_page_files').val(),
        EventsDisplayStatus: $('#new_page_events').val(),
        PostsDisplayStatus: $('#new_page_postscomments').val(),
        ArticlesDisplayStatus: $('#new_page_articles').val(),
        PublicContactEmail: $('#contact-email').val()
    }
    var tagValues = [];
    var lstTagId = $('#tags_selected').val();
    if (lstTagId === null) lstTagId = [];
    var lstKeyWordId = $('#keyword_selected').val();
    if (tagsOrigin)
        for (var i = 0; i < tagsOrigin.length; i++) {
            var tagsItem = { Id: 0, AssociatedKeyWords: [] };
            if (lstTagId.indexOf(tagsOrigin[i].Id.toString()) !== -1)
                tagsItem.Id = tagsOrigin[i].Id;
            if (tagsOrigin[i].ListKeyWord && tagsOrigin[i].ListKeyWord.length > 0) {
                for (var j = 0; j < tagsOrigin[i].ListKeyWord.length; j++) {
                    if (lstKeyWordId && lstKeyWordId.indexOf(tagsOrigin[i].ListKeyWord[j].Id.toString()) !== -1)
                        tagsItem.AssociatedKeyWords.push({ Id: tagsOrigin[i].ListKeyWord[j].Id });
                }
            }
            if (tagsItem.Id > 0) {
                tagValues.push(tagsItem);
            }
        }
    page.Tags = tagValues;

    var lstFiles = new FormData();
    var featureFile = $("#new_featured_image");
    lstFiles.append("featureimage", featureFile[0].files[0]);
    if (featureFile.length > 0 && featureFile[0].files.length > 0)
        $.ajax({
            type: 'post',
            url: '/CommunityAddEdit/UpdateFeature',
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {

                if (response.actionVal == 1) {
                    page.FeaturedImage = response.Object;
                    $('#new_page_featureimage').val(response.Object);
                    $.ajax({
                        type: 'post',
                        url: '/CommunityAddEdit/SaveProceed',
                        data: { page: page },
                        dataType: 'json',
                        success: function (response) {
                            if (response.actionVal === 1) {
                                cleanBookNotification.createSuccess();
                                $('#form_id').val(response.Object);
                                if (!value)
                                    window.location = '/Community/EditPage?edit=' + response.Object;
                            } else if (response.actionVal == 2) {
                                cleanBookNotification.updateSuccess();
                                if (!value)
                                    window.location = '/Community/EditPage?edit=' + response.Object;
                            }
                            else if (response.actionVal == 3) {
                                cleanBookNotification.error(response.msg, "Qbicles");
                            }
                        },
                        error: function (er) {
                            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
                        }
                    });
                }
                else if (response.actionVal == 3 || response.actionVal == 2) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
}
function Validtion() {
    var isValid1 = true, isValid2 = true, isValid3 = true;
    if ($('#new_ImageCaption').val() === "") {
        cleanBookNotification.error(_L("ERROR_MSG_488"), "Qbicles");
        isValid1 = false;
    }
    if ($('#tags_selected').val() === null) {
        cleanBookNotification.error(_L("ERROR_MSG_489"), "Qbicles");
        isValid2 = false;
    }
    if (!$('#form-community-page').valid()) {
        isValid3 = false;
    }
    if (!isValid1 || !isValid2 || !isValid3) return false;

    return true;
}

function SelectedTagsChange() {

    var lstTagId = $('#tags_selected').val();
    if (lstTagId === null) lstTagId = [];
    $('#form_keyword').empty();

    if (tagsOrigin.length > 0) {
        for (var i = 0; i < tagsOrigin.length; i++) {
            // check exists Group
            if (lstTagId)
                if (lstTagId.indexOf(tagsOrigin[i].Id.toString()) !== -1) {
                    var keyword = '';
                    for (var j = 0; j < tagsOrigin[i].ListKeyWord.length; j++) {
                        keyword += "<a href=\"javascript:void(0);\" class=\"topic-label\">";
                        keyword += "<span style=\"margin-bottom:5px; margin-right:5px;\" class=\"label label-info\">" + tagsOrigin[i].ListKeyWord[j].Name + "</span></a>";
                    }
                    $('#form_keyword').append(keyword);
                }
        }
    }
    // reset select2 
    $('select').not('.multi-select').select2();
}


/// edit 

function previewPage() {
    $(".previewPage").removeClass("hidden");
    $(".editPage").addClass("hidden");
    previewArticles();
    $('#preview_id_bodyText').empty();
    $('#preview_id_bodyText').append($('#edit_page_bodytext').val().replace(/(\r\n|\n)/g, "<br/>"));
    $('#preview-contact-email').text($('#contact-email').val().replace(/(\r\n|\n)/g, "<br/>"));
    if ($('#edit_status_alerts').val() == 0) {
        $('#app_main_nav_alerts').addClass('a-btn-disabled');
    } else {
        $('#app_main_nav_alerts').removeClass('a-btn-disabled');
    }
    //post
    if ($('#edit_status_post').val() == 0) {
        $('#app_main_nav_post').addClass('a-btn-disabled');
    } else {
        $('#app_main_nav_post').removeClass('a-btn-disabled');
    }
    // file
    if ($('#edit_status_file').val() == 0) {
        $('#app_main_nav_files').addClass('a-btn-disabled');
    } else {
        $('#app_main_nav_files').removeClass('a-btn-disabled');
    }
    // event
    if ($('#edit_status_event').val() == 0) {
        $('#app_main_nav_events').addClass('a-btn-disabled');
    } else {
        $('#app_main_nav_events').removeClass('a-btn-disabled');
    }

}
function returnEdit() {
    $(".editPage").removeClass("hidden");
    $(".previewPage").addClass("hidden");
}
function changeFeatureImage(ev) {
    if (ValidateFileImage(ev)) {
        var image = $("#file_feature_image");
        reader.onload = function (e) {
            $("#show_featureimage").attr("src", e.target.result);
        }
        reader.readAsDataURL(image[0].files[0]);
    }
}

function ValidtionUpdate() {
    var isValid2 = true, isValid3 = true;

    if ($('#tags_selected').val() === null) {
        cleanBookNotification.error(_L("ERROR_MSG_489"), "Qbicles");
        isValid2 = false;
    }
    if (!$('#form-community-page').valid()) {
        isValid3 = false;
    }
    if (!isValid2 || !isValid3) return false;

    return true;
}

function updatePage() {
    if (!ValidtionUpdate()) return;
    LoadingOverlay();

    setTimeout(function () {
        SaveUpdate();
        $.LoadingOverlay("hide");
    }, 2000);
};

SaveUpdate = function() {
    var page = {
        Id: $('#form_id').val(),
        Title: $('#edit_page_title').val(),
        BodyText: $('#edit_page_bodytext').val().trim(),
        FeaturedImageCaption: $('#edit_page_caption').val(),
        FeaturedImage: $('#edit_page_featureimage').val(),
        Qbicle: { Id: $('#qbicles_page').val() },
        AlertsDisplayStatus: $('#edit_status_alerts').val(),
        FilesDisplayStatus: $('#edit_status_file').val(),
        EventsDisplayStatus: $('#edit_status_event').val(),
        PostsDisplayStatus: $('#edit_status_post').val(),
        ArticlesDisplayStatus: $('#edit_status_articles').val(),
        PublicContactEmail: $('#contact-email').val()

    }
    var tagValues = [];
    var lstTagId = $('#tags_selected').val();
    if (lstTagId === null) lstTagId = [];
    var lstKeyWordId = $('#keyword_selected').val();
    if (tagsOrigin)
        for (var i = 0; i < tagsOrigin.length; i++) {
            var tagsItem = { Id: 0, AssociatedKeyWords: [] };
            if (lstTagId.indexOf(tagsOrigin[i].Id.toString()) !== -1)
                tagsItem.Id = tagsOrigin[i].Id;
            if (tagsOrigin[i].ListKeyWord && tagsOrigin[i].ListKeyWord.length > 0) {
                for (var j = 0; j < tagsOrigin[i].ListKeyWord.length; j++) {
                    if (lstKeyWordId && lstKeyWordId.indexOf(tagsOrigin[i].ListKeyWord[j].Id.toString()) !== -1)
                        tagsItem.AssociatedKeyWords.push({ Id: tagsOrigin[i].ListKeyWord[j].Id });
                }
            }
            if (tagsItem.Id > 0) {
                tagValues.push(tagsItem);
            }
        }
    page.Tags = tagValues;
    var folls = $('#edit_page_followers').val();
    if (folls && folls.length > 0) {
        for (var k = 0; k < folls.length; k++) {
            if (k === 0) {
                page.Follower_1 = { Id: folls[k] }
            } else if (k === 1) {
                page.Follower_2 = { Id: folls[k] }
            } else if (k === 2) {
                page.Follower_3 = { Id: folls[k] }
            } else if (k === 3) {
                page.Follower_4 = { Id: folls[k] }
            } else if (k === 4) {
                page.Follower_5 = { Id: folls[k] }
            }
        }
    }
    var tr_id = $('tbody.body_table tr td').find('input.savetrid');
    var file_value = $('tbody.body_table tr td').find('input.arrow_articles_file');
    var articlesFiles = new FormData();
    for (var i = 0; i < tr_id.length; i++) {
        if (file_value[i].files.length > 0) {
            articlesFiles.append($(tr_id[i]).val(), file_value[i].files[0]);
        }
    }

    var lstFiles = new FormData();
    var featureFile = $("#file_feature_image");
    lstFiles.append("featureImage", featureFile[0].files[0]);
    if (featureFile.length > 0 && featureFile[0].files.length > 0) {
        $.ajax({
            type: 'post',
            url: '/CommunityAddEdit/UpdateFeature',
            data: lstFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                if (response.actionVal === 1) {
                    page.FeaturedImage = response.Object;
                    $('#edit_page_featureimage').val(response.Object);
                    // upload image articles
                    $.ajax({
                        type: 'post',
                        url: '/CommunityAddEdit/UpdateImage',
                        data: articlesFiles,
                        contentType: false, // Not to set any content header  
                        processData: false, // Not to process data  
                        dataType: 'json',
                        success: function (response) {
                            if (response.actionVal === 1) {
                                var lstImage = response.Object == null ? [] : response.Object;
                                for (var j = 0; j < lstImage.length; j++) {
                                    $('#' + lstImage[j].Id + ' input.arrow_articles_image').val(lstImage[j].Image);
                                }
                                var lstTr = $('tbody.body_table tr');
                                var articles = [];
                                for (var k = 0; k < lstTr.length; k++) {
                                    var ar = {
                                        Id: $(lstTr[k]).find('td input.arrow_articles_id').val(),
                                        Title: $(lstTr[k]).find('td input.arrow_articles_title').val(),
                                        Image: $(lstTr[k]).find('td input.arrow_articles_image').val(),
                                        Source: $(lstTr[k]).find('td input.arrow_articles_source').val(),
                                        IsDisplayed: $(lstTr[k]).find('td input.isDisabled_articles').val(),
                                        URL: $(lstTr[k]).find('td input.arrow_articles_url').val(),
                                        DisplayOrder: $(lstTr[k]).find('td input.arrow_displayorder').val()
                                    };
                                    articles.push(ar);
                                }
                                page.Articles = articles;
                                // save page
                                $.ajax({
                                    type: 'post',
                                    url: '/CommunityAddEdit/SaveProceed',
                                    data: { page: page },
                                    dataType: 'json',
                                    success: function (response) {
                                        if (response.actionVal == 2) {
                                            cleanBookNotification.updateSuccess();
                                            window.location = "/Community/CommunityPage?id=" + response.msgId;
                                        }
                                        else if (response.actionVal == 3) {
                                            cleanBookNotification.error(response.msg, "Qbicles");
                                        }
                                    },
                                    error: function (er) {
                                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                                    }
                                });
                            }
                            else if (response.actionVal == 3 || response.actionVal == 2) {
                                cleanBookNotification.error(response.msg, "Qbicles");
                            }
                        },
                        error: function (er) {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    });

                }
                else if (response.actionVal == 3 || response.actionVal == 2) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    } else {
        $.ajax({
            type: 'post',
            url: '/CommunityAddEdit/UpdateImage',
            data: articlesFiles,
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            dataType: 'json',
            success: function (response) {
                if (response.actionVal === 1) {
                    var lstImage = response.Object == null ? [] : response.Object;
                    for (var j = 0; j < lstImage.length; j++) {
                        $('#' + lstImage[j].Id + ' input.arrow_articles_image').val(lstImage[j].Image);
                    }
                    var lstTr = $('tbody.body_table tr');
                    var articles = [];
                    for (var k = 0; k < lstTr.length; k++) {
                        var ar = {
                            Id: $(lstTr[k]).find('td input.arrow_articles_id').val(),
                            Title: $(lstTr[k]).find('td input.arrow_articles_title').val(),
                            Image: $(lstTr[k]).find('td input.arrow_articles_image').val(),
                            Source: $(lstTr[k]).find('td input.arrow_articles_source').val(),
                            IsDisplayed: $(lstTr[k]).find('td input.isDisabled_articles').val(),
                            URL: $(lstTr[k]).find('td input.arrow_articles_url').val(),
                            DisplayOrder: $(lstTr[k]).find('td input.arrow_displayorder').val()
                        };
                        articles.push(ar);
                    }
                    page.Articles = articles;
                    // save page
                    $.ajax({
                        type: 'post',
                        url: '/CommunityAddEdit/SaveProceed',
                        data: { page: page },
                        dataType: 'json',
                        success: function (response) {
                            if (response.actionVal == 2) {
                                cleanBookNotification.updateSuccess();
                                window.location = "/Community/CommunityPage?id=" + response.msgId;
                            }
                            else if (response.actionVal == 3) {
                                cleanBookNotification.error(response.msg, "Qbicles");
                            }
                        },
                        error: function (er) {
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        }
                    });
                }
                else if (response.actionVal == 3 || response.actionVal == 2) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                }
            },
            error: function (er) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        });
    }
}
// form add articles
var imageStr = null;
var idBuild = '';
var isUpdate = false;
function addArticles() {
    // valid form
    var valid = true;
    var re = new RegExp(/^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$/);
    if ($("#form_articles_name").val() === "") {
        valid = false;
        $("#form-add-artile").validate().showErrors({ article_title: "This field is required." });
    }
    if ($("#form_articles_source").val() === "") {
        valid = false;
        $("#form-add-artile").validate().showErrors({ article_source: "This field is required." });
    }
    if ($("#form_articles_image").val() === "" && !isUpdate) {
        valid = false;
        $("#form-add-artile").validate().showErrors({ article_image: "This field is required." });
    }
    if ($("#form_articles_url").val() === "") {
        valid = false;
        $("#form-add-artile").validate().showErrors({ article_link: "This field is required." });
        
    } else {
        if (!re.test($("#form_articles_url").val())) {
            valid = false;
            $("#form-add-artile").validate().showErrors({ article_link: "This entry is not a web address." });
        }
    }
    
    
    if(!valid) return false;
    var url_link = $('#form_articles_url').val();
    //if (url_link.length > 0 && url_link.indexOf("http://") != 0 && url_link.indexOf("https://") != 0) {
    //    url_link = "http://" + url_link;
    //}
    if (!isUpdate) {

        var articles = {
            Title: $('#form_articles_name').val(),
            Image: imageStr,
            file: $("#form_articles_image").clone(true),
            url: url_link,
            isDisplay: true,
            source: $('#form_articles_source').val()
        }
        idBuild = UniqueId();
        var bodyTable = $('tbody.body_table');
        var trCount = $('tbody.body_table tr');
        if (trCount.length == 1 && $('tbody.body_table tr.odd td').length === 1) {
            bodyTable.empty();
            trCount = $('tbody.body_table tr');
        }
        var trStr = '';
        trStr += "<tr id=\"manage_articles_" + idBuild + "\"> <td> <input type=\"hidden\" class=\"savetrid\" value=\"manage_articles_" + idBuild + "\"/>";
        trStr += " " + (trCount.length + 1) + "<input type=\"hidden\" class=\"arrow_displayorder\" value=\"" + (trCount.length + 1) + "\"/> <input type=\"hidden\" class=\"arrow_articles_id\" value=\"0\" />";
        trStr += "</td> <td> <img src=\"" + articles.Image + "\" class=\"img-responsive\"> <input type=\"hidden\" class=\"arrow_articles_image\" value=\"\" />";
        trStr += "<input type=\"file\" class=\"arrow_articles_file hidden\" /> </td> <td> <span style=\"font-size: 13px\"> " + articles.Title + " </span> <input type=\"hidden\" class=\"arrow_articles_title\" value=\"" + articles.Title + "\" />";
        trStr += "</td> <td> <input type=\"hidden\" class=\"arrow_articles_url\" value=\"" + articles.url + "\" /> ";
        trStr += "<input type=\"hidden\" class=\"arrow_articles_source\" value=\"" + articles.source + "\" />";
        if (articles.url.indexOf('http://') == 0 || articles.url.indexOf('https://') == 0) {
            trStr += "<a href=\"" + articles.url + "\" target=\"_blank\">" + articles.source + "</a> </td> <td>  " + (new Date()).toDateString() + " </td> <td>";
        } else {
            trStr += "<a href=\"http://" + articles.url + "\" target=\"_blank\">" + articles.source + "</a> </td> <td>  " + (new Date()).toDateString() + " </td> <td>";
        }
        trStr += "<input type=\"checkbox\" class=\"isDisabled_articles hidden\" value=\"" + articles.isDisplay + "\" />";
        trStr += "<span class=\"label label-success unpublish label-lg\">Active</span>  <span class=\"label label-danger public hidden label-lg\">Hidden</span> </td> ";
        trStr += "<td class=\"text-right\"> <div class=\"btn-group options\">";
        trStr += "<button type=\"button\" class=\"btn btn-primary dropdown-toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
        trStr += "<i class=\"fa fa-cog\"></i> &nbsp; Options </button> <ul class=\"dropdown-menu dropdown-menu-right\" style=\"right: 0;\">";
        trStr += "<li><a href=\"javascript:void(0);\" data-toggle=\"modal\" onclick=\"editArticles('" + idBuild + "')\" data-target=\"#app-community-article-add\">Edit</a></li>";
        trStr += "<li class=\"unpublish\"><a href=\"#\" data-toggle=\"modal\" onclick=\"unpublishArticles('" + idBuild + "', false)\" data-target=\"#\">Unpublish</a></li>";
        trStr += "<li class=\"public hidden\"><a href=\"#\" data-toggle=\"modal\" onclick=\"unpublishArticles('" + idBuild + "', true)\" data-target=\"#\">Publish</a></li>";
        trStr += "<li><a href=\"javascript:void(0);\" data-toggle=\"modal\" id=\"confirmDelete('" + idBuild + "')\" data-target=\"#\">Delete</a></li> </ul> </div> </td> </tr>";
        bodyTable.append(trStr);

        $('tr#manage_articles_' + idBuild + ' input.arrow_articles_file')[0].files = articles.file[0].files;
        idBuild = '';
    } else {
        if ($("#form_articles_image").clone(true)[0].files.length > 0) {
            $('tr#manage_articles_' + idBuild + ' input.arrow_articles_file')[0].files = $("#form_articles_image").clone(true)[0].files;
            $('tr#manage_articles_' + idBuild + ' img.img-responsive').attr('src', imageStr);
        }
        $('tr#manage_articles_' + idBuild + ' input.arrow_articles_title').val($('#form_articles_name').val());
        $($('#manage_articles_' + idBuild + ' td')[2]).find('span').text($('#form_articles_name').val());

        $('tr#manage_articles_' + idBuild + ' input.arrow_articles_url').val(url_link);
        $($('#manage_articles_' + idBuild + ' td')[3]).find('a').attr('href', "http://" + url_link);
        $('tr#manage_articles_' + idBuild + ' input.arrow_articles_source').val($('#form_articles_source').val());
        $($('#manage_articles_' + idBuild + ' td')[3]).find('a').text($('#form_articles_source').val());
    }
    resetForm();
    imageStr = null;

    $('#app-community-article-add').modal('toggle');
}
function resetForm() {
    $('#form_articles_name').val('');
    $('#form_articles_url').val('');
    $('#form_articles_source').val('');
    $("#form_articles_image").val(null);
}

function changeImage(ev) {
    if (ValidateFileImage(ev)) {
        var imageFeature = $("#form_articles_image");
        reader.onload = function (e) {
            imageStr = e.target.result;
        }
        reader.readAsDataURL(imageFeature[0].files[0]);
    }
    
}
function editArticles(id) {
    resetForm();
    $('#form_articles_name').val($('#manage_articles_' + id + ' input.arrow_articles_title').val());
    $('#form_articles_url').val($('#manage_articles_' + id + ' input.arrow_articles_url').val());
    $('#form_articles_source').val($('#manage_articles_' + id + ' input.arrow_articles_source').val());
    isUpdate = true;
    idBuild = id;
    $('#app-community-article-add h5.title_articles').text('Edit an article');
    $('#app-community-article-add button.btn-success').text('Update');
}
function unpublishArticles(id, value) {
    $('tr#manage_articles_' + id + ' input.isDisabled_articles').val(value);
    if (value == false) {
        $('tr#manage_articles_' + id + ' li.unpublish').addClass('hidden');
        $('tr#manage_articles_' + id + ' li.public').removeClass('hidden');
        $('tr#manage_articles_' + id + ' span.unpublish').addClass('hidden');
        $('tr#manage_articles_' + id + ' span.public').removeClass('hidden');
    } else {
        $('tr#manage_articles_' + id + ' li.public').addClass('hidden');
        $('tr#manage_articles_' + id + ' li.unpublish').removeClass('hidden');
        $('tr#manage_articles_' + id + ' span.public').addClass('hidden');
        $('tr#manage_articles_' + id + ' span.unpublish').removeClass('hidden');
    }
}
function confirmDelete(id) {
    idBuild = id;
}
function cancelConfirm() {
    idBuild = '';
}
function deleteArticles() {
    if (idBuild.length > 0) {
        $('tr#manage_articles_' + idBuild).remove();
    }
    $('#confirm-delete').modal('toggle');
}
function addNewArticles() {
    resetForm();
    isUpdate = false;
    idBuild = '';
}


// change 

function titleChange() {
    $('h4.title_preview').text($('#edit_page_title').val());
}
function changeCaption() {
    $('#preview_caption').text($('#edit_page_caption').val());
}
function previewArticles() {
    $('#listItem_articles').empty();
    // read table
    var lstTr = $('tbody.body_table tr');
    var articles = [];
    for (var k = 0; k < lstTr.length; k++) {
        var ar = {
            Title: $(lstTr[k]).find('td input.arrow_articles_title').val(),
            Image: $(lstTr[k]).find('td img.img-responsive').attr('src'),
            URL: $(lstTr[k]).find('td input.arrow_articles_url').val(),
            DisplayOrder: $(lstTr[k]).find('td input.arrow_displayorder').val(),
            IsDisplayed: $(lstTr[k]).find('td input.isDisabled_articles').val()
        };
        if (ar && ar.length>0 && ar.IsDisplayed.toLowerCase() == "true")
            articles.push(ar);
    }
    if (articles.length > 0) {
        
        var arStr = '<div class=\"list_item\">';
        for (var i = 0; i < articles.length; i++) {

            arStr += "<div class=\"item\"> <div class=\"community-card\"> <a href=\"javascript:void(0);\">";
            arStr += "<div class=\"upper\" style=\"background-image: url('" + articles[i].Image + "');\">&nbsp;</div>";
            arStr += "</a> <section class=\"info\"> <a href=\"" + articles[i].URL + "\"> <h2>" + articles[i].Title + "</h2>";
            arStr += "<span>" + articles[i].DisplayOrder + "</span>  </a> </section> </div> </div>";

        }
        arStr += "</div>";
        $('#listItem_articles').append(arStr);
        $("#listItem_articles .list_item").owlCarousel({
            responsive: {
                0: {
                    items: 1
                },
                480: {
                    items: 2
                },
                978: {
                    items: 1
                },
                1200: {
                    items: 2
                }
            },
            margin: 30,
            nav: false,
            dots: true,
            loop: true,
            autoplay: true,
            autplayHoverPause: true,
            autoplayTimeout: 3000
        });
    }
}