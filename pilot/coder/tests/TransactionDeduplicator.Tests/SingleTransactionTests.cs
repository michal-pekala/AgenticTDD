using FluentAssertions;
using TransactionDeduplicator.Pilot;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;
using Error = TransactionDeduplicator.Pilot.ValidationError;

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
            .WithMessage(Error.InvalidTransactionId.Format(invalidId));
    }

    [Fact]
    public void Active_transaction_duplicate_is_rejected()
    {
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }
}
