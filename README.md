# Gardien du bit 

Projet de gestionnaire de mot de passe sécurisé, avec la technologie Balzor et Dotnet réalisé par Arthur TITOS et Kévin QUIERCELIN. 

### Features supplémentaire

- Implémentation du ToTP avec un QR CODE et code

- Session sauvergarder même après fermeture du navigateur (stockage sécurisé des informations dans le localStorage)

- Partage de coffre via un lien sécurisé

- Sécurisation des routes API contre toute attaque par temps d'exécution 

- 

- Ajout de microtransaction via les gif sur l'écran de connexion (non)
### Architecture
🔧 1. api-gardienbit (Back-end ASP.NET Core Web API)
Ce projet représente l’API principale, qui gère la logique métier, les accès aux données, et l’exposition des endpoints.
📁 Structure :
Controllers_ : Contient les contrôleurs de l’API (Controller C#), qui exposent les routes HTTP (ex: GET, POST) à consommer par le front-end.


DAL (Data Access Layer) : Couche d’accès aux données, souvent pour des opérations personnalisées sur la base.


Fixtures : Pour initialiser des données (seeding), utile en développement ou pour des jeux de données de test.


Migrations : Fichiers générés par Entity Framework Core pour gérer l'évolution du schéma de base de données.


Models : Représente les entités de la base de données (ex: User, Vault, etc.).


Repositories : Implémente le pattern Repository pour encapsuler la logique d'accès aux données.


Services : Contient la logique métier, utilisée par les contrôleurs.


Utils : Fonctions utilitaires réutilisables (ex: encryption, validation, génération de token…).


Program.cs : Point d’entrée de l’application (configuration ASP.NET Core).


appsettings.json : Configuration de l’API (connexions, clés, options...).



📦 2. common-gardienbit (Bibliothèque partagée)
Projet commun utilisé par l’API et la WebAssembly. Sert à partager des définitions de types, comme les DTO, les énumérations et les classes utilitaires.
📁 Structure :
DTO (Data Transfer Objects) : Représentations allégées et sécurisées des données échangées entre client et serveur.


Enum : Énumérations partagées (ex: rôles, statuts…).


Utils : Méthodes utilitaires réutilisées dans tous les projets (ex: helpers cryptographiques, extensions…).



🌐 3. IIABlazorWebAssembly (Front-end Blazor WebAssembly)
C’est le front-end SPA (Single Page Application) de ton application, développé avec Blazor WebAssembly.
📁 Structure :
Connected Services : Références aux services distants (ex: Swagger, API Microsoft…).


wwwroot : Fichiers statiques (CSS, images, JS).


Layout : Composants de mise en page (MainLayout, NavMenu, etc.).


Models : Modèles de données spécifiques au front-end (parfois liés aux DTO).


Pages : Composants Blazor .razor représentant les vues/pages.


Services : Contient les classes qui gèrent la logique côté client (ex: appels à l’API via HttpClient, gestion du stockage local...).


Shared : Composants Blazor réutilisables (boutons, dialogues, barres de progression...).


App.razor : Définit la structure de l'application et les routes.


Program.cs : Configure Blazor WebAssembly (services, injection de dépendances, etc).



