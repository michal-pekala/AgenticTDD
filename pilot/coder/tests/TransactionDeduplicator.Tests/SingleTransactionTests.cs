using FluentAssertions;
using FluentAssertions.Extensions;
using TransactionDeduplicator.Pilot;
using Deduplicator = TransactionDeduplicator.Pilot.TransactionDeduplicator;
using Error = TransactionDeduplicator.Pilot.ValidationError;
using TransactionDeduplicator.Tests.Fixture;

namespace TransactionDeduplicator.Tests;

public sealed class SingleTransactionTests
{
    private readonly FakeDateTimeProvider _dateTimeProvider = new();
    private readonly TransactionDeduplicatorOptions _options = new();
    private Deduplicator? _sut;

    private Deduplicator Sut
        => _sut ??= new Deduplicator(_dateTimeProvider, _options);

    private DateTimeOffset Now
    {
        get => _dateTimeProvider.Now;
        set => _dateTimeProvider.Now = value;
    }

    [Fact]
    public void New_transaction_can_be_processed()
        => Sut
            .IsUniqueAndProcess(Guid.NewGuid().ToString())
            .Should().BeTrue();

    [Fact]
    public void Malformed_transaction_id_is_rejected()
    {
        var invalidId = $"not-a-guid-{Guid.NewGuid()}";

        FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(invalidId))
            .Should().Throw<ArgumentException>()
            .WithMessage(Error.InvalidTransactionId.Format(invalidId));
    }

    [Fact]
    public void Active_transaction_duplicate_is_rejected()
    {
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeTrue();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeFalse();
    }

    [Fact]
    public void Active_transaction_duplicate_is_rejected_within_expiration_window()
    {
        _options.UniqnessCheckExpiration = Random.Shared.Next(2, 30).Seconds();
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeTrue();

        Now += _options.UniqnessCheckExpiration - 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeFalse();
    }

    [Fact]
    public void Duplicate_attempt_refreshes_expiration()
    {
        _options.UniqnessCheckExpiration = Random.Shared.Next(2, 30).Seconds();
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeTrue();

        Now += _options.UniqnessCheckExpiration / 2;

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeFalse();

        Now += _options.UniqnessCheckExpiration - 1.Milliseconds();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeFalse();
    }

    [Fact]
    public void Expired_transaction_can_be_processed_again()
    {
        _options.UniqnessCheckExpiration = Random.Shared.Next(2, 30).Seconds();
        var transactionId = Guid.NewGuid().ToString();

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeTrue();

        Now += _options.UniqnessCheckExpiration;

        Sut.IsUniqueAndProcess(transactionId)
            .Should().BeTrue();
    }

    [Fact]
    public void Non_positive_expiration_is_rejected()
    {
        _options.UniqnessCheckExpiration = TimeSpan.Zero;

        FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()))
            .Should().Throw<ArgumentOutOfRangeException>()
            .Which.Message.Should()
            .Be(Error.InvalidUniqnessCheckExpiration.Format(_options.UniqnessCheckExpiration));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_capacity_is_rejected(int capacity)
    {
        _options.UniqnessBufferCapacity = capacity;

        FluentActions
            .Invoking(() => Sut.IsUniqueAndProcess(Guid.NewGuid().ToString()))
            .Should().Throw<ArgumentOutOfRangeException>()
            .Which.Message.Should()
            .Be(Error.InvalidUniqnessBufferCapacity.Format(_options.UniqnessBufferCapacity));
    }
}
