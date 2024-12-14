# CustomHealthCheck


This repository contains custom implementations for health checks in a system using the `Microsoft.Extensions.Diagnostics.HealthChecks` library. The goal is to provide flexible, configurable health checks that can be executed with custom logic and timing.
Enables the already configured healthchecks to be updated and cached in the background. The health checks are then accessed directly from the cache.

## Overview

The system includes the following main components:

- **CustomHealthCheckService**: A class that extends `HealthCheckService` to handle custom health checks and generate a health report.
- **HealthCheckExecutor**: A service that executes individual health checks according to a configuration, including retry logic.
- **CustomHealthCheckBackgroundService**: A background service that manages the periodic execution of health checks.
- **ConfigurationExtensions**: Helper methods to retrieve delay configurations from the system's configuration files.
- **CustomHealthCheckResult**: A class that encapsulates the result of a health check, including the status, description, and duration.


---

## Configuration

The configuration for health checks is typically stored in a configuration file such as `appsettings.json`. The following table outlines the expected structure and values.

| **Configuration Key**                                      | **Description**                                                                                              | **Default Value**     |
|------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------|-----------------------|
| `HealthCheckConfig:{registrationName}:TimeoutSec`          | The timeout duration in seconds for a health check.                                                           | 30 seconds            |
| `HealthCheckConfig:{registrationName}:HealthCheckIntervalSec` | The interval in seconds between health checks (if using a single value).                                      | 60 seconds            |
| `HealthCheckConfig:{registrationName}:HealthCheckIntervalArrayInSeconds` | An array of integers representing retry intervals in seconds. The array elements are used based on retry count. | `[2, 4, 10, 20, 30, 60]` |


### Example Configuration

```json
{
  "HealthCheckConfig": {
    "ServiceA": {
      "TimeoutSec": 45,
      "HealthCheckIntervalSec": 30,
      "HealthCheckIntervalArrayInSeconds": [2, 5, 10, 20]
    },
    "ServiceB": {
      "TimeoutSec": 60,
      "HealthCheckIntervalSec": 20,
      "HealthCheckIntervalArrayInSeconds": [3, 6, 12, 18]
    }
  }
}
```

### Configuration for Each Health Check Registration

- **TimeoutSec**: Sets the maximum duration (in seconds) allowed for a health check to complete. If a health check exceeds this time, it will be canceled and treated as failed.
- **HealthCheckIntervalSec**: Specifies the delay (in seconds) between health checks if using a single value.
- **HealthCheckIntervalArrayInSeconds**: An array of delay values (in seconds) used for retrying a health check. The first value is used on the first failure, the second value on the second failure, and so on. If there are more failures than the array has elements, the last value in the array is used for subsequent retries.

---

## How It Works

1. **HealthCheckExecutor** will read the configuration for each health check and execute it asynchronously.
2. The executor will retry health checks if they fail, using the configured delays between retries.
3. The results of each health check are stored in a dictionary of `CustomHealthCheckResult` objects.
4. The `CustomHealthCheckService` then gathers all the results and returns a comprehensive `HealthReport` when the `CheckHealthAsync` method is called.

This system allows for flexible health check management with retries and customized intervals based on your application's configuration.

---

## Usage

- **Health checks can be configured** for each service in your application, with customized timeouts and retry intervals.
- **Background execution** allows health checks to run periodically without blocking the main application thread.
- **Retry logic** ensures that transient failures do not cause a health check to fail permanently.

### Add HealthCheck:
```
IServiceCollection services;
services.AddCustomHealthCheck();
```

## Registering Health Checks
Health checks can be registered for services like databases or custom checks in your application. Below is a guide on how to register health checks for different use cases.

### Registering Health Check for DBContext (Entity Framework)
To register a health check for a DbContext, you can use the built-in database health check provided by Microsoft.Extensions.Diagnostics.HealthChecks.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("Database", failureStatus: HealthStatus.Unhealthy);
}
```

AddDbContextCheck: This method registers a health check that will check if the DbContext is able to connect to the database. You can specify the health check name (e.g., "Database") and the failure status to report (e.g., Unhealthy).

### Registering a Custom Health Check (Manually)
You can also create and register a custom health check by implementing the IHealthCheck interface. This allows you to define custom logic for checking the health of your application.

``` csharp
public class MyCustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Your custom health check logic
        bool isHealthy = CheckSomeCondition();

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Custom check passed"));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Custom check failed"));
        }
    }

    private bool CheckSomeCondition()
    {
        // Custom condition logic here
        return true; // Example condition
    }
}
```

#### Registering the Custom Health Check
To register your custom health check, add it to the health check service in ConfigureServices.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<MyCustomHealthCheck>("MyCustomHealthCheck", failureStatus: HealthStatus.Unhealthy);
}
```

AddCheck: Registers your custom health check by specifying the class and a name for the check.
### Possibility to overwrite the execution
If you need a different logic, you can overwrite the interface ``IHealthCheckExecutor`` and implement your own logic.

## Testing
Remove health check for testing.

```
services.RemoveHealthChecks();
```
