namespace TransactionDeduplication;

public sealed class CapacityExceededException : Exception
{
    internal CapacityExceededException(int capacity)
    {
    }

    public int Capacity { get; }
}
