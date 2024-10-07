var isBusy = false;
let rankPromotionTypeRowsCollections = [];
var apiDoc = $("#api-uri").val();
var bulkDealItems = [];

function OpenPromotion(promotionKey) {
    $.LoadingOverlay("show");
    window.location.href = "/Administration/PromotionType?key=" + promotionKey;
}

function switchpromos(elm) {
    $(elm).valid();
    $(".promotype").hide();
    var typeval = $(elm).val();
    if (typeval == "1") {
        //#itemdiscount
        $("#itemdiscount").show();
    } else if (typeval == "2") {
        //#orderdiscount
        $("#orderdiscount").show();
    }
    setValidateItemAndOrderByType(typeval);
}

function loadModalPromotionAddEdit(promotionKey, modalState) {
    var $promodal = $("#moniback-promo-add");
    $promodal.empty();
    $promodal.modal("show");
    $promodal.load(
        "/Administration/LoadModalPromotionAddEdit",
        { promotionKey: promotionKey },
        function () {
            $("#moniback-promo-add .checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: "100%",
                maxHeight: 400,
                enableClickableOptGroups: true,
            });
            $("#moniback-promo-add .select2").select2({
                placeholder: "Please select",
            });
            $("#moniback-promo-add .singledateandtime").daterangepicker({
                singleDatePicker: true,
                timePicker: true,
                autoApply: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale: {
                    cancelLabel: "Clear",
                    format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                },
            });
            $("#moniback-promo-add .chktoggle").bootstrapToggle();
            initFormPromotionValidate();
        }
    );
}

function loadModalPromotionView(promotionKey) {
    var $promodal = $("#moniback-promo-view");
    $promodal.empty();
    $promodal.modal("show");
    $promodal.load(
        "/Administration/LoadModalPromotionView",
        { promotionKey: promotionKey },
        function () {
            $("#moniback-promo-view .checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: "100%",
                maxHeight: 400,
                enableClickableOptGroups: true,
            });
            $("#moniback-promo-view .select2").select2({
                placeholder: "Please select",
            });
            $("#moniback-promo-view .singledateandtime").daterangepicker({
                singleDatePicker: true,
                timePicker: true,
                autoApply: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale: {
                    cancelLabel: "Clear",
                    format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                },
            });
            $("#moniback-promo-view .chktoggle").bootstrapToggle();
        }
    );
}

function loadModalRankPromotion() {
    $.LoadingOverlay("show");
    var $promodal = $("#moniback-promo-rank");
    $promodal.empty();
    $promodal.modal("show");
    $promodal.load("/Administration/LoadModalRankPromotion", function () {
        $("#moniback-promo-rank .checkmulti").multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            buttonWidth: "100%",
            maxHeight: 400,
            enableClickableOptGroups: true,
        });

        initMonibackRankPromotionTable();
        LoadRankPromotionList();

        setTimeout(function () {
            $.LoadingOverlay("hide");
        }, 1000);
    });
}

function setMinTimeFromToTimePromotion() {
    var isSetTime = $("#chkSpecificTime").prop("checked");
    var fromTimeVal = $("#frmAddEditDeal input[name=FromTime]").val();
    var $totime = $("#frmAddEditDeal input[name=ToTime]");
    if (fromTimeVal && isSetTime) {
        var listTime = fromTimeVal.split(":");
        listTime[1]++;
        listTime[0] = listTime[0].toLocaleString("en-US", {
            minimumIntegerDigits: 2,
            useGrouping: false,
        });
        listTime[1] = listTime[1].toLocaleString("en-US", {
            minimumIntegerDigits: 2,
            useGrouping: false,
        });
        fromTimeVal = listTime[0] + ":" + listTime[1];
        $totime.attr("min", fromTimeVal);
    } else {
        $totime.removeAttr("min");
    }
}

function showModalFindSKU() {
    var sku = $("#item_sku").val();
    var url = "/Commerce/LoadModalFindTraderItem?sku=" + sku;
    url = url.replace(/\s/g, "%20");
    $("#app-trader-pos-itemlist").load(url);
}

function LoadRankPromotionList() {
    if ($.fn.DataTable.isDataTable("#tblMonibackRankPromotions")) {
        $("#tblMonibackRankPromotions").DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $("#tblMonibackRankPromotions").DataTable().ajax.reload();
        }, 1000);
    }
}

function loadModalPromotionsUsedBy(promotionKey) {
    $.LoadingOverlay("show");
    var $promodal = $("#moniback-promo-usedby");
    $promodal.empty();
    $promodal.modal("show");
    $promodal.load(
        "/Administration/loadModalPromotionsUsedBy",
        { promotionKey: promotionKey },
        function () {
            $("#moniback-promo-usedby .checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: "100%",
                maxHeight: 400,
                enableClickableOptGroups: true,
            });

            initPromotionTypeParticipatingDomainTable(promotionKey);
            LoadPromotionUsedByList();

            setTimeout(function () {
                $.LoadingOverlay("hide");
            }, 1000);
        }
    );
}

function LoadPromotionUsedByList() {
    if ($.fn.DataTable.isDataTable("#tblMonibackPromotionsUsedBy")) {
        $("#tblMonibackPromotionsUsedBy").DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $("#tblMonibackPromotionsUsedBy").DataTable().ajax.reload();
        }, 1000);
    }
}

