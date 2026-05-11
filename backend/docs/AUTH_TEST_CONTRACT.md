# Contrat auth pour les tests (register / login / logout)

Ce document fige le **mapping fonctionnel** entre les parcours auth et le code HTTP actuel de DraftLite, tel que couvert par les tests unitaires et d’intégration.

## Register

| Élément | Détail |
|--------|--------|
| **Endpoint** | `POST /users/register` ([`UsersController`](../DraftLite.API/Controllers/UsersController.cs)) |
| **Auth** | Anonyme (`[AllowAnonymous]`) |
| **Corps** | `RegisterUserRequest` : `email`, `pseudo`, `googleId` (optionnel) |
| **Service** | [`UserService.RegisterAsync`](../DraftLite.SERVICE/Services/UserService.cs) |
| **Succès attendu** | `200 OK` + `UserDto` |
| **Comportement notable** | Si l’email existe déjà : mise à jour du pseudo et du `GoogleId` optionnel, pas de doublon utilisateur. |

## Login (pas d’endpoint dédié)

| Élément | Détail |
|--------|--------|
| **Contrat actuel** | Il n’existe pas de `POST /auth/login`. L’accès aux routes protégées suppose un **Bearer JWT** valide. |
| **Production** | Le middleware JWT est configuré pour valider des **tokens Google** (`JwtRoutingSecurity`, hors environnement `IntegrationTest`). |
| **Tests d’intégration** | Environnement `IntegrationTest` : validation **symétrique HS256** alignée sur [`JwtService`](../DraftLite.SERVICE/Services/JwtService.cs) (même `Secret` / `Issuer` / `Audience` que la config injectée par la factory). |
| **Claims utiles** | `sub` → identifiant utilisateur côté API (même valeur que `User.GoogleId` pour `GET /users` « Me »), `email`, `role`, optionnellement `pseudo`. |

## Logout (stateless)

| Élément | Détail |
|--------|--------|
| **Contrat** | Pas d’invalidation serveur de token : le client **supprime** le JWT. |
| **Responsabilité serveur** | Refuser les requêtes **sans** Bearer valide ou avec token **expiré / altéré** sur les routes `[Authorize]`. |
| **Exemple de route protégée** | `GET /users` (profil courant), `Authorization: Bearer <jwt>`. |

## Matrice statuts HTTP (intégration, routes couvertes)

| Scénario | Route | Statut attendu |
|----------|--------|----------------|
| Register valide | `POST /users/register` | `200` |
| Register invalide (ex. email vide côté service) | `POST /users/register` | `500` (exception non gérée aujourd’hui) |
| « Login » : JWT valide | `GET /users` + Bearer | `200` |
| « Logout » : pas de token | `GET /users` | `401` |
| Token expiré / signature invalide | `GET /users` + Bearer | `401` |
