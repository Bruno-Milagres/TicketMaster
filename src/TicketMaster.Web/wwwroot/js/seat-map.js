// ==============================================
// D3 — MAPA DE ASSENTOS SVG INTERATIVO
// ==============================================
const seatState = {};

function applySeatStatus(seatId, status) {
    var el = document.querySelector('[data-seat="' + seatId + '"]');
    if (!el) return;
    el.classList.remove('available', 'held', 'selected', 'unavailable');
    var classes = ['available', 'held', 'selected', 'unavailable'];
    el.classList.add(classes[status] || 'unavailable');
    seatState[seatId] = status;
}

async function initSeatMap(eventId) {
    try {
        var stateMap = await fetch('/api/events/' + eventId + '/seats').then(function(r) { return r.json(); });
        Object.keys(stateMap).forEach(function(id) {
            applySeatStatus(id, stateMap[id]);
        });

        document.querySelectorAll('[data-seat]').forEach(function(el) {
            el.addEventListener('click', function() {
                var seatId = el.dataset.seat;
                if (seatState[seatId] !== 0) return;
                if (typeof connection !== 'undefined' && connection) {
                    connection.invoke('ReservarAssento', seatId)
                        .catch(function(err) {
                            if (typeof showToast === 'function') {
                                showToast('Erro ao reservar: ' + err, 'danger');
                            }
                        });
                }
            });
        });
    } catch (e) {
        console.error('Erro ao carregar mapa de assentos', e);
    }
}

// Conexão com SignalR — receber atualizações em tempo real
if (typeof connection !== 'undefined' && connection) {
    connection.on('AtualizarAssento', function(seatId, status) {
        applySeatStatus(seatId, status === 'Disponivel' ? 0
            : status === 'Reservado' ? 1
            : status === 'Vendido' ? 2 : 3);
    });
}
