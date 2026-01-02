// <copyright file="StateTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

using Shouldly;

/// <summary>
/// Unit tests for State and StateBase classes.
/// </summary>
public class StateTests
{
    /// <summary>
    /// Tests StateBase constructor with all parameters.
    /// </summary>
    [Fact]
    public void StateBaseConstructorShouldSetAllProperties()
    {
        // Arrange
        const string etag = "test-etag";
        TimeSpan ttl = TimeSpan.FromMinutes(5);

        // Act
        var state = new TestState(etag, ttl);

        // Assert
        state.Etag.ShouldBe(etag);
        state.TimeToLive.ShouldBe(ttl);
    }

    /// <summary>
    /// Tests StateBase constructor with null values.
    /// </summary>
    [Fact]
    public void StateBaseConstructorShouldAcceptNullValues()
    {
        // Act
        var state = new TestState(null, null);

        // Assert
        state.Etag.ShouldBeNull();
        state.TimeToLive.ShouldBeNull();
    }

    /// <summary>
    /// Tests State constructor with value and default optional parameters.
    /// </summary>
    [Fact]
    public void StateConstructorShouldSetValueWithDefaults()
    {
        // Arrange
        const string value = "test-value";

        // Act
        var state = new State<string>(value);

        // Assert
        state.Value.ShouldBe(value);
        state.Etag.ShouldBeNull();
        state.TimeToLive.ShouldBeNull();
    }

    /// <summary>
    /// Tests State constructor with all parameters.
    /// </summary>
    [Fact]
    public void StateConstructorShouldSetAllProperties()
    {
        // Arrange
        const string value = "test-value";
        const string etag = "test-etag";
        TimeSpan ttl = TimeSpan.FromMinutes(5);

        // Act
        var state = new State<string>(value, etag, ttl);

        // Assert
        state.Value.ShouldBe(value);
        state.Etag.ShouldBe(etag);
        state.TimeToLive.ShouldBe(ttl);
    }

    /// <summary>
    /// Tests State with different value types.
    /// </summary>
    [Fact]
    public void StateWithIntegerValueShouldWork()
    {
        // Act
        var state = new State<int>(42, "etag", null);

        // Assert
        state.Value.ShouldBe(42);
        state.Etag.ShouldBe("etag");
    }

    /// <summary>
    /// Tests State with complex object value.
    /// </summary>
    [Fact]
    public void StateWithComplexObjectValueShouldWork()
    {
        // Arrange
        var dummy = new DummyValue
        {
            Name = "Test",
            Started = DateTimeOffset.UtcNow,
            Retries = 5,
            Failed = false,
        };

        // Act
        var state = new State<DummyValue>(dummy, "etag", TimeSpan.FromHours(1));

        // Assert
        state.Value.Name.ShouldBe("Test");
        state.Value.Retries.ShouldBe(5);
        state.Etag.ShouldBe("etag");
        state.TimeToLive.ShouldBe(TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Tests State record with expression to create modified copy.
    /// </summary>
    [Fact]
    public void StateWithExpressionShouldCreateModifiedCopy()
    {
        // Arrange
        var original = new State<string>("value", "old-etag", null);

        // Act
        var modified = original with { Etag = "new-etag" };

        // Assert
        modified.Value.ShouldBe("value");
        modified.Etag.ShouldBe("new-etag");
        original.Etag.ShouldBe("old-etag");
    }

    /// <summary>
    /// Tests State record equality.
    /// </summary>
    [Fact]
    public void StateEqualityShouldCompareAllProperties()
    {
        // Arrange
        var state1 = new State<string>("value", "etag", TimeSpan.FromMinutes(5));
        var state2 = new State<string>("value", "etag", TimeSpan.FromMinutes(5));
        var state3 = new State<string>("value", "different-etag", TimeSpan.FromMinutes(5));

        // Assert
        state1.ShouldBe(state2);
        state1.ShouldNotBe(state3);
    }

    /// <summary>
    /// Tests State with nullable value type.
    /// </summary>
    [Fact]
    public void StateWithNullableValueTypeShouldWork()
    {
        // Act
        var state = new State<int?>(null, "etag", null);

        // Assert
        state.Value.ShouldBeNull();
    }

    /// <summary>
    /// A test state record that inherits from StateBase.
    /// </summary>
    private record TestState(string? Etag, TimeSpan? TimeToLive) : StateBase(Etag, TimeToLive);
}
