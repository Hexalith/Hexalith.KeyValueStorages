// <copyright file="ExceptionTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

using Hexalith.KeyValueStorages.Exceptions;

using Shouldly;

/// <summary>
/// Unit tests for exception classes.
/// </summary>
public class ExceptionTests
{
    /// <summary>
    /// Tests the default constructor of ConcurrencyException.
    /// </summary>
    [Fact]
    public void ConcurrencyExceptionDefaultConstructorShouldCreateInstance()
    {
        // Act
        var exception = new ConcurrencyException<string>();

        // Assert
        _ = exception.ShouldNotBeNull();
        exception.Key.ShouldBeNull();
        exception.Etag.ShouldBeNull();
        exception.ExpectedEtag.ShouldBeNull();
    }

    /// <summary>
    /// Tests the key, etag, and expectedEtag constructor of ConcurrencyException.
    /// </summary>
    [Fact]
    public void ConcurrencyExceptionKeyEtagConstructorShouldSetProperties()
    {
        // Arrange
        const string key = "test-key";
        const string etag = "etag-123";
        const string expectedEtag = "expected-etag-456";

        // Act
        var exception = new ConcurrencyException<string>(key, etag, expectedEtag);

        // Assert
        exception.Key.ShouldBe(key);
        exception.Etag.ShouldBe(etag);
        exception.ExpectedEtag.ShouldBe(expectedEtag);
        exception.Message.ShouldContain(key);
        exception.Message.ShouldContain(etag);
        exception.Message.ShouldContain(expectedEtag);
    }

    /// <summary>
    /// Tests the message and inner exception constructor of ConcurrencyException.
    /// </summary>
    [Fact]
    public void ConcurrencyExceptionMessageAndInnerExceptionConstructorShouldSetProperties()
    {
        // Arrange
        const string message = "Test message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ConcurrencyException<string>(message, innerException);

        // Assert
        exception.Message.ShouldBe(message);
        exception.InnerException.ShouldBe(innerException);
    }

    /// <summary>
    /// Tests the message constructor of ConcurrencyException.
    /// </summary>
    [Fact]
    public void ConcurrencyExceptionMessageConstructorShouldSetMessage()
    {
        // Arrange
        const string message = "Test message";

        // Act
        var exception = new ConcurrencyException<string>(message);

        // Assert
        exception.Message.ShouldBe(message);
        exception.Key.ShouldBeNull();
        exception.Etag.ShouldBeNull();
        exception.ExpectedEtag.ShouldBeNull();
    }

    /// <summary>
    /// Tests ConcurrencyException with integer key type.
    /// </summary>
    [Fact]
    public void ConcurrencyExceptionWithIntegerKeyShouldWork()
    {
        // Arrange
        const int key = 42;
        const string etag = "etag-123";
        const string expectedEtag = "expected-etag-456";

        // Act
        var exception = new ConcurrencyException<int>(key, etag, expectedEtag);

        // Assert
        exception.Key.ShouldBe(key);
        exception.Etag.ShouldBe(etag);
        exception.ExpectedEtag.ShouldBe(expectedEtag);
    }

    /// <summary>
    /// Tests the default constructor of DuplicateKeyException.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionDefaultConstructorShouldCreateInstance()
    {
        // Act
        var exception = new DuplicateKeyException<string>();

        // Assert
        _ = exception.ShouldNotBeNull();
        exception.Key.ShouldBeNull();
    }

    /// <summary>
    /// Tests the key constructor of DuplicateKeyException.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionKeyConstructorShouldSetKeyAndMessage()
    {
        // Arrange
        const string key = "test-key";

        // Act
        var exception = new DuplicateKeyException<string>(key);

        // Assert
        exception.Key.ShouldBe(key);
        exception.Message.ShouldContain(key);
    }

    /// <summary>
    /// Tests the message and inner exception constructor of DuplicateKeyException.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionMessageAndInnerExceptionConstructorShouldSetProperties()
    {
        // Arrange
        const string message = "Test message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new DuplicateKeyException<string>(message, innerException);

        // Assert
        exception.Message.ShouldBe(message);
        exception.InnerException.ShouldBe(innerException);
    }

    /// <summary>
    /// Tests the message constructor of DuplicateKeyException.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionMessageConstructorShouldSetMessage()
    {
        // Arrange
        const string message = "Test message";

        // Act
        var exception = new DuplicateKeyException<string>(message);

        // Assert
        exception.Message.ShouldBe(message);
    }

    /// <summary>
    /// Tests DuplicateKeyException with integer key type.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionWithIntegerKeyShouldWork()
    {
        // Arrange
        const int key = 42;

        // Act
        var exception = new DuplicateKeyException<int>(key);

        // Assert
        exception.Key.ShouldBe(key);
        exception.Message.ShouldContain("42");
    }

    /// <summary>
    /// Tests DuplicateKeyException with long key type.
    /// </summary>
    [Fact]
    public void DuplicateKeyExceptionWithLongKeyShouldWork()
    {
        // Arrange
        const long key = 123456789L;

        // Act
        var exception = new DuplicateKeyException<long>(key);

        // Assert
        exception.Key.ShouldBe(key);
        exception.Message.ShouldContain("123456789");
    }
}