# PostgreSQL Backup – Guida Completa

Questa guida descrive come configurare backup giornalieri automatici di un database PostgreSQL su un server Linux (Ubuntu/Debian).  
I dump vengono compressi, salvati in locale, mantenuti per 7 giorni e resi portabili (nessun vincolo di utenti/ruoli).

---

## 1. Installazione pacchetti (da **root**)

Assicurati di avere i tool di client PostgreSQL e `gzip`:
```bash
apt update
apt install -y postgresql-client gzip
```

---

## 2. Creazione utente DB di sola lettura (da **root**)

Accedi come utente `postgres` e crea il ruolo `backup_user` con password forte:

```bash
sudo -u postgres psql -v ON_ERROR_STOP=1 -c "CREATE ROLE backup_user LOGIN PASSWORD 'SCEGLI_UNA_PASSWORD_FORTE';"
```

---

## 3. Concessione permessi minimi (da **root**)

### Sul database `opencashflow_db`
```bash
sudo -u postgres psql -d postgres -c "GRANT CONNECT ON DATABASE opencashflow_db TO backup_user;"

sudo -u postgres psql -d opencashflow_db -c "GRANT USAGE ON SCHEMA public TO backup_user;"
sudo -u postgres psql -d opencashflow_db -c "GRANT SELECT ON ALL TABLES IN SCHEMA public TO backup_user;"
sudo -u postgres psql -d opencashflow_db -c "GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO backup_user;"
sudo -u postgres psql -d opencashflow_db -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO backup_user;"
sudo -u postgres psql -d opencashflow_db -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON SEQUENCES TO backup_user;"
```

*(ripeti lo stesso blocco se vuoi abilitare anche altri DB)*

---

## 4. File `.pgpass` (da **root**, per l’utente Linux `opencashflow`)

Questo file serve a non digitare la password a mano. Verrà salvato nella home dell’utente Linux `opencashflow`.

```bash
su - opencashflow -c 'printf "127.0.0.1:5432:opencashflow_db:backup_user:SCEGLI_UNA_PASSWORD_FORTE\n" > ~/.pgpass && chmod 600 ~/.pgpass'
```

Verifica:
```bash
su - opencashflow -c 'cat ~/.pgpass'
```

---

## 5. Cartelle di backup e log (da **root**)

Creiamo le directory dove andranno i dump e il log:

```bash
mkdir -p /var/backups/postgres
chown opencashflow:opencashflow /var/backups/postgres

mkdir -p /var/log/opencashflow
touch /var/log/opencashflow/pg_backup.log
chown opencashflow:opencashflow /var/log/opencashflow/pg_backup.log
```

---

## 6. Test manuale di un backup (da **root**, lanciando come utente `opencashflow`)

Esegui un dump manuale per verificare:
```bash
sudo -u opencashflow bash -lc '/usr/bin/pg_dump -h 127.0.0.1 -U backup_user --no-owner --no-privileges opencashflow_db | /usr/bin/gzip > /var/backups/postgres/opencashflow_db_test_$(/usr/bin/date +%F_%H-%M).sql.gz'
```

Controlla i file:
```bash
ls -lh /var/backups/postgres/
```

---

## 7. Configurazione cron (da **utente Linux `opencashflow`**)

Apri il crontab:
```bash
crontab -e
```

Aggiungi queste righe:

```
# Backup opencashflow_db ogni notte alle 2:00
0 2 * * * /usr/bin/pg_dump -h 127.0.0.1 -U backup_user --no-owner --no-privileges opencashflow_db | /usr/bin/gzip > /var/backups/postgres/opencashflow_db_$(/usr/bin/date +\%F_\%H-%M).sql.gz >> /var/log/opencashflow/pg_backup.log 2>&1

# Pulizia: elimina i file più vecchi di 7 giorni ogni notte alle 2:10
10 2 * * * /usr/bin/find /var/backups/postgres/ -type f -name "*.sql.gz" -mtime +7 -delete >> /var/log/opencashflow/pg_backup.log 2>&1
```

---

## 8. Verifica cron (da **utente Linux `opencashflow`**)

Controlla che il crontab sia registrato:
```bash
crontab -l
```

Dopo l’orario pianificato, verifica:
```bash
ls -lh /var/backups/postgres/
tail -n 50 /var/log/opencashflow/pg_backup.log
```

---

## 9. Ripristino di un backup (da qualsiasi host con PostgreSQL)

Crea un database vuoto e importa il dump:
```bash
createdb opencashflow_restore
gunzip -c opencashflow_db_2025-09-24_02-00.sql.gz | psql opencashflow_restore
```

Grazie a `--no-owner --no-privileges`, non servono utenti aggiuntivi.

---

## ✅ Risultato finale

- Backup giornaliero automatico del DB `opencashflow_db`.  
- Dump compressi in `/var/backups/postgres/`.  
- Log in `/var/log/opencashflow/pg_backup.log`.  
- Retention di 7 giorni.  
- File portabili, ripristinabili ovunque senza problemi di ruoli.  

