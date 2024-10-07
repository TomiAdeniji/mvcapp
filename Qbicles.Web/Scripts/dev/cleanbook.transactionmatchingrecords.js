
var postDisplayedCount = 0;
var mediaDisplayedCount = 0;

function loadMoreCommentTask(activityKey, activitiesPageSize) {
    $.ajax({
        url: "/Qbicles/LoadMoreActivityPosts",
        type: "POST",
        data: {
            activityKey: activityKey,
            size: postDisplayedCount * activitiesPageSize
        },
        dataType: 'html',
        success: function (refModel) {
            if (refModel.result) {
                $('#list-comments-task').append(refModel.msg);
                postDisplayedCount = postDisplayedCount + activitiesPageSize;
                if (refModel.actionVal <= postDisplayedCount) {
                    $('#btn-load-more-comment-task').hide();
                }
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_58"), "Qbicles");
            }
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}

function loadMoreMediaTask(activitiesPageSize) {
    $.ajax({
        url: '/TransactionMatching/LoadMoreMedia',
        data: { pageSize: mediaDisplayedCount + activitiesPageSize },
        cache: false,
        type: "POST",
        dataType: 'html',
        beforeSend: function (xhr) {
        },
        success: function (response) {
            if (response === "") {
                $('#btnLoadMedias').remove();
                return;
            }
            $('#list-medias').append(response).hide().fadeIn(250);
            mediaDisplayedCount = mediaDisplayedCount + activitiesPageSize;
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
        }
    });
    
}








var
    $ManyToMany = true,
    $ReferenceAndDate = true,
    $ReferenceToReference1AndDate = true,
    $ReferenceToDescriptionAndDate = true,
    $Reference = true,
    $ReferenceToReference1 = true,
    $ReferenceToDescription = true,
    $DescriptionAndDate = true,
    $Description = true,
    $AmountAndDate = true,
    $Reversals = true,
    $input_daterange = $("input[name='daterange']"),
    $dateRangeGroup = $("#dateRangeGroup"),
    $button_RunTask = $("#button_RunTask"),
    $button_ViewRules = $("#button_ViewRules"),
    $hidden_TaskId = $("#TaskId").val(),
    $hidden_TaskInstanceId = $("#TaskInstanceId").val(),
    $hidden_TransactionMatchingTaskId = 0,
    $hidden_accountId = $("#accountId").val(),
    $hidden_accountId2 = $("#accountId2").val(),
    $hidden_TransactionMatchingTypeId = $("#transactionMatchingTypeId").val(),
    $modal_rules_view = $("#modal-rules-view"),
    $progressMatching = $("#progressMatching"),
    $tableReconciled = $("#table_Reconciled"),
    $task_automatic = $("#task_automatic"),
    $task_manual = $("#task_manual"),
    $task_unmatched = $("#task_unmatched"),
    $div_review_matches_auto = $("#div_review_matches_auto"),
    $div_review_matches_manual = $("#div_review_matches_manual"),
    $modal_review_matches_auto = $("#review_matches_auto"),
    $modal_review_matches_manual = $("#review_matches_manual"),
    $modal_unmatch = $("#modal-unmatch"),
    $matchingGroupId = 0,
    $srtMatch = "",
    $table_creditA = $("#table_creditA"),
    $table_debitB = $("#table_debitB"),
    $table_debitA = $("#table_debitA"),
    $table_creditB = $("#table_creditB"),
    $sumDebitAmount = $("#sumDebitAmount"),
    $sumCreditAmount = $("#sumCreditAmount"),
    $btnrules = $("#btnrules"),
    $sumDebitAmountSelected = 0,
    $sumCreditAmountSelected = 0,
    $selectedCreditsA = 0,
    $selectedDebitB = 0,
    $selectedDebitA = 0,
    $selectedCreditsB = 0,
    $Sum_selectedCreditsA = 0,
    $Sum_selectedDebitB = 0,
    $Sum_selectedDebitA = 0,
    $Sum_selectedCreditsB = 0,
    $reconcileType = "",
    _transactionRecords = [],
    $tdDelete = null,
    $trDelete = null,
    $modal_review_matches_Title = $("#review_matches_manual [class='modal-title']")//,
    ;
function MethodCheck(e, id) {
    switch (id) {
        case "chkManyToMany":
            $ManyToMany = e.checked;
            break;
        case "chkReferenceAndDate":
            $ReferenceAndDate = e.checked;
            $("#ReferenceAndDateGroup").toggle(this.checked);
            if (e.checked) {
                $('#chkReferenceToReference1AndDate').prop('checked', true);
                $('#chkReferenceToDescriptionAndDate').prop('checked', true);
                $ReferenceToReference1AndDate = true;
                $ReferenceToDescriptionAndDate = true;
            }
            else {
                $('#chkReferenceToReference1AndDate').prop('checked', false);
                $('#chkReferenceToDescriptionAndDate').prop('checked', false);
                $ReferenceToReference1AndDate = false;
                $ReferenceToDescriptionAndDate = false;
            }
            break;
        case "chkReferenceToReference1AndDate":
            $ReferenceToReference1AndDate = e.checked;
            break;
        case "chkReferenceToDescriptionAndDate":
            $ReferenceToDescriptionAndDate = e.checked;
            break;
        case "chkReference":
            $Reference = e.checked;
            if (e.checked) {
                $("#reference").css({ 'display': 'block' });
            }
            else {
                $("#reference").css({ 'display': 'none' });
                $("#chkReferenceDate").prop('checked', false);
            }
            break;
        case "chkReferenceToReference1":
            $ReferenceToReference1 = e.checked;
            break;
        case "chkReferenceToDescription":
            $ReferenceToDescription = e.checked;
            break;
        case "chkDescriptionAndDate":
            $DescriptionAndDate = e.checked;
            break;
        case "chkDescription":
            $Description = e.checked;
            if (e.checked) {
                $("#description").css({ 'display': 'block' });
            }
            else {
                $("#description").css({ 'display': 'none' });
                $("#chkDecriptionDate").prop('checked', false);
            }
            break;
        case "chkAmountAndDate":
            $AmountAndDate = e.checked;
            break;
        case "chkReversals":
            $Reversals = e.checked;
            break;
    }
}
function RecalculatedUnmach() {
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/RecalculatedUnmach',
        dataType: 'json',
        data: {
            TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
        },
        success: function (response) {
            if (response.result == true) {
                $('#manual_match').hide();
                $selectedCreditsA = 0;
                $selectedDebitB = 0;
                $selectedDebitA = 0;
                $selectedCreditsB = 0;
                $sumDebitAmount.val(0);
                $sumCreditAmount.val(0);
                $sumDebitAmountSelected = 0;
                $sumCreditAmountSelected = 0;
                $reconcileType = "";
                $('.alert_matches').removeClass('active');
                $('#finish_button2').removeAttr('disabled', '');
                $('#finish_button1').removeAttr('disabled', '');


                //append progress percentages
                $progressMatching.empty();
                $progressMatching.append(response.percentagesProgressBar);
                //append table reconciled
                $tableReconciled.empty();
                $tableReconciled.append(response.tableReconciled);
            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
            }
        }, error: function () {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
        }
    });
}

function InitStartDate() {
    var startDate = null; var endDate = null;
    var d = new Date();
    endDate = d.getDate() + "/" + d.getMonth() + "/" + d.getFullYear();
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/CalculatingStartDate',
        dataType: 'json',
        data: {
            accountIdA: $hidden_accountId,
            accountIdB: $hidden_accountId2,
            selectedDate: startDate,
            taskId: $hidden_TaskId
        },
        success: function (response) {
            var dateformat = $("#hddaterang").attr("dateFormat");
            if (!response.Object) {
                startDate = endDate;// Fix when startDate = null --> Display null.
            }
            else {
                startDate = response.Object;
            }
            $('#lblStartDate').text(startDate + ' to');
            $("#hddaterang").attr("enddate", endDate);
            $("#hddaterang").attr("stardate", startDate);
        }
    });
}
$(document).ready(function () {
    $('#panelClear').css({ 'display': 'none' });
    InitStartDate();
    $("#button_RunTask").show();
    $("#button_ViewRules").show();

    $('.singledate').on('apply.daterangepicker', function (ev, picker) {
        CalculateDateRange(this, picker, false);
        $("#button_RunTask").show();
        $("#button_ViewRules").show();
        $("#variance_2").val($("#hdvariance_2").val()).change();
        $("#variance_1").val($("#hdvariance_1").val()).change();
        $("#Datevariance").val($("#hdDatevariance").val()).change();
    });

    $("#variance_2").val($("#hdvariance_2").val()).change();
    $("#variance_1").val($("#hdvariance_1").val()).change();
    $("#Datevariance").val($("#hdDatevariance").val()).change();

    if ($("#transMatchingTaskId").val() > 0) {
        $hidden_TransactionMatchingTaskId = $("#transMatchingTaskId").val();
        $('#task_output').show();
        //gen progress,table
        _transactionRecords = [];
        RecalculatedUnmach();
    }
    $('.clear-transaction').css({ 'display': 'none' });
    $("#button_RunTask").attr('disabled', '');

    $modal_rules_view.on('hidden.bs.modal', function () {
        $("#button_RunTask").removeAttr('disabled', 'disabled');
    });


    $modal_review_matches_auto.on('hidden.bs.modal', function () {
        _transactionRecords = [];
        $.ajax({
            type: 'post',
            url: '/TransactionMatching/recalculatedUnmach',
            dataType: 'json',
            data: {
                TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
            },
            success: function (response) {
                if (response.result == true) {
                    $('#manual_match').hide();
                    $selectedCreditsA = 0;
                    $selectedDebitB = 0;
                    $selectedDebitA = 0;
                    $selectedCreditsB = 0;
                    $sumDebitAmount.val(0);
                    $sumCreditAmount.val(0);
                    $sumDebitAmountSelected = 0;
                    $sumCreditAmountSelected = 0;
                    $reconcileType = "";
                    $('.alert_matches').removeClass('active');
                    $('#finish_button2').removeAttr('disabled', '');
                    $('#finish_button1').removeAttr('disabled', '');
                    //append progress percentages
                    $progressMatching.empty();
                    $progressMatching.append(response.percentagesProgressBar);
                    //append table reconciled
                    $tableReconciled.empty();
                    $tableReconciled.append(response.tableReconciled);
                }
                else {
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
                }
            }, error: function () {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
            }
        });
    });

    $modal_review_matches_manual.on('hidden.bs.modal', function () {
        _transactionRecords = [];
        $.ajax({
            type: 'post',
            url: '/TransactionMatching/RecalculatedUnmach',
            dataType: 'json',
            data: {
                TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
            },
            success: function (response) {
                if (response.result == true) {
                    $('#manual_match').hide();
                    $selectedCreditsA = 0;
                    $selectedDebitB = 0;
                    $selectedDebitA = 0;
                    $selectedCreditsB = 0;
                    $sumDebitAmount.val(0);
                    $sumCreditAmount.val(0);
                    $sumDebitAmountSelected = 0;
                    $sumCreditAmountSelected = 0;
                    $reconcileType = "";
                    $('.alert_matches').removeClass('active');
                    $('#finish_button2').removeAttr('disabled', '');
                    $('#finish_button1').removeAttr('disabled', '');
                    //append progress percentages
                    $progressMatching.empty();
                    $progressMatching.append(response.percentagesProgressBar);
                    //append table reconciled
                    $tableReconciled.empty();
                    $tableReconciled.append(response.tableReconciled);
                }
                else {
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
                }
            }, error: function () {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
            }
        });
    });



    $button_RunTask.on('click', function () {
        Loading();
        document.body.style.cursor = 'wait';
        $('#transactionMatchingRecords').css('pointer-events', 'none');
        cleanBookNotification.success(_L("ERROR_MSG_61"), "Qbicles");

        var manytomany = $("#chkManyToMany").is(':checked') ? 1 : 0;
        //var onetoone = $("#chkOneToOne").is(':checked') ? 1 : 0;
        var ReferenceAndDate, ReferenceToReference1, ReferenceToDescription, ReferenceToReference1AndDate, ReferenceToDescriptionAndDate;

        var Reference = $("#chkReference").is(':checked') ? 1 : 0;
        var referenceDate = $("#chkReferenceDate").is(':checked') ? 1 : 0;
        if (Reference == 1 && referenceDate == 1) {
            ReferenceAndDate = 1;
            ReferenceToReference1AndDate = 1;
            ReferenceToDescriptionAndDate = 1;
            ReferenceToReference1 = 0;
            ReferenceToDescription = 0;
            Reference = 0;
        }
        else if (Reference == 1) {
            ReferenceAndDate = 0;
            ReferenceToReference1AndDate = 0;
            ReferenceToDescriptionAndDate = 0;
            ReferenceToReference1 = 1;
            ReferenceToDescription = 1;
            Reference = 1;
        }
        else {
            ReferenceAndDate = 0;
            ReferenceToReference1AndDate = 0;
            ReferenceToDescriptionAndDate = 0;
            ReferenceToReference1 = 0;
            ReferenceToDescription = 0;
            Reference = 0;
        }
        var DescriptionAndDate;
        var Description = $("#chkDescription").is(':checked') ? 1 : 0;
        var DescriptionDate = $("#chkDecriptionDate").is(':checked') ? 1 : 0;
        if (Description == 1 && DescriptionDate == 1) {
            DescriptionAndDate = 1;
            Description = 0;
        }
        else {
            DescriptionAndDate = 0;
        }
        var AmountAndDate = $("#chkAmountAndDate").is(':checked') ? 1 : 0;
        var Reversals = $("#chkReversals").is(':checked') ? 1 : 0;
        var Datevariance = $("#Datevariance :selected").val();
        var variance_1 = $("#variance_1 :selected").val();
        var variance_2 = $("#variance_2 :selected").val();
        var variance_name = $("#variance_name").val();
        var rulerId = $("#hdrulerId").val();



        var Configure = {
            TaskId: $hidden_TaskId,
            IsManyToMany_Set: manytomany,
            IsOneToOne_Set: null,
            IsReferenceAndDate_Set: ReferenceAndDate,
            IsRefAndDate_RefToRef1_Set: ReferenceToReference1AndDate,
            IsRefAndDate_RefToDesc_Set: ReferenceToDescriptionAndDate,
            IsReference_Set: Reference,
            IsRef_RefToRef1_Set: ReferenceToReference1,
            IsRef_RefToDesc_Set: ReferenceToDescription,
            IsDescriptionAndDate_Set: DescriptionAndDate,
            IsDescription_Set: Description,
            IsAmountAndDate_Set: AmountAndDate,
            IsReversals_Set: Reversals,
            DateVarianceValue: Datevariance,
            Amount1VarianceValue: variance_1,
            Amount2VarianceValue: variance_2,
            VarianceName: variance_name,
            Id: rulerId
        };
        $.ajax({
            type: 'post',
            url: '/TransactionMatching/SaveTransactionMatching',
            dataType: 'json',
            data: {
                date: $input_daterange.val(),
                taskId: $hidden_TaskId,
                accountId: $hidden_accountId,
                accountId2: $hidden_accountId2,
                transactionMatchingTypeId: $hidden_TransactionMatchingTypeId,
                ConfigureRules: Configure,
                Datevariance: $("#Datevariance :selected").val(),
                taskInstanceId: $("#hdContinute").val() == "1" ? $("#TaskInstanceId").val() : "0"
            },
            success: function (response) {
                if (response.result == true) {
                    $hidden_TaskInstanceId = response.instanceId;
                    $("#TaskInstanceId").val(response.instanceId);
                    $hidden_TransactionMatchingTaskId = response.transactionMatchingTaskId;
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.success(response.message, "Qbicles");
                    $('#task_output').show();
                    $('#run_task').hide();
                    document.body.style.cursor = 'auto';
                    $('#transactionMatchingRecords').css('pointer-events', 'auto');
                    //append progress percentages
                    $progressMatching.append(response.percentagesProgressBar);
                    //append table reconciled
                    $tableReconciled.empty();
                    $tableReconciled.append(response.tableReconciled);
                    RecalculatedUnmach();
                }
                else {
                    $('#task_output').css({ 'display': 'none' });
                    $('#run_task').show();
                    document.body.style.cursor = 'auto';
                    $('#transactionMatchingRecords').css('pointer-events', 'auto');
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.warning(_L("ERROR_MSG_62"), "Qbicles");
                }
                EndLoading();

            }, error: function (xhr, status, error) {
                cleanBookNotification.error(xhr.responseText, "Qbicles");
                document.body.style.cursor = 'auto';
                $('#transactionMatchingRecords').css('pointer-events', 'auto');
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_63"), "Qbicles");
                EndLoading();

            }
        });
    });
});


