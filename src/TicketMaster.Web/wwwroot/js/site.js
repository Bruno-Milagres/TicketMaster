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

// ==============================================
// FALLBACK PARA IMAGENS QUEBRADAS
// ==============================================
function handleImageError(img) {
    if (img.dataset.fallback === 'applied') return;
    img.dataset.fallback = 'applied';

    var fallback = document.createElement('div');
    fallback.className = 'img-fallback d-flex flex-column align-items-center justify-content-center';
    fallback.innerHTML = '<i class="fa-solid fa-image fa-3x mb-2"></i><span class="small">Imagem indisponível</span>';

    // Copia classes relevantes do img original (ex: card-img-top, rounded-start, etc.)
    var copyClasses = ['card-img-top', 'rounded-start', 'rounded-end', 'placeholder-img', 'seat-map-banner', 'checkout-image'];
    copyClasses.forEach(function(cls) {
        if (img.classList.contains(cls)) fallback.classList.add(cls);
    });

    img.parentNode.replaceChild(fallback, img);
}

document.addEventListener('DOMContentLoaded', function () {
    // Tema claro/escuro
    const btn = document.getElementById('btn-theme-toggle');
    if (btn) {
        function applyTheme(theme) {
            document.documentElement.setAttribute('data-theme', theme);
            localStorage.setItem('tm-theme', theme);
        }

        btn.addEventListener('click', function () {
            const current = document.documentElement.getAttribute('data-theme');
            applyTheme(current === 'dark' ? 'light' : 'dark');
        });
    }

    // Fallback para imagens quebradas
    document.querySelectorAll('img').forEach(function(img) {
        if (img.complete && (img.naturalWidth === 0 || img.naturalHeight === 0)) {
            handleImageError(img);
        } else {
            img.addEventListener('error', function() { handleImageError(img); });
        }
    });
});
