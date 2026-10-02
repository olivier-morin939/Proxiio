# Proxiio

## Présentation

**Proxiio** est une application web développée entièrement en **C# avec ASP.NET Core**.

Le projet a pour objectif de créer une plateforme permettant de mettre en relation des **clients ayant des projets de construction** avec des **entrepreneurs** pouvant réaliser ces projets.

L'application est actuellement développée sous la forme d'une **architecture monolithique structurée en plusieurs couches distinctes**. Cette organisation permet de séparer les responsabilités de chaque composant tout en conservant une application déployée comme une seule unité.

Le projet met particulièrement l'accent sur la **maintenabilité**, la **séparation des responsabilités**, la **réutilisabilité du code**, la **testabilité** et la **tolérance aux pannes**.

---

# Architecture

Proxiio utilise actuellement une **architecture monolithique en couches** (*Layered Monolith*).

Bien que l'application soit monolithique et soit exécutée comme une seule application, ses responsabilités sont séparées dans différentes couches.

### Structure principale

```text
Proxiio
│
├── Entities
│
├── Repositories
│
├── RepositoryContracts
│
├── Services
│
├── ServiceContracts
│
└── Tests
```

Chaque couche possède une responsabilité spécifique.

---

## Entities

La couche **Entities** contient les modèles représentant les principales données utilisées par l'application.

Elle permet notamment de définir la structure des objets manipulés par les différentes couches du système.

Exemple :

```csharp
public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }
}
```

Les entités sont utilisées par les services et les repositories afin de représenter les données de l'application.

---

## Repositories

La couche **Repositories** est responsable de l'accès aux données.

Son rôle est de centraliser les opérations permettant de communiquer avec la base de données.

Cela permet aux services de ne pas avoir à connaître directement les détails de l'accès aux données.

Exemple conceptuel :

```text
Service
   ↓
Repository
   ↓
Database
```

Cette séparation facilite notamment :

* la maintenance ;
* les tests unitaires ;
* le remplacement de la technologie d'accès aux données ;
* la réutilisation des opérations d'accès aux données.

---

## RepositoryContracts

La couche **RepositoryContracts** contient les interfaces définissant les opérations offertes par les repositories.

Par exemple :

```csharp
public interface IUserRepository
{
    Task<User?> GetUserById(int id);

    Task<User?> GetUserByEmail(string email);

    Task<User> AddUser(User user);
}
```

Le service dépend donc de l'abstraction plutôt que d'une implémentation concrète.

Cette approche permet de respecter notamment le principe **Dependency Inversion** de SOLID.

---

## Services

La couche **Services** contient la logique métier de l'application.

Les services sont responsables de l'exécution des différentes opérations métier et utilisent les repositories pour accéder aux données.

Architecture simplifiée :

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

Les services permettent ainsi d'éviter de placer la logique métier directement dans les contrôleurs.

---

## ServiceContracts

La couche **ServiceContracts** contient les interfaces des services.

Par exemple :

```csharp
public interface IUserService
{
    Task<UserResponse> AddUser(AddUserRequest request);

    Task<UserResponse?> GetUserById(int id);
}
```

Les contrôleurs peuvent ainsi dépendre de `IUserService` plutôt que directement de `UserService`.

Cela permet de réduire le couplage entre les différentes parties de l'application.

---

# Tests

Le projet possède une couche dédiée aux **tests**.

Les tests permettent de vérifier le comportement des différentes fonctionnalités de l'application et particulièrement de la logique métier.

L'utilisation d'interfaces pour les services et les repositories facilite la création de tests puisque les dépendances peuvent être remplacées par des implémentations simulées (*mocks*).

Architecture de test :

```text
Test
  ↓
Service
  ↓
Mock Repository
```

Cela permet de tester la logique métier sans nécessairement dépendre d'une base de données réelle.

---

# Principes de programmation

Proxiio est développé en suivant plusieurs principes de conception afin de conserver une base de code maintenable et évolutive.

## SOLID

