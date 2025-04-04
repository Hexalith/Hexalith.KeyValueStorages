// <copyright file="KeyToStringSerializer{TKey}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System.Globalization;
using System.Linq;

/// <summary>
/// Provides functionality to serialize a key to a string.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class KeyToStringSerializer<TKey> : IKeySerializer<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Serializes the specified key to a string.
    /// </summary>
    /// <param name="key">The key to serialize.</param>
    /// <returns>A string representation of the key.</returns>
    /// <exception cref="ArgumentException">Thrown when the key cannot be converted to a string.</exception>
    public string Serialize(TKey key)
    {
        string? result = Convert.ToString(key, CultureInfo.InvariantCulture)?.Trim();
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException($"The key '{key}' cannot be converted to a string.", nameof(key));
        }

        // Ensure the result is file name compatible. Check if there are any invalid characters (Path.GetInvalidFileNameChars())
        // If the file name contains invalid characters, escape them using url encoding.
        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (result.IndexOfAny(invalidChars) >= 0)
        {
            // Replace each invalid character with its URL-encoded equivalent
            foreach (char invalidChar in invalidChars.Where(invalidChar => result.Contains(invalidChar)))
            {
                // Convert the char to its hexadecimal representation for URL encoding
                string encodedChar = Uri.EscapeDataString(invalidChar.ToString());
                result = result.Replace(invalidChar.ToString(), encodedChar);
            }
        }

        if (result.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException($"The key '{result}' contains invalid characters for a file name.", nameof(key));
        }

        return result;
    }
}