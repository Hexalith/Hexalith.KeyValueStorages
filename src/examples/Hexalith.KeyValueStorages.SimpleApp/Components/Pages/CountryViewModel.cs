// <copyright file="CountryViewModel.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.SimpleApp.Components.Pages;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a view model for a country.
/// </summary>
internal class CountryViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountryViewModel"/> class.
    /// </summary>
    public CountryViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryViewModel"/> class with the specified country.
    /// </summary>
    /// <param name="country">The country to initialize the view model with.</param>
    public CountryViewModel(Country country)
    {
        Code = country.Code;
        Name = country.Name;
        Currency = country.Currency;
        PhonePrefix = country.PhonePrefix;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryViewModel"/> class.
    /// </summary>
    /// <param name="country">The country state to initialize the view model with.</param>
    public CountryViewModel(CountryState country)
        : this(country.Value)
    {
    }

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets the country represented by this view model.
    /// </summary>
    public Country Country => new(Code, Name, Currency, PhonePrefix);

    /// <summary>
    /// Gets the state of the country represented by this view model.
    /// </summary>
    public CountryState CountryState => new(Country, null, null);

    /// <summary>
    /// Gets or sets the currency of the country.
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the country.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone prefix of the country.
    /// </summary>
    [Required]
    public int PhonePrefix { get; set; }
}