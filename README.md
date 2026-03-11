# InventoryApp – Inventory Management Web Application

## Description
A web application that allows users to create inventories, manage items, define custom fields, and analyze stored data with statistics. 

## Main Features
- **User authentication**: Secure signup, login, and robust session management.
- **Inventory creation and management**: Manage distinct lists with categorization and descriptions.
- **Item CRUD**: Full life-cycle capabilities for individual items inside your inventories.
- **Custom item fields**: Define up to 15 typed custom fields (strings, ints, booleans, markdown text, links) per inventory.
- **Custom ID generation**: Construct custom sequence formats (e.g., `COM-YYYY-0001`) for your specific use cases.
- **Comments and likes**: Engage with items via live commenting and a liking system.
- **Inventory access control**: Granular permissions (read/write access) via an Access tab to collaborate with designated users.
- **Statistics and analytics**: Real-time aggregated data visualizations on your inventory's numbers, frequent names, etc.
- **Search functionality**: Full-text PostgreSQL vector search index spanning items, comments, and field values.

## Technology Stack
- **ASP.NET Core (.NET 9.0)**
- **Entity Framework Core**
- **Razor Pages / MVC**
- **PostgreSQL**
- **Bootstrap 5 (Supports Dark/Light mode natively)**
- **SignalR (Real-time updates)**

---

## How to Run

### Requirements
- .NET SDK (version 9.0+)
- PostgreSQL (or access to a Postgres connection string)

### Steps to Run Locally
1. Clone the repository.
2. Configure the database connection string in `appsettings.json` or as an environment variable (`DATABASE_URL`).
3. Apply database migrations to scaffold the DB schema.
4. Run the application.

```bash
# Restore dependencies
dotnet restore

# Apply migrations and scaffold the database
dotnet ef database update

# Run the project
dotnet run
```

---

## Testing Instructions

**Test Flow**
1. Register a new user account (or use one of the test accounts listed below).
2. Create an inventory and specify a Category.
3. Configure **Custom Fields** via the 'Fields' tab and set up a **Custom ID format**.
4. Create new Items using your custom fields and observe the ID incrementing smoothly.
5. Explore the **Statistics** tab to observe live aggregations based on numerical and string fields.
6. Open an item and test the **Comments** and **Likes** functionality.
7. Test the **Access Control** tab by granting "Collaborator" rights to a different test email and subsequently logging in as them to edit an item.

---

## Test Accounts

The following test accounts are pre-seeded in the database for demonstration and testing purposes.

**Owner account**
- **Email:** alice@example.com
- **Password:** User@123!
- *Use for: Exploring primary inventory creation and managing access.*

**Collaborator**
- **Email:** bob@example.com
- **Password:** User@123!
- *Use for: Testing targeted read/write restrictions defined by Alice's access configurations.*

**Standard user**
- **Email:** charlie@example.com
- **Password:** User@123!
- *Use for: Testing guest/public access on inventories he was not explicitly invited to edit.*
