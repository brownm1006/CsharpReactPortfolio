# Portfolio Club Assurance

Portfolio Club Assurance est une application de démonstration pour un parcours de soumission d'assurance automobile. Le projet regroupe un frontend React, une API backend C# et une base de données PostgreSQL initialisée avec un schéma de soumission.

## Vue d'ensemble

La stack locale démarre avec Docker Compose et contient :

- un frontend React basé sur Vite, situé dans `frontend`;
- un backend C# ASP.NET Core en .NET 8, situé dans `backend`;
- une base PostgreSQL 16 initialisée avec le schéma `quote`;
- Adminer pour consulter la base de données depuis un navigateur;
- un projet de tests backend séparé, situé dans `backend.Tests`.

URLs locales par défaut :

| Service | URL |
| --- | --- |
| Frontend React | http://localhost:5173 |
| Backend API | http://localhost:5080 |
| Adminer | http://localhost:8081 |
| PostgreSQL | `localhost:5432` |

## Important à vérifier si le Firewall ne bloque pas les ports sur votre poste. Où que les ports ne sont pas déjà attribués à un autre processus.

## Installation des dépendances

Les marches à suivre d'installation sont séparées par système d'exploitation :

- [Installation des dépendances sur macOS](docs/installation-dependances-macos.md)
- [Installation des dépendances sur Windows 11](docs/installation-dependances-windows11.md)

Ces guides couvrent notamment Git, Docker, Docker Compose, WSL 2 pour Windows 11, les ports requis et la validation de l'environnement local.

## Démarrage rapide (Vous devez créer un nouvel utilisateur  Docker Personal si vous n'en avez pas un)

Depuis la racine du dépôt :

```bash
docker compose up -d
```

Ouvrez ensuite le frontend :

```text
http://localhost:5173
```

Vérifiez l'état des conteneurs :

```bash
docker compose ps
```

Vérifiez rapidement le backend :

```bash
curl http://localhost:5080/health
curl http://localhost:5080/api/database/health
```

Pour arrêter la stack :

```bash
docker compose down
```

## Frontend React avec Vite

Le frontend se trouve dans `frontend`. Il utilise :

- React;
- Vite;
- TypeScript;
- React Router;
- Vitest et Testing Library pour les tests.

Le frontend implémente le parcours utilisateur de soumission automobile, avec des pages pour l'accueil, la description du véhicule, l'usage du véhicule, la liste des véhicules confirmés et la confirmation.

Commandes utiles depuis le dossier `frontend` :

```bash
npm install
npm run dev
npm test
npm run test:watch
npm run typecheck
npm run build
```

La commande pour lancer les tests frontend est :

```bash
cd frontend
npm test
```

## Backend C#

Le backend se trouve dans `backend`. Il s'agit d'une API ASP.NET Core en .NET 8 qui expose les services nécessaires au parcours de soumission.

Le backend gère notamment :

- la création d'une soumission automobile;
- l'ajout, la consultation et la suppression de véhicules associés à une soumission;
- les listes de référence utilisées par le formulaire;
- les endpoints de santé applicative et de santé de la base de données.

Technologies principales :

- C# et .NET 8;
- ASP.NET Core Minimal APIs;
- Entity Framework Core;
- PostgreSQL avec `Npgsql.EntityFrameworkCore.PostgreSQL`.

La stack Docker Compose lance le backend automatiquement. Pour consulter les logs :

```bash
docker compose logs -f backend
```

## Tests backend

Les tests backend se trouvent dans `backend.Tests`.

Ils couvrent notamment :

- la validation des requêtes véhicule;
- la logique du service de soumission;
- des tests d'intégration HTTP avec une base PostgreSQL temporaire lancée par Testcontainers.

Pour lancer les tests backend depuis la racine du dépôt :

```bash
dotnet test backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

Docker doit être installé et démarré pour les tests d'intégration, car Testcontainers démarre un conteneur PostgreSQL temporaire.

## Base de données

La base PostgreSQL est initialisée avec le script :

```text
scripts/create_quote_schema.sql
```

Paramètres locaux par défaut :

| Paramètre | Valeur |
| --- | --- |
| Système | `PostgreSQL` |
| Serveur | `postgres` |
| Utilisateur | `portfolio` |
| Mot de passe | `portfolio_dev_password` |
| Base de données | `portfolio_assurance` |

 

Adminer est disponible à l'adresse :

```text
http://localhost:8081

Lorsque connecté changer le Schema pour `quote`
```

Dans Adminer, utilisez le serveur `postgres`, car Adminer s'exécute dans Docker Compose.

## Documentation complémentaire

- [Documentation backend](backend/README.md)
- [Contrat API](ApiContract/README.md)
