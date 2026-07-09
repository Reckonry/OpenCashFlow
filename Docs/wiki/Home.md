# OpenCashFlow Wiki

OpenCashFlow is a Developer Preview / Early Self-Hosted Preview.

Start here:

- [Docker Compose Install](Docker-Compose-Install)
- [First-Run Setup Wizard](../setup/first-run-setup-wizard.md)
- [Production Hardening](../ops/production-hardening.md)
- [Backup And Restore Drill](../ops/backup-restore-drill.md)

## Manual Wiki Publishing

This repository keeps wiki source files under `Docs/wiki/`. They are prepared for manual publishing only.

```bash
git clone https://github.com/Reckonry/OpenCashFlow.wiki.git
cp Docs/wiki/*.md OpenCashFlow.wiki/
cd OpenCashFlow.wiki
git add .
git commit -m "Add Docker Compose install guide"
git push
```

Do not publish automatically from CI until the wiki workflow and permissions are explicitly reviewed.