function CalculateDateRange(ev, picker, isdelete) {
    var startDate = null; var endDate = null;
    if (isdelete === true) {
        startDate = ev.split('-')[0].replace(" ", "");
        endDate = ev.split('-')[1].replace(" ", "");
    }
    else {
        startDate = picker.startDate.format('MM/DD/YYYY');
        endDate = picker.endDate.format('MM/DD/YYYY');
    }

    $.ajax({
        type: 'post',
        url: '/TransactionMatching/CalculatingStartDate',
        dataType: 'json',
        data: {
            accountIdA: $hidden_accountId,
            accountIdB: $hidden_accountId2,
            selectedDate: endDate,
            taskId: $hidden_TaskId
        },
        success: function (response) {
            var dateformat = $("#hddaterang").attr("dateFormat");
            if (!response.Object) {
                startDate = endDate;// Fix when startDate = null --> Display null.
            }
            else {
                startDate = response.Object;
            }
            if (isdelete === true) {
                $('#lblStartDate').text(startDate + ' to');
                $("#hddaterang").attr("enddate", endDate);
                $("#hddaterang").attr("stardate", startDate);
            }
            else {
                $("#hddaterang").attr("enddate", picker.endDate.format(dateformat));
                $("#hddaterang").attr("stardate", startDate);
            }
        }
    });
    //}

}
//review transasction matched auto, manual
function Review_matches_record(match) {
    
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/ReviewMatches',
        dataType: 'json',
        data: {
            TransactionanalysistaskId: $hidden_TransactionMatchingTaskId,
            matchedAuto: match,
            TransactionMatchingTypeId: $hidden_TransactionMatchingTypeId
        },
        success: function (response) {
            if (response.result == true) {
                $('#manual_match').hide();
                $selectedCreditsA = 0;
                $selectedDebitB = 0;
                $selectedDebitA = 0;
                $selectedCreditsB = 0;
                $sumDebitAmount.val(0);
                $sumCreditAmount.val(0);
                $sumDebitAmountSelected = 0;
                $sumCreditAmountSelected = 0;
                $reconcileType = "";
                $('.alert_matches').removeClass('active');
                $('#finish_button2').removeAttr('disabled', '');
                $('#finish_button1').removeAttr('disabled', '');
                if (match) {
                    $div_review_matches_auto.empty();
                    $div_review_matches_auto.append(response.retDivMatched);
                    $modal_review_matches_Title.text($("input[name='modal_Automatically_title']").val());
                    for (var i = 0; i < response.tableCount; i++) {
                        $("#tableMatched_" + i).dataTable().fnDestroy();
                        var table = $("#tableMatched_" + i).DataTable({
                            responsive: true,
                            "lengthChange": false,
                            "pageLength": 10,
                            "order": []
                        });
                    }
                }
                else {
                    $div_review_matches_manual.empty();
                    $div_review_matches_manual.append(response.retDivMatched);
                    $modal_review_matches_Title.text($("input[name='modal_Manually_title']").val());
                    for (var i = 0; i < response.tableCount; i++) {
                        $("#tableMatched_" + i).dataTable().fnDestroy();
                        var table = $("#tableMatched_" + i).DataTable({
                            responsive: true,
                            "lengthChange": false,
                            "pageLength": 10,
                            "order": []
                        });
                    }
                }
            }
            else {
                cleanBookNotification.error(_L("ERROR_MSG_64"), "Qbicles");
            }
        }, error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_64"), "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}

