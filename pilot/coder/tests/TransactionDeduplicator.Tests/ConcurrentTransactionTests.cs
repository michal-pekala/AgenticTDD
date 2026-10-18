using FluentAssertions;

namespace TransactionDeduplicator.Tests;

public sealed class ConcurrentTransactionTests : BaseTransactionDeduplicatorTest
{
    [Fact]
    public async Task Concurrent_calls_for_same_unseen_transaction_accept_it_once()
    {
        var transactionId = Guid.NewGuid().ToString();

        var results = await RunConcurrent(() => Sut.IsUniqueAndProcess(transactionId));

        results.Should().ContainSingle(result => result);
    }

    private static Task<T[]> RunConcurrent<T>(Func<T> func, int concurrentCalls = 16)
        => Task.WhenAll(Enumerable
            .Range(0, concurrentCalls)
            .Select(_ => Task.Run(func)));
}
