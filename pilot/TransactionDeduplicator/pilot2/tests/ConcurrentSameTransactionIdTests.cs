using FluentAssertions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ConcurrentSameTransactionIdTests : ConcurrencyTestsBase
{
    private const int Races = 50;

    [Fact]
    public async Task Racing_calls_for_an_unseen_id_accept_it_exactly_once()
    {
        for (var race = 0; race < Races; race++)
        {
            var transactionId = Guid.NewGuid().ToString();

            var results = await RunConcurrent(() => Sut.IsUniqueAndProcess(transactionId));

            results.Should().ContainSingle(result => result);
        }
    }

    [Fact]
    public async Task Racing_calls_for_an_expired_id_accept_it_exactly_once()
    {
        var expiration = Options.UniqnessCheckExpiration;

        for (var race = 0; race < Races; race++)
        {
            var transactionId = Guid.NewGuid().ToString();

            Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

            Now += expiration;

            var results = await RunConcurrent(() => Sut.IsUniqueAndProcess(transactionId));

            results.Should().ContainSingle(result => result);
        }
    }
}
