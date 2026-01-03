// <copyright file="StateHelperTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System.Runtime.Serialization;

using Hexalith.KeyValueStorages.Helpers;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="StateHelper"/> class.
/// </summary>
public partial class StateHelperTests
{
    /// <summary>
    /// Tests that <see cref="StateHelper.GetStateName{TState}"/> returns the <see cref="DataContractAttribute.Name"/>
    /// value when the attribute is present and a name is specified.
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
    /// Tests that <see cref="StateHelper.GetStateName{TState}"/> returns the CLR type name for a closed generic type.
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
    /// Tests that <see cref="StateHelper.GetStateName{TState}"/> returns the CLR type name when the
    /// <see cref="DataContractAttribute"/> is present but <see cref="DataContractAttribute.Name"/> is not specified.
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
    /// Tests that <see cref="StateHelper.GetStateName{TState}"/> returns the CLR type name when no
    /// <see cref="DataContractAttribute"/> is present.
    /// </summary>
    [Fact]
    public void GetStateNameShouldReturnTypeNameWhenNoDataContractAttribute()
    {
        // Act
        string result = StateHelper.GetStateName<TestStateWithoutDataContract>();

        // Assert
        result.ShouldBe(nameof(TestStateWithoutDataContract));
    }
}