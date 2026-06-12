# Installation des dépendances sur Windows 11. 

Ce guide explique comment installer et valider les dépendances nécessaires pour exécuter le projet localement sur Windows 11.

Utilisez PowerShell pour les commandes, sauf indication contraire.

## 1. Vérifier la version de Windows

Validez la version de Windows :

```powershell
winver
```

Windows 11 est recommandé.

Vérifiez les informations système :

```powershell
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"System Type"
```

## 2. Installer ou vérifier WSL 2

Docker Desktop sur Windows utilise WSL 2.

Validez l'état de WSL :

```powershell
wsl --status
wsl --list --verbose
```

Résultat attendu :

- la version par défaut doit être `2`
- toute distribution Linux installée doit indiquer la version `2`

Si WSL est absent ou non configuré :

```powershell
wsl --install
```

Redémarrez Windows si l'installation le demande.

Définissez WSL 2 comme version par défaut :

```powershell
wsl --set-default-version 2
```

Validez de nouveau :

```powershell
wsl --status
```

## 3. Installer ou vérifier Git

Validez que Git est installé :

```powershell
git --version
```

Si Git est absent, installez Git pour Windows :

```powershell
winget install --id Git.Git -e
```

Fermez et rouvrez PowerShell, puis validez :

```powershell
git --version
```

## 4. Installer ou vérifier Docker Desktop

Validez que Docker et Docker Compose sont installés et démarrés :

```powershell
docker --version
docker compose version
docker info
```

Si Docker est absent :

```powershell
winget install --id Docker.DockerDesktop -e
```

Démarrez Docker Desktop depuis le menu Démarrer.

Dans les paramètres de Docker Desktop, validez les options suivantes :

- General : `Use the WSL 2 based engine` est activé
- Resources > WSL Integration : l'intégration est activée pour votre distribution si vous utilisez WSL

Validez de nouveau :

```powershell
docker info
docker compose version
```

## 5. Vérifier les ports requis

Le projet utilise les ports `5432`, `5080`, `5173` et `8081`.

Vérifiez s'ils sont déjà utilisés :

```powershell
netstat -ano | findstr ":5432"
netstat -ano | findstr ":5080"
netstat -ano | findstr ":5173"
netstat -ano | findstr ":8081"
```

Aucune sortie signifie que le port est libre.

Si un port est déjà utilisé, copiez `.env.example` vers `.env` et modifiez le port concerné :

```powershell
Copy-Item .env.example .env
notepad .env
```

Exemples de valeurs alternatives :

```text
POSTGRES_PORT=55432
BACKEND_PORT=15080
FRONTEND_PORT=15173
ADMINER_PORT=18081
```

## 6. Valider l'environnement avec Docker Compose

## Important vous devez obtenir un utilisateur Docker pour être en mesure de lancer Docker Desktop. Si cette application n'est pas lancer le serveur Docker ne fonctionne pas. Aussi la virtualisation doit être disponible dans le Bios de votre poste Windows.

Depuis la racine du dépôt, démarrez les services :

```powershell
docker compose up -d
```

Validez l'état des conteneurs :

```powershell
docker compose ps
```

Résultat attendu :

- `portfolio-assurance-postgres` est `healthy`
- `portfolio-assurance-adminer` est `Up`
- `portfolio-assurance-backend` est `Up`
- `portfolio-assurance-frontend` est `Up`

## Par contre super important , il est possible que les port soit bloquer par le Firewall de windows

Validez le frontend React :

```powershell
curl.exe -I http://localhost:5173
```

Résultat attendu : `HTTP/1.1 200 OK`

Validez l'API backend :

```powershell
curl.exe http://localhost:5080/health
curl.exe http://localhost:5080/api/database/health
```

Résultat attendu : une réponse JSON avec `"status":"ok"`. Le point d'accès de santé de la base de données doit aussi indiquer le nombre de tables du schéma `quote`.

Validez PostgreSQL :

```powershell
docker compose exec -T postgres psql -U portfolio -d portfolio_assurance -c "select table_schema, table_name from information_schema.tables where table_schema = 'quote' order by table_name;"
```

Résultat attendu : des tables du schéma `quote`, par exemple `quotes`, `quote_drivers`, `quote_vehicles` et les tables de référence.

## 7. Arrêter les services

Pour arrêter les conteneurs :

```powershell
docker compose down
```