Le projet applique les principes **SOLID** afin de limiter le couplage entre les différentes composantes.

### S — Single Responsibility Principle

Chaque classe doit avoir une responsabilité clairement définie.

Par exemple :

* les controllers gèrent les requêtes HTTP ;
* les services gèrent la logique métier ;
* les repositories gèrent l'accès aux données.

---

### O — Open/Closed Principle

Les composants doivent pouvoir être étendus sans devoir modifier inutilement leur comportement existant.

L'utilisation d'interfaces permet notamment de remplacer une implémentation par une autre.

---

### L — Liskov Substitution Principle

Les implémentations concrètes doivent pouvoir remplacer leurs abstractions sans modifier le comportement attendu du programme.

---

### I — Interface Segregation Principle

Les interfaces doivent rester suffisamment spécialisées afin d'éviter de forcer les classes à implémenter des fonctionnalités dont elles n'ont pas besoin.

---

### D — Dependency Inversion Principle

Les couches de haut niveau dépendent d'abstractions plutôt que directement d'implémentations concrètes.

Exemple :

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

Le service dépend de `IUserRepository` plutôt que directement de `UserRepository`.

---

# DRY

Le projet suit également le principe **DRY — Don't Repeat Yourself**.

L'objectif est d'éviter de reproduire plusieurs fois la même logique dans différentes parties de l'application.

La séparation en services, contrats et repositories permet notamment de centraliser certaines responsabilités et de réutiliser la logique existante.

---

# Injection de dépendances

Proxiio utilise le système d'**Dependency Injection** fourni par ASP.NET Core.

Les dépendances sont injectées dans les classes plutôt que d'être créées directement à l'intérieur de celles-ci.

Exemple :

```csharp
public UserService(IUserRepository userRepository)
{
    _userRepository = userRepository;
}
```

Cette approche permet :

* de réduire le couplage ;
* de faciliter les tests ;
* de respecter le principe Dependency Inversion ;
* de rendre les composants plus facilement remplaçables.

---

# Tolérance aux pannes et redondance de la base de données

Proxiio prévoit un mécanisme de **redondance de la base de données** afin d'améliorer la disponibilité du système.

L'architecture utilise un serveur de base de données principal ainsi qu'un **serveur de secours**.

Conceptuellement :

```text
                  ┌─────────────────┐
                  │    Proxiio      │
                  └────────┬────────┘
                           │
                           ▼
                 ┌───────────────────┐
                 │ Serveur DB principal│
                 └─────────┬─────────┘
                           │
                    Fonctionnement normal
                           │
                           ▼
                 ┌───────────────────┐
                 │ Serveur DB secours│
                 └───────────────────┘
```

En cas de défaillance du serveur principal, le système peut détecter l'indisponibilité de celui-ci et effectuer un **basculement (*failover*) vers le serveur de secours**.

L'objectif est de limiter l'interruption du service en cas de panne du serveur de base de données principal.

> Le mécanisme exact de détection, de réplication et de basculement dépend de la technologie de base de données et de l'infrastructure utilisée.

---

# 🏛️ Flux général de l'application

Le fonctionnement général d'une requête peut être représenté comme ceci :

```text
Client
  │
  ▼
ASP.NET Core
  │
  ▼
Controller
  │
  ▼
Service Contract
  │
  ▼
Service
  │
  ▼
Repository Contract
  │
  ▼
Repository
  │
  ▼
Database
```

Les résultats suivent ensuite le chemin inverse :

```text
Database
   ↓
Repository
   ↓
Service
   ↓
Controller
   ↓
HTTP Response
   ↓
Client
```

Cette séparation permet de maintenir une responsabilité claire pour chaque couche.

---

# Technologies

Les technologies actuellement utilisées dans le projet sont :

| Technologie              | Utilisation                                   |
| ------------------------ | --------------------------------------------- |
| **C#**                   | Langage principal                             |
| **ASP.NET Core**         | Framework principal                           |
| **.NET**                 | Plateforme d'exécution                        |
| **Dependency Injection** | Gestion des dépendances                       |
| **SQL Database**         | Stockage des données                          |
| **Tests unitaires**      | Vérification du comportement de l'application |

