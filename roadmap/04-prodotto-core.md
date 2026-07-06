# Fase 4 - Stabilizzazione Prodotto Core

## Prompt operativo

Agisci come product engineer su OpenCashFlow.

Obiettivo: stabilizzare il prodotto core open-source: gestione cash-flow, pagamenti, cassa, dashboard, utenti, azienda, export e UX quotidiana. Questa fase deve rendere il software utile per una piccola/media azienda che lo installa internamente.

## Funzionalita core da proteggere

- Login e gestione utenti.
- Azienda/workspace.
- Pagamenti in entrata e uscita.
- Metodi pagamento e tipi documento configurabili.
- Cassa e ledger.
- Dashboard e filtri.
- Audit.
- Export e reportistica.

## Attivita richieste

1. Pagamenti:
   - validazioni coerenti su importo, data, entry type, metodo, documento;
   - soft delete coerente;
   - idempotenza preservata;
   - filtri robusti;
   - paginazione e ordinamento sicuri.

2. Cassa:
   - ledger sempre coerente con pagamenti cash-like;
   - rebuild balance amministrativo;
   - audit su rettifiche manuali;
   - test su update/delete/reapply.

3. Dashboard:
   - KPI chiari per periodo;
   - entrate, uscite, saldo, trend;
   - filtri per data/metodo/utente;
   - rimuovere elementi non core o demo.

4. Export:
   - CSV almeno per pagamenti e cassa;
   - XLSX opzionale;
   - PDF report opzionale;
   - export tenant-scoped e auditato.

5. UI/UX:
   - pulire partial/modali inutilizzati da template;
   - menu coerente con core;
   - responsive mobile per pagamenti e dashboard;
   - testi IT/EN principali completi.

6. Test:
   - unit test su servizi pagamenti/cassa;
   - integration test API principali;
   - test regressione su tenant isolation;
   - test per soft delete.

## Vincoli

- Non reintrodurre dipendenze SaaS nel core.
- Non modificare architettura radicalmente se non serve.
- Mantieni UX pratica e operativa, non marketing.

## Criteri di accettazione

- Un admin puo creare azienda, utenti, lookup e pagamenti.
- Entrate/uscite aggiornano dashboard e cassa correttamente.
- Delete/update pagamenti non corrompono ledger.
- Export funziona e rispetta tenant.
- UI core non mostra pricing, subscription o Stripe se billing e disabilitato.

## Verifiche finali

Esegui:

```bash
dotnet build OpenCashFlow.sln
dotnet test OpenCashFlow.sln
rg -n "Stripe|Subscription|Plan|Pricing|Customer Portal|Upgrade" src/OpenCashFlow.WebApp/Views src/OpenCashFlow.Admin/Views
```

I match UI rimasti devono essere dietro feature flag o documentati come modulo opzionale.
