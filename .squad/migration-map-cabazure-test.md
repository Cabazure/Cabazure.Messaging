# Cabazure.Messaging → Cabazure.Test Migration Map

**Status:** Cabazure.Messaging is already on Cabazure.Test 1.0.1. Migration from Atc.Test → Cabazure.Test is complete. This map documents the changes already applied and serves as a reference for future test work.

---

## Summary

All four test projects have transitioned from `Atc.Test` to `Cabazure.Test 1.0.1`:
- `Cabazure.Messaging.Abstractions.Tests` (empty, no changes)
- `Cabazure.Messaging.EventHub.Tests`
- `Cabazure.Messaging.ServiceBus.Tests`
- `Cabazure.Messaging.StorageQueue.Tests`

### Changes Applied

#### 1. **Package Reference Updates**
All test `.csproj` files now reference:
```xml
<PackageReference Include="Cabazure.Test" Version="1.0.1" />
```
(Previously: `Atc.Test` v2.x)

#### 2. **Global Using Statements**
All test `.csproj` files now declare:
```xml
<Using Include="Cabazure.Test" />
```
(Previously: `Atc.Test`)

#### 3. **Built-in Customizations Removed**
✅ **No `JsonElementCustomization.cs` files found** — they have been deleted. The fixture factory now applies JSON element customization automatically as a built-in default.

#### 4. **Test Initializers Converted**
- **EventHub.Tests** and **StorageQueue.Tests** use `[ModuleInitializer]` pattern:
  ```csharp
  internal static class TestInitializer
  {
      [ModuleInitializer]
      public static void Initialize()
      {
          FixtureFactory.Customizations.Add(new BlobClientOptionsGenerator());
          FixtureFactory.Customizations.Add(new EventHubModelsGenerator());
          FixtureFactory.Customizations.Add(_ => new EventHubsRetryOptions());
      }
  }
  ```
- **ServiceBus.Tests** has no explicit initializer (no backend-specific model generators needed).

#### 5. **API Method Renames — APPLIED**
| Atc.Test | Cabazure.Test | Usage Found | Status |
|----------|---------------|-------------|--------|
| `WaitForCall(...)` | `WaitForReceived(...)` | Not in codebase | — |
| `WaitForCallForAnyArgs(...)` | `WaitForReceivedWithAnyArgs(...)` | ✅ EventHub.Tests | Applied |
| `ReceivedCallWithArgument<T>()` | `ReceivedArg<T>()` | ✅ All backends | Applied |
| `CompareJsonElementUsingJson()` | `UsingJsonElementComparison()` | Not in codebase | — |
| `InvokeProtectedMethod(...)` | `InvokeProtected(...) ` (sync) | ✅ EventHub.Tests | Applied |
| `InvokeProtectedMethod(...) ` | `InvokeProtectedAsync(...)` (async) | Not used | — |

#### 6. **Task Timeout Pattern — NOT MIGRATED YET**
**Status:** No uses of `task.AddTimeout()` found in test code.
- If discovered in the future, replace with BCL method:
  ```csharp
  // Old
  await someTask.AddTimeout();
  
  // New
  await someTask.WaitAsync(TimeSpan.FromSeconds(5));
  ```

#### 7. **Empty Object Equivalency — NOT APPLIED YET**
**Status:** No uses of `HasProperties()` guard pattern detected in test code.
- Current code uses `.BeEquivalentTo(...)` on all types without guards.
- If needed in the future (for types with no public properties):
  ```csharp
  // Per-assertion (local)
  result.Should().BeEquivalentTo<object>(expected, o => o.AllowingEmptyObjects());
  
  // Global (recommended if common across tests)
  [ModuleInitializer]
  public static void Initialize()
      => AssertionOptions.AssertEquivalencyUsing(o => o.AllowingEmptyObjects());
  ```

---

## DI Test Helpers — Keep Local

All three backend test projects (EventHub, ServiceBus, StorageQueue) have identical **`FluentAssertionsExtensions.cs`** files with service descriptor assertion helpers:
```csharp
public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> 
    Contain<TService, TImplementation>(this GenericCollectionAssertions<ServiceDescriptor> collection)
public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> 
    Contain<TService, TImplementation>(this GenericCollectionAssertions<ServiceDescriptor> collection, ServiceLifetime lifetime)
public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> 
    Contain<TService>(this GenericCollectionAssertions<ServiceDescriptor> collection, ServiceLifetime lifetime)
```

**Decision:** These remain **backend-local** (not moved to Cabazure.Test) because they are specific to validating DI container behavior for each messaging backend. No changes needed.

---

## Current Test Stack

- **xUnit v3** (with `[Theory, AutoNSubstituteData]` attribute)
- **Cabazure.Test** 1.0.1 (with `AutoNSubstituteData`, `[ModuleInitializer]`, Frozen, Substitute patterns)
- **AutoFixture** with **AutoNSubstitute**
- **FluentAssertions** (no custom equivalency config applied globally yet)
- **NSubstitute** (with `ReceivedArg<T>()`, `WaitForReceivedWithAnyArgs(...)`)

---

## Search/Replace Patterns for Future Maintenance

| Pattern | Replace With | Notes |
|---------|--------------|-------|
| `\.AddTimeout\(\)` | `.WaitAsync(TimeSpan.FromSeconds(5))` | Not yet in use |
| `\.WaitForCall\(` | `.WaitForReceived(` | Not yet in use |
| `\.WaitForCallForAnyArgs\(` | `.WaitForReceivedWithAnyArgs(` | Already applied |
| `\.ReceivedCallWithArgument<` | `.ReceivedArg<` | Already applied |
| `\.InvokeProtectedMethod\(` | `.InvokeProtected(` | Already applied (sync) |
| `\.CompareJsonElementUsingJson\(\)` | `.UsingJsonElementComparison()` | Not yet in use |
| `if (obj.HasProperties\(\))` | Remove guard; add `o => o.AllowingEmptyObjects()` | Not yet in use |

---

## Validation Checklist

- [x] All `.csproj` files reference Cabazure.Test 1.0.1
- [x] All `<Using Include="Cabazure.Test" />`
- [x] No `JsonElementCustomization.cs` files (deleted)
- [x] All custom `[AutoRegister]` converted to `[ModuleInitializer]` pattern
- [x] API renames applied: `ReceivedArg<T>()`, `WaitForReceivedWithAnyArgs()`, `InvokeProtected()`
- [x] No `AddTimeout()`, `HasProperties()`, or old NSubstitute methods in codebase
- [x] FluentAssertionsExtensions.cs kept local per backend (intentional)
- [x] Build and tests pass post-migration

---

## Next Steps (If Needed)

1. **Add global `AllowingEmptyObjects()`** if future tests encounter empty object comparisons.
2. **Monitor for `AddTimeout()` usage** in new tests; migrate to `WaitAsync()` immediately.
3. **DI test helpers** — consider consolidating to Abstractions if pattern becomes duplicative across backends.

