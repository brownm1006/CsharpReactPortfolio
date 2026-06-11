# PortfolioClubAssurance.Api

Ce dossier contient le backend C# du projet Portfolio Club Assurance. Il s'agit d'une API ASP.NET Core en .NET 8 qui expose les services nécessaires au parcours de soumission d'une soumission d'assurance automobile.

L'API est conçue pour fonctionner avec le frontend React situé dans `../frontend` et une base PostgreSQL initialisée avec le schéma `quote`.

## Rôle du projet

Le backend gère principalement :

- la création d'une soumission automobile;
- la consultation et l'enregistrement des véhicules associés à une soumission;
- les listes de référence utilisées par le formulaire, comme les fabricants, modèles, mois, conditions d'achat et réponses oui/non/inconnu;
- les endpoints de santé applicative et de santé de la base de données.

Le projet cible un périmètre volontairement clair : le produit supporté est actuellement `Auto`.

## Technologies

- .NET 8
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL avec le provider `Npgsql.EntityFrameworkCore.PostgreSQL`
- xUnit pour les tests
- Testcontainers pour les tests d'intégration PostgreSQL

## Structure du dossier

```text
backend/
├── PortfolioClubAssurance.Api.csproj
├── appsettings.Development.json
├── Dockerfile
└── src/
    ├── Controllers/      Endpoints HTTP et mapping des résultats
    ├── Data/             DbContext Entity Framework
    ├── Dtos/             Contrats de requêtes et réponses JSON
    ├── Entities/         Entités persistées dans PostgreSQL
    ├── Options/          Configuration typée
    ├── Repositories/     Accès aux données
    ├── Services/         Logique applicative
    └── Validation/       Validation des requêtes entrantes
```

Les tests ne sont pas dans ce dossier. Ils sont dans :

```text
../backend.Tests/
```

## Choix de conception, en lien avec Java Spring Boot

Le projet reprend des concepts très proches d'une application Java Spring Boot classique, tout en utilisant les idiomes de .NET.

| Concept Spring Boot | Équivalent dans ce projet C# |
| --- | --- |
| `@RestController` | Méthodes d'extension dans `src/Controllers/*Controller.cs` qui déclarent les routes Minimal API |
| `@Service` | Classes de service comme `QuoteService` |
| `@Repository` | Interface `IQuoteRepository` et implémentation `EfQuoteRepository` |
| Spring DI container | Conteneur d'injection de dépendances ASP.NET Core configuré dans `Program.cs` |
| Spring Data JPA / Hibernate | Entity Framework Core avec `QuoteDbContext` |
| `application.yml` / `application.properties` | `appsettings*.json` et variables d'environnement |
| DTOs Java records/classes | Records et classes dans `src/Dtos` |
| Bean validation | Validateurs explicites dans `src/Validation` |

Le choix principal est donc une architecture en couches :

- **Controller / endpoint** : reçoit la requête HTTP et retourne une réponse HTTP.
- **Service** : applique les règles métier, orchestre la validation et l'accès aux données.
- **Repository** : encapsule les requêtes PostgreSQL via Entity Framework Core.
- **DTO** : sépare les contrats HTTP des entités de base de données.
- **Entity** : représente les tables du schéma `quote`.

Cette organisation est familière pour une personne venant de Spring Boot : les responsabilités restent séparées, les dépendances sont injectées, et la couche de persistance reste isolée derrière une interface. La différence principale est le style ASP.NET Core Minimal API, plus léger que les contrôleurs annotés de Spring, mais avec le même objectif : garder les routes simples et déléguer la logique au service.

## Configuration

La connexion PostgreSQL est lue depuis la configuration sous :

```text
ConnectionStrings:QuoteDatabase
```

Avec Docker Compose, cette variable est fournie automatiquement par le fichier `../docker-compose.yml`.

En développement local hors Docker Compose, vous pouvez utiliser une variable d'environnement :

```bash
export ConnectionStrings__QuoteDatabase="Host=localhost;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password"
```

L'origine CORS autorisée par défaut en développement est :

```text
http://localhost:5173
```

## Lancer l'API

Depuis la racine du dépôt, le plus simple est de démarrer toute la stack :

```bash
docker compose up -d
```

L'API est alors disponible par défaut sur :

```text
http://localhost:5080
```

Pour lancer uniquement le projet .NET depuis ce dossier :

1. Depuis la racine du dépôt, démarrez seulement le conteneur PostgreSQL :

```bash
docker compose up -d postgres
```

2. Depuis le dossier `backend`, configurez la chaîne de connexion vers ce conteneur :

```bash
export ConnectionStrings__QuoteDatabase="Host=localhost;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password"
```

3. Lancez l'API :

```bash
dotnet run
```

`dotnet run` ne démarre pas PostgreSQL automatiquement. Il faut donc que le conteneur PostgreSQL du projet, ou une autre instance PostgreSQL compatible, soit déjà disponible.

## Endpoints principaux

Santé :

```text
GET /health
GET /api/database/health
```

Soumissions :

```text
POST /api/quotes
```

Véhicules d'une soumission :

```text
GET    /api/quotes/{quoteId}/vehicles
POST   /api/quotes/{quoteId}/vehicles
GET    /api/quotes/{quoteId}/vehicles/{vehicleId}
DELETE /api/quotes/{quoteId}/vehicles/{vehicleId}
```

Listes de référence :

```text
GET /api/quote/lookups/vehicle-description
GET /api/quote/lookups/vehicle-usage
GET /api/quote/lookups/yes-no-unknown
GET /api/quote/lookups/purchase-conditions
GET /api/quote/lookups/manufacturers
GET /api/quote/lookups/manufacturers/{manufacturerCode}/models
```

Plusieurs endpoints acceptent le paramètre `locale`. La valeur `en` retourne les libellés anglais lorsque disponibles; toute autre valeur utilise le français.

## Tests

Les tests sont dans un projet séparé :

```text
../backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

Pour les exécuter depuis ce dossier `backend` :

```bash
dotnet test ../backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

Ou depuis la racine du dépôt :

```bash
dotnet test backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

Les tests couvrent notamment :

- la validation des requêtes véhicule;
- la logique du service de soumission;
- des tests d'intégration HTTP avec une vraie base PostgreSQL lancée par Testcontainers.

Pour les tests d'intégration, Docker doit être installé et démarré, car Testcontainers lance un conteneur `postgres:16-alpine`.

Il n'est pas nécessaire de démarrer le conteneur PostgreSQL du projet avant `dotnet test`. Les tests créent leur propre conteneur PostgreSQL temporaire avec Testcontainers.

## Commandes utiles

Restaurer les dépendances :

```bash
dotnet restore
```

Compiler le projet :

```bash
dotnet build
```

Lancer les tests :

```bash
dotnet test ../backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

Vérifier rapidement l'API après démarrage :

```bash
curl http://localhost:5080/health
curl http://localhost:5080/api/database/health
```
