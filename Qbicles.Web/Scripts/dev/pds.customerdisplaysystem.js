
var $locationManager = $('#location-manager').val();

$('#search-item-list').keyup(function () {
    $('#item-list').DataTable().search($(this).val()).draw();
});
$('#subfilter-group').on('change', function () {
    var group = $(this).val();
    $('#item-list').DataTable().search(group, false, false, false).draw();
});