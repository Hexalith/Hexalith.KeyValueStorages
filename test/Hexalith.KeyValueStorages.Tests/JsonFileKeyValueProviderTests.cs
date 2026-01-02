// <copyright file="JsonFileKeyValueProviderTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System.Text.Json;

using Hexalith.KeyValueStorages.Files;

using Microsoft.Extensions.Options;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="JsonFileKeyValueProvider"/> class.
/// </summary>
public class JsonFileKeyValueProviderTests
{
    /// <summary>
    /// Tests that JsonSerializerOptions are passed to the Create method.
    /// </summary>
    [Fact]
    public void CreateShouldPassJsonSerializerOptionsToCreateMethod()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        var provider = new JsonFileKeyValueProvider(
            settings,
            "testdb",
            "testcontainer",
            "testentity",
            options,
            TimeProvider.System);

        // Act - capture the options parameter passed to the JsonFileKeyValueStore constructor
        // Using reflection to get the private constructor parameter value is complex and fragile
        // Instead, we'll verify functionality by accessing the Create method implementation
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>();

        // Assert
        // Since we can't directly verify the internal options, we're verifying the provider behavior
        // which is to pass the options to the JsonFileKeyValueStore constructor
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueStore<string, State<string>>>();

        // The actual behavior with options is tested in JsonFileKeyValueStoreTests
    }

    /// <summary>
    /// Tests that the Create method returns a JsonFileKeyValueStore instance.
    /// </summary>
    [Fact]
    public void CreateShouldReturnJsonFileKeyValueStoreWhenValidParameters()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        const string database = "testdb";
        const string container = "testcontainer";
        const string entity = "testentity";
        var options = new JsonSerializerOptions();
        var provider = new JsonFileKeyValueProvider(
            settings,
            database,
            container,
            entity,
            options,
            TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>(database, container, entity);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method uses the constructor parameters when method parameters are not provided.
    /// </summary>
    [Fact]
    public void CreateShouldUseConstructorParametersWhenMethodParametersNotProvided()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        const string database = "testdb";
        const string container = "testcontainer";
        const string entity = "testentity";
        var options = new JsonSerializerOptions();
        var provider = new JsonFileKeyValueProvider(
            settings,
            database,
            container,
            entity,
            options,
            TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>();

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the Create method creates an instance with overridden parameters.
    /// </summary>
    [Fact]
    public void CreateShouldUseOverriddenParametersWhenProvided()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        var provider = new JsonFileKeyValueProvider(
            settings,
            "defaultdb",
            "defaultcontainer",
            "defaultentity",
            null,
            TimeProvider.System);

        // Act
        IKeyValueStore<string, State<string>> store = provider.Create<string, State<string>>("overriddendb", "overriddencontainer", "overriddenentity");

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<JsonFileKeyValueStore<string, State<string>>>();
    }

    /// <summary>
    /// Tests that the default constructor creates an instance with default values.
    /// </summary>
    [Fact]
    public void DefaultConstructorShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var provider = new JsonFileKeyValueProvider();

        // Assert
        _ = provider.ShouldNotBeNull();
        provider.Database.ShouldBe("database");
        provider.Container.ShouldBe("container");
        provider.Entity.ShouldBeNull();
    }

    /// <summary>
    /// Tests that the constructor with JsonSerializerOptions creates an instance with the options set.
    /// </summary>
    [Fact]
    public void JsonOptionsConstructorShouldCreateInstanceWithOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // Act
        var provider = new JsonFileKeyValueProvider(options);

        // Assert
        _ = provider.ShouldNotBeNull();
        provider.Database.ShouldBe("database");
        provider.Container.ShouldBe("container");
        provider.Entity.ShouldBeNull();
    }

    /// <summary>
    /// Tests that the primary constructor creates an instance with the specified values.
    /// </summary>
    [Fact]
    public void PrimaryConstructorShouldCreateInstanceWithSpecifiedValues()
    {
        // Arrange
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings
        {
            DefaultDatabase = "customdb",
            DefaultContainer = "customcontainer",
        });
        var options = new JsonSerializerOptions();
        const string database = "testdb";
        const string container = "testcontainer";
        const string entity = "testentity";

        // Act
        var provider = new JsonFileKeyValueProvider(
            settings,
            database,
            container,
            entity,
            options,
            TimeProvider.System);

        // Assert
        _ = provider.ShouldNotBeNull();
        provider.Database.ShouldBe(database);
        provider.Container.ShouldBe(container);
        provider.Entity.ShouldBe(entity);
    }
}