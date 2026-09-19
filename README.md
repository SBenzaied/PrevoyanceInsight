# PrévoyanceInsight

Démonstrateur technique préparé pour l'entretien "Développeur Full Stack .NET"
(CPEG / Gyffted). Illustre une réponse architecturale aux deux objectifs de l'offre :
une BI pour l'actuariat, et des outils d'IA pilotables par prompting pour produire des
rapports, comparer des plans de prévoyance et détecter des anomalies de masse.

➡️ **Commencez par [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)** pour le contexte et
les diagrammes, puis [`docs/TALKING_POINTS.md`](docs/TALKING_POINTS.md) pour la
préparation de l'entretien.

## Stack

C# / .NET 8 · CQRS (MediatR) · ASP.NET Core · **Serveur MCP** · Blazor · PostgreSQL ·
RavenDB · RabbitMQ (MassTransit) · Docker · NUnit / Moq (TDD)

## Structure

```
src/
  PrevoyanceInsight.Domain          entités métier, aucune dépendance externe
  PrevoyanceInsight.Application     CQRS : commands, queries, ports (interfaces)
  PrevoyanceInsight.Infrastructure  PostgreSQL (EF Core), RavenDB, RabbitMQ
  PrevoyanceInsight.Api             API REST — expose l'Application layer en HTTP
  PrevoyanceInsight.McpServer       serveur MCP — expose la même Application layer
                                    comme outils d'agent IA
  PrevoyanceInsight.Blazor          dashboard de comparaison de plans
tests/
  PrevoyanceInsight.Tests           NUnit + Moq, exemples TDD
docs/
  ARCHITECTURE.md                  choix techniques et diagrammes
  TALKING_POINTS.md                fiche de préparation entretien
```

## Lancer l'infrastructure

```bash
docker compose up -d postgres ravendb rabbitmq
dotnet ef database update --project src/PrevoyanceInsight.Infrastructure --startup-project src/PrevoyanceInsight.Api
dotnet run --project src/PrevoyanceInsight.Api
dotnet run --project src/PrevoyanceInsight.McpServer
```

## Tests

```bash
dotnet test tests/PrevoyanceInsight.Tests
```

## État du projet

Ce dépôt a été rédigé comme scaffold structurant : l'environnement de rédaction n'avait
pas d'accès réseau pour restaurer les paquets NuGet, donc le code n'a pas été compilé
ni exécuté ici. Avant la démo, prévoir un `dotnet restore` + `dotnet build` en local
pour corriger d'éventuels ajustements mineurs de versions de paquets.
