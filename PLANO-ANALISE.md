# 🎫 TicketMaster — Análise e Plano de Evolução

> **Stack:** .NET 9 · ASP.NET Core · EF Core · SQL Server · MassTransit + RabbitMQ · SignalR · OpenTelemetry · Serilog · xUnit + Moq  
> **Arquitetura:** Clean Architecture (Domain → Application → Infrastructure → Web)  
> **Frontend:** Razor Pages + Bootstrap 5 · Tema claro/escuro · CSS Grid (mapa de assentos)

---

## ✅ O Que Já Está Pronto

### Domínio (`TicketMaster.Domain`)

| Componente | Status | Observações |
|---|---|---|
| **Entidade `Ticket`** | ✅ Completo | State machine robusta: `Disponivel → Reservado → Vendido`. Controle de versão para concorrência otimista. Expiração de reserva (15 min). |
| **Entidade `Event`** | ✅ Estrutura básica | Id, Title, EventDate, RoomId. Anêmica — precisa de comportamento. |
| **Entidade `Room`** | ✅ Estrutura básica | Layout armazenado como JSON no EF Core. `SeatCoordinate` com tipo (Standard/VIP/Cadeirante). |
| **`Result` Pattern** | ✅ Feito | Evita exceptions para fluxos esperados (assento ocupado, reserva expirada). |
| **`ConcurrencyException`** | ✅ Feito | Isolada do EF — domínio não depende de infra. |

### Aplicação (`TicketMaster.Application`)

| Componente | Status | Observações |
|---|---|---|
| **`TicketService`** | ✅ Completo | Reservar, confirmar pagamento, expirar reservas, cancelar. Tratamento de concorrência com mensagem amigável. |
| **`EventService`** | ✅ Feito | Listagem de eventos ativos. |
| **`PagamentoCommand`** (record) | ✅ Feito | Mensagem imutável para fila RabbitMQ. |
| **Interfaces de repositório** | ✅ Feito | `ITicketRepository`, `IEventRepository`. |

### Infraestrutura (`TicketMaster.Infrastructure`)

| Componente | Status | Observações |
|---|---|---|
| **`AppDbContext`** | ✅ Feito | Identity + entidades de domínio. Concorrência via `IsConcurrencyToken()` na coluna `Versao`. |
| **`TicketRepository`** | ✅ Feito | CRUD + `ObterReservasVencidasAsync`. |
| **`EventRepository`** | ✅ Feito | Listagem ordenada por data. |
| **Room como JSON** | ✅ Feito | `OwnsOne(r → r.Layout).ToJson()` — schema flexível sem tabela separada. |

### Web / Apresentação (`TicketMaster.Web`)

| Componente | Status | Observações |
|---|---|---|
| **Autenticação (Identity)** | ✅ Completo | Login, registro, gerenciamento de conta. Redirect de usuário logado fora do login. |
| **Home — Lista de eventos** | ✅ Feito | Cards responsivos com data e link para mapa. |
| **Ticket — Mapa de assentos** | ✅ Completo | Grid CSS gerado dinamicamente. Legendas, cores por status, modais de confirmação. |
| **Checkout** | ✅ Feito | Página com timer de 15 min, botão de pagamento, link de volta. |
| **SignalR (`TicketHub`)** | ✅ Feito | Atualização em tempo real por grupo de evento. |
| **`TicketReaperWorker`** | ✅ Feito | Background service que libera reservas expiradas a cada 1 minuto + notifica clientes. |
| **`PagamentoCommandConsumer`** | ✅ Feito | Consumer MassTransit que processa pagamento de forma assíncrona. |
| **Tema claro/escuro** | ✅ Feito | CSS custom properties, toggle salvo no `localStorage`, sem flash ao carregar. |
| **CSS refinado (~900 linhas)** | ✅ Feito | Design system coeso com `DM Sans` + `DM Serif Display`, sombras, transições. |

### Testes (`TicketMaster.Domain.Tests`)

| Componente | Status | Observações |
|---|---|---|
| **`TicketTests`** | ✅ Completo | Estado inicial, reserva, pagamento, expiração, cancelamento, concorrência de versão. |
| **`TicketServiceTests`** | ✅ Completo | Sucesso, assento inexistente, concorrência, expiração, listagem. Todos com Moq. |

### Infraestrutura Geral

| Componente | Status | Observações |
|---|---|---|
| **Docker Compose** | ✅ Feito | SQL Server 2022 + RabbitMQ 3-management. Volumes nomeados. |
| **OpenTelemetry** | ✅ Feito | Tracing com console exporter — rastreamento de requisições. |
| **Serilog** | ✅ Feito | Log estruturado no console. |
| **Solution (.slnx)** | ✅ Feito | Novíssimo formato `.slnx` (Visual Studio 2022). |

---

## 🔧 O Que Podemos Evoluir (Priorizado)

### 🟢 Fácil / Rápido — Ganho Imediato

| # | Mudança | Onde | Motivo |
|---|---------|------|--------|
| 1 | **Remover imagens placeholder inexistentes** | Views (`Home/Index`, `Ticket/Index`, `Ticket/Checkout`) | Quebram visualmente — placeholder não carrega. Substituir por `<div class="placeholder-img">` com background gradiente ou CSS. |
| 2 | **Adicionar CancellationToken nos serviços** | `TicketService`, `EventService`, repositories | Boa prática .NET — evita threads órfãs em shutdown. |
| 3 | **Validar entradas no domínio** | `Event` (title vazio, data passada), `Ticket` (assentoCodigo vazio) | Protege integridade desde a camada mais interna. |
| 4 | **Mover seed data para classe própria** | `Program.cs` → `Data/DataSeeder.cs` | `Program.cs` já está com 120+ linhas — seed polui a configuração. |
| 5 | **Esconder senha do SQL Server** | `appsettings.json`, `docker-compose.yml` | Segurança: usar User Secrets em dev, variáveis de ambiente em prod. |

