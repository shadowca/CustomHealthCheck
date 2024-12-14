using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomHealthCheck
{
	/// <summary>
	/// Represents the result of a custom health check with additional metadata, such as duration.
	/// </summary>
	internal sealed class CustomHealthCheckResult( HealthCheckResult result, TimeSpan duration )
	{
		/// <summary>
		/// Duration of the health check execution.
		/// </summary>
		public TimeSpan Duration { get; } = duration;

		/// <summary>
		/// Description of the health check result, if any.
		/// </summary>
		public string? Description => result.Description;

		/// <summary>
		/// Status indicating whether the health check passed or failed.
		/// </summary>
		public HealthStatus Status => result.Status;

		/// <summary>
		/// Exception information in case the health check failed.
		/// </summary>
		public Exception? Exception => result.Exception;

		/// <summary>
		/// Additional data provided by the health check.
		/// </summary>
		public IReadOnlyDictionary<string, object> Data => result.Data;
	}
}