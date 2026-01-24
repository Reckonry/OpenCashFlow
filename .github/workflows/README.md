# CI – OpenCashFlow (Public Repository)

> Public CI setup focused on build, test, and artifact generation. Deployment steps are intentionally omitted.

Questo repo usa **3 workflow** principali:

1. **Merge & Build/Test su master**  
   `ci-wip-to-master.yml`  
   Per PR/push su `master`: builda e testa; opzionalmente fa merge automatico tra rami via `workflow_dispatch`.

2. **RC / Staging (artifact-based)**  
   `ci-master-to-staging.yml`  
   Su tag **RC** compila *solo i componenti necessari*, pubblica gli **artifact** (cartelle `publish/` + bundle Docker `.tar.gz`), crea/aggiorna la **Release RC (prerelease)**.

3. **Production (artifact-only, no build)**  
   `ci-staging-to-production.yml`  
   Su tag **stabili** scarica gli **artifact RC** corrispondenti e li utilizza per il rilascio. Niente rebuild.

---

## 🏷️ Convenzione tag

| Scopo | Globale | Per componente |
|---|---|---|
| **RC (Staging)** | `vX.Y.Z-RC` | `app-vX.Y.Z-RC`, `api-…-RC`, `landing-…-RC`, `www-…-RC`, `admin-…-RC` |
| **Produzione** | `vX.Y.Z` | `app-vX.Y.Z`, `api-vX.Y.Z`, `landing-vX.Y.Z`, `www-vX.Y.Z`, `admin-vX.Y.Z` |

- In **Prod**, il gate verifica che esista una RC compatibile (es. `v1.2.3` ⇒ richiede `v1.2.3-RC`).  
- Per i tag **per componente** in Prod, è accettata anche la presenza del **plain tag** RC corrispondente (`app-v1.2.3` accetta `app-v1.2.3-RC` *o* `app-v1.2.3`).

---

## 📦 Artifact (generati in RC ➜ usati in Prod)
Per **ogni componente** abilitato in RC vengono generati **due** artifact omonimi:

- `*-publish/`  
  Cartella del `dotnet publish` da utilizzare nei deployment.
- `*-docker/*.tar.gz`  
  Bundle Docker salvato con `docker save | gzip` (allegato anche alla Release RC). Gli artifact vengono trasferiti tra workflow usando `dawidd6/action-download-artifact@v11`, che permette di scaricare artifact da run precedenti (non possibile con `actions/download-artifact` ufficiale).

**Nomi standard:** `app-publish`, `api-publish`, `landing-publish`, `www-publish`, `admin-publish` e `app-docker`, `api-docker`, `landing-docker`, `www-docker`, `admin-docker`.

---

## 🔧 Dettagli workflow

### 1) `ci-wip-to-master.yml`
- **Trigger:** push/PR su `master`, più `workflow_dispatch` con scelta rami.
- **Cosa fa:** restore ➜ build ➜ test; merge automatico opzionale `source → master`.
- **Permessi:** `contents: write` (per il push del merge).

### 2) `ci-master-to-staging.yml` (RC → Staging)
- **Trigger:** tag RC (globali o per componente) o `workflow_dispatch`.
- **Scope:** deciso dal tag (`app-…-RC`, `api-…-RC`, ecc.).
- **Build:** `dotnet publish` **solo** per i componenti selezionati.
- **Artifact:** upload di `*-publish` e `*-docker`.
- **Release RC:** `softprops/action-gh-release@v2` (prerelease) con allegati i bundle docker.
- **Ottimizzazioni:**
  - Concurrency: 
    ```yaml
    concurrency:
      group: rc-${{ github.ref_name }}
      cancel-in-progress: false
    ```
  - .NET flags: `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1`, `DOTNET_CLI_TELEMETRY_OPTOUT=1`
  - **ACT**: se `ACT=true` ➜ `rsync --dry-run` equivalente, salta eventuali passaggi non necessari.
  - Secrets mappati in `env` con fallback `''` per evitare warning linter.

### 3) `ci-staging-to-production.yml` (Prod)
- **Trigger:** tag stabili (globali o per componente) o `workflow_dispatch`.
- **Gate RC:** verifica tramite `git` tags l'esistenza della RC corrispondente (globale o del componente).
- **Scope:** globale `vX.Y.Z` = **tutti** i componenti; altrimenti solo il componente del tag.
- **Build:** **nessuna**.
- **Download artifact:** usa `dawidd6/action-download-artifact@v11` per scaricare da run RC precedenti.
- **Artifact:** download dei `*-publish`/`*-docker` prodotti in RC.
- **Ottimizzazioni:**
  - Concurrency:
    ```yaml
    concurrency:
      group: production
      cancel-in-progress: false
    ```
  - .NET flags come sopra.
  - **ACT**: se `ACT=true` ➜ esegue dry-run equivalente.
  - Secrets mappati in `env` con fallback `''`.

---

## 🔐 Secrets

This public repository does not require any infrastructure or deployment secrets.

All workflows are designed to run without SSH keys or server credentials.

---

## 🧪 Local Testing with act

Only a `GITHUB_TOKEN` (dummy value allowed) is required for local dry-run testing.

---

## ✅ Checklist rilascio
1. **Tag RC** del/i componente/i (o globale) ➜ es. `app-v1.2.3-RC`.
2. Verifica che tutto funzioni.
3. **Tag Prod** corrispondente ➜ es. `app-v1.2.3` (o `v1.2.3`).
4. La pipeline Prod scarica gli **artifact RC** e procede con il rilascio.

