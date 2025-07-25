# DisasterApp - Clean Architecture

This project follows Clean Architecture principles with a well-organized folder structure.

## Project Structure

```
DisasterApp/
├── src/
│   ├── DisasterApp.Domain/           # Core business logic and entities
│   │   ├── Entities/                 # Domain entities
│   │   ├── Interfaces/               # Domain interfaces
│   │   └── ValueObjects/             # Value objects
│   │
│   ├── DisasterApp.Application/      # Application business logic
│   │   ├── Commands/                 # CQRS Commands
│   │   ├── DTOs/                     # Data Transfer Objects
│   │   ├── Mappings/                 # Object mappings
│   │   ├── Queries/                  # CQRS Queries
│   │   └── Services/                 # Application services
│   │       ├── Implementations/
│   │       └── Interfaces/
│   │
│   ├── DisasterApp.Infrastructure/   # External concerns
│   │   ├── Persistence/              # Database context and configurations
│   │   └── Repositories/             # Data access implementations
│   │       ├── Implementations/
│   │       └── Interfaces/
│   │
│   ├── DisasterApp.WebApi/          # Web API layer
│   │   ├── Controllers/             # API controllers
│   │   └── Properties/              # Launch settings
│   │
│   └── DisasterApp.Tests/           # Unit and integration tests
│
├── DisasterApp.sln                 # Solution file
└── README.md                       # This file
```

## Architecture Layers

### 1. Domain Layer (`DisasterApp.Domain`)
- **Purpose**: Contains the core business logic and entities
- **Dependencies**: None (innermost layer)
- **Contents**:
  - `Entities/`: Core business entities (User, DisasterReport, etc.)
  - `Interfaces/`: Domain service interfaces
  - `ValueObjects/`: Immutable value objects

### 2. Application Layer (`DisasterApp.Application`)
- **Purpose**: Contains application-specific business logic
- **Dependencies**: Domain layer only
- **Contents**:
  - `Commands/`: CQRS command handlers
  - `Queries/`: CQRS query handlers
  - `DTOs/`: Data transfer objects for API communication
  - `Services/`: Application services and their interfaces
  - `Mappings/`: Object-to-object mappings

### 3. Infrastructure Layer (`DisasterApp.Infrastructure`)
- **Purpose**: Contains external concerns and implementations
- **Dependencies**: Domain and Application layers
- **Contents**:
  - `Persistence/`: Entity Framework DbContext and configurations
  - `Repositories/`: Data access implementations

### 4. Presentation Layer (`DisasterApp.WebApi`)
- **Purpose**: Web API endpoints and controllers
- **Dependencies**: Application and Infrastructure layers
- **Contents**:
  - `Controllers/`: REST API controllers
  - `Properties/`: Configuration files

### 5. Test Layer (`DisasterApp.Tests`)
- **Purpose**: Unit and integration tests
- **Dependencies**: All layers for testing

## Key Benefits of This Structure

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Inversion**: Dependencies point inward toward the domain
3. **Testability**: Easy to unit test business logic in isolation
4. **Maintainability**: Clear organization makes code easier to maintain
5. **Scalability**: Structure supports growth and team collaboration

## Getting Started

1. Open `DisasterApp.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution
4. Run the `DisasterApp.WebApi` project

## Development Guidelines

- Keep the Domain layer free of external dependencies
- Use dependency injection for cross-layer communication
- Implement interfaces in the Infrastructure layer
- Keep controllers thin - delegate to Application services
- Write tests for business logic in the Domain and Application layers