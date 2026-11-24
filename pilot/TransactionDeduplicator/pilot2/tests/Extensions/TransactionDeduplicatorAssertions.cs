using FluentAssertions;

namespace TransactionDeduplication.Tests.Extensions;

internal static class TransactionDeduplicatorAssertions
{
    internal static void AssertRejected(
        this TransactionDeduplicator sut,
        string? transactionId)
        => FluentActions
            .Invoking(() => sut.IsUniqueAndProcess(transactionId!))
            .Should().Throw<Exception>();

    internal static void AssertException<TException>(
        this TransactionDeduplicator sut,
        string? transactionId,
        string expectedErrorMessage)
        where TException : Exception
        => FluentActions
            .Invoking(() => sut.IsUniqueAndProcess(transactionId!))
            .Should().Throw<TException>()
            .Which.Message.Should()
            .Be(expectedErrorMessage);
}
