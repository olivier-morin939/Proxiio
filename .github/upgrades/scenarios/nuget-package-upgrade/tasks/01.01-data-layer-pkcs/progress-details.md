# Progress Details: Pin Pkcs in the data-layer projects

## Changes
- Added `System.Security.Cryptography.Pkcs` 10.0.13 to `Entities/Entities.csproj`, `Repository/Repository.csproj`, and `RepositoryContracts/RepositoryContracts.csproj`.

## Validation
- `dotnet build` succeeded for all three projects after restore, with no build warnings.
- `dotnet list package --include-transitive` confirms Pkcs resolves to 10.0.13 in each project.
- `dotnet list package --vulnerable --include-transitive` reports no Pkcs vulnerability for these projects.
- Full solution build and test suite are reserved for the final validation after the application/services project group.

## Issues
- None.
