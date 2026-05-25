(function () {
	'use strict';

	// --- Dark mode toggle ---
	const THEME_KEY = 'theme';
	const html = document.documentElement;
	const toggle = document.getElementById('themeToggle');
	const icon = document.getElementById('themeIcon');

	function getIconClass(theme, filled) {
		var base = theme === 'dark' ? 'bi-sun' : 'bi-moon';
		return 'bi ' + base + (filled ? '-fill' : '');
	}

	function applyTheme(theme) {
		html.setAttribute('data-bs-theme', theme);
		if (icon) {
			icon.className = getIconClass(theme, false);
		}
	}

	var saved = localStorage.getItem(THEME_KEY);
	if (!saved) {
		saved = window.matchMedia('(prefers-color-scheme: dark)').matches
			? 'dark'
			: 'light';
	}
	applyTheme(saved);

	if (toggle) {
		toggle.addEventListener('click', function () {
			var current = html.getAttribute('data-bs-theme') || 'light';
			var next = current === 'dark' ? 'light' : 'dark';
			localStorage.setItem(THEME_KEY, next);
			applyTheme(next);
		});

		toggle.addEventListener('mouseenter', function () {
			var theme = html.getAttribute('data-bs-theme') || 'light';
			if (icon) icon.className = getIconClass(theme, true);
		});

		toggle.addEventListener('mouseleave', function () {
			var theme = html.getAttribute('data-bs-theme') || 'light';
			if (icon) icon.className = getIconClass(theme, false);
		});
	}

	// --- Auto-dismiss success alerts after 5 seconds ---
	document.querySelectorAll('.alert-success').forEach(function (alert) {
		setTimeout(function () {
			alert.classList.add('fade-out');
			setTimeout(function () {
				alert.remove();
			}, 500);
		}, 5000);
	});
})();
