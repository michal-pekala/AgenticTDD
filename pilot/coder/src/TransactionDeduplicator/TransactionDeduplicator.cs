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
        var parsedTransactionId = ParseTransactionId(transactionId);
        var currentTime = _dateTimeProvider.CurrentTime;

        if (_transactions.TryGetValue(parsedTransactionId, out var lastAttempt)
            && currentTime < lastAttempt + _options.UniqnessCheckExpiration)
        {
            _transactions[parsedTransactionId] = currentTime;
            return false;
        }

        _transactions[parsedTransactionId] = currentTime;
        return true;
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
