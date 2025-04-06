// <copyright file="InMemoryKeyValueStoreTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using Hexalith.KeyValueStorages.Exceptions;

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
    public async Task AddAsync_ShouldReturnInitialEtag_WhenValueIsAdded()
    {
        // Arrange
        var value = new InMemoryKeyValueStore<long, State<long>>();

        // Act
        string result = await value.AddAsync(
            1L,
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
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenKeyAlreadyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<DuplicateKeyException<long>>(
            async () => await store.AddAsync(1, new State<long>(200L, null, null), CancellationToken.None));
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

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
    public async Task ContainsKeyAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
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
    public async Task GetAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
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
    public async Task GetAsync_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await store.GetAsync(1, CancellationToken.None));
    }

    /// <summary>
    /// Tests the RemoveAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

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
    public async Task RemoveAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
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
    public async Task SetAsync_ShouldThrowInvalidOperationException_WhenEtagDoesNotMatch()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<long>>(
            async () => await store.SetAsync(
                1,
                new State<long>(200, null, "bad etag"),
                CancellationToken.None));
    }

    /// <summary>
    /// Tests the SetAsync method of the InMemoryKeyValueStore class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsync_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await store.SetAsync(1, new State<long>(100L, null, null), CancellationToken.None));
    }

    /// <summary>
    /// Tests the SetAsync method of the InMemoryKeyValueStore class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsync_ShouldUpdateValueAndReturnNewEtag_WhenKeyExistsAndEtagMatches()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
        string etag = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        string newEtag = await store.SetAsync(1, new State<long>(200, null, etag), CancellationToken.None);

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
    public async Task TryGetValueAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();

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
    public async Task TryGetValueAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        var store = new InMemoryKeyValueStore<long, State<long>>();
        _ = await store.AddAsync(1, new State<long>(100L, null, null), CancellationToken.None);

        // Act
        State<long>? result = await store.TryGetAsync(1, CancellationToken.None);

        // Assert
        _ = result.ShouldNotBeNull();
        result.Value.ShouldBe(100);
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }
}