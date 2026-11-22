using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ExpirationBoundaryTests : ExpirationTestsBase
{
    [Fact]
    public void Transaction_id_is_still_a_duplicate_just_before_expiration()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration - 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    [Fact]
    public void Transaction_id_is_accepted_again_exactly_at_expiration()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
    }

    [Fact]
    public void Transaction_id_is_accepted_again_after_expiration()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration + 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
    }
}
