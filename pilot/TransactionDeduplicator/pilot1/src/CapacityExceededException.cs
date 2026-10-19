namespace TransactionDeduplicator.Pilot;

public sealed class CapacityExceededException : Exception
{
    public CapacityExceededException(int capacity)
        : base(ValidationError.CapacityExceeded.Format(capacity))
    {
    }
}
