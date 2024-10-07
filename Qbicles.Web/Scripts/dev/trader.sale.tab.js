var tabSale = "";

$(function () {
    tabSale = getTabTrader().TraderTab;
    activeTab = getTabTrader().SubTraderTab;
   
    if (activeTab === "trader-sale-content") {
        ShowTableSaleRecordsValue();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else if (activeTab === "trader-sale-return-content") {
        ShowTableSaleReturnValue();
        $('a[href="#' + activeTab + '"]').tab('show');
    } else {
        ShowTableSaleRecordsValue();
        $('a[href="#' + activeTab + '"]').tab('show');
    }
});


function ShowTableSaleRecordsValue() {

    //$.LoadingOverlay("show");

    activeTab = 'trader-sale-content';
    setTabTrader(tabSale, activeTab);
    
    var filter = {
        Workgroup: "",
        SaleChanel: "",
        Key: "",
        DateRange: ""
    };
    $('#trader-sale-content').empty();
    $('#trader-sale-content').load('/TraderSales/TraderSaleRecordsTable', filter, function () {
        $('#section_dashboard').load("/TraderSales/TraderSaleGetDataDashBoard?keyword=" + $("#search_dt").val() + "&workGroupId=" + $("#subfilter-group").val() + "&datetime=" + $("#sale-input-datetimerange").val().replace(/\s/g, '%20') + "&channel=" + $("#subfilter-channel").val(), function () {

        });
    });
};

function ShowTableSaleReturnValue() {
    var filter = {
        Workgroup: "",
        SaleChanel: "",
        Key: "",
        DateRange: ""
    };
    activeTab = 'trader-sale-return-content';
    setTabTrader(tabSale, activeTab);
    
    $('#trader-sale-return-content').empty();
    $('#trader-sale-return-content').load('/TraderSalesReturn/TraderSaleReturnTable', filter, function () {
    });
}

function ShowTopSell(modalId) {
    $("#" + modalId).load("/TraderSales/ShowSaleDashboardDetail?modalId=" + modalId +"&keyword=" + $("#search_dt").val() + "&workGroupId=" + $("#subfilter-group").val() + "&datetime=" + $("#sale-input-datetimerange").val().replace(/\s/g, '%20') + "&channel=" + $("#subfilter-channel").val(), function () {
        $("#" + modalId).removeClass().addClass('modal left fade in');
        $("#" + modalId).show();

        document.body.style.overflow = "hidden";
        $modalContent.addEventListener("scroll", preventScroll);
    });
}


function CloseTopSell(modalId) {
    $("#" + modalId).removeClass();
    $("#" + modalId).hide()

    $modal.style.display = "none";
    document.body.style.overflow = "auto";
    $modalContent.removeEventListener("scroll", preventScroll);
}


function preventScroll(event) {
    event.preventDefault();
}
