
var $topicsQbicle = [];
var $selectFilter = $("#filter-group"), $selectOrder = $("#filter-order"),
    $button_add_group = $("#button_add_group"),
    $button_add_account = $("button[name='button_add_account']"),
    $trDelete = null;
//task group
var $modal_group = $("#modal_group"),
    $modal_group_title = $("#modal_group [class='modal-title']"),
    $input_rectaskgroup_id = $("#input_rectaskgroup_id"),
    $input_rectaskgroup_name = $("#input_rectaskgroup_name"),
    $input_rectaskgroup_createddate = $("#input_rectaskgroup_createddate"),
    $input_rectaskgroup_createdbyid = $("#input_rectaskgroup_createdbyid")
    ;

$hdsucessMsg = $("#hdsucessMsg"),
    $button_edit_group = $("button[name='button_edit_group']"),
    $button_remove_group = $("button[name='button_remove_group']"),
    $button_add_reconciliationtask = $("button[name='button_add_reconciliationtask']"),

    $modal_task_remove = $("#modal-delete");
$form_reconciliationtask = $("#form_reconciliationtask"),
    $form_reconciliationtaskgroup = $("#form_reconciliationtaskgroup"),

    $modal_reconciliationtask = $("#modal_reconciliationtask"),
    $modal_group = $("#modal_group"),


    $modal_group_title = $("#modal_group [class='modal-title']"),


    $selectgroup = $("#SelectGroup");
    $select_task_type = $("#typeoftask"),
    $hdReconciliationTaskId = $("#hdReconciliationTaskId"),
    $hdTransactionMatching = $("#hdTransactionMatching"),
    $hdTransactionAnlysis = $("#hdTransactionAnlysis"),
    $hdControlReport = $("#hdControlReport"),
    $hdBalanceAnalysis = $("#hdBalanceAnalysis"),
    $hdTaskTypeIdOld = $("#hdTaskTypeIdOld"),
    $hdCreatedById = $("#hdCreatedById"),
    $hdCreatedDate = $("#hdCreatedDate"),
    $TransactionMatchingTypeId = $("#TransactionMatchingTypeId"),
    $firstaccount = $("#AccountId1"),
    $secondaccount = $("#AccountId2"),
    $TaskExecutionIntervalId = $("#TaskExecutionIntervalId"),
    $taskname = $("#task_Name"),
    $table_instancetask = $("#tblInstanceTask"),
    $AssignedUserId = $("#AssignedUserId"),
    $SelectPriority = $("#SelectPriority"),
    $task_Deadline = $("#task-Deadline"),
    $taskIdDelete = 0,
    $taskNameDelete = 0,
    $taskDesDelete = 0,
    $liDelete = null;
$TaskGroupDelId = null;
var arrtmp = [];

$(document).ready(function () {
    LoadTasks('init');

    $selectFilter.change(function () {
        $("#task-page-display").empty();
        $("#task-page-display").html("<div id='task-content'></div>");

        LoadTasks('');
    });

    $selectOrder.change(function () {
        $("#task-page-display").empty();
        $("#task-page-display").html("<div id='task-content'></div>");

        LoadTasks('');
    });
    //Task form
    $button_add_group.bind('click',
        function (event) {
            if (this.classList.contains('isDisabled')) {
                event.preventDefault();
                return;
            }
            AddGroup();
        });

    $('.btnPreviousTask').click(function () {
        $('.app_subnav > .active').prev('li').find('a').trigger('click');
    });

    // change task of type
    $select_task_type.change(function () {
        $('#InitialTransactionDate').val('');
        $("#lblInitialTransactionDate").hide();
        $("#save-tab-div-1").hide();
        ClearError();
        if ($select_task_type.val() === $hdTransactionMatching.val()) { // if task of task = transaction matching
            $("#save-tab-1").text("Finish and Save");
            $("#save-tab-div-1").show();
            $("#step_2").hide();
            $(".task-account").show();
            $(".task-account .alert").html("Please select the two accounts to perform matching against, then which type of matching you wish to occur in this task");
            $("#taskcount3").show();
            $("#taskcount2").show();
            $("#pnDateVariance").show();
            $("#pnamounVariance").show();
            $("#balance_analysis").hide();

            if ($hdReconciliationTaskId.val() !== "0" && $select_task_type.val() === $select_task_type.attr("data-type")) {

                $TransactionMatchingTypeId[0].selectedIndex = 0;
            } else {
                $firstaccount[0].selectedIndex = -1;
                $secondaccount[0].selectedIndex = -1;
                $firstaccount.trigger("change");
                $secondaccount.trigger("change");
                $TransactionMatchingTypeId[0].selectedIndex = -1;
                $TransactionMatchingTypeId.trigger("change");
            }
            $("#lblInitialTransactionDate").show();

        }
        else if ($select_task_type.val() === $hdTransactionAnlysis.val()) {
            $("#save-tab-1").text("Next");
            $("#step_2").show();
            $("#save-tab-div-1").show();
            $(".task-account").show();
            $("#taskcount3").hide();
            $("#taskcount2").hide();
            $("#pnDateVariance").hide();
            $("#pnamounVariance").hide();
            $("#balance_analysis").hide();
            $(".task-account .alert").html("Please select account to analyse");
            if ($hdReconciliationTaskId.val() !== "0" && $select_task_type.val() === $select_task_type.attr("data-type")) {
                //$firstaccount.val($firstaccount.attr("data-account")).trigger("change");

            } else {
                $firstaccount[0].selectedIndex = -1;
                $firstaccount.trigger("change");
            }
            $secondaccount[0].selectedIndex = -1;
            $secondaccount.trigger("change");
            $TransactionMatchingTypeId[0].selectedIndex = -1;
            $("#lblInitialTransactionDate").show();
        }
        else if ($select_task_type.val() === $hdControlReport.val()) {
            $("#save-tab-1").text("Next");
            $("#step_2").show();
            $("#save-tab-div-1").show();
            $(".task-account").hide();
            $("#pnDateVariance").hide();
            $("#pnamounVariance").hide();
            $("#balance_analysis").hide();
            $firstaccount[0].selectedIndex = -1;
            $firstaccount.trigger("change");
            $secondaccount[0].selectedIndex = -1;
            $secondaccount.trigger("change");
            $TransactionMatchingTypeId[0].selectedIndex = -1;
            $TransactionMatchingTypeId.trigger("change");
        }
        else if ($select_task_type.val() === $hdBalanceAnalysis.val()) {
            $("#save-tab-1").text("Next");
            $("#step_2").show();
            $(".task-account").hide();
            $("#taskcount3").hide();
            $("#taskcount2").hide();
            $("#pnDateVariance").hide();
            $("#pnamounVariance").hide();
            $("#balance_analysis").show();
            $("#save-tab-div-1").show();
        }

    });
    // change first account, second account
    $firstaccount.change(function () {

        if ($firstaccount.val() !== "" && $firstaccount.val() !== null) {
            if ($secondaccount.val() !== "" && $secondaccount.val() !== null && $firstaccount.val() === $secondaccount.val() && $select_task_type.val() !== $hdControlReport.val()) {
                cleanBookNotification.error(_L("ERROR_MSG_474"), "Qbicles");
            }
        }
    });
    $secondaccount.change(function () {

        if ($secondaccount.val() !== "" && $secondaccount.val() !== null) {
            if ($firstaccount.val() !== "" && $firstaccount.val() !== null && $firstaccount.val() === $secondaccount.val() && $select_task_type.val() !== $hdControlReport.val()) {
                cleanBookNotification.error(_L("ERROR_MSG_474"), "Qbicles");
            }
        }
    });
});

