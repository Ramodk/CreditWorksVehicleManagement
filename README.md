# CreditWorks Vehicle Management

This is my solution to the CreditWorks Software Engineer technical assignment. I built a small web application to manage vehicles and assign weight categories automatically. My main focus was to keep the business rules reliable and the code straightforward for another developer to understand and maintain.

## What the application does

The application lets users view vehicles, sort them by owner, manufacturer, year or weight, and see the category assigned to each vehicle. An administrator can add vehicles and manage the category definitions and icons. The dashboard also shows vehicle totals, category counts and recently added vehicles.

Categories are based on weight rather than being manually selected. The initial categories are Light (below 500 kg), Medium (500 kg to below 2500 kg) and Heavy (2500 kg and above). If an administrator changes these ranges, existing vehicles are shown under their new categories automatically.

I also added a simple admin login. Anyone can view the dashboard and vehicle list, but changes require signing in.

## Technologies used

- **Backend:** C#, ASP.NET Core 10 Web API and Entity Framework Core 10
- **Database:** Microsoft SQL Server 2019 or later
- **Frontend:** React 19, TypeScript, Vite and Tailwind CSS
- **Testing:** xUnit, including unit and API integration tests

## Getting started

### Requirements

Install .NET SDK 10, Node.js 20.19+ (or 22.12+), SQL Server 2019 or later, and the EF Core command-line tool:

```bash
dotnet tool install --global dotnet-ef --version '10.*'
```

If you do not have SQL Server installed, you can start SQL Server 2022 in Docker:

```bash
docker run -d --name creditworks-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=<your-strong-password>" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Download and install

```bash
git clone https://github.com/Ramodk/CreditWorksVehicleManagement.git
cd CreditWorksVehicleManagement
dotnet restore
cd creditworks-frontend
npm install
cd ..
```

### Configure the database

I kept the development database password outside the repository using .NET user secrets. Set your own SQL Server password with:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=CreditWorksVehicleDb;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True" \
  --project CreditWorks.Api
```

For other environments, use the `ConnectionStrings__DefaultConnection` environment variable. `TrustServerCertificate=True` is intended for local development, not a production deployment.

Create the database and insert the initial manufacturers, categories and admin account by applying the migrations:

```bash
dotnet ef database update \
  --project CreditWorks.Infrastructure \
  --startup-project CreditWorks.Api
```

The initial manufacturers are Mazda, Mercedes, Honda, Ferrari and Toyota.

### Build and run

Build the backend from the repository root:

```bash
dotnet build
```

Build the frontend:

```bash
cd creditworks-frontend
npm run build
cd ..
```

Start the API in one terminal:

```bash
dotnet run --project CreditWorks.Api --launch-profile http
```

Start the frontend in another terminal:

```bash
cd creditworks-frontend
npm run dev
```

Open **http://localhost:5173** in your browser. The API runs at **http://localhost:5200**, and Vite forwards `/api` requests to it during development. The development OpenAPI document is available at `http://localhost:5200/openapi/v1.json`.

## Admin login

The initial database migration creates a development admin account:

- **Username:** `admin`
- **Password:** `Admin@123`

These credentials are for local testing only. The password is publicly documented, so it **must be changed before any deployment**. Password hashes are stored in the `AdminUsers` table rather than plain-text passwords. There is no password-change screen yet; changing the password currently requires generating a new hash and updating the database.

After logging in through the sidebar, the administrator can add vehicles and edit categories. Sessions use an HTTP-only authentication cookie, and the login endpoint is rate-limited.

## Running the tests

I separated tests that check business rules from tests that exercise the API and database.

To run unit tests without SQL Server:

```bash
dotnet test --filter "FullyQualifiedName!~Integration"
```

Integration tests need a separate SQL Server database named `CreditWorksVehicleManagement_Test`. First apply migrations to that database:

```bash
dotnet ef database update \
  --project CreditWorks.Infrastructure \
  --startup-project CreditWorks.Api \
  --connection "Server=localhost,1433;Database=CreditWorksVehicleManagement_Test;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True"
```

Then configure the test connection and run all tests:

