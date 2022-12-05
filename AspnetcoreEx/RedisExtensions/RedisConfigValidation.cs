using Microsoft.Extensions.Options;

namespace AspnetcoreEx.RedisExtensions;

public class RedisConfigValidation : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
