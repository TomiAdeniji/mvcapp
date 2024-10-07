var isBusy = false;
$(document).ready(function () {
    $(".qbicle-detail .checkmulti").multiselect({
        includeSelectAllOption: false,
        enableFiltering: false,
        buttonWidth: '100%',
        maxHeight: 400,
        enableClickableOptGroups: true
    });
    $('#collapse-unallocated .manage-columns input[type="checkbox"]').change(function () {
        var table = $("#tbl-unallocated").DataTable();
        var column = table.column($(this).attr("data-column"));
        column.visible(!column.visible());
    });
    $("#frmReorder select[name=ExcludeGroupId]").select2({
        placeholder: "Please select",
        allowClear: true
    });
    initCreateGroupModal();
    initFormReorder();
    initDateRange();
    getCurrencySettings();
    initUnallocated();
});
function getCurrencySettings() {
    $.ajax({
        url: "/Qbicles/GetCurrencySettings",
        type: "get",
        async: false,
        success: function (data) {
            if (data)
                currencySetting = data;
            else
                currencySetting = {
                    CurrencySymbol: '',
                    SymbolDisplay: 0,
                    DecimalPlace: 2
                };
        },
        error: function () {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
        }
    });
}
function setBgHeading(el) {
    if ($(el).hasClass('collapsed')) {
        $(el).attr('aria-expanded', true);
    } else {
        $(el).attr('aria-expanded', false);
    }
}
function enablingButton(el) {
    if ($(el).val()) {
        $('#tblcontact-applyall').prop('disabled', false);
    } else {
        $('#tblcontact-applyall').prop('disabled', true);
    }
}
function enablingButtonApplyAllGroup(el, groupid) {
    if ($(el).val()) {
        $('#btnApplyAllGroup' + groupid).prop('disabled', false);
    } else {
        $('#btnApplyAllGroup' + groupid).prop('disabled', true);
    }
}
function applyAllUnallocated() {
    var pricontact = $('#slpricontact-applyall').val();
    if (pricontact) {
        $('#tbl-unallocated .slprimarycontact').val(pricontact).change();
    }
}
function initCreateGroupModal() {
    var $frmCreateGroupReorder = $('#frmCreateGroupReorder');
    $frmCreateGroupReorder.validate({
        ignore: "",
        rules: {
            primarycontactforgroup: {
                required: true
            }
        }
    });
    $frmCreateGroupReorder.submit(function (e) {
        e.preventDefault();
        if ($frmCreateGroupReorder.valid()) {
            $.LoadingOverlay("show");
            var data = getDataUnallocated();
            //Validate Dimensions for TraderItem
            var isValid = true;
            $.each(data.Items, function (index, value) {
                if (value.Dimensions.length == 0) {
                    cleanBookNotification.error(_L('ERROR_MSG_54', [(index + 1)]), "Trader");
                    $('#dimensions' + value.Id).select2('open');
                    $('#primary-contact-modal').modal('hide');
                    LoadingOverlayEnd();
                    isValid = false;
                    return;
                }
            });
            if (!isValid)
                return;
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: JSON.stringify(data),
                dataType: 'json',
                contentType: 'application/json',
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $('#primary-contact-modal').modal('hide');
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                        window.location.reload();
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Trader");
                    } else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                }
            });
        } else {
            return;
        }
    });
}
function getDataUnallocated() {
    var mt = $('#collapse-unallocated select[name=delivery]').val();
    var reorderGroup = {
        ReorderId: $('#reorderId').val(),
        PrimaryContactId: $('#primarycontactforgroup').val(),
        DeliveryMethod: mt ? mt : null,
        DaysToLastBasis: $('#collapse-unallocated select[name=daytolastbasis]').val(),
        DaysToLast: $('#collapse-unallocated input[name=daytolast]').val(),
        Days2Last: $('#collapse-unallocated input[name=day2last]').val(),
        Items: []
    };
    $('#tbl-unallocated').DataTable().rows().every(function (index, element) {
        var row = $(this.node());
        var checked = $(row).find("input[type=checkbox]");
        if ($(checked).prop('checked')) {
            var id = $(checked).val();
            var $table = $('#tbl-unallocated').DataTable();
            var rowData = $table.row(row).data();
            var unit = $(row).find("#unit" + id).val();
            if (rowData.unit) {
                unit = rowData.unit;
            }
            var costPerUnit = $(row).find("#costPerUnit" + id).val();
            if (rowData.costPerUnit) {
                costPerUnit = rowData.costPerUnit;
            }
            var discount = $(row).find("#discount" + id).val();
            if (rowData.discount) {
                discount = rowData.discount;
            }
            var dimensions = $(row).find("#dimensions" + id).val();
            if (rowData.dimensions) {
                dimensions = rowData.dimensions;
            }
            var quantity = $(row).find("#quantity" + id).val();
            if (rowData.quantity) {
                quantity = rowData.quantity;
            }
            var item = {
                Id: id,
                Dimensions: dimensions ? dimensions : [],
                PrimaryContactId: reorderGroup.PrimaryContactId,
                UnitId: unit,
                Quantity: quantity ? quantity:0,
                CostPerUnit: costPerUnit ? costPerUnit : 0,
                Discount: discount ? discount : 0
            };
            reorderGroup.Items.push(item);
        }
    });
    return reorderGroup;
}
function chkAll(elm) {
    var isChecked = $(elm).prop("checked");
    var rows = $('#tbl-unallocated').DataTable().rows({ 'search': 'applied' }).nodes();
    $('input[type="checkbox"]', rows).prop('checked', isChecked);
    checkShowWithselected();
}
function checkShowWithselected() {
    var countChecked = $('#tbl-unallocated').DataTable().$('input[type="checkbox"]:checked').length;
    if (countChecked > 0)
        $('.withselected3').show();
    else
        $('.withselected3').hide();
}
function initFormCalculateQuantities(groupid) {
    var $frmgroup = $('#frmgroup' + groupid);
    $frmgroup.validate({
        ignore: "",
        rules: {
            primarycontact: {
                required: true
            },
            Delivery: {
                required: true
            },
            DaysToLastBasis: {
                required: true
            },
            DaysToLast: {
                required: true
            }
        }
    });
    $frmgroup.submit(function (e) {
        e.preventDefault();
        if ($frmgroup.valid()) {
            $.LoadingOverlay("show");
            var reorderGroup = {
                Id: groupid,
                ReorderId: $('#reorderId').val(),
                PrimaryContactId: $('#group-primary-contact' + groupid).val(),
                DeliveryMethod: $('#Delivery' + groupid).val(),
                DaysToLastBasis: $('#DaysToLastBasis' + groupid).val(),
                DaysToLast: $('#DaysToLast' + groupid).val(),
                Days2Last: $('#filter_daterange' + groupid).val(),
                Items: []
            };
            var isValid = true;
            $('#tbgroup' + groupid).DataTable().rows().every(function (index, element) {
                var row = $(this.node());
                var $table = $('#tbgroup' + groupid).DataTable();
                var rowData = $table.row(row).data();
                var checked = $(row).find("input[type=checkbox]");
                if (rowData.isReorder) {
                    checked = rowData.isReorder;//fix errors not get value when datatable paging
                }
                if ($(checked).prop('checked')) {
                    var id = $(checked).val();
                    var pricontact = $(row).find("#primary" + id);
                    var unit = $(row).find("#unit" + id).val();
                    if (rowData.unit) {
                        unit = rowData.unit;
                    }
                    var costPerUnit = $(row).find("#costPerUnit" + id).val();
                    if (rowData.costPerUnit) {
                        costPerUnit = rowData.costPerUnit;
                    }
                    var discount = $(row).find("#discount" + id).val();
                    if (rowData.discount) {
                        discount = rowData.discount;
                    }
                    var isDisabled = $(row).find("#hdfIsDisabled" + id).val();
                    if (rowData.isDisabled) {
                        isDisabled = rowData.isDisabled;
                    }
                    var dimensions = $(row).find("#dimensions" + id).val();
                    if (rowData.dimensions) {
                        dimensions = rowData.dimensions;
                    }
                    var quantity = $(row).find("#quantity" + id).val();
                    if (rowData.quantity) {
                        quantity = rowData.quantity;
                    }
                    if (!dimensions) {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L('ERROR_MSG_54', [(index + 1)]), "Trader");
                        $($(row).find("#dimensions" + id)).select2('open');
                        isValid = false;
                        return false;
                    }
                    var item = {
                        Id: id,
                        PrimaryContactId: $(pricontact).val(),
                        Dimensions: dimensions ? dimensions : [],
                        UnitId: unit,
                        CostPerUnit: costPerUnit ? costPerUnit:0,
                        Discount: discount ? discount:0,
                        Quantity: quantity ? quantity:0,
                        IsForReorder: true,
                        IsDisabled: isDisabled
                    };
                    reorderGroup.Items.push(item);
                }
            });
            if (!isValid)
                return;
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: JSON.stringify(reorderGroup),
                dataType: 'json',
                contentType: 'application/json',
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    if (data.result) {
                        $("#profile-group" + groupid).load("/TraderInventory/LoadReorderProfileGroup?groupid=" + groupid, function () {
                            $("#profile-group" + groupid + ' .panel-heading h4.panel-title a').click();
                            initDateRange();
                            initControlElm(groupid);
                        });
                        $('.countreorderitems').text(data.Object.countRedorderItems);
                        $('.totalreorder').text(data.Object.total);
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Trader");
                    }
                    LoadingOverlayEnd();
                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                }
            });
        } else {
            return;
        }
    });
    //Init Table
    $('#tbgroup' + groupid).DataTable({
        "destroy": true,
        "responsive": true,
        "drawCallback": function (settings) {
            $(".trackInput" + groupid).on("change", function () {
                var $elm = $(this);
                var $row = $elm.parents("tr");
                var $table = $('#tbgroup' + groupid).DataTable();
                var rowData = $table.row($row).data();
                var checked = $($row).find("input[type=checkbox]");
                var id = $(checked).val();
                var $unit = $($row).find("#unit" + id);
                var quantityOfBaseunit = $unit.find('option[value="' + $unit.val() + '"]').data("quantityofbaseunit");
                var costPerUnit = parseFloat($($row).find("#costPerUnit" + id).val());
                costPerUnit = costPerUnit ?costPerUnit: 0;
                var discount = parseFloat($($row).find("#discount" + id).val());
                discount = discount ? discount : 0;
                var quantity = parseFloat($($row).find("#quantity" + id).val());
                quantity = quantity ? quantity : 0;
                var staxrates = $($row).find("#taxname" + id).val();
                var sumtaxrates = $($row).find("#taxrate" + id).val();
                if (staxrates) {
                    $table.cell('.cell-taxrates' + id).data(showTaxRatesDetail(costPerUnit,quantity,discount, staxrates));
                } else {
                    $table.cell('.cell-taxrates' + id).data('<ul class="unstyled"><li>Tax free</li></ul>');
                }
                var total = parseFloat(costPerUnit) * parseFloat(quantityOfBaseunit ? quantityOfBaseunit : 1) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + parseFloat(sumtaxrates));
                $table.cell('.cell-totalcost' + id).data(toCurrencySymbol(total,false));
                switch ($elm.attr("name")) {
                    case "quantity":
                        rowData.quantity = $elm.val();
                        break;
                    case "discount":
                        rowData.discount = $elm.val();
                        break;
                    case "dimensions":
                        rowData.dimensions = $elm.val();
                        break;
                    case "unit":
                        rowData.unit = $elm.val();
                        break;
                    case "isDisabled":
                        rowData.isDisabled = $elm.val();
                        break;
                    case "isReorder":
                        rowData.isReorder = $elm.prop('checked');
                        break;
                    default:
                        rowData.costPerUnit = $elm.val();
                        break;
                }
            })
        }
    });
    $('#tbgroup' + groupid).on('draw.dt', function () {
        initControlElm(groupid);
    });
};
function setTypeSubmit(type) {
    $('#typesubmit').val(type);
    $('#frmReorder').submit();
}
function initFormReorder() {
    var $frmReorder = $('#frmReorder');
    $frmReorder.validate({
        ignore: "",
        rules: {
            WorkgroupId: {
                required: true
            },
            Delivery: {
                required: true
            },
        }
    });
    $frmReorder.submit(function (e) {
        e.preventDefault();
        if ($frmReorder.valid()) {
            $.LoadingOverlay("show");
            var data = {
                Id: $('#reorderId').val(),
                WorkgroupId: $('#frmReorder select[name=WorkgroupId]').val(),
                Delivery: $('#frmReorder select[name=Delivery]').val(),
                ExcludeGroupId: $('#frmReorder select[name=ExcludeGroupId]').val() ? $('#frmReorder select[name=ExcludeGroupId]').val() : null,
                TypeSubmit: $('#typesubmit').val()
            };
            $.ajax({
                type: this.method,
                cache: false,
                url: this.action,
                data: JSON.stringify(data),
                dataType: 'json',
                contentType: 'application/json',
                beforeSend: function (xhr) {
                    isBusy = true;
                },
                success: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    if (data.result) {
                        cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                        setTabTrader('ItemsProducts', 'inventory-tab');
                        setLocalStorage('sub-inventory-tab', 'inv-reorders');
                        window.location.href = '/Trader/AppTrader';
                    } else if (!data.result && data.msg) {
                        cleanBookNotification.error(_L(data.msg), "Trader");
                    }

                },
                error: function (data) {
                    isBusy = false;
                    LoadingOverlayEnd();
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
                }
            });
        } else {
            return;
        }
    });
}
function excludeProductGroup() {
    var productgroupid = $('#frmReorder select[name=ExcludeGroupId]').val();
    var reorderid = $('#reorderId').val();
    $.post("/TraderInventory/ExcludeReorderItems", { productGroupId: (productgroupid ? productgroupid : 0), reorderId: reorderid }, function (response) {
        if (response.result) {
            window.location.reload();
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function initDateRange() {
    $('.datetimerange').daterangepicker({
        autoUpdateInput: true,
        cancelClass: "btn-danger",
        opens: "right",
        locale: {
            cancelLabel: 'Clear',
            format: $dateFormatByUser.toUpperCase()
        },
        timePicker: false
    });
    $('.datetimerange').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        //$('.datetimerange').html(picker.startDate.format($dateFormatByUser.toUpperCase()) + '-' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        // action here
    });
    $('.datetimerange').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val(null);
        //$('.datetimerange').html('full history');
        // action here
    });
}
function uncheckAllGroup(groupid) {
    $.post("/TraderInventory/UncheckAllReorder", { groupid: groupid }, function (response) {
        if (response.result) {
            $("#profile-group" + groupid).load("/TraderInventory/LoadReorderProfileGroup?groupid=" + groupid, function () {
                $("#profile-group" + groupid + ' .panel-heading h4.panel-title a').click();
                initDateRange();
                initControlElm(groupid);
            });
            $('.countreorderitems').text(response.Object.countRedorderItems);
            $('.totalreorder').text(toCurrencySymbol(response.Object.total, true));
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
        } else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function changeContact(groupid) {
    var contactId = $('#group-primary-contact' + groupid).val();
    $.post("/TraderInventory/ChangeContact", { groupid: groupid, contactid: contactId }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            $("#profile-group" + groupid).load("/TraderInventory/LoadReorderProfileGroup?groupid=" + groupid, function () {
                $("#profile-group" + groupid + ' .panel-heading h4.panel-title a').click();
                initDateRange();
                initControlElm(groupid);
            });
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(response.msg, "Trader");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function moveContacts(groupid) {
    var reorderGroup = {
        Id: groupid,
        ReorderId: $('#reorderId').val(),
        Items: []
    };
    $('#tbgroup' + groupid).DataTable().rows().every(function (index, element) {
        var row = $(this.node());
        var checked = $(row).find("input[type=checkbox]");
        var id = $(checked).val();
        var pricontact = $(row).find("#primary" + id);
        var item = {
            Id: id,
            PrimaryContactId: $(pricontact).val()
        };
        reorderGroup.Items.push(item);
    });
    $.LoadingOverlay("show");
    $.ajax({
        type: "post",
        cache: false,
        url: "/TraderInventory/MoveContacts",
        data: JSON.stringify(reorderGroup),
        dataType: 'json',
        contentType: 'application/json',
        beforeSend: function (xhr) {
            isBusy = true;
        },
        success: function (data) {
            isBusy = false;
            if (data.result) {
                cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
                window.location.reload();
            } else if (!data.result && data.msg) {
                cleanBookNotification.error(_L(data.msg), "Trader");
            } else {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
            }
            LoadingOverlayEnd();
        },
        error: function (data) {
            isBusy = false;
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function changeDelivery() {
    var deliveryid = $('#frmReorder select[name=Delivery]').val();
    $.post("/TraderInventory/ChangeDelivery", { reorderid: $('#reorderId').val(), DeliveryMethod: deliveryid }, function (response) {
        if (response.result) {
            cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Trader");
            $('#lstPrimaryContact select[name=Delivery]').val(deliveryid).change();
        } else if (!response.result && response.msg) {
            cleanBookNotification.error(response.msg, "Trader");
        }
        else {
            cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Trader");
        }
    });
}
function setIsReOrderForItem(itemId, isForReorder, groupid) {
    $.post("/TraderInventory/SetIsReOrderForItem", { itemId: itemId, isReOrder: isForReorder }, function (response) {
        if (response.result && response.Object != null) {
            $('#groupinfo' + groupid).html(response.Object.countGroupItems + " items &nbsp; &nbsp; " + toCurrencySymbol(response.Object.groupPriceTotal, true));
            $('.countreorderitems').text(response.Object.countRedorderItems);
            $('.totalreorder').text(toCurrencySymbol(response.Object.total,true));
        }
    });
}
function initUnallocated() {
    $('#tbl-unallocated').DataTable({
        "destroy": true,
        "drawCallback": function (settings) {
            $(".trackInputUnl").on("change", function () {
                var $elm = $(this);
                var $row = $elm.parents("tr");
                var $table = $('#tbl-unallocated').DataTable();
                var rowData = $table.row($row).data();
                var checked = $($row).find("input[type=checkbox]");
                var id = $(checked).val();
                var costPerUnit = parseFloat($($row).find("#costPerUnit" + id).val());
                costPerUnit = costPerUnit ? costPerUnit : 0;
                var discount = parseFloat($($row).find("#discount" + id).val());
                discount = discount ? discount : 0;
                var quantity = parseFloat($($row).find("#quantity" + id).val());
                quantity = quantity ? quantity : 0;
                var staxrates = $($row).find("#taxname" + id).val();
                var sumtaxrates = $($row).find("#taxrate" + id).val();
                if (staxrates) {
                    
                    $table.cell('.cell-taxrates' + id).data(showTaxRatesDetail(costPerUnit, quantity, discount, staxrates));
                } else {
                    
                    $table.cell('.cell-taxrates' + id).data('<ul class="unstyled"><li>Tax free</li></ul>');
                }
                var total = parseFloat(costPerUnit) * parseFloat(quantity) * (1 - (parseFloat(discount) / 100)) * (1 + parseFloat(sumtaxrates));
                $table.cell('.cell-totalcost' + id).data(toCurrencySymbol(total, false));
                switch ($elm.attr("name")) {
                    case "quantity":
                        rowData.quantity = $elm.val();
                        break;
                    case "discount":
                        rowData.discount = $elm.val();
                        break;
                    case "dimensions":
                        rowData.dimensions = $elm.val();
                        break;
                    case "unit":
                        rowData.unit = $elm.val();
                        break;
                    default:
                        rowData.costPerUnit = $elm.val();
                        break;
                }
            })
        }
    });
    $('#tbl-unallocated').on('draw.dt', function () {
        $("#tbl-unallocated .select2").select2({
            placeholder: "Please select"
        });
        $('#tbl-unallocated input[data-toggle=toggle]').bootstrapToggle();
    });
}
function initControlElm(groupid) {
    $("#profile-group" + groupid + " div.well .select2").select2({
        placeholder: "Please select"
    });
    $("#tbgroup" + groupid + " .select2").select2({
        placeholder: "Please select"
    });
    $('#profile-group' + groupid + ' input[data-toggle=toggle]').bootstrapToggle();
}