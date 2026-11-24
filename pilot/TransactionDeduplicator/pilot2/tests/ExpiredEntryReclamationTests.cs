using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ExpiredEntryReclamationTests : ExpirationTestsBase
{
    [Fact]
    public void Repeated_duplicates_keep_one_entry_for_the_transaction_id()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;
        var duplicateCount = Random.Shared.Next(2, 11);

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        for (var i = 0; i < duplicateCount; i++)
        {
            Now += expiration / 2;

            Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
        }

        Sut.RetainedCount.Should().Be(1);
    }

    [Fact]
    public void Expired_transaction_id_accepted_again_keeps_one_entry()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Sut.RetainedCount.Should().Be(1);
    }

    [Fact]
    public void Sweep_reclaims_expired_transaction_ids()
    {
        var expiration = Options.UniqnessCheckExpiration;
        var transactionIdCount = Random.Shared.Next(2, 11);

        AcceptNewTransactionIds(transactionIdCount);

        Sut.RetainedCount.Should().Be(transactionIdCount);

        Now += expiration;
        Sut.SweepExpired();

        Sut.RetainedCount.Should().Be(0);
    }

    [Fact]
    public void Sweep_keeps_active_transaction_id()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration - 1.Milliseconds();
        Sut.SweepExpired();

        Sut.RetainedCount.Should().Be(1);

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    [Fact]
    public void New_transaction_id_reclaims_expired_ids_from_a_full_buffer()
    {
        Options.UniqnessBufferCapacity = Random.Shared.Next(2, 11);

        var expiration = Options.UniqnessCheckExpiration;

        AcceptNewTransactionIds(Options.UniqnessBufferCapacity);

        Now += expiration;

        Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()).Should().BeTrue();

        Sut.RetainedCount.Should().Be(1);
    }

    private void AcceptNewTransactionIds(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()).Should().BeTrue();
        }
    }
}
