using TransactionDeduplicator.Pilot;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;
using TransactionDeduplicator.Tests.Fixture;

namespace TransactionDeduplicator.Tests;

public abstract class BaseTransactionDeduplicatorTest
{
    private readonly FakeDateTimeProvider _dateTimeProvider = new();
    protected readonly TransactionDeduplicatorOptions _options = new();
    private Deduplicator? _sut;

    protected Deduplicator Sut
        => _sut ??= new Deduplicator(_dateTimeProvider, _options);

    protected DateTimeOffset Now
    {
        get => _dateTimeProvider.Now;
        set => _dateTimeProvider.Now = value;
    }
}