function initPromotionTypeParticipatingDomainTable(promotionKey) {
    $("#tblMonibackPromotionsUsedBy")
        .on("processing.dt", function (e, settings, processing) {
            $("#processingIndicator").css("display", "none");
            if (processing) {
                $("#tblMonibackPromotionsUsedBy").LoadingOverlay("show");
            } else {
                $("#tblMonibackPromotionsUsedBy").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            lengthMenu: [
                [10, 20, 50, 100],
                [10, 20, 50, 100],
            ],
            pageLength: 10,
            deferLoading: 30,
            order: [[1, "desc"]],
            ajax: {
                url: "/Administration/GetParticipatingDomainsInPromotionType",
                type: "POST",
                data: { promotionKey: promotionKey },
            },
            columns: [
                {
                    data: "BusinessName",
                    orderable: true,
                },
                {
                    data: "DateStarted",
                    orderable: false,
                    render: function (data, type, row) {
                        if (data != null) {
                            let dtStart = new Date(parseInt(data.substr(6)));
                            return dtStart.toLocaleDateString();
                        } else {
                            return "";
                        }
                    },
                },
                {
                    data: "ElapseDate",
                    orderable: false,
                    render: function (data, type, row) {
                        //console.log(data, 'data')
                        if (data != null) {
                            let dtStart = new Date(parseInt(data.substr(6)));
                            return dtStart.toLocaleDateString();
                        } else {
                            return "";
                        }

                        //if (type === 'display' || type === 'filter') {
                        //    var date = new Date(data);
                        //    var day = ('0' + date.getDate()).slice(-2);
                        //    var month = ('0' + (date.getMonth() + 1)).slice(-2);
                        //    var year = date.getFullYear();
                        //    return day + '.' + month + '.' + year;
                        //}
                        //return data;
                    },
                },
            ],
        });
}

function initDragDrop() {
    // Listen to click event on rows
    $("#tblMonibackRankPromotions tbody").on("click", "tr", function () {
        var data = table.row(this).data();
        console.log("Row clicked:", data);
    });
}

function initMonibackRankPromotionTable() {
    $("#tblMonibackRankPromotions")
        .on("processing.dt", function (e, settings, processing) {
            $("#processingIndicator").css("display", "none");
            if (processing) {
                $("#tblMonibackRankPromotions").LoadingOverlay("show");
            } else {
                $("#tblMonibackRankPromotions").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            lengthMenu: [
                [10, 20, 50, 100],
                [10, 20, 50, 100],
            ],
            pageLength: 10,
            deferLoading: 30,
            order: [[1, "desc"]],
            ajax: {
                url: "/Administration/GetRankPromotionTypes",
                type: "POST",
            },
            rowReorder: {
                //selector: 'span.reorder',
                selector: "tr",
                //selector: 'tr td:not(:first-child)',
                dataSrc: "Rank",
                update: false, // Prevent automatic reordering
            },
            columns: [
                {
                    data: "Promotion",
                    orderable: false,
                },
                {
                    data: "Type",
                    orderable: false,
                },
                {
                    data: "Duration",
                    orderable: false,
                    render: function (data, type) {
                        return data + " days";
                    },
                },
                {
                    data: "Amount",
                    orderable: false,
                    render: function (data, type) {
                        return new Intl.NumberFormat("en-US", {
                            style: "currency",
                            currency: "NGN",
                        }).format(data);
                    },
                },
                {
                    data: "Rank",
                    orderable: false,
                    render: function (data, type) {
                        return data;
                    },
                },
            ],
        });

    //Re-order
    let table = $("#tblMonibackRankPromotions").DataTable();
    table.on("row-reorder.dt", function (e, diff, edit) {
        let reorderedData = [];
        let temp = edit.triggerRow.data();
        let result =
            "Reorder started on row: " + edit.triggerRow.data()["Promotion"] + "<br>";

        for (let i = 0, ien = diff.length; i < ien; i++) {
            let rowData = table.row(diff[i].node).data();

            console.log(rowData, "rowData");
            result +=
                rowData["Promotion"] +
                " updated to be in position " +
                diff[i].newData +
                " (was " +
                diff[i].oldData +
                ")<br>";

            if (rowData !== undefined || rowData !== null) {
                table.rows().every(function (rowIdx, tableLoop, rowLoop) {
                    let row = this.data();

                    if (row["Key"] === rowData["Key"]) {
                        console.log(row, "row match");
                        row["Rank"] = diff[i].newData;

                        table.row(rowIdx).data(row);
                    }

                    return row;
                });
            }
        }

        console.log(table, "table");
    });

    //Load rank promotion
    LoadRankPromotionList();
}

function SaveRankPromotionOrder() {
    $.LoadingOverlay("show");

    let reorderedData = [];

    $("#tblMonibackRankPromotions")
        .DataTable()
        .rows()
        .every(function (rowIdx, tableLoop, rowLoop) {
            let data = this.data();
            reorderedData.push({
                PromotionTypeKey: data.Key,
                Name: data.Promotion,
                Type: data.Type,
                Duration: data.Duration,
                Price: data.Amount,
                Rank: data.Rank,
            });
        });

    $.ajax({
        url: "/Administration/SaveRankPromotionOrder",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(reorderedData),
        success: function (response) {
            console.log(response, "response");
            if (response.result) {
                cleanBookNotification.createSuccess();
                initMonibackRankPromotionTable();
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(response.msg, "Moniback Promotions");
            } else {
                cleanBookNotification.error(
                    _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                    "Moniback Promotions"
                );
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(
                _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                "Moniback Promotions"
            );
        },
        complete: function () {
            setTimeout(function () {
                $.LoadingOverlay("hide");
            }, 1000);
        }
    });

    //initMonibackRankPromotionTable();
    //LoadRankPromotionList();
}

function initMonibackRankPromotionTableOld() {
    $("#tblMonibackRankPromotions")
        .on("processing.dt", function (e, settings, processing) {
            $("#processingIndicator").css("display", "none");
            if (processing) {
                $("#tblMonibackRankPromotions").LoadingOverlay("show");
            } else {
                $("#tblMonibackRankPromotions").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            lengthMenu: [
                [10, 20, 50, 100],
                [10, 20, 50, 100],
            ],
            pageLength: 10,
            deferLoading: 30,
            order: [[1, "desc"]],
            ajax: {
                url: "/Administration/GetRankPromotionTypes",
                type: "POST",
                //dataSrc: function (json) {
                //    console.log(json, 'json')
                //}
            },
            rowReorder: {
                //selector: 'tr',
                selector: "tr:not(.no-reorder) td:not(:first-child)",
            },
            columns: [
                {
                    data: "Promotion",
                    orderable: false,
                },
                {
                    data: "Type",
                    orderable: false,
                },
                {
                    data: "Duration",
                    orderable: false,
                    render: function (data, type) {
                        return data + " days";
                    },
                },
                {
                    data: "Amount",
                    orderable: false,
                    render: function (data, type) {
                        return data;
                    },
                },
                {
                    data: "Rank",
                    orderable: false,
                    render: function (data, type) {
                        return data;
                    },
                },
            ],
        });
}

function disableInputsAndHideButton() {
    // Disable all input fields
    $("#frmAddEditPromotion input").prop("disabled", true);

    // Disable all textarea fields
    $("#frmAddEditPromotion textarea").prop("disabled", true);

    // Disable all select fields
    $("#frmAddEditPromotion select").prop("disabled", true);

    // Hide the Save Changes button
    $("#stp1").hide();
}

function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this,
            args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        }, delay || 500);
    };
}

function initFormPromotionValidate() {
    var $frmAddEditPromotion = $("#frmAddEditPromotion");

    $frmAddEditPromotion.validate({
        ignore: "",
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $(
                    'a[href="#' +
                    jQuery(validator.errorList[0].element)
                        .closest(".tab-pane")
                        .attr("id") +
                    '"]'
                ).tab("show");
        },
    });
    $frmAddEditPromotion.submit(function (e) {
        e.preventDefault();

        if (isBusy) return;
        if ($frmAddEditPromotion.valid()) {
            $.LoadingOverlay("show");
            //savePromotion();
        }
    });
}

