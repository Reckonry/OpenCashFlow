# OpenCashFlow Roadmap Prompts

Questa cartella contiene prompt operativi per trasformare OpenCashFlow da progetto SaaS ibrido a pacchetto open-source installabile, con una direzione simile a Odoo: core community auto-ospitabile, moduli opzionali e servizi gestiti separati.

## Ordine consigliato

1. `00-ripristino-tecnico.md` - rendere repository, build, Docker e CI riproducibili.
2. `01-de-saas-core.md` - rimuovere il blocco subscription/SaaS dal core.
3. `02-installer-open-source.md` - creare onboarding e configurazione da primo avvio.
4. `03-security-compliance.md` - bonifica segreti, ruoli, sicurezza e audit.
5. `04-prodotto-core.md` - stabilizzare funzionalita cash-flow e UX principale.
6. `05-moduli-odoo-like.md` - preparare architettura modulare e offerta tipo Odoo.

## Come usare i prompt

Ogni documento contiene un prompt pronto da incollare in Codex o passare a un dev. Il prompt e strutturato con:

- contesto del repository;
- obiettivo della fase;
- vincoli tecnici;
- attivita richieste;
- criteri di accettazione;
- verifiche finali.

Le fasi sono intenzionalmente sequenziali. Non conviene iniziare la parte modulare prima che build, Docker e core non-SaaS siano stabili.