//show unmatch
function ManualMatchClick() {
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/ShowUnmatch',
        dataType: 'json',
        data: {
            TransactionanalysistaskId: $hidden_TransactionMatchingTaskId,
            matchingtype: $("#transactionMatchingTypeId").val()
        },
        success: function (response) {
            if (response.result == true) {
                _transactionRecords = [];
                $selectedCreditsA = 0;
                $selectedDebitB = 0;
                $selectedDebitA = 0;
                $selectedCreditsB = 0;
                $sumDebitAmount.val(0);
                $sumCreditAmount.val(0);
                $sumDebitAmountSelected = 0;
                $sumCreditAmountSelected = 0;
                $reconcileType = "";
                $('.alert_matches').removeClass('active');
                $('#manual_match').show();
                $('#finish_button2').removeAttr('disabled', '');
                $('#finish_button1').removeAttr('disabled', '');
                $('.clear-transaction').css({ 'display': 'block' });
                var type = $("#transactionMatchingTypeId").val();
                if (response.table_creditA == "" && response.table_debitB == "")
                    $('#creditAdebitB').hide();
                else {
                    $('#creditAdebitB').show();
                    $table_creditA.find("tbody").find("tr").remove();
                    $table_creditA.find("tbody").append(response.table_creditA);
                    //style
                    if (response.table_creditA == "")
                        $("#div_creditA").css('background-color', 'gainsboro');
                    else
                        $("#div_creditA").css('background-color', '#fff');

                    $table_debitB.find("tbody").find("tr").remove();
                    $table_debitB.find("tbody").append(response.table_debitB);
                    //style
                    if (response.table_debitB == "")
                        $("#div_debitB").css('background-color', 'gainsboro');
                    else
                        $("#div_debitB").css('background-color', '#fff');

                    var heightCD = Math.max($("#div_creditA").height(), $("#div_debitB").height());
                    $("#div_creditA").height(heightCD);
                    $("#div_debitB").height(heightCD);
                    $("#Sum_CreditsA").html(response.Sum_CreditsA);
                    $("#Sum_DebitB").html(response.Sum_DebitB);
                }
                if (response.table_debitA == "" && response.table_creditB == "")
                    $('#debitAcreditB').hide();
                else {
                    $('#debitAcreditB').show();
                    $table_debitA.find("tbody").find("tr").remove();
                    $table_debitA.find("tbody").append(response.table_debitA);
                    //style
                    if (response.table_debitA == "")
                        $("#div_debitA").css('background-color', 'gainsboro');
                    else
                        $("#div_debitA").css('background-color', '#fff');

                    $table_creditB.find("tbody").find("tr").remove();
                    $table_creditB.append(response.table_creditB);
                    //style
                    if (response.table_creditB == "")
                        $("#div_creditB").css('background-color', 'gainsboro');
                    else
                        $("#div_creditB").css('background-color', '#fff');

                    var heightDC = Math.max($("#div_debitA").height(), $("#div_creditB").height());
                    $("#div_debitA").height(heightDC);
                    $("#div_creditB").height(heightDC);
                    $("#Sum_CreditsB").html(response.Sum_CreditsB);
                    $("#Sum_DebitA").html(response.Sum_DebitA);
                }
                if (document.readyState === 'interactive' || document.readyState === 'complete') {
                    $("html, body").animate({
                        scrollTop: 350
                    },
                        2000);

                }
                if (type == "2") {
                    $(".lblAccount1").html($("#lblaccountname1").html() + " - Unmatched Credits");
                    $(".lblAccount11").html($("#lblaccountname2").html() + " - Unmatched Credits");
                    $(".lblAccount2").html($("#lblaccountname1").html() + " - Unmatched Debits");
                    $(".lblAccount22").html($("#lblaccountname2").html() + " - Unmatched Debits");
                    $("#lblTitle1").html('Credit');
                    $("#lblTitle2").html('Debit');
                }
                else {

                    $(".lblAccount1").html($("#lblaccountname1").html() + " - Unmatched Credits");
                    $(".lblAccount11").html($("#lblaccountname2").html() + " - Unmatched Debits");
                    $(".lblAccount2").html($("#lblaccountname1").html() + " - Unmatched Debits");
                    $(".lblAccount22").html($("#lblaccountname2").html() + " - Unmatched Credits");
                    $("#lblTitle1").html('Debit');
                    $("#lblTitle2").html('Credit');
                }


            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_66"), "Qbicles");
            }
        }, error: function () {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_66"), "Qbicles");
        }
    });

}
//clear all un match recored matched auto, manual
function btnClearFillter() {
    ClearFillter();
    cleanBookNotification.success(_L("ERROR_MSG_352"), "Qbicles");
}

