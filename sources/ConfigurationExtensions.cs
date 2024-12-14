using Microsoft.Extensions.Configuration;

namespace CustomHealthCheck;

/// <summary>
/// Configuration extension methods to retrieve delay intervals for health checks based on retry counts.
/// </summary>
internal static class ConfigurationExtensions
{
	private const int DefaultCheckIntervalSec = 60;
	private static readonly int[] DefaultCheckIntervalArrayInSeconds = [2, 4, 10, 20, 30, 60];

	/// <summary>
	/// Gets the delay interval between health check retries.
	/// </summary>
	/// <param name="configuration">Configuration object for health check settings.</param>
	/// <param name="registrationName">The name of the health check registration.</param>
	/// <param name="retryCount">The current retry count for the health check.</param>
	/// <returns>The delay interval as a TimeSpan.</returns>
	public static TimeSpan GetDelay( this IConfiguration configuration, string registrationName, int retryCount )
	{
		IConfigurationSection section = configuration.GetSection( GetConfigurationNameforArray( registrationName ) );
		int[]? values = section.Get<int[]>();

		if( values != null )
		{
			if( values.Length == 0 )
				values = DefaultCheckIntervalArrayInSeconds;

			TimeSpan fromSeconds = TimeSpan.FromSeconds( values.Length > retryCount
				? values[retryCount]
				: values.Last() );

			return fromSeconds;
		}

		int delaySec = configuration
				.GetValue<int?>( GetConfigurationNameForSingleValue( registrationName ) ) ??
			DefaultCheckIntervalSec;
		return TimeSpan.FromSeconds( delaySec );
	}

	/// <summary>
	/// Gets the configuration key name for a single value delay interval.
	/// </summary>
	private static string GetConfigurationNameForSingleValue( string registrationName )
	{
		return $"HealthCheckConfig:{registrationName}:HealthCheckIntervalSec";
	}

	/// <summary>
	/// Gets the configuration key name for an array of delay intervals.
	/// </summary>
	private static string GetConfigurationNameforArray( string registrationName )
	{
		return $"HealthCheckConfig:{registrationName}:HealthCheckIntervalArrayInSeconds";
	}
}