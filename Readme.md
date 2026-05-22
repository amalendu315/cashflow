##CASHFLOW WEBSITE

## Overview

A centralized, multi-tenant web application designed to streamline internal payment requests and cashflow tracking. The system replaces unstructured communication (phone/WhatsApp) with an auditable, role-based ledger where subsidiary teams can request payments, and the parent company's accounts department can review, approve, split, or reject them based on predefined ledgers and vendor lists.

## Goals

1. Eliminate Unstructured Requests: Move 100% of internal payment requests from WhatsApp/phone calls to a traceable web platform.
2. Clean Data Standardization: Enforce the use of predefined Vendor and Company Masters to prevent data entry errors and enable accurate reporting.
3. Streamline Cashflow Visibility: Provide the accounts team with real-time visibility into daily Opening Balances, Inflows, Outflows, and Closing Balances across various ledgers.

## Core User Flow

1. System Setup: Parent Company Admin logs in and populates the Company Master, Ledger Master, and Vendor Master.
2. Cashflow Initialization: Admin manually enters the Opening Balance and any direct Inflows for the day into a specific Ledger (e.g., HDFC Bank).
3. Payment Request: A Subsidiary Team User logs in, selects a predefined Vendor (e.g., SpiceJet), enters an amount (e.g., ₹4,00,000), and submits a payment request.
4. Review & Action: Admin reviews the pending request and chooses a Ledger to fund it. Admin then selects: Approve (Full), Split (Partial payment today, rest scheduled), or Reject.
5. Cashflow Update: Approved amounts automatically log as an Outflow. The system automatically calculates: Opening Balance + Inflow - Outflow = Closing Balance.

## Features

### Master Management (Admin)

- Company Master: Define the different subsidiary teams or departments.
- Ledger Master: Define the various bank accounts or cash pools available for payments.
- Vendor Master: Predefine the approved list of payees to ensure strict data consistency.

### Cashflow & Request Management

- Manual Inflow/Opening Tracking: Interfaces for admins to log starting balances and incoming client funds.
- Split Payment Engine: Logic to partially approve a requested amount and automatically generate a rescheduled, pending request for the remainder.
- Automated Closing Calculations: Real-time mathematical updates to ledger balances based on approved outflows.

### Access Control

- Role-Based Dashboards: Subsidiary users only see their own department's requests. Parent Company Admins have global visibility and approval rights.


## Scope

### In Scope

- Web-based application with responsive UI for desktop and mobile access.
- Relational database schema to handle connected Masters (Companies, Ledgers, Vendors).
- Status tracking for payment requests (Pending, Approved, Split, Rejected).

### Out of Scope

- Direct API integrations with actual, physical bank accounts (all balances are manually logged/calculated).
- Automated invoice scanning or OCR (Optical Character Recognition).
- Payroll processing or employee salary management.

## Success Criteria

1. An Admin can successfully create entries in the Company, Ledger, and Vendor Master tables.
2. A Team User can log in and successfully submit a payment request that is restricted to predefined vendors.
3. An Admin can execute a "Split" action on a request, which correctly updates the day's Outflow and automatically generates a new pending request for the balance.
4. The system accurately and consistently calculates the Closing Balance without manual math errors.
