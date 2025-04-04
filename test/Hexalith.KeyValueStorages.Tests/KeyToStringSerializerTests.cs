// <copyright file="KeyToStringSerializerTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

using Hexalith.KeyValueStorages;

using Shouldly;

using Xunit;

/// <summary>
/// Tests for the <see cref="KeyToStringSerializer{TKey}"/> class.
/// </summary>
public class KeyToStringSerializerTests
{
    /// <summary>
    /// Verifies that serializing a custom object key uses its ToString method.
    /// </summary>
    [Fact]
    public void SerializeCustomObjectKeyShouldUseToString()
    {
        // Arrange
        KeyToStringSerializer<TestKey> serializer = new();
        TestKey key = new("CustomKeyValue");

        // Act
        string result = serializer.Serialize(key);

        // Assert
        result.ShouldBe("CustomKeyValue");
    }

    /// <summary>
    /// Verifies that serializing a custom object key with invalid characters returns an escaped string.
    /// </summary>
    [Fact]
    public void SerializeCustomObjectKeyWithInvalidCharsShouldReturnEscapedString()
    {
        // Arrange
        KeyToStringSerializer<TestKey> serializer = new();
        TestKey key = new("custom<invalid>");

        // Act
        string result = serializer.Serialize(key);

        // Assert
        result.ShouldBe("custom%3Cinvalid%3E");
    }

    /// <summary>
    /// Verifies that serializing an integer key returns its string representation.
    /// </summary>
    [Fact]
    public void SerializeIntegerKeyShouldReturnStringRepresentation()
    {
        // Arrange
        KeyToStringSerializer<int> serializer = new();
        const int key = 12345;

        // Act
        string result = serializer.Serialize(key);

        // Assert
        result.ShouldBe("12345");
    }

    /// <summary>
    /// Verifies that serializing a key that results in an empty string throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void SerializeKeyResultingInEmptyStringShouldThrowArgumentException()
    {
        // Arrange
        KeyToStringSerializer<TestKey> serializer = new();
        TestKey key = new(string.Empty);

        // Act
        Action act = () => serializer.Serialize(key);

        // Assert
        ArgumentException exception = act.ShouldThrow<ArgumentException>();
        exception.ParamName.ShouldBe("key");
    }

    /// <summary>
    /// Verifies that serializing a key that results in a null string throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void SerializeKeyResultingInNullStringShouldThrowArgumentException()
    {
        // Arrange
        KeyToStringSerializer<TestKey> serializer = new();
        TestKey key = new(null); // TestKey's ToString returns null

        // Act
        Action act = () => serializer.Serialize(key);

        // Assert
        // Convert.ToString(null) returns string.Empty which is handled by the IsNullOrWhiteSpace check
        ArgumentException exception = act.ShouldThrow<ArgumentException>();
        exception.ParamName.ShouldBe("key");
    }

    /// <summary>
    /// Verifies that serializing a key that results in a whitespace string throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void SerializeKeyResultingInWhitespaceStringShouldThrowArgumentException()
    {
        // Arrange
        KeyToStringSerializer<TestKey> serializer = new();
        TestKey key = new("   ");

        // Act
        Action act = () => serializer.Serialize(key);

        // Assert
        ArgumentException exception = act.ShouldThrow<ArgumentException>();
        exception.ParamName.ShouldBe("key");
    }

    /// <summary>
    /// Verifies that serializing string keys with invalid characters returns the correctly escaped string.
    /// </summary>
    /// <param name="key">The input key string with potentially invalid characters.</param>
    /// <param name="expected">The expected escaped string.</param>
    [Theory]
    [InlineData("key<invalid>", "key%3Cinvalid%3E")]
    [InlineData("a?b*c:d/e|g\"h", "a%3Fb%2Ac%3Ad%2Fe%7Cg%22h")]
    [InlineData(" leading", "leading")] // Leading space
    [InlineData("trailing ", "trailing")] // Trailing space
    public void SerializeStringKeyWithInvalidCharsShouldReturnEscapedString(string key, string expected)
    {
        // Arrange
        KeyToStringSerializer<string> serializer = new();

        // Act
        string result = serializer.Serialize(key);

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Verifies that serializing a string key without invalid characters returns the same string.
    /// </summary>
    [Fact]
    public void SerializeStringKeyWithoutInvalidCharsShouldReturnSameString()
    {
        // Arrange
        KeyToStringSerializer<string> serializer = new();
        const string key = "ValidKey-123_abc";

        // Act
        string result = serializer.Serialize(key);

        // Assert
        result.ShouldBe(key);
    }

    /// <summary>
    /// Represents a test key record for serialization tests.
    /// </summary>
    /// <param name="Value">The string value of the key.</param>
    private record TestKey(string? Value)
    {
        /// <inheritdoc/>
        public override string? ToString() => Value;
    }
}