# DisasterApp Testing Documentation

## Overview

This document provides comprehensive information about the testing strategy, architecture, and implementation for the DisasterApp API project. The testing framework follows industry best practices and ensures high code quality, reliability, and maintainability.

## Table of Contents

1. [Testing Strategy](#testing-strategy)
2. [Test Architecture](#test-architecture)
3. [Testing Frameworks and Tools](#testing-frameworks-and-tools)
4. [Test Categories](#test-categories)
5. [Test Structure](#test-structure)
6. [Running Tests](#running-tests)
7. [Code Coverage](#code-coverage)
8. [Best Practices](#best-practices)
9. [Continuous Integration](#continuous-integration)
10. [Troubleshooting](#troubleshooting)

## Testing Strategy

### Objectives

- **Quality Assurance**: Ensure all components function correctly and meet requirements
- **Regression Prevention**: Catch bugs early and prevent regressions
- **Documentation**: Tests serve as living documentation of system behavior
- **Confidence**: Enable safe refactoring and feature additions
- **Performance**: Validate system performance under various conditions

### Testing Pyramid

Our testing strategy follows the testing pyramid approach:

```
    /\     E2E Tests (Few)
   /  \    
  /____\   Integration Tests (Some)
 /______\  
/________\ Unit Tests (Many)
```

- **Unit Tests (70%)**: Fast, isolated tests for individual components
- **Integration Tests (20%)**: Tests for component interactions
- **End-to-End Tests (10%)**: Full system workflow tests

## Test Architecture

### Project Structure

```
DisasterApp.Tests/
├── Controllers/           # Controller unit tests
│   ├── AuthControllerTests.cs
│   ├── UserManagementControllerTests.cs
│   └── AuditLogsControllerTests.cs
├── Services/             # Service layer unit tests
│   ├── AuthServiceTests.cs
│   ├── UserManagementServiceTests.cs
│   ├── AuditServiceTests.cs
│   └── OtpServiceTests.cs
├── Repositories/         # Repository unit tests
│   ├── UserRepositoryTests.cs
│   ├── RefreshTokenRepositoryTests.cs
│   ├── BackupCodeRepositoryTests.cs
│   ├── OtpAttemptRepositoryTests.cs
│   ├── OtpCodeRepositoryTests.cs
│   └── PasswordResetTokenRepositoryTests.cs
├── Entities/            # Domain entity unit tests
│   ├── UserTests.cs
│   ├── AuditLogTests.cs
│   └── ...
├── Integration/         # Integration tests
├── Helpers/            # Test utilities and helpers
└── Fixtures/           # Test data and fixtures
```

### Clean Architecture Testing

Our tests are organized according to the Clean Architecture layers:

- **Presentation Layer**: Controller tests
- **Application Layer**: Service tests
- **Infrastructure Layer**: Repository tests
- **Domain Layer**: Entity and domain logic tests

## Testing Frameworks and Tools

### Primary Frameworks

- **xUnit**: Primary testing framework for .NET
- **Moq**: Mocking framework for creating test doubles
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **FluentAssertions**: Fluent assertion library (optional)

### Additional Tools

- **Coverlet**: Code coverage analysis
- **ReportGenerator**: Coverage report generation
- **Bogus**: Test data generation
- **WebApplicationFactory**: Integration testing for ASP.NET Core

### NuGet Packages

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## Test Categories

### Unit Tests

**Purpose**: Test individual components in isolation

**Characteristics**:
- Fast execution (< 100ms per test)
- No external dependencies
- Use mocks and stubs
- High code coverage

**Examples**:
- Service method logic
- Entity validation
- Utility functions
- Business rule validation

### Integration Tests

**Purpose**: Test component interactions and data flow

**Characteristics**:
- Test real database interactions
- Test API endpoints
- Test service integrations
- Slower than unit tests

**Examples**:
- Repository with database
- Controller with services
- Authentication flows
- Data persistence

### End-to-End Tests

**Purpose**: Test complete user workflows

**Characteristics**:
- Test entire application stack
- Use real or test databases
- Simulate user interactions
- Slowest but most comprehensive

**Examples**:
- User registration and login
- Disaster report creation
- Admin user management
- Audit log generation

## Test Structure

### Naming Conventions

**Test Classes**: `{ClassUnderTest}Tests`
```csharp
public class AuthServiceTests
public class UserRepositoryTests
public class AuthControllerTests
```

**Test Methods**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
```csharp
[Fact]
public async Task LoginAsync_ValidCredentials_ReturnsSuccessResult()

[Fact]
public async Task GetUserById_NonExistingUser_ReturnsNull()

[Fact]
public async Task CreateUser_DuplicateEmail_ThrowsException()
```

### Test Method Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and dependencies
    var mockService = new Mock<IService>();
    var controller = new Controller(mockService.Object);
    var inputData = new InputDto { /* test data */ };
    
    mockService.Setup(x => x.Method(It.IsAny<InputDto>()))
               .ReturnsAsync(expectedResult);
    
    // Act - Execute the method under test
    var result = await controller.Method(inputData);
    
    // Assert - Verify the results
    Assert.IsType<OkObjectResult>(result);
    var okResult = result as OkObjectResult;
    Assert.Equal(expectedValue, okResult.Value);
    
    // Verify mock interactions
    mockService.Verify(x => x.Method(It.IsAny<InputDto>()), Times.Once);
}
```

### Test Data Management

**Object Mothers**: Create test objects with default values
```csharp
public static class UserMother
{
    public static User CreateValidUser()
    {
        return new User
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };
    }
    
    public static User CreateInactiveUser()
    {
        var user = CreateValidUser();
        user.IsActive = false;
        return user;
    }
}
```

**Test Builders**: Fluent interface for test object creation
```csharp
public class UserBuilder
{
    private User _user = new User();
    
    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }
    
    public UserBuilder WithName(string firstName, string lastName)
    {
        _user.FirstName = firstName;
        _user.LastName = lastName;
        return this;
    }
    
    public User Build() => _user;
}
```

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Run tests by category
dotnet test --filter "Category=Unit"

# Run tests in parallel
dotnet test --parallel
```

### Visual Studio

1. **Test Explorer**: View → Test Explorer
2. **Run All Tests**: Ctrl+R, A
3. **Run Selected Tests**: Ctrl+R, T
4. **Debug Tests**: Ctrl+R, Ctrl+T

### Visual Studio Code

1. Install .NET Core Test Explorer extension
2. Use Command Palette: "Test: Run All Tests"
3. Use integrated terminal for command line testing

## Code Coverage

### Coverage Goals

- **Overall Coverage**: > 80%
- **Critical Components**: > 90%
- **Business Logic**: > 95%
- **Controllers**: > 85%
- **Services**: > 90%
- **Repositories**: > 85%

### Generating Coverage Reports

```bash
# Generate coverage data
dotnet test --collect:"XPlat Code Coverage"

# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" -reporttypes:Html
```

### Coverage Analysis

- **Line Coverage**: Percentage of code lines executed
- **Branch Coverage**: Percentage of decision branches taken
- **Method Coverage**: Percentage of methods called
- **Class Coverage**: Percentage of classes instantiated

## Best Practices

### General Guidelines

1. **Test Naming**: Use descriptive names that explain the scenario
2. **Single Responsibility**: Each test should verify one specific behavior
3. **Independence**: Tests should not depend on each other
4. **Repeatability**: Tests should produce consistent results
5. **Fast Execution**: Unit tests should run quickly

### Mocking Guidelines

1. **Mock External Dependencies**: Database, web services, file system
2. **Don't Mock Value Objects**: Simple data containers
3. **Verify Important Interactions**: Ensure critical methods are called
4. **Use Strict Mocks**: Fail on unexpected method calls

```csharp
// Good: Mock external dependency
var mockRepository = new Mock<IUserRepository>();

// Good: Setup expected behavior
mockRepository.Setup(x => x.GetByIdAsync(userId))
              .ReturnsAsync(expectedUser);

// Good: Verify interaction
mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
```

### Database Testing

1. **Use In-Memory Database**: For unit tests
2. **Use Test Database**: For integration tests
3. **Clean State**: Reset database between tests
4. **Transaction Rollback**: Use transactions for cleanup

```csharp
private DbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    return new ApplicationDbContext(options);
}
```

### Async Testing

```csharp
[Fact]
public async Task AsyncMethod_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var service = new Service();
    
    // Act
    var result = await service.AsyncMethod(input);
    
    // Assert
    Assert.NotNull(result);
}
```

### Exception Testing

```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsArgumentException()
{
    // Arrange
    var service = new Service();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.Method(invalidInput));
}
```

## Continuous Integration

### GitHub Actions

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
```

### Quality Gates

- All tests must pass
- Code coverage must meet minimum thresholds
- No critical security vulnerabilities
- Code analysis warnings addressed

## Troubleshooting

### Common Issues

**Tests Not Running**
- Check test project references
- Verify test framework packages
- Ensure test methods are public and have [Fact] attribute

**Flaky Tests**
- Remove dependencies between tests
- Use deterministic test data
- Avoid time-dependent assertions
- Clean up resources properly

**Slow Tests**
- Minimize database operations
- Use in-memory databases for unit tests
- Optimize test data setup
- Run tests in parallel

**Mock Issues**
- Verify setup matches actual method calls
- Check parameter matching
- Use It.IsAny<T>() for flexible matching
- Verify mock behavior configuration

### Debugging Tests

1. **Set Breakpoints**: In test methods and code under test
2. **Use Debug Mode**: Run tests in debug mode
3. **Check Test Output**: Review test runner output
4. **Logging**: Add logging to understand test flow

### Performance Optimization

1. **Parallel Execution**: Enable parallel test execution
2. **Test Data**: Minimize test data creation
3. **Setup/Teardown**: Optimize test initialization
4. **Resource Management**: Properly dispose resources

## Conclusion

This testing documentation provides a comprehensive guide for maintaining high-quality tests in the DisasterApp project. Following these guidelines ensures reliable, maintainable, and effective tests that support continuous development and deployment.

For questions or suggestions regarding testing practices, please refer to the development team or create an issue in the project repository.