// See https://aka.ms/new-console-template for more information

using Client.Certificate;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

Console.WriteLine("Hello, World!");

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