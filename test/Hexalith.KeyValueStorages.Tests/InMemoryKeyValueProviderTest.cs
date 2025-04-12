// <copyright file="InMemoryKeyValueProviderTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using Hexalith.KeyValueStorages.InMemory;

using Microsoft.Extensions.Options;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="InMemoryKeyValueProvider"/> class.
/// </summary>
public class InMemoryKeyValueProviderTest
{
    /// <summary>
    /// Tests that the Create method creates an instance of InMemoryKeyValueStore with the correct parameters.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInMemoryKeyValueStore_WhenValidParameters()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        string database = "testdb";
        string container = "testcontainer";
        string entity = "testentity";
        var provider = new InMemoryKeyValueProvider(settings, database, container, entity, TimeProvider.System);

        // Act
        var store = provider.Create<string, State<string>>(database, container, entity);

        // Assert
        store.ShouldNotBeNull();
        store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method uses the constructor parameters when only database parameter is provided.
    /// </summary>
    [Fact]
    public void Create_ShouldUseConstructorParameters_WhenMethodParametersNotProvided()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        string database = "testdb";
        string container = "testcontainer";
        string entity = "testentity";
        var provider = new InMemoryKeyValueProvider(settings, database, container, entity, TimeProvider.System);

        // Act
        var store = provider.Create<string, State<string>>(database);

        // Assert
        store.ShouldNotBeNull();
        store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is null.
    /// </summary>
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenDatabaseIsNull()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        var provider = new InMemoryKeyValueProvider(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(null));
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is empty.
    /// </summary>
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenDatabaseIsEmpty()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        var provider = new InMemoryKeyValueProvider(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(""));
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is whitespace.
    /// </summary>
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenDatabaseIsWhitespace()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        var provider = new InMemoryKeyValueProvider(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(" "));
    }

    /// <summary>
    /// Tests that the Create method creates an instance with overridden parameters.
    /// </summary>
    [Fact]
    public void Create_ShouldUseOverriddenParameters_WhenProvided()
    {
        // Arrange
        var settings = Options.Create(new KeyValueStoreSettings());
        var provider = new InMemoryKeyValueProvider(settings, "defaultdb", "defaultcontainer", "defaultentity", TimeProvider.System);

        // Act
        var store = provider.Create<string, State<string>>("overriddendb", "overriddencontainer", "overriddenentity");

        // Assert
        store.ShouldNotBeNull();
        store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }
} 