---

## ℹ️ Notes

- This repository demonstrates a production-grade CI design.
- Deployment pipelines are intentionally excluded from the public version.
- Artifact-based promotion (RC ➜ Production) is kept to show release discipline.
# CI – OpenCashFlow (Public Repository)

> Public CI setup focused on build, test, and artifact generation. Deployment steps are intentionally omitted.

This repository uses **3 main workflows**:

1. **Merge & Build/Test on master**  
   `ci-wip-to-master.yml`  
   On PRs/pushes to `master`: restores, builds, and tests; optional automatic branch merge via `workflow_dispatch`.

2. **RC / Staging (artifact-based)**  
   `ci-master-to-staging.yml`  
   On **RC tags**, builds *only the required components*, publishes **artifacts** (`publish/` folders + Docker bundles `.tar.gz`), and creates or updates the **RC Release (prerelease)**.

3. **Production (artifact-only, no build)**  
   `ci-staging-to-production.yml`  
   On **stable tags**, downloads the corresponding **RC artifacts** and uses them for the release. No rebuilds.

---

## 🏷️ Tag Convention

| Purpose | Global | Per component |
|---|---|---|
| **RC (Staging)** | `vX.Y.Z-RC` | `app-vX.Y.Z-RC`, `api-…-RC`, `landing-…-RC`, `www-…-RC`, `admin-…-RC` |
| **Production** | `vX.Y.Z` | `app-vX.Y.Z`, `api-vX.Y.Z`, `landing-vX.Y.Z`, `www-vX.Y.Z`, `admin-vX.Y.Z` |

- In **Production**, a gate checks that a compatible RC exists (e.g. `v1.2.3` ⇒ requires `v1.2.3-RC`).  
- For **per-component** Production tags, the corresponding **plain RC tag** is also accepted (`app-v1.2.3` accepts `app-v1.2.3-RC` *or* `app-v1.2.3`).

---

## 📦 Artifacts (generated in RC ➜ used in Production)

For **each enabled component** in RC, **two artifacts** are generated:

- `*-publish/`  
  Output folder from `dotnet publish`, intended for deployment usage.
- `*-docker/*.tar.gz`  
  Docker bundle created via `docker save | gzip` (also attached to the RC Release).  
  Artifacts are transferred between workflows using `dawidd6/action-download-artifact@v11`, which allows downloading artifacts from previous runs (not supported by the official `actions/download-artifact`).

**Standard names:**  
`app-publish`, `api-publish`, `landing-publish`, `www-publish`, `admin-publish`  
and  
`app-docker`, `api-docker`, `landing-docker`, `www-docker`, `admin-docker`.

---

## 🔧 Workflow Details

### 1) `ci-wip-to-master.yml`
- **Trigger:** push/PR on `master`, plus `workflow_dispatch` with branch selection.
- **What it does:** restore ➜ build ➜ test; optional automatic merge `source → master`.
- **Permissions:** `contents: write` (required for merge push).

### 2) `ci-master-to-staging.yml` (RC → Staging)
- **Trigger:** RC tags (global or per component) or `workflow_dispatch`.
- **Scope:** determined by the tag (`app-…-RC`, `api-…-RC`, etc.).
- **Build:** `dotnet publish` **only** for selected components.
- **Artifacts:** upload of `*-publish` and `*-docker`.
- **RC Release:** `softprops/action-gh-release@v2` (prerelease) with Docker bundles attached.
- **Optimizations:**
  - Concurrency:
    ```yaml
    concurrency:
      group: rc-${{ github.ref_name }}
      cancel-in-progress: false
    ```
  - .NET flags: `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1`, `DOTNET_CLI_TELEMETRY_OPTOUT=1`
  - **ACT:** if `ACT=true` ➜ runs dry-run equivalents, skips non-essential steps.
  - Secrets mapped in `env` with empty fallback to avoid linter warnings.

### 3) `ci-staging-to-production.yml` (Production)
- **Trigger:** stable tags (global or per component) or `workflow_dispatch`.
- **RC Gate:** checks via `git` tags that a corresponding RC exists (global or component-level).
- **Scope:** global `vX.Y.Z` = **all** components; otherwise only the tagged component.
- **Build:** **none**.
- **Artifact download:** uses `dawidd6/action-download-artifact@v11` to fetch artifacts from previous RC runs.
- **Artifacts:** download of `*-publish` / `*-docker` generated in RC.
- **Optimizations:**
  - Concurrency:
    ```yaml
    concurrency:
      group: production
      cancel-in-progress: false
    ```
  - Same .NET flags as above.
  - **ACT:** if `ACT=true` ➜ executes dry-run equivalents.
  - Secrets mapped in `env` with empty fallback.

---

## 🔐 Secrets

This public repository does not require any infrastructure or deployment secrets.

All workflows are designed to run without SSH keys or server credentials.

---

## 🧪 Local Testing with act

Only a `GITHUB_TOKEN` (dummy value allowed) is required for local dry-run testing.

---

## ✅ Release Checklist

1. Create **RC tag(s)** for component(s) or global ➜ e.g. `app-v1.2.3-RC`.
2. Verify that everything works as expected.
3. Create the matching **Production tag** ➜ e.g. `app-v1.2.3` (or `v1.2.3`).
4. The Production pipeline downloads the **RC artifacts** and proceeds with the release.

---

## ℹ️ Notes

- This repository demonstrates a production-grade CI design.
- Deployment pipelines are intentionally excluded from the public version.
- Artifact-based promotion (RC ➜ Production) is kept to show release discipline.