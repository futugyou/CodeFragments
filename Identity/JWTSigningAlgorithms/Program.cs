// See https://aka.ms/new-console-template for more information

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

Console.WriteLine("Hello, World!");

var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

var now = DateTime.UtcNow;
var handler = new JsonWebTokenHandler();

string token = handler.CreateToken(new SecurityTokenDescriptor
{
    Issuer = "me",
    Audience = "you",
    NotBefore = now,
    Expires = now.AddMinutes(30),
    IssuedAt = now,
    Claims = new Dictionary<string, object> { { "sub", "123" } },
    SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key), "ES256")
});

Console.WriteLine(token);