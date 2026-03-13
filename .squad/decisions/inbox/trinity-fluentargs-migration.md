# Trinity Decision Inbox: FluentArg matcher cleanup

- For single-call NSubstitute assertions that previously used `Arg.Any<T>()` and then inspected the captured argument with `ReceivedArg<T>()`, prefer `FluentArg.Match<T>(...)` and put the FluentAssertions check inline in the `Received(...)` call.
- Do not apply this cleanup to loop-driven or aggregate verification flows that intentionally use `ReceivedArgs<T>()` after call-count assertions, such as batch processor tests validating many calls together.
- Applied in:
  - `test\Cabazure.Messaging.ServiceBus.Tests\Internal\ServiceBusProcessorServiceTests.cs`
  - `test\Cabazure.Messaging.EventHub.Tests\Internal\EventHubBatchHandlerTests.cs`
