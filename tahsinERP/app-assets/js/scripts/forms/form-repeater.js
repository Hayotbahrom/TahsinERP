/*=========================================================================================
    File Name: Form-Repeater.js
    Description: form repeater page specific js
    ----------------------------------------------------------------------------------------
    Item Name: Frest HTML Admin Template
    Version: 1.0
    Author: PIXINVENT
    Author URL: http://www.themeforest.net/user/pixinvent
==========================================================================================*/

//$(document).ready(function () {
//  // form repeater jquery
//  $('.file-repeater, .contact-repeater, .repeater-default').repeater({
//    show: function () {
//      $(this).slideDown();
//    },
//    hide: function (deleteElement) {
//      if (confirm('Are you sure you want to delete this element?')) {
//        $(this).slideUp(deleteElement);
//      }
//    }
//  });

//});

var formRepeater = $(".form-repeater");

var row = 2;
var col = 1;
formRepeater.on('submit', function (e) {
    e.preventDefault();
});
formRepeater.repeater({
    show: function () {
        var fromControl = $(this).find('.form-control, .form-select');
        var formLabel = $(this).find('.form-label');

        fromControl.each(function (i) {
            var id = 'form-repeater-' + row + '-' + col;
            $(fromControl[i]).attr('id', id);
            $(formLabel[i]).attr('for', id);
            col++;
        });

        row++;

        $(this).slideDown();
    },
    hide: function (e) {
        confirm('Are you sure you want to delete this element?') && $(this).slideUp(e);
    }
});
