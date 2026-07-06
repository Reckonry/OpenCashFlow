# Fase 0 - Ripristino Tecnico

## Prompt operativo

Agisci come senior engineer su questo repository:

`/Users/codewriter90x/Documents/GitHub/Reckonry ORG/OpenCashFlow`

Obiettivo: rendere OpenCashFlow riproducibile da clone pulito. Prima di qualsiasi refactor prodotto, la solution deve fare restore, build e test in modo affidabile, e Docker/CI devono puntare ai progetti reali.

## Contesto rilevante

- La cartella API reale e `src/OpenCashFlow.API`.
- La solution attuale punta a `src/OpenCashFlow.Api/OpenCashFlow.Api.csproj`, che non esiste.
- I test contengono riferimenti duplicati/non validi verso `OpenCashFlow.Api`.
- Docker Compose e Dockerfile usano path storici fuori da `src/`.
- Le GitHub Actions puntano anche a componenti non presenti come `OpenCashFlow.Landing`, `OpenCashFlow.Core`, `OpenCashFlow.Api`.

## Attivita richieste

1. Correggi `OpenCashFlow.sln` in modo che includa i progetti reali:
   - `src/OpenCashFlow.API/OpenCashFlow.API.csproj`
   - `src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj`
   - `src/OpenCashFlow.Admin/OpenCashFlow.Admin.csproj`
   - `src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj`
   - `tests/OpenCashFlow.Test/OpenCashFlow.Test.csproj`

2. Correggi `tests/OpenCashFlow.Test/OpenCashFlow.Test.csproj`:
   - rimuovi riferimenti duplicati;
   - rimuovi riferimenti a `src/OpenCashFlow.Api`;
   - mantieni solo `src/OpenCashFlow.API` e `src/OpenCashFlow.Shared`.

3. Correggi `docker-compose.yml` e `docker-compose.dev.yml`:
   - usa Dockerfile sotto `src/OpenCashFlow.API/Dockerfile` e `src/OpenCashFlow.WebApp/Dockerfile`;
   - elimina o commenta servizi inesistenti come `web` e `landing`;
   - usa nomi container coerenti, non legacy;
   - usa variabili locali e non domini `opencashflow.cloud`.

4. Correggi `src/OpenCashFlow.API/Dockerfile` e `src/OpenCashFlow.WebApp/Dockerfile`:
   - i path di `COPY` devono essere relativi al build context reale;
   - devono funzionare con `docker compose build`.

5. Correggi le workflow GitHub:
   - `ci-wip-to-master.yml` deve fare restore/build/test sulla solution corretta;
   - `ci-master-to-staging.yml` deve pubblicare solo componenti presenti;
   - `ci-staging-to-production.yml` deve scaricare solo artifact presenti;
   - rimuovi riferimenti a componenti inesistenti.

6. Aggiorna `README.md` con quickstart minimo:
   - requisiti;
   - `dotnet restore`;
   - `dotnet build`;
   - `docker compose up`;
   - URL locali previsti.

## Vincoli

- Non cambiare logica business in questa fase.
- Non rimuovere codice SaaS/Stripe qui: limitati a rendere il repo riproducibile.
- Non introdurre nuovi framework.
- Mantieni modifiche piccole e verificabili.

## Criteri di accettazione

- `dotnet sln OpenCashFlow.sln list` mostra solo progetti esistenti.
- `dotnet restore OpenCashFlow.sln` termina con successo.
- `dotnet build OpenCashFlow.sln --configuration Release --no-restore` termina con successo.
- `dotnet test OpenCashFlow.sln --configuration Release --no-build` viene eseguito; se alcuni test falliscono, documenta i fallimenti reali.
- `docker compose config` non contiene servizi o Dockerfile inesistenti.
- Le workflow non referenziano cartelle inesistenti.

## Verifiche finali

Esegui e riporta output sintetico:

```bash
dotnet sln OpenCashFlow.sln list
dotnet restore OpenCashFlow.sln
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
docker compose config
```
