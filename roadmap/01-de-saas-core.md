# Fase 1 - De-SaaS Del Core

## Prompt operativo

Agisci come senior engineer e product architect su OpenCashFlow.

Obiettivo: rimuovere il principio SaaS dal core applicativo. OpenCashFlow deve diventare un software open-source installabile da un'azienda sul proprio server. I concetti multi-company possono restare, ma non devono piu rappresentare clienti paganti di un SaaS.

## Contesto rilevante

- `SubscriptionAuthorizationMiddleware` blocca accessi senza subscription valida.
- `Program.cs` registra il middleware subscription, ma i servizi Stripe/Billing risultano in parte commentati.
- Entita come `Plan`, `Company_Subscription`, campi Stripe e DTO Billing sono mischiati al dominio core.
- UI App/Admin contiene pagine billing, plans, subscriptions, Stripe portal e pricing.

## Strategia desiderata

- Core community installabile: pagamenti, cassa, dashboard, utenti, azienda, ruoli, audit.
- Multi-company opzionale: utile per consulenti, gruppi o studi multi-cliente, non per monetizzazione SaaS.
- Billing/Stripe fuori dal core: modulo opzionale, feature flag o area legacy disattivata.
- Admin diventa "Instance Admin", non "SaaS Super Admin".

## Attivita richieste

1. Disattiva il blocco subscription nel core:
   - `SubscriptionAuthorizationOptions.Enabled` deve essere `false` di default;
   - oppure il middleware non deve essere registrato salvo configurazione esplicita;
   - nessun utente deve essere bloccato per assenza di subscription su installazione standard.

2. Isola la parte SaaS/Billing:
   - mantieni il codice se serve, ma rendilo opzionale;
   - introduci una configurazione tipo `Features:Billing=false`;
   - se billing e disabilitato, controller/servizi/UI non devono rompere il runtime;
   - evita chiamate Stripe quando non configurato.

3. Ripulisci il linguaggio prodotto:
   - sostituisci dove opportuno "subscription expired", "plan required", "customer portal" con concetti opzionali o legacy;
   - nel core parla di "azienda", "workspace", "istanza".

4. Rivedi entita e DTO:
   - non cancellare subito `Plan` e `Company_Subscription` se causerebbe migrazioni rischiose;
   - documenta cosa resta legacy e cosa verra spostato in modulo;
   - separa mentalmente `Company` da "cliente SaaS".

5. Aggiorna UI:
   - nascondi menu Billing/Plans/Subscriptions quando billing e disabilitato;
   - rimuovi CTA verso Stripe dal percorso principale;
   - conserva eventuali schermate admin solo se feature flag attiva.

6. Aggiorna documentazione:
   - README deve descrivere il progetto come pacchetto self-hosted;
   - spiega che servizi gestiti/hosting sono offerte esterne, non requisito del software.

## Vincoli

- Non distruggere il modello dati in questa fase.
- Evita migrazioni invasive salvo necessario.
- Il core deve funzionare senza Stripe, senza subscription, senza SMTP reale.
- Mantieni compatibilita con test esistenti dove possibile.

## Criteri di accettazione

- Una nuova installazione non richiede subscription per usare dashboard, pagamenti, cassa e azienda.
- `SubscriptionAuthorizationMiddleware` non blocca il core di default.
- App e Admin non mostrano billing SaaS se la feature e disabilitata.
- Stripe non e richiesto per avviare API/App/Admin.
- README e configurazioni non posizionano il prodotto come SaaS obbligatorio.

## Verifiche finali

Esegui:

```bash
dotnet build OpenCashFlow.sln
dotnet test OpenCashFlow.sln
rg -n "opencashflow\\.cloud|subscription-expired|Stripe|CreateCheckout|CustomerPortal|SubscriptionAuthorization" src README.md docker-compose*.yml
```

Per ogni match rimasto, indica se e core, legacy o modulo opzionale.
