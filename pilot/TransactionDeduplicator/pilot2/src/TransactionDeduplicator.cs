namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    private readonly TimeSpan _uniquenessCheckExpiration;
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
        _dateTimeProvider = dateTimeProvider;
    }

    public bool IsUniqueAndProcess(string transactionId)
    {
        var id = ParseTransactionId(transactionId);
        var now = _dateTimeProvider.CurrentTime;
        var isActive = _lastAttempts.TryGetValue(id, out var lastAttempt)
            && now < lastAttempt + _uniquenessCheckExpiration;

        _lastAttempts[id] = now;

        return !isActive;
    }

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
