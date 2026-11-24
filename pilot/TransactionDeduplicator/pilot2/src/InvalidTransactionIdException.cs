using TransactionDeduplication.Extensions;

namespace TransactionDeduplication;

internal sealed class InvalidTransactionIdException : ArgumentException
{
    private readonly string _message;

    public InvalidTransactionIdException(string transactionId)
        : base(message: null, paramName: "transactionId")
        => _message = ValidationError.InvalidTransactionId.Format(transactionId);

    public override string Message => _message;
}
