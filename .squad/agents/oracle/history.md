# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Messaging
- **Stack:** C#, .NET libraries, Azure SDKs, xUnit, GitHub Actions
- **Created:** 2026-03-13T07:41:24.960Z

## Learnings

- The repository includes backend-specific samples under `samples/` and a root `README.md` that explains package usage.
- When public behavior changes, docs and samples should be updated alongside code and tests.
- EventHub test customizations should prefer `SpecimenRequestHelper.GetRequestType()` for request-type filtering and `TypeCustomization<T>` base class for single-type builders (centralizes request-matching logic, simplifies intent).
- `FluentArg.Match<T>()` inline matchers are appropriate only when extracting and asserting a single argument with no reuse; multi-argument or multi-assertion cases stay clearer with `ReceivedArg<T>()`.
- Batch-style loops using `ReceivedArgs<T>()` should be preserved to maintain clear iteration intent; do not convert to inline matchers.

## Work Log

### 2026-03-13: Cabazure.Test documentation migration
- Updated `.github/copilot-instructions.md` to reference `Cabazure.Test` instead of `Atc.Test` in the Test conventions section
- Committed focused change on `feature/migrate-cabazure-test` branch
- This aligns documentation with the test framework migration across the project
- Session completed; migration approved; branch ready for user review

### 2026-03-13: Cabazure.Test MIGRATING.md Documentation Attempt
- Drafted "Tips from Migration Experience" section for `Cabazure.Test/MIGRATING.md` with practical guidance from the Cabazure.Messaging migration
- Tank review identified API accuracy issues: FluentArg.Match<T> example used incorrect predicate syntax (should be assertion action); TypeCustomization<T> example used incorrect override pattern (should be constructor/factory)
- Scope issue flagged: section was too EventHub-specific for a general migration guide
- Oracle locked out of revision cycle per Tank decision; Trinity handled revision and delivered approved version
- Status: ✅ COMPLETE (approved version delivered by Trinity)
