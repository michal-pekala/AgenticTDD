using TransactionDeduplication.Extensions;

namespace TransactionDeduplication;

internal sealed class NullTransactionIdException : ArgumentNullException
{
    private readonly string _message = ValidationError.NullTransactionId.Format(
        ValidationError.TransactionIdParameter);

    public NullTransactionIdException()
        : base(ValidationError.TransactionIdParameter)
    {
    }

    public override string Message => _message;
}
