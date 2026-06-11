# Portfolio Club Assurance

Local portfolio artifact for the Assurance Horizon quote-builder flow. The current deployable setup runs a Vite React frontend, an ASP.NET Core backend, and a PostgreSQL database with the inferred quote schema.

## What Runs

- React frontend from `frontend`, served by Vite.
- ASP.NET Core backend from `backend`, run with `dotnet watch` for development.
- PostgreSQL 16 with the `quote` schema initialized from `scripts/create_quote_schema.sql`. The Compose database uses tmpfs storage, so it is reset when the Postgres container restarts.
- Adminer browser database client for demo access to PostgreSQL.

Default local URLs:

- React frontend: http://localhost:5173
- Backend API: http://localhost:5080
- PostgreSQL admin UI: http://localhost:8081
- PostgreSQL: `localhost:5432`

Default database settings:

| Setting | Value |
| --- | --- |
| Database | `portfolio_assurance` |
| User | `portfolio` |
| Password | `portfolio_dev_password` |
| Schema | `quote` |

Backend connection string inside Docker Compose:

```text
ConnectionStrings__QuoteDatabase=Host=postgres;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password
```

## Quick Start

From the repository root:

```bash
docker compose up -d
```

Open:

```text
http://localhost:5173
```

Check containers:

```bash
docker compose ps
```

Validate the backend:

```bash
curl http://localhost:5080/health
curl http://localhost:5080/api/database/health
```

Stop the stack:

```bash
docker compose down
```

Restart Postgres and recreate the demo database from the init SQL:

```bash
docker compose restart postgres
```

## macOS Setup And Validation

These steps assume you do not know what is already installed.

### 1. Check The Machine

Check macOS version:

```bash
sw_vers
```

Check chip type:

```bash
uname -m
```

Expected values:

- Apple Silicon: `arm64`
- Intel: `x86_64`

Docker Desktop and OrbStack both support either architecture.

### 2. Check Git

Validate:

```bash
git --version
```

If Git is missing, macOS may prompt you to install Xcode Command Line Tools. You can also install them directly:

```bash
xcode-select --install
```

Validate again:

```bash
git --version
```

### 3. Check Docker

Validate:

```bash
docker --version
docker compose version
docker info
```

If these commands work, Docker is installed and running.

If Docker is missing, install one of these:

- Docker Desktop for Mac: https://www.docker.com/products/docker-desktop/
- OrbStack: https://orbstack.dev/

After installation, start Docker Desktop or OrbStack, then validate again:

```bash
docker info
docker compose version
```

### 4. Check Required Ports

This project uses ports `5432`, `5080`, `5173`, and `8081`.

Check whether they are already in use:

```bash
lsof -nP -iTCP:5432 -sTCP:LISTEN
lsof -nP -iTCP:5080 -sTCP:LISTEN
lsof -nP -iTCP:5173 -sTCP:LISTEN
lsof -nP -iTCP:8081 -sTCP:LISTEN
```

No output means the port is free.

If a port is already used, copy `.env.example` to `.env` and change the port:

```bash
cp .env.example .env
```

Example alternatives:

```text
POSTGRES_PORT=55432
BACKEND_PORT=15080
FRONTEND_PORT=15173
ADMINER_PORT=18081
```

### 5. Start The Stack

```bash
docker compose up -d
```

Validate:

```bash
docker compose ps
```

Expected:

- `portfolio-assurance-postgres` is `healthy`
- `portfolio-assurance-adminer` is `Up`
- `portfolio-assurance-backend` is `Up`
- `portfolio-assurance-frontend` is `Up`

Validate the React frontend:

```bash
curl -I http://localhost:5173
```

Expected: `HTTP/1.1 200 OK`

Validate the backend API:

```bash
curl http://localhost:5080/health
curl http://localhost:5080/api/database/health
```

Expected: JSON with `"status":"ok"`. The database health endpoint should report the `quote` schema table count.

Validate PostgreSQL:

```bash
docker compose exec -T postgres psql -U portfolio -d portfolio_assurance -c "select table_schema, table_name from information_schema.tables where table_schema = 'quote' order by table_name;"
```

Expected: quote tables such as `quotes`, `quote_drivers`, `quote_vehicles`, and lookup tables.

## Windows 11 Setup And Validation

Use PowerShell for these commands unless noted otherwise.

### 1. Check Windows Version

Validate:

```powershell
winver
```

Windows 11 is recommended.

Check system info:

```powershell
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"System Type"
```

### 2. Check WSL 2

Docker Desktop on Windows uses WSL 2.

Validate:

```powershell
wsl --status
wsl --list --verbose
```

Expected:

- Default version should be `2`
- Any installed Linux distro should show version `2`

If WSL is missing or not configured:

```powershell
wsl --install
```

Restart Windows if prompted.

Set WSL 2 as default:

```powershell
wsl --set-default-version 2
```

Validate again:

```powershell
wsl --status
```

### 3. Check Git

Validate:

```powershell
git --version
```

If Git is missing, install Git for Windows:

```powershell
winget install --id Git.Git -e
```

Close and reopen PowerShell, then validate:

```powershell
git --version
```

### 4. Check Docker Desktop

Validate:

```powershell
docker --version
docker compose version
docker info
```

If Docker is missing:

```powershell
winget install --id Docker.DockerDesktop -e
```

Start Docker Desktop from the Start menu.

In Docker Desktop settings, validate:

- General: `Use the WSL 2 based engine` is enabled.
- Resources > WSL Integration: integration is enabled for your distro if you use WSL.

