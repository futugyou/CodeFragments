
using System.Reflection;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleSystemsManagement;

namespace Aws.Extensions.AspNetCore.Configuration;
public class AwsParameterStoreConfigurationSource(AWSOptions options, AwsClientConfig config) : IConfigurationSource
{
    private readonly AWSOptions _options = options;

    private readonly AwsClientConfig _config = config;

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var credentials = CreateCredentials(_options);
        var config = CreateConfig(_options);
        IAmazonSimpleSystemsManagement manager = new AmazonSimpleSystemsManagementClient(credentials, config);
        return new AwsParameterStoreConfigurationProvider(manager, _options);
    }

    private static AmazonSimpleSystemsManagementConfig CreateConfig(AWSOptions options)
    {
        var config = new AmazonSimpleSystemsManagementConfig();

        options ??= new AWSOptions();

        if (options.DefaultConfigurationMode.HasValue)
        {
            config.DefaultConfigurationMode = options.DefaultConfigurationMode.Value;
        }

        var defaultConfig = options.DefaultClientConfig;
        var emptyArray = new object[0];
        var singleArray = new object[1];

        var clientConfigTypeInfo = options.DefaultClientConfig.GetType();
        var properties = clientConfigTypeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.GetMethod != null && property.SetMethod != null)
            {
                // Skip RegionEndpoint because it is set below and calling the get method on the
                // property triggers the default region fallback mechanism.
                if (string.Equals(property.Name, "RegionEndpoint", StringComparison.Ordinal))
                    continue;

                // DefaultConfigurationMode is skipped from the DefaultClientConfig because it is expected to be set
                // at the top level of AWSOptions which is done before this loop.
                if (string.Equals(property.Name, "DefaultConfigurationMode", StringComparison.Ordinal))
                    continue;

                // Skip setting RetryMode if it is set to legacy but the DefaultConfigurationMode is not legacy.
                // This will allow the retry mode to be configured from the DefaultConfiguration.
                // This is a workaround to handle the inability to tell if RetryMode was explicitly set.
                if (string.Equals(property.Name, "RetryMode", StringComparison.Ordinal) &&
                    defaultConfig.RetryMode == RequestRetryMode.Legacy &&
                    config.DefaultConfigurationMode != DefaultConfigurationMode.Legacy)
                    continue;

                var s = property.GetMethod.Invoke(defaultConfig, emptyArray);
                if (s != null)
                {
                    singleArray[0] = s;
                    property.SetMethod.Invoke(config, singleArray);
                }
            }
        }

        // Setting RegionEndpoint only if ServiceURL was not set, because ServiceURL value will be lost otherwise
        if (options.Region != null && string.IsNullOrEmpty(defaultConfig.ServiceURL))
        {
            config.RegionEndpoint = options.Region;
        }

        return config;
    }

    private AWSCredentials CreateCredentials(AWSOptions options)
    {
        if (options != null)
        {
            if (options.Credentials != null)
            {
                return options.Credentials;
            }
            if (!string.IsNullOrEmpty(options.Profile))
            {
                var chain = new CredentialProfileStoreChain(options.ProfilesLocation);
                if (chain.TryGetAWSCredentials(options.Profile, out AWSCredentials result))
                {
                    return result;
                }
            }
        }

        AWSCredentials credentials = new BasicAWSCredentials(_config.AccessKeyId, _config.SecretAccessKey);

        if (credentials != null)
        {
            return credentials;
        }

        return FallbackCredentialsFactory.GetCredentials() ?? throw new AmazonClientException("Failed to find AWS Credentials for constructing AWS service client");
    }
}