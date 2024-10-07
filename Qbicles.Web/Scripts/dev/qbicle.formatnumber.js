//$(document).ready(function () {
//    $('input.isnumber').keypress(function (e) {

//        var keyCode = e.which;
//        var key = e.key;
//        var text = $(this).val().trim();
//        var zeronumber = false;
//        if (keyCode === 48) zeronumber = true;
//        if ((keyCode === 46 && text.indexOf('.') > 0)) {
//            e.preventDefault();
//        } else {
//            if (!((keyCode >= 48 && keyCode <= 57) || (keyCode === 8 || keyCode === 32 || keyCode === 44 || keyCode === 46))) {
//                e.preventDefault();
//            } else if (keyCode === 44 && text.indexOf('.') > 0) {
//                e.preventDefault();
//            }
//        }
//    });
//    $('input.isnumber').change(function(e) {
//        var text = $(this).val().trim();
//        var text1 = text.replace(/\,/g, "").split(".")[0];
//        var sufix = text.replace(/\,/g, "").split(".")[1];
//        var num = text1.length / 3;
//        if (num > 1) {
//            for (var i = text1.length - 3; i > 0; i -= 3) {
//                text1 = text1.slice(0, i) + "," + text1.slice(i, text1.length);
//            }
//        }
//        if(sufix)
//            $(this).val(text1 + "." + sufix);
//        else $(this).val(text1);
//    });

//    var regExp = /^[A-Za-z0-9][a-zA-Z0-9\\|¦-]*$/;

//    $('input.account-number').on('keypress', function (e) {
//        //console.log($(this).val());
//        //console.log(e.key);
//        if (!regExp.test(e.key)) {
//            e.preventDefault();
//            //$('.message.error').show();
//            //$('.message.success').hide();
//        }
//        //else {
//        //    $('.message.success').show();
//        //    $('.message.error').hide();
//        //}
//        //regExp.test($(this).val()) ? $('.message.success').show() : $('.message.error').show();
//        //e.preventDefault();
//    });
//});

function numberKeyPress(e) {
    var keyCode = e.which;    
    if (keyCode === 48) zeronumber = true;
    if ((keyCode === 46 || keyCode === 44)) {
        e.preventDefault();
    } else {
        if (!((keyCode >= 48 && keyCode <= 57) || (keyCode === 8 || keyCode === 32 || keyCode === 46))) {
            e.preventDefault();
        }
    }
}
function decimalKeyPress(t, e) {
    var $this = $(t);
    if ((e.which != 46 || $this.val().indexOf('.') != -1) &&
        ((e.which < 48 || e.which > 57) &&
            (e.which != 0 && e.which != 8))) {
        e.preventDefault();
    }

    var text = $(t).val();
    if ((e.which == 46) && (text.indexOf('.') == -1)) {
        setTimeout(function () {
            if ($this.val().substring($this.val().indexOf('.')).length > $decimalPlace + 1) {
                $this.val($this.val().substring(0, $this.val().indexOf('.') + $decimalPlace + 1));
            }
        }, 1);
    }

    if ((text.indexOf('.') != -1) &&
        (text.substring(text.indexOf('.')).length > $decimalPlace) &&
        (e.which != 0 && e.which != 8) &&
        ($(t)[0].selectionStart >= text.length - $decimalPlace)) {
        e.preventDefault();
    }
}
function decimalOnPaste(t, e) {
    var text = e.clipboardData.getData('Text');
    if ($.isNumeric(text)) {
        if ((text.substring(text.indexOf('.')).length > $decimalPlace + 1) && (text.indexOf('.') > -1)) {
            e.preventDefault();
            $(this).val(text.substring(0, text.indexOf('.') + $decimalPlace + 1));
        }
    }
    else {
        e.preventDefault();
    }
}

//$('.number-decimal-place').keypress(function (event) {
//    var $this = $(this);
//    if ((event.which != 46 || $this.val().indexOf('.') != -1) &&
//        ((event.which < 48 || event.which > 57) &&
//            (event.which != 0 && event.which != 8))) {
//        event.preventDefault();
//    }

//    var text = $(this).val();
//    if ((event.which == 46) && (text.indexOf('.') == -1)) {
//        setTimeout(function () {
//            if ($this.val().substring($this.val().indexOf('.')).length > $decimalPlace + 1) {
//                $this.val($this.val().substring(0, $this.val().indexOf('.') + $decimalPlace + 1));
//            }
//        }, 1);
//    }

//    if ((text.indexOf('.') != -1) &&
//        (text.substring(text.indexOf('.')).length > $decimalPlace) &&
//        (event.which != 0 && event.which != 8) &&
//        ($(this)[0].selectionStart >= text.length - $decimalPlace)) {
//        event.preventDefault();
//    }
//});

//$('.number-decimal-place').bind("paste", function (e) {
//    var text = e.originalEvent.clipboardData.getData('Text');
//    if ($.isNumeric(text)) {
//        if ((text.substring(text.indexOf('.')).length > $decimalPlace + 1) && (text.indexOf('.') > -1)) {
//            e.preventDefault();
//            $(this).val(text.substring(0, text.indexOf('.') + $decimalPlace + 1));
//        }
//    }
//    else {
//        e.preventDefault();
//    }
//});

function maxDecimalKeyPress(t, e, $maxDecimalPlace) {
    var $this = $(t);
    if ((e.which != 46 || $this.val().indexOf('.') != -1) &&
        ((e.which < 48 || e.which > 57) &&
            (e.which != 0 && e.which != 8))) {
        e.preventDefault();
    }

    var text = $(t).val();
    if ((e.which == 46) && (text.indexOf('.') == -1)) {
        setTimeout(function () {
            if ($this.val().substring($this.val().indexOf('.')).length > $maxDecimalPlace + 1) {
                $this.val($this.val().substring(0, $this.val().indexOf('.') + $maxDecimalPlace + 1));
            }
        }, 1);
    }

    if ((text.indexOf('.') != -1) &&
        (text.substring(text.indexOf('.')).length > $maxDecimalPlace) &&
        (e.which != 0 && e.which != 8) &&
        ($(t)[0].selectionStart >= text.length - $maxDecimalPlace)) {
        e.preventDefault();
    }
}
function maxDecimalOnPaste(t, e, $maxDecimalPlace) {
    var text = e.clipboardData.getData('Text');
    if ($.isNumeric(text)) {
        if ((text.substring(text.indexOf('.')).length > $maxDecimalPlace + 1) && (text.indexOf('.') > -1)) {
            e.preventDefault();
            $(this).val(text.substring(0, text.indexOf('.') + $maxDecimalPlace + 1));
        }
    }
    else {
        e.preventDefault();
    }
}


function maxDecimalOnInput(t, e, $maxDecimalPlace) {
    var $this = $(t);

    var maxDecimal = $maxDecimalPlace; // Set the maximum decimal value here
     // Get the current value of the input field
    var currentValue = $this.val();

    // Remove any non-digit and non-decimal characters from the input value
    var sanitizedValue = currentValue.replace(/[^0-9.]/g, '');

    // Split the sanitized value into integer and decimal parts
    var parts = sanitizedValue.split('.');
    var integerPart = parts[0];
    var decimalPart = parts[1];

    // Ensure that the integer part is within the maximum value
    if (integerPart > maxDecimal) {
        integerPart = maxDecimal.toString();
    }

    // Combine the integer and decimal parts
    var formattedValue = integerPart;
    if (decimalPart !== undefined) {
        formattedValue += '.' + decimalPart;
    }

    // Update the input field with the formatted value
    
    $this.val(formattedValue);
}