> La liste des technologies pourra être complétée au fur et à mesure de l'évolution du projet.

---

# 📁 Structure du projet

Une représentation simplifiée de la structure actuelle est :

```text
Proxiio/
│
├── Entities/
│   └── ...
│
├── Repositories/
│   └── ...
│
├── RepositoryContracts/
│   └── ...
│
├── Services/
│   └── ...
│
├── ServiceContracts/
│   └── ...
│
├── Tests/
│   └── ...
│
├── Program.cs
│
└── ...
```

Cette structure peut évoluer avec l'ajout de nouvelles fonctionnalités.

---

# Installation

## Prérequis

Pour exécuter le projet, il est nécessaire d'avoir installé :

* [.NET SDK](https://dotnet.microsoft.com/download)
* Un serveur de base de données compatible avec la configuration du projet
* Un IDE compatible avec C#, tel que :

  * Visual Studio
  * Visual Studio Code
  * JetBrains Rider

---

## Cloner le projet

```bash
git clone <URL_DU_REPOSITORY>
```

Puis :

```bash
cd Proxiio
```

---

## Configuration

Les paramètres de connexion à la base de données doivent être configurés dans la configuration de l'application.

Par exemple :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

Les informations sensibles telles que les mots de passe ou les clés secrètes ne doivent pas être ajoutées directement dans le dépôt Git.

---

## Lancer le projet

Après avoir configuré l'environnement :

```bash
dotnet restore
```

Puis :

```bash
dotnet build
```

Et finalement :

```bash
dotnet run
```

---

# Sécurité

La sécurité constitue un aspect important du développement de Proxiio.

Les informations sensibles ne doivent pas être stockées directement dans le code source.

Les bonnes pratiques suivantes sont privilégiées :

* validation des données reçues ;
* séparation de la logique métier ;
* utilisation de paramètres pour les requêtes SQL ;
* gestion sécurisée des informations de connexion ;
* contrôle des accès aux fonctionnalités protégées ;
* limitation de l'exposition des informations sensibles.

---

# Évolutivité

Même si Proxiio utilise actuellement une architecture monolithique, la séparation en couches permet de faire évoluer progressivement l'application.

La séparation entre :

```text
Controllers
     ↓
Services
     ↓
Repositories
     ↓
Database
```

permet de modifier ou remplacer certaines implémentations sans devoir réécrire l'ensemble de l'application.

L'architecture actuelle pourrait également évoluer ultérieurement vers une architecture plus distribuée si les besoins du projet le justifient.

---

# État du projet

Le projet est actuellement **en cours de développement**.

Les différentes fonctionnalités seront ajoutées progressivement.

### Fonctionnalités prévues

* [ ] Gestion des utilisateurs
* [ ] Authentification
* [ ] Gestion des profils
* [ ] Gestion des projets de construction
* [ ] Gestion des entrepreneurs
* [ ] Mise en relation clients / entrepreneurs
* [ ] Gestion des demandes
* [ ] Système de recherche
* [ ] Gestion des soumissions
* [ ] Système de communication
* [ ] Tests automatisés supplémentaires
* [ ] Amélioration de la tolérance aux pannes

Cette liste sera mise à jour au fur et à mesure du développement.

---

# Objectifs du projet

Les principaux objectifs techniques de Proxiio sont :

* développer une application **maintenable** ;
* appliquer les principes **SOLID** ;
* éviter la duplication de code avec **DRY** ;
* favoriser la **séparation des responsabilités** ;
* rendre les composants **testables** ;
* réduire le couplage entre les différentes couches ;
* assurer une meilleure **tolérance aux pannes** ;
* permettre au projet d'évoluer facilement avec l'ajout de nouvelles fonctionnalités.

---

# Licence

La licence du projet sera définie ultérieurement.

---

# Auteur

**Proxiio - Olivier Morin**

Projet développé en **C# / ASP.NET Core**.
