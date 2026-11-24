using FluentAssertions;
using TransactionDeduplication.Tests.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ConcurrentLastFreeSlotTests : ConcurrencyTestsBase
{
    private const int Races = 200;

    public ConcurrentLastFreeSlotTests()
        => Options.UniqnessBufferCapacity = Random.Shared.Next(2, 11);

    [Fact]
    public async Task Racing_calls_for_an_unseen_id_at_the_last_free_slot_return_false_except_the_accepted_one()
    {
        for (var race = 0; race < Races; race++)
        {
            var outcomes = await RaceForUnseenIdAtLastFreeSlot();

            outcomes
                .Where(outcome => outcome.Result != true)
                .Should().AllBeEquivalentTo(new CallOutcome(false, null));
        }
    }

    // Each race starts with the buffer holding capacity - 1 active IDs and no expired ones:
    // the IDs left by the previous race expire first, and the first new ID sweeps them all.
    private async Task<CallOutcome[]> RaceForUnseenIdAtLastFreeSlot()
    {
        var transactionId = Guid.NewGuid().ToString();

        Now += Options.UniqnessCheckExpiration;

        for (var i = 0; i < Options.UniqnessBufferCapacity - 1; i++)
        {
            Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()).Should().BeTrue();
        }

        return await RunConcurrent(() => Sut.Attempt(transactionId));
    }
}
