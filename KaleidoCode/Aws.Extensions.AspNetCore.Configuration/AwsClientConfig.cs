
using Amazon.SimpleSystemsManagement;

namespace Aws.Extensions.AspNetCore.Configuration;
public class AwsClientConfig
{
    public int ReloadInterval { get; set; }
    public string[] Section { get; set; } = [];
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string Region { get; set; }
}