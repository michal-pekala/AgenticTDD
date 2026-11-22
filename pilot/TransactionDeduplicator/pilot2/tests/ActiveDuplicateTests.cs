using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests;

public sealed class ActiveDuplicateTests
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
