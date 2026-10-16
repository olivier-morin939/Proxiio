# 01.01-data-layer-pkcs: Pin Pkcs in the data-layer projects

## Objective
Add a direct `System.Security.Cryptography.Pkcs` 10.0.13 reference to the three data-layer projects where NuGet currently resolves vulnerable 9.0.11 transitively.

## Scope
- `Entities/Entities.csproj`
- `Repository/Repository.csproj`
- `RepositoryContracts/RepositoryContracts.csproj`

## Research findings
The solution targets `net10.0` and does not use Central Package Management. The vulnerability audit reports Pkcs 9.0.11 in these three projects. The latest stable version supported for `net10.0` is 10.0.13. The dependency path flows through EF Core / Microsoft.Data.SqlClient and project references. No direct Pkcs or Xml API changes were reported.

## Steps
1. Add an explicit `<PackageReference Include="System.Security.Cryptography.Pkcs" Version="10.0.13" />` to each listed project.
2. Restore and build the affected projects.
3. Confirm the Pkcs NU1902/NU1903 advisories no longer appear for these projects.

**Done when**: All three projects resolve Pkcs 10.0.13, build successfully, and emit no targeted vulnerability warning for Pkcs.

## Scope Inventory

- `Entities/Entities.csproj` has `Microsoft.EntityFrameworkCore.SqlServer` and Tools 10.0.12; NuGet audit resolves Pkcs 9.0.11 transitively.
- `Repository/Repository.csproj` references EF Core 10.0.12 and `Entities`; NuGet audit resolves Pkcs 9.0.11 transitively.
- `RepositoryContracts/RepositoryContracts.csproj` references `Entities`; NuGet audit resolves Pkcs 9.0.11 transitively.
- No crypto package is declared directly and no `Directory.Packages.props` / CPM is present, so direct 10.0.13 package references belong in each of these three project files.
- The supported-version lookup confirms Pkcs 10.0.13 for `net10.0`; validate with targeted restore/build and check for NU1902/NU1903.
