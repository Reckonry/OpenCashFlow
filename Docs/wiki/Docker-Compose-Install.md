# Docker Compose Install

OpenCashFlow preview releases run as a Docker Compose stack with three services:

- `db`
- `api`
- `webapp`

## Minimal Install

```bash
curl -LO https://github.com/Reckonry/OpenCashFlow/releases/download/v0.1.0-preview.2/OpenCashFlow-0.1.0-preview.2.tar.gz
tar -xzf OpenCashFlow-0.1.0-preview.2.tar.gz
cd OpenCashFlow-0.1.0-preview.2
cp .env.example .env
docker compose -f docker-compose.release.yml up -d
```

Open:

```text
http://localhost:5200
```

On a fresh database, complete `/Setup`.

## Important Configuration

Edit `.env` before shared use:

- change `JWT_SECRET`;
- change `POSTGRES_PASSWORD`;
- set `APP_URL` to the real WebApp URL;
- configure SMTP if email features are needed.

## Visual Guide

The source repository includes a visual guide:

```text
Docs/setup/docker-compose-visual-guide.md
```

## Manual Wiki Publishing

```bash
git clone https://github.com/Reckonry/OpenCashFlow.wiki.git
cp Docs/wiki/*.md OpenCashFlow.wiki/
cd OpenCashFlow.wiki
git add .
git commit -m "Add Docker Compose install guide"
git push
```
