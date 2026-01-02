// <copyright file="KeyValueStoreSettingsTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using Shouldly;

/// <summary>
/// Unit tests for the KeyValueStoreSettings class.
/// </summary>
public class KeyValueStoreSettingsTests
{
    /// <summary>
    /// Tests that the default constructor initializes properties with default values.
    /// </summary>
    [Fact]
    public void DefaultConstructorShouldInitializeWithDefaultValues()
    {
        // Act
        var settings = new KeyValueStoreSettings();

        // Assert
        settings.StorageRootPath.ShouldBe("/store");
        settings.DefaultDatabase.ShouldBe("database");
        settings.DefaultContainer.ShouldBe("container");
    }

    /// <summary>
    /// Tests that properties can be set to custom values.
    /// </summary>
    [Fact]
    public void PropertiesShouldBeSettable()
    {
        // Arrange
        var settings = new KeyValueStoreSettings();

        // Act
        settings.StorageRootPath = "/custom/path";
        settings.DefaultDatabase = "customdb";
        settings.DefaultContainer = "customcontainer";

        // Assert
        settings.StorageRootPath.ShouldBe("/custom/path");
        settings.DefaultDatabase.ShouldBe("customdb");
        settings.DefaultContainer.ShouldBe("customcontainer");
    }

    /// <summary>
    /// Tests that ConfigurationName returns the expected configuration section name.
    /// </summary>
    [Fact]
    public void ConfigurationNameShouldReturnExpectedValue()
    {
        // Act
        string result = KeyValueStoreSettings.ConfigurationName();

        // Assert
        result.ShouldBe("Hexalith:KeyValueStorages");
    }

    /// <summary>
    /// Tests that properties can be set to null values.
    /// </summary>
    [Fact]
    public void PropertiesShouldAcceptNullValues()
    {
        // Arrange
        var settings = new KeyValueStoreSettings
        {
            // Act
            StorageRootPath = null,
            DefaultDatabase = null,
            DefaultContainer = null,
        };

        // Assert
        settings.StorageRootPath.ShouldBeNull();
        settings.DefaultDatabase.ShouldBeNull();
        settings.DefaultContainer.ShouldBeNull();
    }
}
