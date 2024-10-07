
var td = null;
var trSelect = null;
var lstNumberRandom = [];
var keyword_taxrate = '';
var charDot = '.';
function Random() {
    var random = Math.floor(Math.random() * 1000) * (-1);
    while (lstNumberRandom.indexOf(random) !== -1) {
        random = Math.floor(Math.random() * 1000) * (-1);
    }
    lstNumberRandom.push(random);
    return random;
}
function ClearRandom() {
    lstNumberRandom = [];
}
if (jQuery().jstree) {

    $('.jstree').jstree({
        "core": {
            "themes": {
                "dots": true,
                "stripes": false,
                "variant": "large"
            }
        },
        "search": {
            "case_insensitive": true,
            "show_only_matches": true
        },
        "plugins": ["themes", "search", "html_data", "ui", "wholerow"]
    });

    //$('.jstree').jstree('open_all');
    $('.jstree').on('select_node.jstree', function (e, data) {
        data.instance.toggle_node(data.node);
    }).on("select_node.jstree", function (e, data) {
        if (data.instance.get_node(data.node, true).children('a').attr('href') !== '#') {
            document.location = data.instance.get_node(data.node, true).children('a').attr('href');
        }
    });

    var showtree = function () {
        $('.treeview').fadeIn(800);
    };
    setTimeout(showtree, 200);

    $(".search-tree").keyup(function () {
        var searchString = $(this).val();
        $('.jstree').jstree('search', searchString);
    });
}
$('.jstree').on("select_node.jstree", function (evt, data) {
    var type = data.node.data.node;
    var id = data.node.data.value;
    selectedNote = data.node.data;

    if (type === 'BKAccount' && td !== null) {
        $($(td).find("input")[0]).val(id);
        $($(td).find("p")[0]).removeClass('hidden');
        $($(td).find("p")[0]).text(selectedNote.name);
        $($(td).find("button.btn-edit")[0]).removeClass('hidden');
        $($(td).find("button.btn-add")[0]).addClass('hidden');
    }
});

