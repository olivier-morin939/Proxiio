# Progress Details: Pin crypto packages in application and service projects

## Changes
- Added `System.Security.Cryptography.Pkcs` 10.0.13 to `ProxiioApp/ProxiioApp.csproj`, `ProxiioTest/ProxiioTest.csproj`, `ServiceContracts/ServiceContracts.csproj`, and `Services/Services.csproj`.
- Added `System.Security.Cryptography.Xml` 10.0.13 to `ProxiioTest/ProxiioTest.csproj`, `ServiceContracts/ServiceContracts.csproj`, and `Services/Services.csproj`.

## Validation
- Restored and built all four affected projects successfully; each build reported 0 warnings and 0 errors.
- `dotnet list package --include-transitive` confirms each applicable package resolves to 10.0.13 in all four projects.
- `dotnet list package --vulnerable --include-transitive` reports no targeted Pkcs/Xml vulnerability in those projects.
- `dotnet test .\ProxiioTest\ProxiioTest.csproj --no-build`: 64 passed, 0 failed, 0 skipped. Test output includes FluentAssertions' license notice; it is not a compiler warning.

## Issues
- None.
