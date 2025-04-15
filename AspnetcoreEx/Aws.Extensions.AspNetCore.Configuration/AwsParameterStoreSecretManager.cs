using System.Text.RegularExpressions;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json.Linq;

namespace Aws.Extensions.AspNetCore.Configuration;

public partial class AwsParameterStoreManager
{
	// TODO: parse path like '/AAA/BBB/CCC/DDDD', but data path starts from BBB or CCC
	// Not important
	public virtual async Task<IEnumerable<Parameter>> ReadParameters(IAmazonSimpleSystemsManagement awsmanager, AwsClientConfig config)
	{
		var sections = config.Section;
		if (sections.Length == 0)
		{
			var parameters = await GetAllParameters(awsmanager);
			return parameters;
		}

		var bypath = sections.Where(p => p.EndsWith('/'));
		var notbypath = sections.Where(p => !p.EndsWith('/'));

		var parametersWithPath = await GetParametersWithPath(awsmanager, bypath);
		var parametersWithOutPath = await GetParametersWithOutPath(awsmanager, notbypath);

		return parametersWithPath.Concat(parametersWithOutPath.SelectMany(p => p.Parameters));
	}

	private async Task<List<Parameter>> GetAllParameters(IAmazonSimpleSystemsManagement awsmanager)
	{
		var parameterlist = new List<Parameter>();
		var nameList = new List<string>();
		string? nextToken = null;
		do
		{
			DescribeParametersRequest request = new()
			{
				MaxResults = 50, // 50 is max
				NextToken = nextToken,
			};
			var response = await awsmanager.DescribeParametersAsync(request).ConfigureAwait(false);
			if (response.Parameters != null && response.Parameters.Count > 0)
			{
				nameList.AddRange(response.Parameters.Select(p => p.Name));
			}

			nextToken = response.NextToken;
		} while (!string.IsNullOrEmpty(nextToken));

		if (nameList.Count == 0)
		{
			return parameterlist;
		}

		var parametersWithOutPath = await GetParametersWithOutPath(awsmanager, nameList);
		return [.. parametersWithOutPath.SelectMany(p => p.Parameters)];
	}

	private async Task<GetParametersResponse[]> GetParametersWithOutPath(IAmazonSimpleSystemsManagement awsmanager, IEnumerable<string> names)
	{
		var namesList = names.Chunk(size: 10); //10 is max
		using var parameterLoader = new ParallelParameterLoader(awsmanager);

		foreach (var subNames in namesList)
		{
			parameterLoader.Add(subNames);
		}

		var loadedParameter = await parameterLoader.WaitForAll().ConfigureAwait(false);
		return loadedParameter;
	}

	private async Task<List<Parameter>> GetParametersWithPath(IAmazonSimpleSystemsManagement awsmanager, IEnumerable<string> bypath)
	{
		var parameterlist = new List<Parameter>();

		foreach (var path in bypath)
		{
			string? nextToken = null;
			do
			{
				GetParametersByPathRequest request = new()
				{
					Path = path,
					NextToken = nextToken,
					WithDecryption = true,
					MaxResults = 10, // 10 is max
					Recursive = true,
				};
				var response = await awsmanager.GetParametersByPathAsync(request).ConfigureAwait(false);

				parameterlist.AddRange(response.Parameters);
				nextToken = response.NextToken;
			} while (!string.IsNullOrEmpty(nextToken));
		}

		return parameterlist;
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