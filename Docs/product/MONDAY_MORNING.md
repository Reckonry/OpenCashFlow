# Monday Morning

It is Monday.

07:30.

The workshop is still quiet.

Coffee on the desk.

You open OpenCashFlow before opening email.

You are not here to browse reports.

You are here to answer one question:

> What can we safely pay this week?

## The Company

Company:

> Northside Workshop

Business:

- small metal workshop;
- 18 employees;
- mostly B2B customers;
- buys steel and components before customers pay;
- uses an accountant for books;
- still plans cash in a spreadsheet.

Today is Monday, August 5.

## What You See First

The first screen is not a chart wall.

It is the Safe-to-Pay Radar.

```text
Safe-to-Pay Radar                         Monday, Aug 5, 07:30

Bank cash                 84,260
Safe cash                 18,340
Committed this week       71,900
Expected this week        52,000
Risk level                High

This week:
You can safely approve 3 of 7 payments.
2 payments should be delayed.
1 supplier should be called.
1 customer is creating payroll risk.
```

The number that matters is not `84,260`.

That is just the bank balance.

The number that matters is `18,340`.

That is safe cash after protecting payroll, VAT, rent, and commitments already approved.

The screen says:

> ACME late payment can break payroll reserve on Aug 12.

That is the moment you lean forward.

## What You Click First

You click:

> Review payment queue

OpenCashFlow shows the payments due this week.

```text
Payment Decision Queue

Payee                 Due      Amount     Recommendation
SteelCo               Aug 7    27,500     Split recommended
Rent                  Aug 8     6,000     Safe to pay
Payroll reserve       Aug 9    38,000     Protected
Machine purchase      Aug 9    32,000     Delay recommended
Tooling supplier      Aug 10    4,900     Safe to pay
Insurance             Aug 12    2,800     Safe to pay
Fasteners Ltd         Aug 12    3,700     Hold until ACME pays
```

You expected to pay SteelCo in full.

OpenCashFlow says not yet.

## What You Learn

You click SteelCo.

OpenCashFlow explains:

```text
SteelCo - 27,500 due Aug 7

If paid in full:
- Safe cash drops from 18,340 to -9,160
- Payroll reserve is breached
- Worst projected balance: -6,800 on Aug 12
- Risk depends on ACME paying this week

Recommended action:
Pay 12,000 now.
Pay 15,500 after ACME lands.
```

This is not an accounting report.

This is a decision.

You now understand that paying SteelCo in full is only safe if ACME pays on time.

ACME has not confirmed.

## The Customer That Worries You

You click the alert:

> ACME payment creates payroll risk.

OpenCashFlow shows:

```text
Customer: ACME Components
Amount expected: 52,000
Original due date: Aug 2
Days late: 3
Confidence: Risky
Last promise: "Should go out early next week"
Cash impact if 15 days late: Payroll reserve breach Aug 12
```

This customer worries you because:

- it is a large amount;
- they have slipped before;
- payroll is due Friday;
- SteelCo is also due this week;
- the machine purchase only works if ACME pays.

You do not need a CRM.

You need to know that ACME is the customer to call today.

## The Scenario You Run

You click:

> Scenario: ACME pays 15 days late

OpenCashFlow updates the forecast.

```text
Scenario: ACME pays Aug 20 instead of Aug 5

Projected low point: -6,800
Payroll reserve breached: Yes
Unsafe payments this week: SteelCo full payment, machine purchase

Fix options:
1. Split SteelCo: low point becomes 9,400
2. Delay machine purchase: low point becomes 18,300
3. Collect 10,000 deposit from Bright Machines: low point becomes 19,400
```

The decision is obvious now.

You do not need a bigger dashboard.

You need to not spend money that only looks available.

## The Decision You Make

You decide:

1. Pay rent.
2. Pay tooling supplier.
3. Pay insurance.
4. Protect payroll.
5. Split SteelCo.
6. Delay machine purchase.
7. Hold Fasteners Ltd until ACME pays.
8. Call ACME before 09:00.

You click:

> Approve safe payments

OpenCashFlow marks:

```text
Approved today:
- Rent: 6,000
- Tooling supplier: 4,900
- Insurance: 2,800
- SteelCo partial: 12,000

Delayed:
- Machine purchase: 32,000 delayed 30 days
- Fasteners Ltd: 3,700 held pending ACME
- SteelCo remaining: 15,500 after ACME receipt
```

## The Payment You Postpone

The machine purchase hurts.

It is a good machine.

The price is fair.

But OpenCashFlow shows:

```text
Machine purchase - 32,000

If approved this week:
- Safe cash drops below zero
- ACME delay creates a negative balance
- Payroll reserve depends on customer timing

Recommendation:
Delay until ACME is received or deposit collected.
```

You postpone it.

Not because the machine is bad.

Because this week is not the week.

## The Supplier You Call

You call SteelCo at 08:15.

You do not call vaguely.

You have a plan.

You say:

> We can send 12,000 today and the remaining 15,500 once ACME clears. Can you hold the next steel delivery if we confirm
> the second payment by Friday?

This is a better conversation than:

> We are tight this week.

OpenCashFlow gave you a specific ask.

## The Customer You Call

You call ACME at 08:45.

You are polite, but direct.

You say:

> Your 52,000 payment was due Friday. We need confirmation today because it affects this week's supplier schedule.

They say:

> It should go out Wednesday.

You update the expected date to Wednesday and mark confidence as medium, not confirmed.

OpenCashFlow updates the Radar.

```text
ACME expected: Aug 7
Confidence: Medium
Risk level: Medium
Safe cash after decisions: 14,900
Payroll reserve: Protected
```

You are not relaxed.

But you are no longer guessing.

## What Happens Next

At 09:10, you send the weekly cash summary to your accountant.

```text
Weekly cash summary

Cash now: 84,260
Safe cash after approved payments: 14,900
Protected payroll: 38,000
Protected VAT reserve: 21,000

Approved:
- Rent
- Tooling supplier
- Insurance
- SteelCo partial

Delayed:
- Machine purchase
- Fasteners Ltd
- SteelCo remaining

Watch:
- ACME 52,000 expected Wednesday, medium confidence

Question:
Can we safely reduce VAT reserve by 5,000 until Friday if ACME slips again?
```

The accountant has context before giving advice.

At 10:30, SteelCo agrees to the split.

At 11:15, ACME sends payment confirmation for Wednesday.

You do not approve the machine purchase yet.

You wait for cash to land.

## What OpenCashFlow Actually Did

OpenCashFlow did not run the company.

It did not replace accounting.

It did not manage inventory.

It did not become CRM.

It did one job:

> It stopped the owner from making a payment decision that looked safe but was not safe.

## Why This Monday Matters

Before OpenCashFlow:

- bank balance looked fine;
- spreadsheet was probably stale;
- SteelCo would have been paid in full;
- machine purchase might have been approved;
- ACME delay would have become a Friday problem;
- payroll reserve would have been at risk.

After OpenCashFlow:

- safe cash was visible;
- ACME risk was explicit;
- SteelCo got a concrete split proposal;
- machine purchase was delayed;
- payroll stayed protected;
- accountant saw the decision context;
- the owner started the week with control.

## Product Test

If OpenCashFlow can produce this Monday morning, it is the right product.

If it cannot, it is still too generic.

The product should be judged by whether it can make this sentence true:

> I opened OpenCashFlow before making payments, and it changed what I did.
