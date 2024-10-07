
var $journalTempBase = $("#journal-template-base"),
    $templateId = $("#template-id"),
    $journalTemplateModal = $("#journal-template-modal");


$(document).ready(function () {


});

var $isBusySaveJournalTemplate = false;
function SaveJournalTemplate() {
    if ($isBusySaveJournalTemplate) {
        return;
    }
    if ($("#journal-template-base").valid()) {
        setTimeout(function () {
            ConfirmSaveJournalTemplate();
        }, 500);

    }
};

function ConfirmSaveJournalTemplate() {
    var jEntry = {
        Group: { Id: $('#journalEntry_Group').val() },
        WorkGroup: { Id: $("#bk-worgroup-select").val() },
        BKTransactions: []
    };
    var tablerows = $('#transaction_table tbody tr');
    var lstFiles = new FormData();
    for (var i = 0; i < tablerows.length; i++) {
        var tr = tablerows[i];
        var tds = tr.children;

        var bkTransaction = {
            Id: $(tr.children[0].children[0]).val(), // index
            Account: { Id: $(tr.children[1].children[0]).val() }, // account
            PostedDate: $(tr.children[2].children[0]).val(),
            Debit: $(tr.children[3].children[0]).val().replace(',', charDot).replace('.', charDot),
            Credit: $(tr.children[4].children[0]).val().replace(',', charDot).replace('.', charDot),
            TaxRate: { Id: $(tr.children[5].children[0]).val() },
            Memo: $(tr.children[6].children[0]).val(),
            Dimensions: []
        };
        var dimensions = $(tr.children[7].children[0]).val();
        if (dimensions !== null)
            dimensions.forEach(function (item) {
                bkTransaction.Dimensions.push({ Id: item });
            });
        jEntry.BKTransactions.push(bkTransaction);
        lstFiles = getFiles((i + 1), lstFiles);
    }

    var template = {
        Name: $("#template-name").val(),
        Description: $("#template-description").val()
    };
    $.ajax({
        type: 'post',
        url: '/BKJournalEntries/SaveJournalEntryTemplate',
        data:
        {
            jEntry: jEntry, template: template
        },
        dataType: 'json',
        success: function (response) {
            if (response.result) {
                $isBusySaveJournalTemplate = false;
                $('#journal-template-modal').modal('toggle');
                cleanBookNotification.createSuccess();
            } else {
                cleanBookNotification.error(response.msg, "Qbicles");
            }
        },
        error: function (er) {
            return [];
        }
    });
};



UsejournalTemplate = function (id) {
    $.ajax({
        type: 'get',
        url: '/Bookkeeping/ListTransactions?templateId=' + id,
        dataType: 'html',
        success: function (response) {
            $('div#table-journal-entry').empty();
            $('div#table-journal-entry').append(response);
            $('.singledate').on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });
            $('.singledateandtime').on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });

            $('.singledate').daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale:
                    {
                        cancelLabel: 'Clear',
                        format: 'DD/MM/YYYY'
                    }
            });
           
            // set multi for table
            $('tr select').not('.multi-select').select2();
            sumDebit(null);
            sumCredit(null);
        },
        error: function (er) {
            return [];
        }
    });
};