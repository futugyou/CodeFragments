using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightSourceGenerator
{
    [Generator]
    public class PlaywrightTemplate : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 注册代码生成
            var sourceProvider = context.AnalyzerConfigOptionsProvider
                .Select((options, _) => GenerateUserProfileClass());

            context.RegisterSourceOutput(sourceProvider, (ctx, source) =>
            {
                ctx.AddSource("UserProfileGenerator.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        /// <summary>
        /// 生成 Playwright UserProfile 代码
        /// </summary>
        private static string GenerateUserProfileClass()
        {
            return @"
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecordTraceTest
{
    public class UserProfileGenerator
    {
        public static async Task GetUserProfile()
        {
            using var playwright = await Playwright.CreateAsync();
            var request = await playwright.APIRequest.NewContextAsync();
            
            var loginData = new Dictionary<string, object>
            {
                { ""password"", ""***"" },
                { ""userName"", ""***"" },
                { ""phoneCode"", ""***"" }
            };

            var response = await request.PostAsync(""https://devtest.services.osim-cloud.com/identity/api/v1.0/account/login"", new() { DataObject = loginData });
            var headers = new Dictionary<string, string>();
            var body = await response.JsonAsync();
            var token = body.Value.GetProperty(""accessToken"").GetString();

            headers.Add(""Accept"", ""application/json"");
            headers.Add(""Authorization"", ""Bearer "" + token);

            request = await playwright.APIRequest.NewContextAsync(new()
            {
                BaseURL = ""https://devtest.services.osim-cloud.com"",
                ExtraHTTPHeaders = headers,
            });

            var userResponse = await request.GetAsync(""/user/api/v1.0/userprofile"");
            var userBody = await userResponse.JsonAsync();
            var resultCode = userBody.Value.GetProperty(""resultCode"").GetInt32();
            var httpStatus = userBody.Value.GetProperty(""httpStatus"").GetInt32();

            // Act & Assert
            Console.WriteLine(userResponse.Ok);
            Console.WriteLine(resultCode == 1);
            Console.WriteLine(httpStatus == 200);
        }
    }
}
";
        }
    }
}
