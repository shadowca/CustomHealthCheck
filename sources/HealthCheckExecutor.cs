using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CustomHealthCheck;

/// <summary>
/// Executes health checks based on configuration settings and logs any failures.
/// Manages retry mechanisms and delay configurations.
/// </summary>
internal sealed class HealthCheckExecutor(
	IConfiguration configuration,
	IServiceScopeFactory scopeFactory,
	IDictionary<string, CustomHealthCheckResult> results,
	ILogger<HealthCheckExecutor> logger )
	: IHealthCheckExecutor
{
	private readonly ConcurrentDictionary<string, int> _runCounter = new();

	/// <summary>
	/// Runs a specified health check, handles retries, and logs failures.
	/// </summary>
	/// <param name="registration">Health check registration information.</param>
	/// <param name="token">Token to signal health check cancellation.</param>
	public async Task ExecuteHealthCheckAsync( HealthCheckRegistration registration, CancellationToken token )
	{
		_runCounter.TryAdd( registration.Name, 0 );

		int timeoutSec = configuration.GetValue<int?>( $"HealthCheckConfig:{registration.Name}:TimeoutSec" ) ?? 30;

		while( !token.IsCancellationRequested )
		{
			using var tokenSource = new CancellationTokenSource( TimeSpan.FromSeconds( timeoutSec ) );
			CancellationToken cancellationToken = CancellationTokenSource.CreateLinkedTokenSource( tokenSource.Token, token ).Token;
			TimeSpan delay;
			long start = Stopwatch.GetTimestamp();
			try
			{
				using var scope = scopeFactory.CreateScope();
				var checkResult = await registration.Factory( scope.ServiceProvider )
					.CheckHealthAsync( new HealthCheckContext { Registration = registration }, cancellationToken );

				results[registration.Name] = new CustomHealthCheckResult( checkResult, Stopwatch.GetElapsedTime( start ) );
				_runCounter[registration.Name] = 0; // Reset counter on success
				delay = configuration.GetDelay( registration.Name, 0 );
			}
			catch( Exception e )
			{
				_runCounter.AddOrUpdate( registration.Name, 1, ( _, current ) => current + 1 );
				results[registration.Name] =
					new CustomHealthCheckResult( HealthCheckResult.Unhealthy( e.Message, e ), Stopwatch.GetElapsedTime( start ) );
				logger.LogError( e, "{RegistrationName} health check failed", registration.Name );
				delay = configuration.GetDelay( registration.Name, _runCounter[registration.Name] );
			}

			await Task.Delay( delay, cancellationToken );
		}
	}
}