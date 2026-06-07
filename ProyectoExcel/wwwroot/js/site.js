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

	// Show loading overlay on any form submit
	document.addEventListener('submit', function () {
		var o = document.getElementById('loadingOverlay');
		if (o) o.classList.remove('d-none');
	});
	// Bootstrap tooltip initialization
	document.addEventListener('DOMContentLoaded', function () {
		document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function(el){
			new bootstrap.Tooltip(el);
		});
	});
	// Prevent double submit
	document.addEventListener('submit', function (e) {
		var btn = e.target.querySelector('[type="submit"]:not([data-allow-multi])');
		if (btn) setTimeout(function(){ btn.disabled = true; }, 0);
	});
})();
