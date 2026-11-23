using TransactionDeduplication.Tests.Fixture;

namespace TransactionDeduplication.Tests.Extensions;

internal static class TransactionDeduplicatorOutcomes
{
    internal static CallOutcome Attempt(
        this TransactionDeduplicator sut,
        string transactionId)
    {
        try
        {
            return new CallOutcome(sut.IsUniqueAndProcess(transactionId), null);
        }
        catch (Exception exception)
        {
            return new CallOutcome(null, exception);
        }
    }
}
