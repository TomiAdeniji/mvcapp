//Make sure jQuery has been loaded before app.js
if (typeof jQuery === "undefined") {
    throw new Error("Qbicles requires jQuery");
}

$.AdminLTE = {};
var $dateFormatByUser = $("#formatDateByUser").val();
var $systemShortDateTimeFomat = $('#currentSystemShortDateFormat').val();
var $systemFormat = 'YYYY/MM/DD HH:mm';
var $timeFormatByUser = $('#formatTimeByUser').val();
var $dateTimeFormatByUser =$dateFormatByUser&&$timeFormatByUser? ($dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser):'';
/* --------------------
 * - Options -
 * --------------------
 * Modify these options to suit your implementation
 */
$.AdminLTE.options = {
    //Add slimscroll to navbar menus
    //This requires you to load the slimscroll plugin
    //in every page before app.js
    navbarMenuSlimscroll: true,
    navbarMenuSlimscrollWidth: "3px", //The width of the scroll bar
    navbarMenuHeight: "200px", //The height of the inner menu
    //General animation speed for JS animated elements such as box collapse/expand and
    //sidebar treeview slide up/down. This options accepts an integer as milliseconds,
    //'fast', 'normal', or 'slow'
    animationSpeed: 500,
    //Sidebar push menu toggle button selector
    sidebarToggleSelector: "[data-toggle='offcanvas']",
    //Activate sidebar push menu
    sidebarPushMenu: true,
    //Activate sidebar slimscroll if the fixed layout is set (requires SlimScroll Plugin)
    sidebarSlimScroll: false,
    //Enable sidebar expand on hover effect for sidebar mini
    //This option is forced to true if both the fixed layout and sidebar mini
    //are used together
    sidebarExpandOnHover: false,
    //BoxRefresh Plugin
    enableBoxRefresh: false,
    //Bootstrap.js tooltip
    enableBSToppltip: false,
    BSTooltipSelector: "[data-toggle='tooltip']",
    //Enable Fast Click. Fastclick.js creates a more
    //native touch experience with touch devices. If you
    //choose to enable the plugin, make sure you load the script
    //before AdminLTE's app.js
    enableFastclick: false,
    //Control Sidebar Options
    enableControlSidebar: false,
    controlSidebarOptions: {
        //Which button should trigger the open/close event
        toggleBtnSelector: "[data-toggle='control-sidebar']",
        //The sidebar selector
        selector: ".control-sidebar",
        //Enable slide over content
        slide: true
    },
    //Define the set of colors to use globally around the website
    colors: {
        lightBlue: "#3c8dbc",
        red: "#f56954",
        green: "#00a65a",
        aqua: "#00c0ef",
        yellow: "#f39c12",
        blue: "#0073b7",
        navy: "#001F3F",
        teal: "#39CCCC",
        olive: "#3D9970",
        lime: "#01FF70",
        orange: "#FF851B",
        fuchsia: "#F012BE",
        purple: "#8E24AA",
        maroon: "#D81B60",
        black: "#222222",
        gray: "#d2d6de"
    },
    //The standard screen sizes that bootstrap uses.
    //If you change these in the variables.less file, change
    //them here too.
    screenSizes: {
        xs: 480,
        sm: 768,
        md: 992,
        lg: 1200
    }
};

/* ------------------
 * - Implementation -
 * ------------------
 * The next block of code implements AdminLTE's
 * functions and plugins as specified by the
 * options above.
 */
$(function () {
    "use strict";

    //Fix for IE page transitions
    $("body").removeClass("hold-transition");

    //Extend options if external options exist
    if (typeof AdminLTEOptions !== "undefined") {
        $.extend(true,
            $.AdminLTE.options,
            AdminLTEOptions);
    }

    //Easy access to options
    var o = $.AdminLTE.options;

    //Set up the object
    _init();

    //Activate the layout maker
    $.AdminLTE.layout.activate();

    //Enable sidebar tree view controls
    $.AdminLTE.tree('.sidebar');


    //Add slimscroll to navbar dropdown
    if (o.navbarMenuSlimscroll && typeof $.fn.slimscroll != 'undefined') {
        $(".navbar .menu").slimscroll({
            height: o.navbarMenuHeight,
            alwaysVisible: false,
            size: o.navbarMenuSlimscrollWidth
        }).css("width", "100%");
    }

    //Activate sidebar push menu
    if (o.sidebarPushMenu) {
        $.AdminLTE.pushMenu.activate(o.sidebarToggleSelector);
    }

    //Activate fast click
    if (o.enableFastclick && typeof FastClick != 'undefined') {
        FastClick.attach(document.body);
    }



});

/* ----------------------------------
 * - Initialize the AdminLTE Object -
 * ----------------------------------
 * All AdminLTE functions are implemented below.
 */
