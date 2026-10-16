# NuGet package upgrade plan

## Overview

**Target**: Eliminate NuGet vulnerability warnings for `System.Security.Cryptography.Pkcs` and `System.Security.Cryptography.Xml` by resolving their vulnerable transitive versions.
**Scope**: Seven .NET 10 projects in `ProxiioApp.slnx`; update only projects where the vulnerability audit currently resolves an affected package.

## Tasks

### 01-pin-crypto-packages: Pin secure cryptography package versions

Add explicit `System.Security.Cryptography.Pkcs` 10.0.13 references to the projects where the package currently resolves to a vulnerable transitive version. Add `System.Security.Cryptography.Xml` 10.0.13 references to its affected projects. The repository does not use Central Package Management, so place package versions in the respective project files. The package API assessment found no direct-reference API changes; validate that package restore preserves compatibility.

**Done when**: All affected projects resolve both requested packages at 10.0.13 where applicable; the targeted NU1902/NU1903 vulnerability warnings are gone; the solution builds and all tests pass.
