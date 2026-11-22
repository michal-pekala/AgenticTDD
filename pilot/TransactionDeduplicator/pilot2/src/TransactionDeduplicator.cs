using TransactionDeduplication.Extensions;

namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    private readonly TransactionDeduplicatorOptions _options;
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
        _options = options;
        _dateTimeProvider = dateTimeProvider;
    }

    public bool IsUniqueAndProcess(string transactionId)
    {
        var id = ParseTransactionId(transactionId);
        var now = _dateTimeProvider.CurrentTime;
        var isActive = _lastAttempts.TryGetValue(id, out var lastAttempt)
            && now < lastAttempt + _options.UniqnessCheckExpiration;

        _lastAttempts[id] = now;

        return !isActive;
    }

    private static Guid ParseTransactionId(string transactionId)
    {
        ArgumentNullException.ThrowIfNull(transactionId);

        if (!Guid.TryParse(transactionId, out var id))
        {
            throw new ArgumentException(
                ValidationError.InvalidTransactionId.Format(transactionId));
        }

        return id;
    }
}
