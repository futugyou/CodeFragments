using System.Text.RegularExpressions;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json.Linq;

namespace Aws.Extensions.AspNetCore.Configuration;

public partial class AwsParameterStoreManager
{
	// TODO: get all/ get paging/ get by path ...  use  DescribeParametersAsync to getall
	// TODO: parse path like '/AAA/BBB/CCC/DDDD', but data path starts from BBB or CCC
	// None of the above is important
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
		var dic = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

		foreach (var parameter in parameters)
		{
			string key = GetKey(parameter);
			var added = ToDictionary(key, parameter, dic);
			if (!added)
			{
				if (!dic.ContainsKey(key))
				{
					dic.Add(key, parameter.Value);
				}
			}
		}

		return dic;
	}


	public virtual string GetKey(Parameter parameter)
	{
		var segment = parameter.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
		return segment[^1];
	}

	private static bool ToDictionary(string parentKey, Parameter parameter, Dictionary<string, string?> dic)
	{
		var result = false;
		try
		{
			var jsonObject = JObject.Parse(parameter.Value);
			var jTokens = jsonObject.DescendantsAndSelf().Where(p => !p.Any());
			foreach (var jToken in jTokens)
			{
				var key = GetSubKey(parentKey, jToken.Path);
				if (!dic.ContainsKey(key))
				{
					dic.Add(key, jToken.ToString());
				}
			}

			result = true;
		}
		catch
		{
		}

		return result;
	}

	private static bool ToDictionary(string parentKey, string value, Dictionary<string, string?> dic)
	{
		var result = false;
		try
		{
			var jsonObject = JObject.Parse(value);
			var jTokens = jsonObject.DescendantsAndSelf().Where(p => !p.Any());
			foreach (var jToken in jTokens)
			{
				var key = GetSubKey(parentKey, jToken.Path);
				if (!dic.ContainsKey(key))
				{
					dic.Add(key, jToken.ToString());
				}
			}

			result = true;
		}
		catch
		{
		}

		return result;
	}

	private static string GetSubKey(string parentKey, string path)
	{
		var sub = SubKeyRegex().Replace(path, ConfigurationPath.KeyDelimiter);
		if (sub.EndsWith(ConfigurationPath.KeyDelimiter))
		{
			sub = sub[..^1];
		}
		return parentKey + ConfigurationPath.KeyDelimiter + sub;
	}

	[GeneratedRegex("[^0-9a-zA-Z]+")]
	private static partial Regex SubKeyRegex();
}