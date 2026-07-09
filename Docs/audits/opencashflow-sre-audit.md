# OpenCashFlow SRE Audit

Date: 2026-07-08

Perspective: Senior Site Reliability Engineer evaluating whether OpenCashFlow can actually be operated. This review ignores software architecture except where it affects operability.

Scope: observable evidence in the repository only.

## Executive SRE Verdict

An operations team should not sleep comfortably with OpenCashFlow in business-critical production today.

The project has a credible local operations foundation: Docker Compose, API health endpoint, PostgreSQL container healthcheck, Release build/test gates, clean-install smoke automation, and an isolated backup/restore smoke drill. Those are useful and unusually concrete for a developer preview.

However, the production operations story is not complete. The root Compose file is explicitly local-evaluation oriented, secrets are environment-variable examples rather than secret-manager integration, `AUTO_MIGRATE=true` is the default in Compose, PostgreSQL is exposed on the host, observability is mostly logging guidance, Sentry is present but commented out, backup/restore is proven only against a disposable smoke stack, and upgrade/rollback is explicitly not fully performed.

If production goes down at 3 AM, an experienced engineer could probably inspect logs, check `/health`, restart containers and restore a small local-style database from a manual dump. They would not have a proven production-grade runbook, RPO/RTO target, alerting path, migration rollback drill or disaster recovery process.

## Evidence Reviewed

Files and areas inspected:

- `docker-compose.yml`
- `scripts/smoke/docker-compose.clean-install.yml`
- `scripts/smoke/clean-install-smoke.sh`
- `scripts/smoke/backup-restore-smoke-drill.sh`
- `Docs/ops/production-hardening.md`
- `Docs/ops/secrets-management.md`
- `Docs/ops/backup-restore-drill.md`
- `Docs/ops/upgrade-migration-drill.md`
- `Docs/testing/clean-install-smoke.md`
- `.github/workflows/quality-gates.yml`
- `.github/workflows/zap-baseline.yml`
- `.github/workflows/zap-full.yml`
- API/WebApp startup files for health, migrations, logging and headers.

Observed local verification from the current working tree:

- Release build previously passed with 0 warnings and 0 errors.
- Release tests previously passed with 261 passed, 0 failed, 0 skipped.
- `docker compose config` previously passed.
- Backup/restore smoke drill documentation records successful restore verification for `Companies`, `Payments`, and `CashLedgers` counts.

## Deployment

Current deployment maturity is local-evaluation focused.

Positive evidence:

- Root `docker-compose.yml` starts PostgreSQL, API and WebApp.
- API and WebApp Dockerfiles exist and are used by Compose.
- Docker Compose has a PostgreSQL healthcheck.
- Compose config validates.
- README and ops docs clearly warn that Compose defaults are local-evaluation defaults.

Operational concerns:

- PostgreSQL is exposed on host port `5432`.
- PostgreSQL credentials default to `postgres/postgres`.
- API connection string defaults to the same superuser-style credentials.
- `AUTO_MIGRATE=true` is set in root Compose.
- HTTP ports are exposed directly.
- No production Compose, Helm chart, Terraform, systemd unit, or reference reverse-proxy deployment is present.
- No demonstrated external PostgreSQL deployment path.
- No demonstrated blue/green, canary, rolling restart, or maintenance-window process.

SRE judgement: deployable locally; not production deployment-ready.

## Docker

Docker is useful but not hardened.

Strengths:

- Isolated smoke Compose stack exists with separate ports and project name.
- Smoke stack avoids colliding with the default local stack.
- `docker compose config` is a quality gate.

Weaknesses:

- Fixed container names in root Compose make multiple local instances awkward.
- Root Compose is not safe as a production template without substantial changes.
- No resource limits, restart policies, log driver settings, read-only filesystem settings, healthcheck for API/WebApp containers, or backup sidecar/job.
- Database data lives in a local Docker volume without retention or off-host replication.

SRE judgement: adequate for developer evaluation; insufficient for operations.

## Secrets

Secrets handling is documented, not operationalized.

Positive evidence:

- `Docs/ops/secrets-management.md` lists required production secrets and rotation procedures.
- README warns to replace secrets before exposure.
- `.env.example` exists.

