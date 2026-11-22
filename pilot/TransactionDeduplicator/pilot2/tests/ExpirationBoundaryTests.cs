using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ExpirationBoundaryTests
{
    private readonly FakeDateTimeProvider _clock = new();
    private TransactionDeduplicator? _sut;

    private TransactionDeduplicatorOptions Options { get; } = new()
    {
        UniqnessCheckExpiration = Random.Shared.Next(2_000, 60_001).Milliseconds(),
    };

    private DateTimeOffset Now
    {
        get => _clock.CurrentTime;
        set => _clock.CurrentTime = value;
    }

    private TransactionDeduplicator Sut
        => _sut ??= new TransactionDeduplicator(Options, _clock, Timeout.InfiniteTimeSpan);

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
