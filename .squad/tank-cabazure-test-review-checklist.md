# Cabazure.Messaging Test Migration Review Checklist

**Task:** Migrate unit tests in Cabazure.Messaging to new Cabazure.Test library  
**Status:** Pre-migration reconnaissance  
**Migration Guide Location:** Not yet found (may need to be created from sibling Cabazure.Test repo)

---

## Current Test Landscape

### Test Projects (3 active, 1 empty)
1. **Cabazure.Messaging.EventHub.Tests** — 20 test files covering publishers, processors, DI, metadata, scenarios
2. **Cabazure.Messaging.ServiceBus.Tests** — 13 test files (similar structure)
3. **Cabazure.Messaging.StorageQueue.Tests** — 11 test files (similar structure)
4. **Cabazure.Messaging.Abstractions.Tests** — Empty (no test files, only .csproj structure)

### Current Test Stack (Uniform across all projects)
```
xUnit v2.9.3
AutoFixture.Xunit2 + AutoNSubstitute
Atc.Test v1.1.18
FluentAssertions (latest)
NSubstitute (with ReceivedExtensions, ExceptionExtensions, ReturnsExtensions)
coverlet.collector v6.0.4
```

---

## Expected Migration Changes

### A. Package Reference Changes (in .csproj)

**Remove:**
- Individual explicit package refs (AutoFixture, AutoFixture.AutoNSubstitute, FluentAssertions, NSubstitute, xunit, Atc.Test versions)

**Add:**
- Single reference to `Cabazure.Test` (version TBD)
  - Must bring transitive dependencies: xUnit, AutoFixture, NSubstitute, FluentAssertions, Atc.Test

**Note:** All test projects already specify global usings for these packages, so no .cs changes needed unless Cabazure.Test renames or removes a namespace.

---

## B. Likely API/Helper Replacements

### Attribute Level
- **[Theory, AutoNSubstituteData]** ← Most common pattern (40+ usages)
  - May become `[Theory, CabazureAutoData]` or `[Theory, CabazureTestAutoData]`
  - **Critical:** Check if parameterless variant exists for [Fact] tests (none currently in codebase)

- **[InlineAutoNSubstituteData(param)]** ← Used in 2 processor service tests (EventHub, ServiceBus)
  - Ensure replacement supports inline parameters

### Custom Assertions (Backend-Specific)
All three backends have **FluentAssertionsExtensions.cs** with two helper methods:
```csharp
.Contain<TService, TImplementation>()
.Contain<TService, TImplementation>(ServiceLifetime lifetime)
.Contain<TService>(ServiceLifetime lifetime)
```
These are thin wrappers around ServiceDescriptor assertions for DI registration tests.

**Action:** Determine if Cabazure.Test provides these helpers or if they stay local to each backend.

### No Breaking Custom Helpers Found
- No custom [Frozen] or [Substitute] usage beyond Atc.Test conventions
- No custom test base classes or exotic assertions

---

## C. Risky Assertion/Helper Differences

### High-Risk Patterns (verify preservation)

1. **Metadata Mapping Tests** (3 tests, one per backend)
   - ServiceBusMetadataTests: Uses Azure SDK's `ServiceBusModelFactory.ServiceBusReceivedMessage()` to construct test messages
   - StorageQueueMetadataTests: Uses direct QueueMessage instantiation
   - EventHubMetadataTests: Uses EventData directly
   - **Risk:** If Cabazure.Test strips or modifies auto-mocking behavior, these factories may fail
   - **Verify:** AutoFixture still auto-generates Azure SDK types (BinaryData, IDictionary<string, object>, DateTimeOffset, etc.)

2. **NoAutoProperties Frozen Pattern** (EventHubPublisherFactoryTests and others)
   ```csharp
   [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions
   ```
   - Used to prevent auto-population of JsonSerializerOptions fields
   - **Risk:** If Cabazure.Test changes fixture conventions, this may behave unexpectedly
   - **Verify:** [NoAutoProperties] is still honored in Cabazure.Test

3. **Substitution of Factories and Providers** (Common in Internal tests)
   ```csharp
   [Frozen, Substitute] EventHubProducerClient client,
   [Frozen, Substitute] Func<object, string> partitionKeySelector,
   ```
   - Heavy use of NSubstitute mocking for Azure SDK clients and internal Func parameters
   - **Risk:** NSubstitute version mismatch or registration order changes
   - **Verify:** Frozen substitutes still work with dependency ordering

