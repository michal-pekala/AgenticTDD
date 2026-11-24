using FluentAssertions;
using TransactionDeduplication.Tests.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ConcurrentDistinctTransactionIdTests : ConcurrencyTestsBase
{
    private const int CallsPerTask = 500;

    [Fact]
    public async Task Concurrent_new_ids_are_all_accepted()
    {
        var outcomes = await RunConcurrent(() => Enumerable
            .Range(0, CallsPerTask)
            .Select(_ => Sut.Attempt(Guid.NewGuid().ToString()))
            .ToArray());

        outcomes.SelectMany(outcome => outcome).Should().AllBeEquivalentTo(new CallOutcome(true, null));
    }

    [Fact]
    public async Task Concurrently_accepted_ids_all_stay_active()
    {
        var transactionIds = await RunConcurrent(() => Enumerable
            .Range(0, CallsPerTask)
            .Select(_ => Guid.NewGuid().ToString())
            .Where(transactionId => Sut.IsUniqueAndProcess(transactionId))
            .ToArray());

        foreach (var transactionId in transactionIds.SelectMany(transactionId => transactionId))
        {
            Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Duplicates_of_an_active_id_return_false_while_new_ids_are_added()
    {
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        var results = await RunConcurrent(() => ProcessNewIdsAndDuplicates(transactionId));

        results.SelectMany(result => result).Should().AllSatisfy(result => result.Should().BeFalse());
    }

    [Fact]
    public async Task Duplicates_of_an_active_id_refresh_its_expiration_while_new_ids_are_added()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration / 2;

        await RunConcurrent(() => ProcessNewIdsAndDuplicates(transactionId));

        Now += expiration / 2;

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    private bool[] ProcessNewIdsAndDuplicates(string activeTransactionId)
        => Enumerable
            .Range(0, CallsPerTask)
            .Select(_ =>
            {
                Sut.IsUniqueAndProcess(Guid.NewGuid().ToString());

                return Sut.IsUniqueAndProcess(activeTransactionId);
            })
            .ToArray();
}