Risks:

- Compose uses fallback JWT secret `local-development-secret-key-change-before-production-0001`.
- Compose uses `postgres/postgres`.
- No integration with Docker secrets, Kubernetes secrets, Vault, cloud secret manager, SOPS, 1Password, AWS/GCP/Azure secret managers, or equivalent.
- Rotation is a manual runbook, not an implemented operational procedure.

SRE judgement: acceptable for local evaluation; production secret management is unimplemented.

## Logging

Logging exists but is not operationally complete.

Positive evidence:

- API and WebApp use Serilog.
- WebApp logs to console and a file path under `../Logs`.
- API has Serilog startup configuration.
- Docs list operational signals that should be retained.

Concerns:

- No centralized log collection configuration.
- No structured log schema contract.
- No log retention policy implemented.
- No log redaction tests.
- Sentry packages/config exist, but Sentry startup/tracing appears commented out.
- Slack logging examples are commented and not a production alerting plan.

SRE judgement: enough for local debugging; not enough for incident response.

## Monitoring And Alerting

Monitoring maturity is low.

Positive evidence:

- API exposes `/health`.
- Docker Compose checks PostgreSQL health.
- CI waits for `/health` in smoke/ZAP flows.

Missing:

- WebApp health endpoint.
- Readiness/liveness separation.
- Database dependency health in API health output.
- Metrics endpoint.
- Dashboards.
- Alert thresholds.
- Pager policy.
- Synthetic monitoring.
- Backup freshness monitoring.
- Disk, CPU, memory and queue monitoring.
- Error-budget/SLO definitions.

SRE judgement: operators would detect problems manually or by external tooling they build themselves.

## Backups

Backup maturity has improved but remains local.

Positive evidence:

- `Docs/ops/backup-restore-drill.md` documents `pg_dump --format=custom`.
- `scripts/smoke/backup-restore-smoke-drill.sh` automates backup and restore using an isolated smoke stack.
- The documented local result verifies:
  - `Companies: 1`
  - `Payments: 1`
  - `CashLedgers: 1`
  - smoke-created company/payment/cash ledger matches.

Missing:

- Scheduled backup job.
- Off-host/off-region backup target.
- Encryption at rest for backup artifacts.
- Retention policy.
- Backup failure alerting.
- Backup restore ownership.
- RPO/RTO targets.
- Production database-size restore timing.

SRE judgement: backup concept is proven locally; production backup system is not present.

## Restore

Restore is partially proven.

Positive evidence:

- Restore into `opencashflow_restore` is automated in the smoke drill.
- Verification queries confirm representative smoke data.

Limitations:

- Restore is into a second database in the same disposable PostgreSQL container.
- No restore into a separate host/container/service.
- No restored application startup against the restored database is proven in the script.
- No production-like dataset.
- No restore timing measurement.
- No operator checklist for deciding restore point and communicating impact.

SRE judgement: encouraging local proof; not enough for a 3 AM production recovery guarantee.

## Migration

Migration readiness is a blocker.

Positive evidence:

- EF Core migrations are present in Infrastructure.
- `AUTO_MIGRATE` is configurable.
- Docs recommend `AUTO_MIGRATE=false` in production and explicit migration steps.

Critical gap:

- `Docs/ops/upgrade-migration-drill.md` explicitly states the upgrade/migration drill was not fully performed.
- No prior release database snapshot exists in the repo.
- No migration runbook with exact precheck, apply, verify and abort commands has been proven.
- No automated migration smoke against copied historical data.

SRE judgement: not production-ready.

## Rollback

Rollback is not proven.

Current documented rollback position is effectively “restore the pre-upgrade backup” for destructive migrations. That can be a valid strategy, but it must be drilled against realistic data and timed.

Missing:

- tested rollback procedure;
- data-loss decision tree;
- operator checklist;
- restore-point selection;
- customer communication plan;
- rollback validation script;
- failed-migration recovery procedure.

SRE judgement: rollback readiness is inadequate.

## Health Checks

Current state:

- API has `/health`.
- PostgreSQL container has `pg_isready`.
- CI and smoke scripts use API `/health`.
- WebApp reachability is checked by HTTP status in smoke workflows.

Gaps:

