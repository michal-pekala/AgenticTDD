using FluentAssertions;
using TransactionDeduplication.Extensions;
using TransactionDeduplication.Tests.Extensions;
using TransactionDeduplication.Tests.Fixture;
using Error = TransactionDeduplication.ValidationError;

namespace TransactionDeduplication.Tests;

public sealed class CapacityLimitTests : ExpirationTestsBase
{
    public CapacityLimitTests()
        => Options.UniqnessBufferCapacity = Random.Shared.Next(2, 11);

    [Fact]
    public void New_transaction_id_is_rejected_when_buffer_is_full_of_active_ids()
    {
        AcceptNewTransactionIds(Options.UniqnessBufferCapacity);

        Sut.AssertRejected(Guid.NewGuid().ToString());
    }

    [Fact]
    public void Capacity_rejection_throws_CapacityExceededException_stating_the_capacity()
    {
        var capacity = Options.UniqnessBufferCapacity;

        AcceptNewTransactionIds(capacity);

        Sut.AssertException<CapacityExceededException>(
            Guid.NewGuid().ToString(),
            Error.CapacityExceeded.Format(capacity));
    }

    [Fact]
    public void Capacity_rejection_exception_holds_the_configured_capacity()
    {
        var capacity = Options.UniqnessBufferCapacity;

        AcceptNewTransactionIds(capacity);

        FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()))
            .Should().Throw<CapacityExceededException>()
            .Which.Capacity.Should().Be(capacity);
    }

    [Fact]
    public void Retained_count_never_exceeds_capacity()
    {
        var capacity = Options.UniqnessBufferCapacity;
        var extraAttempts = Random.Shared.Next(1, 11);

        AcceptNewTransactionIds(capacity);

        for (var i = 0; i < extraAttempts; i++)
        {
            Sut.AssertRejected(Guid.NewGuid().ToString());
        }

        Sut.RetainedCount.Should().Be(capacity);
    }

    [Fact]
    public void Capacity_rejection_leaves_retained_ids_active()
    {
        var transactionIds = AcceptNewTransactionIds(Options.UniqnessBufferCapacity);

        Sut.AssertRejected(Guid.NewGuid().ToString());

        foreach (var transactionId in transactionIds)
        {
            Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
        }
    }

    [Fact]
    public void Duplicate_of_active_id_returns_false_when_buffer_is_full()
    {
        var transactionIds = AcceptNewTransactionIds(Options.UniqnessBufferCapacity);
        var duplicate = transactionIds[Random.Shared.Next(transactionIds.Count)];

        Sut.IsUniqueAndProcess(duplicate).Should().BeFalse();
    }

    [Fact]
    public void Duplicate_of_active_id_refreshes_its_expiration_when_buffer_is_full()
    {
        var expiration = Options.UniqnessCheckExpiration;
        var transactionIds = AcceptNewTransactionIds(Options.UniqnessBufferCapacity);
        var duplicate = transactionIds[Random.Shared.Next(transactionIds.Count)];

        Now += expiration / 2;

        Sut.IsUniqueAndProcess(duplicate).Should().BeFalse();

        Now += expiration / 2;

        Sut.IsUniqueAndProcess(duplicate).Should().BeFalse();
    }

    [Fact]
    public void Full_buffer_with_an_expired_id_accepts_new_transaction_id()
    {
        var expiration = Options.UniqnessCheckExpiration;

        AcceptNewTransactionIds(1);

        Now += expiration / 2;
        AcceptNewTransactionIds(Options.UniqnessBufferCapacity - 1);
        Now += expiration / 2;

        Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()).Should().BeTrue();
    }

    [Fact]
    public void Rejected_transaction_id_is_accepted_once_an_active_id_expires()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        AcceptNewTransactionIds(1);

        Now += expiration / 2;
        AcceptNewTransactionIds(Options.UniqnessBufferCapacity - 1);

        Sut.AssertRejected(transactionId);

        Now += expiration / 2;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
    }

    private List<string> AcceptNewTransactionIds(int count)
    {
        var transactionIds = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var transactionId = Guid.NewGuid().ToString();

            Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

            transactionIds.Add(transactionId);
        }

        return transactionIds;
    }
}
