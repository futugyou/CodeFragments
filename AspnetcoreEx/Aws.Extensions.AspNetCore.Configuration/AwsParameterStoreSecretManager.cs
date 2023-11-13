using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Extensions.AspNetCore.Configuration;

public class AwsParameterStoreManager
{
	public virtual string GetKey(Parameter parameter)
	{
		return parameter.Name.Replace("[", ConfigurationPath.KeyDelimiter).Replace("]", ConfigurationPath.KeyDelimiter).Replace("/", ConfigurationPath.KeyDelimiter);
	}

	// TODO: get all/ get paging/ get by path ...
	// use  DescribeParametersAsync
	public virtual async Task<IEnumerable<Parameter>> ReadParameters(IAmazonSimpleSystemsManagement awsmanager, AwsClientConfig config)
	{
		var sections = config.Section;
		if (sections.Length == 0)
		{
			return Enumerable.Empty<Parameter>();
		}

		var response = await awsmanager.GetParametersAsync(new GetParametersRequest { WithDecryption = true, Names = [.. sections] }).ConfigureAwait(false);
		return response.Parameters;
	}

	public virtual IDictionary<string, string?> GetData(IEnumerable<Parameter> parameters)
	{
		var data = new Dictionary<string, Parameter>(StringComparer.OrdinalIgnoreCase);

		foreach (var parameter in parameters)
		{
			string key = GetKey(parameter);

			if (data.TryGetValue(key, out var currentParameter))
			{
				if (parameter.LastModifiedDate > currentParameter.LastModifiedDate)
				{
					data[key] = parameter;
				}
			}
			else
			{
				data.Add(key, parameter);
			}
			Console.WriteLine(key + parameter.Value);
		}

		return data.ToDictionary(d => d.Key, v => v.Value.Value ?? null, StringComparer.OrdinalIgnoreCase);
	}
}