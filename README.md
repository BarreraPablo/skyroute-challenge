# SkyRoute Travel Platform - Senior Full-Stack Developer Challenge

## 📌 Project Overview
SkyRoute is a travel aggregator platform that allows users to search, compare, and book flights. This repository contains the Flight Search & Booking module, built as a full-stack application. The solution integrates simulated airline providers (GlobalAir and BudgetWings) and calculates dynamic pricing based on specific provider rules.

## 🚀 Tech Stack
- **Backend:** .NET 10, C#, SQL Server
- **Frontend:** Angular 22, TypeScript, SCSS
- **Architecture:** Clean Architecture principles (Backend), Vertical Slice Architecture (Backend), Standalone Components (Angular)

## ⚙️ Setup & Run Instructions

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v22.17.1 or higher)
- [Angular CLI](https://angular.io/cli)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or any Docker-compatible runtime

### Start the infrastructure

From the repository root, start the SQL Server container with Docker Compose:

```bash
docker compose up -d
```

This brings up the local database used by the backend. To stop and remove the container later, run:

```bash
docker compose down
```

### Running the Backend

Open a terminal and navigate to the backend API folder:

```bash
cd backend/src/SkyRoute.Api
```
On the first run, the database will be empty and the schema must be created before starting the API. From the same folder, apply the initial EF Core migration once:

```bash
dotnet ef database update --project ../SkyRoute.Infrastructure --startup-project .
```
Restore dependencies and run the application:
```bash
dotnet restore
dotnet run
```

The API will be available at http://localhost:5000 (or the port specified in your console). You can access the Swagger UI at http://localhost:5000/swagger.

For this challenge only, the appsettings.Development.json file is included in version control with the local LocalDB connection string solely to facilitate and expedite evaluation. In a real production environment, this sensitive configuration would be handled through User Secrets in development and Azure Key Vault, or environment variables, in higher-level environments.

### Running the Frontend

Open a new terminal and navigate to the frontend folder:
```bash
cd frontend
```
Install dependencies:
```bash
npm install
```
Start the development server:
```bash
npm start
```

Open your browser and navigate to http://localhost:4200.

### Recommended local startup order

1. Start the SQL Server container with `docker compose up -d`.
2. Run the backend API with `dotnet run` from `backend/src/SkyRoute.Api`.
3. Run the frontend with `npm start` from `frontend`.

## 🏗️ Architecture & Design Decisions

### Backend: Strategy Pattern for Pricing

To ensure the system is highly scalable and adheres to the Open/Closed Principle (SOLID), the flight retrieval  logic was implemented using the Strategy Pattern.

- Instead of using complex if/else statements for each airline provider, BudgetWingsExternalServiceStrategy and GlobalAirExternalServiceStrategy classes were created.

- When a new provider needs to be added to the platform, we simply create a new strategy class without modifying the core domain logic.

### Backend: Relational Storage for Booking Snapshots

Although a document database would be a natural fit for immutable booking snapshots with minimal relational complexity, SQL Server and EF Core were chosen for this challenge to demonstrate a standard relational integration, typed persistence, and a schema that is easy to evolve.

### Frontend: Modern Angular Practices

- **Standalone Components**: The application completely avoids NgModules, leveraging modern Angular Standalone Components for better modularity and lazy-loading potential.

- **Reactive Forms & Dynamic Validation**: The booking form uses ReactiveFormsModule. The validation for the "Document Type" field dynamically switches between "Passport Number" and "National ID" based on the selected route (international vs. domestic) using a custom ValidatorFn.

- **Client-Side Sorting**: As requested, sorting by Price, Duration, and Departure Time is handled entirely on the client side using Angular Signals/Pipes to avoid unnecessary network requests.

### ⚠️ Trade-offs & Known Limitations

Due to the estimated time constraint of the challenge (3-4 hours), the following compromises were made:

- **Data Persistence**: The current implementation uses in-memory mocked data for the flight providers. However, the IFlightProvider interface was designed so that it can easily be swapped with a real HttpClient implementation in the future.

- **Authentication**: Security/Auth layers were intentionally omitted to focus on the core business logic of the search and pricing algorithms.

- **Future improvements**: Given more time, I would add unit tests for all backend classes, API integration tests, comments for all interfaces and DTOs, and unit tests for the frontend.

### Domain assumptions

- The airport entity does not belong to the domain of this application, so airport data is handled as reference data rather than as a first-class business aggregate.
- The application does not require user login. It collects passenger details only at booking time.
- The database schema allows multiple passengers per booking so the model can grow later, even though the current UI captures only one passenger.
- DocumentType is implemented as an enum because it currently has only two stable values: Passport and NationalId.

## 📁 Project Structure

The repository is divided into two main workspaces to ensure clear separation of concerns and optimized AI context handling.

```text
skyroute-challenge/
│
├── docker-compose.yml               # SQL Server container configuration
├── README.md                        # Global project documentation
│
├── backend/                         # .NET 10 Workspace
│   ├── SkyRoute.sln
│   ├── src/
│   │   ├── SkyRoute.Api/            # Minimal Controllers, Program.cs, and Dependency Injection setup
│   │   ├── SkyRoute.Core/           # Domain Entities, Interfaces, Vertical Slice Services, Requests, Responses, and DTOs (Zero external dependencies)
│   │   └── SkyRoute.Infrastructure/ # EF Core DbContext, Migrations, and Mock Providers
│   └── tests/
│       └── SkyRoute.Core.Tests/     # Unit tests for Pricing Strategies and Business Logic
│
└── frontend/                        # Angular 22 Workspace
    ├── angular.json
    ├── package.json
    └── src/
        ├── app/
        │   ├── core/                # HTTP Services, Interceptors, and Base Models
        │   ├── features/            # Vertical slices of the UI
        │   │   ├── flight-search/   # Search form and results listing components
        │   │   └── booking/         # Reactive booking form and confirmation flow
        │   └── shared/              # Reusable components (flight-card, loaders, sorting pipes)
        └── styles/
            └── _variables.scss      # Design system (colors, typography, spacing)
```

## AI Usage

AI coding assistants (Cursor) were utilized during the development of this challenge.
To maintain a high level of control and architectural integrity:

- **Isolated Contexts**: The frontend and backend were opened in separate IDE workspaces with distinct .cursorrules files. This prevented the AI from mixing C# and TypeScript paradigms.

- **Human-Led Architecture**: While AI was used for rapid code scaffolding and writing repetitive unit tests, the core design decisions (like selecting the Strategy Pattern and structuring the domain models) were strictly designed and directed by me.