// <copyright file="RedisKeyValueStoreSettings.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.RedisDatabase;

using System.Runtime.Serialization;

using Hexalith.Commons.Configurations;

/// <summary>
/// Settings for Redis key-value store.
/// </summary>
[DataContract]
public record RedisKeyValueStoreSettings : ISettings
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    [DataMember(Order = 1)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Redis instance name prefix.
    /// </summary>
    [DataMember(Order = 2)]
    public string? InstanceName { get; set; }

    /// <inheritdoc/>
    public static string ConfigurationName() => "KeyValueStorages:Redis";
}