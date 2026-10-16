# 01-pin-crypto-packages: Pin secure cryptography package versions

Add explicit `System.Security.Cryptography.Pkcs` 10.0.13 references to the projects where the package currently resolves to a vulnerable transitive version. Add `System.Security.Cryptography.Xml` 10.0.13 references to its affected projects. The repository does not use Central Package Management, so place package versions in the respective project files. The package API assessment found no direct-reference API changes; validate that package restore preserves compatibility.

**Done when**: All affected projects resolve both requested packages at 10.0.13 where applicable; the targeted NU1902/NU1903 vulnerability warnings are gone; the solution builds and all tests pass.

## Scope Inventory

- **Projects affected**: `Entities/Entities.csproj`, `Repository/Repository.csproj`, `RepositoryContracts/RepositoryContracts.csproj`, `ProxiioApp/ProxiioApp.csproj`, `ProxiioTest/ProxiioTest.csproj`, `ServiceContracts/ServiceContracts.csproj`, and `Services/Services.csproj`.
- **Distinct concern**: Pin the two vulnerable transitive cryptography packages; no source API migration is expected.
- **Audit findings**: Pkcs resolves to 9.0.11 in Entities/Repository/RepositoryContracts, 10.0.10 in ProxiioApp, and 10.0.12 in ProxiioTest/ServiceContracts/Services. Xml resolves to 10.0.12 in ProxiioTest/ServiceContracts/Services.
- **Dependency paths**: Pkcs is brought in through Microsoft.Data.SqlClient 6.1.6 and Xml; Xml is brought in through Microsoft.AspNetCore.DataProtection 10.0.12 and EPPlus 8.7.1.
- **Package management**: No Central Package Management; package references are versioned in project files. The stable compatible target for both packages on net10.0 is 10.0.13.
- **Validation**: Restore and build affected projects, run the full solution build, then run `ProxiioTest` and verify targeted NU1902/NU1903 advisories are cleared.