function ClearFillter() {
    BtnUnMatchRecordsClick(0, 'Manual', true);
}
function FindMatchrecord(name, amount, date, obj) {
    $(obj).parent().parent().parent().find("tr").removeClass("selected");
    $(obj).closest("tr").toggleClass("selected");
    var type = $("#transactionMatchingTypeId").val();
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/FindMatchRecord',
        dataType: 'json',
        data: {
            name: name,
            amount: amount,
            TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId,
            matchingtype: type,
            date: date
        },
        success: function (response) {
            if (response.result == true) {
                if (type == "2") {
                    if (name == "creditsA") {
                        $table_debitB.find("tbody").find("tr").remove();
                        $table_debitB.find("tbody").append(response.table);
                    }
                    else if (name == "debitB") {
                        $table_debitA.find("tbody").find("tr").remove();
                        $table_debitA.find("tbody").append(response.table);
                    }
                    else if (name == "debitA") {
                        $table_creditB.find("tbody").find("tr").remove();
                        $table_creditB.find("tbody").append(response.table);
                    }
                    else {
                        $table_creditA.find("tbody").find("tr").remove();
                        $table_creditA.append(response.table);
                    }
                }
                else {
                    if (name == "creditsA") {
                        $table_debitB.find("tbody").find("tr").remove();
                        $table_debitB.find("tbody").append(response.table);
                    }
                    else if (name == "debitB") {
                        $table_creditA.find("tbody").find("tr").remove();
                        $table_creditA.find("tbody").append(response.table);
                    }
                    else if (name == "debitA") {
                        $table_creditB.find("tbody").find("tr").remove();
                        $table_creditB.find("tbody").append(response.table);
                    }
                    else {
                        $table_debitA.find("tbody").find("tr").remove();
                        $table_debitA.append(response.table);
                    }
                }
                $('#clearfillter').removeAttr('disabled');
                $('#panelClear').css({ 'display': 'block' });
            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
            }
        }, error: function () {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
        }
    });
}
//un match recored matched auto, manual
function BtnUnMatchRecordsClick(matchingGroupId, srtMatch, isclear) {
    $matchingGroupId = matchingGroupId; $srtMatch = srtMatch;
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/Unmatched',
        dataType: 'json',
        data: {
            matchingGroupId: $matchingGroupId
        },
        success: function (response) {
            if (response.result == true) {
                $('#matchedGroup_' + $srtMatch + '_' + $matchingGroupId).fadeOut(360).attr('style', 'background-color: red !important');
                $('#matchedGroup_' + $srtMatch + '_' + $matchingGroupId).fadeTo("slow", 0.01, function () { //fade
                    $(this).slideUp("slow", function () { //slide up
                        $(this).remove(); //then remove from the DOM
                    });
                });
                if (matchingGroupId == 0) {

                    $.ajax({
                        type: 'post',
                        url: '/TransactionMatching/RecalculatedUnmach',
                        dataType: 'json',
                        data: {
                            TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
                        },
                        success: function (response) {

                            if (response.result == true) {
                                //append progress percentages
                                $progressMatching.empty();
                                $progressMatching.append(response.percentagesProgressBar);
                                //append table reconciled
                                $tableReconciled.empty();
                                $tableReconciled.append(response.tableReconciled);
                            }
                            else {
                                cleanBookNotification.clearmessage();
                                cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
                            }
                        }, error: function () {
                            cleanBookNotification.clearmessage();
                            cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
                        }
                    });
                }
                if (isclear)
                    ManualMatchClick();
                var btn_matchingGroupId = ".btnremove" + matchingGroupId;
                var tr_delete = $(".modal-content").find(btn_matchingGroupId);
                tr_delete.parent().remove();
                $('#clearfillter').attr('disabled', 'disabled');
                $('#panelClear').css({ 'display': 'none' });
                cleanBookNotification.clearmessage();
                if (!isclear)
                    cleanBookNotification.success(_L("ERROR_MSG_660"), "Qbicles");
            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
            }
        }, error: function () {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_60"), "Qbicles");
        }
    });
}

