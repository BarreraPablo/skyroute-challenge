# SkyRoute Travel Platform - Senior Full-Stack Developer Challenge

## 📌 Project Overview
SkyRoute is a travel aggregator platform that allows users to search, compare, and book flights. This repository contains the Flight Search & Booking module, built as a full-stack application. The solution integrates simulated airline providers (GlobalAir and BudgetWings) and calculates dynamic pricing based on specific provider rules.

## 🚀 Tech Stack
- **Backend:** .NET 10, C#
- **Frontend:** Angular 22, TypeScript, SCSS
- **Architecture:** Clean Architecture principles (Backend), Vertical Slice Architecture (Backend), Standalone Components (Angular)

## ⚙️ Setup & Run Instructions

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v20.19.0 or higher)
- [Angular CLI](https://angular.io/cli)

### Running the Backend

Open a terminal and navigate to the backend API folder:

```bash
cd backend/src/SkyRoute.Api
```
Restore dependencies and run the application:
```bash
dotnet restore
dotnet run
```

The API will be available at http://localhost:5000 (or the port specified in your console). You can access the Swagger UI at http://localhost:5000/swagger.

Running the Frontend

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

## 🏗️ Architecture & Design Decisions

### Backend: Strategy Pattern for Pricing

To ensure the system is highly scalable and adheres to the Open/Closed Principle (SOLID), the pricing calculation logic was implemented using the Strategy Pattern.

- Instead of using complex if/else statements for each airline provider, GlobalAirPricingStrategy and BudgetWingsPricingStrategy classes were created.

- When a new provider needs to be added to the platform, we simply create a new strategy class without modifying the core domain logic.

### Frontend: Modern Angular Practices

- **Standalone Components**: The application completely avoids NgModules, leveraging modern Angular Standalone Components for better modularity and lazy-loading potential.

- **Reactive Forms & Dynamic Validation**: The booking form uses ReactiveFormsModule. The validation for the "Document Type" field dynamically switches between "Passport Number" and "National ID" based on the selected route (international vs. domestic) using a custom ValidatorFn.

- **Client-Side Sorting**: As requested, sorting by Price, Duration, and Departure Time is handled entirely on the client side using Angular Signals/Pipes to avoid unnecessary network requests.

### ⚠️ Trade-offs & Known Limitations

Due to the estimated time constraint of the challenge (3-4 hours), the following compromises were made:

- **Data Persistence**: The current implementation uses in-memory mocked data for the flight providers. However, the IFlightProvider interface was designed so that it can easily be swapped with a real HttpClient implementation in the future.

- **Database**: A relational database (e.g., SQL Server with Entity Framework Core) was not configured. The booking flow returns a generated reference code, but state is not persisted across application restarts.

- **Authentication**: Security/Auth layers were intentionally omitted to focus on the core business logic of the search and pricing algorithms.

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
│   │   ├── SkyRoute.Contracts/      # Requests, Responses, and DTOs (Zero external dependencies)
│   │   ├── SkyRoute.Core/           # Domain Entities, Interfaces, and Vertical Slice Services
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