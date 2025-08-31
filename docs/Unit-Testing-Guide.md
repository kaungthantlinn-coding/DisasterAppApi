# Unit Testing Guide

## Overview

This guide provides detailed instructions for writing effective unit tests in the DisasterApp project. Unit tests form the foundation of our testing strategy, ensuring individual components work correctly in isolation.

## Table of Contents

1. [Unit Testing Principles](#unit-testing-principles)
2. [Test Structure](#test-structure)
3. [Testing Different Components](#testing-different-components)
4. [Mocking Strategies](#mocking-strategies)
5. [Common Patterns](#common-patterns)
6. [Examples](#examples)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## Unit Testing Principles

### FIRST Principles

- **Fast**: Tests should execute quickly (< 100ms)
- **Independent**: Tests should not depend on other tests
- **Repeatable**: Tests should produce consistent results
- **Self-Validating**: Tests should have clear pass/fail results
- **Timely**: Tests should be written close to the production code

### Characteristics of Good Unit Tests

1. **Isolated**: Test one unit of functionality
2. **Deterministic**: Same input always produces same output
3. **Readable**: Clear intent and easy to understand
4. **Maintainable**: Easy to update when requirements change
5. **Trustworthy**: Reliable indicators of code quality

## Test Structure

### AAA Pattern (Arrange, Act, Assert)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and dependencies
    var mockDependency = new Mock<IDependency>();
    var systemUnderTest = new SystemUnderTest(mockDependency.Object);
    var input = new InputData { /* test data */ };
    
    mockDependency.Setup(x => x.Method(It.IsAny<InputData>()))
                  .ReturnsAsync(expectedResult);
    
    // Act - Execute the method under test
    var result = await systemUnderTest.Method(input);
    
    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal(expectedValue, result.Property);
    
    // Verify interactions
    mockDependency.Verify(x => x.Method(It.IsAny<InputData>()), Times.Once);
}
```

### Test Method Naming

**Pattern**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

**Examples**:
```csharp
// Good naming examples
LoginAsync_ValidCredentials_ReturnsSuccessResult()
GetUserById_NonExistingUser_ReturnsNull()
CreateUser_DuplicateEmail_ThrowsArgumentException()
ValidatePassword_EmptyPassword_ReturnsFalse()
CalculateTotal_WithDiscount_ReturnsDiscountedAmount()

// Poor naming examples
TestLogin() // Too generic
Test1() // No meaning
LoginTest() // Doesn't describe scenario
```

## Testing Different Components

### Testing Controllers

**Focus Areas**:
- HTTP status codes
- Response data
- Model validation
- Authorization
- Exception handling

```csharp
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;
    
    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDto 
        { 
            Email = "test@example.com", 
            Password = "password123" 
        };
        var expectedResult = new AuthResult 
        { 
            Success = true, 
            Token = "jwt-token" 
        };
        
        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                       .ReturnsAsync(expectedResult);
        
        // Act
        var result = await _controller.Login(loginDto);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var authResult = Assert.IsType<AuthResult>(okResult.Value);
        Assert.True(authResult.Success);
        Assert.Equal("jwt-token", authResult.Token);
    }
    
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto 
        { 
            Email = "test@example.com", 
            Password = "wrongpassword" 
        };
        var expectedResult = new AuthResult 
        { 
            Success = false, 
            Message = "Invalid credentials" 
        };
        
        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                       .ReturnsAsync(expectedResult);
        
        // Act
        var result = await _controller.Login(loginDto);
        
        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var authResult = Assert.IsType<AuthResult>(unauthorizedResult.Value);
        Assert.False(authResult.Success);
        Assert.Equal("Invalid credentials", authResult.Message);
    }
}
```

### Testing Services

**Focus Areas**:
- Business logic
- Data validation
- Exception handling
- Dependency interactions
- Return values

```csharp
public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UserManagementService>> _mockLogger;
    private readonly UserManagementService _service;
    
    public UserManagementServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserManagementService>>();
        _service = new UserManagementService(
            _mockUserRepository.Object, 
            _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!"
        };
        
        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName
        };
        
        _mockUserRepository.Setup(x => x.GetByEmailAsync(createUserDto.Email))
                          .ReturnsAsync((User)null);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
                          .ReturnsAsync(expectedUser);
        
        // Act
        var result = await _service.CreateUserAsync(createUserDto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(createUserDto.Email, result.Email);
        Assert.Equal(createUserDto.FirstName, result.FirstName);
        Assert.Equal(createUserDto.LastName, result.LastName);
        
        _mockUserRepository.Verify(x => x.GetByEmailAsync(createUserDto.Email), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsArgumentException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePassword123!"
        };
        
        var existingUser = new User { Email = createUserDto.Email };
        
        _mockUserRepository.Setup(x => x.GetByEmailAsync(createUserDto.Email))
                          .ReturnsAsync(existingUser);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateUserAsync(createUserDto));
        
        Assert.Contains("already exists", exception.Message);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
```

### Testing Repositories

**Focus Areas**:
- Data access operations
- Query logic
- Entity mapping
- Database interactions
- Error handling

```csharp
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;
    
    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }
    
    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }
    
    [Fact]
    public async Task GetByEmailAsync_NonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexisting@example.com");
        
        // Assert
        Assert.Null(result);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Testing Entities

**Focus Areas**:
- Property validation
- Business rules
- Calculated properties
- Entity behavior
- Equality comparisons

