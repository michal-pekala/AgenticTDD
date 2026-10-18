namespace TransactionDeduplicator.Pilot;

public static class ValidationError
{
    public const string InvalidTransactionId =
        "TransactionId value '{0}' is not a valid Guid.";

    public const string InvalidUniqnessCheckExpiration =
        "UniqnessCheckExpiration value '{0}' must be greater than zero.";

    public const string InvalidUniqnessBufferCapacity =
        "UniqnessBufferCapacity value '{0}' must be greater than zero.";

    public const string CapacityExceeded =
        "Transaction buffer capacity of {0} has been reached.";
}
