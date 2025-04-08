// <copyright file="FileKeyValueStoreSettings.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System.Runtime.Serialization;

using Hexalith.Commons.Configurations;

/// <summary>
/// Represents the settings for a JSON-based key-value store.
/// </summary>
[DataContract]
public class FileKeyValueStoreSettings : ISettings
{
    /// <summary>
    /// The default name of the database.
    /// </summary>
    public const string DefaultDatabase = "database";

    /// <summary>
    /// The default root path for storing files.
    /// </summary>
    public const string DefaultStorageRootPath = "/store";

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    [DataMember(Order = 2)]
    public string? Database { get; set; } = FileKeyValueStoreSettings.DefaultDatabase;

    /// <summary>
    /// Gets or sets the root path for storing files.
    /// </summary>
    [DataMember(Order = 1)]
    public string? StorageRootPath { get; set; } = FileKeyValueStoreSettings.DefaultStorageRootPath;

    /// <summary>
    /// Gets the configuration name for the JSON key-value store settings.
    /// </summary>
    /// <returns>The configuration name as a string.</returns>
    public static string ConfigurationName() => $"{nameof(Hexalith)}:{nameof(KeyValueStorages)}";
}