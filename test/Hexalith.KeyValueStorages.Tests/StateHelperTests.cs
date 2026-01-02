// <copyright file="StateHelperTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;
using System.Runtime.Serialization;

using Hexalith.KeyValueStorages.Helpers;

using Shouldly;

/// <summary>
/// Unit tests for the StateHelper class.
/// </summary>
public class StateHelperTests
{
    /// <summary>
    /// Tests that GetStateName returns the DataContract name when the attribute is present.
    /// </summary>
    [Fact]
    public void GetStateNameShouldReturnDataContractNameWhenAttributeIsPresent()
    {
        // Act
        string result = StateHelper.GetStateName<TestStateWithDataContract>();

        // Assert
        result.ShouldBe("CustomStateName");
    }

    /// <summary>
    /// Tests that GetStateName returns the type name when no DataContract attribute is present.
    /// </summary>
    [Fact]
    public void GetStateNameShouldReturnTypeNameWhenNoDataContractAttribute()
    {
        // Act
        string result = StateHelper.GetStateName<TestStateWithoutDataContract>();

        // Assert
        result.ShouldBe(nameof(TestStateWithoutDataContract));
    }

    /// <summary>
    /// Tests that GetStateName returns the type name when DataContract has no Name property.
    /// </summary>
    [Fact]
    public void GetStateNameShouldReturnTypeNameWhenDataContractHasNoName()
    {
        // Act
        string result = StateHelper.GetStateName<TestStateWithDataContractNoName>();

        // Assert
        result.ShouldBe(nameof(TestStateWithDataContractNoName));
    }

    /// <summary>
    /// Tests that GetStateName works with the State generic type.
    /// </summary>
    [Fact]
    public void GetStateNameShouldReturnNameForGenericStateType()
    {
        // Act
        string result = StateHelper.GetStateName<State<string>>();

        // Assert
        result.ShouldBe("State`1");
    }

    /// <summary>
    /// A test state class with a DataContract attribute and a custom name.
    /// </summary>
    [DataContract(Name = "CustomStateName")]
    private record TestStateWithDataContract(string? Etag, TimeSpan? TimeToLive) : StateBase(Etag, TimeToLive);

    /// <summary>
    /// A test state class without a DataContract attribute.
    /// </summary>
    private record TestStateWithoutDataContract(string? Etag, TimeSpan? TimeToLive) : StateBase(Etag, TimeToLive);

    /// <summary>
    /// A test state class with a DataContract attribute but no name specified.
    /// </summary>
    [DataContract]
    private record TestStateWithDataContractNoName(string? Etag, TimeSpan? TimeToLive) : StateBase(Etag, TimeToLive);
}
