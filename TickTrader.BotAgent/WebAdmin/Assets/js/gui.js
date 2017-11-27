var mobile_menu_initialized = false;
var toggle_initialized = false;
var mobile_menu_visible = 0;

var lbd = {
    misc: {
        navbar_menu_visible: 0
    },

    initRightMenu: debounce(function () {
        if (!toggle_initialized) {
            $toggle = $('.navbar-toggle');
            $layer = $('.close-layer');

            $toggle.click(function () {

                if (mobile_menu_visible == 1) {
                    $('html').removeClass('nav-open');
                    $layer.removeClass('visible');

                    setTimeout(function () {
                        $toggle.removeClass('toggled');
                    }, 400);

                    setTimeout(function () {
                        $('.close-layer').remove();
                    }, 100);

                    mobile_menu_visible = 0;
                } else {
                    setTimeout(function () {
                        $toggle.addClass('toggled');
                    }, 430);

                    $layer = $('<div class="close-layer"/>');
                    $layer.appendTo(".wrapper");

                    setTimeout(function () {
                        $layer.addClass('visible');
                    }, 100);

                    $('.close-layer').on("click", function () {
                        $toggle = $('.navbar-toggle');
                        $('html').removeClass('nav-open');

                        $layer.removeClass('visible');

                        setTimeout(function () {
                            $('.close-layer').remove();
                            $toggle.removeClass('toggled');
                        }, 370);

                        mobile_menu_visible = 0;
                    });

                    $('html').addClass('nav-open');
                    mobile_menu_visible = 1;

                }

            });

            toggle_initialized = true;
        }
    }, 200)
}

$(document).ready(function () {
    window_width = $(window).width();
    $sidebar = $('.sidebar');

    if (window_width <= 991) {
        lbd.initRightMenu();
    }
});

$(window).resize(function () {
    if ($sidebar.length != 0) {
        lbd.initRightMenu();
    }
});

function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        }, wait);
        if (immediate && !timeout) func.apply(context, args);
    };
};
