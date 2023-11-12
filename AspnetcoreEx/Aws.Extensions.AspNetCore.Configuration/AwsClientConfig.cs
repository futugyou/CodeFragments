
using Amazon.SimpleSystemsManagement;

namespace Aws.Extensions.AspNetCore.Configuration;
public class AwsClientConfig
{
    public bool UseKeySecret { get; set; }

    public string AccessKeyId { get; set; }

    public string SecretAccessKey { get; set; }
    public string Region { get; set; }
}