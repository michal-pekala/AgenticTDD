using System.Collections.Concurrent;

namespace TransactionDeduplicator.Pilot;

public sealed class TransactionDeduplicator
{
    private readonly ConcurrentDictionary<Guid, byte> _transactions = new();
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
        return _transactions.TryAdd(parsedTransactionId, 0);
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