function _init() {
    'use strict';
    /* Layout
     * ======
     * Fixes the layout height in case min-height fails.
     *
     * @type Object
     * @usage $.AdminLTE.layout.activate()
     *        $.AdminLTE.layout.fix()
     *        $.AdminLTE.layout.fixSidebar()
     */
    $.AdminLTE.layout = {
        activate: function () {
            var _this = this;
            _this.fix();
            _this.fixSidebar();
            $(window, ".wrapper").resize(function () {
                _this.fix();
                _this.fixSidebar();
            });
        },
        fix: function () {
            //Get window height and the wrapper height
            var neg = $('.main-header').outerHeight() + $('.main-footer').outerHeight();
            var window_height = $(window).height();
            var sidebar_height = $(".sidebar").height();
            //Set the min-height of the content and sidebar based on the
            //the height of the document.
            if ($("body").hasClass("fixed")) {
                $(".content-wrapper, .right-side").css('min-height', window_height - $('.main-footer').outerHeight());
            } else {
                var postSetWidth;
                if (window_height >= sidebar_height) {
                    $(".content-wrapper, .right-side").css('min-height', window_height - neg);
                    postSetWidth = window_height - neg;
                } else {
                    $(".content-wrapper, .right-side").css('min-height', sidebar_height);
                    postSetWidth = sidebar_height;
                }

                //Fix for the control sidebar height
                var controlSidebar = $($.AdminLTE.options.controlSidebarOptions.selector);
                if (typeof controlSidebar !== "undefined") {
                    if (controlSidebar.height() > postSetWidth)
                        $(".content-wrapper, .right-side").css('min-height', controlSidebar.height());
                }

            }
        },
        fixSidebar: function () {
            //Make sure the body tag has the .fixed class
            if (!$("body").hasClass("fixed")) {
                if (typeof $.fn.slimScroll != 'undefined') {
                    $(".sidebar").slimScroll({ destroy: true }).height("auto");
                }
                return;
            } else if (typeof $.fn.slimScroll == 'undefined' && window.console) {
                window.console.error("Error: the fixed layout requires the slimscroll plugin!");
            }
            //Enable slimscroll for fixed layout
            if ($.AdminLTE.options.sidebarSlimScroll) {
                if (typeof $.fn.slimScroll != 'undefined') {
                    //Destroy if it exists
                    $(".sidebar").slimScroll({ destroy: true }).height("auto");
                    //Add slimscroll
                    $(".sidebar").slimscroll({
                        height: ($(window).height() - $(".main-header").height()) + "px",
                        color: "rgba(0,0,0,0.2)",
                        size: "3px"
                    });
                }
            }
        }
    };

    /* PushMenu()
     * ==========
     * Adds the push menu functionality to the sidebar.
     *
     * @type Function
     * @usage: $.AdminLTE.pushMenu("[data-toggle='offcanvas']")
     */
    $.AdminLTE.pushMenu = {
        activate: function (toggleBtn) {
            //Get the screen sizes
            var screenSizes = $.AdminLTE.options.screenSizes;

            //Enable sidebar toggle
            $(document).on('click', toggleBtn, function (e) {
                e.preventDefault();

                //Enable sidebar push menu
                if ($(window).width() > (screenSizes.sm - 1)) {
                    if ($("body").hasClass('sidebar-collapse')) {
                        $("body").removeClass('sidebar-collapse').trigger('expanded.pushMenu');
                    } else {
                        $("body").addClass('sidebar-collapse').trigger('collapsed.pushMenu');
                    }
                }
                //Handle sidebar push menu for small screens
                else {
                    if ($("body").hasClass('sidebar-open')) {
                        $("body").removeClass('sidebar-open').removeClass('sidebar-collapse').trigger('collapsed.pushMenu');
                    } else {
                        $("body").addClass('sidebar-open').trigger('expanded.pushMenu');
                    }
                }
            });

            $(".content-wrapper").click(function () {
                //Enable hide menu when clicking on the content-wrapper on small screens
                if ($(window).width() <= (screenSizes.sm - 1) && $("body").hasClass("sidebar-open")) {
                    $("body").removeClass('sidebar-open');
                }
            });

            //Enable expand on hover for sidebar mini
            if ($.AdminLTE.options.sidebarExpandOnHover
                || ($('body').hasClass('fixed')
                    && $('body').hasClass('sidebar-mini'))) {
                this.expandOnHover();
            }
        },
        expandOnHover: function () {
            var _this = this;
            var screenWidth = $.AdminLTE.options.screenSizes.sm - 1;
            //Expand sidebar on hover
            $('.main-sidebar').hover(function () {
                if ($('body').hasClass('sidebar-mini')
                    && $("body").hasClass('sidebar-collapse')
                    && $(window).width() > screenWidth) {
                    _this.expand();
                }
            }, function () {
                if ($('body').hasClass('sidebar-mini')
                    && $('body').hasClass('sidebar-expanded-on-hover')
                    && $(window).width() > screenWidth) {
                    _this.collapse();
                }
            });
        },
        expand: function () {
            $("body").removeClass('sidebar-collapse').addClass('sidebar-expanded-on-hover');
        },
        collapse: function () {
            if ($('body').hasClass('sidebar-expanded-on-hover')) {
                $('body').removeClass('sidebar-expanded-on-hover').addClass('sidebar-collapse');
            }
        }
    };

    /* Tree()
     * ======
     * Converts the sidebar into a multilevel
     * tree view menu.
     *
     * @type Function
     * @Usage: $.AdminLTE.tree('.sidebar')
     */
    $.AdminLTE.tree = function (menu) {
        var _this = this;
        var animationSpeed = $.AdminLTE.options.animationSpeed;
        $(menu).on('click', 'li a', function (e) {
            //Get the clicked link and the next element
            var $this = $(this);
            var checkElement = $this.next();

            //Check if the next element is a menu and is visible
            if ((checkElement.is('.treeview-menu')) && (checkElement.is(':visible')) && (!$('body').hasClass('sidebar-collapse'))) {
                //Close the menu
                checkElement.slideUp(animationSpeed, function () {
                    checkElement.removeClass('menu-open');
                    //Fix the layout in case the sidebar stretches over the height of the window
                    //_this.layout.fix();
                });
                checkElement.parent("li").removeClass("active");
            }
            //If the menu is not visible
            else if ((checkElement.is('.treeview-menu')) && (!checkElement.is(':visible'))) {
                //Get the parent menu
                var parent = $this.parents('ul').first();
                //Close all open menus within the parent
                var ul = parent.find('ul:visible').slideUp(animationSpeed);
                //Remove the menu-open class from the parent
                ul.removeClass('menu-open');
                //Get the parent li
                var parent_li = $this.parent("li");

                //Open the target menu and add the menu-open class
                checkElement.slideDown(animationSpeed, function () {
                    //Add the class active to the parent li
                    checkElement.addClass('menu-open');
                    parent.find('li.active').removeClass('active');
                    parent_li.addClass('active');
                    //Fix the layout in case the sidebar stretches over the height of the window
                    _this.layout.fix();
                });
            }
            //if this isn't a link, prevent the page from being redirected
            if (checkElement.is('.treeview-menu')) {
                e.preventDefault();
            }
        });
    };


}





$.fn.extend({
    animateCss: function (animationName) {
        var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
        this.addClass('animated ' + animationName).one(animationEnd, function () {
            $(this).removeClass('animated ' + animationName);
        });
    }
});

$(document).on('shown.bs.modal', function (e) {
    if ($.fn.dataTable)
        $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
});

$(window).resize(function (e) {
    if ($.fn.dataTable)
        $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
});

if (jQuery().formBuilder) {

    $('.finalise-form').toggle();

    jQuery(document).ready(function ($) {
        var buildWrap = document.querySelector('.build-wrap'),
            renderWrap = document.querySelector('.render-wrap'),
            editBtn = document.getElementById('edit-form'),
            formData = window.sessionStorage.getItem('formData'),
            editing = true,
            fbOptions = {
                dataType: 'json'
            };

        if (formData) {
            fbOptions.formData = JSON.parse(formData);
        }

        var toggleEdit = function () {
            document.body.classList.toggle('form-rendered', editing);
            editing = !editing;
        };

        //var formBuilder = $(buildWrap).formBuilder(fbOptions).data('formBuilder');

        $('.form-builder-save').click(function () {
            toggleEdit();
            $(renderWrap).formRender({
                dataType: 'json',
                formData: formBuilder.formData
            });
            $('.finalise-form').toggle();

            window.sessionStorage.setItem('formData', JSON.stringify(formBuilder.formData));
        });

        //editBtn.onclick = function () {
        //    toggleEdit();
        //    $('.finalise-form').toggle();
        //};
    });
}


