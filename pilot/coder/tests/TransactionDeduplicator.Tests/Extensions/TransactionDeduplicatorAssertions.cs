using FluentAssertions;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;

namespace TransactionDeduplicator.Tests.Extensions;

internal static class TransactionDeduplicatorAssertions
{
    internal static void AssertException<TException>(
        this Deduplicator sut,
        string transactionId,
        string expectedErrorMessage)
        where TException : Exception
        => FluentActions
            .Invoking(() => sut.IsUniqueAndProcess(transactionId))
            .Should().Throw<TException>()
            .Which.Message.Should()
            .Be(expectedErrorMessage);
}
