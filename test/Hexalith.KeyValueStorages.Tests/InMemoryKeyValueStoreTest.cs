// <copyright file="InMemoryKeyValueStoreTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Exceptions;
using Hexalith.KeyValueStorages.InMemory;

using Shouldly;

/// <summary>
/// Unit tests for the InMemoryKeyValueStore class.
/// </summary>
public class InMemoryKeyValueStoreTest
{
    /// <summary>
    /// Tests the AddAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsyncShouldReturnInitialEtagWhenValueIsAdded()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        string result = await store.AddAsync(
            1,
            new State<long>(100L, null, null),
            CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests that the AddAsync method of the InMemoryKeyValueStore class throws an exception when adding a duplicate key.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsyncShouldThrowInvalidOperationExceptionWhenKeyAlreadyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<DuplicateKeyException<long>>(
            async () => await store.AddAsync(1, new State<long>(200L, null, null), CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsyncShouldReturnFalseWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        bool result = await store.ContainsKeyAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsyncShouldReturnTrueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        bool result = await store.ContainsKeyAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Tests the GetAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsyncShouldReturnValueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        State<long> result = await store.GetAsync(1, CancellationToken.None);

        // Assert
        result.Value.ShouldBe(100);
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests the GetAsync method of the InMemoryKeyValueStore class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsyncShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await store.GetAsync(1, CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the RemoveAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsyncShouldReturnFalseWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        bool result = await store.RemoveAsync(1, null, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the RemoveAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsyncShouldReturnTrueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        bool result = await store.RemoveAsync(1, null, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        (await store.ContainsKeyAsync(1, CancellationToken.None)).ShouldBeFalse();
    }

    /// <summary>
    /// Tests the SetAsync method of the InMemoryKeyValueStore class throws an exception when the etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldThrowInvalidOperationExceptionWhenEtagDoesNotMatch()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<long>>(
            async () => await store.SetAsync(
                1,
                new State<long>(200, "bad etag", null),
                CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the SetAsync method of the InMemoryKeyValueStore class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await store.SetAsync(1, new State<long>(100L, null, null), CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the SetAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldUpdateValueAndReturnNewEtagWhenKeyExistsAndEtagMatches()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        string etag = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        string newEtag = await store.SetAsync(1, new State<long>(200, etag, null), CancellationToken.None);

        // Assert
        newEtag.ShouldNotBeNullOrWhiteSpace();
        newEtag.ShouldNotBe(etag);
        State<long> result = await store.GetAsync(1, CancellationToken.None);
        result.Value.ShouldBe(200);
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests the TryGetValueAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryGetValueAsyncShouldReturnNullWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        State<long>? result = await store.TryGetAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests the TryGetValueAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryGetValueAsyncShouldReturnValueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        State<long>? result = await store.TryGetAsync(1, CancellationToken.None);

        // Assert
        _ = result.ShouldNotBeNull();
        result.Value.ShouldBe(100);
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests the ExistsAsync method of the InMemoryKeyValueStore class returns false when key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsAsyncShouldReturnFalseWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        bool result = await store.ExistsAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the ExistsAsync method of the InMemoryKeyValueStore class returns true when key exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsAsyncShouldReturnTrueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        bool result = await store.ExistsAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Tests the AddOrUpdateAsync method adds a new value when key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddOrUpdateAsyncShouldAddValueWhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);

        // Act
        string result = await store.AddOrUpdateAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        State<long> value = await store.GetAsync(1, CancellationToken.None);
        value.Value.ShouldBe(100L);
    }

    /// <summary>
    /// Tests the AddOrUpdateAsync method updates an existing value when key exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddOrUpdateAsyncShouldUpdateValueWhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        string result = await store.AddOrUpdateAsync(1, new State<long>(200L, null, null), CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        State<long> value = await store.GetAsync(1, CancellationToken.None);
        value.Value.ShouldBe(200L);
    }

    /// <summary>
    /// Tests the AddOrUpdateAsync method throws ConcurrencyException when etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddOrUpdateAsyncShouldThrowConcurrencyExceptionWhenEtagDoesNotMatch()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<long>>(
            async () => await store.AddOrUpdateAsync(1, new State<long>(200L, "bad-etag", null), CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the RemoveAsync method throws ConcurrencyException when etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsyncShouldThrowConcurrencyExceptionWhenEtagDoesNotMatch()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>(
            UniqueIdHelper.GenerateUniqueStringId(),
            UniqueIdHelper.GenerateUniqueStringId(),
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<long>>(
            async () => await store.RemoveAsync(1, "bad-etag", CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the Clear method removes all entries for the current database and container.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ClearShouldRemoveAllEntriesForDatabaseAndContainer()
    {
        // Arrange
        string database = UniqueIdHelper.GenerateUniqueStringId();
        string container = UniqueIdHelper.GenerateUniqueStringId();
        var store = new InMemoryKeyValueStore<long, State<long>>(
            database,
            container,
            TimeProvider.System);
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);
        _ = await store.AddAsync(2, new State<long>(200L, null, null), CancellationToken.None);
        _ = await store.AddAsync(3, new State<long>(300L, null, null), CancellationToken.None);

        // Act
        store.Clear();

        // Assert
        (await store.ContainsKeyAsync(1, CancellationToken.None)).ShouldBeFalse();
        (await store.ContainsKeyAsync(2, CancellationToken.None)).ShouldBeFalse();
        (await store.ContainsKeyAsync(3, CancellationToken.None)).ShouldBeFalse();
    }

    /// <summary>
    /// Tests the TimeToLiveCleanup method removes expired entries.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TimeToLiveCleanupShouldRemoveExpiredEntries()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        string database = UniqueIdHelper.GenerateUniqueStringId();
        string container = UniqueIdHelper.GenerateUniqueStringId();
        var store = new InMemoryKeyValueStore<long, State<long>>(
            database,
            container,
            timeProvider);

        // Add a value with a 1 minute TTL
        _ = await store.AddAsync(1, new State<long>(100L, null, TimeSpan.FromMinutes(1)), CancellationToken.None);
        // Add a value without TTL
        _ = await store.AddAsync(2, new State<long>(200L, null, null), CancellationToken.None);

        // Advance time by 2 minutes
        timeProvider.AdvanceTime(TimeSpan.FromMinutes(2));

        // Act
        store.TimeToLiveCleanup();

        // Assert
        (await store.ContainsKeyAsync(1, CancellationToken.None)).ShouldBeFalse();
        (await store.ContainsKeyAsync(2, CancellationToken.None)).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that expired values are automatically removed on access.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsyncShouldReturnKeyNotFoundExceptionWhenValueHasExpired()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        string database = UniqueIdHelper.GenerateUniqueStringId();
        string container = UniqueIdHelper.GenerateUniqueStringId();
        var store = new InMemoryKeyValueStore<long, State<long>>(
            database,
            container,
            timeProvider);

        // Add a value with a 1 minute TTL
        _ = await store.AddAsync(1, new State<long>(100L, null, TimeSpan.FromMinutes(1)), CancellationToken.None);

        // Advance time by 2 minutes
        timeProvider.AdvanceTime(TimeSpan.FromMinutes(2));

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await store.GetAsync(1, CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests that TryGetAsync returns null when value has expired.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryGetAsyncShouldReturnNullWhenValueHasExpired()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        string database = UniqueIdHelper.GenerateUniqueStringId();
        string container = UniqueIdHelper.GenerateUniqueStringId();
        var store = new InMemoryKeyValueStore<long, State<long>>(
            database,
            container,
            timeProvider);

        // Add a value with a 1 minute TTL
        _ = await store.AddAsync(1, new State<long>(100L, null, TimeSpan.FromMinutes(1)), CancellationToken.None);

        // Advance time by 2 minutes
        timeProvider.AdvanceTime(TimeSpan.FromMinutes(2));

        // Act
        State<long>? result = await store.TryGetAsync(1, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}