
$('#builder').queryBuilder({
    filters: [{
        id: 'date',
        label: 'Date',
        type: 'date',
        plugin: 'datepicker',
        plugin_config: {
            todayBtn: 'linked',
            todayHighlight: true,
            autoclose: true
        }
    }, {
        id: 'Debit',
        label: 'Debit',
        type: 'double',
        validation: {
            min: 0,
            step: 0.01
        }
    }, {
        id: 'Credit',
        label: 'Credit',
        type: 'double',
        validation: {
            min: 0,
            step: 0.01
        }
    }, {
        id: 'Balance',
        label: 'Balance',
        type: 'double',
        validation: {
            min: 0,
            step: 0.01
        }
    }, {
        id: 'Reference',
        label: 'Reference',
        type: 'string'
    },
    {
        id: 'Description',
        label: 'Description',
        type:'string'
    }
  ]
});

