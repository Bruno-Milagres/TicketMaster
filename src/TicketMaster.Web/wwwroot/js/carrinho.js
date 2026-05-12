// ==============================================
// CARRINHO DE COMPRAS — TicketMaster
// Gerencia estado no localStorage, sincroniza UI
// ==============================================
const Carrinho = (function () {
    const STORAGE_KEY = 'tm-carrinho';

    function obter() {
        try {
            return JSON.parse(localStorage.getItem(STORAGE_KEY)) || [];
        } catch { return []; }
    }

    function salvar(itens) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(itens));
        atualizarContador();
        dispararEvento();
    }

    function dispararEvento() {
        window.dispatchEvent(new CustomEvent('carrinho:atualizado'));
    }

    // --- API pública ---

    function adicionar(tipoIngressoId, eventId, nomeEvento, nomeIngresso, preco, quantidade) {
        var itens = obter();
        var existente = itens.find(function (i) {
            return i.tipoIngressoId === tipoIngressoId && i.eventId === eventId;
        });

        if (existente) {
            existente.quantidade += quantidade || 1;
        } else {
            itens.push({
                tipoIngressoId: tipoIngressoId,
                eventId: eventId,
                nomeEvento: nomeEvento,
                nomeIngresso: nomeIngresso,
                preco: preco,
                quantidade: quantidade || 1
            });
        }
        salvar(itens);
    }

    function remover(tipoIngressoId, eventId) {
        var itens = obter().filter(function (i) {
            return !(i.tipoIngressoId === tipoIngressoId && i.eventId === eventId);
        });
        salvar(itens);
    }

    function atualizarQuantidade(tipoIngressoId, eventId, quantidade) {
        var itens = obter();
        var item = itens.find(function (i) {
            return i.tipoIngressoId === tipoIngressoId && i.eventId === eventId;
        });
        if (item) {
            if (quantidade <= 0) {
                remover(tipoIngressoId, eventId);
                return;
            }
            item.quantidade = quantidade;
            salvar(itens);
        }
    }

    function limpar() {
        salvar([]);
    }

    function totalItens() {
        return obter().reduce(function (acc, i) { return acc + i.quantidade; }, 0);
    }

    function totalPreco() {
        return obter().reduce(function (acc, i) { return acc + (i.preco * i.quantidade); }, 0);
    }

    function formatarPreco(valor) {
        return 'R$ ' + valor.toFixed(2).replace('.', ',');
    }

    function atualizarContador() {
        var total = totalItens();
        document.querySelectorAll('.carrinho-contador').forEach(function (el) {
            el.textContent = total;
            el.style.display = total > 0 ? 'inline' : 'none';
        });
    }

    // --- Render do carrinho na página ---
    function renderizarCarrinho(containerId) {
        var container = document.getElementById(containerId);
        if (!container) return;

        var itens = obter();
        if (itens.length === 0) {
            container.innerHTML = '<div class="alert alert-info text-center my-4">Seu carrinho está vazio.</div>';
            return;
        }

        var html = '<div class="table-responsive"><table class="table table-carrinho"><thead><tr>' +
            '<th>Evento</th><th>Ingresso</th><th>Preço Unit.</th><th>Qtd</th><th>Subtotal</th><th></th>' +
            '</tr></thead><tbody>';

        itens.forEach(function (item, index) {
            var subtotal = item.preco * item.quantidade;
            html += '<tr data-index="' + index + '">' +
                '<td>' + escapeHtml(item.nomeEvento) + '</td>' +
                '<td>' + escapeHtml(item.nomeIngresso) + '</td>' +
                '<td class="text-nowrap">' + formatarPreco(item.preco) + '</td>' +
                '<td><input type="number" class="form-control form-control-sm qtd-input" ' +
                'style="width:70px" value="' + item.quantidade + '" min="1" ' +
                'data-tipo="' + item.tipoIngressoId + '" data-evento="' + item.eventId + '" /></td>' +
                '<td class="text-nowrap subtotal-cell">' + formatarPreco(subtotal) + '</td>' +
                '<td><button class="btn btn-sm btn-outline-danger btn-remover" ' +
                'data-tipo="' + item.tipoIngressoId + '" data-evento="' + item.eventId + '">' +
                '<i class="fa-solid fa-trash"></i></button></td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        html += '<div class="d-flex justify-content-between align-items-center mt-3">' +
            '<h4 class="mb-0">Total: <span id="carrinho-total">' + formatarPreco(totalPreco()) + '</span></h4>' +
            '<div class="d-flex gap-2">' +
            '<button class="btn btn-outline-secondary" id="btn-limpar-carrinho">Limpar</button>' +
            '<a href="/Carrinho/Checkout" class="btn btn-success"><i class="fa-solid fa-credit-card me-1"></i>Finalizar Compra</a>' +
            '</div></div>';

        container.innerHTML = html;

        // Eventos
        container.querySelectorAll('.qtd-input').forEach(function (input) {
            input.addEventListener('change', function () {
                var qtd = parseInt(this.value) || 1;
                atualizarQuantidade(this.dataset.tipo, this.dataset.evento, qtd);
                renderizarCarrinho(containerId);
            });
        });

        container.querySelectorAll('.btn-remover').forEach(function (btn) {
            btn.addEventListener('click', function () {
                remover(this.dataset.tipo, this.dataset.evento);
                renderizarCarrinho(containerId);
            });
        });

        var btnLimpar = document.getElementById('btn-limpar-carrinho');
        if (btnLimpar) {
            btnLimpar.addEventListener('click', function () {
                if (confirm('Limpar todos os itens do carrinho?')) {
                    limpar();
                    renderizarCarrinho(containerId);
                }
            });
        }
    }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }

    // --- Inicialização ---
    function init() {
        // Atualiza contador nos headers
        atualizarContador();

        // Se estiver na página do carrinho, renderiza
        if (document.getElementById('carrinho-container')) {
            renderizarCarrinho('carrinho-container');
        }

        // Botões "Adicionar ao carrinho" — data attributes
        document.querySelectorAll('.btn-add-carrinho').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                adicionar(
                    this.dataset.tipoId,
                    this.dataset.eventId,
                    this.dataset.eventoNome,
                    this.dataset.ingressoNome,
                    parseFloat(this.dataset.preco),
                    parseInt(this.dataset.quantidade) || 1
                );
                // Feedback visual
                var original = this.innerHTML;
                this.innerHTML = '<i class="fa-solid fa-check"></i> Adicionado!';
                this.classList.remove('btn-primary');
                this.classList.add('btn-success');
                setTimeout(function () {
                    this.innerHTML = original;
                    this.classList.remove('btn-success');
                    this.classList.add('btn-primary');
                }.bind(this), 1500);
            });
        });
    }

    // Inicializa quando o DOM estiver pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return {
        obter: obter,
        adicionar: adicionar,
        remover: remover,
        atualizarQuantidade: atualizarQuantidade,
        limpar: limpar,
        totalItens: totalItens,
        totalPreco: totalPreco,
        formatarPreco: formatarPreco,
        renderizarCarrinho: renderizarCarrinho,
        atualizarContador: atualizarContador
    };
})();
