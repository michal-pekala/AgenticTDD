namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    public TransactionDeduplicator()
    {
    }

    internal TransactionDeduplicator(
        TransactionDeduplicatorOptions options,
        IDateTimeProvider dateTimeProvider,
        TimeSpan cleanerInterval)
    {
    }

    public bool IsUniqueAndProcess(string transactionId)
        => true;
}
