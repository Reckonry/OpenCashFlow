# Fase 2 - Installer Open Source

## Prompt operativo

Agisci come senior engineer specializzato in prodotti self-hosted.

Obiettivo: rendere OpenCashFlow installabile da un'azienda o da un tecnico con un flusso chiaro di primo avvio. L'utente non deve modificare codice o appsettings per creare la prima azienda e il primo amministratore.

## Contesto rilevante

- Oggi esistono seed hardcoded in `ApplicationDbContext`.
- La registrazione crea azienda e utente, ma e pensata come flusso SaaS pubblico.
- Le configurazioni contengono domini e valori statici.
- Docker dovrebbe essere il percorso principale per aziende non tecniche.

## Attivita richieste

1. Definisci un modello di primo avvio:
   - se non esiste alcuna azienda/admin, mostra setup wizard;
   - crea `InstanceAdmin` e prima `Company`;
   - imposta lingua, valuta, timezone, paese;
   - SMTP deve essere opzionale.

2. Implementa configurazione self-hosted:
   - `.env.example` deve contenere solo variabili necessarie;
   - nessun valore reale o personale;
   - `APP_URL`, `API_URL`, `JWT_SECRET`, `DEFAULT_CONN_STRING`, `SMTP_*` opzionali.

3. Rivedi i seed:
   - rimuovi dati personali;
   - non creare utenti reali predefiniti;
   - mantieni solo ruoli e lookup di sistema;
   - se serve un admin demo, abilitalo solo con `SEED_DEMO_DATA=true`.

4. Crea documentazione installazione:
   - installazione con Docker Compose;
   - installazione manuale con .NET e PostgreSQL;
   - upgrade/migrazioni;
   - backup/restore;
   - reset password admin.

5. Rendi migrazioni prevedibili:
   - `AUTO_MIGRATE` deve essere esplicito;
   - in produzione documenta come applicare migration;
   - il design-time factory deve leggere env/config, non connection string hardcoded.

6. Migliora health checks:
   - endpoint API `/health`;
   - controllo database;
   - documentazione per reverse proxy.

## Vincoli

- Non richiedere servizi esterni per il primo avvio.
- Non rendere SMTP obbligatorio.
- Non usare credenziali predefinite note in produzione.
- Mantieni setup wizard idempotente: se l'istanza e gia configurata, non deve riapparire.

## Criteri di accettazione

- `docker compose up` avvia DB, API e App.
- Primo accesso mostra un setup wizard se DB vuoto.
- Dopo setup, utente admin puo accedere e usare il core.
- Nessun utente personale o hash credenziale reale e presente nei seed.
- `.env.example` e sufficiente per installare localmente.

## Verifiche finali

Esegui:

```bash
dotnet build OpenCashFlow.sln
docker compose config
rg -n "baron|nestia|SuperSoldi|opencashflow\\.cloud|YOUR_Password|K7jRvDkh" src .env.example docker-compose*.yml
```

Ogni match sensibile deve essere rimosso o giustificato come placeholder innocuo.
