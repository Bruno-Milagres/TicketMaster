# TicketMaster

Sistema web de gerenciamento de eventos e venda de ingressos desenvolvido em .NET 8 com Clean Architecture, CQRS e atualizações em tempo real.

---

## Visão Geral

O TicketMaster é uma plataforma completa para criação, publicação e gerenciamento de eventos com venda de ingressos. O sistema permite que administradores criem salas e eventos, publiquem ingressos por setor e que usuários reservem e paguem seus assentos com controle de disponibilidade em tempo real.

---

## Arquitetura

O projeto segue os princípios de **Clean Architecture**, dividido em quatro camadas com dependências sempre apontando para o núcleo de domínio:

```
src/
├── TicketMaster.Domain/          # Entidades, regras de negócio, enums e eventos de domínio
├── TicketMaster.Application/     # Casos de uso (CQRS), interfaces e validações
├── TicketMaster.Infrastructure/  # Persistência, cache, mensageria e serviços externos
└── TicketMaster.Web/             # API REST, interface MVC, SignalR Hub e workers

test/
├── TicketMaster.Domain.Tests/
└── TicketMaster.IntegrationTests/
```

### Camada de Domínio (`TicketMaster.Domain`)

Contém as entidades ricas com regras de negócio encapsuladas, sem dependências externas:

| Entidade | Descrição |
|---|---|
| `Event` | Evento com ciclo de vida **Rascunho → Publicado → Cancelado** |
| `Ticket` | Ingresso com estados **Disponível → Reservado → Vendido**, expiração automática de 15 minutos |
| `Room` | Sala/venue onde o evento ocorre, com mapa SVG de assentos |
| `CamaroteGroup` | Agrupamento de assentos VIP (camarote) |
| `TipoIngresso` | Tipo de ingresso com precificação por setor |
| `Pedido` / `ItemPedido` | Pedido de compra com itens e totais |
| `PrecoHistorico` | Histórico de variações de preço por tipo de ingresso |
| `RefreshToken` | Token de renovação de sessão JWT |

### Camada de Aplicação (`TicketMaster.Application`)

Implementa o padrão **CQRS com MediatR**:

**Commands:**
- `ReservarAssento` — reserva um assento para o usuário autenticado
- `CancelarReserva` — cancela a reserva do usuário
- `ConfirmarPagamento` — confirma o pagamento e marca o ingresso como vendido
- `ExpirarReservasVencidas` — libera ingressos com reservas fora do prazo
- `CriarSala` / `AtualizarSala` / `ExcluirSala` — gerenciamento de salas

**Queries:**
- `ListarEventosAtivos` — lista todos os eventos publicados
- `ListarSalas` — lista as salas cadastradas
- `ObterSalaPorId` — detalhes de uma sala
- `ObterIngressosPorEvento` — ingressos e disponibilidade de um evento

**Pipeline Behaviors:**
- `ValidationBehavior` — executa validações FluentValidation automaticamente antes de cada command/query

### Camada de Infraestrutura (`TicketMaster.Infrastructure`)

- **`AppDbContext`** — contexto do Entity Framework Core com mapeamento das entidades
- **`DataSeeder`** — seed automático de usuário admin e dados de demonstração na inicialização
- **Repositories** — `TicketRepository`, `EventRepository`, `RoomRepository`
- **`CachedTicketRepository`** — decorator sobre `TicketRepository` que aplica cache distribuído Redis (Decorator Pattern)
- **`QuotaService`** — controle de cota de ingressos por usuário
- **`LogEmailService`** — implementação de e-mail via log (substituível por SMTP em produção)
- **Migrations** — migrations do EF Core para criação e evolução do schema SQL Server

### Camada Web (`TicketMaster.Web`)

- **Controllers REST:** `AuthController`, `CheckoutController`, `SeatsController`, `TicketController`, `HomeController`
- **`TicketHub`** (SignalR) — Hub para notificações em tempo real de disponibilidade de assentos; clientes entram no grupo do evento via `EntrarNaSalaDoEvento`
- **`TicketReaperWorker`** — `BackgroundService` que executa a cada 1 minuto para liberar reservas vencidas e notificar clientes conectados via SignalR
- **`PagamentoCommandConsumer`** — consumer MassTransit que processa mensagens de pagamento da fila RabbitMQ
- **`EventHandlers`** — handlers de eventos de domínio (ex.: `AssentoLiberadoEventHandler`)
- **Areas** — área de administração com gerenciamento de salas e eventos
- **Views** — interface Razor com mapa interativo de assentos (SVG)

---

## Tecnologias

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 8 |
| Framework Web | ASP.NET Core MVC + Razor Pages |
| ORM | Entity Framework Core 8 |
| Banco de Dados | SQL Server 2022 |
| Cache Distribuído | Redis 7 (StackExchange.Redis) |
| Mensageria | RabbitMQ 3 + MassTransit |
| Tempo Real | ASP.NET Core SignalR |
| CQRS / Mediator | MediatR |
| Validação | FluentValidation |
| Autenticação | ASP.NET Core Identity + JWT Bearer |
| Logging | Serilog |
| Observabilidade | OpenTelemetry (tracing) |
| Documentação API | Swagger / OpenAPI |
| QR Code | Geração de QR Code por ingresso |
| Compressão | Brotli + Gzip (Response Compression) |
| Containerização | Docker + Docker Compose |

---

## Funcionalidades

- **Gestão de eventos** com ciclo de vida completo (Rascunho → Publicado → Cancelado)
- **Mapa interativo de assentos** em SVG com seleção visual por setor e suporte a camarotes
- **Reserva com expiração automática** — ingressos reservados expiram em 15 minutos se não confirmados
- **Liberação automática em background** via `TicketReaperWorker` com notificação em tempo real
- **Disponibilidade em tempo real** via SignalR — todos os usuários conectados veem assentos ocupados/liberados instantaneamente
- **Fluxo de checkout** com confirmação de pagamento via mensageria assíncrona (RabbitMQ)
- **Cache inteligente** de ingressos com Redis (Decorator sobre o repositório)
- **QR Code** gerado por ingresso para validação na entrada do evento
- **Controle de acesso por roles** (Admin / Usuário) via ASP.NET Core Identity
- **Área administrativa** para gerenciamento de salas e eventos
- **Seed automático** de dados de demonstração e usuário admin na primeira execução
- **Histórico de preços** por tipo de ingresso

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

---

## Testes

```bash
dotnet test TicketMaster.slnx -v minimal
```