function CheckMatchs(obj) {
    var checkedName = $(obj).attr("name");
    var type = $("#transactionMatchingTypeId").val();
    var debitAmount = parseFloat($(obj).attr("debitAmount"));
    var creditAmount = parseFloat($(obj).attr("creditAmount"));
    if ($(obj).is(':checked')) {

        if (type == "2") {
            _transactionRecords.push($(obj).attr("value"));

            $(obj).closest("tr").toggleClass("selected");

            if (checkedName == "creditsA") {
                $selectedCreditsA++;
                $Sum_selectedCreditsA = (parseFloat($Sum_selectedCreditsA) + creditAmount).toFixed($decimalPlace);
                $sumCreditAmountSelected += creditAmount;
            } else if (checkedName == "debitB") {
                $selectedDebitB++;
                $Sum_selectedDebitB = (parseFloat($Sum_selectedDebitB) + debitAmount).toFixed($decimalPlace);
                $sumDebitAmountSelected += debitAmount;
            }
            else if (checkedName == "debitA") {
                $selectedDebitA++;
                $Sum_selectedDebitA = (parseFloat($Sum_selectedDebitA) + debitAmount).toFixed($decimalPlace);
                $sumCreditAmountSelected += debitAmount;
            }
            else if (checkedName == "creditsB") {
                $selectedCreditsB++;
                $Sum_selectedCreditsB = (parseFloat($Sum_selectedCreditsB) + creditAmount).toFixed($decimalPlace);
                $sumDebitAmountSelected += creditAmount;
            }
        }
        else {
            _transactionRecords.push($(obj).attr("value"));

            $(obj).closest("tr").toggleClass("selected");

            if (checkedName == "creditsA") {
                $selectedCreditsA++;
                $Sum_selectedCreditsA = (parseFloat($Sum_selectedCreditsA) + creditAmount).toFixed($decimalPlace);
            } else if (checkedName == "debitB") {
                $selectedDebitB++;
                $Sum_selectedDebitB = (parseFloat($Sum_selectedDebitB) + debitAmount).toFixed($decimalPlace);
            }
            else if (checkedName == "debitA") {
                $selectedDebitA++;
                $Sum_selectedDebitA = (parseFloat($Sum_selectedDebitA) + debitAmount).toFixed($decimalPlace);
            }
            else if (checkedName == "creditsB") {
                $selectedCreditsB++;
                $Sum_selectedCreditsB = (parseFloat($Sum_selectedCreditsB) + creditAmount).toFixed($decimalPlace);
            }
            $sumDebitAmountSelected += debitAmount;
            $sumCreditAmountSelected += creditAmount;
        }

    }
    else {
        var index = _transactionRecords.indexOf($(obj).attr("value"));
        if (index > -1)
            _transactionRecords.splice(index, 1);
        $(obj).closest("tr").removeClass("selected");
        if (checkedName == "creditsA") {
            $selectedCreditsA--;
            $Sum_selectedCreditsA = (parseFloat($Sum_selectedCreditsA) - creditAmount).toFixed($decimalPlace);
        } else if (checkedName == "debitB") {
            $selectedDebitB--;
            $Sum_selectedDebitB = (parseFloat($Sum_selectedDebitB) - debitAmount).toFixed($decimalPlace);
        }
        else if (checkedName == "debitA") {
            $selectedDebitA--;
            $Sum_selectedDebitA = (parseFloat($Sum_selectedDebitA) - debitAmount).toFixed($decimalPlace);
        }
        else if (checkedName == "creditsB") {
            $selectedCreditsB--;
            $Sum_selectedCreditsB = (parseFloat($Sum_selectedCreditsB) - creditAmount).toFixed($decimalPlace);
        }
        $sumDebitAmountSelected -= debitAmount;
        $sumCreditAmountSelected -= creditAmount;

    }
    $sumDebitAmount.val($sumDebitAmountSelected.toFixed($decimalPlace));
    $sumCreditAmount.val($sumCreditAmountSelected.toFixed($decimalPlace));

    Manage_selected();
}
function Manage_selected() {

    // no selected
    if ($selectedCreditsA == 0 &&
        $selectedDebitA == 0 &&
        $selectedCreditsB == 0 &&
        $selectedDebitB == 0) {
        $('.alert_matches').removeClass('active');
        $('#finish_button2').removeAttr('disabled', '');
        $('#finish_button1').removeAttr('disabled', '');
    }
    //Reversal
    else if (($selectedCreditsA == 1 && $selectedDebitA == 1 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB == 1 && $selectedDebitB == 1)
    ) {

        $('.alert_matches p').html('Selected </span>1 Reversal</span>');
        $('#reconcile_button').text('Reversal Match');
        $('.alert_matches').addClass('active');
        $('#finish_button2').attr('disabled', '');
        $('#finish_button1').attr('disabled', '');
        $reconcileType = "reversal";
        if ($sumDebitAmount.val() == $sumCreditAmount.val()) {
            $('#reconcile_button').removeAttr('disabled', '');
        }
        else {
            $("#reconcile_button").attr('title', 'Can not Reversal because Credit not equals Debit');
            $('#reconcile_button').attr('disabled', '');
        }

    }
    // Manual
    else if (
        (($selectedCreditsA > 0 && $selectedDebitA == 0) && ($selectedCreditsB > 0 || $selectedDebitB > 0)) ||
        (($selectedCreditsA == 0 && $selectedDebitA > 0) && ($selectedCreditsB > 0 || $selectedDebitB > 0)) ||

        (($selectedCreditsA > 0 || $selectedDebitA > 0) && ($selectedCreditsB > 0 && $selectedDebitB == 0)) ||
        (($selectedCreditsA > 0 || $selectedDebitA > 0) && ($selectedCreditsB == 0 && $selectedDebitB > 0)) ||

        ($selectedCreditsA > 0 && $selectedDebitA > 0 && $selectedCreditsB > 0 && $selectedDebitB > 0)
    ) {
        if ($sumDebitAmount.val() == $sumCreditAmount.val()) {
            $('#reconcile_button').removeAttr('disabled', '');
        }
        else {
            $("#reconcile_button").attr('title', 'Can not Match because Credit not equals Debit');
            $('#reconcile_button').attr('disabled', '');
        }

        $reconcileType = "manual";
        $('.alert_matches p').html('Selected <span>' + ($selectedCreditsA + $selectedDebitA) + '</span> against <span>' + ($selectedCreditsB + $selectedDebitB) + '</span>');
        $('#reconcile_button').text('Manual Match');
        $('.alert_matches').addClass('active');
        $('#finish_button2').attr('disabled', '');
        $('#finish_button1').attr('disabled', '');
    }
    // only one selected
    else if ($selectedCreditsA + $selectedDebitA + $selectedCreditsB + $selectedDebitB == 1) {
        $('.alert_matches').removeClass('active');
        $('#finish_button2').removeAttr('disabled', '');
        $('#finish_button1').removeAttr('disabled', '');
    }
    // only selected one transaction
    else if (($selectedCreditsA > 0 && $selectedDebitA > 0 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA > 0 && $selectedDebitA == 0 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA > 0 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB > 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB == 0 && $selectedDebitB > 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB > 0 && $selectedDebitB > 0)

    ) {
        $('.alert_matches').removeClass('active');
        $('#finish_button2').removeAttr('disabled', '');
        $('#finish_button1').removeAttr('disabled', '');
    }

    else if ((($selectedCreditsA > 2 && $selectedDebitA == 0 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA > 2 && $selectedCreditsB == 0 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB > 2 && $selectedDebitB == 0) ||
        ($selectedCreditsA == 0 && $selectedDebitA == 0 && $selectedCreditsB == 0 && $selectedDebitB > 2)
    )) {
        $('.alert_matches').removeClass('active');
        $('#finish_button2').removeAttr('disabled', '');
        $('#finish_button1').removeAttr('disabled', '');
    }

};

