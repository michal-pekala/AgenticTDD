namespace TransactionDeduplicator.Pilot;

public sealed class TransactionDeduplicator
{
    public bool IsUniqueAndProcess(string transactionId)
    {
        ParseTransactionId(transactionId);
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
