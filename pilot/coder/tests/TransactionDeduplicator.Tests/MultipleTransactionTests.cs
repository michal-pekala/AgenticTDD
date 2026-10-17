using FluentAssertions;
using TransactionDeduplicator.Pilot;
using Error = TransactionDeduplicator.Pilot.ValidationError;
using TransactionDeduplicator.Tests.Extensions;

namespace TransactionDeduplicator.Tests;

public sealed class MultipleTransactionTests : BaseTransactionDeduplicatorTest
{
    [Fact]
    public void New_transaction_is_rejected_when_capacity_is_full()
    {
        _options.UniqnessBufferCapacity = 1;
        var activeTransactionId = Guid.NewGuid().ToString();
        var newTransactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(activeTransactionId)
            .Should().BeTrue();

        Sut.AssertException<CapacityExceededException>(
            newTransactionId,
            Error.CapacityExceeded.Format(_options.UniqnessBufferCapacity));

        Sut.IsUniqueAndProcess(activeTransactionId)
            .Should().BeFalse();
    }
}
