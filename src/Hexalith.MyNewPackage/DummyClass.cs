// <copyright file="DummyClass.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.MyNewPackage;

/// <summary>
/// Represents a dummy class for demonstration purposes.
/// <param name="SampleValue">A sample value to demonstrate functionality.</param>
/// </summary>
public record DummyClass(string SampleValue) : IDummyClass;