var $promotionKey = "";
var $promotionName = "";
var $promotionStop = false;
var $promotionMessage = false;

function savePromotionType() {
    var frmname = "#frmAddEditPromotion";

    //init parameters
    var _paramaters = {
        model: {
            PromotionTypeKey: $(frmname + " input[name=Key]").val(),
            Name: $(frmname + " input[name=Name]").val(),
            Description: $(frmname + " textarea[name=Description]").val(),
            Icon: $(frmname + " select[name=Icon]").val(),
            Type: $(frmname + " select[name=Type]").val(),
            Duration: $(frmname + " input[name=Duration]").val(),
            Price: $(frmname + " input[name=Price]").val(),
            Rank: $(frmname + " input[name=Rank]").val(),
        },
    };

    console.log(_paramaters, "params");

    //Server side ajax call
    $.ajax({
        type: "post",
        url: "/Administration/SavePromotionType",
        data: _paramaters,
        dataType: "json",
        success: function (response) {
            console.log(response, "response");
            if (response.result) {
                cleanBookNotification.createSuccess();
                $("#moniback-promo-add").modal("hide");
                LoadListMonibackPromotion();
                $(window).scrollTop(300);
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(response.msg, "Moniback Promotions");
            } else {
                cleanBookNotification.error(
                    _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                    "Moniback Promotions"
                );
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(
                _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                "Moniback Promotions"
            );
        },
    }).always(function () {
        LoadingOverlayEnd();
    });
}

//customize Date-picker
function initDateTimeCustomize() {
    var currentDate = moment().format($dateTimeFormatByUser);
    var endCurrentDate = moment().endOf("day");
    //init Advertise time
    $(".singledateandtime-displaydate").daterangepicker({
        minDate: currentDate,
        maxDate: endCurrentDate,
        singleDatePicker: true,
        timePicker: true,
        autoApply: true,
        showDropdowns: true,
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "left",
        locale: {
            cancelLabel: "Clear",
            format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
        },
    });
    // init Start and End
    if (
        $(
            "#frmAddEditPromotion input[name=StartDate], #frmAddEditPromotion input[name=EndDate]"
        ).length
    ) {
        $(
            "#frmAddEditPromotion input[name=StartDate], #frmAddEditPromotion input[name=EndDate]"
        ).daterangepicker(
            {
                alwaysShowCalendars: true,
                minDate: currentDate,
                timePicker: true,
                autoApply: true,
                autoUpdateInput: false,
                showDropdowns: true,
                cancelClass: "btn-danger",
                locale: {
                    cancelLabel: "Clear",
                    format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                },
            },
            function (start, end, label) {
                var selectedStartDate = start.format($dateTimeFormatByUser);
                var selectedEndDate = end.format($dateTimeFormatByUser);

                $checkinInput = $("#frmAddEditPromotion input[name=StartDate]");
                $checkoutInput = $("#frmAddEditPromotion input[name=EndDate]");

                $checkinInput.val(selectedStartDate);
                $checkoutInput.val(selectedEndDate);

                var checkOutPicker = $checkoutInput.data("daterangepicker");
                checkOutPicker.setStartDate(selectedStartDate);
                checkOutPicker.setEndDate(selectedEndDate);

                var checkInPicker = $checkinInput.data("daterangepicker");
                checkInPicker.setStartDate(selectedStartDate);
                checkInPicker.setEndDate(selectedEndDate);

                //limit end date of Advertise by re-init
                $(".singledateandtime-displaydate").daterangepicker({
                    minDate: currentDate,
                    maxDate: selectedEndDate,
                    singleDatePicker: true,
                    timePicker: true,
                    autoApply: true,
                    showDropdowns: true,
                    autoUpdateInput: true,
                    cancelClass: "btn-danger",
                    opens: "left",
                    locale: {
                        cancelLabel: "Clear",
                        format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                    },
                });
                calculateDaysForVoucherUsed();
            }
        );
    }

    if (
        $(
            "#frmAddEditDeal input[name=StartDate], #frmAddEditDeal input[name=EndDate]"
        ).length
    ) {
        $(
            "#frmAddEditDeal input[name=StartDate], #frmAddEditDeal input[name=EndDate]"
        ).daterangepicker(
            {
                alwaysShowCalendars: true,
                minDate: currentDate,
                timePicker: true,
                autoApply: true,
                autoUpdateInput: false,
                showDropdowns: true,
                cancelClass: "btn-danger",
                locale: {
                    cancelLabel: "Clear",
                    format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                },
            },
            function (start, end, label) {
                var selectedStartDate = start.format($dateTimeFormatByUser);
                var selectedEndDate = end.format($dateTimeFormatByUser);

                $checkinInput = $("#frmAddEditDeal input[name=StartDate]");
                $checkoutInput = $("#frmAddEditDeal input[name=EndDate]");

                $checkinInput.val(selectedStartDate);
                $checkoutInput.val(selectedEndDate);

                var checkOutPicker = $checkoutInput.data("daterangepicker");
                checkOutPicker.setStartDate(selectedStartDate);
                checkOutPicker.setEndDate(selectedEndDate);

                var checkInPicker = $checkinInput.data("daterangepicker");
                checkInPicker.setStartDate(selectedStartDate);
                checkInPicker.setEndDate(selectedEndDate);

                //limit end date of Advertise by re-init
                $(".singledateandtime-displaydate").daterangepicker({
                    minDate: currentDate,
                    maxDate: selectedEndDate,
                    singleDatePicker: true,
                    timePicker: true,
                    autoApply: true,
                    showDropdowns: true,
                    autoUpdateInput: true,
                    cancelClass: "btn-danger",
                    opens: "left",
                    locale: {
                        cancelLabel: "Clear",
                        format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                    },
                });
                calculateDaysForVoucherUsedInBulkDeal();
            }
        );
    }
}

function calculateDaysForVoucherUsedInBulkDeal() {
    var startDate = $("#bulkdeal-promo-add input[name=StartDate]").val();
    var endDate = $("#bulkdeal-promo-add input[name=EndDate]").val();
    $.post(
        "/Commerce/GetCalculateDatesForPromotions",
        { endDate: endDate, startDate: startDate },
        function (response) {
            var $selectdays = $("#bulkdeal-promo-add select[name=Days]");
            $("#bulkdeal-promo-add select[name=Days] > option").each(function () {
                //alert(this.text + ' ' + this.value);
                if (response) {
                    var objDayCal = response.find((o) => o.ShortName === this.value);
                    if (!objDayCal) {
                        $(this).attr("disabled", true);
                    } else $(this).attr("disabled", false);
                }
            });
            $selectdays.multiselect("refresh");
        }
    );
}

function LoadListMonibackPromotion() {
    if ($.fn.DataTable.isDataTable("#tblMonibackPromotions")) {
        $("#tblMonibackPromotions").DataTable().ajax.reload();
    } else {
        setTimeout(function () {
            $("#tblMonibackPromotions").DataTable().ajax.reload();
        }, 1000);
    }
}

function initMonibackPromotionTable() {
    $("#tblMonibackPromotions")
        .on("processing.dt", function (e, settings, processing) {
            $("#processingIndicator").css("display", "none");
            if (processing) {
                $("#tblMonibackPromotions").LoadingOverlay("show");
            } else {
                $("#tblMonibackPromotions").LoadingOverlay("hide", true);
            }
        })
        .DataTable({
            destroy: true,
            serverSide: true,
            paging: true,
            searching: false,
            responsive: true,
            lengthMenu: [
                [10, 20, 50, 100],
                [10, 20, 50, 100],
            ],
            pageLength: 10,
            deferLoading: 30,
            order: [[2, "desc"]],
            ajax: {
                url: "/Administration/GetPromotionTypes",
                type: "POST",
                data: function (d) {
                    return $.extend({}, d, {
                        keyword: $("#request-search").val(),
                        dateRange: $("#request-daterange").val(),
                        type: $("#request-type").val(),
                    });
                },
            },
            columns: [
                {
                    data: "Icon",
                    orderable: false,
                    render: function (data, type, row, meta) {
                        return (
                            '<i class="fa fa-' +
                            row.Icon +
                            '" style="font-size: 25px;color: #fc8b02;"></i>'
                        );
                    },
                },
                {
                    data: "Name",
                    name: "Name",
                    orderable: true,
                    render: function (value, type, row) {
                        return row.Name;
                    },
                },
                {
                    data: "CreatedDate",
                    name: "CreatedDate",
                    orderable: true,
                    render: function (data, value, type, row) {
                        return data;
                    },
                },
                {
                    data: "IsActive",
                    name: "IsActive",
                    orderable: true,
                    render: function (data, type, row) {
                        if (data === true)
                            return (
                                '<span class="label label-success label-lg" id="status-' +
                                row.Key +
                                '">Active</span>'
                            );
                        else
                            return (
                                '<span class="label label-danger label-lg" id="status-' +
                                row.Key +
                                '">Deactivated</span>'
                            );
                    },
                },
                {
                    data: null,
                    orderable: false,
                    render: function (value, type, row) {
                        var str = ' <div class="btn-group options">';
                        str +=
                            ' <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"> Options &nbsp; <i class="fa fa-angle-down"></i></button>';
                        str += '<ul class="dropdown-menu">';
                        str += "  <li>";
                        str +=
                            '<a href="javascript:void(0)" onclick="loadModalPromotionView(\'' +
                            row.Key +
                            "')\">View</a>";
                        str += "</li>";
                        str += "  <li>";
                        str +=
                            '<a href="javascript:void(0)" onclick="loadModalPromotionAddEdit(\'' +
                            row.Key +
                            "', 'edit')\">Edit </a>";
                        str += "</li>";
                        str += " <li>";
                        str +=
                            ' <a href="javascript:void(0)" id="openOrClose-' +
                            row.Id +
                            '" onclick="managePromotionTypeStatus(\'' +
                            row.Key +
                            "')\">";
                        str += row.IsActive === false ? "Activate" : "Deactivate";
                        str += " </a>";
                        str += " </li>";
                        str += "  <li>";
                        str +=
                            '<a href="javascript:void(0)" onclick="loadModalPromotionsUsedBy(\'' +
                            row.Key +
                            "')\">Used by</a>";
                        str += "</li>";
                        str += " </ul>";
                        str += "</div>";
                        return str;
                    },
                },
            ],
        });

    $("#request-type").select2();
    $("#request-daterange").daterangepicker({
        autoUpdateInput: false,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: "Clear",
            format: "DD-MM-YYYY",
        },
    });

    $("#request-daterange").on("apply.daterangepicker", function (ev, picker) {
        $(this).val(
            picker.startDate.format($dateFormatByUser.toUpperCase()) +
            " - " +
            picker.endDate.format($dateFormatByUser.toUpperCase())
        );
        LoadListMonibackPromotion();
    });

    $("#request-daterange").on("cancel.daterangepicker", function (ev, picker) {
        $(this).val("");
        LoadListMonibackPromotion();
    });

    $("#request-search").keyup(
        searchThrottle(function () {
            LoadListMonibackPromotion();
        })
    );

    $("#request-type").change(function () {
        LoadListMonibackPromotion();
    });

    LoadListMonibackPromotion();
}

