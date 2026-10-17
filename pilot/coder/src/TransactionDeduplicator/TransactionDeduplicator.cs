using System.Collections.Concurrent;

namespace TransactionDeduplicator.Pilot;

public sealed class TransactionDeduplicator
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _transactions = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TransactionDeduplicatorOptions _options;

    public TransactionDeduplicator()
        : this(new SystemDateTimeProvider(), new TransactionDeduplicatorOptions())
    {
    }

    internal TransactionDeduplicator(
        IDateTimeProvider dateTimeProvider,
        TransactionDeduplicatorOptions options)
    {
        _dateTimeProvider = dateTimeProvider;
        _options = options;
    }

    public bool IsUniqueAndProcess(string transactionId)
    {
        ValidateOptions();

        var parsedTransactionId = ParseTransactionId(transactionId);
        var currentTime = _dateTimeProvider.CurrentTime;

        var isActiveDuplicate =
            _transactions.TryGetValue(parsedTransactionId, out var lastAttempt)
            && currentTime < lastAttempt + _options.UniqnessCheckExpiration;

        _transactions[parsedTransactionId] = currentTime;

        return !isActiveDuplicate;
    }

    private void ValidateOptions()
    {
        if (_options.UniqnessCheckExpiration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                paramName: null,
                message:
                ValidationError.InvalidUniqnessCheckExpiration.Format(
                    _options.UniqnessCheckExpiration));
        }

        if (_options.UniqnessBufferCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName: null,
                message:
                ValidationError.InvalidUniqnessBufferCapacity.Format(
                    _options.UniqnessBufferCapacity));
        }
    }

    private static Guid ParseTransactionId(string transactionId)
    {
        if (!Guid.TryParse(transactionId, out var parsedTransactionId))
        {
            throw new ArgumentException(
                ValidationError.InvalidTransactionId.Format(transactionId));
        }

        return parsedTransactionId;
    }
}
