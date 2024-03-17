/*!
    * Start Bootstrap - SB Admin Pro v2.0.5 (https://shop.startbootstrap.com/product/sb-admin-pro)
    * Copyright 2013-2023 Start Bootstrap
    * Licensed under SEE_LICENSE (https://github.com/StartBootstrap/sb-admin-pro/blob/master/LICENSE)
    */
window.addEventListener('DOMContentLoaded', event => {
    // Activate feather
    feather.replace();

    // Enable tooltips globally
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Enable popovers globally
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        // Uncomment Below to persist sidebar toggle between refreshes
        // if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
        //     document.body.classList.toggle('sidenav-toggled');
        // }
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sidenav-toggled'));
        });
    }

    // Close side navigation when width < LG
    const sidenavContent = document.body.querySelector('#layoutSidenav_content');
    if (sidenavContent) {
        sidenavContent.addEventListener('click', event => {
            const BOOTSTRAP_LG_WIDTH = 992;
            if (window.innerWidth >= 992) {
                return;
            }
            if (document.body.classList.contains("sidenav-toggled")) {
                document.body.classList.toggle("sidenav-toggled");
            }
        });
    }


    // ** to use this code, must add JQuery at html , example 
    // ** <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    // ** <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>

    // ** use URL to match
    //$(function () {
    //    // Add active state to sidbar nav links
    //    let path = window.location.pathname;
    //    let query = window.location.search;

    //    // Check if path ends with .html or .aspx
    //    let matches = path.match(/([\w-]+\.(html|aspx))/);

    //    if (matches) {
    //        let activatedPath = matches[0];
    //        let targetAnchor = $('[href="' + activatedPath + query + '"]');
    //        let collapseAncestors = targetAnchor.parents('.collapse');

    //        targetAnchor.addClass('active');

    //        collapseAncestors.each(function () {
    //            $(this).addClass('show');
    //            let dataTarget = '#' + $(this).attr('id');
    //            $('[data-bs-target="' + dataTarget + '"]').removeClass('collapsed');
    //        });
    //    }
    //});

    $(function ()
    {
        // Add active state to sidbar nav links
        let path = window.location.pathname;
        let query = window.location.search;
        let searchParams = new URLSearchParams(window.location.search);
        let tempID = searchParams.get('ID');

        // Check if path ends with .html or .aspx
        let matches = path.match(/([\w-]+\.(html|aspx))/);

        if (matches) {
            let activatedPath = matches[0];
            //let linkId1 = 'HPSub2WS1Dashboard';  //$('[href="' + activatedPath + '"]').attr('id');
            //let targetAnchor1 = $('[href="' + activatedPath + query + '"]');
            let targetAnchor = $('#' + tempID + '.nav-link');

            let collapseAncestors = targetAnchor.parents('.collapse');

            targetAnchor.addClass('active');

            collapseAncestors.each(function () {
                $(this).addClass('show');
                let dataTarget = '#' + $(this).attr('id');
                $('[data-bs-target="' + dataTarget + '"]').removeClass('collapsed');
            });
        }
    });




});
