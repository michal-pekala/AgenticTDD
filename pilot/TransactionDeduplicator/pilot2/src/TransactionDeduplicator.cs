using System.Collections.Concurrent;

namespace TransactionDeduplication;

public sealed class TransactionDeduplicator
{
    private const long Removed = long.MinValue;

    private readonly long _uniquenessCheckExpirationTicks;
    private readonly int _uniquenessBufferCapacity;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConcurrentDictionary<Guid, Entry> _entries = new();
    private readonly ConcurrentDictionary<Guid, bool> _claims = new();
    private readonly Lock _sweepLock = new();
    private long _retained;

    public TransactionDeduplicator()
        : this(
            new TransactionDeduplicatorOptions(),
            new SystemDateTimeProvider(),
            Timeout.InfiniteTimeSpan)
    {
    }

    internal TransactionDeduplicator(
        TransactionDeduplicatorOptions options,
        IDateTimeProvider dateTimeProvider,
        TimeSpan cleanerInterval)
    {
        _uniquenessCheckExpirationTicks = options.UniqnessCheckExpiration.Ticks;
        _uniquenessBufferCapacity = options.UniqnessBufferCapacity;
        _dateTimeProvider = dateTimeProvider;
    }

    internal int RetainedCount => _entries.Count;

    public bool IsUniqueAndProcess(string transactionId)
    {
        var id = ParseTransactionId(transactionId);
        var now = _dateTimeProvider.CurrentTime.UtcTicks;

        while (true)
        {
            var isUnique = _entries.TryGetValue(id, out var entry)
                ? ProcessKnown(id, entry, now)
                : ProcessNew(id, now);

            if (isUnique.HasValue)
            {
                return isUnique.Value;
            }
        }
    }

    internal void SweepExpired() => SweepExpired(_dateTimeProvider.CurrentTime.UtcTicks);

    private void SweepExpired(long now)
    {
        lock (_sweepLock)
        {
            foreach (var (id, entry) in _entries)
            {
                var lastAttempt = Volatile.Read(ref entry.LastAttemptTicks);

                if (lastAttempt != Removed
                    && !IsActive(lastAttempt, now)
                    && TryUpdate(entry, lastAttempt, Removed))
                {
                    Remove(id, entry);
                }
            }
        }
    }

    // Returns null when another call changed the entry first and the caller must retry.
    private bool? ProcessKnown(
        Guid id,
        Entry entry,
        long now)
    {
        var lastAttempt = Volatile.Read(ref entry.LastAttemptTicks);

        if (lastAttempt == Removed)
        {
            Remove(id, entry);

            return null;
        }

        if (!IsActive(lastAttempt, now))
        {
            return TryUpdate(entry, lastAttempt, now) ? true : null;
        }

        if (now <= lastAttempt)
        {
            return false;
        }

        return TryUpdate(entry, lastAttempt, now) ? false : null;
    }

    // Returns null when another call holds the claim for this ID, or has just added it, and the caller must retry.
    private bool? ProcessNew(
        Guid id,
        long now)
    {
        if (!_claims.TryAdd(id, true))
        {
            Thread.Yield();

            return null;
        }

        try
        {
            if (_entries.ContainsKey(id))
            {
                return null;
            }

            AddNew(id, now);

            return true;
        }
        finally
        {
            _claims.TryRemove(id, out _);
        }
    }

    // Only the claim holder adds this ID, and it has checked that the ID is absent, so the add cannot collide.
    private void AddNew(
        Guid id,
        long now)
    {
        ReserveSlot(now);

        _entries[id] = new Entry(now);
    }

    private void ReserveSlot(long now)
    {
        if (TryReserveSlot())
        {
            return;
        }

        SweepExpired(now);

        if (!TryReserveSlot())
        {
            throw new CapacityExceededException(_uniquenessBufferCapacity);
        }
    }

    private bool TryReserveSlot()
    {
        if (Interlocked.Increment(ref _retained) <= _uniquenessBufferCapacity)
        {
            return true;
        }

        Interlocked.Decrement(ref _retained);

        return false;
    }

    private void Remove(
        Guid id,
        Entry entry)
    {
        if (_entries.TryRemove(KeyValuePair.Create(id, entry)))
        {
            Interlocked.Decrement(ref _retained);
        }
    }

    private bool TryUpdate(
        Entry entry,
        long expected,
        long value)
        => Interlocked.CompareExchange(ref entry.LastAttemptTicks, value, expected) == expected;

    private bool IsActive(
        long lastAttempt,
        long now)
        => now - lastAttempt < _uniquenessCheckExpirationTicks;

    private Guid ParseTransactionId(string transactionId)
    {
        if (transactionId is null)
        {
            throw new NullTransactionIdException();
        }

        if (!Guid.TryParse(transactionId, out var id))
        {
            throw new InvalidTransactionIdException(transactionId);
        }

        return id;
    }

    private sealed class Entry(long lastAttemptTicks)
    {
        public long LastAttemptTicks = lastAttemptTicks;
    }
}
