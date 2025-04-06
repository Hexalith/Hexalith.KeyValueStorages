// <copyright file="ConcurrencyException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Exceptions;

using System;

/// <summary>
/// Exception thrown when a concurrency conflict occurs during a storage operation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class ConcurrencyException<TKey> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException{TKey}"/> class.
    /// </summary>
    public ConcurrencyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException{TKey}"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConcurrencyException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException{TKey}"/> class.
    /// </summary>
    /// <param name="key">The key that caused the concurrency conflict.</param>
    /// <param name="etag">The etag of the key.</param>
    /// <param name="expectedEtag">The expected etag for the key.</param>
    public ConcurrencyException(TKey key, string etag, string expectedEtag)
        : base($"The key '{key}' has an etag '{etag}' but the expected etag is '{expectedEtag}'.")
    {
        Key = key;
        Etag = etag;
        ExpectedEtag = expectedEtag;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException{TKey}"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConcurrencyException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the etag of the key that caused the concurrency conflict.
    /// </summary>
    public string? Etag { get; }

    /// <summary>
    /// Gets the expected etag for the key.
    /// </summary>
    public string? ExpectedEtag { get; }

    /// <summary>
    /// Gets the key that caused the concurrency conflict.
    /// </summary>
    public TKey? Key { get; }
}