function managePromotionTypeStatus(key) {
    $("#tblMonibackPromotions").DataTable().ajax.reload();
    $.ajax({
        type: "POST",
        url: "/Administration/ManagePromotionTypeStatus",
        datatype: "json",
        data: { key: key },
        success: function (refModel) {
            if (refModel.result) {
                cleanBookNotification.success("Change successful! ", "Qbicles");
                LoadListMonibackPromotion();
            } else {
                cleanBookNotification.error(refModel.msg, "Qbicles");
            }
        },
        error: function (error) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
    });
}

//Bulk Deal part
function loadModalBulkAddEdit(promotionKey) {
    $.LoadingOverlay("show");
    var $promodal = $("#bulkdeal-promo-add");
    $promodal.empty();
    $promodal.modal("show");
    $promodal.load(
        "/Administration/loadAddEditBulkDeal",
        { promotionKey: promotionKey },
        function () {
            $("#bulkdeal-promo-add .checkmulti").multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonWidth: "100%",
                maxHeight: 400,
                enableClickableOptGroups: true,
            });
            $("#bulkdeal-promo-add .select2").select2({
                placeholder: "Please select",
            });
            $("#bulkdeal-promo-add .singledateandtime").daterangepicker({
                singleDatePicker: true,
                timePicker: true,
                autoApply: true,
                showDropdowns: true,
                autoUpdateInput: true,
                cancelClass: "btn-danger",
                opens: "left",
                locale: {
                    cancelLabel: "Clear",
                    format: $dateFormatByUser.toUpperCase() + " " + $timeFormatByUser,
                },
            });
            $("#itemdiscount").show();
            $("#bulkdeal-promo-add .chktoggle").bootstrapToggle();
            initFormBulkDealPromotionValidate();
            setTimeout(function () {
                $.LoadingOverlay("hide");
            }, 500);
        }
    );
}

