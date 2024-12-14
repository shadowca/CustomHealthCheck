using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomHealthCheck
{
	/// <summary>
	/// Custom implementation of HealthCheckService that evaluates the health status of various services
	/// and aggregates results in a health report.
	/// </summary>
	internal sealed class CustomHealthCheckService( IDictionary<string, CustomHealthCheckResult> results ) : HealthCheckService
	{
		/// <summary>
		/// Executes health checks based on a predicate and returns a comprehensive HealthReport.
		/// </summary>
		/// <param name="predicate">Optional filter for health checks to execute.</param>
		/// <param name="cancellationToken">Token to cancel the health check operation.</param>
		/// <returns>Task representing the aggregated HealthReport of all checks.</returns>
		public override Task<HealthReport> CheckHealthAsync( Func<HealthCheckRegistration, bool>? predicate,
			CancellationToken cancellationToken = new() )
		{
			return Task.FromResult( new HealthReport(
				results.ToDictionary( x => x.Key, x => new HealthReportEntry(
					x.Value.Status, x.Value.Description, x.Value.Duration, x.Value.Exception, x.Value.Data ) ),
				GetSumOfDuration( results ) ) );
		}

		/// <summary>
		/// Calculates the total duration of all health check results.
		/// </summary>
		/// <param name="results">Dictionary of health check results.</param>
		/// <returns>Aggregated time span of all health checks.</returns>
		private static TimeSpan GetSumOfDuration( IDictionary<string, CustomHealthCheckResult> results )
		{
			return results
				.Aggregate( TimeSpan.Zero, ( subtotal, t ) => subtotal.Add( t.Value.Duration ) );
		}
	}
}