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

## Troubleshooting: SQL Post-Login Timeout

If you see an error similar to:

`Connection Timeout Expired ... [Post-Login] complete=...`

Use this checklist:

1. Verify the SQL server name in your connection string uses `tcp:<server>.database.windows.net,1433`.
2. Verify Azure SQL firewall rules allow your client public IP.
3. If you are on a restricted corporate network, either:
   - Set Azure SQL **Connection policy** to **Proxy**, or
   - Allow outbound TCP ports `11000-11999` (required by Redirect mode).
4. Ensure the SQL login/user in the connection string is valid and not locked.
5. Confirm there is no VPN/proxy TLS interception blocking SQL TLS negotiation.

This API now logs additional SQL diagnostics (error number/state/class) and returns `503` when the database is unreachable.

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
