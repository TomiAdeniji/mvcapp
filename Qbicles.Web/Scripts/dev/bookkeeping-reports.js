var $frm_execute = $("#frm-execute-report");
var $frm_execute_balance =$('#frm-execute-report-balance');
$(document).ready(function () {
    $frm_execute.validate({ ignore: "" });
    $frm_execute.submit(function (event) {
        if (!$frm_execute.valid()) {
            $("#navtab-income a[href=#incomerep1]").click();
            event.preventDefault();
            return;
        }
        $('#treevalue').val(JSON.stringify(getValueTree()));
    });
    $frm_execute_balance.validate({ ignore: "" });
    $frm_execute_balance.submit(function (event) {
        if (!$frm_execute_balance.valid()) {
            $("#navtab-balance a[href=#balancerep1]").click();
            event.preventDefault();
            return;
        }
        $('#treevaluebalance').val(JSON.stringify(getValueTreeBlance()));
    });
    //$('#treevalue').val(JSON.stringify(getValueTree()));
    //$('#treevaluebalance').val(JSON.stringify(getValueTreeBlance()));
});
function getValueTree() {
    var $tree = $('#jstree-icreportentry').jstree();
    var data = $tree.get_json();
    var treedata = [];
    getChildItemTree(treedata, data);
    return treedata;
}
function getValueTreeBlance() {
    var $tree = $('#jstree-blreportentry').jstree();
    var data = $tree.get_json();
    var treedata = [];
    getChildItemBalance(treedata, data);
    var lstANodes = [];
    getAllIdsChild(lstANodes, data);
    $('#lstANodes').val(JSON.stringify(lstANodes));
    return treedata;
}
function getChildItemBalance(it_add, it_data) {
    $.each(it_data, function (index, value) {
        var item = {
            id: value.id.replace('ic-', ''),
            text: $(value.text).filter('h5').text(),
            isExpanded: value.state.opened,
            amount:0,
            children: [],
            allChildIds:[]//save all child ANodeID
        };
        item.allChildIds.push(item.id);
        if (value.children && value.children.length > 0) {
            getChildItemBalance(item.children, value.children);
            getAllIdsChild(item.allChildIds, value.children);
        }
        it_add.push(item);
    });
}
function getChildItemTree(it_add, it_data) {
    $.each(it_data, function (index, value) {
        var item = {
            id: value.id.replace('ic-', ''),
            text: $(value.text).filter('h5').text(),
            isExpanded: value.state.opened,
            children: []
        };
        item.children.push(item.id);
        if (value.children && value.children.length > 0) {
            getChildItemTree(it_add, value.children);
            getAllIdsChild(item.children, value.children);
        }
        it_add.push(item);
        
    });
}
function getAllIdsChild(child,data) {
    $.each(data, function (index, value) {
        child.push(value.id.replace('ic-', ''));
        if (value.children && value.children.length > 0) {
            getAllIdsChild(child, value.children);
        }
    });
}