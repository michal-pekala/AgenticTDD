namespace TransactionDeduplicator.Pilot;

public sealed class TransactionDeduplicator
{
    public bool IsUniqueAndProcess(string transactionId)
    {
        ValidateTransactionId(transactionId);
        return true;
    }

    private static void ValidateTransactionId(string transactionId)
    {
        if (!Guid.TryParse(transactionId, out _))
        {
            throw new ArgumentException(
                $"TransactionId value '{transactionId}' is not a valid Guid.");
        }
    }
}
