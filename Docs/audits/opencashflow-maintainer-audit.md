# OpenCashFlow Maintainer Audit

Date: 2026-07-08

Perspective: long-time maintainer of a successful .NET open-source project evaluating whether to contribute to
OpenCashFlow.

Scope: repository evidence only. This review focuses on repository, issues, roadmap, CI, tests, architecture,
documentation, project governance, and contribution experience. It does not judge code style.

## Executive Maintainer Verdict

I would star OpenCashFlow and consider a small first pull request.

I would not become a maintainer, sponsor it, or recommend that new contributors invest significant time until the project
has clearer governance, a release cadence, and fewer production-readiness blockers.

The repository has enough structure to be worth watching: clear README, AGPL license, contribution guide, code of
conduct, issue forms, PR template, quality gates, ZAP workflows, roadmap, tests, database integration-test foundation,
and operational documentation. It reads like a serious Developer Preview.

The repository is not yet a mature open-source contributor ecosystem. Observable gaps remain: no maintainer guide, no
decision-making process, no public release cadence, no stable version, no documented triage policy, no project board
evidence in the local repository, no contributor recognition policy, and some repository residue such as `Docs/.DS_Store`
visible in the tree.

## Observable Evidence Reviewed

Reviewed files and structure:

- `README.md`
- `CONTRIBUTING.md`
- `CODE_OF_CONDUCT.md`
- `CHANGELOG.md`
- `SECURITY.md`
- `LICENSE`
- `.github/ISSUE_TEMPLATE/*.yml`
- `.github/pull_request_template.md`
- `.github/workflows/quality-gates.yml`
- `.github/workflows/zap-baseline.yml`
- `.github/workflows/zap-full.yml`
- `.github/workflows/README.md`
- `Docs/ROADMAP.md`
- `Docs/quality/quality-gates-report.md`
- `Docs/testing/skipped-tests-backlog.md`
- `Docs/testing/db-test-triage.md`
- `Docs/testing/db-integration-tests.md`
- solution structure from `dotnet sln OpenCashFlow.sln list`
- project references from `*.csproj`
- repository status from `git status --short --branch`

Current observable worktree note:

- The current branch is `ops/backup-restore-smoke-drill`.
- Several audit reports are untracked in the current checkout.
- `Docs/.DS_Store` is still visible in the Docs tree.

Those do not invalidate the project, but they reduce first-impression polish for a maintainer.

## Repository

Strengths:

- The top-level README explains project purpose, status, scope, quickstart, architecture and license.
- The project is explicit about being a Developer Preview / Early Self-Hosted Preview.
- The solution structure is understandable:
  - `OpenCashFlow.API`
  - `OpenCashFlow.Application`
  - `OpenCashFlow.Contracts`
  - `OpenCashFlow.Domain`
  - `OpenCashFlow.Infrastructure`
  - `OpenCashFlow.WebApp`
  - multiple test projects.
- Legacy SaaS/Admin/Stripe runtime is described as removed from the active core.
- The repository has an AGPL-3.0 license, which is appropriate for a networked self-hosted application if that is the
  intended governance model.

Weaknesses:

- There is still visible cleanup residue (`Docs/.DS_Store`).
- There are many historical/migration/hardening documents. They are useful, but they make the project feel like it is
  still mid-transition.
- The README is honest, but a new contributor still has to navigate many docs to find the highest-value starting point.

Maintainer view: worth exploring, not yet frictionless.

## Issues

The local repository includes useful issue forms:

- bug report form with reproduction steps, environment and safety check;
- feature request form focused on user/operator problem, risks and area;
- config disables blank issues;
- security contact routes vulnerability reports away from public issues.

What is not observable locally:

- active issue count;
- stale issue ratio;
- maintainer response time;
- labels actually used in practice;
- project board or milestone discipline;
- good-first-issue inventory.

Maintainer view: the templates are good, but issue hygiene cannot be verified from the local repository alone.

## Roadmap

The roadmap is unusually honest. It clearly says the project is not production-ready and names concrete work:

- database integration test migration;
- production hardening;
- backup/restore drill;
- upgrade/migration drill;
- GitHub alert closeout;
- frontend/static asset cleanup;
- Infrastructure/Contracts boundary reduction.

Strength:

- The roadmap avoids false maturity claims.
- It names real engineering work rather than vague feature wishes.

Weakness:

- The roadmap is mostly a blocker list. It does not yet show release milestones, ownership, target dates or how
  contributors can claim work.
- There is no visible milestone policy for `0.x`, `1.0`, or stable release readiness.

Maintainer view: good direction, incomplete execution model.

## CI

Positive signals:

- `quality-gates.yml` runs on non-draft PRs to `main` and `development`.
- It restores, audits NuGet vulnerabilities, builds Release, runs Release tests, and validates Docker Compose config.
- Permissions are read-only.
- Legacy promotion workflows were removed according to `.github/workflows/README.md`.
- ZAP baseline and full scans exist for non-draft PRs to `main`.
- Workflow docs explain active and removed workflows.

Limitations:

- Formatting is documented as deferred; `dotnet format --verify-no-changes` is not a required gate.
- Stricter analyzers and warning-as-error are deferred.
- SBOM publishing is deferred.
- The vulnerability audit depends on GitHub-hosted access to NuGet advisory data; local docs note a sandboxed attempt did
  not complete in one earlier run.
- ZAP scans only target `main`, not `development`.

Maintainer view: conservative and sane for a preview. Not yet mature enough for a large contributor base without more
automated hygiene.

## Tests

Positive signals:

- The solution has multiple test projects:
  - Domain tests;
  - Application tests;
  - Database tests;
  - broader API/integration-style tests.
- `Docs/testing/skipped-tests-backlog.md` states the compiled suite has `0 skipped`.
- The skipped test backlog keeps historical context rather than silently hiding risk.
- `OpenCashFlow.Database.Tests` exists and uses PostgreSQL Testcontainers.
- Database integration docs distinguish real persistence tests from historical excluded DB drafts.

Risks:

- `tests/OpenCashFlow.Test/Tests/db/**/*.cs` are still excluded from compilation and documented as historical drafts.
- `Docs/testing/db-test-triage.md` lists 71 declared historical DB tests, 62 skip attributes, and multiple high-value
  persistence scenarios not fully migrated.
- The database integration foundation is useful but still small.
- The docs themselves warn that zero skipped tests in the compiled suite does not eliminate excluded DB-test debt.

Maintainer view: the test culture is improving and transparent, but there is still meaningful hidden-domain risk in
historical DB coverage.

## Architecture

Positive signals:

- The solution has explicit Clean Architecture-style projects.
- `Application` references `Domain`, not API/Infrastructure/Contracts.
- `Domain` is isolated.
- `WebApp` references `Contracts`, not `Infrastructure`.
- `Infrastructure` owns EF/persistence implementation.
- Contribution docs explicitly state architectural boundaries and warn against reintroducing `OpenCashFlow.Shared`.

Concerns:

- `Infrastructure` still references `Contracts`; docs identify Infrastructure/Contracts boundary reduction as remaining
  work.
- `API` references `Contracts`, `Application`, and `Infrastructure`, which is expected for composition but keeps API as a
  broad integration surface.
- Many migration documents imply significant architecture cleanup has happened recently and some debt remains.

Maintainer view: architecture is understandable and moving in the right direction. It is not yet boring or fully settled.

## Documentation

Strong areas:

- README is clear and honest.
- Contribution guide is practical.
- Roadmap is conservative.
- Security documentation exists.
- Quality gate report explains CI choices.
- Testing docs explain skipped/excluded test state.
- Ops docs exist for production hardening, backup/restore, upgrade/migration and secrets.

Weak areas:

- Documentation volume is high and can feel like an audit archive rather than a curated contributor path.
- Some documents still read like migration-phase records.
- There is no concise maintainer guide.
- There is no "architecture decision index" for contributors to quickly find current policy versus historical notes.

Maintainer view: much better than most early projects, but curation is the next bottleneck.

## Project Governance

Observable strengths:

- AGPL license is explicit.
- Code of Conduct exists.
- Security reporting route exists.
- Funding file exists.
- PR and issue templates exist.

Observable gaps:

- No maintainer roster.
- No decision-making model.
- No release manager/triage ownership.
- No documented review SLA or expectations.
- No governance model for breaking changes.
- No contributor recognition path.
- No documented process for becoming a maintainer.
- No public support policy beyond general docs.