function ReconcileNow() {
    $.ajax({
        url: "/TransactionMatching/ReconcileNow",
        type: "post",
        dataType: "json",
        data: {
            TransactionRecordList: JSON.stringify(_transactionRecords),
            transACount: $selectedCreditsA + $selectedDebitA,
            transBCount: $selectedCreditsB + $selectedDebitB,
            reconcileType: $reconcileType
        }
    }).done(function (data) {
        if (data.result == true) {

            $('.alert_matches').removeClass('active');
            $('#finish_button2').removeAttr('disabled', '');
            $('#clearfillter').removeAttr('disabled', '');

            for (var i = 0; i < _transactionRecords.length; i++) {
                var trId = "trRemoveId-" + _transactionRecords[i];
                $("#" + trId).css("background-color", "#FF3700");
                $("#" + trId).fadeOut(1500, function () {
                    $("#" + trId).remove();
                });
            }
            // changes total Credits, Debit
            var total_remaining_CRA = (parseFloat($("#Sum_CreditsA").text().replace(/,/g, '')) - $Sum_selectedCreditsA).toFixed($decimalPlace).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
            $("#Sum_CreditsA").html(total_remaining_CRA);
            var total_remaining_DBB = (parseFloat($("#Sum_DebitB").text().replace(/,/g, '')) - $Sum_selectedDebitB).toFixed($decimalPlace).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
            $("#Sum_DebitB").html(total_remaining_DBB);
            var total_remaining_DBA = (parseFloat($("#Sum_DebitA").text().replace(/,/g, '')) - $Sum_selectedDebitA).toFixed($decimalPlace).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
            $("#Sum_DebitA").html(total_remaining_DBA);
            var total_remaining_CRB = (parseFloat($("#Sum_CreditsB").text().replace(/,/g, '')) - $Sum_selectedCreditsB).toFixed($decimalPlace).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
            $("#Sum_CreditsB").html(total_remaining_CRB);
            //Reset VALUE
            $Sum_selectedCreditsA = 0;
            $Sum_selectedCreditsB = 0;
            $Sum_selectedDebitA = 0;
            $Sum_selectedDebitB = 0;
            _transactionRecords = [];
            $selectedCreditsA = 0;
            $selectedDebitB = 0;
            $selectedDebitA = 0;
            $selectedCreditsB = 0;
            $.ajax({
                type: 'post',
                url: '/TransactionMatching/RecalculatedUnmach',
                dataType: 'json',
                data: {
                    TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
                },
                success: function (response) {
                    if (response.result == true) {
                        //$('#manual_match').hide();
                        //append progress percentages
                        $progressMatching.empty();
                        $progressMatching.append(response.percentagesProgressBar);
                        //append table reconciled
                        $tableReconciled.empty();
                        $tableReconciled.append(response.tableReconciled);
                    }
                    else {
                        cleanBookNotification.clearmessage();
                        cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
                    }
                }, error: function () {
                    cleanBookNotification.clearmessage();
                    cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
                }
            });

            ClearFillter();
            cleanBookNotification.clearmessage();
            cleanBookNotification.success(_L("ERROR_MSG_353"), "Qbicles");
        }
        else {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
            return;
        }
    }).error(function (data) {
        cleanBookNotification.clearmessage();
        cleanBookNotification.error(_L("ERROR_MSG_65"), "Qbicles");
        return;
    });
}

