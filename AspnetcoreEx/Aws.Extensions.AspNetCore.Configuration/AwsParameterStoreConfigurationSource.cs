using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime.Credentials;
using Amazon.SimpleSystemsManagement;

namespace Aws.Extensions.AspNetCore.Configuration;
public class AwsParameterStoreConfigurationSource(AWSOptions options, AwsClientConfig config) : IConfigurationSource
{
    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        options.Credentials = CreateCredentials();
        var manager = options.CreateServiceClient<IAmazonSimpleSystemsManagement>();
        return new AwsParameterStoreConfigurationProvider(manager, config);
    }

    private AWSCredentials CreateCredentials()
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

        AWSCredentials credentials = new BasicAWSCredentials(config.AccessKeyId, config.SecretAccessKey);

        if (credentials != null)
        {
            return credentials;
        }

        return DefaultAWSCredentialsIdentityResolver.GetCredentials() ?? throw new AmazonClientException("Failed to find AWS Credentials for constructing AWS service client");
    }
}