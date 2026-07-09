# OpenCashFlow Customer Adoption Audit

Date: 2026-07-08

Perspective: CTO of a manufacturing company looking for a self-hosted cash-flow management system.

Scope: first visit to the GitHub repository. Observable evidence only. This review intentionally ignores code style and focuses on trust, professionalism, documentation, roadmap, maintenance signals, risk and adoption confidence.

## Executive Customer Verdict

I would not migrate company financial data into OpenCashFlow today.

I would keep it on a shortlist for technical evaluation because the maintainers are unusually honest about project status, risks and production blockers. The repository reads like a serious Developer Preview, not like abandonware or a toy. It has a clear README, roadmap, security policy, contribution guide, changelog, Docker quickstart, tests, quality gates, smoke testing and backup/restore documentation.

However, as a manufacturing company CTO, I would not ask my CEO to approve operational use yet. The project explicitly says it is not production-ready. It has no stable release, no proven upgrade/migration drill, no production-ready deployment model, no support offering, no published SLA, and no evidence that production restore, security hardening or long-term maintenance have been proven with real customer data.

Decision: evaluate only with synthetic data. Do not adopt for live finance operations yet.

## First Customer Impression

The repository answers the basic customer questions quickly:

- what it is: self-hosted cash-flow management;
- who it is for: small companies, consultants, accounting studios and teams;
- what it is not: not a production-ready accounting suite, not fiscal/tax/payroll/invoicing certification, not SaaS/Stripe dependent;
- maturity: Developer Preview / Early Self-Hosted Preview;
- how to run: .NET 10 and Docker Compose quickstart;
- license: AGPL-3.0.

That clarity builds trust. The project does not oversell itself.

The same honesty also blocks adoption. A customer seeking a stable cash-flow system will immediately see repeated warnings that this should not be used for business-critical financial operations.

## Would I Trust These Developers?

Partially, for evaluation.

Trust-building evidence:

- README is explicit that the project is not production-ready.
- Roadmap lists hardening work rather than pretending it is done.
- Security docs describe dependency hygiene, headers, CSP, auth, tenant isolation and remaining security-sensitive areas.
- Contributing docs set boundaries and discourage random framework churn.
- Test backlog says skipped tests are currently zero in the compiled suite.
- Backup/restore docs distinguish local proof from production proof.
- Upgrade/migration docs explicitly say the drill is not fully performed.

Trust-reducing evidence:

- There is no stable release.
- There is no commercial support statement.
- There is no documented maintainer support model.
- There is no production reference deployment.
- Some docs still mention historical/migration-era concerns.
- Repository hygiene is not perfect; for example, `Docs/.DS_Store` is still present in the current checkout.

Customer trust score: moderate for engineering honesty, low for business dependency.

## Would I Believe This Project Will Still Exist In Three Years?

I cannot conclude that from the repository alone.

Positive signals:

- The roadmap is coherent and conservative.
- The project has open-source packaging: contributing guide, code of conduct, changelog, security policy and issue/PR templates.
- The architecture and documentation suggest sustained engineering effort.
- The project has moved to .NET 10 and has active hardening work documented.

Missing signals:

- no stable releases;
- no release cadence;
- no public governance model;
- no named maintainers/support commitments in the reviewed docs;
- no adoption/community evidence visible from the local repository;
- no long-term support policy;
- no public funding/support model beyond repository metadata.

Three-year continuity confidence: uncertain.

## Would I Migrate Company Data Into It?

No.

Reason:

- README explicitly says not to use it for regulated or business-critical financial operations.
- Production hardening docs list blockers before production.
- Backup/restore is proven only against a disposable local smoke stack.
- Upgrade/migration drill is explicitly not fully performed.
- There is no stable release or migration compatibility promise.
- Support and recovery obligations are not defined.

I would only load synthetic or anonymized sample data for evaluation.

## Would I Convince My CEO To Approve It?

Not for production.

I could justify a small engineering evaluation if the business wants a self-hosted cash-flow option and accepts that this is a preview. I would not present it to the CEO as a deployable finance system.

CEO-facing message:

> OpenCashFlow is promising and transparent, but it is not ready to hold our real financial data. We can evaluate it in a lab. We should not depend on it operationally until stable releases, support, backup/restore, upgrade and security evidence improve.

## Would I Pay For Support?

Not yet.

I might pay for a professional assessment, implementation spike or hardening engagement if the maintainers offered it. I would not pay recurring production support until:

- stable release exists;
- support terms and SLAs are published;
- upgrade policy is proven;
- production deployment guide is validated;
- backup/restore and incident response are production-ready;
- security review/pentest status is clear.

## Would I Recommend It To Another Company?

I would recommend it only as an open-source project to watch or evaluate.

I would not recommend it as an operational accounting/cash-flow system for another company’s real data.

## Trust

Strengths:

- Honest maturity statement.
- Clear self-hosted scope.
- Clear warning against production use.
- Good open-source docs.
- Visible testing and quality-gate culture.

Weaknesses:

- No stable release.
- No support model.
- No production readiness proof.
- No customer references or adoption signal in the repository.
- No demonstrated three-year maintenance guarantee.

Trust score: 5.5/10.

## Professionalism

The public presentation is professional for a Developer Preview:

- polished README;
- roadmap;
- security policy;
- contributing guide;
- changelog;
- code of conduct;
- quality gates;
- testing docs;
- operational docs.

Professionalism is reduced by:

- developer-preview caveats throughout;
- migration/hardening docs that show substantial work remains;
- a small repository hygiene issue (`Docs/.DS_Store`);
- no stable releases.

Professionalism score: 7/10 for a preview, 4/10 for a vendor-grade product.

## Documentation

Documentation is a strength.

Customer-useful docs:

- README explains project status and scope.
- `Docs/ROADMAP.md` is conservative and clear.
- `Docs/installation.md` exists.
- `Docs/ops/production-hardening.md` explains local defaults versus production.
- `Docs/ops/backup-restore-drill.md` documents a real local drill.
- `Docs/ops/upgrade-migration-drill.md` documents what is not yet proven.
- `SECURITY.md` explains security posture and reporting.

Documentation gap:

- no customer-oriented “Can I use this in production?” decision guide beyond warnings;
- no support/SLA document;
- no migration guide between releases because stable releases do not exist;
- no production reference architecture validated end-to-end.

Documentation score: 7/10.

## Roadmap

The roadmap is credible because it is conservative. It does not promise magic. It names hard work:

- database integration migration;
- production hardening;
- backup/restore drill;
- upgrade/migration drill;
- GitHub alert closeout;
- frontend/static asset cleanup;
- Infrastructure/Contracts boundary reduction.

Customer concern:

Most roadmap items are things I would want completed before production adoption. That means the roadmap is useful, but it also confirms the product is not ready for my company’s financial operations.

Roadmap score: 6/10.

## Signals Of Long-Term Maintenance

Positive:

- .NET 10 baseline;
- active security/dependency documentation;
- quality-gate workflow;
- ZAP workflows;
- tests and smoke scripts;
- open-source project files.

Negative or missing:

- no stable release;
- no release cadence;
- no LTS/support policy;
- no clear maintainer roster;
- no public support channel policy beyond general contribution/security docs;
- no evidence of commercial backing in the repository;
- no customer adoption references.

Maintenance confidence score: 4.5/10.

## Risk

Customer adoption risks:

1. Data migration risk: no stable release or proven upgrade path.
2. Recovery risk: local backup/restore proof exists, but production backup/restore is not proven.
3. Security risk: auth, tenant isolation, PIN/fast-login and reset password are still called out as stabilization areas.
4. Operational risk: production deployment docs are requirements, not a validated production deployment.
5. Support risk: no SLA or support model.
6. Continuity risk: no evidence from the repository that the project will be maintained for three years.
7. Compliance risk: README says it is not certified fiscal, tax, payroll or invoicing software.

Risk score: 7/10 high for production adoption, acceptable for lab evaluation.

## Confidence

Confidence as a codebase worth evaluating: moderate-high.

Confidence as a system to run a manufacturing company’s cash-flow data today: low.

Confidence score: 4/10 for adoption, 7/10 for evaluation.

## Customer Decision Matrix

| Question | Answer | Rationale |
| --- | --- | --- |
| Would I trust these developers? | Partially | They are honest and organized, but there is no stable/support evidence. |
| Would I believe this project will still exist in three years? | Unknown | Repository signals effort, but no governance, release cadence or support commitments. |
| Would I migrate company data into it? | No | Project says not production-ready; upgrade/recovery not proven. |
| Would I convince my CEO to approve it? | Only for evaluation | Not for live finance operations. |
| Would I pay for support? | Not yet | No support offering/SLA/stable release evidence. |
| Would I recommend it to another company? | Watch/evaluate only | Not production use. |

## What Would Change My Mind

Before customer adoption, I would need:

1. A stable release with clear release notes.
2. A production deployment guide validated end-to-end.
3. A proven upgrade/migration drill from a previous release.
4. A production-like backup/restore drill with RPO/RTO.
5. A security review or pentest summary.
6. Clear support/SLA options.
7. A maintainer and governance statement.
8. Dependency alert closure.
9. A customer-safe migration/import/export story.
10. Clear documentation for what happens if the project is abandoned.

## Scores

| Area | Score | Customer Interpretation |
| --- | ---: | --- |
| Trust | 5.5 | Honest and transparent, but not yet proven for business reliance. |
| Professionalism | 7.0 | Strong preview presentation; not vendor-grade maturity. |
| Documentation | 7.0 | Clear and extensive, with good warnings. |
| Roadmap | 6.0 | Credible but mostly lists blockers before production readiness. |
| Long-term maintenance signals | 4.5 | Good engineering signals, weak governance/support/release signals. |
| Risk | 7.0 | High for real financial data. |
| Confidence | 4.0 | Low confidence for adoption, higher for evaluation. |

## Final Customer Adoption Verdict

As a manufacturing company CTO, I would not adopt OpenCashFlow for live company cash-flow management today.

I would approve a sandbox evaluation by an internal technical team if we are looking for a self-hosted option and willing to participate early. I would not migrate real data, ask the CEO for production approval, pay for ongoing support, or recommend it to another company as a production system until the project reaches a stable release with proven operations, security and support commitments.

OpenCashFlow earns interest. It does not yet earn business trust.