function loadActiveBulkDealPromotions(isHideSpinner) {
    var _paramaters = {
        name: $("#promos-active input[name=search-bulk-deal]").val(),
        daterange: $("#promos-active input[name=bulk-deal-daterange-search]").val(),
        type: $("#promos-active select[name=type]").val(),
    };

    var $content = $("#promos-active .flex-grid-quarters-lg");
    if (!isHideSpinner) $content.LoadingOverlay("show");
    $content.load(
        "/Administration/LoadActiveBulkDealPromotions",
        { filterModel: _paramaters },
        function () {
            $content.LoadingOverlay("hide");
        }
    );
}

function loadSearchItemsForBulkDealPromotions(isHideSpinner) {
    var _paramaters = {
        keyword: $("#promos-active input[name=search-bulk-deal]").val(),
        daterange: $("#promos-active input[name=bulk-deal-daterange-search]").val(),
        type: $("#promos-active select[name=bulk-deal-type]").val(),
    };

    var $content = $("#promos-active .flex-grid-quarters-lg");
    if (!isHideSpinner) $content.LoadingOverlay("show");
    $content.load(
        "/Administration/loadSearchItemsForBulkDealPromotions",
        { filterModel: _paramaters },
        function () {
            $content.LoadingOverlay("hide");
        }
    );
}

function initFormBulkDealPromotionValidate() {
    var $frmAddEditDeal = $("#frmAddEditDeal");

    $frmAddEditDeal.validate({
        ignore: "",
        invalidHandler: function (e, validator) {
            if (validator.errorList.length)
                $(
                    'a[href="#' +
                    jQuery(validator.errorList[0].element)
                        .closest(".tab-pane")
                        .attr("id") +
                    '"]'
                ).tab("show");
        },
    });
    $frmAddEditDeal.submit(function (e) {
        e.preventDefault();

        // run when turn off time range
        setMinTimeFromToTimeBulkDealPromotion();

        if (isBusy) return;
        if ($frmAddEditDeal.valid()) {
            $.LoadingOverlay("show");
            var fileImg = document.getElementById("filefeaturedimg").files;
            var imgobjkey = $("#hdffeaturedimguri").val();
            if (!imgobjkey || (fileImg && fileImg.length > 0)) {
                UploadMediaS3ClientSide("filefeaturedimg").then(function (
                    mediaS3Object
                ) {
                    if (
                        mediaS3Object.objectKey === "no_image" ||
                        mediaS3Object.objectKey === "no-image"
                    ) {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                        return;
                    } else {
                        $("#hdffeaturedimguri").val(mediaS3Object.objectKey);
                        saveBulkDealPromotion();
                    }
                });
            } else saveBulkDealPromotion();
        }
    });
}

function setMinTimeFromToTimeBulkDealPromotion() {
    var isSetTime = $("#chkSpecificTime").prop("checked");
    var fromTimeVal = $("#frmAddEditDeal input[name=FromTime]").val();
    var $totime = $("#frmAddEditDeal input[name=ToTime]");
    if (fromTimeVal && isSetTime) {
        var listTime = fromTimeVal.split(":");
        listTime[1]++;
        listTime[0] = listTime[0].toLocaleString("en-US", {
            minimumIntegerDigits: 2,
            useGrouping: false,
        });
        listTime[1] = listTime[1].toLocaleString("en-US", {
            minimumIntegerDigits: 2,
            useGrouping: false,
        });
        fromTimeVal = listTime[0] + ":" + listTime[1];
        $totime.attr("min", fromTimeVal);
    } else {
        $totime.removeAttr("min");
    }
}

function saveBulkDealPromotion() {
    var frmname = "#frmAddEditDeal";
    var days = $(frmname + " select[name=Days]").val();
    if (!days) days = [];
    var _paramaters = {
        model: {
            PromotionKey: $(frmname + " input[name=PromotionKey]").val(),
            Name: $(frmname + " input[name=Name]").val(),
            Description: $(frmname + " textarea[name=Description]").val(),
            StartDateString: $(frmname + " input[name=StartDate]").val(),
            EndDateString: $(frmname + " input[name=EndDate]").val(),
            DisplayDateString: $("#chkAdvertiseCustome").prop("checked")
                ? $(frmname + " input[name=DisplayDate]").val()
                : $(frmname + " input[name=StartDate]").val(),
            DaysOfweek: $("#chkSpecificTime").prop("checked") ? days.join(",") : [],
            FromTime: $(frmname + " input[name=FromTime]").val(),
            ToTime: $(frmname + " input[name=ToTime]").val(),
            VoucherExpiryDateString: $("#chkExpiryDate").prop("checked")
                ? $("#expiry-date").val()
                : "",
        },
        itemDiscountVoucherInfo: {},
        //orderDiscountVoucherInfo: {}
    };
    //var locIds = $(frmname + ' select[name=Locations]').val();

    //var _locations = [];
    //$.each(locIds, function (index, value) {
    //    _locations.push({ Id: value });
    //});
    var type = $(frmname + " select[name=Type]").val();
    //var plan = $(frmname + ' input[name=Plan]').val();
    _paramaters.model.Type = type;
    //_paramaters.model.PlanType = {
    //    Id: plan
    //};
    _paramaters.featuredImageUri = $("#hdffeaturedimguri").val();

    if (type == "1") {
        _paramaters.itemDiscountVoucherInfo = {
            Id: 0,
            MaxVoucherCount: $(frmname + " input[name=MaxVoucherCount]").val(),
            MaxVoucherCountPerCustomer: $(
                frmname + " input[name=MaxVoucherCountPerCustomer]"
            ).val(),
            TermsAndConditions: $(
                frmname + " textarea[name=TermsAndConditions]"
            ).val(),
            BusinessesTermsAndConditions: $(
                frmname + " textarea[name=BusinessesTermsAndConditions]"
            ).val(),
            IsDraft: "true",
            //Locations: ($('#chkSpecificLocation').prop('checked') ? _locations : []),
            //ItemSKU: $(frmname + ' input[name=hdfItemSKU]').val(),
            ItemDiscount: $(frmname + " input[name=ItemDiscount]").val(),
            //MaxNumberOfItemsPerOrder: $(frmname + ' input[name=MaxNumberOfItemsPerOrder]').val()
        };

        if (!_paramaters.itemDiscountVoucherInfo.MaxVoucherCount)
            //infinite claims
            _paramaters.itemDiscountVoucherInfo.MaxVoucherCount = -1;
        //if (!_paramaters.itemDiscountVoucherInfo.MaxNumberOfItemsPerOrder)//infinite claims
        //    _paramaters.itemDiscountVoucherInfo.MaxNumberOfItemsPerOrder = -1;
    } else if (type == "2") {
        //_paramaters.orderDiscountVoucherInfo = {
        //    Id: 0,
        //    MaxVoucherCount: $(frmname + ' input[name=MaxVoucherCount]').val(),
        //    MaxVoucherCountPerCustomer: $(frmname + ' input[name=MaxVoucherCountPerCustomer]').val(),
        //    TermsAndConditions: $(frmname + ' textarea[name=TermsAndConditions]').val(),
        //    BusinessesTermsAndConditions: $(frmname + ' textarea[name=BusinessesTermsAndConditions]').val(),
        //    IsDraft: true,
        //    //Locations: ($('#chkSpecificLocation').prop('checked') ? _locations : []),
        //    OrderDiscount: $(frmname + ' input[name=OrderDiscount]').val(),
        //    MaxDiscountValue: $(frmname + ' input[name=MaxDiscountValue]').val()
        //};
        //if (!_paramaters.orderDiscountVoucherInfo.MaxVoucherCount)//infinite claims
        //    _paramaters.orderDiscountVoucherInfo.MaxVoucherCount = -1;
        //if (!_paramaters.orderDiscountVoucherInfo.MaxDiscountValue)//infinite claims
        //    _paramaters.orderDiscountVoucherInfo.MaxDiscountValue = -1;
    }
    $.ajax({
        type: "post",
        url: "/Administration/SaveBulkDealPromotion",
        data: _paramaters,
        dataType: "json",
        success: function (response) {
            if (response.result) {
                cleanBookNotification.createSuccess();
                $("#bulkdeal-promo-add").modal("hide");
                LoadFilterToBulkDealPromotions(true);
            } else if (!response.result && response.msg) {
                cleanBookNotification.error(response.msg, "Bulk Deal Creation");
            } else {
                cleanBookNotification.error(
                    _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                    "Bulk Deal Creation"
                );
            }
        },
        error: function (er) {
            cleanBookNotification.error(er.responseText, "Qbicles");
            cleanBookNotification.error(
                _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                "Promotions & vouchers"
            );
        },
    }).always(function () {
        LoadingOverlayEnd();
    });
}
// Event listener for role filter dropdown
$("#subfilter-group").on("change", function () {
    table.draw();
});

