using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using Aws.Extensions.AspNetCore.Configuration;

namespace Microsoft.Extensions.Configuration;
public static class AwsParameterStoreConfigurationExtensions
{
    public const string DEFAULT_CONFIG_SECTION = "AWS";
    public static IConfigurationBuilder AddAwsParameterStore(this IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        var config = new AwsClientConfig();
        configuration.GetSection(DEFAULT_CONFIG_SECTION).Bind(config);
        var options = configuration.GetAWSOptions();
        configurationBuilder.Add(new AwsParameterStoreConfigurationSource(options, config));

        return configurationBuilder;
    }

    public static IConfigurationBuilder AddAwsParameterStore(this IConfigurationBuilder configurationBuilder, AWSOptions options)
    {
        var configuration = configurationBuilder.Build();
        var config = new AwsClientConfig();
        configuration.GetSection(DEFAULT_CONFIG_SECTION).Bind(config);
        configurationBuilder.Add(new AwsParameterStoreConfigurationSource(options, config));
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddAwsParameterStore(this IConfigurationBuilder configurationBuilder, AwsClientConfig config)
    {
        var configuration = configurationBuilder.Build();
        var options = configuration.GetAWSOptions();
        configurationBuilder.Add(new AwsParameterStoreConfigurationSource(options, config));
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddAwsParameterStore(this IConfigurationBuilder configurationBuilder, AWSOptions options, AwsClientConfig config)
    {
        configurationBuilder.Add(new AwsParameterStoreConfigurationSource(options, config));
        return configurationBuilder;
    }
}
