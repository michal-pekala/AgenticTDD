namespace TransactionDeduplication.Tests.Fixture;

internal static class InvalidTransactionIds
{
    public static TheoryData<string?> All => new()
    {
        null,
        "",
        " ",
        "\t\r\n",
        "not-a-guid",
    };
}
