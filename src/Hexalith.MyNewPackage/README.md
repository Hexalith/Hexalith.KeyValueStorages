# Hexalith MyNewPackage

## Overview

This repository serves as a template for creating new packages for the Hexalith platform. It provides the essential structure and configuration needed to develop, test, and integrate new functionality with the Hexalith ecosystem.

## Purpose

The Hexalith MyNewPackage template simplifies the process of creating new packages by providing:

- Standardized project structure
- Pre-configured build settings
- Testing framework setup
- Integration points with the Hexalith platform

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 or another compatible IDE
- Git

### Creating a New Package

1. Use this repository as a template
2. Clone your new repository
3. Rename the solution and projects to match your package name
4. Update the namespace references
5. Implement your functionality

## Project Structure

- `src/` - Source code for the package
- `test/` - Unit and integration tests
- `samples/` - Example implementations

## Development Guidelines

- Follow C# coding standards and best practices
- Use primary constructors for classes and records
- Include XML documentation for public, protected, and internal members
- Write unit tests using XUnit and Shouldly

## Building and Testing

```powershell
dotnet build
dotnet test
```

## Contributing

Contributions are welcome. Please ensure your code adheres to the project standards and is covered by tests.

## License

[License information](LICENSE)

## Learn More

- [Hexalith Documentation](https://github.com/Hexalith)
- [Getting Started with Hexalith](https://github.com/Hexalith)