function formatDate(jsonDate) {
    if (jsonDate !== null) {
        var value = new Date
            (
                parseInt(jsonDate.replace(/(^.*\()|([+-].*$)/g, ''))
            );

        var mm = ((value.getMonth() + 1) < 10 ? '0' : '') + (value.getMonth() + 1);

        var dd = (value.getDate() < 10 ? '0' : '') + value.getDate();
        return dd + "/" + mm + "/" + value.getFullYear();
    } return '';

}

//add task
function save_task() {
    var isValid = true;
    if ($select_task_type.val() !== $hdTransactionMatching.val()) {
        //$form_reconciliationtask.validate().showErrors({ Name: _L("ERROR_MSG_475") });
        cleanBookNotification.error(_L("ERROR_MSG_475"), "Qbicles");
        isValid = false;
    }
    if ($form_reconciliationtask.valid()) {
        if ($select_task_type.val() === null) {
            cleanBookNotification.error(_L("ERROR_MSG_477"), "Qbicles");
            isValid = false;
        } else if ($select_task_type.val() === $hdTransactionMatching.val()) {
            if ($firstaccount.val() === null || $firstaccount.val() === "") {
                cleanBookNotification.error(_L("ERROR_MSG_478"), "Qbicles");
                isValid = false;
            } else if ($secondaccount.val() === null || $secondaccount.val() === "") {
                cleanBookNotification.error(_L("ERROR_MSG_479"), "Qbicles");
                isValid = false;
            } else if ($firstaccount.val() === $secondaccount.val()) {
                cleanBookNotification.error(_L("ERROR_MSG_480"), "Qbicles");
                isValid = false;
            }
        } else if ($select_task_type.val() === $hdTransactionAnlysis.val()) {
            if ($firstaccount.val() === null || $firstaccount.val() === "") {
                cleanBookNotification.error(_L("ERROR_MSG_478"), "Qbicles");
                isValid = false;
            }
        }
        if ($("#task_workgroup").val() === null) {
            $form_reconciliationtask.validate().showErrors({ task_workgroup: _L("ERROR_MSG_168") });
            //cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
            isValid = false;
        }
        if (!isValid) return;
        if ($AssignedUserId.val() === "") {
            bootbox.confirm({
                show: true,
                backdrop: true,
                closeButton: true,
                animate: true,
                className: "my-modal",
                title: "Qbicles",
                message: "The Assign To is not set when the Task is saved. Are you sure continue?",
                callback: function(result) {
                    if (!result) {
                        $('body').css('overflow-y', 'hidden');
                        return;
                    }
                    $('body').css('overflow-y', 'auto');
                    saveaddtask();
                }
            });
        } else {
            saveaddtask();
        }
    } else {
        if ($("#task_workgroup").val() === null) {
            $form_reconciliationtask.validate().showErrors({ task_workgroup: _L("ERROR_MSG_168") });
            //cleanBookNotification.error(_L("ERROR_MSG_168"), "Qbicles");
            isValid = false;
        }
        if (!isValid) return;
    }
}

function saveaddtask() {
    $.ajax({
        type: 'post',
        url: '/Tasks/TaskNameCheck',
        datatype: 'json',
        data: { name: $taskname.val(), id: $hdReconciliationTaskId.val() },
        success: function (dupplicate) {
            if (!dupplicate.dupplicate) {
                var typeoftaskId = $("#typeoftask :selected").val();
                if (typeoftaskId === $("#hdBalanceAnalysis").val()) {
                    if ($('#tblBalanceAnalysis tbody tr td').length <= 1) {
                        cleanBookNotification.error("You not enter balance mapping ruler", "Qbicles");
                        return;
                    }
                    else {
                        $("#step_1").removeClass('active').addClass('disabled disabledTab');
                        $("#stepv1").removeClass('in active').addClass('disabled disabledTab');
                        $("#step_2").removeClass('disabled disabledTab').addClass('active');
                        $("#stepv2").removeClass('disabled disabledTab').addClass('in active');
                        $("#step2-vtab").css({ 'color': '#337ab7' });
                        $("#step1-vtab").removeAttr('style');
                    }
                }
                else {
                    SaveTask();
                }
            }
            else
                $form_reconciliationtask.validate().showErrors({ Name: "name of reconciliation task already exists" });
        }
    });
}
function SaveTask() {

    var taskName = $taskname.val();
    var typeoftaskId = $("#typeoftask").val();
    var AccountId1 = $firstaccount.val();
    var AccountId2 = $secondaccount.val();
    var TransactionMatchingTypeId = $TransactionMatchingTypeId.val();
    var TaskExecutionIntervalId = $TaskExecutionIntervalId.val();
    var AssignedUserId = $AssignedUserId.val();
    var Description = $("#Task-Description").val();
    var chkDateVariance = $("#chkDateVariance").prop('checked');
    var chkamounVariance = $("#chkamounVariance").prop('checked');


    var TaskId = $("#hdReconciliationTaskId").val();
    var TaskGroupId = $selectgroup.val();
    var TaskTypeIdOldId = $("#hdTaskTypeIdOld").val();
    var DateVariance = $("#DateVariance").val();
    var amounVariance = $("#amounVariance").val();
    var transactionmatchingtaskrulesaccessId = $("#hdtransactionmatchingtaskrulesaccessId").val();
    var dateExcute = $('#InitialTransactionDate').val();

    var arrBalance = [], arrBalanceAction = [];
    if (typeoftaskId === $("#hdTransactionMatching").val()) {
        if (dateExcute === '') {
            cleanBookNotification.error(_L("ERROR_MSG_482"), "Qbicles");
            return;
        }
    }
    else if (typeoftaskId === $("#hdTransactionAnlysis").val()) {
        if (dateExcute === '') {
            cleanBookNotification.error(_L("ERROR_MSG_482"), "Qbicles");
            return;
        }
    }
    if (typeoftaskId === $("#hdBalanceAnalysis").val()) {
        if (arrNumberrow.length <= 0) {
            cleanBookNotification.error(_L("ERROR_MSG_483"), "Qbicles");
            return;
        }


        if ($('#tblBalanceAnalysis tbody tr td').length <= 1) {
            cleanBookNotification.error(_L("ERROR_MSG_484"), "Qbicles");
            return;
        }
        for (var j = 0; j < arrNumberrow.length; ++j) {
            var id = $("#hdValue" + arrNumberrow[j]).val();
            var Id = 0, Name = "";
            var arrtmp = id.split('#');
            if (arrtmp.length > 1) {
                Id = arrtmp[0] === "" ? "0" : arrtmp[0];
                Name = arrtmp[1];
            }
            else
                Name = arrtmp[0];
            var item2 = {
                Id: Id,
                Name: Name
            };
            arrBalanceAction.push(item2);
        }

        for (var r = 0; r < arrEdit.length; r++) {
            var desc1 = $("#desc1" + arrEdit[r]).html();
            var ref1 = $("#ref1" + arrEdit[r]).html();
            var desc2 = $("#desc2" + arrEdit[r]).html();
            var ref2 = $("#ref2" + arrEdit[r]).html();
            var min = $("#min" + arrEdit[r]).html();
            var max = $("#max" + arrEdit[r]).html();
            var item = {
                Id: 0,
                TaskId: TaskId,
                Description1: desc1,
                Reference1: ref1,
                Description2: desc2,
                Reference2: ref2,
                MinDifference: min,
                MaxDifference: max
            };
            arrBalance.push(item);
        }

    }


    var task = {
        Id: TaskId,
        Name: taskName,
        Description: Description,
        ReconciliationTaskGroupId: TaskGroupId,
        TaskExecutionIntervalId: TaskExecutionIntervalId,
        TaskTypeId: typeoftaskId,
        AssignedUserId: AssignedUserId,
        WorkGroup: { Id: $("#task_workgroup").val() },
        TransactionMatchingTypeId: TransactionMatchingTypeId === '' ? '0' : TransactionMatchingTypeId,
        IsActionNotify: $("#IsActionNotify").val(),
        IsActionReport: $("#IsActionReport").val(),
        IsActionNewLedger: $("#IsActionNewLedger").val(),
        InitialTransactionDate: $('#InitialTransactionDate').val(),
        CreatedById: $hdCreatedById.val(),
        CreatedDate: $hdCreatedDate.val()
    };

    var model = {
        rectask: task,
        InitialTransactionDate: dateExcute,
        AccountId1: AccountId1,
        AccountId2: AccountId2,
        TaskTypeIdOld: TaskTypeIdOldId,
        DateVariance: DateVariance,
        amounVariance: amounVariance,
        transactionmatchingtaskrulesaccessId: transactionmatchingtaskrulesaccessId,
        balancemapingruler: arrBalance,
        balanceAction: arrBalanceAction,
        Priority: $SelectPriority.val(),
        Deadline: $task_Deadline.val(),
        WorkGroupId: $("#task_workgroup").val()
    };
    $.ajax({
        type: 'post',
        url: '/Tasks/Save_ManageReconciliationTasks',
        datatype: 'json',
        data: model,
        success: function (data) {
            if (data.status) {
                $("#task-page-display").empty();
                $("#task-page-display").html("<div id='task-content'></div>");
                LoadTasks('');
                $modal_reconciliationtask.modal('toggle');
                if (TaskId > 0)
                    cleanBookNotification.updateSuccess();
                else
                    cleanBookNotification.createSuccess();
            }
        }
    });
}
function GetRules(id) {
    $.ajax({
        url: "/Tasks/get_rulesaccess",
        type: "GET",
        dataType: "json",
        data: { id: id },
        success: function (data) {
            if (data !== '') {
                $("#hdtransactionmatchingtaskrulesaccessId").val(data.Id);

                $("#chkDateVariance").prop('checked', data.IsDateVarianceVisible).change();
                $("#chkamounVariance").prop('checked', data.IsAmountVarianceVisible).change();
                $("#DateVariance").val(data.IsDateVarianceVisible);
                $("#amounVariance").val(data.IsAmountVarianceVisible);
            }
        }
    });
}
function GetBalanceAction(id) {
    $("#lblwell div").remove();
    arrNumberrow = [];
    $.ajax({
        url: "/Tasks/get_editBalanceAction",
        type: "GET",
        dataType: "json",
        data: { id: id },
        success: function (data) {
            var str = '';
            if (data !== '') {

                for (var i = 0; i < data.length; ++i) {
                    str += '    <div class="row">';
                    str += '<div class="col-md-10">' + data[i].Name;
                    str += '<input type="hidden" id="hdValue' + i + '" value="' + data[i].Id + '#' + data[i].Name + '" />';
                    str += ' </div>';
                    str += '<div class="col-md-2"><a href="#" style="margin-left: -15px;" onclick="DeleteItembalanceanalysisaction(this,' + i + ',' + data[i].Id + ')">Delete</a></div>';
                    str += '</div>';

                    arrNumberrow.push(i);
                    numberrow++;
                }
                $("#lblwell").append(str);
            }
        }
    });
}
function GetBalanceMapingRuler(id) {
    $("#tblBalanceAnalysis tbody tr").remove();
    arrEdit = [];
    $.ajax({
        url: "/Tasks/get_editBalanceMapingRuler",
        type: "GET",
        dataType: "json",
        data: { id: id },
        success: function (data) {

            if (data !== '') {
                var str2 = '';

                for (var i = 0; i < data.length; ++i) {
                    arrtmp.push(data[i].Reference1 + '#' + data[i].Reference2);
                    str2 += '    <tr id="balance' + i + '">';
                    str2 += '<td><input type="checkbox" name="trows[]" id="chk' + i + '" onchange="CheckBoxChange(this,' + i + ',\'' + data[i].Reference1 + '#' + data[i].Reference2 + '\',' + data[i].Id + ')"></td>';
                    str2 += '<td><span class="editable" id="desc1' + i + '">' + data[i].Description1 + '</span></td>';
                    str2 += '<td><span class="editable" id="ref1' + i + '">' + data[i].Reference1 + '</span></td>';
                    str2 += '<td><span class="editable" id="desc2' + i + '">' + data[i].Description2 + '</span></td>';
                    str2 += '<td><span class="editable" id="ref2' + i + '">' + data[i].Reference2 + '</span></td>';
                    str2 += '<td><span class="editable" id="min' + i + '">' + data[i].MinDifference + '</span></td>';
                    str2 += '<td><span class="editable" id="max' + i + '">' + data[i].MaxDifference + '</span></td>';
                    str2 += '<td class="table_options"><a href="#" onclick="Removebalanceanalysismappingrule(this,' + i + ',\'' + data[i].Reference1 + '#' + data[i].Reference2 + '\',' + data[i].Id + ')">Delete</a></td>';
                    str2 += ' </tr>';
                }
                $('#lblBalanceTable').html('');

                var str = '<table class="table table-hover t5style" id="tblBalanceAnalysis" cellspacing="0" width="100%">';
                str += '     <thead>';
                str += '              <tr>';
                str += '                <th></th>';
                str += '                <th>Description 1</th>';
                str += '                 <th>Reference 1</th>';
                str += '                 <th>Description 2</th>';
                str += '                 <th>Reference 2</th>';
                str += '                <th>Ref 1 - 2 min</th>';
                str += '                 <th>Ref 1 - 2 max</th>';
                str += '                <th>Options</th>';
                str += '            </tr>';
                str += '        </thead>';
                str += '         <tbody>';
                str += str2;
                str += '   </tbody>';
                str += '     </table>';
                $('#lblBalanceTable').append(str);

                $("#tblBalanceAnalysis").DataTable({
                    "sDom": '<"top">rt<"bottomtable"ip><"clear">',
                    "bInfo": true,
                    "bFilter": false,
                    "bPaginate": true,
                    "bSearchable": false,
                    "sPaginationType": "numbers",
                    "pageLength": 5,
                    "columnDefs": [{
                        "targets": 0,
                        "orderable": false
                    }, {
                        "targets": 7,
                        "orderable": false
                    }]

                });

                $("#description_1_add").val('');
                $("#reference_1_add").val('');
                $("#description_2_add").val('');
                $("#reference_2_add").val('');
                $("#ref_min_add").val('');
                $("#ref_max_add").val('');
            }
        }
    });
}


function Addbalanceanalysismappingrule() {
    var desc1 = $("#description_1_add").val();
    var ref1 = $("#reference_1_add").val();
    var desc2 = $("#description_2_add").val();
    var ref2 = $("#reference_2_add").val();
    var min = $("#ref_min_add").val();
    var max = $("#ref_max_add").val();
    if (desc1 === '') {
        cleanBookNotification.warning("You have not entered a description 1!", "Message");
        return;
    }
    if (ref1 === '') {
        cleanBookNotification.warning("You have not entered a Reference 1!", "Message");
        return;
    }
    if (desc2 === '') {
        cleanBookNotification.warning("You have not entered a description 2!", "Message");
        return;
    }
    if (ref2 === '') {
        cleanBookNotification.warning("You have not entered a Reference 2!", "Message");
        return;
    }
    if (min === '') {
        cleanBookNotification.warning("You have not entered a min value!", "Message");
        return;
    }
    if (max === '') {
        cleanBookNotification.warning("You have not entered a max value!", "Message");
        return;
    }
    var TaskId = $("#hdReconciliationTaskId").val();
    if (TaskId !== '' && TaskId !== '0') {
        $.ajax({
            type: 'post',
            url: '/BalanceAnalysis/CheckName',
            dataType: 'json',
            data: { name1: ref1, Name2: ref2, TaskId: TaskId },
            success: function (dupplicate) {
                var result = false;
                if (dupplicate.ref1 === 1) {
                    cleanBookNotification.warning("Reference 1 already exists", "Message");
                    result = true;
                }
                if (dupplicate.ref2 === 1) {
                    cleanBookNotification.warning("Reference 2 already exists", "Message");
                    result = true;
                }
                if (!result) {
                    for (var i = 0; i < arrtmp.length; ++i) {
                        var arr = arrtmp[i].split('#');
                        if (arr[0] === ref1) {
                            cleanBookNotification.warning("Reference 1 already exists", "Message");
                            result = true;
                        }
                        if (arr[1] === ref2) {
                            cleanBookNotification.warning("Reference 2 already exists", "Message");
                            result = true;
                        }
                    }
                }
                if (result)
                    return;
                $("#tblBalanceAnalysis tbody tr.odd").remove();
                arrtmp.push(ref1 + '#' + ref2);
                var str = '';
                indexbalanceanalysis = arrEdit.length;
                str = '    <tr id="balance' + indexbalanceanalysis + '">';
                str += '<td><input type="checkbox" name="trows[]" id="chk' + indexbalanceanalysis + '" onchange="CheckBoxChange(this,' + indexbalanceanalysis + ',\'' + ref1 + '#' + ref2 + '\',0)"></td>';
                str += '<td><span class="editable" id="desc1' + indexbalanceanalysis + '">' + desc1 + '</span></td>';
                str += '<td><span class="editable" id="ref1' + indexbalanceanalysis + '">' + ref1 + '</span></td>';
                str += '<td><span class="editable" id="desc2' + indexbalanceanalysis + '">' + desc2 + '</span></td>';
                str += '<td><span class="editable" id="ref2' + indexbalanceanalysis + '">' + ref2 + '</span></td>';
                str += '<td><span class="editable" id="min' + indexbalanceanalysis + '">' + min + '</span></td>';
                str += '<td><span class="editable" id="max' + indexbalanceanalysis + '">' + max + '</span></td>';
                str += '<td class="table_options"><a href="#" onclick="Removebalanceanalysismappingrule(this,' + indexbalanceanalysis + ',\'' + ref1 + '#' + ref2 + '\',0)">Delete</a></td>';
                str += ' </tr>';
                $("#tblBalanceAnalysis tbody").append(str);
                arrEdit.push(indexbalanceanalysis);
                $("#description_1_add").val('');
                $("#reference_1_add").val('');
                $("#description_2_add").val('');
                $("#reference_2_add").val('');
                $("#ref_min_add").val('');
                $("#ref_max_add").val('');
            }
        });
    }
    else {
        var result = false;
        for (var i = 0; i < arrtmp.length; ++i) {
            var arr = arrtmp[i].split('#');
            if (arr[0] === ref1) {
                cleanBookNotification.warning("Reference 1 already exists", "Message");
                result = true;
            }
            if (arr[1] === ref2) {
                cleanBookNotification.warning("Reference 2 already exists", "Message");
                result = true;
            }
        }
        if (result)
            return;
        $("#tblBalanceAnalysis tbody tr.odd").remove();
        arrtmp.push(ref1 + '#' + ref2);
        var str = '';
        var tmp = $("#tblBalanceAnalysis tbody tr:last").attr('id');
        tmp = tmp.replace('balance', '');
        indexbalanceanalysis = parseInt(tmp);

        str = '    <tr id="balance' + indexbalanceanalysis + '">';
        str += '<td><input type="checkbox" name="trows[]" id="chk' + indexbalanceanalysis + '" onchange="CheckBoxChange(this,' + indexbalanceanalysis + ',\'' + ref1 + '#' + ref2 + '\',0)"></td>';
        str += '<td><span class="editable" id="desc1' + indexbalanceanalysis + '">' + desc1 + '</span></td>';
        str += '<td><span class="editable" id="ref1' + indexbalanceanalysis + '">' + ref1 + '</span></td>';
        str += '<td><span class="editable" id="desc2' + indexbalanceanalysis + '">' + desc2 + '</span></td>';
        str += '<td><span class="editable" id="ref2' + indexbalanceanalysis + '">' + ref2 + '</span></td>';
        str += '<td><span class="editable" id="min' + indexbalanceanalysis + '">' + min + '</span></td>';
        str += '<td><span class="editable" id="max' + indexbalanceanalysis + '">' + max + '</span></td>';
        str += '<td class="table_options"><a href="#" onclick="Removebalanceanalysismappingrule(this,' + indexbalanceanalysis + ',\'' + ref1 + '#' + ref2 + '\',0)">Delete</a></td>';
        str += ' </tr>';
        $("#tblBalanceAnalysis tbody").append(str);
        arrEdit.push(indexbalanceanalysis);
        $("#description_1_add").val('');
        $("#reference_1_add").val('');
        $("#description_2_add").val('');
        $("#reference_2_add").val('');
        $("#ref_min_add").val('');
        $("#ref_max_add").val('');
    }
}
function Removebalanceanalysismappingrule(obj, index, condition, id) {
    $(obj).parent().parent().remove();
    var index1 = arrtmp.indexOf(condition);
    if (index1 >= 0)
        arrtmp.splice(index1, 1);
    var index2 = arrEdit.indexOf(index);
    if (index2 >= 0)
        arrEdit.splice(index2, 1);

    var arrId = [], arrIndex = [];
    arrId.push(id);
    arrIndex.push(index);
    $.ajax({
        type: 'post',
        url: '/BalanceAnalysis/DeleteBalanceMapingRuler',
        dataType: 'json',
        data: { lstId: arrId, lstindex: arrIndex },
        success: function (data) {

        }
    });
}
var arrtmp2 = [], arrEdit = [], Idarr = [];
var multiplebalanceanalysismappingrule = [], indexbalanceanalysis = 0;
function RemoveMultiplebalanceanalysismappingrule() {
    var arrId = [], arrIndex = [];
    for (var i = 0; i < multiplebalanceanalysismappingrule.length; ++i) {
        $("#balance" + multiplebalanceanalysismappingrule[i]).remove();
        var index = arrtmp.indexOf(arrtmp2[i]);
        if (index >= 0)
            arrtmp.splice(index, 1);
        var index2 = arrEdit.indexOf(multiplebalanceanalysismappingrule[i]);
        if (index2 >= 0) {
            arrEdit.splice(index2, 1);
            arrIndex.push(index2);
        }

        if (Idarr[i] > 0) {
            arrId.push(Idarr[i]);
        }
    }
    $.ajax({
        type: 'post',
        url: '/BalanceAnalysis/DeleteBalanceMapingRuler',
        dataType: 'json',
        data: { lstId: arrId, lstindex: arrIndex },
        success: function (data) {

        }
    });
}
function CheckBoxChange(obj, index, condition, id) {
    if ($(obj).is(':checked')) {
        multiplebalanceanalysismappingrule.push(index);
        arrtmp2.push(condition);
        Idarr.push(id);
    }
    else {
        var index2 = multiplebalanceanalysismappingrule.indexOf(id);
        if (index2 >= 0)
            multiplebalanceanalysismappingrule.splice(index2, 1);
        var index3 = arrtmp2.indexOf(condition);
        if (index3 >= 0)
            arrtmp2.splice(index3, 1);
    }
    if (multiplebalanceanalysismappingrule.length > 0) {
        $("#btnRemoveMultipleBalanceAnalysisMapRuler").removeAttr("disabled");
    }
    else {
        $("#btnRemoveMultipleBalanceAnalysisMapRuler").attr("disabled", "disabled");
    }
}
var numberrow = 0, arrNumberrow = [], taskId = 0;
function Add2balanceanalysisaction() {
    var ddlbalanceanalysispredefaction = $("#ddlbalanceanalysispredefaction :selected").val();
    var ddlbalanceanalysispredefactionName = $("#ddlbalanceanalysispredefaction :selected").html();
    var tmp = ddlbalanceanalysispredefaction + '#' + ddlbalanceanalysispredefactionName;
    if (ddlbalanceanalysispredefaction !== '' && ddlbalanceanalysispredefaction !== undefined) {
        var check = true;
        for (var i = 0; i < arrNumberrow.length; ++i) {
            var id = $("#hdValue" + arrNumberrow[i]).val();
            if (id === tmp) {
                check = false;
                break;
            }
        }
        if (check) {
            var str = '    <div class="row">';
            str += '<div class="col-md-10">' + ddlbalanceanalysispredefactionName;
            str += '<input type="hidden" id="hdValue' + numberrow + '" value="' + tmp + '" />';
            str += ' </div>';
            str += '<div class="col-md-2"><a href="#" style="margin-left: -15px;" onclick="DeleteItembalanceanalysisaction(this,' + numberrow + ',0)">Delete</a></div>';
            str += '</div>';
            $("#lblwell").append(str);
            arrNumberrow.push(numberrow);
            numberrow++;
        }
        else {
            $("#lblWarningError").css({ "display": "block" });
            $("#lblErrorMessage").html(ddlbalanceanalysispredefactionName + " already exists");
        }
    }
    else {
        $("#lblWarningError").css({ "display": "block" });
        $("#lblErrorMessage").html("Please select or enter at least one Option");
    }
}

function Add2balanceanalysisaction2() {
    var ddlbalanceanalysispredefactionName = $("#add_new_option").val();
    var tmp = '#' + ddlbalanceanalysispredefactionName;
    if (ddlbalanceanalysispredefactionName !== '') {
        var check = true;
        for (var i = 0; i < arrNumberrow.length; ++i) {
            var id = $("#hdValue" + arrNumberrow[i]).val();
            if (id.indexOf(tmp) > 0) {
                check = false;
                break;
            }
        }
        if (check) {
            var str = '    <div class="row">';
            str += '<div class="col-md-10">' + ddlbalanceanalysispredefactionName;
            str += '<input type="hidden" id="hdValue' + numberrow + '" value="' + tmp + '" />';
            str += ' </div>';
            str += '<div class="col-md-2"><a href="#" style="margin-left: -15px;" onclick="DeleteItembalanceanalysisaction(this,' + numberrow + ',0)">Delete</a></div>';
            str += '</div>';
            $("#lblwell").append(str);
            arrNumberrow.push(numberrow);
            numberrow++;
        }
        else {
            $("#lblWarningError").css({ "display": "block" });
            $("#lblErrorMessage").html(ddlbalanceanalysispredefactionName + " already exists");
        }
    }
    else {
        $("#lblWarningError").css({ "display": "block" });
        $("#lblErrorMessage").html("Please enter an option");
    }
}
function DeleteItembalanceanalysisaction(obj, num, id) {
    $(obj).parent().parent().remove();
    var index = arrNumberrow.indexOf(num);
    if (index >= 0)
        arrNumberrow.splice(index, 1);
    if (id > 0) {

        $.ajax({
            type: 'post',
            url: '/BalanceAnalysis/DeleteBalanceAction',
            dataType: 'json',
            data: { Id: id },
            success: function (data) {

            }
        });
    }
}
function Back() {
    $("#step_2").removeClass('active').addClass('disabled disabledTab');
    $("#stepv2").removeClass('in active').addClass('disabled disabledTab');
    $("#step_1").removeClass('disabled disabledTab').addClass('active');
    $("#stepv1").removeClass('disabled disabledTab').addClass('in active');
    $("#step1-vtab").css({ 'color': '#337ab7' });
    $("#step2-vtab").removeAttr('style');
}
function uploadfile() {
    var file = document.forms['form_reconciliationtask']['bulk_add'].files[0];
    if (file !== undefined) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            data.append("file", file);
            var TaskId = $("#hdReconciliationTaskId").val();
            if (TaskId === '')
                TaskId = '0';
            var index = $('#tblBalanceAnalysis tr:last').attr('id');

            if (index === undefined)
                index = 0;
            else
                index = parseInt(index.replace('balance', '')) + 1;
            $.ajax({
                type: "POST",
                url: '/BalanceAnalysis/ReadSheets?Id=' + TaskId + '&index=' + index,
                contentType: false,
                processData: false,
                data: data,
                success: function (data) {

                    if (data.status === 2) {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                        //delete file
                        $('#txtUploadFile').val('');
                    }
                    else if (data.status === 1) {
                        if (data.dataerror !== '') {
                            $("#linkerrorfile").attr('href', '..' + data.dataerror);
                            $("#modal_error").modal('show');
                            $('#txtUploadFile').val('');
                        }
                        else {
                            $('#lblBalanceTable').html('');

                            var str = '<table class="table table-hover t5style" id="tblBalanceAnalysis" cellspacing="0" width="100%">';
                            str += '     <thead>';
                            str += '              <tr>';
                            str += '                <th></th>';
                            str += '                <th>Description 1</th>';
                            str += '                 <th>Reference 1</th>';
                            str += '                 <th>Description 2</th>';
                            str += '                 <th>Reference 2</th>';
                            str += '                <th>Ref 1 - 2 min</th>';
                            str += '                 <th>Ref 1 - 2 max</th>';
                            str += '                <th>Options</th>';
                            str += '            </tr>';
                            str += '        </thead>';
                            str += '         <tbody>';
                            str += data.datasuccess;
                            str += '   </tbody>';
                            str += '     </table>';
                            $('#lblBalanceTable').append(str);

                            $("#tblBalanceAnalysis").DataTable({
                                "sDom": '<"top">rt<"bottomtable"ip><"clear">',
                                "bInfo": true,
                                "bFilter": false,
                                "bPaginate": true,
                                "bSearchable": false,
                                "sPaginationType": "numbers",
                                "pageLength": 5,
                                "columnDefs": [{
                                    "targets": 0,
                                    "orderable": false
                                }, {
                                    "targets": 7,
                                    "orderable": false
                                }]

                            });
                            $('#txtUploadFile').val('');
                        }
                    }
                },
                error: function (xhr, status, p3, p4) {
                    cleanBookNotification.error("Have an error when Scaning virus. Please contact the system administrator for help.", "Qbicles");
                    $('#txtUploadFile').val('');
                    return false;
                }
            });
        } else {
            alert("This browser doesn't support HTML5 file uploads!");
        }
    }
}
//  Task Group
$("#form_group").submit(function (e) {

    e.preventDefault();
    $.ajax({
        type: this.method,
        cache: false,
        url: this.action,
        enctype: 'multipart/form-data',
        data: new FormData(this),
        processData: false,
        contentType: false,
        success: function (refModel) {
            if (refModel.result) {
                $modal_group.modal('toggle');

                if (refModel.actionVal === 1) {
                    $("#list_view").prepend(refModel.Object.toString());
                    $("#grid_view").prepend(refModel.msg);
                    $selectFilter.append("<option value='" + refModel.msgId + "'>" + refModel.msgName + "</option>");
                    $selectgroup.prepend("<option value='" + refModel.msgId + "'>" + refModel.msgName + "</option>");
                    cleanBookNotification.success(_L("ERROR_MSG_485", [$input_rectaskgroup_name.val()]), "Qbicles");
                }
                else if (refModel.actionVal === 2) {
                    $("#task-group-name-grid-" + $input_rectaskgroup_id.val()).text(refModel.msg);
                    $("#task-group-name-list-" + $input_rectaskgroup_id.val()).text(refModel.msg);

                    $('#SelectGroup [value="' + refModel.msgId + '"]').text(refModel.msgName);
                    $('#filter-group [value="' + refModel.msgId + '"]').text(refModel.msgName);

                    cleanBookNotification.success(_L("ERROR_MSG_486", [$input_rectaskgroup_name.val()]), "Qbicles");
                }
            }
            else
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
        error: function (data) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }

    });
});
function save_recGroup() {
    Save_Group();
}
function Save_Group() {
    if ($("#form_group").valid()) {
        var model = {
            Id: $input_rectaskgroup_id.val(),
            Name: $input_rectaskgroup_name.val()
        };
        $.ajax({
            type: 'post',
            url: '/Tasks/dupplicateRecTaskGroupCheck',
            dataType: 'json',
            data: model,
        }).done(function (refModel) {
            if (refModel.dupplicate)
                $("#form_group").validate().showErrors({ Name: "Task Group name of '" + $input_rectaskgroup_name.val() + "' already exists." });
            else {
                $("#form_group").trigger("submit");
            }
        }).fail(function (xhr, err) {
            $("#form_group").validate().showErrors({ Name: "Error checking existing name of group in the current Domain!" });
        })
    }
}
function EditGroup(groupId, controlId) {
    if ($("#" + controlId).hasClass('isDisabled')) {
        return;
    }
    ClearError();
    if (groupId && groupId > 0) {
        $.ajax({
            type: 'GET',
            url: "/Tasks/get_reconciliationtaskgroups",
            datatype: 'json',
            data: { id: groupId },
            success: function (refModel) {
                if (refModel.Id) {

                    $("#form_group").validate().resetForm();
                    $input_rectaskgroup_id.val(refModel.Id);
                    $input_rectaskgroup_name.val(refModel.Name);
                    $input_rectaskgroup_createddate.val(refModel.CreatedDate
                        ? new Date(parseInt(refModel.CreatedDate.substr(6))).toJSON()
                        : null);
                    $input_rectaskgroup_createdbyid.val(refModel.CreatedById);

                    $modal_group_title.text("Edit Group");
                    $("#save-group").text("Confirm");

                    $modal_group.modal('toggle');
                }
            },
            error: function (data) {
                cleanBookNotification.error("Have an error, detail: " + data.error, "Qbicles");
            }
        });

    }
}
function AddGroup() {
    ClearError();
    $input_rectaskgroup_id.val(0);
    $input_rectaskgroup_name.val("");
    $input_rectaskgroup_createddate.val("");
    $input_rectaskgroup_createdbyid.val("");

    $modal_group_title.text("Add a Group");
    $("#save-group").text("Add now");
    $modal_group.modal('toggle');
}
// End task group

