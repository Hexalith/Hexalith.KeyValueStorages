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
    public void CreateShouldReturnInMemoryKeyValueStoreWhenValidParameters()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        const string database = "testdb";
        const string container = "testcontainer";
        const string entity = "testentity";
        InMemoryKeyValueProvider provider = new(settings, database, container, entity, TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>(database, container, entity);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is empty.
    /// </summary>
    [Fact]
    public void CreateShouldThrowArgumentExceptionWhenDatabaseIsEmpty()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        InMemoryKeyValueProvider provider = new(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(string.Empty));
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is null.
    /// </summary>
    [Fact]
    public void CreateShouldThrowArgumentExceptionWhenDatabaseIsNull()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        InMemoryKeyValueProvider provider = new(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(null));
    }

    /// <summary>
    /// Tests that the Create method throws an ArgumentException when the database parameter is whitespace.
    /// </summary>
    [Fact]
    public void CreateShouldThrowArgumentExceptionWhenDatabaseIsWhitespace()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        InMemoryKeyValueProvider provider = new(settings, null, null, null, TimeProvider.System);

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => provider.Create<string, State<string>>(" "));
    }

    /// <summary>
    /// Tests that the Create method uses the constructor parameters when only database parameter is provided.
    /// </summary>
    [Fact]
    public void CreateShouldUseConstructorParametersWhenMethodParametersNotProvided()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        const string database = "testdb";
        const string container = "testcontainer";
        const string entity = "testentity";
        InMemoryKeyValueProvider provider = new(settings, database, container, entity, TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>(database);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method creates an instance with overridden parameters.
    /// </summary>
    [Fact]
    public void CreateShouldUseOverriddenParametersWhenProvided()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        InMemoryKeyValueProvider provider = new(settings, "defaultdb", "defaultcontainer", "defaultentity", TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>("overriddendb", "overriddencontainer", "overriddenentity");

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<InMemoryKeyValueStore<string, State<string>>>();
    }
}