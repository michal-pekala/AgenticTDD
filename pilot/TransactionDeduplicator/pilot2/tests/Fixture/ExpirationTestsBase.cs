using FluentAssertions.Extensions;

namespace TransactionDeduplication.Tests.Fixture;

public abstract class ExpirationTestsBase
{
    private readonly FakeDateTimeProvider _clock = new();
    private TransactionDeduplicator? _sut;

    protected TransactionDeduplicatorOptions Options { get; } = new()
    {
        UniqnessCheckExpiration = Random.Shared.Next(2_000, 60_001).Milliseconds(),
    };

    protected DateTimeOffset Now
    {
        get => _clock.CurrentTime;
        set => _clock.CurrentTime = value;
    }

    protected TransactionDeduplicator Sut
        => _sut ??= new TransactionDeduplicator(Options, _clock, Timeout.InfiniteTimeSpan);
}
