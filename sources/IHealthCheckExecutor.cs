using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomHealthCheck;

public interface IHealthCheckExecutor
{
	Task ExecuteHealthCheckAsync(HealthCheckRegistration registration, CancellationToken token);
}