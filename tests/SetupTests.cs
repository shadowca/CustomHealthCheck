using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace CustomHealthCheck.Tests
{
	public class SetupTests
	{
		[Fact]
		public void CallingAddCustomHealthChecks_Should_Add_The_Required_Services()
		{
			IServiceCollection collection = new ServiceCollection();

			collection.AddCustomHealthCheck();
			collection.AddOptions<HealthCheckServiceOptions>();
			collection.AddLogging();
			collection.AddSingleton<IConfiguration>( new ConfigurationBuilder().Build() );

			ServiceProvider buildServiceProvider = collection.BuildServiceProvider();
			Assert.NotNull( buildServiceProvider.GetRequiredService<HealthCheckService>() );
			Assert.NotNull( buildServiceProvider.GetRequiredService<IDictionary<string, CustomHealthCheckResult>>() );
			Assert.NotNull( buildServiceProvider.GetRequiredService<IHostedService>() );
		}

		[Fact]
		public void CallingAddCustomHealthChecks_MultipleTimes_Should_Add_The_Required_Services_Once()
		{
			IServiceCollection collection = new ServiceCollection();

			collection.AddCustomHealthCheck();
			collection.AddCustomHealthCheck();
			collection.AddCustomHealthCheck();
			collection.AddCustomHealthCheck();


			collection.AddOptions<HealthCheckServiceOptions>();
			collection.AddLogging();
			collection.AddSingleton<IConfiguration>( new ConfigurationBuilder().Build() );
			ServiceProvider buildServiceProvider = collection.BuildServiceProvider();

			ShouldBeOne( buildServiceProvider.GetRequiredService<IEnumerable<HealthCheckService>>() );
			ShouldBeOne( buildServiceProvider.GetRequiredService<IEnumerable<IDictionary<string, CustomHealthCheckResult>>>() );
			ShouldBeOne( buildServiceProvider.GetRequiredService<IEnumerable<IHostedService>>()
				.Where( x => x is CustomHealthCheckBackgroundService ) );
		}

		private static void ShouldBeOne( IEnumerable collection )
		{
			Assert.NotNull( collection );
			Assert.Single( collection );
		}
	}
}