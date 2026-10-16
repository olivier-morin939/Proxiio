# 01.02-application-crypto: Pin crypto packages in the application and service projects

## Objective
Add direct references to the stable fixed cryptography package versions in the remaining projects affected by transitive vulnerabilities.

## Scope
- `ProxiioApp/ProxiioApp.csproj`: Pkcs 10.0.13
- `ProxiioTest/ProxiioTest.csproj`: Pkcs and Xml 10.0.13
- `ServiceContracts/ServiceContracts.csproj`: Pkcs and Xml 10.0.13
- `Services/Services.csproj`: Pkcs and Xml 10.0.13

## Research findings
The solution targets `net10.0`, has no Central Package Management file, and the latest stable supported version for both packages is 10.0.13. The audit reports Pkcs 10.0.10 in ProxiioApp and Pkcs/Xml 10.0.12 in ProxiioTest, ServiceContracts, and Services. Xml is transitive through Microsoft.AspNetCore.DataProtection 10.0.12 and EPPlus 8.7.1; Pkcs is also brought through Microsoft.Data.SqlClient and Xml. No source API migration is expected.

## Steps
1. Add direct package references at 10.0.13 for each listed package in each project.
2. Restore and build all four projects.
3. Run the `ProxiioTest` tests.
4. Confirm targeted Pkcs/Xml NU1902/NU1903 advisories are gone from these projects.

**Done when**: The specified projects resolve each applicable package to 10.0.13, build, and tests pass without targeted vulnerability warnings.

## Scope Inventory

- `ProxiioApp/ProxiioApp.csproj` resolves Pkcs 10.0.10 through its SQL Server dependency graph; add Pkcs 10.0.13 only.
- `ProxiioTest/ProxiioTest.csproj` resolves Pkcs and Xml 10.0.12 transitively through referenced projects and test dependencies; add both explicit references.
- `ServiceContracts/ServiceContracts.csproj` directly references `Microsoft.AspNetCore.DataProtection` 10.0.12, which brings in vulnerable Xml/Pkcs; add both explicit references.
- `Services/Services.csproj` directly references EPPlus 8.7.1 and DataProtection 10.0.12, which bring in vulnerable Xml/Pkcs; add both explicit references.
- `get_project_dependencies` confirms versions are project-local, there is no CPM, and no requested crypto package is currently direct. Target versions are stable 10.0.13. No source APIs in the application consume the changed crypto APIs directly.
- Validate with restored builds for all four projects, a vulnerability audit for the four projects, and the full `ProxiioTest` test suite.
