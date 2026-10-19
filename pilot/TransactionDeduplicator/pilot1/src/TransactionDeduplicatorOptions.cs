namespace TransactionDeduplicator.Pilot;

public sealed class TransactionDeduplicatorOptions
{
    public TimeSpan UniqnessCheckExpiration { get; set; } = TimeSpan.FromSeconds(10);

    public int UniqnessBufferCapacity { get; set; } = 1_000_000;
}
