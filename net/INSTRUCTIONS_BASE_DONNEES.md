# Comment ouvrir la base de données SQLite

## Emplacement du fichier
Le fichier de base de données se trouve à :
```
C:\Users\Noua\Desktop\net\hotel.db
```

## Méthodes pour ouvrir la base de données

### Option 1 : DB Browser for SQLite (RECOMMANDÉ - Interface graphique)
1. **Télécharger** : https://sqlitebrowser.org/dl/
2. **Installer** l'application
3. **Ouvrir DB Browser for SQLite**
4. **Cliquer sur "Ouvrir une base de données"**
5. **Naviguer vers** : `C:\Users\Noua\Desktop\net\hotel.db`
6. **Explorer les tables** :
   - Users
   - Rooms
   - Reservations

**Fonctionnalités** :
- ✅ Vue graphique des données
- ✅ Modification des données
- ✅ Exécution de requêtes SQL
- ✅ Export/Import de données

### Option 2 : Extension VS Code
1. **Ouvrir VS Code**
2. **Installer l'extension** : "SQLite" (par alexcvzz)
3. **Dans VS Code** : Faire Clic droit sur `hotel.db` → "Open Database"
4. **Explorer** les tables dans la barre latérale

### Option 3 : Ligne de commande SQLite
1. **Télécharger SQLite** : https://www.sqlite.org/download.html
   - Télécharger "sqlite-tools-win-x64-xxxxx.zip"
   - Extraire dans un dossier
2. **Ouvrir PowerShell** dans le dossier du projet
3. **Exécuter** :
   ```powershell
   sqlite3 hotel.db
   ```
4. **Commandes utiles** :
   ```sql
   .tables                    -- Voir toutes les tables
   .schema Users              -- Voir la structure de la table Users
   SELECT * FROM Users;       -- Voir tous les utilisateurs
   SELECT * FROM Rooms;       -- Voir toutes les chambres
   SELECT * FROM Reservations; -- Voir toutes les réservations
   .exit                     -- Quitter
   ```

### Option 4 : DBeaver (Outil professionnel)
1. **Télécharger** : https://dbeaver.io/download/
2. **Installer** DBeaver
3. **Créer une nouvelle connexion** :
   - Type : SQLite
   - Chemin : `C:\Users\Noua\Desktop\net\hotel.db`
4. **Explorer** les données

### Option 5 : Extension Chrome/Firefox
1. **Installer** l'extension "SQLite Viewer" pour Chrome
2. **Ouvrir** le fichier `hotel.db` dans le navigateur

## Structure des tables

### Table Users
- **Id** : Identifiant unique
- **Name** : Nom de l'utilisateur
- **Email** : Email (unique)
- **PasswordHash** : Mot de passe haché (BCrypt)
- **Role** : "Client" ou "Réceptionniste"

### Table Rooms
- **Id** : Identifiant unique
- **Number** : Numéro de chambre (unique)
- **Type** : Type (Simple, Double, Suite, etc.)
- **Capacity** : Capacité (nombre de personnes)
- **PricePerNight** : Prix par nuit
- **IsActive** : Si la chambre est active

### Table Reservations
- **Id** : Identifiant unique
- **UserId** : Référence à Users
- **RoomId** : Référence à Rooms
- **CheckIn** : Date d'arrivée
- **CheckOut** : Date de départ
- **Status** : 0=EnAttente, 1=Confirmee, 2=Annulee, 3=Terminee
- **CreatedAt** : Date de création
- **Total** : Montant total

## Requêtes SQL utiles

### Voir tous les utilisateurs
```sql
SELECT Id, Name, Email, Role FROM Users;
```

### Voir toutes les chambres
```sql
SELECT * FROM Rooms ORDER BY Number;
```

### Voir les réservations avec détails
```sql
SELECT 
    r.Id,
    u.Name AS Client,
    ro.Number AS Chambre,
    r.CheckIn,
    r.CheckOut,
    r.Status,
    r.Total
FROM Reservations r
JOIN Users u ON r.UserId = u.Id
JOIN Rooms ro ON r.RoomId = ro.Id
ORDER BY r.CreatedAt DESC;
```

### Compter les réservations par statut
```sql
SELECT 
    CASE Status
        WHEN 0 THEN 'En attente'
        WHEN 1 THEN 'Confirmée'
        WHEN 2 THEN 'Annulée'
        WHEN 3 THEN 'Terminée'
    END AS Statut,
    COUNT(*) AS Nombre
FROM Reservations
GROUP BY Status;
```

## ⚠️ Important

- **Ne modifiez PAS directement la base de données** pendant que l'application tourne
- **Fermez l'application** avant d'ouvrir la base de données pour éviter les conflits
- **Faites une copie de sauvegarde** avant de modifier manuellement les données
- Les **mots de passe sont hachés** (BCrypt), vous ne pouvez pas les voir en clair


