# Hexalith.KeyValueStorages Examples

This directory contains working example projects that demonstrate real-world usage patterns for Hexalith.KeyValueStorages.

## Examples Overview

| Example | Type | Demonstrates |
|---------|------|--------------|
| [JsonExample](./Hexalith.KeyValueStorages.JsonExample/README.md) | Console App | Basic CRUD operations |
| [SimpleApp](./Hexalith.KeyValueStorages.SimpleApp/README.md) | Blazor Server | Full web application |

## JsonExample - Console Application

A minimal console application demonstrating core functionality.

### What You'll Learn

- Creating a file-based key-value store
- Adding and retrieving values
- Updating with ETag validation
- Handling concurrency conflicts
- Removing values

### Key Code Patterns

```csharp
// Create the store
var store = new JsonFileKeyValueStore<string, CountryState>(
    settings,
    database: "example",
    container: "countries");

// Add a new country
var france = new CountryState
{
    Code = "FR",
    Name = "France",
    Currency = "EUR"
};
string etag = await store.AddAsync("FR", france, cancellationToken);

// Update with concurrency check
var updated = france with { Name = "French Republic" };
string newEtag = await store.SetAsync("FR", updated, cancellationToken);

// Retrieve
var retrieved = await store.GetAsync("FR", cancellationToken);
Console.WriteLine($"Country: {retrieved.Name}");
```

### Running the Example

```bash
cd src/examples/Hexalith.KeyValueStorages.JsonExample
dotnet run
```

**Expected Output:**
```
Adding countries...
Added France with ETag: abc123
Added China with ETag: def456
...
Retrieving France...
Country: France, Currency: EUR
...
```

## SimpleApp - Blazor Server Application

A complete web application with user interface for managing country data.

### What You'll Learn

- Integrating with ASP.NET Core dependency injection
- Building CRUD UI with Blazor components
- Managing an index of all stored keys
- Handling user interactions with optimistic concurrency
- Displaying and editing state in web forms

### Application Structure

```
SimpleApp/
├── Program.cs           # DI configuration
├── Models/
│   ├── Country.cs       # Domain model
│   ├── CountryState.cs  # State wrapper
│   └── CountryIndex.cs  # Index for listing
├── Components/
│   ├── CountryList.razor    # List view
│   ├── CountryEdit.razor    # Edit form
│   └── CountryCreate.razor  # Create form
└── wwwroot/
    └── data/            # Storage directory
```

### Key Patterns Demonstrated

#### Dependency Injection Setup

```csharp
// Program.cs
builder.Services.Configure<KeyValueStorageSettings>(
    builder.Configuration.GetSection("Hexalith:KeyValueStorages"));

builder.Services.AddSingleton<IKeyValueProvider, JsonFileKeyValueProvider>();

builder.Services.AddSingleton<CountryService>();
```

#### Service Layer

```csharp
public class CountryService
{
    private readonly IKeyValueStore<string, CountryState> _store;
    private readonly IKeyValueStore<string, CountryIndexState> _indexStore;

    public async Task<IReadOnlyList<Country>> GetAllCountriesAsync(
        CancellationToken cancellationToken)
    {
        var index = await _indexStore.TryGetAsync("all", cancellationToken);
        if (index == null) return [];

        var countries = new List<Country>();
        foreach (var code in index.CountryCodes)
        {
            var state = await _store.TryGetAsync(code, cancellationToken);
            if (state != null)
                countries.Add(state.ToCountry());
        }
        return countries;
    }
}
```

#### Blazor Component

```razor
@* CountryList.razor *@
@inject CountryService CountryService

<h3>Countries</h3>

@if (_countries == null)
{
    <p>Loading...</p>
}
else if (!_countries.Any())
{
    <p>No countries found.</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Code</th>
                <th>Name</th>
                <th>Currency</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var country in _countries)
            {
                <tr>
                    <td>@country.Code</td>
                    <td>@country.Name</td>
                    <td>@country.Currency</td>
                    <td>
                        <button @onclick="() => EditCountry(country.Code)">Edit</button>
                        <button @onclick="() => DeleteCountry(country.Code)">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private IReadOnlyList<Country>? _countries;

    protected override async Task OnInitializedAsync()
    {
        _countries = await CountryService.GetAllCountriesAsync(default);
    }
}
```

### Running the Example

```bash
cd src/examples/Hexalith.KeyValueStorages.SimpleApp
dotnet run
```

Open your browser to `https://localhost:5001` to see the application.

### Data Storage

The application stores data in JSON files:

```
wwwroot/data/
└── simpleapp/
    └── countries/
        └── country/
            ├── FR.json
            ├── US.json
            └── CN.json
```

Each file contains the state with ETag:

```json
{
  "Code": "FR",
  "Name": "France",
  "Currency": "EUR",
  "PhonePrefix": "+33",
  "Etag": "abc123xyz"
}
```

## Creating Your Own Example

Use this template to get started:

```csharp
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Abstractions;
using Hexalith.KeyValueStorages.Files;

// 1. Define your state
public record MyItemState : StateBase
{
    public static string Name => "MyItem";
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

// 2. Create the store
var settings = new KeyValueStorageSettings { StorageRootPath = "./data" };
var store = new JsonFileKeyValueStore<string, MyItemState>(
    settings, "myapp", "items");

// 3. Use it!
var item = new MyItemState
{
    Title = "My First Item",
    Description = "This is a test"
};

string etag = await store.AddAsync("item-001", item, default);
Console.WriteLine($"Created with ETag: {etag}");
```

## Additional Resources

- [Best Practices](../../docs/BEST_PRACTICES.md) - Design patterns and tips
- [Troubleshooting](../../docs/TROUBLESHOOTING.md) - Common issues and solutions
- [API Documentation](../libraries/README.md) - Package reference