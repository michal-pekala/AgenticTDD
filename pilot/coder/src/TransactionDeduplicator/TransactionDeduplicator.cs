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

        while (true)
        {
            var currentTime = _dateTimeProvider.CurrentTime;
            if (_transactions.TryAdd(parsedTransactionId, currentTime))
            {
                return true;
            }

            if (!_transactions.TryGetValue(parsedTransactionId, out var lastAttempt))
            {
                continue;
            }

            currentTime = _dateTimeProvider.CurrentTime;
            if (currentTime >= lastAttempt + _options.UniqnessCheckExpiration)
            {
                var expiredEntry = new KeyValuePair<Guid, DateTimeOffset>(
                    parsedTransactionId,
                    lastAttempt);

                if (((ICollection<KeyValuePair<Guid, DateTimeOffset>>)_transactions)
                    .Remove(expiredEntry))
                {
                    continue;
                }

                continue;
            }

            if (_transactions.TryUpdate(parsedTransactionId, currentTime, lastAttempt))
            {
                return false;
            }
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
