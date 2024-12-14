using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomHealthCheck
{
	public static class Setup
	{
		public static IHealthChecksBuilder AddCustomHealthCheck( [NotNull] this IServiceCollection services )
		{
			services.TryAddSingleton<HealthCheckService, CustomHealthCheckService>();
			services.TryAddSingleton<IHealthCheckExecutor, HealthCheckExecutor>();
			services.TryAddSingleton<IDictionary<string, CustomHealthCheckResult>>( _ =>
				new ConcurrentDictionary<string, CustomHealthCheckResult>() );
			services.AddHostedService<CustomHealthCheckBackgroundService>();
			return services.AddHealthChecks();
		}

		public static IServiceCollection RemoveHealthChecks( [NotNull] this IServiceCollection collection )
		{
			ServiceDescriptor serviceDescriptor =
				collection.First( s => s.ImplementationType == typeof( CustomHealthCheckBackgroundService ) );
			collection.Remove( serviceDescriptor );
			return collection;
		}
	}
}