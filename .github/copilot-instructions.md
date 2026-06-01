# Copilot Instructions

## Azure Guidelines
- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool, ask the user to enable it.

## Analysis Mode
- When handling tasks that require analysis only, do not apply changes, modify files, generate migrations, or commits.
- Maintain protection for areas related to Sales, Orders, Billing, Inventory, Kardex, Purchases, and Accounting. For security changes, apply a minimum patch: do not touch operational modules without massive refactoring and ensure final compilation.
- For architectural phases, operate strictly in design mode: do not modify code or files, do not generate patches, only target the model. Define models, matrices, flows, and transitions to freeze design without implementation.