4. **Scenario Tests (Integration-Style)** (SingleConnectionsScenarioTests, MultipleConnectionsScenarioTests)
   - These use *real* ServiceCollection and DI container construction
   - Reflect over internal fields to validate behavior (e.g., `GetField("<producer>P", ...)`)
   - **Risk:** If Cabazure.Test changes how fixture setup works, reflection-based tests may break
   - **Verify:** Real DI container setup still works; no breaking changes to ServiceCollection handling

5. **CancellationToken Parameters** (Used in all async publisher/processor tests)
   - Standard xUnit async pattern, should be safe
   - **Verify:** CancellationToken still auto-generates without special handling

6. **BeEquivalentTo() Assertions** (Metadata tests)
   - FluentAssertions deep equality checks on complex Azure SDK types
   - **Risk:** If AutoFixture changes how it generates Azure types, equivalence may fail due to property defaults
   - **Verify:** Generated Azure SDK types still match expectations

---

## D. Files & Patterns Trinity Must Not Miss

### By Backend

#### EventHub Tests (20 files)
```
DependencyInjection/
  ├─ ServiceCollectionExtensionsTests.cs    [3 scenarios: no name, with name, with builder]
  ├─ EventHubBuilderTests.cs                [Publisher/Processor registration flows]
  ├─ EventHubPublisherBuilderTests.cs       [Partition key selector, JSON options]
  └─ EventHubProcessorBuilderTests.cs       [Processor factory, error handler registration]

Internal/
  ├─ EventHubPublisherTests.cs              [SendAsync calls, metadata mapping, partition routing]
  ├─ EventHubPublisherFactoryTests.cs       [Factory.Create<T>() with registration lookup]
  ├─ EventHubProducerProviderTests.cs       [Client caching by connection name]
  ├─ EventHubProcessorTests.cs              [ProcessAsync, deserialization, error handling]
  ├─ EventHubProcessorServiceTests.cs       [Hosted service lifecycle, [InlineAutoNSubstituteData]]
  ├─ EventHubBatchHandlerTests.cs           [Batch processing, processor error handlers]
  ├─ EventHubConsumerClientFactoryTests.cs  [Factory registration lookup]
  ├─ EventHubStatelessProcessorTests.cs     [Stateless variant without checkpointing]
  └─ BlobStorageProviderTests.cs            [Checkpoint storage setup]

Scenarios/
  ├─ SingleConnectionsScenarioTests.cs      [Reflection-based DI validation]
  └─ MultipleConnectionsScenarioTests.cs    [Multiple Event Hub instances per app]

Other/
  ├─ EventHubMetadataTests.cs               [EventData → EventHubMetadata mapping]
  ├─ CabazureEventHubOptionsTests.cs        [Options validation]
  └─ FluentAssertionsExtensions.cs          [ServiceDescriptor custom assertions]
```

**Critical Files for Trinity:**
- EventHubPublisherTests.cs — payload serialization + metadata + partition key mapping
- EventHubProcessorTests.cs — deserialization + error handling
- ServiceCollectionExtensionsTests.cs — DI registration correctness
- EventHubProcessorServiceTests.cs — [InlineAutoNSubstituteData] usage; hosted service lifecycle

#### ServiceBus Tests (13 files)
Similar structure; key files:
- ServiceBusMetadataTests.cs — uses ServiceBusModelFactory (most complex metadata construction)
- ServiceBusPublisherTests.cs — SendAsync behavior
- ServiceBusProcessorServiceTests.cs — [InlineAutoNSubstituteData] usage
- FluentAssertionsExtensions.cs — duplicates EventHub helpers

#### StorageQueue Tests (11 files)
Similar structure; simpler metadata (no ModelFactory needed).

#### Abstractions Tests
Empty placeholder; no migration needed yet.

---

## E. Specific Assertions to Validate Post-Migration

### 1. AutoNSubstituteData Compatibility
- [ ] All [Theory, AutoNSubstituteData] tests compile and run
- [ ] [InlineAutoNSubstituteData(param)] still works with inline args
- [ ] [Frozen] parameter capture works
- [ ] [Substitute] produces NSubstitute mocks, not null

