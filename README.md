# TicketMaster

Sistema de gerenciamento de tickets desenvolvido em .NET com arquitetura limpa.

## Tecnologias

- **Backend:** ASP.NET Core
- **Banco de Dados:** SQL Server
- **Mensageria:** RabbitMQ
- **Cache:** Redis

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

## Como executar

1. Clone o repositório:
   ```bash
   git clone https://github.com/Bruno-Milagres/TicketMaster.git
   cd TicketMaster
   ```

2. Suba os serviços de infraestrutura:
   ```bash
   cp .env.example .env
   docker-compose up -d
   ```

3. Execute a aplicação:
   ```bash
   dotnet run --project src/TicketMaster.Web
   ```

## Estrutura do Projeto

```
src/
  TicketMaster.Domain/          # Entidades e regras de negócio
  TicketMaster.Application/     # Casos de uso
  TicketMaster.Infrastructure/  # Repositórios e integrações
  TicketMaster.Web/             # API e interface web
test/
  TicketMaster.Domain.Tests/
  TicketMaster.IntegrationTests/
```

## Testes

```bash
dotnet test TicketMaster.slnx
```
