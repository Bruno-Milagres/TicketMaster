// ==============================================
// THEME TOGGLE — Persiste em LocalStorage
// ==============================================
(function () {
    var stored = localStorage.getItem('tm-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', stored);
    // Atualiza o ícone assim que o DOM estiver pronto
    document.addEventListener('DOMContentLoaded', function () {
        updateToggleIcon(stored);
    });
})();

function updateToggleIcon(theme) {
    var btn = document.getElementById('btn-theme-toggle');
    if (!btn) return;
    var icon = btn.querySelector('i');
    if (!icon) {
        icon = document.createElement('i');
        btn.appendChild(icon);
    }
    // Apenas UM ícone — mutuamente exclusivo
    icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
}

(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('btn-theme-toggle');
        if (!btn) return;
        btn.addEventListener('click', function () {
            var current = document.documentElement.getAttribute('data-theme') || 'dark';
            var next = current === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-theme', next);
            localStorage.setItem('tm-theme', next);
            updateToggleIcon(next);
        });
    });
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
    var copyClasses = ['card-img-top', 'rounded-start', 'rounded-end', 'placeholder-img', 'seat-map-banner', 'checkout-image'];
    copyClasses.forEach(function(cls) { if (img.classList.contains(cls)) fallback.classList.add(cls); });
    img.parentNode.replaceChild(fallback, img);
}

document.addEventListener('DOMContentLoaded', function () {
    // Fallback imagens
    document.querySelectorAll('img').forEach(function(img) {
        if (img.complete && (img.naturalWidth === 0 || img.naturalHeight === 0)) {
            handleImageError(img);
        } else {
            img.addEventListener('error', function() { handleImageError(img); });
        }
    });

    // Loading states em formulários
    document.querySelectorAll('form[data-loading]').forEach(function(form) {
        form.addEventListener('submit', function() {
            var btn = form.querySelector('[type="submit"]');
            if (!btn || btn.disabled) return;
            btn.disabled = true;
            btn.dataset.originalText = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Aguarde...';
            setTimeout(function() { btn.disabled = false; btn.innerHTML = btn.dataset.originalText; }, 10000);
        });
    });
});

// A3 — Toast de feedback
function showToast(message, type) {
    if (!message) return;
    type = type || 'success';
    var container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.id = 'toast-container';
        document.body.appendChild(container);
    }
    var id = 'toast-' + Date.now();
    container.insertAdjacentHTML('beforeend',
        '<div id="' + id + '" class="toast align-items-center text-bg-' + type + ' border-0" role="alert">' +
        '<div class="d-flex"><div class="toast-body">' + message + '</div>' +
        '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div></div>');
    var el = document.getElementById(id);
    new bootstrap.Toast(el, { delay: 4000 }).show();
    el.addEventListener('hidden.bs.toast', function() { el.remove(); });
}
window.showToast = showToast;
