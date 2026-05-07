// ==============================================
// TEMA CLARO / ESCURO
// ==============================================

(function () {
    const STORAGE_KEY = 'tm-theme';
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const saved = localStorage.getItem(STORAGE_KEY);
    const theme = saved ?? (prefersDark ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', theme);
})();

document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('btn-theme-toggle');
    if (!btn) return;

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('tm-theme', theme);
    }

    btn.addEventListener('click', function () {
        const current = document.documentElement.getAttribute('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    });
});
