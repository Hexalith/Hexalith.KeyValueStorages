// <copyright file="JsonKeyValueStorageHelperTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;
using System.IO;
using System.Text.Json;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Files;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

/// <summary>
/// Unit tests for JsonKeyValueStorageHelper class.
/// </summary>
public class JsonKeyValueStorageHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonKeyValueStorageHelperTests"/> class.
    /// </summary>
    public JsonKeyValueStorageHelperTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), UniqueIdHelper.GenerateDateTimeId());
        _ = Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Tests that the registered provider can create a store instance.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreProviderShouldCreateStore()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });
        _ = services.AddJsonFileKeyValueStore("test-store");
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? keyValueProvider = provider.GetKeyedService<IKeyValueProvider>("test-store");

        // Act
        IKeyValueStore<string, State<DummyValue>> store = keyValueProvider!.Create<string, State<DummyValue>>();

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueStore<string, State<DummyValue>>>();
    }

    /// <summary>
    /// Tests that multiple stores can be registered with different names.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldAllowMultipleRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });

        // Act
        _ = services.AddJsonFileKeyValueStore("store1");
        _ = services.AddJsonFileKeyValueStore("store2");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store1 = provider.GetKeyedService<IKeyValueProvider>("store1");
        IKeyValueProvider? store2 = provider.GetKeyedService<IKeyValueProvider>("store2");
        _ = store1.ShouldNotBeNull();
        _ = store2.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore registers the service correctly.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });

        // Act
        _ = services.AddJsonFileKeyValueStore("test-store");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueProvider>();
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore throws when name is empty.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldThrowWhenNameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddJsonFileKeyValueStore(string.Empty));
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore throws when name is null.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldThrowWhenNameIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddJsonFileKeyValueStore(null!));
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore throws when services is null.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => services!.AddJsonFileKeyValueStore("test"));
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore with JsonSerializerOptions works.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldUseCustomJsonSerializerOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        _ = services.AddJsonFileKeyValueStore("test-store", options: options);

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddJsonFileKeyValueStore with custom parameters works.
    /// </summary>
    [Fact]
    public void AddJsonFileKeyValueStoreShouldUseCustomParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "defaultdb";
            opt.DefaultContainer = "defaultcontainer";
        });

        // Act
        _ = services.AddJsonFileKeyValueStore("test-store", "customdb", "customcontainer", "customentity");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddPolymorphicJsonFileKeyValueStore registers the service correctly.
    /// </summary>
    [Fact]
    public void AddPolymorphicJsonFileKeyValueStoreShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = _testDirectory;
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });

        // Act
        _ = services.AddPolymorphicJsonFileKeyValueStore("test-store");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueProvider>();
    }

    /// <summary>
    /// Tests that AddPolymorphicJsonFileKeyValueStore throws when name is null.
    /// </summary>
    [Fact]
    public void AddPolymorphicJsonFileKeyValueStoreShouldThrowWhenNameIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddPolymorphicJsonFileKeyValueStore(null!));
    }

    /// <summary>
    /// Tests that AddPolymorphicJsonFileKeyValueStore throws when services is null.
    /// </summary>
    [Fact]
    public void AddPolymorphicJsonFileKeyValueStoreShouldThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => services!.AddPolymorphicJsonFileKeyValueStore("test"));
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