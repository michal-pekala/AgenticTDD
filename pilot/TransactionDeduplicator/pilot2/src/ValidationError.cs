namespace TransactionDeduplication;

internal static class ValidationError
{
    public const string InvalidTransactionId = "TransactionId value '{0}' is not a valid Guid. (Parameter 'transactionId')";

    public const string NullTransactionId = "TransactionId must not be null. (Parameter 'transactionId')";
}
