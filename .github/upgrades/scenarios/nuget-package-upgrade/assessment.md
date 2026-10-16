# NuGet package upgrade assessment

_Mode: **quick assessment** — package API diffs only; no per-project source scan was run._

## Recommended versions

- **System.Security.Cryptography.Pkcs**: not referenced by any scoped project, or no supported version was found.
- **System.Security.Cryptography.Xml**: not referenced by any scoped project, or no supported version was found.

## Public API changes

No package API diffs were produced (no scoped project moves any requested package to a new version).

## Breaking-change findings

- Version divergence findings (Pkg.0003): 0
- Requested-version-unsupported findings (Pkg.0002): 0

- Quick mode does not scan source, so there are no per-line `PkgApi` usage findings. Review the
  per-package API diffs above and rely on build errors during execution to pinpoint affected code.
- A full code scan can locate the exact source location of every breaking-change usage across the repo.
  It is opt-in and slower — re-run the assessment with `fullScan=true` only if the user requests it.

## Next steps

1. Proceed to planning to triage the API changes above and plan the code fixes (if any).

## Supplemental transitive vulnerability analysis

The package-upgrade assessment did not resolve these packages because they are transitive, not direct project references. `dotnet list ProxiioApp.slnx package --vulnerable --include-transitive` confirms:

- `System.Security.Cryptography.Pkcs`: 9.0.11 in Entities, Repository, and RepositoryContracts; 10.0.10 in ProxiioApp; 10.0.12 in ProxiioTest, ServiceContracts, and Services.
- `System.Security.Cryptography.Xml`: 10.0.12 in ProxiioTest, ServiceContracts, and Services.
- `dotnet nuget why` traces Pkcs through Microsoft.Data.SqlClient 6.1.6 and Xml; Xml is also brought in through Microsoft.AspNetCore.DataProtection 10.0.12 and EPPlus 8.7.1.
- No `Directory.Packages.props` / Central Package Management file is present. The projects target `net10.0`.
- `get_supported_package_version` resolved the latest stable compatible version as 10.0.13 for both packages on `net10.0`.

Plan: explicitly reference the secure 10.0.13 versions in the projects where NuGet resolves vulnerable transitive versions. The API-diff assessment contains no direct-reference diffs; validate compatibility with restore, a full build, and tests.

