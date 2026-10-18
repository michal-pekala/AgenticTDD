using TransactionDeduplicator.Pilot;

namespace TransactionDeduplicator.Tests.Fixture;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.UnixEpoch;

    public DateTimeOffset CurrentTime => Now;
}