// Event listener for search input
$("#search-user-list").on("keyup", function () {
    table.search(this.value).draw();
});

function LoadFilterToBulkDealPromotions(isHideSpinner) {
    var _paramaters = {
        Name: $("#tab-bulkdeal input[name=search-bulk-deal]").val(),
        DateRange: $("#tab-bulkdeal input[name=scheduleddate-bulk-deal]").val(),
        Type: $("#tab-bulkdeal select[name=type-bulk-deal]").val(),
        //status: $('#promos-active select[name=status]').val(),
    };
    var $content = $("#tab-bulkdeal .flex-grid-quarters-lg");
    if (!isHideSpinner) $content.LoadingOverlay("show");
    $content.load(
        "/Administration/ApplyFilterToBulkDealPromotions",
        { bulkDealParameter: _paramaters },
        function () {
            $content.LoadingOverlay("hide");
        }
    );
}

function ApplySearchToBulkDealPromotions1() {
    $("#bulk-deal-list").LoadingOverlay("show");
    var bulkDealParameter = {
        Name: $("#search-bulk-deal").val(),
        Date: $("#bulk-deal-daterange-search").val(),
        Type: $("#bulk-deal-type :selected").val(),
    };
    $.ajax({
        type: "post",
        url: "/Qbicles/ApplyFilterQbicle",
        data: bulkDealParameter,
        dataType: "html",
        success: function (response) {
            if (response !== "") {
                $("#bulk-deal-dash-grid").empty();
                $("#bulk-deal-dash-grid").append(response);
            }
        },
        error: function (er) { },
    }).always(function () {
        $("#bulk-deal-list").LoadingOverlay("hide", true);
    });
}
function ApplySearchToBulkDealPromotions() {
    $("#bulk-deal-list").LoadingOverlay("show");
    var cubeParameter = {
        Open: $checkOpen.is(":checked"),
        Closed: $checkClosed.is(":checked"),
        Name: $nameSearch.val(),
        Peoples: $selectPeople.val(),
        Topics: $selectTopic.val(),
        order: $("#order :selected").val(),
        IsShowHidden: $("#isShowHidden").is(":checked") ? true : false,
    };
    $("#remove-filters").removeAttr("disabled");
    $.ajax({
        type: "post",
        url: "/Qbicles/ApplyFilterQbicle",
        data: cubeParameter,
        dataType: "html",
        success: function (response) {
            if (response !== "") {
                $("##bulk-deal-dash-grid").empty();
                $("##bulk-deal-dash-grid").append(response);
            }
        },
        error: function (er) { },
    }).always(function () {
        $("##bulk-deal-list").LoadingOverlay("hide", true);
    });
}
function ActivateOrDeactiveBulkDeal(key, isHidden) {
    $.ajax({
        type: "post",
        url: "/Qbicles/ShowOrHideBulkDeal",
        data: { key: key, isHidden: !isHidden },
        dataType: "json",
        success: function (res) {
            if (res.result) {
                ApplySearchToBulkDealPromotions();
            } else {
                cleanBookNotification.error(
                    _L("ERROR_MSG_EXCEPTION_SYSTEM"),
                    "Qbicles"
                );
            }
        },
        error: function (er) {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        },
    });
}
function RemoveFilter() {
    $checkOpen.prop("checked", true).trigger("change");
    $checkClosed.prop("checked", false).trigger("change");
    $nameSearch.val("");
    $selectPeople.val("").trigger("change");
    $selectTopic.val("").trigger("change");
    $("#order").val(4).trigger("change");
    $("#isShowHidden").bootstrapToggle("off");
    ApplySearchToBulkDealPromotions();
    $("#remove-filters").attr("disabled", "disabled");
}
function BulkDealSelected(key, module, paramaters) {
    //$.ajax({
    //    type: 'post',
    //    url: '/Commons/BindingQbicleParameter',
    //    dataType: 'json',
    //    data: {
    //        key: key,
    //        ModuleSelected: module
    //    },
    //    success: function (refModel) {
    //        if (refModel.result === true) {
    //            if (typeof paramaters === "undefined")
    //                window.location.href = "/Qbicles/Dashboard";
    //            else
    //                window.location.href = "/Qbicles/Dashboard?" + paramaters;
    //        }
    //    }
    //});
    return;
}
function loadBulkDealPromotions(isHideSpinner) {
    var _paramaters = {
        keyword: $("#promos-active input[name=search]").val(),
        daterange: $("#promos-active input[name=scheduleddate]").val(),
        type: $("#promos-active select[name=type]").val(),
        status: $("#promos-active select[name=status]").val(),
    };

    var $content = $("#promos-active .flex-grid-quarters-lg");
    if (!isHideSpinner) $content.LoadingOverlay("show");
    $content.load(
        "/Administration/LoadBulkDealPromotions",
        { filterModel: _paramaters },
        function () {
            $content.LoadingOverlay("hide");
        }
    );
}

