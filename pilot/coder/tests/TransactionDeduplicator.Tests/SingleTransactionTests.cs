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

    [Fact]
    public void Malformed_transaction_id_is_rejected()
    {
        var invalidId = $"not-a-guid-{Guid.NewGuid()}";

        FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(invalidId))
            .Should().Throw<ArgumentException>()
            .WithMessage($"TransactionId value '{invalidId}' is not a valid Guid.");
    }
}