function formatResult(result) {
    if (!result.id) {
        return result.text;

    }
    else {

    }

    /*
  var $state = $(
    '<span><img src="vendor/images/flags/' + state.element.value.toLowerCase() + '.png" class="img-flag" /> ' + state.text + '</span>'
  );
  return $state;
  */
};


if (jQuery().datetimepicker) {
    function enable_dtpicker() {
        $('.dtpicker').datetimepicker({
            lang: 'en',
            format: 'd.m.Y',

            onShow: function (ct) {
                this.setOptions({
                    maxDate: jQuery('.dtpicker').val() ? jQuery('.dtpicker').val() : false
                })
            },
            timepicker: false
        });
    }
}

function totop() {
    $("html, body").animate({ scrollTop: 0 }, 0);
    return false;
}


function tobottom() {
    $("html, body").animate({ scrollTop: document.body.scrollHeight }, "slow");
}


function manage_selected(int, ext) {
    if (int > 0 && ext > 0) {
        $('.alert_matches p').html('Selected <span>' + int + '</span> against <span>' + ext + '</span>');
        $('.alert_matches').addClass('active');
    } else {
        $('.alert_matches').removeClass('active');
    }
};


function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}


//$('body').on('DOMNodeInserted', '#builder select', function () {
//    $(this).select2();
//});


jQuery(document).ready(function () {
    var accordionsMenu = $('.cd-accordion-menu');

    if (accordionsMenu.length > 0) {

        accordionsMenu.each(function () {
            var accordion = $(this);
            //detect change in the input[type="checkbox"] value
            accordion.on('change', 'input[type="checkbox"]', function () {
                var checkbox = $(this);
                (checkbox.prop('checked')) ? checkbox.siblings('ul').attr('style', 'display:none;').slideDown(300) : checkbox.siblings('ul').attr('style', 'display:block;').slideUp(300);
            });
        });
    }
});


function update_cost(item, unit) {
    if (item !== "" && unit !== "") {
        $('#cpu').val('100.00');
    }
}


function switch_setup(from, to) {
    $(from).hide();
    $(to).fadeIn();

    $('.wizard-steps li').each(function () {
        if ($('a', this).data('form') === from) {
            that = this;
            $(this).removeClass('active').addClass('complete');
        }
        if ($('a', this).data('form') === to) {
            that = this;
            $(this).addClass('active');
        }
    });
}