function debitChange(ev) {
    trSelect = ev.parentElement.parentElement;
    var thisDebit = ev;
    var debitVal = $(ev).val();
    var index = $(trSelect).find("td.bkTransactionRowId input.row-index").val();
    var childrens = $("#new-journal-entry span.parenttransaction_" + index);
    if (childrens.length > 0) {
        $.ajax({
            type: 'get',
            url: '/BKJournalEntries/GetTaxRate?id=',
            dataType: 'json',
            success: function (response) {
                if (response !== null)
                    for (var i = 0; i < childrens.length; i++) {
                        var trTransaction = childrens[i].parentElement.parentElement;
                        if ($(trTransaction).find("td.td_debit input").val() > 0 || ($(trTransaction).find("td.td_debit input").val() === 0 && $(trTransaction).find("td.td_credit input").val() === 0 && i === 0)) {
                            var debitValue = debitVal * (response.Rate / 100);
                            $(trTransaction).find("td.td_debit input").val(debitValue.toFixed($decimalPlace));
                            $(trTransaction).find("td.td_credit input").val(0);
                        }
                        if ($(trTransaction).find("td.td_credit input").val() > 0 || ($(trTransaction).find("td.td_debit input").val() === 0 && $(trTransaction).find("td.td_credit input").val() === 0 && i === 1)) {
                            var creditValue = debitVal * (response.Rate / 100);
                            $(trTransaction).find("td.td_credit input").val(creditValue.toFixed($decimalPlace));
                            $(trTransaction).find("td.td_debit input").val(0);
                        }
                    }
                sumDebit(thisDebit);
                sumCredit(null);
            }
        });
    } else {
        sumDebit(thisDebit);
        sumCredit(null);
    }

}
function creditChange(ev) {
    trSelect = ev.parentElement.parentElement;
    var creditVal = $(ev).val();
    var thisCredit = ev;
    var index = $(trSelect).find("td.bkTransactionRowId input.row-index").val();
    var childrens = $("#new-journal-entry span.parenttransaction_" + index);
    if (childrens.length > 0) {
        $.ajax({
            type: 'get',
            url: '/BKJournalEntries/GetTaxRate?id=',
            dataType: 'json',
            success: function (response) {
                if (response !== null)
                    for (var i = 0; i < childrens.length; i++) {
                        var trTransaction = childrens[i].parentElement.parentElement;
                        if ($(trTransaction).find("td.td_debit input").val() > 0 || ($(trTransaction).find("td.td_debit input").val() === 0 && $(trTransaction).find("td.td_credit input").val() === 0 && i === 0)) {
                            var debitValue = creditVal * (response.Rate / 100);
                            $(trTransaction).find("td.td_debit input").val(debitValue.toFixed($decimalPlace));
                            $(trTransaction).find("td.td_credit input").val(0);
                        }
                        if ($(trTransaction).find("td.td_credit input").val() > 0 || ($(trTransaction).find("td.td_debit input").val() === 0 && $(trTransaction).find("td.td_credit input").val() === 0 && i === 1)) {
                            var creditValue = creditVal * (response.Rate / 100);
                            $(trTransaction).find("td.td_credit input").val(creditValue.toFixed($decimalPlace));
                            $(trTransaction).find("td.td_debit input").val(0);
                        }
                    }
                sumCredit(thisCredit);
                sumDebit(null);
            }
        });
    } else {
        sumCredit(thisCredit);
        sumDebit(null);
    }
}
function tdchange() {
    setTimeout(function () {
        // set multi for table
        $('tr select').not('.multi-select').select2();
    }, 1);
}
function getAccount(id, dimension) {
    $.ajax({
        type: 'get',
        url: '/BKJournalEntries/GetDimensionByAccountId?id=' + id,
        dataType: 'html',
        success: function (response) {
            if (response !== "") {
                $(dimension.children[0]).val(JSON.parse(response));
                $('table.datatable select').not('.multi-select').select2();
            }
        },
        error: function (er) {
            console.error(er);
            //cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    });
    // get and update account new row transaction children
    var index = $(trSelect).find("td.bkTransactionRowId input.row-index").val();
    var childrens = $("#new-journal-entry span.parenttransaction_" + index);
    if (childrens.length > 0)
        $.ajax({
            type: 'get',
            url: '/BKJournalEntries/GetByAccountId?id=' + id,
            dataType: 'json',
            success: function (response) {
                if (response !== "") {
                    var trTransaction = childrens[0].parentElement.parentElement;
                    $(trTransaction).find("td.td_account input").val(response.Id);
                    $(trTransaction).find("td.td_account p").text(response.Name);
                    $(trTransaction).find("td.td_account p").removeClass('hidden');
                    $(trTransaction).find("td.td_account button.btn-edit").removeClass('hidden');
                    $(trTransaction).find("td.td_account button.btn-add").addClass('hidden');

                }
            },
            error: function (er) {
                console.error(er);
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
}
// set multi for table
$('table.datatable select').not('.multi-select').select2();

function selectedAccount(ev, idParentAcc, isDebit) {
    td = ev;
    trSelect = td.parentElement;
    //$('.jstree ul li button').addClass('btnhide');
    if (!idParentAcc || isDebit === null) idParentAcc = 0;

    if (idParentAcc || idParentAcc === 0) {

        $.ajax({
            type: 'post',
            url: '/Bookkeeping/TreeViewAccountByNodeIdPartial',
            data: { id: idParentAcc, number: 0 },
            dataType: 'html',
            success: function (response) {
                $('#app-bookkeeping-treeview div#templateAccount').empty();
                $('#app-bookkeeping-treeview div#templateAccount').append(response);
                loadTreeAccount();
                if (idParentAcc > 0) {
                    $.ajax({
                        type: 'post',
                        url: '/BKJournalEntries/GetListCoANodeParentId',
                        data: { id: idParentAcc },
                        dataType: 'json',
                        success: function (response) {
                            setTimeout(function () {
                                selectNode(response.Object);
                            }, 100);
                        },
                        error: function (er) {
                            console.error(er);
                            return [];
                        }
                    });
                }
            },
            error: function (er) {
                console.error(er);
                return [];
            }
        });

    }
}
function loadTreeAccount() {

    $('.jstree').jstree({
        "core": {
            "themes": {
                "dots": true,
                "stripes": false,
                "variant": "large"
            }
        },
        "search": {
            "case_insensitive": true,
            "show_only_matches": true
        },
        "plugins": ["themes", "search", "html_data", "ui", "wholerow"]
    });

    //$('.jstree').jstree('open_all');
    $('.jstree').on('select_node.jstree', function (e, data) {
        data.instance.toggle_node(data.node);
    }).on("select_node.jstree", function (e, data) {
        if (data.instance.get_node(data.node, true).children('a').attr('href') !== '#') {
            document.location = data.instance.get_node(data.node, true).children('a').attr('href');
        }
    });
    $('.jstree').on("select_node.jstree", function (evt, data) {
        var type = data.node.data.node;
        var id = data.node.data.value;
        selectedNote = data.node.data;

        if (type === 'BKAccount' && td !== null) {
            $($(td).find("input")[0]).val(id);
            $($(td).find("p")[0]).removeClass('hidden');
            $($(td).find("p")[0]).text(selectedNote.name);
            $($(td).find("button.btn-edit")[0]).removeClass('hidden');
            $($(td).find("button.btn-add")[0]).addClass('hidden');
        }
    });
    var showtree = function () {
        $('.treeview').fadeIn(800);
    };
    setTimeout(showtree, 200);

    $(".search-tree").keyup(function () {
        var searchString = $(this).val();
        $('.jstree').jstree('search', searchString);
    });

}
function selectNode(arrayId) {
    $("div#jstree_id").jstree("close_all");

    for (var i = 0; i < arrayId.length; i++) {
        if (i === (arrayId.length - 1)) {
            $("div#jstree_id").jstree("select_node", ".groupaccount_" + arrayId[i]);
            $("div#jstree_id").jstree("open_node", ".groupaccount_" + arrayId[i]);
        } else {
            $("div#jstree_id").jstree("open_node", ".groupaccount_" + arrayId[i]);
        }
    }
}

function selectAccount(ev, id) {
    var dimension = $(td)[0].parentElement.children[7];
    getAccount(id, dimension);
    setTimeout(function () {
        $('#app-bookkeeping-treeview').modal('toggle');
        td = null;
    }, 500);

}
function initSelectedAccount() {

}


function sumDebit(e) {

    if (e !== null) {
        var elementTr = e.parentElement.parentElement;
        var debitVal = $((elementTr).children[4].children[0]).val();
        if (debitVal.length > 0)
            $(elementTr.children[5].children[0]).attr('disabled', true);
        else if (debitVal.length === 0)
            $(elementTr.children[5].children[0]).attr('disabled', false);
    }

    var td_debits = $('#transaction_table td.td_debit input.table-transaction-debit');
    var sumDebitValue = 0;
    if (td_debits.length > 0) {
        for (var i = 0; i < td_debits.length; i++) {
            var item = $(td_debits[i]).val() === "" ? 0 : $(td_debits[i]).val();
            sumDebitValue += checkformatnumber(item);
        }
        $('span.sumDedit').text(sumDebitValue.toFixed($decimalPlace));
    } else {
        $('span.sumDedit').text('0');
    }
    var debit = checkformatnumber(sumDebitValue.toFixed($decimalPlace));
    var credit = $($('span.sumCredit')[0]).text() === "0" || $($('span.sumCredit')[0]).text() === "" ? 0 : checkformatnumber($($('span.sumCredit')[0]).text());
    if (debit - credit === 0) {
        $('button.savetemplate').attr('disabled', false);
        $('button.submitforreview').attr('disabled', false);
    } else {
        $('button.savetemplate').attr('disabled', true);
        $('button.submitforreview').attr('disabled', true);
    }
}
function sumCredit(e) {
    if (e !== null) {
        var elementTr = e.parentElement.parentElement;
        var debitVal = $((elementTr).children[5].children[0]).val();
        if (debitVal.length > 0)
            $(elementTr.children[4].children[0]).attr('disabled', true);
        else if (debitVal.length === 0)
            $(elementTr.children[4].children[0]).attr('disabled', false);

    }

    var td_credits = $('#transaction_table td.td_credit input');
    var sumCreditValue = 0;
    if (td_credits.length > 0) {
        for (var i = 0; i < td_credits.length; i++) {
            var item = $(td_credits[i]).val() === "" ? 0 : $(td_credits[i]).val();
            sumCreditValue += checkformatnumber(item);
        }
        $('span.sumCredit').text(sumCreditValue.toFixed($decimalPlace));
    } else {
        $('span.sumCredit').text('0');
    }
    var credit = checkformatnumber(sumCreditValue.toFixed($decimalPlace));
    var debit = $($('span.sumDedit')[0]).text() === "0" || $($('span.sumDedit')[0]).text() === "" ? 0 : checkformatnumber($($('span.sumDedit')[0]).text());
    if (debit - credit === 0) {
        $('button.savetemplate').attr('disabled', false);
        $('button.submitforreview').attr('disabled', false);
    } else {
        $('button.savetemplate').attr('disabled', true);
        $('button.submitforreview').attr('disabled', true);
    }
}


function DeleteAllBkTransactionRows() {
    var rowChilds = $('#new-journal-entry table.journal-transactions tbody tr.isSelectedTaxRate_tr');
    for (var r = 0; i < rowChilds.length; r++) {
        rowChilds[r].remove();
    }
    //setTimeout(function () {
    if ($('#transaction_table tbody tr').length > 2) {
        var rows = $('#transaction_table tbody tr');
        for (var i = rows.length - 1; i >= 0; i--) {
            $(rows[i]).remove();
            $('#attachments-view-' + (i + 1)).remove();
            sumDebit(null);
            sumCredit(null);
        }
        AddNewBkTransactionRow(true);
    }
    //}, 500);

}



function validAccount(template) {
    var accs = $('#transaction_table td.td_account input');
    for (var i = 0; i < accs.length; i++) {
        if ($(accs[i]).val() === "0") {
            cleanBookNotification.error(_L("ERROR_MSG_455", [i + 1]), "Qbicles");
            return false;
        }
    }

    if (!template) {
        var menos = $('#transaction_table td.td_memo input');
        for (var j = 0; j < menos.length; j++) {
            if ($(menos[j]).val() === "") {
                cleanBookNotification.error(_L("ERROR_MSG_456", [i + 1]), "Qbicles");
                return false;
            }
        }
    }


    return true;
}


function checknumber(e, evt, debit) {
    var val = $(e).val();
    var keyCode = evt.which;
    var dot = false;
    //if ((keyCode == 46 || keyCode == 44) && evt.key != charDot) evt.preventDefault();
    if (evt.key === '.') {
        dot = val.indexOf('.') !== -1;
    }
    var caratPos = 0;
    var number = e.value.split('.');
    if (e.createTextRange) {
        var r = document.selection.createRange().duplicate();
        r.moveEnd('character', e.value.length);
        if (r.text === '') caratPos = e.value.length;
        caratPos = e.value.lastIndexOf(r.text);
    } else caratPos = e.selectionStart;

    var dotPos = e.value.indexOf(".");
    if (caratPos > dotPos && dotPos > -1 && (number[1].length > 1)) {
        dot = true;
    }

    /*
      8 - (backspace)
      32 - (space)
      48-57 - (0-9)Numbers
    */
    if ((keyCode !== 8 || keyCode === 32) && ((keyCode < 48 && keyCode !== 46) || keyCode > 57 || dot)) {
        evt.preventDefault();
    }

}
var selectedTaxRate = null;
function SelectedTaxRate(ev) {
    selectedTaxRate = ev;
}
function selectedChange() {
    selectedTaxRate = null;
}
function checkformatnumber(number) {
    if (parseFloat(number) !== 0 && (number.indexOf('.') > -1 || number.indexOf(',') > -1)) {
        var number1 = parseFloat(number.replace(',', '.'));
        var number2 = parseFloat(number.replace('.', ','));
        if (parseFloat(number) > 0) {
            if (number1 > number2) return number1;
            else return number2;
        } else {
            if (number1 > number2) return number2;
            else return number1;
        }
    } else return parseFloat(number);
}

function CheckDeleteAllBkTransations() {
    var rows = $('#new-journal-entry table.journal-transactions tbody tr').length;
    var rowChild = $('#new-journal-entry table.journal-transactions tbody tr.isSelectedTaxRate_tr').length;
    if ((rows - rowChild) > 2) {
        $('#clearAll').attr('disabled', false);
        $('.delete_row').attr('disabled', false);
    } else {
        $('#clearAll').attr('disabled', true);
        $('.delete_row').attr('disabled', true);
    }
};



// ----------- workgroup ---------
var $workgroupId = 0;
ChangeBKWorkgroup = function () {
    $workgroupId = $("#bk-worgroup-select").val();
    if ($workgroupId !== "") {
        $(".submit-for-review").empty();
        $.ajax({
            type: "get",
            url: "/Bookkeeping/getworkgroup?id=" + $workgroupId,
            dataType: "json",
            success: function (response) {
                $('.preview-workgroup').show();
                if (response.result) {
                    $(".preview-workgroup table tr td.workgroup_process").text(response.Object.Process);
                    $(".preview-workgroup table tr td.workgroup_qbicle").text(response.Object.Qbicle);
                    $(".preview-workgroup table tr td.workgroup_member").text(response.Object.Members);
                } else {
                    $(".preview-workgroup table tr td.workgroup_process").text('');
                    $(".preview-workgroup table tr td.workgroup_qbicle").text('');
                    $(".preview-workgroup table tr td.workgroup_member").text('');
                }
                $(".submit-for-review").empty().append(response.msg);
            },
            error: function (er) {
                console.error(er);
                $('.preview-workgroup').hide();
                cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
            }
        });
    } else {
        $('.preview-workgroup').hide();
    }
};



// ---------------- Save ----------------------------

//var $isBusyAddJournal = false;
var $submitReviewModal = $("#submit-for-review");

function SubmitJournalEntryToReview() {

    if (!$("#new-journal-entry").valid()) {
        return;
    }

    VerifyJournalPostedDate($("#journal-posted-date-select").val(), 1).then(function (response) {
        if (!response.result) {
            LoadingOverlayEnd();
            return;
        }
        if (validAccount())
            $submitReviewModal.modal('toggle');
    });

}
function SaveJournalEntryTemplate() {
    if (validAccount('template'))
        $('#journal-template-modal').modal('toggle');
}
function SendJournalEntryToReview() {

    if (!$("#new-journal-entry").valid()) {
        return;
    }
    BkTransactoinProcessForm();

}








function VerifyJournalPostedDate(journalPostedDate, selectedType) {
    var dfd = new $.Deferred();
    // selectedType = 1 ->Journal date, = 2 -> Transaction date
    var jPostedDate = journalPostedDate;
    if (journalPostedDate === null || typeof journalPostedDate === "undefined") {
        jPostedDate = $("#journal-posted-date-select").val();
    }
    var bKPostedDates = [];

    var tablerows = $('#transaction_table tbody tr');

    for (var i = 0; i < tablerows.length; i++) {
        var tr = tablerows[i];
        var postedDate = $(tr).find("td.td_date input").val();
        bKPostedDates.push(postedDate);
    }

    $.ajax({
        type: "post",
        url: "/Bookkeeping/VerifyJournalPostedDate?journalPostedDate=" + jPostedDate,
        dataType: "json",
        data: { bkPostedDates: bKPostedDates.toString() },
        success: function (response) {
            if (response.result) {
                if (selectedType === 1) {
                    //return dfd.promise();
                    dfd.resolve(response);
                    return true; //return dfd.promise(); return dfd.promise(); return dfd.promise(); return dfd.promise(); return dfd.promise();
                }

                var minDate = new Date(Date.parse($("#closed-book-date").val()));
                minDate.setDate(minDate.getDate() + 1);

                $('#journalEntry_PostedDate').data('daterangepicker').remove();
                $('#journalEntry_PostedDate').daterangepicker({
                    singleDatePicker: true,
                    minDate: minDate,
                    //"startDate": new Date(Date.parse(correctDate)),
                    timePicker: true,
                    //autoApply: true,
                    showDropdowns: true,
                    autoUpdateInput: true,
                    cancelClass: "btn-danger",
                    opens: "left",
                    locale: {
                        cancelLabel: 'Clear',
                        format: $dateFormatByUser.toUpperCase() + ' HH:mm'
                    }
                }).on('apply.daterangepicker',
                    function (e, picker) {
                        var postedDate = picker.startDate.format($dateFormatByUser.toUpperCase() + ' HH:mm');
                        $("#journal-posted-date-select").val(postedDate);
                        VerifyJournalPostedDate(postedDate, 1);
                    }).on('cancel.daterangepicker', function (ev, picker) {
                        //$(this).val(new Date());
                    });
                dfd.resolve(response);
                return true;
            } else {
                cleanBookNotification.warning("Journal date must be constrained to be later than or equal to the latest date time selected for any transaction in the Journal Entry (" + response.msg + ")", "Qbicles");

                var correctDate = new Date(Date.parse(convertStringToDate(response.msg, getDateTimeFormat())));
                $('#journalEntry_PostedDate').data('daterangepicker').remove();
                $('#journalEntry_PostedDate').daterangepicker({
                    singleDatePicker: true,
                    minDate: new Date(Date.parse(correctDate)),
                    "startDate": new Date(Date.parse(correctDate)),
                    timePicker: true,
                    //autoApply: true,
                    showDropdowns: true,
                    autoUpdateInput: true,
                    cancelClass: "btn-danger",
                    opens: "left",
                    locale: {
                        cancelLabel: 'Clear',
                        format: $dateFormatByUser.toUpperCase() + ' HH:mm'
                    }
                }).on('apply.daterangepicker',
                    function (e, picker) {
                        var postedDate = picker.startDate.format($dateFormatByUser.toUpperCase() + ' HH:mm');
                        $("#journal-posted-date-select").val(postedDate);
                        VerifyJournalPostedDate(postedDate, 1);
                    }).on('cancel.daterangepicker', function (ev, picker) {
                        //$(this).val(new Date());
                    });

                dfd.resolve(response);
                return false;
            }
        },
        error: function (er) {
            console.error(er);
            dfd.resolve("");
            cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
    return dfd.promise();
};



function JournalEntryAddEditConfirmAdd(bkTransactionRowId) {

    var attachments = JournalEntryAddEditGetFiles(bkTransactionRowId);

    if (attachments && attachments.length > 0) {

        $("#attachments-view-" + bkTransactionRowId + " ul.domain-change-list").empty();
        _.forEach(attachments, function (file) {
            var li = " <li> <a href='javascript:void(0)'>";
            li += "<img src=\"" + file.IconPath + "\" style=\"max-width: 80px; height: auto; padding-right: 10px;\">";
            li += file.Name + " </a> </li>";
            $("#attachments-view-" + bkTransactionRowId + " ul.domain-change-list").append(li);
        });
    } else {
        $("ul.domain-change-list").empty();
    }

}


function JournalEntryAddEditAddAnother(bkTransactionRowId) {
    var inputFiles = $(".attachments_" + bkTransactionRowId + " input.inputfile");
    var attInput = "<div class=\"row attachment_row\"> <div class=\"col-xs-12\">";
    attInput += "<div class=\"form-group\"> <label for=\"name\">Name</label> <input type=\"text\" name=\"name\" class=\"form-control inputfilename" + (inputFiles.length + 1) + "\">";
    attInput += "</div> </div> <div class=\"col-xs-12\"> <div class=\"form-group\"> <label for=\"file\">File</label>";
    attInput += "<input type=\"file\" name=\"file\" onchange=\"JournalEntryAddEditChangeFile(this," + (inputFiles.length + 1) + "," + bkTransactionRowId + ")\" class=\"form-control inputfile\">  </div>  </div> </div>";
    $(".attachments_" + bkTransactionRowId + " div.repeater_wrap").append(attInput);
}


function JournalEntryAddEditChangeFile(evt, index, bkTransactionRowId) {
    var fileName = $(".attachments_" + bkTransactionRowId + " input.inputfilename" + index).val();
    if ($(evt).length > 0 && $(evt)[0].files.length > 0 && fileName.lenght === 0)
        fileName = $(evt)[0].files[0].name;
    var files = $(".attachments_" + bkTransactionRowId + " input.inputfile");
    for (var i = 0; i < files.length; i++) {
        if (files[0] !== evt && $(files[0]).val() === $(evt).val()) {
            $(evt).val('');
            //cleanBookNotification.error("Have an error, file: " + fileName + "<br> Already exists.", "Qbicles");
            fileName = '';
        }
    }
    $('.attachments_' + bkTransactionRowId + ' .inputfilename' + index).val(fileName);
}























function AddNewBkTransactionRow(initAgain) {
    $.LoadingOverlay("show");
    var rows = $('#new-journal-entry table.journal-transactions tbody tr').length;
    if (!initAgain) {
        if (rows === 1 && $('#new-journal-entry table.journal-transactions tbody tr.odd').length === 1) {
            rows = 0;
            $('#new-journal-entry table.journal-transactions tbody').empty();
        } else {
            rows = parseInt($($('#new-journal-entry table.journal-transactions tbody tr')[rows - 1].children[0]).text());
        }
    } else {
        $('#new-journal-entry table.journal-transactions tbody').empty();
        rows = 0;
    }

    var rowNewId = GenerateUUID();
    $.ajax({
        type: 'get',
        url: '/BKJournalEntries/AddNewTableRow?index=' + (rows + 1) + "&rowNewId=" + rowNewId,
        dataType: 'html',
        success: function (response) {
            $('#new-journal-entry table.journal-transactions tbody').append(response);
            $.ajax({
                type: 'get',
                url: '/BKJournalEntries/AttachmentViewPartial?index=' + (rows + 1) + "&rowNewId=" + rowNewId,
                dataType: 'html',
                success: function (response) {
                    $('#new-journal-entry').append(response);
                    if (initAgain) {
                        initAgain = false;
                        AddNewBkTransactionRow();
                    }

                },
                error: function (er) {
                    console.error(er);
                    //cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
                }
            });

        },
        error: function (er) {
            console.error(er);
            //cleanBookNotification.error("Have an error, detail: " + er.error, "Qbicles");
        }
    }).always(function () {
        CheckDeleteAllBkTransations();
        LoadingOverlayEnd();
    });
}


// action on bk transaction row
function SetBkTransactionAttachmentRow(evt) {
    td = evt;
    $(evt).addClass('currentRowAttachment');
};


function TrashBkTransactionRow(trElement, bkTransactionRowId) {
    var idtran = $(trElement).find("td.bkTransactionRowId input").val();
    var childrens = $("#new-journal-entry span.parenttransaction_" + idtran);
    if (childrens.length) {
        for (var i = 0; i < childrens.length; i++) {
            $(childrens[i].parentElement.parentElement).remove();
        }
    }
    if ($('#transaction_table tbody tr').length > 2) {
        $(trElement).remove();
        $('#attachments-view-' + bkTransactionRowId).remove();
        sumDebit(null);
        sumCredit(null);
        setTimeout(function () { CheckDeleteAllBkTransations(); }, 100);
    }

};

function JournalEntryAddEditGetFiles(bkTransactionRowId) {
    var elementFiles = $('#attachments-view-' + bkTransactionRowId + ' .attachments_' + bkTransactionRowId + ' input.inputfile');
    var $bkTransactionAttachments = [];
    for (var i = 0; i < elementFiles.length; i++) {
        //var elementFileNames = $('#attachments-view-' + bkTransactionRowId + ' .attachments_' + bkTransactionRowId + ' input.inputfilename' + (i + 1));
        if (elementFiles[i].files.length > 0) {

            var fileAdd = elementFiles[i].files[0];
            fileExtension = fileAdd.name.split('.').pop();
            var fileType = _.find($qbiclesFileTypes, function (o) { return o.Extension === fileExtension; });
            var attachmentAdd = {
                Id: GenerateUUID(),//bkTransactionRowId,//GenerateUUID(), new media upload
                Name: fileAdd.name,
                Extension: fileAdd.name.split('.').pop(),
                Size: fileAdd.size,
                IconPath: fileType.IconPath,
                File: fileAdd
            };
            if ($("div.attachments_" + bkTransactionRowId + " .inputfilename" + (i + 1)).val() !== "") {
                attachmentAdd.Name = $("div.attachments_" + bkTransactionRowId + " .inputfilename" + (i + 1)).val() + "." + fileExtension;
            }
            $bkTransactionAttachments.push(attachmentAdd);

            $bkTransactionAttachmentsUpload.push(attachmentAdd)
        }
        else {
            //edit bull attachment -------------------
            if ($("#fileid-" + bkTransactionRowId + "-" + (i + 1)).length > 0) {
                var attachmentEdit = {
                    Id: $("#fileid-" + bkTransactionRowId + "-" + (i + 1)).val(),// Id of media associated. bkTransactionRowId,//GenerateUUID(),
                    Name: $(".inputfilename" + (i + 1)).val(),
                    IconPath: $("#inputiconpath_edit" + (i + 1)).val()
                };
                $bkTransactionAttachments.push(attachmentEdit);
            }
        }
    }
    return $bkTransactionAttachments;
}


var $bkTransactions = [];

var $bkTransactionAttachmentsUpload = [];

function BkTransactoinProcessForm() {
    $.LoadingOverlay("show");


    $bkTransactionAttachmentsUpload = [];
    $bkTransactions = [];
    //process data
    var tablerows = $('#transaction_table tbody tr');

    for (var i = 0; i < tablerows.length; i++) {

        var tr = tablerows[i];
        var idParent = $(tr).find("td.td_dimension input.parentid-transaction").val();
        var postedDate = convertStringToDate($(tr).find("td.td_date input").val());


        var bkTransactionRowId = $(tr).find("td.bkTransactionRowId input").val();
       
        var id = bkTransactionRowId;

        var bkTransaction = {
            Id: id,
            Account: { Id: $(tr).find("td.td_account input").val() }, // account
            Reference: $(tr).find("td.td_reference input").val(),
            PostedDate: postedDate,//$(tr.children[2].children[0]).val(),
            Debit: $(tr).find("td.td_debit input").val().replace(',', charDot).replace('.', charDot),//$(tr.children[3].children[0]).val().replace(',', charDot).replace('.', charDot),
            Credit: $(tr).find("td.td_credit input").val().replace(',', charDot).replace('.', charDot),//$(tr.children[4].children[0]).val().replace(',', charDot).replace('.', charDot),
            Memo: $(tr).find("td.td_memo input").val(),//$(tr.children[6].children[0]).val(),
            Dimensions: [],
            AssociatedFiles: []
        };
        if (idParent !== 0) {
            bkTransaction.Parent = { Id: idParent }
        }
        var dimensions = $(tr).find("td.td_dimension select").val();// $(tr.children[7].children[0]).val();
        if (dimensions !== null)
            dimensions.forEach(function (item) {
                bkTransaction.Dimensions.push({ Id: item });
            });


        var $bkTransactionMedias = JournalEntryAddEditGetFiles(bkTransactionRowId);

        bkTransaction.AssociatedFiles = $bkTransactionMedias;

        

        $bkTransactions.push(bkTransaction);
    }


    if ($bkTransactionAttachmentsUpload.length > 0) {
        //need processing valid upload before

        UploadBatchMediasS3ClientSide($bkTransactionAttachmentsUpload).then(function () {
            SaveJournalEntry();
        });
    }
    else {
        SaveJournalEntry();
    }
}
function SaveJournalEntry() {
    //$.LoadingOverlay("show");

    var jEntry = {
        Id: $('#journalEntryId').val(),
        Description: $('#journalEntry_desciption').val(),
        PostedDate: convertStringToDate($('#journalEntry_PostedDate').val()),
        Group: { Id: $('#journalEntry_Group').val() },
        WorkGroup: { Id: $("#bk-worgroup-select").val() }//,
        //BKTransactions: []
    };
    var $bkTransactionAndAssociatedFiles = [];
    _.forEach($bkTransactions, function (bk) {
        _.forEach(bk.AssociatedFiles, function (file) {
            file.File = {};
        });
        $bkTransactionAndAssociatedFiles.push(bk);
    });


    $.ajax({
        type: 'post',
        url: '/BKJournalEntries/SaveJournalEntry',
        data:
        {
            jEntry: jEntry,
            bKTransactions: $bkTransactionAndAssociatedFiles
        },
        dataType: 'json',
        success: function (response) {
            if (response.actionVal === 1 || response.actionVal === 2) {
                $('.qbicle-detail *').prop('disabled', true);
                $submitReviewModal.modal('hide');
                if (response.actionVal === 1) {
                    cleanBookNotification.createSuccess();
                } else if (response.actionVal === 2) {
                    cleanBookNotification.updateSuccess();
                }
                setTimeout(function () {
                    window.location.href = '/Bookkeeping/JournalEntries';
                }, 1500);
                
            } else {
                console.log(response.msg);
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            console.error(er);
            cleanBookNotification.error("Have an error: " + er.error, "Qbicles");
            return [];
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
};


var $qbiclesFileTypes = [];

$(document).ready(function () {
    $qbiclesFileTypes = [];
    $.ajax({
        type: 'post',
        url: '/FileTypes/GetFileTypes',
        dataType: 'json',
        success: function (response) {
            $qbiclesFileTypes = response;
        },
        error: function (er) {
            console.log(er);
        }
    }).always(function () {
        LoadingOverlayEnd();
    });
});