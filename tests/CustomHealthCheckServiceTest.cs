using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomHealthCheck.Tests
{
	public class CustomHealthCheckServiceTest
	{
		[Fact]
		public async Task EmptyArray()
		{
			var sut = new CustomHealthCheckService( new ConcurrentDictionary<string, CustomHealthCheckResult>() );
			var result = await sut.CheckHealthAsync( CancellationToken.None );

			Assert.NotNull(result);
			Assert.Equal(HealthStatus.Healthy, result.Status);
			Assert.Equal(TimeSpan.Zero, result.TotalDuration);
		}

		[Theory]
		[InlineData( false )]
		[InlineData( true )]
		public async Task EmptyArrayWithMethod( bool returnValueOfPredicate )
		{
			var sut = new CustomHealthCheckService( new ConcurrentDictionary<string, CustomHealthCheckResult>() );
			var result = await sut.CheckHealthAsync( registration => returnValueOfPredicate, CancellationToken.None );

			Assert.NotNull(result);
			Assert.Equal(HealthStatus.Healthy, result.Status);
			Assert.Equal(TimeSpan.Zero, result.TotalDuration);
		}

		[Theory]
		[MemberData( nameof(CustomHealthCheckResults) )]
		internal async Task DifferentValues( Dictionary<string, CustomHealthCheckResult> dictionary,
			HealthStatus expectedStatus, TimeSpan expectedSumDuration )
		{
			var customHealthCheckResults = new ConcurrentDictionary<string, CustomHealthCheckResult>();
			customHealthCheckResults.TryAdd( "RabbitMq",
				new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Healthy, "Description" ), TimeSpan.FromSeconds( 1 ) ) );
			var sut = new CustomHealthCheckService( dictionary );
			var result = await sut.CheckHealthAsync( CancellationToken.None );

			Assert.NotNull( result );
			Assert.Equal( expectedStatus, result.Status );
			Assert.Equal( expectedSumDuration, result.TotalDuration );
		}

		public static IEnumerable<object[]> CustomHealthCheckResults()
		{
			yield return new object[]
			{
				new Dictionary<string, CustomHealthCheckResult>()
				{
					{
						"RabbitMQ",
						new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Healthy, "Description" ),
							TimeSpan.FromSeconds( 1 ) )
					}
				},
				HealthStatus.Healthy, TimeSpan.FromSeconds( 1 )
			};

			yield return new object[]
			{
				new Dictionary<string, CustomHealthCheckResult>()
				{
					{
						"RabbitMQ",
						new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Healthy, "Description" ),
							TimeSpan.FromSeconds( 1 ) )
					},
					{
						"MongoDB",
						new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Unhealthy, "Description" ),
							TimeSpan.FromSeconds( 30 ) )
					},
				},
				HealthStatus.Unhealthy, TimeSpan.FromSeconds( 31 )
			};

			yield return new object[]
			{
				new Dictionary<string, CustomHealthCheckResult>()
				{
					{
						"RabbitMQ",
						new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Unhealthy, "Description" ),
							TimeSpan.FromSeconds( 1 ) )
					},
					{
						"MongoDB",
						new CustomHealthCheckResult( new HealthCheckResult( HealthStatus.Unhealthy, "Description" ),
							TimeSpan.FromSeconds( 2 ) )
					},
				},
				HealthStatus.Unhealthy, TimeSpan.FromSeconds( 3 )
			};
		}
	}
}