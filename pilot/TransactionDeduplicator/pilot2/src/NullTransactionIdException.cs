namespace TransactionDeduplication;

internal sealed class NullTransactionIdException : ArgumentNullException
{
    public NullTransactionIdException()
        : base("transactionId")
    {
    }

    public override string Message => ValidationError.NullTransactionId;
}
