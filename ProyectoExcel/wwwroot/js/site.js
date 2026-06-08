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

	window.showLoadingOverlay = function () {
		var o = document.getElementById('loadingOverlay');
		if (!o) return;

		var status = o.querySelector('[data-loading-status]');
		var text = o.getAttribute('data-loading-text') || 'Loading...';
		if (status) {
			status.textContent = '';
			status.textContent = text;
		}

		o.classList.remove('d-none');
		o.setAttribute('aria-hidden', 'false');
		document.body.setAttribute('aria-busy', 'true');
	};

	window.hideLoadingOverlay = function () {
		var o = document.getElementById('loadingOverlay');
		if (!o) return;

		o.classList.add('d-none');
		o.setAttribute('aria-hidden', 'true');
		document.body.removeAttribute('aria-busy');
	};

	// Show loading overlay and prevent double submit on successful form submission
	document.addEventListener('submit', function (e) {
		setTimeout(function () {
			if (e.defaultPrevented) return;

			showLoadingOverlay();

			var submitter = e.submitter;
			if (submitter) {
				if (!submitter.hasAttribute('data-allow-multi')) {
					submitter.disabled = true;
				}
			} else if (e.target && typeof e.target.querySelector === 'function') {
				var btn = e.target.querySelector('[type="submit"]:not([data-allow-multi])');
				if (btn) btn.disabled = true;
			}
		}, 0);
	});
	function initTooltips() {
		if (typeof bootstrap === 'undefined' || !bootstrap.Tooltip) return;
		document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
			if (!bootstrap.Tooltip.getInstance(el)) {
				new bootstrap.Tooltip(el);
			}
		});
	}

	window.initTooltips = initTooltips;
	document.addEventListener('DOMContentLoaded', initTooltips);
})();