```bash
export CREDITWORKS_TEST_CONNECTION="Server=localhost,1433;Database=CreditWorksVehicleManagement_Test;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True"
dotnet test
```

The tests cover weight boundaries, category calculation, gaps and overlaps, category updates, vehicle validation, sorting and admin access. Integration tests use a separate database to avoid changing development data.

## How I organised the solution

I used separate projects so that the core rules are not mixed with API controllers or database code:

```text
CreditWorks.Domain          Vehicle, Manufacturer, AdminUsers and VehicleCategory entities
CreditWorks.Application     Validation rules and service interfaces
CreditWorks.Infrastructure  EF Core, migrations and service implementations
CreditWorks.Api             Controllers, API models, authentication and error handling
CreditWorks.Tests           Unit and integration tests
creditworks-frontend        React and TypeScript user interface
```

I kept the architecture fairly simple for the size of this assignment. Services work directly with EF Core's `DbContext` instead of adding a separate generic repository layer. This reduces unnecessary code while keeping the business rules testable.

## Database and category decisions

The database uses four main tables: `Vehicles`, `Manufacturers`, `VehicleCategories` and `AdminUsers`. Manufacturers are stored as records rather than hard-coded into the frontend. Vehicle weights and category limits use decimal values with two decimal places to avoid rounding problems at boundaries.

One important decision was **not to save the category ID on each vehicle**. Instead, the application calculates the category from the vehicle's weight and the current category definitions whenever vehicle data is returned. This means a change to category ranges is immediately reflected in the vehicle list, without needing to update every vehicle record.

I use **inclusive lower bounds and exclusive upper bounds**. In other words, a range includes its starting weight but not its ending weight:

- `499.99 kg` is Light.
- `500.00 kg` is Medium.
- `2499.99 kg` is Medium.
- `2500.00 kg` is Heavy.

Category ranges must start at zero, meet without gaps or overlaps, and end with an open-ended final category. A vehicle's weight must still be greater than zero.

I chose to save the **whole category configuration in one request** rather than changing each range separately. Moving one boundary usually affects two categories, so validating and saving them together avoids leaving the database in an invalid state. The API checks the complete configuration before saving it.

Icons are stored as short keys and mapped to Lucide icons in the frontend. This avoids storing image files in the database.

## Validation and error handling

Validation runs on the server even when the frontend already checks the same input. For example, the API rejects missing owner names, invalid years, non-positive weights, weights with more than two decimal places and invalid category configurations.

Expected validation problems return readable `400 Bad Request` responses. Missing records return `404`, and unexpected failures return a general error response without exposing internal stack traces. EF Core handles database queries without building SQL from raw user input.

## Assumptions

I made a few practical decisions where the assignment left room for interpretation:

- A vehicle's manufacture year can be between 1886 and the next calendar year.
- Weight must be positive, with at most two decimal places; the final category has no maximum weight.
- An owner can have more than one vehicle, so duplicate owner names are allowed.
- Manufacturer management is handled through database data rather than an admin screen.
- Category names must be unique, ignoring letter case.
- The extra admin login is intended to protect data-changing actions; public users can still view vehicle information.

## Known limitations and possible improvements

This is a technical assignment rather than a fully deployed production system. There are a few things I would improve if I continued developing it.

The vehicle list currently loads all vehicles, so pagination and server-side filtering would be useful with a larger dataset. The dashboard also calculates its totals in the browser; a dedicated summary API would scale better. The API supports editing and deleting vehicles, but those actions are not yet available through the frontend.

Category changes are validated, but simultaneous edits by different administrators could still overwrite one another. Optimistic concurrency checks would help. I would also add a change-password screen, admin account management, frontend component tests and a more complete production deployment configuration. The current development setup uses Vite's proxy and is not intended to be deployed unchanged.

## Final note

My aim with this solution was to meet the assignment requirements while keeping the implementation understandable. I spent particular attention on category boundaries, data consistency and tests, because those are the parts where small mistakes could lead to incorrect results. The project is structured so that another developer can follow the code and extend it without having to redesign the whole application.
