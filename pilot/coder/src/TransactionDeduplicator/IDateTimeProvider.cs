namespace TransactionDeduplicator.Pilot;

internal interface IDateTimeProvider
{
    DateTimeOffset CurrentTime { get; }
}

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
}
