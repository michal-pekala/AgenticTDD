namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    private readonly TransactionDeduplicatorOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Dictionary<string, DateTimeOffset> _lastAttempts = new();

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
        _options = options;
        _dateTimeProvider = dateTimeProvider;
    }

    public bool IsUniqueAndProcess(string transactionId)
    {
        var now = _dateTimeProvider.CurrentTime;
        var isActive = _lastAttempts.TryGetValue(transactionId, out var lastAttempt)
            && now < lastAttempt + _options.UniqnessCheckExpiration;

        _lastAttempts[transactionId] = now;

        return !isActive;
    }
}
