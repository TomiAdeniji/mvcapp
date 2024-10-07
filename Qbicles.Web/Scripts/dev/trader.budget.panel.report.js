

FilterBudgetPanelReport = function() {
    var filter = {
        Id: $("#budget-scenario-id").val(),
        Workgroup: $("#filter-workgroup").val(),
        Dimensions: $("#filter-dimensions").val().toString(),
        DateRange: $("#filter-datetimerange").val()
    };

    var ajaxUri = '/TraderBudget/BudgetPanelReport';
    AjaxElementLoadPost(ajaxUri, filter, 'budget-panel-report-ul');
};