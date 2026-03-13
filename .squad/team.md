# Squad Team

> Azure messaging libraries for .NET with shared publisher/processor abstractions across Event Hubs, Service Bus, and Storage Queues.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Morpheus | Lead Architect | `.squad/agents/morpheus/charter.md` | ✅ Active |
| Trinity | Backend Library Engineer | `.squad/agents/trinity/charter.md` | ✅ Active |
| Link | Azure Messaging Engineer | `.squad/agents/link/charter.md` | ✅ Active |
| Tank | Tester & Reviewer | `.squad/agents/tank/charter.md` | ✅ Active |
| Oracle | Docs & Samples Engineer | `.squad/agents/oracle/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Clear bug fixes in one backend package
- Missing or broken unit tests
- Small builder or registration updates following existing patterns
- README or sample fixes with explicit acceptance criteria
- Dependency version bumps and mechanical package maintenance

**🟡 Needs review — route to @copilot but require squad review:**
- Medium refactors within one package
- New sample wiring that follows an established backend pattern
- Expanding existing publisher or processor options with tests

**🔴 Not suitable — keep with squad members:**
- Public API boundary changes across packages
- Cross-backend abstraction design
- Security-sensitive changes around credentials or message handling
- Architecture or package-ownership decisions
- Changes that span code, tests, docs, and samples with trade-off decisions

## Project Context

- **Owner:** Ricky Kaare Engelharth
- **Stack:** C#, .NET libraries, Azure SDKs for Event Hubs/Service Bus/Storage Queues, xUnit, GitHub Actions
- **Description:** Transport-agnostic messaging abstractions plus backend-specific Azure integrations for publishing and processing messages
- **Project:** Cabazure.Messaging
- **Created:** 2026-03-13
