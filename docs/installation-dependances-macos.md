# Installation des dépendances sur macOS. 
## Non testé car mon Laptop Mac a déjà un environnement Docker fonctionnel

Ce guide explique comment installer et valider les dépendances nécessaires pour exécuter le projet localement sur macOS.

## 1. Vérifier la machine

Vérifiez la version de macOS :

```bash
sw_vers
```

Vérifiez le type de processeur :

```bash
uname -m
```

Valeurs attendues :

- Apple Silicon : `arm64`
- Intel : `x86_64`

Docker Desktop et OrbStack prennent en charge les deux architectures.

## 2. Vérifier Git

Validez que Git est installé :

```bash
git --version
```

Si Git est absent, macOS peut proposer l'installation des outils de ligne de commande Xcode. Vous pouvez aussi lancer l'installation directement :

```bash
xcode-select --install
```

Validez ensuite l'installation :

```bash
git --version
```

## 3. Installer ou vérifier Docker

Validez que Docker et Docker Compose sont installés et démarrés :

```bash
docker --version
docker compose version
docker info
```

Si ces commandes fonctionnent, Docker est installé et en cours d'exécution.

Si Docker est absent, installez l'une des options suivantes :

- Docker Desktop pour Mac : https://www.docker.com/products/docker-desktop/
- OrbStack : https://orbstack.dev/

Après l'installation, démarrez Docker Desktop ou OrbStack, puis validez de nouveau :

```bash
docker info
docker compose version
```

## 4. Vérifier les ports requis

Le projet utilise les ports `5432`, `5080`, `5173` et `8081`.

Vérifiez s'ils sont déjà utilisés :

```bash
lsof -nP -iTCP:5432 -sTCP:LISTEN
lsof -nP -iTCP:5080 -sTCP:LISTEN
lsof -nP -iTCP:5173 -sTCP:LISTEN
lsof -nP -iTCP:8081 -sTCP:LISTEN
```

Aucune sortie signifie que le port est libre.

Si un port est déjà utilisé, copiez `.env.example` vers `.env` et modifiez le port concerné :

```bash
cp .env.example .env
```

Exemples de valeurs alternatives :

```text
POSTGRES_PORT=55432
BACKEND_PORT=15080
FRONTEND_PORT=15173
ADMINER_PORT=18081
```

## 5. Valider l'environnement avec Docker Compose

Depuis la racine du dépôt, démarrez les services :

```bash
docker compose up -d
```

Validez l'état des conteneurs :

```bash
docker compose ps
```

Résultat attendu :

- `portfolio-assurance-postgres` est `healthy`
- `portfolio-assurance-adminer` est `Up`
- `portfolio-assurance-backend` est `Up`
- `portfolio-assurance-frontend` est `Up`

Validez le frontend React :

```bash
curl -I http://localhost:5173
```

Résultat attendu : `HTTP/1.1 200 OK`

Validez l'API backend :

```bash
curl http://localhost:5080/health
curl http://localhost:5080/api/database/health
```

Résultat attendu : une réponse JSON avec `"status":"ok"`. Le point d'accès de santé de la base de données doit aussi indiquer le nombre de tables du schéma `quote`.

Validez PostgreSQL :

```bash
docker compose exec -T postgres psql -U portfolio -d portfolio_assurance -c "select table_schema, table_name from information_schema.tables where table_schema = 'quote' order by table_name;"
```

Résultat attendu : des tables du schéma `quote`, par exemple `quotes`, `quote_drivers`, `quote_vehicles` et les tables de référence.

## 6. Arrêter les services

Pour arrêter les conteneurs :

```bash
docker compose down
```

