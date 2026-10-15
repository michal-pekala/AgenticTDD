using FluentAssertions;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;

namespace TransactionDeduplicator.Tests;

public sealed class FirstSeenTransactionTests
{
    [Fact]
    public void First_valid_transaction_is_accepted()
    {
        var deduplicator = new Deduplicator();

        var result = deduplicator.IsUniqueAndProcess("8b2f1f24-49bc-40fc-9c23-4c6a66c9a2f1");

        result.Should().BeTrue();
    }
}
