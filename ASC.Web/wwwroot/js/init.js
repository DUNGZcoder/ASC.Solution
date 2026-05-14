(function ($) {

    $(function () {

        $('.sidenav').sidenav();
        $('.parallax').parallax();

        // Prevent browser back and forward buttons.
        if (window.history && window.history.pushState) {

            window.history.pushState('forward', null, window.location.href);

            $(window).on('popstate', function () {

                window.history.pushState('forward', null, window.location.href);

            });

        }

        // Prevent right-click on entire window
        $(document).ready(function () {

            $(window).on("contextmenu", function () {

                return false;

            });

        });

    }); // end of document ready

})(jQuery); // end of jQuery name space