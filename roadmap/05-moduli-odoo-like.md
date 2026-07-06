# Fase 5 - Architettura Modulare Stile Odoo

## Prompt operativo

Agisci come software architect e product strategist.

Obiettivo: progettare e iniziare a implementare una struttura modulare per OpenCashFlow, simile nel principio a Odoo: core community installabile, moduli opzionali, possibilita di servizi gestiti, supporto e configurazioni professionali.

## Direzione prodotto

OpenCashFlow non deve tornare a essere un SaaS obbligatorio. Deve essere:

- installabile autonomamente;
- estendibile con moduli;
- configurabile per aziende diverse;
- monetizzabile tramite hosting gestito, supporto, setup, formazione, moduli avanzati o marketplace.

## Moduli candidati

- `Core.CashFlow`: pagamenti, cassa, dashboard base.
- `Billing`: subscription/Stripe/manual billing, opzionale.
- `ItalianEInvoicing`: fattura elettronica italiana, SDI, XML.
- `Forecasting`: previsioni cash-flow.
- `CRM`: clienti/fornitori leggero.
- `Inventory`: magazzino leggero se collegato ai flussi.
- `HR`: dipendenti, presenze, turni, costi.
- `Reports`: report avanzati, PDF, XLSX, scheduled reports.

## Attivita richieste

1. Definisci modello moduli:
   - manifest modulo;
   - nome, versione, dipendenze, feature flags;
   - migrazioni per modulo;
   - menu/UI registrabili;
   - servizi registrabili.

2. Separa core da optional:
   - identifica namespace e cartelle da spostare;
   - Billing/Stripe e primo candidato;
   - evita big bang: prepara interfacce e feature flags prima dello spostamento fisico.

3. Disegna registry:
   - elenco moduli disponibili;
   - moduli installati;
   - stato abilitato/disabilitato per istanza;
   - dipendenze e compatibilita versione.

4. Progetta migrazioni modulari:
   - core migration sempre necessarie;
   - migration modulo applicate solo se modulo installato;
   - rollback o disinstallazione documentata.

5. UI modulare:
   - menu generato da moduli;
   - permessi per modulo;
   - pagine admin per abilitare/disabilitare moduli;
   - nessun menu morto se modulo disabilitato.

6. Packaging:
   - community edition nel repo;
   - moduli ufficiali nella stessa repo o repo separate;
   - documentazione per installare moduli;
   - versione semantica.

7. Strategia commerciale non invasiva:
   - hosting gestito esterno;
   - supporto e configurazione;
   - moduli enterprise opzionali;
   - nessun blocco artificiale nel core AGPL.

## Vincoli

- Non implementare marketplace completo subito.
- Non spezzare il core funzionante.
- Evita plugin dinamici complessi se una modularizzazione statica con feature flags basta nella prima iterazione.
- Mantieni compatibilita AGPL e documenta licenze.

## Criteri di accettazione

- Esiste una proposta tecnica scritta per moduli.
- Billing/Stripe e marcato come modulo opzionale.
- Il core puo avviarsi senza moduli opzionali.
- Menu e permessi sono predisposti per feature/module flags.
- Roadmap moduli ha priorita e dipendenze chiare.

## Verifiche finali

Produci almeno:

- `docs/modules/architecture.md`
- `docs/modules/billing-module.md`
- `docs/modules/module-manifest.md`
- issue/task list per estrarre Billing dal core

Esegui:

```bash
dotnet build OpenCashFlow.sln
rg -n "Stripe|Billing|Subscription|Plan" src
```

Classifica i match come:

- core legittimo;
- modulo Billing;
- legacy da rimuovere;
- documentazione.