```csharp
public class UserTests
{
    [Fact]
    public void GetFullName_WithFirstAndLastName_ReturnsFullName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        var fullName = user.GetFullName();
        
        // Assert
        Assert.Equal("John Doe", fullName);
    }
    
    [Fact]
    public void IsLockedOut_WithLockoutEndInFuture_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30)
        };
        
        // Act
        var isLockedOut = user.IsLockedOut();
        
        // Assert
        Assert.True(isLockedOut);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void IncrementFailedLoginAttempts_ValidAttempts_IncrementsCount(int initialCount)
    {
        // Arrange
        var user = new User { FailedLoginAttempts = initialCount };
        
        // Act
        user.IncrementFailedLoginAttempts();
        
        // Assert
        Assert.Equal(initialCount + 1, user.FailedLoginAttempts);
    }
}
```

## Mocking Strategies

### When to Mock

**Mock These**:
- External services (APIs, databases)
- File system operations
- Network calls
- Time-dependent operations
- Complex dependencies

**Don't Mock These**:
- Value objects
- Simple data structures
- The system under test
- Framework types (unless necessary)

### Mock Setup Patterns

```csharp
// Basic setup
mockService.Setup(x => x.Method(parameter))
           .Returns(result);

// Async setup
mockService.Setup(x => x.MethodAsync(parameter))
           .ReturnsAsync(result);

// Conditional setup
mockService.Setup(x => x.Method(It.Is<string>(s => s.Contains("test"))))
           .Returns(result);

// Exception setup
mockService.Setup(x => x.Method(parameter))
           .Throws<ArgumentException>();

// Callback setup
mockService.Setup(x => x.Method(It.IsAny<User>()))
           .Callback<User>(user => user.Id = Guid.NewGuid())
           .Returns(result);
```

### Verification Patterns

```csharp
// Verify method was called
mockService.Verify(x => x.Method(parameter), Times.Once);

// Verify method was never called
mockService.Verify(x => x.Method(parameter), Times.Never);

// Verify with any parameter
mockService.Verify(x => x.Method(It.IsAny<string>()), Times.AtLeastOnce);

// Verify all setups were called
mockService.VerifyAll();

// Verify no other calls were made
mockService.VerifyNoOtherCalls();
```

## Common Patterns

### Testing Async Methods

```csharp
[Fact]
public async Task AsyncMethod_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var service = new Service();
    var input = new InputData();
    
    // Act
    var result = await service.AsyncMethod(input);
    
    // Assert
    Assert.NotNull(result);
}
```

### Testing Exceptions

```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsArgumentException()
{
    // Arrange
    var service = new Service();
    var invalidInput = new InvalidInputData();
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => service.Method(invalidInput));
    
    Assert.Contains("expected error message", exception.Message);
}
```

### Parameterized Tests

```csharp
[Theory]
[InlineData("valid@email.com", true)]
[InlineData("invalid-email", false)]
[InlineData("", false)]
[InlineData(null, false)]
public void ValidateEmail_VariousInputs_ReturnsExpectedResult(string email, bool expected)
{
    // Arrange
    var validator = new EmailValidator();
    
    // Act
    var result = validator.ValidateEmail(email);
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Testing with Test Data Builders

```csharp
public class UserBuilder
{
    private User _user = new User();
    
    public static UserBuilder Create() => new UserBuilder();
    
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
    
    public UserBuilder AsActive()
    {
        _user.IsActive = true;
        return this;
    }
    
    public User Build() => _user;
}

// Usage in tests
[Fact]
public void TestMethod()
{
    // Arrange
    var user = UserBuilder.Create()
        .WithEmail("test@example.com")
        .WithName("John", "Doe")
        .AsActive()
        .Build();
    
    // Act & Assert
    // ...
}
```

## Best Practices

### Test Organization

1. **Group Related Tests**: Use nested classes for logical grouping
2. **Consistent Structure**: Follow AAA pattern consistently
3. **Clear Naming**: Use descriptive test names
4. **Single Assertion**: Focus on one behavior per test

### Test Data Management

1. **Minimal Data**: Use only necessary test data
2. **Realistic Data**: Use realistic but simple test data
3. **Data Builders**: Use builders for complex objects
4. **Constants**: Define test constants for reusable values

### Performance Considerations

1. **Fast Tests**: Keep unit tests under 100ms
2. **Parallel Execution**: Design tests to run in parallel
3. **Resource Cleanup**: Dispose resources properly
4. **Minimal Setup**: Avoid expensive setup operations

### Maintainability

1. **DRY Principle**: Avoid duplicating test code
2. **Helper Methods**: Extract common test logic
3. **Clear Intent**: Make test purpose obvious
4. **Regular Refactoring**: Keep tests clean and updated

## Troubleshooting

### Common Issues

**Test Not Running**
- Check [Fact] or [Theory] attribute
- Ensure method is public
- Verify test project references

**Mock Not Working**
- Check setup matches actual call
- Verify parameter matching
- Use It.IsAny<T>() for flexible matching

**Async Test Issues**
- Use async/await properly
- Don't mix sync and async code
- Handle exceptions in async methods

**Flaky Tests**
- Remove time dependencies
- Use deterministic data
- Avoid shared state
- Clean up properly

### Debugging Tips

1. **Use Debugger**: Set breakpoints in tests
2. **Add Logging**: Use test output for debugging
3. **Isolate Issues**: Run single tests
4. **Check Mocks**: Verify mock setup and calls

## Conclusion

Effective unit testing is crucial for maintaining code quality and enabling confident refactoring. Follow these guidelines to create reliable, maintainable, and valuable unit tests that support the long-term success of the DisasterApp project.

Remember: Good tests are an investment in code quality and developer productivity. Take time to write them well, and they will pay dividends throughout the project lifecycle.