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

        if (!isActiveDuplicate
            && !_transactions.ContainsKey(parsedTransactionId)
            && _transactions.Count >= _options.UniqnessBufferCapacity)
        {
            throw new CapacityExceededException(_options.UniqnessBufferCapacity);
        }

        _transactions[parsedTransactionId] = currentTime;

        return !isActiveDuplicate;
    }

    private void ValidateOptions()
    {
        ValidateOption(
            _options.UniqnessCheckExpiration,
            expiration => expiration > TimeSpan.Zero,
            ValidationError.InvalidUniqnessCheckExpiration);

        ValidateOption(
            _options.UniqnessBufferCapacity,
            capacity => capacity > 0,
            ValidationError.InvalidUniqnessBufferCapacity);
    }

    private static void ValidateOption<T>(
        T value,
        Func<T, bool> condition,
        string errorMessageTemplate)
    {
        if (!condition(value))
        {
            throw new ArgumentOutOfRangeException(
                paramName: null,
                errorMessageTemplate.Format(value));
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
