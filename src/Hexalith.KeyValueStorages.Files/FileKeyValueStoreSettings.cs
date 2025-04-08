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
/// <param name="StorageRootPath">The root path for storing files.</param>
/// <param name="Database">The name of the database.</param>
[DataContract]
public record FileKeyValueStoreSettings(
   [property: DataMember(Order = 1)] string? StorageRootPath = FileKeyValueStoreSettings.DefaultStorageRootPath,
   [property: DataMember(Order = 2)] string? Database = FileKeyValueStoreSettings.DefaultDatabase) : ISettings
{
    /// <summary>
    /// The default root path for storing files.
    /// </summary>
    public const string DefaultStorageRootPath = "/store";

    /// <summary>
    /// The default name of the database.
    /// </summary>
    public const string DefaultDatabase = "database";

    /// <summary>
    /// Gets the configuration name for the JSON key-value store settings.
    /// </summary>
    /// <returns>The configuration name as a string.</returns>
    public static string ConfigurationName() => $"{nameof(Hexalith)}:{nameof(KeyValueStorages)}";
}