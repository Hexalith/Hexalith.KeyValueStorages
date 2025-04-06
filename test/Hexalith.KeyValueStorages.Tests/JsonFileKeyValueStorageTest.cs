// <copyright file="JsonFileKeyValueStorageTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Exceptions;
using Hexalith.KeyValueStorages.Files;

using Shouldly;

using Xunit;

/// <summary>
/// Unit tests for the <see cref="JsonFileKeyValueStorage{TKey, TState}"/> class.
/// </summary>
public partial class JsonFileKeyValueStorageTest : IDisposable
{
    private readonly JsonFileKeyValueStorage<string, State<DummyValue>> _storage;
    private readonly string _testDirectory;
    private readonly FakeTimeProvider _timeProvider;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorageTest"/> class.
    /// </summary>
    public JsonFileKeyValueStorageTest()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), UniqueIdHelper.GenerateDateTimeId());
        _ = Directory.CreateDirectory(_testDirectory);
        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _storage = new JsonFileKeyValueStorage<string, State<DummyValue>>(
            _testDirectory,
            new JsonSerializerOptions { WriteIndented = true },
            "DummyValues",
            key => key + ".json",
            _timeProvider);
    }

    /// <summary>
    /// Tests the AddAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsync_ShouldReturnInitialEtag_WhenValueIsAdded()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);

        // Act
        string result = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        File.Exists(Path.Combine(_testDirectory, "DummyValues", "key1.json")).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that the AddAsync method of the JsonFileKeyValueStorage class throws an exception when adding a duplicate key.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenKeyAlreadyExists()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<DuplicateKeyException<string>>(
            async () => await _storage.AddAsync("key1", state, CancellationToken.None));
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Act
        bool result = await _storage.ContainsKeyAsync("nonexistent", CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act
        bool result = await _storage.ContainsKeyAsync("key1", CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tests that expired values are automatically removed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenValueHasExpired()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Expiring",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };

        // Create a state with a TTL of 1 minute
        var state = new State<DummyValue>(dummyValue, TimeSpan.FromMinutes(1), null);
        _ = await _storage.AddAsync("expiring", state, CancellationToken.None);

        // Advance time by 2 minutes to make the value expire
        _timeProvider.AdvanceTime(TimeSpan.FromMinutes(2));

        // Act
        State<DummyValue>? result = await _storage.TryGetAsync("expiring", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        File.Exists(Path.Combine(_testDirectory, "DummyValues", "expiring")).ShouldBeFalse();
    }

    /// <summary>
    /// Tests the GetAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act
        State<DummyValue> result = await _storage.GetAsync("key1", CancellationToken.None);

        // Assert
        result.Value.Name.ShouldBe("Test");
        result.Value.Retries.ShouldBe(5);
        result.Value.Failed.ShouldBeFalse();
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Tests the GetAsync method of the JsonFileKeyValueStorage class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsync_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist() =>

        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(
            async () => await _storage.GetAsync("nonexistent", CancellationToken.None));

    /// <summary>
    /// Tests the RemoveAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Act
        bool result = await _storage.RemoveAsync("nonexistent", null, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the RemoveAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act
        bool result = await _storage.RemoveAsync("key1", null, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        File.Exists(Path.Combine(_testDirectory, "DummyValues", "key1")).ShouldBeFalse();
        (await _storage.ContainsKeyAsync("key1", CancellationToken.None)).ShouldBeFalse();
    }

    /// <summary>
    /// Tests the RemoveAsync method of the JsonFileKeyValueStorage class throws an exception when the etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsync_ShouldThrowConcurrencyException_WhenEtagDoesNotMatch()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<string>>(
            async () => await _storage.RemoveAsync("key1", "bad-etag", CancellationToken.None));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class throws an exception when the etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsync_ShouldThrowConcurrencyException_WhenEtagDoesNotMatch()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<string>>(
            async () => await _storage.SetAsync(
                "key1",
                new State<DummyValue>(dummyValue, null, "bad-etag"),
                CancellationToken.None));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsync_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await _storage.SetAsync("nonexistent", new State<DummyValue>(dummyValue, null, null), CancellationToken.None));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsync_ShouldUpdateValueAndReturnNewEtag_WhenKeyExistsAndEtagMatches()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        string etag = await _storage.AddAsync("key1", state, CancellationToken.None);

        var updatedValue = new DummyValue
        {
            Name = "Updated",
            Started = DateTimeOffset.UtcNow,
            Retries = 10,
            Failed = true,
        };

        // Act
        string newEtag = await _storage.SetAsync("key1", new State<DummyValue>(updatedValue, null, etag), CancellationToken.None);

        // Assert
        newEtag.ShouldNotBeNullOrWhiteSpace();
        newEtag.ShouldNotBe(etag);

        State<DummyValue> result = await _storage.GetAsync("key1", CancellationToken.None);
        result.Value.Name.ShouldBe("Updated");
        result.Value.Retries.ShouldBe(10);
        result.Value.Failed.ShouldBeTrue();
        result.Etag.ShouldBe(newEtag);
    }

    /// <summary>
    /// Tests the TryGetAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryGetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Act
        State<DummyValue>? result = await _storage.TryGetAsync("nonexistent", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests the TryGetAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task TryGetAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        var dummyValue = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = false,
        };
        var state = new State<DummyValue>(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act
        State<DummyValue>? result = await _storage.TryGetAsync("key1", CancellationToken.None);

        // Assert
        _ = result.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test");
        result.Value.Retries.ShouldBe(5);
        result.Value.Failed.ShouldBeFalse();
        result.Etag.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    /// <param name="disposing">Whether the method is being called from Dispose().</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    if (Directory.Exists(_testDirectory))
                    {
                        Directory.Delete(_testDirectory, true);
                    }
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }

            _disposed = true;
        }
    }
}