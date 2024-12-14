using Microsoft.Extensions.Configuration;

namespace CustomHealthCheck.Tests
{
	public class DelayTests
	{
		[Fact]
		public void When_nothing_is_configured_it_is_default_60_seconds()
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();
			Dictionary<string, string?> initialValue = new Dictionary<string, string?>();
			builder.AddInMemoryCollection( initialValue );

			IConfigurationRoot configuration = builder.Build();
			var span = configuration.GetDelay( "", 0 );
			Assert.Equal( 1, span.Minutes );
		}

		[Fact]
		public void When_only_the_old_way_is_configured_it_should_be_used()
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();
			Dictionary<string, string?> initialValue = new Dictionary<string, string?>();
			initialValue.Add("HealthCheckConfig:DynamicCreditLimitDbContext:HealthCheckIntervalSec", "20");
			builder.AddInMemoryCollection(initialValue);

			IConfigurationRoot configuration = builder.Build();
			var span = configuration.GetDelay("DynamicCreditLimitDbContext", 0);
			Assert.Equal(20, span.Seconds);
		}

		[Theory]
		[InlineData( [new[] { 2, 4, 6, 10, 20, 50, 60 },0,2] )]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 1, 4])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 2, 6])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 3, 10])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 4, 20])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 5, 50])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 6, 60])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 100, 60])]
		public void When_only_the_new_way_is_configured_it_should_be_used(int[] values, int retry, int expectedResult)
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();

			Dictionary<string, string> initialValue = new Dictionary<string, string>();
			if( values.Length > 0 )
			{
				for( int i = 0; i < values.Length; i++ )
				{
					initialValue.Add($"HealthCheckConfig:DynamicCreditLimitDbContext:HealthCheckIntervalArrayInSeconds:{i}", "" + values[i]);
				}
			}
			builder.AddInMemoryCollection(initialValue);

			IConfigurationRoot configuration = builder.Build();
			var span = configuration.GetDelay("DynamicCreditLimitDbContext", retry);
			Assert.Equal(expectedResult, span.TotalSeconds);
		}

		[Theory]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 0, 2])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 1, 4])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 2, 6])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 3, 10])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 4, 20])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 5, 50])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 6, 60])]
		[InlineData([new[] { 2, 4, 6, 10, 20, 50, 60 }, 100, 60])]
		public void When_both_are_configured_it_should_be_used_the_new_one(int[] values, int retry, int expectedResult)
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();

			Dictionary<string, string> initialValue = new Dictionary<string, string>();
			if (values.Length > 0)
			{
				for (int i = 0; i < values.Length; i++)
				{
					initialValue.Add($"HealthCheckConfig:DynamicCreditLimitDbContext:HealthCheckIntervalArrayInSeconds:{i}", "" + values[i]);
				}
			}
			initialValue.Add("HealthCheckConfig:DynamicCreditLimitDbContext:HealthCheckIntervalSec", "500");
			builder.AddInMemoryCollection(initialValue);

			IConfigurationRoot configuration = builder.Build();
			var span = configuration.GetDelay("DynamicCreditLimitDbContext", retry);
			Assert.Equal(expectedResult, span.TotalSeconds);
		}
	}
}