$(document).ready(function () {
    $('#schedule_type').on('change', function (e) {
        var type = $(this).val();
        var toshow = '#schedule_' + type;
        $('.scheduling_method').hide();
        $(toshow).show();
    });

    $('.rowmask').change(function (e) {
        if ($(this).prop('checked') == false) {
            $(this).closest('tr').find('.maskable').css('opacity', '0.2');
        } else {
            $(this).closest('tr').find('.maskable').css('opacity', '1');
        }
    });

    // Select2 plugin extension for variable ordering
    var $select2 = $('select.select2').select2();

    /**
     * defaults: Cache order of the initial values
     * @type {object}
     */
    var defaults = $select2.select2('data');
    if (defaults) {
        defaults.forEach(obj => {
            var order = $select2.data('preserved-order') || [];
            order[order.length] = obj.id;
            $select2.data('preserved-order', order)
        });
    }



    /**
     * select2_renderselections
     * @param  {jQuery Select2 object}
     * @return {null}
     */
    function select2_renderSelections($select2) {
        var order = $select2.data('preserved-order') || [];
        var $container = $select2.next('.select2-container');
        var $tags = $container.find('li.select2-selection__choice');
        var $input = $tags.last().next();

        // apply tag order
        order.forEach(val => {
            var $el = $tags.filter(function (i, tag) {
                return $(tag).data('data').id === val;
            });
            $input.before($el);
        });
    }


    /**
     * selectionHandler
     * @param  {Select2 Event Object}
     * @return {null}
     */
    function selectionHandler(e) {
        var $select2 = $(this);
        var val = e.params.data.id;
        var order = $select2.data('preserved-order') || [];

        switch (e.type) {
            case 'select2:select':
                order[order.length] = val;
                break;
            case 'select2:unselect':
                var found_index = order.indexOf(val);
                if (found_index >= 0)
                    order.splice(found_index, 1);
                break;
        }
        $select2.data('preserved-order', order); // store it for later
        select2_renderSelections($select2);
    }

    $select2.on('select2:select select2:unselect', selectionHandler);

    // END Select2 extension






    $('.horizontal-portlets .portlet-header input[type="checkbox"]').bind('click', function (e) {
        $('#selected-items').show();
    });

    $('#payment-for').on('change', function (e) {
        var method = $(this).val();
        $('#payment-specifics').show();
        if (method == "invoice") {
            $('#contact-selector').hide();
            $('#invoice-selector').fadeIn();
        } else {
            $('#invoice-selector').hide();
            $('#contact-selector').fadeIn();
        }
    });

    $('#transfer_req').on('change', function (e) {
        var method = $(this).val();
        $('#transfer-specifics').show();

        if (method == "p2p") {
            $('#goods_in').hide();
            $('#goods_out').hide();
            $('#p2p').fadeIn();
            $('#routing').fadeIn();
            $('#route_goods').hide();
            $('#route_p2p').show();
        }

        if (method == "goods_in") {
            $('#p2p').hide();
            $('#goods_in').fadeIn();
            $('#goods_out').hide();
            $('#route_p2p').hide();
            $('#routing').hide();
            $('#route_goods').show();
        }

        if (method == "goods_out") {
            $('#p2p').hide();
            $('#goods_in').hide();
            $('#goods_out').fadeIn();
            $('#route_p2p').hide();
            $('#routing').hide();
            $('#route_goods').show();
        }
    });


    $('.jump-to').bind('click', function (e) {
        var target = $(this).attr('href');
        $('html, body').animate({
            scrollTop: $(target).offset().top - 200
        }, 1200);
    });

    $('[data-toggle="popover"]').popover();

    $(document).on('click', function (e) {
        $('[data-toggle="popover"],[data-original-title]').each(function () {
            //the 'is' for buttons that trigger popups
            //the 'has' for icons within a button that triggers a popup
            if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
                (($(this).popover('hide').data('bs.popover') || {}).inState || {}).click = false  // fix for BS 3.3.6
            }

        });
    });

    $('.trigger-setup').bind('click', function (e) {
        var form = $(this).data('form');
        $(form).fadeIn();
    });


    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        if ($.fn.dataTable)
            $($.fn.dataTable.tables(true)).DataTable()
                .columns.adjust()
                .responsive.recalc();
    });

    $('#balance-payment').on('change', function () {
        var payment = $(this).val();
        var balance = '500000';
        balance = balance - payment;
        $('#invoice-remaining').html(balance);
    });

    $('#transfer_type').on('change', function () {
        var method = $(this).val();

        $('#source').hide();
        $('#destination').hide();
        $(method).toggle();
    });

    $('#item').on('change', function () {
        var item = $(this).val();
        var unit = $('#unit').val();
        update_cost(item, unit);
    });

    $('#unit').on('change', function () {
        var unit = $(this).val();
        var item = $('#item').val();
        update_cost(item, unit);
    });

    $('#recipe-ingredient-quantity').on('change', function (e) {
        var unitcost = $(this).data('unitcost');
        var multiply = $(this).val();
        var cost = unitcost * multiply;
        $('#recipe-ingredient-cost').html(cost);
    });

    $('#recipe-ingredient-quantity-2').on('change', function (e) {
        var unitcost = $(this).data('unitcost');
        var multiply = $(this).val();
        var cost = unitcost * multiply;
        $('#recipe-ingredient-cost-2').html(cost);
    });

    $('#recipe-ingredient-add').on('change', function (e) {
        var unitcost = $(this).select2().find(":selected").data("unitcost");
        $('#recipe-ingredient-add-quantity').attr('data-unitcost', unitcost);
        $('#recipe-ingredient-add-quantity').val(1);
        $('#recipe-ingredient-add-cost').html(unitcost);
    });

    $('#recipe-ingredient-add-quantity').on('change', function () {
        var unitcost = $(this).data('unitcost');
        var multiply = $(this).val();
        var cost = unitcost * multiply;
        $('#recipe-ingredient-add-cost').html(cost);
    });


    $('.loading-button').bind('click', function () {
        $('.general-state', this).toggle();
        $('.loading-state', this).toggle();
        $(this).addClass('animated shake');
    });

    $('.manage-columns input[type="checkbox"]').on('change', function () {
        var table = $('#community-list').DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });

    $('.manage-columns.v2 input[type="checkbox"]').on('change', function () {
        var table = $.closest($('.community-list')).DataTable();
        var column = table.column($(this).attr('data-column'));
        column.visible(!column.visible());
    });

    $('#toggle_custom').on('change', function () {
        sel = $(this).val();
        if (sel == "Custom") {
            $('#custom_view').fadeIn();
        } else {
            $('#custom_view').fadeOut();
        }
    });


    $('#sales-delivery').on('change', function (e) {
        if ($(this).val() == "delivery") {
            $('.delivery-stored').fadeIn();
        } else {
            $('.delivery-stored').hide();
        }
    });

    $('#delivery-new').on('click', function (e) {
        $('.delivery-stored').hide();
        $('.delivery-details').fadeIn();
    });

    $('#qty-example').on('change', function () {
        var qty = $(this).val();
        var total = qty * 15;
        $('.qty').html(total);
    });

    $('button[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        if ($.fn.dataTable)
            $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
    });

    $('q[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        if ($.fn.dataTable)
            $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
    });


    $('.approval_panel_switch').bind('click', function (e) {
        $('.approval_panel').removeClass('triggered');
        $(this).closest('.approval_panel').addClass('triggered');
    });

    var oTable = $('#community-list');
    $('#search_dt').keyup(function () {
        $('#community-list').DataTable().search($(this).val()).draw();
    })

    var oTable = $('.community-list');
    $('.search_dt').keyup(function () {
        $('.community-list').DataTable().search($(this).val()).draw();
    })

    $('#subfilter-group').on('change', function () {
        var group = $(this).val();
        $('#community-list').DataTable().search(group, false, false, false).draw();
    });

    var oTable = $('#qbicles-list');
    $('#search_dt').keyup(function () {
        $('#qbicles-list').DataTable().search($(this).val()).draw();
    })

    $('#community-search button').bind('click', function (e) {
        $('.counter').fadeIn().animateCss('bounce');
    });

    $('.community-tags a.topic-label').bind('click', function (e) {
        e.preventDefault();
        $(this).data('tag');
        var original = $('#tagselect').val();
        var filters = original + ',' + tag;

        /* Programatically add the new option to the multiselect's list of selected values
        
        $('#tagselect').val([filters]); 
        $('#tagselect').trigger('change'); 
        */
    });

    $('.community-dash-main .categories .flex .col').bind('click', function (e) {
        $('.community-dash-main .categories .flex .col').removeClass('active');
        $(this).addClass('active');
        $('.display_catgory').hide();
    });

    $('.featured button').bind('click', function (e) {
        e.preventDefault();
    });

    $(document).on('click', '.trigger_ledger', function (e) {
        var ledger = $(this).data('ledger');
        $('.jstree').jstree(true).select_node(ledger);
    });

    $('#add_attachment_trigger').bind('click', function (e) {
        e.preventDefault();
        var repeater_rows = $('.add_attachment_row').length;
        var block = "";

        if (repeater_rows < 6) {
            block = $('.repeater_wrap:last').last().clone();
            $(block).insertAfter('.repeater_wrap');
        }
    });

    $('#stars a').bind('click', function (e) {
        e.preventDefault();
        var stars = $(this).data('stars');
        $('#stars-selected').html(stars);
    });

    $('.slide_detail').bind('click', function (e) {
        e.preventDefault();
        var target = '.attachments-list.' + $(this).data('target');
        $(target).slideToggle();
        $('i', this).toggleClass('fa-eye');
        $('i', this).toggleClass('fa-remove');
    });

    if ($(document).width() < 768) {
        if (jQuery().waypoint) {
            $('section.content').waypoint(function (direction) {
                if (direction == "down") {
                    $('.mobile-further').removeClass('open');
                }
                if (direction == "up") {
                    $('.mobile-further').addClass('open');
                }
            });
        }



        // Hide Header on on scroll down
        var didScroll;
        var lastScrollTop = 0;
        var delta = 5;
        var navbarHeight = $('header').outerHeight();

        $(window).scroll(function (event) {
            didScroll = true;
        });

        setInterval(function () {
            if (didScroll) {
                hasScrolled();
                didScroll = false;
            }
        }, 250);

        function hasScrolled() {
            var st = $(this).scrollTop();

            // Make sure they scroll more than delta
            if (Math.abs(lastScrollTop - st) <= delta)
                return;

            // If they scrolled down and are past the navbar, add class .nav-up.
            // This is necessary so you never see what is "behind" the navbar.
            if (st > lastScrollTop && st > navbarHeight) {
                // Scroll Down
                $('.mobile-further').removeClass('open');
            } else {
                // Scroll Up
                if (st + $(window).height() < $(document).height()) {
                    $('.mobile-further').addClass('open');
                }
            }

            lastScrollTop = st;
        }
    }


    $('#status').on('change', function (e) {
        var sel = $(this).val();
        if (sel == "Complete") {
            $(this).hide();
            $('.complete-indicator').show();
            $('#status-label').removeClass('label-warning').addClass('label-success');
            $('#status-label').html('Complete');
        }
    });



    $("#sortable").sortable();
    $("#sortable").disableSelection();

    $('.switch_select').on('change', function (e) {

        var toshow = $(this).val();
        $('.task_route').hide();
        $(toshow).fadeIn();
    });

    $('.configure_rules').bind('click', function (e) {
        $('.btn').next().prop('disabled', false);
    });

    $('.activity-overview.successful').bind('click', function (e) {
        $(this).hide();
    });

    $('.query_to_result').bind('click', function (e) {
        $('#forms').fadeIn();
    });

    $('.execute').bind('click', function (e) {
        e.preventDefault();
        $('#query').hide();
        $('.query_reset').show();
        $('#results').fadeIn();
    });

    $('body').bind('click', function (e) {
        var container = $(".sticky-footer");
        if (!container.is(e.target) && container.has(e.target).length === 0 && container.hasClass('opened')) {
            container.removeClass('opened');
        }
    });

    $('#fqt').change(function (e) {
        if ($(this).prop('checked') == true) {
            $('.task_parameters').fadeIn();
        } else {
            $('.task_parameters').fadeOut();
        }
        if ($(this).prop('checked') == false && $('#fqf').prop('checked') == false) {
            $('.execute').fadeOut();
        } else {
            $('.execute').fadeIn();
        }
    });

    $('#fqf').change(function (e) {
        if ($(this).prop('checked') == true) {
            $('.form_parameters').fadeIn();
        } else {
            $('.form_parameters').fadeOut();
        }
        if ($(this).prop('checked') == false && $('#fqt').prop('checked') == false) {
            $('.execute').fadeOut();
        } else {
            $('.execute').fadeIn();
        }
    });


    // Transaction matching routine
    var internal = 0;
    var external = 0;
    $('.matching input:checkbox').removeAttr('checked');

    $('.matching input:checkbox').on('change', function (e) {
        if ($(this).is(':checked')) {
            $(this).closest("tr").toggleClass("selected");
            if ($(this).attr("name") == "internal[]") {
                internal++;
            } else {
                external++;
            }
        } else {
            $(this).closest("tr").removeClass("selected");
            if ($(this).attr("name") == "internal[]") {
                internal--;
            } else {
                external--;
            }
        }
        manage_selected(internal, external);
    });

    $('.switcher').bind('click', function (e) {
        var tohide = $(this).data('tohide');
        var toshow = $(this).data('toshow');

        $(tohide).hide();
        $(toshow).fadeIn();
    });

    $('.trigger_load').bind('click', function (e) {

        $.LoadingOverlay("show");
        setTimeout(function () {
            $.LoadingOverlay("hide");
        }, 2000);
    });

    $('#close_tm').bind('click', function (e) {
        $('#task-tm-stats').hide();
        $('#manual_match').hide();
        $('#task-tm-close').fadeIn();
    });

    $('select[name="tab_switcher"]').on('change', function (e) {
        var id = $(this).val();
        $('a[href="' + id + '"]').tab('show');
    });

    $('.mobile-discuss').bind('click', function (e) {
        e.preventDefault();
        $('.sticky-footer').addClass('opened')
        $('.filtering_panel').hide();
        $('.sticky-footer .dropup.create').hide();
        $('.sticky-footer .obfuscated').fadeIn();
        $('.footer_discussion').addClass('new-ft-fix');
    });

    $('.mobile-discuss-end').bind('click', function (e) {
        e.preventDefault();
        $('.sticky-footer').removeClass('opened');
        $('.filtering_panel').show();
        $('.sticky-footer .dropup.create').show();
        $('.sticky-footer .obfuscated').fadeOut();
        $('.footer_discussion').removeClass('new-ft-fix');
    });

    $('.datatable th input[type="checkbox"]').bind('click', function (e) {
        e.stopPropagation();

        if ($(this).is(':checked')) {
            $('.datatable td input[type="checkbox"]').attr('checked', true);
        } else {
            $('.datatable td input[type="checkbox"]').attr('checked', false);
        }
    });

    $('.showme').on('click', function (e) {
        var target = $(this).data('target');
        $(target).toggle();
    });

    $('.upload_preview_close').bind('click', function (e) {
        e.preventDefault();
        $('#upload_preview').hide();
    });

    $('#date_demo').on('change', function () {
        if ($(this).val() == 'Date') {
            $('#date_options').show();
        }
        else {
            $('#date_options').hide();
        }
    });

    $('.required_file').on('change', function () {
        $('.required_file_action').prop('disabled', false);
    });

    $('.required_file_action').on('click', function (e) {
        e.preventDefault();
        var target = $(this).data('div');
        $(target).show();
    });

    $('.toggle_view').bind('click', function (e) {
        e.preventDefault();
        var view = $(this).attr('href');

        $('.toggle_view').removeClass('active');
        $(this).addClass('active');

        $('.tab-content').hide();
        $(view).show();

        if ($.fn.dataTable)
            $($.fn.dataTable.tables(true)).DataTable()
                .columns.adjust()
                .responsive.recalc();
    });

    $('.select2').select2({
        placeholder: 'Please select'
    });
    $('.select2.taginput').select2({
        placeholder: 'Please select',
        tags: true
    });

    $('.tag-select .dropdown-menu a').on('click', function (e) {
        e.preventDefault();
        $('#topic-title').val($(this).html());
    });

    $('.pin-this').bind('click', function (e) {
        e.preventDefault();

        if ($(this).hasClass('pinned')) {
            $(this).removeClass('pinned');
        } else {
            $(this).addClass('pinned');
        }
    });


    $('.build-form').bind("DOMSubtreeModified", function () {
        enable_dtpicker();
    });


    if (jQuery().queryBuilder) {

        var rules_basic = {
            condition: 'AND',
            rules: [{
                id: 'price',
                operator: 'less',
                value: 10.25
            }]
        };

        $('#builder').queryBuilder({
            filters: [{
                id: 'name',
                label: 'Name',
                type: 'string'
            }, {
                id: 'category',
                label: 'Category',
                type: 'integer',
                input: 'select',
                values: {
                    1: 'Books',
                    2: 'Movies',
                    3: 'Music',
                    4: 'Tools',
                    5: 'Goodies',
                    6: 'Clothes'
                },
                operators: ['equal', 'not_equal', 'in', 'not_in', 'is_null', 'is_not_null']
            }, {
                id: 'in_stock',
                label: 'In stock',
                type: 'integer',
                input: 'radio',
                values: {
                    1: 'Yes',
                    0: 'No'
                },
                operators: ['equal']
            }, {
                id: 'price',
                label: 'Price',
                type: 'double',
                validation: {
                    min: 0,
                    step: 0.01
                }
            }, {
                id: 'id',
                label: 'Identifier',
                type: 'string',
                placeholder: '____-____-____',
                operators: ['equal', 'not_equal'],
                validation: {
                    format: /^.{4}-.{4}-.{4}$/
                }
            }],

            rules: rules_basic
        });

    }


    if (jQuery().DataTable) {
        $('.datatable').each(
            function (index, element) {
                var _table = $(element);
                if (!$.fn.DataTable.isDataTable(_table)) {
                    _table.DataTable({
                        responsive: true,
                        order: [[0, 'asc']],
                        "language": {
                            "lengthMenu": "_MENU_ &nbsp; per page"
                        }
                    });
                }
            }
        );
        
        $('.datatablereport').DataTable({
            responsive: false,
            searching: false,
            paging: false,
            scrollX: true,
            scrollY: false,
            info: false,
            initComplete: function (settings, json) {
                $('.dataTables_scrollBody thead tr').css({ visibility: 'collapse' });
            },
            columnDefs: [
                { width: 300, targets: 0 }
            ],
            fixedColumns: true,
            scrollCollapse: true,
            ordering: false,
            "language": {
                "lengthMenu": "_MENU_ &nbsp; per page"
            }
        });

        $('.datatable-draggable').DataTable({
            responsive: true,
            rowReorder: true,
            columnDefs: [
                { orderable: false, targets: [1, 2, 3, 4, 5, 6] },
                { orderable: false, className: 'roworder', targets: [1, 2, 3, 4, 5, 6] }
            ],
            rowReorder: {
                selector: 'td:nth-child(1), td:nth-child(3), td:nth-child(4), td:nth-child(5), td:nth-child(6)'
            },
        });

        $('.datatable').show();
    }

    $('.closeall').click(function () {
        $('.panel-collapse.in')
            .collapse('hide');
    });

    $('.openall').click(function () {
        $('.panel-collapse:not(".in")')
            .collapse('show');
    });


    if (jQuery().sortable) {
        $(function () {
            $(".column").sortable({
                connectWith: ".column",
                handle: ".portlet-content",
                revert: 0,
                cancel: ".portlet-toggle",
                placeholder: "portlet-placeholder ui-corner-all"
            });

            $(".portlet")
                .addClass("ui-widget ui-widget-content ui-helper-clearfix ui-corner-all")
                .find(".portlet-header")
                .addClass("ui-widget-header ui-corner-all")
                .prepend("<span class='ui-icon ui-icon-minusthick portlet-toggle'></span>");

            $(".portlet-toggle").on("click", function () {
                var icon = $(this);
                icon.toggleClass("ui-icon-minusthick ui-icon-plusthick");
                icon.closest(".portlet").find(".portlet-content").toggle();
            });
        });
    }


    if (jQuery().gridster) {
        $(".gridster ul").gridster({
            widget_margins: [10, 10],
            widget_base_dimensions: [200, 100],
            max_cols: 4
        });
    }

    $('#approval_option').on('change', function () {
        var chosen = $(this).val();

        if (chosen == "procurement") {
            $('.procurement_options').show();
        } else {
            $('.procurement_options').hide();
        }
    });

    if (jQuery().owlCarousel) {

        $('.step_feature').owlCarousel({
            video: true,
            items: 1,
            loop: false,
            dots: true,
            videoWidth: '100%',
            videoHeight: 370
        });

        $(".community-carousel").owlCarousel({
            responsive: {
                0: {
                    items: 1
                },
                768: {
                    items: 2
                },
                1200: {
                    items: 2
                }
            },
            margin: 30,
            nav: false,
            dots: true,
            loop: ($('.item').length > 1),
            autoplay: true,
            autplayHoverPause: true,
            autoplayTimeout: 3000
        });

        $(".articles-carousel").owlCarousel({
            responsive: {
                0: {
                    items: 1
                },
                480: {
                    items: 2
                },
                978: {
                    items: 1
                },
                1200: {
                    items: 2
                }
            },
            margin: 30,
            nav: false,
            dots: true,
            loop: true,
            autoplay: true,
            autplayHoverPause: true,
            autoplayTimeout: 3000
        });

    }

    $('.sidebar-menu.notifications a').on('mouseenter', function () {
        $('span', this).stop().animateCss('bounce');
    });

    $('.sidebar-menu li.updates-link a').on('mouseenter', function () {
        $('label', this).stop().animateCss('bounce');
    });


    $('select[name="app_tab"]').on('change', function () {
        var tab = $(this).val();
        $('.switch_tab').removeClass('active');
        $(tab).addClass('active');
        window.location.href = tab;
    });

    $(".modal-fullscreen").on('show.bs.modal', function () {
        setTimeout(function () {
            $(".modal-backdrop").addClass("modal-backdrop-fullscreen");
        }, 0);

        $(this).animate({ scrollTop: $(document).height() + $(window).height() });

    });
    $(".modal-fullscreen").on('hidden.bs.modal', function () {
        $(".modal-backdrop").addClass("modal-backdrop-fullscreen");
    });

    if (jQuery().mixItUp) {
        var $filterSelect = $('#mix-filters');
        var $sortSelect = $('#mix-sorting');
        $container = $('#mix-wrapper');
        $container.mixItUp();

        $filterSelect.on('change', function () {
            $container.mixItUp('filter', this.value);
        });

        $sortSelect.on('change', function () {
            $container.mixItUp('sort', this.value);
        });
    }


    if (jQuery().monthly) {
        $('#event-calendar').monthly({
            mode: 'event',
            xmlUrl: 'events.xml',
            stylePast: true
        });
    }

    var limit_domain = 'all';
    var limit_type = 'all';

    $('#domain-limit').on('change', function () {
        $('.sortable li').show();

        if ($(this).val() !== 'all') {
            var limit_domain = $(this).val();

            $('.sortable li').each(function () {
                if ($(this).hasClass(limit_domain)) {
                    // Match   
                } else {
                    $(this).hide();
                }
            });
        }
    });

    $('#type-limit').on('change', function () {
        $('.sortable li').show();

        if ($(this).val() !== 'all') {
            var limit_type = $(this).val();

            $('.sortable li').each(function () {
                if ($(this).hasClass(limit_type)) {
                    // Match   
                } else {
                    $(this).hide();
                }
            });
        }
    });


    if (jQuery().daterangepicker) {
        $('.daterange').daterangepicker({
            autoUpdateInput: false,
            cancelClass: "btn-danger",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase()
            }
        });
        $('.daterangetonow').daterangepicker({
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            drops: "down",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase()
            },
            timePicker: false,
            maxDate: moment()
        });
        $('.daterangefromnow').daterangepicker({
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            drops: "down",
            opens: "right",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase()
            },
            timePicker: false,
            minDate: moment()
        });
        $('.daterange').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase())).change();
            $('#date_range').html(picker.startDate.format($dateFormatByUser.toUpperCase()) + ' - ' + picker.endDate.format($dateFormatByUser.toUpperCase()));
        });

        $('.daterange').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val(null).change();
            $('#date_range').html('full history');
        });

        $('.singledate').daterangepicker({
            singleDatePicker: true,
            showDropdowns: true,
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase()
            }
        });

        $('.singledateandtime').daterangepicker({
            singleDatePicker: true,
            timePicker: true,
            autoApply: true,
            showDropdowns: true,
            autoUpdateInput: true,
            cancelClass: "btn-danger",
            opens: "left",
            locale: {
                cancelLabel: 'Clear',
                format: $dateFormatByUser.toUpperCase() + ' ' + $timeFormatByUser
    }
        });
    }

    if (jQuery().jscroll) {
        $('.lazy').jscroll({
            loadingHtml: '<img src="/Content/DesignStyle/img/loading.gif" alt="Loading" style="display: block; margin: 30px auto;" />',
            nextSelector: 'a.load-more:last',
            autoTrigger: true
        });
    }

    $('a.pin').bind('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('i', this).toggleClass('fa-thumb-tack');
        $('i', this).toggleClass('fa-check green');
    });


    if (jQuery().fancybox) {
        $(".image-pop").fancybox({
            beforeLoad: function () {
                var el, id = $(this.element).data('title-id');

                if (id) {
                    el = $('#' + id);

                    if (el.length) {
                        this.title = el.html();
                    }
                }
            },
            padding: 0
        });
    }


    if (jQuery().jstree) {

        $('.jstree').jstree({
            "core": {
                "themes": {
                    "dots": true,
                    "stripes": false,
                    "variant": "large"
                }
            },
            "search": {
                "case_insensitive": true,
                "show_only_matches": true
            },
            "plugins": ["themes", "search", "html_data", "ui", "wholerow"]
        });

        //$('.jstree').on("select_node.jstree", function (evt, data) {

        //    var node_id = (data.node.id); // element id
        //    var node = $("#" + node_id).data("node");
        //    var content = 'coa-' + node + '.php'; // get value of element attribute
        //    var toload = content;

        //    if ($(document).width() < 978) {
        //        $('html, body').animate({
        //            scrollTop: $('#content').offset().top - 120
        //        }, 'slow');
        //    }

        //    $('#content').html("<div class='text-center' style='margin-top: 50px;'><img src='dist/img/loading.gif' class='loader'></div>");

        //    $('#content').load(toload, function (response, status, xhr) {
        //        if (status == "error") {
        //            var msg = "<p class='text-center'>I haven't yet added the page to simulate a result here - Graham</p> ";
        //            $("#content").html(msg);
        //        }
        //    });

        //});




        $('.jstree').on('select_node.jstree', function (e, data) {
            data.instance.toggle_node(data.node);
        }).on("select_node.jstree", function (e, data) {
            if (data.instance.get_node(data.node, true).children('a').attr('href') != '#') {
                document.location = data.instance.get_node(data.node, true).children('a').attr('href');
            }
        });


        var showtree = function () {
            $('.treeview').fadeIn(800);
        };
        setTimeout(showtree, 200);

        $(".search-tree").keyup(function () {
            var searchString = $(this).val();
            $('.jstree').jstree('search', searchString);
        });
    }


    $('.report_date_type').on('change', function (e) {
        var type = $(this).val();
        if (type == "Custom date range") {
            $('.date_type_2').show();
            $('.reporting_period').html(type);
        } else {
            $('.date_type_2').hide();
            $('.reporting_period').html(type);
        }
    });

    $('.report_value').on('keyup', function () {
        var target = $(this).data('target');
        $(target).html($(this).val());
    });

    $('.report_toggle').on('change', function () {
        var target = $(this).data('target');

        if ($(this).prop("checked") == true) {
            $(target).fadeIn();
        } else {
            $(target).fadeOut();
        }
    });


    $('a.sidebar-toggle').bind('click', function () {
        $('body').toggleClass('user-defined');
        $('.footer_discussion').toggleClass('expanded');
    });

    $('.filtering_panel:not(.tree) .button').bind('click', function (e) {
        $('.filtering_panel').toggleClass('open');
        $('i', this).toggleClass('fa-sliders');
        $('i', this).toggleClass('fa-remove');
        $('.dimensions_trigger i').toggleClass('fa-hashtag');
        $('.dimensions_trigger i').toggleClass('fa-remove');
    });

    $('.filtering_panel.tree .button').bind('click', function (e) {
        $('.filtering_panel.tree').toggleClass('open');
        $('i', this).toggleClass('fa-bars');
        $('i', this).toggleClass('fa-remove');
    });

    $('#dimensions').bind('click', function (e) {
        e.preventDefault();
        $('.app_dimensions').toggle();
        $('i', this).toggleClass('fa-hashtag');
        $('i', this).toggleClass('fa-remove');
    });

    $('#locations').bind('click', function (e) {
        e.preventDefault();
        $('.app_locations').toggle();
        //$('i', this).toggleClass('fa-map-marker');
        //$('i', this).toggleClass('fa-remove');
    });

    $('#add_topic').bind('click', function () {
        $('#topic_button').html('<label class="label label-info" style="margin-bottom: 0;">Topic 2 x</label>');
    });

    $('#recurring').on('change', function () {
        var value = $(this).val();
        if (value != 'no') {
            $('#deadline').prop('disabled', true);
        }
        else {
            $('#deadline').prop('disabled', false);
        }
    });



    function toggleIcon(e) {
        $(e.target)
            .prev('.panel-heading')
            .find(".more-less")
            .toggleClass('fa-plus fa-minus');
    }
    $('.panel-group').on('hidden.bs.collapse', toggleIcon);
    $('.panel-group .panel-group').on('hidden.bs.collapse', toggleIcon);
    $('.panel-group').on('shown.bs.collapse', toggleIcon);
    $('.panel-group .panel-group').on('shown.bs.collapse', toggleIcon);


    function toggleIcon2(e) {
        $(e.target)
            .prev('.panel-heading')
            .find(".more-less")
            .toggleClass('fa-minus fa-plus');
    }
    $('.panel-group.inverted').on('hidden.bs.collapse', toggleIcon2);
    $('.panel-group.inverted .panel-group').on('hidden.bs.collapse', toggleIcon2);
    $('.panel-group.inverted').on('shown.bs.collapse', toggleIcon2);
    $('.panel-group.inverted .panel-group').on('shown.bs.collapse', toggleIcon2);



    //if (jQuery().typeahead) {

    //    var substringMatcher = function (strs) {
    //        return function findMatches(q, cb) {
    //            var matches, substringRegex;

    //            // an array that will be populated with substring matches
    //            matches = [];

    //            // regex used to determine if a string contains the substring `q`
    //            substrRegex = new RegExp(q, 'i');

    //            // iterate through the pool of strings and for any string that
    //            // contains the substring `q`, add it to the `matches` array
    //            $.each(strs, function (i, str) {
    //                if (substrRegex.test(str)) {
    //                    matches.push(str);
    //                }
    //            });

    //            cb(matches);
    //        };
    //    };

    //    var topics = ['Assignments', 'General', 'Sales'];

    //    $('.typeahead').typeahead({
    //        hint: true,
    //        highlight: true,
    //        minLength: 1
    //    },
    //    {
    //        name: 'topics',
    //        source: substringMatcher(topics)
    //    });


    //}

    $('#task_type').on('change', function () {
        var target = $(this).val();

        $('.form-builder .switch').addClass('hidden-xs');
        $('.form-builder .switch').addClass('hidden-sm');
        $('.form-builder .switch').addClass('hidden-md');
        $('.form-builder .switch').addClass('hidden-lg');
        $(target).removeClass('hidden-xs');
        $(target).removeClass('hidden-sm');
        $(target).removeClass('hidden-md');
        $(target).removeClass('hidden-lg');
    });


    if (jQuery().select2) {
        $(".chosen-select").select2();
        $(".chosen-multiple").select2({ multiple: true });

        $("ul.select2-selection__rendered").sortable({
            containment: 'parent'
        });

        $('.select2').on('select2:select', function (e) {
            $("ul.select2-selection__rendered").sortable({
                containment: 'parent'
            });
        });
        if ($(document).width() < 768) {
            $(".select2, .select2-multiple").on('select2:open', function (e) {
                $('.select2-search input').prop('focus', false);
            });
        }
    }



    if (jQuery().datetimepicker) {

        $('.dtpicker').datetimepicker({
            lang: 'en',
            format: 'd/m/Y H:i',

            onShow: function (ct) {
                this.setOptions({
                    maxDate: jQuery('#date_timepicker_end').val() ? jQuery('#date_timepicker_end').val() : false
                })
            },
            timepicker: true
        });

        $('.dtpicker2').datetimepicker({
            lang: 'en',
            format: 'd/m/Y',

            onShow: function (ct) {
                this.setOptions({
                    maxDate: jQuery('#date_timepicker_end').val() ? jQuery('#date_timepicker_end').val() : false
                })
            },
            timepicker: false
        });

        $("body").on('focus', 'input[type="date"]', function () {
            $(this).datetimepicker('show');
        });

    }

    $('.login-body').animateCss('fadeInUp');

    $('.page_refresh').bind('click', function (e) {
        e.preventDefault();
        window.location.reload();
    });

    $('.portlet.rework input[type="checkbox"]').bind('click', function () {
        $(this).closest('.portlet').toggleClass('toggled');

        if ($(this).prop("checked") == true) {
            $('.alert_matches.projects').addClass('active');
        } else {
            $('.alert_matches.projects').removeClass('active');
        }
    });

    $('.reply_options a').bind('click', function (e) {
        //$('.quick-reply').hide();
        e.preventDefault();
        var target = "." + $(this).data('target');
        $(target).toggle();
    });

    $('.reply_cancel').bind('click', function () {
        $('.quick-reply').hide();
    });


    if (jQuery().validate) {

        // Create journal entry form validation
        $('#new-journal-entry').validate(
            {
                rules: {
                    debit1: {
                        required: true,
                        number: true,
                        onkeyup: true
                    }
                }
            });

        // Login form validation
        $('#login').validate(
            {
                rules: {
                    email: {
                        required: true,
                        email: true,
                        maxlength: 300
                    },
                    password: {
                        required: true,
                        maxlength: 300
                    }
                }
            });

        // Reset password
        $('#password-reset').validate(
            {
                rules: {
                    password: {
                        required: true,
                        maxlength: 300
                    },
                    confirm_password: {
                        required: true,
                        maxlength: 300,
                        equalTo: '#password'
                    }
                }
            });

        // Create task
        $('#form_task_create').validate(
            {
                ignore: 'input[type=hidden]',
                errorClass: "error",
                errorPlacement: function (error, element) {
                    var elem = $(element);
                    error.insertAfter(element);
                },
                rules: {
                    task_title: {
                        required: true,
                        maxlength: 300
                    },
                    assign_to: {
                        required: true
                    }
                },
                highlight: function (element, errorClass, validClass) {

                    var elem = $(element);

                    elem.addClass(errorClass);

                },
                unhighlight: function (element, errorClass, validClass) {
                    var elem = $(element);

                    if (elem.hasClass('sl')) {
                        elem.siblings('.sl').find('.select2-choice').removeClass(errorClass);
                    } else {
                        elem.removeClass(errorClass);
                    }
                }
            });

    }


    //$('#approval_type').on('change', function () {
    //    if ($(this).val() == "journal") {
    //        $('#initiates').hide();
    //    } else {
    //        $('#initiates').show();
    //    }
    //});

    $('.toggle_display').on('change', function (e) {
        var show = '#' + $(this).val();
        $('.toggleable').hide();
        $(show).show();
        $('.title_type').html($(this).val());
    });

    $('.step_edit').bind('click', function () {
        var target = $(this).data('target');
        $('.step_add').hide();
        $(target).show();
    });


    // Cycle app nav tabs with button triggers
    $('.btnNext').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('.app_subnav .active').next('li').find('a').trigger('click');
    });

    $('.btnPrevious').click(function () {
        var parent = $(this).closest('.modal');
        $(parent).find('.app_subnav .active').prev('li').find('a').trigger('click');
    });


    // Cycle form tabs with button triggers
    $('.next-form').click(function (e) {
        e.preventDefault();
        $('.navigate_forms > .active').next('li').find('a').trigger('click');
    });

    $('.previous-form').click(function (e) {
        e.preventDefault();
        $('.navigate_forms > .active').prev('li').find('a').trigger('click');
    });
});
function manage_options(target) {
    $('.sidebar-options').hide();
    $(target).show();
}