function FinishRemaining() {
    Loading("Finish Remaining");
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/FinishRemaining_Alerts',
        dataType: 'json',
        data: {
            taskInstanceId: $hidden_TaskInstanceId,
            taskName: $("#lbltaskname").html(),
            account1: $("#lblaccountname1").html(),
            account2: $("#lblaccountname2").html()
        },
    });
    var taskid = $("#TaskId").val();
    var taskName = $("#lbltaskname").html();
    var TaskInstanceId = $("#TaskInstanceId").val();
    var accountId = $("#accountId").val();
    var accountName = $("#lblaccountname1").html();
    var accountId2 = $("#accountId2").val();
    var accountName2 = $("#lblaccountname2").html();
    var transactionMatchingTypeId = $("#transactionMatchingTypeId").val();
    var transMatchingTaskId = $("#transMatchingTaskId").val();
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/FinishRemaining',
        dataType: 'json',
        data: {
            accountName: accountName,
            accountId: accountId,
            accountName2: accountName2,
            accountId2: accountId2,
            taskid: taskid,
            taskname: taskName,
            transactionMatchingTypeId: transactionMatchingTypeId,
            transactionmatchingtaskId: transMatchingTaskId,
            taskInstanceId: $hidden_TaskInstanceId
        },
        success: function (response) {
            if (response.taskid > 0) {
                cleanBookNotification.clearmessage();
                cleanBookNotification.success(_L("ERROR_MSG_354"), "Qbicles");
                EndLoading();
                ViewTransactionMatchingReport(response.accountName, response.accountId, response.accountName2,
                    response.accountId2, response.taskid, response.taskName, response.transactionMatchingTypeId,
                    response.transactionmatchingTaskId, response.taskInstanceId,
                    response.userName, response.date, response.time, response.countTransactionsMatched,
                    response.countTransactionsManual, response.countTransactionsUnMatched);
            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_67"), "Qbicles");
            }
            EndLoading();
        }, error: function () {
            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_67"), "Qbicles");
            EndLoading();
        }
    });
}
function DeleteMatch() {

    $("#manual_match").css({ "display": "none" });
    Loading();
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/Revise',
        dataType: 'json',
        data: {
            TransactionMatchingTaskId: $hidden_TransactionMatchingTaskId
        },
        success: function (response) {

            if (response.result == true) {

                var dateRange = $("#hddaterang").attr("stardate") + "-" + $("#hddaterang").attr("enddate");
                CalculateDateRange(dateRange, true, true);
                $("#button_RunTask").show();
                $("#button_ViewRules").show();
                $("#button_RunTask").attr('disabled', '');
                cleanBookNotification.clearmessage();
                cleanBookNotification.success(_L("ERROR_MSG_68"), "Qbicles");
                $('#task_output').css({ 'display': 'none' });
                $('#run_task').css({ 'display': 'block' });
                $('#manual_match').css({ 'display': 'none' });
                $('#panelClear').css({ 'display': 'none' });
                document.body.style.cursor = 'auto';
                $('#transactionMatchingRecords').css('pointer-events', 'auto');
                $("#variance_2").val($("#hdvariance_2").val()).change();
                $("#variance_1").val($("#hdvariance_1").val()).change();
                $("#Datevariance").val($("#hdDatevariance").val()).change();
                $("#hddaterang").attr("enddate", "");
                $("#hddaterang").attr("stardate", "");
                $("#hdContinute").val(0);

            }
            else {
                cleanBookNotification.clearmessage();
                cleanBookNotification.error(_L("ERROR_MSG_69"), "Qbicles");
            }

            EndLoading();
        }, error: function () {

            cleanBookNotification.clearmessage();
            cleanBookNotification.error(_L("ERROR_MSG_69"), "Qbicles");
            EndLoading();
        }
    });
}
function Revise() {
    $("#task_output").css({ "display": "none" });
    $("#run_task").css({ "display": "block" });
    $("#manual_match").css({ "display": "none" });
    $("#hdContinute").val(1);
    $("#hddaterang").attr("enddate", "");
    $("#hddaterang").attr("stardate", "");
    $("#drp_autogen0").html('');
    $("#drp_autogen0").append('Select end date for analysis <span class="ui-button-icon-space"> </span><span class="ui-button-icon ui-icon ui-icon-triangle-1-s" style="float: right;"></span>');
}

