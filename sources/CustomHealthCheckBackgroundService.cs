using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CustomHealthCheck
{
	/// <summary>
	/// Background service to execute all registered health checks periodically.
	/// </summary>
	internal sealed class CustomHealthCheckBackgroundService(
		IHealthCheckExecutor healthCheckExecutor,
		IOptions<HealthCheckServiceOptions> healthCheckServiceOptions )
		: BackgroundService
	{
		private readonly IOptions<HealthCheckServiceOptions> _healthCheckServiceOptions =
			healthCheckServiceOptions ?? throw new ArgumentNullException( nameof(healthCheckServiceOptions) );

		/// <summary>
		/// Initiates execution of all registered health checks asynchronously.
		/// </summary>
		/// <param name="stoppingToken">Token to stop the execution of health checks.</param>
		protected override async Task ExecuteAsync( CancellationToken stoppingToken )
		{
			if( _healthCheckServiceOptions.Value.Registrations.Count == 0 )
				return;

			IEnumerable<Task> tasks = _healthCheckServiceOptions.Value.Registrations.Select( registration =>
				Task.Run( async () => { await healthCheckExecutor.ExecuteHealthCheckAsync( registration, stoppingToken ); },
					stoppingToken ) );

			await Task.WhenAll( tasks );
		}
	}
}