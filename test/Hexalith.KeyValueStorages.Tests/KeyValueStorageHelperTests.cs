// <copyright file="KeyValueStorageHelperTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

using Hexalith.KeyValueStorages.Helpers;
using Hexalith.KeyValueStorages.InMemory;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

/// <summary>
/// Unit tests for KeyValueStorageHelper class.
/// </summary>
public class KeyValueStorageHelperTests
{
    /// <summary>
    /// Tests that the registered provider can create a store instance.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreProviderShouldCreateStore()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = "/test";
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });
        _ = services.AddMemoryKeyValueStore("test-store");
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? keyValueProvider = provider.GetKeyedService<IKeyValueProvider>("test-store");

        // Act
        IKeyValueStore<long, State<string>> store = keyValueProvider!.Create<long, State<string>>();

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<InMemoryKeyValueStore<long, State<string>>>();
    }

    /// <summary>
    /// Tests that multiple stores can be registered with different names.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldAllowMultipleRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = "/test";
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });

        // Act
        _ = services.AddMemoryKeyValueStore("store1");
        _ = services.AddMemoryKeyValueStore("store2");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store1 = provider.GetKeyedService<IKeyValueProvider>("store1");
        IKeyValueProvider? store2 = provider.GetKeyedService<IKeyValueProvider>("store2");
        _ = store1.ShouldNotBeNull();
        _ = store2.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore registers the service correctly.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = "/test";
            opt.DefaultDatabase = "testdb";
            opt.DefaultContainer = "testcontainer";
        });

        // Act
        _ = services.AddMemoryKeyValueStore("test-store");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<InMemoryKeyValueProvider>();
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore throws when name is empty.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldThrowWhenNameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddMemoryKeyValueStore(string.Empty));
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore throws when name is null.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldThrowWhenNameIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddMemoryKeyValueStore(null!));
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore throws when name is whitespace.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldThrowWhenNameIsWhitespace()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => services.AddMemoryKeyValueStore("   "));
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore throws when services is null.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => services!.AddMemoryKeyValueStore("test"));
    }

    /// <summary>
    /// Tests that AddMemoryKeyValueStore with custom parameters works.
    /// </summary>
    [Fact]
    public void AddMemoryKeyValueStoreShouldUseCustomParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.Configure<KeyValueStoreSettings>(opt =>
        {
            opt.StorageRootPath = "/test";
            opt.DefaultDatabase = "defaultdb";
            opt.DefaultContainer = "defaultcontainer";
        });

        // Act
        _ = services.AddMemoryKeyValueStore("test-store", "customdb", "customcontainer", "customentity");

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IKeyValueProvider? store = provider.GetKeyedService<IKeyValueProvider>("test-store");
        _ = store.ShouldNotBeNull();
    }
}