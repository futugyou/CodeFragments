// See https://aka.ms/new-console-template for more information

using Client.Certificate;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

Console.WriteLine("Hello, World!");

//await MtlsEndpointClientCredentials();

async Task MtlsEndpointClientCredentials()
{

    // discover endpoints from metadata
    var handler = new SocketsHttpHandler();
    handler.SslOptions.ClientCertificates = new X509CertificateCollection { Certificate.Get() };
    var client = new HttpClient(handler);
    var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
    if (disco.IsError)
    {
        Console.WriteLine(disco.Error);
        return;
    }

    var request = new ClientCredentialsTokenRequest
    {
        //Address = disco.TokenEndpoint,
        Address = disco.MtlsEndpointAliases.TokenEndpoint,
        ClientId = "client",
        //ClientSecret = "secret",
        Scope = "api1",
    };
    var tokenResponse = await client.RequestClientCredentialsTokenAsync(request);

    if (tokenResponse.IsError)
    {
        Console.WriteLine(tokenResponse.Error);
        return;
    }

    Console.WriteLine(tokenResponse.Json);

    // call api
    var apiClient = new HttpClient();
    apiClient.SetBearerToken(tokenResponse.AccessToken);

    var response = await apiClient.GetAsync("https://localhost:5003/WeatherForecast");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(response.StatusCode);
    }
    else
    {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(JArray.Parse(content));
        Console.ReadLine();
    }
}

await ResourceOwnerPasswordModel();

async Task ResourceOwnerPasswordModel()
{
    var client = new HttpClient();
    var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
    if (disco.IsError)
    {
        Console.WriteLine(disco.Error);
        return;
    }

    var request = new PasswordTokenRequest
    {
        //获取Token的地址
        Address = disco.TokenEndpoint,
        //客户端Id
        ClientId = "resource_owner_client",
        //客户端密码
        ClientSecret = "secret",
        //要访问的api资源
        Scope = "api1 openid profile",
        UserName = "tang@tang.com",
        Password = "1qaz@WSX"
    };
    var tokenResponse = await client.RequestPasswordTokenAsync(request);

    if (tokenResponse.IsError)
    {
        Console.WriteLine(tokenResponse.Error);
        return;
    }

    Console.WriteLine(tokenResponse.Json);
    // call api
    var apiClient = new HttpClient();

    apiClient.SetBearerToken(tokenResponse.AccessToken);

    var response = await apiClient.GetAsync("https://localhost:5003/WeatherForecast");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(response.StatusCode);
    }
    else
    {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(JArray.Parse(content));
        Console.ReadLine();
    }
}