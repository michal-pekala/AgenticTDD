namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    private readonly TimeSpan _uniquenessCheckExpiration;
    private readonly int _uniquenessBufferCapacity;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Dictionary<Guid, DateTimeOffset> _lastAttempts = new();

    public TransactionDeduplicator()
        : this(
            new TransactionDeduplicatorOptions(),
            new SystemDateTimeProvider(),
            Timeout.InfiniteTimeSpan)
    {
    }

    internal TransactionDeduplicator(
        TransactionDeduplicatorOptions options,
        IDateTimeProvider dateTimeProvider,
        TimeSpan cleanerInterval)
    {
        _uniquenessCheckExpiration = options.UniqnessCheckExpiration;
        _uniquenessBufferCapacity = options.UniqnessBufferCapacity;
        _dateTimeProvider = dateTimeProvider;
    }

    internal int RetainedCount => _lastAttempts.Count;

    public bool IsUniqueAndProcess(string transactionId)
    {
        var id = ParseTransactionId(transactionId);
        var now = _dateTimeProvider.CurrentTime;
        var isKnown = _lastAttempts.TryGetValue(id, out var lastAttempt);
        var isActive = isKnown && IsActive(lastAttempt, now);

        if (!isKnown && _lastAttempts.Count >= _uniquenessBufferCapacity)
        {
            SweepExpired(now);

            if (_lastAttempts.Count >= _uniquenessBufferCapacity)
            {
                throw new CapacityExceededException(_uniquenessBufferCapacity);
            }
        }

        _lastAttempts[id] = now;

        return !isActive;
    }

    internal void SweepExpired() => SweepExpired(_dateTimeProvider.CurrentTime);

    private void SweepExpired(DateTimeOffset now)
    {
        foreach (var (id, lastAttempt) in _lastAttempts)
        {
            if (!IsActive(lastAttempt, now))
            {
                _lastAttempts.Remove(id);
            }
        }
    }

    private bool IsActive(
        DateTimeOffset lastAttempt,
        DateTimeOffset now)
        => now < lastAttempt + _uniquenessCheckExpiration;

    private Guid ParseTransactionId(string transactionId)
    {
        if (transactionId is null)
        {
            throw new NullTransactionIdException();
        }

        if (!Guid.TryParse(transactionId, out var id))
        {
            throw new InvalidTransactionIdException(transactionId);
        }

        return id;
    }
}