### 2. Azure SDK Type Auto-Generation
- [ ] EventData, BinaryData, IDictionary<>, DateTimeOffset, etc. auto-generate
- [ ] ServiceBusModelFactory still produces valid test instances
- [ ] QueueMessage still auto-generates (no factory)
- [ ] NoAutoProperties stops auto-population where specified

### 3. Substitution & NSubstitute Integration
- [ ] Frozen substitutes resolve before non-frozen
- [ ] .Received(n) still captures calls
- [ ] .Returns*() methods still work
- [ ] Arg.Is<T>() still matches predicates

### 4. FluentAssertions & Should() Extensions
- [ ] .Should().NotBeNull() works
- [ ] .Should().BeEquivalentTo() deep-compares Azure types
- [ ] .Should().Throw<T>().WithMessage() pattern intact
- [ ] Custom .Contain<TService, TImplementation>() still available (either migrated or kept local)

### 5. Reflection-Based Scenario Tests
- [ ] ServiceCollection.BuildServiceProvider() works
- [ ] GetField<T>(BindingFlags.NonPublic | BindingFlags.Instance) still finds backing fields
- [ ] Reflection assertions on producer, consumer, options properties pass

### 6. Async/Cancellation Token Handling
- [ ] Async test methods with CancellationToken parameters pass
- [ ] No "token not generated" or "null reference" errors

---

## F. Global Usings Implications

Current .csproj global usings (uniform across all projects):
```xml
<Using Include="System.ComponentModel.DataAnnotations" />
<Using Include="Atc.Test" />
<Using Include="AutoFixture" />
<Using Include="AutoFixture.AutoNSubstitute" />
<Using Include="AutoFixture.Xunit2" />
<Using Include="FluentAssertions" />
<Using Include="NSubstitute" />
<Using Include="NSubstitute.ReturnsExtensions" />
<Using Include="NSubstitute.ExceptionExtensions" />
<Using Include="NSubstitute.ReceivedExtensions" />
<Using Include="Xunit" />
```

**Action:** Determine if Cabazure.Test .csproj should inherit these or if Trinity removes duplicates after adding Cabazure.Test reference.

---

## G. No Tests to Remove (All Are Coverage-Essential)

- **Metadata tests** → Validate Azure SDK type mapping
- **Publisher tests** → Verify serialization + SendAsync behavior
- **Processor tests** → Verify deserialization + error handling
- **DI tests** → Confirm registration and builder fluency
- **Scenario tests** → Integration-level validation
- All other tests → Incremental coverage

---

## H. Migration Sequencing Recommendation

1. **Update .csproj** for one backend (e.g., EventHub)
   - Remove explicit package refs
   - Add Cabazure.Test reference
   - Update global usings if needed
2. **Build & test** that backend only
3. **Validate** all test patterns (autodata, frozen, substitutes, assertions)
4. **Repeat** for ServiceBus and StorageQueue
5. **Leave Abstractions.Tests** as-is (no tests to migrate yet)

---

## I. Open Questions for Cabazure.Test Migration Guide

1. What is the replacement for `[Theory, AutoNSubstituteData]`?
2. Does the guide provide `[InlineAutoNSubstituteData]` or equivalent?
3. Are FluentAssertionsExtensions helpers (DI assertions) included in Cabazure.Test, or stay backend-local?
4. Should global usings be modified, or does Cabazure.Test.csproj supply them all?
5. Does Cabazure.Test maintain NSubstitute integration, or switch mocking frameworks?
6. Are there breaking changes to AutoFixture conventions (NoAutoProperties, Frozen ordering)?
7. How does Cabazure.Test handle reflection-based scenario tests?
8. What version of xUnit, AutoFixture, NSubstitute, FluentAssertions does Cabazure.Test pin?

---

## Summary

**Low Risk:** Most test logic is straightforward; few custom helpers.  
**Medium Risk:** Metadata tests rely heavily on AutoFixture auto-generation of Azure SDK types; must verify equivalence still holds.  
**High Risk:** Scenario tests use reflection; if DI setup changes, these will fail silently.  

**Trinity's Checklist (Pre-Edit):**
- [ ] Obtain and review Cabazure.Test MIGRATION.md or equivalent
- [ ] Verify AutoNSubstituteData replacement is provided
- [ ] Confirm [InlineAutoNSubstituteData] support
- [ ] Check if DI assertion helpers are bundled or must stay local
- [ ] Plan global usings update strategy
- [ ] Test one backend end-to-end before mass migration

