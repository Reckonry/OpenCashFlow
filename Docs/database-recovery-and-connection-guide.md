# 📘 Database Recovery & Connection Guide

Questa guida ti permette di ripristinare completamente il database PostgreSQL per il progetto `OpenCashFlow` e riconnettere correttamente l'API in caso di problemi. È pensata per gestire scenari critici in produzione e in fase di sviluppo.

---

## 🔁 1. Reset completo del database

### 🔪 Terminare connessioni attive

```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'opencashflow_db'
  AND pid <> pg_backend_pid();
```

### 💣 Drop & Create database

```sql
DROP DATABASE IF EXISTS opencashflow_db;
CREATE DATABASE opencashflow_db;
```

### 👤 Creazione utente (se non esiste già)

```sql
CREATE USER opencashflow WITH PASSWORD 'your_strong_password';
```

### 🛡️ Assegnazione privilegi completi

```sql
GRANT USAGE ON SCHEMA public TO opencashflow;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO opencashflow;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO opencashflow;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO opencashflow;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT USAGE, SELECT ON SEQUENCES TO opencashflow;
```

---

## 🧱 2. Caricamento schema iniziale

Assicurati di avere lo script `init_schema.sql` nella directory `/scripts` del progetto.

### ▶️ Esecuzione dello script:

```bash
psql -U postgres -d opencashflow_db -f ./scripts/init_schema.sql
```

---

## 🔌 3. Verifica connessione API

> ❗ Importante: la stringa di connessione **NON** viene letta da `appsettings.json` se l'API viene avviata come servizio Linux.

### ✅ Dove controllarla:

* File systemd del servizio (es: `/etc/systemd/system/OpenCashFlow.API.service`)
* Parametri di avvio del servizio (es: `--connectionStrings:DefaultConnection=...`)
* Variabili d'ambiente

### 🛠 Come aggiornare temporaneamente:

```bash
sudo systemctl edit OpenCashFlow.API.service
```

> Modifica la riga `ExecStart` per puntare alla nuova stringa di connessione

### 🔁 Ricarica il servizio:

```bash
sudo systemctl daemon-reexec
sudo systemctl daemon-reload
sudo systemctl restart OpenCashFlow.API.service
```

---

## ✅ 4. Verifiche post-deploy

### Test API:

* `/v1/Authentication/login`
* `/ping` se presente

### Log da controllare:

```bash
journalctl -u OpenCashFlow.API.service -f
```

---

## 🚨 5. Errori comuni & soluzioni

### 🔐 `42501: permission denied for table AspNetUsers`

> L’utente non ha i privilegi sul DB:

* Verifica che l'utente nel DB abbia i privilegi sulle tabelle (vedi sezione 1).

### 🔒 `FATAL: role does not exist`

> L’utente specificato nella stringa di connessione non esiste nel DB:

* Crea l'utente con `CREATE USER`.

### 🧨 `28P01: password authentication failed`

> Password errata nella stringa di connessione:

* Verifica che la password nel servizio o nelle env var corrisponda a quella del database.

### 🔁 `database is being accessed by other users`

> Stai cercando di droppare un database ancora utilizzato:

* Usa il comando `pg_terminate_backend` per terminare le connessioni.

---

## 🧠 Suggerimento finale

> Documenta SEMPRE ogni variazione alla connessione, e salva una copia aggiornata dello script di init per avere sempre pronto un ripristino rapido.

# 📘 Database Recovery & Connection Guide (Generic)

This guide describes a **generic procedure** to fully reset, recover, and reconnect a PostgreSQL database for an application and its API, both in development and production-like environments.

> ⚠️ **Important**
> This document is intentionally **generic** and does **not** describe any specific production environment.
> Replace all placeholders (`<...>`) with values appropriate to your own setup.

---

## 🔁 1. Full database reset

### 🔪 Terminate active connections

```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = '<database_name>'
  AND pid <> pg_backend_pid();
```

### 💣 Drop & recreate database

```sql
DROP DATABASE IF EXISTS <database_name>;
CREATE DATABASE <database_name>;
```

### 👤 Create application user (if not already present)

```sql
CREATE USER <db_user> WITH PASSWORD '<STRONG_PASSWORD>';
```

### 🛡️ Grant required privileges

```sql
GRANT USAGE ON SCHEMA public TO <db_user>;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO <db_user>;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO <db_user>;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO <db_user>;

ALTER DEFAULT PRIVILEGES IN SCHEMA public
GRANT USAGE, SELECT ON SEQUENCES TO <db_user>;
```

---

## 🧱 2. Load initial schema

Ensure you have an initialization script (for example `init_schema.sql`) available in your project.

### ▶️ Execute the script

```bash
psql -U postgres -d <database_name> -f ./scripts/init_schema.sql
```

---

## 🔌 3. API connection verification

> ❗ **Note**
> When an API is started as a Linux service, the connection string is often **not read** from `appsettings.json`.

### ✅ Where to check the connection string

- systemd service file (e.g. `/etc/systemd/system/<app_name>.API.service`)
- Service start parameters (e.g. `--connectionStrings:DefaultConnection=...`)
- Environment variables

### 🛠 Temporary update (systemd override)

```bash
sudo systemctl edit <app_name>.API.service
```

> Update the `ExecStart` line to point to the new connection string.

### 🔁 Reload and restart the service

```bash
sudo systemctl daemon-reexec
sudo systemctl daemon-reload
sudo systemctl restart <app_name>.API.service
```

---

## ✅ 4. Post-deploy checks

### API endpoints to test

- `/v1/authentication/login`
- `/ping` (if implemented)

### Logs to inspect

```bash
journalctl -u <app_name>.API.service -f
```

---

## 🚨 5. Common errors & solutions

### 🔐 `42501: permission denied for table ...`

> The database user does not have sufficient privileges.

- Verify that the user has privileges on tables and sequences (see section 1).

### 🔒 `FATAL: role does not exist`

> The role specified in the connection string does not exist.

- Create the user with `CREATE USER`.

### 🧨 `28P01: password authentication failed`

> Invalid password in the connection string.

- Ensure the password in the service or environment variables matches the database user password.

### 🔁 `database is being accessed by other users`

> You are trying to drop a database that is still in use.

- Use `pg_terminate_backend` to terminate active connections.

---

## 🧠 Final recommendation

Always document **every change** to database connections and keep an up-to-date schema initialization script to ensure fast and reliable recovery.