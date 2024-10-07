$(document).ready(function () {
    initTableTaskInstance();
    initSearch();
});
function initSearch() {
    $('#txtSearch').keyup(searchThrottle(function () {
        $('#tblTaskInstances').DataTable().ajax.reload();
    }));
    $('#slform').change(searchThrottle(function () {
        $('#tblTaskInstances').DataTable().ajax.reload();
    }));
}
function initTableTaskInstance() {
    var $tblComplianceTasks = $('#tblTaskInstances');
    $tblComplianceTasks.dataTable({
        destroy: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: true,
        pageLength: 10,
        order: [[1, "desc"]],
        ajax: {
            "url": "/Operator/SearchTaskIntances",
            "data": function (d) {
                return $.extend({}, d, {
                    "keyword": $('#txtSearch').val(),
                    "form": $('#slform').val() ? $('#slform').val() : 0,
                    "complianceTaskId": $('#ipcomplianceTaskId').val(),
                    "taskId": $('#iptaskId').val()
                });
            }
        },
        columns: [
            { "title": "Form", "data": "Form", "searchable": true, "orderable": true },
            { "title": "Submitted", "data": "Submitted", "searchable": true, "orderable": true },
            { "title": "Score", "data": "Score", "searchable": false, "orderable": false },
            { "title": "Options", "data": "Id", "searchable": false, "orderable": false }
        ],
        columnDefs: [
            {
                "targets": 2,
                "data": "Score",
                "render": function (data, type, row, meta) {
                    return data ? (data+"%"):"0%";
                }
            },
            {
                "targets": 3,
                "data": "Id",
                "render": function (data, type, row, meta) {
                    var _htmlOptions = '<a href="/Operator/ComplianceTaskSubmission?id=' + row.ComplianceTaskId + '&tid=' + row.TaskId + '&fid=' + row.FormInsId+'" class="btn btn-info" target="_blank" style="text-decoration: none;"><i class="fa fa-eye"></i> &nbsp; View</a>';
                    return _htmlOptions;
                }
            },
        ]
    });
}
function searchThrottle(f, delay) {
    var timer = null;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = window.setTimeout(function () {
            f.apply(context, args);
        },
            delay || 800);
    };
}