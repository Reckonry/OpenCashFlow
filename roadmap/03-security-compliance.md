# Fase 3 - Sicurezza E Compliance

## Prompt operativo

Agisci come security-minded senior engineer.

Obiettivo: bonificare sicurezza, segreti, ruoli, sessioni e audit per rendere OpenCashFlow adatto a essere pubblicato e installato da aziende reali.

## Contesto rilevante

- Esistono segreti statici in appsettings e factory.
- Esistono seed con utenti, hash password, dati personali e PIN hash.
- Ruoli `GIManagers` e `OCFManagers` sono incoerenti.
- Fast login via PIN esiste e va messo sotto controlli forti.
- Soft delete e audit non sono coerenti in tutto il dominio.

## Attivita richieste

1. Bonifica segreti:
   - rimuovi JWT statici da config committata;
   - rimuovi password SMTP e connection string reali;
   - aggiorna `.env.example` con placeholder sicuri;
   - documenta rotazione segreti.

2. Rivedi ruoli e policy:
   - definisci ruoli standard: `InstanceAdmin`, `CompanyAdmin`, `Employee`;
   - mappa o depreca `GIManagers`/`OCFManagers`;
   - allinea API, App, Admin e seed.

3. Hardening autenticazione:
   - cookie `HttpOnly`, `Secure`, `SameSite` coerenti;
   - durata sessioni configurabile;
   - gestione refresh token se esistente;
   - rate limit su login, reset password e fast login.

4. Hardening fast login:
   - PIN mai in chiaro;
   - limiti tentativi;
   - scadenza o rotazione PIN configurabile;
   - log audit su successo/fallimento;
   - possibilita di disabilitarlo per istanza.

5. Audit:
   - eventi minimi: login, logout, reset password, CRUD pagamenti, export, cambio ruoli, configurazione azienda;
   - audit deve essere tenant-scoped;
   - non loggare token, password, PIN o segreti.

6. Soft delete:
   - decidere regola unica per pagamenti e lookup;
   - se esiste `IsDeleted`, evitare delete fisico salvo purge amministrativa;
   - aggiornare repository e test.

7. Licenze e terze parti:
   - creare `THIRD-PARTY-NOTICES.md`;
   - verificare licenze asset vendorizzati;
   - rimuovere asset non redistribuibili.

## Vincoli

- Non introdurre dipendenze enterprise o servizi obbligatori.
- Non rompere il setup locale.
- Non loggare dati sensibili durante nuove verifiche.

## Criteri di accettazione

- Nessun segreto reale in repo.
- Ruoli e policy sono coerenti tra API/App/Admin.
- Login e fast login hanno rate limit e audit.
- Soft delete e audit pagamenti sono coerenti.
- `THIRD-PARTY-NOTICES.md` esiste e copre librerie principali.

## Verifiche finali

Esegui:

```bash
dotnet build OpenCashFlow.sln
dotnet test OpenCashFlow.sln
rg -n "password|secret|Stripe__SecretKey|SuperSoldi|K7jRvDkh|baron|nestia|GIManagers|OCFManagers" src .env.example docker-compose*.yml README.md SECURITY.md
```

Classifica i match rimasti in:

- placeholder sicuro;
- documentazione;
- codice legacy da rimuovere;
- problema ancora aperto.