- API health appears minimal; no evidence from this audit of full dependency breakdown.
- WebApp has no dedicated `/health`.
- No readiness endpoint to block traffic before dependencies are ready.
- No liveness endpoint distinction.
- No health check auth/tenant/payment/cash dependency probe.

SRE judgement: good minimum for local smoke; insufficient for orchestrated production operations.

## Recovery And Disaster Recovery

Disaster recovery is documented as a need, not implemented.

There is no observable evidence of:

- DR environment;
- restore into alternate region/host;
- backup replication;
- DNS failover;
- runbook for database corruption;
- runbook for lost admin access beyond installation notes;
- incident commander checklist;
- communications template;
- post-incident review template.

SRE judgement: DR is not ready.

## Incident Response

Incident response is partial.

Positive evidence:

- `Docs/ops/secrets-management.md` includes steps for leaked secrets.
- Security docs warn not to publish exploitable details.
- Production hardening docs list signals to monitor.

Missing:

- severity levels;
- escalation paths;
- on-call ownership;
- contact list;
- incident timeline template;
- customer notification criteria;
- recovery decision matrix;
- audit evidence preservation procedure.

SRE judgement: no complete incident response system.

## Runbooks

Existing runbooks/docs:

- production hardening;
- secrets management;
- backup/restore drill;
- upgrade/migration drill plan;
- clean-install smoke;
- database recovery and connection guide.

Runbook quality:

- Good for maintainers and evaluators.
- Too incomplete for on-call production operators.
- Some docs are plans rather than executed procedures.
- No single “3 AM outage” runbook exists.

SRE judgement: a good start, not enough for reliable operations.

## Can Operations Sleep At Night?

Not for production.

They can sleep if OpenCashFlow is running as a disposable evaluation environment with no critical data. They cannot sleep if it holds financial data for many companies without additional operational engineering.

## If Production Goes Down At 3 AM

Could they recover?

- Maybe, if the failure is simple container restart, local database issue, or known health endpoint failure.
- Not confidently for data corruption, bad migration, secret compromise, database volume failure, or regional outage.

Could they restore?

- They have a local `pg_dump`/`pg_restore` pattern and smoke proof.
- They do not have a production backup system, off-host backups, restore timing, or RPO/RTO evidence.

Would they know what to do?

- For local smoke/evaluation: mostly yes.
- For production incident: not reliably. The repository lacks a complete incident runbook.

## Scores

Scale: 0 is absent; 10 is production-grade and proven.

| Area | Score | Rationale |
| --- | ---: | --- |
| Operations readiness | 4.0 | Local run paths and docs exist, but no production operating model, alerting, SLOs or on-call runbooks. |
| Recovery readiness | 4.0 | Local backup/restore smoke is proven; production backup/restore, RPO/RTO and DR are not proven. |
| Deployment maturity | 3.5 | Docker Compose works for evaluation, but root Compose is not a production deployment artifact. |
| Operational risk | 7.5 | High risk for business-critical use due to unproven migration, rollback, monitoring, DR and production restore. |

## Required SRE Work Before Production

1. Add production deployment reference with TLS reverse proxy, external PostgreSQL, secret injection and `AUTO_MIGRATE=false`.
2. Add API readiness/liveness checks and WebApp health endpoint.
3. Add structured centralized logging guidance and a working example.
4. Add metrics and alerting baseline: uptime, error rate, latency, DB connectivity, disk, backup freshness.
5. Implement scheduled backup automation with off-host storage and encryption.
6. Run restore drill into a separate environment and start the app against restored data.
7. Define and test RPO/RTO.
8. Execute upgrade/migration drill against a realistic previous-version database.
9. Execute rollback-by-restore drill and document timing.
10. Write a 3 AM incident runbook with severity, escalation, communication and recovery checklists.
11. Add DR plan for host/volume loss and database corruption.
12. Replace local-evaluation defaults in any production example.

## Final SRE Decision

OpenCashFlow can be operated as a developer-preview evaluation stack.

OpenCashFlow cannot yet be responsibly operated as a business-critical financial system without a dedicated SRE hardening phase.

I would not put customers or 120 companies on it until backup, restore, migration, rollback, monitoring and incident response are proven under production-like conditions.
