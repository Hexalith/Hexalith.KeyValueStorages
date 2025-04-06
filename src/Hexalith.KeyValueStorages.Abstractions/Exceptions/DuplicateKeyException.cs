// <copyright file="DuplicateKeyException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Exceptions;

using System;

/// <summary>
/// Exception thrown when a duplicate key is found in the key-value store.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class DuplicateKeyException<TKey> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    public DuplicateKeyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>"
    public DuplicateKeyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="key">The key that caused the exception.</param>
    public DuplicateKeyException(TKey key)
        : base($"A duplicate key was found: {key}") => Key = key;

    /// <summary>
    /// Gets the key that caused the exception.
    /// </summary>
    public TKey Key { get; } = default!;
}