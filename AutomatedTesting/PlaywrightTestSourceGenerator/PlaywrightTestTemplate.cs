using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace PlaywrightTestSourceGenerator;

[Generator]
public class PlaywrightTestTemplate : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册初始化时生成的 attribute
        context.RegisterPostInitializationOutput(i =>
        {
            i.AddSource("GlobalParameterAttribute.g.cs", @"using System;
namespace SourceGeneratorAttributes;
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class GlobalParameterAttribute : Attribute
{
    public int OptionalNumber { get; set; } = 10;
    public string OptionalString { get; set; } = ""test"";
    public GlobalParameterAttribute(string requiredString, int requiredNumber) {}
}");
        });

        // 解析 JSON 配置文件
        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select((file, cancellationToken) =>
            {
                var jsonText = file.GetText(cancellationToken)?.ToString();
                if (jsonText is null) return default;

                var jNode = JsonNode.Parse(jsonText);
                return (name: (string)jNode["ServiceName"], apiList: jNode["APIList"]);
            })
            .Where(data => data.name is not null && data.apiList is not null);

        // 生成测试代码
        context.RegisterSourceOutput(jsonFiles, (spc, options) =>
        {
            var sourceCode = GenerateTestClass(options.name, options.apiList);
            spc.AddSource($"{options.name}GeneratorTest.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        });
    }

    private static string GenerateTestClass(string name, JsonNode apiList)
    {
        StringBuilder sourceBuilder = new StringBuilder(@$"
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace RecordTraceTest;

[TestClass]
public class {name}GeneratorTest : PlaywrightTest
{{
");

        foreach (var item in apiList.AsArray())
        {
            var apiname = (string)item["AcionName"];
            var url = (string)item["Url"];

            sourceBuilder.Append(@$"
    [TestMethod]
    public async Task {apiname}Test()
    {{
        var request = await Playwright.APIRequest.NewContextAsync();
        var data = new Dictionary<string, object>() {{
          {{ ""password"", ""***!""}},
          {{ ""userName"", ""***.cn"" }},
          {{ ""phoneCode"", ""***"" }}
        }};

        var response = await request.PostAsync(""https://devtest.services.osim-cloud.com/identity/api/v1.0/account/login"", new() {{ DataObject = data }});
        var headers = new Dictionary<string, string>();
        var body = await response.JsonAsync();
        var token = body.Value.GetProperty(""accessToken"").GetString();

        headers.Add(""Accept"", ""application/json"");
        headers.Add(""Authorization"", ""Bearer "" + token);

        request = await Playwright.APIRequest.NewContextAsync(new()
        {{
            BaseURL = ""https://devtest.services.osim-cloud.com"",
            ExtraHTTPHeaders = headers,
        }});

        var userReponse = await request.GetAsync(""{url}"");
        var userbody = await userReponse.JsonAsync();
        var resultCode = userbody.Value.GetProperty(""resultCode"").GetInt32();
        var httpStatus = userbody.Value.GetProperty(""httpStatus"").GetInt32();

        Assert.IsTrue(userReponse.Ok);
        Assert.IsTrue(resultCode == 1);
        Assert.IsTrue(httpStatus == 200);
    }}
");
        }

        sourceBuilder.Append(@"
}
");
        return sourceBuilder.ToString();
    }
}
