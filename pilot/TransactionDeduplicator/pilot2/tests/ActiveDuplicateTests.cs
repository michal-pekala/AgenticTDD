using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ActiveDuplicateTests : ExpirationTestsBase
{
    [Fact]
    public void Duplicate_of_active_transaction_id_returns_false()
    {
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    [Fact]
    public void Duplicate_attempt_restarts_expiration_from_its_own_time()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration / 2;

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();

        Now += expiration - 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }
}
