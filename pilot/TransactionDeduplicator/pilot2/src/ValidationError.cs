namespace TransactionDeduplication;

internal static class ValidationError
{
    public const string TransactionIdParameter = "transactionId";

    public const string InvalidTransactionId = "TransactionId value '{0}' is not a valid Guid. (Parameter '{1}')";

    public const string NullTransactionId = "TransactionId must not be null. (Parameter '{0}')";
}
