# ES.ConexaoSolidaria.Campanhas

Microsserviço de **Campanhas e Doações** da plataforma **Conexão Solidária**, desenvolvido para a ONG Esperança Solidária como parte do Hackathon POSTECH/FIAP.

Responsável por:
- Criação, alteração, conclusão e cancelamento de campanhas de arrecadação;
- Painel de Transparência público, com listagem das campanhas ativas;
- Busca avançada de campanhas via **Elasticsearch** (com tolerância a erros de digitação);
- Recebimento de intenções de doação e publicação do evento `DonationCreatedEvent` para processamento assíncrono pelo Worker;
- Consulta de doações por campanha, por usuário ou do próprio doador logado.

---

## Sumário
- [Arquitetura](#arquitetura)
- [Stack Tecnológica](#stack-tecnológica)
- [Perfis e Regras de Acesso](#perfis-e-regras-de-acesso)
- [Endpoints](#endpoints)
- [Como Rodar Localmente](#como-rodar-localmente)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Eventos de Domínio](#eventos-de-dominio)
- [Observabilidade](#observabilidade)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Github Actions](#github-actions)

---

## Arquitetura

Este serviço segue **Clean Architecture** dividido em quatro projetos (`Api`, `Application`, `Domain`, `Infrastructure`) e implementa CQRS via handlers de Use Case (`IUseCaseHandler<TCommand, TResult>`).

Ao registrar uma doação, a API **não** atualiza o valor arrecadado da campanha diretamente: ela publica um `DonationCreatedEvent` em um broker de mensageria. Um serviço **Worker** separado consome esse evento, processa a doação e atualiza o valor arrecadado da campanha de forma assíncrona e desacoplada.

> O diagrama completo da arquitetura da plataforma (todos os microsserviços, bancos de dados, broker e observabilidade) está no repositório de infraestrutura — [https://github.com/gmerendi/ES.ConexaoSolidaria.Infra]

## Stack Tecnológica

- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core** + **PostgreSQL** (dados relacionais de campanhas e doações)
- **Elasticsearch** (busca avançada de campanhas)
- **DynamoDB Local** (log de auditoria)
- **Redis** (cache de sessão, blacklist de tokens JWT e cache de campanhas)
- **RabbitMQ** (via **MassTransit**) em ambiente local, com fallback para **Amazon SQS** em ambiente `LAB`
- **JWT Bearer Authentication**
- **Prometheus** (métricas expostas em `/metrics`)
- **Docker** / **Docker Compose**
- **xUnit** (testes)

## Perfis e Regras de Acesso

| Role | Descrição |
|---|---|
| `GESTOR_ONG` | Cria, altera, cancela e conclui campanhas; consulta doações por campanha/usuário |
| `DOADOR` | Consulta campanhas, realiza doações e vê suas próprias doações |

- Campanhas não podem ser criadas com data de término no passado, e a meta financeira deve ser maior que zero.
- Uma campanha só pode ser editada enquanto estiver com status **ATIVA**.
- O Painel de Transparência (`GET /campanhas/todas`) é **público** e retorna apenas campanhas com status **ATIVA**.
- Doações não podem ser feitas para campanhas encerradas (**CONCLUIDA**) ou **CANCELADA**s.

## Endpoints

### Campanhas — `api/v1/campanhas`
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/` | `GESTOR_ONG` | Cria uma nova campanha |
| `GET` | `/` | `GESTOR_ONG`, `DOADOR` | Consulta uma campanha por Guid (qualquer status) |
| `GET` | `/todas` | Público | Painel de Transparência — lista campanhas **ATIVA**s (paginado) |
| `PUT` | `/` | `GESTOR_ONG` | Altera uma campanha ativa |
| `PUT` | `/cancel` | `GESTOR_ONG` | Cancela uma campanha ativa |
| `PUT` | `/concluir` | `GESTOR_ONG` | Conclui uma campanha ativa |
| `GET` | `/busca` | `GESTOR_ONG`, `DOADOR` | Busca avançada (Elasticsearch) por título, descrição, status e datas |

### Doações — `api/v1/doacoes`
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/` | `GESTOR_ONG`, `DOADOR` | Cria uma intenção de doação (processada de forma assíncrona pelo Worker) |
| `GET` | `/campanha` | `GESTOR_ONG` | Lista doações de uma campanha específica |
| `GET` | `/usuario` | `GESTOR_ONG` | Lista doações de um usuário específico |
| `GET` | `/self` | `GESTOR_ONG`, `DOADOR` | Lista as próprias doações do usuário logado |

### Observabilidade
| Rota | Descrição |
|---|---|
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |
| `/metrics` | Métricas no formato Prometheus |

A documentação interativa (Swagger) fica disponível em `/swagger` quando a API está rodando.

## Como Rodar Localmente

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop 4.79.0

### Subindo tudo com Docker Compose (recomendado)

O `docker-compose.yml` já sobe toda a infraestrutura necessária: PostgreSQL, Redis, DynamoDB Local, RabbitMQ, Elasticsearch e a própria API.

```bash
# 1. Clone o repositório
git clone https://github.com/gmerendi/ES.ConexaoSolidaria.Campanhas.git
cd ES.ConexaoSolidaria.Campanhas

# 2. Suba todos os serviços
docker compose up -d --build

# 3. Acompanhe os logs da API (opcional)
docker compose logs -f cs.campanhas.api
```

Serviços expostos:

| Serviço | URL/Porta |
|---|---|
| API de Campanhas | http://localhost:5002 (Swagger em `/swagger`) |
| PostgreSQL | localhost:5433 |
| Redis | localhost:6379 |
| DynamoDB Local | localhost:8000 |
| Elasticsearch | http://localhost:9200 |
| RabbitMQ (AMQP / Management UI) | http://localhost:15672 (`fiap` / `fiap123`) |

As migrations do banco são aplicadas automaticamente na subida da aplicação.

> **Atenção:** esta API depende de um usuário autenticado (token JWT) emitido pelo serviço `ES.ConexaoSolidaria.Usuarios` para os endpoints protegidos. Suba o serviço de Usuários e faça login antes de testar as rotas autenticadas.

## Variáveis de Ambiente

Todas já vêm configuradas no `docker-compose.yml` para o ambiente local. Principais chaves:

| Variável | Descrição |
|---|---|
| `ConnectionStrings__Database` | Conexão com o PostgreSQL |
| `ConnectionStrings__AuditLog` | Endpoint do DynamoDB (auditoria) |
| `ConnectionStrings__Redis` | Conexão com o Redis |
| `ConnectionStrings__ElasticSearch` | Endpoint do Elasticsearch |
| `RabbitMq__Host` / `RabbitMq__Username` / `RabbitMq__Password` | Conexão com o RabbitMQ |
| `Jwt__SecretKey` / `Jwt__Issuer` / `Jwt__Audience` / `Jwt__ExpirationHours` | Configuração do token JWT (deve ser a mesma chave usada pelo serviço de Usuários) |
| `AES__KEY` | Chave usada para criptografia de dados sensíveis |
| `Cache__CampanhaAtivaTTLSeconds` / `Cache__CampanhaNaoAtivaTTLSeconds` | TTL do cache de campanhas no Redis |
| `Application__Type` | `LOCAL` (usa RabbitMQ) ou `LAB` (usa SQS/AWS) |
| `Admin__Email` / `Admin__Password` | Credenciais do usuário administrador seed |

> **Atenção:** os valores no `docker-compose.yml` são apenas para desenvolvimento local. Nunca reutilize essas chaves em produção.

## Eventos de Dominio

- **DonationCreatedEvent** — publicado ao registrar uma intenção de doação; consumido pelo Worker, que processa e atualiza o valor arrecadado da campanha.

## Observabilidade

- A API expõe métricas em `/metrics` (Prometheus).
- Health checks disponíveis em `/health/ready` e `/health/live`.
- Este repositório não inclui a stack de Prometheus/Grafana — ela é provisionada centralmente no repositório de infraestrutura/observabilidade da plataforma.

## Testes

```bash
dotnet test
```

Os testes de unidade (xUnit) estão em `tests/Campanhas.Test` e cobrem as regras de domínio e casos de uso.

## Estrutura do Projeto

```
ES.ConexaoSolidaria.Campanhas/
├── src/
│   ├── Campanhas.Api/              # Controllers (Campanhas, Doações), Program.cs, Middlewares, Swagger, Health Checks
│   ├── Campanhas.Application/      # Use Cases (Commands/Queries/Handlers) por Feature
│   ├── Campanhas.Domain/           # Entidades (Campanha, Doacao), Value Objects, Eventos de Domínio, regras de negócio
│   └── Campanhas.Infrastructure/   # EF Core, Repositórios, Messaging, Cache, ElasticSearch, Auditoria, Métricas
├── tests/
│   └── Campanhas.Test/             # Testes de unidade (xUnit)
├── docker-compose.yml              # Orquestração local (API + infra)
└── ES.ConexaoSolidaria.Campanhas.slnx
```

Projeto desenvolvido para o Hackathon **POSTECH** — grupo 1.

## Github Actions

O repositório contém um pipeline GitHub Actions, acionado a cada push na branch principal. O pipeline compila o código (.NET build), executa os testes e gera a imagem Docker.
