# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Messaging
- **Stack:** C#, .NET libraries, Azure SDKs, xUnit, GitHub Actions
- **Created:** 2026-03-13T07:41:24.960Z

## Learnings

- Tests use xUnit with AutoFixture AutoNSubstitute, Atc.Test, FluentAssertions, and NSubstitute.
- Publisher and processor changes should be covered with payload serialization, metadata mapping, and DI registration tests where applicable.
