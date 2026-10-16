# Mise à niveau NuGet — packages cryptographiques vulnérables

## Strategy
Épingler les versions stables corrigées des dépendances cryptographiques transitives dans les projets concernés, sans introduire Central Package Management.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Pace**: Standard
- **Scope**: Toute la solution `ProxiioApp.slnx`.
- **Packages**: `System.Security.Cryptography.Pkcs` et `System.Security.Cryptography.Xml`.
- **Version policy**: 10.0.13 pour les deux packages, dernière version stable compatible sur `net10.0` ; aucune préversion.
- **Full scan**: Quick assessment par défaut ; demander avant toute analyse sémantique exhaustive.

## Decisions
- Les deux packages sont transitifs et l’évaluation NuGet dédiée ne les détecte pas comme références directes ; l’audit `dotnet list package --vulnerable --include-transitive` confirme les versions vulnérables et les projets concernés.
- Forcer des références directes à 10.0.13 dans les projets affectés, puis valider par restauration, compilation et tests.

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->

## Source Control
- **Source Branch**: master
- **Working Branch**: nuget-package-upgrade-crypto
- **Pending Changes**: Commités sur `master` à la demande explicite de l’utilisateur avant création de la branche.
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)
