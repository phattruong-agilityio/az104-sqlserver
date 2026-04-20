# Azure SQL Server Practice Application

This is a practice project demonstrating how to connect a .NET web API to Azure SQL Server. The application provides a simple REST API for managing person records.
It's a part of AZ-104 learning.

## Technologies Used

- .NET 10
- ASP.NET Core Minimal APIs
- Microsoft.Data.SqlClient
- Swagger/OpenAPI
- Azure SQL Database

## Prerequisites

- .NET 10 SDK
- Visual Studio 2022/2026 (or later) OR Visual Studio Code

## Project Overview

This ASP.NET Core minimal API application demonstrates:
- Connecting to Azure SQL Database
- Basic CRUD operations using ADO.NET
- Swagger/OpenAPI documentation
- SQL parameterized queries

## How to Run

### Running in Visual Studio

1. **Open the Project**
   - Launch Visual Studio
   - Open the solution/project file

2. **Configure Connection String**
   - Already on "appsettings.json" (public for training purpose)

3. **Run the Application**
   - Press **F5** or click the **Start** button (green play button)
   - Visual Studio will build and launch the application
   - Your default browser will open automatically

4. **Access Swagger UI**
   - The app automatically redirects to `/swagger`
   - URL: `https://localhost:<port>/swagger/index.html` (Example: `https://localhost:7155/swagger/index.html`)

### Running via Terminal/Command Line

1. **Navigate to Project Directory**
   ```bash
   git clone https://github.com/phattruong-agilityio/az104-sqlserver.git
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Application**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run --launch-profile https
   ```

5. **Access the Application**
   
    - Open your browser and navigate to `https://localhost:<port>/swagger/index.html` (Example: `https://localhost:7155/swagger/index.html`)
