$(function () {
    var isBusy = false;
    var $viewDisplay;
    var uiEls = {
        $form_topic_addedit: $('#form_topic_addedit'),
        $form_topic_delete: $('#frm_topic_delete'),
        $form_topic_move: $('#frm_move_topic')
    };
    viewFunc = {
        loadTopic: function (id) {
            $.ajax({
                url: "/Topics/LoadTopicById",
                data: { id: id },
                type: "GET",
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
            }).done(function (refModel) {
                if (refModel.result && refModel.Object) {
                    var formId = "#form_topic_addedit ";
                    $(formId + 'input[name=Name]').val(refModel.Object.Name);
                    $(formId + 'input[name=Summary]').val(refModel.Object.Summary);
                    $(formId + 'input[name=Id]').val(refModel.Object.Id);
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_105"), "Qbicles");
                }
                isBusy = false;
            }).fail(function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                isBusy = false;
            });
        },
        loadTopicManager: function (view) {
            $('#topics').LoadingOverlay("show", { minSize: "70x60px" });
            //var view = $('select[name=select-view]').val();
            $viewDisplay = view;
            //var topics = $('#tp-select').val();
            $.ajax({
                url: "/Qbicles/GenerateTopicManager",
                data: { view: $viewDisplay, topics: [0] },
                //data: { view: $viewDisplay, topics: topics },
                type: "post",
                dataType: "html",
                success: function (data) {
                    $('#topics .spacing').html(data);
                    $('#topics').LoadingOverlay("hide");
                    totop();
                }
            }).fail(function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                $('#topics').LoadingOverlay("hide");
            });
        },
        resetFormTopic: function () {
            uiEls.$form_topic_addedit.trigger("reset");
            $('#topic-add input.valid').removeClass("valid");
            $('#topic-add input.error').removeClass("error");
        },
        countAssociatedActivities: function (id) {
            $.ajax({
                url: "/Topics/CountAssociatedActivities",
                data: { id: id },
                type: "GET",
                dataType: "json",
                beforeSend: function (xhr) {
                    isBusy = true;
                },
            }).done(function (refModel) {
                if (refModel.result && refModel.Object) {
                    if (refModel.Object.countActivities > 0) {
                        var messWarring = "The topic you are trying to delete has <strong>" + refModel.Object.countActivities + "</strong> associated activities. Please choose a new topic to assign these records to";
                        $('#delete-topic .help-text p').html(messWarring);
                        $('.move-container').show();
                    } else {
                        var messWarring = "The topic you are trying to delete has <strong>" + refModel.Object.countActivities + "</strong> associated activities. Are you sure you want to delete this topic?";
                        $('#delete-topic .help-text p').html(messWarring);
                        $('.move-container').hide();
                    }

                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_105"), "Qbicles");
                }
                isBusy = false;
            }).fail(function () {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                isBusy = false;
            });
        },
        InitTopic: function (qbicleId, selectId, crTopicId) {
            $.ajax({
                type: "post",
                url: "/Topics/GetTopicDelMoveByQbicle",
                datatype: "json",
                data: {
                    key: qbicleId,
                    topicId: crTopicId
                },
                success: function (refModel) {

                    if (refModel.result) {
                        $("#" + selectId).empty().append(refModel.Object);
                        $('#' + selectId).select2();
                    }
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                }
            });
        },
        deleteTopic: function (currentTopicId) {
            $('#delete-topic select[name=topicMoveId] option[value="' + currentTopicId + '"]').hide();
            $('#topicDeleteId').val(currentTopicId);
        },
        moveToTopicModal: function (currentTopicId, activityId) {
            uiEls.$form_topic_move.submit();
        },
        dragMoveToTopic: function (topicMoveId, topicCurrentId, activityId) {
            viewFunc.updateTopic(topicMoveId, topicCurrentId, activityId, false);
        },
        updateTopic: function (topicMoveId, topicCurrentId, activityId, ismodal) {
            $.ajax({
                type: 'Post',
                url: '/Topics/MoveAtivityTopic',
                data: { topicMoveId: topicMoveId, topicCurrentId: topicCurrentId, activityId: activityId },
                dataType: "json",
                async: false,
                beforeSend: function (xhr) { isBusy = true; $.LoadingOverlay("show"); },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('div[avid=' + activityId + ']').attr('c-topicid', topicMoveId);
                        $('#move-to-group').modal('hide');
                        cleanBookNotification.success(_L("ERROR_MSG_106"), "Topic");
                        $('#move_cr_topicId').val(0);
                        $('#move_cr_AvId').val(0);
                        if (ismodal) {
                            $('div[avid=' + activityId + ']').prependTo('div[topicid=' + topicMoveId + ']');
                        }
                    } else if (data.result === false && data.msg)
                        cleanBookNotification.error(data.msg, "Qbicles");
                    else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        },
        ReSortable:function()
        {
            $("#topic-pipeline .column").sortable({
                connectWith: ".column",
                handle: ".portlet-content",
                revert: 0,
                cancel: ".portlet-toggle",
                placeholder: "portlet-placeholder ui-corner-all",
                receive: function (event, ui) {
                    var item = ui.item[0];
                    if (item) {
                         
                        //avid="tk.Id" c-topicid="item.Id"
                        var avid = $(item).attr("avid");
                        var c_topicid = $(item).attr("c-topicid");
                        var topicid = $(item.parentElement).attr("topicid");
                        viewFunc.dragMoveToTopic(topicid, c_topicid, avid);
                    }
                }
            });
        }
    };

    $(document).ready(function () {
        $.validator.addMethod("maxlen", function (value, element, len) {
            return value == "" || value.length <= len;
        }, "Please enter no more than 250 characters.");
        uiEls.$form_topic_addedit.validate(
        {
            rules: {
                Name: {
                    required: true,
                    minlength: 5,
                    maxlength: 50
                }
            }
        });
        uiEls.$form_topic_move.submit(function (e) {
            e.preventDefault();
            if (isBusy) {
                return;
            }
            var currentTopicId = $('#move_cr_topicId').val();
            var activityId = $('#move_cr_AvId').val();
            viewFunc.updateTopic($('#frm_move_topic select[name=topicMoveId]').val(), currentTopicId, activityId, true);
        });
        uiEls.$form_topic_delete.submit(function (e) {
            e.preventDefault();
            if (isBusy) {
                return;
            }
            if ($('#topicDeleteId').val() == "1") {
                cleanBookNotification.error(_L("ERROR_MSG_107"), "Qbicles");
                return;
            }
            $.LoadingOverlay("show");
            $.ajax({
                type: this.method,
                url: this.action,
                data: $(this).serialize(),
                dataType: "json",
                async: false,
                beforeSend: function (xhr) { isBusy = true; },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#delete-topic').modal('hide');
                        var t = $('#table-topic-list').DataTable();
                        t.row("#tp" + $('#topicDeleteId').val()).remove();
                        t.draw();
                        cleanBookNotification.success(_L("ERROR_MSG_107"), "Topic");
                        $('#topicDeleteId').val(0);
                    } else if (data.result === false && data.msg)
                        cleanBookNotification.error(data.msg, "Qbicles");
                    else
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    LoadingOverlayEnd();
                }
            });
        });
        $('#topic-add').on('shown.bs.modal', function () {
            var topicId = parseInt($('#topicId').val());
            if (topicId > 0) {
                $('#topic-add h5.modal-title').text("Edit Topic");
                viewFunc.loadTopic(topicId);
            } else {
                $('#topic-add h5.modal-title').text("Add a Topic");
                var formId = "#form_topic_addedit ";
                $(formId + 'input[name=Name]').val('');
                $(formId + 'input[name=Summary]').val('');
                $(formId + 'input[name=Id]').val(0);
            }
        });
        $('#delete-topic').on('shown.bs.modal', function () {
            var topicId = parseInt($('#topicDeleteId').val());
            if (topicId > 0) {
                viewFunc.countAssociatedActivities(topicId);
                viewFunc.InitTopic($('#tp_QbicleId').val(), "lst_topic_delete", topicId);
            }
        });
        $('#move-to-group').on('shown.bs.modal', function () {
            viewFunc.InitTopic($('#move_tp_QbicleId').val(), "lst_move_topic", $('#move_cr_topicId').val());
        });
        uiEls.$form_topic_addedit.submit(function (e) {
            e.preventDefault();
            if (isBusy) {
                return;
            }
            if (uiEls.$form_topic_addedit.valid()) {
                $.ajax({
                    type: this.method,
                    url: this.action,
                    data: $(this).serialize(),
                    dataType: "json",
                    async: false,
                    beforeSend: function (xhr) { isBusy = true; $.LoadingOverlay("show"); },
                    success: function (data) {
                        isBusy = false;
                        if (data.result) {
                            $('#topic-add').modal('hide');
                            viewFunc.resetFormTopic();
                            cleanBookNotification.success(_L("ERROR_MSG_663"), "Topic");
                            var topic = data.Object;
                            if (topic) {
                                //var typeview = $('select[name=select-view]').val();
                                if ($viewDisplay == "list") {
                                    var actionrow = '<button class="btn btn-warning" data-toggle="modal" data-target="#topic-add" onclick="$(\'#topicId\').val(' + topic.Id + ')"><i class="fa fa-pencil"></i></button>';
                                    if (topic.Name.toLowerCase() != "general")
                                        actionrow += '&nbsp;<button class="btn btn-danger" data-toggle="modal" data-target="#delete-topic" onclick="$(\'#topicDeleteId\').val(' + topic.Id + ');"><i class="fa fa-trash"></i></button>';
                                    var t = $('#table-topic-list').DataTable();
                                    var row = t.row.add([
                                          topic.Name,
                                          topic.Summary,
                                          topic.CreatedDate,
                                          topic.Creator,
                                          topic.isTrader ? '<span class="label label-lg label-info">Trader</span>' : '',
                                          topic.Instances,
                                          actionrow
                                    ]).node().id = 'tp' + topic.Id;
                                    if ($("#tp" + topic.Id).length > 0) {
                                        t.row("#tp" + topic.Id).remove();
                                    }
                                    t.draw();
                                }else
                                {
                                    var  pipelineBlock= '<div class="pipeline-block"><div class="topic-detail">'+topic.Name+'</div><div class="horizontal-portlets">';
                                    pipelineBlock += '<div id="tp_line_' + topic.Id + '" topicid="' + topic.Id + '" class="column ui-sortable"></div></div></div>';
                                    $('#topic-pipeline div.pipeline').append(pipelineBlock);
                                    viewFunc.ReSortable();
                                }
                                addTopicToFilter(topic.Id, topic.Name);
                            }

                        } else if (data.result === false && data.msg)
                            uiEls.$form_topic_addedit.validate().showErrors({ Name: data.msg });
                        else
                            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        LoadingOverlayEnd();
                    },
                    error: function (data) {
                        isBusy = false;
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        LoadingOverlayEnd();
                    }
                });
            }
        });

    });
})
