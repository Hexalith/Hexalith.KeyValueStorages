// <copyright file="ProviderCapabilities.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// CONTRACT DEFINITION - New file
// Target: src/libraries/Hexalith.KeyValueStorages.Abstractions/ProviderCapabilities.cs

namespace Hexalith.KeyValueStorages;

using System.Runtime.Serialization;

/// <summary>
/// Describes the capabilities of a key-value store provider.
/// </summary>
/// <param name="SupportsAcidTransactions">
/// Indicates whether the provider supports full ACID transactions for batch operations.
/// When true, batch operations use native provider transactions with isolation guarantees.
/// When false, batch operations use compensating transactions (best-effort atomicity).
/// </param>
/// <param name="SupportsBatchOperations">
/// Indicates whether the provider supports batch operations.
/// All providers in the Hexalith.KeyValueStorages library support batch operations.
/// </param>
/// <param name="MaxBatchSize">
/// The maximum number of items allowed in a single batch operation.
/// Null indicates no provider-imposed limit; the library default of 1000 applies.
/// Values must be greater than zero when specified.
/// </param>
/// <remarks>
/// <para>
/// Use this record to query provider capabilities before executing batch operations.
/// This allows callers to make informed decisions about retry strategies and
/// consistency expectations.
/// </para>
/// <para>
/// Provider capability values:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Provider</term>
/// <description>SupportsAcidTransactions</description>
/// </listheader>
/// <item>
/// <term>InMemory</term>
/// <description>false (uses lock-based atomicity)</description>
/// </item>
/// <item>
/// <term>JsonFile</term>
/// <description>false (uses temp directory + atomic move)</description>
/// </item>
/// <item>
/// <term>Redis</term>
/// <description>true (uses MULTI/EXEC transactions)</description>
/// </item>
/// <item>
/// <term>DaprActor</term>
/// <description>true (uses Dapr state transactions)</description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// if (store.Capabilities.SupportsAcidTransactions)
/// {
///     // Full ACID guarantees - safe for concurrent access
///     await store.AddRangeAsync(items, cancellationToken);
/// }
/// else
/// {
///     // Best-effort atomicity - consider additional coordination
///     await store.AddRangeAsync(items, cancellationToken);
/// }
/// </code>
/// </example>
[DataContract]
public sealed record ProviderCapabilities(
    [property: DataMember(Order = 1)] bool SupportsAcidTransactions,
    [property: DataMember(Order = 2)] bool SupportsBatchOperations,
    [property: DataMember(Order = 3)] int? MaxBatchSize)
{
    /// <summary>
    /// Gets the default capabilities for providers without ACID transaction support.
    /// </summary>
    public static ProviderCapabilities BestEffort { get; } = new(
        SupportsAcidTransactions: false,
        SupportsBatchOperations: true,
        MaxBatchSize: null);

    /// <summary>
    /// Gets the default capabilities for providers with full ACID transaction support.
    /// </summary>
    public static ProviderCapabilities FullAcid { get; } = new(
        SupportsAcidTransactions: true,
        SupportsBatchOperations: true,
        MaxBatchSize: null);
}
