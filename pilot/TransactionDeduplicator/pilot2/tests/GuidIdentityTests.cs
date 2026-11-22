using FluentAssertions;

namespace TransactionDeduplication.Tests;

public sealed class GuidIdentityTests
{
    private TransactionDeduplicator? _sut;

    private TransactionDeduplicator Sut => _sut ??= new TransactionDeduplicator();

    [Theory]
    [InlineData("D", "N")]
    [InlineData("N", "D")]
    [InlineData("D", "B")]
    [InlineData("D", "P")]
    [InlineData("D", "X")]
    [InlineData("B", "P")]
    public void Same_guid_in_another_text_format_is_a_duplicate(string firstFormat, string secondFormat)
    {
        var guid = Guid.NewGuid();

        Sut.IsUniqueAndProcess(guid.ToString(firstFormat)).Should().BeTrue();

        Sut.IsUniqueAndProcess(guid.ToString(secondFormat)).Should().BeFalse();
    }

    [Theory]
    [InlineData("N")]
    [InlineData("D")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public void Same_guid_in_other_letter_casing_is_a_duplicate(string format)
    {
        var transactionId = Guid.NewGuid().ToString(format);

        Sut.IsUniqueAndProcess(transactionId.ToLowerInvariant()).Should().BeTrue();

        Sut.IsUniqueAndProcess(transactionId.ToUpperInvariant()).Should().BeFalse();
    }
}
