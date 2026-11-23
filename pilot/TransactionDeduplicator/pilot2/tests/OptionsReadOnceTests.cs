using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class OptionsReadOnceTests : ExpirationTestsBase
{
    [Fact]
    public void Expiration_shortened_after_construction_does_not_apply()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Options.UniqnessCheckExpiration = expiration / 2;
        Now += expiration - 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    [Fact]
    public void Expiration_lengthened_after_construction_does_not_apply()
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Options.UniqnessCheckExpiration = expiration * 2;
        Now += expiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
    }
}
