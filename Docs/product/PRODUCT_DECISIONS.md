# Product Decisions

This document states what OpenCashFlow will not become.

The goal is to protect the product from becoming a generic ERP.

## Decision 1: No Full ERP

OpenCashFlow will not become a full ERP.

Reason:

ERP scope is too broad and already served by mature competitors. OpenCashFlow wins by being focused on operational cash
decisions.

Implication:

ERP-like modules must prove they directly strengthen cash visibility or remain outside the core.

## Decision 2: No Payroll

OpenCashFlow will not run payroll.

Reason:

Payroll is compliance-heavy, region-specific, and high-risk.

Implication:

OpenCashFlow may track payroll commitments and cash impact, but payroll calculation and filing belong elsewhere.

## Decision 3: No Accounting Replacement

OpenCashFlow will not replace accounting software.

Reason:

Accounting requires compliance, reconciliation, tax rules, reporting standards, and professional review.

Implication:

OpenCashFlow imports from, exports to, and collaborates with accounting systems.

## Decision 4: No CRM

OpenCashFlow will not become a CRM.

Reason:

CRM optimizes sales pipelines and customer relationships. OpenCashFlow optimizes cash decisions.

Implication:

Customer records exist only as needed for receivables, payment risk, and cash context.

## Decision 5: No Inventory ERP

OpenCashFlow will not become an inventory ERP.

Reason:

Inventory requires item masters, warehouses, stock movements, costing, purchasing, MRP, and operational complexity.

Implication:

OpenCashFlow may track cash commitments for inventory purchases, but not stock operations.

## Decision 6: No Ecommerce Platform

OpenCashFlow will not become ecommerce.

Reason:

Ecommerce is a separate product category.

Implication:

OpenCashFlow may import orders, invoices, payouts, and expected cash from ecommerce systems.

## Decision 7: No HR Suite

OpenCashFlow will not become HR software.

Reason:

HR includes hiring, time off, payroll, compliance, performance, documents, and employee lifecycle management.

Implication:

OpenCashFlow may track payroll cash commitments and user permissions, not HR operations.

## Decision 8: No Tax Compliance Engine

OpenCashFlow will not calculate or file taxes as a compliance engine.

Reason:

Tax rules vary by country, change often, and require specialist accountability.

Implication:

OpenCashFlow may reserve cash for taxes, track tax due dates, and import estimated tax obligations.

## Decision 9: No Marketplace Until The Core Habit Works

OpenCashFlow will not launch a broad plugin marketplace early.

Reason:

Marketplaces amplify a product that already has demand. They do not create demand.

Implication:

Start with official modules and templates. Add community marketplace later.

## Decision 10: Cash Cockpit First

Every product decision should reinforce:

> weekly operational cash decisions.

If a feature does not support that, it should wait.

## Decision 11: Cash Custody Before Safe-To-Pay

OpenCashFlow will not present forecast or Safe-to-Pay recommendations as reliable until Cash Custody is implemented and
Cash Integrity can prove it.

Reason:

Safe-to-Pay depends on trusted cash. Trusted cash requires a custody chain: source, custodian, reason, actor, audit,
immutable posted facts, transfer links, reversal/correction, reconciliation, visible discrepancies, and confidence
status.

Implication:

Forecast and Safe-to-Pay may be designed and prototyped, but must remain WIP/Experimental until the Cash Custody decision
record and Cash Integrity proof requirements are implemented and covered by tests.