Validate again:

```powershell
docker info
docker compose version
```

### 5. Check Required Ports

This project uses ports `5432`, `5080`, `5173`, and `8081`.

Check whether they are already in use:

```powershell
netstat -ano | findstr ":5432"
netstat -ano | findstr ":5080"
netstat -ano | findstr ":5173"
netstat -ano | findstr ":8081"
```

No output means the port is free.

If a port is already used, copy `.env.example` to `.env` and change the port:

```powershell
Copy-Item .env.example .env
notepad .env
```

Example alternatives:

```text
POSTGRES_PORT=55432
BACKEND_PORT=15080
FRONTEND_PORT=15173
ADMINER_PORT=18081
```

### 6. Start The Stack

From the repository root:

```powershell
docker compose up -d
```

Validate:

```powershell
docker compose ps
```

Expected:

- `portfolio-assurance-postgres` is `healthy`
- `portfolio-assurance-adminer` is `Up`
- `portfolio-assurance-backend` is `Up`
- `portfolio-assurance-frontend` is `Up`

Validate the React frontend:

```powershell
curl.exe -I http://localhost:5173
```

Expected: `HTTP/1.1 200 OK`

Validate the backend API:

```powershell
curl.exe http://localhost:5080/health
curl.exe http://localhost:5080/api/database/health
```

Expected: JSON with `"status":"ok"`. The database health endpoint should report the `quote` schema table count.

## Backend Development Container

The backend service is designed for development:

- It uses `mcr.microsoft.com/dotnet/sdk:8.0`, not a runtime-only image.
- It bind-mounts `./backend` to `/app`.
- It runs `dotnet watch run`, so C# changes rebuild automatically.
- It stores NuGet packages in a named Docker volume, `dotnet-nuget-cache`.

Useful commands:

```bash
docker compose logs -f backend
docker compose exec backend dotnet --info
docker compose exec backend dotnet restore
docker compose exec backend dotnet build
```

The Compose connection string points the backend at the PostgreSQL service by Docker service name:

```text
Host=postgres;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password
```

In Docker Compose this is provided to ASP.NET Core with the environment variable:

```text
ConnectionStrings__QuoteDatabase=Host=postgres;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password
```

From the host machine, use:

```text
Host=localhost;Port=5432;Database=portfolio_assurance;Username=portfolio;Password=portfolio_dev_password
```

Validate PostgreSQL:

```powershell
docker compose exec -T postgres psql -U portfolio -d portfolio_assurance -c "select table_schema, table_name from information_schema.tables where table_schema = 'quote' order by table_name;"
```

Expected: quote tables such as `quotes`, `quote_drivers`, `quote_vehicles`, and lookup tables.

## Backend Tests

The backend test project contains unit tests for vehicle request validation and integration tests for the HTTP API with a disposable PostgreSQL database created by Testcontainers.

Docker must be running before the integration tests start.

Run from the repository root:

```bash
dotnet test backend.Tests/PortfolioClubAssurance.Api.Tests.csproj
```

The integration tests:

- Start `postgres:16-alpine` through Testcontainers.
- Execute `scripts/create_quote_schema.sql`.
- Host the ASP.NET Core API with `WebApplicationFactory`.
- Validate lookup endpoints, quote creation, vehicle save/list/read/delete, and validation errors.

## Browser Database Admin

Open Adminer:

```text
http://localhost:8081
```

Use this connection profile:

| Field | Value |
| --- | --- |
| System | `PostgreSQL` |
| Server | `postgres` |
| Username | `portfolio` |
| Password | `portfolio_dev_password` |
| Database | `portfolio_assurance` |

Adminer runs inside Docker Compose, so the database server is the Compose service name `postgres`, not `localhost`.

If you changed `ADMINER_PORT` in `.env`, use that host port instead of `8081`.

## Testcontainers Notes

For integration tests, use the same database image and schema script as Compose:

- Image: `postgres:16-alpine`
- Database: `portfolio_assurance`
- User: `portfolio`
- Password: `portfolio_dev_password`
- Init script: `scripts/create_quote_schema.sql`

The important rule is that integration tests should create their own PostgreSQL container instead of depending on the Compose database. This keeps tests isolated and repeatable on developer machines and CI.

In .NET, the Testcontainers setup should:

1. Start a PostgreSQL container using `postgres:16-alpine`.
2. Copy or execute `scripts/create_quote_schema.sql` during test initialization.
3. Build the application connection string from the container host and mapped port.
4. Dispose the container after the test run.

## Troubleshooting

### Docker Is Not Running

Symptom:

```text
Cannot connect to the Docker daemon
```

Fix:

- Start Docker Desktop or OrbStack.
- Run `docker info` again.

### Port Is Already Allocated

Symptom:

```text
Bind for 0.0.0.0:5432 failed: port is already allocated
```

Fix:

1. Copy `.env.example` to `.env`.
2. Change `POSTGRES_PORT`, `BACKEND_PORT`, `FRONTEND_PORT`, or `ADMINER_PORT`.
3. Run `docker compose up -d` again.

### Database Schema Did Not Change After Editing SQL

The Compose PostgreSQL service stores its data in tmpfs for demo use. Restart Postgres to recreate the database and rerun the init SQL:

```bash
docker compose restart postgres
```

### Check Logs

```bash
docker compose logs postgres
docker compose logs backend
docker compose logs frontend
```

### Full Cleanup

This removes containers and local development cache volumes:

```bash
docker compose down -v
```
