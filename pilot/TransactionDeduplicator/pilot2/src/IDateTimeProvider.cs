namespace TransactionDeduplication;

internal interface IDateTimeProvider
{
    DateTimeOffset CurrentTime { get; }
}
