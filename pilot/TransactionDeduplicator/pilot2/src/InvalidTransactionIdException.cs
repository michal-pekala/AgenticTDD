using TransactionDeduplication.Extensions;

namespace TransactionDeduplication;

internal sealed class InvalidTransactionIdException : ArgumentException
{
    private readonly string _message;

    public InvalidTransactionIdException(string transactionId)
        : base(message: null, paramName: ValidationError.TransactionIdParameter)
        => _message = ValidationError.InvalidTransactionId.Format(
            transactionId,
            ValidationError.TransactionIdParameter);

    public override string Message => _message;
}
