namespace TransactionDeduplication.Tests.Fixture;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset CurrentTime { get; set; } = DateTimeOffset.UtcNow;
}