//  Task
//remove attr
function RemoveAttr() {
    $taskname.removeAttr("disabled");
    $select_task_type.removeAttr("disabled");
    $TaskExecutionIntervalId.removeAttr("disabled");
    $TransactionMatchingTypeId.removeAttr("disabled");
    $firstaccount.removeAttr("disabled");
    $secondaccount.removeAttr("disabled");
}
//clear form
function clearForm() {
    $TaskExecutionIntervalId[0].selectedIndex = 0;
    $taskname.val("");

    $firstaccount[0].selectedIndex = -1;
    $firstaccount.trigger("change");
    $secondaccount[0].selectedIndex = -1;
    $secondaccount.trigger("change");
    $(".task-account").hide();
    $TransactionMatchingTypeId[0].selectedIndex = -1;
    $selectgroup[0].selectedIndex = 0;
    $("textarea[name='Description']").val("");
    $("#description_1_add").val('');
    $("#reference_1_add").val('');
    $("#description_2_add").val('');
    $("#reference_2_add").val('');
    $("#ref_min_add").val('');
    $("#ref_max_add").val('');
    $("#tblBalanceAnalysis tbody tr").remove();
    $("#lblwell div").remove();
    arrEdit = [];
    $("#lblWarningError").css({ "display": "none" });
    $("#lblErrorMessage").html("");
    $("#balance_analysis").css({ "display": "none" });
    $('#InitialTransactionDate').val('');
    $SelectPriority[0].selectedIndex = 0;
    $task_Deadline.val('');
    $select_task_type[0].selectedIndex = 0;
    $select_task_type.trigger("change");
    $(".tab-content").removeClass("hidden");
    $(".tab-content").removeAttr("style");
}
//get user
function GetUser(userId) {
    $.ajax({
        url: "/Tasks/GetUserAsign",
        type: "GET",
        dataType: "json",
        success: function (response) {
            var str = "";
            $("#AssignedUserId option").remove();
            for (var i = 0; i < response.length; i++) {
                str += "<option value='" + response[i].Id + "'>" + response[i].UserName + "</option>";
            }
            $AssignedUserId.html(str);
            if (userId !== "") {
                $AssignedUserId.val(userId);
            }
            $AssignedUserId.trigger("change");
        }
    });
}

