# 📝 DraftLite — A Notion-like Collaborative Workspace

> A full-stack portfolio project — a Notion-inspired workspace app with multi-project management, rich page editing, and real-time collaboration sharing.

![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular&logoColor=white)
![C#](https://img.shields.io/badge/C%23-ASP.NET_Core-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

<!-- ---

## 🌐 Live Demo

> 🔗 [Notion.vercel.app](https://Notion.vercel.app) &nbsp;|&nbsp; 📹 [Watch demo video](#) -->

---

## 📖 About

**DraftLite** is a personal portfolio project built to demonstrate full-stack engineering skills across a modern, typed technology stack. It replicates the core experience of Notion — users can organize their work into projects, write and structure content using a block-based editor, and collaborate with others via shareable links with role-based access.

The project intentionally covers a broad range of engineering concerns: OAuth authentication, relational data modeling, permission enforcement, audit logging, and a reactive Angular SPA all deployed via a CI/CD pipeline.

---

## ✨ Features

- **Google OAuth login** — one-click sign-in, no passwords
- **Multi-project workspace** — create, rename, and delete projects
- **Block-based page editor** — text blocks and simple tables per page
- **Drag-and-drop page reordering** within a project
- **Sharing with roles** — invite collaborators as Reader or Writer
- **Multi user** — All usre on the same project see modification in real time
- **Audit trail** — every page edit records who changed it and when
- **Auto-save** — edits are persisted automatically with a 1.5s debounce
- **Responsive UI** — works on desktop and mobile

---

## 🏗 Architecture

```md
┌─────────────────────┐        ┌───────────────────────┐        ┌──────────────┐
│   Angular 17 SPA    │  HTTP  │  ASP.NET Core API     │  EF    │  PostgreSQL  │
│   (Vercel)          │ ─────► │  (Railway)            │ ─────► │  (Supabase)  │
│                     │        │                       │        │              │
│  - AuthService      │        │  - AuthController     │        │  - Users     │
│  - HTTP Interceptor │        │  - ProjectsController │        │  - Projects  │
│  - Route Guards     │        │  - PagesController    │        │  - Pages     │
│  - Block Editor     │        │  - Permission Service │        │  - Collab.   │
└─────────────────────┘        │  - JWT Middleware     │        │  - AuditLog  │
                               └───────────────────────┘        └──────────────┘
```

**Auth flow:**

1. User clicks "Sign in with Google" in Angular
2. Google returns an OAuth token to the Angular app
3. Angular sends the token to `POST /auth/google-login`
4. The API validates it, upserts the User record, and returns a signed JWT
5. Angular stores the JWT in memory; an HTTP interceptor attaches it to every subsequent request

---

## 🗄 Data Model

| Table | Key columns |
| --- | --- |
| `users` | `id`, `google_id`, `email`, `pseudo`, `created_at`, `isActive`, `ban_at`, `ban_reason`, `role_id -> role` |
| `role` | `id`, `name` |
| `projectRole` | `id`, `name` |
| `projects` | `id`, `owner_id → users`, `title`, `created_at`, `updated_at`, `deleted_at` |
| `pages` | `id`, `project_id → projects`, `title`, `blocks` (JSONB), `order_index`, `created_at`, `updated_at`, `deleted_at` |
| `project_collaborators` | `project_id`, `user_id`, `role_id -> projectRole` (reader/writer/projectAdmin/owner), `invited_by -> users` |
| `projectHistory` | `id`, `project_id`, `page_id`, `user_id`, `action`, `base_version`, `version`, `patch` (JSONB), `created_at` |
| `audit_log` | `id`, `entity_type`, `entity_id`, `user_id`, `action`, `changed_at` |

Page content is stored as a **JSONB array of blocks**, allowing new block types to be added without schema migrations:

```json
[
  { "id": "a1b2", "type": "text",  "style":{}, "content": "My first note **bold** __italic__ --trait--" },
  { "id": "a1b2", "type": "text",  "style":{}, "content": "---" }, # Line
  { "id": "a1b2", "type": "ListPuce",  "style":{}, "content": ["First", "second", "ect"] },
  { "id": "c3d4", "type": "table", "style":{}, "cols":{"number":00, "width":["140px", ...]}, 
    "content": [
      [#empty, {"style":{"aligne": "left", "overflow":"returnLine"}, "type":"text", "content":"blabla"}]
      [{"style":{"aligne": "center", "overflow":"hide", "number":".00, negativeRed"}, "type":"number", "content":100 #100.00}]
      [,,{"style":{"aligne": "center", "overflow":"hide", "dateFormat":"dd/MM/YY"}, "type":"date", "content":#TimeStamp}]
    ] 
  }
]
```

---

## 🔐 Permissions

Every API request to a project or page runs through a `PermissionService` before the controller handles it:

| Role | Read pages | Edit pages | Manage sharing | Delete project |
| --- | :---: | :---: | :---: | :---: |
| **Owner** | ✅ | ✅ | ✅ | ✅ |
| **Writer** | ✅ | ✅ | ❌ | ❌ |
| **Reader** | ✅ | ❌ | ❌ | ❌ |

---

## 🛠 Tech Stack

| Layer | Technology | Reason |
| --- | --- | --- |
| Front-end | Angular 17 (standalone components) | Strongly typed, enterprise-grade, demonstrates SPA skills |
| Auth | Google OAuth 2.0 + JWT | Industry standard, no password management needed |
| Back-end | C# / ASP.NET Core 8 | Strongly typed, fast REST APIs, excellent EF Core ORM |
| ORM | Entity Framework Core | Code-first migrations, clean LINQ queries |
| Database | PostgreSQL 16 | Relational integrity for permissions + JSONB for block content |
| Hosting (FE) | Vercel | Zero-config Angular deploy, free tier |
| Hosting (API) | Railway | Easy .NET container deploy, free tier |
| Database host | Supabase | Managed PostgreSQL, free tier |
| CI/CD | GitHub Actions | Build, test, and deploy on every push to `main` |
| API docs | Swagger / OpenAPI | Auto-generated, browsable at `/swagger` |

---

## 🚀 Getting Started

### Prerequisites

- Node.js 20+
- .NET 8 SDK
- PostgreSQL 16 (or a Supabase project)
- A Google Cloud project with OAuth 2.0 credentials

### 1. Clone the repo

```bash
git clone https://github.com/iDnull-dev/DraftLite.git
cd DraftLite
```

### 2. Configure the back-end

```bash
cd backend
cp appsettings.example.json appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=DraftLite;Username=postgres;Password=yourpassword"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  },
  "Jwt": {
    "Secret": "YOUR_JWT_SECRET_MIN_32_CHARS",
    "Issuer": "DraftLite-api",
    "Audience": "DraftLite-app"
  }
}
```

Run migrations and start the API:

```bash
dotnet ef database update
dotnet run
# API running at https://localhost:5001
# Swagger UI at https://localhost:5001/swagger
```

### 3. Configure the front-end

```bash
cd ../frontend
cp src/environments/environment.example.ts src/environments/environment.ts
```

Edit `environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001',
  googleClientId: 'YOUR_GOOGLE_CLIENT_ID'
};
```

Start the Angular dev server:

```bash
npm install
ng serve
# App running at http://localhost:4200
```

---

## 📁 Project Structure

```md
Notion/
├── DraftLite.Angular/                  # Angular 17 SPA
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/     # Shared components
│   │   │   ├── config/         # settings
│   │   │   ├── guards/         # security
│   │   │   ├── pages/          # pages
│   │   │   ├── plugins/        # plugins
│   │   │   ├── services/       # services
│   │   │   ├── stores/         # stores
│   │   │   └── app.routes.ts
│   │   └── environments/
│   └── angular.json
│
├── backend/                   
│   ├── DraftLite.API/
│   │   ├── Controllers/             
│   │   ├── DependencyInjection/
│   │   ├── Mapper/
│   │   ├── Hubs/ 
│   │   └── Security/
│   ├── DraftLite.DATA/              
│   │   ├── Entities/             
│   │   ├── Migrations/
│   │   └── MigrationScript/
│   ├── DraftLite.DTO/
|   |   └── AppSettings/             
│   └── DraftLite.SERVICE/
│       ├── Constants/
│       ├── Exceptions/
│       ├── Interfaces/ 
│       └── Services/           
│
├── .github/
│   └── workflows/
│       └── ci.yml             # GitHub Actions CI/CD
│
└── README.md
```

---

## 🧪 Testing

```bash
# Back-end unit tests (xUnit)
cd backend
dotnet test

# Front-end unit tests (Jest)
cd frontend
ng test --watch=false
```

Key test coverage:

- `PermissionService` — all role combinations
- `AuthGuard` — redirect behaviour for unauthenticated users
- `PagesController` — CRUD with mocked service layer

---

## 📡 REST API

Full interactive docs available at `/swagger` when running locally.

| Method | Endpoint | restricted | Description |
| --- | --- | --- | --- |
| USERS | `/users` | | |
| POST | `/users/register` | Anonymous, JWT | Add users to data base if don't existe |
| GET | `/users/` | Anonymous, JWT | Get users info from db base on jwt |
| GET | `/users/{searchName}` | LoginUser, JWT | Get users list form name |
| PUT | `/users/` | LoginUser, JWT | Update User (Pseudo, email) |
| PUT | `/users/{id}` | Admin, JWT | Update User (Pseudo, email, active, ban) |
| DELETE | `/users/{id}` | Admin, JWT | Delete User |
| projects | `/projects` | | |
| POST | `/projects` | LoginUser, JWT | Create a project |
| GET | `/projects` | LoginUser, JWT | List user's projects and sheared project |
| GET | `/projects/history/{id}` | LoginUser, Admin, JWT | Project list of history |
| GET | `/projects/content/{id}` | LoginUser, Admin, JWT | Project content |
| PUT | `/projects/{id}` | LoginUser(owner), Admin, JWT | Update Name, user's list sheared and permition |
| DELETE | `/projects/{id}` | LoginUser(owner), Admin, JWT | Delete a project |
| GET | `/projects/user/{id}` | Admin, JWT | List user's projects and sheared project |
| Page | `/projects/{id}/pages` | | |
| POST | `/projects/{id}/pages` | LoginUser, JWT | Create a page |
| GET | `/projects/{id}/pages` | LoginUser, Admin, JWT | List pages in a project |
| GET | `/projects/{id}/pages/content/{id}` | LoginUser, Admin, JWT | Page content |
| PUT | `/projects/{id}/pages/{id}` | LoginUser, Admin, JWT | Update page Name, User sheared, User permistion |
| DELETE | `/projects/{id}/pages/{id}` | LoginUser, Admin, JWT | Delete a page |

---

## 📡 WEB SOCKET

Full interactive docs available at `/swagger` when running locally.

| Name | Endpoint | Description |
|---|---|---|---|
| Shared_project_update | `/sharedProjed_[google_id]` | Used to notify connected user that is been add or remove off a project |
| Project_update | `/project_[id]` | Used to send cursor position and realtime update of the pages, and block |

---

## 👤 Author

**Lucas**
<!-- - Portfolio: [yourportfolio.dev](https://yourportfolio.dev) -->
- GitHub: [@lucas](https://github.com/iDnull-dev/)
- LinkedIn: [@lucas](https://linkedin.com/in/lucas.landrecy-dev)

---

## 📄 License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
