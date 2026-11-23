using TransactionDeduplication.Extensions;

namespace TransactionDeduplication;

public sealed class CapacityExceededException : Exception
{
    internal CapacityExceededException(int capacity)
        : base(ValidationError.CapacityExceeded.Format(capacity))
        => Capacity = capacity;

    public int Capacity { get; }
}