//function getTopic(qbicleId) {
//    $.ajax({
//        type: 'post',
//        url: '/Topics/GetTopicByQbicle',
//        datatype: 'json',
//        data: {
//            qbicleId: qbicleId
//        },
//        success: function (refModel) {
//            $topicsByQbicle = refModel.Object;
//        }
//    });
//}

function AddTask(groupId, controlId) {
    if ($("#" + controlId).hasClass('isDisabled')) {
        return;
    }
    clearForm();
    RemoveAttr();
    GetUser();
    $("#task_workgroup").select2().val(null).change();
    $("#task-modal-title").text("Add Task");
    $modal_reconciliationtask.modal('toggle');
    $selectgroup.val(groupId);
    $hdReconciliationTaskId.val("0");

    $("#hdtransactionmatchingtaskrulesaccessId").val(0);
    $("#chkDateVariance").prop('checked', false).change();
    $("#chkamounVariance").prop('checked', false).change();
    $("#DateVariance").val(0);
    $("#amounVariance").val(0);
}


function EditTask(id, name, ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }
    $hdReconciliationTaskId.val(id);
    clearForm();
    if (id > 0) {
        GetTaskEdit(id, name);
    }
}
// edit task
function GetTaskEdit(id, name) {
    $.ajax({
        url: "/Tasks/checkEdit",
        type: "GET",
        dataType: "json",
        data: { id: id },
        success: function (response) {
            if (response.Status === true) {
                var is_InPresson = response.isInPresson;
                if (is_InPresson) {
                    $("#task-name-info").html(name);
                    $("#modal-warning").modal('toggle');
                }
                else {
                    $.ajax({
                        url: "/Tasks/GetTaskEdit",
                        type: "GET",
                        dataType: "json",
                        data: { id: id },
                        success: function (response) {
                            $("#task_workgroup").select2().val(response.WorkGroupId).change();

                            setTimeout(function () {
                                GetRules(id);
                            }, 100);

                            setTimeout(function () {
                                GetBalanceMapingRuler(id);
                            }, 100);
                            setTimeout(function () {
                                GetBalanceAction(id);
                            }, 100);

                            setTimeout(function () {
                                if (response) {

                                    $SelectPriority.val(response.Priority);
                                    if (response.Deadline === "01-01-0001 00:01")
                                        $task_Deadline.val('');
                                    else
                                        $task_Deadline.val(response.Deadline);

                                    var is_InPresson = response.isInPresson;
                                    if (is_InPresson) {
                                        $("#task-name-info").html(id);
                                        $("#modal-warning").modal('toggle');
                                        return false;
                                    }
                                    var taskinstance = response.taskinstance;
                                    var task = response.task;
                                    var taskaccount = response.taskaccount;
                                    

                                    if (task !== null) {
                                        $('#InitialTransactionDate').val('');
                                        $("#lblInitialTransactionDate").hide();
                                        $selectgroup.val(task.ReconciliationTaskGroupId);
                                        $taskname.val(task.Name);

                                        $("textarea[name='Description']").val(task.Description);
                                        $select_task_type.val(task.TaskTypeId).trigger("change");
                                        $hdTaskTypeIdOld.val(task.TaskTypeId);
                                        $TaskExecutionIntervalId.val(task.TaskExecutionIntervalId);

                                        setTimeout(function () {
                                            GetUser(task.AssignedUserId);
                                        }, 200);


                                        $hdCreatedById.val(task.CreatedById);
                                        $hdCreatedDate.val(task.CreatedDate);
                                        if (task.TaskTypeId == $hdTransactionMatching.val()) {
                                            $("#save-tab-1").text("Finish and Save");
                                            $("#save-tab-div-1").show();
                                            $("#step_2").hide();

                                            $("#balance_analysis").hide();
                                            $(".task-account").show();
                                            $TransactionMatchingTypeId.val(task.TransactionMatchingTypeId);
                                            $TransactionMatchingTypeId.trigger("change");
                                            $("#lblInitialTransactionDate").show();

                                            $('#InitialTransactionDate').val(formatDate(task.InitialTransactionDate));

                                        } else if (task.TaskTypeId == $hdTransactionAnlysis.val()) {
                                            $("#save-tab-1").text("Next");
                                            $("#step_2").show();
                                            $("#save-tab-div-1").show();
                                            $(".task-account").show();
                                            $("#taskcount3").hide();
                                            $("#taskcount2").hide();
                                            $("#balance_analysis").hide();
                                            $("#lblInitialTransactionDate").show();
                                            $('#InitialTransactionDate').val(formatDate(task.InitialTransactionDate));
                                        }
                                        else if ($select_task_type.val() == $hdBalanceAnalysis.val()) {
                                            $("#save-tab-1").text("Next");
                                            $("#step_2").show();
                                            $(".task-account").hide();
                                            $("#taskcount3").hide();
                                            $("#taskcount2").hide();
                                            $("#pnDateVariance").hide();
                                            $("#pnamounVariance").hide();
                                            $("#balance_analysis").show();

                                        }
                                        if (taskaccount.length > 0) {
                                            for (var i = 0; i < taskaccount.length; i++) {
                                                var account = taskaccount[i];

                                                if (account.Order === 1) {
                                                    $firstaccount.val(account.AccountId);
                                                    $firstaccount.trigger("change");
                                                } else {
                                                    $secondaccount.val(account.AccountId);
                                                    $secondaccount.trigger("change");
                                                }
                                            }
                                        }

                                        if (taskinstance === true) {
                                            $taskname.attr({ "disabled": "disabled" });
                                            $select_task_type.attr({ "disabled": "disabled" });
                                            $TaskExecutionIntervalId.attr({ "disabled": "disabled" });
                                            $TransactionMatchingTypeId.attr({ "disabled": "disabled" });
                                            $firstaccount.attr({ "disabled": "disabled" });
                                            $secondaccount.attr({ "disabled": "disabled" });
                                        } else {
                                            RemoveAttr();
                                        }
                                    }
                                    $(".tab-content").removeClass("hidden");
                                    $(".tab-content").removeAttr("style");
                                    $("#form_reconciliationtask.tab-content").show();
                                    $("#task-modal-title").text("Edit Task");
                                    $modal_reconciliationtask.modal('toggle');
                                }

                            }, 500);
                        }
                    });
                }
            }
        }
    });
}

