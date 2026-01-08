---
description: How to use EF Core scaffolding to generate entities from existing database
---

# Database Scaffolding Workflow

This workflow guides you on how to scaffold entities from an existing PostgreSQL database into the Clean Architecture structure.

## Prerequisites

1. Ensure you have the EF Core tools installed globally:
```bash
dotnet tool install --global dotnet-ef
```

2. Ensure your database exists and the connection string is configured in `appsettings.Development.json`

## Scaffolding Commands

### 1. Scaffold All Tables to Infrastructure (Temporary)

First, scaffold to a temporary location in Infrastructure:

```bash
cd src/VNVTStore.Infrastructure
dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=vnvtstore;Username=postgres;Password=123456" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Persistence/ScaffoldedEntities --context-dir Persistence --context ScaffoldedDbContext --force
```

### 2. Scaffold Specific Tables Only

```bash
dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=vnvtstore;Username=postgres;Password=123456" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Persistence/ScaffoldedEntities --table users --table products --table orders --force
```

### 3. Using Connection String from appsettings

```bash
cd src/VNVTStore.API
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Npgsql.EntityFrameworkCore.PostgreSQL --project ../VNVTStore.Infrastructure --output-dir Persistence/ScaffoldedEntities --context ScaffoldedDbContext --force
```

## Post-Scaffolding Steps

After scaffolding, you need to manually:

1. **Move entities to Domain layer**: Copy the scaffolded entities from `Infrastructure/Persistence/ScaffoldedEntities` to `Domain/Entities`

2. **Clean up entities**: 
   - Remove EF Core data annotations (move to Fluent API configurations)
   - Add private setters for encapsulation
   - Add factory methods (static `Create` methods)
   - Inherit from `BaseEntity` or `AggregateRoot`

3. **Update entity configurations**: Ensure `Infrastructure/Persistence/Configurations` has corresponding Fluent API configurations

4. **Delete scaffolded files**: Remove the temporary scaffolded files after migration

## Creating Migrations

### Add a new migration

```bash
cd src/VNVTStore.API
dotnet ef migrations add MigrationName --project ../VNVTStore.Infrastructure
```

### Update database

```bash
dotnet ef database update --project ../VNVTStore.Infrastructure
```

### Generate SQL script (for production)

```bash
dotnet ef migrations script --project ../VNVTStore.Infrastructure --output migration.sql
```

## Tips

- Always review scaffolded code before using it
- Scaffolding is meant for initial entity generation, not for ongoing sync
- Use migrations for schema changes after initial setup
- Keep Domain entities clean (no EF Core dependencies)
