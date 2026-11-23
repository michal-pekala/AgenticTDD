using FluentAssertions.Extensions;

namespace TransactionDeduplication.Tests.Fixture;

public abstract class ConcurrencyTestsBase : ExpirationTestsBase
{
    private readonly TimeSpan _timeout = 10.Seconds();

    protected Task<T[]> RunConcurrent<T>(Func<T> func, int concurrentCalls = 16)
    {
        // Create the lazy Sut before the calls start, so they all share one instance.
        _ = Sut;

        var start = new Barrier(concurrentCalls);

        return Task
            .WhenAll(Enumerable
                .Range(0, concurrentCalls)
                .Select(_ => Task.Factory.StartNew(
                    () => StartTogether(start, func),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)))
            .WaitAsync(_timeout);
    }

    private T StartTogether<T>(Barrier start, Func<T> func)
        => start.SignalAndWait(_timeout)
            ? func()
            : throw new TimeoutException("Concurrent calls did not start together.");
}
