using FluentAssertions;

namespace TransactionDeduplication.Tests;

public sealed class FirstSeenTransactionTests
{
    private TransactionDeduplicator? _sut;

    private TransactionDeduplicator Sut => _sut ??= new TransactionDeduplicator();

    [Fact]
    public void Unseen_transaction_id_returns_true()
        => Sut
            .IsUniqueAndProcess(Guid.NewGuid().ToString())
            .Should().BeTrue();
}