function SaveAndContinue() {
    cleanBookNotification.success(_L("ERROR_MSG_70"), "Qbicles");
    setTimeout(function () {
        window.location.href = "/Apps/Tasks";
    }, 2000);

}

function ViewTransactionMatchingReport(accountName, accountId, accountName2, accountId2, taskid, taskname, transactionMatchingTypeId, transactionmatchingtaskId, taskInstanceId, userName, date, time, countTransactionsMatched, countTransactionsManual, countTransactionsUnMatched) {
    $.LoadingOverlay("show");
    $.ajax({
        type: 'post',
        url: '/Tasks/RedirectTransactionMatchingReport',
        dataType: 'json',
        data: {
            accountName: accountName,
            accountId: accountId,
            accountName2: accountName2,
            accountId2: accountId2,
            taskid: taskid,
            taskname: taskname,
            transactionMatchingTypeId: transactionMatchingTypeId,
            transactionmatchingTaskId: transactionmatchingtaskId,
            taskInstanceId: taskInstanceId,
            userName: userName,
            date: date,
            time: time,
            countTransactionsMatched: countTransactionsMatched,
            countTransactionsManual: countTransactionsManual,
            countTransactionsUnMatched: countTransactionsUnMatched
        },
        success: function (response) {
            if (response === true)
                window.location.href = "/TransactionMatching/TransactionMatchingReport";
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
}
