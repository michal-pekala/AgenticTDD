using FluentAssertions;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;

namespace TransactionDeduplicator.Tests;

public sealed class FirstSeenTransactionTests
{
    private readonly Deduplicator Sut = new();

    [Fact]
    public void First_valid_transaction_is_accepted()
        => Sut
            .IsUniqueAndProcess(Guid.NewGuid().ToString())
            .Should().BeTrue();
}
