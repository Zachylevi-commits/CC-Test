// Progressive enhancement for the Open Heavens Church site. All interactive
// behaviour degrades gracefully when JS is disabled.

(function () {
    'use strict';

    // Mobile nav toggle
    document.querySelectorAll('[data-component="site-header"] .nav-toggle').forEach(function (toggle) {
        var nav = document.getElementById(toggle.getAttribute('aria-controls'));
        if (!nav) { return; }

        toggle.addEventListener('click', function () {
            var open = nav.classList.toggle('is-open');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    });

    // Tabs (WAI-ARIA pattern)
    document.querySelectorAll('[data-component="tabs"]').forEach(function (root) {
        var tabs = Array.prototype.slice.call(root.querySelectorAll('[role="tab"]'));
        var panels = tabs.map(function (t) { return document.getElementById(t.getAttribute('aria-controls')); });

        function activate(index) {
            tabs.forEach(function (t, i) {
                var active = i === index;
                t.setAttribute('aria-selected', active ? 'true' : 'false');
                t.setAttribute('tabindex', active ? '0' : '-1');
                t.classList.toggle('is-active', active);
                if (panels[i]) {
                    panels[i].classList.toggle('is-active', active);
                    panels[i].hidden = !active;
                }
            });
        }

        tabs.forEach(function (tab, i) {
            tab.addEventListener('click', function () { activate(i); });
            tab.addEventListener('keydown', function (e) {
                if (e.key === 'ArrowRight') {
                    activate((i + 1) % tabs.length);
                    tabs[(i + 1) % tabs.length].focus();
                } else if (e.key === 'ArrowLeft') {
                    var prev = (i - 1 + tabs.length) % tabs.length;
                    activate(prev);
                    tabs[prev].focus();
                } else if (e.key === 'Home') {
                    activate(0); tabs[0].focus();
                } else if (e.key === 'End') {
                    activate(tabs.length - 1); tabs[tabs.length - 1].focus();
                }
            });
        });
    });

    // Song platform menus ("Listen on…")
    function closeAllSongMenus(except) {
        document.querySelectorAll('.song-card__menu').forEach(function (m) {
            if (m === except) { return; }
            m.hidden = true;
            var t = document.querySelector('[aria-controls="' + m.id + '"]');
            if (t) { t.setAttribute('aria-expanded', 'false'); }
        });
    }

    document.querySelectorAll('.song-card__menu-toggle').forEach(function (toggle) {
        var menu = document.getElementById(toggle.getAttribute('aria-controls'));
        if (!menu) { return; }

        toggle.addEventListener('click', function (e) {
            e.stopPropagation();
            var willOpen = menu.hidden;
            closeAllSongMenus(menu);
            menu.hidden = !willOpen;
            toggle.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
            if (willOpen) {
                var firstLink = menu.querySelector('a');
                if (firstLink) { firstLink.focus(); }
            }
        });

        menu.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                menu.hidden = true;
                toggle.setAttribute('aria-expanded', 'false');
                toggle.focus();
            }
        });
    });

    document.addEventListener('click', function () { closeAllSongMenus(null); });
})();
