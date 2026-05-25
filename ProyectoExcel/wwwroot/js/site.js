(function () {
    'use strict';

    // --- Dark mode toggle ---
    const THEME_KEY = 'theme';
    const html = document.documentElement;
    const toggle = document.getElementById('themeToggle');
    const icon = document.getElementById('themeIcon');

    function applyTheme(theme) {
        html.setAttribute('data-bs-theme', theme);
        if (icon) {
            icon.textContent = theme === 'dark' ? '\u2600' : '\u263E';
        }
    }

    var saved = localStorage.getItem(THEME_KEY);
    if (!saved) {
        saved = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    applyTheme(saved);

    if (toggle) {
        toggle.addEventListener('click', function () {
            var current = html.getAttribute('data-bs-theme') || 'light';
            var next = current === 'dark' ? 'light' : 'dark';
            localStorage.setItem(THEME_KEY, next);
            applyTheme(next);
        });
    }

    // --- Auto-dismiss success alerts after 5 seconds ---
    document.querySelectorAll('.alert-success').forEach(function (alert) {
        setTimeout(function () {
            alert.classList.add('fade-out');
            setTimeout(function () { alert.remove(); }, 500);
        }, 5000);
    });
})();
