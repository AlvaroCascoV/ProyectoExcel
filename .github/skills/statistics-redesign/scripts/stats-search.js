// Paste inside @section Scripts { } AFTER the existing chart initialization block
(function () {
    'use strict';
    var input   = document.getElementById('statsSearch');
    var countEl = document.getElementById('statsSearchCount');
    var tbody   = document.querySelector('.ta-table-sticky tbody');
    if (!input || !tbody) return;
    input.addEventListener('input', function () {
        var q = input.value.trim().toLowerCase();
        var rows = tbody.querySelectorAll('tr');
        var n = 0;
        rows.forEach(function (row) {
            var name = row.cells[1] ? row.cells[1].textContent.toLowerCase() : '';
            var show = !q || name.includes(q);
            row.style.display = show ? '' : 'none';
            if (show) n++;
        });
        if (countEl) {
            countEl.textContent = q ? n + ' resultado' + (n !== 1 ? 's' : '') : '';
            countEl.style.display = q ? 'inline' : 'none';
        }
    });
})();
