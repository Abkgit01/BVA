function CommaFormatted(amount) {
    var delimiter = ","; // replace comma if desired
    var a = amount.split('.', 2)
    var d = a[1];
    var i = parseInt(a[0]);
    if (isNaN(i)) { return ''; }
    var minus = '';
    if (i < 0) { minus = '-'; }
    i = Math.abs(i);
    var n = new String(i);
    var a = [];
    while (n.length > 3) {
        var nn = n.substr(n.length - 3);
        a.unshift(nn);
        n = n.substr(0, n.length - 3);
    }
    if (n.length > 0) { a.unshift(n); }
    n = a.join(delimiter);
    if (d.length < 1) { amount = n; }
    else { amount = n + '.' + d; }
    amount = minus + amount;
    return amount;
}
$(document).ready(function () {
    $('#VouchersTable').DataTable();
});
var appTable = $("#MyAppsTable").DataTable({

    dom: 'Bfrtip',
    buttons: [
        'pageLength',
        'copyHtml5',
        'excelHtml5',
        'csvHtml5',
        'pdfHtml5',
        {
            extend: 'print',
            text: 'Print all',
            exportOptions: {
                modifier: {
                    selected: null
                }
            }
        },
        {
            extend: 'colvis',
            collectionLayout: 'fixed two-column'
        }

    ],

    language: {
        buttons: {
            colvis: 'Change columns'
        }
    },

    select: true,

    "paging": true,
    "ordering": true,
    "info": true,
    "searching": true,
    "order": [[5, "desc"]],
    "processing": true,
    stateSave: true


});