function GetDomainRoleAccounts(taskId) {
    return $.ajax({
        url: "/Tasks/GetDomainRoleAccounts",
        type: "GET",
        dataType: "json",
        data: { taskId: taskId }
    });
}
function TaskDelete(id, name, description, ev) {
    if (ev.className.indexOf('isDisabled') === 0) {
        return;
    }
    $.ajax({
        type: 'post',
        url: '/Tasks/ValidDeleteTask',
        dataType: 'json',
        data: { taskId: id },
        success: function (ref) {
            if (ref.status === true) {
                $("#permission-del").show();
                $("#confirm-del").hide();
                $('#task-name-permission').text(name);
            } else {

                $trDelete = $("#task-tr-" + id);
                $liDelete = $("#task-" + id);

                $taskIdDelete = id;
                $taskNameDelete = name;
                $taskDesDelete = description;

                $("#permission-del").hide();
                $("#confirm-del").show();
                $('#task-name-confirm').text($taskNameDelete);
            }
            $modal_task_remove.modal('toggle');
        }
    })

}
function deleteTask() {
    if ($taskIdDelete <= 0)
        return;
    var model = {
        Id: $taskIdDelete,
        Name: $taskNameDelete,
        Description: $taskDesDelete,
    };
    $.ajax({
        type: 'post',
        url: '/Tasks/DeleteTask',
        dataType: 'json',
        data: model,
        success: function (res) {
            if (res.status) {
                $modal_task_remove.modal('hide');
                cleanBookNotification.removeSuccess();
                //change the background color to red before removing
                $($trDelete).css("background-color", "#FF3700");
                $($trDelete).fadeOut(1500, function () {
                    $($trDelete).remove();
                });
                $($liDelete).css("background-color", "#FF3700");
                $($liDelete).fadeOut(1500, function () {
                    $($liDelete).remove();
                });
                $taskIdDelete = 0;
            }
            else
                cleanBookNotification.removeFail();
        }
    })
}