### 🟡 Médio — Melhoria Significativa

| # | Mudança | Onde | Motivo |
|---|---------|------|--------|
| 6 | **Adicionar eventos de domínio + MediatR** | `TicketMaster.Application` | Desacoplar efeitos colaterais (notificação SignalR, logging) do fluxo principal. Quando um ticket é vendido, quem notifica não deveria ser o controller — o domínio levanta `TicketVendidoEvent` e handlers escutam. |
| 7 | **CQRS com MediatR nos controllers** | `TicketController` | Controller gigante (4 actions POST + 3 GET). Commands/Queries separam responsabilidade e permitem validação via pipeline do MediatR. |
| 8 | **FluentValidation** | `TicketMaster.Application` | Validar `ReservarAssentoCommand`, `PagarCommand` etc. antes de chegar no serviço. |
| 9 | **Testes de integração com EF Core + SQL Server** | Projeto novo `TicketMaster.IntegrationTests` | Testar repositórios de verdade — o mock não pega problemas de tracking, concorrência real, JSON serialization. |
| 10 | **Painel administrativo para salas** | CRUD de `Room` no MVC | Hoje salas só são criadas via seed. Um admin precisa gerenciar layout, assentos, tipos (VIP, Cadeirante). |
| 11 | **Enriquecer `Event` com comportamento de domínio** | `Event.cs` | Evento publicado muda de rascunho → publicado → cancelado. Adicionar status, método `Publicar()`, `Cancelar()`. |
| 12 | **Notificar clientes por grupo (não `All`)** | `TicketReaperWorker` | Hoje `_hubContext.Clients.All` — envia para TODOS os usuários. Deveria enviar apenas para o grupo do evento. |

### 🔴 Complexo — Requer Arquitetura / Ferramentas Externas

| # | Mudança | Onde | Motivo |
|---|---------|------|--------|
| 13 | **Gateway de pagamento real** | `TicketMaster.Web` + novo serviço | Hoje o "pagamento" só muda o status no banco. Integrar Stripe, Mercado Pago ou similares. |
| 14 | **EF Core Migrations** | `TicketMaster.Infrastructure` | `EnsureCreatedAsync` não permite versionamento de schema. Em múltiplos ambientes (dev/staging/prod) você precisa de migrations. |
| 15 | **Pipeline CI/CD (GitHub Actions)** | `.github/workflows/` | Build + testes + análise de qualidade a cada push. Pasta de workflows existe mas está vazia. |
| 16 | **Telemetria real (OpenTelemetry exporter)** | `Program.cs` | Console exporter é só para debug. Enviar tracing para Jaeger, Grafana Tempo, ou Application Insights. |
| 17 | **Testes de controller / SignalR / MassTransit** | `TicketMaster.Web.Tests` | Testar o consumer real com `TestBus` do MassTransit, o hub com `HubConnection` em memória, os controllers com `WebApplicationFactory`. |
| 18 | **Melhorar o `TicketRepository.AtualizarAsync`** | `TicketRepository.cs` | Hoje só chama `SaveChangesAsync` sem Update explícito — funciona pelo tracking, mas é frágil. `_context.Tickets.Update(ticket)` antes de salvar seria mais explícito e permitiria desanexado. |
| 19 | **Logs centralizados (ELK / Seq)** | Infra + `Program.cs` | Serilog só escreve no console. Um destino estruturado (Seq, Elasticsearch) permite buscar logs por evento, usuário, assento. |

---

## 🗺️ Roadmap Sugerido

```
Sprint 1 — Higiene (itens 1–5)
  ├─ Remover imagens quebradas
  ├─ CancellationToken em toda a cadeia async
  ├─ Validação de domínio
  ├─ DataSeeder próprio
  └─ Esconder senha

Sprint 2 — Arquitetura (itens 6–8)
  ├─ MediatR + CQRS nos controllers
  ├─ Eventos de domínio
  └─ FluentValidation

Sprint 3 — Qualidade (itens 9, 17)
  ├─ Projeto de testes de integração
  └─ Testes de controller / hub / consumer

Sprint 4 — Administração (itens 10–11)
  ├─ CRUD de salas/layout
  └─ Comportamento de Event (publicação, cancelamento)

Sprint 5 — Produção (itens 13–16, 18–19)
  ├─ Gateway de pagamento
  ├─ EF Core Migrations
  ├─ CI/CD
  ├─ Telemetria real
  └─ Logs centralizados
```

---

## 📊 Métricas Atuais

| Camada | Projetos | Testes | Cobertura estimada |
|--------|----------|--------|--------------------|
| Domain | 1 | ~30 testes | ✅ 90%+ |
| Application | 1 | (via Domain.Tests) | 🔶 60% |
| Infrastructure | 1 | 0 | ❌ 0% |
| Web | 1 | 0 | ❌ 0% |
| **Total** | **5** | **~30 testes** | **~35%** |

---

*Documento gerado em {{REASONIX_CURRENT_DATE}} — análise baseada no código-fonte do repositório.*
