# Project Setup

This project is built with **.NET 8** and uses **Entity Framework Core** for database management.

## Getting Started

Follow these steps to configure and run the project locally.

### 1. Clone the repository

```bash
git clone <REPOSITORY_URL>
cd <PROJECT_FOLDER>
```

### 2. Configure the connection string

Open the `appsettings.json` file and update the connection string to point to the SQL Server instance where you want the database to be created.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 3. Create the migration

Open the **Package Manager Console** in Visual Studio and run:

```powershell
Add-Migration InitialCreate
```

> If the initial migration already exists in the project, you can skip this step.

### 4. Update the database

Run the following command to apply the migrations and create the database:

```powershell
Update-Database
```

### 5. Seed initial roles

After the database has been created, execute the following SQL script:

```sql
INSERT INTO users_role (Name, Description, CreatedDate, IsActive)
VALUES
    ('Nurse', 'Nurse role', GETDATE(), 1),
    ('Psychologist', 'Psychologist role', GETDATE(), 1),
    ('Head Nurse', 'Head nurse role', GETDATE(), 1),
    ('Head of Psychology', 'Head of psychology role', GETDATE(), 1),
    ('Coordinator', 'Coordinator role', GETDATE(), 1),
    ('Student', 'Student role', GETDATE(), 1),
    ('Administrator', 'Administrator role', GETDATE(), 1),
    ('Teacher', 'Teacher role', GETDATE(), 1);
```

## Quick Summary

- Clone the repository
- Update the connection string in `appsettings.json`
- Run `Add-Migration InitialCreate`
- Run `Update-Database`
- Execute the SQL script to insert initial roles
