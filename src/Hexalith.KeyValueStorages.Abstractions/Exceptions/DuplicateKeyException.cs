// <copyright file="DuplicateKeyException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Exceptions;

using System;
using System.Runtime.Serialization;

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
    /// <param name="message"></param>
    public DuplicateKeyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public DuplicateKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="key"></param>
    public DuplicateKeyException(TKey key)
        : base($"A duplicate key was found: {key}") => Key = key;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateKeyException{TKey}"/> class.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DuplicateKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public TKey Key { get; } = default!;
}