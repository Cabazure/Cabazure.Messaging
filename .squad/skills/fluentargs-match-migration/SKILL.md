# Skill: FluentArgs.Match Migration for NSubstitute Mock Assertions

**Version:** 1.0  
**Owner:** Tank (Tester & Reviewer)  
**Created:** 2025-01-10  
**Context:** Cabazure.Messaging test suite; generalizable to any NSubstitute + FluentArgs project

---

## Overview

This skill documents the pattern for migrating from the older Cabazure.Test `Arg.Any<T>()` + `.ReceivedArg<T>()` assertion pattern to the newer **FluentArgs.Match** pattern. It enables test authors to write cleaner, more readable assertions on mock calls.

---

## The Problem

**Old Pattern (Cabazure.Test + NSubstitute):**
```csharp
// 1. Verify call with Arg.Any
_ = mock.Received(1).Method(Arg.Any<ServiceBusMessage>(), cancellationToken);

// 2. Extract the actual argument
var actualMessage = mock.ReceivedArg<ServiceBusMessage>();

// 3. Assert on the extracted value
actualMessage.ContentType
    .Should()
    .BeEquivalentTo(expectedContentType);
```

**Issues:**
- Three separate statements for a single logical assertion
- Argument extraction happens outside the `.Received()` call, making intent unclear
- Easy to accidentally extract the wrong argument in multi-call scenarios
- Reads like a code smell: "Check if called, then extract, then validate"

---

## The Solution

**New Pattern (FluentArgs.Match):**
```csharp
mock.Received(1).Method(
    FluentArgs.Match<ServiceBusMessage>(msg =>
        msg.ContentType.Should().BeEquivalentTo(expectedContentType)),
    cancellationToken);
```

**Benefits:**
- Single statement makes intent crystal clear
- Matcher is inline with the call verification
- Assertion logic stays close to the `.Received()` call
- Eliminates intermediate extraction variable
- Type-safe: matcher is scoped to the specific argument position

---

## When to Use This Pattern

✅ **Use FluentArgs.Match when:**
- You want to both verify a call was made AND validate the argument's properties
- The argument is a complex object (message, options, metadata) with multiple properties to validate
- You extract the argument via `ReceivedArg<T>()` immediately after `.Received()` for assertions

❌ **Do NOT use when:**
- You only care about verifying the call was made (no assertion on the argument value)
- You need the extracted argument for multiple separate assertions across test setup
- The argument is a primitive type with a simple expected value (use `Arg.Is<T>()` instead)
- You need to transform the extracted value before assertion (e.g., `.ReceivedArg<IEnumerable<T>>().Single()`)
- You use the extracted value in a secondary mock verification call (e.g., `modifier.Received(1).Invoke(message, extractedValue)`)
- Loop-based verification flows using `ReceivedArgs<T>()` (plural) for aggregate assertions

---

## Migration Checklist

1. **Identify the pattern:**
   ```csharp
   mock.Received(n).Method(...Arg.Any<T>()...);
   var extracted = mock.ReceivedArg<T>();
   extracted.SomeProperty.Should()...;
   ```

2. **Move the assertion inside the `.Received()` call:**
   ```csharp
   mock.Received(n).Method(
       FluentArgs.Match<T>(arg => arg.SomeProperty.Should()...),
       ...);
   ```

3. **Remove the extraction variable and separate assertion.**

4. **Run tests to confirm behavior unchanged.**

---

## Examples from Cabazure.Messaging

### Example 1: Single Property Assertion
**Before:**
```csharp
// ServiceBusPublisherTests.cs: PublishAsync_Sends_Serialized_Message
var eventData = sender.ReceivedArg<ServiceBusMessage>();
eventData.Body
    .ToObjectFromJson<TMessage>(serializerOptions)
    .Should()
    .BeEquivalentTo(message);
```

**After:**
```csharp
sender.Received(1).SendMessageAsync(
    FluentArgs.Match<ServiceBusMessage>(msg =>
        msg.Body
            .ToObjectFromJson<TMessage>(serializerOptions)
            .Should()
            .BeEquivalentTo(message)),
    cancellationToken);
```

### Example 2: Multiple Properties
**Before:**
```csharp
// ServiceBusPublisherTests.cs: PublishAsync_Sends_Metadate_From_PublishingOptions
var eventData = sender.ReceivedArg<ServiceBusMessage>();
eventData.ContentType.Should().BeEquivalentTo(options.ContentType);
eventData.CorrelationId.Should().BeEquivalentTo(options.CorrelationId);
eventData.MessageId.Should().BeEquivalentTo(options.MessageId);
```

