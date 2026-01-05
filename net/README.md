# Système de Gestion d'Hôtel - Disponibilités & Réservations

Application web ASP.NET Core MVC pour la gestion d'un hôtel avec recherche de disponibilités et réservations en ligne.

## Technologies utilisées

- **ASP.NET Core 8.0** (MVC)
- **C#**
- **Entity Framework Core 8.0**
- **SQLite** (base de données locale)
- **BCrypt.Net** (hachage des mots de passe)
- **Bootstrap 5.3** (interface utilisateur)

## Fonctionnalités

### Pour les Clients
- Inscription et connexion
- Recherche de chambres disponibles par dates (check-in / check-out)
- Réservation de chambres
- Consultation de ses réservations
- Annulation de réservations

### Pour les Réceptionnistes
- Gestion complète des chambres (CRUD)
- Gestion des réservations (confirmation, modification, annulation)
- Tableau de bord avec :
  - Arrivées du jour
  - Départs du jour
  - Taux d'occupation
  - Statistiques des chambres

## Règles métier

- **Anti-chevauchement** : Une chambre ne peut pas être réservée sur des dates qui se croisent
- **Validation des dates** : La date de départ doit être postérieure à la date d'arrivée
- **Statuts de réservation** : En attente, Confirmée, Annulée, Terminée

## Installation

### Prérequis

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou supérieur
- Un éditeur de code (Visual Studio, VS Code, Rider, etc.)

### Étapes d'installation

1. **Cloner ou télécharger le projet**

2. **Ouvrir un terminal dans le répertoire du projet**

3. **Restaurer les dépendances NuGet**
   ```bash
   dotnet restore
   ```

4. **Créer la base de données SQLite**
   ```bash
   dotnet ef database update
   ```
   
   **Note** : Si vous n'avez pas les outils EF Core installés globalement :
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   
   La base de données `hotel.db` sera créée automatiquement au premier lancement grâce à `EnsureCreated()` dans `Program.cs`.

5. **Lancer l'application**
   ```bash
   dotnet run
   ```

6. **Ouvrir votre navigateur** et aller à `https://localhost:5001` ou `http://localhost:5000`

## Comptes de démonstration

L'application crée automatiquement deux comptes de test lors de la première exécution :

### Client
- **Email** : `client@test.com`
- **Mot de passe** : `client123`

### Réceptionniste
- **Email** : `reception@hotel.com`
- **Mot de passe** : `reception123`

## Structure du projet

```
HotelManagement/
├── Controllers/          # Contrôleurs MVC
│   ├── ClientController.cs
│   ├── HomeController.cs
│   └── ReceptionController.cs
├── Data/                # Accès aux données
│   ├── HotelDbContext.cs
│   └── SeedData.cs
├── Models/              # Modèles de données
│   ├── User.cs
│   ├── Room.cs
│   ├── Reservation.cs
│   ├── SearchViewModel.cs
│   └── DashboardViewModel.cs
├── Services/            # Services métier
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IReservationService.cs
│   └── ReservationService.cs
├── Views/               # Vues Razor
│   ├── Client/
│   ├── Home/
│   ├── Reception/
│   └── Shared/
├── wwwroot/             # Fichiers statiques
│   ├── css/
│   └── js/
├── Program.cs           # Point d'entrée
├── appsettings.json     # Configuration
└── HotelManagement.csproj
```

## Base de données

La base de données SQLite (`hotel.db`) contient trois tables principales :

- **Users** : Utilisateurs (clients et réceptionnistes)
- **Rooms** : Chambres de l'hôtel
- **Reservations** : Réservations avec dates, statuts et totaux

### Modèle de données

- **User** : Id, Name, Email, PasswordHash, Role
- **Room** : Id, Number, Type, Capacity, PricePerNight, IsActive
- **Reservation** : Id, UserId, RoomId, CheckIn, CheckOut, Status, CreatedAt, Total

## Utilisation

### Pour un Client

1. S'inscrire ou se connecter avec le compte client
2. Accéder à "Rechercher" pour chercher des chambres disponibles
3. Sélectionner des dates d'arrivée et de départ
4. Choisir une chambre parmi les résultats et cliquer sur "Réserver"
5. Consulter ses réservations dans "Mes Réservations"
6. Annuler une réservation si nécessaire (avant la date d'arrivée)

### Pour un Réceptionniste

1. Se connecter avec le compte réceptionniste
2. **Tableau de bord** : Voir les arrivées/départs du jour et les statistiques
3. **Chambres** : Ajouter, modifier ou supprimer des chambres
4. **Réservations** : Consulter toutes les réservations, modifier leurs statuts ou dates

## Sécurité

- Les mots de passe sont hachés avec BCrypt
- Authentification par session
- Séparation des rôles (Client / Réceptionniste) avec des filtres d'autorisation
- Validation des données côté serveur

## Développement

### Ajouter une migration

Si vous modifiez les modèles de données :

```bash
dotnet ef migrations add NomDeLaMigration
dotnet ef database update
```

### Exécuter les tests

Si vous ajoutez des tests unitaires :

```bash
dotnet test
```

## Notes

- La base de données SQLite est créée localement dans le répertoire du projet
- Les données initiales (chambres et comptes de test) sont créées automatiquement au premier lancement
- Pour un environnement de production, il est recommandé d'utiliser une base de données plus robuste (SQL Server, PostgreSQL, etc.)

## Auteur

Projet créé dans le cadre d'un système de gestion d'hôtel.

## Licence

Ce projet est fourni à des fins éducatives et de démonstration.