Maintainer view: governance is the largest blocker to deeper contribution.

## Contribution Experience

What would help me open a first PR:

- Clear local build/test commands.
- Conservative contribution rules.
- PR template asks for risk and checks.
- Issue forms request useful reproduction detail.
- Good small areas are suggested in `CONTRIBUTING.md`.
- The codebase is split into recognizable layers.

What would slow me down:

- Many docs to read before knowing what is current.
- Production-readiness blockers are mixed with architecture and migration history.
- No visible list of beginner-ready issues in the local repository.
- No maintainer guide explaining review standards or ownership.
- Some remaining repository residue weakens confidence in hygiene.

Maintainer view: I would open a small PR, but I would not yet invest in a large feature.

## Would I Star It?

Yes.

Reason: the project is honest, structured, self-hosted, AGPL-licensed, and has credible early engineering discipline.
Starring is appropriate as a signal to watch the project.

## Would I Fork It?

Maybe.

I would fork it to experiment or prepare a focused PR. I would not fork it to build a production derivative yet because
release stability and operational maturity are not proven.

## Would I Open A PR?

Yes, for a small bounded change.

Good first PR candidates:

- remove repository residue;
- improve docs curation;
- migrate one historical DB test group into `OpenCashFlow.Database.Tests`;
- add a missing integration test;
- reduce Infrastructure/Contracts coupling in a narrow area;
- clean frontend static assets where behavior can be preserved.

I would avoid large features until governance and review expectations are clearer.

## Would I Become A Maintainer?

No, not from current evidence.

Reason: the project does not yet publish a maintainer process, ownership model, review expectations, release discipline or
governance. Becoming a maintainer without those would carry high process risk.

## Would I Sponsor It?

Not yet.

Reason: funding links exist, but the repository does not show a support roadmap, funding goals, maintainer commitments or
clear use of sponsorship funds. I might sponsor after seeing consistent releases and issue/PR responsiveness.

## Would I Recommend Contributors Join?

Qualified yes.

I would recommend experienced .NET contributors join if they are comfortable with early-preview cleanup, tests,
documentation, security hardening and architecture work.

I would not recommend it yet to new contributors expecting a highly curated onboarding path or fast maintainer feedback.

## Scores

| Area | Score | Maintainer Interpretation |
| --- | ---: | --- |
| Repository | 7.0 | Clear structure and purpose, with minor hygiene residue. |
| Issues | 6.0 | Good templates; live issue health not observable locally. |
| Roadmap | 6.5 | Honest and concrete, but lacks milestones and ownership. |
| CI | 7.0 | Conservative quality gates, read-only permissions, no deployment automation. |
| Tests | 6.5 | Compiled suite has zero skips and DB foundation exists; historical DB tests remain excluded. |
| Architecture | 7.0 | Good layer split; some boundary debt remains documented. |
| Documentation | 7.0 | Extensive and honest, but needs curation for contributors. |
| Governance | 4.0 | Code of Conduct and templates exist; maintainer/release process missing. |
| Contribution Experience | 6.0 | Good for focused PRs; not yet frictionless for sustained contribution. |
| Overall Contributor Attractiveness | 6.5 | Worth watching and contributing small fixes; not yet a mature maintainer ecosystem. |

## Maintainer Risk Register

1. Governance risk: no visible maintainer process or decision model.
2. Release risk: no stable release or cadence.
3. Test risk: historical DB tests remain excluded, even though documented.
4. Scope risk: many production-readiness blockers remain open.
5. Documentation sprawl: strong docs exist, but contributors need clearer "current truth" paths.
6. Hygiene risk: visible residue such as `Docs/.DS_Store` should be removed.
7. Architecture debt: Infrastructure/Contracts coupling remains on roadmap.

## Final Maintainer Verdict

OpenCashFlow is a credible early open-source project for experienced contributors who like stabilization work. It has
enough discipline to justify a star and a small PR. It does not yet have enough governance, release maturity or
maintainer process clarity to justify becoming a maintainer, sponsoring it, or recommending broad contributor adoption.

The next best step is not a new feature. It is contributor-operability work: maintainer guide, issue triage policy,
release milestone plan, curated current-doc index, DB test migration, and removal of final repository residue.
