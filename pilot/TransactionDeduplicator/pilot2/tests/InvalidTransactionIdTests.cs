using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplication.Extensions;
using TransactionDeduplication.Tests.Extensions;
using TransactionDeduplication.Tests.Fixture;
using Error = TransactionDeduplication.ValidationError;

namespace TransactionDeduplication.Tests;

public sealed class InvalidTransactionIdTests : ExpirationTestsBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("not-a-guid")]
    public void Invalid_transaction_id_is_rejected(string? invalidId)
        => Sut
            .AssertRejected(invalidId);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("not-a-guid")]
    public void Invalid_transaction_id_throws_ArgumentException(string? invalidId)
        => FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(invalidId!))
            .Should().Throw<ArgumentException>();

    [Fact]
    public void Malformed_transaction_id_error_message_names_the_value()
    {
        var invalidId = $"not-a-guid-{Guid.NewGuid()}";

        Sut.AssertException<ArgumentException>(invalidId, Error.InvalidTransactionId.Format(invalidId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("not-a-guid")]
    public void Active_id_is_still_a_duplicate_after_invalid_id_is_rejected(string? invalidId)
    {
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Sut.AssertRejected(invalidId);

        Sut.IsUniqueAndProcess(transactionId).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("not-a-guid")]
    public void Rejected_invalid_id_does_not_restart_expiration_of_active_id(string? invalidId)
    {
        var transactionId = Guid.NewGuid().ToString();
        var expiration = Options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();

        Now += expiration - 1.Milliseconds();

        Sut.AssertRejected(invalidId);

        Now += 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("not-a-guid")]
    public void Empty_guid_is_still_unseen_after_invalid_id_is_rejected(string? invalidId)
    {
        Sut.AssertRejected(invalidId);

        Sut.IsUniqueAndProcess(Guid.Empty.ToString()).Should().BeTrue();
    }
}