function checkAmount(obj, type) {
    var chk = $(obj).is(':checked');
    if (type === 0) {
        $("#DateVariance").val(chk ? 1 : 0);
    }
    else {
        $("#amounVariance").val(chk ? 1 : 0);
    }
}
function ViewInstanceMatchingTask(taskid, taskname, ev) {
    if (ev.className.indexOf('a-btn-disabled') === 0) {
        return;
    }
    $(".modal-taskname").text(taskname);
    $table_instancetask.dataTable().fnDestroy();
    $table_instancetask.find("tbody").find("tr").remove();
    $.ajax({
        type: 'post',
        url: '/TransactionMatching/GetTaskInstanceMatching',
        dataType: 'json',
        data: {
            taskid: taskid, runTask: $("#right-run-task").val()
        },
        success: function (response) {
            $table_instancetask.dataTable().fnDestroy();
            $table_instancetask.find("tbody").find("tr").remove();

            $table_instancetask.find("tbody").append(response.msg);
            var table = $table_instancetask.DataTable({
                responsive: true,
                "lengthChange": true,
                "pageLength": 10,
                "columnDefs": [{
                    "targets": 3,
                    "orderable": false
                }]
            });
            $("#modal-task-instances").modal('toggle');
        }
    });
}

//  Common
function LoadTasks(init) {
    cleanBookNotification.clearmessage();
    $.ajax({
        url: "/Tasks/LoadTasks/",
        data: {
            groupId: $selectFilter.val(),
            orderBy: $selectOrder.val()
        },
        cache: false,
        type: "POST",
        success: function (data) {
            if (data.length !== 0) {
                $(data.ModelString).insertAfter("#task-content").hide().fadeIn(1000);
                if (init === 'init')
                    DisplayView('grid');
                else if (typeof (Storage) !== "undefined") {
                    var showType = sessionStorage.getItem('showType');
                    DisplayView(showType);
                } else {
                    DisplayView('grid');
                }
            }
        },
        error: function (xhr, status, error) {
            cleanBookNotification.error(xhr.responseText, "Qbicles");
        }
    });
}
function DisplayView(showType) {
    SetShowAccountType(showType);


    //$('.toggle_view').removeClass('active');
    $('.task-view').hide();
    if (showType === 'grid') {
        $("#grid-view").addClass('active');
        $("#grid_view").show();
    }
    else {
        $("#list-view").addClass('active');
        $("#list_view").show();
    }

}

function SetShowAccountType(showType) {
    if (typeof (Storage) !== "undefined") {
        sessionStorage.removeItem('showType')
        sessionStorage.setItem('showType', showType);
    } else {
        cleanBookNotification.warning('Your browser is too old. Please upgrade your browser!', "Qbicles");
    }
}

function swichPage() {
    var selectPage = document.getElementById("menu-select").value;
    if (selectPage === "Task")
        window.location = "/Apps/Tasks";
    else if (selectPage === "Account")
        window.location = "/Apps/Accounts";
    else if (selectPage === "Config")
        window.location = "/Apps/CleanBookConfig";
}
function ClearError() {
    $("label.error").hide();
    $(".error").removeClass("error");
    $(".valid").removeClass("valid");
    $("label.valid").hide();
    $(".valid").removeClass("valid");
}
