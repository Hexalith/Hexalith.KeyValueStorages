// <copyright file="KeyValueStoreSettings.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System.Runtime.Serialization;

using Hexalith.Commons.Configurations;

/// <summary>
/// Represents the settings for a JSON-based key-value store.
/// </summary>
[DataContract]
public class KeyValueStoreSettings : ISettings
{
    private const string _defaultContainerName = "container";

    private const string _defaultDatabaseName = "database";

    private const string _defaultStorageRootPath = "/store";

    /// <summary>
    /// Gets or sets the default container name.
    /// </summary>
    [DataMember(Order = 3)]
    public string? DefaultContainer { get; set; } = _defaultContainerName;

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    [DataMember(Order = 2)]
    public string? DefaultDatabase { get; set; } = _defaultDatabaseName;

    /// <summary>
    /// Gets or sets the root path for storing files.
    /// </summary>
    [DataMember(Order = 1)]
    public string? StorageRootPath { get; set; } = _defaultStorageRootPath;

    /// <summary>
    /// Gets the configuration name for the JSON key-value store settings.
    /// </summary>
    /// <returns>The configuration name as a string.</returns>
    public static string ConfigurationName() => $"{nameof(Hexalith)}:{nameof(KeyValueStorages)}";
}