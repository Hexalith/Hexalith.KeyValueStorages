// <copyright file="JsonFileKeyValueStorageTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Exceptions;
using Hexalith.KeyValueStorages.Files;

using Microsoft.Extensions.Options;

using Shouldly;

using Xunit;

/// <summary>
/// Unit tests for the <see cref="JsonFileKeyValueStore{TKey, TState}"/> class.
/// </summary>
public class JsonFileKeyValueStorageTest : IDisposable
{
    private readonly JsonFileKeyValueStore<string, State<DummyValue>> _storage;
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
        _storage = new JsonFileKeyValueStore<string, State<DummyValue>>(
            Options.Create(new KeyValueStoreSettings
            {
                StorageRootPath = _testDirectory,
                DefaultDatabase = "TestDatabase",
                DefaultContainer = "DummyValues",
            }),
            null,
            null,
            null,
            null,
            _timeProvider);
    }

    /// <summary>
    /// Tests the AddAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsyncShouldReturnInitialEtagWhenValueIsAdded()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);

        // Act
        string result = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        File.Exists(_storage.GetFilePath("key1")).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that the AddAsync method of the JsonFileKeyValueStorage class throws an exception when adding a duplicate key.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddAsyncShouldThrowInvalidOperationExceptionWhenKeyAlreadyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<DuplicateKeyException<string>>(
            async () => await _storage.AddAsync("key1", state, CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the AddOrUpdateAsync method adds a new value when key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddOrUpdateAsyncShouldAddValueWhenKeyDoesNotExist()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);

        // Act
        string result = await _storage.AddOrUpdateAsync("key1", state, CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        State<DummyValue> stored = await _storage.GetAsync("key1", CancellationToken.None);
        stored.Value.Name.ShouldBe("Test");
    }

    /// <summary>
    /// Tests the AddOrUpdateAsync method updates an existing value when key exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AddOrUpdateAsyncShouldUpdateValueWhenKeyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Original",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        string etag = await _storage.AddAsync("key1", state, CancellationToken.None);

        DummyValue updatedValue = new()
        {
            Name = "Updated",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = true,
        };

        // Act
        string result = await _storage.AddOrUpdateAsync("key1", new State<DummyValue>(updatedValue, etag, null), CancellationToken.None);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldNotBe(etag);
        State<DummyValue> stored = await _storage.GetAsync("key1", CancellationToken.None);
        stored.Value.Name.ShouldBe("Updated");
        stored.Value.Retries.ShouldBe(5);
        stored.Value.Failed.ShouldBeTrue();
    }

    /// <summary>
    /// Tests the ContainsKeyAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ContainsKeyAsyncShouldReturnFalseWhenKeyDoesNotExist()
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
    public async Task ContainsKeyAsyncShouldReturnTrueWhenKeyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
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
    /// Tests the ExistsAsync method returns false when file doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsAsyncShouldReturnFalseWhenFileDoesNotExist()
    {
        // Act
        bool result = await _storage.ExistsAsync("nonexistent", CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Tests the ExistsAsync method returns true when file exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsAsyncShouldReturnTrueWhenFileExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act
        bool result = await _storage.ExistsAsync("key1", CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that expired values are automatically removed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAsyncShouldReturnNullWhenValueHasExpired()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Expiring",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };

        // Create a state with a TTL of 1 minute
        State<DummyValue> state = new(dummyValue, null, TimeSpan.FromMinutes(1));
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
    public async Task GetAsyncShouldReturnValueWhenKeyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
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
    public async Task GetAsyncShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist() =>

        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(
            async () => await _storage.GetAsync("nonexistent", CancellationToken.None).ConfigureAwait(false));

    /// <summary>
    /// Tests the GetDirectoryPath method returns the expected path.
    /// </summary>
    [Fact]
    public void GetDirectoryPathShouldReturnExpectedPath()
    {
        // Act
        string result = _storage.GetDirectoryPath();

        // Assert
        result.ShouldContain(_testDirectory);
        result.ShouldContain("TestDatabase");
        result.ShouldContain("DummyValues");
    }

    /// <summary>
    /// Tests the GetFilePath method returns the expected path.
    /// </summary>
    [Fact]
    public void GetFilePathShouldReturnExpectedPath()
    {
        // Act
        string result = _storage.GetFilePath("testkey");

        // Assert
        result.ShouldContain("testkey.json");
        result.ShouldContain(_testDirectory);
    }

    /// <summary>
    /// Tests the RemoveAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveAsyncShouldReturnFalseWhenKeyDoesNotExist()
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
    public async Task RemoveAsyncShouldReturnTrueWhenKeyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
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
    public async Task RemoveAsyncShouldThrowConcurrencyExceptionWhenEtagDoesNotMatch()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<string>>(
            async () => await _storage.RemoveAsync("key1", "bad-etag", CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class throws an exception when the etag doesn't match.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldThrowConcurrencyExceptionWhenEtagDoesNotMatch()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        _ = await _storage.AddAsync("key1", state, CancellationToken.None);

        // Act & Assert
        _ = await Should.ThrowAsync<ConcurrencyException<string>>(
            async () => await _storage.SetAsync(
                "key1",
                new State<DummyValue>(dummyValue, "bad-etag", null),
                CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class throws an exception when the key doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };

        // Act & Assert
        _ = await Should.ThrowAsync<KeyNotFoundException>(
            async () => await _storage.SetAsync("nonexistent", new State<DummyValue>(dummyValue, null, null), CancellationToken.None).ConfigureAwait(false));
    }

    /// <summary>
    /// Tests the SetAsync method of the JsonFileKeyValueStorage class.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAsyncShouldUpdateValueAndReturnNewEtagWhenKeyExistsAndEtagMatches()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 0,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
        string etag = await _storage.AddAsync("key1", state, CancellationToken.None);

        DummyValue updatedValue = new()
        {
            Name = "Updated",
            Started = DateTimeOffset.UtcNow,
            Retries = 10,
            Failed = true,
        };

        // Act
        string newEtag = await _storage.SetAsync("key1", new State<DummyValue>(updatedValue, etag, null), CancellationToken.None);

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
    public async Task TryGetAsyncShouldReturnNullWhenKeyDoesNotExist()
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
    public async Task TryGetAsyncShouldReturnValueWhenKeyExists()
    {
        // Arrange
        DummyValue dummyValue = new()
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = false,
        };
        State<DummyValue> state = new(dummyValue, null, null);
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Not applicable")]
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