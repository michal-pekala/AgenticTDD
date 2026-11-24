using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Extensions;
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

    [Fact]
    public void Capacity_raised_after_construction_does_not_apply()
    {
        Options.UniqnessBufferCapacity = Random.Shared.Next(2, 11);

        var capacity = Options.UniqnessBufferCapacity;

        for (var i = 0; i < capacity; i++)
        {
            Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()).Should().BeTrue();
        }

        Options.UniqnessBufferCapacity = capacity * 2;

        Sut.AssertRejected(Guid.NewGuid().ToString());
    }
}
