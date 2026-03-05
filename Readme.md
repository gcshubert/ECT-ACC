# ECT.ACC

A .NET 10 REST API implementing the **Energy-Control-Time (ECT) Framework** and **Algorithmic Control Characterization (ACC) Framework** — quantitative analytical tools for evaluating process feasibility across scientific and engineering domains.

---

## What This Is

The ECT Framework provides a general mathematical method for determining whether a proposed mechanism has sufficient capacity to achieve an observed outcome within available energy and time constraints. The core equation is:

```
T = k / (E × C)
```

Where:
- **T** = time required for completion
- **E** = usable energy
- **C** = control factor (dimensionless)
- **k** = complexity constant determined by the outcome

The ACC Framework extends ECT by translating abstract control values into concrete, measurable properties — information processing requirements, coordination capabilities, precision demands — and classifying deficits into four types:

| Type | Name | Description |
|------|------|-------------|
| A | Throughput Gap | Process is correct but too slow |
| B | Precision Gap | Outcome specificity beyond natural tolerance |
| C | Coordination Gap | Multi-component integration required |
| D | Specification Gap | Functional discrimination required — most challenging |

This API allows users to define scenarios, store ECT parameters, and compute ACC deficit analyses programmatically.

---

## Solution Architecture

```
ECT.ACC/
├── ECT.ACC.Api          # ASP.NET Core Web API — controllers, services, middleware
├── ECT.ACC.Data         # EF Core data layer — models, DbContext, math utilities
└── ECT.ACC.Contracts    # Shared DTOs — API contract between backend and clients
```

### Why This Structure?

The solution is organized around **separation of concerns** at both the project and architectural layer levels:

**`ECT.ACC.Contracts`** is intentionally isolated with no dependencies on the other projects. It defines the API surface — the data shapes that flow between the API and any consuming client. Keeping contracts in their own project means a future C# client could reference this library directly without pulling in EF or ASP.NET dependencies.

**`ECT.ACC.Data`** owns all persistence concerns — EF Core models, the `ECTDbContext`, migrations, and the `ECTMath` calculation engine. Isolating data access here means the API layer never directly constructs queries, and the math logic is testable independently of HTTP concerns.

**`ECT.ACC.Api`** is deliberately thin — controllers delegate immediately to a service layer, which handles orchestration between the math engine and the data layer. Controllers know nothing about EF; services know nothing about HTTP.

This layered approach mirrors enterprise .NET patterns and makes each layer independently testable and replaceable.

---

## Tech Stack

| Concern | Technology |
|---------|------------|
| Framework | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 (Code First) |
| Database | SQL Server (ECTFramework database) |
| API Documentation | Swagger / Swashbuckle |
| Architecture | Layered — Controllers → Services → EF Core |

---

## Key Design Decisions

### ScientificValue — Coefficient/Exponent Pattern

The ECT and ACC frameworks regularly produce numbers of astronomical magnitude (e.g., C_required ≈ 10^174,000 for abiogenesis scenarios). Standard `double` and `decimal` types cannot represent these values accurately.

All quantitative values are stored as a **coefficient × 10^exponent** pair:

```csharp
public class ScientificValue
{
    public double Coefficient { get; set; }  // e.g. 1.85
    public double Exponent { get; set; }     // e.g. 174000
}
```

Arithmetic operations (multiply, divide) are performed in log space to maintain precision across extreme magnitudes. This is handled by the `ECTMath` static class in `ECT.ACC.Data`.

### EF Owned Entities

`ScientificValue` is mapped as an EF Core **owned entity**, meaning its coefficient and exponent columns are embedded directly in the parent table rather than requiring a separate join. This keeps the schema clean while preserving the type abstraction in code.

### Service Layer Owns Calculations

ECT and ACC calculations (`ComputeMinimumControl`, `ComputeDeficit`, `ClassifyDeficit`) live in the service layer, not the controllers or the data models. This ensures:
- Business logic is testable without an HTTP context
- Controllers remain focused on request/response handling
- The math engine (`ECTMath`) can be reused across multiple services

### CORS

The API is configured to accept requests from `http://localhost:5173` (the default Vite dev server port) in preparation for the companion React frontend (`CSReactApp`).

---

## API Endpoints

### Scenarios

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/Scenarios` | Retrieve all scenarios |
| GET | `/api/Scenarios/{id}` | Retrieve a scenario by ID |
| POST | `/api/Scenarios` | Create a new scenario with parameters |
| PUT | `/api/Scenarios/{id}` | Update scenario name/description |
| PUT | `/api/Scenarios/{id}/parameters` | Update ECT parameters for a scenario |
| DELETE | `/api/Scenarios/{id}` | Delete a scenario |

### Deficit Analysis

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/DeficitAnalysis/scenario/{scenarioId}` | Retrieve stored analysis for a scenario |
| POST | `/api/DeficitAnalysis/scenario/{scenarioId}/compute` | Compute and persist ACC deficit analysis |



---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (local instance)
- Visual Studio 2026 or VS Code

### Setup

1. Clone the repository
2. Update the connection string in `ECT.ACC.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "ECTDatabase": "Server=YOUR_SERVER;Database=ECTFramework;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
3. Apply migrations to create the database:
```
Update-Database -Project ECT.ACC.Data -StartupProject ECT.ACC.Api -Verbose
```
4. Run the project — Swagger UI will open at `https://localhost:{port}/swagger`

---

## Related Projects

- **[ECT](https://github.com/gcshubert/ECT)** — React + Vite frontend consuming this API, providing an interactive ECT calculator and ACC deficit visualization

### Published Frameworks

**ECT Framework** — Energy-Control-Time
- DOI: [https://doi.org/10.17605/OSF.IO/ZD6PV](https://doi.org/10.17605/OSF.IO/ZD6PV)
- OSF: [https://osf.io/yqu5m](https://osf.io/yqu5m)

**ACC Framework** — Algorithmic Control Characterization
- DOI: [https://doi.org/10.17605/OSF.IO/765FA](https://doi.org/10.17605/OSF.IO/765FA)
- OSF: [https://osf.io/u5rkq](https://osf.io/u5rkq)
---

## Author

**Geoffrey Shubert**
[linkedin.com/in/geoffrey-shubert](https://www.linkedin.com/in/geoffrey-shubert) | [github.com/gcshubert](https://github.com/gcshubert)