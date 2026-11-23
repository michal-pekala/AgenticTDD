using FluentAssertions;
using TransactionDeduplication.Tests.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ConcurrentCapacityTests : ConcurrencyTestsBase
{
    public ConcurrentCapacityTests()
        => Options.UniqnessBufferCapacity = Random.Shared.Next(100, 1_001);

    [Fact]
    public async Task Concurrent_new_ids_beyond_capacity_accept_exactly_capacity()
    {
        var outcomes = await AttemptNewTransactionIdsConcurrently();

        outcomes.Count(outcome => outcome.Result == true).Should().Be(Options.UniqnessBufferCapacity);
    }

    [Fact]
    public async Task Concurrent_new_ids_beyond_capacity_are_rejected_with_CapacityExceededException()
    {
        var outcomes = await AttemptNewTransactionIdsConcurrently();

        outcomes
            .Where(outcome => outcome.Result != true)
            .Should().AllSatisfy(outcome => outcome.Exception.Should().BeOfType<CapacityExceededException>());
    }

    private async Task<CallOutcome[]> AttemptNewTransactionIdsConcurrently()
    {
        var callsPerTask = Options.UniqnessBufferCapacity / 4;

        var outcomes = await RunConcurrent(() => Enumerable
            .Range(0, callsPerTask)
            .Select(_ => Sut.Attempt(Guid.NewGuid().ToString()))
            .ToArray());

        return outcomes.SelectMany(outcome => outcome).ToArray();
    }
}