**After:**
```csharp
sender.Received(1).SendMessageAsync(
    FluentArgs.Match<ServiceBusMessage>(msg =>
    {
        msg.ContentType.Should().BeEquivalentTo(options.ContentType);
        msg.CorrelationId.Should().BeEquivalentTo(options.CorrelationId);
        msg.MessageId.Should().BeEquivalentTo(options.MessageId);
    }),
    cancellationToken);
```

### Example 3: Processor Metadata
**Before:**
```csharp
// ServiceBusProcessorServiceTests.cs: Processor_Is_Called_When_Client_Receives_Message
_ = processor.Received(1).ProcessAsync(
    message,
    Arg.Any<ServiceBusMetadata>(),
    args.CancellationToken);
processor.ReceivedArg<ServiceBusMetadata>()
    .Should()
    .BeEquivalentTo(ServiceBusMetadata.Create(args.Message));
```

**After:**
```csharp
processor.Received(1).ProcessAsync(
    message,
    FluentArgs.Match<ServiceBusMetadata>(metadata =>
        metadata.Should()
            .BeEquivalentTo(ServiceBusMetadata.Create(args.Message))),
    args.CancellationToken);
```

---

## Common Pitfalls

### Pitfall 1: Forgetting the Matcher Lambda
❌ **Wrong:**
```csharp
mock.Received(1).Method(FluentArgs.Match<T>);  // Missing lambda
```

✅ **Right:**
```csharp
mock.Received(1).Method(FluentArgs.Match<T>(arg => arg.Should()...));
```

### Pitfall 2: Confusing Arg.Is with FluentArgs.Match
❌ **Wrong (for assertions):**
```csharp
// Arg.Is<T>() is for equality matching, not assertions
mock.Received(1).Method(Arg.Is<T>(x => x.Property == expected));
```

✅ **Right:**
```csharp
// FluentArgs.Match is for fluent assertions
mock.Received(1).Method(FluentArgs.Match<T>(x =>
    x.Property.Should().Be(expected)));
```

### Pitfall 3: Using Before Multiple Separate Assertions
❌ **Fragile (position-dependent):**
```csharp
mock.Received(1).Method(
    Arg.Any<T1>(),
    Arg.Any<T2>());
var t1 = mock.ReceivedArg<T1>();
var t2 = mock.ReceivedArg<T2>();
// If order changes, both extractions break!
```

✅ **Safe (type-dependent):**
```csharp
mock.Received(1).Method(
    FluentArgs.Match<T1>(arg => arg.Should()...),
    FluentArgs.Match<T2>(arg => arg.Should()...));
// Type safety ensures each matcher matches its intended argument
```

---

## When to Ask for Help

- **Q:** Can I use FluentArgs.Match on a primitive argument?  
  **A:** Yes, but `Arg.Is<T>()` is more concise for simple equality: `Arg.Is<string>("expected")`

- **Q:** Can I extract the argument outside for reuse?  
  **A:** Not with FluentArgs.Match. If you need the value elsewhere, keep the old `.ReceivedArg<T>()` pattern (that's a sign the test might be doing too much).

- **Q:** What if the assertion I want is complex?  
  **A:** Move the complex logic into a local helper method called from the matcher:
  ```csharp
  mock.Received(1).Method(
      FluentArgs.Match<T>(arg => AssertMessageProperties(arg, expected)));

  private void AssertMessageProperties(ServiceBusMessage msg, Expected expected)
  {
      msg.ContentType.Should().BeEquivalentTo(expected.ContentType);
      msg.MessageId.Should().BeEquivalentTo(expected.MessageId);
      // ... more assertions
  }
  ```

---

## References

- **Cabazure.Messaging Audit:** `.squad/audits/tank-fluentargs-pattern-audit.md`
- **NSubstitute Docs:** https://nsubstitute.github.io/
- **FluentAssertions:** https://fluentassertions.com/
- **Cabazure.Test Migration Map:** `.squad/migration-map-cabazure-test.md`

---

## Review Checklist for Code Reviewers

When reviewing a FluentArgs.Match migration:

- [ ] Assertion logic inside the matcher is equivalent to the old extraction + assertion
- [ ] No intermediate variables left over from the extraction pattern
- [ ] Matcher is scoped to a single argument type
- [ ] Test still passes with the new pattern
- [ ] Intent is clearer than the old three-statement pattern

---

*Skill documented by Tank. Update this file if new patterns or best practices emerge.*
