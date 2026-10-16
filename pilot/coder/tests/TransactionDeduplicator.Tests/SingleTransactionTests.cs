using FluentAssertions;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;

namespace TransactionDeduplicator.Tests;

public sealed class SingleTransactionTests
{
    private Deduplicator Sut { get; } = new();

    [Fact]
    public void New_transaction_can_be_processed()
        => Sut
            .IsUniqueAndProcess(Guid.NewGuid().ToString())
            .Should().BeTrue();
}
