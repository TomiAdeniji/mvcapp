function SelectedChangeBu(id) {
    if ($transferRequirement === "p2p" || $editMode !== "")
        SetTransferCost(id);
    else if ($transferRequirement === "goods_in")
        SetTransferCostPurchase(id);
    else if ($transferRequirement === "goods_out")
        SetTransferCostSale(id);
};

function SetTransferCost(id) {
    if ($editMode === "Sale") {
        SetTransferCostSale(id);
    } else if ($editMode === "Purchase") {
        SetTransferCostPurchase(id);
    }
};

function SetTransferCostSale(id) {
    calculationTransferQuantity(id, '.total_sale_');
};

function SetTransferCostPurchase(id) {
    calculationTransferQuantity(id, '.total_purchase_');
};


function calculationTransferQuantity(id, type) {
    var remaining = 0;
    var totalPurchase = parseFloat($(type + id).val());
    var quantyityTransfer = parseFloat($('.transfer_tr_' + id + ' td.transfer_td_tran_quan input').val());

    if (quantyityTransfer < 0) {
        quantyityTransfer = 0;
        $('.transfer_tr_' + id + ' td.transfer_td_tran_quan input').val(quantyityTransfer);
    }

    var selectedUnit = $('.transfer_tr_' + id + ' td select.transfer_td_tran_unit').val().split('|');

    var quantityOfBaseunit = selectedUnit[2];

    remaining = totalPurchase - quantyityTransfer * quantityOfBaseunit;

    if (remaining < 0) {
        quantyityTransfer = totalPurchase;
        remaining = 0;
        if (quantityOfBaseunit === "0") {
            quantityOfBaseunit = 1;

        }
        cleanBookNotification.warning(_L("ERROR_MSG_638", [totalPurchase / quantityOfBaseunit]), "Qbicles");
        $('.transfer_tr_' + id + ' td.transfer_td_tran_quan input').val(totalPurchase / quantityOfBaseunit);
    }
    remaining = (remaining / parseFloat(quantityOfBaseunit));
    $('.transfer_td_tran_cost_' + id + ' span').text(remaining);
}
