// <copyright file="DummyClassTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.MyNewPackage.Tests;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="DummyClass"/> class.
/// </summary>
public class DummyClassTest
{
    /// <summary>
    /// Tests that the SampleValue property of DummyClass is true.
    /// </summary>
    [Fact]
    public void Test1()
    {
        // Arrange
        var value = new DummyClass("test");

        // Act
        string result = value.SampleValue;

        // Assert
        result.ShouldBe("test");
    }
}