# PostgreSQL Backup – Reference Guide (Generic Example)

This document describes a **generic approach** to configure automated daily backups of a PostgreSQL database on a Linux server (Ubuntu/Debian).

> ⚠️ **Important**
> This guide is intentionally **generic** and does **not** describe any specific production environment.
> Replace all placeholders (`<...>`) with values appropriate to your own setup.

Backups are:
- generated daily
- compressed
- stored locally
- retained for a fixed number of days
- portable (no role/ownership coupling)

---

## 1. Install required packages (as **root**)

Ensure PostgreSQL client tools and `gzip` are installed:

```bash
apt update
apt install -y postgresql-client gzip
```

---

## 2. Create a read-only database user (as **root**)

Connect as the `postgres` user and create a dedicated backup role:

```bash
sudo -u postgres psql -v ON_ERROR_STOP=1 -c "CREATE ROLE <backup_user> LOGIN PASSWORD '<STRONG_PASSWORD>';"
```

---

## 3. Grant minimal permissions (as **root**)

### On database `<database_name>`

```bash
sudo -u postgres psql -d postgres -c "GRANT CONNECT ON DATABASE <database_name> TO <backup_user>;"

sudo -u postgres psql -d <database_name> -c "GRANT USAGE ON SCHEMA public TO <backup_user>;"
sudo -u postgres psql -d <database_name> -c "GRANT SELECT ON ALL TABLES IN SCHEMA public TO <backup_user>;"
sudo -u postgres psql -d <database_name> -c "GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO <backup_user>;"
sudo -u postgres psql -d <database_name> -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO <backup_user>;"
sudo -u postgres psql -d <database_name> -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON SEQUENCES TO <backup_user>;"
```

*(Repeat this block for each database you want to back up.)*

---

## 4. `.pgpass` file (as **root**, for Linux user `<linux_user>`)

The `.pgpass` file allows passwordless authentication for `pg_dump`.
It must be readable **only** by its owner.

```bash
su - <linux_user> -c 'printf "127.0.0.1:5432:<database_name>:<backup_user>:<STRONG_PASSWORD>\n" > ~/.pgpass && chmod 600 ~/.pgpass'
```

Verify:
```bash
su - <linux_user> -c 'cat ~/.pgpass'
```

---

## 5. Backup and log directories (as **root**)

Create directories for backups and logs:

```bash
mkdir -p /var/backups/postgres
chown <linux_user>:<linux_user> /var/backups/postgres

mkdir -p /var/log/<app_name>
touch /var/log/<app_name>/pg_backup.log
chown <linux_user>:<linux_user> /var/log/<app_name>/pg_backup.log
```

---

## 6. Manual backup test (as **root**, running as `<linux_user>`)

Run a manual dump to verify the configuration:

```bash
sudo -u <linux_user> bash -lc '/usr/bin/pg_dump -h 127.0.0.1 -U <backup_user> --no-owner --no-privileges <database_name> | /usr/bin/gzip > /var/backups/postgres/<database_name>_test_$(/usr/bin/date +%F_%H-%M).sql.gz'
```

Check the output:
```bash
ls -lh /var/backups/postgres/
```

---

## 7. Cron configuration (as Linux user `<linux_user>`)

Edit the crontab:

```bash
crontab -e
```

Add the following entries:

```
# Daily backup at 02:00
0 2 * * * /usr/bin/pg_dump -h 127.0.0.1 -U <backup_user> --no-owner --no-privileges <database_name> | /usr/bin/gzip > /var/backups/postgres/<database_name>_$(/usr/bin/date +\%F_\%H-%M).sql.gz >> /var/log/<app_name>/pg_backup.log 2>&1

# Cleanup: delete backups older than 7 days (02:10)
10 2 * * * /usr/bin/find /var/backups/postgres/ -type f -name "*.sql.gz" -mtime +7 -delete >> /var/log/<app_name>/pg_backup.log 2>&1
```

Adjust schedule and retention as needed.

---

## 8. Cron verification (as Linux user `<linux_user>`)

Verify the crontab:
```bash
crontab -l
```

After the scheduled run, check:
```bash
ls -lh /var/backups/postgres/
tail -n 50 /var/log/<app_name>/pg_backup.log
```

---

## 9. Restore a backup (from any host with PostgreSQL)

Create an empty database and restore the dump:

```bash
createdb <restore_database>
gunzip -c <database_name>_YYYY-MM-DD_HH-MM.sql.gz | psql <restore_database>
```

Thanks to `--no-owner --no-privileges`, no additional roles are required.

---

## ✅ Final result

- Automated daily backups for `<database_name>`.
- Compressed dumps stored in `/var/backups/postgres/`.
- Logs written to `/var/log/<app_name>/pg_backup.log`.
- Retention policy (example: 7 days).
- Fully portable dumps, restorable in any environment.