function loadSearchAndAddToBulkDealTab() {
    var url = $("#tab-search-add").attr("href");
    activeTab = url.substring(url.indexOf("#") + 1);
    setTabTrader(tabItemproduct, activeTab);
    var ajaxUri = "/Trader/ListTraderItem";
    $("#tab-search-add").LoadingOverlay("show");
    cleanItemAndProductSubTabs();
    $("#tab-search-add").load(ajaxUri, function () {
        $('.manage-columns input[type="checkbox"]').on("change", function () {
            var table = $("#tb_trader_items").DataTable();
            var column = table.column($(this).attr("data-column"));
            column.visible(!column.visible());
        });
        $("#tab-search-add").LoadingOverlay("hide");
        searchOnTableItems();
    });
}
function searchOnTableSearchAndAdd() {
    var listKey = [];

    var keys = $("#search_dt").val().split(" ");
    if (
        $("#search_dt").val() !== "" &&
        $("#search_dt").val() !== null &&
        keys.length > 0
    ) {
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] !== "") listKey.push(keys[i]);
        }
    }
    $("#tb_trader_items")
        .DataTable()
        .search(listKey.join("|"), true, false, true)
        .draw();
    $("#tb_trader_items").val("");
}

function LoadItemOverviewItemProductForSearchAndAddBulkDeal() {
    const searchTerm = $("#search_dt").val().toLowerCase();
    //if (!searchTerm) {
    //    bulkDealItems = [];
    //    $("#tblSearchAddToBulkDeal tbody").empty();
    //    $('.tblChk').not(this).prop('checked', false);
    //    $('.largerCheckbox').not(this).prop('checked', false);
    //    $("#trLoader").show();

    //    updateSelectedCount();
    //    return;
    //}
    var url = "/Administration/GetItemOverviewItemProductForSearchAndAddBulkDeal";
    console.log(searchTerm);

    //$.ajax({
    //    type: "GET",
    //    url: url,
    //    data: {
    //        keysearch: searchTerm,
    //        start: 0,
    //        take:30,
    //    },
    //    beforeSend: function () {
    //        $("#trLoader").show();
    //    },
    //    success: function (results) {
    //        $("#trLoader").remove();
    //        $("#tblSearchAddToBulkDeal tbody").empty(); // Clear the table before appending new rows
    //        console.log(LoadItemOverviewItemProductForSearchAndAddBulkDeal, { results });
    //        console.log({ apiDoc });

    //        results.forEach((element, index) => {
    //            let dynamicTR = "<tr>";
    //            dynamicTR += "<td> <input type='checkbox' data-id=" + element.Id + " class='largerCheckbox tblChk chk" + index + "' /></td>";
    //            dynamicTR += "<td><img src='" + apiDoc + element.ImageUri + "' alt='Item Image' class='img-thumbnail' style='width:90px; height:90px;'></td>";
    //            dynamicTR += "<td class='itemname'>" + element.ItemName + "</td>";
    //            dynamicTR += "<td class='sku'>" + element.SKU + "</td>";
    //            dynamicTR += "<td class='barcode'>" + element.Barcode + "</td>";
    //            dynamicTR += "<td class='business'>" + element.Business.DomainName + "</td>";
    //            dynamicTR += "<td class='location'>" + element.Location + "</td>";
    //            dynamicTR += "<td class='hidden-column'>" + element.Id + "</td>";
    //            dynamicTR += "<td class='hidden-column'>" + element.Business.DomainId + "</td>";
    //            dynamicTR += " </tr>";
    //            $("#tblSearchAddToBulkDeal tbody").append(dynamicTR);
    //        });
    //    }
    //});

    bdlTable = $("#tblSearchAddToBulkDeal").DataTable({
        processing: true,
        serverSide: true,
        //searching: false,
        ajax: {
            url: url, // Update this to your actual controller/action URL
            type: "GET",
            data: function (d) {
                d.searchValue = searchTerm; // Custom search value
                //d.startDate = $('#startDate').val(); // Custom parameter
                //d.endDate = $('#endDate').val();     // Custom parameter
                //d.start = d.start;                   // DataTables automatically includes `start`
                //d.length = d.length;                 // DataTables automatically includes `length`
            },
            beforeSend: function (jqXHR, settings) {
                $("#trLoader").show();
            },
            dataSrc: function (json) {
                $("#trLoader").remove(); // Hide the loader when data is received
                let bulkDealItems = json.data;
                console.log({ bulkDealItems });
                return bulkDealItems; // Return the data array from the response
            },
            complete: function () {
                $("#trLoader").remove();
            },
        },
        columns: [
            {
                data: "id",
                render: function (data, type, row) {
                    return (
                        '<input type="checkbox" class="largerCheckbox tblChk" data-id="' +
                        data +
                        '">'
                    );
                },
            },
            {
                data: "ImageUri",
                orderable: false,
                render: function (data, type, row) {
                    return (
                        '<img src="' +
                        apiDoc +
                        data +
                        '" alt="Item Image" style="width:50px;height:50px;border-radius:50%;" />'
                    );
                },
            },
            { data: "ItemName" },
            { data: "SKU" },
            { data: "Barcode" },
            { data: "Business" },
            { data: "Location" },
            { data: "ItemId", visible: false }, // Hidden UserId column
            { data: "DomainId", visible: false }, // Hidden UserId column
        ],
    });
    //table.draw(); // Redraw the DataTable with the new search value
}
function updateSelectedCount() {
    $("#selectedElement").text(bulkDealItems.length); // Update the count display
    console.log({ bulkDealItems }); // For debugging: logs the array of selected rows
}
function ClearSelectedItems() {
    bulkDealItems.length = 0;
    $("#addToBulkDeal").hide();
    $("#clearSelectedItems").hide();
    var isChecked = $("#chkAll").prop("checked");
    if (isChecked) {
        $("#chkAll").prop("checked", false).trigger("change");
    }
    else {
        $("#chkAll").prop("checked", true).trigger("change");
        $("#chkAll").prop("checked", false).trigger("change");
    }
    $("#selectedElement").text(bulkDealItems.length);
}
function AddToBulkDealPromotion() {
    $.LoadingOverlay("show");
    var $promodal = $("#add-to-bulkdeal");
    $promodal.empty();
    $promodal.modal("show");

    $promodal.load("/Administration/loadAddToBulkDeal", function () {
        setTimeout(function () {
            $.LoadingOverlay("hide");
        }, 500);
    });
}
function InitAddToBulkDealPromotionTable() {
    console.log("I was here at AddToBulkDealPromotion");
    $("#tblBulkDealCreation").DataTable({
        data: bulkDealItems,
        columns: [
            {
                data: "ImageUri",
                orderable: false,
                render: function (data, type, row) {
                    return (
                        '<img src="' +
                        apiDoc +
                        data +
                        '" alt="Item Image" style="width:50px;height:50px;border-radius:50%;" />'
                    );
                },
            },
            { data: "ItemName" },
            { data: "SKU" },
            { data: "Barcode" },
            { data: "Business" },
            { data: "Location" },
            { data: "ItemId", visible: false }, // Hidden UserId column
            { data: "DomainId", visible: false }, // Hidden UserId column
        ],
    });
    $("#bulkDealSelect .select2").select2({
        placeholder: "Please select",
    });
    $('#bulkDealSelect .select2').change(function () {
        // Get the selected option
        var selectedOption = $(this).find('option:selected');

        // Extract the value and other attributes
        var selectedId = selectedOption.val(); // Gets the value of the selected option (business.Id)
        var selectedName = selectedOption.data('Name'); // Gets the Name from the data attribute
        var description = selectedOption.data('Description'); // Example of another data attribute

        // You now have access to the selected object's properties
        console.log('Selected ID:', selectedId);
        console.log('Selected Name:', selectedName);
        console.log('Other Property:', description);

        // You can then use these values as needed in your code
    });
}
function CreateBulkPromotions() {
    console.log("Items and Business", { bulkDealItems })
}
function updateBulkDealElements() {
    if (bulkDealItems.length) {
        $("#addToBulkDeal").show();
        $("#clearSelectedItems").show();
    } else {
        $("#addToBulkDeal").hide();
        $("#clearSelectedItems").hide();
    }
    $("#selectedElement").text(bulkDealItems.length);
}
$(document).ready(function () {
    // Function to update the displayed count of selected search and add items bulk deal

    $(".checkmulti").multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        buttonWidth: "100%",
        maxHeight: 400,
        enableClickableOptGroups: true,
    });

    $(".select2avatarDomain").select2({
        placeholder: "Please select",
        templateResult: formatOptions,
        templateSelection: formatSelected,
        dropdownCssClass: "withdrop",
    });

    $(".select2avatar").select2({
        placeholder: "Please select",
        templateResult: formatOptions,
        templateSelection: formatSelected,
    });

    $("#search_dt").keyup(
        delay(function () {
            LoadItemOverviewItemProductForSearchAndAddBulkDeal();
        }, 1000)
    );

    $('a[data-toggle="tab"]').on("shown.bs.tab", function (e) {
        var target = $(e.target).attr("data-target"); // Activated tab
        console.log("Switched to tab: " + target);

        // Your custom function based on the active tab
        if (target === "#tab-bulkdeal") {
            console.log("Bulk Deal tab is active");

            $("#search-bulk-deal").keyup(
                searchThrottle(function () {
                    LoadFilterToBulkDealPromotions(true);
                })
            );

            $("#bulk-deal-type").change(function () {
                LoadFilterToBulkDealPromotions(true);
            });

            $(
                "#bulk-deal-daterange-search input[name=scheduleddate-bulk-deal]"
            ).change(function () {
                LoadFilterToBulkDealPromotions(true);
            });
        } else if (target === "#tab-search-add") {
            // LoadItemOverviewItemProductForSearchAndAddBulkDeal()
            $("#clearSelectedItems").hide();
            $("#addToBulkDeal").hide();
            var bdlTable = $("#tblSearchAddToBulkDeal").DataTable({
                lengthMenu: [25, 50, 100],
                processing: true,
                serverSide: true,
                responsive: true,
                destroy: true,
                //searching: false,
                ajax: {
                    url: "/Administration/GetItemOverviewItemProductForSearchAndAddBulkDeal", // Update this to your actual controller/action URL
                    type: "GET",
                    data: function (d) {
                        d.searchValue = $("#search_add_dt").val().toLowerCase(); // Custom search value
                        //d.startDate = $('#startDate').val(); // Custom parameter
                        //d.endDate = $('#endDate').val();     // Custom parameter
                        //d.start = d.start;                   // DataTables automatically includes `start`
                        //d.length = d.length;                 // DataTables automatically includes `length`
                    },
                    beforeSend: function (jqXHR, settings) {
                        $("#trLoader").show();
                    },
                    dataSrc: function (json) {
                        $("#trLoader").remove(); // Hide the loader when data is received
                        let bulkDealItems = json.data;
                        console.log({ bulkDealItems });
                        return bulkDealItems; // Return the data array from the response
                    },
                    complete: function () {
                        $("#trLoader").remove();
                    },
                },
                columns: [
                    {
                        data: "id",
                        render: function (data, type, row) {
                            return (
                                '<input type="checkbox" class="largerCheckbox tblChk" data-id="' +
                                data +
                                '">'
                            );
                        },
                    },
                    {
                        data: "ImageUri",
                        orderable: false,
                        render: function (data, type, row) {
                            return (
                                '<img src="' +
                                apiDoc +
                                data +
                                '" alt="Item Image" style="width:50px;height:50px;border-radius:50%;" />'
                            );
                        },
                    },
                    { data: "ItemName" },
                    { data: "SKU" },
                    { data: "Barcode" },
                    { data: "Business" },
                    { data: "Location" },
                    { data: "ItemId", visible: false }, // Hidden UserId column
                    { data: "DomainId", visible: false }, // Hidden UserId column
                ],
            });

            // Checkbox change event handling
            $("#tblSearchAddToBulkDeal").on("change", ".tblChk", function () {
                var rowData = bdlTable.row($(this).closest("tr")).data(); // Get row data as a JavaScript object

                if ($(this).prop("checked")) {
                    bulkDealItems.push(rowData);
                } else {
                    var isChecked = $("#chkAll").prop("checked");
                    if (isChecked) {
                        $("#chkAll").prop("checked", false);
                    }
                    bulkDealItems = bulkDealItems.filter(function (item) {
                        return item.Id !== rowData.Id; // Remove unchecked row from selectedRows
                    });
                }

                // Update the selected element count
                updateBulkDealElements()
            });

            // Select/Deselect all checkboxes
            $("#chkAll").change(function () {
                var isChecked = $(this).prop("checked");
                $(".tblChk").prop("checked", isChecked).trigger("change");
            });

            // Bind custom search input to DataTables search
            $("#search_add_dt").keyup(
                delay(function () {
                    bdlTable.draw(); // Redraw the DataTable with the new search value
                }, 1000)
            );
        }
    });
    initMonibackPromotionTable();
    initMonibackRankPromotionTable();
    initDragDrop();
});