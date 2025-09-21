namespace AspnetcoreEx.Redis;

public class RedisConfigValidation : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
