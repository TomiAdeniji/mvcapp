var $saleReturnId = $("#sale-return-id").val();

var $returnItemId = 0;
function ConfirmDeleteReturnItem(id) {
    $returnItemId = id;
    $("#name-delete").text($("#return-item-name-" + $returnItemId).text());
    $("#confirm-delete").modal('show');
}


function CancelDelete() {
    $('#confirm-delete').modal('hide');
};
function DeleteReturnItem() {

    
    $.LoadingOverlay("show");

    CheckStatus($saleReturnId, 'SaleReturn').then(function (res) {

        if (res.result && res.Object !== "Approved" && res.Object !== "Denied" && res.Object !== "Discarded") {
            $.ajax({
                type: "delete",
                url: "/TraderSalesReturn/DeleteReturnItem",
                datatype: "json",
                data: {
                    returnItem: { Id: $returnItemId }
                },
                success: function (refModel) {

                    if (refModel.result) {
                        $('#confirm-delete').modal('hide');
                        var $trDelete = $("#table-tr-return-item-" + $returnItemId);
                        $($trDelete).css("background-color", "#FF3700");
                        $($trDelete).fadeOut(500,
                            function () {
                                var table = $("#sale-return-manager-table").DataTable();
                                table.destroy();
                                $("#sale-return-manager-table tbody tr#table-tr-return-item-" + $returnItemId).remove();
                                $("#sale-return-manager-table").DataTable().draw();
                            });
                        cleanBookNotification.removeSuccess();
                    }
                    else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        } else {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () {
                location.reload();
            }, 1000);
       
        }
    });

}


var strValueOld = "";
var strValueChanged = "";
function OnFocusOutReturnQty(id, value, maxValue) {
   
    strValueChanged = value;
    var returnQty = parseFloat(value);
    var saleQty = parseFloat(maxValue);
    if (returnQty < 0) {
        $("return-item-row-qty-" + id).val(saleQty);
        cleanBookNotification.error(_L("ERROR_MSG_650"), "Qbicles");
        return;
    }
    if (strValueChanged !== strValueOld) {
        if (returnQty > saleQty) {
            cleanBookNotification.error(_L("ERROR_MSG_651"), "Qbicles");
            $("return-item-row-qty-" + id).val(saleQty);

            return;
        }
    }
    UpdateReturnQuantity(id, value);
};

function OnFocusReturnQty(value) {
    strValueOld = value;
};

function UpdateReturnQuantity(id, returnValue) {
    
    $.LoadingOverlay("show");
    CheckStatus($saleReturnId, 'SaleReturn').then(function (res) {
        
        if (res.result && res.Object !== "Approved" && res.Object !== "Denied" && res.Object !== "Discarded") {
            $.ajax({
                type: "post",
                url: "/TraderSalesReturn/UpdateReturnItemQuantity",
                datatype: "json",
                data: {
                    returnItem: { Id: id, ReturnQuantity: returnValue }
                },
                success: function (refModel) {

                    if (refModel.result) {

                        cleanBookNotification.updateSuccess();
                    }
                    else {
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        } else {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () {
                location.reload();
            }, 1000);
        }
    });

};


var strValueCreditOld = "";
var strValueCreditChanged = "";
function OnFocusOutReturnCredit(id, value) {
    strValueCreditChanged = value;
    
    if (parseFloat(value) < 0) {
        cleanBookNotification.error(_L("ERROR_MSG_652"), "Qbicles");
        $("return-item-row-credit-" + id).val(0);
        return;
    }
    if (strValueCreditChanged !== strValueCreditOld)
        UpdateReturnCredit(id, value);
};

function OnFocusReturnCredit(value) {
    strValueCreditOld = value;
};

function UpdateReturnCredit(id, creditValue) {
    $.LoadingOverlay("show");
    CheckStatus($saleReturnId, 'SaleReturn').then(function (res) {

        if (res.result && res.Object !== "Approved" && res.Object !== "Denied" && res.Object !== "Discarded") {

            $.ajax({
                type: "post",
                url: "/TraderSalesReturn/UpdateReturnItemCredit",
                datatype: "json",
                data: {
                    returnItem: { Id: id, Credit: creditValue }
                },
                success: function (refModel) {

                    if (refModel.result) {
                        cleanBookNotification.updateSuccess();
                    }
                    else {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        } else {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () {
                location.reload();
            }, 1000);
        }
    });

};

function UpdateIsReturnToInventory(isReturn, id) {

    $.LoadingOverlay("show");
    CheckStatus($saleReturnId, 'SaleReturn').then(function (res) {
        if (res.result && res.Object !== "Approved" && res.Object !== "Denied" && res.Object !== "Discarded") {

            $.ajax({
                type: "post",
                url: "/TraderSalesReturn/UpdateReturnItemIsReturnedToInventory",
                datatype: "json",
                data: {
                    returnItem: { Id: id, IsReturnedToInventory: isReturn }
                },
                success: function (refModel) {
                    if (refModel.result) {
                        cleanBookNotification.updateSuccess();
                    }
                    else {
                        LoadingOverlayEnd();
                        cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                    }
                },
                error: function (xhr) {
                    cleanBookNotification.error(xhr.responseText, "Qbicles");
                }
            }).always(function () {
                LoadingOverlayEnd();
            });
        } else {
            LoadingOverlayEnd();
            cleanBookNotification.error(_L("ERROR_MSG_272"), "Qbicles");
            setTimeout(function () {
                location.reload();
            }, 1000);
        }
    });

};