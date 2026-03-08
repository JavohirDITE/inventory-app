# Inventory Management App

A full-stack ASP.NET Core MVC web application for creating and managing custom collections of items. Built with C#, Entity Framework Core, PostgreSQL, and Bootstrap 5.

## Features

- **Authentication**: Email/Password registration and login via ASP.NET Core Identity.
- **OAuth Ready**: Configured for Google/Facebook OAuth integration via environment variables.
- **Custom Collections**: Users can create Inventories (collections) with custom fields (Integer, String, Text, Boolean, Link) to map their items.
- **Custom IDs**: Inventory owners can design custom ID formats (incorporating Sequence, Date, GUID, and fixed text) for items within their collection.
- **Access Control**: Inventories can be Public (read/write for all authenticated users) or Private (read-only for guests, write-access granted manually via an autocomplete UI).
- **PostgreSQL Full-Text Search**: Robust GIN-indexed search across Item names, Custom Field strings, and Inventory metadata.
- **Real-time Interaction**: Integrated SignalR for live discussion commenting on items.
- **Likes**: Users can like items, tracking popularity.
- **Admin Panel**: Dedicated minimal admin panel to promote/block/delete users, instantly invalidating their sessions.
- **Responsive UI**: Bootstrap 5 with Dark/Light theme toggle capability.

## Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC, C#
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Real-time**: SignalR
- **Frontend**: HTML5, CSS3, Vanilla JavaScript, Bootstrap 5, DOMPurify (XSS prevention), Marked.js

## Pre-requisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) server running locally or externally.

## Setup & Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. **Database Configuration**
   The application determines the connection string via the standard local `appsettings.json` or by parsing the Railway-standard `DATABASE_URL` environment variable.
   
   To use a local Postgres database, open `appsettings.json` and adjust the `DefaultConnection`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=InventoryAppDb;Username=postgres;Password=yourpassword"
   }
   ```
   **Alternatively**, set the environment variable:
   ```bash
   # Windows PowerShell
   $env:DATABASE_URL="postgres://postgres:yourpassword@localhost:5432/InventoryAppDb"
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```
   *Note: Entity Framework Migrations and Test Data Seeding are automatically applied upon startup.*

## Test Credentials & Seeding

The application automatically seeds 3 mock users and 2 mock inventories when it starts up containing a fresh database:

- **User 1 (Owner)**: `alice@example.com` / `User@123!`
- **User 2 (Collaborator)**: `bob@example.com` / `User@123!`
- **User 3**: `charlie@example.com` / `User@123!`

**Admin Account Seeding**:
To test the Admin panel, you must elevate an account. Set the `ADMIN_EMAIL` environment variable *before* running the app, and the startup logic will grant that user the Admin role:
```bash
# Windows PowerShell
$env:ADMIN_EMAIL="alice@example.com"
dotnet run
```
You can then access the Admin dashboard via the top navigation or navigate to `/Admin`.

## Deployment Notes (Railway / Heroku)

This application is configured for immediate deployment to PaaS providers like Railway.

**Required Environment Variables in Production:**
- `DATABASE_URL` or `DATABASE_PUBLIC_URL` (PostgreSQL connection string provided by the host).
- `ADMIN_EMAIL` (E.g. `your.email@example.com` - promotes your first login to Admin).

*(Optional)* **OAuth Variables:**
- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`
- `Authentication:Facebook:AppId`
- `Authentication:Facebook:AppSecret`

The runtime will automatically execute `context.Database.Migrate()` on startup, meaning no manual schema creation is required in production. SignalR is configured without hardcoded paths, working natively behind standard reverse proxies.
