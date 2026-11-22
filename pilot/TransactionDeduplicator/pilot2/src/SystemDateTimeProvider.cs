namespace TransactionDeduplication;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
}
