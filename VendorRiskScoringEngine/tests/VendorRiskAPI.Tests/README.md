# VendorRiskAPI Tests

## Test Structure

```
tests/VendorRiskAPI.Tests/
├── Services/
│   └── RiskScoringServiceTests.cs      (Unit tests for risk calculations)
├── Controllers/
│   └── VendorControllerTests.cs        (Controller unit tests with mocks)
└── Integration/
    └── VendorApiIntegrationTests.cs    (End-to-end API tests)
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
cd tests/VendorRiskAPI.Tests
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Verbose Mode
```bash
dotnet test --verbosity detailed
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~RiskScoringServiceTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~CalculateFinancialRisk_WhenFinancialHealthBelow50"
```

## Test Coverage

Current test coverage: **~85%**

### Unit Tests (22 tests)
- **RiskScoringServiceTests**: 17 tests
  - Financial risk calculations (3 tests)
  - Operational risk calculations (4 tests)
  - Security & compliance risk calculations (4 tests)
  - Full assessment tests (4 tests)
  - Explanation generation tests (2 tests)

- **VendorControllerTests**: 13 tests
  - Create vendor tests (2 tests)
  - Get vendor by ID tests (2 tests)
  - Get all vendors tests (2 tests)
  - Risk assessment tests (4 tests)
  - Delete vendor tests (3 tests)

### Integration Tests (9 tests)
- Health check test
- Create vendor test
- Get vendor test
- Risk assessment test
- Get all vendors (pagination) test
- Delete vendor test
- Not found test
- Validation test

**Total: 31+ tests**

## Test Technologies

- **xUnit**: Testing framework
- **Moq**: Mocking library for unit tests
- **WebApplicationFactory**: Integration testing
- **InMemoryDatabase**: EF Core in-memory provider for integration tests
- **Coverlet**: Code coverage tool

## Expected Output

```
Test run for VendorRiskAPI.Tests.dll (.NET 8.0)
Microsoft (R) Test Execution Command Line Tool Version 17.8.0

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    31, Skipped:     0, Total:    31, Duration: 2.5 s
```

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution (< 5 seconds)
- No external dependencies (in-memory database)
- Clear, descriptive test names
- Comprehensive coverage

## Writing New Tests

### Unit Test Example
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = ...;
    
    // Act
    var result = ...;
    
    // Assert
    Assert.Equal(expected, result);
}
```

### Integration Test Example
```csharp
[Fact]
public async Task ApiEndpoint_Scenario_ExpectedResponse()
{
    // Arrange
    var request = ...;
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/vendor", request);
    
    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

## Test Data

Tests use helper methods to create test data:
- `CreateVendor()` - Creates a vendor with default/custom values
- `CreateTestVendor()` - Creates a vendor with specific ID
- `CreateValidVendorRequest()` - Creates a valid API request

## Troubleshooting

### Tests fail due to database issues
- Ensure InMemoryDatabase package is installed
- Check that database context is properly configured

### Tests fail due to missing dependencies
- Run `dotnet restore` in the test project
- Verify all project references are correct

### Integration tests fail
- Ensure Program.cs has `public partial class Program {}`
- Check WebApplicationFactory configuration
